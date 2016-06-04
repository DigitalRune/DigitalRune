// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation.Transitions
{
  /// <summary>
  /// Gradually replaces all existing animations with the new animation.
  /// </summary>
  internal sealed class FadeInAndReplaceAllTransition : AnimationTransition
  {
    private readonly TimeSpan _fadeInDuration;
    private AnimationController _fadeInController;


    /// <summary>
    /// Initializes a new instance of the <see cref="FadeInAndReplaceAllTransition"/> class.
    /// </summary>
    /// <param name="fadeInDuration">The duration over which the new animation fades in.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fadeInDuration"/> is 0 or negative.
    /// </exception>
    public FadeInAndReplaceAllTransition(TimeSpan fadeInDuration)
    {
      if (fadeInDuration <= TimeSpan.Zero)
        throw new ArgumentOutOfRangeException("fadeInDuration", "The fade-in duration must be greater than 0.");

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
      _fadeInController = animationManager.StartAnimation(fadeInAnimation, AnimationInstance.WeightProperty);

      // Add animation.
      animationManager.Add(AnimationInstance, HandoffBehavior.Compose, null);
    }


    /// <inheritdoc/>
    protected override void OnUpdate(AnimationManager animationManager)
    {
      if (_fadeInController.State == AnimationState.Stopped)
      {
        // Fade-in has completed.
        animationManager.RemoveBefore(AnimationInstance);
        animationManager.Remove(this);
      }
    }
  }
}
