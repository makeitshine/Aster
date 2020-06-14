using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PTLStation
{
    public class Order : DataTable
    {
        public bool InProcess { get; set; }
        private OrderBorder my_border;
        public string DestinationZone;

        public OrderBorder MyBorder
        {
            get
            {
                return my_border;
            }
            set
            {
                my_border = value;
                if (value != null)
                {
                    my_border.orderName = TableName;
                    //my_border.quantity = Rows.Count.ToString();
                }
            }
        }

        //public string TableDate { get; set; }

        public int order_id { get; set; }
        public int priority { get; set; }

        public DataTable existingCarts { get; set; }

        public Order()
        {
            InProcess = false;
            existingCarts = new DataTable();
        }
    }
    /*public bool InProcess { get; set; }
    private DataTable orders;
    public int priority { get; set; }
    private DataTable PTLs;

    private string order_name;
    private int order_id;

    public event EventHandler smth;

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

    public OrderBorder myBorder { get; set; }

    public Order(DataTable orderTable, DataTable PTLS, string order_name, int order_id)
    {
        InProcess = false;
        orders = orderTable;
        PTLs = PTLS;
        this.order_name = order_name;
        this.order_id = order_id;
    }

    public bool Start(PTLConnection ptlconn)
    {
        foreach (DataRow row in orders.Rows)
        {
            DataRow[] r = PTLs.Select("Global = " + row[1]);
            Direction dir = Direction.Up;
            if (Convert.ToBoolean(r[0][4]))
                dir = Direction.Down;
            if (!ptlconn.SetLEDStatus(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), LEDColor.Blue, LEDStatus.On))
                return false;
            if (!ptlconn.DisplayQuantity(Convert.ToInt32(r[0][2]), Convert.ToInt32(r[0][3]), Convert.ToInt32(row[2]), dir, 0, 0))
                return false;
        }
        return true;
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
            //MessageBox.Show(r.Length.ToString());
            smth?.Invoke(this, new EventArgs());
        }
    }
}*/
}
