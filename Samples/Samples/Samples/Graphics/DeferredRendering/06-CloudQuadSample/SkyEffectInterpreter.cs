using DigitalRune.Graphics.Effects;


namespace Samples.Graphics
{
  // Provides descriptions for the new parameters in the effect Cloud.fx.
  // All 3 new parameters are "Global" which means all effect instances use the
  // same values and the values do not depend on the object being rendered.
  public class SkyEffectInterpreter : DictionaryEffectInterpreter
  {
    public SkyEffectInterpreter()
    {
      ParameterDescriptions.Add(SkyEffectParameterSemantics.SunDirection, (p, i) => new EffectParameterDescription(p, SkyEffectParameterSemantics.SunDirection, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SkyEffectParameterSemantics.SunLight,     (p, i) => new EffectParameterDescription(p, SkyEffectParameterSemantics.SunLight, i, EffectParameterHint.Global));
      ParameterDescriptions.Add(SkyEffectParameterSemantics.SkyLight,     (p, i) => new EffectParameterDescription(p, SkyEffectParameterSemantics.SkyLight, i, EffectParameterHint.Global));
    }
  }
}
