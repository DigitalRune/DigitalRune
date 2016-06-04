#if !WP7 && !WP8
using System;
using System.Collections.Generic;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Samples.Graphics;
using DirectionalLight = DigitalRune.Graphics.DirectionalLight;


namespace Samples
{
  // Implements a deferred lighting render pipeline, supporting lights and shadows,
  // Screen Space Ambient Occlusion (SSAO), High Dynamic Range (HDR) lighting, sky
  // rendering, post-processing, ...
  // The intermediate render targets (G-buffer, light buffer, shadow masks) can be
  // visualized for debugging.
  // Beginners can use this graphics screen as it is. Advanced developers can adapt
  // the render pipeline to their needs.
  public class DeferredGraphicsScreen : GraphicsScreen, IDisposable
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    //private readonly SampleFramework _sampleFramework;

    protected readonly SpriteBatch SpriteBatch;
    private readonly SceneRenderer _opaqueMeshSceneRenderer;
    private readonly DecalRenderer _decalRenderer;
    private readonly BillboardRenderer _billboardRenderer;
#if !XBOX360
    private readonly TerrainClipmapRenderer _terrainClipmapRenderer;
#endif
    private readonly CloudMapRenderer _cloudMapRenderer;
    private readonly PlanarReflectionRenderer _planarReflectionRenderer;
    private readonly WaterWavesRenderer _waterWavesRenderer;
    private readonly GBufferRenderer _gBufferRenderer;
    private readonly LensFlareRenderer _lensFlareRenderer;
    private readonly SkyRenderer _skyRenderer;
    private readonly FogRenderer _fogRenderer;
    private readonly DebugRenderer _internalDebugRenderer;
    private readonly RebuildZBufferRenderer _rebuildZBufferRenderer;
    //private readonly Texture2D _reticle;
    //private readonly UnderwaterPostProcessor _underwaterPostProcessor;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    public bool IsDisposed { get; private set; }

    public SceneCaptureRenderer SceneCaptureRenderer { get; private set; }
#if !XBOX360
    public TerrainRenderer TerrainRenderer { get; private set; }
#endif
    public MeshRenderer MeshRenderer { get; private set; }
    public ShadowMapRenderer ShadowMapRenderer { get; private set; }
    public ShadowMaskRenderer ShadowMaskRenderer { get; private set; }
    public LightBufferRenderer LightBufferRenderer { get; private set; }
    public SceneRenderer AlphaBlendSceneRenderer { get; private set; }

    // The active camera used to render the scene. This property must be set by
    // the samples. The default value is null.
    public CameraNode ActiveCameraNode { get; set; }

    public Scene Scene { get; private set; }

    public PostProcessorChain PostProcessors { get; private set; }

    // A debug renderer which can be used by the samples and game objects.
    // (Note: DebugRenderer.Clear() is not called automatically.)
    public DebugRenderer DebugRenderer { get; private set; }

    public bool VisualizeIntermediateRenderTargets { get; set; }

    public DeferredGraphicsDebugMode DebugMode { get; set; }

    public bool EnableLod { get; set; }

    public bool EnableSoftParticles
    {
      get { return _billboardRenderer.EnableSoftParticles; }
      set { _billboardRenderer.EnableSoftParticles = value; }
    }

    public bool EnableOffscreenParticles
    {
      get { return _billboardRenderer.EnableOffscreenRendering; }
      set { _billboardRenderer.EnableOffscreenRendering = value; }
    }

