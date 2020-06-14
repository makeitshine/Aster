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
using System.IO;
using System.Diagnostics;

namespace PTLStation
{
    /// <summary>
    /// Interaction logic for OrderWindow.xaml
    /// </summary>
    public partial class ReplenishmentWindow : Window
    {
        private NewComing replenishTable;
        public int priority { get; set; }
        private DataTable PTLs;
        private string cart_barcode;
        //private int order_id;
        //ReplenishContentItem expandedItem;

        public NewComing TableReplenishment
        {
            get
            {
                return replenishTable;
            }
        }

        public int ItemCount
        {
            get
            {
                return replenishTable.Rows.Count;
            }
        }

        public string CartBarcode
        {
            get
            {
                return cart_barcode;
            }
        }

        /*public int OrderID
        {
            get
            {
                return order_id;
            }
        }*/

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
                }
            }
        }

        List<Tuple<int, int, string>>[] action_list;
        DataTable dt;

        public ReplenishmentWindow(NewComing replenishTable, DataTable PTLS)
        {
            InitializeComponent();
            //expandedItem = null;
            cart_barcode = replenishTable.TableName;
            this.replenishTable = replenishTable;
            PTLs = PTLS;

            this.Owner = (MainWindow)Application.Current.MainWindow;
            this.ResizeMode = ResizeMode.NoResize;
            this.Closing += (se, e) =>
            {
                if (!Confirmed && !(Owner as MainWindow).close)
                {
                    MessageBox.Show("Тауарлар толығымен орналастырылмады!");
                }
            };
            this.Loaded += (se, ev) =>
            {
                Owner.IsEnabled = false;
                if (!Start())
                    Close();
            };

            string[] st = { "Gateway", "ID" };

            dt = PTLs.DefaultView.ToTable(true, st);
            dt.Columns[0].ColumnName = "Gateway";
            dt.Columns[1].ColumnName = "ID";

            action_list = new List<Tuple<int, int, string>>[dt.Rows.Count];
            for (int ind = 0; ind < dt.Rows.Count; ind++)
            {
                action_list[ind] = new List<Tuple<int, int, string>>();
            }



            foreach (DataRow row in replenishTable.Rows)
            {
                if (Convert.ToInt32(row[3]) == 0)
                {
                    DataRow[] r = PTLs.Select("Global = " + row[1]);
                    if (r.Length > 0)
                    {
                        DataRow[] rw = dt.Select("Gateway = " + r[0][2] + " AND ID = " + r[0][3]);
                        if (rw.Length > 0)
                        {
                            int index = dt.Rows.IndexOf(rw[0]);
                            string short_series = row[9].ToString().Substring(Math.Max(0, row[9].ToString().Length - 4));
                            Tuple<int, int, string> t2 = new Tuple<int, int, string>(Convert.ToInt32(row[0]), Convert.ToInt32(row[2]), short_series);
                            action_list[index].Add(t2);

                        }
                    }
                }
            }


            orderTab.Header = "Корзина " + cart_barcode;
            int i = 1;
            //OrdersPanel.Visibility = Visibility.Collapsed;
            string[] s = { "" };
            foreach (DataRow row in replenishTable.Rows)
            {
                ReplenishContentItem ordc = new ReplenishContentItem(i, Convert.ToInt32(row[0]), s, row[6].ToString(), row[5].ToString(), row[9].ToString(), Convert.ToInt32(row[2]), Convert.ToInt32(row[1]));

                if (Convert.ToInt32(row[3]) == 0)
                {
                    ordc.QuantityReplenished = Convert.ToInt32(row[2]);
                }
                else if (Convert.ToInt32(row[3]) == 1)
                {
                    ordc.Confirmed = true;
                    ordc.QuantityReplenished = Convert.ToInt32(row[4]);
                }
                else if (Convert.ToInt32(row[3]) == 2)
                {
                    ordc.QuantityReplenished = Convert.ToInt32(row[4]);
                    ordc.Expanded = true;
                }
                ordc.ShowButtonPressed += (se, ev) =>
                {
                    DataRow[] r2 = replenishTable.Select("Desc_id = " + (se as ReplenishContentItem).RefID);
                    if (r2.Length > 0)
                    {
                        DataRow[] r = PTLs.Select("Global = " + r2[0][1]);
                        if (r.Length > 0)
                        {
                            ConfirmORDC(se as ReplenishContentItem, r2, (se as ReplenishContentItem).QuantityReplenished.ToString(), r);
                            //((MainWindow)Application.Current.MainWindow).ptlconn.ClearNode(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]));

                            /*DataRow[] newrows = PTLs.Select("Gateway = " + r[0][2] + "AND ID = " + r[0][3]);

                            foreach (DataRow newrow in newrows)
                            {
                                DataRow[] dr = replenishTable.Select("PTL = " + newrow[1]);
                                if (dr.Length > 0 && !Convert.ToBoolean(dr[0][3]))
                                {
                                    Direction dir = Direction.Up;
                                    if (Convert.ToBoolean(newrow[4]))
                                        dir = Direction.Down;
                                    ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(newrow[2]), Convert.ToInt32(newrow[3]), LEDColor.Amber, LEDStatus.On);
                                    ((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(newrow[2]), Convert.ToInt32(newrow[3]), Convert.ToInt32(dr[0][2]), dir, 0, 0);
                                }
                            }*/
                        }
                    }
                };
                ordc.ConfirmButtonPressed += (se, ev) =>
                {
                    DataRow[] r2 = replenishTable.Select("Desc_id = " + (se as ReplenishContentItem).RefID);
                    if (r2.Length > 0)
                    {
                        DataRow[] r = PTLs.Select("Global = " + r2[0][1]);
                        if (r.Length > 0)
                        {
                            ConfirmPodp((se as ReplenishContentItem), r2, (se as ReplenishContentItem).QuantityPodpitka.ToString(), r);
                        }
                    }
                };
                ordc.CancelButtonPressed += (se, ev) => // HERE
                {
                    ordc.cancellation = true;
                    DataRow[] r2 = replenishTable.Select("Desc_id = " + (se as ReplenishContentItem).RefID);
                    if (r2.Length > 0)
                    {
                        r2[0][3] = 1;
                        (this.Owner as MainWindow).sqlQueriesToSend.Add("UPDATE ptl_database.newcomingsdescription "
                            + "SET status=1, statusPotpitka=0 WHERE idNewComingsDescription=" + r2[0][0] + ";");
                        ordc.Confirmed = true;

                        DataRow[] PTLrow = PTLs.Select("Global = " + r2[0][1]);
                        if (PTLrow.Length > 0)
                        {
                            DataRow[] rw = dt.Select("Gateway = " + PTLrow[0][2] + " AND ID = " + PTLrow[0][3]);
                            if (rw.Length > 0)
                            {
                                ((MainWindow)Application.Current.MainWindow).ptlconn.ClearNode(Convert.ToInt32(PTLrow[0][2]), Convert.ToInt32(PTLrow[0][3]));
                                int index = dt.Rows.IndexOf(rw[0]);
                                if (action_list[index].Count > 0)
                                    action_list[index].RemoveAt(0);
                                if (action_list[index].Count > 0)
                                {
                                    SendToPTL(action_list[index][0]);
                                }
                            }
                        }
                    }


                    bool all_confirmed = true;
                    foreach (ReplenishContentItem o in OrdersPanel.Children.OfType<ReplenishContentItem>())
                    {
                        if (!o.Confirmed)
                            all_confirmed = false;
                    }
                    if (all_confirmed)
                        this.Confirmed = true;
                };
                /*ordc.ExpandMe += (se, ev) =>
                {
                    if (expandedItem != null)
                    {
                        expandedItem.Expanded = false;
                    }
                    expandedItem = (se as ReplenishContentItem);
                    expandedItem.Expanded = true;
                };*/
                OrdersPanel.Children.Add(ordc);
                i++;
            }
        }

        private void finish_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }



        void SendToPTL(Tuple<int, int, string> act)
        {
            foreach (ReplenishContentItem ordc in OrdersPanel.Children.OfType<ReplenishContentItem>())
            {
                if (ordc.RefID == act.Item1)
                {
                    DataRow[] r = PTLs.Select("Global = " + ordc.PTL_global);
                    if (r.Length > 0)
                    {
                        Direction dir = Direction.Up;
                        if (Convert.ToBoolean(r[0][4]))
                            dir = Direction.Down;
                        /*if (!act.Item1)
                        {*/
                        ((MainWindow)Application.Current.MainWindow).ptlconn.ClearNode(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]));
                        ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Red, LEDStatus.On);
                        ((MainWindow)Application.Current.MainWindow).ptlconn.DisplaySeries(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), act.Item3.ToString());



                        //((MainWindow)Application.Current.MainWindow).ptlconn.ClearNode(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]));
                        ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Amber, LEDStatus.On);
                        ((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), Convert.ToChar(act.Item2));
                        

                        //ordc.status = Status.series_sent;
                        /*}
                        else
                        {
                            ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Blue, LEDStatus.On);
                            ((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), Convert.ToInt32(act.Item3), dir, 0, 0);
                            ordc.status = Status.series_confirmed;
                        }*/
                    }
                    break;
                }
            }
        }



        public bool Start()
        {
            foreach (List<Tuple<int, int, string>> act_q in action_list)
            {
                if (act_q.Count > 0)
                {
                    Tuple<int, int, string> act = act_q[0];
                    SendToPTL(act);
                }
            }
            OrdersPanel.Visibility = Visibility.Visible;
            return true;

            /*foreach (DataRow row in replenishTable.Rows)
            {
                if (Convert.ToInt32(row[3]) == 0)
                {
                    DataRow[] r = PTLs.Select("Global = " + row[1]);
                    if (r.Length > 0)
                    {
                        Direction dir = Direction.Up;
                        if (Convert.ToBoolean(r[0][4]))
                            dir = Direction.Down;
                        if (!((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Amber, LEDStatus.On))
                        {
                            //return false;
                        }
                        if (!((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), Convert.ToInt32(row[2]), dir, 0, 0))
                        {
                            //return false;
                        }
                    }
                }
            }
            //OrdersPanel.Visibility = Visibility.Visible;
            return true;*/
        }

        private void ConfirmORDC(ReplenishContentItem ordc, DataRow[] r, string message, DataRow[] PTLrow)
        {
            /*DataRow[] r2 = replenishTable.Select("PTL = " + r[0][1]);
            if (r2.Length > 0)
            {*/
            
            DataRow[] rw = dt.Select("Gateway = " + PTLrow[0][2] + " AND ID = " + PTLrow[0][3]);
            if (Convert.ToInt32(message) == Convert.ToInt32(r[0][2]))
            {
                r[0][3] = 1;
                r[0][4] = message.Trim();
                (this.Owner as MainWindow).sqlQueriesToSend.Add("UPDATE ptl_database.newcomingsdescription SET status=1, datetimeProcessed=CURRENT_TIMESTAMP, actQuant=" + message.Trim() //HERE
                        + " WHERE idNewComingsDescription=" + r[0][0] + ";");
                ordc.Confirmed = true;

                if (rw.Length > 0)
                {
                    ((MainWindow)Application.Current.MainWindow).ptlconn.ClearNode(Convert.ToInt32(PTLrow[0][2]), Convert.ToInt32(PTLrow[0][3]));
                    int index = dt.Rows.IndexOf(rw[0]);
                    if (action_list[index].Count > 0)
                        action_list[index].RemoveAt(0);
                    if (action_list[index].Count > 0)
                    {
                        SendToPTL(action_list[index][0]);
                    }
                }
            }
            else if (Convert.ToInt32(message) < Convert.ToInt32(r[0][2]))
            {
                r[0][3] = 2;
                r[0][4] = message.Trim();
                (this.Owner as MainWindow).sqlQueriesToSend.Add("UPDATE ptl_database.newcomingsdescription SET status=2, datetimeProcessed=CURRENT_TIMESTAMP, actQuant=" + message.Trim()
                        + " WHERE idNewComingsDescription=" + r[0][0] + ";");
                ordc.Expanded = true;
                if (rw.Length > 0)
                {
                    Direction dir = Direction.Up;
                    if (Convert.ToBoolean(PTLrow[0][4]))
                        dir = Direction.Down;

                    ((MainWindow)Application.Current.MainWindow).ptlconn.ClearNode(Convert.ToInt32(PTLrow[0][2]), Convert.ToInt32(PTLrow[0][3]));
                    ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(PTLrow[0][2]), Convert.ToInt32(PTLrow[0][3]), LEDColor.Cyan, LEDStatus.Blink_1sec);
                  //  ((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(PTLrow[0][2]), Convert.ToInt32(PTLrow[0][3]), -(Convert.ToInt32(r[0][2]) - Convert.ToInt32(message)), dir, 0, 0);
                }
            }
            //}
            bool all_confirmed = true;
            foreach (ReplenishContentItem o in OrdersPanel.Children.OfType<ReplenishContentItem>())
            {
                if (!o.Confirmed)
                    all_confirmed = false;
            }
            if (all_confirmed)
                this.Confirmed = true;
        }

        private void ConfirmPodp(ReplenishContentItem ordc, DataRow[] r2, string message, DataRow[] PTLrow)
        {
            //DataRow[] r2 = replenishTable.Select("Desc_id = " + ordc.RefID);
            /*if (r2.Length > 0)
            {*/
            r2[0][3] = 1;
            r2[0][4] = message;
            (this.Owner as MainWindow).sqlQueriesToSend.Add("UPDATE ptl_database.newcomingsdescription SET status=1, statusPotpitka=1, datetimeProcessed=CURRENT_TIMESTAMP, quantPotpitka=" + ordc.QuantityPodpitka
                    + ", userId=" + (this.Owner as MainWindow).currentUser + " WHERE idNewComingsDescription=" + r2[0][0] + ";");
            ordc.Confirmed = true;
            //}

            DataRow[] rw = dt.Select("Gateway = " + PTLrow[0][2] + " AND ID = " + PTLrow[0][3]);
            if (rw.Length > 0)
            {
                int index = dt.Rows.IndexOf(rw[0]);
                if (action_list[index].Count > 0)
                    action_list[index].RemoveAt(0);
                if (action_list[index].Count > 0)
                {
                    SendToPTL(action_list[index][0]);
                }
            }

            bool all_confirmed = true;
            foreach (ReplenishContentItem o in OrdersPanel.Children.OfType<ReplenishContentItem>())
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
                    DataRow[] r2 = replenishTable.Select("PTL = " + r[0][1] + " AND " + "(status = 2)");
                    if (r2.Length == 0)
                        r2 = replenishTable.Select("PTL = " + r[0][1] + " AND " + "(status = 0)");
                    if (r2.Length > 0)
                    {
                        foreach (ReplenishContentItem ordc in OrdersPanel.Children.OfType<ReplenishContentItem>())
                        {
                            if (ordc.RefID == Convert.ToInt32(r2[0][0]))
                            {
                                if (!ordc.Confirmed && !ordc.Expanded && ordc.Series_confirmed)
                                {
                                    ordc.QuantityReplenished = Convert.ToInt32(message.Trim());
                                    ConfirmORDC(ordc, r2, message, r);
                                    ordc.Series_confirmed = false;
                                }
                                else if (!ordc.Confirmed && ordc.Expanded)
                                {
                                    ordc.QuantityPodpitka = Convert.ToInt32(new string(message.Where(c => Char.IsDigit(c)).ToArray()));
                                    ConfirmPodp(ordc, r2, message, r);
                                    ordc.Series_confirmed = false;
                                }
                                else
                                {
                                    ordc.Series_confirmed = true;
                                }
                            }
                            
                        }
                    }
                }

            }
        }

        private void pause_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void print_button_Click(object sender, RoutedEventArgs e)
        {
            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);             //путь к рабочему столу
            string file_name = System.IO.Path.Combine(desktopFolder, "для печати.txt");                      //соединяем путь к рабочему столу и наименование файла
            StreamWriter objWriter;
            objWriter = new StreamWriter(file_name);
            objWriter.WriteLine("Наименование                  Адрес Хранения        Серия            Количество");
            string text;                                                                                     //текст который будем записывать построчно в данный файл
            foreach (DataRow row in replenishTable.Rows)
            {
                string naming = row[6].ToString();
                string seria = row[9].ToString();

                if (row[6].ToString().Length > 28)
                {
                    naming = row[6].ToString().Substring(0, 28);
                }
                if (row[9].ToString().Length > 15)
                {
                    seria = row[9].ToString().Substring(Math.Max(0, row[10].ToString().Length - 5));
                }
                text = naming.PadRight(30) + row[5].ToString().PadRight(22) + seria.ToString().PadRight(17) + row[2].ToString();
                objWriter.WriteLine(text);
            }
            
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(file_name);
            psi.Verb = "PRINT";

            Process.Start(psi);
            objWriter.Close();
            MessageBox.Show("Файл принтерге жіберілді");
        }
    }
}
