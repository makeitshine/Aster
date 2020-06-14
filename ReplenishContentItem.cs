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
    class ReplenishContentItem : Grid
    {
        private double width_of;
        private double height_of;
        //public event EventHandler CloseKAS;
        public event EventHandler ShowButtonPressed;
        public event EventHandler ConfirmButtonPressed;
        public event EventHandler CancelButtonPressed;
        //public event EventHandler ExpandMe;
 
        Border minus, plus, minus2, plus2;
        StackPanel sp;
        SolidColorBrush mybrush;
        TextBlock quantityText2;
        TextBox quantityText;
        bool expanded;
        int id, act_quant, quant_coll, quant_pod;
        private bool _confirmed, ser_confirmed, cancel;

        public bool cancellation
        {
            set
            {
                cancel = value;
            }
            get
            {
                return cancel;
            }
        }

        public int ActualQuantity
        {
            get
            {
                return act_quant;
            }
        }

        public bool Series_confirmed
        {
            set
            {
                ser_confirmed = value;
            }
            get
            {
                return ser_confirmed;
            }
        }

        public bool Confirmed
        {
            set
            {
                _confirmed = value;
                if (value)
                {
                    Children.OfType<Button>().First().IsEnabled = false;
                    sp.Children.OfType<Button>().First().IsEnabled = false;
                    sp.Children.OfType<Button>().Last().IsEnabled = false;
                    //Expanded = false;
                    minus.IsEnabled = false;
                    plus.IsEnabled = false;
                    minus2.IsEnabled = false;
                    plus2.IsEnabled = false;
                    if (cancellation)
                        Background = Brushes.Yellow;
                    else
                        Background = Brushes.LightGray;
                }
                else
                {

                }
            }
            get
            {
                return _confirmed;
            }
        }

        public bool Expanded
        {
            get
            {
                return expanded;
            }
            set
            {
                expanded = value;
                if (value)
                {
                    QuantityPodpitka = act_quant - quant_coll;
                    minus.IsEnabled = false;
                    plus.IsEnabled = false;
                    Children.OfType<Button>().First().IsEnabled = false;

                    DoubleAnimation dbl = new DoubleAnimation()
                    {
                        To = 40,
                        Duration = new Duration(TimeSpan.FromMilliseconds(100))
                    };
                    sp.BeginAnimation(HeightProperty, dbl);

                }
                else
                {
                    DoubleAnimation dbl = new DoubleAnimation()
                    {
                        To = 0,
                        Duration = new Duration(TimeSpan.FromMilliseconds(100))
                    };
                    sp.BeginAnimation(HeightProperty, dbl);
                }
            }
        }

        public int QuantityReplenished
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

        public int QuantityPodpitka
        {
            set
            {
                quant_pod = value;
                quantityText2.Text = value.ToString();
            }
            get
            {
                return quant_pod;
            }
        }

        public int RefID { get; set; }
        public int PTL_global { get; set; }
        /// <summary>
        /// A grid representing one item of the order. Appears in stack panel in the order Window.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="barcode"></param>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="expiration"></param>
        /// <param name="quant"></param>
        public ReplenishContentItem(int id, int ref_id, string[] barcodes, string name, string address, string series, int quant, int global_ptl)
        {

            RefID = ref_id;
            _confirmed = false;
            ser_confirmed = false;
            cancel = false;
            PTL_global = global_ptl;
            this.id = id;
            act_quant = quant;
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
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Star) }); // Quantity
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Star) }); // Quantity replenished
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) }); // OK button
            RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Main row
            RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Expandable row with confirm button

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
            Grid.SetColumnSpan(frame, 8);
            Grid.SetRow(frame, 0);
            Grid.SetRowSpan(frame, 2);
            Children.Add(frame);

            // shows item id
            TextBlock id_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 5, 2),
                FontSize = 18,
                Text = id.ToString(),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            SetColumn(id_block, 0);
            SetRow(id_block, 0);
            Children.Add(id_block);

            // shows item barcode
            TextBlock barcode_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 5, 2),
                FontSize = 18,
                Text = barcodes[0]
            };
            SetColumn(barcode_block, 1);
            SetRow(barcode_block, 0);
            Children.Add(barcode_block);

            // shows item name
            TextBlock name_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 5, 2),
                FontSize = 18,
                Text = name
            };

            SetColumn(name_block, 2);
            SetRow(name_block, 0);
            Children.Add(name_block);

            // shows item address
            TextBlock address_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 4, 2),
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = address
            };
            SetColumn(address_block, 3);
            SetRow(address_block, 0);
            Children.Add(address_block);

            //shows item expiratio date
            /*TextBlock exp_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 4, 2),
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Right,
                Text = expiration
            };
            SetColumn(exp_block, 4);
            SetRow(exp_block, 0);
            Children.Add(exp_block);*/


            TextBlock series_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 4, 2),
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = series
            };
            SetColumn(series_block, 4);
            SetRow(series_block, 0);
            Children.Add(series_block);

            // shows actual quantity of an item
            TextBlock quant_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 4, 2),
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = quant.ToString()
            };
            SetColumn(quant_block, 5);
            SetRow(quant_block, 0);
            Children.Add(quant_block);

            {
                // a grid containing 3 columns for +/- buttons and the actual quanity collected
                Grid quan = new Grid();
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                SetColumn(quan, 6);
                SetRow(quan, 0);
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
                /**min = new Button()
                {
                    Content = "-"
                };*/
                minus.MouseDown += (se, ev) =>
                {
                    if (quant_coll > 0)
                    {
                        QuantityReplenished -= 1;
                    }
                };
                SetColumn(minus, 0);
                quan.Children.Add(minus);

                // the replenished quantity text block
                quantityText = new TextBox()
                {
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center     
                };
                quantityText.TextChanged += quantityText_TextChanged;
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
                /*pl = new Button()
                {
                    Content = "+"
                };*/
                plus.MouseDown += (se, ev) =>
                {
                    if (quant_coll < act_quant)
                    {
                        QuantityReplenished += 1;
                    }
                };
                SetColumn(plus, 2);
                quan.Children.Add(plus);
            }
            TextBlock txt = new TextBlock()
            {
                FontSize = 16,
                Margin = new Thickness(4, 0, 4, 0),
                Text = "OK"
            };

            Button btn = new Button()
            {
                Content = txt,
                Margin = new Thickness(5, 3, 5, 2)
            };
            btn.Click += (se, ev) =>
            {
                ShowButtonPressed?.Invoke(this, new EventArgs());
            };
            SetColumn(btn, 7);
            SetRow(btn, 0);
            Children.Add(btn);

            {
                // A stack panel on the second expandable row, containing confirm button
                sp = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Height = 0
                };
                SetColumn(sp, 0);
                SetColumnSpan(sp, 7);
                SetRow(sp, 1);
                Children.Add(sp);

                TextBlock potp = new TextBlock()
                {
                    FontSize = 16,
                    Margin = new Thickness(4, 0, 4, 0),
                    Text = "Подпитка: ",
                    VerticalAlignment = VerticalAlignment.Center
                };
                sp.Children.Add(potp);

                int rad = 10;
                minus2 = new Border()
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
                    Background = Brushes.White,
                    Margin = new Thickness(5, 0, 5, 0)
                };
                minus2.MouseEnter += (se, ev) =>
                {
                    (se as Border).Background = Brushes.LightBlue;
                };
                minus2.MouseLeave += (se, ev) =>
                {
                    (se as Border).Background = Brushes.White;
                };
                minus2.MouseDown += (se, ev) =>
                {
                    if (QuantityPodpitka > 0)
                    {
                        QuantityPodpitka -= 1;
                    }
                };
                sp.Children.Add(minus2);

                // the actual quanityt text block
                quantityText2 = new TextBlock()
                {
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(5, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                SetColumn(quantityText2, 1);
                sp.Children.Add(quantityText2);

                // plus(+) button 
                plus2 = new Border()
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
                    Background = Brushes.White,
                    Margin = new Thickness(5, 0, 5, 0)
                };
                plus2.MouseEnter += (se, ev) =>
                {
                    (se as Border).Background = Brushes.LightBlue;
                };
                plus2.MouseLeave += (se, ev) =>
                {
                    (se as Border).Background = Brushes.White;
                };
                /*pl = new Button()
                {
                    Content = "+"
                };*/
                plus2.MouseDown += (se, ev) =>
                {
                    if ((quant_coll + QuantityPodpitka) < act_quant)
                    {
                        QuantityPodpitka += 1;
                    }
                };

                sp.Children.Add(plus2);

                Button conf = new Button()
                {
                    Content = "OK",
                    Margin = new Thickness(5)
                };
                conf.Click += (se, ev) =>
                {
                    ConfirmButtonPressed?.Invoke(this, new EventArgs());
                };
                sp.Children.Add(conf);
                Button canc = new Button()
                {
                    Content = "Отмена",
                    Margin = new Thickness(5)
                };
                canc.Click += (se, ev) =>
                {
                    CancelButtonPressed?.Invoke(this, new EventArgs());
                    Expanded = false;
                };
                sp.Children.Add(canc);

            }
        }
        private void quantityText_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(quantityText.Text))
            {
                
            }
            else
            {
                int q = Convert.ToInt32(quantityText.Text);
                if (q <= act_quant)
                {
                    QuantityReplenished = q;
                    QuantityPodpitka = act_quant - QuantityReplenished;
                }
                else
                {
                    QuantityReplenished = act_quant;
                    QuantityPodpitka = 0;
                    MessageBox.Show("Указанное количество не должно превышать заданное количество!");
                }
            }
        }
    }
}
