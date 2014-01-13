using HDLibrary.Wpf.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
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
    public partial class CallWindow
    {
        private HwndSource hwndSource;
        private WindowInteropHelper wih;
        IntPtr clipboardViewerNext;
        private bool getCopyValue = false;

        const int WM_DRAWCLIPBOARD = 0x0308;
        const int WM_CHANGECBCHAIN = 0x030D;

        private Timer copyTimer = new Timer(2000);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        /*static IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam)
        {
            return IntPtr.Zero;
        }*/

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetClipboardViewer(IntPtr hWnd);
        /*public static IntPtr SetClipboardViewer(IntPtr hWnd)
        {
            return IntPtr.Zero;
        }*/


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeClipboardChain(
            IntPtr hWndRemove,  // handle to window to remove
            IntPtr hWndNewNext  // handle to next window
            );
        /*public static bool ChangeClipboardChain(
            IntPtr hWndRemove,  // handle to window to remove
            IntPtr hWndNewNext  // handle to next window
            )
        {
            return false;
        }*/

        private void InitShortcut()
        {
            Console.WriteLine("Entered InitShortcut");

            copyTimer.AutoReset = false;
            copyTimer.Elapsed += copyTimer_Elapsed;

            HotKeyHost hotKeyHost = new HotKeyHost((HwndSource)HwndSource.FromVisual(this));
            Console.WriteLine("Created new HotKeyHost");
            var callKey = new HotKey(Key.F1, ModifierKeys.Control, true);
            //var callKey = new HotKey(Key.F3, ModifierKeys.Control | ModifierKeys.Shift, true);
            Console.WriteLine("Created new hotkey.");
            callKey.HotKeyPressed += callKey_HotKeyPressed;
            Console.WriteLine("Added hotkey event");
            try
            {
                hotKeyHost.AddHotKey(callKey);
            }
            catch (Win32Exception e)
            {
                MessageBox.Show("Kon hotkey niet registreren");
                Console.WriteLine(e);
            }
            catch (HotKeyAlreadyRegisteredException e)
            {
                MessageBox.Show("Hotkey al geregistreerd");
                Console.WriteLine(e);
            }
            Console.WriteLine("HotKey registered");

            wih = new WindowInteropHelper(this);
            hwndSource = HwndSource.FromHwnd(wih.Handle);
            if (hwndSource != null)
                hwndSource.AddHook(MainWindowProc);

            RegisterClipboardViewer();
        }

        void copyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Copy timeout!");
            getCopyValue = false;
            this.Dispatcher.Invoke((Action)delegate()
            {
                this.ActivateThoroughly();
                MessageBox.Show(this, "Het telefoonnummer kon niet worden gelezen. Probeer het nogmaals.");
            });
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
            copyTimer.Start();
            System.Windows.Forms.SendKeys.SendWait("^c");
        }

        void callKey_HotKeyPressed(object sender, HotKeyEventArgs e)
        {
            //this.ActivateThoroughly();
            Console.WriteLine("HotKey called");

            copyTimer.Stop();
            getCopyValue = false;

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
                    this.txtPhoneNumber.Text = selectedText.Replace("\r\n", "").Replace("\n", "").Trim();
                    this.ActivateThoroughly();
                    if (Properties.Settings.Default.AutoDial)
                    {
                        ActionCall();
                    } 
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
                        copyTimer.Stop();
                        getCopyValue = false;
                        var selectedText = Clipboard.GetText();
                        txtPhoneNumber.Text = selectedText.Replace("\r\n", "").Replace("\n", "").Trim();
                        Clipboard.Clear();
                        this.ActivateThoroughly();
                        if (Properties.Settings.Default.AutoDial)
                        {
                            ActionCall();
                        }            
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
    }
}