    public bool DrawReticle { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public DeferredGraphicsScreen(IServiceLocator services)
      : base(services.GetInstance<IGraphicsService>())
    {
      //_sampleFramework = services.GetInstance<SampleFramework>();
      var contentManager = services.GetInstance<ContentManager>();

      SpriteBatch = GraphicsService.GetSpriteBatch();

      // Let's create the necessary scene node renderers:
#if !XBOX360
      TerrainRenderer = new TerrainRenderer(GraphicsService);
#endif
      MeshRenderer = new MeshRenderer();

      // The _opaqueMeshSceneRenderer combines all renderers for opaque
      // (= not alpha blended) meshes.
      _opaqueMeshSceneRenderer = new SceneRenderer();
#if !XBOX360
      _opaqueMeshSceneRenderer.Renderers.Add(TerrainRenderer);
#endif
      _opaqueMeshSceneRenderer.Renderers.Add(MeshRenderer);

      _decalRenderer = new DecalRenderer(GraphicsService);
      _billboardRenderer = new BillboardRenderer(GraphicsService, 2048)
      {
        EnableSoftParticles = true,

        // If you have an extreme amount of particles that cover the entire screen,
        // you can turn on offscreen rendering to improve performance.
        //EnableOffscreenRendering = true,
      };

      // The AlphaBlendSceneRenderer combines all renderers for transparent
      // (= alpha blended) objects.
      AlphaBlendSceneRenderer = new SceneRenderer();
      AlphaBlendSceneRenderer.Renderers.Add(MeshRenderer);
      AlphaBlendSceneRenderer.Renderers.Add(_billboardRenderer);
      AlphaBlendSceneRenderer.Renderers.Add(new WaterRenderer(GraphicsService));
      //AlphaBlendSceneRenderer.Renderers.Add(new FogSphereRenderer(GraphicsService));
      //AlphaBlendSceneRenderer.Renderers.Add(new VolumetricLightRenderer(GraphicsService));

#if !XBOX360
      // Update terrain clipmaps. (Only necessary if TerrainNodes are used.)
      _terrainClipmapRenderer = new TerrainClipmapRenderer(GraphicsService);
#endif

      // Renderer for cloud maps. (Only necessary if LayeredCloudMaps are used.)
      _cloudMapRenderer = new CloudMapRenderer(GraphicsService);

      // Renderer for SceneCaptureNodes. See also SceneCapture2DSample.
      // In the constructor we specify a method which is called in SceneCaptureRenderer.Render() 
      // when the scene must be rendered for the SceneCaptureNodes.
      SceneCaptureRenderer = new SceneCaptureRenderer(context =>
      {
        // Get scene nodes which are visible by the current camera.
        CustomSceneQuery sceneQuery = Scene.Query<CustomSceneQuery>(context.CameraNode, context);
        // Render scene (with post-processing, with lens flares, no debug rendering, no reticle).
        RenderScene(sceneQuery, context, true, true, false, false);
      });

      // Renderer for PlanarReflectionNodes. See also PlanarReflectionSample.
      // In the constructor we specify a method which is called in PlanarReflectionRenderer.Render() 
      // to create the reflection images.
      _planarReflectionRenderer = new PlanarReflectionRenderer(context =>
      {
        // Get scene nodes which are visible by the current camera.
        CustomSceneQuery sceneQuery = Scene.Query<CustomSceneQuery>(context.CameraNode, context);

        var planarReflectionNode = (PlanarReflectionNode)context.ReferenceNode;

        // Planar reflections are often for WaterNodes. These nodes should not be rendered 
        // into their own reflection map because when the water surface is displaced by waves, 
        // some waves could be visible in the reflection. 
        // --> Remove the water node from the renderable nodes. (In our samples, the water
        // node is the parent of the reflection node.)
        if (planarReflectionNode.Parent is WaterNode)
        {
          var index = sceneQuery.RenderableNodes.IndexOf(planarReflectionNode.Parent);
          if (index >= 0)
            sceneQuery.RenderableNodes[index] = null;
        }

        // Render scene (no post-processing, no lens flares, no debug rendering, no reticle).
        RenderScene(sceneQuery, context, false, false, false, false);
      });

      _waterWavesRenderer = new WaterWavesRenderer(GraphicsService);

      // The shadow map renderer renders a depth image from the viewpoint of the light and
      // stores it in LightNode.Shadow.ShadowMap.
      ShadowMapRenderer = new ShadowMapRenderer(context =>
      {
        var query = context.Scene.Query<ShadowCasterQuery>(context.CameraNode, context);
        if (query.ShadowCasters.Count == 0)
          return false;

        _opaqueMeshSceneRenderer.Render(query.ShadowCasters, context);
        return true;
      });

      // The shadow mask renderer evaluates the shadow maps, does shadow filtering 
      // and stores the resulting shadow factor in a screen space image 
      //(see LightNode.Shadow.ShadowMask/ShadowMaskChannel).
      ShadowMaskRenderer = new ShadowMaskRenderer(GraphicsService, 2);

      // Optionally, we can blur the shadow mask to make the shadows smoother.
      var blur = new Blur(GraphicsService)
      {
        IsAnisotropic = false,
        IsBilateral = true,
        EdgeSoftness = 0.05f,
        Scale = 1f,
        Enabled = false,  // Disable blur by default.
      };
      blur.InitializeGaussianBlur(11, 3, true);
      ShadowMaskRenderer.Filter = blur;

      // Renderers which create the intermediate render targets:
      // Those 2 renderers are implemented in this sample. Those functions could
      // be implemented directly in this class but we have created separate classes
      // to make the code more readable.
      _gBufferRenderer = new GBufferRenderer(GraphicsService, _opaqueMeshSceneRenderer, _decalRenderer);
      LightBufferRenderer = new LightBufferRenderer(GraphicsService);

      // Other specialized renderers:
      _lensFlareRenderer = new LensFlareRenderer(GraphicsService);
      _skyRenderer = new SkyRenderer(GraphicsService);
      _fogRenderer = new FogRenderer(GraphicsService);
      _internalDebugRenderer = new DebugRenderer(GraphicsService, null);
      _rebuildZBufferRenderer = new RebuildZBufferRenderer(GraphicsService);

      Scene = new Scene();

      // This screen needs a HDR filter to map high dynamic range values back to
      // low dynamic range (LDR).
      PostProcessors = new PostProcessorChain(GraphicsService);
      PostProcessors.Add(new HdrFilter(GraphicsService)
      {
        EnableBlueShift = true,
        BlueShiftCenter = 0.0004f,
        BlueShiftRange = 0.5f,
        //BlueShiftColor = new Vector3F(1.05f / 4f, 0.97f / 4f, 1.27f / 4f),  // Default physically-based blue-shift
        BlueShiftColor = new Vector3F(0.25f, 0.25f, 0.7f),  // More dramatic blue-shift
        MinExposure = 0,
        MaxExposure = 10,
        BloomIntensity = 1,
        BloomThreshold = 0.6f,
      });
      //_underwaterPostProcessor = new UnderwaterPostProcessor(GraphicsService, contentManager);
      //PostProcessors.Add(_underwaterPostProcessor);

      // Use 2D texture for reticle.
      //_reticle = contentManager.Load<Texture2D>("Reticle");

      // Use the sprite font of the GUI.
      //var uiContentManager = services.GetInstance<ContentManager>("UIContent");
      //var spriteFont = uiContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
      var spriteFont = contentManager.Load<SpriteFont>("DigitalRune.Editor.Game/Fonts/DejaVuSans");
      DebugRenderer = new DebugRenderer(GraphicsService, spriteFont)
      {
        DefaultColor = new Color(0, 0, 0),
        DefaultTextPosition = new Vector2F(10),
      };

      EnableLod = true;
    }


    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    protected virtual void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
#if !XBOX360
          TerrainRenderer.Dispose();
#endif
          MeshRenderer.Dispose();
          _decalRenderer.Dispose();
          AlphaBlendSceneRenderer.Dispose();
#if !XBOX360
          _terrainClipmapRenderer.Dispose();
#endif
          _cloudMapRenderer.Dispose();
          _waterWavesRenderer.Dispose();
          SceneCaptureRenderer.Dispose();
          _planarReflectionRenderer.Dispose();
          ShadowMapRenderer.Dispose();
          ShadowMaskRenderer.Dispose();
          LightBufferRenderer.Dispose();
          _lensFlareRenderer.Dispose();
          _skyRenderer.Dispose();
          _fogRenderer.Dispose();
          _internalDebugRenderer.Dispose();
          Scene.Dispose(false);
          PostProcessors.Dispose();
          DebugRenderer.Dispose();
        }

