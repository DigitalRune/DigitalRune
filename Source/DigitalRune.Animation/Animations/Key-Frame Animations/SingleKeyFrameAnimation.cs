// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Animation.Traits;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates a <see cref="Single"/> value using key frames.
  /// </summary>
  /// <inheritdoc/>
  public class SingleKeyFrameAnimation : KeyFrameAnimation<float>
  {
    /// <inheritdoc/>
    public override IAnimationValueTraits<Single> Traits
    {
      get { return SingleTraits.Instance; }
    }
  }
}
