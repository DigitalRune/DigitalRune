// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Mathematics.Interpolation
{
  /// <summary>
  /// Defines a curve key (control point) of a piecewise curve.
  /// </summary>
  /// <typeparam name="TParam">
  /// The type of the curve parameter (usually <see cref="float"/> or <see cref="double"/>).
  /// </typeparam>
  /// <typeparam name="TPoint">
  /// The type of the curve points (such as <see cref="Vector2F"/>, <see cref="Vector3F"/>, etc.).
  /// </typeparam>
  /// <inheritdoc cref="PiecewiseCurve{TParam,TPoint,TCurveKey}"/>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  [DebuggerDisplay("{GetType().Name,nq}(Parameter = {Parameter}, Point = {Point}, TangentIn = {TangentIn}, TangentOut = {TangentOut}, Interpolation = {Interpolation})")]
  public abstract class CurveKey<TParam, TPoint>
  {
    /// <summary>
    /// Gets or sets a value that defines where this curve key is positioned on the curve.
    /// </summary>
    /// <value>The parameter value.</value>
    /// <remarks>
    /// <para>
    /// The parameter is normally zero at the first curve key and increases as we move along the 
    /// curve.
    /// </para>
    /// <para>
    /// Depending on where or how the curve is used the curve parameter could be interpreted as 
    /// <i>time</i> (to describe when a certain point is reached) or <i>distance</i> (to describe 
    /// the distance from the start of the curve). Other interpretations can be used as well.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
    public TParam Parameter
    {
      get { return GetParameter(); }
      set { SetParameter(value); }
    }


    /// <summary>
    /// Gets or sets the curve point for this curve key.
    /// </summary>
    /// <value>The curve point (control point).</value>
    /// <remarks>
    /// Depending on the type of interpolation that is used for the current segment of the curve,
    /// the key may be point on the curve (see <see cref="SplineInterpolation.Bezier"/>, 
    /// <see cref="SplineInterpolation.CatmullRom"/>, <see cref="SplineInterpolation.Hermite"/>,
    /// <see cref="SplineInterpolation.Linear"/>, <see cref="SplineInterpolation.StepLeft"/>, 
    /// <see cref="SplineInterpolation.StepCentered"/>, <see cref="SplineInterpolation.StepRight"/>)
    /// or may only be a control point that does not directly on the curve (see 
    /// <see cref="SplineInterpolation.BSpline"/>).
    /// </remarks>
    public TPoint Point { get; set; }


    /// <summary>
    /// Gets or sets the incoming tangent or the control point before this curve key.
    /// </summary>
    /// <value>The incoming tangent or the control point before this curve key.</value>
    /// <remarks>
    /// <para>
    /// This property is used to compute previous curve segment (the spline that ends at this curve 
    /// key).
    /// </para>
    /// <para>
    /// The meaning of this property depends on the type of interpolation that is for the previous
    /// curve segment:
    /// <list type="table">
    /// <listheader>
    /// <term>SplineInterpolation</term>
    /// <description>Meaning</description>
    /// </listheader>
    /// <item>
    /// <term>
    /// <see cref="SplineInterpolation.Hermite"/>
    /// </term>
    /// <description>
    /// Hermite splines require tangent information. Therefore, this property defines the incoming 
    /// tangent of the spline that ends at this curve key.
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// <see cref="SplineInterpolation.Bezier"/>
    /// </term>
    /// <description>
    /// Bézier splines require additional control points. Therefore, this property defines the 
    /// control point before this curve key.
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// Other
    /// </term>
    /// <description>
    /// All other types of interpolation do not need any additional information. The property 
    /// <see cref="TangentIn"/> is unused.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public TPoint TangentIn { get; set; }


    /// <summary>
    /// Gets or sets the outgoing tangent or the control point after this curve key.
    /// </summary>
    /// <value>The outgoing tangent or the control point after this curve key.</value>
    /// <remarks>
    /// <para>
    /// This property is used to compute the current curve segment (the spline that starts at this 
    /// curve key).
    /// </para>
    /// <para>
    /// The meaning of this property depends on the type of interpolation that is for the current
    /// curve segment:
    /// <list type="table">
    /// <listheader>
    /// <term>SplineInterpolation</term>
    /// <description>Meaning</description>
    /// </listheader>
    /// <item>
    /// <term>
    /// <see cref="SplineInterpolation.Hermite"/>
    /// </term>
    /// <description>
    /// Hermite splines require tangent information. Therefore, this property defines the outgoing 
    /// tangent of the spline between this curve key and the next.
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// <see cref="SplineInterpolation.Bezier"/>
    /// </term>
    /// <description>
    /// Bézier splines require additional control points. Therefore, this property defines the 
    /// control point after this curve key.
    /// </description>
    /// </item>
    /// <item>
    /// <term>
    /// Other
    /// </term>
    /// <description>
    /// All other types of interpolation do not need any additional information. The property 
    /// <see cref="TangentOut"/> is unused.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public TPoint TangentOut { get; set; }


    /// <summary>
    /// Gets or sets the type of interpolation (the spline type) used for the current curve segment
    /// between this key and the next key.
    /// </summary>
    /// <value>
    /// The type of interpolation used for the current curve segment (the spline between this curve
    /// key and the next curve key).
    /// </value>
    public SplineInterpolation Interpolation { get; set; }


    /// <summary>
    /// Gets the parameter.
    /// </summary>
    /// <returns>The parameter.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    protected abstract TParam GetParameter();


    /// <summary>
    /// Sets the parameter.
    /// </summary>
    /// <param name="value">The parameter</param>
    protected abstract void SetParameter(TParam value);
  }
}