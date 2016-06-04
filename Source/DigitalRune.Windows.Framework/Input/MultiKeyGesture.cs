// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using static System.FormattableString;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Defines a keyboard combination of multiple key strokes that can be used to invoke a command.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The multi-key gesture is canceled when ESC is pressed. It is also canceled if more than
    /// <see cref="MaximumDelayBetweenKeyPresses"/> passes between to key strokes.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The property <see cref="KeyGesture.Key"/> is not used and will
    /// always return <see cref="Key.None"/> - use <see cref="Keys"/> instead! If
    /// <see cref="KeyGesture.Modifiers"/> are specified then the modifier keys need to be pressed
    /// during the whole multi-key gesture.
    /// </para>
    /// <para>
    /// When several multi-key gestures start with the same key strokes they should have the same
    /// length (= the same number of keys). For example, when key gestures "Ctrl+K" and Ctrl+K,
    /// Ctrl+F" are defined, the application cannot decide which gesture is the right one. The
    /// resulting behavior is undefined. The following combination of key gestures works fine:
    /// "Ctrl+K, Ctrl+F, Ctrl+D", "Ctrl+K, Ctrl+F, Ctrl+F", and "Ctrl+F".
    /// </para>
    /// <para>
    /// The <see cref="KeyGesture.DisplayString"/> is usually set automatically, however a custom
    /// display string can be set using the constructor
    /// <see cref="MultiKeyGesture(IEnumerable{Key}, ModifierKeys, string)"/>.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [TypeConverter(typeof(MultiKeyGestureConverter))]
    [ValueSerializer(typeof(MultiKeyGestureValueSerializer))]
    public class MultiKeyGesture : KeyGesture
    {
        // NOTE: MultiKeyGesture needs to derive from KeyGesture because MenuItem treats KeyGesture special.
        // MenuItem tries to cast the InputGesture to KeyGesture. On success it uses 
        // KeyGesture.GetDisplayStringForCulture() to display the key stroke.

        // Credits: This class is based on the blog entry by Kent Boogaart.
        // See http://kentb.blogspot.com/2009/03/multikeygesture.html.
        // But I [MartinG] had to change the class drastically. The original design did not handle 
        // conflicting key gestures such as "Ctrl+X" and "Ctrl+K, Ctrl+X" correctly.

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly List<Key> _keys;
        private int _currentKeyIndex;
        private int _lastKeyPress;
        private UIElement _currentTarget;
        private bool _reportMatch;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the keys associated with the gesture.
        /// </summary>
        /// <value>The keys associated with the gesture.</value>
        public ReadOnlyCollection<Key> Keys
        {
            get { return _keys.AsReadOnly(); }
        }


        /// <summary>
        /// Gets or sets the maximum allowed delay between key presses.
        /// </summary>
        /// <value>The maximum delay between key presses. The default value is 2 second.</value>
        /// <remarks>
        /// A multi-key gesture is canceled if more than the allowed amount of time passes between
        /// two key strokes.
        /// </remarks>
        public static TimeSpan MaximumDelayBetweenKeyPresses { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="MultiKeyGesture"/> class.
        /// </summary>
        static MultiKeyGesture()
        {
            MaximumDelayBetweenKeyPresses = TimeSpan.FromSeconds(2);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MultiKeyGesture"/> class.
        /// </summary>
        /// <param name="keys">The keys associated with the gesture.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keys"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// A specified <see cref="Key"/> value is invalid.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="keys"/> is empty.
        /// </exception>
        public MultiKeyGesture(IEnumerable<Key> keys)
            : this(keys, ModifierKeys.None)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MultiKeyGesture"/> class.
        /// </summary>
        /// <param name="keys">The keys associated with the gesture.</param>
        /// <param name="modifiers">The modifiers associated with the gesture.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keys"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// A specified <see cref="Key"/> value or the <see cref="ModifierKeys"/> value is invalid.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="keys"/> is empty.
        /// </exception>
        public MultiKeyGesture(IEnumerable<Key> keys, ModifierKeys modifiers)
            : this(keys, modifiers, string.Empty)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MultiKeyGesture"/> class.
        /// </summary>
        /// <param name="keys">The keys associated with the gesture.</param>
        /// <param name="modifiers">The modifiers associated with the gesture.</param>
        /// <param name="displayString">
        /// A string representation of the <see cref="MultiKeyGesture"/>. (If <see langword="null"/>
        /// or empty, the <see cref="KeyGesture.DisplayString"/> is determined automatically.)
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="keys"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// A specified <see cref="Key"/> value or the <see cref="ModifierKeys"/> value is invalid.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="keys"/> is empty.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
        public MultiKeyGesture(IEnumerable<Key> keys, ModifierKeys modifiers, string displayString)
            : base(Key.None, modifiers, GetDisplayString(keys, modifiers, displayString))
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));
            if (!ModifierKeysConverter.IsDefinedModifierKeys(modifiers))
                throw new InvalidEnumArgumentException(nameof(modifiers), (int)modifiers, typeof(ModifierKeys));

            _keys = new List<Key>(keys);
            if (_keys.Count == 0)
                throw new ArgumentException("A multi key gesture needs to have at least one key.");
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// When overridden in a derived class, determines whether the specified
        /// <see cref="InputGesture"/> matches the input associated with the specified
        /// <see cref="InputEventArgs"/> object.
        /// </summary>
        /// <param name="targetElement">The target of the command.</param>
        /// <param name="inputEventArgs">The input event data to compare this gesture to.</param>
        /// <returns>
        /// <see langword="true"/> if the gesture matches the input; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (_currentTarget != null)
            {
                if (_currentTarget == targetElement)
                {
                    if (_reportMatch)
                    {
                        // We have already checked the key (see OnPreviewKeyDown) and found a match.
                        Reset();
                        return true;
                    }
                    else
                    {
                        // We have already installed an event handler: 
                        // The check is happening in OnPreviewKeyDown. We can exit here.
                        return false;
                    }
                }
                else
                {
                    // We are already within a multi-key gesture. But suddenly the targetElement has changed.
                    Reset();
                }
            }

            KeyEventArgs eventArgs = inputEventArgs as KeyEventArgs;
            if ((eventArgs == null) || !MultiKeyGestureConverter.IsDefinedKey(eventArgs.Key))
                return false;

            // ----- Check first key stroke

            if (Keyboard.Modifiers != Modifiers)
            {
                // Wrong modifiers.
                return false;
            }

            if (eventArgs.Key != _keys[0])
            {
                // Wrong key.
                return false;
            }

            // First key matches!
            _lastKeyPress = eventArgs.Timestamp;

            if (_keys.Count == 1)
            {
                // The current multi-key gesture only has one key.
                return true;
            }

            // We need to check more than one key.
            // --> Install an event handler for PreviewKeyDown.

            UIElement uiElement = targetElement as UIElement;
            if (uiElement == null)
            {
                // We currently can only check for multiple keys if the target is an UIElement.
                throw new InvalidOperationException(Invariant($"MultiKeyGesture has invalid target element ({targetElement}). The target element needs to be of type UIElement."));
            }

            _currentTarget = uiElement;
            _currentTarget.PreviewKeyDown += OnPreviewKeyDown;
            _currentKeyIndex = 1;
            return false;
        }


        private void OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if ((eventArgs == null) || !MultiKeyGestureConverter.IsDefinedKey(eventArgs.Key))
            {
                Reset();
                return;
            }

            var timeSinceLastKeyPress = TimeSpan.FromMilliseconds(eventArgs.Timestamp - _lastKeyPress);
            if (timeSinceLastKeyPress > MaximumDelayBetweenKeyPresses)
            {
                // Took too long to press next key.
                Reset();
                return;
            }

            if (ShouldIgnoreKey(eventArgs.Key))
            {
                // When a modifier key is set, we ignore certain keys (LeftCtrl, RightCtrl, ...).
                // I.e. the user can release/press LeftCtrl multiple times. We only track the relevant keys.
                eventArgs.Handled = true;
                return;
            }

            if (eventArgs.Key == Key.Escape)
            {
                // Cancel multi-key gesture.
                eventArgs.Handled = true;
                Reset();
                return;
            }

            if (Keyboard.Modifiers != Modifiers)
            {
                // Wrong modifiers.
                Reset();
                return;
            }

            Debug.Assert(_currentKeyIndex > 0, "At least one key of MultiKeyGesture should have been pressed.");

            if (_keys[_currentKeyIndex] != eventArgs.Key)
            {
                // Wrong key pressed.
                Reset();
                return;
            }

            _currentKeyIndex++;

            if (_currentKeyIndex < _keys.Count)
            {
                // We are still in the multi-key gesture, waiting for next key stroke.
                _lastKeyPress = eventArgs.Timestamp;
                eventArgs.Handled = true;
                return;
            }

            // Match complete!
            // Report a success on the next call of Matches().
            _reportMatch = true;

            // Set eventArgs.Handled to prevent other key gestures from firing.
            // For example: If we have detected "Ctrl+K, Ctrl+X" then we do not want "Ctrl+X" (Cut)
            // to be fired.
            eventArgs.Handled = true;

            // However, now we have a problem! eventArgs.Handled is set, so the KeyDown event is
            // canceled and Matches() never gets called. To solve this problem, we re-raise a dummy
            // KeyDown event with Key.None. This way Matches() gets called again.
            // [MartinG: Sorry, for this little "hack", but this is the only way, we can prevent
            // conflicting key gestures such as "Ctrl+X" (Cut) from firing.]
            _currentTarget.Dispatcher.BeginInvoke(
              DispatcherPriority.Input,
              new Action(
                () => _currentTarget.RaiseEvent(
                      new KeyEventArgs(
                        eventArgs.KeyboardDevice,
                        eventArgs.InputSource,
                        eventArgs.Timestamp,
                        Key.None) { RoutedEvent = Keyboard.KeyDownEvent }))
              );
        }


        /// <summary>
        /// Resets the multi-key gesture.
        /// </summary>
        private void Reset()
        {
            // Remove PreviewKeyDown event handler.
            if (_currentTarget != null)
            {
                _currentTarget.PreviewKeyDown -= OnPreviewKeyDown;
                _currentTarget = null;
            }

            _currentKeyIndex = 0;
            _reportMatch = false;
        }


        /// <summary>
        /// Checks whether this key should be ignored.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// <see langword="true"/> is the key should be ignored; otherwise, <see langword="false"/>.
        /// </returns>
        private bool ShouldIgnoreKey(Key key)
        {
            if ((Modifiers & (ModifierKeys.Windows | ModifierKeys.Control | ModifierKeys.Alt)) != ModifierKeys.None)
            {
                // The multi-key gesture uses modifier keys. In this case we want to ignore key
                // events for these keys. That means, the user can press/release LeftCtrl as often
                // as she wants. We only care for the relevant keys.
                switch (key)
                {
                    case Key.LWin:
                    case Key.RWin:
                    case Key.LeftCtrl:
                    case Key.RightCtrl:
                    case Key.LeftAlt:
                    case Key.RightAlt:
                        return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Gets the display string.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <param name="modifiers">The modifiers.</param>
        /// <param name="displayString">The user-defined display string.</param>
        /// <returns>The display string.</returns>
        private static string GetDisplayString(IEnumerable<Key> keys, ModifierKeys modifiers, string displayString)
        {
            // This static method is another little "hack": We want to set KeyGesture.DisplayString
            // to make sure that the key gesture, such as "Ctrl+K, Ctrl+D" is automatically set for
            // MenuItems. However, KeyGesture.DisplayString is readonly and can only be set in the
            // constructor. Therefore, we call this method immediately before the base constructor.

            if (!string.IsNullOrEmpty(displayString))
            {
                // Use the user-defined display string.
                return displayString;
            }

            // Automatically create the display string.
            return MultiKeyGestureConverter.GetDisplayStringForCulture(keys, modifiers, null, CultureInfo.CurrentCulture);
        }
        #endregion
    }
}
