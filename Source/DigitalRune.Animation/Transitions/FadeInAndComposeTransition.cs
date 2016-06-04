// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation.Transitions
{
  /// <summary>
  /// Gradually combines the new animation with existing animations by adding the animation into 
  /// composition chains.
  /// </summary>
  internal sealed class FadeInAndComposeTransition : AnimationTransition
  {
    // Note: In theory it is possible that the user creates a FadeInAndComposeTransition(previousAnimation),
    // stops/recycles the previousAnimation, and then starts the transition. In this case 
    // the results are undefined. (Should not happen in practice.)

    private readonly AnimationInstance _previousAnimation;
    private readonly TimeSpan _fadeInDuration;


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="FadeInAndComposeTransition"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="FadeInAndComposeTransition"/> class.
    /// </summary>
    /// <param name="fadeInDuration">The duration over which the new animation fades in.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fadeInDuration"/> is 0 or negative.
    /// </exception>
    public FadeInAndComposeTransition(TimeSpan fadeInDuration)
      : this(null, fadeInDuration)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="FadeInAndComposeTransition"/> class.
    /// </summary>
    /// <param name="previousAnimation">The animation that should be replaced.</param>
    /// <param name="fadeInDuration">The duration over which the new animation fades in.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fadeInDuration"/> is 0 or negative.
    /// </exception>
    public FadeInAndComposeTransition(AnimationInstance previousAnimation, TimeSpan fadeInDuration)
    {
      if (fadeInDuration <= TimeSpan.Zero)
        throw new ArgumentOutOfRangeException("fadeInDuration", "The fade-in duration must be greater than 0.");

      _previousAnimation = previousAnimation;
      _fadeInDuration = fadeInDuration;
    }


    /// <inheritdoc/>
    protected override void OnInitialize(AnimationManager animationManager)
    {
      // Start fade-in animation on animation weight.
      var fadeInAnimation = new SingleFromToByAnimation
      {
        From = 0,
        Duration = _fadeInDuration,
        EasingFunction = DefaultEase,
        FillBehavior = FillBehavior.Stop,
      };
      animationManager.StartAnimation(fadeInAnimation, AnimationInstance.WeightProperty);
      animationManager.Add(AnimationInstance, HandoffBehavior.Compose, _previousAnimation);
      animationManager.Remove(this);
    }
  }
}
