using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PortalServer { get; set; }
        public bool AutoDial { get; set; }

        public Settings()
        {
            InitializeComponent();
#if PUBLIC
            lblPortalServer.Visibility = System.Windows.Visibility.Collapsed;
            txtPortalServer.Visibility = System.Windows.Visibility.Collapsed;
#endif
        }

        protected override void OnActivated(EventArgs e)
        {
            txtUsername.Text = Username ?? "";
            txtPassword.Password = Password ?? "";
            txtPortalServer.Text = PortalServer ?? "";
            chkAutoDial.IsChecked = AutoDial;
            base.OnActivated(e);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            txtPortalServer.Text = txtPortalServer.Text.Replace("http://","").Replace("https://", "").Replace("/", "");
            Username = txtUsername.Text;
            Password = txtPassword.Password;
            PortalServer = txtPortalServer.Text;
            AutoDial = chkAutoDial.IsChecked ?? false;
            Properties.Settings.Default.Username = txtUsername.Text;
            Properties.Settings.Default.Password = txtPassword.Password;
            Properties.Settings.Default.PortalServer = txtPortalServer.Text;
            Properties.Settings.Default.AutoDial = chkAutoDial.IsChecked ?? false;
            Properties.Settings.Default.Save();

            base.OnClosing(e);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

    }
}
