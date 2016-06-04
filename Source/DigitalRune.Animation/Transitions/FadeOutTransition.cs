// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation.Transitions
{
  /// <summary>
  /// Gradually removes the existing animation.
  /// </summary>
  internal sealed class FadeOutTransition : AnimationTransition
  {
    private readonly TimeSpan _fadeOutDuration;
    private AnimationController _fadeOutController;


    /// <summary>
    /// Initializes a new instance of the <see cref="FadeOutTransition"/> class.
    /// </summary>
    /// <param name="fadeOutDuration">
    /// The duration over which the existing animation fades out.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="fadeOutDuration"/> is 0 or negative.
    /// </exception>
    public FadeOutTransition(TimeSpan fadeOutDuration)
    {
      if (fadeOutDuration <= TimeSpan.Zero)
        throw new ArgumentOutOfRangeException("fadeOutDuration", "The fade-out duration must be greater than 0.");

      _fadeOutDuration = fadeOutDuration;
    }


    /// <inheritdoc/>
    protected override void OnInitialize(AnimationManager animationManager)
    {
      // Start fade-out animation on animation weight.
      var fadeOutAnimation = new SingleFromToByAnimation
      {
        To = 0,
        Duration = _fadeOutDuration,
        EasingFunction = DefaultEase,
        FillBehavior = FillBehavior.Stop,
      };
      _fadeOutController = animationManager.StartAnimation(fadeOutAnimation, AnimationInstance.WeightProperty);
    }


    /// <inheritdoc/>
    protected override void OnUpdate(AnimationManager animationManager)
    {
      if (_fadeOutController.State == AnimationState.Stopped)
      {
        // Fade-out has completed.
        animationManager.Remove(AnimationInstance);
        animationManager.Remove(this);  // (Optional: When the animation instance is removed the 
                                        // transition will be removed automatically.)
      }
    }
  }
}
