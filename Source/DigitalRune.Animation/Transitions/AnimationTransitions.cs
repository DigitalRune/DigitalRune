// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Animation.Transitions;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Provides a set of predefined transitions to start or stop animations. 
  /// </summary>
  /// <remarks>
  /// <strong>Important:</strong> Animation transitions cannot be reused for multiple animations.
  /// When an animation is started a new animation transition needs to be created using one of the 
  /// methods of this class.
  /// </remarks>
  public static class AnimationTransitions
  {
    /// <summary>
    /// Takes a snapshot of the current animation and then starts the new animation. The new
    /// animation is initialized with the snapshot and takes effect immediately. The previous 
    /// animations are stopped and removed from the animation system. 
    /// </summary>
    /// <returns>The <see cref="AnimationTransition"/>.</returns>
    /// <remarks>
    /// Usually, the property's base value is passed to the first animation in composition chain. 
    /// When using <see cref="SnapshotAndReplace"/> a snapshot of the current animation value is 
    /// created. The first animation in the composition chain will receive the snapshot instead of 
    /// the base value as its input. The snapshot will be active until a new snapshot is created (by
    /// starting a new animation using <see cref="SnapshotAndReplace"/>), or until all animations on
    /// the property are stopped.
    /// </remarks>
    public static AnimationTransition SnapshotAndReplace()
    {
      return new SnapshotAndReplaceAllTransition();
    }


    /// <overloads>
    /// <summary>
    /// Replaces existing animations with a new animation. The previous animations are stopped and 
    /// removed from the animation system.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Replaces all existing animations with the new animation. The new animation takes effect
    /// immediately. The previous animations are stopped and removed from the animation system. 
    /// </summary>
    /// <returns>The <see cref="AnimationTransition"/>.</returns>
    public static AnimationTransition Replace()
    {
      return new ReplaceAllTransition();
    }


    /// <summary>
    /// Gradually replaces all existing animations with the new animation. The new animation fades
    /// in over the specified duration. After this duration the previous animations are stopped and 
    /// removed from the animation system. 
    /// </summary>
    /// <param name="fadeInDuration">The duration over which the new animation fades in.</param>
    /// <returns>The <see cref="AnimationTransition"/>.</returns>
    public static AnimationTransition Replace(TimeSpan fadeInDuration)
    {
      if (fadeInDuration > TimeSpan.Zero)
        return new FadeInAndReplaceAllTransition(fadeInDuration);

      return new ReplaceAllTransition();
    }


    /// <summary>
    /// Replaces the specified animation with the new animation. The new animation takes effect
    /// immediately. The previous animation is stopped and removed from the animation system. 
    /// </summary>
    /// <param name="previousAnimation">The animation that should be replaced.</param>
    /// <returns>The <see cref="AnimationTransition"/>.</returns>
    public static AnimationTransition Replace(AnimationInstance previousAnimation)
    {
      return new ReplaceTransition(previousAnimation);
    }


    /// <summary>
    /// Gradually replaces the specified animation with the new animation. The new animation fades
    /// in over the specified duration. After this duration the previous animation is stopped and 
    /// removed from the animation system. 
    /// </summary>
    /// <param name="previousAnimation">The animation that should be replaced.</param>
    /// <param name="fadeInDuration">The duration over which the new animation fades in.</param>
    /// <returns>The <see cref="AnimationTransition"/>.</returns>
    public static AnimationTransition Replace(AnimationInstance previousAnimation, TimeSpan fadeInDuration)
    {
      if (fadeInDuration > TimeSpan.Zero)
        return new FadeInAndReplaceTransition(previousAnimation, fadeInDuration);

      return new ReplaceTransition(previousAnimation);
    }


    /// <overloads>
    /// <summary>
    /// Combines a new animation with existing animations by adding the new animation to the 
    /// composition chains immediately.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Combines the new animation. with existing animations by appending the new animation to the 
    /// end of the composition chains.
    /// </summary>
    /// <returns>The <see cref="AnimationTransition"/>.</returns>
    public static AnimationTransition Compose()
    {
      return new ComposeTransition();
    }


    /// <summary>
    /// Gradually combines the new animation with existing animations by appending the new animation 
    /// to the end of the composition chains. The new animation fades in over the specified 
    /// duration.
    /// </summary>
    /// <param name="fadeInDuration">The duration over which the new animation fades in.</param>
    /// <returns>The <see cref="AnimationTransition"/>.</returns>
    public static AnimationTransition Compose(TimeSpan fadeInDuration)
    {
      if (fadeInDuration > TimeSpan.Zero)
        return new FadeInAndComposeTransition(fadeInDuration);

      return new ComposeTransition();
    }


    /// <summary>
    /// Combines the new animation with existing animations by inserting the new animation after the 
    /// specified animation into the composition chains. The new animation takes effect immediately.
    /// </summary>
    /// <param name="previousAnimation">
    /// The animation after which the new animation should be added.
    /// </param>
    /// <returns>The <see cref="AnimationTransition"/>.</returns>
    public static AnimationTransition Compose(AnimationInstance previousAnimation)
    {
      return new ComposeTransition(previousAnimation);
    }


    /// <summary>
    /// Combines the new animation with existing animations by inserting the new animation after the
    /// specified animation into the composition chains. The new animation fades in over the
    /// specified duration.
    /// </summary>
    /// <param name="previousAnimation">
    /// The animation after which the new animation should be added.
    /// </param>
    /// <param name="fadeInDuration">The duration over which the new animation fades in.</param>
    /// <returns>The <see cref="AnimationTransition"/>.</returns>
    public static AnimationTransition Compose(AnimationInstance previousAnimation, TimeSpan fadeInDuration)
    {
      if (fadeInDuration > TimeSpan.Zero)
        return new FadeInAndComposeTransition(previousAnimation, fadeInDuration);

      return new ComposeTransition(previousAnimation);
    }
  }
}
