// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides the descriptions for XNA stock effects.
  /// </summary>
  /// <remarks>
  /// This effect interpreter provides descriptions for the XNA stock effects: 
  /// <see cref="AlphaTestEffect"/>, <see cref="BasicEffect"/>, <see cref="DualTextureEffect"/>, 
  /// <see cref="EnvironmentMapEffect"/>, and <see cref="SkinnedEffect"/>.
  /// </remarks>
  public class StockEffectInterpreter : DictionaryEffectInterpreter
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="StockEffectInterpreter"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public StockEffectInterpreter()
    {
      ParameterDescriptions.Add("Texture",  (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.DiffuseTexture, 0, EffectParameterHint.Material));
      // Texture2 is explicitly handled below.
      //ParameterDescriptions.Add("Texture2", (p, i) => new EffectParameterDescription(p, "Texture2", 0, EffectParameterHint.Material));
      
      ParameterDescriptions.Add("DiffuseColor",  (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.DiffuseColor, 0, EffectParameterHint.Material));
      ParameterDescriptions.Add("EmissiveColor", (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.EmissiveColor, 0, EffectParameterHint.Material));
      ParameterDescriptions.Add("SpecularColor", (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.SpecularColor, 0, EffectParameterHint.Material));
      ParameterDescriptions.Add("SpecularPower", (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.SpecularPower, 0, EffectParameterHint.Material));

      ParameterDescriptions.Add("FresnelFactor",          (p, i) => new EffectParameterDescription(p, DefaultEffectParameterSemantics.FresnelPower, 0, EffectParameterHint.Material));
      ParameterDescriptions.Add("EnvironmentMap",         (p, i) => new EffectParameterDescription(p, "XnaEnvironmentMap", i, EffectParameterHint.Material));
      ParameterDescriptions.Add("EnvironmentMapAmount",   (p, i) => new EffectParameterDescription(p, "XnaEnvironmentMapAmount", 0, EffectParameterHint.Material));
      ParameterDescriptions.Add("EnvironmentMapSpecular", (p, i) => new EffectParameterDescription(p, "XnaEnvironmentMapSpecular", 0, EffectParameterHint.Material));

      ParameterDescriptions.Add("DirLight0Direction",     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightDirection, 0, EffectParameterHint.Global));
      ParameterDescriptions.Add("DirLight0DiffuseColor",  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightDiffuse, 0, EffectParameterHint.Global));
      ParameterDescriptions.Add("DirLight0SpecularColor", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightSpecular, 0, EffectParameterHint.Global));

      ParameterDescriptions.Add("DirLight1Direction",     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightDirection, 1, EffectParameterHint.Global));
      ParameterDescriptions.Add("DirLight1DiffuseColor",  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightDiffuse, 1, EffectParameterHint.Global));
      ParameterDescriptions.Add("DirLight1SpecularColor", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightSpecular, 1, EffectParameterHint.Global));

      ParameterDescriptions.Add("DirLight2Direction",     (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightDirection, 2, EffectParameterHint.Global));
      ParameterDescriptions.Add("DirLight2DiffuseColor",  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightDiffuse, 2, EffectParameterHint.Global));
      ParameterDescriptions.Add("DirLight2SpecularColor", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.DirectionalLightSpecular, 2, EffectParameterHint.Global));

      ParameterDescriptions.Add("EyePosition", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.CameraPosition, 0, EffectParameterHint.Global));

      ParameterDescriptions.Add("AlphaTest", (p, i) => new EffectParameterDescription(p, "XnaAlphaTest", 0, EffectParameterHint.Material));

      ParameterDescriptions.Add("FogColor",  (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.FogColor, 0, EffectParameterHint.Global));
      ParameterDescriptions.Add("FogVector", (p, i) => new EffectParameterDescription(p, "XnaFogVector", 0, EffectParameterHint.Local));   // Local not global because XNA adds the world matrix to this parameter...

      ParameterDescriptions.Add("WorldViewProj", (p, i) => new EffectParameterDescription(p, SceneEffectParameterSemantics.WorldViewProjection, 0, EffectParameterHint.PerInstance));

      // The ShaderIndex is a local parameter: It depends on material settings and local lights.
      ParameterDescriptions.Add("ShaderIndex", (p, i) => new EffectParameterDescription(p, "XnaShaderIndex", 0, EffectParameterHint.PerInstance));
    }


    /// <inheritdoc/>
    public override EffectTechniqueDescription GetDescription(Effect effect, EffectTechnique technique)
    {
      if (effect is AlphaTestEffect
          || effect is BasicEffect
          || effect is DualTextureEffect
          || effect is EnvironmentMapEffect
          || effect is SkinnedEffect)
      {
        return new EffectTechniqueDescription(effect, technique);
      }

      return null;
    }


    /// <inheritdoc/>
    public override EffectParameterDescription GetDescription(Effect effect, EffectParameter parameter)
    {
      if (parameter == null)
        throw new ArgumentNullException("parameter");

      if (effect is AlphaTestEffect 
          || effect is BasicEffect 
          || effect is DualTextureEffect 
          || effect is EnvironmentMapEffect 
          || effect is SkinnedEffect)
      {
        // Handle Texture2 here:
        if (parameter.Name == "Texture2")
        {
          // Texture2 is different because the base class thinks this is "Texture" with index
          // 2. But it should use index 1. 
          return new EffectParameterDescription(parameter, DefaultEffectParameterSemantics.DiffuseTexture, 1, EffectParameterHint.Material);
        }

        return base.GetDescription(effect, parameter);
      }

      return null;
    }
  }
}
