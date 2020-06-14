using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTLStation
{
    public class Inventory : DataTable
    {
        public bool InProcess { get; set; }
        private OrderBorder my_border;

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
                    my_border.orderName = TableDate;
                    my_border.quantity = Rows.Count.ToString();
                }
            }
        }

        public string TableDate { get; set; }

        public Inventory()
        {
            InProcess = false;
        }

    }
}
