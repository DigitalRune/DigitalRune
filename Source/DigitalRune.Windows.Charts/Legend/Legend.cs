// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Shows a list that describes the chart elements in a chart panel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="Legend"/> is associated with a <see cref="ChartPanel"/> by setting the property
    /// <see cref="Source"/>. The element assigned to <see cref="Source"/> needs to be a
    /// <see cref="ChartPanel"/> or a <see cref="DependencyObject"/> that contains a
    /// <see cref="ChartPanel"/> within its visual tree.
    /// </para>
    /// <para>
    /// The <see cref="Legend"/> will create and show a <see cref="LegendItem"/> for each
    /// <see cref="ChartElement"/> in the <see cref="Charts.ChartPanel"/>.
    /// <see cref="ChartElement"/>s where the property <see cref="ChartElement.IsVisibleInLegend"/>
    /// is <see langword="false"/> are excluded.
    /// </para>
    /// <para>
    /// A <see cref="LegendItem"/> by default shows a symbol representing the
    /// <see cref="ChartElement"/> and the title of the <see cref="ChartElement"/>. Types derived
    /// from <see cref="ChartElement"/> need to implement <see cref="ChartElement.GetLegendSymbol"/>
    /// to have a custom symbol in the <see cref="Legend"/>.
    /// </para>
    /// </remarks>
    [StyleTypedProperty(Property = "TitleStyle", StyleTargetType = typeof(ContentControl))]
    [StyleTypedProperty(Property = "LegendItemStyle", StyleTargetType = typeof(LegendItem))]
    [TemplatePart(Name = "PART_ItemsPanel", Type = typeof(Panel))]
    public class Legend : Control
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        /// <summary>
        /// The default value of the <see cref="Title"/> property.
        /// </summary>
        private const string DefaultTitle = "Legend";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly BooleanToVisibilityConverter BooleanToVisibilityConverter = new BooleanToVisibilityConverter();
        private ReadOnlyObservableCollection<ChartElement> _chartElements;
        private Panel _itemsPanel;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------    
        #endregion


        //--------------------------------------------------------------
        #region Depencency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="LegendItemStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendItemStyleProperty = DependencyProperty.Register(
            "LegendItemStyle",
            typeof(Style),
            typeof(Legend),
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
            typeof(Legend),
            new PropertyMetadata(null, OnChartPanelChanged));

        /// <summary>
        /// Gets or sets the source of the legend.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The source of the legend. Either a <see cref="ChartPanel"/> or an element that has a
        /// <see cref="ChartPanel"/> in its visual tree.
        /// </value>
        [Description("Gets or sets the source of the legend.")]
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
            typeof(Legend),
            new PropertyMetadata(DefaultTitle));

        /// <summary>
        /// Gets or sets the title of the legend.
        /// This is a dependency property.
        /// </summary>
        /// <value>The title of the legend. The default value is "Legend".</value>
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
            typeof(Legend),
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
        #region Creation and Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="Legend"/> class.
        /// </summary>
        public Legend()
        {
            DefaultStyleKey = typeof(Legend);
        }
#else
        /// <summary>
        /// Initializes the <see cref="Legend"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static Legend()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Legend), new FrameworkPropertyMetadata(typeof(Legend)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------   

        /// <summary>
        /// Called when property <see cref="Source"/> changes.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnChartPanelChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var legend = (Legend)dependencyObject;
            var source = (DependencyObject)eventArgs.NewValue;
            legend.OnChartPanelChanged(source);
        }


        private void OnChartPanelChanged(DependencyObject source)
        {
            if (_chartElements != null)
            {
                ((INotifyCollectionChanged)_chartElements).CollectionChanged -= OnChartElementsChanged;
                _chartElements = null;
            }

            if (source != null)
            {
                var chartPanel = source.GetVisualSubtree().OfType<ChartPanel>().FirstOrDefault();
                if (chartPanel != null)
                {
                    _chartElements = chartPanel.ChartElements;
                    ((INotifyCollectionChanged)_chartElements).CollectionChanged += OnChartElementsChanged;
                }
            }

            Update();
        }


        /// <summary>
        /// Called when <see cref="Charts.ChartPanel.ChartElements"/> collection of the
        /// <see cref="Source"/> changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnChartElementsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
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
        /// Updates the legends.
        /// </summary>
        private void Update()
        {
            if (_itemsPanel == null)
                return;

            _itemsPanel.Children.Clear();

            if (_chartElements == null || _chartElements.Count <= 0)
                return;

            // Populate panel with legend items.
            foreach (var chartElement in _chartElements)
            {
                var legendItem = new LegendItem();
                legendItem.Symbol = chartElement.GetLegendSymbol();
                legendItem.SetBinding(LegendItem.LabelProperty, new Binding("Title") { Source = chartElement });
                legendItem.SetBinding(StyleProperty, new Binding("LegendItemStyle") { Source = this });
                legendItem.SetBinding(VisibilityProperty, new Binding("IsVisibleInLegend") { Source = chartElement, Converter = BooleanToVisibilityConverter });

                _itemsPanel.Children.Add(legendItem);
            }
        }
        #endregion
    }
}
