// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation.Transitions
{
  /// <summary>
  /// Immediately replaces all existing animations with the new animation.
  /// </summary>
  internal sealed class ReplaceAllTransition : AnimationTransition
  {
    /// <inheritdoc/>
    protected override void OnInitialize(AnimationManager animationManager)
    {
      animationManager.Add(AnimationInstance, HandoffBehavior.Replace, null);
      animationManager.Remove(this);
    }
  }
}
