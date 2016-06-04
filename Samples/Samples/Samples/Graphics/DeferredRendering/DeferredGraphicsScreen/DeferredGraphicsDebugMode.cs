#if !WP7 && !WP8
namespace Samples
{
  /// <summary>
  /// Debug modes for the <see cref="DeferredGraphicsScreen"/>.
  /// </summary>
  public enum DeferredGraphicsDebugMode
  {
    /// <summary>
    /// Normal rendering.
    /// </summary>
    None,

    /// <summary>
    /// Render the diffuse light buffer instead of the shaded materials.
    /// </summary>
    VisualizeDiffuseLightBuffer,

    /// <summary>
    /// Render the specular light buffer instead of the shaded materials.
    /// </summary>
    VisualizeSpecularLightBuffer,
  };
}
#endif
