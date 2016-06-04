// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


namespace DigitalRune.Windows.Framework
{
    public partial class DragDropBehavior
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private DropCommandParameter _dropCommandParameter;
        private Point _currentMousePosition;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="AllowDrop"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowDropProperty = DependencyProperty.Register(
            "AllowDrop",
            typeof(bool),
            typeof(DragDropBehavior),
            new PropertyMetadata(Boxed.BooleanTrue, OnAllowDropChanged));

        /// <summary>
        /// Gets or sets a value indicating whether an element can be dropped onto the associated 
        /// object.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if an element can be dropped onto the associated object; 
        /// otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>
        /// </value>
        [Description("Gets or sets a value indicating whether an element can be dropped onto the associated object.")]
        [Category(Categories.Default)]
        public bool AllowDrop
        {
            get { return (bool)GetValue(AllowDropProperty); }
            set { SetValue(AllowDropProperty, Boxed.Get(value)); }
        }

        /// <summary>
        /// Identifies the <see cref="DropCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DropCommandProperty = DependencyProperty.Register(
            "DropCommand",
            typeof(ICommand),
            typeof(DragDropBehavior),
            new PropertyMetadata((ICommand)null));

        /// <summary>
        /// Gets or sets the command that is invoked when the user wants to drop an element.
        /// This is a dependency property.
        /// </summary>
        /// <value>The command that is invoked when the user wants to drop an element.</value>
        [Description("Gets or sets the command that is invoked when the user wants to drop an element.")]
        [Category(Categories.Default)]
        public ICommand DropCommand
        {
            get { return (ICommand)GetValue(DropCommandProperty); }
            set { SetValue(DropCommandProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="DragTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DragTemplateProperty = DependencyProperty.Register(
            "DragTemplate",
            typeof(DataTemplate),
            typeof(DragDropBehavior),
            new PropertyMetadata((DataTemplate)null));

        /// <summary>
        /// Gets or sets the data template used to show a representation of the dragged data near 
        /// the mouse cursor.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The data template used to show a representation of the dragged data near the mouse 
        /// cursor.
        /// </value>
        /// <remarks>
        /// <para>
        /// This property is only relevant for controls which are targets of a drag-and-drop 
        /// operation. When data is dragged onto these controls, the data will be shown
        /// displayed at the mouse position using the data template.
        /// </para>
        /// <para>
        /// The data that will be displayed near the mouse is defined by 
        /// <see cref="DropCommandParameter.Data"/>. This property must be set by the 
        /// <see cref="ICommand.CanExecute"/> of the <see cref="DropCommand"/>.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the data template used to show a representation of the dragged data near the mouse cursor.")]
        [Category(Categories.Default)]
        public DataTemplate DragTemplate
        {
            get { return (DataTemplate)GetValue(DragTemplateProperty); }
            set { SetValue(DragTemplateProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="BetweenItemsZone"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BetweenItemsZoneProperty = DependencyProperty.Register(
            "BetweenItemsZone",
            typeof(double),
            typeof(DragDropBehavior),
            new PropertyMetadata(0.5));

        /// <summary>
        /// Gets or sets a value that determines the size of the area where items are dropped
        /// between existing items instead of onto an item.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// Gets or sets a value that determines the size of the area where items are dropped
        /// between existing items instead of onto an item. <see cref="BetweenItemsZone"/> is a 
        /// relative value in the range [0, 0.5]. The default value is 0.5.
        /// </value>
        /// <remarks>
        /// <para>
        /// This property is only relevant if this behavior is attached to an 
        /// <see cref="ItemsControl"/>. To drop onto an existing item, the mouse has to be moved
        /// over an item. To drop between existing items, the mouse has to be moved to the edge of 
        /// an existing item. <see cref="BetweenItemsZone"/> is used to decide if a mouse position
        /// means the user wants to drop onto an item vs. before/after the item.
        /// </para>
        /// <para>
        /// <see cref="BetweenItemsZone"/> is a relative value in the range [0, 0.5]. If it is 0,
        /// then the user cannot drop between items.
        /// </para>
        /// <para>
        /// If <see cref="BetweenItemsZone"/> is 0.5, then we drop before an existing item if 
        /// the mouse is in the first half (50%) of an item. We drop after an existing item if
        /// the mouse is in the second half (50%) of an item. It is not possible, to drop onto
        /// an existing item.
        /// </para>
        /// <para>
        /// If <see cref="BetweenItemsZone"/> is 0.25 (default), then we drop before an existing item 
        /// if the mouse is in the first quarter (25%) of an item. We drop after an existing item if
        /// the mouse is in the second quarter (25%) of an item. If the mouse is between 25% and 
        /// 75% of the control size, then we drop onto the item.
        /// </para>
        /// </remarks>
        [Description("Gets or sets a value that determines the size of the area where items can be dropped between existing items.")]
        [Category(Categories.Default)]
        public double BetweenItemsZone
        {
            get { return (double)GetValue(BetweenItemsZoneProperty); }
            set { SetValue(BetweenItemsZoneProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ShowBetweenItemsIndicator"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowBetweenItemsIndicatorProperty = DependencyProperty.Register(
            "ShowBetweenItemsIndicator",
            typeof(bool),
            typeof(DragDropBehavior),
            new PropertyMetadata(Boxed.BooleanTrue));
        

        /// <summary>
        /// Gets or sets a value that determines whether an indicator should be drawn when items are 
        /// dropped between existing items instead of onto an item.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if an indicator should be drawn when items are dropped between
        /// existing items instead of onto an item; otherwise, <see langword="false" />.</value>
        /// <remarks>
        /// <para>
        /// If this property is <see langword="true" />, an indicator (e.g. a horizontal or
        /// vertical line with arrows) is drawn if the current drop position is between 
        /// two items of an items control. 
        /// </para>
        /// <para>
        /// See also <see cref="BetweenItemsZone"/>.
        /// </para>
        /// </remarks>
        [Description("Gets or sets a value that determines whether an indicator should be drawn when items are dropped between existing items instead of onto an item.")]
        [Category(Categories.Default)]
        public bool ShowBetweenItemsIndicator
        {
            get { return (bool)GetValue(ShowBetweenItemsIndicatorProperty); }
            set { SetValue(ShowBetweenItemsIndicatorProperty, Boxed.Get(value)); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnAllowDropChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (DragDropBehavior)dependencyObject;
            behavior.OnAllowDropChanged();
        }


        private void OnAllowDropChanged()
        {
            if (AssociatedObject == null)
                return;

            if (AllowDrag)
                EnableDrop();
            else
                DisableDrop();
        }


        private void EnableDrop()
        {
            Debug.Assert(AllowDrop, "Sanity check.");

            // It is necessary to set AllowDrop to receive the required events.
            AssociatedObject.AllowDrop = true;

            // Register the required event handlers.
            AssociatedObject.DragEnter += OnDragEnterOrDragOver;
            AssociatedObject.DragOver += OnDragEnterOrDragOver;
            AssociatedObject.DragLeave += OnDragLeave;
            AssociatedObject.Drop += OnDrop;
        }


        private void DisableDrop()
        {
            // Abort any drag-and-drop.
            RemoveDragAdorner();
            RemoveDropAdorner();
            _dropCommandParameter = null;

            // Unregister event handlers.
            AssociatedObject.DragEnter -= OnDragEnterOrDragOver;
            AssociatedObject.DragOver -= OnDragEnterOrDragOver;
            AssociatedObject.DragLeave -= OnDragLeave;
            AssociatedObject.Drop -= OnDrop;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void OnDragEnterOrDragOver(object sender, DragEventArgs eventArgs)
        {
            _currentMousePosition = eventArgs.GetPosition(AssociatedObject);

            // Check where the mouse is and if we should drop onto or between 
            // items of an ItemsControl.
            FrameworkElement targetItemContainer;
            bool isVertical;
            bool insertAfter;
            int targetIndex;
            DetermineDropTarget(eventArgs, out targetItemContainer, out isVertical, out insertAfter, out targetIndex);

            // Create/update the _dropCommandParameter.
            UpdateDropCommandParameter(eventArgs, targetIndex);

            var dropCommand = DropCommand ?? DefaultDropCommand;
            if (dropCommand.CanExecute(_dropCommandParameter))
            {
                if (ShowBetweenItemsIndicator && _dropCommandParameter.TargetIndex >= 0)
                    UpdateDropAdorner(targetItemContainer, isVertical, insertAfter);
                else
                    RemoveDropAdorner();
            }
            else
            {
                eventArgs.Effects = DragDropEffects.None;
                RemoveDropAdorner();
            }

            // Update position of preview near mouse immediately to avoid lag.
            UpdateDragAdornerPosition(_currentMousePosition);

            // There may be multiple DragEnter/DragOver/DragLeave events within one frame.
            // Defer showing/hiding of preview to avoid flickering.
            Dispatcher.BeginInvoke(new Action(UpdateDragAdornerVisibility));

            eventArgs.Handled = true;
        }


        private void OnDragLeave(object sender, DragEventArgs eventArgs)
        {
            _dropCommandParameter = null;

            RemoveDropAdorner();

            // There may be multiple DragEnter/DragOver/DragLeave events within one frame.
            // Defer showing/hiding of preview to avoid flickering.
            Dispatcher.BeginInvoke(new Action(UpdateDragAdornerVisibility));
        }


        private void UpdateDragAdornerVisibility()
        {
            if (_dropCommandParameter != null && _dropCommandParameter.Data != null)
                UpdateDragAdorner(_currentMousePosition);
            else
                RemoveDragAdorner();
        }


        private void OnDrop(object sender, DragEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                    return;

                eventArgs.Handled = true;

                // Check where the mouse is and if we should drop onto or between 
                // items of an ItemsControl.
                FrameworkElement targetItemContainer;
                bool isVertical;
                bool insertAfter;
                int targetIndex;
                DetermineDropTarget(eventArgs, out targetItemContainer, out isVertical, out insertAfter, out targetIndex);

                UpdateDropCommandParameter(eventArgs, targetIndex);

                var dropCommand = DropCommand ?? DefaultDropCommand;
                if (dropCommand.CanExecute(_dropCommandParameter))
                {
                    dropCommand.Execute(_dropCommandParameter);

                    // DragEventArgs.Effects is reported back by DragDrop.DoDragDrop.
                    eventArgs.Effects = _dropCommandParameter.DragEventArgs.Effects;
                }
            }
            finally
            {
                RemoveDragAdorner();
                RemoveDropAdorner();
                _dropCommandParameter = null;
            }
        }


        private void UpdateDropCommandParameter(DragEventArgs eventArgs, int targetIndex)
        {
            if (_dropCommandParameter == null)
                _dropCommandParameter = new DropCommandParameter();

            _dropCommandParameter.DragEventArgs = eventArgs;
            _dropCommandParameter.TargetIndex = targetIndex;
        }


        private void DetermineDropTarget(DragEventArgs eventArgs, out FrameworkElement targetItemContainer,
                                         out bool isVertical, out bool insertAfter, out int targetIndex)
        {
            // Following flags are only relevant when the target is a not empty ItemContainer.
            insertAfter = false;
            isVertical = false;

            var targetItemsControl = AssociatedObject as ItemsControl;
            if (targetItemsControl == null)
            {
                // ----- AssociatedObject is not an ItemsControl.
                targetItemContainer = null;
                targetIndex = -1;
                return;
            }

            int targetItemsControlCount = targetItemsControl.Items.Count;
            if (targetItemsControlCount <= 0)
            {
                // ----- AssociatedObject is an empty ItemsControl.
                targetItemContainer = null;
                targetIndex = 0;
                return;
            }

            // Update isVertical.
            var itemContainerGenerator = targetItemsControl.ItemContainerGenerator;
            var firstItemContainer = (FrameworkElement)itemContainerGenerator.ContainerFromIndex(0);
            isVertical = HasVerticalOrientation(firstItemContainer);

            // Get target item container that raised the event.
            targetItemContainer =
                targetItemsControl.ContainerFromElement((DependencyObject)eventArgs.OriginalSource)
                as FrameworkElement;

            if (targetItemContainer == null)
            {
                // ----- Mouse is over the empty part of an ItemsControl, but ItemsControl is not empty
                targetItemContainer =
                    itemContainerGenerator.ContainerFromIndex(targetItemsControlCount - 1) as
                        FrameworkElement;
                insertAfter = true;
                targetIndex = targetItemsControlCount;
                return;
            }

            // ----- Mouse is over an item container.
            Point positionRelativeToItemContainer = eventArgs.GetPosition(targetItemContainer);

            // Check if we should drop before, in or after the item.
            double f = Math.Max(0, Math.Min(0.5, BetweenItemsZone));
            double position = isVertical 
                            ? positionRelativeToItemContainer.Y 
                            : positionRelativeToItemContainer.X;
            double itemSize = isVertical ? targetItemContainer.ActualHeight : targetItemContainer.ActualWidth;

            if (position < itemSize * f)
            {
                insertAfter = false;
                targetIndex = itemContainerGenerator.IndexFromContainer(targetItemContainer);
            }
            else if (position > itemSize - itemSize * f)
            {
                insertAfter = true;
                targetIndex = itemContainerGenerator.IndexFromContainer(targetItemContainer) + 1;
            }
            else
            {
                targetIndex = -1;
            }
        }

        
        /// <summary>
        /// Determines the orientation of the panel of the <see cref="ItemsControl"/> that contains
        /// the given item container.
        /// </summary>
        /// <param name="itemContainer">The item container.</param>
        /// <returns>
        /// <see langword="true"/> if items panel has a vertical orientation; otherwise,
        /// <see langword="false"/>. (Also returns <see langword="true"/> if the orientation of the
        /// panel is unknown.)
        /// </returns>
        /// <remarks>
        /// The orientation is needed to figure out where to draw the adorner that indicates where
        /// the item will be dropped.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="itemContainer"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static bool HasVerticalOrientation(FrameworkElement itemContainer)
        {
            if (itemContainer == null)
                throw new ArgumentNullException("itemContainer");

            bool hasVerticalOrientation = true;
            var panel = VisualTreeHelper.GetParent(itemContainer) as Panel;

            // Handle different types of panels.
            if (panel is StackPanel)
            {
                hasVerticalOrientation = (((StackPanel)panel).Orientation == Orientation.Vertical);
            }
            else if (panel is WrapPanel)
            {
                hasVerticalOrientation = (((WrapPanel)panel).Orientation == Orientation.Vertical);
            }
            else if (panel is VirtualizingStackPanel)
            {
                hasVerticalOrientation = (((VirtualizingStackPanel)panel).Orientation == Orientation.Vertical);
            }

            // TODO: Add support for additional Panel types.

            return hasVerticalOrientation;
        }
        #endregion
    }
}
