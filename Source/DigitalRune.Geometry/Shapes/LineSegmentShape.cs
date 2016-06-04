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
  /// Represents a line segment.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class can be used if an <see cref="IGeometricObject"/> with a line segment shape is
  /// needed. Use the <see cref="LineSegment"/> structure instead if you need a lightweight
  /// representation of a line segment (avoids allocating memory on the heap).
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class LineSegmentShape : ConvexShape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point (center of line segment).
    /// </summary>
    /// <value>The center of the line segment.</value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space). 
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get { return (_start + _end) / 2; }
    }


    /// <summary>
    /// Gets or sets the start point.
    /// </summary>
    /// <value>The start point.</value>
    public Vector3F Start
    {
      get { return _start; }
      set
      {
        if (_start != value)
        {
          _start = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3F _start;


    /// <summary>
    /// Gets or sets the end point.
    /// </summary>
    /// <value>The end point.</value>
    public Vector3F End
    {
      get { return _end; }
      set
      {
        if (_end != value)
        {
          _end = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3F _end;


    /// <summary>
    /// Gets the length.
    /// </summary>
    /// <value>The length.</value>
    public float Length
    {
      get { return (_end - _start).Length; }
    }


    /// <summary>
    /// Gets the squared length.
    /// </summary>
    /// <value>The squared length.</value>
    public float LengthSquared
    {
      get { return (_end - _start).LengthSquared; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="LineSegmentShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="LineSegmentShape"/> class.
    /// </summary>
    /// <remarks>
    /// Creates a line segment where <see cref="Start"/> and <see cref="End"/> are (0, 0, 0).
    /// </remarks>
    public LineSegmentShape()
      : this (Vector3F.Zero, Vector3F.Zero)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LineSegmentShape"/> class from two points.
    /// </summary>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    public LineSegmentShape(Vector3F start, Vector3F end)
    {
      _start = start;
      _end = end;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LineSegmentShape"/> class from a 
    /// <see cref="LineSegment"/>.
    /// </summary>
    /// <param name="lineSegment">
    /// The line segment from which properties are copied.
    /// </param>
    public LineSegmentShape(LineSegment lineSegment)
    {
      _start = lineSegment.Start;
      _end = lineSegment.End;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new LineSegmentShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (LineSegmentShape)sourceShape;
      _start = source.Start;
      _end = source.End;
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      Vector3F worldStart = pose.ToWorldPosition(_start * scale);
      Vector3F worldEnd = pose.ToWorldPosition(_end * scale);
      Vector3F minimum = Vector3F.Min(worldStart, worldEnd);
      Vector3F maximum = Vector3F.Max(worldStart, worldEnd);
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
      if (Vector3F.Dot(_start, direction) > Vector3F.Dot(_end, direction))
        return _start;
      else
        return _end;
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
      if (Vector3F.Dot(_start, directionNormalized) > Vector3F.Dot(_end, directionNormalized))
        return _start;
      else
        return _end;
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
    /// <remarks>
    /// This creates a mesh with a single degenerate triangle that represents the line segment.
    /// </remarks>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      // Make a mesh with 1 degenerate triangle
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(
        new Triangle
        {
          Vertex0 = Start,
          Vertex1 = Start,
          Vertex2 = End,
        }, 
        true, 
        Numeric.EpsilonF,
        false);
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
      return String.Format(CultureInfo.InvariantCulture, "LineSegmentShape {{ Start = {0}, End = {1} }}", _start, _end);
    }
    #endregion
  }
}
