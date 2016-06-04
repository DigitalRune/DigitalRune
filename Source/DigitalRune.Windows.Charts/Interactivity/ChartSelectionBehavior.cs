// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
#if SILVERLIGHT
using System.Windows.Controls.Primitives;
#endif


namespace DigitalRune.Windows.Charts.Interactivity
{
    /// <summary>
    /// Allows the user to select objects inside a <see cref="ChartPanel"/> using the mouse.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An object inside the <see cref="ChartPanel"/> is selected by clicking the object with the
    /// left mouse button. (Clicking in an empty area of the <see cref="ChartPanel"/> does not
    /// deselect an object. Objects are deselected when the user selects another object.) When an
    /// object is selected the attached property
    /// <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.IsSelected"/>
    /// is set to <see langword="true"/>. Objects are ignored when the attached property
    /// <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.Selectable"/>
    /// is set to <see langword="false"/>.
    /// </para>
    /// <para>
    /// When a user clicks an element inside a <see cref="ChartPanel"/> the
    /// <see cref="SelectionChanged"/> event is raised before the attached property
    /// <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.IsSelected"/>
    /// is changed. Event handlers can intercept the selection process and provide their own logic.
    /// Event handlers should set the property <see cref="ChartSelectionChangedEventArgs.Handled"/>
    /// of the event arguments to <see langword="true"/> to prevent
    /// <see cref="ChartSelectionBehavior"/> from changing the attached property
    /// <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.IsSelected"/>.
    /// Selections that are changed by code can not be intercepted.
    /// </para>
    /// <para>
    /// When clicking the empty graph space, the user can drag a selection rectangle to select a
    /// rectangular region inside the chart area. However, the selection rectangle does not
    /// automatically select objects in the <see cref="ChartPanel"/>. Instead the event
    /// <see cref="SelectionRectangle"/> is raised when the selection rectangle is committed. The
    /// event handler needs to select or deselect objects depending on the selected rectangular
    /// region.
    /// </para>
    /// <para>
    /// The control <see cref="Interactivity.SelectionRectangle"/> is used to represent the
    /// selection rectangle. The style of this control can be modified to change the appearance of
    /// the selection rectangle.
    /// </para>
    /// </remarks>
    public class ChartSelectionBehavior : Behavior<ChartPanel>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _mouseLeftButtonDownHandled;
        private Point _mouseDownPosition;
        private SelectionRectangle _selectionRectangle;

#if SILVERLIGHT
        private Popup _popup;
#else
        private Adorner _adorner;
#endif
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------    

        /// <summary>
        /// Event raised when a child of the <see cref="DefaultChartPanel"/> is going to be selected
        /// or unselected. (This event is only raised if mouse interaction causes the selection
        /// change!)
        /// </summary>
        public event EventHandler<ChartSelectionChangedEventArgs> SelectionChanged;