        // Release unmanaged resources.

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Updates the graphics screen. - This method is called by GraphicsManager.Update().
    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // The Scene class has an Update() method which must be call once per frame.
      Scene.Update(deltaTime);
    }


    // Renders the graphics screen. - This method is called in GraphicsManager.Render().
    protected override void OnRender(RenderContext context)
    {
      if (ActiveCameraNode == null)
        return;

      // Our scene and the camera must be set in the render context. This info is
      // required by many renderers.
      context.Scene = Scene;
      context.CameraNode = ActiveCameraNode;

      // LOD (level of detail) settings are also specified in the context.
      context.LodCameraNode = ActiveCameraNode;
      context.LodHysteresis = 0.5f;
      context.LodBias = EnableLod ? 1.0f : 0.0f;
      context.LodBlendingEnabled = false;

      // ----- Preprocessing
      // For some scene nodes we have to update some off-screen render targets before the
      // actual scene is rendered. 
      // 
      // We only have to do this for the scene nodes which are visible
      // by the camera frustum:
      PreprocessingSceneQuery preprocessingQuery = Scene.Query<PreprocessingSceneQuery>(context.CameraNode, context);

#if !XBOX360
      // TODO:
      _terrainClipmapRenderer.Render(preprocessingQuery.TerrainNodes, context);
#endif

      // Generate cloud maps.
      // Only necessary if LayeredCloudMaps are used. If the cloud maps are static 
      // and the settings do not change, it is not necessary to generate the 
      // cloud maps in every frame. But in the SkySample we use animated cloud maps.
      // The CloudMapRenderer can be called several times per frame, it will only 
      // do the work once per frame.
      // See also SkySample.
      _cloudMapRenderer.Render(preprocessingQuery.CloudLayerNodes, context);

      // Compute ocean waves.
      // Only necessary if WaterNodes with OceanWaves are used.
      _waterWavesRenderer.Render(preprocessingQuery.WaterNodes, context);

      // Perform render-to-texture operations.
      // Only necessary if SceneCaptureNodes are used. 
      // See also SceneCapture2DSample.
      SceneCaptureRenderer.Render(preprocessingQuery.SceneCaptureNodes, context);

      // Render reflections.
      // Only necessary if PlanarReflectionNodes are used. 
      // See also PlanarReflectionSample.
      _planarReflectionRenderer.Render(preprocessingQuery.PlanarReflectionNodes, context);

      // ----- Scene Rendering
      // Get all scene nodes which overlap the camera frustum.
      CustomSceneQuery sceneQuery = Scene.Query<CustomSceneQuery>(context.CameraNode, context);

      // Render the scene nodes of the sceneQuery.
      RenderScene(sceneQuery, context, true, true, true, DrawReticle);

      // ----- Clean-up
      context.Scene = null;
      context.CameraNode = null;
      context.LodCameraNode = null;
      context.LodHysteresis = 0;
    }


    protected internal void RenderScene(CustomSceneQuery sceneQuery, RenderContext context,
      bool doPostProcessing, bool renderLensFlares, bool renderDebugOutput, bool renderReticle)
    {
      var renderTargetPool = GraphicsService.RenderTargetPool;
      var graphicsDevice = GraphicsService.GraphicsDevice;
      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;
      var originalSourceTexture = context.SourceTexture;

      // All intermediate render targets have the size of the target viewport.
      int width = context.Viewport.Width;
      int height = context.Viewport.Height;
      context.Viewport = new Viewport(0, 0, width, height);

      // The render context can be used to share any data, for example:
      // Store a shared RebuildZBufferRenderer in the context. 
      context.Data[RenderContextKeys.RebuildZBufferRenderer] = _rebuildZBufferRenderer;

      // ----- G-Buffer Pass
      // The GBufferRenderer creates context.GBuffer0 and context.GBuffer1.
      _gBufferRenderer.Render(sceneQuery.RenderableNodes, sceneQuery.DecalNodes, context);

      // ----- Shadow Pass
      // The ShadowMapRenderer renders the shadow maps which are stored in the light nodes.
      context.RenderPass = "ShadowMap";
      ShadowMapRenderer.Render(sceneQuery.Lights, context);
      context.RenderPass = null;

      // The ShadowMaskRenderer renders the shadows and stores them in one or more render
      // targets ("shadows masks").
      ShadowMaskRenderer.Render(sceneQuery.Lights, context);

      RecycleShadowMaps(sceneQuery.Lights);

      // ----- Light Buffer Pass
      // The LightBufferRenderer creates context.LightBuffer0 (diffuse light) and
      // context.LightBuffer1 (specular light).
      LightBufferRenderer.Render(sceneQuery.Lights, context);

      // Normally, we do not need the shadow masks anymore - except if we want to 
      // display them for debugging.
      if (!VisualizeIntermediateRenderTargets)
        ShadowMaskRenderer.RecycleShadowMasks();

      // ----- Material Pass
      if (DebugMode == DeferredGraphicsDebugMode.None)
      {
        // In the material pass we render all meshes and decals into a single full-screen
        // render target. The shaders combine the material properties (diffuse texture, etc.)
        // with the light buffer info.
        context.RenderTarget =
          renderTargetPool.Obtain2D(new RenderTargetFormat(width, height, false, SurfaceFormat.HdrBlendable,
            DepthFormat.Depth24Stencil8));
        graphicsDevice.SetRenderTarget(context.RenderTarget);
        context.Viewport = graphicsDevice.Viewport;
        graphicsDevice.Clear(Color.Gray);
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        graphicsDevice.BlendState = BlendState.Opaque;
        context.RenderPass = "Material";
        _opaqueMeshSceneRenderer.Render(sceneQuery.RenderableNodes, context);
        _decalRenderer.Render(sceneQuery.DecalNodes, context);
        context.RenderPass = null;
      }
      else
      {
        // For debugging: 
        // Ignore the material pass. Keep rendering into one of the light buffers
        // to visualize only the lighting results.
        if (DebugMode == DeferredGraphicsDebugMode.VisualizeDiffuseLightBuffer)
          context.RenderTarget = context.LightBuffer0;
        else
          context.RenderTarget = context.LightBuffer1;
      }

      // The meshes rendered in the last step might use additional floating-point
      // textures (e.g. the light buffers) in the different graphics texture stages.
      // We reset the texture stages (setting all GraphicsDevice.Textures to null),
      // otherwise XNA might throw exceptions.
      graphicsDevice.ResetTextures();

      // ----- Occlusion Queries
      if (renderLensFlares)
        _lensFlareRenderer.UpdateOcclusion(sceneQuery.LensFlareNodes, context);

      // ----- Sky
      _skyRenderer.Render(sceneQuery.SkyNodes, context);

      // ----- Fog
      _fogRenderer.Render(sceneQuery.FogNodes, context);

      // ----- Forward Rendering of Alpha-Blended Meshes and Particles
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.AlphaBlend;
      context.RenderPass = "AlphaBlend";
      AlphaBlendSceneRenderer.Render(sceneQuery.RenderableNodes, context, RenderOrder.BackToFront);
      context.RenderPass = null;
      graphicsDevice.ResetTextures();

      renderTargetPool.Recycle(context.SourceTexture);
      context.SourceTexture = null;

      //_underwaterPostProcessor.Enabled = IsCameraUnderwater(sceneQuery, context.CameraNode);

      // ----- Post Processors
      context.SourceTexture = context.RenderTarget;
      context.RenderTarget = originalRenderTarget;
      context.Viewport = originalViewport;
      if (doPostProcessing)
      {
        // The post-processors modify the scene image and the result is written into
        // the final render target - which is usually the back  buffer (but this could
        // also be another off-screen render target used in another graphics screen).
        PostProcessors.Process(context);
      }
      else
      {
        // Only copy the current render target to the final render target without post-processing.
        graphicsDevice.SetRenderTarget(originalRenderTarget);
        graphicsDevice.Viewport = originalViewport;
        SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        SpriteBatch.Draw(context.SourceTexture, new Rectangle(0, 0, originalViewport.Width, originalViewport.Height), Color.White);
        SpriteBatch.End();
      }
      renderTargetPool.Recycle((RenderTarget2D)context.SourceTexture);
      context.SourceTexture = null;

      // ----- Lens Flares
      if (renderLensFlares)
        _lensFlareRenderer.Render(sceneQuery.LensFlareNodes, context);

      // ----- Debug Output
      if (renderDebugOutput)
      {
        // ----- Optional: Restore the Z-Buffer
        // Currently, the hardware depth buffer is not initialized with useful data because
        // every time we change the render target, XNA deletes the depth buffer. If we want
        // the debug rendering to use correct depth buffer, we can restore the depth buffer
        // using the RebuildZBufferRenderer. If we remove this step, then the DebugRenderer
        // graphics will overlay the whole 3D scene.
        _rebuildZBufferRenderer.Render(context, true);

        // Render debug info added by game objects.
        DebugRenderer.Render(context);

        // Render intermediate render targets for debugging.
        // We do not use the public DebugRenderer here because the public DebugRenderer
        // might not be cleared every frame (the game logic can choose how it wants to
        // use the public renderer).
        if (VisualizeIntermediateRenderTargets)
        {
          _internalDebugRenderer.DrawTexture(context.GBuffer0, new Rectangle(0, 0, 200, 200));
          _internalDebugRenderer.DrawTexture(context.GBuffer1, new Rectangle(200, 0, 200, 200));
          _internalDebugRenderer.DrawTexture(context.LightBuffer0, new Rectangle(400, 0, 200, 200));
          _internalDebugRenderer.DrawTexture(context.LightBuffer1, new Rectangle(600, 0, 200, 200));
          for (int i = 0; i < ShadowMaskRenderer.ShadowMasks.Count; i++)
          {
            var shadowMask = ShadowMaskRenderer.ShadowMasks[i];
            if (shadowMask != null)
              _internalDebugRenderer.DrawTexture(shadowMask, new Rectangle((i) * 200, 200, 200, 200));
          }

          _internalDebugRenderer.Render(context);
          _internalDebugRenderer.Clear();
        }
      }

      //// ----- Draw Reticle
      //if (renderReticle && _sampleFramework.IsGuiVisible)
      //{
      //  SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
      //  SpriteBatch.Draw(
      //    _reticle,
      //    new Vector2(originalViewport.Width / 2 - _reticle.Width / 2, originalViewport.Height / 2 - _reticle.Height / 2),
      //    Color.Black);
      //  SpriteBatch.End();
      //}

      // ----- Clean-up
      // It is very important to give every intermediate render target back to the
      // render target pool!
      renderTargetPool.Recycle(context.GBuffer0);
      context.GBuffer0 = null;
      renderTargetPool.Recycle(context.GBuffer1);
      context.GBuffer1 = null;
      renderTargetPool.Recycle((RenderTarget2D)context.Data[RenderContextKeys.DepthBufferHalf]);
      context.Data.Remove(RenderContextKeys.DepthBufferHalf);
      if (DebugMode != DeferredGraphicsDebugMode.VisualizeDiffuseLightBuffer)
        renderTargetPool.Recycle(context.LightBuffer0);
      context.LightBuffer0 = null;
      if (DebugMode != DeferredGraphicsDebugMode.VisualizeSpecularLightBuffer)
        renderTargetPool.Recycle(context.LightBuffer1);
      context.LightBuffer1 = null;
      ShadowMaskRenderer.RecycleShadowMasks();
      context.Data.Remove(RenderContextKeys.RebuildZBufferRenderer);
      context.SourceTexture = originalSourceTexture;
    }


    // Recycle all shadow maps except the shadow map of the directional light.
    // Most lights can move off screen and should not hold on to their shadow maps.
    // The directional light shadow map is not recycled because it is visible in 
    // every frame. Further, it could be used by alpha-blended objects (though not in 
    // this project). We also have to keep the shadow map for the next frame if shadow
    // caching is used (see CascadedShadowMapSample).
    private void RecycleShadowMaps(List<SceneNode> lights)
    {
      var renderTargetPool = GraphicsService.RenderTargetPool;
      foreach (var node in lights)
      {
        var lightNode = (LightNode)node;
        if (lightNode.Shadow != null && !(lightNode.Light is DirectionalLight))
        {
          renderTargetPool.Recycle(lightNode.Shadow.ShadowMap);
          lightNode.Shadow.ShadowMap = null;
        }
      }
    }


    private static bool IsCameraUnderwater(CustomSceneQuery query, CameraNode cameraNode)
    {
      var cameraPosition = cameraNode.PoseWorld.Position;
      foreach (var node in query.RenderableNodes)
      {
        var waterNode = node as WaterNode;
        if (waterNode != null && waterNode.IsUnderwater(cameraPosition))
          return true;
      }

      return false;
    }
    #endregion
  }
}
#endif
