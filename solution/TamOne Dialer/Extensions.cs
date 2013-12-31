using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace TamOne_Dialer
{
    public static class Extensions
    {
        public static void ActivateThoroughly(this Window window)
        {
            if (!window.IsVisible)
            {
                window.Show();
            }

            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }

            window.Activate();
            window.Topmost = true;
            window.Topmost = false;
            window.Focus();
        }
    }
}
