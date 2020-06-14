using System;
using System.Collections.Generic;
using System.Data;
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

namespace PTLStation
{
    /// <summary>
    /// Interaction logic for EnterCartBarcodeWindow.xaml
    /// </summary>
    public partial class LogInWindow : Window
    {
        public int userID;
        public string barcode;
        DataTable Users;

        public LogInWindow(Window window, DataTable users)
        {
            this.Left = (SystemParameters.PrimaryScreenWidth - 400) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - 180) / 3;
            userID = -1;
            InitializeComponent();
            Users = users;
            Owner = window;
            this.Loaded += (se, ev) =>
            {
                Owner.IsEnabled = false;
                Keyboard.Focus(userName);
            };
            this.Closing += (se, ev) =>
            {
                if (userID == -1)
                {
                    //ev.Cancel = true;
                    //Keyboard.Focus(userName);
                    Owner.Close();
                }
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
            try
            {
                DataRow[] r = Users.Select("barcode = '" + ((TextBox)sender).Text + "'");
                if (r.Length > 0)
                {
                    userID = Convert.ToInt32(r[0][0]);
                    this.Close();
                }
            }
            catch
            {

            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] r = Users.Select("username = '" + userName.Text + "'");
                if (r.Length > 0)
                {
                    if (r[0][2].ToString() == password.Password)
                    {
                        this.userID = Convert.ToInt32(r[0][0]);
                        this.Close();
                    }
                }
                else
                {
                    userName.Text = "";
                    password.Password = "";
                    Keyboard.Focus(userName);
                }
            }
            catch
            {

            }
        }
    }
}
