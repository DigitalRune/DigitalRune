// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Contains attached dependency properties to correct the scoping of access keys
    /// within the WPF framework.
    /// </summary>
    /// <remarks>
    /// Source: http://coderelief.net/2012/07/29/wpf-access-keys-scoping/
    /// </remarks>
    public static class AccessKeyScope
    {
        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Window.AccessKeyScope.IsEnabled"/>
        /// attached dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value indicating whether the element defines a scope for access keys.
        /// </summary>
        /// <value>The a value indicating whether the element defines a scope for access keys.</value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(AccessKeyScope),
            new PropertyMetadata(Boxed.BooleanFalse, OnIsEnabledChanged));


        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Window.AccessKeyScope.IsEnabled"/>
        /// attached property from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Window.AccessKeyScope.IsEnabled"/>
        /// attached property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static bool GetIsEnabled(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return (bool)obj.GetValue(IsEnabledProperty);
        }


        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Window.AccessKeyScope.IsEnabled"/>
        /// attached property to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.SetValue(IsEnabledProperty, Boxed.Get(value));
        }


        /// <summary>
        /// Called when the <see cref="P:DigitalRune.Window.AccessKeyScope.IsEnabled"/> property
        /// changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnIsEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if ((bool)eventArgs.NewValue)
                AccessKeyManager.AddAccessKeyPressedHandler(dependencyObject, OnAccessKeyPressed);
            else
                AccessKeyManager.RemoveAccessKeyPressedHandler(dependencyObject, OnAccessKeyPressed);
        }


        /// <summary>
        /// Fixes access key scoping bug within the WPF framework.
        /// </summary>
        /// <param name="sender">Potential target of the current access keys.</param>
        /// <param name="eventArgs">
        /// Info object for the current access keys and proxy to effect it's confirmation.
        /// </param>
        /// <remarks>
        /// <para>
        /// The problem is that all access key presses are scoped to the active window, regardless
        /// of what properties, handlers, scope etc. you may have set. Targets are objects that have
        /// potential to be the target of the access keys in effect.
        /// </para>
        /// <para>
        /// If you happen to have a current object focused and you press the access keys of one of
        /// it's child's targets it will execute the child target. But, if you also have an ancestor
        /// target, the ancestor target will be executed instead. That goes against intuition and
        /// standard Windows behavior.
        /// </para>
        /// <para>
        /// The root of this logic (bug) is within the HwndSource.OnMnemonicCore method. If the
        /// scope is set to anything but the active window's HwndSource, the target will not be
        /// executed and the handler for the next target in the chain will be called.
        /// </para>
        /// <para>
        /// This handler gets called for every target within the scope, which because of the bug is
        /// always at the window level of the active window. If you set <c>eventArgs.Handled</c> to
        /// <see langword="true"/>, no further handlers in the chain will be executed. However
        /// because setting the scope to anything other than active window's HwndSource causes the
        /// target not to be acted on, we can use it to not act on the target while not canceling
        /// the chain either, thereby allowing us to skip to the next target's handler. Note that if
        /// a handler does act on the target it will inheritably break the chain because the menu
        /// will lose focus and the next handlers won't apply anymore; because a target has already
        /// been confirmed.
        /// </para>
        /// <para>
        /// We will use this knowledge to resolve the issue. We will set the scope to something
        /// other than the active window's HwndSource, if we find that the incorrect element is
        /// being targeted for the access keys (because the target is out of scope). This will cause
        /// the target to be skipped and the next target's handler will be called.
        /// </para>
        /// <para>
        /// If we detect the target is correct, we'll just leave everything alone so the target will
        /// be confirmed.
        /// </para>
        /// <para>
        /// NOTE: Do not call AccessKeyManager.IsKeyRegistered as it will cause a
        /// <see cref="T:System.StackOverflowException"/> to be thrown. The key is registered
        /// otherwise this handler wouldn't be called for it, therefore there is no need to call it.
        /// </para>
        /// </remarks>
        private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs eventArgs)
        {
            var focusedElement = Keyboard.FocusedElement as FrameworkElement;
            if (focusedElement == null)
                return; // No focused element.

            if (sender == focusedElement)
                return; // This is the correct target.

            // Look through descendants tree to see if this target is a descendant of the focused
            // element. We will stop looking at either the end of the tree or if a object with
            // multiple children is encountered that this target isn't a descendant of.
            //
            // If no valid target is found, we'll set the scope to the sender which results in
            // skipping to the next target handler in the chain (due to the bug).

            DependencyObject obj = focusedElement;
            while (obj != null)
            {
                int childCount = VisualTreeHelper.GetChildrenCount(obj);
                for (int i = 0; i < childCount; i++)
                {
                    if (VisualTreeHelper.GetChild(obj, i) == sender)
                        return; // Found correct target; let it execute.
                }

                if (childCount > 1)
                {
                    // This target isn't a direct descendant and there are multiple
                    // direct descendants; skip this target.
                    eventArgs.Scope = sender;
                    return;
                }

                if (childCount == 1)
                {
                    // This target isn't a direct descendant, but we'll keep looking
                    // down the descendants chain to see if it's a descendant of the
                    // direct descendant.
                    obj = VisualTreeHelper.GetChild(obj, 0);
                }
                else
                {
                    // End of the line; skip this target.
                    eventArgs.Scope = sender;
                    return;
                }
            }
        }
    }
}
