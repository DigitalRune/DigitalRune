// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a capsule centered at the local origin and upright along the y-axis.
  /// </summary>
  /// <remarks>
  /// A capsule is like a <see cref="CylinderShape"/> with spherical caps.
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class CapsuleShape : ConvexShape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point (center of capsule).
    /// </summary>
    /// <value>The center of the capsule (0, 0, 0).</value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get { return Vector3F.Zero; }
    }


    /// <summary>
    /// Gets or sets the total height (including the spherical caps).
    /// </summary>
    /// <value>The height (including the spherical caps).</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 2 * <see cref="Radius"/>. (The height must be greater 
    /// than or equal to 2 * radius.)
    /// </exception>
    public float Height
    {
      get { return _height; }
      set
      {
        if (value < 2 * _radius)
          throw new ArgumentOutOfRangeException("value", "The height must be greater or equal 2 * radius.");

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
    /// <paramref name="value"/> is either negative or greater than <see cref="Height"/> / 2. (The 
    /// radius must be less than or equal to height / 2.)
    /// </exception>
    public float Radius
    {
      get { return _radius; }
      set
      {
        if (value < 0 || 2 * value > _height)
          throw new ArgumentOutOfRangeException("value", "The radius must be greater than or equal to 0 and less or equal height / 2.");

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
    /// Initializes a new instance of the <see cref="CapsuleShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="CapsuleShape"/> class.
    /// </summary>
    /// <remarks>
    /// Creates an empty capsule.
    /// </remarks>
    public CapsuleShape()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="CapsuleShape"/> class with the given radius and 
    /// height.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <param name="height">The height (including the spherical caps).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="height"/> is less than 2 * <see cref="Radius"/>. (The height must be greater 
    /// than or equal to 2 * radius.)
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="radius"/> is negative.
    /// </exception>
    public CapsuleShape(float radius, float height)
    {
      if (height < 2 * radius)
        throw new ArgumentOutOfRangeException("height", "The height must be greater or equal 2 * radius.");
      if (radius < 0)
        throw new ArgumentOutOfRangeException("radius", "The radius must be greater than or equal to 0.");

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
      return new CapsuleShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (CapsuleShape)sourceShape;
      _radius = source.Radius;
      _height = source.Height;
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      if (scale.X == scale.Y && scale.Y == scale.Z)
      {
        // Uniform scaling.
        float uniformScale = Math.Abs(scale.X);
        float scaledHeight = uniformScale * _height;
        float scaledRadius = uniformScale * _radius;

        float halfHeightWithoutCaps = (scaledHeight / 2 - scaledRadius);

        // Imagine the skeleton of the capsule as a line: 
        //   (0, -halfExtentWithoutCaps, 0) to (0, halfExtent, 0)

        // To create the AABB we rotate these to points and then just add the radius.
        Vector3F p1 = pose.ToWorldPosition(new Vector3F(0, halfHeightWithoutCaps, 0));
        Vector3F p2 = pose.ToWorldPosition(new Vector3F(0, -halfHeightWithoutCaps, 0));
        Vector3F radius = new Vector3F(scaledRadius);
        Vector3F minimum = Vector3F.Min(p1, p2) - radius;
        Vector3F maximum = Vector3F.Max(p1, p2) + radius;
        return new Aabb(minimum, maximum);
      }
      else
      {
        // Non-uniform scaling.
        return base.GetAabb(scale, pose);
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
      // We return a point on one of the sphere caps. If direction points up,
      // we take the upper cap otherwise the lower cap.
      Vector3F capCenter = new Vector3F(0, _height / 2 - _radius, 0);
      if (directionNormalized.Y < 0)
        capCenter = -capCenter;

      return capCenter + directionNormalized * _radius;
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
    /// <returns>The volume of this shape.</returns>
    public float GetVolume()
    {
      float radius = Radius;

      // Volume of both spherical caps
      float sphereVolume = 4.0f / 3.0f * ConstantsF.Pi * radius * radius * radius;

      // Volume of cylinder
      float cylinderVolume = ConstantsF.Pi * radius * radius * (Height - 2 * radius);

      return sphereVolume + cylinderVolume;
    }


    /// <summary>
    /// Gets the volume of this capsule.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used.</param>
    /// <returns>The volume of this capsule.</returns>
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
      int numberOfSegments = (int)Math.Ceiling(ConstantsF.PiOver2 / alpha) * 4;

      // Apply the iteration limit - in case absoluteDistanceThreshold is 0.
      // Lets say each iteration doubles the number of segments. This is an arbitrary interpretation
      // of the "iteration limit".
      numberOfSegments = Math.Min(numberOfSegments, 2 << iterationLimit);
      
      alpha = ConstantsF.TwoPi / numberOfSegments;

      TriangleMesh mesh = new TriangleMesh();

      // The world space vertices are created by rotating "radius vectors" with this rotations.
      QuaternionF rotationY = QuaternionF.CreateRotationY(alpha);
      QuaternionF rotationZ = QuaternionF.CreateRotationZ(alpha);

      // We use two nested loops: In each loop a "radius vector" is rotated further to get a 
      // new vertex.
      Vector3F rLow = Vector3F.UnitX * Radius;    // Radius vector for the lower vertex.
      for (int i = 1; i <= numberOfSegments / 4; i++)
      {
        Vector3F rHigh = rotationZ.Rotate(rLow);  // Radius vector for the higher vertex.

        // In the inner loop we create lines and triangles between 4 vertices, which are created
        // with the radius vectors rLow0, rLow1, rHigh0, rHigh1.
        Vector3F rLow0 = rLow;
        Vector3F rHigh0 = rHigh;
        for (int j = 1; j <= numberOfSegments; j++)
        {
          Vector3F rLow1 = rotationY.Rotate(rLow0);
          Vector3F rHigh1 = rotationY.Rotate(rHigh0);

          // Two top hemisphere triangles
          mesh.Add(new Triangle
          {
            Vertex0 = new Vector3F(0, Height / 2 - Radius, 0) + rLow0,
            Vertex1 = new Vector3F(0, Height / 2 - Radius, 0) + rLow1,
            Vertex2 = new Vector3F(0, Height / 2 - Radius, 0) + rHigh0,
          }, false);
          if (i < numberOfSegments / 4)  // At the "northpole" only a triangle is needed. No quad.
          {
            mesh.Add(new Triangle
            {
              Vertex0 = new Vector3F(0, Height / 2 - Radius, 0) + rLow1,
              Vertex1 = new Vector3F(0, Height / 2 - Radius, 0) + rHigh1,
              Vertex2 = new Vector3F(0, Height / 2 - Radius, 0) + rHigh0,
            }, false);
          }

          // Two bottom hemisphere triangles
          mesh.Add(new Triangle
          {
            Vertex0 = new Vector3F(0, -Height / 2 + Radius, 0) - rLow0,
            Vertex1 = new Vector3F(0, -Height / 2 + Radius, 0) - rHigh0,
            Vertex2 = new Vector3F(0, -Height / 2 + Radius, 0) - rLow1,
          }, false);
          if (i < numberOfSegments / 4)  // At the "southpole" only a triangle is needed. No quad.
          {
            mesh.Add(new Triangle
            {
              Vertex0 = new Vector3F(0, -Height / 2 + Radius, 0) - rLow1,
              Vertex1 = new Vector3F(0, -Height / 2 + Radius, 0) - rHigh0,
              Vertex2 = new Vector3F(0, -Height / 2 + Radius, 0) - rHigh1,
            }, false);
          }

          // Two side triangles
          if (i == 1)
          {
            mesh.Add(new Triangle
            {
              Vertex0 = new Vector3F(0, -Height / 2 + Radius, 0) + rLow0,
              Vertex1 = new Vector3F(0, -Height / 2 + Radius, 0) + rLow1,
              Vertex2 = new Vector3F(0, Height / 2 - Radius, 0) + rLow0,
            }, false);
            mesh.Add(new Triangle
            {
              Vertex0 = new Vector3F(0, -Height / 2 + Radius, 0) + rLow1,
              Vertex1 = new Vector3F(0, Height / 2 - Radius, 0) + rLow1,
              Vertex2 = new Vector3F(0, Height / 2 - Radius, 0) + rLow0,
            }, false);
          }

          rLow0 = rLow1;
          rHigh0 = rHigh1;
        }

        rLow = rHigh;
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
      return String.Format(CultureInfo.InvariantCulture,"CapsuleShape {{ Radius = {0}, Height = {1} }}", _radius, _height);
    }
    #endregion
  }
}
