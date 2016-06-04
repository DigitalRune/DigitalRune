// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if XNA && (WINDOWS || XBOX)
using DigitalRune.Animation.Traits;
using Microsoft.Xna.Framework.GamerServices;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Animates a <see cref="AvatarExpression"/> value using key frames.
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// This is a key frame animation for <see cref="AvatarExpression"/>s. See
  /// <see cref="KeyFrameAnimation{T}"/> for more information.
  /// </para>
  /// <para>
  /// <see cref="KeyFrameAnimation{T}.EnableInterpolation"/> is <see langword="false"/> per default.
  /// </para>
  /// </remarks>
  public class AvatarExpressionKeyFrameAnimation : KeyFrameAnimation<AvatarExpression>
  {
    /// <inheritdoc/>
    public override IAnimationValueTraits<AvatarExpression> Traits
    {
      get { return AvatarExpressionTraits.Instance; }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AvatarExpressionKeyFrameAnimation"/> class.
    /// </summary>
    public AvatarExpressionKeyFrameAnimation()
    {
      EnableInterpolation = false;
    }
  }
}
#endif
