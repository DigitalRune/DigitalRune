#if !MONOGAME && !WP7 && !WP8
// TODO: Add annotation support to MonoGame.

using System.Collections.Generic;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // Create a RepeatTechniqueBinding for all effects which use the 
  // RepeatParameter annotation.
  public class RepeatTechniqueBinder : IEffectBinder
  {
    public EffectTechniqueBinding GetBinding(Effect effect)
    {
      // For simplicity, we assume that the effect has exactly one technique.
      var repeatTechniqueDescription = effect.GetTechniqueDescriptions()[0] as RepeatTechniqueDescription;
      if (repeatTechniqueDescription != null)
        return new RepeatTechniqueBinding(repeatTechniqueDescription);

      return null;
    }

    public EffectParameterBinding GetBinding(Effect effect, EffectParameter parameter, IDictionary<string, object> opaqueData)
    {
      return null;
    }
  }
}
#endif
