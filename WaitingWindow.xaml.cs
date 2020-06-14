using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PTLStation
{
    /// <summary>
    /// Interaction logic for WaitingWindow.xaml
    /// </summary>
    /// 

    public partial class WaitingWindow : Window
    {
        public bool isReady;

        /// <summary>
        /// Waiting window is displayed while waiting for connection.
        /// </summary>
        public WaitingWindow()
        {
            this.Owner = (MainWindow)Application.Current.MainWindow;
            isReady = false;
            InitializeComponent();
            this.Left = (SystemParameters.PrimaryScreenWidth - 400) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - 200) / 3;
            this.ResizeMode = ResizeMode.NoResize;
            this.Closing += (se, ev) =>
            {
                if (!isReady)
                    Application.Current.Shutdown();
            };
            this.Loaded += WaitingWindow_Loaded;
        }

        private void WaitingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
