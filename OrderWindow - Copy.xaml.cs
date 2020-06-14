using MySql.Data.MySqlClient;
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
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        //Order myOrder;
        public bool InProcess { get; set; }
        private DataTable orders;
        public int priority { get; set; }
        private DataTable PTLs;
        private string order_name;
        private int order_id;
        public event EventHandler HideMe;
        OrderContentItem expandedItem;

        public int ItemCount
        {
            get
            {
                return orders.Rows.Count;
            }
        }

        public string OrderName
        {
            get
            {
                return order_name;
            }
        }

        public int OrderID
        {
            get
            {
                return order_id;
            }
        }

        private OrderBorder my_border;

        private bool _conf;
        bool Confirmed
        {
            get { return _conf; }
            set
            {
                _conf = value;
                if (value)
                {
                    finish_button.IsEnabled = true;
                    (myBorder.Parent as StackPanel).Children.Remove(myBorder);
                }
            }
        }

        /// <summary>
        /// The border element associated with this order displayed on the stack panel in the main window
        /// </summary>
        public OrderBorder myBorder
        {
            get
            {
                return my_border;
            }
            set
            {
                my_border = value;
                my_border.orderName = "Заказ " + OrderName;
                my_border.quantity = ItemCount;
            }
        }

        public DataTable existingCarts
        {
            set
            {
                foreach (DataRow s in value.Rows)
                {
                    listBox.Items.Add(new TextBlock() { Text = s[1].ToString() });
                }
            }
        }

        public void Open()
        {
            this.Show();
            if (listBox.Items.Count == 0)
            {
                EnterCartBarcodeWindow ent = new EnterCartBarcodeWindow(this);
                ent.Closed += (sen, e) =>
                {
                    if (!IsEnabled)
                        HideMe?.Invoke(this, new EventArgs());
                    else
                    {
                        if (!Start())
                            HideMe?.Invoke(this, new EventArgs());
                    }
                };
                ent.Show();
            }
            else
            {
                this.IsEnabled = true;
                if (!Start())
                    HideMe?.Invoke(this, new EventArgs());
            }
        }

        public OrderWindow(DataTable orderTable, DataTable PTLS, string order_name, int order_id)
        {
            InitializeComponent();
            expandedItem = null;
            InProcess = false;
            orders = orderTable;
            PTLs = PTLS;
            this.order_name = order_name;
            this.order_id = order_id;

            this.Owner = (MainWindow)Application.Current.MainWindow;
            this.ResizeMode = ResizeMode.NoResize;
            this.Closing += (se, e) =>
            {
                if (!Confirmed && !(Owner as MainWindow).close)
                {
                    MessageBox.Show("Заказ не собран!");
                    e.Cancel = true;
                }
            };

            orderName.Text = order_name;
            int i = 1;
            OrdersPanel.Visibility = Visibility.Collapsed;
            string[] s = { "" };
            foreach (DataRow row in orders.Rows)
            {
                //

                OrderContentItem ordc = new OrderContentItem(i, Convert.ToInt32(row[1]), s, row[7].ToString(), row[6].ToString(), row[3].ToString(), Convert.ToInt32(row[2]));

                if (Convert.ToInt32(row[4]) == 0)
                {
                    ordc.QuantityCollected = Convert.ToInt32(row[2]);
                }
                else
                {
                    ordc.Confirmed = true;
                    ordc.QuantityCollected = Convert.ToInt32(row[5]);
                }
                ordc.ShowButtonPressed += (se, ev) =>
                {
                    DataRow[] r = PTLs.Select("Global = " + (se as OrderContentItem).RefID);
                    if (r.Length > 0)
                    {
                        ConfirmORDC(se as OrderContentItem, r, (se as OrderContentItem).QuantityCollected.ToString());
                        ((MainWindow)Application.Current.MainWindow).ptlconn.ClearNode(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]));

                        DataRow[] newrows = PTLs.Select("Gateway = " + r[0][2] + "AND ID = " + r[0][3]);

                        foreach (DataRow newrow in newrows)
                        {
                            DataRow[] dr = orders.Select("PTL = " + newrow[1]);
                            if (dr.Length > 0 && !Convert.ToBoolean(dr[0][4]))
                            {
                                Direction dir = Direction.Up;
                                if (Convert.ToBoolean(newrow[4]))
                                    dir = Direction.Down;
                                ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(newrow[2]), Convert.ToInt32(newrow[3]), LEDColor.Blue, LEDStatus.On);
                                ((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(newrow[2]), Convert.ToInt32(newrow[3]), Convert.ToInt32(dr[0][2]), dir, 0, 0);
                            }
                        }
                    }
                };
                ordc.ExpandMe += (se, ev) =>
                {
                    if (expandedItem != null)
                    {
                        expandedItem.Expanded = false;
                    }
                    expandedItem = (se as OrderContentItem);
                    expandedItem.Expanded = true;
                };
                OrdersPanel.Children.Add(ordc);
                i++;
            }
        }

        private void add_cart_Click(object sender, RoutedEventArgs e)
        {
            EnterCartBarcodeWindow ent = new EnterCartBarcodeWindow(this);
            ent.Show();
            ent.Closed += (se, ev) =>
            {
                IsEnabled = true;
            };
        }

        private void remove_cart_Click(object sender, RoutedEventArgs e)
        {
            if (listBox.Items.Count > 1)
            {
                (Owner as MainWindow).sqlQueriesToSend.Add("DELETE FROM order_baskets WHERE basket_barcode="
                    + (listBox.SelectedItem as TextBlock).Text + " AND order_id=" + OrderID + ";");
                listBox.Items.Remove(listBox.SelectedItem as TextBlock);
            }
        }

        private void finish_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public bool Start()
        {
            foreach (DataRow row in orders.Rows)
            {
                if (Convert.ToInt32(row[4]) == 0)
                {
                    DataRow[] r = PTLs.Select("Global = " + row[1]);
                    if (r.Length > 0)
                    {
                        Direction dir = Direction.Up;
                        if (Convert.ToBoolean(r[0][4]))
                            dir = Direction.Down;
                        if (!((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Blue, LEDStatus.On))
                            return false;
                        if (!((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), Convert.ToInt32(row[2]), dir, 0, 0))
                            return false;
                    }
                }
            }
            OrdersPanel.Visibility = Visibility.Visible;
            return true;
        }

        private void ConfirmORDC(OrderContentItem ordc, DataRow[] r, string message)
        {
            DataRow[] r2 = orders.Select("PTL = " + r[0][1]);
            if (r2.Length > 0)
            {
                r2[0][4] = 1;
                r2[0][5] = message.Trim();
                (this.Owner as MainWindow).sqlQueriesToSend.Add("UPDATE ptl_database.orders_description SET status=1, act_quant=" + message.Trim()
                        + " WHERE description_id=" + r2[0][0] + ";");
            }

            ordc.Confirmed = true;
            bool all_confirmed = true;
            foreach (OrderContentItem o in OrdersPanel.Children.OfType<OrderContentItem>())
            {
                if (!o.Confirmed)
                    all_confirmed = false;
            }
            if (all_confirmed)
                this.Confirmed = true;
        }

        /// <summary>
        /// Receiving message from Gateway: the confirmation and shortage.
        /// </summary>
        /// <param name="gateway"></param>
        /// <param name="node"></param>
        /// <param name="direction"></param>
        /// <param name="message"></param>
        public void ReceiveMess(int gateway, int node, Direction direction, string message)
        {
            char s = 's';
            if (direction == Direction.Up)
            {
                s = '0';
            }
            else if (direction == Direction.Down)
            {
                s = '1';
            }
            if (s != 's')
            {
                DataRow[] r = PTLs.Select("Gateway = " + gateway + " AND ID = " + node + " AND up_down = " + s);
                if (r.Length > 0)
                {
                    foreach (OrderContentItem ordc in OrdersPanel.Children.OfType<OrderContentItem>())
                    {
                        if (ordc.RefID == Convert.ToInt32(r[0][1]))
                        {
                            ordc.QuantityCollected = Convert.ToInt32(message.Trim());
                            ConfirmORDC(ordc, r, message);
                        }
                    }

                }
            }
        }
    }
}
