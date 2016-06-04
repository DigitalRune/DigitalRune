// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Animation.Traits;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Provides a base implementation for animations.
  /// </summary>
  /// <typeparam name="T">The type of the animation value.</typeparam>
  /// <remarks>
  /// <para>
  /// <see cref="Animation{T}"/> provides a base implementation which is extended by the different
  /// types of animations in DigitalRune Animation. The base class implements the interfaces 
  /// <see cref="ITimeline"/> and <see cref="IAnimation{T}"/>, which means that an 
  /// <see cref="Animation{T}"/> is both a timeline and an animation. The timeline part defines when
  /// an animation starts and how long it is active. (When the animation system is playing an 
  /// animation it is actually playing back a timeline.) The animation part defines the actual 
  /// animation of a value.
  /// </para>
  /// <para>
  /// See interfaces <see cref="ITimeline"/> and <see cref="IAnimation{T}"/> to read more about 
  /// timelines and animations.
  /// </para>
  /// </remarks>
  public abstract class Animation<T> : IAnimation<T> 
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

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
    public string TargetObject { get; set; }


    /// <summary>
    /// Gets or sets the property to which the animation is applied by default.
    /// </summary>
    /// <value>
    /// The property to which the animation is applied by default. The default value is 
    /// <see langword="null"/>
    /// </value>
    /// <inheritdoc cref="IAnimation.TargetProperty"/>
    public string TargetProperty { get; set; }


    /// <inheritdoc/>
    public abstract IAnimationValueTraits<T> Traits { get; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Animation{T}"/> class.
    /// </summary>
    protected Animation()
    {
      FillBehavior = FillBehavior.Hold;
      IsAdditive = false;
      TargetObject = null;
      TargetProperty = null;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public AnimationInstance CreateInstance()
    {
      return AnimationInstance<T>.Create(this);
    }


    /// <inheritdoc/>
    public BlendAnimation CreateBlendAnimation()
    {
      return new BlendAnimation<T>();
    }


    /// <inheritdoc/>
    public TimeSpan? GetAnimationTime(TimeSpan time)
    {
      return AnimationHelper.GetAnimationTime(this, time);
    }


    /// <inheritdoc/>
    public AnimationState GetState(TimeSpan time)
    {
      return AnimationHelper.GetState(this, time);
    }


    /// <inheritdoc/>
    public abstract TimeSpan GetTotalDuration();


    /// <inheritdoc/>
    public void GetValue(TimeSpan time, ref T defaultSource, ref T defaultTarget, ref T result)
    {
      TimeSpan? animationTime = GetAnimationTime(time);
      if (animationTime == null)
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
        // the source value is not overwritten by GetValueCore().
        // --> Use local variable to get animation value. 
        T value;
        traits.Create(ref defaultSource, out value);

        // Evaluate animation.
        GetValueCore(time, ref defaultSource, ref defaultTarget, ref value);

        // Add the animation output to the source value.
        // (Order of parameters: The additive animation is usually defined in the local 
        // (untransformed) space of the object and therefore needs to be applied first.)
        traits.Add(ref value, ref defaultSource, ref result);

        traits.Recycle(ref value);
      }
    }


    /// <summary>
    /// Evaluates the animation function at the specified animation time.
    /// </summary>
    /// <param name="time">The animation time.</param>
    /// <param name="defaultSource">
    /// In: The source value that should be used by the animation if the animation does not have its 
    /// own source value.
    /// </param>
    /// <param name="defaultTarget">
    /// In: The target value that should be used by the animation if the animation does not have its 
    /// own target value.
    /// </param>
    /// <param name="result">
    /// Out: The value of the animation at the given time.
    /// </param>
    /// <remarks>
    /// <para>
    /// The method <see cref="GetValueCore"/> implements the <i>animation function</i>. It is called
    /// automatically by <see cref="GetValue"/> to compute the current animation value.
    /// </para>
    /// <para>
    /// Note that the parameters are passed by reference. <paramref name="defaultSource"/> and
    /// <paramref name="defaultTarget"/> are input parameters. The resulting animation value is 
    /// stored in <paramref name="result"/>. 
    /// </para>
    /// <para>
    /// The values of the <paramref name="defaultSource"/> and the <paramref name="defaultTarget"/>
    /// parameter depends on where the animation is used. If the animation is used to animate an 
    /// <see cref="IAnimatableProperty{T}"/> then the values depend on the position of the animation
    /// in the composition chain:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// If the animation has replaced another animation using 
    /// <see cref="AnimationTransitions.SnapshotAndReplace"/>: <paramref name="defaultSource"/> is
    /// the last output value of the animation which was replaced and 
    /// <paramref name="defaultTarget"/> is the base value of the animated property.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// If the animation is the first in an animation composition chain: 
    /// <paramref name="defaultSource"/> and <paramref name="defaultTarget"/> are the base value of
    /// the animated property.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// If the animation is not the first in an animation composition chain: 
    /// <paramref name="defaultSource"/> is the output of the previous stage in the composition 
    /// chain and <paramref name="defaultTarget"/> is the base value of the animated property.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The sole purpose of this method is to evaluate the
    /// animation function at the given time. All other tasks (handling of additive animations,
    /// animation blending) are automatically handled by the base class.
    /// </para>
    /// <para>
    /// The parameter <paramref name="time"/> contains the <i>local time</i> of the animation. (Any 
    /// parameters such as <see cref="TimelineClip.Delay"/>, <see cref="TimelineClip.Duration"/>,
    /// <see cref="TimelineClip.Speed"/>, or <see cref="TimelineClip.LoopBehavior"/> have already 
    /// been applied.)
    /// </para>
    /// </remarks>
    protected abstract void GetValueCore(TimeSpan time, ref T defaultSource, ref T defaultTarget, ref T result);
    #endregion
  }
}
