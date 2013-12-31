using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TamOne_Dialer
{
    /// <summary>
    /// Interaction logic for UserSettings.xaml
    /// </summary>
    public partial class UserSettings : Window
    {
        public UserSettings()
        {
            InitializeComponent();
        }

        public PortalSession Session { get; private set; }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (btnLogin.Content.ToString() == "Inloggen")
            {
                lblNotice.Visibility = Visibility.Visible;
                lblNotice.Content = "Bezig met inloggen...";
                btnLogin.IsEnabled = false;
                PortalSession.BeginLogin(txtUsername.Text, txtPassword.Password, (PortalSession.LoginResult)delegate(PortalSession p)
                {
                    this.Dispatcher.Invoke((Action)delegate()
                        {
                            if (p != null)
                            {
                                Session = p;
                                lblNotice.Content = "Ingelogd.";
                                btnLogin.Content = "Uitloggen";
                                btnLogin.IsEnabled = false;
                                
                                p.BeginGetDeviceList((PortalSession.DeviceListResult)delegate(ICollection<Device> devices)
                                {
                                    this.Dispatcher.Invoke((Action)delegate()
                                    {
                                        cmdDevices.Items.Clear();
                                        foreach (var device in devices)
                                        {
                                            cmdDevices.Items.Add(device.Name);
                                            grpToestel.Visibility = Visibility.Visible;
                                        }
                                        cmdDevices.SelectedIndex = 0;
                                    });
                                    
                                    
                                });

                            }
                            else
                            {
                                lblNotice.Visibility = Visibility.Visible;
                                //lblNotice.Content = "Inloggen mislukt";
                                btnLogin.IsEnabled = true;
                            }
                        });
                });
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Session.Device = new Device((string)cmdDevices.SelectedItem, (string)cmdDevices.SelectedItem);
            this.Close();
            //this.Visibility = Visibility.Hidden;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
