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
  /// Represents an infinite line.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class can be used if an <see cref="IGeometricObject"/> with a line shape is needed. Use
  /// the <see cref="Line"/> structure instead if you need a lightweight representation of a line
  /// (avoids allocating memory on the heap).
  /// </para>
  /// <para>
  /// The line is defined using a point on the line and the direction. The <see cref="Direction"/> 
  /// is always stored as a normalized vector.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class LineShape : Shape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a point on the line.
    /// </summary>
    /// <value>A point on the line.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnLine")]
    public Vector3F PointOnLine
    {
      get { return _pointOnLine; }
      set
      {
        if (_pointOnLine != value)
        {
          _pointOnLine = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3F _pointOnLine;


    /// <summary>
    /// Gets or sets the direction.
    /// </summary>
    /// <value>The direction of the line. Must be normalized.</value>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is not normalized.
    /// </exception>
    public Vector3F Direction
    {
      get { return _direction; }
      set
      {
        if (!value.IsNumericallyNormalized)
          throw new ArgumentException("The line direction must be normalized.");

        if (_direction != value)
        {
          _direction = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3F _direction;


    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>An inner point - same as <see cref="PointOnLine"/>.</value>
    public override Vector3F InnerPoint
    {
      get { return _pointOnLine; }
    }


    /// <summary>
    /// Gets or sets the length of the mesh that represents a <see cref="LineShape"/>.
    /// </summary>
    /// <value>The length of the mesh.</value>
    /// <remarks>
    /// See <see cref="OnGetMesh"/> for more information.
    /// </remarks>
    public static float MeshSize { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //-------------------------------------------------------------- 

    /// <summary>
    /// Initializes static members of the <see cref="LineShape"/> class.
    /// </summary>
    static LineShape()
    {
      MeshSize = 1000;
    }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="LineShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="LineShape"/> class.
    /// </summary>
    /// <remarks>
    /// Creates a line through the origin in x-axis direction.
    /// </remarks>
    public LineShape()
      : this(Vector3F.Zero, Vector3F.UnitX)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LineShape"/> class from a point and direction.
    /// </summary>
    /// <param name="pointOnLine">A point on the line.</param>
    /// <param name="direction">The direction.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="direction"/> is not normalized.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnLine")]
    public LineShape(Vector3F pointOnLine, Vector3F direction)
    {
      if (!direction.IsNumericallyNormalized)
        throw new ArgumentException("The line direction must be normalized.", "direction");

      _pointOnLine = pointOnLine;
      _direction = direction;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LineShape"/> class from a <see cref="Line"/>.
    /// </summary>
    /// <param name="line">The line structure from which properties are copied.</param>
    /// <exception cref="ArgumentException">
    /// The line direction is not normalized.
    /// </exception>
    public LineShape(Line line)
    {
      if (!line.Direction.IsNumericallyNormalized)
        throw new ArgumentException("The line direction must be normalized.", "line");

      _pointOnLine = line.PointOnLine;
      _direction = line.Direction;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new LineShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (LineShape)sourceShape;
      _pointOnLine = source.PointOnLine;
      _direction = source.Direction;
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      // Note: Compute AABB in world space.
      Vector3F direction = pose.ToWorldDirection(_direction * scale);
      Vector3F pointOnLine = pose.ToWorldPosition(_pointOnLine * scale);

      // Most of the time the AABB fills the whole space. Only when the line is axis-aligned then
      // the AABB is different.
      Vector3F minimum = new Vector3F(float.NegativeInfinity);
      Vector3F maximum = new Vector3F(float.PositiveInfinity);

      // Using numerical comparison we "clamp" the line into an axis-aligned plane if possible.
      if (Numeric.IsZero(direction.X))
      {
        minimum.X = pointOnLine.X;
        maximum.X = pointOnLine.X;
      }
      if (Numeric.IsZero(direction.Y))
      {
        minimum.Y = pointOnLine.Y;
        maximum.Y = pointOnLine.Y;
      }
      if (Numeric.IsZero(direction.Z))
      {
        minimum.Z = pointOnLine.Z;
        maximum.Z = pointOnLine.Z;
      }

      return new Aabb(minimum, maximum);
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
    /// This method returns a mesh with a single degenerate triangle. The triangle represents a
    /// line with the length <see cref="MeshSize"/>. The triangle is centered on 
    /// <see cref="PointOnLine"/>.
    /// </remarks>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      Vector3F start = PointOnLine - Direction * (MeshSize / 2);
      Vector3F end = PointOnLine + Direction * (MeshSize / 2);
      // Make a mesh with 1 degenerate triangle
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(new Triangle
      {
        Vertex0 = start,
        Vertex1 = start,
        Vertex2 = end,
      }, true, Numeric.EpsilonF, false);
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
      return String.Format(CultureInfo.InvariantCulture, "LineShape {{ PointOnLine = {0}, Direction = {1} }}", _pointOnLine, _direction);
    }
    #endregion
  }
}