        /// <summary>
        /// Event raised when a selection rectangle is drawn and committed.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The selection rectangle does not automatically select objects in the
        /// <see cref="DefaultChartPanel"/>. Instead the event <see cref="SelectionRectangle"/> is
        /// raised when the selection rectangle is committed. The event handler needs to select or
        /// deselect objects depending on the selected rectangular region.
        /// </para>
        /// </remarks>
        public event EventHandler<ChartSelectionRectangleEventArgs> SelectionRectangle;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(ChartSelectionBehavior),
            new PropertyMetadata(Boxed.BooleanTrue));

        /// <summary>
        /// Gets or sets a value indicating whether this behavior is enabled.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the behavior is enabled; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether this behavior is enabled.")]
        [Category(Categories.Default)]
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="IsSelectionRectangleEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectionRectangleEnabledProperty = DependencyProperty.Register(
            "IsSelectionRectangleEnabled",
            typeof(bool),
            typeof(ChartSelectionBehavior),
            new PropertyMetadata(Boxed.BooleanFalse));

        /// <summary>
        /// Gets or sets a value indicating whether the user can draw a selection rectangle to
        /// select elements in the <see cref="ChartPanel"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the selection rectangle is enabled; otherwise,
        /// <see langword="false"/>. The selection rectangle is disabled by default. The default
        /// value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// The selection rectangle does not automatically select objects in the
        /// <see cref="ChartPanel"/>. Instead the event <see cref="SelectionRectangle"/> is raised
        /// when the selection rectangle is committed. The event handler needs to select or deselect
        /// objects depending on the selected rectangular region.
        /// </para>
        /// </remarks>
        [Description("Gets or sets a value indicating whether the dragging a selection rectangle is enabled.")]
        [Category(Categories.Default)]
        public bool IsSelectionRectangleEnabled
        {
            get { return (bool)GetValue(IsSelectionRectangleEnabledProperty); }
            set { SetValue(IsSelectionRectangleEnabledProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Attached Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.IsSelected"/>
        /// attached dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value indicating whether the child element of a <see cref="ChartPanel"/>
        /// is selected. 
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the element is selected; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(ChartSelectionBehavior),
#if SILVERLIGHT
            new PropertyMetadata(Boxed.BooleanFalse));
#else
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
#endif

        /// <summary>
        /// Gets the value of the
        /// <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.IsSelected"/>
        /// attached property from a given element.
        /// </summary>
        /// <param name="element">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the
        /// <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.IsSelected"/>
        /// attached property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static bool GetIsSelected(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (bool)element.GetValue(IsSelectedProperty);
        }

        /// <summary>
        /// Sets the value of the
        /// <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.IsSelected"/>
        /// attached property to a given element.
        /// </summary>
        /// <param name="element">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static void SetIsSelected(DependencyObject element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(IsSelectedProperty, value);
        }


        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.Selectable"/>
        /// attached dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value indicating whether the child element of a <see cref="ChartPanel"/>
        /// can be selected.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the element can be selected; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty SelectableProperty = DependencyProperty.RegisterAttached(
            "Selectable",
            typeof(bool),
            typeof(ChartSelectionBehavior),
            new PropertyMetadata(Boxed.BooleanTrue));

        /// <summary>
        /// Gets the value of the
        /// <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.Selectable"/>
        /// attached property from a given element.
        /// </summary>
        /// <param name="element">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the
        /// <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.Selectable"/>
        /// attached property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static bool GetSelectable(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (bool)element.GetValue(SelectableProperty);
        }

        /// <summary>
        /// Sets the value of the
        /// <see cref="P:DigitalRune.Windows.Charts.Interactivity.ChartSelectionBehavior.Selectable"/>
        /// attached property to a given element.
        /// </summary>
        /// <param name="element">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static void SetSelectable(DependencyObject element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(SelectableProperty, value);
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Selects or unselects all children of a <see cref="DefaultChartPanel"/>.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Selects all children of this <see cref="DefaultChartPanel"/>.
        /// </summary>
        /// <remarks>
        /// The attached property <strong>ChartSelectionBehavior.IsSelected</strong> indicates
        /// whether a element in a <see cref="ChartPanel"/> is selected. Elements are ignored and
        /// cannot be selected when the attached property
        /// <strong>ChartSelectionBehavior.Selectable</strong> is set to <see langword="false"/>.
        /// </remarks>
        public void SelectAll()
        {
            SelectAll(true);
        }


        /// <summary>
        /// Unselects all children of this <see cref="DefaultChartPanel"/>.
        /// </summary>
        /// <remarks>
        /// The attached property <strong>ChartSelectionBehavior.IsSelected</strong> indicates
        /// whether a element in a <see cref="ChartPanel"/> is selected. Elements are ignored and
        /// cannot be selected when the attached property
        /// <strong>ChartSelectionBehavior.Selectable</strong> is set to <see langword="false"/>.
        /// </remarks>
        public void UnselectAll()
        {
            SelectAll(false);
        }


        /// <summary>
        /// Selects or unselects all children of a <see cref="DefaultChartPanel"/>.
        /// </summary>
        /// <param name="select">
        /// <see langword="true"/> to select all children; otherwise <see langword="false"/>.
        /// </param>
        /// <remarks>
        /// The attached property <strong>ChartSelectionBehavior.IsSelected</strong> indicates
        /// whether a element in a <see cref="ChartPanel"/> is selected. Elements are ignored and
        /// cannot be selected when the attached property
        /// <strong>ChartSelectionBehavior.Selectable</strong> is set to <see langword="false"/>.
        /// </remarks>
        public void SelectAll(bool select)
        {
            ChartPanel chartPanel = AssociatedObject;
            if (chartPanel != null)
            {
                foreach (UIElement child in chartPanel.Children)
                {
                    if (child != null
                        && GetSelectable(child)
                        && GetIsSelected(child) != select)
                    {
                        SetIsSelected(child, select);
                    }
                }
            }
        }


        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the <see cref="Behavior{T}.AssociatedObject"/>.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            ChartPanel chartPanel = AssociatedObject;
            if (chartPanel != null)
            {
                chartPanel.MouseLeftButtonDown += OnMouseLeftButtonDown;
#if SILVERLIGHT
                // Silverlight does not support Preview-events.
                chartPanel.MouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                chartPanel.MouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
#else
                chartPanel.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                chartPanel.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
#endif
            }
        }


        /// <summary>
        /// Called when the <see cref="Behavior{T}"/> is about to detach from the 
        /// <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        /// <remarks>
        /// When this method is called, detaching can not be canceled. The 
        /// <see cref="Behavior{T}.AssociatedObject"/> is still set.
        /// </remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            ChartPanel chartPanel = AssociatedObject;
            if (chartPanel != null)
            {
                chartPanel.MouseLeftButtonDown -= OnMouseLeftButtonDown;
#if SILVERLIGHT
                // Silverlight does not support Preview-events.
                chartPanel.MouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                chartPanel.MouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
#else
                chartPanel.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                chartPanel.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
#endif
            }
        }


        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (!IsEnabled)
            {
                // Behavior is disabled.
                return;
            }

