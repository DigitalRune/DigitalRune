// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a single segment of a 3-dimensional cubic Cardinal spline (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="CardinalSegment3F"/> can be used to smoothly interpolate between two points.
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
  public class CardinalSegment3F : ICurve<float, Vector3F>, IRecyclable
  {
    /// <summary>
    /// Gets or sets the previous point.
    /// </summary>
    public Vector3F Point1 { get; set; }


    /// <summary>
    /// Gets or sets the start point.
    /// </summary>
    public Vector3F Point2 { get; set; }


    /// <summary>
    /// Gets or sets the end point.
    /// </summary>
    public Vector3F Point3 { get; set; }


    /// <summary>
    /// Gets or sets the subsequent point.
    /// </summary>
    public Vector3F Point4 { get; set; }


    /// <summary>
    /// Gets or sets the tension constant.
    /// </summary>
    public float Tension { get; set; }


    /// <summary>
    /// Computes a point on the curve.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>The curve point.</returns>
    public Vector3F GetPoint(float parameter)
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
    public Vector3F GetTangent(float parameter)
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
      return CurveHelper.GetLength(this, start, end, 2, maxNumberOfIterations, tolerance);
    }


    /// <inheritdoc/>
    public void Flatten(ICollection<Vector3F> points, int maxNumberOfIterations, float tolerance)
    {
      CurveHelper.Flatten(this, points, maxNumberOfIterations, tolerance);
    }


    //--------------------------------------------------------------
    #region Resource Pooling
    //--------------------------------------------------------------

    private static readonly ResourcePool<CardinalSegment3F> Pool = new ResourcePool<CardinalSegment3F>(
       () => new CardinalSegment3F(),                 // Create
       null,                                          // Initialize
       null                                           // Uninitialize
       );


    /// <summary>
    /// Creates an instance of the <see cref="CardinalSegment3F"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="CardinalSegment3F"/> class.
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
    public static CardinalSegment3F Create()
    {
      return Pool.Obtain();
    }


    /// <inheritdoc/>
    public void Recycle()
    {
      Point1 = new Vector3F();
      Point2 = new Vector3F();
      Point3 = new Vector3F();
      Point4 = new Vector3F();
      Tension = 0;

      Pool.Recycle(this);
    }
    #endregion
  }
}
