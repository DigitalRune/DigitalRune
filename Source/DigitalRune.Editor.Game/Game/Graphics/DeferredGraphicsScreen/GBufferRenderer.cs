#if !WP7 && !WP8
using System.Collections.Generic;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples
{
  // This renderer renders opaque geometry (usually the opaque submeshes of MeshNodes) 
  // and creates the G-Buffer which stores the depth values, normal vectors and other information.
  // The G-Buffer is stored in the render context. It can be used by the following
  // renderers (e.g. by the LightBufferRenderer or the post-processors), and it must
  // be recycled by the graphics screen.
  public class GBufferRenderer 
  {
    // Pre-allocated data structures to avoid allocations at runtime.
    private readonly RenderTargetBinding[] _renderTargetBindings = new RenderTargetBinding[2];

    private readonly ClearGBufferRenderer _clearGBufferRenderer;
    private readonly SceneNodeRenderer _sceneNodeRenderer;
    private readonly DecalRenderer _decalRenderer;
    private readonly DownsampleFilter _downsampleFilter;


    public GBufferRenderer(IGraphicsService graphicsService, SceneNodeRenderer sceneNodeRenderer, DecalRenderer decalRenderer)
    {
      _clearGBufferRenderer = new ClearGBufferRenderer(graphicsService);
      _sceneNodeRenderer = sceneNodeRenderer;
      _decalRenderer = decalRenderer;

      // This filter is used to downsample the depth buffer (GBuffer0).
      _downsampleFilter = PostProcessHelper.GetDownsampleFilter(graphicsService);
    }


    public void Render(IList<SceneNode> sceneNodes, IList<SceneNode> decalNodes, RenderContext context)
    {
      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var renderTargetPool = graphicsService.RenderTargetPool;
      var target = context.RenderTarget;
      var viewport = context.Viewport;

      // The G-buffer consists of two full-screen render targets into which we render 
      // depth values, normal vectors and other information.
      var width = context.Viewport.Width;
      var height = context.Viewport.Height;
      context.GBuffer0 = renderTargetPool.Obtain2D(new RenderTargetFormat(
        width, 
        height,
#if !XBOX
        true,        // Note: Only the SaoFilter for SSAO requires mipmaps to boost performance.
#endif
        SurfaceFormat.Single, 
        DepthFormat.Depth24Stencil8));
      context.GBuffer1 = renderTargetPool.Obtain2D(new RenderTargetFormat(width, height, false, SurfaceFormat.Color, DepthFormat.None));

      // Set the device render target to the G-buffer.
      _renderTargetBindings[0] = new RenderTargetBinding(context.GBuffer0);
      _renderTargetBindings[1] = new RenderTargetBinding(context.GBuffer1);
      graphicsDevice.SetRenderTargets(_renderTargetBindings);
      context.RenderTarget = context.GBuffer0;

      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.BlendState = BlendState.Opaque;

      // Clear the z-buffer.
      graphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1, 0);

      // Initialize the G-buffer with default values. 
      _clearGBufferRenderer.Render(context);

      // Render the scene nodes using the "GBuffer" material pass.
      context.RenderPass = "GBuffer";
      graphicsDevice.DepthStencilState = DepthStencilState.Default;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      graphicsDevice.BlendState = BlendState.Opaque;
      _sceneNodeRenderer.Render(sceneNodes, context);

      if (_decalRenderer != null && decalNodes.Count > 0)
      {
        // Render decal nodes using the "GBuffer" material pass.
        // Decals are rendered as "deferred decals". The geometry information is 
        // read from GBuffer0 and the decal normals are blended with GBuffer1, which
        // has to be set as the first render target. (That means a new GBuffer1 is 
        // created. The original GBuffer1 is recycled afterwards.)
        var renderTarget = renderTargetPool.Obtain2D(new RenderTargetFormat(width, height, false, SurfaceFormat.Color, DepthFormat.None));
        graphicsDevice.SetRenderTarget(renderTarget);
        context.RenderTarget = renderTarget;

        // Copy GBuffer1 to current render target and restore the depth buffer.
        var rebuildZBufferRenderer = (RebuildZBufferRenderer)context.Data[RenderContextKeys.RebuildZBufferRenderer];
        rebuildZBufferRenderer.Render(context, context.GBuffer1);

        // Blend decals with the render target.
        _decalRenderer.Render(decalNodes, context);

        // The new render target replaces the GBuffer1.
        renderTargetPool.Recycle(context.GBuffer1);
        context.GBuffer1 = renderTarget;
      }
      context.RenderPass = null;

      // The depth buffer is downsampled into a buffer of half width and half height.
      RenderTarget2D depthBufferHalf = renderTargetPool.Obtain2D(new RenderTargetFormat(width / 2, height / 2, false, context.GBuffer0.Format, DepthFormat.None));
      context.SourceTexture = context.GBuffer0;
      context.RenderTarget = depthBufferHalf;
      context.Viewport = new Viewport(0, 0, depthBufferHalf.Width, depthBufferHalf.Height);
      _downsampleFilter.Process(context);
      context.SourceTexture = null;

      // Store the result in the render context. Depending on the settings, the downsampled 
      // depth buffer is used by the SsaoFilter (if SsaoFilter.DownsampleFactor == 2), or 
      // by the BillboardRenderer (if EnableOffscreenRendering is set).
      context.Data[RenderContextKeys.DepthBufferHalf] = depthBufferHalf;

      context.RenderTarget = target;
      context.Viewport = viewport;
      _renderTargetBindings[0] = new RenderTargetBinding();
      _renderTargetBindings[1] = new RenderTargetBinding();
    }
  }
}
#endif