#if !SILVERLIGHT
            if (Mouse.Captured != null)
            {
                // Another interaction is active.
                return;
            }
#endif

            ChartPanel chartPanel = AssociatedObject;
            _mouseDownPosition = eventArgs.GetPosition(chartPanel);

            if (IsSelectionRectangleEnabled
                && !_mouseLeftButtonDownHandled
                && (Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Windows)) == 0)
            {
                // Check whether the mouse is inside a chart area.
                var xAxes = chartPanel.Children
                                      .OfType<Axis>()
                                      .Where(axis => axis.IsXAxis)
                                      .ToArray();
                var yAxes = chartPanel.Children
                                      .OfType<Axis>()
                                      .Where(axis => axis.IsYAxis)
                                      .ToArray();

                foreach (Axis xAxis in xAxes)
                {
                    foreach (Axis yAxis in yAxes)
                    {
                        Rect chartAreaBounds = ChartPanel.GetChartAreaBounds(xAxis, yAxis);
                        if (chartAreaBounds.Contains(_mouseDownPosition))
                        {
                            // Mouse is inside the chart area spanned by current axis pair.
                            // --> Start dragging selection rectangle.
                            StartSelectionRectangle();
                            eventArgs.Handled = true;
                            break;
                        }
                    }
                }
            }
        }


        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (!IsEnabled)
            {
                // Behavior is disabled.
                return;
            }

#if !SILVERLIGHT
            if (Mouse.Captured != null)
            {
                // Another interaction is active.
                return;
            }
