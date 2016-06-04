// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation.Transitions
{
  /// <summary>
  /// Gradually replaces an existing animation with the new animation.
  /// </summary>
  internal sealed class FadeInAndReplaceTransition : AnimationTransition
  {
    private readonly AnimationInstance _previousAnimation;
    private readonly int _previousAnimationRunCount;
    private readonly TimeSpan _fadeInDuration;
    private AnimationController _fadeInController;


    /// <summary>
    /// Initializes a new instance of the <see cref="FadeInAndReplaceTransition"/> class.
    /// </summary>
    /// <param name="previousAnimation">The animation that should be replaced.</param>
    /// <param name="fadeInDuration">The duration over which the new animation fades in.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="previousAnimation"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fadeInDuration"/> is 0 or negative.
    /// </exception>
    public FadeInAndReplaceTransition(AnimationInstance previousAnimation, TimeSpan fadeInDuration)
    {
      if (previousAnimation == null)
        throw new ArgumentNullException("previousAnimation");
      if (fadeInDuration <= TimeSpan.Zero)
        throw new ArgumentOutOfRangeException("fadeInDuration", "The fade-in duration must be greater than 0.");

      _previousAnimation = previousAnimation;
      _previousAnimationRunCount = previousAnimation.RunCount;
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
      animationManager.Add(AnimationInstance, HandoffBehavior.Compose, _previousAnimation);
    }


    /// <inheritdoc/>
    protected override void OnUpdate(AnimationManager animationManager)
    {
      if (_fadeInController.State == AnimationState.Stopped)
      {
        // Fade-in has completed.
        if (_previousAnimation.RunCount == _previousAnimationRunCount) // Do nothing, if previous animation has been recycled.
          animationManager.Remove(_previousAnimation);

        animationManager.Remove(this);
      }
    }
  }
}
