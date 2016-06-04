using System;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  /// <summary>
  /// A graphics screen which demonstrates <i>Weighted Blended Order-Independent Transparency</i>.
  /// </summary>
  public sealed class WeightedBlendedOITScreen : GraphicsScreen, IDisposable
  {
    private readonly MeshRenderer _meshRenderer;

    #region ----- WBOIT -----
    private readonly RenderTargetBinding[] _renderTargetBindings = new RenderTargetBinding[2];

    // Blend state used for rendering transparent object into the WBOIT render targets.
    private BlendState _wboitRenderBlendState = new BlendState
    {
      ColorSourceBlend = Blend.One,
      ColorDestinationBlend = Blend.One,
      AlphaSourceBlend = Blend.Zero,
      AlphaDestinationBlend = Blend.InverseSourceAlpha
    };

    // Blend state used for compositing the WBOIT render targets with the scene.
    private BlendState _wboitCombineBlendState = new BlendState
    {
      ColorSourceBlend = Blend.InverseSourceAlpha,
      ColorDestinationBlend = Blend.SourceAlpha,
      AlphaSourceBlend = Blend.Zero,
      AlphaDestinationBlend = Blend.One
    };

    // Effect used for compositing the WBOIT render targets with the scene.
    private readonly Effect _wboitEffect;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterTextureA;
    private readonly EffectParameter _parameterTextureB;
    #endregion


    /// <summary>
    /// Gets or sets a value indicating whether weighted blended OIT is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true" /> to render the scene with weighed blended OIT; otherwise,
    /// <see langword="false" /> to render the scene with regular alpha blending. The default value
    /// is <see langword="true"/>.
    /// </value>
    public bool EnableWboit { get; set; }


    /// <summary>
    /// Gets the scene.
    /// </summary>
    /// <value>The scene.</value>
    public Scene Scene { get; private set; }


    /// <summary>
    /// Gets or sets the active camera.
    /// </summary>
    /// <value>The active camera.</value>
    public CameraNode ActiveCameraNode { get; set; }


    public WeightedBlendedOITScreen(IServiceLocator services)
      : base(services.GetInstance<IGraphicsService>())
    {
      Coverage = GraphicsScreenCoverage.Full;
      Scene = new Scene();
      EnableWboit = true;

      _meshRenderer = new MeshRenderer();

      var content = services.GetInstance<ContentManager>();
      _wboitEffect = content.Load<Effect>("WeightedBlendedOIT/WeightedBlendedOIT");
      _parameterViewportSize = _wboitEffect.Parameters["ViewportSize"];
      _parameterTextureA = _wboitEffect.Parameters["TextureA"];
      _parameterTextureB = _wboitEffect.Parameters["TextureB"];
    }


    public void Dispose()
    {
      Scene.Dispose(false);
      _meshRenderer.Dispose();
      _wboitRenderBlendState.Dispose();
      _wboitCombineBlendState.Dispose();
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      Scene.Update(deltaTime);
    }


    protected override void OnRender(RenderContext context)
    {
      // Abort if no active camera is set.
      if (ActiveCameraNode == null)
        return;

      // Set the current camera and current scene in the render context. This 
      // information is very important; it is used by the scene node renderers.
      context.CameraNode = ActiveCameraNode;
      context.Scene = Scene;

      // Frustum Culling: Get all the scene nodes that intersect the camera frustum.
      var query = Scene.Query<WeightedBlendedOITQuery>(context.CameraNode, context);

      if (EnableWboit)
        RenderWeightedBlendedOIT(query, context);
      else
        RenderAlphaBlended(query, context);

      context.CameraNode = null;
      context.Scene = null;
    }


    private void RenderAlphaBlended(WeightedBlendedOITQuery query, RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;

      graphicsDevice.Clear(Color.CornflowerBlue);

      // Render opaque mesh nodes.
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      context.RenderPass = "Default";
      _meshRenderer.Render(query.SceneNodes, context, RenderOrder.Default);
      context.RenderPass = null;

      // Render transparent mesh nodes with alpha blending.
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.AlphaBlend;
      context.RenderPass = "AlphaBlend";
      _meshRenderer.Render(query.TransparentNodes, context, RenderOrder.Default);
      context.RenderPass = null;
    }


    private void RenderWeightedBlendedOIT(WeightedBlendedOITQuery query, RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;

      var target = context.RenderTarget;
      var viewport = context.Viewport;

      // ----- Transparent pass
      // Set up WBOIT render targets.
      var renderTargetPool = GraphicsService.RenderTargetPool;
      var renderTargetA = renderTargetPool.Obtain2D(
        new RenderTargetFormat(
          context.Viewport.Width,
          context.Viewport.Height,
          false,
          SurfaceFormat.HdrBlendable,
          DepthFormat.Depth24Stencil8));
      var renderTargetB = renderTargetPool.Obtain2D(
        new RenderTargetFormat(
          context.Viewport.Width,
          context.Viewport.Height,
          false,
#if MONOGAME
          SurfaceFormat.HalfSingle,
#else
          SurfaceFormat.HdrBlendable,
#endif
          DepthFormat.None));

      _renderTargetBindings[0] = new RenderTargetBinding(renderTargetA);
      _renderTargetBindings[1] = new RenderTargetBinding(renderTargetB);
      graphicsDevice.SetRenderTargets(_renderTargetBindings);

      context.RenderTarget = renderTargetA;
      context.Viewport = graphicsDevice.Viewport;

      // Clear render targets to (0, 0, 0, 1).
      graphicsDevice.Clear(Color.Black);

      // Make a Z-only pass to render opaque objects in to the depth buffer.
      // (Color output is disabled.)
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = GraphicsHelper.BlendStateNoColorWrite;
      context.RenderPass = "ZOnly";
      _meshRenderer.Render(query.SceneNodes, context, RenderOrder.Default);
      context.RenderPass = null;

      // Render transparent objects into the WBOIT render targets.
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.BlendState = _wboitRenderBlendState;
      context.RenderPass = "WeightedBlendedOIT";
      _meshRenderer.Render(query.TransparentNodes, context, RenderOrder.Default);
      context.RenderPass = null;

      _renderTargetBindings[0] = default(RenderTargetBinding);
      _renderTargetBindings[1] = default(RenderTargetBinding);

      // ----- Opaque pass
      // Switch back to original render target.
      graphicsDevice.SetRenderTarget(target);
      context.RenderTarget = target;
      context.Viewport = viewport;

      graphicsDevice.Clear(Color.CornflowerBlue);

      // Render opaque objects.
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      context.RenderPass = "Default";
      _meshRenderer.Render(query.SceneNodes, context, RenderOrder.Default);
      context.RenderPass = null;

      // ----- Combine pass
      // Combine the WBOIT render targets with the scene.
      graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.BlendState = _wboitCombineBlendState;
      _parameterViewportSize.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _parameterTextureA.SetValue(renderTargetA);
      _parameterTextureB.SetValue(renderTargetB);
      _wboitEffect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawFullScreenQuad();
      _parameterTextureA.SetValue((Texture2D)null);
      _parameterTextureB.SetValue((Texture2D)null);

      renderTargetPool.Recycle(renderTargetA);
      renderTargetPool.Recycle(renderTargetB);
    }
  }
}
