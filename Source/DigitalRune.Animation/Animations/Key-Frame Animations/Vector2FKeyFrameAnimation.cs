// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Animation.Traits;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates a <see cref="Vector2F"/> value using key frames.
  /// </summary>
  /// <inheritdoc/>
  public class Vector2FKeyFrameAnimation : KeyFrameAnimation<Vector2F>
  {
    /// <inheritdoc/>
    public override IAnimationValueTraits<Vector2F> Traits
    {
      get { return Vector2FTraits.Instance; }
    }
  }
}
