using HDLibrary.Wpf.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TamOne_Dialer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HwndSource hwndSource;
        private WindowInteropHelper wih;
        IntPtr clipboardViewerNext;
        private bool getCopyValue = false;
        private UserSettings settings;

        private PortalSession session;

        const int WM_DRAWCLIPBOARD = 0x0308;
        const int WM_CHANGECBCHAIN = 0x030D;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(
            IntPtr hWndRemove,  // handle to window to remove
            IntPtr hWndNewNext  // handle to next window
            );

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HotKeyHost hotKeyHost = new HotKeyHost((HwndSource)HwndSource.FromVisual(App.Current.MainWindow));
            var callKey = new HotKey(Key.F3, ModifierKeys.Control | ModifierKeys.Shift, true);
            callKey.HotKeyPressed += callKey_HotKeyPressed;
            hotKeyHost.AddHotKey(callKey);
            Console.WriteLine("HotKey registered");

            wih = new WindowInteropHelper(this);
            hwndSource = HwndSource.FromHwnd(wih.Handle);
            if (hwndSource != null)
                hwndSource.AddHook(MainWindowProc);

            RegisterClipboardViewer();
        }

        public void RegisterClipboardViewer()
        {
            clipboardViewerNext = SetClipboardViewer(hwndSource.Handle);
        }

        public void UnregisterClipboardViewer()
        {
            ChangeClipboardChain(hwndSource.Handle, clipboardViewerNext);
        }

        private void CopyFromActiveProgram()
        {
            getCopyValue = true;
            System.Windows.Forms.SendKeys.SendWait("^c");
        }

        void callKey_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            //this.ActivateThoroughly();
            Console.WriteLine("HotKey called");
            var element = AutomationElement.FocusedElement;
            //AutomationElement.

            if (element != null)
            {
                object pattern = null;
                if (element.TryGetCurrentPattern(TextPattern.Pattern, out pattern))
                {
                    var tp = (TextPattern)pattern;
                    var sb = new StringBuilder();

                    foreach (var r in tp.GetSelection())
                    {
                        sb.AppendLine(r.GetText(-1));
                    }

                    var selectedText = sb.ToString();
                    Console.WriteLine(selectedText);
                    this.txtPhoneNumber.Text = selectedText;
                    this.ActivateThoroughly();
                    return;
                }
            }
            CopyFromActiveProgram();

        }

        private IntPtr MainWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_DRAWCLIPBOARD:

                    if (getCopyValue && Clipboard.ContainsText())
                    {
                        getCopyValue = false;
                        var selectedText = Clipboard.GetText();
                        txtPhoneNumber.Text = selectedText;
                        Clipboard.Clear();
                        this.ActivateThoroughly();
                    }
                    // Send message along, there might be other programs listening to the copy command.
                    SendMessage(clipboardViewerNext, msg, wParam, lParam);
                    break;

                case WM_CHANGECBCHAIN:
                    if (wParam == clipboardViewerNext)
                    {
                        clipboardViewerNext = lParam;
                    }
                    else
                    {
                        SendMessage(clipboardViewerNext, msg, wParam, lParam);
                    }
                    break;
            }

            return IntPtr.Zero;
        }
        

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (settings == null)
            {
                settings = new UserSettings();
            }

            //settings.Visibility = Visibility.Visible;
            settings.ShowDialog();
            this.session = settings.Session;

            //settings.Activate();

            //new UserSettings().Activate();
        }

        private void btnCall_Click(object sender, RoutedEventArgs e)
        {
            if (this.session != null)
            {
                session.BeginCall(txtPhoneNumber.Text);
            }
        }
    }
}
