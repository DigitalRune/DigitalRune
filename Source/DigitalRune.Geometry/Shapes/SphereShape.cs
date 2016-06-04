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
  /// Represents a sphere.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class SphereShape : ConvexShape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point (center of sphere).
    /// </summary>
    /// <value>The center of the sphere (0, 0, 0).</value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get { return Vector3F.Zero; }
    }


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
    /// Initializes a new instance of the <see cref="SphereShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="SphereShape"/> class.
    /// </summary>
    /// <remarks>
    /// Creates an empty sphere (radius = 0).
    /// </remarks>
    public SphereShape()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SphereShape"/> class with the given radius.
    /// </summary>
    /// <param name="radius">The radius.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="radius"/> is negative.
    /// </exception>
    public SphereShape(float radius)
    {
      if (radius < 0)
        throw new ArgumentOutOfRangeException("radius", "The radius must be greater than or equal to 0.");

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
      return new SphereShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (SphereShape)sourceShape;
      _radius = source.Radius;
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      if (scale.X == scale.Y && scale.Y == scale.Z)
      {
        // Uniform scaling.
        Vector3F halfExtent = new Vector3F(_radius * Math.Abs(scale.X));
        return new Aabb(pose.Position - halfExtent, pose.Position + halfExtent);
      }
      else
      {
        // Non-uniform scaling.
        // TODO: This can be optimized because the shape is symmetric about its origin. 
        // base.GetAabb() shoots in all 6 directions, but we can shoot in only 3 directions 
        // and mirror the result. - Same can be done for Capsule, Cylinder, ...
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
      return directionNormalized * _radius;
    }


    /// <overloads>
    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the volume of this sphere.
    /// </summary>
    /// <returns>The volume of this sphere.
    /// </returns>
    public float GetVolume()
    {
      return 4 * ConstantsF.Pi * _radius * _radius * _radius / 3;
    }


    /// <summary>
    /// Gets the volume of this sphere.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used.</param>
    /// <returns>The volume of this sphere.</returns>
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
      float alpha = (float)Math.Acos((_radius - absoluteDistanceThreshold) / _radius) * 2;
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
      Vector3F rLow = Vector3F.UnitX * _radius;   // Radius vector for the lower vertex.
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
            Vertex0 = rLow0,
            Vertex1 = rLow1,
            Vertex2 = rHigh0,
          }, false);

          if (i < numberOfSegments / 4)  // At the "northpole" only a triangle is needed. No quad.
          {
            mesh.Add(new Triangle
            {
              Vertex0 = rLow1,
              Vertex1 = rHigh1,
              Vertex2 = rHigh0,
            }, false);
          }

          // Two bottom hemisphere triangles
          mesh.Add(new Triangle
          {
            Vertex0 = -rLow0,
            Vertex1 = -rHigh0,
            Vertex2 = -rLow1,
          }, false);

          if (i < numberOfSegments / 4)  // At the "southpole" only a triangle is needed. No quad.
          {
            mesh.Add(new Triangle
            {
              Vertex0 = -rLow1,
              Vertex1 = -rHigh0,
              Vertex2 = -rHigh1,
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
      return String.Format(CultureInfo.InvariantCulture, "SphereShape {{ Radius = {0} }}", _radius);
    }
    #endregion
  }
}
