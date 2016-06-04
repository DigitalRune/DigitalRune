// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents the <i>Minkowski Sum</i> of two geometric objects.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This shape is defined as the <i>Minkowski Sum</i> of two geometric objects A and B: A + B.
  /// The shapes of A and B must be of type <see cref="ConvexShape"/>.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]   
#endif
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class MinkowskiSumShape : ConvexShape
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
      get
      {
        // Return the sum of child inner points.
        // Return the difference of child inner points.
        Vector3F innerPointA = _objectA.Pose.ToWorldPosition(_objectA.Shape.InnerPoint);
        Vector3F innerPointB = _objectB.Pose.ToWorldPosition(_objectB.Shape.InnerPoint);
        return innerPointA + innerPointB;
      }
    }


    /// <summary>
    /// Gets or sets the first <see cref="IGeometricObject"/>.
    /// </summary>
    /// <value>The first <see cref="IGeometricObject"/>.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="GeometryException">
    /// <paramref name="value"/> is not convex.
    /// </exception>
    public IGeometricObject ObjectA
    {
      get { return _objectA; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (_objectA != value)
        {
          if (_objectA != null)
          {
            _objectA.PoseChanged -= OnChildPoseChanged;
            _objectA.ShapeChanged -= OnChildShapeChanged;
          }

          _objectA = value;
          _objectA.PoseChanged += OnChildPoseChanged;
          _objectA.ShapeChanged += OnChildShapeChanged;

          CheckShapes();
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private IGeometricObject _objectA;


    /// <summary>
    /// Gets or sets the second <see cref="GeometricObject"/>.
    /// </summary>
    /// <value>The second <see cref="GeometricObject"/>.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="GeometryException"><paramref name="value"/> is not convex.
    /// </exception>
    public IGeometricObject ObjectB
    {
      get { return _objectB; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (_objectB != value)
        {
          if (_objectB != null)
          {
            _objectB.PoseChanged -= OnChildPoseChanged;
            _objectB.ShapeChanged -= OnChildShapeChanged;
          }

          _objectB = value;
          _objectB.PoseChanged += OnChildPoseChanged;
          _objectB.ShapeChanged += OnChildShapeChanged;

          CheckShapes();
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private IGeometricObject _objectB;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="MinkowskiSumShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="MinkowskiSumShape"/> class.
    /// </summary>
    public MinkowskiSumShape()
    {
      _objectA = new GeometricObject(new PointShape());
      _objectA.PoseChanged += OnChildPoseChanged;
      _objectA.ShapeChanged += OnChildShapeChanged;

      _objectB = new GeometricObject(new PointShape());
      _objectB.PoseChanged += OnChildPoseChanged;
      _objectB.ShapeChanged += OnChildShapeChanged;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="MinkowskiSumShape"/> class from two geometric
    /// objects.
    /// </summary>
    /// <param name="objectA">The first geometric object.</param>
    /// <param name="objectB">The second geometric object.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="GeometryException">
    /// <paramref name="objectA"/> is not convex.
    /// </exception>
    /// <exception cref="GeometryException">
    /// <paramref name="objectB"/> is not convex.
    /// </exception>
    public MinkowskiSumShape(IGeometricObject objectA, IGeometricObject objectB)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      _objectA = objectA;
      _objectA.PoseChanged += OnChildPoseChanged;
      _objectA.ShapeChanged += OnChildShapeChanged;

      _objectB = objectB;
      _objectB.PoseChanged += OnChildPoseChanged;
      _objectB.ShapeChanged += OnChildShapeChanged;
      CheckShapes();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      var cloneA = ObjectA.Clone();
      var cloneB = ObjectB.Clone();
      return new MinkowskiSumShape(cloneA, cloneB);
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
    }
    #endregion


    /// <summary>
    /// Checks if the child shapes are convex.
    /// </summary>
    /// <exception cref="GeometryException">
    /// <see cref="ObjectA"/> is not convex.
    /// </exception>
    /// <exception cref="GeometryException">
    /// <see cref="ObjectB"/> is not convex.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void CheckShapes()
    {
      if (_objectA != null && _objectA.Shape is ConvexShape == false)
        throw new GeometryException("Geometric object A of a Minkowski sum must have a convex shape.");
      if (_objectB != null && _objectB.Shape is ConvexShape == false)
        throw new GeometryException("Geometric object B of a Minkowski sum must have a convex shape.");
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
      Vector3F directionLocalA = _objectA.Pose.ToLocalDirection(directionNormalized);
      Vector3F directionLocalB = _objectB.Pose.ToLocalDirection(directionNormalized);
      Vector3F pointALocalA = ((ConvexShape)_objectA.Shape).GetSupportPointNormalized(directionLocalA);
      Vector3F pointBLocalB = ((ConvexShape)_objectB.Shape).GetSupportPointNormalized(directionLocalB);
      Vector3F pointA = _objectA.Pose.ToWorldPosition(pointALocalA);
      Vector3F pointB = _objectB.Pose.ToWorldPosition(pointBLocalB);
      return pointA + pointB;
    }


    /// <summary>
    /// Called when child pose was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="EventArgs"/> instance containing the event data.
    /// </param>
    private void OnChildPoseChanged(object sender, EventArgs eventArgs)
    {
      OnChanged(ShapeChangedEventArgs.Empty);
    }


    /// <summary>
    /// Called when child shape was changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="ShapeChangedEventArgs"/> instance containing the event data.
    /// </param>
    private void OnChildShapeChanged(object sender, ShapeChangedEventArgs eventArgs)
    {
      CheckShapes();
      OnChanged(eventArgs);
    }
    #endregion
  }
}
