// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Defines a line.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This is a lightweight structure. To define a line shape for <see cref="IGeometricObject"/>
  /// use <see cref="LineShape"/>.
  /// </para>
  /// <para>
  /// Two <see cref="Line"/>s are considered as equal if the fields <see cref="PointOnLine"/> and 
  /// <see cref="Direction"/> are equal.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public struct Line : IEquatable<Line>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// A point on the line.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnLine")]
    public Vector3F PointOnLine;


    /// <summary>
    /// The normalized direction vector.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3F Direction;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of <see cref="Line"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of <see cref="Line"/> from a point and a direction.
    /// </summary>
    /// <param name="pointOnLine">A point on the line.</param>
    /// <param name="direction">The direction. (Must be normalized.)</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnLine")]
    public Line(Vector3F pointOnLine, Vector3F direction)
    {
      PointOnLine = pointOnLine;
      Direction = direction;
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Line"/> from a <see cref="LineShape"/>.
    /// </summary>
    /// <param name="lineShape">
    /// The <see cref="LineShape"/> from which properties are copied.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="lineShape"/> is <see langword="null"/>.
    /// </exception>
    public Line(LineShape lineShape)
    {
      if (lineShape == null)
        throw new ArgumentNullException("lineShape");

      PointOnLine = lineShape.PointOnLine;
      Direction = lineShape.Direction;
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
      return obj is Line && Equals((Line)obj);
    }


    /// <summary>
    /// Determines whether the specified <see cref="Line"/> is equal to the current 
    /// <see cref="Line"/>.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(Line other)
    {
      return PointOnLine == other.PointOnLine && Direction == other.Direction;
    }


    /// <summary>
    /// Tests if two <see cref="Line"/>s are equal.
    /// </summary>
    /// <param name="line1">The first <see cref="Line"/>.</param>
    /// <param name="line2">The second <see cref="Line"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Line"/>s are equal; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Line line1, Line line2)
    {
      return line1.PointOnLine == line2.PointOnLine
             && line1.Direction == line2.Direction;
    }


    /// <summary>
    /// Tests if two <see cref="Line"/>s are different.
    /// </summary>
    /// <param name="line1">The first <see cref="Line"/>.</param>
    /// <param name="line2">The second <see cref="Line"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Line"/>s are different; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Line line1, Line line2)
    {
      return line1.PointOnLine != line2.PointOnLine
             || line1.Direction != line2.Direction;
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
        int hashCode = PointOnLine.GetHashCode();
        hashCode = (hashCode * 397) ^ Direction.GetHashCode();
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
      return String.Format(CultureInfo.InvariantCulture, "Line {{ PointOnLine = {0}, Direction = {1} }}", PointOnLine, Direction);
    }


    /// <summary>
    /// Applies a scaling to the <see cref="Line"/>.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <exception cref="NotSupportedException">
    /// <paramref name="scale"/> is a non-uniform scaling. Non-uniform scaling of lines is not 
    /// supported.
    /// </exception>
    internal void Scale(ref Vector3F scale)
    {
      if (scale.X != scale.Y || scale.Y != scale.Z)
        throw new NotSupportedException("Computing collisions for lines with non-uniform scaling is not supported.");

      Debug.Assert(Direction.IsNumericallyNormalized, "Line direction should be normalized.");

      PointOnLine *= scale.X;

      // Since we have only uniform scaling: Nothing to do for Direction.
    }


    /// <summary>
    /// Transforms the <see cref="Line"/> from local space to world space by applying a 
    /// <see cref="Pose"/>.
    /// </summary>
    /// <param name="pose">The pose (position and orientation).</param>
    internal void ToWorld(ref Pose pose)
    {
      Debug.Assert(Direction.IsNumericallyNormalized, "Line direction should be normalized.");

      PointOnLine = pose.ToWorldPosition(PointOnLine);
      Direction = pose.ToWorldDirection(Direction);
    }


    /// <summary>
    /// Transforms the <see cref="Line"/> from world space to local space by applying the inverse of 
    /// a <see cref="Pose"/>.
    /// </summary>
    /// <param name="pose">The pose (rotation and translation).</param>
    internal void ToLocal(ref Pose pose)
    {
      Debug.Assert(Direction.IsNumericallyNormalized, "Line direction should be normalized.");

      PointOnLine = pose.ToLocalPosition(PointOnLine);
      Direction = pose.ToLocalDirection(Direction);
    }
    #endregion
  }
}
