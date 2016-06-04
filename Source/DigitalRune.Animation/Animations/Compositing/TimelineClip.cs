// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Animation
{
  /// <summary>
  /// Plays back a clip of another animation timeline.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="TimelineClip"/> has the following purposes:
  /// <list type="number">
  /// <item>
  /// <description>
  /// <para>
  /// <strong>Select animation clip:</strong> The properties <see cref="ClipStart"/> and 
  /// <see cref="ClipEnd"/> can be used to select an interval from another animation timeline (see 
  /// property <see cref="Timeline"/>). The properties are optional: <see cref="ClipStart"/> and 
  /// <see cref="ClipEnd"/> are <see langword="null"/> by default, which means that the entire
  /// animation timeline is selected. It is also possible to set only <see cref="ClipStart"/> or 
  /// <see cref="ClipEnd"/> - in this case only one side of the original animation timeline will be
  /// clipped.
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
  /// The property <see cref="LoopBehavior"/> defines the behavior past the end of the clip. The 
  /// default loop behavior is <see cref="Animation.LoopBehavior.Constant"/> which means that the 
  /// last value of the clip is returned for the rest of the duration. The loop behavior 
  /// <see cref="Animation.LoopBehavior.Cycle"/> causes the clip to be repeated from the start. The 
  /// loop behavior <see cref="Animation.LoopBehavior.Oscillate"/> (also known as 'auto-reverse' or 
  /// 'ping-pong') automatically repeats the clip in reverse order, so that the clip is played back 
  /// and forth for the defined duration.
  /// </para>
  /// <para>
  /// Note that the loop behavior <see cref="Animation.LoopBehavior.CycleOffset"/> is not supported.
  /// This behavior is only available for animations of a certain type (use
  /// <see cref="AnimationClip{T}"/> instead).
  /// </para>
  /// </remarks>
  public class TimelineClip : ITimeline
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the original animation timeline from which a clip is played back.
    /// </summary>
    /// <value>The original animation timeline from which a clip played back.</value>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true)]
#endif
    public ITimeline Timeline { get; set; }


    /// <summary>
    /// Gets or sets a value that specifies how the animation behaves when it reaches the end of its 
    /// duration.
    /// </summary>
    /// <value>
    /// A value that specifies how the animation behaves when it reaches the end of its duration.
    /// The default value is <see cref="Animation.FillBehavior.Hold"/>.
    /// </value>
    /// <inheritdoc cref="ITimeline.FillBehavior"/>
    public FillBehavior FillBehavior { get; set; }


    /// <summary>
    /// Gets or sets the object to which the animation is applied by default.
    /// </summary>
    /// <value>
    /// The object to which the animation is applied by default. The default value is 
    /// <see langword="null"/>.
    /// </value>
    /// <inheritdoc cref="ITimeline.TargetObject"/>
    public string TargetObject { get; set; }


    /// <summary>
    /// Gets or sets the start time of the animation clip.
    /// </summary>
    /// <value>
    /// The time at which the original timeline should be started. The default value is 
    /// <see langword="null"/>, which indicates that the original timeline should be played from the
    /// beginning.
    /// </value>
    /// <remarks>
    /// <see cref="ClipStart"/> and <see cref="ClipEnd"/> define the interval of the original 
    /// timeline that should be played.
    /// </remarks>
    public TimeSpan? ClipStart { get; set; }


    /// <summary>
    /// Gets or sets the end time of the animation clip.
    /// </summary>
    /// <value>
    /// The time at which the original timeline should be stopped. The default value is 
    /// <see langword="null"/>, which indicates that the original timeline should be played until it
    /// ends.
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


    /// <summary>
    /// Gets or sets the behavior of the animation past the end of the animation clip.
    /// </summary>
    /// <value>
    /// The behavior of the animation past the end of the animation clip. The default value is 
    /// <see cref="Animation.LoopBehavior.Constant"/>.
    /// </value>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is set to <see cref="Animation.LoopBehavior.CycleOffset"/>. This 
    /// loop behavior is not supported by a <see cref="TimelineClip"/>. (Check out 
    /// <see cref="AnimationClip{T}"/> instead.)
    /// </exception>
    public LoopBehavior LoopBehavior
    {
      get { return _loopBehavior; }
      set
      {
        if (value == LoopBehavior.CycleOffset)
          throw new ArgumentException("TimelineClips do not support the loop behavior 'CycleOffset'. "
                                      + "This loop behavior can only applied to animations of a certain type, but not to untyped animation timelines. "
                                      + "Use a AnimationClip<T> instead if you want to create a cumulative animation.", "value");
        _loopBehavior = value;
      }
    }
    private LoopBehavior _loopBehavior;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineClip"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineClip"/> class.
    /// </summary>
    public TimelineClip()
      : this(null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TimelineClip"/> class for the given timeline.
    /// </summary>
    /// <param name="timeline">The timeline.</param>
    public TimelineClip(ITimeline timeline)
    {
      Timeline = timeline;

      FillBehavior = FillBehavior.Hold;
      TargetObject = null;

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

    /// <inheritdoc/>
    public AnimationInstance CreateInstance()
    {
      var animationInstance = AnimationInstance.Create(this);
      animationInstance.Children.Add(Timeline.CreateInstance());

      return animationInstance;
    }


    /// <summary>
    /// Gets the interval of the original timeline that should be played.
    /// </summary>
    /// <param name="start">The start time of the animation clip.</param>
    /// <param name="end">The end time of the animation clip.</param>
    /// <param name="length">The length of the animation clip.</param>
    /// <exception cref="InvalidAnimationException">
    /// Invalid timeline clip. <see cref="ClipStart"/> is greater than <see cref="ClipEnd"/>.
    /// </exception>
    private void GetClip(out TimeSpan start, out TimeSpan end, out TimeSpan length)
    {
      if (Timeline == null)
      {
        start = TimeSpan.Zero;
        end = TimeSpan.Zero;
        length = TimeSpan.Zero;
      }
      else
      {
        start = ClipStart ?? TimeSpan.Zero;
        end = ClipEnd ?? Timeline.GetTotalDuration();

        if (start > end)
          throw new InvalidAnimationException("The start time of the timeline clip must not be greater than the end time.");

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


    /// <inheritdoc/>
    public TimeSpan? GetAnimationTime(TimeSpan time)
    {
      // Get the effective start, end and duration of the clip.
      TimeSpan clipStart, clipEnd, clipLength;
      GetClip(out clipStart, out clipEnd, out clipLength);

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
      TimeSpan duration = Duration ?? clipLength;

      // ----- FillBehavior
      if (time > duration)
      {
        if (FillBehavior == FillBehavior.Stop)
        {
          // The animation has stopped.
          return null;
        }

        // The animation holds the last value.
        Debug.Assert(FillBehavior == FillBehavior.Hold);
        time = duration;
      }

      // ----- Adjust animation time to play/loop clip from original timeline.
      time = clipStart + ClipOffset + time;
      AnimationHelper.LoopParameter(time, clipStart, clipEnd, LoopBehavior, out time);

      // ----- Reverse
      if (IsClipReversed)
        time = clipEnd - (time - clipStart);

      return time;
    }


    /// <inheritdoc/>
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
    #endregion
  }
}
