// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;


namespace DigitalRune.Windows.Framework
{
    public partial class DragDropBehavior
    {
        // The code in this file uses the DragCommand to start a drag-and-drop operation.
        // This part does not use adorners.
        // Overview:
        // - Mouse down event:
        //      Creates _dragCommandParameter.
        //      Calls DragCommand.CanExecute.
        // - Mouse move event:
        //      When mouse movement exceeds a threshold, DragCommand.Execute is called.
        //      DragCommand.Execute is responsible for starting the drag-and-drop operation.
        // - Mouse up event:
        //      Cancels the operation by setting _dragCommandParameter to null.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private DragCommandParameter _dragCommandParameter;
        private bool _inDragDrop;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="AllowDrag"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowDragProperty = DependencyProperty.Register(
            "AllowDrag",
            typeof(bool),
            typeof(DragDropBehavior),
            new PropertyMetadata(Boxed.BooleanTrue, OnAllowDragChanged));

        /// <summary>
        /// Gets or sets a value indicating whether a drag-and-drop operation can be initiated on 
        /// the associated object.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if a drag-and-drop operation can be initiated on the associated
        /// object; otherwise, <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether a drag-and-drop operation can be initiated on the associated object.")]
        [Category(Categories.Default)]
        public bool AllowDrag
        {
            get { return (bool)GetValue(AllowDragProperty); }
            set { SetValue(AllowDragProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="UsePreviewEvent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UsePreviewEventProperty = DependencyProperty.Register(
            "UsePreviewEvent",
            typeof(bool),
            typeof(DragDropBehavior),
            new PropertyMetadata(Boxed.BooleanTrue));

        /// <summary>
        /// Gets or sets a value indicating whether to start dragging on the
        /// <see cref="UIElement.PreviewMouseLeftButtonDown"/> or the
        /// <see cref="UIElement.MouseLeftButtonDown"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to listen for the tunneling event (preview event) of the left
        /// mouse button; otherwise, <see langword="false"/> to listen for the bubbling event of the
        /// left mouse button. The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether to listen for the PreviewMouseLeftButtonDown or the MouseLeftButtonDown.")]
        [Category(Categories.Default)]
        public bool UsePreviewEvent
        {
            get { return (bool)GetValue(UsePreviewEventProperty); }
            set { SetValue(UsePreviewEventProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="DragCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DragCommandProperty = DependencyProperty.Register(
            "DragCommand",
            typeof(ICommand),
            typeof(DragDropBehavior),
            new PropertyMetadata((ICommand)null));

        /// <summary>
        /// Gets or sets the command that is invoked when the user wants to drag an element to 
        /// initiate a drag-and-drop operation.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The command that is invoked when the user wants to drag an element.
        /// The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// This attached dependency property can be set to override the default drag-and-drop behavior.
        /// The <see cref="DragCommandParameter"/> is passed as the parameter of the 
        /// <see cref="ICommand.CanExecute"/> and <see cref="ICommand.Execute"/> methods. 
        /// <see cref="ICommand.CanExecute"/> is called directly before <see cref="ICommand.Execute"/>.
        /// The same instance of the <see cref="DragCommandParameter"/>s is passed to both methods.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the command that is invoked when the user wants to drag an element to initiate a drag-and-drop operation.")]
        [Category(Categories.Default)]
        public ICommand DragCommand
        {
            get { return (ICommand)GetValue(DragCommandProperty); }
            set { SetValue(DragCommandProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnAllowDragChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (DragDropBehavior)dependencyObject;
            behavior.OnAllowDragChanged();
        }


        private void OnAllowDragChanged()
        {
            if (AssociatedObject == null)
                return;

            if (AllowDrag)
                EnableDrag();
            else
                DisableDrag();
        }


        private void EnableDrag()
        {
            Debug.Assert(AllowDrag, "Sanity check.");

            // Register the required event handlers.
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
        }


        private void DisableDrag()
        {
            // Abort current drag operation.
            _dragCommandParameter = null;

            // Unregister event handlers.
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
        }


        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.Handled || !UsePreviewEvent)
                return;

            _dragCommandParameter = new DragCommandParameter
            {
                AssociatedObject = AssociatedObject,
                HitObject = eventArgs.OriginalSource as DependencyObject,
                MousePosition = eventArgs.GetPosition(AssociatedObject),

            };

            OnMouseLeftButtonDown(eventArgs);
        }


        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.Handled || UsePreviewEvent)
                return;

            OnMouseLeftButtonDown(eventArgs);
        }


        private void OnMouseLeftButtonDown(MouseButtonEventArgs eventArgs)
        {
            _dragCommandParameter = new DragCommandParameter
            {
                AssociatedObject = AssociatedObject,
                HitObject = eventArgs.OriginalSource as DependencyObject,
                MousePosition = eventArgs.GetPosition(AssociatedObject),
            };

            // Call DragCommand.CanExecute() and let the view model decide whether 
            // dragging is possible. The drag-and-drop starts in the mouse move 
            // event when the user moves the mouse by a significant distance.
            var dragCommand = DragCommand ?? DefaultDragCommand;
            if (!dragCommand.CanExecute(_dragCommandParameter))
                _dragCommandParameter = null;
        }


        private void OnPreviewMouseMove(object sender, MouseEventArgs eventArgs)
        {
            if (eventArgs.Handled)
                return;

            if (_dragCommandParameter == null)
                return;

            // Avoid reentrance
            if (_inDragDrop)
            {
                // Not sure why this happens. It can happen when dragging from a Button...
                return;
            }

            // Only start drag-and-drop when user moved the mouse by a reasonable amount.
            if (!ExceedsMinimumDragDistance(_dragCommandParameter.MousePosition, eventArgs.GetPosition(AssociatedObject)))
                return;

            _inDragDrop = true;

            // The user wants to drag something.
            try
            {
                var dragCommand = DragCommand ?? DefaultDragCommand;
                if (dragCommand.CanExecute(_dragCommandParameter))
                {
                    // Call DragCommand to start the drag-and-drop operation.
                    dragCommand.Execute(_dragCommandParameter);
                    eventArgs.Handled = true;
                }
            }
            finally
            {
                _inDragDrop = false;
                _dragCommandParameter = null;
            }
        }


        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs eventArgs)
        {
            // Cancel drag operation.
            _dragCommandParameter = null;
        }


        /// <summary>
        /// Determines whether mouse movement is big enough to start a drag operation.
        /// </summary>
        /// <param name="initialPosition">The initial mouse position.</param>
        /// <param name="currentPosition">The current mouse position.</param>
        /// <returns>
        /// <see langword="true"/> if delta between the <paramref name="initialPosition"/> and the
        /// <paramref name="currentPosition"/> is movement big enough to start a drag operation;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        private static bool ExceedsMinimumDragDistance(Point initialPosition, Point currentPosition)
        {
            // Note: If SystemParameters does not existing in Silverlight, just use 4.

            return Math.Abs(currentPosition.X - initialPosition.X) >= SystemParameters.MinimumHorizontalDragDistance
                   || Math.Abs(currentPosition.Y - initialPosition.Y) >= SystemParameters.MinimumVerticalDragDistance;
        }
        #endregion
    }
}
