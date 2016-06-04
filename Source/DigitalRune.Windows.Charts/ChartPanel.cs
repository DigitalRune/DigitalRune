// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// A <see cref="Panel"/> that shows charts consisting of <see cref="Axis"/> elements,
    /// <see cref="Chart"/>s, <see cref="ChartElement"/>s, or custom <see cref="UIElement"/>s.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="ChartPanel"/> is a special kind of <see cref="Panel"/> used to render 2D
    /// charts such as line charts, bar charts, scatter plots, etc.
    /// </para>
    /// <para>
    /// A <see cref="ChartPanel"/> can have the following children:
    /// <list type="bullet">
    /// <item>
    /// <strong>Axes:</strong>
    /// <para>
    /// <see cref="Axis"/> elements are the first elements that should be added to a
    /// <see cref="ChartPanel"/>. <see cref="Axis"/> elements are required because all other
    /// children of a <see cref="ChartPanel"/> are positioned relative to their associated x-axis
    /// and y-axis. An <see cref="Axis"/> is positioned absolutely within the
    /// <see cref="ChartPanel"/> by setting the properties <see cref="Axis.OriginX"/>,
    /// <see cref="Axis.OriginY"/>, <see cref="Axis.Length"/>, and <see cref="Orientation"/>.
    /// Depending on the <see cref="Axis.Orientation"/> an <see cref="Axis"/> defines either a
    /// horizontal x-axis or a vertical y-axis.
    /// </para>
    /// <para>
    /// When a <see cref="ChartPanel"/> is used the <see cref="Axis"/> elements need to be added
    /// manually to the panel. A <see cref="DefaultChartPanel"/> is specialized
    /// <see cref="ChartPanel"/> that automatically provides two axis pairs. See
    /// <see cref="DefaultChartPanel"/> for more info.
    /// </para>
    /// </item>
    /// <item>
    /// <strong>Charts:</strong>
    /// <para>
    /// <see cref="Chart"/>s (incl. <see cref="BarChart"/>, <see cref="BarChartGroup"/>,
    /// <see cref="LineChart"/>, <see cref="LineChartGroup"/>, <see cref="ScatterPlot"/> and
    /// specialized variants such as <see cref="ColoredBarChart"/>, <see cref="ColoredLineChart"/>,
    /// or <see cref="HeatChart"/>) are the main elements used in a <see cref="ChartPanel"/>. A
    /// <see cref="Chart"/> needs to be assigned to an x-axis and a y-axis (see properties
    /// <see cref="ChartElement.XAxis"/> and <see cref="ChartElement.YAxis"/>). Then the data to
    /// visualize needs to be assigned to the <see cref="Chart"/> (see property
    /// <see cref="Chart.DataSource"/>). Depending on the chart type, such as
    /// <see cref="LineChart"/>, <see cref="BarChart"/>, the data is visualized in different ways.
    /// </para>
    /// <para>
    /// New chart types can be added by implementing a WPF control that inherits
    /// <see cref="Chart"/>.
    /// </para>
    /// </item>
    /// <item>
    /// <strong>ChartElements:</strong>
    /// <para>
    /// <see cref="ChartElement"/>s are auxiliary elements that can be rendered inside a
    /// <see cref="ChartPanel"/>. Elements derived from <see cref="ChartElement"/> are for example
    /// <see cref="ChartBackground"/>, <see cref="ChartGrid"/>,
    /// <see cref="HorizontalChartLine"/>, etc.
    /// </para>
    /// <para>
    /// Similar to <see cref="Charts"/>, an x-axis and a y-axis need to be assigned to each
    /// <see cref="ChartElement"/>. But in contrast to <see cref="Chart"/>s,
    /// <see cref="ChartElement"/>s do not have a <see cref="Chart.DataSource"/>.
    /// </para>
    /// </item>
    /// <item>
    /// <strong>UIElements:</strong>
    /// <para>
    /// Aside from <see cref="ChartElement"/> and <see cref="Chart"/>s any <see cref="UIElement"/>
    /// can be added to <see cref="ChartPanel"/>. <see cref="UIElement"/>s also need to be assigned
    /// to an x-axis and a y-axis. This can be done by setting the attached dependency properties
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/> and
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/>. The position of the
    /// <see cref="UIElement"/> is specified by setting the attached dependency properties
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X1"/>,
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y1"/>,
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X2"/> and
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y2"/>. The position specified by these
    /// properties are not absolute positions in device-independent pixels. Instead, these positions
    /// are data values on the associated x- and y-axis.
    /// </para>
    /// <para>
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X1"/> and
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y1"/> are mandatory and typically define
    /// the top, left corner of the child element.
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X2"/> and
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y2"/> are optional and can be set to
    /// define the bottom, right corner of the area where the element is placed.
    /// </para>
    /// <para>
    /// When the element is derived from <see cref="FrameworkElement"/> the properties
    /// <see cref="FrameworkElement.HorizontalAlignment"/> and
    /// <see cref="FrameworkElement.VerticalAlignment"/> can be used to define the position of the
    /// element relative to the upper left corner (if only
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X1"/> and
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y1"/> are set) or relative to the
    /// rectangular area (if <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X1"/>,
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y1"/>,
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X2"/> and
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y2"/> are set).
    /// </para>
    /// <para>
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X1"/>,
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y1"/>,
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X2"/>, and
    /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y2"/> have no effect when they are set on
    /// an element that is derived from <see cref="ChartElement"/>. Elements derived from
    /// <see cref="ChartElement"/> - such as <see cref="ChartGrid"/>, <see cref="LineChart"/>,
    /// <see cref="BarChart"/>, etc. - position themselves automatically relative to their
    /// <see cref="ChartElement.XAxis"/> and <see cref="ChartElement.YAxis"/>.
    /// </para>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Chart Area:</strong> The rectangular area that is spanned by an x- and a y-axes is
    /// called "chart area". Elements such <see cref="ChartBackground"/> or
    /// <see cref="ChartGrid"/> can be used to draw the background of a chart area.
    /// </para>
    /// <para>
    /// <strong>Z-Indices:</strong> The z-order of chart elements is defined using the
    /// <strong>ZIndex</strong> attached property ( <strong>Panel.ZIndex</strong> in WPF and
    /// <strong>Canvas.ZIndex</strong> in Silverlight).
    /// </para>
    /// </remarks>
    public partial class ChartPanel : Panel
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // Workaround for Silverlight: 
        // To detect when the Children collection has changed, we store our own children collection
        // and compare it with Children in OnLayoutUpdated.
        // (WPF would allow to replace the Children collection with a custom ObservableCollection.
        // However, this solution is not available in Silverlight.)
        private readonly List<UIElement> _children = new List<UIElement>();

        private readonly List<Axis> _monitoredAxes = new List<Axis>();
        private ReadOnlyCollection<Axis> _monitoredAxesReadOnly;

        private readonly ObservableCollection<ChartElement> _chartElements = new ObservableCollection<ChartElement>();
        private ReadOnlyObservableCollection<ChartElement> _chartElementsReadOnly;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a read-only collection of all chart axes associated with the chart elements.
        /// </summary>
        /// <value>A read-only collection of all chart axes.</value>
        /// <remarks>
        /// Chart axes do not have to be children of the chart panel. Chart elements may be
        /// associated with axes outside of the chart panel.
        /// </remarks>
        public ReadOnlyCollection<Axis> Axes
        {
            get
            {
                if (_monitoredAxesReadOnly == null)
                    _monitoredAxesReadOnly = new ReadOnlyCollection<Axis>(_monitoredAxes);

                return _monitoredAxesReadOnly;
            }
        }


        /// <summary>
        /// Gets a read-only collection of all chart elements.
        /// </summary>
        /// <value>A read-only collection of all chart elements.</value>
        /// <remarks>
        /// <para>
        /// This collections is assembled by collecting all <see cref="ChartElement"/> instances in
        /// <see cref="Panel.Children"/>.
        /// </para>
        /// <para>
        /// This collection is similar to <see cref="Panel.Children"/>. The differences between
        /// <see cref="Panel.Children"/> and <see cref="ChartElements"/> are:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <see cref="ChartElements"/> is read-only.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <see cref="ChartElements"/> contains only elements that implement
        /// <see cref="ChartElement"/>. <see cref="Panel.Children"/> contains all logical children
        /// of the <see cref="ChartPanel"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// When multiple chart series are grouped using a <see cref="ChartGroup"/> (e.g.
        /// <see cref="BarChartGroup"/>, <see cref="LineChartGroup"/>) then
        /// <see cref="Panel.Children"/> contains the <see cref="ChartGroup"/> element.
        /// <see cref="ChartElements"/> does not contain the <see cref="ChartGroup"/> element, it
        /// directly contains the charts that are grouped.
        /// </description>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        public ReadOnlyObservableCollection<ChartElement> ChartElements
        {
            get
            {
                if (_chartElementsReadOnly == null)
                    _chartElementsReadOnly = new ReadOnlyObservableCollection<ChartElement>(_chartElements);

                return _chartElementsReadOnly;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

#if !SILVERLIGHT
        /// <summary>
        /// Identifies the <see cref="PreviewMouseDoubleClick"/> routed event. (Not available in
        /// Silverlight.)
        /// </summary>
        public static readonly RoutedEvent PreviewMouseDoubleClickEvent = Control.PreviewMouseDoubleClickEvent.AddOwner(typeof(ChartPanel));

        /// <summary>
        /// Occurs when a mouse button is double clicked.
        /// </summary>
        public event MouseButtonEventHandler PreviewMouseDoubleClick
        {
            add { AddHandler(PreviewMouseDoubleClickEvent, value); }
            remove { RemoveHandler(PreviewMouseDoubleClickEvent, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MouseDoubleClick"/> routed event. (Not available in
        /// Silverlight.)
        /// </summary>
        public static readonly RoutedEvent MouseDoubleClickEvent = Control.MouseDoubleClickEvent.AddOwner(typeof(ChartPanel));

        /// <summary>
        /// Occurs when a mouse button is double clicked.
        /// </summary>
        public event MouseButtonEventHandler MouseDoubleClick
        {
            add { AddHandler(MouseDoubleClickEvent, value); }
            remove { RemoveHandler(MouseDoubleClickEvent, value); }
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

#if !SILVERLIGHT
        /// <summary>
        /// Initializes static members of the <see cref="ChartPanel"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ChartPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChartPanel), new FrameworkPropertyMetadata(typeof(ChartPanel)));
            FocusableProperty.OverrideMetadata(typeof(ChartPanel), new FrameworkPropertyMetadata(Boxed.BooleanTrue));

            EventManager.RegisterClassHandler(typeof(ChartPanel), PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), true);
            EventManager.RegisterClassHandler(typeof(ChartPanel), MouseLeftButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), true);
            EventManager.RegisterClassHandler(typeof(ChartPanel), PreviewMouseRightButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), true);
            EventManager.RegisterClassHandler(typeof(ChartPanel), MouseRightButtonDownEvent, new MouseButtonEventHandler(HandleDoubleClick), true);
        }
#endif


        /// <summary>
        /// Initializes a new instance of the <see cref="ChartPanel"/> class.
        /// </summary>
        public ChartPanel()
        {
            LayoutUpdated += OnLayoutUpdated;
            Loaded += OnLayoutUpdated;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

#if !SILVERLIGHT
        #region ----- Double Click -----

        /// <summary>
        /// Handles double clicks.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="MouseButtonEventArgs"/> instance containing the event data.
        /// </param>
        private static void HandleDoubleClick(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.ClickCount == 2)
            {
                var chartPanel = (ChartPanel)sender;
                var args = new MouseButtonEventArgs(eventArgs.MouseDevice, eventArgs.Timestamp, eventArgs.ChangedButton, eventArgs.StylusDevice);
                if ((eventArgs.RoutedEvent == PreviewMouseLeftButtonDownEvent) || (eventArgs.RoutedEvent == PreviewMouseRightButtonDownEvent))
                {
                    args.RoutedEvent = PreviewMouseDoubleClickEvent;
                    args.Source = eventArgs.OriginalSource;
                    args.Handled = eventArgs.Handled;
                    chartPanel.OnPreviewMouseDoubleClick(args);
                }
                else
                {
                    args.RoutedEvent = MouseDoubleClickEvent;
                    args.Source = eventArgs.OriginalSource;
                    args.Handled = eventArgs.Handled;
                    chartPanel.OnMouseDoubleClick(args);
                }
                if (args.Handled)
                {
                    eventArgs.Handled = true;
                }
            }
        }


        /// <summary>
        /// Raises the <see cref="PreviewMouseDoubleClick"/> routed event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="MouseButtonEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding
        /// <see cref="OnPreviewMouseDoubleClick"/> in a derived class, be sure to call the base
        /// class's <see cref="OnPreviewMouseDoubleClick"/> method so that registered delegates
        /// receive the event.
        /// </remarks>
        protected virtual void OnPreviewMouseDoubleClick(MouseButtonEventArgs eventArgs)
        {
            Debug.Assert(eventArgs != null && eventArgs.RoutedEvent == PreviewMouseDoubleClickEvent, "Invalid arguments for ChartPanel.OnPreviewMouseDoubleClick.");
            RaiseEvent(eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="MouseDoubleClick"/> routed event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="MouseButtonEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnMouseDoubleClick"/>
        /// in a derived class, be sure to call the base class's <see cref="OnMouseDoubleClick"/>
        /// method so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnMouseDoubleClick(MouseButtonEventArgs eventArgs)
        {
            Debug.Assert(eventArgs != null && eventArgs.RoutedEvent == MouseDoubleClickEvent, "Invalid arguments for ChartPanel.OnMouseDoubleClick.");
            RaiseEvent(eventArgs);
        }
        #endregion
#endif


        private void OnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            var newElements = Children.OfType<UIElement>()
                                      .Where(element => !_children.Contains(element))
                                      .ToArray();
            var oldElements = _children.Where(element => !Children.Contains(element))
                                       .ToArray();

            if (newElements.Length > 0 || oldElements.Length > 0)
            {
                // Update the internal copy of the Children collection.
                foreach (var element in oldElements)
                    _children.Remove(element);

                foreach (var element in newElements)
                    _children.Add(element);

                // Detect if any axes need to be monitored.
                DetectAxes();
            }

            // Update the ChartElements collection.
            // (Always call this method, even if Children collection has not changed. The
            // descendants of the children might have changed. For example, when a ChartElement is
            // created using data templates it will be the child of a ContentPresenter. The creation
            // of the data template might be deferred.)
            UpdateChartElements();
        }


        /// <summary>
        /// Updates the <see cref="ChartElements"/> collection if the children of the chart panel
        /// have changed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void UpdateChartElements()
        {
            // Check whether the ChartElements have changed.
            // The previous ChartElements are stored in _chartElements.
            var chartElements = new List<ChartElement>(Children.Count);
            foreach (UIElement child in Children)
            {
                // Copy child because child is readonly. 
                // (The iteration variable of a foreach loop is readonly!)
                DependencyObject element = child;

                // If child is a ContentPresenter, find the first descendant that isn't one.
                while (element is ContentPresenter && VisualTreeHelper.GetChildrenCount(element) > 0)
                    element = VisualTreeHelper.GetChild(element, 0);

                if (element is ChartGroup)
                {
                    var chartGroup = (ChartGroup)element;
                    foreach (var chart in chartGroup.Charts)
                        chartElements.Add(chart);
                }
                else if (element is ChartElement)
                {
                    var chartElement = (ChartElement)element;
                    chartElements.Add(chartElement);
                }
            }

            bool haveChartElementsChanged = false;
            if (chartElements.Count != _chartElements.Count)
            {
                haveChartElementsChanged = true;
            }
            else
            {
                for (int i = 0; i < chartElements.Count; i++)
                {
                    if (chartElements[i] != _chartElements[i])
                    {
                        haveChartElementsChanged = true;
                        break;
                    }
                }
            }

            if (haveChartElementsChanged)
            {
                // Update _chartElements.
                _chartElements.Clear();
                foreach (ChartElement chartElement in chartElements)
                    _chartElements.Add(chartElement);
            }
        }


#if !SILVERLIGHT
        /// <summary>
        /// Returns a geometry for a clipping mask. The mask applies if the layout system attempts
        /// to arrange an element that is larger than the available display space.
        /// </summary>
        /// <param name="layoutSlotSize">
        /// The size of the part of the element that does visual presentation.
        /// </param>
        /// <returns>The clipping geometry.</returns>
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            if (ClipToBounds)
                return new RectangleGeometry(new Rect(RenderSize));

            return null;
        }
#endif


        /// <summary>
        /// Gets the element that meets the given criteria. 
        /// </summary>
        /// <param name="element">The child of the <see cref="ChartPanel"/>.</param>
        /// <param name="predicate">A predicate that defines the criteria.</param>
        /// <returns>
        /// The element (<paramref name="element"/> or a descendant of <paramref name="element"/> -
        /// see remarks) that  meets the criteria. Returns <see langword="null"/> if element does
        /// not meet the criteria.
        /// </returns>
        /// <remarks>
        /// If <paramref name="element"/> is a <see cref="ContentPresenter"/> then the
        /// <paramref name="element"/> is ignored and its child is checked instead. If the child
        /// meets the <paramref name="predicate"/> it is returned instead of
        /// <paramref name="element"/>. <see langword="null"/> is returned if neither
        /// <paramref name="element"/> nor the child of a <see cref="ContentPresenter"/> meets the
        /// criteria.
        /// </remarks>
        private static DependencyObject GetElement(DependencyObject element, Predicate<DependencyObject> predicate)
        {
            if (predicate(element))
                return element;

            // ----- Support for ItemsControl:
            // The ChartPanel can be used as the ItemsPanel of an ItemsControl. When items are
            // created using data templates, ContentPresenters are added to the ChartPanel. The
            // instanced data templates are the children of the ContentPresenters.

            // Only check ContentPresenters. Children of other types are ignored.
            while (element is ContentPresenter && VisualTreeHelper.GetChildrenCount(element) > 0)
            {
                element = VisualTreeHelper.GetChild(element, 0);
                if (predicate(element))
                    return element;
            }

            return null;
        }


        /// <summary>
        /// Gets the element that needs to be arranged manually by the chart panel and that has the
        /// relevant attached dependency properties.
        /// </summary>
        /// <param name="element">The child of the <see cref="ChartPanel"/>.</param>
        /// <returns>
        /// The element (<paramref name="element"/> or a descendant of <paramref name="element"/>)
        /// that needs to be arranged manually and that has the relevant attached dependency
        /// properties. Returns <see langword="null"/> if element can be ignored.
        /// </returns>
        private static DependencyObject GetElementThatRequiresArrange(DependencyObject element)
        {
            return GetElement(element, RequiresArrange);
        }


        /// <summary>
        /// Determines whether the given element needs to be positioned by the chart panel or
        /// whether it positions itself automatically.
        /// </summary>
        /// <param name="element">The element that needs to be checked.</param>
        /// <returns>
        /// <see langword="true"/> if the element needs to be positioned by the chart panel;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        private static bool RequiresArrange(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (element is Axis || element is IChartElement)
                return false;

            // Both XAxis and YAxis need to be set to fulfill the requirements.
            Axis xAxis = GetXAxis(element);
            Axis yAxis = GetYAxis(element);
            return xAxis != null && yAxis != null;
        }


        /// <summary>
        /// Gets the element that has the x- and y-axis defined.
        /// </summary>
        /// <param name="element">The child of the <see cref="ChartPanel"/>.</param>
        /// <returns>
        /// The element (<paramref name="element"/> or a descendant of <paramref name="element"/>)
        /// that needs to be arranged manually and that has the relevant attached dependency
        /// properties. Returns <see langword="null"/> if element can be ignored.
        /// </returns>
        private static DependencyObject GetElementWithAxes(DependencyObject element)
        {
            return GetElement(element, HasAxes);
        }


        /// <summary>
        /// Determines whether the given element needs to be positioned by the chart panel or
        /// whether it positions itself automatically.
        /// </summary>
        /// <param name="element">The element that needs to be checked.</param>
        /// <returns>
        /// <see langword="true"/> if the element needs to be positioned by the chart panel;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        private static bool HasAxes(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            // Both XAxis and YAxis need to be set to fulfill the requirements.
            Axis xAxis = GetXAxis(element);
            Axis yAxis = GetYAxis(element);
            return xAxis != null && yAxis != null;
        }


        /// <summary>
        /// Measures the child elements of a <see cref="ChartPanel"/> in anticipation of arranging
        /// them during the <see cref="ArrangeOverride(Size)"/> pass.
        /// </summary>
        /// <param name="availableSize">
        /// An upper limit <see cref="Size"/> that should not be exceeded.
        /// </param>
        /// <returns>
        /// A <see cref="Size"/> that represents the size that is required to arrange child content.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // Ensure that all axes are up to date.
            foreach (Axis axis in _monitoredAxes)
                axis.Update();

            // Measure each child and calculate the bounds of child.
            UIElementCollection children = Children;
            for (int i = 0; i < children.Count; ++i)
            {
                // Get child element.
                UIElement child = children[i];
                if (child == null)
                    continue;

                // Get the child or one of its descendants that needs to be positioned.
                DependencyObject elementThatRequiresArrange = GetElementThatRequiresArrange(child);
                if (elementThatRequiresArrange == null)
                {
                    // This child ignores the attached properties (X1, Y1, X2, Y2).
                    child.Measure(availableSize);
                    continue;
                }

                // Get associated axes.
                Axis xAxis = GetXAxis(elementThatRequiresArrange);
                Axis yAxis = GetYAxis(elementThatRequiresArrange);

                // Check whether we have to position the element relative to a point (X1, Y1) 
                // or inside a region (X1, Y1) - (X2, Y2).
                // Convert positions given in data values to pixel positions. 
                double x1 = xAxis.GetPosition(GetX1(elementThatRequiresArrange));
                double y1 = yAxis.GetPosition(GetY1(elementThatRequiresArrange));
                double x2 = xAxis.GetPosition(GetX2(elementThatRequiresArrange));
                double y2 = yAxis.GetPosition(GetY2(elementThatRequiresArrange));

                double availableWidth = Double.PositiveInfinity;
                double availableHeight = Double.PositiveInfinity;

                if (Numeric.IsFinite(x1) && Numeric.IsFinite(x2))
                {
                    // X1 and X2 are set.
                    double range = Math.Abs(x2 - x1);
                    if (!Numeric.IsZero(range))
                        availableWidth = range;
                }

                if (Numeric.IsFinite(y1) && Numeric.IsFinite(y2))
                {
                    // Y1 and Y2 are set.
                    double range = Math.Abs(y2 - y1);
                    if (!Numeric.IsZero(range))
                        availableHeight = range;
                }

                // Measure child and store its desired arrangement.
                Size elementConstraint = new Size(availableWidth, availableHeight);
                child.Measure(elementConstraint);
            }

            // Do not demand any size.
            // (The ChartPanel is similar to a canvas.)
            return new Size();
        }


        /// <summary>
        /// Arranges the children of <see cref="ChartPanel"/>.
        /// </summary>
        /// <param name="finalSize">
        /// The size that this <see cref="ChartPanel"/> should use to arrange its child elements.
        /// </param>
        /// <returns>
        /// A <see cref="Size"/> that represents the arranged size of this <see cref="ChartPanel"/>
        /// and its descendants.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Ensure that all axes are up to date.
            foreach (Axis axis in _monitoredAxes)
                axis.Update();

            UIElementCollection children = Children;
            for (int i = 0; i < children.Count; ++i)
            {
                UIElement child = children[i];
                if (child == null)
                    continue;

                // Get the child or one of its descendants that needs to be positioned.
                DependencyObject elementThatRequiresArrange = GetElementThatRequiresArrange(child);
                if (elementThatRequiresArrange == null)
                {
                    // This child ignores the attached properties (X1, Y1, X2, Y2).
                    Rect childBounds = new Rect(new Point(), finalSize);

                    // Arrange element,
                    child.Arrange(childBounds);

                    // Clip to chart area.
                    if (GetClipToChartArea(child))
                        ClipElementToChartArea(child, childBounds);
                }
                else
                {
                    // Determine alignment.
                    HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;
                    VerticalAlignment verticalAlignment = VerticalAlignment.Top;
                    if (elementThatRequiresArrange is FrameworkElement)
                    {
                        FrameworkElement frameworkElement = (FrameworkElement)elementThatRequiresArrange;
                        horizontalAlignment = frameworkElement.HorizontalAlignment;
                        verticalAlignment = frameworkElement.VerticalAlignment;
                    }

                    // Get associated axes.
                    Axis xAxis = GetXAxis(elementThatRequiresArrange);
                    Axis yAxis = GetYAxis(elementThatRequiresArrange);

                    // Check whether we have to position the element relative to a point (X1, Y1) 
                    // or inside a region (X1, Y1) - (X2, Y2).
                    // Convert positions given in data values to pixel positions. 
                    double x1 = xAxis.GetPosition(GetX1(elementThatRequiresArrange));
                    double y1 = yAxis.GetPosition(GetY1(elementThatRequiresArrange));
                    double x2 = xAxis.GetPosition(GetX2(elementThatRequiresArrange));
                    double y2 = yAxis.GetPosition(GetY2(elementThatRequiresArrange));

                    bool isX1Valid = Numeric.IsFinite(x1);
                    bool isY1Valid = Numeric.IsFinite(y1);
                    bool isX2Valid = Numeric.IsFinite(x2);
                    bool isY2Valid = Numeric.IsFinite(y2);

                    double availableWidth = Double.PositiveInfinity;
                    double availableHeight = Double.PositiveInfinity;

                    if (!isX1Valid && !isX2Valid)
                    {
                        // No coordinates set. Use 0 and 'left' alignment.
                        x1 = 0;
                        x2 = Double.NaN;
                        horizontalAlignment = HorizontalAlignment.Left;
                    }
                    else if (!isX1Valid)
                    {
                        // Only X2 set. Use X2 as position.
                        x1 = x2;
                        x2 = Double.NaN;
                    }
                    else if (isX2Valid)
                    {
                        // X1 and X2 are set.
                        int result = Numeric.Compare(x1, x2);
                        if (result < 0)
                        {
                            // X1 < X2: Horizontal region.
                            availableWidth = x2 - x1;
                        }
                        else if (result == 0)
                        {
                            // X1 == X2: Use only X1.
                            x2 = Double.NaN;
                        }
                        else
                        {
                            // X2 > X1: Horizontal region, but swapped.
                            ChartHelper.Swap(ref x1, ref x2);
                            availableWidth = x2 - x1;
                        }
                    }

                    if (!isY1Valid && !isY2Valid)
                    {
                        // No coordinates set. Use 0 and 'top' alignment.
                        y1 = 0;
                        y2 = Double.NaN;
                        verticalAlignment = VerticalAlignment.Top;
                    }
                    else if (!isY1Valid)
                    {
                        // Only Y2 set. Use Y2 as position.
                        y1 = y2;
                        y2 = Double.NaN;
                    }
                    else if (isY2Valid)
                    {
                        // Y1 and Y2 are set.
                        int result = Numeric.Compare(y1, y2);
                        if (result < 0)
                        {
                            // Y1 < Y2: Horizontal region.
                            availableHeight = y2 - y1;
                        }
                        else if (result == 0)
                        {
                            // Y1 == Y2: Use only Y1.
                            y2 = Double.NaN;
                        }
                        else
                        {
                            // Y2 > Y1: Horizontal region, but swapped.
                            ChartHelper.Swap(ref y1, ref y2);
                            availableHeight = y2 - y1;
                        }
                    }

                    // Get size of child.
                    double elementWidth = child.DesiredSize.Width;
                    double elementHeight = child.DesiredSize.Height;

                    if (elementWidth == 0.0 && elementHeight == 0.0 && child is FrameworkElement)
                    {
                        // Fix for Silverlight.
                        FrameworkElement frameworkElement = (FrameworkElement)child;
                        elementWidth = frameworkElement.ActualWidth;
                        elementHeight = frameworkElement.ActualHeight;
                    }

                    // Compute bounds of the child element.
                    Rect childBounds = new Rect();

                    // Position child horizontally.
                    if (Numeric.IsNaN(x2))
                    {
                        // Position child relative to point.
                        switch (horizontalAlignment)
                        {
                            case HorizontalAlignment.Left:
                            case HorizontalAlignment.Stretch:
                                childBounds.X = x1;
                                childBounds.Width = elementWidth;
                                break;
                            case HorizontalAlignment.Center:
                                childBounds.X = x1 - elementWidth / 2.0;
                                childBounds.Width = elementWidth;
                                break;
                            case HorizontalAlignment.Right:
                                childBounds.X = x1 - elementWidth;
                                childBounds.Width = elementWidth;
                                break;
                        }
                    }
                    else
                    {
                        // Position child inside horizontal region.
                        switch (horizontalAlignment)
                        {
                            case HorizontalAlignment.Left:
                                childBounds.X = x1;
                                childBounds.Width = elementWidth;
                                break;
                            case HorizontalAlignment.Center:
                                childBounds.X = x1 + availableWidth / 2.0 - elementWidth / 2.0;
                                childBounds.Width = elementWidth;
                                break;
                            case HorizontalAlignment.Right:
                                childBounds.X = x2 - elementWidth;
                                childBounds.Width = elementWidth;
                                break;
                            case HorizontalAlignment.Stretch:
                                childBounds.X = x1;
                                childBounds.Width = availableWidth;
                                break;
                        }
                    }

                    // Position child vertically.
                    if (Numeric.IsNaN(y2))
                    {
                        // Position child relative to point.
                        switch (verticalAlignment)
                        {
                            case VerticalAlignment.Top:
                            case VerticalAlignment.Stretch:
                                childBounds.Y = y1;
                                childBounds.Height = elementHeight;
                                break;
                            case VerticalAlignment.Center:
                                childBounds.Y = y1 - elementHeight / 2.0;
                                childBounds.Height = elementHeight;
                                break;
                            case VerticalAlignment.Bottom:
                                childBounds.Y = y1 - elementHeight;
                                childBounds.Height = elementHeight;
                                break;
                        }
                    }
                    else
                    {
                        // Position child inside vertical region.
                        switch (verticalAlignment)
                        {
                            case VerticalAlignment.Top:
                                childBounds.Y = y1;
                                childBounds.Height = elementHeight;
                                break;
                            case VerticalAlignment.Center:
                                childBounds.Y = y1 + availableHeight / 2.0 - elementHeight / 2.0;
                                childBounds.Height = elementHeight;
                                break;
                            case VerticalAlignment.Bottom:
                                childBounds.Y = y2 - elementHeight;
                                childBounds.Height = elementHeight;
                                break;
                            case VerticalAlignment.Stretch:
                                childBounds.Y = y1;
                                childBounds.Height = availableHeight;
                                break;
                        }
                    }

                    // Arrange element.
                    child.Arrange(childBounds);

                    // Clip to chart area.
                    if (elementThatRequiresArrange is UIElement && GetClipToChartArea(elementThatRequiresArrange))
                    {
                        UIElement element = (UIElement)elementThatRequiresArrange;
                        ClipElementToChartArea(element, childBounds);
                    }
                }
            }

            return finalSize;
        }


        /// <summary>
        /// Clips a child element to the chart area.
        /// </summary>
        /// <param name="element">The child element.</param>
        /// <param name="elementBounds">The bounds of <paramref name="element"/>.</param>
        private static void ClipElementToChartArea(UIElement element, Rect elementBounds)
        {
            // Get associated axes:
            Axis xAxis = GetXAxis(element);
            Axis yAxis = GetYAxis(element);

            if (xAxis == null || yAxis == null)
            {
                // Axes are not set properly.
                // Do not clip the element.
                element.Clip = null;
            }
            else
            {
                // Compute the chart area rectangle defined by the associated axes.
                Rect clipRectangle = GetChartAreaBounds(xAxis, yAxis);

                // Transform rectangle into local coordinates.
                clipRectangle.X = clipRectangle.X - elementBounds.X;
                clipRectangle.Y = clipRectangle.Y - elementBounds.Y;

                var frameworkElement = element as FrameworkElement;
                if (frameworkElement != null)
                {
                    // Add offset if Margin is set.
                    var margin = frameworkElement.Margin;
                    clipRectangle.X -= margin.Left;
                    clipRectangle.Y -= margin.Top;
                }

                // Set clipping geometry on child element.
                var clipGeometry = element.Clip as RectangleGeometry;
                if (clipGeometry == null)
                    clipGeometry = new RectangleGeometry();

                clipGeometry.Rect = clipRectangle;
                element.Clip = clipGeometry;
            }
        }


        /// <summary>
        /// Computes the bounding rectangle of a chart area.
        /// </summary>
        /// <param name="xAxis">The x-axis of the chart area.</param>
        /// <param name="yAxis">The y-axis of the chart area.</param>
        /// <returns>
        /// The bounding rectangle of the chart area. Returns <see cref="Rect.Empty"/> if one or
        /// both axes are <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Either <paramref name="xAxis"/> or <paramref name="yAxis"/> has the wrong orientation.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public static Rect GetChartAreaBounds(Axis xAxis, Axis yAxis)
        {
            if (xAxis == null || yAxis == null)
                return Rect.Empty;

            if (xAxis.Orientation != Orientation.Horizontal)
                throw new ArgumentException("X-axis must be a horizontal axis.", "xAxis");
            if (yAxis.Orientation != Orientation.Vertical)
                throw new ArgumentException("Y-axis must be a vertical axis.", "yAxis");

            return new Rect(xAxis.OriginX, yAxis.OriginY - yAxis.Length, xAxis.Length, yAxis.Length);
        }


        /// <summary>
        /// Detects the axes that need to be monitored.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Child elements derived from <see cref="ChartElement"/>s are automatically updated when
        /// the associated axes changed. Child elements that are not derived from
        /// <see cref="ChartElement"/> need to be repositioned by the <see cref="ChartPanel"/>.
        /// </para>
        /// <para>
        /// Every time children are added/removed, or when the attached dependency properties
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/> or
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/> this method needs to be
        /// called. The method detects the axes that need to be monitored and installs an event
        /// handler that updates the chart panel if required.
        /// </para>
        /// </remarks>
        private void DetectAxes()
        {
            // Get all children that need to be positioned manually.
            var elements = Children.OfType<UIElement>()
                                   .Select(GetElementWithAxes)
                                   .Where(element => element != null)
                                   .ToArray();

            // Get all x- and y-axes of these elements.
            var xAxes = elements.Select(GetXAxis).Distinct();
            var yAxes = elements.Select(GetYAxis).Distinct();
            var axes = xAxes.Concat(yAxes).ToArray();

            // Determine which axes were added and which were removed since the last time.
            var oldAxes = _monitoredAxes.Where(axis => !axes.Contains(axis)).ToArray();
            var newAxes = axes.Where(axis => !_monitoredAxes.Contains(axis)).ToArray();

            // Remove event handler for old axes.
            foreach (Axis axis in oldAxes)
            {
                _monitoredAxes.Remove(axis);
                axis.Invalidated -= OnAxisChanged;
            }

            // Add event handler for new axes.
            foreach (Axis axis in newAxes)
            {
                _monitoredAxes.Add(axis);
                axis.Invalidated += OnAxisChanged;
            }
        }


        /// <summary>
        /// Internal use only. Do not call explicitly.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
#if SILVERLIGHT
        // Note: Axis.Updated is a weak event. Therefore the event handler OnAxisChanged needs to be
        // public, because weak events require full trust.
        public
#else
        private
#endif
        void OnAxisChanged(object sender, EventArgs eventArgs)
        {
            InvalidateArrange();
        }
        #endregion
    }
}
