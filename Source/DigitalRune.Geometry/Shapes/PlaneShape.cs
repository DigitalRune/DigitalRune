// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a plane.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class can be used if an <see cref="IGeometricObject"/> with a plane shape is needed. Use
  /// the <see cref="Plane"/> structure instead if you need a lightweight representation of a plane
  /// (avoids allocating memory on the heap).
  /// </para>
  /// <para>
  /// A plane shape divides the world into two half-spaces. The negative half-space is the solid
  /// volume of this shape. The plane <see cref="Normal"/> is stored normalized and points into the
  /// positive half-space (which is not part of the shape).
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class PlaneShape : Shape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Vector3F _normal;
    private float _distanceFromOrigin;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the outward pointing normal vector.
    /// </summary>
    /// <value>The outward pointing normal vector. Must be normalized.</value>
    /// <remarks>
    /// This vector points away from the volume of this shape.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not normalized.</exception>
    public Vector3F Normal
    {
      get { return _normal; }
      set 
      {
        if (!value.IsNumericallyNormalized)
          throw new ArgumentException("The plane normal must be normalized.");

        if (_normal != value)
        {
          _normal = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }


    /// <summary>
    /// Gets or sets the distance of the plane from the origin (also known as the "plane constant").
    /// </summary>
    /// <value>The distance from the origin.</value>
    /// <remarks>
    /// This value is the distance from the plane point nearest to the origin projected onto the 
    /// normal vector. This distance can be negative to signify a negative plane offset.
    /// </remarks>
    public float DistanceFromOrigin
    {
      get { return _distanceFromOrigin; }
      set 
      {
        if (_distanceFromOrigin != value)
        {
          _distanceFromOrigin = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }


    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>An inner point.</value>
    public override Vector3F InnerPoint
    {
      get { return _normal * _distanceFromOrigin; }
    }


    /// <summary>
    /// Gets or sets the size of the mesh that represents a <see cref="PlaneShape"/>.
    /// </summary>
    /// <value>The size of the mesh.</value>
    /// <remarks>
    /// See <see cref="OnGetMesh"/> for more information.
    /// </remarks>
    public static float MeshSize { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="PlaneShape"/> class.
    /// </summary>
    static PlaneShape()
    {
      MeshSize = 100;
    }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaneShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaneShape"/> class.
    /// </summary>
    /// <remarks>
    /// Creates a plane which lies in the xz-plane and <see cref="Normal"/> points in y-axis 
    /// direction.
    /// </remarks>
    public PlaneShape()
      : this(Vector3F.UnitY, 0)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="PlaneShape"/> class from a normal vector and
    /// the distance from the origin.
    /// </summary>
    /// <param name="normal">
    /// The outward pointing normal vector of the plane. Must be normalized.
    /// </param>
    /// <param name="distanceFromOrigin">The distance from the origin.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="normal"/> is not normalized.
    /// </exception>
    public PlaneShape(Vector3F normal, float distanceFromOrigin)
    {
      if (!normal.IsNumericallyNormalized)
        throw new ArgumentException("The plane normal must be normalized.", "normal");

      _normal = normal;
      _distanceFromOrigin = distanceFromOrigin;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="PlaneShape"/> class from three points.
    /// </summary>
    /// <param name="point0">A point on the plane.</param>
    /// <param name="point1">A point on the plane.</param>
    /// <param name="point2">A point on the plane.</param>
    /// <remarks>
    /// This constructor creates a <see cref="PlaneShape"/> from three points in the plane. The
    /// points must be ordered counter-clockwise. The front-face (which points into the empty
    /// half-space) is defined through the counter-clockwise order of the points.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="point0"/>, <paramref name="point1"/>, and <paramref name="point2"/> do not 
    /// form a valid triangle.
    /// </exception>
    public PlaneShape(Vector3F point0, Vector3F point1, Vector3F point2)
    {
      if (Vector3F.AreNumericallyEqual(point0, point1)
          || Vector3F.AreNumericallyEqual(point0, point2)
          || Vector3F.AreNumericallyEqual(point1, point2))
        throw new ArgumentException("The points do not form a valid triangle.");

      // Compute normal vector.
      _normal = Vector3F.Cross(point1 - point0, point2 - point0);

      if (!_normal.TryNormalize())
        throw new ArgumentException("The points do not form a valid triangle.");

      // Compute the distance from the origin.
      _distanceFromOrigin = Vector3F.Dot(point0, _normal);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="PlaneShape"/> class.
    /// </summary>
    /// <param name="normal">
    /// The outward pointing normal vector of the plane. Must be normalized.</param>
    /// <param name="pointOnPlane">A point on the plane.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="normal"/> is not normalized.
    /// </exception>
    public PlaneShape(Vector3F normal, Vector3F pointOnPlane)
    {
      if (!normal.IsNumericallyNormalized)
        throw new ArgumentException("The plane normal must be normalized.", "normal");

      _normal = normal;
      _distanceFromOrigin = Vector3F.Dot(pointOnPlane, _normal);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="PlaneShape"/> class.
    /// </summary>
    /// <param name="plane">
    /// The plane from which normal vector and distance from origin are copied.
    /// </param>
    /// <exception cref="ArgumentException">The plane normal is not normalized.</exception>
    public PlaneShape(Plane plane)
    {
      if (!plane.Normal.IsNumericallyNormalized)
        throw new ArgumentException("The plane normal must be normalized.", "plane");

      _normal = plane.Normal;
      _distanceFromOrigin = plane.DistanceFromOrigin;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new PlaneShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (PlaneShape)sourceShape;
      _normal = source.Normal;
      _distanceFromOrigin = source.DistanceFromOrigin;
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      // Uniform scales do not influence the normal direction. Non-uniform scales change the normal
      // direction, but don't make much sense for planes. --> Return an infinite AABB.
      if (scale.X != scale.Y || scale.Y != scale.Z)
        return new Aabb(new Vector3F(float.NegativeInfinity), new Vector3F(float.PositiveInfinity));

      // Note: Compute AABB in world space
      Vector3F normal = pose.ToWorldDirection(_normal);

      // Negative uniform scaling --> invert normal.
      if (scale.X < 0)
        normal = -normal;

      // Apply scaling.
      float scaledDistance = _distanceFromOrigin * scale.X;

      // Most of the time the AABB fills the whole space. Only when the plane is axis-aligned then
      // the AABB is different.
      // Using numerical comparison we "clamp" the plane to an axis-aligned plane if possible.
      if (Vector3F.AreNumericallyEqual(normal, Vector3F.UnitX))
      {
        Vector3F minimum = new Vector3F(float.NegativeInfinity);
        Vector3F maximum = new Vector3F(pose.Position.X + scaledDistance, float.PositiveInfinity, float.PositiveInfinity);
        return new Aabb(minimum, maximum);
      }
      else if (Vector3F.AreNumericallyEqual(normal, Vector3F.UnitY))
      {
        Vector3F minimum = new Vector3F(float.NegativeInfinity);
        Vector3F maximum = new Vector3F(float.PositiveInfinity, pose.Position.Y + scaledDistance, float.PositiveInfinity);
        return new Aabb(minimum, maximum);
      }
      else if (Vector3F.AreNumericallyEqual(normal, Vector3F.UnitZ))
      {
        Vector3F minimum = new Vector3F(float.NegativeInfinity);
        Vector3F maximum = new Vector3F(float.PositiveInfinity, float.PositiveInfinity, pose.Position.Z + scaledDistance);
        return new Aabb(minimum, maximum);
      }
      else if (Vector3F.AreNumericallyEqual(normal, -Vector3F.UnitX))
      {
        Vector3F minimum = new Vector3F(pose.Position.X - scaledDistance, float.NegativeInfinity, float.NegativeInfinity);
        Vector3F maximum = new Vector3F(float.PositiveInfinity);
        return new Aabb(minimum, maximum);
      }
      else if (Vector3F.AreNumericallyEqual(normal, -Vector3F.UnitY))
      {
        Vector3F minimum = new Vector3F(float.NegativeInfinity, pose.Position.Y - scaledDistance, float.NegativeInfinity);
        Vector3F maximum = new Vector3F(float.PositiveInfinity);
        return new Aabb(minimum, maximum);
      }
      else if (Vector3F.AreNumericallyEqual(normal, -Vector3F.UnitZ))
      {
        Vector3F minimum = new Vector3F(float.NegativeInfinity, float.NegativeInfinity, pose.Position.Z - scaledDistance);
        Vector3F maximum = new Vector3F(float.PositiveInfinity);
        return new Aabb(minimum, maximum);
      }
      else
      {
        // Plane is not axis-aligned. --> AABB is infinite
        return new Aabb(new Vector3F(float.NegativeInfinity), new Vector3F(float.PositiveInfinity));
      }
    }



    /// <summary>
    /// Gets the volume of this plane.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used</param>
    /// <returns>Positive infinity (<see cref="float.PositiveInfinity"/>)</returns>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      return float.PositiveInfinity;
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "PlaneShape {{ Normal = {0}, DistanceFromOrigin = {1} }}", _normal, _distanceFromOrigin);
    }
    

    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    /// <remarks>
    /// This method creates a triangle mesh that represents a square lying in the plane. The square
    /// has an edge length of <see cref="MeshSize"/>.
    /// </remarks>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      Vector3F center = Normal * DistanceFromOrigin;
      Vector3F orthoNormal1 = Normal.Orthonormal1;
      Vector3F orthoNormal2 = Normal.Orthonormal2;

      // Plane 

      // Make 4 triangles around the center.
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(new Triangle
      {
        Vertex0 = center,
        Vertex1 = center + orthoNormal1 * MeshSize / 2 - orthoNormal2 * MeshSize / 2,
        Vertex2 = center + orthoNormal1 * MeshSize / 2 + orthoNormal2 * MeshSize / 2,
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = center,
        Vertex1 = center + orthoNormal1 * MeshSize / 2 + orthoNormal2 * MeshSize / 2,
        Vertex2 = center - orthoNormal1 * MeshSize / 2 + orthoNormal2 * MeshSize / 2,
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = center,
        Vertex1 = center - orthoNormal1 * MeshSize / 2 + orthoNormal2 * MeshSize / 2,
        Vertex2 = center - orthoNormal1 * MeshSize / 2 - orthoNormal2 * MeshSize / 2,
      }, true);
      mesh.Add(new Triangle
      {
        Vertex0 = center,
        Vertex1 = center - orthoNormal1 * MeshSize / 2 - orthoNormal2 * MeshSize / 2,
        Vertex2 = center + orthoNormal1 * MeshSize / 2 - orthoNormal2 * MeshSize / 2,
      }, true);

      return mesh;
    }
    #endregion
  }
}
