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
  /// Represents a triangle.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class can be used if an <see cref="IGeometricObject"/> with a triangle shape is needed.
  /// Use the <see cref="Triangle"/> structure instead if you need a lightweight representation of a
  /// triangle (avoids allocating memory on the heap).
  /// </para>
  /// <para>
  /// The triangle front face is where the vertices are ordered counter-clockwise.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class TriangleShape : ConvexShape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>An inner point.</value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get { return (_vertex0 + _vertex1 + _vertex2) / 3; }
    }


    /// <summary>
    /// Gets the normal.
    /// </summary>
    /// <value>The normal.</value>
    public Vector3F Normal
    {
      get
      {
        Vector3F normal = Vector3F.Cross(_vertex1 - _vertex0, _vertex2 - _vertex0);
        if (!normal.TryNormalize())
          normal = Vector3F.UnitY;

        return normal;
      }
    }


    /// <summary>
    /// Gets or sets the vertex at the specified index.
    /// </summary>
    /// <param name="index">The index of the triangle point.</param>
    /// <value>The vertex with the given index.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is out of range.
    /// </exception>
    public Vector3F this[int index]
    {
      get
      {
        switch (index)
        {
          case 0: return _vertex0;
          case 1: return _vertex1;
          case 2: return _vertex2;
          default:
            throw new ArgumentOutOfRangeException("index");
        }
      }
      set
      {
        switch (index)
        {
          case 0: Vertex0 = value; break;
          case 1: Vertex1 = value; break;
          case 2: Vertex2 = value; break;
          default:
            throw new ArgumentOutOfRangeException("index");
        }
      }
    }


    /// <summary>
    /// Gets or sets the first vertex.
    /// </summary>
    /// <value>The first vertex.</value>
    public Vector3F Vertex0
    {
      get { return _vertex0; }
      set
      {
        if (_vertex0 != value)
        {
          _vertex0 = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3F _vertex0;


    /// <summary>
    /// Gets or sets the second vertex.
    /// </summary>
    /// <value>The second vertex.</value>
    public Vector3F Vertex1
    {
      get { return _vertex1; }
      set
      {
        if (_vertex1 != value)
        {
          _vertex1 = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3F _vertex1;


    /// <summary>
    /// Gets or sets the third vertex.
    /// </summary>
    /// <value>The third vertex.</value>
    public Vector3F Vertex2
    {
      get { return _vertex2; }
      set
      {
        if (_vertex2 != value)
        {
          _vertex2 = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3F _vertex2;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleShape"/> class.
    /// </summary>
    /// <remarks>
    /// Creates a triangle where all vertices are at the origin (0, 0, 0).
    /// </remarks>
    public TriangleShape()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleShape"/> class from the given vertices.
    /// </summary>
    /// <param name="vertex0">The first vertex.</param>
    /// <param name="vertex1">The second vertex.</param>
    /// <param name="vertex2">The third vertex.</param>
    public TriangleShape(Vector3F vertex0, Vector3F vertex1, Vector3F vertex2)
    {
      _vertex0 = vertex0;
      _vertex1 = vertex1;
      _vertex2 = vertex2;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleShape"/> class from a 
    /// <see cref="Triangle"/>.
    /// </summary>
    /// <param name="triangle">
    /// The <see cref="Triangle"/> structure from which vertices are copied.
    /// </param>
    public TriangleShape(Triangle triangle)
    {
      _vertex0 = triangle.Vertex0;
      _vertex1 = triangle.Vertex1;
      _vertex2 = triangle.Vertex2;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new TriangleShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (TriangleShape)sourceShape;
      _vertex0 = source.Vertex0;
      _vertex1 = source.Vertex1;
      _vertex2 = source.Vertex2;
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      // Note: Compute AABB in world space
      Vector3F vertex0 = pose.ToWorldPosition(_vertex0 * scale);
      Vector3F vertex1 = pose.ToWorldPosition(_vertex1 * scale);
      Vector3F vertex2 = pose.ToWorldPosition(_vertex2 * scale);
      Vector3F minimum = Vector3F.Min(vertex0, Vector3F.Min(vertex1, vertex2));
      Vector3F maximum = Vector3F.Max(vertex0, Vector3F.Max(vertex1, vertex2));
      return new Aabb(minimum, maximum);
    }


    /// <summary>
    /// Gets a support point for a given direction.
    /// </summary>
    /// <param name="direction">
    /// The direction for which to get the support point. The vector does not need to be normalized.
    /// The result is undefined if the vector is a zero vector.
    /// </param>
    /// <returns>
    /// A support point regarding the given direction.
    /// </returns>
    /// <remarks>
    /// <para>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </para>
    /// </remarks>
    public override Vector3F GetSupportPoint(Vector3F direction)
    {
      float distance0 = Vector3F.Dot(direction, _vertex0);
      float distance1 = Vector3F.Dot(direction, _vertex1);
      float distance2 = Vector3F.Dot(direction, _vertex2);

      if (distance0 >= distance1 && distance0 >= distance2)
        return _vertex0;

      if (distance1 >= distance2)
        return _vertex1;

      return _vertex2;
    }


    /// <summary>
    /// Gets a support point for a given normalized direction vector.
    /// </summary>
    /// <param name="directionNormalized">
    /// The normalized direction vector for which to get the support point.
    /// </param>
    /// <returns>
    /// A support point regarding the given direction.
    /// </returns>
    /// <remarks>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </remarks>
    public override Vector3F GetSupportPointNormalized(Vector3F directionNormalized)
    {
      float distance0 = Vector3F.Dot(directionNormalized, _vertex0);
      float distance1 = Vector3F.Dot(directionNormalized, _vertex1);
      float distance2 = Vector3F.Dot(directionNormalized, _vertex2);

      if (distance0 >= distance1 && distance0 >= distance2)
        return _vertex0;

      if (distance1 >= distance2)
        return _vertex1;

      return _vertex2;
    }


    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used</param>
    /// <returns>0</returns>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      return 0;
    }
    


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(new Triangle(this), true);
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
      return String.Format(CultureInfo.InvariantCulture, "TriangleShape {{ Vertex0 = {0}, Vertex1 = {1}, Vertex2 = {2} }}", _vertex0, _vertex1, _vertex2);
    }
    #endregion
  }
}
