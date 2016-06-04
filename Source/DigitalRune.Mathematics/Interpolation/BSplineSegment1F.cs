// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a single segment of a 1-dimensional cubic B-spline (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="BSplineSegment1F"/> can be used to smoothly interpolate between two points. 
  /// </para>
  /// <para>
  /// Given a series of points (<see cref="Point1"/>, <see cref="Point2"/>, <see cref="Point3"/>, 
  /// <see cref="Point4"/>) this method performs a smooth approximation between the points 
  /// <see cref="Point2"/> and <see cref="Point3"/>. The approximation curve is known as 
  /// <i>B-spline</i>. It is not guaranteed that the curve runs through the given points 
  /// <see cref="Point2"/> and <see cref="Point3"/>!
  /// </para>
  /// <para>
  /// The curve function <i>point = C(parameter)</i> takes a scalar parameter and returns a point
  /// on the curve (see <see cref="GetPoint"/>). The curve parameter lies in the interval [0,1]; it 
  /// is also known as <i>interpolation parameter</i>, <i>interpolation factor</i> or <i>weight of 
  /// the target point</i>.
  /// </para>
  /// </remarks>
  public class BSplineSegment1F : ICurve<float, float>, IRecyclable
  {
    /// <summary>
    /// Gets or sets the previous point.
    /// </summary>
    public float Point1 { get; set; }


    /// <summary>
    /// Gets or sets the start point.
    /// </summary>
    public float Point2 { get; set; }


    /// <summary>
    /// Gets or sets the end point.
    /// </summary>
    public float Point3 { get; set; }


    /// <summary>
    /// Gets or sets the subsequent point.
    /// </summary>
    public float Point4 { get; set; }


    /// <summary>
    /// Computes a point on the curve.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>The curve point.</returns>
    public float GetPoint(float parameter)
    {
      float u = parameter;
      float u2 = u * u;
      float u3 = u2 * u;
      return ((-u3 + 3 * u2 - 3 * u + 1) * Point1
             + (3 * u3 - 6 * u2 + 4) * Point2
             + (-3 * u3 + 3 * u2 + 3 * u + 1) * Point3
             + (u3) * Point4
             ) / 6;
    }


    /// <inheritdoc/>
    public float GetTangent(float parameter)
    {
      float u = parameter;
      float u2 = u * u;
      return ((- 3 * u2 + 3 * 2 * u - 3) * Point1
             + (3 * 3 * u2 - 6 * 2* u) * Point2
             + (-3 * 3 * u2 + 3 * 2 * u + 3) * Point3
             + (3 * u2) * Point4
             ) / 6;
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
      points.Add(GetPoint(0));
      points.Add(GetPoint(1));
    }


    //--------------------------------------------------------------
    #region Resource Pooling
    //--------------------------------------------------------------

    private static readonly ResourcePool<BSplineSegment1F> Pool = new ResourcePool<BSplineSegment1F>(
       () => new BSplineSegment1F(),                  // Create
       null,                                          // Initialize
       null                                           // Uninitialize
       );


    /// <summary>
    /// Creates an instance of the <see cref="BSplineSegment1F"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="BSplineSegment1F"/> class.
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
    public static BSplineSegment1F Create()
    {
      return Pool.Obtain();
    }


    /// <inheritdoc/>
    public void Recycle()
    {
      Point1 = 0;
      Point2 = 0;
      Point3 = 0;
      Point4 = 0;

      Pool.Recycle(this);
    }
    #endregion
  }
}
