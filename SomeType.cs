using System;
using System.Windows;
using System.Windows.Controls;

namespace PTLStation
{
    /// <summary>
    /// Just some class, helping animate a scroll viewer
    /// </summary>
    internal class SomeType : ContentControl
    {
        double scrollOffset;
        ScrollViewer scrl;

        public static readonly DependencyProperty ScrollOffsetProperty =
            DependencyProperty.Register("ScrollOffset", typeof(double), typeof(SomeType),
            new FrameworkPropertyMetadata(0.0, new PropertyChangedCallback(OnScrollOffsetChanged)));

        public SomeType(ScrollViewer tomove)
        {
            scrl = tomove;
        }

        public double ScrollOffset
        {
            get { return (double)GetValue(ScrollOffsetProperty); }
            set { SetValue(ScrollOffsetProperty, value); }
        }

        private void SetValue(DependencyProperty scrollOffsetProperty, double value)
        {
            scrollOffset = value;
        }
        
        private static void OnScrollOffsetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SomeType myObj = obj as SomeType;

            if (myObj != null)
                myObj.scrl.ScrollToVerticalOffset(myObj.ScrollOffset);
        }
    }
}