// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Shapes;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Draws the background of a chart area.
    /// </summary>
    /// <remarks>
    /// The chart area is the rectangle defined by the <see cref="ChartElement.XAxis"/> and the
    /// <see cref="ChartElement.YAxis"/>. The brush used to fill the chart background can be set
    /// using the property <see cref="Control.Background"/>.
    /// </remarks>
    [TemplatePart(Name = "PART_Background", Type = typeof(FrameworkElement))]
    public class ChartBackground : ChartElement
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private FrameworkElement _background;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartBackground"/> class.
        /// </summary>
        public ChartBackground()
        {
            DefaultStyleKey = typeof(ChartBackground);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="ChartBackground"/> class.
        /// </summary>
        static ChartBackground()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChartBackground), new FrameworkPropertyMetadata(typeof(ChartBackground)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _background = null;
            base.OnApplyTemplate();
            _background = GetTemplateChild("PART_Background") as FrameworkElement;
            Invalidate();
        }


        /// <summary>
        /// Raises the <see cref="ChartElement.Updated"/> event.
        /// </summary>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnUpdate"/> in a
        /// derived class, be sure to call the base class's <see cref="OnUpdate"/> method so that
        /// registered delegates receive the event.
        /// </remarks>
        protected override void OnUpdate()
        {
            if (_background != null)
            {
                Rect bounds = ChartPanel.GetChartAreaBounds(XAxis, YAxis);
                _background.Margin = new Thickness(bounds.Left, bounds.Top, 0, 0);
                _background.Width = bounds.Width;
                _background.Height = bounds.Height;
                _background.HorizontalAlignment = HorizontalAlignment.Left;
                _background.VerticalAlignment = VerticalAlignment.Top;
                _background.Visibility = Visibility.Visible;
            }

            base.OnUpdate();
        }


        /// <inheritdoc/>
        protected override UIElement OnGetLegendSymbol()
        {
            var grid = new Grid
            {
                MinWidth = 16,
                MinHeight = 16,
            };

            var background = new Rectangle
            {
                Width = 16,
                Height = 16,
            };
            background.SetBinding(Shape.FillProperty, new Binding("Background") { Source = this });
            grid.Children.Add(background);
            return grid;
        }
        #endregion
    }
}
