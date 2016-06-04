// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides bindings for the XNA stock effects.
  /// </summary>
  /// <remarks>
  /// This effect binder provides bindings for the XNA stock effects: <see cref="AlphaTestEffect"/>, 
  /// <see cref="BasicEffect"/>, <see cref="DualTextureEffect"/>, 
  /// <see cref="EnvironmentMapEffect"/>, and <see cref="SkinnedEffect"/>.
  /// </remarks>
  public class StockEffectBinder : IEffectBinder
  {
    /// <inheritdoc/>
    public EffectTechniqueBinding GetBinding(Effect effect)
    {
      if (effect is AlphaTestEffect)
        return new AlphaTestEffectTechniqueBinding();

      if (effect is BasicEffect)
        return new BasicEffectTechniqueBinding();

      if (effect is DualTextureEffect)
        return new DualTextureEffectTechniqueBinding();

      if (effect is EnvironmentMapEffect)
        return new EnvironmentMapEffectTechniqueBinding();

      if (effect is SkinnedEffect)
        return new SkinnedEffectTechniqueBinding();

      return null;
    }


    /// <inheritdoc/>
    public EffectParameterBinding GetBinding(Effect effect, EffectParameter parameter, IDictionary<string, object> opaqueData)
    {
      if (effect is AlphaTestEffect
          || effect is BasicEffect
          || effect is DualTextureEffect
          || effect is EnvironmentMapEffect
          || effect is SkinnedEffect)
      {
        // The ShaderIndex parameter is set by effect technique bindings.
        var description = effect.GetParameterDescriptions()[parameter];
        if (description.Semantic == "XnaShaderIndex")
          return new NullParameterBinding<int>(effect, parameter);

        if (description.Semantic == "XnaFogVector")
          return new FogVectorParameterBinding(effect, parameter);
      }

      return null;
    }
  }
}
