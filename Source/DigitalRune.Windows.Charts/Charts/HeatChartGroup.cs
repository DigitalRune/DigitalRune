// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Shapes;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a group of <see cref="HeatChart"/>s.
    /// </summary>
    /// <remarks>
    /// The <see cref="HeatChart"/>s are distributed on the y-axis with a gap between them.
    /// </remarks>
    [StyleTypedProperty(Property = "BarStyle", StyleTargetType = typeof(Rectangle))]
    public class HeatChartGroup : ChartGroup<HeatChart>
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

        /// <summary>
        /// Identifies the <see cref="BarGap"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BarGapProperty = DependencyProperty.Register(
            "BarGap",
            typeof(double),
            typeof(HeatChartGroup),
            new PropertyMetadata(0.5, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the width of the gap between two bars relative to the bar width.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The width of the gap between to bars relative to the bar width. The default value is
        /// 0.5.
        /// </value>
        /// <remarks>
        /// Examples: 0 means no gap. 1 means the gap width is equal to the bar width. 2 means the
        /// gap width is twice the bar width.
        /// </remarks>
        [Description("Gets or sets the width of the gap between two bars in relation to the bar width.")]
        [Category(ChartCategories.Default)]
        public double BarGap
        {
            get { return (double)GetValue(BarGapProperty); }
            set { SetValue(BarGapProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="FillPalette"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FillPaletteProperty =
#if SILVERLIGHT
            DependencyProperty.Register(
                "FillPalette",
                typeof(Palette),
                typeof(HeatChartGroup),
                new PropertyMetadata(null, OnRelevantPropertyChanged));
#else
            HeatChart.FillPaletteProperty.AddOwner(
                typeof(HeatChartGroup),
                new PropertyMetadata(null, OnRelevantPropertyChanged));
#endif

        /// <summary>
        /// Gets or sets the palette for used to fill the heat chart.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="Palette"/> that is used to fill the heat chart. The default value is
        /// <see langword="null"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the palette for used to fill the heat chart.")]
        [Category(ChartCategories.Default)]
        public Palette FillPalette
        {
            get { return (Palette)GetValue(FillPaletteProperty); }
            set { SetValue(FillPaletteProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StrokePalette"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokePaletteProperty =
#if SILVERLIGHT
            DependencyProperty.Register(
                "StrokePalette",
                typeof(Palette),
                typeof(HeatChartGroup),
                new PropertyMetadata(null, OnRelevantPropertyChanged));
#else
            HeatChart.StrokePaletteProperty.AddOwner(
                typeof(HeatChartGroup),
                new PropertyMetadata(null, OnRelevantPropertyChanged));
#endif

        /// <summary>
        /// Gets or sets the palette used to fill the outline of the heat chart. This is a
        /// dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="Palette"/> that is used to fill the outline of sections of the heat chart.
        /// The default value is <see langword="null"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the palette used to fill the outline of the heat chart.")]
        [Category(ChartCategories.Default)]
        public Palette StrokePalette
        {
            get { return (Palette)GetValue(StrokePaletteProperty); }
            set { SetValue(StrokePaletteProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="HeatChartGroup"/> class.
        /// </summary>
        public HeatChartGroup()
        {
            DefaultStyleKey = typeof(HeatChartGroup);
        }
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static HeatChartGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeatChartGroup), new FrameworkPropertyMetadata(typeof(HeatChartGroup)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when a relevant property is changed and the charts needs to be updated.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnRelevantPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var chartGroup = (ChartGroup)dependencyObject;
            chartGroup.Invalidate();
        }


        /// <inheritdoc/>
        protected override AxisScale OnSuggestYScale()
        {
            var scale = new TextScale(-0.5, Items.Count - 0.5);

            int index = 0;
            foreach (var chart in Charts)
                scale.Labels.Add(new TextLabel(index++, chart.Title));

            return scale;
        }


        /// <inheritdoc/>
        public override void ValidateData()
        {
        }
        #endregion
    }
}
