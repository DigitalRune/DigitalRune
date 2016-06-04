// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a single segment of a 1-dimensional cubic Bézier spline (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="BezierSegment1F"/> can be used to smoothly interpolate between two points.
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
  public class BezierSegment1F : ICurve<float, float>, IRecyclable
  {
    /// <summary>
    /// Gets or sets the start point.
    /// </summary>
    public float Point1 { get; set; }


    /// <summary>
    /// Gets or sets the end point.
    /// </summary>
    public float Point2 { get; set; }


    /// <summary>
    /// Gets or sets the first control point.
    /// </summary>
    public float ControlPoint1 { get; set; }


    /// <summary>
    /// Gets or sets the second control point.
    /// </summary>
    public float ControlPoint2 { get; set; }


    /// <summary>
    /// Computes a point on the curve.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>The curve point.</returns>
    public float GetPoint(float parameter)
    {
      // Polynomial form:
      float u = parameter;
      float u2 = u * u;
      float u3 = u2 * u;
      float uNeg = (1 - u);
      float uNeg2 = uNeg * uNeg;
      float uNeg3 = uNeg2 * uNeg;
      float result = uNeg3 * Point1 + 3 * u * uNeg2 * ControlPoint1 + 3 * u2 * uNeg * ControlPoint2 + u3 * Point2;

      return result;
    }


    /// <inheritdoc/>
    public float GetTangent(float parameter)
    {
      float u = parameter;
      float u2 = u * u;
      float result = (-3 + 6 * u - 3 * u2) * Point1 + (3 - 12 * u + 9 * u2) * ControlPoint1 + (6 * u - 9 * u2) * ControlPoint2 + 3 * u2 * Point2;
      return result;
    }


    /// <inheritdoc/>
    public float GetLength(float start, float end, int maxNumberOfIterations, float tolerance)
    {
      return Math.Abs(GetPoint(end) - GetPoint(start));
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public void Flatten(ICollection<float> points, int maxNumberOfIterations, float tolerance)
    {
      points.Add(Point1);
      points.Add(Point2);
    }


    // ----- GetParameter with de Casteljau. In release builds this is a bit slower than Newton-Raphson rootfinding.
    ///// <summary>
    ///// Gets the approximated parameter that was used to get the specified interpolated value.
    ///// </summary>
    ///// <param name="interpolatedPoint">A point on the spline.</param>
    ///// <param name="maxNumberOfIterations">
    ///// The maximum number of iterations which are taken to compute the parameter.
    ///// </param>
    ///// <param name="tolerance">
    ///// The tolerance value. This method will return an approximation of the precise parameter. The absolute error
    ///// will be less than this tolerance value.
    ///// </param>
    ///// <returns>
    ///// The (approximated) curve parameter for the given interpolated point.
    ///// </returns>
    ///// <remarks>
    ///// This method performs the inverse of the curve function: <i>parameter = C<sup>-1</sup>(point)</i>
    ///// The parameter is computed with an iterative
    ///// algorithm. The iterations end when the <paramref name="maxNumberOfIterations"/> were
    ///// performed, or when the <paramref name="tolerance"/> criterion is met - whichever comes
    ///// first.
    ///// </remarks>
    //public float GetParameter(float interpolatedPoint, int maxNumberOfIterations, float tolerance)
    //{
    //  if (tolerance < 0)
    //    throw new ArgumentOutOfRangeException("tolerance", "The tolerance must be greater than 0.");
    //  if (Numeric.AreEqual(interpolatedPoint, Point1, tolerance))
    //    return 0;
    //  if (Numeric.AreEqual(interpolatedPoint, Point2, tolerance))
    //    return 1;

    //  // Use bisection with de Casteljau.
    //  float p1 = Point1;
    //  float p2 = Point2;
    //  float cp1 = ControlPoint1;
    //  float cp2 = ControlPoint2;
    //  float parameter = 0.5f;
    //  float stepSize = 0.5f;
    //  for (int numberOfIterations = 0; numberOfIterations < maxNumberOfIterations; numberOfIterations++)
    //  {
    //    float a = (cp1 + p1) / 2;
    //    float b = (cp2 + cp1) / 2;
    //    float c = (p2 + cp2) / 2;
    //    float d = (b + a) / 2;
    //    float e = (c + b) / 2;
    //    float p = (e + d) / 2;

    //    int comparison = Numeric.Compare(interpolatedPoint, p, tolerance);
    //    if (comparison == 0)
    //      return parameter;   // Finished.
    //    stepSize /= 2;
    //    if (comparison < 0)
    //    {
    //      // Search left half.
    //      parameter -= stepSize;
    //      cp1 = a;
    //      cp2 = d;
    //      p2 = p;
    //    }
    //    else
    //    {
    //      // Search right half.
    //      parameter += stepSize;
    //      p1 = p;
    //      cp1 = e;
    //      cp2 = c;
    //    }
    //  }
    //  return parameter;
    //}    


    //--------------------------------------------------------------
    #region Resource Pooling
    //--------------------------------------------------------------

    private static readonly ResourcePool<BezierSegment1F> Pool = new ResourcePool<BezierSegment1F>(
       () => new BezierSegment1F(),                   // Create
       null,                                          // Initialize
       null                                           // Uninitialize
       );


    /// <summary>
    /// Creates an instance of the <see cref="BezierSegment1F"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="BezierSegment1F"/> class.
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
    public static BezierSegment1F Create()
    {
      return Pool.Obtain();
    }


    /// <inheritdoc/>
    public void Recycle()
    {
      Point1 = 0;
      Point2 = 0;
      ControlPoint1 = 0;
      ControlPoint2 = 0;

      Pool.Recycle(this);
    }
    #endregion
  }
}
