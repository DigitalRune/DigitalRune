// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Defines a plane.
  /// </summary> 
  /// <remarks>
  /// <para>
  /// This is a lightweight structure. To define a plane shape for <see cref="IGeometricObject"/>
  /// use <see cref="PlaneShape"/>.
  /// </para>
  /// <para>
  /// A plane can be described by a plane equation: <i>a * x + b * y + c * z = d</i>, where 
  /// <i>(x, y, z)</i> is a point on the plane and <i>(a, b, c)</i> is the <see cref="Normal"/>
  /// vector. <i>d</i> is the "plane constant", which is equal to the distance from the origin if
  /// the normal is normalized.
  /// </para>
  /// <para>
  /// Two <see cref="Plane"/>s are considered as equal if <see cref="Normal"/> and 
  /// <see cref="DistanceFromOrigin"/> are equal.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public struct Plane : IEquatable<Plane>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The normalized, outward pointing normal vector.
    /// </summary>
    /// <remarks>
    /// This vector points away from solid half-space into the empty half-space.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3F Normal;


    /// <summary>
    /// The distance of the plane from the origin (also known as the "plane constant").
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is the plane constant d from the plane equation <i>a * x + b * y + c * z = d</i>.
    /// If the <see cref="Normal"/> is normalized, then this value is the distance from the plane
    /// point nearest to the origin projected onto the normal vector. This distance can be negative
    /// to signify a negative plane offset.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public float DistanceFromOrigin;
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
    /// Initializes a new instance of <see cref="Plane"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of <see cref="Plane"/> from normal vector and distance to the 
    /// origin.
    /// </summary>
    /// <param name="normal">
    /// The normalized, outward pointing normal vector of the plane. 
    /// </param>
    /// <param name="distanceFromOrigin">
    /// The distance from the origin.
    /// </param>
    public Plane(Vector3F normal, float distanceFromOrigin)
    {
      Normal = normal;
      DistanceFromOrigin = distanceFromOrigin;
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Plane"/> from three points.
    /// </summary>
    /// <param name="point0">A point on the plane.</param>
    /// <param name="point1">A point on the plane.</param>
    /// <param name="point2">A point on the plane.</param>
    /// <remarks>
    /// <para>
    /// This constructor creates a <see cref="Plane"/> from three points in the plane. The points 
    /// must be ordered counter-clockwise. The front-face (which points into the empty half-space) 
    /// is defined through the counter-clockwise order of the points.
    /// </para>
    /// <para>
    /// The result is undefined if the points lie on a line.
    /// </para>
    /// </remarks>
    public Plane(Vector3F point0, Vector3F point1, Vector3F point2)
    {
      // Compute normal vector.
      Normal = Vector3F.Cross(point1 - point0, point2 - point0).Normalized;

      // Compute the distance from the origin.
      DistanceFromOrigin = Vector3F.Dot(point0, Normal);
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Plane"/> from normal vector and a point on the 
    /// plane.
    /// </summary>    
    /// <param name="normal">
    /// The normalized, outward pointing normal vector of the plane.
    /// </param>
    /// <param name="pointOnPlane">A point on the plane.</param>
    public Plane(Vector3F normal, Vector3F pointOnPlane)
    {
      Normal = normal;
      DistanceFromOrigin = Vector3F.Dot(pointOnPlane, Normal);
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Plane"/> from a <see cref="PlaneShape"/>.
    /// </summary>
    /// <param name="planeShape">
    /// The <see cref="PlaneShape"/> from which normal vector and distance from origin are copied.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="planeShape"/> is <see langword="null"/>.
    /// </exception>
    public Plane(PlaneShape planeShape)
    {
      if (planeShape == null)
        throw new ArgumentNullException("planeShape");

      Normal = planeShape.Normal;
      DistanceFromOrigin = planeShape.DistanceFromOrigin;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //-------------------------------------------------------------- 

    /// <summary>
    /// Normalizes the plane.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Most operations require a plane to be normalized, i.e. the normal must be a unit vector. But
    /// certain operations may return a general plane which is not normalized. In these cases this
    /// method can be called to re-normalize the plane.
    /// </para>
    /// <para>
    /// Normalizing a plane means that the <see cref="Normal"/> and <see cref="DistanceFromOrigin"/>
    /// are multiplied by the same scale factor to get a normalized plane normal.
    /// </para>
    /// </remarks>
    public void Normalize()
    {
      float length = Normal.Length;
      if (Numeric.IsZero(length))
        throw new DivideByZeroException("Cannot normalize plane. The length of the normal vector is 0.");

      float scale = 1.0f / length;
      Normal.X *= scale;
      Normal.Y *= scale;
      Normal.Z *= scale;
      DistanceFromOrigin *= scale;
    }


    /// <summary>
    /// Tries to normalizes the plane.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the plane was normalized; otherwise, <see langword="false"/> if
    /// the plane could not be normalized. (The length of the normal is numerically zero.)
    /// </returns>
    /// <inheritdoc cref="Normalize"/>
    public bool TryNormalize()
    {
      float lengthSquared = Normal.LengthSquared;
      if (Numeric.IsZero(lengthSquared, Numeric.EpsilonFSquared))
        return false;

      float length = (float)Math.Sqrt(lengthSquared);

      float scale = 1.0f / length;
      Normal.X *= scale;
      Normal.Y *= scale;
      Normal.Z *= scale;
      DistanceFromOrigin *= scale;
      return true;
    }


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
      return obj is Plane && Equals((Plane)obj);
    }


    /// <summary>
    /// Determines whether the specified <see cref="Plane"/> is equal to the current 
    /// <see cref="Plane"/>.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(Plane other)
    {
      return Normal == other.Normal && DistanceFromOrigin == other.DistanceFromOrigin;
    }


    /// <summary>
    /// Tests if two <see cref="Plane"/>s are equal.
    /// </summary>
    /// <param name="plane1">The first <see cref="Plane"/>.</param>
    /// <param name="plane2">The second <see cref="Plane"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Plane"/>s are equal; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Plane plane1, Plane plane2)
    {
      return plane1.Normal == plane2.Normal
             && plane1.DistanceFromOrigin == plane2.DistanceFromOrigin;
    }


    /// <summary>
    /// Tests if two <see cref="Plane"/>s are different.
    /// </summary>
    /// <param name="plane1">The first <see cref="Plane"/>.</param>
    /// <param name="plane2">The second <see cref="Plane"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Plane"/>s are different; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Plane plane1, Plane plane2)
    {
      return plane1.Normal != plane2.Normal
             || plane1.DistanceFromOrigin != plane2.DistanceFromOrigin;
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
        int hashCode = Normal.GetHashCode();
        hashCode = (hashCode * 397) ^ DistanceFromOrigin.GetHashCode();
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
      return String.Format(CultureInfo.InvariantCulture, "Plane {{ Normal = {0}, DistanceFromOrigin = {1} }}", Normal, DistanceFromOrigin);
    }


    /// <summary>
    /// Applies a scaling to the <see cref="Plane"/>.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <exception cref="NotSupportedException">
    /// <paramref name="scale"/> is a non-uniform scaling. Non-uniform scaling of planes is not 
    /// supported.
    /// </exception>
    internal void Scale(ref Vector3F scale)
    {
      if (scale.X != scale.Y || scale.Y != scale.Z)
        throw new NotSupportedException("Computing collisions for planes with non-uniform scaling is not supported.");

      Debug.Assert(Normal.IsNumericallyNormalized, "Plane normal should be normalized.");

      if (scale.X < 0)
      {
        DistanceFromOrigin *= -scale.X;
        Normal = -Normal;
      }
      else
      {
        DistanceFromOrigin *= scale.X;
        // Since we have only uniform scaling: Nothing to do for Normal.
      }
    }


    /// <summary>
    /// Transforms the <see cref="Plane"/> from local space to world space by applying a 
    /// <see cref="Pose"/>.
    /// </summary>
    /// <param name="pose">The pose (position and orientation).</param>
    internal void ToWorld(ref Pose pose)
    {
      Debug.Assert(Normal.IsNumericallyNormalized, "Plane normal should be normalized.");

      // Transform normal.
      Normal = pose.ToWorldDirection(Normal);

      // Calculate a point on the new plane.
      Vector3F pointOnPlane = pose.Position + Normal * DistanceFromOrigin;

      // Project point on to normal vector to get the new DistanceFromOrigin.
      DistanceFromOrigin = Vector3F.Dot(pointOnPlane, Normal);
    }


    /// <summary>
    /// Transforms the <see cref="Plane"/> from world space to a local space by applying the inverse
    /// of a <see cref="Pose"/>.
    /// </summary>
    /// <param name="pose">The pose (position and orientation).</param>
    internal void ToLocal(ref Pose pose)
    {
      Debug.Assert(Normal.IsNumericallyNormalized, "Plane normal should be normalized.");

      // TODO: Simplify: This should be the same as:
      // NormalLocal = pose.ToLocalDirection(worldPlane.Normal),
      // DistanceFromOriginLocal = DistanceFromOrigin - Vector3F.Dot(worldPlane.Normal, pose.Position),

      // Calculate a point on the new plane.
      Vector3F pointOnPlane = Normal * DistanceFromOrigin;

      // Transform normal.
      Normal = pose.ToLocalDirection(Normal);

      // Transform point on plane.
      pointOnPlane = pose.ToLocalPosition(pointOnPlane);

      // Project point on to normal vector to get the new DistanceFromOrigin.
      DistanceFromOrigin = Vector3F.Dot(pointOnPlane, Normal);
    }
    #endregion
  }
}
