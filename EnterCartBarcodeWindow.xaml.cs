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
    public partial class EnterCartBarcodeWindow : Window
    {
        public string barcode;
        private List<string> baskets;

        public EnterCartBarcodeWindow(Window window, DataTable baskets_all)
        {
            baskets = new List<string>();
            foreach (DataRow row in baskets_all.Rows)
            {
                baskets.Add(row[0].ToString());
            }
            InitializeComponent();
            Owner = window;
            this.Loaded += (se, ev) =>
            {
                Owner.IsEnabled = false;
                Keyboard.Focus(textBox);
            };
            barcode = "";
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).Text.Contains("'"))
            {
                int t = ((TextBox)sender).CaretIndex;
                ((TextBox)sender).Text = new string(((TextBox)sender).Text.Where(c => (c != '\'')).ToArray());
                ((TextBox)sender).CaretIndex = Math.Max(0, t - 1);
                return;
            }
            if (((TextBox)sender).Text.Length >= 13)
            {
                int found = 0;
                foreach (string bas in baskets)
                {
                    if (((TextBox)sender).Text == bas)
                    {
                        found = 1;
                        barcode = ((TextBox)sender).Text;
                        Owner.IsEnabled = true;
                        Close();
                        break;
                    }
                }
                if (found == 0)
                {
                    MessageBox.Show("Бесеттің коды дұрыс емес");
                }

                //(Owner as OrderWindow).listBox.Items.Add(new TextBlock() { Text = ((TextBox)sender).Text });
                /*((Owner as OrderWindow).Owner as MainWindow).sqlQueriesToSend.Add("INSERT INTO "
                    + "ptl_database.order_baskets (basket_barcode, order_id, comp_id) VALUES ('" + ((TextBox)sender).Text
                    + "', "+ (Owner as OrderWindow).orders.order_id + ", " + ((Owner as OrderWindow).Owner as MainWindow).myID + ");");*/
                
            }
        }
    }
}
