// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates a value using an animation curve. (Base implementation.)
  /// </summary>
  /// <typeparam name="TValue">
  /// The type of the animation value.
  /// </typeparam>
  /// <typeparam name="TPoint">
  /// The type of the curve points, such as <see cref="Vector2F"/>, <see cref="Vector3F"/>, etc.
  /// </typeparam>
  /// <typeparam name="TCurveKey">
  /// The type of the curve key. (A type derived from <see cref="CurveKey{TParam,TPoint}"/>.)
  /// </typeparam>
  /// <typeparam name="TCurve">
  /// The type of the curve. (A type derived from 
  /// <see cref="PiecewiseCurve{TParam,TPoint,TCurveKey}"/>.)
  /// </typeparam>
  /// <remarks>
  /// <para>
  /// The animation curve (see property 
  /// <see cref="AnimationCurve{TValue,TPoint,TCurveKey,TCurve}.Curve"/>) contains several curve
  /// keys (also known as 'key frames') that define the change of the value. The curve parameter is
  /// the animation time. A curve key defines the current value at a certain point in time. It also
  /// defines the type of interpolation that is used for the segment between the current and the
  /// next curve key. All relevant types of spline-based interpolations can be used for the curve
  /// segments.
  /// </para>
  /// <para>
  /// <strong>Duration:</strong> An animation curve, by default, runs from the start (curve 
  /// parameter = 0) until the last curve key is reached. The parameter of the last curve key
  /// determines the natural duration of the animation. The optional properties 
  /// <see cref="StartParameter"/> and <see cref="EndParameter"/> can be used to explicitly define
  /// which part of the animation curve should be played.
  /// </para>
  /// <para>
  /// <strong>Loop Behavior:</strong> When the <see cref="StartParameter"/> is less than the 
  /// parameter of the first curve key or <see cref="EndParameter"/> is greater than the parameter 
  /// of the last curve key then the curve is automatically repeated using a certain loop behavior. 
  /// The loop behavior can be defined using the properties 
  /// <see cref="PiecewiseCurve{TParam, TPoint, TCurveKey}.PreLoop"/> and 
  /// <see cref="PiecewiseCurve{TParam, TPoint, TCurveKey}.PostLoop"/> of the <see cref="Curve"/>.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> The animation curve requires that the curve keys are sorted 
  /// ascending by their parameter (time value). The method 
  /// <see cref="PiecewiseCurve{TParam,TPoint,TCurveKey}.Sort"/> can be called to sort all curve 
  /// keys.
  /// </para>
  /// </remarks>
  public abstract class AnimationCurve<TValue, TPoint, TCurveKey, TCurve> 
    : Animation<TValue> 
      where TCurveKey : CurveKey<float, TPoint>
      where TCurve : PiecewiseCurve<float, TPoint, TCurveKey>
      where TValue : IEquatable<TValue>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private TimeSpan _startTime;
    private TimeSpan _endTime;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the curve that defines the animation.
    /// </summary>
    /// <value>The curve that defines the animation.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public TCurve Curve { get; set; }


    /// <summary>
    /// Gets or sets the start parameter.
    /// </summary>
    /// <value>The start parameter. The default value is <see cref="float.NaN"/>.</value>
    /// <remarks>
    /// <para>
    /// An animation curve, by default, runs from the start (curve parameter = 0) until the last
    /// curve key is reached. The parameter of the last curve key determines the natural duration of
    /// the animation. The properties <see cref="StartParameter"/> and <see cref="EndParameter"/>
    /// can be used to explicitly define which part of the animation curve should be played.
    /// </para>
    /// <para>
    /// The <see cref="StartParameter"/> can be set to <see cref="float.NaN"/> to play the animation
    /// from the start (curve parameter = 0).
    /// </para>
    /// <para>
    /// The <see cref="EndParameter"/> can be set to <see cref="float.NaN"/> to automatically play 
    /// the animation until the last curve key is reached.
    /// </para>
    /// </remarks>
    public float StartParameter
    {
      get { return _startParameter; }
      set
      {
        _startParameter = value;

        if (!Numeric.IsNaN(value))
        {
          // Cache value as TimeSpan.
          if (float.IsNegativeInfinity(value))
            _startTime = TimeSpan.MinValue;
          else if (float.IsPositiveInfinity(value))
            _startTime = TimeSpan.MaxValue;
          else
            _startTime = new TimeSpan((long)(value * TimeSpan.TicksPerSecond));
        }
      }
    }
    private float _startParameter;


    /// <summary>
    /// Gets or sets the end parameter.
    /// </summary>
    /// <value>The end parameter. The default value is <see cref="float.NaN"/>.</value>
    /// <inheritdoc cref="StartParameter"/>
    public float EndParameter
    {
      get { return _endParameter; }
      set
      {
        _endParameter = value;

        if (!Numeric.IsNaN(value))
        {
          // Cache value as TimeSpan.
          if (float.IsNegativeInfinity(value))
            _endTime = TimeSpan.MinValue;
          else if (float.IsPositiveInfinity(value))
            _endTime = TimeSpan.MaxValue;
          else
            _endTime = new TimeSpan((long)(value * TimeSpan.TicksPerSecond));
        }
      }
    }
    private float _endParameter;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="AnimationCurve{TValue,TPoint,TCurveKey,TCurve}"/> class.
    /// </summary>
    protected AnimationCurve()
    {
      StartParameter = float.NaN;
      EndParameter = float.NaN;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Determines the natural duration of the curve.
    /// </summary>
    /// <returns>The natural duration of the curve.</returns>
    private TimeSpan GetNaturalDuration()
    {
      // The natural length is determined by the last key of the curve.
      float length = 0;

      var curve = Curve;
      if (curve != null)
      {
        int numberOfKeys = curve.Count;
        if (numberOfKeys > 0)
          length = curve[numberOfKeys - 1].Parameter;
      }

      return new TimeSpan((long)(length * TimeSpan.TicksPerSecond));
    }


    /// <summary>
    /// Gets the interval of the curve that should be played.
    /// </summary>
    /// <param name="start">The start parameter.</param>
    /// <param name="end">The end parameter.</param>
    /// <param name="length">The length of the interval.</param>
    /// <exception cref="InvalidAnimationException">
    /// Invalid <see cref="StartParameter"/> and <see cref="EndParameter"/>.
    /// </exception>
    private void GetClip(out TimeSpan start, out TimeSpan end, out TimeSpan length)
    {
      if (Curve == null)
      {
        start = TimeSpan.Zero;
        end = TimeSpan.Zero;
        length = TimeSpan.Zero;
      }
      else
      {
        start = Numeric.IsNaN(StartParameter) ? TimeSpan.Zero : _startTime;
        end = Numeric.IsNaN(EndParameter) ? GetNaturalDuration() : _endTime;

        if (start > end)
          throw new InvalidAnimationException("The start parameter of an animation curve must not be greater than the end parameter.");

        length = end - start;
      }
    }


    /// <inheritdoc/>
    public override TimeSpan GetTotalDuration()
    {
      TimeSpan start, end, length;
      GetClip(out start, out end, out length);
      return length;
    }


    /// <inheritdoc cref="Animation{T}.GetValueCore"/>
    protected override void GetValueCore(TimeSpan time, ref TValue defaultSource, ref TValue defaultTarget, ref TValue result)
    {
      var curve = Curve;
      if (curve != null && curve.Count > 0)
      {
        TimeSpan start, end, length;
        GetClip(out start, out end, out length);
        float parameter = (float)(start + time).TotalSeconds;
        var value = GetValueFromPoint(curve.GetPoint(parameter));
        Traits.Copy(ref value, ref result);
      }
      else
      {
        Traits.Copy(ref defaultSource, ref result);
      }
    }


    /// <summary>
    /// Gets the animation value from a given point on the curve.
    /// </summary>
    /// <param name="point">The point on the curve.</param>
    /// <returns>The animation value.</returns>
    protected abstract TValue GetValueFromPoint(TPoint point);
    #endregion
  }
}
