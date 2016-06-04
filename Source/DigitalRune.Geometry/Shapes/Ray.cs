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
  /// Defines a ray.
  /// </summary> 
  /// <remarks>
  /// <para>
  /// This is a lightweight structure. To define a ray shape for an <see cref="IGeometricObject"/>
  /// use <see cref="RayShape"/>.
  /// </para>
  /// <para>
  /// Two <see cref="Ray"/>s are considered as equal if <see cref="Origin"/>, 
  /// <see cref="Direction"/> and <see cref="Length"/> are equal.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public struct Ray : IEquatable<Ray>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The origin of the ray.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3F Origin;


    /// <summary>
    /// The normalized direction of the ray.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3F Direction;


    /// <summary>
    /// The finite length of the ray.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public float Length;
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
    /// Initializes a new instance of <see cref="Ray"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of <see cref="Line"/> with the given origin and direction.
    /// </summary>
    /// <param name="origin">The origin.</param>
    /// <param name="direction">The direction.</param>
    /// <param name="length">The finite length.</param>
    public Ray(Vector3F origin, Vector3F direction, float length)
    {
      Origin = origin;
      Direction = direction;
      Length = length;
    }


    /// <summary>
    /// Initializes a new instance of <see cref="Ray"/> from a <see cref="RayShape"/>.
    /// </summary>
    /// <param name="rayShape">
    /// The <see cref="RayShape"/> from which origin and direction are copied.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="rayShape"/> is <see langword="null"/>.
    /// </exception>
    public Ray(RayShape rayShape)
    {
      if (rayShape == null)
        throw new ArgumentNullException("rayShape");

      Origin = rayShape.Origin;
      Direction = rayShape.Direction;
      Length = rayShape.Length;
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
      return obj is Ray && Equals((Ray)obj);
    }


    /// <summary>
    /// Determines whether the specified <see cref="Ray"/> is equal to the current 
    /// <see cref="Ray"/>.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(Ray other)
    {
      return Origin == other.Origin
             && Direction == other.Direction
             && Length == other.Length;
    }


    /// <summary>
    /// Tests if two <see cref="Ray"/>s are equal.
    /// </summary>
    /// <param name="ray1">The first <see cref="Ray"/>.</param>
    /// <param name="ray2">The second <see cref="Ray"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Ray"/>s are equal; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Ray ray1, Ray ray2)
    {
      return ray1.Origin == ray2.Origin 
             && ray1.Direction == ray2.Direction
             && ray1.Length == ray2.Length;
    }


    /// <summary>
    /// Tests if two <see cref="Ray"/>s are different.
    /// </summary>
    /// <param name="ray1">The first <see cref="Ray"/>.</param>
    /// <param name="ray2">The second <see cref="Ray"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Ray"/>s are different; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Ray ray1, Ray ray2)
    {
      return ray1.Origin != ray2.Origin 
             || ray1.Direction != ray2.Direction
             || ray1.Length != ray2.Length;
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
        int hashCode = Origin.GetHashCode();
        hashCode = (hashCode * 397) ^ Direction.GetHashCode();
        hashCode = (hashCode * 397) ^ Length.GetHashCode();
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
      return String.Format(CultureInfo.InvariantCulture, "Ray {{ Origin = {0}, Direction = {1}, Length = {2} }}", Origin, Direction, Length);
    }

    
    /// <summary>
    /// Applies a scaling to the <see cref="Ray"/>.
    /// </summary>
    /// <param name="scale">The scale.</param>
    /// <exception cref="NotSupportedException">
    /// <paramref name="scale"/> is a non-uniform scaling. Non-uniform scaling of rays is not 
    /// supported.
    /// </exception>
    internal void Scale(ref Vector3F scale)
    {
      if (scale.X == scale.Y && scale.Y == scale.Z)
      {
        // ----- Uniform scaling
        Debug.Assert(Direction.IsNumericallyNormalized, "Ray direction should be normalized.");
        Debug.Assert(Length > 0 && Numeric.IsGreater(Length, 0) || !float.IsInfinity(Length), "Ray length must be in the range 0 < length < infinity.");

        if (scale.X < 0)
        {
          Origin *= scale.X;
          Length *= -scale.X;
          Direction = -Direction;
        }
        else
        {
          Origin *= scale.X;
          Length *= scale.X;
          // Since we have only uniform scaling: Nothing to do for Direction.
        }
      }
      else
      {
        // ----- Nonuniform scaling
        // Transfrom the origin and end point and compute the new ray parameters from origin/end.
        var end = Origin + Length * Direction;

        Origin *= scale;
        end *= scale;
        
        var direction = end - Origin;
        Length = direction.Length;
      
        if (!Numeric.IsZero(Length))
          Direction = direction / Length;
        // else keep old direction.
      }
    }


    /// <summary>
    /// Transforms the <see cref="Ray"/> from local space to world space by applying a 
    /// <see cref="Pose"/>.
    /// </summary>
    /// <param name="pose">The pose (position and orientation).</param>
    internal void ToWorld(ref Pose pose)
    {
      Debug.Assert(Direction.IsNumericallyNormalized, "Ray direction should be normalized. Length = " + Direction.Length);
      Debug.Assert(Length > 0 && Numeric.IsGreater(Length, 0) || !float.IsInfinity(Length), "Ray length must be in the range 0 < length < infinity.");

      Origin = pose.ToWorldPosition(Origin);
      Direction = pose.ToWorldDirection(Direction);
    }


    /// <summary>
    /// Transforms the <see cref="Ray"/> from world space to local space by applying the inverse of 
    /// a <see cref="Pose"/>.
    /// </summary>
    /// <param name="pose">The pose (rotation and translation).</param>
    internal void ToLocal(ref Pose pose)
    {
      Debug.Assert(Direction.IsNumericallyNormalized, "Ray direction should be normalized. Length = " + Direction.Length);
      Debug.Assert(Length > 0 && Numeric.IsGreater(Length, 0) || !float.IsInfinity(Length), "Ray length must be in the range 0 < length < infinity.");

      Origin = pose.ToLocalPosition(Origin);
      Direction = pose.ToLocalDirection(Direction);
    }
    #endregion
  }
}
