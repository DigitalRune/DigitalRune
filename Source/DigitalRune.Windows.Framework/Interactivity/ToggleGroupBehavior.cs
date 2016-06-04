// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Enables easy toggling of <see cref="ToggleButton"/>s that belong to the same group.
    /// </summary>
    public class ToggleGroupBehavior : Behavior<FrameworkElement>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private object _groupId;
        private bool _isChecked;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Attached Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Framework.ToggleGroupBehavior.Id"/> attached dependency
        /// property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// 
        /// <summary>
        /// Gets or sets the group ID.
        /// </summary>
        /// <value>The group ID.</value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty IdProperty = DependencyProperty.RegisterAttached(
            "Id",
            typeof(object),
            typeof(ToggleGroupBehavior),
            new FrameworkPropertyMetadata(null));


        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Framework.ToggleGroupBehavior.Id"/> attached
        /// property from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Framework.ToggleGroupBehavior.Id"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static object GetId(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return obj.GetValue(IdProperty);
        }


        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Framework.ToggleGroupBehavior.Id"/> attached
        /// property to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static void SetId(DependencyObject obj, object value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            obj.SetValue(IdProperty, value);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
        }


        /// <summary>
        /// Called when the behavior is being detached from its 
        /// <see cref="Behavior{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;

            base.OnDetaching();
        }


        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            var button = GetToggleButton(eventArgs);
            if (button != null)
                eventArgs.Handled = BeginToggle(button);
        }


        private ToggleButton GetToggleButton(RoutedEventArgs eventArgs)
        {
            var element = eventArgs.OriginalSource as DependencyObject
                          ?? eventArgs.Source as DependencyObject;

            while (element != null && element != AssociatedObject)
            {
                var toggleButton = element as ToggleButton;
                if (toggleButton != null)
                    return toggleButton;

                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        }


        private bool BeginToggle(ToggleButton button)
        {
            if (Mouse.Captured != null)
                return false;

            if (!Mouse.Capture(AssociatedObject, CaptureMode.SubTree))
                return false;

            _groupId = GetId(button);
            if (_groupId == null)
                return false;

            _isChecked = !button.IsChecked ?? true;
            button.IsChecked = _isChecked;

            AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
            AssociatedObject.PreviewMouseUp += OnPreviewMouseUp;
            AssociatedObject.LostMouseCapture += OnLostMouseCapture;
            return true;
        }


        private void EndToggle()
        {
            AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
            AssociatedObject.PreviewMouseUp -= OnPreviewMouseUp;
            AssociatedObject.LostMouseCapture -= OnLostMouseCapture;
            AssociatedObject.ReleaseMouseCapture();

            _groupId = null;
        }


        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs eventArgs)
        {
            EndToggle();
        }


        private void OnLostMouseCapture(object sender, MouseEventArgs eventArgs)
        {
            EndToggle();
        }


        private void OnPreviewMouseMove(object sender, MouseEventArgs eventArgs)
        {
            var button = GetToggleButton(eventArgs);
            if (button != null && Equals(GetId(button), _groupId))
                button.IsChecked = _isChecked;

            eventArgs.Handled = true;
        }
        #endregion
    }
}
