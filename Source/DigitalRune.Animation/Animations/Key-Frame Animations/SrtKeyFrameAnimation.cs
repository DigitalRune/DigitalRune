// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Animation.Character;
using DigitalRune.Animation.Traits;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates an <see cref="SrtTransform"/> using key frames.
  /// </summary>
  /// <inheritdoc/>
  public class SrtKeyFrameAnimation : KeyFrameAnimation<SrtTransform>
  {
    /// <inheritdoc/>
    public override IAnimationValueTraits<SrtTransform> Traits
    {
      get { return SrtTransformTraits.Instance; }
    }
  }
}
