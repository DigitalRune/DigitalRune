// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a scatter plot.
    /// </summary>
    /// <remarks>
    /// <para>A scatter plot presents data as non-connected points.</para>
    /// <para>
    /// A marker is drawn at the position of each data point. The properties
    /// <see cref="Chart.DataPointStyle"/> and <see cref="Chart.DataPointTemplate"/> defines the
    /// visual appearance of a marker. The <see cref="FrameworkElement.DataContext"/> of the marker
    /// will be set to the corresponding item in the <see cref="Chart.DataSource"/>. The property
    /// <see cref="FrameworkElement.Tag"/> will be set to a <see cref="Point"/> containing the x and
    /// y value where the marker is placed.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ScatterPlot")]
    public class ScatterPlot : Chart
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="ScatterPlot"/> class.
        /// </summary>
        public ScatterPlot()
        {
            DefaultStyleKey = typeof(ScatterPlot);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="Chart"/> class.
        /// </summary>
        static ScatterPlot()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScatterPlot), new FrameworkPropertyMetadata(typeof(ScatterPlot)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Raises the <see cref="ChartElement.Updated"/> event.
        /// </summary>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnUpdate"/> in a
        /// derived class, be sure to call the base class's <see cref="OnUpdate"/> method so that
        /// the base class <see cref="Chart"/> can update the data source if required.
        /// </remarks>
        protected override void OnUpdate()
        {
            base.OnUpdate();

            Debug.Assert(Canvas.Children.Count == 0, "Canvas should be cleared in base class.");

            if (Data == null || Data.Count == 0 || DataPointTemplate == null)
            {
                // A relevant property is not set.
                return;
            }

            var xAxis = XAxis;
            var yAxis = YAxis;
            var xScale = xAxis.Scale;
            var yScale = yAxis.Scale;

            // Draw only data which is in the visible data range.
            double leftCutoff = xScale.Min;
            double rightCutoff = xScale.Max;
            double bottomCutoff = yScale.Min;
            double topCutoff = yScale.Max;

            // Loop over data pairs and create markers.
            for (int i = 0; i < Data.Count; ++i)
            {
                DataPoint data = Data[i];

                if (!Numeric.IsFinite(data.X)
                    || !Numeric.IsFinite(data.Y)
                    || data.X < leftCutoff
                    || rightCutoff < data.X
                    || data.Y < bottomCutoff
                    || topCutoff < data.Y)
                {
                    // Data values are outside chart area.
                    continue;
                }

                // Create marker
                FrameworkElement marker = CreateDataPoint(data.DataContext ?? data);
                if (marker == null)
                    return;

                Canvas.Children.Add(marker);

                // Save position in tag for PositionMarker().
                // Alternatively, we could set the property as an attached property.
                double x = xAxis.GetPosition(data.X);
                double y = yAxis.GetPosition(data.Y);
                marker.Tag = new Point(x, y);

#if SILVERLIGHT
                // Silverlight: Position marker immediately, because some elements do not raise a 
                // SizeChanged event.
                PositionMarker(marker);
#endif

                // Position the marker as soon as it is measured.
                marker.SizeChanged += MarkerSizeChanged;
            }
        }


        private static void MarkerSizeChanged(object sender, SizeChangedEventArgs eventArgs)
        {
            PositionMarker((FrameworkElement)sender);
        }


        private static void PositionMarker(FrameworkElement marker)
        {
            if (!(marker.Tag is Point))
                return;

            Point position = (Point)marker.Tag;

            double x = position.X;
            double y = position.Y;

            double width = marker.DesiredSize.Width;
            double height = marker.DesiredSize.Height;

            if (width == 0.0 && height == 0.0)
            {
                // Fix for Silverlight.
                width = marker.ActualWidth;
                height = marker.ActualHeight;
            }

            // Position child horizontally.
            switch (marker.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    // Default: do nothing.
                    break;
                case HorizontalAlignment.Center:
                case HorizontalAlignment.Stretch:
                    x = x - width / 2.0;
                    break;
                case HorizontalAlignment.Right:
                    x = x - width;
                    break;
            }

            // Position child vertically.
            switch (marker.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    // Default: do nothing.
                    break;
                case VerticalAlignment.Center:
                case VerticalAlignment.Stretch:
                    y = y - height / 2.0;
                    break;
                case VerticalAlignment.Bottom:
                    y = y - height;
                    break;
            }

            Canvas.SetLeft(marker, x);
            Canvas.SetTop(marker, y);
        }


        /// <inheritdoc/>
        protected override UIElement OnGetLegendSymbol()
        {
            if (DataPointTemplate != null)
            {
                var grid = new Grid
                {
                    MinWidth = 16,
                    MinHeight = 16,
                };

                var legendSymbol = CreateDataPoint(null);
                if (legendSymbol != null)
                {
                    legendSymbol.HorizontalAlignment = HorizontalAlignment.Center;
                    legendSymbol.VerticalAlignment = VerticalAlignment.Center;
                    legendSymbol.IsHitTestVisible = false;
                    grid.Children.Add(legendSymbol);
                }

                return grid;
            }

            return base.OnGetLegendSymbol();
        }


        /// <inheritdoc/>
        public override void ValidateData()
        {
        }
        #endregion
    }
}
