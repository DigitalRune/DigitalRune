namespace Samples.Graphics
{
  // Defines the semantics for the new parameters used by the effect Cloud.fx.
  public static class SkyEffectParameterSemantics
  {
    // The direction to the sun in world space (Vector3).
    public const string SunDirection = "SunDirection";

    // The color of the sun light as RGB (Vector3).
    public const string SunLight = "SunLight";

    // The color of the sky light (ambient light from the sky) as RGB (Vector3).
    public const string SkyLight = "SkyLight";
  }
}
