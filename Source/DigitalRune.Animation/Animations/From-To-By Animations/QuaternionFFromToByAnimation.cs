// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Animation.Traits;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates a <see langword="QuaternionF"/> value from/to/by a certain value.
  /// </summary>
  /// <inheritdoc/>
  public class QuaternionFFromToByAnimation : FromToByAnimation<QuaternionF>
  {
    /// <inheritdoc/>
    public override IAnimationValueTraits<QuaternionF> Traits
    {
      get { return QuaternionFTraits.Instance; }
    }
  }
}
