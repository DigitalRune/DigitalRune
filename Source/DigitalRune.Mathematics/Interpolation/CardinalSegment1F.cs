// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a single segment of a 1-dimensional cubic Cardinal spline (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="CardinalSegment1F"/> can be used to smoothly interpolate between two points.
  /// </para>
  /// <para>
  /// Given a series of points (<see cref="Point1"/>, <see cref="Point2"/>, <see cref="Point3"/>, 
  /// <see cref="Point4"/>) this method performs a smooth interpolation between the points 
  /// <see cref="Point2"/> and <see cref="Point3"/>. The interpolation curve is known as as 
  /// <i>Cardinal spline</i>. The curve (<i>spline</i>) is a specialization of the <i>cubic Hermit 
  /// spline</i>. The curve runs through the control points <see cref="Point2"/> and 
  /// <see cref="Point3"/>. The tangent at each point is computed from the previous point and the 
  /// following point. The tangent <i>t<sub>i</sub></i> for point <i>p<sub>i</sub></i> is computed 
  /// as:
  /// </para>
  /// <para>
  /// <i>t<sub>i</sub></i> = 1/2 (1 - <i>tension</i>) (<i>p<sub>i+1</sub></i> - <i>p<sub>i-1</sub></i>),
  /// </para>
  /// <para>
  /// where <i>tension</i> is a parameter, which is a constant that modifies the length of the
  /// tangent. <i>tension</i> = 1 will yield zero tangents and <i>tension</i> = 0 yields a 
  /// Catmull-Rom spline.
  /// </para>
  /// <para>
  /// The curve function <i>point = C(parameter)</i> takes a scalar parameter and returns a point
  /// on the curve (see <see cref="GetPoint"/>). The curve parameter lies in the interval [0,1]; it 
  /// is also known as <i>interpolation parameter</i>, <i>interpolation factor</i> or <i>weight of 
  /// the target point</i>. <i>C(0)</i> returns the start point <see cref="Point2"/>; <i>C(1)</i> 
  /// returns the end point <see cref="Point3"/>.
  /// </para>
  /// </remarks>
  public class CardinalSegment1F : ICurve<float, float>, IRecyclable
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
    /// Gets or sets the tension constant.
    /// </summary>
    public float Tension { get; set; }


    /// <summary>
    /// Computes a point on the curve.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>The curve point.</returns>
    public float GetPoint(float parameter)
    {
      float k = 1f / 2f * (1f - Tension);
      
      float u = parameter;
      float u2 = u * u;
      float u3 = u2 * u;
      return (2 * u3 - 3 * u2 + 1) * Point2
              + (u3 - 2 * u2 + u) * k * (Point3 - Point1)
              + (-2 * u3 + 3 * u2) * Point3
              + (u3 - u2) * k * (Point4 - Point2);
    }


    /// <inheritdoc/>
    public float GetTangent(float parameter)
    {
      float k = 1f / 2f * (1f - Tension);

      float u = parameter;
      float u2 = u * u;
      return (2 * 3 * u2 - 3 * 2 * u) * Point2
              + (3 * u2 - 2 * 2 * u + 1) * k * (Point3 - Point1)
              + (-2 * 3 * u2 + 3 * 2 * u) * Point3
              + (3 * u2 - 2 * u) * k * (Point4 - Point2);
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
      points.Add(Point2);
      points.Add(Point3);
    }


    //--------------------------------------------------------------
    #region Resource Pooling
    //--------------------------------------------------------------

    private static readonly ResourcePool<CardinalSegment1F> Pool = new ResourcePool<CardinalSegment1F>(
       () => new CardinalSegment1F(),                 // Create
       null,                                          // Initialize
       null                                           // Uninitialize
       );


    /// <summary>
    /// Creates an instance of the <see cref="CardinalSegment1F"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="CardinalSegment1F"/> class.
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
    public static CardinalSegment1F Create()
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
      Tension = 0;

      Pool.Recycle(this);
    }
    #endregion
  }
}
