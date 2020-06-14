using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PTLStation
{
    /// <summary>
    /// A border element for the order, displayed on the stack panel in the main window
    /// </summary>
    public class OrderBorder : Border
    {
        private TextBlock orderName_textBlock;

        private TextBlock count_textBlock;

        public string orderName
        {
            set { orderName_textBlock.Text = value; }
        }

        public string quantity
        {
            set
            {
                count_textBlock.Text = value;
            }
        }

        public int ID { get; set; }

        public OrderBorder()
        {
            this.BorderThickness = new Thickness(1);
            this.CornerRadius = new CornerRadius(3);
            this.BorderBrush = Brushes.Black;
            this.Background = Brushes.LightGray;
            this.Margin = new Thickness(4);

            Grid insideGrid = new Grid()
            {
                Margin = new Thickness(4)
            };
            insideGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            insideGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

            Child = insideGrid;

            orderName_textBlock = new TextBlock()
            {
                FontSize = 16
            };
            Grid.SetRow(orderName_textBlock, 0);
            insideGrid.Children.Add(orderName_textBlock);

            count_textBlock = new TextBlock()
            {
                FontSize = 16
            };
            Grid.SetRow(count_textBlock, 1);
            insideGrid.Children.Add(count_textBlock);

        }
    }
}
