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
    public partial class InventoryWindow : Window
    {
        //Order myOrder;
        public Inventory orders;
        public int priority { get; set; }
        private DataTable PTLs;
        private string order_name;
        private int order_id;
        private List<string> medical_series;
        private List<string> medical_series_full;
        public event EventHandler HideMe;
        InventoryContentItem expandedItem;
        DataTable med_series;
        MySqlConnection connect;

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
                my_border.orderName = "Заказ " + OrderName;
                my_border.quantity = ItemCount;
            }
        }*/

        List<Tuple<bool, int, string>>[] action_list;
        DataTable dt;

        public InventoryWindow(Inventory orderTable, DataTable PTLS, MySqlConnection conn)
        {
            InitializeComponent();
            connect = conn;
            
            medical_series = new List<string>();
            medical_series_full = new List<string>();
            expandedItem = null;
            orders = orderTable;
            PTLs = PTLS;
            this.order_name = orderTable.TableName;
            //this.order_id = order_id;
            string[] st = { "Gateway", "ID" };
            dt = PTLs.DefaultView.ToTable(true, st);
            /*string disp = "";
            foreach (DataRow row in dt.Rows)
            {
                disp += row[0].ToString() + ", " + row[1].ToString() + "\n";
            }

            MessageBox.Show(disp);*/
            
            //string disp = "";
           

            dt.Columns[0].ColumnName = "Gateway";
            dt.Columns[1].ColumnName = "ID";

            action_list = new List<Tuple<bool, int, string>>[dt.Rows.Count];

            for (int ind = 0; ind < dt.Rows.Count; ind++)
            {
                action_list[ind] = new List<Tuple<bool, int, string>>();
            }

            this.Owner = (MainWindow)Application.Current.MainWindow;
            this.ResizeMode = ResizeMode.NoResize;
            this.Closing += (se, e) =>
            {
                if (!Confirmed && !(Owner as MainWindow).close)
                {
                    MessageBox.Show("Инвентаризация аяқталмады!");
                    e.Cancel = true;
                }
            };

            this.Loaded += (se, ev) =>
            {
                Start();
            };


            foreach (DataRow row in orders.Rows)
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

                            Tuple<bool, int, string> t = new Tuple<bool, int, string>(false, Convert.ToInt32(row[0]), row[2].ToString());
                            action_list[index].Add(t);

                            Tuple<bool, int, string> t2 = new Tuple<bool, int, string>(true, Convert.ToInt32(row[0]), "000000");
                            action_list[index].Add(t2);

                        }
                    }
                }
            }

            int i = 1;
            //OrdersPanel.Visibility = Visibility.Collapsed;
            string[] s = { "" };
            foreach (DataRow row in orders.Rows)
            {
                
                InventoryContentItem ordc = new InventoryContentItem(i, Convert.ToInt32(row[0]), s, row[6].ToString(), row[5].ToString(), row[2].ToString(), Convert.ToInt32(row[1]), medical_series, 1);

                if (Convert.ToInt32(row[3]) == 0)
                {
                    ordc.QuantityCollected = 0;
                }
                else
                {
                    ordc.status = Status.confirmed;
                    ordc.QuantityCollected = Convert.ToInt32(row[4]);
                }
                ordc.ShowButtonPressed1 += (se, ev) =>              // if plus (new series) button pressed
                {
                    medical_series.Clear();
                    medical_series_full.Clear();
                 
                    {
                        med_series = new DataTable();
                        string stu = "SELECT seriesNumber, seriesFull FROM medicamentsseries WHERE mdCode='" + row[7].ToString() + "';";                                // I am working here
                        MySqlDataAdapter da1 = new MySqlDataAdapter(stu, connect);
                        da1.Fill(med_series);
                        //MessageBox.Show(row[7].ToString());
                    }
                    if (med_series.Rows.Count > 0)
                    {
                        med_series.Columns[0].ColumnName = "medical_series";                                                 //MEDICAL SERIES
                        med_series.Columns[1].ColumnName = "medical_series_full";
                    }
                    
                    foreach (DataRow row2 in med_series.Rows)
                    {
                        medical_series.Add(row2[0].ToString());
                        medical_series_full.Add(row2[1].ToString());
                    }
                    //MessageBox.Show(medical_series[0]);
                    InventoryContentItem ordc1 = new InventoryContentItem(i, Convert.ToInt32(row[0]), s, row[6].ToString(), row[5].ToString(), row[2].ToString(), Convert.ToInt32(row[1]), medical_series, 0); // Last parameter is mode (for series). 0 means series will be chosen by combobox
      
                    if (Convert.ToInt32(row[3]) == 0)
                    {
                        ordc1.QuantityCollected = 0;
                    }
                    else
                    {
                        ordc1.status = Status.confirmed;
                        ordc1.QuantityCollected = Convert.ToInt32(row[4]);
                    }
                    OrdersPanel.Children.Add(ordc1);                      // adding new element to the panel, but this time there is a combobox for series
                    i++;
                    ordc1.ShowButtonPressed += (se1, ev1) =>
                    {
                        DataRow[] r = PTLs.Select("Global = " + (se1 as InventoryContentItem).PTL_global);
                        if (r.Length > 0)
                        {
                            string ser_full = "";
                            foreach (DataRow row2 in med_series.Rows)
                            {
                                if (string.Equals((se1 as InventoryContentItem).Additional_series, row2[0].ToString())) {
                                    ser_full = row2[1].ToString();
                                    break;
                                }
                            }
                            ConfirmORDC(se1 as InventoryContentItem, r, ((se1 as InventoryContentItem).QuantityCollected.ToString() + ";" + (se1 as InventoryContentItem).Additional_series) + ";" + row[5].ToString() + ";" + row[8].ToString() + ";" + row[7].ToString() + ";" + ser_full);   // add new parameters here for splitting
                            ((MainWindow)Application.Current.MainWindow).ptlconn.ClearNode(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]));

                            DataRow[] rw = dt.Select("Gateway = " + r[0][2] + " AND ID = " + r[0][3]);
                            if (rw.Length > 0)
                            {
                                int index = dt.Rows.IndexOf(rw[0]);
                                int c = action_list[index].Count;

                                for (int ind = c - 1; ind >= 0; ind--)
                                {
                                    if (action_list[index][ind].Item2 == (se1 as InventoryContentItem).RefID)
                                    {
                                        action_list[index].RemoveAt(ind);
                                    }
                                }
                                if (action_list[index].Count > 0)
                                    SendToPTL(action_list[index][0]);
                            }
                        }

                    };
                };


                ordc.ShowButtonPressed += (se, ev) =>
                {
                    DataRow[] r = PTLs.Select("Global = " + (se as InventoryContentItem).PTL_global);
                    if (r.Length > 0)
                    {
                        ConfirmORDC(se as InventoryContentItem, r, ((se as InventoryContentItem).QuantityCollected.ToString()));     
                        //MessageBox.Show((se as InventoryContentItem).Additional_series);     
                        ((MainWindow)Application.Current.MainWindow).ptlconn.ClearNode(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]));

                        DataRow[] rw = dt.Select("Gateway = " + r[0][2] + " AND ID = " + r[0][3]);
                        if (rw.Length > 0)
                        {
                            int index = dt.Rows.IndexOf(rw[0]);
                            int c = action_list[index].Count;

                            for (int ind = c - 1; ind >= 0; ind--)
                            {
                                if (action_list[index][ind].Item2 == (se as InventoryContentItem).RefID)
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
                    }
                };
                /*ordc.ExpandMe += (se, ev) =>
                {
                    if (expandedItem != null)
                    {
                        expandedItem.Expanded = false;
                    }
                    expandedItem = (se as InventoryContentItem);
                    expandedItem.Expanded = true;
                };*/
                OrdersPanel.Children.Add(ordc);
                i++;
            }
        }



        private void finish_button_Click(object sender, RoutedEventArgs e)
        {
            (this.Owner as MainWindow).sqlQueriesToSend.Add("UPDATE inventoriestable" +
                    " SET status=1 WHERE inventoryTableName=" + "'" + orders.TableName + "';");
            this.Close();
        }

        void SendToPTL(Tuple<bool, int, string> act)
        {
            foreach (InventoryContentItem ordc in OrdersPanel.Children.OfType<InventoryContentItem>())
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
                            ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Red, LEDStatus.On);
                            ((MainWindow)Application.Current.MainWindow).ptlconn.DisplaySeries(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), Convert.ToChar(act.Item3));
                            ordc.status = Status.series_sent;
                        }
                        else
                        {
                            ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Red, LEDStatus.Blink_1sec);
                            ((MainWindow)Application.Current.MainWindow).ptlconn.DisplayStockQuantity(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), "000000", dir, 0, 0, 5);
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
            return true;
        }

        private void ConfirmORDC(InventoryContentItem ordc, DataRow[] r, string message)
        {
            DataRow[] r2 = orders.Select("PTL = " + r[0][1]);
            if (r2.Length > 0)
            {
                r2[0][3] = 1;
                string[] values = message.Split(';');
                r2[0][4] = Convert.ToInt32(values[0]);


                if (values.Length >= 2 && values[1] != "")
                {
                    String new_ser = values[1];
                    var command = "INSERT INTO ptl_database." + orders.TableName + "(mdCode, address, zone, series, seriesFull, status, actQuantity, userId) VALUES('" + values[4].ToString() + "', '" + values[2].ToString() + "', " + values[3].ToString() + ", '" + new_ser + "', '" + (values[5].ToString()) + "', 1, " + values[0].ToString() + ", " + (this.Owner as MainWindow).currentUser + ")";
                    (this.Owner as MainWindow).sqlQueriesToSend.Add(command); // This is mine

                }
                else if (values.Length < 2)
                {
                    (this.Owner as MainWindow).sqlQueriesToSend.Add("UPDATE ptl_database." + orders.TableName +
                    " SET status=1, userId=" + (this.Owner as MainWindow).currentUser + ", actQuantity=" + Convert.ToInt32(values[0]) + ", datetimeChecked=CURRENT_TIMESTAMP WHERE invId=" + ordc.RefID + ";"); // HERE SENDING TO DATABASE
                }
            }


            ordc.status = Status.confirmed;
            bool all_confirmed = true;
            foreach (InventoryContentItem o in OrdersPanel.Children.OfType<InventoryContentItem>())
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
                    foreach (InventoryContentItem ordc in OrdersPanel.Children.OfType<InventoryContentItem>())
                    {
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
                                ConfirmORDC(ordc, r, message);
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
                    //}

                }
            }
        }

        private void print_button_Click(object sender, RoutedEventArgs e)
        {
            string desktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);             //путь к рабочему столу
            string file_name = System.IO.Path.Combine(desktopFolder, "для печати.txt");                      //соединяем путь к рабочему столу и наименование файла
            StreamWriter objWriter;
            objWriter = new StreamWriter(file_name);
            objWriter.WriteLine("Наименование                            Адрес Хранения           Серия");
            string text;                                                                                     //текст который будем записывать построчно в данный файл
            foreach (DataRow row in orders.Rows)
            {
                string naming = row[6].ToString();
                string seria = row[2].ToString();
                if (row[6].ToString().Length > 38)
                {
                    naming = row[6].ToString().Substring(0, 38);
                }
                if (row[2].ToString().Length > 10)
                {
                    seria = row[2].ToString().Substring(Math.Max(0, row[10].ToString().Length - 5));
                }
                text = naming.PadRight(40) + row[5].ToString().PadRight(25) + seria.ToString();
                objWriter.WriteLine(text);
                //MessageBox.Show(row[6].ToString());
            }
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(file_name);
            psi.Verb = "PRINT";

            Process.Start(psi);
            objWriter.Close();
            MessageBox.Show("Файл принтерге жіберілді");
        }
    }
}
