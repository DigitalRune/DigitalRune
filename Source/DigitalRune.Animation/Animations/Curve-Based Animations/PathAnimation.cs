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
  /// Animates a point that follows a predefined path. (Base implementation.)
  /// </summary>
  /// <typeparam name="TPoint">
  /// The type of the path points, such as <see cref="Vector2F"/>, <see cref="Vector3F"/>, etc.
  /// </typeparam>
  /// <typeparam name="TPathKey">
  /// The type of the path key. (A type derived from <see cref="CurveKey{TParam,TPoint}"/>.)
  /// </typeparam>
  /// <typeparam name="TPath">
  /// The type of the path. (A type derived from 
  /// <see cref="PiecewiseCurve{TParam,TPoint,TCurveKey}"/>.)
  /// </typeparam>
  /// <remarks>
  /// <para>
  /// A path animation moves a point in space along a predefined path (see property 
  /// <see cref="Path"/>). The animation path is a spline-based curve consisting of several path 
  /// keys (also known as 'key frames'). A path key defines a point in space. The parameter of a 
  /// path key is the animation time - the time at which the point on the path is reached. A path
  /// key also defines the type of interpolation that is used for the segment between the current 
  /// and the next path key. All relevant types of spline-based interpolations can be used for the 
  /// path segments.
  /// </para>
  /// <para>
  /// <strong>Duration:</strong> A path animation, by default, runs from the start (parameter = 0)
  /// until the last path key is reached. The parameter of the last path key determines the natural
  /// duration of the animation. The optional properties <see cref="StartParameter"/> and 
  /// <see cref="EndParameter"/> can be used to explicitly define which part of the path should be 
  /// played.
  /// </para>
  /// <para>
  /// <strong>Loop Behavior:</strong> When the <see cref="StartParameter"/> is less than the 
  /// parameter of the first path key or <see cref="EndParameter"/> is greater than the parameter 
  /// of the last path key then the path is automatically repeated using a certain loop behavior. 
  /// The loop behavior can be defined using the properties 
  /// <see cref="PiecewiseCurve{TParam, TPoint, TCurveKey}.PreLoop"/> and 
  /// <see cref="PiecewiseCurve{TParam, TPoint, TCurveKey}.PostLoop"/> of the <see cref="Path"/>.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> The path animation requires that the path keys are sorted 
  /// ascending by their parameter (time value). The method 
  /// <see cref="PiecewiseCurve{TParam,TPoint,TCurveKey}.Sort"/> can be called to sort all path 
  /// keys.
  /// </para>
  /// </remarks>
  public abstract class PathAnimation<TPoint, TPathKey, TPath>
    : Animation<TPoint>
      where TPathKey : CurveKey<float, TPoint>
      where TPath : PiecewiseCurve<float, TPoint, TPathKey>
      where TPoint : IEquatable<TPoint>
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
    /// Gets or sets the animation path.
    /// </summary>
    /// <value>The animation path.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public TPath Path { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the animation returns the tangent or the point on 
    /// the path.
    /// </summary>
    /// <value>
    /// If <see langword="true"/> the animation returns the tangent of the path. If 
    /// <see langword="false"/> the animation returns the point on the path. The default value is 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// A <see cref="Path"/> is a function that defines a point and a tangent at a given parameter. 
    /// The parameter in this case is the animation time. By default, the 
    /// <see cref="Animation{T}.GetValue"/> method of the animation return points defined by the 
    /// path. When <see cref="ReturnsTangent"/> is set to <see langword="true"/> the animation 
    /// returns the tangents along the path instead of the points.
    /// </remarks>
    public bool ReturnsTangent { get; set; }


    /// <summary>
    /// Gets or sets the start parameter.
    /// </summary>
    /// <value>The start parameter. The default value is <see cref="float.NaN"/>.</value>
    /// <remarks>
    /// <para>
    /// A path animation, by default, runs from the start (parameter = 0) until the last path key is
    /// reached. The parameter of the last path key determines the natural duration of the 
    /// animation. The properties <see cref="StartParameter"/> and <see cref="EndParameter"/> can be
    /// used to explicitly define which part of the path should be played.
    /// </para>
    /// <para>
    /// The <see cref="StartParameter"/> can be set to <see cref="float.NaN"/> to play the animation
    /// from the start (parameter = 0).
    /// </para>
    /// <para>
    /// The <see cref="EndParameter"/> can be set to <see cref="float.NaN"/> to automatically play 
    /// the animation until the last path key is reached.
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
    /// Initializes a new instance of the <see cref="PathAnimation{TPoint, TPathKey, TPath}"/> class.
    /// </summary>
    protected PathAnimation()
    {
      StartParameter = float.NaN;
      EndParameter = float.NaN;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Determines the natural duration of the path.
    /// </summary>
    /// <returns>The natural duration of the path.</returns>
    private TimeSpan GetNaturalDuration()
    {
      // The natural length is determined by the last key of the path.
      float length = 0;

      var path = Path;
      if (path != null)
      {
        int numberOfKeys = path.Count;
        if (numberOfKeys > 0)
          length = path[numberOfKeys - 1].Parameter;
      }

      return new TimeSpan((long)(length * TimeSpan.TicksPerSecond));
    }


    /// <summary>
    /// Gets the interval of the path that should be played.
    /// </summary>
    /// <param name="start">The start parameter.</param>
    /// <param name="end">The end parameter.</param>
    /// <param name="length">The length of the interval.</param>
    /// <exception cref="InvalidAnimationException">
    /// Invalid <see cref="StartParameter"/> and <see cref="EndParameter"/>.
    /// </exception>
    private void GetClip(out TimeSpan start, out TimeSpan end, out TimeSpan length)
    {
      if (Path == null)
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
          throw new InvalidAnimationException("The start parameter of a path animation must not be greater than the end parameter.");

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
    protected override void GetValueCore(TimeSpan time, ref TPoint defaultSource, ref TPoint defaultTarget, ref TPoint result)
    {
      var path = Path;
      if (path != null && path.Count > 0)
      {
        TimeSpan start, end, length;
        GetClip(out start, out end, out length);
        float parameter = (float)(start + time).TotalSeconds;
        var value = ReturnsTangent ? path.GetTangent(parameter) : path.GetPoint(parameter);
        Traits.Copy(ref value, ref result);
      }
      else
      {
        Traits.Copy(ref defaultSource, ref result);
      }
    }
    #endregion
  }
}
