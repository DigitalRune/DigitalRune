// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a cylinder centered at the local origin and upright along the y-axis.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class CylinderShape : ConvexShape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point (center of cylinder).
    /// </summary>
    /// <value>The center of the cylinder (0, 0, 0).</value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get { return Vector3F.Zero; }
    }


    /// <summary>
    /// Gets or sets the height (which is along the y-axis).
    /// </summary>
    /// <value>The height.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float Height
    {
      get { return _height; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The height must be greater than or equal to 0.");

        if (_height != value)
        {
          _height = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _height;


    /// <summary>
    /// Gets or sets the radius.
    /// </summary>
    /// <value>The radius.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float Radius
    {
      get { return _radius; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "The radius must be greater than or equal to 0.");

        if (_radius != value)
        {
          _radius = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private float _radius;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="CylinderShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="CylinderShape"/> class.
    /// </summary>
    /// <remarks>
    /// Creates an empty cylinder.
    /// </remarks>
    public CylinderShape()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="CylinderShape"/> class with the given radius
    /// and height.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="height">The height (which is along the y-axis).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="radius"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="height"/> is negative.
    /// </exception>
    public CylinderShape(float radius, float height)
    {
      if (radius < 0)
        throw new ArgumentOutOfRangeException("radius", "The radius must be greater than or equal to 0.");
      if (height < 0)
        throw new ArgumentOutOfRangeException("height", "The height must be greater than or equal to 0.");

      _height = height;
      _radius = radius;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new CylinderShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (CylinderShape)sourceShape;
      _radius = source.Radius;
      _height = source.Height;
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      if (scale.X == scale.Y && scale.Y == scale.Z)
      {
        // Get world axes in local space. They are equal to the rows of the orientation matrix.
        Matrix33F rotationMatrix = pose.Orientation;
        Vector3F worldX = rotationMatrix.GetRow(0);
        Vector3F worldY = rotationMatrix.GetRow(1);
        Vector3F worldZ = rotationMatrix.GetRow(2);

        // Get extreme points along positive axes.
        Vector3F halfExtent = new Vector3F(
          Vector3F.Dot(GetSupportPointNormalized(worldX), worldX),
          Vector3F.Dot(GetSupportPointNormalized(worldY), worldY),
          Vector3F.Dot(GetSupportPointNormalized(worldZ), worldZ));

        // Apply scale.
        halfExtent *= Math.Abs(scale.X);

        Vector3F minimum = pose.Position - halfExtent;
        Vector3F maximum = pose.Position + halfExtent;

        Debug.Assert(minimum <= maximum);

        return new Aabb(minimum, maximum);
      }
      else
      {
        // Non-uniform scaling.
        return base.GetAabb(scale, pose);
      }
    }


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
    /// </remarks>
    public override Vector3F GetSupportPoint(Vector3F direction)
    {
      // The general formula for cylinders with arbitrary orientation is in 
      // Bergen: "Collision Detection in Interactive 3D Environments", pp. 136.
      // The formula for cylinders with up-axis = y-axis is simpler:

      Vector3F directionInXYPlane = new Vector3F(direction.X, 0, direction.Z);
      if (directionInXYPlane.TryNormalize())
      {
        // The general case.
        if (direction.Y >= 0)
          return Vector3F.UnitY * _height / 2 + _radius * directionInXYPlane;
        else
          return -Vector3F.UnitY * _height / 2 + _radius * directionInXYPlane;
      }
      else
      {
        // localDirection == +/-(0, 1, 0)
        Debug.Assert(Numeric.IsZero(direction.X) && Numeric.IsZero(direction.Z), "X and Y of direction are expected to be (near) zero.");

        if (direction.Y >= 0)
          return new Vector3F(_radius, _height / 2, 0);
        else
          return new Vector3F(_radius, -_height / 2, 0);
      }
    }


    /// <summary>
    /// Gets a support point for a given normalized direction vector.
    /// </summary>
    /// <param name="directionNormalized">
    /// The normalized direction vector for which to get the support point.
    /// </param>
    /// <returns>A support point regarding the given direction.</returns>
    /// <remarks>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </remarks>
    public override Vector3F GetSupportPointNormalized(Vector3F directionNormalized)
    {
      // The general formula for cylinders with arbitrary orientation is in 
      // Bergen: "Collision Detection in Interactive 3D Environments", pp. 136.
      // The formula for cylinders with up-axis = y-axis is simpler:

      Vector3F directionInXYPlane = new Vector3F(directionNormalized.X, 0, directionNormalized.Z);
      if (directionInXYPlane.TryNormalize())
      {
        // The general case.
        if (directionNormalized.Y >= 0)
          return Vector3F.UnitY * _height / 2 + _radius * directionInXYPlane;
        else
          return -Vector3F.UnitY * _height / 2 + _radius * directionInXYPlane;
      }
      else
      {
        // localDirection == +/-(0, 1, 0)
        Debug.Assert(Numeric.IsZero(directionNormalized.X) && Numeric.IsZero(directionNormalized.Z), "X and Y of direction are expected to be (near) zero.");

        if (directionNormalized.Y >= 0)
          return new Vector3F(_radius, _height / 2, 0);
        else
          return new Vector3F(_radius, -_height / 2, 0);
      }
    }


    /// <overloads>
    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// <returns>
    /// The volume of this shape.
    /// </returns>
    public float GetVolume()
    {
      float radius = Radius;
      return ConstantsF.Pi * radius * radius * Height;
    }


    /// <summary>
    /// Gets the volume of this cylinder.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used.</param>
    /// <returns>The volume of this cylinder.</returns>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      return GetVolume();
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      // Estimate required segment angle for given accuracy. 
      // (Easy to derive from simple drawing of a circle segment with a triangle used to represent
      // the segment.)
      float alpha = (float)Math.Acos((Radius - absoluteDistanceThreshold) / Radius) * 2;
      int numberOfSegments = (int)Math.Ceiling(ConstantsF.TwoPi / alpha);      

      // Apply the iteration limit - in case absoluteDistanceThreshold is 0.
      // Lets say each iteration doubles the number of segments. This is an arbitrary interpretation
      // of the "iteration limit".
      numberOfSegments = Math.Min(numberOfSegments, 2 << iterationLimit);

      alpha = ConstantsF.TwoPi / numberOfSegments;

      Vector3F r0 = new Vector3F(Radius, 0, 0);
      QuaternionF rotation = QuaternionF.CreateRotationY(alpha);

      TriangleMesh mesh = new TriangleMesh();

      for (int i = 1; i <= numberOfSegments; i++)
      {
        Vector3F r1 = rotation.Rotate(r0);

        // Bottom triangle
        mesh.Add(new Triangle
        {
          Vertex0 = new Vector3F(0, -Height / 2, 0),
          Vertex1 = new Vector3F(0, -Height / 2, 0) + r1,
          Vertex2 = new Vector3F(0, -Height / 2, 0) + r0,
        }, false);

        // Top triangle
        mesh.Add(new Triangle
        {
          Vertex0 = new Vector3F(0, +Height / 2, 0),
          Vertex1 = new Vector3F(0, +Height / 2, 0) + r0,
          Vertex2 = new Vector3F(0, +Height / 2, 0) + r1,
        }, false);

        // Two side triangles
        mesh.Add(new Triangle
        {
          Vertex0 = new Vector3F(0, -Height / 2, 0) + r0,
          Vertex1 = new Vector3F(0, -Height / 2, 0) + r1,
          Vertex2 = new Vector3F(0, +Height / 2, 0) + r0,
        }, false);
        mesh.Add(new Triangle
        {
          Vertex0 = new Vector3F(0, -Height / 2, 0) + r1,
          Vertex1 = new Vector3F(0, +Height / 2, 0) + r1,
          Vertex2 = new Vector3F(0, +Height / 2, 0) + r0,
        }, false);

        r0 = r1;
      }

      mesh.WeldVertices();

      return mesh;
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "CylinderShape {{ Radius = {0}, Height = {1} }}", _radius, _height);
    }
    #endregion
  }
}
