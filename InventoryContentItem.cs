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
    enum Status
    {
        series_sent = 0,
        series_confirmed = 1,
        confirmed = 2,
        idle = 3
    }
    class InventoryContentItem : Grid
    {
        private double width_of;
        private double height_of;
        //public event EventHandler CloseKAS;
        public event EventHandler ShowButtonPressed;
        public event EventHandler ShowButtonPressed1;
        //public event EventHandler ExpandMe;

        Border minus, plus;
        //StackPanel sp;
        SolidColorBrush mybrush;
        TextBox quantityText;
        ComboBox additional_series;
        //bool expanded;
        int id, quant_coll;
        //private bool _confirmed;
        string add_ser;
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
                    quantityText.IsEnabled = false;

                    minus.IsEnabled = false;
                    plus.IsEnabled = false;

                    Background = Brushes.LightGray;
                }
            }
        }

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
        public string Additional_series
        {
            set
            {
                if (additional_series.SelectedIndex > -1)
                {
                    add_ser = additional_series.SelectedValue.ToString();
                }
                else
                {
                    add_ser = "";
                }
            }
            get
            {
                if (additional_series.SelectedIndex > -1)
                {
                    add_ser = additional_series.SelectedValue.ToString();
                }
                else
                {
                    add_ser = "";
                }

                return add_ser;
            }
        }

        public int RefID { get; set; }
        public int PTL_global { get; set; }
        public string Series { get; set; }
        public List<string> Med_series { get; set; }

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
        public InventoryContentItem(int id, int ref_id, string[] barcodes, string name, string address, string series, int global_ptl, List<string> med_series, int mode)
        {
            if (mode == 0)
            {
                //MessageBox.Show("0");
            }
            else
            {
                //MessageBox.Show("1");
            }
            st = Status.idle;
            PTL_global = global_ptl;
            Med_series = med_series;
            Series = series;
            RefID = ref_id;
            //_confirmed = false;
            this.id = id;
            this.Loaded += (s, ev) =>
            {
                mybrush = Background as SolidColorBrush;
            };
            width_of = SystemParameters.PrimaryScreenWidth * 0.6;
            height_of = width_of * 0.75;

            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(4, GridUnitType.Star) }); // Number
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) }); // Barcode
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(20, GridUnitType.Star) }); // Name
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10, GridUnitType.Star) }); // Address
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8, GridUnitType.Star) }); // Series
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(9, GridUnitType.Star) }); // Quantity collected
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) }); // +
            ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) }); // OK
            //RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Main row
            //RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto }); // Expandable row with confirm button

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
            Grid.SetColumnSpan(frame, 9);
            /*Grid.SetRow(frame, 0);
            Grid.SetRowSpan(frame, 2);*/
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
            //SetRow(id_block, 0);
            Children.Add(id_block);

            // shows item barcode
            TextBlock barcode_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 5, 2),
                FontSize = 18,
                Text = barcodes[0]
            };
            SetColumn(barcode_block, 1);
            //SetRow(barcode_block, 0);
            Children.Add(barcode_block);

            // shows item name
            TextBlock name_block = new TextBlock()
            {
                Margin = new Thickness(2, 2, 5, 2),
                FontSize = 18,
                Text = name
            };

            SetColumn(name_block, 2);
            //SetRow(name_block, 0);
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
            //SetRow(address_block, 0);
            Children.Add(address_block);

            //show item series
            if (mode == 1)
            {
                TextBlock exp_block = new TextBlock()
                {
                    Margin = new Thickness(2, 2, 4, 2),
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = series
                };
                SetColumn(exp_block, 4);
                //SetRow(exp_block, 0);
                Children.Add(exp_block);
            }
            if (mode == 0)
            {
                //Additional series
                additional_series = new ComboBox();
                for (int i = 0; i < Med_series.Count; i++)
                {
                    if (Series != Med_series[i])
                    {
                        additional_series.Items.Add(Med_series[i]);          //!!!
                    }

                }
                SetColumn(additional_series, 4);
                //SetRow(exp_block, 0);
                Children.Add(additional_series);
            }


            {
                // a grid containing 3 columns for +/- buttons and the actual quanity collected
                Grid quan = new Grid();
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                quan.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                SetColumn(quan, 5);
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
                quantityText = new TextBox()
                {
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(3)
                };
                quantityText.TextChanged += (se, ev) =>
                {
                    string s = "";
                    for (int i = 0; i < (se as TextBox).Text.Length; i++)
                    {
                        if (char.IsDigit((se as TextBox).Text[i]))
                        {
                            s += (se as TextBox).Text[i];
                        }
                    }
                    if (s.Length > 0)
                        quant_coll = Convert.ToInt32(s);
                    if (s != (se as TextBox).Text)
                        (se as TextBox).Text = s;

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
                    QuantityCollected += 1;
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
                txt.Text = "+";

                Button btn1 = new Button()
                {
                    Content = txt,
                    Margin = new Thickness(5, 3, 5, 2)
                };
                btn1.Click += (se, ev) =>
                {
                    ShowButtonPressed1?.Invoke(this, new EventArgs());
                };
                SetColumn(btn1, 6);
                Children.Add(btn1);
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
                SetColumn(btn, 7);
                Children.Add(btn);
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
