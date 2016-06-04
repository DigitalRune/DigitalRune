// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a single segment of a 2-dimensional cubic Catmull-Rom spline (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="CatmullRomSegment2F"/> can be used to smoothly interpolate between two points.
  /// </para>
  /// <para>
  /// Given a series of points (<see cref="Point1"/>, <see cref="Point2"/>, <see cref="Point3"/>, 
  /// <see cref="Point4"/>) this method performs a smooth interpolation between the points 
  /// <see cref="Point2"/> and <see cref="Point3"/>. The interpolation curve is known as 
  /// <i>Catmull-Rom spline</i>. The curve (<i>spline</i>) is a specialization of the <i>cubic 
  /// Hermit spline</i>. The curve runs through the control points <see cref="Point2"/> and 
  /// <see cref="Point3"/>. The tangent at each point is the average of the slope from the previous 
  /// point and the slope to the next point.
  /// </para>
  /// <para>
  /// The Catmull-Rom spline is defined as:
  /// </para>
  /// <para>
  /// C(<i>u</i>) = ((-<i>u</i><sup>3</sup> + 2 <i>u</i><sup>2</sup> - <i>u</i>) <i>p<sub>1</sub></i>
  ///                + (3 <i>u</i><sup>3</sup> - 5 <i>u</i><sup>2</sup> + 2) <i>p<sub>2</sub></i>
  ///                + (-3 <i>u</i><sup>3</sup> + 4 <i>u</i><sup>2</sup> + <i>u</i>) <i>p<sub>3</sub></i>
  ///                + (<i>u</i><sup>3</sup> - <i>u</i><sup>2</sup>) <i>p<sub>4</sub></i>
  ///               )/ 2
  /// </para>
  /// <para>
  /// where <i>u</i> is the interpolation parameter.
  /// </para>
  /// <para>
  /// The curve function <i>point = C(parameter)</i> takes a scalar parameter and returns a point
  /// on the curve (see <see cref="GetPoint"/>). The curve parameter lies in the interval [0,1]; it 
  /// is also known as <i>interpolation parameter</i>, <i>interpolation factor</i> or <i>weight of 
  /// the target point</i>. <i>C(0)</i> returns the start point <see cref="Point2"/>; <i>C(1)</i> 
  /// returns the end point <see cref="Point3"/>.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Catmull")]
  public class CatmullRomSegment2F : ICurve<float, Vector2F>, IRecyclable
  {
    /// <summary>
    /// Gets or sets the previous point.
    /// </summary>
    public Vector2F Point1 { get; set; }


    /// <summary>
    /// Gets or sets the start point.
    /// </summary>
    public Vector2F Point2 { get; set; }


    /// <summary>
    /// Gets or sets the end point.
    /// </summary>
    public Vector2F Point3 { get; set; }


    /// <summary>
    /// Gets or sets the subsequent point.
    /// </summary>
    public Vector2F Point4 { get; set; }


    /// <summary>
    /// Computes a point on the curve.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>The curve point.</returns>
    public Vector2F GetPoint(float parameter)
    {
      float u = parameter;
      float u2 = u * u;
      float u3 = u2 * u;
      return ((-u3 + 2 * u2 - u) * Point1
              + (3 * u3 - 5 * u2 + 2) * Point2
              + (-3 * u3 + 4 * u2 + u) * Point3
              + (u3 - u2) * Point4
             ) / 2;
    }


    /// <inheritdoc/>
    public Vector2F GetTangent(float parameter)
    {
      float u = parameter;
      float u2 = u * u;
      return ((-3 * u2 + 2 * 2 * u - 1) * Point1
              + (3 * 3 * u2 - 5 * 2 * u) * Point2
              + (-3 * 3 * u2 + 4 * 2 * u + 1) * Point3
              + (3 * u2 - 2 * u) * Point4
             ) / 2;
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

    private static readonly ResourcePool<CatmullRomSegment2F> Pool = new ResourcePool<CatmullRomSegment2F>(
       () => new CatmullRomSegment2F(),               // Create
       null,                                          // Initialize
       null                                           // Uninitialize
       );


    /// <summary>
    /// Creates an instance of the <see cref="CatmullRomSegment2F"/> class. (This method reuses a 
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="CatmullRomSegment2F"/> class.
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
    public static CatmullRomSegment2F Create()
    {
      return Pool.Obtain();
    }


    /// <inheritdoc/>
    public void Recycle()
    {
      Point1 = new Vector2F();
      Point2 = new Vector2F();
      Point3 = new Vector2F();
      Point4 = new Vector2F();

      Pool.Recycle(this);
    }
    #endregion
  }
}
