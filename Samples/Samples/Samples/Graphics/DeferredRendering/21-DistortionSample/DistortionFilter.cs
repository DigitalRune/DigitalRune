#if !WP7 && !WP8
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // This post-processor distorts the image.
  //
  // Property DistortionTexture:
  // The distortion is controlled by a texture. The scene is not distorted where the alpha value of
  // the texture is 0. Red and Green contain the distortion offset. Very similar to a normal map.
  //
  // Property Scene:
  // This post-processor owns a Scene. External game objects can add scene nodes to this scene.
  // This scene will be rendered into the distortion texture. Currently, only BillboardNodes and
  // ParticleSystemNodes are supported. But this could be extended to support meshes and any other
  // graphics objects.
  public class DistortionFilter : PostProcessor
  {
    // Notes:
    // We could also use the blue channel of the distortion texture to store a blur scale and
    // perform a Poisson blur in the shader.

    private readonly BillboardRenderer _billboardRenderer;

    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _strengthParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _distortionTextureParameter;

    public Texture2D DistortionTexture { get; private set; }

    public Scene Scene { get; private set; }

    // The strength of the distortion.
    public float Strength { get; set; }


    public DistortionFilter(IGraphicsService graphicsService, ContentManager content)
      : base(graphicsService)
    {
      _billboardRenderer = new BillboardRenderer(GraphicsService, 512)
      {
        EnableSoftParticles = true,
      };

      _effect = content.Load<Effect>("PostProcessing/DistortionFilter");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _strengthParameter = _effect.Parameters["Strength"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _distortionTextureParameter = _effect.Parameters["DistortionTexture"];

      // The distortion texture can have a lower resolution to improve performance.
      DistortionTexture = new RenderTarget2D(
        GraphicsService.GraphicsDevice,
        320,
        180,
        false, SurfaceFormat.Color,
        DepthFormat.None);

      Scene = new Scene();

      Strength = 0.01f;
    }


    protected override void OnProcess(RenderContext context)
    {
      // Render Scene into DistortionTexture.
      bool doDistortion = RenderDistortionTexture(context);

      // Execute post-processing effect.
      var graphicsDevice = GraphicsService.GraphicsDevice;
      graphicsDevice.SetRenderTarget(context.RenderTarget);
      graphicsDevice.Viewport = context.Viewport;

      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _sourceTextureParameter.SetValue(context.SourceTexture);

      if (doDistortion)
      {
        _strengthParameter.SetValue(Strength);
        _distortionTextureParameter.SetValue(DistortionTexture);
      }
      else
      {
        _strengthParameter.SetValue(0.0f);
        _distortionTextureParameter.SetValue((Texture2D)null);
      }

      _effect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawFullScreenQuad();
    }


    private bool RenderDistortionTexture(RenderContext context)
    {
      Scene.Update(context.DeltaTime);
      var sceneQuery = Scene.Query<CustomSceneQuery>(context.CameraNode, context);

      if (sceneQuery.RenderableNodes.Count == 0)
        return false;

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;

      graphicsDevice.SetRenderTarget((RenderTarget2D)DistortionTexture);

      // A color of (0.5, 0.5, *) is equal to a distortion offset of 0.
      // Alpha is initialized to 0 (transparent).
      graphicsDevice.Clear(new Color(0.5f, 0.5f, 0, 0));

      // TODO: Here we could call a mesh renderer to render meshes.

      // Render billboards and particle systems.
      _billboardRenderer.Render(sceneQuery.RenderableNodes, context);

      return true;
    }
  }
}
#endif