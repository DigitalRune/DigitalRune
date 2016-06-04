// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Triggers when a double-click is detected.
    /// </summary>
    public class DoubleClickTrigger : TriggerBase<UIElement>
    {
        /// <summary>
        /// Called after the trigger is attached to an <see cref="TriggerBase{T}.AssociatedObject"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.PreviewMouseDown += OnPreviewMouseDown;
        }


        /// <summary>
        /// Called when the trigger is being detached from its 
        /// <see cref="TriggerBase{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PreviewMouseDown -= OnPreviewMouseDown;
        }


        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.ClickCount == 2)
            {
                InvokeActions(null);
                eventArgs.Handled = true;
            }
        }
    }
}
