// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Selects all content of the <see cref="TextBox"/> when the element receives the focus.
    /// </summary>
    public class SelectAllOnGotFocusBehavior : Behavior<TextBox>
    {
        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += GotFocus;
        }


        /// <summary>
        /// Called when the behavior is being detached from its 
        /// <see cref="Behavior{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.GotFocus -= GotFocus;
            base.OnDetaching();
        }


        private void GotFocus(object sender, RoutedEventArgs eventArgs)
        {
            Dispatcher.BeginInvoke(new Action(AssociatedObject.SelectAll));
        }
    }
}
