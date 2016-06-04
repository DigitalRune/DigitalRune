// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a convex hull of a set of points.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This shape is a collection of points from which the convex hull is computed.
  /// </para>
  /// <para>
  /// A <see cref="ConvexHullOfPoints"/> is similar to a <see cref="ConvexPolyhedron"/> except that
  /// the shape can be changed dynamically. Points in the <see cref="ConvexHullOfPoints"/> can be 
  /// added or removed at runtime.
  /// </para>
  /// <para>
  /// Use a <see cref="ConvexHullOfPoints"/> if the points in the shape need to be modified at 
  /// runtime. Use a <see cref="ConvexPolyhedron"/> if the set of points is fixed and a high
  /// performance is required.
  /// </para>
  /// </remarks>
  //[Serializable]   // Property Points is not serializable because interfaces are not automatically serializable.
  public class ConvexHullOfPoints : ConvexShape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The cached local space AABB
    private Aabb _aabbLocal = new Aabb(new Vector3F(float.NaN), new Vector3F(float.NaN));

    // The cached inner point.
    private Vector3F _innerPoint = new Vector3F(float.NaN);
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>
    /// An inner point which is the average of all points; or (0, 0, 0) if <see cref="Points"/> is
    /// empty.
    /// </value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get
      {
        // Check if inner point is cached.
        if (Numeric.IsNaN(_innerPoint.X))
        {
          // Compute the average of all points.
          int numberOfPoints = _points.Count;
          Vector3F innerPoint = new Vector3F();
          if (numberOfPoints > 0)
          {
            for (int i = 0; i < numberOfPoints; i++)
            {
              Vector3F point = _points[i];
              innerPoint += point;
            }

            innerPoint /= numberOfPoints;
          }

          _innerPoint = innerPoint;
        }

        return _innerPoint;
      }
    }


    /// <summary>
    /// Gets or sets the list with the points contained in the convex hull.
    /// </summary>
    /// <value>
    /// A list of all points contained in the convex hull. Must not be <see langword="null"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public IList<Vector3F> Points
    {
      get { return _points; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (_points != value)
        {
          _points = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private IList<Vector3F> _points;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ConvexHullOfPoints"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ConvexHullOfPoints"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor creates a new empty <see cref="ConvexHullOfPoints"/>.
    /// </remarks>
    public ConvexHullOfPoints()
    {
      _points = new List<Vector3F>();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConvexHullOfPoints"/> class from a sequence
    /// of points.
    /// </summary>
    /// <param name="points">
    /// A collection of points which are copied into the <see cref="Points"/> list.
    /// </param>
    public ConvexHullOfPoints(IEnumerable<Vector3F> points)
    {
      _points = new List<Vector3F>();
      if (points != null)
        foreach (Vector3F p in points)
          _points.Add(p);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConvexHullOfPoints"/> class from a list of 
    /// points.
    /// </summary>
    /// <param name="points">
    /// The point list. A reference to this list is stored in <see cref="Points"/>. The list is not
    /// copied.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    public ConvexHullOfPoints(IList<Vector3F> points)
    {
      if (points == null)
        throw new ArgumentNullException("points");

      _points = points;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new ConvexHullOfPoints();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (ConvexHullOfPoints)sourceShape;
      int numberOfPoints = source.Points.Count;
      for (int i = 0; i < numberOfPoints; i++)
        _points.Add(source._points[i]);
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      // Recompute local cached AABB if it is invalid.
      if (Numeric.IsNaN(_aabbLocal.Minimum.X))
        _aabbLocal = base.GetAabb(Vector3F.One, Pose.Identity);

      // Apply scale and pose to AABB.
      return _aabbLocal.GetAabb(scale, pose);
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
      return GetSupportPointInternal(ref direction);
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
    public override Vector3F GetSupportPointNormalized(Vector3F directionNormalized)
    {
      return GetSupportPointInternal(ref directionNormalized);
    }


    private Vector3F GetSupportPointInternal(ref Vector3F direction)
    {
      // The direction vector does not need to be normalized: Below we project the points onto
      // the direction vector and measure the length of the projection. However, we do not need
      // the correct length, we only need a value which we can compare.

      // Return point with the largest distance in the given direction.
      Vector3F supportVertex = new Vector3F();
      float maxDistance = float.NegativeInfinity;
      int numberOfPoints = _points.Count;
      for (int i = 0; i < numberOfPoints; i++)
      {
        Vector3F vertex = _points[i];
        float distance = Vector3F.Dot(vertex, direction);
        if (distance > maxDistance)
        {
          supportVertex = vertex;
          maxDistance = distance;
        }
      }

      return supportVertex;
    }


    /// <summary>
    /// Invalidates this instance.
    /// </summary>
    /// <remarks>
    /// This method must be called if the content of <see cref="Points"/> was changed. 
    /// This method calls <see cref="OnChanged"/>.
    /// </remarks>
    public void Invalidate()
    {
      OnChanged(ShapeChangedEventArgs.Empty);
    }


    /// <inheritdoc/>
    protected override void OnChanged(ShapeChangedEventArgs eventArgs)
    {
      // Set cached AABB to "invalid".
      _aabbLocal = new Aabb(new Vector3F(float.NaN), new Vector3F(float.NaN));
      _innerPoint = new Vector3F(float.NaN);

      base.OnChanged(eventArgs);
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      DcelMesh mesh = GeometryHelper.CreateConvexHull(Points);
      return mesh.ToTriangleMesh();
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "ConvexHullOfPoints {{ Count = {0} }}", _points.Count);
    }
    #endregion
  }
}