#endif

            ChartPanel chartPanel = AssociatedObject;
            _mouseDownPosition = eventArgs.GetPosition(chartPanel);
            _mouseLeftButtonDownHandled = false;

            // An element inside the ChartPanel was clicked.
            UIElement elementHit = chartPanel.GetChildContainingElement(eventArgs.OriginalSource as DependencyObject);
            if (elementHit != null)
            {
                // If clicked item was not selected, then deselect all other non-clicked children
                if (GetSelectable(elementHit) && !GetIsSelected(elementHit))
                {
                    foreach (UIElement child in chartPanel.Children)
                    {
                        bool isSelected = child == elementHit;
                        SelectElement(child, isSelected);
                    }
                }

                _mouseLeftButtonDownHandled = true;
            }

            // Important: Do not set eventArgs.Handled, because the event needs to tunnel to
            // the clicked object.

#if !SILVERLIGHT
            if (elementHit != null)
                elementHit.Focus();
#endif
        }


        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs eventArgs)
        {
            if (_selectionRectangle != null)
            {
                // End of a selection with selection rectangle.
                EndSelectionRectangle(true);
            }
        }


        /// <summary>
        /// Starts the selection.
        /// </summary>
        private void StartSelectionRectangle()
        {
            AssociatedObject.LostMouseCapture += OnMouseCaptureLost;

#if SILVERLIGHT
            AssociatedObject.KeyDown += OnPreviewKeyDown;
            AssociatedObject.MouseMove += OnPreviewMouseMove;
#else
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
            AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
#endif
            AssociatedObject.CaptureMouse();

            AddSelectionRectangle();
        }


        /// <summary>
        /// Finishes the selection.
        /// </summary>
        /// <param name="commit">
        /// If set to <see langword="true"/> the elements in the selection rectangle are selected; 
        /// otherwise the selection is aborted.
        /// </param>
        private void EndSelectionRectangle(bool commit)
        {
            if (_selectionRectangle != null)
            {
                ChartPanel chartPanel = AssociatedObject;
                if (commit)
                {
                    Point start = new Point(Canvas.GetLeft(_selectionRectangle), Canvas.GetTop(_selectionRectangle));
                    Point end = new Point(start.X + _selectionRectangle.Width, start.Y + _selectionRectangle.Height);
                    ChartSelectionRectangleEventArgs eventArgs = new ChartSelectionRectangleEventArgs(
                      start.X,
                      start.Y,
                      end.X,
                      end.Y,
                      Keyboard.Modifiers);

                    OnSelectionRectangle(eventArgs);
                }

                RemoveSelectionRectangle();
                chartPanel.LostMouseCapture -= OnMouseCaptureLost;

#if SILVERLIGHT
                chartPanel.KeyDown -= OnPreviewKeyDown;
                chartPanel.MouseMove -= OnPreviewMouseMove;
#else
                chartPanel.PreviewKeyDown -= OnPreviewKeyDown;
                chartPanel.PreviewMouseMove -= OnPreviewMouseMove;
#endif

                chartPanel.ReleaseMouseCapture();
            }
        }


        private void AddSelectionRectangle()
        {
            if (_selectionRectangle != null)
                return;

            _selectionRectangle = new SelectionRectangle();
            Canvas.SetLeft(_selectionRectangle, _mouseDownPosition.X);
            Canvas.SetTop(_selectionRectangle, _mouseDownPosition.Y);
            _selectionRectangle.Width = 0;
            _selectionRectangle.Height = 0;

            // We want to clip the selection rectangle to the area of the chart panel. Unfortunately
            // we cannot directly set a clip rectangle on the popup. But we can set a clip rectangle
            // on the selection rectangle. The position of the clip rectangle is relative to the
            // selection rectangle.
            _selectionRectangle.Clip = new RectangleGeometry { Rect = new Rect(-_mouseDownPosition.X, -_mouseDownPosition.Y, AssociatedObject.ActualWidth, AssociatedObject.ActualHeight) };

            Canvas canvas = new Canvas();
            canvas.Children.Add(_selectionRectangle);

#if SILVERLIGHT
            // Use Popup in Silverlight.
            _popup = new Popup();
            _popup.Child = canvas;
            _popup.IsOpen = true;
            AssociatedObject.Children.Add(_popup);
#else
            // Use Adorner in WPF.
            _adorner = new SingleChildAdorner(AssociatedObject, canvas);
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            adornerLayer.Add(_adorner);
#endif
        }


        private void RemoveSelectionRectangle()
        {
            if (_selectionRectangle == null)
                return;

#if SILVERLIGHT
            AssociatedObject.Children.Remove(_popup);
            _selectionRectangle = null;
            _popup = null;
#else
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            adornerLayer.Remove(_adorner);
            _selectionRectangle = null;
#endif
        }


        private void OnMouseCaptureLost(object sender, MouseEventArgs eventArgs)
        {
            EndSelectionRectangle(false);
        }


        private void OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Escape)
            {
                EndSelectionRectangle(false);
                eventArgs.Handled = true;
            }
        }


        private void OnPreviewMouseMove(object sender, MouseEventArgs eventArgs)
        {
            if (_selectionRectangle != null)
            {
                // Update selection rectangle.
                Point mousePosition = eventArgs.GetPosition(AssociatedObject);

                double left = Math.Min(_mouseDownPosition.X, mousePosition.X);
                double top = Math.Min(_mouseDownPosition.Y, mousePosition.Y);
                double height = Math.Abs(mousePosition.Y - _mouseDownPosition.Y);
                double width = Math.Abs(mousePosition.X - _mouseDownPosition.X);
                Canvas.SetLeft(_selectionRectangle, left);
                Canvas.SetTop(_selectionRectangle, top);
                _selectionRectangle.Width = width;
                _selectionRectangle.Height = height;

                // Update the clip rectangle. (The clip rectangle is relative to the selection rectangle.)
                RectangleGeometry clipRectangle = (RectangleGeometry)_selectionRectangle.Clip;
                clipRectangle.Rect = new Rect(-left, -top, AssociatedObject.ActualWidth, AssociatedObject.ActualHeight);
            }
        }


        private void SelectElement(UIElement element, bool select)
        {
            if (element == null)
                return;

            bool selectable = GetSelectable(element);
            bool isAlreadySelected = GetIsSelected(element);

            if (selectable)
            {
                // Select object
                var eventArgs = new ChartSelectionChangedEventArgs(element, select);
                OnSelected(eventArgs);
                if (!eventArgs.Handled)
                    SetIsSelected(element, select);
            }
            else
            {
                // Make sure that all non-selectable elements are not selected.
                if (isAlreadySelected)
                {
                    var eventArgs = new ChartSelectionChangedEventArgs(element, false);
                    OnSelected(eventArgs);
                    if (!eventArgs.Handled)
                        SetIsSelected(element, false);
                }
            }
        }


        /// <summary>
        /// Raises the <see cref="SelectionChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="ChartSelectionChangedEventArgs"/> object that provides the arguments for the
        /// event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnSelected"/> in a
        /// derived class, be sure to call the base class's <see cref="OnSelected"/> method so that
        /// registered delegates receive the event.
        /// </remarks>
        protected virtual void OnSelected(ChartSelectionChangedEventArgs eventArgs)
        {
            var handler = SelectionChanged;

            if (handler != null)
                handler(this, eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="SelectionRectangle"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="ChartSelectionRectangleEventArgs"/> object that provides the arguments for
        /// the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnSelectionRectangle"/>
        /// in a derived class, be sure to call the base class's <see cref="OnSelectionRectangle"/>
        /// method so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnSelectionRectangle(ChartSelectionRectangleEventArgs eventArgs)
        {
            var handler = SelectionRectangle;

            if (handler != null)
                handler(this, eventArgs);
        }
        #endregion
    }
}
