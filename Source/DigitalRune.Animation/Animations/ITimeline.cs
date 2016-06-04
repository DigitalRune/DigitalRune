// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Positions an animation along a timeline.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="ITimeline"/> is used to define when an animation starts and how long it is 
  /// active. Timelines can be played back by the animation system.
  /// </para>
  /// <para>
  /// <strong>Fill Behavior:</strong> The property <see cref="FillBehavior"/> defines the behavior
  /// of the animation when its duration is exceeded. The fill behavior 
  /// <see cref="Animation.FillBehavior.Hold"/> indicates that the animation holds its last 
  /// animation value when the end of the duration is reached. The last animation value is returned
  /// until the animation is reset or stopped. The fill behavior 
  /// <see cref="Animation.FillBehavior.Stop"/> indicates that the animation should be removed when 
  /// the end of the duration is reached.
  /// </para>
  /// <para>
  /// <strong>Animation State:</strong> The current state of an animation (see enumeration 
  /// <see cref="AnimationState"/>) depends on the current time on the timeline. The method 
  /// <see cref="GetState"/> can be used to query the current state for a given time value. The 
  /// animation can be <see cref="AnimationState.Delayed"/> when the animation is scheduled but has
  /// not yet started. <see cref="AnimationState.Playing"/> indicates that the animation is active. 
  /// When the time reaches the end of the duration the state becomes either 
  /// <see cref="AnimationState.Filling"/> when the <see cref="FillBehavior"/> is set to 
  /// <see cref="Animation.FillBehavior.Hold"/> or <see cref="AnimationState.Stopped"/> when the 
  /// fill behavior is set to <see cref="Animation.FillBehavior.Stop"/>. When a timeline is 
  /// <see cref="AnimationState.Stopped"/> it is automatically removed from the animation system.
  /// </para>
  /// <para>
  /// <strong>Animation Time:</strong> The animation time is the local time of the animation. The
  /// animation time is required when the animation needs to be evaluated. The function 
  /// <see cref="GetAnimationTime"/> can be used to convert a time value on the timeline to the 
  /// animation time. 
  /// </para>
  /// <para>
  /// <strong>Nested Timelines:</strong> Timelines can be nested: A <see cref="TimelineGroup"/>, for
  /// example, is a timeline that groups other animations. The time values of a nested timeline are
  /// relative to the parent timeline.
  /// </para>
  /// </remarks>
  public interface ITimeline
  {
    /// <summary>
    /// Gets a value that specifies how the animation behaves when it reaches the end of its 
    /// duration.
    /// </summary>
    /// <value>
    /// A value that specifies how the animation behaves when it reaches the end of its duration.
    /// </value>
    FillBehavior FillBehavior { get; }


    /// <summary>
    /// Gets the object to which the animation is applied by default.
    /// </summary>
    /// <value>
    /// The object to which the animation is applied by default.
    /// </value>
    /// <remarks>
    /// See <see cref="IAnimation{T}"/> for more information.
    /// </remarks>
    string TargetObject { get; }


    /// <summary>
    /// Creates an animation instance that can be used to play back the animation. 
    /// (For internal use only.)
    /// </summary>
    /// <returns>
    /// An <see cref="AnimationInstance"/> that can be used to play back the animation.
    /// </returns>
    AnimationInstance CreateInstance();


    /// <summary>
    /// Gets the animation time for the specified time on the timeline.
    /// </summary>
    /// <param name="time">The time on the timeline.</param>
    /// <returns>
    /// The animation time. (The return value is <see langword="null"/> if the animation is not 
    /// active at <paramref name="time"/>.)
    /// </returns>
    TimeSpan? GetAnimationTime(TimeSpan time);


    /// <summary>
    /// Gets the state of the animation for the specified time on the timeline.
    /// </summary>
    /// <param name="time">The time on the timeline.</param>
    /// <returns>The state of the animation.</returns>
    AnimationState GetState(TimeSpan time);


    /// <summary>
    /// Gets the total length of the timeline.
    /// </summary>
    /// <returns>The total length of the timeline.</returns>
    /// <remarks>
    /// <para>
    /// The total duration is the effective length of the animation timeline. Depending on the type 
    /// of timeline, the total duration can be the natural duration of the underlying animation or 
    /// might be set explicitly by the user.
    /// </para>
    /// <para>
    /// <strong>Notes to Implementors:</strong> The duration must be 0 or a positive value. 
    /// <see cref="TimeSpan.MaxValue"/> can be returned to indicate that the animation runs forever.
    /// </para>
    /// </remarks>
    TimeSpan GetTotalDuration();
  }
}
