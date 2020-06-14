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
    public partial class PodpitkaWindow : Window
    {
        //Order myOrder;
        //public DataTable podpitka_t;
        public int priority { get; set; }
        private DataTable PTLs;
        private string order_name;
        private int order_id;
        private NewComing podpitkaTable;

        public NewComing TablePodpitka
        {
            get
            {
                return podpitkaTable;
            }
        }

        public int ItemCount
        {
            get
            {
                return podpitkaTable.Rows.Count;
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

        List<Tuple<int, int>>[] action_list;
        DataTable dt;
        public PodpitkaWindow(NewComing podpitka_table, DataTable PTLS)
        {
            InitializeComponent();
            this.podpitkaTable = podpitka_table;
            //podpitka_t = podpitka_table;
            PTLs = PTLS;
            //this.order_id = order_id;
            string[] st = { "Gateway", "ID" };
            dt = PTLs.DefaultView.ToTable(true, st);

            dt.Columns[0].ColumnName = "Gateway";
            dt.Columns[1].ColumnName = "ID";

            action_list = new List<Tuple<int, int>>[dt.Rows.Count];

            for (int ind = 0; ind < dt.Rows.Count; ind++)
            {
                action_list[ind] = new List<Tuple<int, int>>();
            }

            this.Owner = (MainWindow)Application.Current.MainWindow;
            this.ResizeMode = ResizeMode.NoResize;

            this.Loaded += (se, ev) =>
            {
                Start();
            };

            foreach (DataRow row in podpitkaTable.Rows)
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
                            Tuple<int, int> t2 = new Tuple<int, int>(Convert.ToInt32(row[0]), Convert.ToInt32(row[2]));
                            action_list[index].Add(t2);

                        }
                    }
                }
            }

            int i = 1;
            string[] s = { "" };
            foreach (DataRow row in podpitkaTable.Rows)
            {
                PodpitkaContentItem ordc = new PodpitkaContentItem(i, Convert.ToInt32(row[0]), s, row[6].ToString(), row[5].ToString(), Convert.ToInt32(row[2]), Convert.ToInt32(row[1]));

                if (Convert.ToInt32(row[3]) == 0)
                {
                    ordc.QuantityCollected = Convert.ToInt32(row[2]);
                }
                else
                {
                    ordc.Confirmed = true;
                    ordc.QuantityCollected = Convert.ToInt32(row[4]);
                }
                ordc.ShowButtonPressed += (se, ev) =>
                {
                    DataRow[] r = PTLs.Select("Global = " + (se as PodpitkaContentItem).PTL_global);
                    if (r.Length > 0)
                    {
                        ConfirmORDC(se as PodpitkaContentItem, r, (se as PodpitkaContentItem).QuantityCollected.ToString());
                        ((MainWindow)Application.Current.MainWindow).ptlconn.ClearNode(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]));

                        DataRow[] newrows = PTLs.Select("Gateway = " + r[0][2] + "AND ID = " + r[0][3]);

                        /*foreach (DataRow newrow in newrows)
                        {
                            foreach (PodpitkaContentItem pp in OrdersPanel.Children.OfType<PodpitkaContentItem>())
                            {
                                if (!pp.Confirmed && pp.PTL_global == Convert.ToInt32(newrow[1]))
                                {
                                    Direction dir = Direction.Up;
                                    if (Convert.ToBoolean(newrow[4]))
                                        dir = Direction.Down;
                                    ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(newrow[2]), Convert.ToInt32(newrow[3]), LEDColor.Cyan, LEDStatus.On);
                                    ((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(newrow[2]), Convert.ToInt32(newrow[3]), pp.Quantity, dir, 0, 0);

                                }
                            }
                        }*/

                        DataRow[] rw = dt.Select("Gateway = " + r[0][2] + " AND ID = " + r[0][3]);
                        if (rw.Length > 0)
                        {
                            int index = dt.Rows.IndexOf(rw[0]);
                            int c = action_list[index].Count;

                            for (int ind = c - 1; ind >= 0; ind--)
                            {
                                if (action_list[index][ind].Item1 == (se as PodpitkaContentItem).RefID)
                                {
                                    action_list[index].RemoveAt(ind);
                                }
                            }
                            if (action_list[index].Count > 0)
                                SendToPTL(action_list[index][0]);
                        }
                    }
                };
                OrdersPanel.Children.Add(ordc);
                i++;
            }
        }

        private void finish_button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void SendToPTL(Tuple<int, int> act)
        {
            foreach (PodpitkaContentItem ordc in OrdersPanel.Children.OfType<PodpitkaContentItem>())
            {
                if (ordc.RefID == act.Item1)
                {
                    DataRow[] r = PTLs.Select("Global = " + ordc.PTL_global);
                    if (r.Length > 0)
                    {
                        Direction dir = Direction.Up;
                        if (Convert.ToBoolean(r[0][4]))
                            dir = Direction.Down;

                        ((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Pink, LEDStatus.On);
                        ((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), Convert.ToChar(act.Item2));
                        ordc.SentToPTL = true;
                    }
                    break;
                }
            }
        }

        public bool Start()
        {
            foreach (List<Tuple<int, int>> act_q in action_list)
            {
                if (act_q.Count > 0)
                {
                    Tuple<int, int> act = act_q[0];
                    SendToPTL(act);
                }
            }
            /*foreach (PodpitkaContentItem pp in OrdersPanel.Children.OfType<PodpitkaContentItem>())
            {
                if (!pp.Confirmed)
                {
                    DataRow[] r = PTLs.Select("Global = " + pp.PTL_global);
                    if (r.Length > 0)
                    {
                        Direction dir = Direction.Up;
                        if (Convert.ToBoolean(r[0][4]))
                            dir = Direction.Down;
                        if (!((MainWindow)Application.Current.MainWindow).ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Cyan, LEDStatus.On))
                        {
                            //return false;
                        }
                        if (!((MainWindow)Application.Current.MainWindow).ptlconn.DisplayQuantity(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), pp.Quantity, dir, 0, 0))
                        {
                            //return false;
                        }
                    }
                }
            }*/
            return true;
        }

        private void ConfirmORDC(PodpitkaContentItem ordc, DataRow[] r, string message)
        {
            DataRow[] r2 = podpitkaTable.Select("PTL = " + r[0][1]);
            if (r2.Length > 0)
            {
                //r2[0][3] = 1;
                //r2[0][4] = Convert.ToInt32(message.Trim());
                (this.Owner as MainWindow).sqlQueriesToSend.Add("UPDATE ptl_database.podpitkatable" +
                    " SET status=1, userId=" + (this.Owner as MainWindow).currentUser + ", actQuant=" + Convert.ToInt32(message.Trim()) + ", podpitkaDate=CURRENT_TIMESTAMP WHERE idPod=" + ordc.RefID + ";"); 

                foreach (OrderBorder ordbrd in (this.Owner as MainWindow).podpitka_sp.Children.OfType<OrderBorder>())
                {
                    if (ordbrd.ID == Convert.ToInt32(r2[0][0]))
                    {
                        (this.Owner as MainWindow).podpitka_sp.Children.Remove(ordbrd);
                        break;
                    }
                }
                podpitkaTable.Rows.Remove(r2[0]);

                if (podpitkaTable.Rows.Count > 0)
                    (this.Owner as MainWindow).podpitka_tab.Header = "Подпитка (" + podpitkaTable.Rows.Count + ")";
                else
                    (this.Owner as MainWindow).podpitka_tab.Header = "Подпитка";
            }
            ordc.Confirmed = true;
            bool all_confirmed = true;
            foreach (PodpitkaContentItem o in OrdersPanel.Children.OfType<PodpitkaContentItem>())
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
                    foreach (PodpitkaContentItem ordc in OrdersPanel.Children.OfType<PodpitkaContentItem>())
                    {
                        if (!ordc.Confirmed && ordc.SentToPTL && ordc.PTL_global == Convert.ToInt32(r[0][1]))
                        {
                            ordc.QuantityCollected = Convert.ToInt32(message.Trim());
                            ConfirmORDC(ordc, r, message);
                            DataRow[] rw = dt.Select("Gateway = " + gateway + " AND ID = " + node);
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
                            /*DataRow[] rw = dt.Select("Gateway = " + gateway + " AND ID = " + node);
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
                            }*/
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
            objWriter.WriteLine("Наименование                            Адрес Хранения           Количество");
            string text;                                                                                     //текст который будем записывать построчно в данный файл
            foreach (DataRow row in podpitkaTable.Rows)
            {
                string naming = row[6].ToString();
                if (row[6].ToString().Length > 38)
                {
                    naming = row[6].ToString().Substring(0, 38);
                }
                text = naming.PadRight(40) + row[5].ToString().PadRight(25) + row[2].ToString();
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
