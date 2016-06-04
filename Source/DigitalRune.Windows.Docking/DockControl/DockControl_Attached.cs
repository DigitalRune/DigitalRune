// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows.Docking
{
    partial class DockControl
    {
        //--------------------------------------------------------------
        #region Attached Dependency Properties & Routed Events
        //--------------------------------------------------------------    

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Docking.DockControl.DockWidth"/>
        /// attached dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets the desired width of a pane in the docking layout.
        /// </summary>
        /// <value>The desired width of a pane in the docking layout.</value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty DockWidthProperty = DependencyProperty.RegisterAttached(
            "DockWidth",
            typeof(GridLength),
            typeof(DockControl),
            new FrameworkPropertyMetadata(
                DockHelper.BoxedGridLengthOneStar,
                FrameworkPropertyMetadataOptions.AffectsMeasure
                | FrameworkPropertyMetadataOptions.AffectsParentMeasure
                | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDockSizeChanged));


        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Docking.DockControl.DockWidth"/>
        /// attached property from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Docking.DockControl.DockWidth"/>
        /// attached property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static GridLength GetDockWidth(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return (GridLength)obj.GetValue(DockWidthProperty);
        }


        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Docking.DockControl.DockWidth"/>
        /// attached property to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static void SetDockWidth(DependencyObject obj, GridLength value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.SetValue(DockWidthProperty, value);
        }


        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Docking.DockControl.DockHeight"/>
        /// attached dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets the desired height of a pane in the docking layout.
        /// </summary>
        /// <value>The desired height of a pane in the docking layout.</value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty DockHeightProperty = DependencyProperty.RegisterAttached(
            "DockHeight",
            typeof(GridLength),
            typeof(DockControl),
            new FrameworkPropertyMetadata(
                DockHelper.BoxedGridLengthOneStar,
                FrameworkPropertyMetadataOptions.AffectsMeasure
                | FrameworkPropertyMetadataOptions.AffectsParentMeasure
                | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDockSizeChanged));


        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Docking.DockControl.DockHeight"/>
        /// attached property from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Docking.DockControl.DockHeight"/>
        /// attached property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static GridLength GetDockHeight(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return (GridLength)obj.GetValue(DockHeightProperty);
        }


        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Docking.DockControl.DockHeight"/>
        /// attached property to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static void SetDockHeight(DependencyObject obj, GridLength value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.SetValue(DockHeightProperty, value);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the <see cref="P:DigitalRune.Windows.Docking.DockControl.DockWidth"/> or the
        /// <see cref="P:DigitalRune.Windows.Docking.DockControl.DockHeight"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void OnDockSizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            // Propagate the DockWidth/Height to children.
            if (dependencyObject is DockAnchorPane)
            {
                var dockAnchorPane = (DockAnchorPane)dependencyObject;
                var child = dockAnchorPane.ChildPane;
                if (child != null)
                {
                    Debug.Assert(child is DockAnchorPane || child is DockSplitPane || child is DockTabPane);

                    // If this pane is a *-size pane we do not change children that have absolute size.
                    var oldLength = (GridLength)child.GetValue(eventArgs.Property);
                    var newLength = (GridLength)eventArgs.NewValue;
                    if ((oldLength.IsStar && newLength.IsStar) || (!oldLength.IsStar && !newLength.IsStar))
                        child.SetValue(eventArgs.Property, eventArgs.NewValue);
                }
            }
            else if (dependencyObject is DockSplitPane)
            {
                var dockSplitPane = (DockSplitPane)dependencyObject;
                var orientation = dockSplitPane.Orientation;
                for (int i = 0; i < dockSplitPane.Items.Count; i++)
                {
                    var child = dockSplitPane.ItemContainerGenerator.ContainerFromIndex(i);
                    if (child != null)
                    {
                        Debug.Assert(child is DockAnchorPane || child is DockSplitPane || child is DockTabPane);

                        if (orientation == Orientation.Vertical && eventArgs.Property == DockWidthProperty
                            || orientation == Orientation.Horizontal && eventArgs.Property == DockHeightProperty)
                        {
                            child.SetValue(eventArgs.Property, eventArgs.NewValue);
                        }
                    }
                }
            }
            else if (dependencyObject is DockTabPane)
            {
                var dockTabPane = (DockTabPane)dependencyObject;
                for (int i = 0; i < dockTabPane.Items.Count; i++)
                {
                    var dockTabItem = dockTabPane.ItemContainerGenerator.ContainerFromIndex(i) as DockTabItem;
                    if (dockTabItem != null)
                    {
                        // If this pane is a *-size pane we do not change children that have absolute size.
                        var oldLength = (GridLength)dockTabItem.GetValue(eventArgs.Property);
                        var newLength = (GridLength)eventArgs.NewValue;
                        if ((oldLength.IsStar && newLength.IsStar) || (!oldLength.IsStar && !newLength.IsStar))
                            dockTabItem.SetValue(eventArgs.Property, eventArgs.NewValue);
                    }
                }
            }
            else if (dependencyObject is DockTabItem)
            {
                var dockTabItem = (DockTabItem)dependencyObject;
                var dockTabPane = ItemsControl.ItemsControlFromItemContainer(dockTabItem) as DockTabPane;
                if (dockTabPane != null && dockTabPane.Items.Count == 1)
                {
                    // Copy the relevant property to the DockTabPane if this is the only child.
                    dockTabPane.SetValue(eventArgs.Property, eventArgs.NewValue);
                }
            }
        }
        #endregion
    }
}
