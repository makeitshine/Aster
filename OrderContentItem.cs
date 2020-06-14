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
    class OrderContentItem : Grid
    {
        //public event EventHandler CloseKAS;
        public event EventHandler ShowButtonPressed;
        //public event EventHandler ExpandMe;


        ToolTip tip;

        Border minus, plus;
        //StackPanel sp;
        SolidColorBrush mybrush;
        TextBlock quantityText;
        //bool expanded;
        int id, act_quant, quant_coll;
        //private bool _confirmed;

        public int ActualQuantity
        {
            get
            {
                return act_quant;
            }
        }

        /*public bool Confirmed
        {
            set
            {
                _confirmed = value;
                if (value)
                {
                    sp.Children.OfType<Button>().First().IsEnabled = false;
                    Expanded = false;
                    min.IsEnabled = false;
                    pl.IsEnabled = false;
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
        }*/

        /*public bool Expanded
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
        }*/

        private Status st;
        public Status status
        {
            get
            {
                return st;
            }
            set
            {
                st = value;
                if (value == Status.confirmed)
                {
                    Children.OfType<Button>().First().IsEnabled = false;
                    //Expanded = false;
                    minus.IsEnabled = false;
                    plus.IsEnabled = false;
                    Background = Brushes.LightGray;
                }
            }
        }

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
        /// <summary>
        /// A grid representing one item of the order. Appears in stack panel in the order Window.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="barcode"></param>
        /// <param name="name"></param>
        /// <param name="address"></param>
        /// <param name="expiration"></param>
        /// <param name="quant"></param>
        public OrderContentItem(int id, int ref_id, string[] barcodes, string name, string address, string expiration, int quant, int global_ptl)
        {
            RefID = ref_id;
            PTL_global = global_ptl;
            //_confirmed = false;
            this.id = id;
            act_quant = quant;
            this.Loaded += (s, ev) =>
            {
                mybrush = Background as SolidColorBrush;
            };

            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(4, GridUnitType.Star) }); // Number
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) }); // Barcode
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25, GridUnitType.Star) }); // Name
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Star) }); // Address
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Star) }); // Expiration Date
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Star) }); // Quantity
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Star) }); // Quantity collected
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) });
            //RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Main row
            //RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Expandable row with confirm button

            // Just a border around the whole item
            Border frame = new Border()
            {
                BorderBrush = Brushes.Gray,

            };
                frame.BorderThickness = new Thickness(0, 0, 1, 1);

            Grid.SetColumn(frame, 0);
            Grid.SetColumnSpan(frame, 8);
            /*Grid.SetRow(frame, 0);
            Grid.SetRowSpan(frame, 2);*/
            Children.Add(frame);

            for (int i = 0; i < 7; i++)
            {
                Border div = new Border()
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 0, 1, 0)
                };
                Grid.SetColumn(div, i);
                Children.Add(div);
            }

            // shows item id
            TextBlock id_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 5, 2),
                FontSize = 16,
                Text = id.ToString(),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            SetColumn(id_block, 0);
            //SetRow(id_block, 0);
            Children.Add(id_block);

            // shows item barcode
            TextBlock barcode_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 5, 2),
                FontSize = 16,
                Text = barcodes[0]
            };
            SetColumn(barcode_block, 1);
            //SetRow(barcode_block, 0);
            Children.Add(barcode_block);

            // shows item name
            TextBlock name_block = new TextBlock()
            {
                Margin = new Thickness(5, 2, 5, 2),
                FontSize = 16,          // шрифт названий препаратов в "начать сборку" после добавления корзины
                Text = name
            };

            SetColumn(name_block, 2);
            //SetRow(name_block, 0);
            Children.Add(name_block);

            name_block.MouseEnter += new System.Windows.Input.MouseEventHandler(Name_block_MouseEnter);
            name_block.MouseLeave += new System.Windows.Input.MouseEventHandler(Name_block_MouseLeave);

            // shows item address
            TextBlock address_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 4, 2),
                FontSize = 16,      // шрифт адрессов хранения в "начать сборку" после добавления корзины
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = address
            };
            SetColumn(address_block, 3);
            //SetRow(address_block, 0);
            Children.Add(address_block);

            //shows item expiratio date
            TextBlock exp_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 4, 2),
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = expiration
            };
            SetColumn(exp_block, 4);
            //SetRow(exp_block, 0);
            Children.Add(exp_block);

            // shows actual quantity of an item
            TextBlock quant_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 4, 2),
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = quant.ToString()
            };
            SetColumn(quant_block, 5);
            //SetRow(quant_block, 0);
            Children.Add(quant_block);

            {
                // a grid containing 3 columns for +/- buttons and the actual quanity collected
                Grid quan = new Grid();
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                SetColumn(quan, 6);
                //SetRow(quan, 0);
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
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        FontSize = 14
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
                        QuantityCollected -= 1;
                    }
                };
                SetColumn(minus, 0);
                quan.Children.Add(minus);

                // the actual quanityt text block
                quantityText = new TextBlock()
                {
                    FontSize = 16,
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
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        FontSize = 14
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
                        QuantityCollected += 1;
                    }
                };
                SetColumn(plus, 2);
                quan.Children.Add(plus);
            }

            {
                // A stack panel on the second expandable row, containing confirm button
                /*sp = new StackPanel()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Height = 0
                };
                SetColumn(sp, 0);
                SetColumnSpan(sp, 7);
                SetRow(sp, 1);
                Children.Add(sp);*/

                {
                    TextBlock txt = new TextBlock()
                    {
                        FontSize = 14,
                        Margin = new Thickness(4, 0, 4, 0)
                    };
                    txt.Text = "OK";

                    Button btn = new Button()
                    {
                        Content = txt,
                        Margin = new Thickness(3),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    btn.Click += (se, ev) =>
                    {
                        ShowButtonPressed?.Invoke(this, new EventArgs());
                    };
                    SetColumn(btn, 7);
                    //SetRow(btn, 0);
                    Children.Add(btn);
                }
            }

            /*{
                // click on the grid events
                UIElement source = null;
                bool captured = false;

                MouseDown += (se, ev) =>
                {
                    source = (UIElement)se;
                    ev.MouseDevice.Capture(source);
                    captured = true;
                };

                MouseUp += (se, ev) =>
                {
                    if (captured)
                    {
                        ev.MouseDevice.Capture(null);
                        captured = false;

                        ExpandMe?.Invoke(this, new EventArgs());
                    }
                };
            }*/
        }

        private void Name_block_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            tip = new ToolTip();
            tip.Content = (sender as TextBlock).Text;
            tip.IsOpen = true;
        }
        private void Name_block_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {

            if(tip != null)
            {
                tip.IsOpen = false;
                tip = null;
            }
        }

        /*public void AnimateMe()
        {
            SynchronizationContext _uiContext = SynchronizationContext.Current;
            SolidColorBrush[] sld = new SolidColorBrush[2];
            sld[0] = Brushes.Red;
            sld[1] = mybrush;
            Background = sld[0];
            int i = 1;
            System.Timers.Timer p_timer = new System.Timers.Timer(300);
            p_timer.AutoReset = false;
            p_timer.Elapsed += (se, ev) =>
            {
                _uiContext.Post(new SendOrPostCallback(new Action<object>(o =>
                {
                    Background = sld[i % 2];
                    i++;
                    if (i < 8)
                    {
                        p_timer.Start();
                    }
                })), null);
            };
            p_timer.Start();
        }*/
    }
}
