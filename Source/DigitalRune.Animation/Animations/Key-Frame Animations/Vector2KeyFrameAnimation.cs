// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !UNITY
using DigitalRune.Animation.Traits;
using Microsoft.Xna.Framework;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates a <see cref="Vector2"/> value using key frames.
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <inheritdoc/>
  public class Vector2KeyFrameAnimation : KeyFrameAnimation<Vector2>
  {
    /// <inheritdoc/>
    public override IAnimationValueTraits<Vector2> Traits
    {
      get { return Vector2Traits.Instance; }
    }
  }
}
#endif
