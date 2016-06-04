// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Provides helper methods for parsing XML.
  /// </summary>
  internal static class XmlHelper
  {
    // Accepted list formats are:
    //   "R,G,B,A"
    //   "R;G;B;A"
    //   "R G B A"
    private static readonly char[] ListSeparators = { ',', ';', ' ' };


    /// <summary>
    /// Gets the mandatory attribute from the specified XML element.
    /// </summary>
    /// <param name="element">The XML element.</param>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="identity">The content identity.</param>
    /// <returns>The attribute value.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="element"/> or <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    public static string GetMandatoryAttribute(this XElement element, string name, ContentIdentity identity)
    {
      if (element == null)
        throw new ArgumentNullException("element");
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("The attribute name must not be empty.", "name");

      var attribute = element.Attribute(name);
      if (attribute == null)
      {
        string message = GetExceptionMessage(element, "\"{0}\" attribute is missing.", name);
        throw new InvalidContentException(message, identity);
      }

      string s = (string)attribute;
      if (s.Length == 0)
      {
        string message = GetExceptionMessage(element, "\"{0}\" attribute must not be empty.", name);
        throw new InvalidContentException(message, identity);
      }

      return s;
    }


    /// <summary>
    /// Converts the specified <see cref="XAttribute"/> to an effect parameter value.
    /// </summary>
    /// <param name="attribute">The XML attribute to parse. Can be <see langword="null"/>.</param>
    /// <param name="defaultValue">
    /// The default value, used if <paramref name="attribute"/> is null or empty.
    /// </param>
    /// <param name="identity">The content identity.</param>
    /// <returns>The effect parameter value.</returns>
    /// <exception cref="InvalidContentException">
    /// Error parsing <paramref name="attribute"/>.
    /// </exception>
    public static object ToParameterValue(this XAttribute attribute, object defaultValue, ContentIdentity identity)
    {
      // TODO: Add support for Int32, Quaternion, Matrix.
      string value = (string)attribute;
      if (value == null)
        return defaultValue;

      try
      {
        var values = value.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries);

        // Empty value
        if (values.Length == 0)
          return defaultValue;

        if (values.Any(IsBoolean))
        {
          // Boolean
          if (values.Length == 1)
            return bool.Parse(values[0]);

          // Boolean[]
          bool[] array = new bool[values.Length];
          for (int i = 0; i < values.Length; i++)
            array[i] = bool.Parse(values[i]);

          return array;
        }
        else
        {
          // Single
          if (values.Length == 1)
            return float.Parse(values[0], CultureInfo.InvariantCulture);

          // Vector2
          if (values.Length == 2)
          {
            Vector2 result;
            result.X = float.Parse(values[0], CultureInfo.InvariantCulture);
            result.Y = float.Parse(values[1], CultureInfo.InvariantCulture);
            return result;
          }

          // Vector3
          if (values.Length == 3)
          {
            Vector3 result;
            result.X = float.Parse(values[0], CultureInfo.InvariantCulture);
            result.Y = float.Parse(values[1], CultureInfo.InvariantCulture);
            result.Z = float.Parse(values[2], CultureInfo.InvariantCulture);
            return result;
          }

          // Vector4
          if (values.Length == 4)
          {
            Vector4 result;
            result.X = float.Parse(values[0], CultureInfo.InvariantCulture);
            result.Y = float.Parse(values[1], CultureInfo.InvariantCulture);
            result.Z = float.Parse(values[2], CultureInfo.InvariantCulture);
            result.W = float.Parse(values[3], CultureInfo.InvariantCulture);
            return result;
          }
          else
          {
            // Single[]
            float[] array = new float[values.Length];
            for (int i = 0; i < values.Length; i++)
              array[i] = float.Parse(values[i], CultureInfo.InvariantCulture);

            return array;
          }
        }
      }
      catch (Exception exception)
      {
        var message = GetExceptionMessage(attribute, "Could not parse parameter value: '{0}'", value);
        throw new InvalidContentException(message, identity, exception);
      }
    }


    private static bool IsBoolean(string value)
    {
      bool dummy;
      return bool.TryParse(value, out dummy);
    }


    /// <summary>
    /// Converts the specified <see cref="XAttribute"/> to a color value.
    /// </summary>
    /// <param name="attribute">The XML attribute to parse. Can be <see langword="null"/>.</param>
    /// <param name="defaultValue">
    /// The default value, used if <paramref name="attribute"/> is null or empty.
    /// </param>
    /// <param name="identity">The content identity.</param>
    /// <returns>The color value.</returns>
    /// <exception cref="InvalidContentException">
    /// Error parsing <paramref name="attribute"/>.
    /// </exception>
    public static Color ToColor(this XAttribute attribute, Color defaultValue, ContentIdentity identity)
    {
      string value = (string)attribute;
      if (value == null)
        return defaultValue;

      try
      {
        var values = value.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries);
        if (values.Length == 3)
        {
          Color color = new Color();
          color.R = byte.Parse(values[0], CultureInfo.InvariantCulture);
          color.G = byte.Parse(values[1], CultureInfo.InvariantCulture);
          color.B = byte.Parse(values[2], CultureInfo.InvariantCulture);
          color.A = byte.MaxValue;
          return color;
        }
        else if (values.Length == 4)
        {
          Color color = new Color();
          color.R = byte.Parse(values[0], CultureInfo.InvariantCulture);
          color.G = byte.Parse(values[1], CultureInfo.InvariantCulture);
          color.B = byte.Parse(values[2], CultureInfo.InvariantCulture);
          color.A = byte.Parse(values[3], CultureInfo.InvariantCulture);
          return color;
        }
        else
        {
          var message = GetExceptionMessage(attribute, "Could not parse color value: '{0}'", value);
          throw new InvalidContentException(message, identity);
        }
      }
      catch (Exception exception)
      {
        var message = GetExceptionMessage(attribute, "Could not parse color value: '{0}'", value);
        throw new InvalidContentException(message, identity, exception);
      }
    }


    /// <summary>
    /// Converts to the specified <see cref="XAttribute"/> to a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="attribute">The XML attribute to parse. Can be <see langword="null"/>.</param>
    /// <param name="defaultValue">
    /// The default value, used if <paramref name="attribute"/> is null or empty.
    /// </param>
    /// <param name="identity">The content identity.</param>
    /// <returns>The 3D vector.</returns>
    /// <exception cref="InvalidContentException">
    /// Error parsing <paramref name="attribute"/>.
    /// </exception>
    public static Vector3 ToVector3(this XAttribute attribute, Vector3 defaultValue, ContentIdentity identity)
    {
      string value = (string)attribute;
      if (value == null)
        return defaultValue;

      try
      {
        var values = value.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries);
        if (values.Length == 3)
        {
          Vector3 result;
          result.X = float.Parse(values[0], CultureInfo.InvariantCulture);
          result.Y = float.Parse(values[1], CultureInfo.InvariantCulture);
          result.Z = float.Parse(values[2], CultureInfo.InvariantCulture);
          return result;
        }
        else
        {
          var message = GetExceptionMessage(attribute, "Could not parse 3-dimensional vector: '{0}'", value);
          throw new InvalidContentException(message, identity);
        }
      }
      catch (Exception exception)
      {
        var message = GetExceptionMessage(attribute, "Could not parse 3-dimensional vector: '{0}'", value);
        throw new InvalidContentException(message, identity, exception);
      }
    }


    /// <summary>
    /// Converts the specified <see cref="XAttribute"/> to a <see cref="DRTextureFormat"/> value.
    /// </summary>
    /// <param name="attribute">The XML attribute to parse. Can be <see langword="null"/>.</param>
    /// <param name="defaultValue">
    /// The default value, used if <paramref name="attribute"/> is null or empty.
    /// </param>
    /// <param name="identity">The content identity.</param>
    /// <returns>The texture format.</returns>
    /// <exception cref="InvalidContentException">
    /// Error parsing <paramref name="attribute"/>.
    /// </exception>
    public static DRTextureFormat ToTextureFormat(this XAttribute attribute, DRTextureFormat defaultValue, ContentIdentity identity)
    {
      string value = (string)attribute;
      if (string.IsNullOrWhiteSpace(value))
        return defaultValue;

      switch (value.Trim().ToUpperInvariant())
      {
        case "NOCHANGE":
          return DRTextureFormat.NoChange;
        case "COLOR":
          return DRTextureFormat.Color;
        case "DXT":
          return DRTextureFormat.Dxt;
        case "NORMAL":
          return DRTextureFormat.Normal;
        case "NORMALINVERTY":
          return DRTextureFormat.NormalInvertY;
        default:
          var message = GetExceptionMessage(attribute, "Could not parse texture format: '{0}'", value);
          throw new InvalidContentException(message, identity);
      }
    }


    /// <summary>
    /// Gets the message text including line info for exceptions that occur when parsing XML.
    /// </summary>
    /// <param name="attribute">The current attribute.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <returns>The exception message including line info.</returns>
    public static string GetExceptionMessage(XAttribute attribute, string format, params object[] args)
    {
      if (attribute == null)
        throw new ArgumentNullException("attribute");

      var cultureInfo = CultureInfo.InvariantCulture;
      string message = string.Format(cultureInfo, format, args);

      // Append line info, if available.
      var lineInfo = (IXmlLineInfo)attribute;
      if (lineInfo.HasLineInfo())
      {
        message += string.Format(
          cultureInfo, 
          " (Attribute: \"{0}\", Line: {1}, Position: {2})",
          attribute.Name, lineInfo.LineNumber, lineInfo.LinePosition);
      }
      else
      {
        message += string.Format(
          cultureInfo, 
          " (Attribute: \"{0}\")", 
          attribute.Name);
      }

      return message;
    }


    /// <summary>
    /// Gets the message text including line info for exceptions that occur when parsing XML.
    /// </summary>
    /// <param name="element">The current element.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">An object array that contains zero or more objects to format.</param>
    /// <returns>The exception message including line info.</returns>
    public static string GetExceptionMessage(XElement element, string format, params object[] args)
    {
      if (element == null)
        throw new ArgumentNullException("element");

      var cultureInfo = CultureInfo.InvariantCulture;
      string message = string.Format(cultureInfo, format, args);

      // Append line info, if available.
      var lineInfo = (IXmlLineInfo)element;
      if (lineInfo.HasLineInfo())
      {
        message += string.Format(
          cultureInfo, 
          " (Element: \"{0}\", Line: {1}, Position: {2})", element.Name, 
          lineInfo.LineNumber, lineInfo.LinePosition);
      }
      else
      {
        message += string.Format(
          cultureInfo, 
          " (Element: \"{0}\")", 
          element.Name);
      }

      return message;
    }
  }
}
