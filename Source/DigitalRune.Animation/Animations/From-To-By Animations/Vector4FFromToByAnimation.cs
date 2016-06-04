// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Animation.Traits;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates a <see langword="Vector4F"/> value from/to/by a certain value.
  /// </summary>
  /// <inheritdoc/>
  public class Vector4FFromToByAnimation : FromToByAnimation<Vector4F>
  {
    /// <inheritdoc/>
    public override IAnimationValueTraits<Vector4F> Traits
    {
      get { return Vector4FTraits.Instance; }
    }
  }
}
