// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a single segment of a 2-dimensional cubic Hermite spline (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="HermiteSegment2F"/> can be used to smoothly interpolate between two points.
  /// </para>
  /// <para>
  /// The curve runs through the points <see cref="Point1"/> and <see cref="Point2"/>. The tangents 
  /// at these points can be controlled with <see cref="Tangent1"/> and <see cref="Tangent2"/>. 
  /// The curve smoothly interpolates between <see cref="Point1"/> and <see cref="Point2"/>.
  /// </para>
  /// <para>
  /// Multiple splines can be patched together by matching the tangents at the control points.
  /// </para>
  /// <para>
  /// The curve function <i>point = C(parameter)</i> takes a scalar parameter and returns a point
  /// on the curve (see <see cref="GetPoint"/>). The curve parameter lies in the interval [0,1]; it 
  /// is also known as <i>interpolation parameter</i>, <i>interpolation factor</i> or <i>weight of 
  /// the target point</i>. <i>C(0)</i> returns the start point <see cref="Point1"/>; <i>C(1)</i> 
  /// returns the end point <see cref="Point2"/>.
  /// </para>
  /// <para>
  /// The curve is defined as:
  /// </para>
  /// <para>
  /// C(<i>u</i>) = (2<i>u</i><sup>3</sup> - 3<i>u</i><sup>2</sup> + 1) <i>p<sub>1</sub></i>
  ///               + (<i>u</i><sup>3</sup> - 2<i>u</i><sup>2</sup> + <i>u</i>) <i>t<sub>1</sub></i>
  ///               + (-2<i>u</i><sup>3</sup> + 3<i>u</i><sup>2</sup>) <i>p<sub>2</sub></i>
  ///               + (<i>u</i><sup>3</sup> - <i>u</i><sup>2</sup>) <i>t<sub>2</sub></i>,
  /// </para>
  /// <para>
  /// where <i>u</i> is the interpolation parameter, <i>p</i> are the start/end points and <i>t</i>
  /// are the start/end tangents.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class HermiteSegment2F : ICurve<float, Vector2F>, IRecyclable
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
    /// Gets or sets the tangent at <see cref="Point1"/>.
    /// </summary>
    public Vector2F Tangent1 { get; set; }


    /// <summary>
    /// Gets or sets the tangent at <see cref="Point2"/>.
    /// </summary>
    public Vector2F Tangent2 { get; set; }


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
      return (2 * u3 - 3 * u2 + 1) * Point1
              + (u3 - 2 * u2 + u) * Tangent1
              + (-2 * u3 + 3 * u2) * Point2
              + (u3 - u2) * Tangent2;
    }


    /// <inheritdoc/>
    public Vector2F GetTangent(float parameter)
    {
      float u = parameter;
      float u2 = u * u;
      return (2 * 3 * u2 - 3 * 2 * u) * Point1
              + (3 * u2 - 2 * 2 * u + 1) * Tangent1
              + (-2 * 3 * u2 + 3 * 2 * u) * Point2
              + (3 * u2 - 2 * u) * Tangent2;
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

    private static readonly ResourcePool<HermiteSegment2F> Pool = new ResourcePool<HermiteSegment2F>(
       () => new HermiteSegment2F(),                  // Create
       null,                                          // Initialize
       null                                           // Uninitialize
       );


    /// <summary>
    /// Creates an instance of the <see cref="HermiteSegment2F"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="HermiteSegment2F"/> class.
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
    public static HermiteSegment2F Create()
    {
      return Pool.Obtain();
    }


    /// <inheritdoc/>
    public void Recycle()
    {
      Point1 = new Vector2F();
      Point2 = new Vector2F();
      Tangent1 = new Vector2F();
      Tangent2 = new Vector2F();

      Pool.Recycle(this);
    }
    #endregion
  }
}
