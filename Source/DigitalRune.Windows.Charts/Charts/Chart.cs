// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a chart in a chart panel. (Base class)
    /// </summary>
    [ContentProperty("DataSource")]
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    public abstract class Chart : ChartElement
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _isDataSourceValid;
        private IDisposable _dataSourceSubscription;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the drawing <see cref="System.Windows.Controls.Canvas"/>.
        /// </summary>
        /// <value>The <see cref="System.Windows.Controls.Canvas"/>.</value>
        /// <remarks>
        /// <para>
        /// The canvas needs to be defined in the control template and called "PART_Canvas". If this
        /// part is missing, a dummy canvas will be created which can be used for drawing. The dummy
        /// canvas is created, so that derived classes don't have to check whether
        /// <see cref="Canvas"/> is <see langword="null"/>. However, if "PART_Canvas" is missing in
        /// the control template, nothing will be rendered.
        /// </para>
        /// <para>
        /// The canvas is automatically cleared in <see cref="OnUpdate"/>. Derived classes need to
        /// add all required elements to the canvas every update.
        /// </para>
        /// </remarks>
        protected Canvas Canvas { get; private set; }


        /// <summary>
        /// Gets the data represented as a list of data points.
        /// </summary>
        /// <value>
        /// The data represented as a list of data points. Can be <see langword="null"/> - see
        /// remarks.
        /// </value>
        /// <remarks>
        /// The method <see cref="UpdateDataSource"/> converts <see cref="DataSource"/> to a list of
        /// data points. The property returns <see langword="null"/> if the data source is invalid
        /// or <see cref="UpdateDataSource"/> has not been called yet.
        /// <see cref="UpdateDataSource"/> is called automatically during
        /// <see cref="ChartElement.Update"/> or when the chart is rendered.
        /// </remarks>
        public IList<DataPoint> Data { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="DataPointStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataPointStyleProperty = DependencyProperty.Register(
            "DataPointStyle",
            typeof(Style),
            typeof(Chart),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the style that is assigned to the data points.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="Style"/> that is assignable to the <see cref="DataPointTemplate"/>. This
        /// style is optional. The default value is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets style that is assigned to the data points.")]
        [Category(ChartCategories.Styles)]
        public Style DataPointStyle
        {
            get { return (Style)GetValue(DataPointStyleProperty); }
            set { SetValue(DataPointStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="DataPointTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataPointTemplateProperty = DependencyProperty.Register(
            "DataPointTemplate",
            typeof(DataTemplate),
            typeof(Chart),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the data template used to represent the data points.
        /// This is a dependency property.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="FrameworkElement.DataContext"/> of the instantiated template will be set
        /// to the its corresponding item in <see cref="Chart.DataSource"/>.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the data template used to represent the data points.")]
        [Category(ChartCategories.Default)]
        public DataTemplate DataPointTemplate
        {
            get { return (DataTemplate)GetValue(DataPointTemplateProperty); }
            set { SetValue(DataPointTemplateProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Group"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(
            "Group",
            typeof(ChartGroup),
            typeof(Chart),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the chart group.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The <see cref="ChartGroup"/> or <see langword="null"/> if this chart is not part of a
        /// group. The default value is <see langword="null"/>. The property is set automatically by
        /// the <see cref="ChartGroup"/> when the <see cref="Chart"/> is added the group.
        /// </value>
        [Description("Gets or sets the chart group.")]
        [Category(ChartCategories.Default)]
        public ChartGroup Group
        {
            get { return (ChartGroup)GetValue(GroupProperty); }
            set { SetValue(GroupProperty, value); }
        }


        #region ----- Data Source -----

        /// <summary>
        /// Identifies the <see cref="DataSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
            "DataSource",
            typeof(IEnumerable),
            typeof(Chart),
            new PropertyMetadata(null, OnDataSourceChanged));

        /// <summary>
        /// Gets or sets the data source containing the data points.
        /// This is a dependency property.
        /// </summary>
        /// <value>A collection that contains data points.</value>
        [Description("Gets or sets the data source containing the data points.")]
        [Category(ChartCategories.Default)]
        public IEnumerable DataSource
        {
            get { return (IEnumerable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="XValuePath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XValuePathProperty = DependencyProperty.Register(
            "XValuePath",
            typeof(PropertyPath),
            typeof(Chart),
            new PropertyMetadata(null, OnValuePathChanged));

        /// <summary>
        /// Gets or sets the binding path for the x values.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The binding path for the x values. The default value is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the binding path for the x values.")]
        [Category(ChartCategories.Default)]
        [TypeConverter(typeof(PropertyPathConverter))]
        public PropertyPath XValuePath
        {
            get { return (PropertyPath)GetValue(XValuePathProperty); }
            set { SetValue(XValuePathProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="YValuePath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YValuePathProperty = DependencyProperty.Register(
            "YValuePath",
            typeof(PropertyPath),
            typeof(Chart),
            new PropertyMetadata(null, OnValuePathChanged));

        /// <summary>
        /// Gets or sets the binding path for the y values.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The binding path for the y values. The default value is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the binding path for the y values.")]
        [Category(ChartCategories.Default)]
        [TypeConverter(typeof(PropertyPathConverter))]
        public PropertyPath YValuePath
        {
            get { return (PropertyPath)GetValue(YValuePathProperty); }
            set { SetValue(YValuePathProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="XYValuePath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XYValuePathProperty = DependencyProperty.Register(
            "XYValuePath",
            typeof(PropertyPath),
            typeof(Chart),
            new PropertyMetadata(null, OnValuePathChanged));

        /// <summary>
        /// Gets or sets the binding path for the (x, y) values.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The binding path for the (x, y) values. The default value is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets the binding path for the (x, y) values.")]
        [Category(ChartCategories.Default)]
        [TypeConverter(typeof(PropertyPathConverter))]
        public PropertyPath XYValuePath
        {
            get { return (PropertyPath)GetValue(XYValuePathProperty); }
            set { SetValue(XYValuePathProperty, value); }
        }
        #endregion

        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="Chart"/> class.
        /// </summary>
        protected Chart()
        {
            DefaultStyleKey = typeof(Chart);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="Chart"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static Chart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Chart), new FrameworkPropertyMetadata(typeof(Chart)));
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
            var chart = (Chart)dependencyObject;
            chart.Invalidate();
        }


        /// <summary>
        /// Called when a property path (<see cref="XValuePath"/>, <see cref="YValuePath"/>, or
        /// <see cref="XYValuePath"/>) changes.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnValuePathChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var chart = (Chart)dependencyObject;
            chart.InvalidateDataSource();
        }


        /// <summary>
        /// Called when the <see cref="DataSource"/> changes.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnDataSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var chart = (Chart)dependencyObject;
            var newValue = (IEnumerable)eventArgs.NewValue;
            chart.OnDataSourceChanged(newValue);
        }


        private void OnDataSourceChanged(IEnumerable dataSource)
        {
            // Unsubscribe from previous collection.
            if (_dataSourceSubscription != null)
            {
                _dataSourceSubscription.Dispose();
                _dataSourceSubscription = null;
            }

            // Subscribe to new collection using weak event pattern.
            var observableDataSource = dataSource as INotifyCollectionChanged;
            if (observableDataSource != null)
            {
                _dataSourceSubscription =
                    WeakEventHandler<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>.Register(
                        observableDataSource,
                        this,
                        handler => new NotifyCollectionChangedEventHandler(handler),
                        (sender, handler) => sender.CollectionChanged += handler,
                        (sender, handler) => sender.CollectionChanged -= handler,
                        (listener, sender, eventArgs) => listener.InvalidateDataSource());
            }

            InvalidateDataSource();
        }


        /// <summary>
        /// Called when the property <see cref="ChartElement.XAxis"/> changes.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnXAxisChanged(DependencyPropertyChangedEventArgs eventArgs)
        {
            if (Group != null)
                return;

            base.OnXAxisChanged(eventArgs);
        }


        /// <summary>
        /// Called when the property <see cref="ChartElement.YAxis"/> changes.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnYAxisChanged(DependencyPropertyChangedEventArgs eventArgs)
        {
            if (Group != null)
                return;

            base.OnYAxisChanged(eventArgs);
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (Canvas != null)
            {
                // Clean up.
                Canvas.Children.Clear();
                Canvas = null;
            }

            base.OnApplyTemplate();
            Canvas = GetTemplateChild("PART_Canvas") as Canvas ?? new Canvas();
            Invalidate();
        }


        /// <summary>
        /// Invalidates the data source. (Needs to be called manually, when the
        /// data source does not implement <see cref="INotifyCollectionChanged"/>.)
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method needs to be called when the data changes and the data source is not a list
        /// of <see cref="DataPoint"/>s or <see cref="Point"/>s that implements
        /// <see cref="INotifyCollectionChanged"/>.
        /// </para>
        /// <para>
        /// This method also invalidates the chart by calling <see cref="ChartElement.Invalidate"/>.
        /// </para>
        /// </remarks>
        public void InvalidateDataSource()
        {
            _isDataSourceValid = false;
            Invalidate();
        }


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
            // Cleanup
            Canvas.Children.Clear();

            UpdateDataSource();

#if !SILVERLIGHT
            // Clear BitmapCache to ensure that everything is redrawn.
            var cacheMode = CacheMode;
            if (cacheMode != null)
            {
                CacheMode = null;
                CacheMode = cacheMode;
            }
#endif

            base.OnUpdate();
        }


        /// <summary>
        /// Updates the chart when the data has been changed. (Optional - The data source is 
        /// updated automatically when the chart is drawn.)
        /// </summary>
        public void UpdateDataSource()
        {
            if (_isDataSourceValid)
                return;

            Data = null;

            Axis xAxis = XAxis;
            Axis yAxis = YAxis;
            if (xAxis == null || yAxis == null)
            {
                // Postpone update until axes are set.
                return;
            }

            CultureInfo culture = ChartHelper.GetCulture(this);
            IList<TextLabel> xLabels = GetLabels(xAxis);
            IList<TextLabel> yLabels = GetLabels(yAxis);

            Data = ChartDataHelper.CreateChartDataSource(DataSource, XValuePath, YValuePath, XYValuePath, culture, xLabels, yLabels);

#if DEBUG
            ValidateData();
#endif

            _isDataSourceValid = true;

            if (xAxis.AutoScale)
                xAxis.Invalidate();

            if (yAxis.AutoScale)
                yAxis.Invalidate();
        }


        private static IList<TextLabel> GetLabels(Axis axis)
        {
            AxisScale scale = axis.Scale;
            IList<TextLabel> labels = null;
            TextScale textScale = scale as TextScale;
            if (textScale != null)
                labels = (textScale).Labels;

            return labels;
        }


        /// <summary>
        /// Instantiates a new data point from the <see cref="DataPointTemplate"/>.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <returns>
        /// The <see cref="FrameworkElement"/> created from <see cref="DataPointTemplate"/>.
        /// <see langword="null"/> if the template could not be loaded.
        /// </returns>
        /// <exception cref="ChartException">
        /// Cannot create data point. <see cref="DataPointTemplate"/> is not set.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DataPointTemplate")]
        protected FrameworkElement CreateDataPoint(object dataContext)
        {
            if (DataPointTemplate == null)
                throw new ChartException("Cannot create data point. DataPointTemplate is not set.");

            var marker = DataPointTemplate.LoadContent() as FrameworkElement;
            if (marker != null)
            {
                if (dataContext != null)
                    marker.DataContext = dataContext;
                ChartPanel.SetIsDataPoint(marker, true);
                marker.SetBinding(StyleProperty, new Binding("DataPointStyle") { Source = this });
            }

            return marker;
        }


        /// <inheritdoc/>
        protected override AxisScale OnSuggestXScale()
        {
            return SuggestScale(Data, true);
        }


        /// <inheritdoc/>
        protected override AxisScale OnSuggestYScale()
        {
            return SuggestScale(Data, false);
        }


        /// <summary>
        /// Suggests a default scale for a data source.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="suggestXScale">
        /// If set to <see langword="true"/> the x values of the data source are used; otherwise the
        /// y values.
        /// </param>
        /// <returns>
        /// A scale for the data source or <see langword="null"/> if the data source is invalid.
        /// </returns>
        /// <remarks>A linear scale the encloses all data values is returned.</remarks>
        private static AxisScale SuggestScale(IList<DataPoint> dataSource, bool suggestXScale)
        {
            if (dataSource == null || dataSource.Count == 0)
                return null;

            DataPoint data = dataSource[0];
            double min = suggestXScale ? data.X : data.Y;
            double max = suggestXScale ? data.X : data.Y;

            for (int i = 1; i < dataSource.Count; i++)
            {
                data = dataSource[i];
                double value = suggestXScale ? data.X : data.Y;

                if (Numeric.IsNaN(value))
                    continue;

                if (Numeric.IsNaN(min))
                    min = value;
                if (Numeric.IsNaN(max))
                    max = value;

                if (value < min)
                    min = value;
                else if (value > max)
                    max = value;
            }

            if (Numeric.IsNaN(min) || Numeric.IsNaN(max))
                return null;

            Debug.Assert(min <= max, "Minimum of scale must be less than or equal to maximum.");

            if (Numeric.AreEqual(min, max))
            {
                // Range is numerically zero.
                if (Numeric.IsZero(min))
                {
                    // min and max are close to 0. 
                    // Choose an appropriate interval with a range of 1.
                    if (min >= 0)
                    {
                        // Choose interval [0, 1].
                        Debug.Assert(0 <= min && min <= max);
                        return new LinearScale(0, 1);
                    }

                    if (max <= 0)
                    {
                        // Choose interval [-1, 0].
                        Debug.Assert(min <= max && max <= 0);
                        return new LinearScale(-1, 0);
                    }

                    // Choose interval [-0.5, 0.5].
                    Debug.Assert(min <= 0 && 0 <= max);
                    return new LinearScale(-0.5, 0.5);
                }

                // Expand interval.
                double offset = 0.5 * Math.Min(Math.Abs(min), Math.Abs(max));
                min = min - offset;
                max = max + offset;

                // Return a default scale with a range > 0.
                return new LinearScale(min, max);
            }

            return new LinearScale(min, max);
        }


        /// <summary>
        /// Validates the data and throws an exception if the data is invalid for this chart type
        /// and the current chart settings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is automatically called in Debug builds. You can also call this method
        /// manually if you want to check data in Release builds.
        /// </para>
        /// <para>
        /// This method should be called after the data source and the chart settings have been set.
        /// The method checks if the data values in the data source are valid for the current
        /// settings. If they are not valid, an exception is thrown.
        /// </para>
        /// </remarks>
        /// <exception cref="ChartDataException">
        /// Invalid chart data.
        /// </exception>
        public abstract void ValidateData();
        #endregion
    }
}
