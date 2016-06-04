// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides the descriptions of the default effect parameters.
  /// </summary>
  /// <remarks>
  /// See <see cref="DefaultEffectParameterSemantics"/> for a list of supported semantics.
  /// </remarks>
  public class DefaultEffectInterpreter : DictionaryEffectInterpreter
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultEffectInterpreter"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public DefaultEffectInterpreter()
    {
      // Animation
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.MorphWeight, (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.MorphWeight, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.Bones, (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.Bones, i, EffectParameterHint.PerInstance));

      // Material
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.DiffuseColor,    (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.DiffuseColor, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.DiffuseTexture,  (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.DiffuseTexture, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.SpecularColor,   (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.SpecularColor, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.SpecularTexture, (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.SpecularTexture, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.SpecularPower,   (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.SpecularPower, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.EmissiveColor,   (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.EmissiveColor, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.EmissiveTexture, (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.EmissiveTexture, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.Opacity,         (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.Opacity, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.Alpha,           (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.Alpha, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.BlendMode,       (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.BlendMode, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.ReferenceAlpha,  (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.ReferenceAlpha, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.NormalTexture,   (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.NormalTexture, i, EffectParameterHint.Material));
      //ParameterDescriptions.Add(DefaultEffectParameterSemantics.Height,        (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.Height, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.FresnelPower,    (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.FresnelPower, i, EffectParameterHint.Material));
      //ParameterDescriptions.Add(DefaultEffectParameterSemantics.Refraction,    (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.Refraction, i, EffectParameterHint.Material));
      //ParameterDescriptions.Add(DefaultEffectParameterSemantics.TextureMatrix, (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.TextureMatrix, i, EffectParameterHint.Material));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.InstanceColor,   (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.InstanceColor, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.InstanceAlpha,   (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.InstanceAlpha, i, EffectParameterHint.PerInstance));

      // Render Properties
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.PassIndex,        (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.PassIndex, i, EffectParameterHint.PerPass));
      //ParameterDescriptions.Add(DefaultEffectParameterSemantics.RenderTargetSize, (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.RenderTargetSize, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.SourceTexture,    (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.SourceTexture, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.ViewportSize,     (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.ViewportSize, i, EffectParameterHint.Global));

      // Simulation
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.Time,        (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.Time, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.LastTime,    (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.LastTime, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.ElapsedTime, (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.ElapsedTime, i, EffectParameterHint.Global));

      // Deferred Rendering
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.GBuffer,               (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.GBuffer, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.LightBuffer,           (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.LightBuffer, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.NormalsFittingTexture, (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.NormalsFittingTexture, i, EffectParameterHint.Global));

      // Misc
      //ParameterDescriptions.Add(DefaultEffectParameterSemantics.RandomValue, (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.RandomValue, i, EffectParameterHint.PerInstance));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.DitherMap,      (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.DitherMap, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.JitterMap,      (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.JitterMap, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.JitterMapSize,  (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.JitterMapSize, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.NoiseMap,       (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.NoiseMap, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.Debug,          (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.Debug, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(DefaultEffectParameterSemantics.NaN,            (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.NaN, i, EffectParameterHint.Global));
    }
  }
}
