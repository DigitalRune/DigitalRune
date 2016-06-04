// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a convex shape. 
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public abstract class ConvexShape : Shape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Computes the axis-aligned bounding box (AABB) for this shape positioned in world space using
    /// the given scale and <see cref="Pose"/>.
    /// </summary>
    /// <param name="scale">
    /// The scale factor by which the shape should be scaled. The scaling is applied in the shape's
    /// local space before the pose is applied.
    /// </param>
    /// <param name="pose">
    /// The <see cref="Pose"/> of the shape. This pose defines how the shape should be positioned in
    /// world space.
    /// </param>
    /// <returns>The AABB of the shape positioned in world space.</returns>
    /// <remarks>
    /// <para>
    /// The AABB is axis-aligned to the axes of the world space (or the parent coordinate space). 
    /// </para>
    /// <para>
    /// The default implementation in <see cref="ConvexShape"/> uses the support mapping to compute 
    /// the AABB. Often the AABB can be computed more efficiently; in such cases this method should 
    /// be overridden in derived classes.
    /// </para>
    /// </remarks>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      if (scale.X == scale.Y && scale.Y == scale.Z)
      {
        // Uniform scaling.

        // Get world axes in local space. They are equal to the rows of the orientation matrix.
        Matrix33F rotationMatrix = pose.Orientation;
        Vector3F worldX = rotationMatrix.GetRow(0);
        Vector3F worldY = rotationMatrix.GetRow(1);
        Vector3F worldZ = rotationMatrix.GetRow(2);

        // Get extreme points along all axes.
        Vector3F minimum = new Vector3F(
          Vector3F.Dot(GetSupportPointNormalized(-worldX), worldX),
          Vector3F.Dot(GetSupportPointNormalized(-worldY), worldY),
          Vector3F.Dot(GetSupportPointNormalized(-worldZ), worldZ));
        minimum = minimum * scale.X + pose.Position;

        Vector3F maximum = new Vector3F(
          Vector3F.Dot(GetSupportPointNormalized(worldX), worldX),
          Vector3F.Dot(GetSupportPointNormalized(worldY), worldY),
          Vector3F.Dot(GetSupportPointNormalized(worldZ), worldZ));
        maximum = maximum * scale.X + pose.Position;

        // Check minimum and maximum because scaling could be negative.
        if (minimum <= maximum)
          return new Aabb(minimum, maximum);
        else
          return new Aabb(maximum, minimum);
      }
      else
      {
        // Non-uniform scaling.

        // Get world axes in local space. They are equal to the rows of the orientation matrix.
        Matrix33F rotationMatrix = pose.Orientation;
        Vector3F worldX = rotationMatrix.GetRow(0);
        Vector3F worldY = rotationMatrix.GetRow(1);
        Vector3F worldZ = rotationMatrix.GetRow(2);

        // Get extreme points along all axes.
        Vector3F minimum = new Vector3F(
          Vector3F.Dot(GetSupportPoint(-worldX, scale), worldX),
          Vector3F.Dot(GetSupportPoint(-worldY, scale), worldY),
          Vector3F.Dot(GetSupportPoint(-worldZ, scale), worldZ));

        Vector3F maximum = new Vector3F(
          Vector3F.Dot(GetSupportPoint(worldX, scale), worldX),
          Vector3F.Dot(GetSupportPoint(worldY, scale), worldY),
          Vector3F.Dot(GetSupportPoint(worldZ, scale), worldZ));

        minimum += pose.Position;
        maximum += pose.Position;

        // Component-wise check minimum and maximum because scaling could be negative!
        if (minimum.X > maximum.X)
          MathHelper.Swap(ref minimum.X, ref maximum.X);
        if (minimum.Y > maximum.Y)
          MathHelper.Swap(ref minimum.Y, ref maximum.Y);
        if (minimum.Z > maximum.Z)
          MathHelper.Swap(ref minimum.Z, ref maximum.Z);

        Debug.Assert(minimum <= maximum);

        return new Aabb(minimum, maximum);
      }
    }




    /// <overloads>
    /// <summary>
    /// Gets a support point for a given direction.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets a support point for a given direction.
    /// </summary>
    /// <param name="direction">
    /// The direction for which to get the support point. The vector does not need to be normalized.
    /// The result is undefined if the vector is a zero vector.
    /// </param>
    /// <returns>A support point regarding the given direction.</returns>
    /// <remarks>
    /// <para>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away 
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong>
    /// The default implementation of this method normalizes the direction and calls
    /// <see cref="GetSupportPointNormalized"/>. If this method is overridden,
    /// don't forget to check whether direction is a zero vector and to normalize 
    /// <paramref name="direction"/> if required.
    /// </para>
    /// </remarks>
    public virtual Vector3F GetSupportPoint(Vector3F direction)
    {
      if (!direction.TryNormalize())
        direction = Vector3F.UnitX;

      return GetSupportPointNormalized(direction);
    }


    /// <summary>
    /// Gets a support point for a given direction and a given non-uniform scaling.
    /// </summary>
    /// <param name="direction">
    /// The direction for which to get the support point on the scaled shape. The vector does not 
    /// need to be normalized. The result is undefined if the vector is a zero vector.
    /// </param>
    /// <param name="scale">
    /// The scale that is applied to the shape. This can be a non-uniform 3D scaling.
    /// </param>
    /// <returns>
    /// A support point regarding the given direction on the scaled shape.
    /// </returns>
    /// <remarks>
    /// <para>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </para>
    /// <para>
    /// If only a uniform scale should be applied, it is faster to call 
    /// <see cref="GetSupportPoint(Vector3F)"/> or <see cref="GetSupportPointNormalized"/> and
    /// scale the resulting support point position. 
    /// </para>
    /// </remarks>
    public Vector3F GetSupportPoint(Vector3F direction, Vector3F scale)
    {
      // We need to find the support point in the scaled space. Shape.GetSupportPoint() can 
      // compute the support point for the unscaled space. We have to transform the support 
      // direction to the unscaled space. The support direction is like a normal vector
      // (or tangential covector). It must be transformed with (M^-1)^T. If M is a scaling matrix,
      // (M^-1)^T is the inverse scaling matrix.

      // In other words:
      // To transform points from the unscaled space to the scaled space, we multiply with Scale.
      // To transform points from the scaled space to the unscaled space, we divide by Scale.
      // To transform normals from the unscaled space to the scaled space, we divide by Scale.
      // To transform normals from the scaled space to the unscaled space, we multiply with Scale.

      return GetSupportPoint(scale * direction) * scale;
    }


    /// <summary>
    /// Gets a support point for a given normalized direction vector.
    /// </summary>
    /// <param name="directionNormalized">
    /// The normalized direction vector for which to get the support point. 
    /// </param>
    /// <returns>A support point regarding the given direction.</returns>
    /// <remarks>
    /// <para>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away 
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </para>
    /// </remarks>
    public abstract Vector3F GetSupportPointNormalized(Vector3F directionNormalized);


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    /// <remarks>
    /// <para>
    /// <see cref="ConvexShape"/> provides a base implementation for <see cref="OnGetMesh"/> that
    /// samples the support mapping and automatically generates a mesh. But derived classes should
    /// override <see cref="OnGetMesh"/> if they can provide a more efficient implementation.
    /// </para>
    /// </remarks>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      // Use SupportMappingSampler to get surface points.
      IList<Vector3F> points = GeometryHelper.SampleConvexShape(this, absoluteDistanceThreshold, iterationLimit);
      DcelMesh mesh = GeometryHelper.CreateConvexHull(points);
      return mesh.ToTriangleMesh();
    }
    #endregion
  }
}
