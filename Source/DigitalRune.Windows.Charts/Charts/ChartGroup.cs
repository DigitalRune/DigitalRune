// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Groups multiple charts into a single chart. (Base class)
    /// </summary>
    /// <remarks>
    /// Settings in a <see cref="ChartGroup"/> will override local settings in the individual
    /// grouped charts, so that all grouped charts use the same consistent settings.
    /// </remarks>
    public abstract class ChartGroup : ItemsControl, IChartElement
    {
        // ChartGroup should be derived from ItemsControl and ChartElement, but C# does not support
        // multiple inheritance. Therefore we need to implement IChartElement and duplicate the
        // functionality ChartElement.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _invalidating;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public bool IsValid { get; private set; }


        /// <summary>
        /// Gets the charts in the chart group.
        /// </summary>
        /// <value>The charts in the chart group.</value>
        public IEnumerable<Chart> Charts
        {
            get
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var chart = ItemContainerGenerator.ContainerFromIndex(i) as Chart;

                    // Stop if item containers haven't been generated.
                    if (chart == null)
                        yield break;

                    yield return chart;
                }
            }
        }


        /// <inheritdoc/>
        public event EventHandler<EventArgs> Invalidated;


        /// <inheritdoc/>
        public event EventHandler<EventArgs> Updated;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        #region ----- Axes -----

        /// <summary>
        /// Identifies the <see cref="XAxis"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(
            "XAxis",
            typeof(Axis),
            typeof(ChartGroup),
            new PropertyMetadata(null, OnXAxisChanged));

        /// <summary>
        /// Gets or sets the x-axis.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The associated x-axis of the <see cref="ChartPanel"/>. The default value is
        /// <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// Settings these value has the same effect as setting the
        /// <strong>ChartPanel.XAxis</strong> attached dependency property.
        /// </remarks>
        [Description("Gets or sets the x-axis.")]
        [Category(ChartCategories.Default)]
        public Axis XAxis
        {
            get { return (Axis)GetValue(XAxisProperty); }
            set { SetValue(XAxisProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="YAxis"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register(
            "YAxis",
            typeof(Axis),
            typeof(ChartGroup),
            new PropertyMetadata(null, OnYAxisChanged));

        /// <summary>
        /// Gets or sets the y-axis.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The associated y-axis of the <see cref="ChartPanel"/>. The default value is
        /// <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// Settings these value has the same effect as setting the
        /// <strong>ChartPanel.YAxis</strong> attached dependency property.
        /// </remarks>
        [Description("Gets or sets the y-axis.")]
        [Category(ChartCategories.Default)]
        public Axis YAxis
        {
            get { return (Axis)GetValue(YAxisProperty); }
            set { SetValue(YAxisProperty, value); }
        }
        #endregion

        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartGroup"/> class.
        /// </summary>
        protected ChartGroup()
        {
            DefaultStyleKey = typeof(ChartGroup);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="ChartGroup"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ChartGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChartGroup), new FrameworkPropertyMetadata(typeof(ChartGroup)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnXAxisChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.OldValue == eventArgs.NewValue)
                return;

            // Synchronize the attached dependency property ChartPanel.XAxis with the dependency
            // property ChartGroup.XAxis.
            Axis xAxis = (Axis)eventArgs.NewValue;
            ChartPanel.SetXAxis(dependencyObject, xAxis);

            // Call ChartElement.OnXAxisChanged to raise the Invalidated event.
            ((ChartGroup)dependencyObject).OnXAxisChanged(eventArgs);
        }


        private static void OnYAxisChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.OldValue == eventArgs.NewValue)
                return;

            // Synchronize the attached dependency property ChartPanel.YAxis with the dependency
            // property ChartElement.YAxis.
            Axis yAxis = (Axis)eventArgs.NewValue;
            ChartPanel.SetYAxis(dependencyObject, yAxis);

            // Call ChartElement.OnYAxisChanged to raise the Invalidated event.
            ((ChartGroup)dependencyObject).OnYAxisChanged(eventArgs);
        }


        /// <summary>
        /// Called when the property <see cref="XAxis"/> changes.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnXAxisChanged(DependencyPropertyChangedEventArgs eventArgs)
        {
            OnAxisChanged(eventArgs);

            // Propagate change to grouped charts.
            var xAxis = XAxis;
            foreach (var chart in Charts)
                chart.XAxis = xAxis;
        }


        /// <summary>
        /// Called when the property <see cref="YAxis"/> changes.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected virtual void OnYAxisChanged(DependencyPropertyChangedEventArgs eventArgs)
        {
            OnAxisChanged(eventArgs);

            // Propagate change to grouped charts.
            var yAxis = YAxis;
            foreach (var chart in Charts)
                chart.YAxis = yAxis;
        }


        private void OnAxisChanged(DependencyPropertyChangedEventArgs eventArgs)
        {
            var oldAxis = (Axis)eventArgs.OldValue;
            var newAxis = (Axis)eventArgs.NewValue;

            if (oldAxis != null)
            {
                oldAxis.Updated -= OnAxisUpdated;
                oldAxis.UnregisterChartElement(this);
            }

            if (newAxis != null)
            {
                newAxis.RegisterChartElement(this);
                newAxis.Updated += OnAxisUpdated;
            }

            Invalidate();
        }


