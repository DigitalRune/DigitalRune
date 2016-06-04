// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Shows a list that describes the sectors of a pie chart.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="PieChartLegend"/> is associated with a <see cref="PieChart"/> by setting the
    /// property <see cref="Source"/>. The element assigned to <see cref="Source"/> needs to be a
    /// <see cref="PieChart"/> or a <see cref="DependencyObject"/> that contains a 
    /// <see cref="PieChart"/> within its visual tree.
    /// </para>
    /// <para>
    /// The <see cref="PieChartLegend"/> will create and show a <see cref="LegendItem"/> for each
    /// sector of the pie chart. The legend items consists of two part: 
    /// <list type="number">
    /// <item>
    /// A <see cref="LegendItem.Symbol"/> which represents the sector of the pie chart. The symbol
    /// is rendered automatically and updated when the pie chart changes.
    /// </item>
    /// <item>
    /// A <see cref="LegendItem.Label"/> which shows a description of the data item. By default, the
    /// label is just the <see cref="object.ToString"/>-representation of the data item. This can be
    /// changed by changing the <see cref="LegendItem.LabelTemplate"/> of the
    /// <see cref="LegendItem"/>s.
    /// </item>
    /// </list>
    /// The data context of a <see cref="LegendItem"/>s is the data item of the pie chart's
    /// <see cref="Chart.DataSource"/>. The property <see cref="LegendItemStyle"/> can set to
    /// customize the <see cref="LegendItem"/>s.
    /// </para>
    /// </remarks>
    [StyleTypedProperty(Property = "LegendItemStyle", StyleTargetType = typeof(LegendItem))]
    [TemplatePart(Name = "PART_ItemsPanel", Type = typeof(Panel))]
    public class PieChartLegend : Control
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        /// <summary>
        /// The default value of the <see cref="Title"/> property.
        /// </summary>
        private const string DefaultTitle = "Pie Chart Legend";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private IDisposable _pieChartSubscription;
        private PieChart _pieChart;
        private Panel _itemsPanel;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="LegendItemStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendItemStyleProperty = DependencyProperty.Register(
            "LegendItemStyle",
            typeof(Style),
            typeof(PieChartLegend),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style used for the legend items.
        /// This is a dependency property.
        /// </summary>
        /// <value>The style used for the legend items.</value>
        [Description("Gets or sets the style used for the legend items.")]
        [Category(Categories.Default)]
        public Style LegendItemStyle
        {
            get { return (Style)GetValue(LegendItemStyleProperty); }
            set { SetValue(LegendItemStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(DependencyObject),
            typeof(PieChartLegend),
            new PropertyMetadata(null, OnPieChartChanged));

        /// <summary>
        /// Gets or sets the source of the legend.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The source of the legend. Either a <see cref="PieChart"/> or an element that has a
        /// <see cref="PieChart"/> in its visual tree.
        /// </value>
        [Description("Gets or sets the pie chart.")]
        [Category(ChartCategories.Default)]
        public DependencyObject Source
        {
            get { return (DependencyObject)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(object),
            typeof(PieChartLegend),
            new PropertyMetadata(DefaultTitle));

        /// <summary>
        /// Gets or sets the title of the legend.
        /// This is a dependency property.
        /// </summary>
        /// <value>The title of the legend. The default value is "Pie Chart Legend".</value>
        [Description("Gets or sets the title of the legend.")]
        [Category(ChartCategories.Default)]
        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="TitleStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleStyleProperty = DependencyProperty.Register(
            "TitleStyle",
            typeof(Style),
            typeof(PieChartLegend),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the legend title.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style of the legend title.")]
        [Category(ChartCategories.Default)]
        public Style TitleStyle
        {
            get { return (Style)GetValue(TitleStyleProperty); }
            set { SetValue(TitleStyleProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="PieChartLegend"/> class.
        /// </summary>
        public PieChartLegend()
        {
            DefaultStyleKey = typeof(PieChartLegend);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="PieChartLegend"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static PieChartLegend()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PieChartLegend), new FrameworkPropertyMetadata(typeof(PieChartLegend)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the <see cref="Source"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnPieChartChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var pieChartLegend = (PieChartLegend)dependencyObject;
            var source = (DependencyObject)eventArgs.NewValue;
            pieChartLegend.OnPieChartChanged(source);
        }


        private void OnPieChartChanged(DependencyObject source)
        {
            // Unsubscribe from previous collection.
            if (_pieChartSubscription != null)
            {
                _pieChartSubscription.Dispose();
                _pieChartSubscription = null;
            }

            // Get pie chart from source.
            if (source != null)
                _pieChart = source.GetVisualSubtree().OfType<PieChart>().FirstOrDefault();

            if (_pieChart != null)
            {
                // Bind legend title to pie chart title.
                SetBinding(TitleProperty, new Binding("Title") { Source = _pieChart });

                // Subscribe to changes in pie chart using weak event pattern.
                _pieChartSubscription =
                    WeakEventHandler<EventArgs>.Register(
                        _pieChart,
                        this,
                        (sender, handler) => sender.Updated += handler,
                        (sender, handler) => sender.Updated -= handler,
                        (listener, sender, eventArgs) => listener.Update());
            }
            else
            {
                // Reset legend title.
                ClearValue(TitleProperty);
            }

            Update();
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal 
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_itemsPanel != null)
            {
                _itemsPanel.Children.Clear();
                _itemsPanel = null;
            }

            base.OnApplyTemplate();

            _itemsPanel = GetTemplateChild("PART_ItemsPanel") as Panel;
            Update();
        }


        /// <summary>
        /// Updates the pie chart legend.
        /// </summary>
        private void Update()
        {
            if (_itemsPanel == null)
                return;

            _itemsPanel.Children.Clear();

            if (_pieChart != null)
            {
                foreach (var symbol in _pieChart.GetPieChartLegendSymbols())
                {
                    var legendItem = new LegendItem();
                    legendItem.DataContext = symbol.DataContext;
                    legendItem.Symbol = symbol;
                    legendItem.Label = symbol.DataContext;
                    legendItem.SetBinding(StyleProperty, new Binding("LegendItemStyle") { Source = this });

                    _itemsPanel.Children.Add(legendItem);
                }
            }
        }
        #endregion
    }
}
