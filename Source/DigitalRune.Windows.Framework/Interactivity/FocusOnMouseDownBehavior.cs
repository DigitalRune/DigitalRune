// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Sets the focus when a mouse button is pressed over the element.
    /// </summary>
    public class FocusOnMouseDownBehavior : Behavior<FrameworkElement>
    {
        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDown += OnMouseDown;
        }


        /// <summary>
        /// Called when the behavior is being detached from its
        /// <see cref="Behavior{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= OnMouseDown;
            base.OnDetaching();
        }


        private void OnMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            AssociatedObject.Focus();
        }
    }
}
