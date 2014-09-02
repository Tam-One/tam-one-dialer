using Hardcodet.Wpf.TaskbarNotification;
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
        private TaskbarIcon trayIcon;

        public CallWindow()
        {
            //Console.WriteLine();
            InitializeComponent();
            trayIcon = (TaskbarIcon)FindResource("TrayIcon");
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
            this.Closing += CallWindow_Closing;
            trayIcon.TrayMouseDoubleClick += TrayIcon_TrayMouseDoubleClick;
        }

        void TrayIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.ActivateThoroughly();
        }

        void CallWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        void session_PrefixListFailed(PortalSession2.PrefixListFailedEventArgs e)
        {
            prefixListValid = false;
            this.Dispatcher.Invoke((Action)delegate()
            {
                ShowNotification("er ging iets mis", "De nummerherkenningslijst kon niet worden opgehaald.", NotificationType.Error, e.Exception);
            });
        }

        void session_DeviceListFailed(PortalSession2.DeviceListFailedEventArgs e)
        {
            deviceListValid = false;
            this.Dispatcher.Invoke((Action)delegate()
            {
                ShowNotification("er ging iets mis", "De toestellijst kon niet worden opgehaald.", NotificationType.Error, e.Exception);
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
                ShowNotification("belstatus", "Belopdracht mislukt.", NotificationType.Error, e.Exception);
                /*if (e.Exception != null)
                {
                    if (this.IsActive)
                    {
                        MessageBox.Show("Belopdracht mislukt.\n\n" + e.Exception.Message);
                    }
                    else
                    {
                        trayIcon.ShowBalloonTip("Belstatus", "Belopdracht mislukt." + e.Exception.Message, BalloonIcon.Error);
                    }
                }
                else
                {
                    if (this.IsActive)
                    {
                        MessageBox.Show("Belopdracht mislukt.");
                    }
                    else
                    {
                        trayIcon.ShowBalloonTip("Belstatus", "Belopdracht mislukt.", BalloonIcon.Error);
                    }
                }*/
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
                //trayIcon.ShowBalloonTip("Belstatus", "Bellen naar " + e.Given + "...", BalloonIcon.Info);
                ShowNotification("belstatus", "Bellen naar " + e.Given + "...", NotificationType.Info, NotificationMode.BalloonWhenInactiveOnly);
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
                    ShowNotification("er ging iets mis", "De nummerherkenningslijst is leeg. Neem contact op met Tam One.", NotificationType.Warning);
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
                    ShowNotification("er is iets mis", "Er zijn geen toestellen gekoppeld aan uw account. U kunt deze applicatie niet gebruiken.", NotificationType.Error, NotificationMode.MessageBoxOnly);
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
                    ShowNotification("er ging iets mis", "Inloggen mislukt", NotificationType.Error, e.Exception, NotificationMode.MessageBoxOnly);
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
                this.ActivateThoroughly();
                ShowSettings();
            }
            else if (cmdPrefixes.SelectedItem == null || cmdDevices.SelectedItem == null)
            {
                ShowNotification("oeps", "Selecteer een toestel en een nummerherkenningsoptie.", NotificationType.Info, NotificationMode.MessageBoxOnly);
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

        private void TrayMenu_ShowMainWindow(object sender, RoutedEventArgs e)
        {
            this.ActivateThoroughly();
        }

        private void TrayMenu_Shutdown(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        enum NotificationType
        {
            Info,
            Warning,
            Error
        }

        enum NotificationMode
        {
            MessageBoxOnly = 1,
            BalloonWhenInactiveOnly = 2,
            MessageBoxOrBalloon = 3
        }

        private void ShowNotification(string title, string message, NotificationType type, NotificationMode mode = NotificationMode.MessageBoxOrBalloon)
        {
            ShowNotification(title, message, type, null, mode);
        }

        private void ShowNotification(string title, string message, NotificationType type, Exception e, NotificationMode mode = NotificationMode.MessageBoxOrBalloon)
        {
            if (mode == NotificationMode.MessageBoxOnly || (this.IsActive && mode == NotificationMode.MessageBoxOrBalloon))
            {
                this.ActivateThoroughly();
                MessageBox.Show(this, message + (e != null ? ("\n\n" + e.Message) : ""), "Tam One Click: " + title, MessageBoxButton.OK, convertNotificationTypeToMessageBoxImage(type));
            }

            if (!this.IsActive && (mode == NotificationMode.BalloonWhenInactiveOnly || mode == NotificationMode.MessageBoxOrBalloon))
            {
                trayIcon.ShowBalloonTip("Tam One Click: " + title, message + (e != null ? ("\n\n" + e.Message) : ""), convertNotificationTypeToBalloonIcon(type));
            }
        }

        private MessageBoxImage convertNotificationTypeToMessageBoxImage(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Error: return MessageBoxImage.Error;
                case NotificationType.Info: return MessageBoxImage.Information;
                case NotificationType.Warning: return MessageBoxImage.Warning;
                default: return MessageBoxImage.None;
            }
        }

        private BalloonIcon convertNotificationTypeToBalloonIcon(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Error: return BalloonIcon.Error;
                case NotificationType.Info: return BalloonIcon.Info;
                case NotificationType.Warning: return BalloonIcon.Warning;
                default: return BalloonIcon.None;
            }
        }

        
    }
}
