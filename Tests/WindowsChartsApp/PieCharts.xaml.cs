using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;
using DigitalRune.Windows.Charts;


namespace SampleApplication
{
    public class SalesFigures
    {
        public string Period { get; set; }  // Used as x-value.
        public double Sales { get; set; }   // Used as y-value.
        public string Comment { get; set; } // Shown as tooltip.
    }


    public class SalesFiguresCollection : Collection<SalesFigures>
    {
    }


    /// <summary>
    /// A simple behavior that animates pie chart sectors when the mouse is over.
    /// </summary>
    public class ExplodePieChartBehavior : Behavior<PieChartItem>
    {
        private Storyboard _pushOutStoryboard;
        private Storyboard _pullInStoryboard;


        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseEnter += OnMouseEnter;
            AssociatedObject.MouseLeave += OnMouseLeave;

            // Animate the "PieChartItem.Offset" property when the mouse is over item.
            var animation = new DoubleAnimation { To = 20, Duration = new Duration(TimeSpan.FromMilliseconds(150)) };
            Storyboard.SetTarget(animation, AssociatedObject);
            Storyboard.SetTargetProperty(animation, new PropertyPath(PieChartItem.OffsetProperty));
            _pushOutStoryboard = new Storyboard();
            _pushOutStoryboard.Children.Add(animation);

            animation = new DoubleAnimation { To = 0, Duration = new Duration(TimeSpan.FromMilliseconds(150)) };
            Storyboard.SetTarget(animation, AssociatedObject);
            Storyboard.SetTargetProperty(animation, new PropertyPath(PieChartItem.OffsetProperty));
            _pullInStoryboard = new Storyboard();
            _pullInStoryboard.Children.Add(animation);
        }


        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.MouseEnter -= OnMouseEnter;
            AssociatedObject.MouseLeave -= OnMouseLeave;

            _pushOutStoryboard.Stop();
            _pullInStoryboard.Stop();
            _pushOutStoryboard = null;
            _pullInStoryboard = null;
        }


        private void OnMouseEnter(object sender, MouseEventArgs eventArgs)
        {
            _pushOutStoryboard.Begin();
        }


        private void OnMouseLeave(object sender, MouseEventArgs eventArgs)
        {
            _pullInStoryboard.Begin();
        }
    }


    public partial class PieCharts
    {
        public PieCharts()
        {
            InitializeComponent();
        }
    }
}
