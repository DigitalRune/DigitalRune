// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !NETFX_CORE && !PORTABLE

using System;
using System.ComponentModel;
using System.Globalization;


namespace DigitalRune.Mathematics.Algebra.Design
{
  /// <summary>
  /// Converts a <see cref="Vector2D"/> to and from string representation.
  /// </summary>
  public class Vector2DConverter 
#if !SILVERLIGHT && !WP7 && !WP8 && !XBOX
    : ExpandableObjectConverter
#else
    : TypeConverter
#endif
  {
    /// <summary>
    /// Returns whether this converter can convert an object of the given type to the type of this
    /// converter, using the specified context.
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
      if (sourceType == typeof(string))
        return true;

      return base.CanConvertFrom(context, sourceType);
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
        return true;

      return base.CanConvertTo(context, destinationType);
    }


    /// <summary>
    /// Converts the given object to the type of this converter, using the specified context and
    /// culture information. 
    /// </summary>
    /// <param name="context">
    /// An <see cref="ITypeDescriptorContext"/> that provides a format context. 
    /// </param>
    /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
    /// <param name="value">The <see cref="Object"/> to convert.</param>
    /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      string s = value as string;
      if (s != null)
        return Vector2D.Parse(s, culture);

      return base.ConvertFrom(context, culture, value);
    }


    /// <summary>
    /// Converts the given value object to the specified type, using the specified context and
    /// culture information. 
    /// </summary>
    /// <param name="context">
    /// An <see cref="ITypeDescriptorContext"/> that provides a format context. 
    /// </param>
    /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
    /// <param name="value">The <see cref="Object"/> to convert.</param>
    /// <param name="destinationType">
    /// A <see cref="Type"/> that represents the type you want to convert to. 
    /// </param>
    /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
      if (destinationType == typeof(string) && value is Vector2D)
        return ((Vector2D)value).ToString(culture);

      return base.ConvertTo(context, culture, value, destinationType);
    }
  }
}
#endif
