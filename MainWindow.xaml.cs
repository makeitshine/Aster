using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PTLStation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public PTLConnection ptlconn;
        private SynchronizationContext _uiContext;
        public MySqlConnection conn;
        private OrderWindow cur_ord;

        public List<string> sqlQueriesToSend;
        /// <summary>
        /// 2 sec timer to refresh orders from DB
        /// </summary>
        private System.Timers.Timer timer_db;

        public int currentUser;

        /// <summary>
        /// this computer's ID
        /// </summary>
        public int myID;
        /// <summary>
        /// storage of this computer's zones and PTLs
        /// </summary>
        DataTable myZones, myPTLs;
        /// <summary>
        /// this computer's zones represented as a string for DB query
        /// </summary>
        string myzones_str;
        /// <summary>
        /// this computer's gateway numbers stored in array, used to connect to PTLs with PTLConnection class
        /// </summary>
        int[] gateway_nums;
        /// <summary>
        /// Stores the id of last loaded order. Used to check for new orders in database
        /// </summary>
        int last_order_id;
        int last_order_id1;
        /// <summary>
        /// Stores the id of last newcoming. Used to check for new comings in database
        /// </summary>
        int last_newcoming_id;
        int last_podpitka_id;
        int last_inventory_id;
        /// <summary>
        /// A list of all orders
        /// </summary>
        List<Order> order_list;
        /// <summary>
        /// A list of all new comings
        /// </summary>
        List<NewComing> newcomings_list;
        List<Inventory> inventories_list;
        List<NewComing> podpitka_list;
        DataTable all_baskets;

        ReplenishmentWindow cur_newc;
        ReplenishmentWindow current_newcoming
        {
            get
            {
                return cur_newc;
            }
            set
            {
                if (value != null)
                {
                    value.TableReplenishment.InProcess = true;
                }
                cur_newc = value;
            }
        }
        InventoryWindow inv_wind;
        InventoryWindow current_inventory
        {
            get
            {
                return inv_wind;
            }
            set
            {
                if (value != null)
                {
                    value.orders.InProcess = true;
                }
                inv_wind = value;
            }
        }

        PodpitkaWindow current_pod_window;

        /// <summary>
        /// the current order, which is being collected by a person. Null if a person is not collecting.
        /// </summary>
        OrderWindow current_Order
        {
            get
            {
                return cur_ord;
            }
            set
            {
                if (value != null)
                {
                    value.InProcess = true;
                }
                cur_ord = value;
            }
        }

        public bool close;

        public MainWindow()
        {
            close = false;
            _uiContext = SynchronizationContext.Current;
            myID = -1;
            myZones = myPTLs = null;
            myzones_str = "-1";
            currentUser = -1;
            gateway_nums = null;
            last_order_id = last_newcoming_id = last_inventory_id = last_podpitka_id = last_order_id1 = 0;
            order_list = new List<Order>();
            newcomings_list = new List<NewComing>();
            inventories_list = new List<Inventory>();
            podpitka_list = new List<NewComing>();
            current_pod_window = null;
            cur_ord = null;
            sqlQueriesToSend = new List<string>();
            ptlconn = null;
            all_baskets = new DataTable();



            InitializeComponent();
            this.Closing += (se, ev) =>
            {
                if (currentUser != -1)
                {
                    if (MessageBox.Show("Программадан шығасыз ба?", "Программадан шығасыз ба?", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        close = true;
                        if (ptlconn != null)
                            ptlconn.Disconnect();

                        Application.Current.Shutdown();
                    }
                    else
                    {
                        ev.Cancel = true;
                    }
                }
            };
            this.Loaded += MainWindow_Loaded;
            timer_db = new System.Timers.Timer(2000);
            timer_db.AutoReset = false;
            timer_db.Elapsed += (se, ev) =>
            {
                _uiContext.Post(new SendOrPostCallback(new Action<object>(o =>
                {
                    timer_db.Stop();
                    CheckOrders();
                })), null);
            };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            WaitingWindow waitwind = new WaitingWindow();
            waitwind.Show();

            BackgroundWorker bgwr = new BackgroundWorker();
            bgwr.WorkerReportsProgress = true;

            // Background Worker to check for connection every 5 sec.
            bgwr.DoWork += (se, ev) =>
            {
                while (true)
                {
                    try
                    {
                        establishConn();
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (ex is NotFoundException)
                        {
                            ev.Result = ex;
                            break;
                        }
                        else if (ex is DBNotConnectedException)
                        {
                            Thread.Sleep(5000);
                        }
                    }
                }
            };
            // After the connection is established loading parameters from database or show error in case of exception
            bgwr.RunWorkerCompleted += (se, ev) =>
            {
                if (ev.Result is Exception)
                {
                    MessageBoxResult result = MessageBox.Show((ev.Result as Exception).Message);
                    if (result == MessageBoxResult.OK)
                    {
                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    /*try
                    {*/
                    {
                        List<string> ips = GetLocalIPAddress();
                        DataTable ipd = new DataTable();
                        foreach (string ip in ips)
                        {
                            string stu = "SELECT CompId FROM computers WHERE CompIp='" + ip + "'";
                            MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                            ipd = new DataTable();
                            da1.Fill(ipd);
                            if (ipd.Rows.Count > 0)
                                break;
                        }
                        if (ipd.Rows.Count < 1)
                        {
                            throw new UnassignedCategoryException("IP не найден");
                        }
                        myID = Convert.ToInt32(ipd.Rows[0][0]);
                    }
                    //myID = 1;
                    {
                        string stu = "SELECT zoneId, zoneName FROM zones WHERE compId=" + myID;
                        MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                        myZones = new DataTable();
                        da1.Fill(myZones);

                        if (myZones.Rows.Count < 1)
                        {
                            throw new UnassignedCategoryException("Зоны не назначены");
                        }
                        myzones_str = myZones.Rows[0][0].ToString();
                        for (int i = 1; i < myZones.Rows.Count; i++)
                        {
                            myzones_str += ", " + myZones.Rows[i][0].ToString();
                        }
                    }

                    {
                        string stu = "SELECT id, username, password, barcode FROM users";
                        MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                        DataTable users = new DataTable();
                        da1.Fill(users);

                        if (users.Rows.Count < 1)
                        {
                            throw new UnassignedCategoryException("Нет зарегистрированных пользователей");
                        }

                        users.Columns[0].ColumnName = "user_id";
                        users.Columns[1].ColumnName = "username";
                        users.Columns[2].ColumnName = "password";
                        users.Columns[3].ColumnName = "barcode";

                        LogInWindow login = new LogInWindow(this, users);
                        login.Closed += (sen, eve) =>
                        {
                            this.currentUser = (sen as LogInWindow).userID;
                            DataRow[] r = users.Select("user_id = " + this.currentUser);
                            if (r.Length > 0)
                            {
                                this.Title += ". Пользователь: " + r[0][1].ToString();
                            }
                        };
                        login.ShowDialog();
                    }

                    {
                        string stu = "SELECT refId, PTLGlobal, GatewayId, PTLId, PTLUp0Down1 FROM ptlref WHERE CompId=" + myID;
                        MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);      // Тут он получает дату   
                        myPTLs = new DataTable();
                        da1.Fill(myPTLs);

                        myPTLs.Columns[0].ColumnName = "Ref";
                        myPTLs.Columns[1].ColumnName = "Global";
                        myPTLs.Columns[2].ColumnName = "Gateway";
                        myPTLs.Columns[3].ColumnName = "ID";
                        myPTLs.Columns[4].ColumnName = "up_down";

                        string[] st = { "Gateway" };
                        DataTable dtable = myPTLs.DefaultView.ToTable(true, st);
                        gateway_nums = new int[dtable.Rows.Count];
                        for (int i = 0; i < dtable.Rows.Count; i++)
                        {
                            gateway_nums[i] = Convert.ToInt32(dtable.Rows[i][0]);
                            StatusGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                            GatewayStatus g = new GatewayStatus(gateway_nums[i])
                            {
                                HorizontalAlignment = HorizontalAlignment.Center
                            };
                            Grid.SetColumn(g, i);
                            StatusGrid.Children.Add(g);
                        }
                    }
                    closeConn();
                    CheckOrders();

                    ptlconn = new PTLConnection(gateway_nums);
                    ptlconn.connectionStatesChanged += Ptlconn_connectionStatesChanged;
                    ptlconn.ConfirmOccured += Ptlconn_ConfirmOccured;
                    ptlconn.ShortageOccured += Ptlconn_ConfirmOccured;
                    ptlconn.StockConfirmationOccured += Ptlconn_ConfirmOccured;

                    int x = ptlconn.Connect();
                    if (x < 0)
                    {
                        throw new Exception("Ошибка");
                    }

                    //CheckOrders();
                    /*}
                    catch (Exception ex)
                    {
                        MessageBoxResult result = MessageBox.Show(ex.Message);
                        if (result == MessageBoxResult.OK)
                        {
                            Application.Current.Shutdown();
                        }
                    }*/
                }
                waitwind.isReady = true;
                waitwind.Close();
                this.IsEnabled = true;
            };
            bgwr.RunWorkerAsync();
        }

        /*private void Ptlconn_ShortageOccured(object sender, ConfirmButtonArgs e)
        {
            if (current_Order != null)
            {
                current_Order.ReceiveMess(e.Gateway, e.Node, e.direction, e.Message);
            }
            else if (current_newcoming != null)
            {
                current_newcoming.ReceiveMess(e.Gateway, e.Node, e.direction, e.Message);
            }
        }*/

        private void Ptlconn_ConfirmOccured(object sender, ConfirmButtonArgs e)
        {
            if (current_Order != null)
            {
                current_Order.ReceiveMess(e.Gateway, e.Node, e.direction, e.Message);
            }
            else if (current_newcoming != null)
            {
                current_newcoming.ReceiveMess(e.Gateway, e.Node, e.direction, e.Message);
            }
            else if (current_inventory != null)
            {
                current_inventory.ReceiveMess(e.Gateway, e.Node, e.direction, e.Message);
            }
            else if (current_pod_window != null)
            {
                current_pod_window.ReceiveMess(e.Gateway, e.Node, e.direction, e.Message);
            }
        }

        private void Ptlconn_connectionStatesChanged(object sender, ConnStateArgs e)
        {
            for (int i = 0; i < e.status.Length; i++)
            {
                (StatusGrid.Children[i] as GatewayStatus).On = e.status[i]; // Gateway status indicators, show red or green when gateway is diconnected or connected.
            }
        }

        /// <summary>
        /// Checks the database for new orders and executes new sql queries if they exist.
        /// </summary>
        private void CheckOrders()
        {
            try
            {
                establishConn();
                // execute all new sql queries and delete from waiting list
                while (sqlQueriesToSend.Count > 0)
                {
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = sqlQueriesToSend[0];
                    cmd.Prepare();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (MySqlException ex)
                    {

                    }
                    sqlQueriesToSend.RemoveAt(0);
                }

                DataTable inventories = new DataTable();
                {
                    string stu = "SELECT inventoryId, inventoryTableName, inventoryDate FROM inventoriestable WHERE inventoryId >" + last_inventory_id + " ORDER BY inventoryId ASC";
                    MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                    da1.Fill(inventories);
                }

                if (inventories.Rows.Count > 0)
                {
                    last_inventory_id = Convert.ToInt32(inventories.Rows[inventories.Rows.Count - 1][0]);
                    foreach (DataRow row in inventories.Rows)
                    {
                        Inventory desc = new Inventory();
                        {
                            string t = row[1].ToString(); // table name
                            string stu = "SELECT " + t + ".invId, locations.PTL, " + t + ".series, " + t + ".status, "
                                + t + ".actQuantity, locations.address, medicaments.mdName, " + t + ".mdCode, " + t + ".zone, " + t + ".userId FROM " + t +
                                " INNER JOIN medicaments ON " + t + ".mdCode=medicaments.code INNER JOIN locations ON "      // HERE !!! (!!!!)
                                + "medicaments.id=locations.mdId AND " + t + ".address = locations.address WHERE "
                                + t + ".zone IN(" + myzones_str + ");";
                            MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                            da1.Fill(desc);
                        }
                        if (desc.Rows.Count > 0)
                        {
                            desc.Columns[0].ColumnName = "inv_id";
                            desc.Columns[1].ColumnName = "PTL";
                            desc.Columns[2].ColumnName = "series";
                            desc.Columns[3].ColumnName = "status";
                            desc.Columns[4].ColumnName = "act_quant";
                            desc.Columns[5].ColumnName = "address";
                            desc.Columns[6].ColumnName = "md_name";
                            desc.Columns[7].ColumnName = "md_code";
                            desc.Columns[8].ColumnName = "zone";
                            desc.Columns[9].ColumnName = "user_id";


                            DataRow[] r = desc.Select("status > 0");

                            if (r.Length >= desc.Rows.Count)
                            {
                                continue;
                            }

                            desc.TableDate = row[2].ToString();
                            desc.TableName = row[1].ToString();

                            OrderBorder ordbrd = new OrderBorder();
                            Inventory_sp.Children.Add(ordbrd);
                            desc.MyBorder = ordbrd;

                            inventories_list.Add(desc);                                                              // INVENTORY LIST
                            inventory.Header = "Инвентаризация (" + inventories_list.Count + ")";
                            MediaPlayer mplayer = new MediaPlayer();
                            mplayer.Open(new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "1.wav")));
                            mplayer.Volume = 100;
                            mplayer.Play();
                        }
                    }
                }

                DataTable new_comings = new DataTable();
                {
                    string stu = "SELECT newcId, cartBarcode, docId FROM newcomings WHERE zoneId IN(" + myzones_str
                        + ") AND newcId >" + last_newcoming_id + " ORDER BY newcId ASC";
                    MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                    da1.Fill(new_comings);
                }

                if (new_comings.Rows.Count > 0)
                {
                    last_newcoming_id = Convert.ToInt32(new_comings.Rows[new_comings.Rows.Count - 1][0]);
                    foreach (DataRow row in new_comings.Rows)
                    {
                        NewComing desc = new NewComing();
                        {
                            string stu = "SELECT newcomingsdescription.idNewComingsDescription, locations.PTL, "
                              + "newcomingsdescription.quantity, newcomingsdescription.status, "
                              + "newcomingsdescription.actQuant, locations.address, medicaments.mdName, "
                              + "newcomingsdescription.statusPotpitka, newcomingsdescription.quantPotpitka, newcomingsdescription.series, newcomingsdescription.docId "
                              + "FROM newcomingsdescription INNER JOIN medicaments ON newcomingsdescription.mdCode="
                              + "medicaments.code INNER JOIN locations ON medicaments.id=locations.mdId AND "
                              + "newcomingsdescription.address=locations.address WHERE newcomingsdescription.cartBarcode='"
                              + row[1] + "' AND newcomingsdescription.status='0' AND newcomingsdescription.docId = '" + row[2] + "';";
                            MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                            da1.Fill(desc);
                        }

                        /*{
                            DataTable dttable = new DataTable();
                            string stu = "SELECT new_comings_description.id_new_comings_description, "
                              + "new_comings_description.quantity, new_comings_description.status, "
                              + "new_comings_description.act_quant, medicaments.md_name FROM new_comings_description "
                              + "INNER JOIN medicaments ON new_comings_description.md_id=medicaments.id WHERE new_comings_description.cart_barcode='" + row[1] + "';";
                            MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                            da1.Fill(dttable);
                        }*/

                        if (desc.Rows.Count > 0)
                        {
                            desc.Columns[0].ColumnName = "Desc_id";
                            desc.Columns[1].ColumnName = "PTL";
                            desc.Columns[2].ColumnName = "Quantity";
                            desc.Columns[3].ColumnName = "status";
                            desc.Columns[4].ColumnName = "act_quant";
                            desc.Columns[5].ColumnName = "address";
                            desc.Columns[6].ColumnName = "md_name";
                            desc.Columns[7].ColumnName = "status_potpitka";
                            desc.Columns[8].ColumnName = "quant_potpitka";
                            desc.Columns[9].ColumnName = "series";
                            desc.Columns[10].ColumnName = "doc_id";

                            DataRow[] r = desc.Select("status = 1");

                            if (r.Length >= desc.Rows.Count)
                            {
                                continue;
                            }

                            desc.TableName = row[1].ToString();
                            OrderBorder ordbrd = new OrderBorder();
                            Replenish_sp.Children.Add(ordbrd);
                            desc.MyBorder = ordbrd;

                            newcomings_list.Add(desc);
                            zagruzka.Header = "Загрузка (" + newcomings_list.Count + ")";
                            MediaPlayer mplayer = new MediaPlayer();
                            mplayer.Open(new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "1.wav")));
                            mplayer.Volume = 100;
                            mplayer.Play();

                        }
                    }
                }



                {
                    string stu = "SELECT basketBarcode FROM ordersbasketsall";
                    MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                    da1.Fill(all_baskets);
                }


                DataTable orders = new DataTable();
                int checking = 0;
                {
                    string stu = "SELECT orderId, orderName, priority, zoneDest FROM orders WHERE AND stat=0 AND orderId > " + last_order_id + " ORDER BY orderId ASC LIMIT 40;";
                    MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                    da1.Fill(orders);
                   /* if (orders.Rows.Count == 0)
                    {
                        string stu1 = "SELECT orderId, orderName, priority, zoneDest FROM orders WHERE needSmartShelfChannel=0 AND stat=0 AND orderId > " + last_order_id1 + " ORDER BY orderId ASC LIMIT 40;";
                        MySqlDataAdapter da2 = new MySqlDataAdapter(stu1, conn);
                        da2.Fill(orders);
                        checking = 1;
                    }

                    /*order_list.Clear();
                    last_order_id = 0;
                    last_order_id1 = 0;
                    Collect_sp.Children.Clear();*/

                }
                if (orders.Rows.Count > 0)
                {
                    if (checking == 0)
                    {
                        last_order_id = Convert.ToInt32(orders.Rows[orders.Rows.Count - 1][0]);
                    }
                    else
                    {
                        last_order_id1 = Convert.ToInt32(orders.Rows[orders.Rows.Count - 1][0]);
                    }
                    foreach (DataRow row in orders.Rows)
                    {
                        if (order_list.Count >= 40 || Collect_sp.Children.Count >= 40)
                        {
                            break;
                        }
                        else
                        {
                            string dest_zone = null;
                            if (Convert.ToInt32(row[3]) != -1)
                            {
                                string stu = "SELECT zoneName FROM zones WHERE zoneId=" + row[3];
                                MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                                DataTable zt = new DataTable();
                                da1.Fill(zt);
                                dest_zone = zt.Rows[0][0].ToString();
                            }

                            Order desc = new Order();
                            {
                                string stu = "SELECT ordersdescription.descriptionId, locations.PTL, ordersdescription.quantity, "
                                    + "ordersdescription.expirationMonth, ordersdescription.status, ordersdescription.actQuant, "
                                    + "locations.address, medicaments.mdName, ordersdescription.expirationYear, ordersdescription.palletized, ordersdescription.seriesNumber, ordersdescription.zoneDest FROM ordersdescription "
                                    + "INNER JOIN medicaments ON ordersdescription.mdCode=medicaments.code INNER JOIN locations ON "
                                    + "medicaments.id=locations.mdId AND ordersdescription.address=locations.address WHERE "
                                    + "ordersdescription.orderName='" + row[1] + "' AND ordersdescription.zoneId IN(" + myzones_str + ");";                               //ORDER
                                MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                                da1.Fill(desc);
                            }
                            if (desc.Rows.Count > 0)
                            {
                                desc.Columns[0].ColumnName = "Desc_id";
                                desc.Columns[1].ColumnName = "PTL";
                                desc.Columns[2].ColumnName = "Quantity";
                                desc.Columns[3].ColumnName = "Expiration_month";
                                desc.Columns[4].ColumnName = "status";
                                desc.Columns[5].ColumnName = "act_quant";
                                desc.Columns[6].ColumnName = "address";
                                desc.Columns[7].ColumnName = "md_name";
                                desc.Columns[8].ColumnName = "Expiration_year";
                                desc.Columns[9].ColumnName = "Palletized";
                                desc.Columns[10].ColumnName = "series_number";
                                desc.Columns[11].ColumnName = "zone_dest";

                                int tot_q = 0;

                                foreach (DataRow row1 in desc.Rows)
                                {
                                    tot_q = tot_q + Convert.ToInt32(row1[2]);
                                }


                                DataRow[] r = desc.Select("status > 0");

                                if (r.Length >= desc.Rows.Count)
                                {
                                    continue;
                                }

                                //check all zones with this orderId.
                                string checkZonesString = "SELECT zoneId, status FROM ordersdescription WHERE orderName='" + row[1] + "';";
                                MySqlDataAdapter checkZonesData = new MySqlDataAdapter(checkZonesString, conn);
                                DataTable checkZonesDataTable = new DataTable();
                                checkZonesData.Fill(checkZonesDataTable);

                                int collectedPartsCount = 0;
                                foreach (DataRow rowtemp in checkZonesDataTable.Rows)
                                {
                                    if ((int)rowtemp[1] > 0)
                                    {
                                        collectedPartsCount++;
                                    }
                                }

                                // check if any cart with this orderId appeared.
                                string checkBasketsString = "SELECT DISTINCT * FROM orderbaskets WHERE orderId=" + row[0] + ";";
                                MySqlDataAdapter checkBasketsData = new MySqlDataAdapter(checkBasketsString, conn);
                                DataTable checkBasketsDataTable = new DataTable();
                                checkBasketsData.Fill(checkBasketsDataTable);

                                int orderBasketsCount = 0;
                                foreach (DataRow row1 in checkBasketsDataTable.Rows)
                                {
                                    orderBasketsCount++;
                                }
                                int orderBasketsCollectedCount = 0;
                                foreach (DataRow row1 in checkBasketsDataTable.Rows)
                                {
                                    if ((int)row1[4] > 0)
                                    {
                                        orderBasketsCollectedCount++;
                                    }
                                }

                                desc.TableName = row[1].ToString();
                                desc.priority = Convert.ToInt32(row[2]);
                                desc.order_id = Convert.ToInt32(row[0]);
                                desc.DestinationZone = dest_zone;         // HERE ZONE in desc

                                string stu = "SELECT bId, basketBarcode FROM orderbaskets WHERE orderId=" + row[0] + " AND compId=" + myID + ";";
                                MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                                DataTable carts = new DataTable();
                                da1.Fill(carts);


                                OrderBorder ordbrd = new OrderBorder()
                                {
                                    quantity = tot_q.ToString()
                                };
                                if (orderBasketsCount == 0)
                                {
                                    ordbrd.Background = Brushes.Green;
                                    ordbrd.orderName_textBlock.Foreground = Brushes.White;
                                    ordbrd.count_textBlock.Foreground = Brushes.White;
                                }
                                if (orderBasketsCount > 0)
                                {
                                    ordbrd.Background = (Brush)(new System.Windows.Media.BrushConverter()).ConvertFromString("#FFE7D700");
                                }
                                if (orderBasketsCollectedCount >= 1)
                                {
                                    ordbrd.Background = Brushes.Orange;
                                }
                                if (checkZonesDataTable.Rows.Count > 0)
                                {
                                    float collectedPerc = (float)collectedPartsCount / ((float)checkZonesDataTable.Rows.Count);
                                    if (collectedPerc > 0.7)
                                    {
                                        ordbrd.Background = (Brush)(new System.Windows.Media.BrushConverter()).ConvertFromString("#FFDA3F3F");
                                        ordbrd.orderName_textBlock.Foreground = Brushes.White;
                                        ordbrd.count_textBlock.Foreground = Brushes.White;
                                    }
                                }
                               /* string stuSs = "SELECT * FROM smartshelfchannels WHERE orderId=" + row[0] + ";";
                                MySqlDataAdapter dataSs = new MySqlDataAdapter(stuSs, conn);
                                DataTable dataTableSs = new DataTable();
                                dataSs.Fill(dataTableSs);
                                if (dataTableSs.Rows.Count == 0)
                                {
                                    ordbrd.Background = (Brush)(new System.Windows.Media.BrushConverter()).ConvertFromString("#FF324FB3");
                                    ordbrd.orderName_textBlock.Foreground = Brushes.White;
                                    ordbrd.count_textBlock.Foreground = Brushes.White;
                                    desc.priority = -1;
                                }
                                */
                                ordbrd.orderId = (int)row[0];
                                ordbrd.MouseLeftButtonDown += new MouseButtonEventHandler(OrderBorder_Click);

                                /// place the order in the list according to its priority. Or if order is in process place on top
                                int i = 0;
                                if (r.Length == 0)
                                {
                                    if (carts.Rows.Count > 0)
                                    {
                                        string c = carts.Rows[0][0].ToString();
                                        for (int iny = 1; iny < carts.Rows.Count; iny++)
                                        {
                                            c += ", " + carts.Rows[iny][0];
                                        }
                                        MySqlCommand cmd = new MySqlCommand();
                                        cmd.Connection = conn;
                                        cmd.CommandText = "DELETE FROM orderbaskets WHERE bId IN(" + c + ");";
                                        cmd.Prepare();
                                        cmd.ExecuteNonQuery();
                                    }
                                    for (i = 0; i < order_list.Count; i++)
                                    {
                                        if (order_list[i].priority > desc.priority && !order_list[i].InProcess)
                                        {
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    desc.InProcess = true;
                                    desc.existingCarts = carts;
                                }
                                Collect_sp.Children.Add(ordbrd);
                                desc.MyBorder = ordbrd;
                                order_list.Add(desc);           //HERE desc inserts into order_list
                                sborka.Header = "Сборка (" + order_list.Count + ")";
                                MediaPlayer mplayer = new MediaPlayer();
                                mplayer.Open(new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "1.wav")));
                                mplayer.Volume = 100;
                                mplayer.Play();
                            }
                        }
                    }
                }

                //Sort by priority
                order_list = order_list.OrderByDescending(a => a.priority).ThenBy(a => a.order_id).ToList<Order>();
                Collect_sp.Children.Clear();
                foreach (Order o in order_list)
                {
                    if (o.InProcess)
                    {
                        Collect_sp.Children.Insert(0, o.MyBorder);
                    }
                    else
                    {
                        Collect_sp.Children.Add(o.MyBorder);
                    }
                    if (Collect_sp.Children.Count >= 40)
                    {
                        break;
                    }
                }

                // baskets monitoring
            /*    DataTable basketsDataTable = new DataTable();
                string stuBaskets = "SELECT * FROM orderbaskets WHERE compId=" + myID + " ORDER BY orderId ASC;";
                MySqlDataAdapter basketsData = new MySqlDataAdapter(stuBaskets, conn);
                basketsData.Fill(basketsDataTable);

                foreach (DataRow basketRow in basketsDataTable.Rows)
                {
                    DataTable ssDataTable = new DataTable();
                    string stuSs = "SELECT * FROM smartshelfchannels WHERE orderId=" + basketRow[2] + ";";
                    MySqlDataAdapter ssData = new MySqlDataAdapter(stuSs, conn);
                    ssData.Fill(ssDataTable);
                    TextBlock foundTb = baskets_sp.FindName("b" + basketRow[1].ToString()) as TextBlock;

                    if ((Int32)basketRow[4] < 1 && ssDataTable.Rows.Count > 0 && foundTb == null)
                    {
                        TextBlock tb = new TextBlock();
                        tb.Text = basketRow[1].ToString();
                        baskets_sp.RegisterName("b" + basketRow[1].ToString(), tb);
                        baskets_sp.Children.Add(tb);
                    }
                    else if ((Int32)basketRow[4] >= 1 && ssDataTable.Rows.Count > 0 && foundTb != null)
                    {
                        baskets_sp.Children.Remove(foundTb);
                    }
                }
                */
                DataTable podpitka_table = new DataTable();
                {
                    string stu = "SELECT id, docName, docNumber FROM podpitkaDocuments WHERE id > " + last_podpitka_id + " ORDER BY id ASC";
                    MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                    da1.Fill(podpitka_table);



                    /*string stu = "SELECT podpitka_table.id_pod, locations.PTL, podpitka_table.quantity, "
                        + "podpitka_table.status, podpitka_table.act_quant,locations.address, medicaments.md_name, podpitka_table.series "
                        + "FROM podpitka_table INNER JOIN medicaments ON podpitka_table.md_code=medicaments.code "
                        + "INNER JOIN locations ON medicaments.id=locations.md_id AND podpitka_table.address="
                        + "locations.address WHERE podpitka_table.id_pod>" + last_podpitka_id
                        + " AND podpitka_table.zone_id IN(" + myzones_str + ") AND podpitka_table.status<1 "
                        + "ORDER BY podpitka_table.id_pod ASC";
                    MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                    da1.Fill(podpitka_table);*/
                }
                if (podpitka_table.Rows.Count > 0)
                {
                    last_podpitka_id = Convert.ToInt32(podpitka_table.Rows[podpitka_table.Rows.Count - 1][0]);
                    foreach (DataRow row in podpitka_table.Rows)
                    {
                        NewComing desc_p = new NewComing();
                        {
                            string stu = "SELECT podpitkatable.idPod, locations.PTL, podpitkatable.quantity, "
                        + "podpitkatable.status, podpitkatable.actQuant, locations.address, medicaments.mdName, podpitkatable.series "
                        + "FROM podpitkatable INNER JOIN medicaments ON podpitkatable.mdCode=medicaments.code "
                        + "INNER JOIN locations ON medicaments.id=locations.mdId AND podpitkatable.address="
                        + "locations.address WHERE podpitkatable.zoneId IN(" + myzones_str + ") AND podpitkatable.status<1 AND podpitkatable.docId = '" + row[0] + "';";
                            MySqlDataAdapter da1 = new MySqlDataAdapter(stu, conn);
                            da1.Fill(desc_p);
                        }

                        if (desc_p.Rows.Count > 0)
                        {
                            desc_p.Columns[0].ColumnName = "id_pod";
                            desc_p.Columns[1].ColumnName = "PTL";
                            desc_p.Columns[2].ColumnName = "Quantity";
                            desc_p.Columns[3].ColumnName = "status";
                            desc_p.Columns[4].ColumnName = "act_quant";
                            desc_p.Columns[5].ColumnName = "address";
                            desc_p.Columns[6].ColumnName = "md_name";
                            desc_p.Columns[7].ColumnName = "series";

                            DataRow[] r = desc_p.Select("status = 1");

                            if (r.Length >= desc_p.Rows.Count)
                            {
                                continue;
                            }

                            desc_p.TableName = row[1].ToString();
                            OrderBorder ordbrd = new OrderBorder();
                            podpitka_sp.Children.Add(ordbrd);
                            desc_p.MyBorder = ordbrd;

                            podpitka_list.Add(desc_p);
                            podpitka_tab.Header = "Подпитка (" + podpitka_list.Count + ")";
                            MediaPlayer mplayer = new MediaPlayer();
                            mplayer.Open(new Uri(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "1.wav")));
                            mplayer.Volume = 100;
                            mplayer.Play();

                        }
                    }

                }
                closeConn();





                /*
                if (podpitka_table.Rows.Count > 0)
                {
                    last_podpitka_id = Convert.ToInt32(podpitka_table.Rows[podpitka_table.Rows.Count - 1][0]);

                    podpitka_table.Columns[0].ColumnName = "id_pod";
                    podpitka_table.Columns[1].ColumnName = "PTL";
                    podpitka_table.Columns[2].ColumnName = "Quantity";
                    podpitka_table.Columns[3].ColumnName = "status";
                    podpitka_table.Columns[4].ColumnName = "act_quant";
                    podpitka_table.Columns[5].ColumnName = "address";
                    podpitka_table.Columns[6].ColumnName = "md_name";
                    podpitka_table.Columns[7].ColumnName = "series";

                    if (podpitka_list.Rows.Count == 0)
                    {
                        podpitka_list = podpitka_table;
                    }
                    else
                    {
                        podpitka_list.Merge(podpitka_table);
                    }

                    podpitka_tab.Header = "Подпитка (" + podpitka_list.Rows.Count + ")";

                    foreach (DataRow row in podpitka_table.Rows)
                    {
                        OrderBorder ordbrd = new OrderBorder()
                        {
                            orderName = row[6].ToString(),
                            quantity = row[2].ToString(),
                            ID = Convert.ToInt32(row[0])
                        };
                        podpitka_sp.Children.Add(ordbrd);
                    }
                }

                closeConn();*/
            }
            catch (Exception ex)
            {
                MessageBoxResult result = MessageBox.Show(ex.Message);
                if (result == MessageBoxResult.OK)
                {
                    Application.Current.Shutdown();
                }
            }
            timer_db.Start();
        }

        /// <summary>
        /// Gets the list of all ip addresses on this computer
        /// </summary>
        /// <returns></returns>
        public static List<string> GetLocalIPAddress()
        {
            List<string> list = new List<string>();
            /*while (true)
            {*/
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    list.Add(ip.ToString());
                }
            }
            if (list.Count > 0)
            {
                return list;
            }
            //Thread.Sleep(1000);
            //}
            throw new Exception("Local IP Address Not Found!");
        }

        public void establishConn()
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "serverip.txt"), Encoding.Default);
            }
            catch
            {
                throw new NotFoundException("Файл настроек поврежден!");
            }
            string connectionString = "server=" + lines[0] + ";uid=newuser;pwd=virtshelf;database=ptl_database";

            try
            {
                conn.Close();
            }
            catch { }

            try
            {
                conn = new MySqlConnection(connectionString);
                conn.Open();
            }
            catch
            {
                throw new DBNotConnectedException("Проверьте соединение с базой данных!");
            }
        }

        private void OrderBorder_Click(object sender, MouseButtonEventArgs e)           // кнопка начать сборку
        {
            if (e.ClickCount == 2)
            {

                bool ok = true;
                /*for (int i = 0; i < StatusGrid.Children.Count; i++)
                {
                    if (!(StatusGrid.Children[i] as GatewayStatus).On)
                    {
                        ok = false;
                        break;
                    }
                }*/
                if (ok)
                {
                    this.IsEnabled = false;
                    foreach (int g in gateway_nums)
                    {
                        ptlconn.ClearNode(g, 252);
                    }
                    Order neededOrder = order_list.Find(item => item.order_id == ((OrderBorder)sender).orderId);
                    OrderWindow ordwnd = new OrderWindow(neededOrder, myPTLs, all_baskets, conn);     // берем первый из списка заказ
                                                                                                      /*ordwnd.HideMe += (se, ev) =>
                                                                                                      {
                                                                                                          this.IsEnabled = true;
                                                                                                          current_Order = null;
                                                                                                          (se as OrderWindow).Hide();
                                                                                                          this.Focus();
                                                                                                      };*/
                    ordwnd.Closed += (se, ev) =>
                    {
                        this.IsEnabled = true;
                        if ((se as OrderWindow).Confirmed)
                        {
                            order_list.Remove((se as OrderWindow).orders);
                            Collect_sp.Children.Remove(cur_ord.orders.MyBorder);                      //IMPORTANT
                            string head = "Сборка";
                            if (order_list.Count > 0)
                            {
                                head += " (" + order_list.Count + ")";
                            }

                            sborka.Header = head;
                        }
                        else
                        {
                            foreach (int g in gateway_nums)
                            {
                                ptlconn.ClearNode(g, 252);
                            }
                        }
                        current_Order = null;
                        order_list.Clear();
                        last_order_id = 0;
                        last_order_id1 = 0;
                        Collect_sp.Children.Clear();
                        this.Focus();
                    };
                    current_Order = ordwnd;

                    current_Order.Open();    // Вызываем функцию Open в OrderWindows
                }
                else if (!ok)
                {
                    MessageBox.Show("Не все шлюзы соединены. Проверьте соединение.");
                }
                else if (order_list.Count < 1)
                {
                    MessageBox.Show("Тапсырыс жоқ.");
                }
            }
        }


        private void Collect_but_Click(object sender, RoutedEventArgs e)           // кнопка начать сборку
        {
            CapsAPI.AB_LCD_DspMsg(1, -5, "Akerke");
            CapsAPI.AB_LCD_DspNum(1, -5, 200);
           
            bool ok = true;
            if (ok && order_list.Count > 0)
            {
                //order_list[0].InProcess = true;
                this.IsEnabled = false;
                foreach (int g in gateway_nums)
                {
                    ptlconn.ClearNode(g, 252);
                }

                OrderWindow ordwnd = new OrderWindow(order_list[0], myPTLs, all_baskets, conn);     // берем первый из списка заказ
                ordwnd.Closed += (se, ev) =>
                {
                    this.IsEnabled = true;
                    if ((se as OrderWindow).Confirmed)
                    {
                        order_list.Remove((se as OrderWindow).orders);
                        Collect_sp.Children.Remove(cur_ord.orders.MyBorder);                      //IMPORTANT
                        string head = "Сборка";
                        if (order_list.Count > 0)
                        {
                            head += " (" + order_list.Count + ")";
                        }

                        sborka.Header = head;
                    }
                    else
                    {
                        foreach (int g in gateway_nums)
                        {
                            ptlconn.ClearNode(g, 252);
                        }
                    }
                    current_Order = null;
                    order_list.Clear();
                    last_order_id = 0;
                    last_order_id1 = 0;
                    Collect_sp.Children.Clear();
                    this.Focus();
                };
                current_Order = ordwnd;

                current_Order.Open();    // Вызываем функцию Open в OrderWindows
            }
            else if (!ok)
            {
                MessageBox.Show("Не все шлюзы соединены. Проверьте соединение.");
            }
            else if (order_list.Count < 1)
            {
                MessageBox.Show("Тапсырыс жоқ.");
            }
        }

        private void Replenish_but_Click(object sender, RoutedEventArgs e)                       //кнопка начать загрузку
        {
            EnterCartBarcode_forReplenishment ent = new EnterCartBarcode_forReplenishment(this);
            ent.BarcodeEnetered += (se, ev) =>
            {
                bool ok = true;
                /*for (int i = 0; i < StatusGrid.Children.Count; i++)
                {
                    if (!(StatusGrid.Children[i] as GatewayStatus).On)
                    {
                        ok = false;
                        break;
                    }
                }*/
                NewComing foundcode_table = null;
                foreach (NewComing table in newcomings_list)
                {
                    if (table.TableName == ev.barcode)
                    {
                        foundcode_table = table;
                        break;
                    }
                }
                if (foundcode_table != null)
                {
                    if (newcomings_list.Count > 0 && ok)
                    {
                        foreach (int g in gateway_nums)
                        {
                            ptlconn.ClearNode(g, 252);
                        }

                        ReplenishmentWindow rpl = new ReplenishmentWindow(foundcode_table, myPTLs);
                        rpl.Closed += (s, eve) =>
                        {
                            this.IsEnabled = true;
                            if ((s as ReplenishmentWindow).Confirmed)
                            {
                                newcomings_list.Remove((s as ReplenishmentWindow).TableReplenishment);
                                Replenish_sp.Children.Remove((s as ReplenishmentWindow).TableReplenishment.MyBorder);
                                string head = "Загрузка";
                                if (newcomings_list.Count > 0)
                                {
                                    head += " (" + newcomings_list.Count + ")";
                                }

                                zagruzka.Header = head;
                            }
                            else
                            {
                                foreach (int g in gateway_nums)
                                {
                                    ptlconn.ClearNode(g, 252);
                                }
                            }
                            current_newcoming = null;
                            this.Focus();
                        };
                        rpl.Show();
                        current_newcoming = rpl;

                    }
                    else if (!ok)
                    {
                        MessageBox.Show("Не все шлюзы соединены. Проверьте соединение.");
                    }
                    else if (newcomings_list.Count < 1)
                    {
                        MessageBox.Show("Нет корзин в списке.");
                    }
                }
                else
                {
                    Replenish_but_Click(Replenish_but, new RoutedEventArgs());
                }
            };
            ent.Closed += (se, ev) =>
            {
                IsEnabled = true;
            };
            ent.Show();
        }

        private void inventory_button_Click(object sender, RoutedEventArgs e)
        {

            bool ok = true;
            /*for (int i = 0; i < StatusGrid.Children.Count; i++)
            {
                if (!(StatusGrid.Children[i] as GatewayStatus).On)
                {
                    ok = false;
                    break;
                }
            }*/
            if (ok && inventories_list.Count > 0)
            {
                foreach (int g in gateway_nums)
                {
                    ptlconn.ClearNode(g, 252);
                }

                InventoryWindow rpl = new InventoryWindow(inventories_list[0], myPTLs, conn);
                rpl.Closed += (s, eve) =>
                {
                    this.IsEnabled = true;
                    if ((s as InventoryWindow).Confirmed)
                    {
                        inventories_list.Remove((s as InventoryWindow).orders);
                        Inventory_sp.Children.Remove((s as InventoryWindow).orders.MyBorder);
                        string head = "Инвентаризация";
                        if (inventories_list.Count > 0)
                        {
                            head += " (" + inventories_list.Count + ")";
                        }

                        inventory.Header = head;
                    }
                    else
                    {
                        foreach (int g in gateway_nums)
                        {
                            ptlconn.ClearNode(g, 252);
                        }
                    }
                    current_inventory = null;
                    this.Focus();
                };
                this.IsEnabled = false;
                rpl.Show();
                current_inventory = rpl;
            }
            else if (!ok)
            {
                MessageBox.Show("Не все шлюзы соединены. Проверьте соединение.");
            }
            else if (inventories_list.Count < 1)
            {
                MessageBox.Show("Инвентаризация жасауға тапсырма жоқ.");
            }
        }

        private void podpitka_button_Click(object sender, RoutedEventArgs e)        // кнопка подпитка!
        {
            bool ok = true;
            /*for (int i = 0; i < StatusGrid.Children.Count; i++)
            {
                if (!(StatusGrid.Children[i] as GatewayStatus).On)
                {
                    ok = false;
                    break;
                }
            }*/
            if (ok && podpitka_list.Count > 0)
            {
                PodpitkaWindow pdtp = new PodpitkaWindow(podpitka_list[0], myPTLs);
                pdtp.Closed += (se, ev) =>
                {
                    this.IsEnabled = true;
                    if ((se as PodpitkaWindow).Confirmed)
                    {
                        podpitka_list.Remove((se as PodpitkaWindow).TablePodpitka);
                        podpitka_sp.Children.Remove(current_pod_window.TablePodpitka.MyBorder);
                    }
                    else
                    {
                        foreach (int g in gateway_nums)
                        {
                            ptlconn.ClearNode(g, 252);
                        }
                    }
                    current_pod_window = null;
                    this.Focus();

                };
                pdtp.Show();
                this.IsEnabled = false;
                current_pod_window = pdtp;
            }
            else if (!ok)
            {
                MessageBox.Show("Не все шлюзы соединены. Проверьте соединение.");
            }
            else if (podpitka_list.Count < 1)
            {
                MessageBox.Show("Қосымша толтыруға тапсырма жоқ.");
            }

        }

        private void Refresh_but_Click(object sender, RoutedEventArgs e)
        {
            order_list.Clear();
            last_order_id = 0;
            last_order_id1 = 0;
            Collect_sp.Children.Clear();
        }

        private void clear_button_Click(object sender, RoutedEventArgs e)
        {
            establishConn();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "DELETE FROM orders; DELETE FROM ordersdescription; DELETE FROM new_comings; DELETE FROM new_comings_description; DELETE FROM new_comings_documents; DELETE FROM orderbaskets; DELETE FROM podpitkatable; DELETE FROM podpitkadocuments; DELETE FROM inventoriestable";
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
            catch { }

            closeConn();
            MessageBox.Show("База данных очищена. Можете закрыть программу.");
        }

        private void clear_button2_Click(object sender, RoutedEventArgs e)
        {
            establishConn();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = "UPDATE smartshelfchannels SET orderId = -channelId;";
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
            catch { }

            closeConn();
            MessageBox.Show("Смарт шкаф очищен.");
        }

        private void baskets_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text.Length == 13)
            {
                foreach (Object o in baskets_sp.Children)
                {
                    TextBlock t = o as TextBlock;
                    if (t.Text == tb.Text)
                    {
                        t.Background = (Brush)(new System.Windows.Media.BrushConverter()).ConvertFromString("#FF8F089A");
                        t.Foreground = Brushes.White;
                    }
                }
            }
        }

        public void closeConn()
        {
            conn.Close();
        }
    }
}
