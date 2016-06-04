// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a scaled convex shape.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This shape can be used to scale any <see cref="ConvexShape"/> stored in the property
  /// <see cref="Shape"/>. For performance reasons this shape should not be used if the child shape
  /// can be scaled directly. For example, if a box should be scaled, it is more efficient to change
  /// the box extent (e.g. <see cref="BoxShape.WidthX"/>) directly.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class ScaledConvexShape : ConvexShape
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
        return _shape.InnerPoint * _scale;
      }
    }


    /// <summary>
    /// Gets or sets the scale factor.
    /// </summary>
    /// <value>
    /// The scale factors for scaling in x, y and z. The default value is (1, 1, 1)
    /// which means "no scaling".
    /// </value>
    public Vector3F Scale
    {
      get { return _scale; }
      set
      {
        if (_scale != value)
        {
          _scale = value;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private Vector3F _scale;


    /// <summary>
    /// Gets or sets the convex shape that is scaled.
    /// </summary>
    /// <value>
    /// The convex shape that is scaled. The default shape is a simple <see cref="PointShape"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public ConvexShape Shape
    {
      get { return _shape; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (_shape != value)
        {
          if (_shape != null)
            _shape.Changed -= OnChildShapeChanged;

          _shape = value;
          _shape.Changed += OnChildShapeChanged;
          OnChanged(ShapeChangedEventArgs.Empty);
        }
      }
    }
    private ConvexShape _shape;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ScaledConvexShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ScaledConvexShape"/> class.
    /// </summary>
    public ScaledConvexShape()
    {
      // Note: Virtual OnChanged() must not be called in constructor.
      _shape = new PointShape();
      _shape.Changed += OnChildShapeChanged;
      _scale = Vector3F.One;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ScaledConvexShape"/> class from two geometric
    /// objects.
    /// </summary>
    /// <param name="shape">The convex shape that should be scaled.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="shape"/> is <see langword="null"/>.
    /// </exception>
    public ScaledConvexShape(ConvexShape shape)
    {
      if (shape == null)
        throw new ArgumentNullException("shape");

      // Note: Virtual OnChanged() must not be called in constructor.
      _shape = shape;
      _shape.Changed += OnChildShapeChanged;
      _scale = Vector3F.One; 
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ScaledConvexShape"/> class from two geometric
    /// objects.
    /// </summary>
    /// <param name="shape">The convex shape that should be scaled.</param>
    /// <param name="scale">The scale of the convex shape.</param>
    /// <exception cref="ArgumentNullException">
    /// 	<paramref name="shape"/> is <see langword="null"/>.
    /// </exception>
    public ScaledConvexShape(ConvexShape shape, Vector3F scale)
    {
      if (shape == null)
        throw new ArgumentNullException("shape");

      // Note: Virtual OnChanged() must not be called in constructor.
      _shape = shape;
      _shape.Changed += OnChildShapeChanged;
      _scale = scale; 
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      var shape = (ConvexShape)Shape.Clone();
      var scale = Scale;
      return new ScaledConvexShape(shape, scale);
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
    }
    #endregion


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
      return _shape.GetSupportPoint(direction, _scale);
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
      if (_scale.X == _scale.Y && _scale.Y == _scale.Z)
      {
        // Uniform scaling: Simply scale the support point position.
        // No need to change directionNormalized. We can use GetSupportPointNormalized.
        return _shape.GetSupportPointNormalized(directionNormalized) * _scale;
      }
      else
      {
        // Non-uniform scaling: We have to scale the support direction see comments in 
        // GetSupportPoint(). We cannot use GetSupportPointNORMALIZED().
        return _shape.GetSupportPoint(directionNormalized, _scale);
      }
    }


    /// <inheritdoc/>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      Vector3F scale = Vector3F.Absolute(Scale);
      return Shape.GetVolume(relativeError, iterationLimit) * scale.X * scale.Y * scale.Z;
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
      OnChanged(eventArgs);
    }
    #endregion
  }
}
