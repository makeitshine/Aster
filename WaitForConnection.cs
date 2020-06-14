using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Timers;
using System.Diagnostics;
using System.ComponentModel;

namespace PTLStation
{
    class WaitForConnection : Popup // The popup window is displayed when waiting for connection to database
    {
        //public event EventHandler ClosePressed;

        public WaitForConnection(string waitString) 
        {
            Border brdr = new Border()
            {
                Width = 400,
                Background = Brushes.White,
                BorderThickness = new Thickness(1),
                Height = 200
            };
            this.Placement = PlacementMode.Relative;
            this.PlacementTarget = (MainWindow)Application.Current.MainWindow;
            this.HorizontalOffset = (((MainWindow)Application.Current.MainWindow).ActualWidth - 400) / 2;
            this.VerticalOffset = (((MainWindow)Application.Current.MainWindow).ActualHeight - 200) / 3;
            
            this.Child = brdr;

            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Vertical;
            brdr.Child = sp;
            sp.VerticalAlignment = VerticalAlignment.Center;
            TextBlock txt = new TextBlock()
            {
                Text = waitString,
                Margin = new Thickness(10),
                FontSize = 19,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            sp.Children.Add(txt);

            Button btn = new Button();
            sp.Children.Add(btn);
            btn.Content = "Закрыть";
            btn.Margin = new Thickness(10);
            btn.HorizontalAlignment = HorizontalAlignment.Center;
            btn.FontSize = 16;
            btn.Click += (se, ev) =>
            {
                //ClosePressed(this, new EventArgs());
                //Process.Start("explorer.exe");
                Application.Current.Shutdown();
            };

        }
    }
}