#if SILVERLIGHT
        /// <exclude/>
        public void OnAxisUpdated(object sender, EventArgs eventArgs)
        {
            Invalidate();
        }
#else
        private void OnAxisUpdated(object sender, EventArgs eventArgs)
        {
            Invalidate();
        }
#endif


        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">Element used to display the specified item.</param>
        /// <param name="item">Specified item.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var chart = (Chart)element;
            chart.Group = this;     // Important: Group needs to be set before axes are set.
            chart.XAxis = XAxis;
            chart.YAxis = YAxis;
            chart.Invalidated += OnChartInvalidated;
        }


        /// <summary>
        /// When overridden in a derived class, undoes the effects of the
        /// <see cref="ItemsControl.PrepareContainerForItemOverride(DependencyObject,object)"/>
        /// method.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The item.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            var chart = (Chart)element;
            chart.XAxis = null;
            chart.YAxis = null;
            chart.Group = null;     // Important: Group should be reset after axis are removed.
            chart.Invalidated -= OnChartInvalidated;
        }


        private void OnChartInvalidated(object sender, EventArgs eventArgs)
        {
            // Avoid re-entrance.
            if (_invalidating)
                return;

            _invalidating = true;

            try
            {
                Invalidate();
            }
            finally
            {
                _invalidating = false;
            }
        }


        /// <inheritdoc/>
        public void Invalidate()
        {
            IsValid = false;
            InvalidateMeasure();
            OnInvalidate();
        }


        /// <inheritdoc/>
        public void Update()
        {
            if (IsValid)
                return;

#if !SILVERLIGHT
            // Ensure that the children (BarCharts, LineCharts, ...) have been generated.
            bool containersGenerated = this.EnsureItemContainers();
            if (!containersGenerated)
                return;
#endif

            var xAxis = XAxis;
            var yAxis = YAxis;
            if (xAxis != null && xAxis.Scale != null
                && yAxis != null && yAxis.Scale != null
                && Numeric.IsZeroOrPositiveFinite(xAxis.Length)
                && Numeric.IsZeroOrPositiveFinite(yAxis.Length))
            {
                // Update chart element.
                OnUpdate();
                IsValid = true;
            }
        }


        /// <summary>
        /// Raises the <see cref="Invalidated"/> event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnInvalidate"/> in a
        /// derived class, be sure to call the base class's <see cref="OnInvalidate"/> method so
        /// that registered delegates receive the event.
        /// </para>
        /// </remarks>
        protected virtual void OnInvalidate()
        {
            foreach (var chart in Charts)
                chart.Invalidate();

            var handler = Invalidated;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }


        /// <summary>
        /// Raises the <see cref="Updated"/> event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnUpdate"/> in a
        /// derived class, be sure to call the base class's <see cref="OnUpdate"/> method so that
        /// registered delegates receive the event.
        /// </para>
        /// </remarks>
        protected virtual void OnUpdate()
        {
            var xAxis = XAxis;
            var yAxis = YAxis;
            var charts = Charts.ToArray();

            // Update data sources.
            foreach (var chart in charts)
            {
                // Ensure that the correct axes are used.
                if (xAxis != null && chart.XAxis != xAxis)
                    chart.XAxis = xAxis;
                if (yAxis != null && chart.YAxis != yAxis)
                    chart.YAxis = yAxis;

                chart.UpdateDataSource();
            }

#if DEBUG
            if (!WindowsHelper.IsInDesignMode && charts.All(c => c.IsValid && c.XAxis != null && c.YAxis != null))
            {
                // Validate data.
                // But only 
                //   - in DEBUG mode 
                //     (for performance reasons)
                //   - at runtime 
                //     (Do not validate the data in Visual Studio Designer or Expression Blend, because
                //     designers often create invalid intermediate states. If we would throw an exception 
                //     every time ValidateData() fails we would break the designer.)
                //   - when all charts are valid
                //     (Silverlight is often very lazy when resolving bindings. Therefore, charts can be 
                //     invalid, even if everything is set correctly in XAML.)
                ValidateData();
            }
#endif

            // Draw charts.
            foreach (var chart in charts)
                chart.Update();

            var handler = Updated;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }


        /// <summary>
        /// Called to remeasure a control.
        /// </summary>
        /// <param name="constraint">The maximum size that the method can return.</param>
        /// <returns>
        /// The size of the control, up to the maximum specified by <paramref name="constraint"/>.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Update();
            Size size = base.MeasureOverride(constraint);

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (size.Width == 0.0 && Numeric.IsPositiveFinite(constraint.Width))
                size.Width = constraint.Width;
            if (size.Height == 0.0 && Numeric.IsPositiveFinite(constraint.Height))
                size.Height = constraint.Height;
            // ReSharper restore CompareOfFloatsByEqualityOperator

            return size;
        }


        /// <summary>
        /// Suggests a scale for the x-axis.
        /// </summary>
        /// <returns>An appropriate scale for the x-axis. (Can be <see langword="null"/>.)</returns>
        /// <remarks>
        /// <para>
        /// When the <see cref="XAxis"/> is invalidated and <see cref="Axis.AutoScale"/> is set to
        /// <see langword="true"/> the <see cref="XAxis"/> calls <see cref="SuggestXScale"/> to 
        /// determine the optimal scale.
        /// </para>
        /// <para>
        /// The method <see cref="SuggestXScale"/> calls <see cref="OnSuggestXScale"/>.
        /// <see cref="OnSuggestXScale"/> can be overwritten in a derived
        /// class to return a suitable scale. The default implementation of 
        /// <see cref="OnSuggestXScale"/> returns <see langword="null"/>.
        /// </para>
        /// </remarks>
        public AxisScale SuggestXScale()
        {
            return OnSuggestXScale();
        }


        /// <summary>
        /// Called by <see cref="SuggestXScale"/> to suggest a scale for the x-axis
        /// </summary>
        /// <inheritdoc cref="SuggestXScale"/>
        protected virtual AxisScale OnSuggestXScale()
        {
            AxisScale scale = null;
            foreach (var chart in Charts)
            {
                var suggestedScale = chart.SuggestXScale();
                if (suggestedScale != null)
                {
                    if (scale == null)
                        scale = suggestedScale;
                    else
                        scale.Add(suggestedScale);
                }
            }

            return scale;
        }


        /// <summary>
        /// Suggests a scale for the y-axis.
        /// </summary>
        /// <returns>An appropriate scale for the y-axis. (Can be <see langword="null"/>.)</returns>
        /// <remarks>
        /// <para>
        /// When the <see cref="YAxis"/> is invalidated and <see cref="Axis.AutoScale"/> is set to
        /// <see langword="true"/> the <see cref="YAxis"/> calls <see cref="SuggestYScale"/> to 
        /// determine the optimal scale.
        /// </para>
        /// <para>
        /// The method <see cref="SuggestYScale"/> calls <see cref="OnSuggestYScale"/>.
        /// <see cref="OnSuggestXScale"/> can be overwritten in a derived
        /// class to return a suitable scale. The default implementation of 
        /// <see cref="OnSuggestYScale"/> returns <see langword="null"/>.
        /// </para>
        /// </remarks>
        public AxisScale SuggestYScale()
        {
            return OnSuggestYScale();
        }


        /// <summary>
        /// Called by <see cref="SuggestYScale"/> to suggest a scale for the y-axis
        /// </summary>
        /// <inheritdoc cref="SuggestYScale"/>
        protected virtual AxisScale OnSuggestYScale()
        {
            AxisScale scale = null;
            foreach (var chart in Charts)
            {
                var suggestedScale = chart.SuggestYScale();
                if (suggestedScale != null)
                {
                    if (scale == null)
                        scale = suggestedScale;
                    else
                        scale.Add(suggestedScale);
                }
            }

            return scale;
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
        /// <exception cref="ChartException">
        /// Data is not valid.
        /// </exception>
        public abstract void ValidateData();
        #endregion
    }
}
