// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Handles RETURN, ESCAPE and TAB keys for the Quick Find combo box.
    /// </summary>
    public class QuickFindBehavior : Behavior<ComboBox>
    {
        private readonly WeakReference<IInputElement> _oldFocus = new WeakReference<IInputElement>(null);


        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.GotKeyboardFocus += OnGotKeyboardFocus;
            AssociatedObject.LostKeyboardFocus += OnLostKeyboardFocus;
            AssociatedObject.PreviewKeyDown += OnKeyDown;
        }


        /// <summary>
        /// Called when the behavior is being detached from its
        /// <see cref="Behavior{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.GotKeyboardFocus -= OnGotKeyboardFocus;
            AssociatedObject.LostKeyboardFocus -= OnLostKeyboardFocus;
            AssociatedObject.PreviewKeyDown -= OnKeyDown;

            base.OnDetaching();
        }


        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs eventArgs)
        {
            _oldFocus.SetTarget(eventArgs.OldFocus);
        }


        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs eventArgs)
        {
            _oldFocus.SetTarget(null);
        }


        private void OnKeyDown(object sender, KeyEventArgs eventArgs)
        {
            var comboBox = (ComboBox)sender;
            var toolBarComboBoxViewModel = (ToolBarComboBoxViewModel)comboBox.DataContext;
            var quickFindCommandItem = (QuickFindCommandItem)toolBarComboBoxViewModel.CommandItem;
            if (eventArgs.Key == Key.Return)
            {
                // Execute FindNext action.
                quickFindCommandItem.SearchExtension.FindNext();
                eventArgs.Handled = true;
            }
            else if (eventArgs.Key == Key.Escape)
            {
                // Clear search query and move focus back to previous element.
                quickFindCommandItem.SearchExtension.Query.FindPattern = null;
                MoveFocus();
                eventArgs.Handled = true;
            }
            else if (eventArgs.Key == Key.Tab)
            {
                // Move focus back to previous element.
                MoveFocus();
                eventArgs.Handled = true;
            }
        }


        private void MoveFocus()
        {
            IInputElement oldFocus;
            if (_oldFocus.TryGetTarget(out oldFocus))
                oldFocus.Focus();
        }
    }
}
