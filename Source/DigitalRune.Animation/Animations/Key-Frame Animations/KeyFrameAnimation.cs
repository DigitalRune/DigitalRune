// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates a value based on predefined key frames. (Base implementation.)
  /// </summary>
  /// <typeparam name="T">The type of the animation value.</typeparam>
  /// <remarks>
  /// <para>
  /// A key frame animation contains a list of key frames (see property <see cref="KeyFrames"/>) 
  /// that define the animation values at certain points in time. When the animation is played the 
  /// class automatically looks up the animation value in the list of key frames.
  /// </para>
  /// <para>
  /// <strong>Key Frame Interpolation:</strong> The property <see cref="EnableInterpolation"/>
  /// defines whether interpolation between key frames is enabled. When the property is set
  /// (default) the values between two key frames are interpolated. Each key frame animation class
  /// decides which type of a interpolation is most appropriate. For example, linear interpolation
  /// (LERP) is used for <see langword="Single"/>, <see cref="Vector2F"/>, <see cref="Vector3F"/>, 
  /// etc. LERP is also used for <see cref="QuaternionF"/> (Spherical linear interpolation (SLERP)
  /// is not used for performance reasons). When 
  /// interpolation is disabled, the animation returns the value of the previous key frame. 
  /// </para>
  /// <para>
  /// Please note that key frame animations (such as <see cref="SingleKeyFrameAnimation"/>, 
  /// <see cref="Vector2FKeyFrameAnimation"/>, <see cref="Vector3FKeyFrameAnimation"/>, etc.) 
  /// provide only limited control over interpolation between key frames. Curve-based animations 
  /// (such as <see cref="Curve2FAnimation"/>, <see cref="Path2FAnimation"/>, 
  /// <see cref="Path3FAnimation"/>, etc.) offer more advanced control: Curve-based animations allow
  /// to define an interpolation spline for each segment of the animation.
  /// </para>
  /// <para>
  /// <strong>Cyclic Animations:</strong> A key frame animation, by default, runs until the last 
  /// key frame is reached. The types <see cref="TimelineClip"/> and <see cref="AnimationClip{T}"/>
  /// can be used to repeat the entire key frame animation (or a certain clip) for a number of times 
  /// using a certain loop-behavior (see <see cref="TimelineClip.LoopBehavior"/>).
  /// </para>
  /// <para>
  /// <strong>Important:</strong> The key frame animation requires that the key frames are sorted 
  /// ascending by their time value. The method <see cref="KeyFrameCollection{T}.Sort"/> can be 
  /// called to sort all key frames.
  /// </para>
  /// </remarks>
  public abstract class KeyFrameAnimation<T> : Animation<T> 
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether values between key frames are interpolated.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if interpolation of key frames is enabled; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    public bool EnableInterpolation { get; set; }


    /// <summary>
    /// Gets the collection of key frames.
    /// </summary>
    /// <value>The collection of key frames.</value>
#if XNA || MONOGAME
    [ContentSerializer]
#endif
    public KeyFrameCollection<T> KeyFrames { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyFrameAnimation{T}"/> class.
    /// </summary>
    protected KeyFrameAnimation()
    {
      EnableInterpolation = true;
      KeyFrames = new KeyFrameCollection<T>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the index of the key frame <i>before</i> or at the given animation time.
    /// </summary>
    /// <param name="time">The animation time.</param>
    /// <returns>
    /// The index of the key frame or <c>-1</c> if no suitable key frame exists.
    /// </returns>
    /// <remarks>
    /// This method assumes that the key frames are sorted and returns index of the key frame with 
    /// the largest <see cref="IKeyFrame{T}.Time"/> value that is less than or equal to the given 
    /// parameter <paramref name="time"/>. The time value will lie between the key frame at the 
    /// returned index and the key frame at index + 1. If <paramref name="time"/> is beyond the 
    /// start or end of the path, a key frame index according to <see cref="LoopBehavior"/> is 
    /// returned.
    /// </remarks>
    private int GetKeyFrameIndex(TimeSpan time)
    {
      var keyFrames = KeyFrames;
      Debug.Assert(keyFrames != null, "GetKeyFrameIndex should not be called if the KeyFrameCollection is null.");

      int numberOfKeys = keyFrames.Count;
      Debug.Assert(numberOfKeys > 1, "GetKeyFrameIndex should not be called if there is only one key frame.");

      // Search range.
      int start = 0;
      int end = numberOfKeys - 1;
     
      // Handle looping. (Not needed because the time is clamped in GetValueCore.)
      //var first = keyFrames[start];
      //var last = keyFrames[end];
      //AnimationHelper.LoopParameter(time, first.Time, last.Time, LoopBehavior.Constant, out time);
      //Debug.Assert(first.Time <= time && time <= last.Time, "LoopParameter should return value in the interval [startTime, endTime].");

      // Binary search.
      while (start <= end)
      {
        int index = start + (end - start >> 1);
        int comparison = TimeSpan.Compare(keyFrames[index].Time, time);
        if (comparison == 0)
        {
          return index;
        }

        if (comparison < 0)
        {
          Debug.Assert(time > keyFrames[index].Time);
          start = index + 1;
        }
        else
        {
          Debug.Assert(time < keyFrames[index].Time);
          end = index - 1;
        }
      }

      return start - 1;
    }


    /// <inheritdoc/>
    public override TimeSpan GetTotalDuration()
    {
      // The duration is determined by the last key of the curve.
      var keyFrames = KeyFrames;
      Debug.Assert(keyFrames != null);

      int numberOfKeyFrames = keyFrames.Count;
      if (numberOfKeyFrames > 0)
        return keyFrames[numberOfKeyFrames - 1].Time;

      return TimeSpan.Zero;
    }


    /// <inheritdoc/>
    protected override void GetValueCore(TimeSpan time, ref T defaultSource, ref T defaultTarget, ref T result)
    {
      var keyFrames = KeyFrames;
      Debug.Assert(keyFrames != null);

      int numberOfKeyFrames = keyFrames.Count;
      if (numberOfKeyFrames == 0)
      {
        Traits.Copy(ref defaultSource, ref result);
        return;
      }

      var first = keyFrames[0];
      var last = keyFrames[keyFrames.Count - 1];

      // Correct time parameter.
      if (time < first.Time)
        time = first.Time;
      else if (time > last.Time)
        time = last.Time;

      // Special case: Only 1 key frame.
      if (numberOfKeyFrames == 1)
      {
        var value = keyFrames[0].Value;
        Traits.Copy(ref value, ref result);
        return;
      }

      // Get animation value.
      int index = GetKeyFrameIndex(time);
      if (!EnableInterpolation || index == numberOfKeyFrames - 1)
      {
        // No interpolation required.
        var value = keyFrames[index].Value;
        Traits.Copy(ref value, ref result);
        return;
      }

      // Handle linear interpolation.
      var source = keyFrames[index];
      var target = keyFrames[index + 1];

      TimeSpan sourceTime = source.Time;
      TimeSpan targetTime = target.Time;
      TimeSpan distance = targetTime - sourceTime;
      float parameter = (distance > TimeSpan.Zero) ? (float)(time.Ticks - sourceTime.Ticks) / distance.Ticks : 0.0f;

      T sourceValue = source.Value;
      T targetValue = target.Value;
      Traits.Interpolate(ref sourceValue, ref targetValue, parameter, ref result);
    }
    #endregion
  }
}
