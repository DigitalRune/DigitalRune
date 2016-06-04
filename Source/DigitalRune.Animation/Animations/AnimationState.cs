// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation
{
  /// <summary>
  /// Defines the state of an animation.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Animations are positioned on a timeline. The current state of an animation depends on the 
  /// current time on the timeline. The method <see cref="ITimeline.GetState"/> can be used to query
  /// the current state for a given time value. 
  /// </para>
  /// <para>
  /// <see cref="Playing"/> indicates that the animation is active and produces animation values.
  /// </para>
  /// <para>
  /// The animation state is <see cref="Delayed"/> when the timeline is active, but the start of the
  /// animation has been delayed. The types <see cref="TimelineClip"/> and 
  /// <see cref="AnimationClip{T}"/> can be used to delay the start of an animation. The animation
  /// state is <see cref="Delayed"/> as long as the current time is less than the 
  /// <see cref="TimelineClip.Delay"/>. The animation does not produce an output in this state.
  /// </para>
  /// <para>
  /// When the time value reaches the end of the duration the state becomes either 
  /// <see cref="Filling"/> when the <see cref="ITimeline.FillBehavior"/> is set to 
  /// <see cref="FillBehavior.Hold"/> or <see cref="Stopped"/> when the fill behavior is set to 
  /// <see cref="FillBehavior.Stop"/>.
  /// </para>
  /// </remarks>
  public enum AnimationState
  {
    /// <summary>
    /// The animation is halted and does not return any values.
    /// </summary>
    Stopped,

    /// <summary>
    /// The start of the animation has been delayed and animation does not yet return any values. 
    /// (Animations can be delayed using the types <see cref="TimelineClip"/> or 
    /// <see cref="AnimationClip{T}"/>.)
    /// </summary>
    Delayed,

    /// <summary>
    /// The animation is active and produces an animation value.
    /// </summary>
    Playing,

    /// <summary>
    /// The duration of the animation is exceeded and the animation returns the last animation 
    /// value.
    /// </summary>
    Filling,
  }
}
