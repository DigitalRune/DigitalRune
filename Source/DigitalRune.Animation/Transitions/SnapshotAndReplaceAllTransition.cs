// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Animation.Transitions
{
  /// <summary>
  /// Takes a snapshot of the current animation values and then replaces all existing animations 
  /// with the new animation. The new animation will be initialized with the snapshot value.
  /// </summary>
  internal sealed class SnapshotAndReplaceAllTransition : AnimationTransition
  {
    /// <inheritdoc/>
    protected override void OnInitialize(AnimationManager animationManager)
    {
      animationManager.Add(AnimationInstance, HandoffBehavior.SnapshotAndReplace, null);
      animationManager.Remove(this);
    }
  }
}
