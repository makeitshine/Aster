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
        public Order orders;
        public int priority { get; set; }
        private DataTable PTLs;
        //private string order_name;
        //private int order_id;
        //public event EventHandler HideMe;
        OrderContentItem expandedItem;

        /*public int ItemCount
        {
            get
            {
                return orders.Rows.Count;
            }
        }*/

        /*public string OrderName
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
        }*/

        //private OrderBorder my_border;

        private bool _conf;
        public bool Confirmed
        {
            get { return _conf; }
            set
            {
                _conf = value;
                if (value)
                {
                    finish_button.IsEnabled = true;
                    //(myBorder.Parent as StackPanel).Children.Remove(myBorder);
                }
            }
        }

        /// <summary>
        /// The border element associated with this order displayed on the stack panel in the main window
        /// </summary>
        /*public OrderBorder myBorder
        {
            get
            {
                return my_border;
            }
            set
            {
                my_border = value;
                my_border.orderName = "Заказ " + orders.TableName;
                my_border.quantity = ItemCount;
            }
        }*/

        /*public DataTable existingCarts
        {
            set
            {
                foreach (DataRow s in value.Rows)
                {
                    listBox.Items.Add(new TextBlock() { Text = s[1].ToString() });
                }
            }
        }*/

        public void Open()
        {
            this.Show();
            if (listBox.Items.Count == 0)
            {
                EnterCartBarcodeWindow ent = new EnterCartBarcodeWindow(this);
                ent.Closed += (sen, e) =>
                {
                    if (!IsEnabled)
                    {
                        Close();
                    }
                    //HideMe?.Invoke(this, new EventArgs());
                    else
                    {
                        if (!Start())
                        {
                            Close();
                        }
                        //HideMe?.Invoke(this, new EventArgs());
                        else
                        {
                            (Owner as MainWindow).sqlQueriesToSend.Add("INSERT INTO "
                                + "ptl_database.order_baskets (basket_barcode, order_id, comp_id) VALUES ('" + ((EnterCartBarcodeWindow)sen).barcode
                                + "', " + orders.order_id + ", " + (Owner as MainWindow).myID + ");");
                        }
                    }
                };
                ent.Show();
            }
            else
            {
                this.IsEnabled = true;
                if (!Start())
                {
                    Close();
                }
                //HideMe?.Invoke(this, new EventArgs());
            }
        }

        List<Tuple<bool, int, string>>[] action_list;
        DataTable dt;

        public OrderWindow(Order orderTable, DataTable PTLS)
        {
            InitializeComponent();
            expandedItem = null;
            InProcess = false;
            orders = orderTable;
            PTLs = PTLS;

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

            foreach (DataRow row in orders.existingCarts.Rows)
            {
                listBox.Items.Add(new TextBlock() { Text = row[1].ToString() });
            }

            string[] st = { "Gateway", "ID" };

            dt = PTLs.DefaultView.ToTable(true, st);
            dt.Columns[0].ColumnName = "Gateway";
            dt.Columns[1].ColumnName = "ID";

            action_list = new List<Tuple<bool, int, string>>[dt.Rows.Count];
            for (int ind = 0; ind < dt.Rows.Count; ind++)
            {
                action_list[ind] = new List<Tuple<bool, int, string>>();
            }

            foreach (DataRow row in orders.Rows)
            {
                if (Convert.ToInt32(row[4]) == 0)
                {
                    DataRow[] r = PTLs.Select("Global = " + row[1]);
                    if (r.Length > 0)
                    {
                        DataRow[] rw = dt.Select("Gateway = " + r[0][2] + " AND ID = " + r[0][3]);
                        if (rw.Length > 0)
                        {
                            int index = dt.Rows.IndexOf(rw[0]);

                            int max_exp = 6;

                            int month = DateTime.Now.Month;
                            int year = DateTime.Now.Year % 2000;

                            int thisdate = (Convert.ToInt32(row[3]) - month) + 12 * (Convert.ToInt32(row[8]) - year);

                            if (thisdate <= max_exp && thisdate >=0)
                            {
                                Tuple<bool, int, string> t = new Tuple<bool, int, string>(false, Convert.ToInt32(row[0]), (row[3].ToString() + "-" + row[8].ToString()));
                                action_list[index].Add(t);
                            }
                            Tuple<bool, int, string> t2 = new Tuple<bool, int, string>(true, Convert.ToInt32(row[0]), row[2].ToString());
                            action_list[index].Add(t2);

                        }
                    }
                }
            }

            orderName.Text = orders.TableName;
            int i = 1;
            OrdersPanel.Visibility = Visibility.Collapsed;
            string[] s = { "" };
            foreach (DataRow row in orders.Rows)
            {
                //

                OrderContentItem ordc = new OrderContentItem(i, Convert.ToInt32(row[0]), s, row[7].ToString(), row[6].ToString(), (row[3].ToString() + "-" + row[8].ToString()), Convert.ToInt32(row[2]), Convert.ToInt32(row[1]));

                if (Convert.ToInt32(row[4]) == 0)
                {
                    ordc.QuantityCollected = Convert.ToInt32(row[2]);
                }
                else
                {
                    ordc.status = Status.confirmed;
                    ordc.QuantityCollected = Convert.ToInt32(row[5]);
                }
                ordc.ShowButtonPressed += (se, ev) =>
                {
                    DataRow[] order_row = orders.Select("Desc_id = " + (se as OrderContentItem).RefID);

                    if (order_row.Length > 0)
                    {
                        ConfirmORDC(se as OrderContentItem, order_row, (se as OrderContentItem).QuantityCollected.ToString());
                    }

                    DataRow[] r = PTLs.Select("Global = " + (se as OrderContentItem).PTL_global);
                    if (r.Length > 0)
                    {
                        //ConfirmORDC(se as OrderContentItem, r, (se as OrderContentItem).QuantityCollected.ToString());
                        ((MainWindow)Application.Current.MainWindow).ptlconn.ClearNode(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]));

                        DataRow[] rw = dt.Select("Gateway = " + r[0][2] + " AND ID = " + r[0][3]);
                        if (rw.Length > 0)
                        {
                            int index = dt.Rows.IndexOf(rw[0]);
                            int c = action_list[index].Count;

                            for (int ind = c - 1; ind >= 0; ind--)
                            {
                                if (action_list[index][ind].Item2 == (se as OrderContentItem).RefID)
                                {
                                    action_list[index].RemoveAt(ind);
                                }
                            }
                            /*for (int ind = 0; ind < c; ind++)
                            {
                                Tuple<bool, int, string> t = action_list[index].Dequeue();
                                if (t.Item2 != (se as InventoryContentItem).RefID)
                                {
                                    action_list[index].Enqueue(t);
                                }
                            }*/
                            if (action_list[index].Count > 0)
                                SendToPTL(action_list[index][0]);
                        }

                        /*DataRow[] newrows = PTLs.Select("Gateway = " + r[0][2] + "AND ID = " + r[0][3]);

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
                        }*/
                    }
                };
                /*ordc.ExpandMe += (se, ev) =>
                {
                    if (expandedItem != null)
                    {
                        expandedItem.Expanded = false;
                    }
                    expandedItem = (se as OrderContentItem);
                    expandedItem.Expanded = true;
                };*/
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
                    + (listBox.SelectedItem as TextBlock).Text + " AND order_id=" + orders.order_id + ";");
                listBox.Items.Remove(listBox.SelectedItem as TextBlock);
            }
        }

        private void finish_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void SendToPTL(Tuple<bool, int, string> act)
        {
            foreach (OrderContentItem ordc in OrdersPanel.Children.OfType<OrderContentItem>())
            {
                if (ordc.RefID == act.Item2)
                {
                    DataRow[] r = PTLs.Select("Global = " + ordc.PTL_global);
                    if (r.Length > 0)
                    {
                        Direction dir = Direction.Up;
                        if (Convert.ToBoolean(r[0][4]))
                            dir = Direction.Down;
                        if (!act.Item1)
                        {
                            ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Blue, LEDStatus.Blink_1sec);
                            ((MainWindow)Application.Current.MainWindow).ptlconn.DisplaySeries(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), act.Item3, dir, 0, 0);
                            ordc.status = Status.series_sent;
                        }
                        else
                        {
                            ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Blue, LEDStatus.On);
                            ((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), Convert.ToInt32(act.Item3), dir, 0, 0);
                            ordc.status = Status.series_confirmed;
                        }
                    }
                    break;
                }
            }
        }

        public bool Start()
        {
            foreach (List<Tuple<bool, int, string>> act_q in action_list)
            {
                if (act_q.Count > 0)
                {
                    Tuple<bool, int, string> act = act_q[0];
                    SendToPTL(act);
                }
            }
            OrdersPanel.Visibility = Visibility.Visible;
            return true;

            /*foreach (DataRow row in orders.Rows)
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
                        { }// return true;
                        if (!((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), Convert.ToInt32(row[2]), dir, 0, 0))
                        { }   //return true;
                    }
                }
            }
            OrdersPanel.Visibility = Visibility.Visible;
            return true;*/
        }

        private void ConfirmORDC(OrderContentItem ordc, DataRow[] order_row, string message)
        {
            //DataRow[] r2 = orders.Select("PTL = " + r[0][1]);
            /*if (r2.Length > 0)
            {*/
            order_row[0][4] = 1;
            order_row[0][5] = Convert.ToInt32(message.Trim());
                (this.Owner as MainWindow).sqlQueriesToSend.Add("UPDATE ptl_database.orders_description SET status=1, act_quant=" + message.Trim()
                        + ", user_id=" + (this.Owner as MainWindow).currentUser + " WHERE description_id=" + order_row[0][0] + ";");
            //}
            ordc.status = Status.confirmed;
            //ordc.Confirmed = true;
            bool all_confirmed = true;
            foreach (OrderContentItem o in OrdersPanel.Children.OfType<OrderContentItem>())
            {
                if (o.status != Status.confirmed)
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
                        /*if (ordc.RefID == Convert.ToInt32(r[0][1]))
                        {
                            ordc.QuantityCollected = Convert.ToInt32(message.Trim());
                            ConfirmORDC(ordc, r, message);
                        }*/
                        if (ordc.PTL_global == Convert.ToInt32(r[0][1]))
                        {
                            DataRow[] rw = dt.Select("Gateway = " + gateway + " AND ID = " + node);
                            if (ordc.status == Status.series_sent)
                            {
                                if (rw.Length > 0)
                                {
                                    int index = dt.Rows.IndexOf(rw[0]);
                                    action_list[index].RemoveAt(0);
                                    if (action_list[index].Count > 0)
                                    {
                                        SendToPTL(action_list[index][0]);
                                    }
                                }
                                break;
                            }
                            else if (ordc.status == Status.series_confirmed)
                            {
                                ordc.QuantityCollected = Convert.ToInt32(message.Trim());
                                DataRow[] r2 = orders.Select("PTL = " + r[0][1]);
                                if (r2.Length > 0)
                                {
                                    ConfirmORDC(ordc, r2, message);
                                }
                                if (rw.Length > 0)
                                {
                                    int index = dt.Rows.IndexOf(rw[0]);
                                    action_list[index].RemoveAt(0);
                                    if (action_list[index].Count > 0)
                                    {
                                        SendToPTL(action_list[index][0]);
                                    }
                                }
                                break;
                            }
                        }
                    }

                }
            }
        }
    }
}
