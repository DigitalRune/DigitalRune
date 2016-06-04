// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Defines an axis-aligned bounding box (AABB).
  /// </summary>
  /// <remarks>
  /// <para>
  /// This type is used to represent a bounding volume. It is like a box where the faces are aligned
  /// to the axes of the coordinate space.
  /// </para>
  /// <para>
  /// An AABB is defined by the positions of the <see cref="Minimum"/> and the <see cref="Maximum"/> 
  /// box corner. <see cref="Minimum"/> must be less than <see cref="Maximum"/>. But this rule is 
  /// not enforced; no exceptions are thrown in the setter of the properties.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Aabb")]
  public struct Aabb : IEquatable<Aabb>
  {
    // Notes: AABBs occur very frequently. If they are classes they would account 
    // for 20-30% of the garbage collector garbage (tested with memory profiling of
    // MassTest project). Aabbs are structs to avoid garbage collection overhead.
    //
    // Note: We do not use a field to allow direct access: Aabb.Minimum += ...;
    //
    // TODO: Ideas for new AABB methods.
    // static Aabb.Union(AABB) - static equivalent to Grow()
    // Translate(Vectore3F) - Moves the AABB

    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The minimum position.
    /// </summary>
    /// <remarks>
    /// This is the vertex of the AABB with smallest position.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3F Minimum;


    /// <summary>
    /// The maximum position.
    /// </summary>
    /// <remarks>
    /// This is the vertex of the AABB with the largest position.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public Vector3F Maximum;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the center.
    /// </summary>
    /// <value>The center.</value>
    public Vector3F Center
    {
      get { return (Minimum + Maximum) / 2; }
    }


    /// <summary>
    /// Gets the extent vector (<see cref="Maximum"/> - <see cref="Minimum"/>).
    /// </summary>
    /// <value>The extent (the widths in x, y, and z) of the AABB.</value>
    public Vector3F Extent
    {
      get { return Maximum - Minimum; }
    }


    /// <summary>
    /// Gets the volume of this axis-aligned bounding box.
    /// </summary>
    /// <value>The volume of the axis-aligned bounding box.</value>
    public float Volume
    {
      get
      {
        return (Maximum.X - Minimum.X) * (Maximum.Y - Minimum.Y) * (Maximum.Z - Minimum.Z);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of <see cref="Aabb"/>.
    /// </summary>
    /// <param name="minimum">The minimum.</param>
    /// <param name="maximum">The maximum.</param>
    public Aabb(Vector3F minimum, Vector3F maximum)
    {
      Minimum = minimum;
      Maximum = maximum;
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
      return obj is Aabb && Equals((Aabb)obj);
    }


    /// <summary>
    /// Determines whether the specified <see cref="Aabb"/> is equal to the current 
    /// <see cref="Aabb"/>.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(Aabb other)
    {
      return Minimum == other.Minimum && Maximum == other.Maximum;
    }


    /// <overloads>
    /// <summary>
    /// Determines whether two <see cref="Aabb"/> are equal (regarding a given tolerance).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether two AABBs are equal (regarding the tolerance 
    /// <see cref="Numeric.EpsilonF"/>).
    /// </summary>
    /// <param name="first">The first AABB.</param>
    /// <param name="second">The second AABB.</param>
    /// <returns>
    /// <see langword="true"/> if the AABBs are equal (within the tolerance 
    /// <see cref="Numeric.EpsilonF"/>); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// If the components of <see cref="Minimum"/> and <see cref="Maximum"/> differ less then 
    /// <see cref="Numeric.EpsilonF"/> the AABBs are considered as being equal.
    /// </remarks>
    public static bool AreNumericallyEqual(Aabb first, Aabb second)
    {
      return Vector3F.AreNumericallyEqual(first.Minimum, second.Minimum)
             && Vector3F.AreNumericallyEqual(first.Maximum, second.Maximum);
    }


    /// <summary>
    /// Determines whether two AABBs are equal (regarding the given tolerance).
    /// </summary>
    /// <param name="first">The first AABB.</param>
    /// <param name="second">The second AABB.</param>
    /// <param name="epsilon">The tolerance value.</param>
    /// <returns>
    /// <see langword="true"/> if the AABBs are equal (within the tolerance 
    /// <see cref="Numeric.EpsilonF"/>); otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// If the components of <see cref="Minimum"/> and <see cref="Maximum"/> differ less then 
    /// <paramref name="epsilon"/> the AABBs are considered as being equal.
    /// </remarks>
    public static bool AreNumericallyEqual(Aabb first, Aabb second, float epsilon)
    {
      return Vector3F.AreNumericallyEqual(first.Minimum, second.Minimum, epsilon)
             && Vector3F.AreNumericallyEqual(first.Maximum, second.Maximum, epsilon);
    }


    /// <summary>
    /// Tests if two <see cref="Aabb"/>s are equal.
    /// </summary>
    /// <param name="aabbA">The first <see cref="Aabb"/>.</param>
    /// <param name="aabbB">The second <see cref="Aabb"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Aabb"/>s are equal; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "aabb")]
    public static bool operator ==(Aabb aabbA, Aabb aabbB)
    {
      return aabbA.Minimum == aabbB.Minimum && aabbA.Maximum == aabbB.Maximum;
    }


    /// <summary>
    /// Tests if two <see cref="Aabb"/>s are different.
    /// </summary>
    /// <param name="aabbA">The first <see cref="Aabb"/>.</param>
    /// <param name="aabbB">The second <see cref="Aabb"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="Aabb"/>s are different; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "aabb")]
    public static bool operator !=(Aabb aabbA, Aabb aabbB)
    {
      return aabbA.Minimum != aabbB.Minimum || aabbA.Maximum != aabbB.Maximum;
    }
    #endregion


    /// <summary>
    /// Determines whether the current AABB contains a given AABB.
    /// </summary>
    /// <param name="other">The other AABB.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="other"/> is fully contained in this AABB; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Aabb other)
    {
      return Minimum.X <= other.Minimum.X
             && Minimum.Y <= other.Minimum.Y
             && Minimum.Z <= other.Minimum.Z
             && Maximum.X >= other.Maximum.X
             && Maximum.Y >= other.Maximum.Y
             && Maximum.Z >= other.Maximum.Z;
    }


    /// <summary>
    /// Computes the world space AABB for a scaled, rotated and translated AABB.
    /// </summary>
    /// <param name="scale">The scale of the AABB.</param>
    /// <param name="pose">
    /// The <see cref="Pose"/> that defines the rotation and translation. This pose defines how this
    /// <see cref="Aabb"/> should be positioned in world space.
    /// </param>
    /// <returns>
    /// The world space AABB that encloses this scaled, rotated and translated local space AABB.
    /// </returns>
    /// <remarks>
    /// Use this method if this AABB represents a bounding box in local space of an
    /// <see cref="IGeometricObject"/>. This method computes the world space AABB that encloses this
    /// AABB.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Aabb GetAabb(Vector3F scale, Pose pose)
    {
      Aabb aabb = this;
      aabb.Scale(scale);
      return aabb.GetAabb(pose);
    }


    /// <summary>
    /// Computes the world space AABB for a rotated and translated AABB.
    /// </summary>
    /// <param name="pose">
    /// The <see cref="Pose"/> that defines the rotation and translation. This pose defines how 
    /// this <see cref="Aabb"/> should be positioned in world space.
    /// </param>
    /// <returns>
    /// The world space AABB that encloses this rotated and translated local space AABB.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Use this method if this AABB represents a bounding box in local space of an
    /// <see cref="IGeometricObject"/>. This method computes the world space AABB that encloses this
    /// AABB.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Aabb GetAabb(Pose pose)
    {
      //if (pose == Pose.Identity)
      //  return new Aabb(Minimum, Maximum);

      // Use the same algorithm as in BoxShape.GetAabb().
      //Vector3F halfExtent = (Maximum - Minimum) / 2;
      Vector3F halfExtent;
      halfExtent.X = (Maximum.X - Minimum.X) / 2;
      halfExtent.Y = (Maximum.Y - Minimum.Y) / 2;
      halfExtent.Z = (Maximum.Z - Minimum.Z) / 2;
      //Vector3F center = (Minimum + Maximum) / 2;
      Vector3F center;
      center.X = (Minimum.X + Maximum.X) / 2;
      center.Y = (Minimum.Y + Maximum.Y) / 2;
      center.Z = (Minimum.Z + Maximum.Z) / 2;
      Vector3F centerWorld = pose.ToWorldPosition(center);

      // Get world axes in local space. They are equal to the rows of the orientation matrix.
      //Matrix33F rotationMatrix = pose.Orientation;
      //Vector3F worldX = rotationMatrix.GetRow(0);
      //Vector3F worldY = rotationMatrix.GetRow(1);
      //Vector3F worldZ = rotationMatrix.GetRow(2);
      Vector3F worldX, worldY, worldZ;
      worldX.X = pose.Orientation.M00;
      worldX.Y = pose.Orientation.M01;
      worldX.Z = pose.Orientation.M02;
      worldY.X = pose.Orientation.M10;
      worldY.Y = pose.Orientation.M11;
      worldY.Z = pose.Orientation.M12;
      worldZ.X = pose.Orientation.M20;
      worldZ.Y = pose.Orientation.M21;
      worldZ.Z = pose.Orientation.M22;

      // The half extent vector is in the +x/+y/+z octant of the world. We want to project
      // the extent onto the world axes. The half extent projected onto world x gives us the 
      // x extent. 
      // The world axes in local space could be in another world octant. We could now either find 
      // out the in which octant the world axes is pointing and build the correct half extent vector
      // for this octant. OR we mirror the world axis vectors into the +x/+y/+z octant by taking
      // the absolute vector.
      //worldX = Vector3F.Absolute(worldX);
      //worldY = Vector3F.Absolute(worldY);
      //worldZ = Vector3F.Absolute(worldZ);
      worldX.X = Math.Abs(worldX.X);
      worldX.Y = Math.Abs(worldX.Y);
      worldX.Z = Math.Abs(worldX.Z);
      worldY.X = Math.Abs(worldY.X);
      worldY.Y = Math.Abs(worldY.Y);
      worldY.Z = Math.Abs(worldY.Z);
      worldZ.X = Math.Abs(worldZ.X);
      worldZ.Y = Math.Abs(worldZ.Y);
      worldZ.Z = Math.Abs(worldZ.Z);

      // Now we project the extent onto the world axes.
      //Vector3F halfExtentWorld = new Vector3F(Vector3F.Dot(halfExtent, worldX), Vector3F.Dot(halfExtent, worldY), Vector3F.Dot(halfExtent, worldZ));
      Vector3F halfExtentWorld;
      halfExtentWorld.X = halfExtent.X * worldX.X + halfExtent.Y * worldX.Y + halfExtent.Z * worldX.Z;
      halfExtentWorld.Y = halfExtent.X * worldY.X + halfExtent.Y * worldY.Y + halfExtent.Z * worldY.Z;
      halfExtentWorld.Z = halfExtent.X * worldZ.X + halfExtent.Y * worldZ.Y + halfExtent.Z * worldZ.Z;

      //return new Aabb(centerWorld - halfExtentWorld, centerWorld + halfExtentWorld);
      Aabb result;
      result.Minimum.X = centerWorld.X - halfExtentWorld.X;
      result.Minimum.Y = centerWorld.Y - halfExtentWorld.Y;
      result.Minimum.Z = centerWorld.Z - halfExtentWorld.Z;
      result.Maximum.X = centerWorld.X + halfExtentWorld.X;
      result.Maximum.Y = centerWorld.Y + halfExtentWorld.Y;
      result.Maximum.Z = centerWorld.Z + halfExtentWorld.Z;
      return result;
    }


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
        int hashCode = Minimum.GetHashCode();
        hashCode = (hashCode * 397) ^ Maximum.GetHashCode();
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    // Note: Following is a standard method for Shapes. Maybe it is useful for AABBs too?
    ///// <summary>
    ///// Gets a support point for a given direction.
    ///// </summary>
    ///// <param name="direction">The direction for which to get the support point.</param>
    ///// <returns>
    ///// A support point regarding the given direction.
    ///// </returns>
    ///// <remarks>
    ///// <para>
    ///// A support point regarding a distance is an extreme point of the shape that is furthest away
    ///// from the center regarding the given distance. This point is not necessarily unique.
    ///// </para>
    ///// <para>
    ///// <paramref name="direction"/> must not be a zero vector, but it may be non-normalized.
    ///// </para>
    ///// <para>
    ///// <strong>Notes to Inheritors:</strong>
    ///// Throw an exception if <paramref name="direction"/> is a zero vector. Don't forget to normalize
    ///// <paramref name="direction"/> if required.
    ///// </para>
    ///// </remarks>
    //public override Vector3F GetSupportPoint(Vector3F direction)
    //{      
    //  Vector3F localSupportVertex = new Vector3F
    //  {
    //    X = ((direction.X >= 0) ? Maximum.X : Minimum.X),
    //    Y = ((direction.Y >= 0) ? Maximum.Y : Minimum.Y),
    //    Z = ((direction.Z >= 0) ? Maximum.Z : Minimum.Z)
    //  };
    //  return localSupportVertex;
    //}


    /// <overloads>
    /// <summary>
    /// Increases the size of the AABB.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Grows the AABB so that it includes the given point.
    /// </summary>
    /// <param name="point">The point to include.</param>
    public void Grow(Vector3F point)
    {
      Minimum = Vector3F.Min(Minimum, point);
      Maximum = Vector3F.Max(Maximum, point);
    }


    /// <summary>
    /// Grows the AABB so that it includes the given AABB.
    /// </summary>
    /// <param name="aabb">The AABB to include.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public void Grow(Aabb aabb)
    {
      Minimum = Vector3F.Min(Minimum, aabb.Minimum);
      Maximum = Vector3F.Max(Maximum, aabb.Maximum);
    }


    /// <summary>
    /// Grows the AABB so that it includes the AABB of the given geometric object.
    /// </summary>
    /// <param name="geometricObject">The geometric object to include.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
    public void Grow(IGeometricObject geometricObject)
    {
      Aabb geometryAabb = geometricObject.Aabb;
      Minimum = Vector3F.Min(Minimum, geometryAabb.Minimum);
      Maximum = Vector3F.Max(Maximum, geometryAabb.Maximum);
    }


    /// <summary>
    /// Merges the specified AABBs.
    /// </summary>
    /// <param name="first">The first AABB.</param>
    /// <param name="second">The second AABB.</param>
    /// <returns>The AABB that includes both child AABBs.</returns>
    public static Aabb Merge(Aabb first, Aabb second)
    {
      first.Grow(second);
      return first;
    }


    /// <summary>
    /// Scales the AABB.
    /// </summary>
    /// <param name="scale">
    /// The scale factors for the dimensions x, y and z.
    /// The scale factors can be negative to mirror the AABB.
    /// </param>
    /// <remarks>
    /// The AABB limits are multiplied with the scale factors.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1719:ParameterNamesShouldNotMatchMemberNames")]
    public void Scale(Vector3F scale)
    {
      // Scale Minimum and Maximum. If a scale factor is negative we have to exchange
      // Minimum and Maximum.
      Vector3F oldMin = Minimum;
      Vector3F oldMax = Maximum;

      if (scale.X >= 0)
      {
        Minimum.X = oldMin.X * scale.X;
        Maximum.X = oldMax.X * scale.X;
      }
      else
      {
        Minimum.X = oldMax.X * scale.X;
        Maximum.X = oldMin.X * scale.X;
      }

      if (scale.Y >= 0)
      {
        Minimum.Y = oldMin.Y * scale.Y;
        Maximum.Y = oldMax.Y * scale.Y;
      }
      else
      {
        Minimum.Y = oldMax.Y * scale.Y;
        Maximum.Y = oldMin.Y * scale.Y;
      }

      if (scale.Z >= 0)
      {
        Minimum.Z = oldMin.Z * scale.Z;
        Maximum.Z = oldMax.Z * scale.Z;
      }
      else
      {
        Minimum.Z = oldMax.Z * scale.Z;
        Maximum.Z = oldMin.Z * scale.Z;
      }
    }


    /// <summary>
    /// Translates the AABB.
    /// </summary>
    /// <param name="translation">The displacement vector.</param>
    /// <remarks>
    /// The <paramref name="translation"/> is added to the AABB limits.
    /// </remarks>
    public void Translate(Vector3F translation)
    {
      Minimum += translation;
      Maximum += translation;
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "Aabb {{ Minimum = {0}, Maximum = {1} }}", Minimum, Maximum);
    }
    #endregion
  }
}
