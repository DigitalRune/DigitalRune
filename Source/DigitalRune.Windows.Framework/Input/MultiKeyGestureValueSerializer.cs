// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Markup;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Converts instances of <see cref="String"/> to and from instances of 
    /// <see cref="MultiKeyGesture"/>. 
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public class MultiKeyGestureValueSerializer : ValueSerializer
    {
        /// <summary>
        /// Determines whether the specified <see cref="String"/> can be converted to an instance of
        /// the type that the implementation of <see cref="ValueSerializer"/> supports.
        /// </summary>
        /// <param name="value">String to evaluate for conversion.</param>
        /// <param name="context">Context information that is used for conversion.</param>
        /// <returns>
        /// <see langword="true"/> if the value can be converted; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool CanConvertFromString(string value, IValueSerializerContext context)
        {
            return true;
        }


        /// <summary>
        /// Determines whether the specified object can be converted into a <see cref="String"/>.
        /// </summary>
        /// <param name="value">The object to evaluate for conversion.</param>
        /// <param name="context">Context information that is used for conversion.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="value"/> can be converted into a 
        /// <see cref="String"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool CanConvertToString(object value, IValueSerializerContext context)
        {
            var gesture = value as MultiKeyGesture;
            return gesture != null
                   && ModifierKeysConverter.IsDefinedModifierKeys(gesture.Modifiers)
                   && MultiKeyGestureConverter.IsDefinedKey(gesture.Key);
        }


        /// <summary>
        /// Converts a <see cref="String"/> to an instance of the type that the implementation of
        /// <see cref="ValueSerializer"/> supports.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="context">Context information that is used for conversion.</param>
        /// <returns>
        /// A new instance of the type that the implementation of <see cref="ValueSerializer"/>
        /// supports based on the supplied <paramref name="value"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <paramref name="value"/> cannot be converted.
        /// </exception>
        public override object ConvertFromString(string value, IValueSerializerContext context)
        {
            var converter = TypeDescriptor.GetConverter(typeof(KeyGesture));
            if (converter != null)
                return converter.ConvertFromString(value);

            return base.ConvertFromString(value, context);
        }


        /// <summary>
        /// Converts the specified object to a <see cref="String"/>.
        /// </summary>
        /// <param name="value">The object to convert into a string.</param>
        /// <param name="context">Context information that is used for conversion.</param>
        /// <returns>
        /// A string representation of the specified object.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// <paramref name="value"/> cannot be converted.
        /// </exception>
        public override string ConvertToString(object value, IValueSerializerContext context)
        {
            var converter = TypeDescriptor.GetConverter(typeof(KeyGesture));
            if (converter != null)
                return converter.ConvertToInvariantString(value);

            return base.ConvertToString(value, context);
        }
    }
}
