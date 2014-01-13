using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TamOne_Dialer
{
    /// <summary>
    /// Interaction logic for CallWindow.xaml
    /// </summary>
    public partial class CallWindow
    {
        private PortalSession2 session = new PortalSession2();
        private bool freshStart = true;
        private bool deviceListValid = false;
        private bool prefixListValid = false;

        public CallWindow()
        {
            //Console.WriteLine();
            InitializeComponent();
            session.LoggingIn += session_LoggingIn;
            session.LoginFailed += session_LoginFailed;
            session.LoginSucceeded += session_LoginSucceeded;
            session.DeviceListReceived += session_DeviceListReceived;
            session.PrefixListReceived += session_PrefixListReceived;
            session.Called += session_Called;
            session.CallFailed += session_CallFailed;
            session.DeviceListFailed += session_DeviceListFailed;
            session.PrefixListFailed += session_PrefixListFailed;
            this.Activated += CallWindow_Activated;
            this.Loaded += CallWindow_Loaded;
        }

        void session_PrefixListFailed(PortalSession2.PrefixListFailedEventArgs e)
        {
            prefixListValid = false;
            this.Dispatcher.Invoke((Action)delegate()
            {
                if (e.Exception == null)
                {
                    MessageBox.Show("De nummerherkenningslijst kon niet worden opgehaald.");
                }
                else
                {
                    MessageBox.Show("De nummerherkenningslijst kon niet worden opgehaald.\n\n" + e.Exception.Message);
                }
            });
        }

        void session_DeviceListFailed(PortalSession2.DeviceListFailedEventArgs e)
        {
            deviceListValid = false;
            this.Dispatcher.Invoke((Action)delegate()
            {
                if (e.Exception == null)
                {
                    MessageBox.Show("De toestellijst kon niet worden opgehaald.");
                }
                else
                {
                    MessageBox.Show("De toestellijst kon niet worden opgehaald.\n\n" + e.Exception.Message);
                }
            });
        }

        void CallWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //this.InitShortcut();
            //var timer = new DispatcherTimer();
            //timer.Tick += timer_Tick;
            //timer.Interval = new TimeSpan(0, 0, 3);
            //timer.Start();
            //RegisterClipboardViewer();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //((DispatcherTimer)sender).Stop();
            this.InitShortcut();
        }

        void session_CallFailed(PortalSession2.CallFailedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate()
            {
                Console.WriteLine("Call failed.");
                if (e.Exception != null)
                {
                    MessageBox.Show("Belopdracht mislukt.\n\n" + e.Exception.Message);
                }
                else
                {
                    MessageBox.Show("Belopdracht mislukt.");
                }
            });
        }

        void CallWindow_Activated(object sender, EventArgs e)
        {
            if (freshStart)
            {
                freshStart = false;
                this.InitShortcut();
                setUpSession();
                if (Properties.Settings.Default.Username != "" || Properties.Settings.Default.Password != "")
                {
                    session.Login(Properties.Settings.Default.Username, Properties.Settings.Default.Password, PortalSession2.LoginType.SILENT);
                }
                else
                {
                    ShowSettings();
                }
            }
        }

        void session_Called(PortalSession2.CalledEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate()
            {
                Console.WriteLine("Call succeeded.");
                //txtPhoneNumber.Text = e.Dialed;
            });
        }

        void session_PrefixListReceived(PortalSession2.PrefixListReceivedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate()
            {
                Console.WriteLine("Prefix list received.");
                cmdPrefixes.Items.Clear();
                foreach (var prefix in e.Prefixes)
                {
                    cmdPrefixes.Items.Add(prefix);
                }
                if (e.Prefixes.Count == 0)
                {
                    prefixListValid = false;
                    MessageBox.Show("De nummerherkenningslijst is leeg. Neem contact op met TamOne.");
                }
                else
                {
                    prefixListValid = true;
                    cmdPrefixes.SelectedIndex = 0;
                }
            });
        }

        void session_DeviceListReceived(PortalSession2.DeviceListReceivedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate()
            {
                Console.WriteLine("Device list received.");
                cmdDevices.Items.Clear();
                foreach (var device in e.Devices)
                {
                    cmdDevices.Items.Add(device); 
                }
                if (e.Devices.Count == 0)
                {
                    deviceListValid = false;
                    MessageBox.Show("Er zijn geen toestellen gekoppeld aan uw account. U kunt deze applicatie niet gebruiken.");
                }
                else
                {
                    deviceListValid = true;
                    cmdDevices.SelectedIndex = 0;
                }
                
            });
        }

        void session_LoginSucceeded(PortalSession2.LoginSucceededEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate()
            {
                Console.WriteLine("Login succeeded.");
                if (e.Name == null)
                {
                    lblStatus.Content = "Ingelogd als " + session.Username + ".";
                }
                else
                {
                    lblStatus.Content = "Ingelogd als " + e.Name + ".";
                }
            });
            session.GetDeviceList();
            session.GetPrefixList();
        }

        void session_LoginFailed(PortalSession2.LoginFailedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate()
            {
                Console.WriteLine("Login failed.");
                if (e.LoginType != PortalSession2.LoginType.SILENT)
                {
                    if (e.Exception != null)
                    {
                        MessageBox.Show("Inloggen mislukt.\n\n" + e.Exception.Message);
                    }
                    else
                    {
                        MessageBox.Show("Inloggen mislukt.");
                    }
                }
                lblStatus.Content = "U bent niet ingelogd.";
            });
        }

        void session_LoggingIn(PortalSession2.LoggingInEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate()
            {
                if (e.LoginType != PortalSession2.LoginType.INTERNAL)
                {
                    prefixListValid = false;
                    deviceListValid = false;
                    cmdDevices.Items.Clear();
                    cmdPrefixes.Items.Clear();
                }
                Console.WriteLine("Logging in...");
                lblStatus.Content = "Bezig met inloggen...";
            });
        }

        private void DoMove(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ShowSettings()
        {
            var settingsDialog = new Settings();
            var location = btnSettings.PointToScreen(new Point(0, 0));
            settingsDialog.Left = location.X - settingsDialog.Width + btnSettings.ActualWidth;
            settingsDialog.Top = location.Y + btnSettings.ActualHeight + 10;
            settingsDialog.Owner = this;
            settingsDialog.Username = Properties.Settings.Default.Username;
            settingsDialog.Password = Properties.Settings.Default.Password;
            settingsDialog.PortalServer = Properties.Settings.Default.PortalServer;
            settingsDialog.AutoDial = Properties.Settings.Default.AutoDial;

            bool? result = settingsDialog.ShowDialog();
            if (result.GetValueOrDefault(false))
            {
#if PUBLIC
                session.EndpointPrefix = new Uri("https://portal.voipcentrale.nl/");
#else
                session.EndpointPrefix = new Uri("https://" + settingsDialog.PortalServer + "/");
#endif

                session.Login(settingsDialog.Username, settingsDialog.Password, PortalSession2.LoginType.NORMAL);
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void btnCall_Click(object sender, RoutedEventArgs e)
        {
            ActionCall();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var helpDialog = new HelpWindow();
            var location = btnHelp.PointToScreen(new Point(0, 0));
            helpDialog.Left = location.X - helpDialog.Width + btnHelp.ActualWidth;
            helpDialog.Top = location.Y + btnHelp.ActualHeight + 10;
            helpDialog.Owner = this;
            
            helpDialog.ShowDialog();
        }

        private void setUpSession()
        {
            session.EndpointPrefix = new Uri("https://" + Properties.Settings.Default.PortalServer + "/");
        }

        private void ActionCall()
        {
            if (session.LoginState == PortalSession2.LoginStates.LOGGED_OUT || !prefixListValid || !deviceListValid)
            {
                ShowSettings();
            }
            else if (cmdPrefixes.SelectedItem == null || cmdDevices.SelectedItem == null)
            {
                MessageBox.Show("Selecteer een toestel en een nummerherkenningsoptie.");
            }
            else if (session.LoginState == PortalSession2.LoginStates.LOGGED_IN)
            {
                session.Call((Prefix)cmdPrefixes.SelectedItem, "0", txtPhoneNumber.Text.Replace("\r\n", "").Replace("\n", ""), (Device)cmdDevices.SelectedItem);
            }
        }

        private void LogoClick(object sender, MouseButtonEventArgs e)
        {
            session.AssociateNewCookieContainer();
            Console.WriteLine("Associated new cookie container.");
        }

        
    }
}
