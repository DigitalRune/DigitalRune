// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace DigitalRune.Game.UI.Rendering
{
  /// <summary>
  /// Provides methods that help to load and parse a UI theme.
  /// </summary>
  public static class ThemeHelper
  {
    // Accepted list formats are:
    //   "R,G,B,A"
    //   "R;G;B;A"
    //   "R G B A"
    private static readonly char[] ListSeparators = { ',', ';', ' ' };


    /// <summary>
    /// Converts the specified string representation of a horizontal alignment to its 
    /// <see cref="HorizontalAlignment"/> equivalent, or throws an exception if the string cannot be
    /// converted to a <see cref="HorizontalAlignment"/>.
    /// </summary>
    /// <param name="value">
    /// The value. If this value is <see langword="null"/> or an empty string, 
    /// <see cref="HorizontalAlignment.Left"/> is returned as the default value.
    /// </param>
    /// <returns>The <see cref="HorizontalAlignment"/>.</returns>
    /// <exception cref="FormatException">
    /// Cannot convert <paramref name="value"/> to <see cref="HorizontalAlignment"/>.
    /// </exception>
    public static HorizontalAlignment ParseHorizontalAlignment(string value)
    {
      if (string.IsNullOrEmpty(value))
        return HorizontalAlignment.Left;

      switch (value.Trim().ToUpperInvariant())
      {
        case "LEFT": return HorizontalAlignment.Left;
        case "CENTER": return HorizontalAlignment.Center;
        case "RIGHT": return HorizontalAlignment.Right;
        case "STRETCH": return HorizontalAlignment.Stretch;
        default:
          string message = string.Format(CultureInfo.InvariantCulture, "Could not parse horizontal alignment: '{0}'", value);
          throw new FormatException(message);
      }
    }


    /// <summary>
    /// Converts the specified string representation of a vertical alignment to its 
    /// <see cref="VerticalAlignment"/> equivalent, or throws an exception if the string cannot be
    /// converted to a <see cref="VerticalAlignment"/>.
    /// </summary>
    /// <param name="value">
    /// The value. If this value is <see langword="null"/> or an empty string, 
    /// <see cref="VerticalAlignment.Top"/> is returned as the default value.
    /// </param>
    /// <returns>The <see cref="VerticalAlignment"/>.</returns>
    /// <exception cref="FormatException">
    /// Cannot convert <paramref name="value"/> to <see cref="VerticalAlignment"/>.
    /// </exception>
    public static VerticalAlignment ParseVerticalAlignment(string value)
    {
      if (string.IsNullOrEmpty(value))
        return VerticalAlignment.Top;

      switch (value.Trim().ToUpperInvariant())
      {
        case "TOP": return VerticalAlignment.Top;
        case "CENTER": return VerticalAlignment.Center;
        case "BOTTOM": return VerticalAlignment.Bottom;
        case "STRETCH": return VerticalAlignment.Stretch;
        default:
          string message = string.Format(CultureInfo.InvariantCulture, "Could not parse vertical alignment: '{0}'", value);
          throw new FormatException(message);
      }
    }


    /// <summary>
    /// Converts the specified string representation of a tile mode to its 
    /// <see cref="TileMode"/> equivalent, or throws an exception if the string cannot be
    /// converted to a <see cref="TileMode"/>.
    /// </summary>
    /// <param name="value">
    /// The value. If this value is <see langword="null"/> or an empty string, 
    /// <see cref="TileMode.None"/> is returned as the default value.
    /// </param>
    /// <returns>The <see cref="TileMode"/>.</returns>
    /// <exception cref="FormatException">
    /// Cannot convert <paramref name="value"/> to <see cref="TileMode"/>.
    /// </exception>
    public static TileMode ParseTileMode(string value)
    {
      if (string.IsNullOrEmpty(value))
        return TileMode.None;

      switch (value.Trim().ToUpperInvariant())
      {
        case "NONE":
          return TileMode.None;
        case "TILEX":
          return TileMode.TileX;
        case "TILEY":
          return TileMode.TileY;
        case "TILEXY":
          return TileMode.TileXY;
        default:
          string message = string.Format(CultureInfo.InvariantCulture, "Could not parse tile mode: '{0}'", value);
          throw new FormatException(message);
      }
    }


    /// <summary>
    /// Converts the specified string representation of a color to its <see cref="Color"/> 
    /// equivalent, or throws an exception if the string cannot be converted to a 
    /// <see cref="Color"/>.
    /// </summary>
    /// <param name="value">
    /// The value. If this value is <see langword="null"/> or an empty string, the 
    /// <paramref name="defaultColor"/> is returned.
    /// </param>
    /// <param name="defaultColor">The default color that is used for empty strings.</param>
    /// <returns>The <see cref="Color"/>.</returns>
    /// <exception cref="FormatException">
    /// Cannot convert <paramref name="value"/> to <see cref="Color"/>.
    /// </exception>
    public static Color ParseColor(string value, Color defaultColor)
    {
      if (string.IsNullOrEmpty(value))
        return defaultColor;

      try
      {
        Vector4F vector = ParseVector4F(value);
        return new Color((byte)vector.X, (byte)vector.Y, (byte)vector.Z, (byte)vector.W);
      }
      catch (Exception exception)
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Could not parse color: '{0}'", value);
        throw new FormatException(message, exception);
      }
    }


    /// <summary>
    /// Converts the specified string representation of a 2-dimensional vector to its 
    /// <see cref="Vector2F"/> equivalent, or throws an exception if the string cannot be
    /// converted to a <see cref="Vector2F"/>.
    /// </summary>
    /// <param name="value">
    /// The value. If this value is <see langword="null"/> or an empty string, 
    /// <see cref="Vector2F.Zero"/> is returned as the default value.
    /// </param>
    /// <returns>The <see cref="Vector2F"/>.</returns>
    /// <exception cref="FormatException">
    /// Cannot convert <paramref name="value"/> to <see cref="Vector2F"/>.
    /// </exception>
    public static Vector2F ParseVector2F(string value)
    {
      if (string.IsNullOrEmpty(value))
        return Vector2F.Zero;

      var values = value.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries);
      if (values.Length != 2)
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Could not parse 2-element vector: '{0}'", value);
        throw new FormatException(message);
      }

      Vector2F result;
      result.X = float.Parse(values[0], CultureInfo.InvariantCulture);
      result.Y = float.Parse(values[1], CultureInfo.InvariantCulture);
      return result;
    }


    /// <summary>
    /// Converts the specified string representation of a 3-dimensional vector to its 
    /// <see cref="Vector3F"/> equivalent, or throws an exception if the string cannot be
    /// converted to a <see cref="Vector3F"/>.
    /// </summary>
    /// <param name="value">
    /// The value. If this value is <see langword="null"/> or an empty string, 
    /// <see cref="Vector3F.Zero"/> is returned as the default value.
    /// </param>
    /// <returns>The <see cref="Vector3F"/>.</returns>
    /// <exception cref="FormatException">
    /// Cannot convert <paramref name="value"/> to <see cref="Vector3F"/>.
    /// </exception>
    public static Vector3F ParseVector3F(string value)
    {
      if (string.IsNullOrEmpty(value))
        return Vector3F.Zero;

      var values = value.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries);
      if (values.Length != 3)
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Could not parse 3-element vector: '{0}'", value);
        throw new FormatException(message);
      }

      Vector3F result;
      result.X = float.Parse(values[0], CultureInfo.InvariantCulture);
      result.Y = float.Parse(values[1], CultureInfo.InvariantCulture);
      result.Z = float.Parse(values[2], CultureInfo.InvariantCulture);
      return result;
    }


    /// <summary>
    /// Converts the specified string representation of a 4-dimensional vector to its 
    /// <see cref="Vector4F"/> equivalent, or throws an exception if the string cannot be
    /// converted to a <see cref="Vector4F"/>.
    /// </summary>
    /// <param name="value">
    /// The value. If this value is <see langword="null"/> or an empty string, 
    /// <see cref="Vector4F.Zero"/> is returned as the default value.
    /// </param>
    /// <returns>The <see cref="Vector4F"/>.</returns>
    /// <exception cref="FormatException">
    /// Cannot convert <paramref name="value"/> to <see cref="Vector4F"/>.
    /// </exception>
    public static Vector4F ParseVector4F(string value)
    {
      if (string.IsNullOrEmpty(value))
        return Vector4F.Zero;

      var values = value.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries);
      if (values.Length != 4)
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Could not parse 4-element vector: '{0}'", value);
        throw new FormatException(message);
      }

      Vector4F result;
      result.X = float.Parse(values[0], CultureInfo.InvariantCulture);
      result.Y = float.Parse(values[1], CultureInfo.InvariantCulture);
      result.Z = float.Parse(values[2], CultureInfo.InvariantCulture);
      result.W = float.Parse(values[3], CultureInfo.InvariantCulture);
      return result;
    }


    /// <summary>
    /// Converts the specified string representation of a rectangle to its <see cref="Rectangle"/> 
    /// equivalent, or throws an exception if the string cannot be converted to a 
    /// <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="value">
    /// The value. If this value is <see langword="null"/> or an empty string, a rectangle is 
    /// returned where all values are 0.
    /// </param>
    /// <returns>
    /// The <see cref="Rectangle"/>.
    /// </returns>
    /// <exception cref="FormatException">
    /// Cannot convert <paramref name="value"/> to <see cref="Rectangle"/>.
    /// </exception>
    public static Rectangle ParseRectangle(string value)
    {
      try
      {
        Vector4F vector = ParseVector4F(value);
        return new Rectangle((int)vector.X, (int)vector.Y, (int)vector.Z, (int)vector.W);
      }
      catch (Exception exception)
      {
        string message = string.Format(CultureInfo.InvariantCulture, "Could not parse rectangle: '{0}'", value);
        throw new FormatException(message, exception);
      }
    }
  }
}
