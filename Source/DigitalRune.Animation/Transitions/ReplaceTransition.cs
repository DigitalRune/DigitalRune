// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Animation.Transitions
{
  /// <summary>
  /// Replaces an existing animation with the new animation.
  /// </summary>
  internal sealed class ReplaceTransition : AnimationTransition
  {
    // Note: In theory it is possible that the user creates a ReplaceTransition(previousAnimation),
    // stops/recycles the previousAnimation, and then starts the transition. In this case 
    // the results are undefined. (Should not happen in practice.)

    private readonly AnimationInstance _previousAnimation;


    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceTransition"/> class.
    /// </summary>
    /// <param name="previousAnimation">The animation that should be replaced.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="previousAnimation"/> is <see langword="null"/>.
    /// </exception>
    public ReplaceTransition(AnimationInstance previousAnimation)
    {
      if (previousAnimation == null)
        throw new ArgumentNullException("previousAnimation");

      _previousAnimation = previousAnimation;
    }


    /// <inheritdoc/>
    protected override void OnInitialize(AnimationManager animationManager)
    {
      animationManager.Add(AnimationInstance, HandoffBehavior.Compose, _previousAnimation);
      animationManager.Remove(_previousAnimation);
      animationManager.Remove(this);
    }
  }
}
