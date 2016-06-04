// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Animation.Traits;
using DigitalRune.Mathematics;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Animation
{
  /// <summary>
  /// Plays back a clip of another animation.
  /// </summary>
  /// <typeparam name="T">The type of the animation value.</typeparam>
  /// <remarks>
  /// <para>
  /// The <see cref="AnimationClip{T}"/> has the following purposes:
  /// <list type="number">
  /// <item>
  /// <description>
  /// <para>
  /// <strong>Select animation clip:</strong> The properties <see cref="ClipStart"/> and 
  /// <see cref="ClipEnd"/> can be used to select an interval from another animation (see property
  /// <see cref="Animation"/>). The properties are optional: <see cref="ClipStart"/> and 
  /// <see cref="ClipEnd"/> are <see langword="null"/> by default, which means that the entire
  /// animation is selected. It is also possible to set only <see cref="ClipStart"/> or 
  /// <see cref="ClipEnd"/> - in this case only one side of the original animation will be clipped.
  /// </para>
  /// <para>
  /// The property <see cref="ClipOffset"/> defines a time offset which is applied when the 
  /// animation clip is played back. If the selected clip is, for example, 10 seconds long and 
  /// <see cref="ClipOffset"/> is 5 seconds, then the playback of the clip will start in the middle.
  /// </para>
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// <strong>Arrange animation clip on timeline:</strong> The properties <see cref="Delay"/>, 
  /// <see cref="Duration"/>, <see cref="Speed"/> can be used to position the selected animation 
  /// clip along the timeline: The start of the animation clip can be postponed using the property 
  /// <see cref="Delay"/>. The duration of the animation can be overridden using the property 
  /// <see cref="Duration"/>. Note that, when the duration exceeds the actual length of the clip 
  /// than the clip is automatically repeated using a certain loop behavior (see below). The 
  /// <see cref="Speed"/> defines the rate at which the animation clip is played back.
  /// </description>
  /// </item>
  /// </list>
  /// </para> 
  /// <para>
  /// <strong>Loop Behavior:</strong> The property <see cref="Duration"/> defines the length of the 
  /// playback. If the duration is not set (default value is <see langword="null"/>), the animation 
  /// clip plays exactly once (clip length = <see cref="ClipEnd"/> - <see cref="ClipStart"/>). If 
  /// the users sets a duration greater than the actual length of the clip, the clip is repeated 
  /// using a certain loop behavior. 
  /// </para>
  /// <para>
  /// The property <see cref="LoopBehavior"/> defines the behavior past the end of the animation 
  /// clip. The default loop behavior is <see cref="DigitalRune.Animation.LoopBehavior.Constant"/> 
  /// which means that the last value of the animation clip is returned for the rest of the 
  /// duration. The loop behavior <see cref="DigitalRune.Animation.LoopBehavior.Cycle"/> causes the 
  /// animation clip to be repeated from the start. The loop behavior 
  /// <see cref="DigitalRune.Animation.LoopBehavior.CycleOffset"/> is similar to 
  /// <see cref="DigitalRune.Animation.LoopBehavior.Cycle"/> except that it also applies an offset
  /// to the animation values in each new iteration. The loop behavior 
  /// <see cref="DigitalRune.Animation.LoopBehavior.Oscillate"/> (also known as 'auto-reverse' or
  /// 'ping-pong') automatically repeats the animation clip in reverse order, so that the animation
  /// clip is played back and forth for the defined duration.
  /// </para>
  /// </remarks>
  public class AnimationClip<T> : IAnimation<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the original animation from which a clip is played back.
    /// </summary>
    /// <value>The original animation from which a clip is played back.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public IAnimation<T> Animation { get; set; }


    /// <summary>
    /// Gets or sets a value that specifies how the animation behaves when it reaches the end of its 
    /// duration.
    /// </summary>
    /// <value>
    /// A value that specifies how the animation behaves when it reaches the end of its duration.
    /// The default value is <see cref="DigitalRune.Animation.FillBehavior.Hold"/>.
    /// </value>
    /// <inheritdoc cref="TimelineClip.FillBehavior"/>
    public FillBehavior FillBehavior { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the output of the animation is added to the current
    /// value of the property that is being animated.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this animation is additive; otherwise, <see langword="false"/>.
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool IsAdditive { get; set; }


    /// <summary>
    /// Gets or sets the object to which the animation is applied by default.
    /// </summary>
    /// <value>
    /// The object to which the animation is applied by default. The default value is 
    /// <see langword="null"/>.
    /// </value>
    /// <inheritdoc cref="ITimeline.TargetObject"/>
    public string TargetObject
    {
      get 
      {
        // Fall back to Animation.TargetObject if _targetObject is empty.
        if (String.IsNullOrEmpty(_targetObject) && Animation != null)
          return Animation.TargetObject;
        else
          return _targetObject;
      }
      set { _targetObject = value; }
    }
    private string _targetObject;


    /// <summary>
    /// Gets or sets the property to which the animation is applied by default.
    /// </summary>
    /// <value>
    /// The property to which the animation is applied by default. The default value is 
    /// <see langword="null"/>
    /// </value>
    /// <inheritdoc cref="IAnimation.TargetProperty"/>
    public string TargetProperty
    {
      get
      {
        // Fall back to Animation.TargetProperty if _targetProperty is empty.
        if (String.IsNullOrEmpty(_targetProperty) && Animation != null)
          return Animation.TargetProperty;
        else
          return _targetProperty;
      }
      set { _targetProperty = value; }
    }
    private string _targetProperty;


    /// <summary>
    /// Gets or sets the start time of the animation clip.
    /// </summary>
    /// <value>
    /// The time at which the original animation should be started. The default value is 
    /// <see langword="null"/>, which indicates that the original animation should be played from 
    /// the beginning.
    /// </value>
    /// <remarks>
    /// <see cref="ClipStart"/> and <see cref="ClipEnd"/> define the interval of the original 
    /// animation that should be played.
    /// </remarks>
    public TimeSpan? ClipStart { get; set; }


    /// <summary>
    /// Gets or sets the end time of the animation clip.
    /// </summary>
    /// <value>
    /// The time at which the original animation should be stopped. The default value is 
    /// <see langword="null"/>, which indicates that the original animation should be played until 
    /// it ends.
    /// </value>
    /// <inheritdoc cref="ClipStart"/>
    public TimeSpan? ClipEnd { get; set; }


    /// <summary>
    /// Gets the time offset that is applied to the selected animation clip.
    /// </summary>
    /// <value>
    /// A time offset that is applied to the animation clip. The default value is 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// The property <see cref="ClipOffset"/> defines a time offset which is applied when the
    /// animation clip is played back. If the selected clip is, for example, 10 seconds long and
    /// <see cref="ClipOffset"/> is 5 seconds, then the playback of the clip will start in the
    /// middle.
    /// </para>
    /// <para>
    /// By default, the animation clip is played forward from <see cref="ClipStart"/> to
    /// <see cref="ClipEnd"/>. In this case the <see cref="ClipOffset"/> is added to the
    /// <see cref="ClipStart"/>.
    /// </para>
    /// <para>
    /// When <see cref="IsClipReversed"/> is set, the animation clip is played backward from
    /// <see cref="ClipEnd"/> to <see cref="ClipStart"/>. In this case the <see cref="ClipOffset"/>
    /// is subtracted from <see cref="ClipEnd"/>.
    /// </para>
    /// </remarks>
    public TimeSpan ClipOffset { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether to play the clip in reverse.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the clip is played in reverse; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    public bool IsClipReversed { get; set; }


    /// <summary>
    /// Gets or sets the time at which the animation clip begins.
    /// </summary>
    /// <value>
    /// The time at which the animation should begin. The default value is 0.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property can be used to delay the start of an animation. The delay time marks the time
    /// on the timeline when the animation clip starts. The <see cref="Speed"/> does not affect the 
    /// delay. For example, an animation clip with a delay of 3 seconds, a duration of 10 seconds 
    /// and a speed ratio of 2 will start after 3 seconds and run for 5 seconds with double speed.
    /// </para>
    /// <para>
    /// Note: The delay time can also be negative. For example, an animation with a delay time of 
    /// -2.5 seconds and a duration of 5 seconds will start right in the middle of the animation 
    /// clip.
    /// </para>
    /// </remarks>
    public TimeSpan Delay { get; set; }


    /// <summary>
    /// Gets or sets the duration for which the animation clip is played.
    /// </summary>
    /// <value>
    /// The duration for which the animation clip is played. The default value is 
    /// <see langword="null"/>, which indicates that the animation clip should be played once from 
    /// <see cref="ClipStart"/> to <see cref="ClipEnd"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The properties <see cref="ClipStart"/> and <see cref="ClipEnd"/> define the duration of the
    /// animation clip. If the properties are not set then the original animation is played in its
    /// entirety. The property <see cref="Duration"/> can be set to override the duration of the 
    /// animation clip. If <see cref="Duration"/> is greater than the length of the animation clip, 
    /// the clip will be repeated using the defined loop behavior (see <see cref="LoopBehavior"/>).
    /// </para>
    /// <para>
    /// The effective duration depends on the <see cref="Speed"/>: For example, an animation clip 
    /// with a delay of 3 seconds, a duration of 10 seconds and a speed ratio of 2 will start after 
    /// 3 seconds and run for 5 seconds with double speed.
    /// </para>
    /// <para>
    /// The default value is <see langword="null"/>, which indicates that the duration is 
    /// 'automatic' or 'unknown'. In this case the animation clip plays exactly once. A duration of
    /// <see cref="TimeSpan.MaxValue"/> can be set to repeat the animation clip forever. 
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public TimeSpan? Duration
    {
      get { return _duration; }
      set
      {
        if (value.HasValue && value.Value < TimeSpan.Zero)
          throw new ArgumentOutOfRangeException("value", "The duration of an animation must not be negative.");

        _duration = value;
      }
    }
    private TimeSpan? _duration;


    /// <summary>
    /// Gets or sets the speed ratio at which the animation clip is played.
    /// </summary>
    /// <value>
    /// The rate at which time progresses for the animation clip. The value must be a finite number 
    /// greater than or equal to 0. The default value is 1.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or not a finite value.
    /// </exception>
    public float Speed
    {
      get { return _speed; }
      set
      {
        if (!Numeric.IsZeroOrPositiveFinite(value))
          throw new ArgumentOutOfRangeException("value", "The speed must be a finite number greater than or equal to 0.");

        _speed = value;
      }
    }
    private float _speed;


    /// <inheritdoc cref="IAnimation{T}.Traits"/>
    public IAnimationValueTraits<T> Traits
    {
      get
      {
        if (Animation != null)
          return Animation.Traits;

        return null;
      }
    }


    /// <summary>
    /// Gets or sets the behavior of the animation past the end of the animation clip.
    /// </summary>
    /// <value>
    /// The behavior of the animation past the end of the animation clip. The default value is 
    /// <see cref="DigitalRune.Animation.LoopBehavior.Constant"/>.
    /// </value>
    public LoopBehavior LoopBehavior { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationClip{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationClip{T}"/> class.
    /// </summary>
    public AnimationClip()
      : this(null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationClip{T}"/> class for the given
    /// animation.
    /// </summary>
    /// <param name="animation">The original animation.</param>
    public AnimationClip(IAnimation<T> animation)
    {
      Animation = animation;

      FillBehavior = FillBehavior.Hold;
      IsAdditive = false;
      TargetObject = null;
      TargetProperty = null;

      ClipStart = null;
      ClipEnd = null;
      ClipOffset = TimeSpan.Zero;
      IsClipReversed = false;

      Delay = TimeSpan.Zero;
      Duration = null;
      Speed = 1.0f;
      LoopBehavior = LoopBehavior.Constant;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc cref="ITimeline.CreateInstance"/>
    public AnimationInstance CreateInstance()
    {
      return AnimationInstance<T>.Create(this);
    }


    /// <inheritdoc cref="IAnimation.CreateBlendAnimation"/>
    public BlendAnimation CreateBlendAnimation()
    {
      return new BlendAnimation<T>();
    }


    /// <summary>
    /// Gets the interval of the original animation that should be played.
    /// </summary>
    /// <param name="start">The start time of the animation clip.</param>
    /// <param name="end">The end time of the animation clip.</param>
    /// <param name="length">The length of the animation clip.</param>
    /// <exception cref="InvalidAnimationException">
    /// Invalid animation clip. <see cref="ClipStart"/> is greater than <see cref="ClipEnd"/>.
    /// </exception>
    private void GetClip(out TimeSpan start, out TimeSpan end, out TimeSpan length)
    {
      if (Animation == null)
      {
        start = TimeSpan.Zero;
        end = TimeSpan.Zero;
        length = TimeSpan.Zero;
      }
      else
      {
        start = ClipStart ?? TimeSpan.Zero;
        end = ClipEnd ?? Animation.GetTotalDuration();

        if (start > end)
          throw new InvalidAnimationException("The start time of the animation clip must not be greater than the end time.");

        length = end - start;
      }
    }


    private TimeSpan GetDefaultDuration()
    {
      TimeSpan start, end, length;
      GetClip(out start, out end, out length);
      return length;
    }


    /// <inheritdoc cref="ITimeline.GetState"/>
    public AnimationState GetState(TimeSpan time)
    {
      // ----- Delay
      time -= Delay;
      if (time < TimeSpan.Zero)
      {
        // The animation has not started.
        return AnimationState.Delayed;
      }

      // ----- Speed
      time = new TimeSpan((long)(time.Ticks * (double)Speed));

      // ----- Duration
      TimeSpan duration = Duration ?? GetDefaultDuration();

      // ----- FillBehavior
      if (time > duration)
      {
        if (FillBehavior == FillBehavior.Stop)
        {
          // The animation has stopped.
          return AnimationState.Stopped;
        }

        // The animation holds the final value.
        Debug.Assert(FillBehavior == FillBehavior.Hold);
        return AnimationState.Filling;
      }

      return AnimationState.Playing;
    }


    /// <inheritdoc cref="ITimeline.GetAnimationTime"/>
    public virtual TimeSpan? GetAnimationTime(TimeSpan time)
    {
      // ----- Delay
      time -= Delay;
      if (time < TimeSpan.Zero)
      {
        // The animation has not started.
        return null;
      }

      // ----- Speed
      time = new TimeSpan((long)(time.Ticks * (double)Speed));

      // ----- Duration
      TimeSpan duration = Duration ?? GetDefaultDuration();

      // ----- FillBehavior
      if (time > duration)
      {
        if (FillBehavior == FillBehavior.Stop)
        {
          // The animation has stopped.
          return null;
        }

        // The animation holds the final value.
        Debug.Assert(FillBehavior == FillBehavior.Hold);
        return duration;
      }

      return time;
    }


    /// <inheritdoc cref="ITimeline.GetTotalDuration"/>
    public TimeSpan GetTotalDuration()
    {
      float speed = Speed;
      if (Numeric.IsZero(speed))
        return TimeSpan.MaxValue;

      TimeSpan delay = Delay;

      TimeSpan duration = Duration ?? GetDefaultDuration();
      if (duration == TimeSpan.MaxValue)
        return TimeSpan.MaxValue;

      return new TimeSpan(delay.Ticks + (long)(duration.Ticks / (double)speed));
    }


    /// <inheritdoc cref="IAnimation{T}.GetValue"/>
    /// <exception cref="InvalidAnimationException">
    /// Cannot evaluate animation clip because the animation clip is empty.
    /// </exception>
    public void GetValue(TimeSpan time, ref T defaultSource, ref T defaultTarget, ref T result)
    {
      if (Animation == null)
        throw new InvalidAnimationException("Cannot evaluate animation clip because the animation clip is empty.");

      TimeSpan? animationTime = GetAnimationTime(time);
      if (!animationTime.HasValue)
      {
        // Animation is inactive and does not produce any output.
        Traits.Copy(ref defaultSource, ref result);
        return;
      }

      time = animationTime.Value;
      if (time < TimeSpan.Zero)
      {
        // Animation has not started yet.
        Traits.Copy(ref defaultSource, ref result);
        return;
      }

      if (!IsAdditive)
      {
        // Evaluate animation.
        GetValueCore(time, ref defaultSource, ref defaultTarget, ref result);
      }
      else
      {
        // Additive animation.
        var traits = Traits;

        // 'defaultSource' and 'result' may be the same instance! We need to ensure that
        // the source value is not overwritten in GetValueCore().
        // --> Create temporary copy of defaultSource. 
        T source;
        traits.Create(ref defaultSource, out source);
        traits.Copy(ref defaultSource, ref source);

        // Evaluate animation.
        GetValueCore(time, ref defaultSource, ref defaultTarget, ref result);

        // Add the animation output to the source value.
        // (Order of parameters: The additive animation is usually defined in the local 
        // (untransformed) space of the object and therefore needs to be applied first.)
        traits.Add(ref result, ref source, ref result);

        traits.Recycle(ref source);
      }
    }


    private void GetValueCore(TimeSpan time, ref T defaultSource, ref T defaultTarget, ref T result)
    {
      if (Animation == null)
      {
        Traits.Copy(ref defaultSource, ref result);
        return;
      }

      TimeSpan clipStart, clipEnd, clipLength;
      GetClip(out clipStart, out clipEnd, out clipLength);

      // Correct time parameter.
      time = clipStart + ClipOffset + time;

      TimeSpan loopedTime;
      bool hasCycleOffset = AnimationHelper.LoopParameter(time, clipStart, clipEnd, LoopBehavior, out loopedTime);

      // ----- Reverse
      if (IsClipReversed)
        loopedTime = clipEnd - (loopedTime - clipStart);

      if (!hasCycleOffset)
      {
        // Get animation value.
        Animation.GetValue(loopedTime, ref defaultSource, ref defaultTarget, ref result);
      }
      else
      {
        // Animation with cycle offset.
        var traits = Traits;

        // 'defaultSource', 'defaultTarget' and 'result' may be the same instance! We need to 
        // ensure that the source and target values are not overwritten by GetValue().
        // --> Use local variable to get animation value. 
        T value, startValue, endValue, cycleOffset;
        traits.Create(ref defaultSource, out value);
        traits.Create(ref defaultSource, out startValue);
        traits.Create(ref defaultSource, out endValue);
        traits.Create(ref defaultSource, out cycleOffset);

        // Get animation value.
        Animation.GetValue(loopedTime, ref defaultSource, ref defaultTarget, ref value);

        // Apply cycle offset.
        Animation.GetValue(clipStart, ref defaultSource, ref defaultTarget, ref startValue);
        Animation.GetValue(clipEnd, ref defaultSource, ref defaultTarget, ref endValue);
        
        if (IsClipReversed)
          MathHelper.Swap(ref startValue, ref endValue);

        AnimationHelper.GetCycleOffset(time, clipStart, clipEnd, ref startValue, ref endValue, traits, LoopBehavior, ref cycleOffset);
        traits.Add(ref value, ref cycleOffset, ref result);

        traits.Recycle(ref cycleOffset);
        traits.Recycle(ref endValue);
        traits.Recycle(ref startValue);
        traits.Recycle(ref value);
      }
    }
    #endregion
  }
}
