using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PTLStation
{
    class PodpitkaContentItem : Grid
    {
        private double width_of;
        private double height_of;
        public event EventHandler ShowButtonPressed;

        Border minus, plus;
        SolidColorBrush mybrush;
        TextBlock quantityText;
        int id, quant_coll;
        public int Quantity { get; set; }

        private bool _confirmed;
        public bool Confirmed
        {
            get
            {
                return _confirmed;
            }
            set
            {
                _confirmed = value;
                if (value)
                {
                    Children.OfType<Button>().First().IsEnabled = false;
                    minus.IsEnabled = false;
                    plus.IsEnabled = false;
                    Background = Brushes.LightGray;
                }
            }
        }

        public bool SentToPTL { get; set; }
        
        public int QuantityCollected
        {
            set
            {
                quant_coll = value;
                quantityText.Text = value.ToString();
            }
            get
            {
                return quant_coll;
            }
        }
        public int RefID { get; set; }
        public int PTL_global { get; set; }
        //public int Quantity { get; set; }
        //public bool Series_sent { get; set; }
        /// <summary>
        /// A grid representing one item of the order. Appears in stack panel in the order Window.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="barcode"></param>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="expiration"></param>
        /// <param name="quant"></param>
        public PodpitkaContentItem(int id, int ref_id, string[] barcodes, string name, string address, int quantity, int global_ptl)
        {
            _confirmed = false;
            SentToPTL = false;
            PTL_global = global_ptl;
            Quantity = quantity;
            RefID = ref_id;
            this.id = id;
            this.Loaded += (s, ev) =>
            {
                mybrush = Background as SolidColorBrush;
            };
            width_of = SystemParameters.PrimaryScreenWidth * 0.6;
            height_of = width_of * 0.75;

            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(4, GridUnitType.Star) }); // Number
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) }); // Barcode
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25, GridUnitType.Star) }); // Name
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Star) }); // Address
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Star) }); // Quantity
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Star) }); // Quantity collected
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) });

            // Just a border around the whole item
            Border frame = new Border()
            {
                BorderBrush = Brushes.Black
            };
            if (id == 1)
                frame.BorderThickness = new Thickness(1);
            else
                frame.BorderThickness = new Thickness(1, 0, 1, 1);

            Grid.SetColumn(frame, 0);
            Grid.SetColumnSpan(frame, 7);
            Children.Add(frame);

            // shows item id
            TextBlock id_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 5, 2),
                FontSize = 18,
                Text = id.ToString()
            };
            SetColumn(id_block, 0);
            Children.Add(id_block);

            // shows item barcode
            TextBlock barcode_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 5, 2),
                FontSize = 18,
                Text = barcodes[0]
            };
            SetColumn(barcode_block, 1);
            Children.Add(barcode_block);

            // shows item name
            TextBlock name_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 5, 2),
                FontSize = 18,
                Text = name
            };

            SetColumn(name_block, 2);
            Children.Add(name_block);

            // shows item address
            TextBlock address_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 4, 2),
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = address
            };
            SetColumn(address_block, 3);
            Children.Add(address_block);

            //shows item expiratio date
            TextBlock exp_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 4, 2),
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = Quantity.ToString()
            };
            SetColumn(exp_block, 4);
            Children.Add(exp_block);

            {
                // a grid containing 3 columns for +/- buttons and the actual quanity collected
                Grid quan = new Grid();
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                SetColumn(quan, 5);
                Children.Add(quan);

                // minus(-) button
                int rad = 10;
                minus = new Border()
                {
                    CornerRadius = new CornerRadius(rad),
                    Width = rad * 2,
                    Height = rad * 2,
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Child = new TextBlock()
                    {
                        Text = "-",
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    Background = Brushes.White

                };
                minus.MouseEnter += (se, ev) =>
                {
                    (se as Border).Background = Brushes.LightBlue;
                };
                minus.MouseLeave += (se, ev) =>
                {
                    (se as Border).Background = Brushes.White;
                };
                minus.MouseDown += (se, ev) =>
                {
                    if (quant_coll > 0)
                    {
                        QuantityCollected -= 1;
                    }
                };
                SetColumn(minus, 0);
                quan.Children.Add(minus);

                // the actual quanityt text block
                quantityText = new TextBlock()
                {
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                SetColumn(quantityText, 1);
                quan.Children.Add(quantityText);
                

                // plus(+) button 
                plus = new Border()
                {
                    CornerRadius = new CornerRadius(rad),
                    Width = rad * 2,
                    Height = rad * 2,
                    BorderThickness = new Thickness(1),
                    BorderBrush = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Child = new TextBlock()
                    {
                        Text = "+",
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    Background = Brushes.White

                };
                plus.MouseEnter += (se, ev) =>
                {
                    (se as Border).Background = Brushes.LightBlue;
                };
                plus.MouseLeave += (se, ev) =>
                {
                    (se as Border).Background = Brushes.White;
                };
                plus.MouseDown += (se, ev) =>
                {
                    if (quant_coll < Quantity)
                    {
                        QuantityCollected += 1;
                    }
                };
                SetColumn(plus, 2);
                quan.Children.Add(plus);
            }

            {
                TextBlock txt = new TextBlock()
                {
                    FontSize = 16,
                    Margin = new Thickness(4, 0, 4, 0)
                };
                txt.Text = "OK";

                Button btn = new Button()
                {
                    Content = txt,
                    Margin = new Thickness(5, 3, 5, 2)
                };
                btn.Click += (se, ev) =>
                {
                    ShowButtonPressed?.Invoke(this, new EventArgs());
                };
                SetColumn(btn, 6);
                Children.Add(btn);
            }
        }
    }
}
