#if !WP7 && !WP8
using DigitalRune.Graphics;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework;


namespace Samples.Graphics
{
  // Provides effect parameter bindings for the new parameters in the effect "Cloud.fx".
  // All 3 effect parameter are set automatically using delegates. 
  // The required values are already computed by the DynamicSkyObject. 
  public class SkyEffectBinder : DictionaryEffectBinder
  {
    public DynamicSkyObject DynamicSkyObject { get; set; }


    public SkyEffectBinder()
    {
      Vector3Bindings.Add(SkyEffectParameterSemantics.SunDirection, (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetSunDirection));
      Vector3Bindings.Add(SkyEffectParameterSemantics.SunLight,     (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetSunLight));
      Vector3Bindings.Add(SkyEffectParameterSemantics.SkyLight,     (e, p, o) => CreateDelegateParameterBinding<Vector3>(e, p, GetSkyLight));
    }


    private Vector3 GetSunDirection(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      return (Vector3)DynamicSkyObject.SunDirection;
    }


    private Vector3 GetSunLight(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      return (Vector3)DynamicSkyObject.SunLight;
    }


    private Vector3 GetSkyLight(DelegateParameterBinding<Vector3> binding, RenderContext context)
    {
      return (Vector3)DynamicSkyObject.AmbientLight;
    }
  }
}
#endif