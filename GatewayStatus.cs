using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace PTLStation
{
    /// <summary>
    /// A grid showing an indicator and the id of a gateway
    /// </summary>
    class GatewayStatus : Grid
    {
        Border stat;
        private bool isOn;
        /// <summary>
        /// Sets the indicator red or green if the corresponding gateway is disconnected or connected respctfully. True = connected, False = disconnected
        /// </summary>
        public bool On
        {
            set
            {
                isOn = value;
                if (value)
                {
                    stat.Background = Brushes.Green;
                }
                else
                {
                    stat.Background = Brushes.Red;
                }
            }
            get
            {
                return isOn;
            }
        }

        public GatewayStatus(int n)
        {
            isOn = false;
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            this.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

            int stath = 20;
            stat = new Border()
            {
                Height = stath,
                Width = stath,
                CornerRadius = new CornerRadius(stath / 2),
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                Background = Brushes.Red,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(2)
            };
            Grid.SetColumn(stat, 0);
            Children.Add(stat);

            TextBlock Gid = new TextBlock()
            {
                Text = n.ToString() + " Шлюз",
                FontSize = 19,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(4, 2, 2, 4),
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(Gid, 1);
            Children.Add(Gid);
        }
    }
}
