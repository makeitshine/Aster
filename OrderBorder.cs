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
        public TextBlock orderName_textBlock;

        public TextBlock count_textBlock;

        public int orderId;

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
            this.CornerRadius = new CornerRadius(1);
            this.BorderBrush = Brushes.Gray;
            this.Background = Brushes.Gainsboro;
            //this.Margin = new Thickness(4);

            Grid insideGrid = new Grid()
            {
                Margin = new Thickness(4)
            };
            insideGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) });
            insideGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) });

            Child = insideGrid;

            orderName_textBlock = new TextBlock()
            {
                FontSize = 18   // Шрифт списка продуктов на основной панели
            };
            Grid.SetColumn(orderName_textBlock, 0);
            insideGrid.Children.Add(orderName_textBlock);

            count_textBlock = new TextBlock()
            {
                FontSize = 20,  // шрифт количества товаров (сами числа)
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(count_textBlock, 1);
            insideGrid.Children.Add(count_textBlock);

        }
    }
}
