using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PTLStation
{
    class CartBarcodeBorder : Border
    {
        public string Barcode { get; set; }
        public event EventHandler deleteMe;
        private TextBlock cartN;
        
        public int ID
        {
            set
            {
                cartN.Text = "Корзина №" + value;
            }
        }
        public bool _isEN { get; set; }
        
        public CartBarcodeBorder(int id, string barcode)
        {
            _isEN = true;
            Barcode = barcode;
            //this.Height = 50;
            this.BorderThickness = new Thickness(1);
            this.BorderBrush = Brushes.Black;
            this.CornerRadius = new CornerRadius(2);

            Grid mainGrid = new Grid()
            {
                Margin = new Thickness(2)
            };
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(45, GridUnitType.Pixel) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            this.Child = mainGrid;

            Image cartimage = new Image()
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/cart_big.png")),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid.SetColumn(cartimage, 0);
            Grid.SetRow(cartimage, 0);
            Grid.SetRowSpan(cartimage, 2);
            mainGrid.Children.Add(cartimage);

            Image deleteimage = new Image()
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/delete.png")),
                Visibility= Visibility.Collapsed,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            deleteimage.MouseUp += (se, ev) =>
            {
                deleteMe?.Invoke(this, new EventArgs());
            };

            Grid.SetColumn(deleteimage, 0);
            Grid.SetRow(deleteimage, 0);
            Grid.SetRowSpan(deleteimage, 2);
            mainGrid.Children.Add(deleteimage);

            cartN = new TextBlock()
            {
                Text = "Корзина №" + id
            };
            Grid.SetColumn(cartN, 1);
            Grid.SetRow(cartN, 0);
            mainGrid.Children.Add(cartN);

            TextBlock cartB = new TextBlock()
            {
                Text = barcode
            };
            Grid.SetColumn(cartB, 1);
            Grid.SetRow(cartB, 1);
            mainGrid.Children.Add(cartB);

            this.MouseEnter += (se, ev) =>
            {
                Background = Brushes.LightBlue;
                if (_isEN)
                {
                    deleteimage.Visibility = Visibility.Visible;
                    cartimage.Visibility = Visibility.Collapsed;
                }
            };

            this.MouseLeave += (se, ev) =>
            {
                Background = Brushes.White;
                if (_isEN)
                {
                    deleteimage.Visibility = Visibility.Collapsed;
                    cartimage.Visibility = Visibility.Visible;
                }
            };
        }
    }
}
