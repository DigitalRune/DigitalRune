// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Specifies the effect type.
  /// </summary>
  public enum DREffectType
  {
    // IMPORTANT: This enumeration must be kept in sync with the corresponding 
    // enumeration in the EffectBindingReader.

    /// <summary>
    /// The predefined effect that supports alpha testing.
    /// </summary>
    AlphaTestEffect,

    /// <summary>
    /// The predefined basic rendering effect.
    /// </summary>
    BasicEffect,

    /// <summary>
    /// The predefined effect that supports two-layer multitexturing.
    /// </summary>
    DualTextureEffect,

    /// <summary>
    /// The predefined effect that supports environment mapping.
    /// </summary>
    EnvironmentMapEffect,

    /// <summary>
    /// The predefined effect for rendering skinned models.
    /// </summary>
    SkinnedEffect,

    /// <summary>
    /// A custom effect.
    /// </summary>
    CustomEffect,
  }
}
