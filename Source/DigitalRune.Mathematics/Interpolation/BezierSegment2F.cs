// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a single segment of a 2-dimensional cubic Bézier splines (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="BezierSegment2F"/> can be used to smoothly interpolate between two points.
  /// </para>
  /// <para>
  /// It is a curve that connects two points: <see cref="Point1"/> and <see cref="Point2"/>. Two 
  /// more points are required to define the curvature of the spline: <see cref="ControlPoint1"/> 
  /// and <see cref="ControlPoint2"/>. The curve smoothly interpolates between <see cref="Point1"/> 
  /// and <see cref="Point2"/>.
  /// </para>
  /// <para>
  /// The curve is a function <i>point = C(parameter)</i> that takes a scalar parameter and returns 
  /// a point on the curve (see <see cref="GetPoint"/>). The curve parameter lies in the interval 
  /// [0,1]; it is also known as <i>interpolation parameter</i>, <i>interpolation factor</i> or 
  /// <i>weight of the target point</i>. <i>C(0)</i> returns the start point <see cref="Point1"/>; 
  /// <i>C(1)</i> returns the end point <see cref="Point2"/>.
  /// </para>
  /// <para>
  /// The curve is defined as:
  /// </para>
  /// <para>
  /// C(<i>u</i>) = (1 - <i>u</i>)<sup>3</sup> <i>p<sub>1</sub></i>
  ///               + 3u (1 - <i>u</i>)<sup>2</sup> <i>cp<sub>1</sub></i>
  ///               + 3u<sup>2</sup> (1 - <i>u</i>) <i>cp<sub>2</sub></i>
  ///               + u<sup>3</sup> <i>p<sub>2</sub></i>
  /// </para>
  /// <para>
  /// where <i>u</i> is the curve parameter, <i>p</i> are the start/end points and 
  /// <i>cp</i> are the control points.
  /// </para>
  /// </remarks>
  public class BezierSegment2F : ICurve<float, Vector2F>, IRecyclable
  {
    /// <summary>
    /// Gets or sets the start point.
    /// </summary>
    public Vector2F Point1 { get; set; }


    /// <summary>
    /// Gets or sets the end point.
    /// </summary>
    public Vector2F Point2 { get; set; }


    /// <summary>
    /// Gets or sets the first control point.
    /// </summary>
    public Vector2F ControlPoint1 { get; set; }


    /// <summary>
    /// Gets or sets the second control point.
    /// </summary>
    public Vector2F ControlPoint2 { get; set; }


    /// <summary>
    /// Computes a point on the curve.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>The curve point.</returns>
    public Vector2F GetPoint(float parameter)
    {
      // Polynomial form:
      float u = parameter;
      float u2 = u * u;
      float u3 = u2 * u;
      float uNeg = (1 - u);
      float uNeg2 = uNeg * uNeg;
      float uNeg3 = uNeg2 * uNeg;
      Vector2F result = uNeg3 * Point1 + 3 * u * uNeg2 * ControlPoint1 + 3 * u2 * uNeg * ControlPoint2 + u3 * Point2;
      return result;
    }


    /// <inheritdoc/>
    public Vector2F GetTangent(float parameter)
    {
      float u = parameter;
      float u2 = u * u;
      Vector2F result = (-3 + 6 * u - 3 * u2) * Point1 + (3 - 12 * u + 9 * u2) * ControlPoint1 + (6 * u - 9 * u2) * ControlPoint2 + 3 * u2 * Point2;
      return result;
    }


    // Get length using recursive de Casteljau subdivision. - not faster than numerical integration
    internal float GetLengthWithDeCasteljau(int maxNumberOfIterations, float tolerance)
    {
      float length = (Point2 - ControlPoint2).Length + (ControlPoint2 - ControlPoint1).Length + (ControlPoint1 - Point1).Length;

      return GetDeCasteljauLength(Point1, ControlPoint1, ControlPoint2, Point2, length, 0, maxNumberOfIterations, tolerance);
    }


    // length contains the approximated length for this spline. We subdivide and compare the length.
    // If there is room for improvement, we subdivide further.
    private static float GetDeCasteljauLength(Vector2F point1, Vector2F controlPoint1, Vector2F controlPoint2, Vector2F point2, float length, int iteration, int maxNumberOfIterations, float tolerance)
    {
      iteration++;

      // Compute more precise length by subdividing spline into two splines and comparing with length.
      Vector2F a = (point1 + controlPoint1) / 2;
      Vector2F b = (controlPoint1 + controlPoint2) / 2;
      Vector2F c = (controlPoint2 + point2) / 2;
      Vector2F d = (a + b) / 2;
      Vector2F e = (b + c) / 2;
      Vector2F f = (d + e) / 2;

      float length1 = (point1 - a).Length + (a - d).Length + (d - f).Length;
      float length2 = (f - e).Length + (e - c).Length + (c - point2).Length;
      float sum = length1 + length2;
      if (iteration >= maxNumberOfIterations || Numeric.AreEqual(sum, length, tolerance))
        return sum; // Max iterations reached or length is precise enough.

      // Recursive call for more precision.
      length1 = GetDeCasteljauLength(point1, a, d, f, length1, iteration, maxNumberOfIterations, tolerance);
      length2 = GetDeCasteljauLength(f, e, c, point2, length2, iteration, maxNumberOfIterations, tolerance);
      return length1 + length2;
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


    //--------------------------------------------------------------
    #region Resource Pooling
    //--------------------------------------------------------------

    private static readonly ResourcePool<BezierSegment2F> Pool = new ResourcePool<BezierSegment2F>(
       () => new BezierSegment2F(),                   // Create
       null,                                          // Initialize
       null                                           // Uninitialize
       );


    /// <summary>
    /// Creates an instance of the <see cref="BezierSegment2F"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="BezierSegment2F"/> class.
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
    public static BezierSegment2F Create()
    {
      return Pool.Obtain();
    }


    /// <inheritdoc/>
    public void Recycle()
    {
      Point1 = new Vector2F();
      Point2 = new Vector2F();
      ControlPoint1 = new Vector2F();
      ControlPoint2 = new Vector2F();

      Pool.Recycle(this);
    }
    #endregion
  }
}
