// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a 2-dimensional elliptic arc segment (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="ArcSegment2F"/> draws an elliptic arc between the start point 
  /// <see cref="Point1"/> and the end point <see cref="Point2"/>.
  /// </para>
  /// <para>
  /// To draw a full ellipse, set <see cref="Point1"/> equal to <see cref="Point2"/> and 
  /// <see cref="IsLargeArc"/> to <see langword="true"/>. An ellipse will be drawn so that
  /// <see cref="Point1"/> and <see cref="Point2"/> lie on the positive x-axis of the ellipse.
  /// </para>
  /// </remarks>
  public class ArcSegment2F : ICurve<float, Vector2F>, IRecyclable
  {
    // Notes:
    // http://www.w3.org/TR/SVG/implnote.html#ArcImplementationNotes describes how 
    // the arc is computed.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isDirty = true;
    private float _sinTheta;       // sin(RotationAngle)
    private float _cosTheta;       // cos(RotationAngle)
    private Vector2F _center;      // The center of the ellipse.
    private float _rX;             // Radius.X with out-of-range corrections.
    private float _rY;             // Radius.Y with out-of-range corrections.
    private float _angle1;         // The angle of Point1.
    private float _angleDelta;     // The angle from Point1 to Point2.
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the start point.
    /// </summary>
    /// <value>The start point. The default value is (1, 0).</value>
    public Vector2F Point1
    {
      get { return _point1; }
      set
      {
        if (_point1 == value)
          return;

        _point1 = value;
        _isDirty = true;
      }
    }
    private Vector2F _point1;


    /// <summary>
    /// Gets or sets the end point.
    /// </summary>
    /// <value>The end point. The default value is (0, 1).</value>
    public Vector2F Point2
    {
      get { return _point2; }
      set
      {
        if (_point2 == value)
          return;

        _point2 = value;
        _isDirty = true;
      }
    }
    private Vector2F _point2;




    /// <summary>
    /// Gets or sets a value that indicates whether the arc should be greater than 180 degrees.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the arc should be greater than 180 degrees; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    public bool IsLargeArc
    {
      get { return _isLargeArc; }
      set
      {
        if (_isLargeArc == value)
          return;

        _isLargeArc = value;
        _isDirty = true;
      }
    }
    private bool _isLargeArc;


    /// <summary>
    /// Gets or sets the radii (semi-major and semi-minor axis) of the ellipse.
    /// </summary>
    /// <value>The radii of the ellipse. The default value is (1, 1).</value>
    /// <exception cref="ArgumentException">
    /// A radius is 0 or negative.
    /// </exception>
    public Vector2F Radius
    {
      get { return _radius; }
      set
      {
        if (_radius == value)
          return;
        if (Radius.X <= 0 || Radius.Y <= 0)
          throw new ArgumentException("The radius must be greater than 0.");

        _radius = value;
        _isDirty = true;
      }
    }
    private Vector2F _radius;


    /// <summary>
    /// Gets or sets the angle from the current x-axis to the x-axis of the ellipse.
    /// </summary>
    /// <value>
    /// The angle (in radians) from the current x-axis to the x-axis of the ellipse. The default
    /// value is 0.
    /// </value>
    public float RotationAngle
    {
      get { return _rotationAngle; }
      set
      {
        if (_rotationAngle == value)
          return;

        _rotationAngle = value;
        _isDirty = true;
      }
    }
    private float _rotationAngle;


    /// <summary>
    /// Gets or sets a value indicating whether the arc is drawn in clockwise or counter-clockwise
    /// direction.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the arc is drawn in clockwise direction; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    public bool SweepClockwise
    {
      get { return _sweepClockwise; }
      set
      {
        if (_sweepClockwise == value)
          return;

        _sweepClockwise = value;
        _isDirty = true;
      }
    }
    private bool _sweepClockwise;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ArcSegment2F"/> class.
    /// </summary>
    public ArcSegment2F()
    {
      _radius = new Vector2F(1);
      _point1 = new Vector2F(1, 0);
      _point2 = new Vector2F(0, 1);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void ComputeParameters()
    {
      if (!_isDirty)
        return;

      _isDirty = false;

      _cosTheta = (float)Math.Cos(-RotationAngle);
      _sinTheta = (float)Math.Sin(-RotationAngle);

      _rX = Radius.X;
      _rY = Radius.Y;

      // Handle the case where Point1 == Point2. --> Draw nothing or 
      // a full circle. (Note: SVG or WPF do not draw a full circle.)
      if (Vector2F.AreNumericallyEqual(Point1, Point2))
      {
        _center.X = Point1.X - _rX * _cosTheta;
        _center.Y = Point1.Y - _rX * _sinTheta;

        _angle1 = 0;
        if (!IsLargeArc)
        {
          _angleDelta = 0;
        }
        else
        {
          if (SweepClockwise)
            _angleDelta = -ConstantsF.TwoPi;
          else
            _angleDelta = ConstantsF.TwoPi;
        }
        return;
      }

      // Step 1: Compute (x1′, y1′)
      var p = new Matrix22F(_cosTheta, _sinTheta,
                           -_sinTheta, _cosTheta)
              * new Vector2F((Point1.X - Point2.X) / 2,
                             (Point1.Y - Point2.Y) / 2);

      // Ensure radii are large enough.
      var pX2 = p.X * p.X;
      var pY2 = p.Y * p.Y;
      float lambda = pX2 / (_rX * _rX) + pY2 / (_rY * _rY);
      if (lambda > 1)
      {
        var sqrtLambda = (float)Math.Sqrt(lambda);
        _rX = sqrtLambda * _rX;
        _rY = sqrtLambda * _rY;
      }

      // Step 2: Compute (cx′, cy′)
      var signC = (IsLargeArc != !SweepClockwise) ? +1 : -1;
      var rX2 = _rX * _rX;
      var rY2 = _rY * _rY;
      var c = signC * (float)Math.Sqrt(Math.Max((rX2 * rY2 - rX2 * pY2 - rY2 * pX2) / (rX2 * pY2 + rY2 * pX2), 0))
              * new Vector2F(_rX * p.Y / _rY, -_rY * p.X / _rX);

      // Step 3: Compute (cx, cy) from (cx′, cy′)
      _center = new Matrix22F(_cosTheta, -_sinTheta,
                              _sinTheta, _cosTheta)
                * c + new Vector2F((Point1.X + Point2.X) / 2, (Point1.Y + Point2.Y) / 2);

      // Step 4: Compute θ1 and Δθ
      _angle1 = GetAngle(new Vector2F(1, 0), new Vector2F((p.X - c.X) / _rX, (p.Y - c.Y) / _rY));
      _angleDelta = GetAngle(new Vector2F((p.X - c.X) / _rX, (p.Y - c.Y) / _rY),
                              new Vector2F((-p.X - c.X) / _rX, (-p.Y - c.Y) / _rY)) % ConstantsF.TwoPi;

      if (SweepClockwise && _angle1 > 0)
        _angle1 -= ConstantsF.TwoPi;

      if (!SweepClockwise && _angleDelta < 0)
        _angleDelta += ConstantsF.TwoPi;

      // The above code should be according to the SVG implementation notes. However, 
      // we need to make some more adjustments:

      // Make sure that _angleDelta is the correct small or large angle.
      if (IsLargeArc && Math.Abs(_angleDelta) < ConstantsF.Pi
          || !IsLargeArc && Math.Abs(_angleDelta) >= ConstantsF.Pi)
      {
        if (_angleDelta >= 0)
          _angleDelta = -ConstantsF.TwoPi + _angleDelta;
        else
          _angleDelta = ConstantsF.TwoPi + _angleDelta;
      }

      // Make sure that the sweep direction is correct.
      if (!SweepClockwise && _angleDelta < 0)
        _angleDelta = ConstantsF.TwoPi + _angleDelta;
      else if (SweepClockwise && _angleDelta > 0)
        _angleDelta = -ConstantsF.TwoPi + _angleDelta;
    }


    private static float GetAngle(Vector2F u, Vector2F v)
    {
      var sign = Math.Sign(u.X * v.Y - u.Y * v.X);
      if (sign == 0)
        sign = -1;

      return sign * (float)Math.Acos(MathHelper.Clamp(Vector2F.Dot(u, v) / (u.Length * v.Length), -1, 1));
    }


    /// <summary>
    /// Computes a point on the curve.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>The curve point.</returns>
    public Vector2F GetPoint(float parameter)
    {
      if (Numeric.IsZero(parameter))
        return Point1;
      if (Numeric.AreEqual(parameter, 1))
        return Point2;

      ComputeParameters();

      float angle = _angle1 + _angleDelta * parameter;
      float cosAngle = (float)Math.Cos(angle);
      float sinAngle = (float)Math.Sin(angle);

      return new Vector2F(
        _center.X + _rX * cosAngle * _cosTheta - _rY * sinAngle * _sinTheta,
        _center.Y + _rX * cosAngle * _sinTheta + _rY * sinAngle * _cosTheta);
    }


    /// <inheritdoc/>
    public Vector2F GetTangent(float parameter)
    {
      ComputeParameters();

      float angle = _angle1 + _angleDelta * parameter;
      float cosAngle = (float)Math.Cos(angle);
      float sinAngle = (float)Math.Sin(angle);

      return new Vector2F(
        _angleDelta * (_rX * -sinAngle * _cosTheta - _rY * cosAngle * _sinTheta),
        _angleDelta * (_rX * -sinAngle * _sinTheta + _rY * cosAngle * _cosTheta));
    }


    /// <inheritdoc/>
    public float GetLength(float start, float end, int maxNumberOfIterations, float tolerance)
    {
      return CurveHelper.GetLength(this, start, end, 2, maxNumberOfIterations, tolerance);
    }


    /// <inheritdoc/>
    public void Flatten(ICollection<Vector2F> points, int maxNumberOfIterations, float tolerance)
    {
      CurveHelper.Flatten(this, points, maxNumberOfIterations, tolerance);
    }


    /// <summary>
    /// Returns a <see cref="String" /> that represents this instance.
    /// </summary>
    /// <returns>A <see cref="String" /> that represents this instance.</returns>
    public override string ToString()
    {
      return string.Format(
        CultureInfo.InvariantCulture,
        "Point1: {0}, Point2: {1}, Radius: {2}, IsLargeArc: {3}, SweepClockwise: {4}, RotationAngle: {5}",
        Point1, Point2, Radius, IsLargeArc, SweepClockwise, RotationAngle);
    }
    #endregion


    //--------------------------------------------------------------
    #region Resource Pooling
    //--------------------------------------------------------------

    private static ResourcePool<ArcSegment2F> _pool;


    /// <summary>
    /// Creates an instance of the <see cref="ArcSegment2F"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="ArcSegment2F"/> class.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap. 
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle"/> when the instance is no longer 
    /// needed.
    /// </para>
    /// </remarks>
    public static ArcSegment2F Create()
    {
      if (_pool == null)
        _pool = new ResourcePool<ArcSegment2F>(
          () => new ArcSegment2F(),   // Create
          null,                       // Initialize
          null);                      // Uninitialize

      return _pool.Obtain();
    }


    /// <inheritdoc/>
    public void Recycle()
    {
      Point1 = new Vector2F();
      Point2 = new Vector2F();

      _pool.Recycle(this);
    }
    #endregion
  }
}
