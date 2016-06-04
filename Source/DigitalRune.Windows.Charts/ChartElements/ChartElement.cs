// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Base class for all chart elements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Chart elements are UI elements that require an <see cref="XAxis"/> and <see cref="YAxis"/>.
    /// Chart elements can provide a legend symbol (see method <see cref="GetLegendSymbol"/>) and a
    /// <see cref="Title"/> for display in a chart legend. The property
    /// <see cref="IsVisibleInLegend"/> indicates whether the chart element should be listed in the
    /// chart legend.
    /// </para>
    /// </remarks>
    public abstract class ChartElement : Control, IChartElement
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public bool IsValid { get; private set; }


        /// <inheritdoc/>
        public event EventHandler<EventArgs> Invalidated;


        /// <inheritdoc/>
        public event EventHandler<EventArgs> Updated;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        #region ----- Legend -----

        /// <summary>
        /// Identifies the <see cref="IsVisibleInLegend"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsVisibleInLegendProperty = DependencyProperty.Register(
            "IsVisibleInLegend",
            typeof(bool),
            typeof(ChartElement),
            new PropertyMetadata(Boxed.BooleanTrue));

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ChartElement"/> should be listed
        /// in the <see cref="Legend"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="ChartElement"/> should be shown in the
        /// <see cref="Legend"/>; otherwise, <see langword="false"/>. The default value is
        /// <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the chart element should be listed in the legend.")]
        [Category(ChartCategories.Default)]
        public bool IsVisibleInLegend
        {
            get { return (bool)GetValue(IsVisibleInLegendProperty); }
            set { SetValue(IsVisibleInLegendProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(ChartElement),
            new PropertyMetadata("Unnamed"));

        /// <summary>
        /// Gets or sets the title of the chart element (as displayed in the legend).
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The name of this chart that is displayed in the legend. The default value is
        /// <c>"Unnamed"</c>.
        /// </value>
        [Description("Gets or sets the title of the chart element (as displayed in the legend).")]
        [Category(ChartCategories.Default)]
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        #endregion


        #region ----- Axes -----

        /// <summary>
        /// Identifies the <see cref="XAxis"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(
            "XAxis",
            typeof(Axis),
            typeof(ChartElement),
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
            typeof(ChartElement),
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
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnXAxisChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.OldValue == eventArgs.NewValue)
                return;

            // Synchronize the attached dependency property ChartPanel.XAxis with the dependency
            // property ChartElement.XAxis.
            Axis xAxis = (Axis)eventArgs.NewValue;
            ChartPanel.SetXAxis(dependencyObject, xAxis);

            // Call ChartElement.OnXAxisChanged to raise the Invalidated event.
            ((ChartElement)dependencyObject).OnXAxisChanged(eventArgs);
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
            ((ChartElement)dependencyObject).OnYAxisChanged(eventArgs);
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

            // Ensure that all template parts are created.
            ApplyTemplate();

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

            // base.MeasureOverride() returns (0, 0) in most cases. ChartElements usually contain a 
            // Canvas which does not demand any space in Measure().
            // --> But the ChartElements should fill up the available space.
            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (size.Width == 0.0 && Numeric.IsPositiveFinite(constraint.Width))
                size.Width = constraint.Width;
            if (size.Height == 0.0 && Numeric.IsPositiveFinite(constraint.Height))
                size.Height = constraint.Height;
            // ReSharper restore CompareOfFloatsByEqualityOperator

            return size;
        }


        /// <summary>
        /// Gets the legend symbol.
        /// </summary>
        /// <returns>
        /// A <see cref="UIElement"/> representing the legend symbol of this chart element.
        /// <see langword="null"/> if the element does not provide a legend symbol.
        /// </returns>
        /// <remarks>
        /// The method <see cref="GetLegendSymbol"/> calls <see cref="OnGetLegendSymbol"/> to create
        /// the legend symbol. <see cref="OnGetLegendSymbol"/> can be overwritten in a derived
        /// class to return a suitable legend symbol. The default implementation of 
        /// <see cref="OnGetLegendSymbol"/> returns <see langword="null"/> (no legend symbol).
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public UIElement GetLegendSymbol()
        {
            var legendSymbol = OnGetLegendSymbol();

            // Set DataContext of legend symbol to same object as ChartElement to allow styles with
            // complex data binding.
            var element = legendSymbol as FrameworkElement;
            if (element != null)
                element.DataContext = DataContext ?? this;

            return legendSymbol;
        }


        /// <summary>
        /// Called by <see cref="GetLegendSymbol"/> to compute the legend symbol.
        /// </summary>
        /// <inheritdoc cref="GetLegendSymbol"/>
        protected virtual UIElement OnGetLegendSymbol()
        {
            return null;
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
            return null;
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
            return null;
        }
        #endregion
    }
}
