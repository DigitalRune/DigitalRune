#if !MONOGAME && !WP7 && !WP8
// TODO: Add annotation support to MonoGame.

using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // An IEffectInterpreter which supports the RepeatParameter pass annotation. If a 
  // pass uses the annotation "string RepeatParameter = ...; ", then a 
  // RepeatTechniqueDescription is created for the technique. 
  public class RepeatTechniqueInterpreter : IEffectInterpreter
  {
    public EffectTechniqueDescription GetDescription(Effect effect, EffectTechnique technique)
    {
      foreach (var pass in technique.Passes)
        foreach (var annotation in pass.Annotations)
          if (annotation.Name == "RepeatParameter")
            return new RepeatTechniqueDescription(effect, technique);

      return null;
    }

    public EffectParameterDescription GetDescription(Effect effect, EffectParameter parameter)
    {
      return null;
    }
  }
}
#endif