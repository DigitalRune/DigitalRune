// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace DigitalRune.Game.UI
{
  /// <summary>
  /// Represents a 2-dimensional rectangle (single precision).
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  public struct RectangleF : IEquatable<RectangleF>
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// An empty rectangle (all values set to zero).
    /// </summary>
    public static readonly RectangleF Empty = new RectangleF(0.0f, 0.0f, 0.0f, 0.0f);
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The x-coordinate of the rectangle. (Same as <see cref="Left"/>.)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float X;


    /// <summary>
    /// The y-coordinate of the rectangle. (Same as <see cref="Top"/>.)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float Y;


    /// <summary>
    /// The width of the rectangle.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public float Width;


    /// <summary>
    /// The height of the rectangle.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public float Height;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the x-coordinate of the left side of the rectangle.
    /// </summary>
    /// <value>The x-coordinate of the left side of the rectangle.</value>
    public float Left
    {
      get { return X; }
    }


    /// <summary>
    /// Gets the x-coordinate of the right side of the rectangle.
    /// </summary>
    /// <value>The x-coordinate of the right side of the rectangle.</value>
    public float Right
    {
      get { return X + Width; }
    }


    /// <summary>
    /// Gets the y-coordinate of the top side of the rectangle.
    /// </summary>
    /// <value>The y-coordinate of the top side of the rectangle.</value>
    public float Top
    {
      get { return Y; }
    }


    /// <summary>
    /// Gets the y-coordinate of the bottom side of the rectangle.
    /// </summary>
    /// <value>The y-coordinate of the bottom side of the rectangle.</value>
    public float Bottom
    {
      get { return Y + Height; }
    }


    /// <summary>
    /// Gets or sets the position of the upper, left corner of the rectangle.
    /// </summary>
    /// <value>The position of the upper, left corner of the rectangle.</value>
    public Vector2F Location
    {
      get { return new Vector2F(X, Y); }
      set
      {
        X = value.X;
        Y = value.Y;
      }
    }


    /// <summary>
    /// Gets or sets the size (width, height) of the rectangle.
    /// </summary>
    /// <value>The size (width, height) of the rectangle.</value>
    public Vector2F Size
    {
      get { return new Vector2F(Width, Height);}
      set 
      { 
        Width = value.X;
        Height = value.Y;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleF"/> struct.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleF"/> struct with the given values.
    /// </summary>
    /// <param name="x">The x-coordinate of the rectangle.</param>
    /// <param name="y">The y-coordinate of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public RectangleF(float x, float y, float width, float height)
    {
      X = x;
      Y = y;
      Width = width;
      Height = height;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RectangleF"/> struct from a 
    /// <see cref="Rectangle"/>.
    /// </summary>
    /// <param name="rectangle">The rectangle.</param>
    public RectangleF(Rectangle rectangle)
    {
      X = rectangle.X;
      Y = rectangle.Y;
      Width = rectangle.Width;
      Height = rectangle.Height;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>
    /// A 32-bit signed integer that is the hash code for this instance.
    /// </returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        int hashCode = X.GetHashCode();
        hashCode = (hashCode * 397) ^ Y.GetHashCode();
        hashCode = (hashCode * 397) ^ Width.GetHashCode();
        hashCode = (hashCode * 397) ^ Height.GetHashCode();
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <overloads>
    /// <summary>
    /// Indicates whether the current object is equal to another object.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <param name="obj">Another object to compare to.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="obj"/> and this instance are the same type and
    /// represent the same value; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is RectangleF && this == (RectangleF)obj;
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(RectangleF other)
    {
      return this == other;
    }


    /// <summary>
    /// Compares two <see cref="RectangleF"/>s to determine whether they are the same.
    /// </summary>
    /// <param name="rectangle1">The first rectangle.</param>
    /// <param name="rectangle2">The second rectangle.</param>
    /// <returns>
    /// <see langword="true"/> if the rectangles are equal; otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator ==(RectangleF rectangle1, RectangleF rectangle2)
    {
      return rectangle1.X == rectangle2.X
             && rectangle1.Y == rectangle2.Y
             && rectangle1.Width == rectangle2.Width
             && rectangle1.Height == rectangle2.Height;
    }


    /// <summary>
    /// Compares two <see cref="RectangleF"/>s to determine whether they are the different.
    /// </summary>
    /// <param name="rectangle1">The first rectangle.</param>
    /// <param name="rectangle2">The second rectangle.</param>
    /// <returns>
    /// <see langword="true"/> if the rectangles are different; otherwise <see langword="false"/>.
    /// </returns>
    public static bool operator !=(RectangleF rectangle1, RectangleF rectangle2)
    {
      return !(rectangle1 == rectangle2);
    }


    /// <overloads>
    /// <summary>
    /// Determines whether the rectangle contains a point or rectangle.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether rectangle contains the specified point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <returns>
    /// <see langword="true"/> if the rectangle contains the point; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Contains(Vector2F point)
    {
      return X <= point.X && point.X <= Right
             && Y <= point.Y && point.Y <= Bottom;
    }


    /// <summary>
    /// Determines whether rectangle contains the specified rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <returns>
    /// <see langword="true"/> if this rectangle contains <paramref name="rectangle"/>; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Contains(RectangleF rectangle)
    {
      return X <= rectangle.X && rectangle.Right <= Right
             && Y <= rectangle.Y && rectangle.Bottom <= Bottom;
    }


    /// <summary>
    /// Determines whether this rectangle intersects with the specified rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle.</param>
    /// <returns>
    /// <see langword="true"/> if the rectangle intersect; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Intersects(RectangleF rectangle)
    {
      return X < rectangle.Right && rectangle.X < Right
             && Y < rectangle.Bottom && rectangle.Y < Bottom;
    }


    /// <summary>
    /// Creates a rectangle that contains the overlap between the given rectangles.
    /// </summary>
    /// <param name="rectangle1">The first rectangle.</param>
    /// <param name="rectangle2">The second rectangle.</param>
    /// <returns>
    /// The overlap between <paramref name="rectangle1"/> and <paramref name="rectangle2"/>.
    /// </returns>
    public static RectangleF Intersect(RectangleF rectangle1, RectangleF rectangle2)
    {
      float left = Math.Max(rectangle1.X, rectangle2.X);
      float top = Math.Max(rectangle1.Y, rectangle2.Y);
      float right = Math.Min(rectangle1.Right, rectangle2.Right);
      float bottom = Math.Min(rectangle1.Bottom, rectangle2.Bottom);

      if (left < right && top < bottom)
        return new RectangleF(left, top, right - left, bottom - top);

      return Empty;
    }


    /// <summary>
    /// Creates the smallest rectangle that contains the given rectangles.
    /// </summary>
    /// <param name="rectangle1">The first rectangle.</param>
    /// <param name="rectangle2">The second rectangle.</param>
    /// <returns>
    /// The smallest rectangle that contains <paramref name="rectangle1"/> and 
    /// <paramref name="rectangle2"/>.
    /// </returns>
    public static RectangleF Union(RectangleF rectangle1, RectangleF rectangle2)
    {
      float left = Math.Min(rectangle1.X, rectangle2.X);
      float top = Math.Min(rectangle1.Y, rectangle2.Y);
      float right = Math.Max(rectangle1.Right, rectangle2.Right);
      float bottom = Math.Max(rectangle1.Bottom, rectangle2.Bottom);

      return new RectangleF(left, top, right - left, bottom - top);
    }


    /// <summary>
    /// Converts the <see cref="RectangleF"/> (floating-point, single precision) to a 
    /// <see cref="Rectangle"/> (integer precision).
    /// </summary>
    /// <param name="round">
    /// If set to <see langword="true"/> the values will be rounded; otherwise, the decimal part
    /// will be truncated.</param>
    /// <returns>A <see cref="Rectangle"/>.</returns>
    public Rectangle ToRectangle(bool round)
    {
      if (round)
      {
        return new Rectangle(
          (int)(X + 0.5f),
          (int)(Y + 0.5f),
          (int)(Width + 0.5f), 
          (int)(Height + 0.5f));
      }

      return new Rectangle((int)X, (int)Y, (int)Width, (int)Height);
    }


    /// <overloads>
    /// <summary>
    /// Returns the string representation of this rectangle.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Returns the string representation of this rectangle.
    /// </summary>
    /// <returns>The string representation of this rectangle.</returns>
    public override string ToString()
    {
      return ToString(CultureInfo.InvariantCulture);
    }


    /// <summary>
    /// Returns the string representation of this rectangle using the specified culture-specific
    /// format information.
    /// </summary>
    /// <param name="provider">
    /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information
    /// </param>
    /// <returns>The string representation of this rectangle.</returns>
    public string ToString(IFormatProvider provider)
    {
      return string.Format(provider, "RectangleF {{ X = {0}, Y = {1}, Width = {2}, Height = {3} }}", X, Y, Width, Height);
    }  
    #endregion
  }
}
