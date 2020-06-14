using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;

namespace PTLStation
{
    /// <summary>
    /// Interaction logic for EnterCartBarcodeWindow.xaml
    /// </summary>
    public partial class EnterCartBarcode_forReplenishment : Window
    {
        public event EventHandler<BarcodeEnteredEventArgs> BarcodeEnetered;
        

        public EnterCartBarcode_forReplenishment(Window window)
        {
            
            InitializeComponent();
            Owner = window;
            this.Loaded += (se, ev) =>
            {
                Owner.IsEnabled = false;
                Keyboard.Focus(textBox);
            };
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Length >= 13)
            {
                
                Close();
                BarcodeEnetered?.Invoke(this, new BarcodeEnteredEventArgs() { barcode = ((TextBox)sender).Text });
                    
                
            }
        }
    }

    public class BarcodeEnteredEventArgs : EventArgs
    {
        public string barcode;
    }
}
