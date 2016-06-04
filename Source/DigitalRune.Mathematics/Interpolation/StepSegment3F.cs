// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a step interpolation between two 3-dimensional points (single-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// The curve function <i>point = C(parameter)</i> takes a scalar parameter and returns a point
  /// on the curve (see <see cref="GetPoint"/>). The curve parameter lies in the interval [0,1]; it 
  /// is also known as <i>interpolation parameter</i>, <i>interpolation factor</i> or <i>weight of 
  /// the target point</i>. <i>C(0)</i> returns the start point <see cref="Point1"/>; <i>C(1)</i> 
  /// returns the end point <see cref="Point2"/>. The curve is not continuous; it consist only of 
  /// the two points, <see cref="Point1"/> and <see cref="Point2"/>.
  /// </para>
  /// <para>
  /// The tangents and the length of this special kind of curve are zero.
  /// </para>
  /// </remarks>
  public class StepSegment3F : ICurve<float, Vector3F>, IRecyclable
  {
    /// <summary>
    /// Gets or sets the start point.
    /// </summary>
    public Vector3F Point1 { get; set; }


    /// <summary>
    /// Gets or sets the end point.
    /// </summary>
    public Vector3F Point2 { get; set; }


    /// <summary>
    /// Gets or sets the type of step interpolation.
    /// </summary>
    public StepInterpolation StepType { get; set; }


    /// <summary>
    /// Computes a point on the curve.
    /// </summary>
    /// <param name="parameter">The curve parameter.</param>
    /// <returns>The curve point.</returns>
    public Vector3F GetPoint(float parameter)
    {
      return InterpolationHelper.Step(Point1, Point2, parameter, StepType);
    }


    /// <inheritdoc/>
    public Vector3F GetTangent(float parameter)
    {
      return Vector3F.Zero;
    }


    /// <inheritdoc/>
    public float GetLength(float start, float end, int maxNumberOfIterations, float tolerance)
    {
      return 0;
    }


    /// <inheritdoc/>
    public void Flatten(ICollection<Vector3F> points, int maxNumberOfIterations, float tolerance)
    {
    }


    //--------------------------------------------------------------
    #region Resource Pooling
    //--------------------------------------------------------------

    private static readonly ResourcePool<StepSegment3F> Pool = new ResourcePool<StepSegment3F>(
       () => new StepSegment3F(),                     // Create
       null,                                          // Initialize
       null                                           // Uninitialize
       );


    /// <summary>
    /// Creates an instance of the <see cref="StepSegment3F"/> class. (This method reuses a
    /// previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="StepSegment3F"/> class.
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
    public static StepSegment3F Create()
    {
      return Pool.Obtain();
    }


    /// <inheritdoc/>
    public void Recycle()
    {
      Point1 = new Vector3F();
      Point2 = new Vector3F();
      StepType = StepInterpolation.Left;

      Pool.Recycle(this);
    }
    #endregion
  }
}
