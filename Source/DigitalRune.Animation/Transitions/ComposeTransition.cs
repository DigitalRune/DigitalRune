// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation.Transitions
{
  /// <summary>
  /// Combines the new animation with existing animations by adding the animation into composition 
  /// chains.
  /// </summary>
  internal sealed class ComposeTransition : AnimationTransition
  {
    // Note: In theory it is possible that the user creates a ComposeTransition(previousAnimation),
    // stops/recycles the previousAnimation, and then starts the transition. In this case 
    // the results are undefined. (Should not happen in practice.)

    private readonly AnimationInstance _previousAnimation;


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ComposeTransition"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ComposeTransition"/> class.
    /// </summary>
    public ComposeTransition()
      : this(null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ComposeTransition"/> class.
    /// </summary>
    /// <param name="previousAnimation">The animation that should be replaced.</param>
    public ComposeTransition(AnimationInstance previousAnimation)
    {
      _previousAnimation = previousAnimation;
    }


    /// <inheritdoc/>
    protected override void OnInitialize(AnimationManager animationManager)
    {
      animationManager.Add(AnimationInstance, HandoffBehavior.Compose, _previousAnimation);
      animationManager.Remove(this);
    }
  }
}
