using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Data;

namespace PTLStation
{
    class CartBarcodesWrapPanel : WrapPanel
    {
        int id;
        OrderWindow wind;
        DataTable baskets_all;

        public CartBarcodesWrapPanel(OrderWindow window, bool addVisible, DataTable baskets)
        {
            baskets_all = baskets;
            wind = window;
            id = 1;

            Grid buttonGrid = new Grid();

            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

            Image plusImage = new Image() { Source = new BitmapImage(new Uri("pack://application:,,,/add.png")) };
            Grid.SetColumn(plusImage, 0);
            buttonGrid.Children.Add(plusImage);

            TextBlock dobavit = new TextBlock() { Text = "Добавить", VerticalAlignment = VerticalAlignment.Center };
            if (!addVisible)
            {
                dobavit.Visibility = Visibility.Collapsed;
            }
            Grid.SetColumn(dobavit, 1);
            buttonGrid.Children.Add(dobavit);

            Button addCart = new Button()
            {
                Content = buttonGrid,
                Height = 45,
                Focusable = false,
                Margin = new Thickness(2)
            };
            addCart.Click += (se, ev) =>
            {
                EnterCartBarcodeWindow ent = new EnterCartBarcodeWindow(window, baskets_all);
                ent.Show();
                ent.Closed += Ent_Closed;
            };
            Children.Add(addCart);
        }

        private void Ent_Closed(object sender, EventArgs e)
        {
            bool ok = true;
            if ((sender as EnterCartBarcodeWindow).barcode.Length < 13)
            {
                wind.IsEnabled = true;
                return;
            }

            foreach (CartBarcodeBorder cbb in Children.OfType<CartBarcodeBorder>())
            {
                if (cbb.Barcode == (sender as EnterCartBarcodeWindow).barcode)
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                wind.IsEnabled = true;
                AddCart((sender as EnterCartBarcodeWindow).barcode);
            }
            else
            {
                MessageBox.Show("Такая корзина уже присутствует!");
                EnterCartBarcodeWindow ent = new EnterCartBarcodeWindow(wind, baskets_all);
                ent.Show();
                ent.Closed += Ent_Closed;
            }
        }

        public void AddCart(string barcode)
        {
            AddExistingCart(barcode);
            (wind.Owner as MainWindow).sqlQueriesToSend.Add("INSERT INTO "
                    + "ptl_database.orderbaskets (basketBarcode, orderId, compId) VALUES ('" + barcode
                    + "', " + wind.orders.order_id + ", " + (wind.Owner as MainWindow).myID + ");");
            (wind.Owner as MainWindow).sqlQueriesToSend.Add("UPDATE "
                    + "ptl_database.orderbaskets SET status=0, orderId='" + wind.orders.order_id + "', compId=" + (wind.Owner as MainWindow).myID + " WHERE basketBarcode='" + barcode + "';");

        }

        public void AddExistingCart(string barcode)
        {
            CartBarcodeBorder crtb = new CartBarcodeBorder(id, barcode)
            {
                Height = 45,
                Margin = new Thickness(2)
            };

            //if (Children.Count <= 1)
            //{
                //crtb._isEN = false;
            //}
            //else
            //{
                foreach (CartBarcodeBorder cbb in Children.OfType<CartBarcodeBorder>())
                {
                    cbb._isEN = true;
                }
            //}

            crtb.deleteMe += (se, ev) =>
            {
                RemoveCart(se as CartBarcodeBorder);
            };
            Children.Insert(Children.Count - 1, crtb);
            id++;
        }

        public void RemoveCart(CartBarcodeBorder crtb)
        {
            (wind.Owner as MainWindow).sqlQueriesToSend.Add("DELETE FROM orderbaskets WHERE basketBarcode="
                       + crtb.Barcode + " AND orderId=" + wind.orders.order_id + ";");

            Children.Remove(crtb);

            /*int id = 1;
            foreach (CartBarcodeBorder cbb in Children.OfType<CartBarcodeBorder>())
            {
                cbb.ID = id;
                if (Children.OfType<CartBarcodeBorder>().Count() == 1)
                {
                    cbb._isEN = false;
                }
                id++;
            }
            this.id = id;*/

        }
    }
}
