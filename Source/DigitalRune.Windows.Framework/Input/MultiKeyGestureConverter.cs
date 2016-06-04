// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Converts a <see cref="MultiKeyGesture"/> object to and from other types.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public class MultiKeyGestureConverter : TypeConverter
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private const char DisplayStringSeparator = ':';
        private const char KeysSeparator = ',';
        private const char ModifiersDelimiter = '+';
        private static readonly KeyConverter KeyConverter = new KeyConverter();
        private static readonly ModifierKeysConverter ModifierKeysConverter = new ModifierKeysConverter();
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of
        /// this converter, using the specified context.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="sourceType">
        /// A <see cref="Type"/> that represents the type you want to convert from.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this converter can perform the conversion; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return (sourceType == typeof(string));
        }


        /// <summary>
        /// Returns whether this converter can convert the object to the specified type, using the
        /// specified context.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="destinationType">
        /// A <see cref="Type"/> that represents the type you want to convert to.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this converter can perform the conversion; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (context != null && context.Instance != null)
                {
                    MultiKeyGesture instance = context.Instance as MultiKeyGesture;
                    if (instance != null)
                    {
                        if (!ModifierKeysConverter.IsDefinedModifierKeys(instance.Modifiers))
                            return false;

                        foreach (Key key in instance.Keys)
                        {
                            if (!IsDefinedKey(key))
                                return false;
                        }

                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Converts the given value object from the specified type, using the specified context and
        /// culture information.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="culture">
        /// A <see cref="CultureInfo"/>. If <see langword="null"/> is passed, the current culture is
        /// assumed.
        /// </param>
        /// <param name="value">The <see cref="Object"/> to convert.</param>
        /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
        /// <exception cref="NotSupportedException">
        /// The conversion cannot be performed.
        /// </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string s = value as string;
            if (s != null)
            {
                // Split "Ctrl+Shift+A, Ctrl+Shift+B, Ctrl+Shift+C:DisplayString" at ':' and get DisplayString.
                string[] keyStrokesAndDisplayString = s.Split(DisplayStringSeparator);
                string displayString = string.Empty;
                if (keyStrokesAndDisplayString.Length > 1)
                    displayString = keyStrokesAndDisplayString[1].Trim();

                // Split "Ctrl+Shift+A, Ctrl+Shift+B, Ctrl+Shift+C" at ','.
                string[] keyStrokes = keyStrokesAndDisplayString[0].Split(KeysSeparator);

                // First key includes modifier "Ctrl+A" 
                var keys = new List<Key>();
                ModifierKeys modifiers;
                Key key;
                GetModifiersAndKey(keyStrokes[0], out modifiers, out key);
                keys.Add(key);
                for (int i = 1; i < keyStrokes.Length; i++)
                {
                    // Get remaining key strokes
                    ModifierKeys otherModifiers;
                    GetModifiersAndKey(keyStrokes[i], out otherModifiers, out key);
                    if (otherModifiers != modifiers)
                    {
                        // All keys need to have the same modifiers.
                        throw GetConvertFromException(s);
                    }

                    keys.Add(key);
                }

                if (keys.Count == 0)
                    keys.Add(Key.None);

                return new MultiKeyGesture(keys, modifiers, displayString);
            }

            return base.ConvertFrom(context, culture, value);
        }


        private static void GetModifiersAndKey(string keyStroke, out ModifierKeys modifiers, out Key key)
        {
            if (string.IsNullOrEmpty(keyStroke))
            {
                modifiers = ModifierKeys.None;
                key = Key.None;
                return;
            }

            int index = keyStroke.LastIndexOf(ModifiersDelimiter);
            if (index == -1)
            {
                modifiers = ModifierKeys.None;
                key = (Key)KeyConverter.ConvertFrom(keyStroke);
            }
            else
            {
                string modifierString = keyStroke.Substring(0, index).Trim();
                string keyString = keyStroke.Substring(index + 1).Trim();
                modifiers = (ModifierKeys)ModifierKeysConverter.ConvertFrom(modifierString);
                key = (Key)KeyConverter.ConvertFrom(keyString);
            }
        }


        /// <summary>
        /// Converts the given value object to the specified type, using the specified context and
        /// culture information.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="culture">
        /// A <see cref="CultureInfo"/>. If null is passed, the current culture is assumed.
        /// </param>
        /// <param name="value">The <see cref="Object"/> to convert.</param>
        /// <param name="destinationType">
        /// The <see cref="Type"/> to convert the <paramref name="value"/> parameter to.
        /// </param>
        /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="destinationType"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The conversion cannot be performed.
        /// </exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
                throw new ArgumentNullException(nameof(destinationType));

            if (destinationType == typeof(string))
            {
                if (value == null)
                    return string.Empty;

                var gesture = value as MultiKeyGesture;
                if (gesture != null)
                {
                    string result = GetDisplayStringForCulture(gesture.Keys, gesture.Modifiers, context, culture);

                    if (!string.IsNullOrEmpty(gesture.DisplayString) && gesture.DisplayString != result)
                    {
                        // Append user-defined display string.
                        result += DisplayStringSeparator + gesture.DisplayString;
                    }

                    return result;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }


        internal static bool IsDefinedKey(Key key)
        {
            return ((key >= Key.None) && (key <= Key.OemClear));
        }


        internal static string GetDisplayStringForCulture(IEnumerable<Key> keys, ModifierKeys modifiers, ITypeDescriptorContext context, CultureInfo culture)
        {
            if (keys == null)
                throw new ArgumentNullException(nameof(keys));

            int numberOfKeys = keys.Count();
            if (numberOfKeys == 0 || (numberOfKeys == 1 && keys.First() == Key.None))
                return string.Empty;

            var result = new StringBuilder();
            string firstKey = KeyConverter.ConvertTo(context, culture, keys.First(), typeof(string)) as string;
            if (!string.IsNullOrEmpty(firstKey))
            {
                string modifiersString = (string)ModifierKeysConverter.ConvertTo(context, culture, modifiers, typeof(string));
                bool hasModifiers = !string.IsNullOrEmpty(modifiersString);
                if (hasModifiers)
                {
                    result.Append(modifiersString);     // result = "Ctrl+Shift"
                    result.Append(ModifiersDelimiter);  // result = "Ctrl+Shift+"
                }

                result.Append(firstKey);                // result = "Ctrl+Shift+A"

                for (int i = 1; i < numberOfKeys; i++)
                {
                    string key = KeyConverter.ConvertTo(context, culture, keys.ElementAt(i), typeof(string)) as string;
                    if (!string.IsNullOrEmpty(key))
                    {
                        result.Append(KeysSeparator);   // result = "Ctrl+Shift+A,"
                        result.Append(' ');             // result = "Ctrl+Shift+A, "

                        if (hasModifiers)
                        {
                            result.Append(modifiersString);     // result = "Ctrl+Shift+A, Ctrl+Shift"
                            result.Append(ModifiersDelimiter);  // result = "Ctrl+Shift+A, Ctrl+Shift+"
                        }

                        result.Append(key);             // result = "Ctrl+Shift+A, Ctrl+Shift+B"
                    }
                }
            }

            return result.ToString();
        }
        #endregion
    }
}
