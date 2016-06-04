// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Defines a line segment.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This is a lightweight structure. To define a line segment shape for 
  /// <see cref="IGeometricObject"/> use <see cref="LineSegmentShape"/>.
  /// </para>
  /// <para>
  /// Two <see cref="LineSegment"/>s are considered as equal if there end points are equal and in
  /// the same order. (Line segments are not equal if <see cref="Start"/> and <see cref="End"/> are
  /// swapped.)
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public struct LineSegment : IEquatable<LineSegment>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The start point.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3F Start;


    /// <summary>
    /// The end point.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3F End;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the length.
    /// </summary>
    /// <value>The length.</value>
    public float Length
    {
      get { return (End - Start).Length; }
    }


    /// <summary>
    /// Gets the squared length.
    /// </summary>
    /// <value>The squared length.</value>
    public float LengthSquared
    {
      get { return (End - Start).LengthSquared; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of <see cref="LineSegment"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of <see cref="LineSegment"/> from two points.
    /// </summary>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    public LineSegment(Vector3F start, Vector3F end)
    {
      Start = start;
      End = end;
    }


    /// <summary>
    /// Initializes a new instance of <see cref="LineSegment"/> from a 
    /// <see cref="LineSegmentShape"/>.
    /// </summary>
    /// <param name="lineSegmentShape">
    /// The line segment from which properties are copied.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="lineSegmentShape"/> is <see langword="null"/>.
    /// </exception>
    public LineSegment(LineSegmentShape lineSegmentShape)
    {
      if (lineSegmentShape == null)
        throw new ArgumentNullException("lineSegmentShape");

      Start = lineSegmentShape.Start;
      End = lineSegmentShape.End;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //-------------------------------------------------------------- 

    #region ----- Equality -----

    /// <overloads>
    /// <summary>
    /// Determines whether the specified <see cref="Object"/> is equal to the current 
    /// <see cref="Object"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified <see cref="Object"/> is equal to the current 
    /// <see cref="Object"/>.
    /// </summary>
    /// <param name="obj">
    /// The <see cref="Object"/> to compare with the current <see cref="Object"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object"/> is equal to the current 
    /// <see cref="Object"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is LineSegment && Equals((LineSegment)obj);
    }


    /// <summary>
    /// Determines whether the specified <see cref="LineSegment"/> is equal to the current 
    /// <see cref="LineSegment"/>.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(LineSegment other)
    {
      return Start == other.Start && End == other.End;
    }


    /// <summary>
    /// Tests if two <see cref="LineSegment"/>s are equal.
    /// </summary>
    /// <param name="lineSegment1">The first <see cref="LineSegment"/>.</param>
    /// <param name="lineSegment2">The second <see cref="LineSegment"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="LineSegment"/>s are equal; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator ==(LineSegment lineSegment1, LineSegment lineSegment2)
    {
      return lineSegment1.Start == lineSegment2.Start
             && lineSegment1.End == lineSegment2.End;
    }


    /// <summary>
    /// Tests if two <see cref="LineSegment"/>s are different.
    /// </summary>
    /// <param name="lineSegment1">The first <see cref="LineSegment"/>.</param>
    /// <param name="lineSegment2">The second <see cref="LineSegment"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="LineSegment"/>s are different; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator !=(LineSegment lineSegment1, LineSegment lineSegment2)
    {
      return lineSegment1.Start != lineSegment2.Start
             || lineSegment1.End != lineSegment2.End;
    }
    #endregion


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures
    /// like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        int hashCode = Start.GetHashCode();
        hashCode = (hashCode * 397) ^ End.GetHashCode();
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "LineSegment {{ Start = {0}, End = {1} }}", Start, End);
    }
    #endregion
  }
}
