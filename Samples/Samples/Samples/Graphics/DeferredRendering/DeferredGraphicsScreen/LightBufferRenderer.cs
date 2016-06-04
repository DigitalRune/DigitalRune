#if !WP7 && !WP8
using System;
using System.Collections.Generic;
using DigitalRune;
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples
{
  // The type of ambient occlusion.
  public enum AmbientOcclusionType
  {
    None,   // No ambient occlusion
    SSAO,   // Using SsaoFilter (Screen Space Ambient Occlusion)
    SAO,    // Using SaoFilter (Scalable Ambient Obscurance)
  }


  // This renderer renders light nodes and creates the light buffer which stores 
  // the accumulated diffuse and specular light intensities. 
  // The light buffer is stored in the render context. It can be used by the 
  // following renderers (usually by the "Material" pass), and it must be recycled 
  // by the graphics screen.
  public sealed class LightBufferRenderer : IDisposable
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _disposed;
    private readonly IGraphicsService _graphicsService;

    // Pre-allocate array to avoid allocations at runtime.
    private readonly RenderTargetBinding[] _renderTargetBindings = new RenderTargetBinding[2];

    private PostProcessor _ssaoFilter;
    private int _ssaoDownsampleFactor;
    private readonly CopyFilter _copyFilter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    public LightRenderer LightRenderer { get; private set; }


    // Get/set ambient occlusion type and the _ssaoFilter.
    public AmbientOcclusionType AmbientOcclusionType
    {
      get { return _ambientOcclusionType; }
      set
      {
        if (_ambientOcclusionType == value)
          return;

        if (_ssaoFilter != null)
          _ssaoFilter.Dispose();

        _ambientOcclusionType = value;

        switch (_ambientOcclusionType)
        {
          case AmbientOcclusionType.None:
            _ssaoFilter = null;
            break;
          case AmbientOcclusionType.SSAO:
            _ssaoDownsampleFactor = 2;
            _ssaoFilter = new SsaoFilter(_graphicsService)
            {
              DownsampleFactor = _ssaoDownsampleFactor,

              // Normally the SsaoFilter applies the occlusion values directly to the 
              // source texture. But here the filter should ignore the input image and 
              // create a grayscale image (white = no occlusion, black = max occlusion).
              CombineWithSource = false,
            };

            break;
          case AmbientOcclusionType.SAO:
            _ssaoDownsampleFactor = 1;
            _ssaoFilter = new SaoFilter(_graphicsService)
            {
              // Normally the SaoFilter applies the occlusion values directly to the 
              // source texture. But here the filter should ignore the input image and 
              // create a grayscale image (white = no occlusion, black = max occlusion).
              CombineWithSource = false,
            };
            break;
        }

        _ambientOcclusionType = value;
      }
    }
    private AmbientOcclusionType _ambientOcclusionType;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public LightBufferRenderer(IGraphicsService graphicsService)
    {
      _graphicsService = graphicsService;
      LightRenderer = new LightRenderer(graphicsService);
      AmbientOcclusionType = AmbientOcclusionType.SSAO;
      _copyFilter = new CopyFilter(graphicsService);
    }


    public void Dispose()
    {
      if (!_disposed)
      {
        _disposed = true;
        LightRenderer.Dispose();
        _ssaoFilter.SafeDispose();
        _copyFilter.Dispose();
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    public void Render(IList<SceneNode> lights, RenderContext context)
    {
      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var renderTargetPool = graphicsService.RenderTargetPool;

      var target = context.RenderTarget;
      var viewport = context.Viewport;
      var width = viewport.Width;
      var height = viewport.Height;

      RenderTarget2D aoRenderTarget = null;
      if (_ssaoFilter != null)
      {
        // Render ambient occlusion info into a render target.
        aoRenderTarget = renderTargetPool.Obtain2D(new RenderTargetFormat(
          width / _ssaoDownsampleFactor,
          height / _ssaoDownsampleFactor,
          false,
          SurfaceFormat.Color,
          DepthFormat.None));

        // PostProcessors require that context.SourceTexture is set. But since 
        // _ssaoFilter.CombineWithSource is set to false, the SourceTexture is not 
        // used and we can set it to anything except null.
        context.SourceTexture = aoRenderTarget;
        context.RenderTarget = aoRenderTarget;
        context.Viewport = new Viewport(0, 0, aoRenderTarget.Width, aoRenderTarget.Height);
        _ssaoFilter.Process(context);
        context.SourceTexture = null;
      }

      // The light buffer consists of two full-screen render targets into which we 
      // render the accumulated diffuse and specular light intensities.
      var lightBufferFormat = new RenderTargetFormat(width, height, false, SurfaceFormat.HdrBlendable, DepthFormat.Depth24Stencil8);
      context.LightBuffer0 = renderTargetPool.Obtain2D(lightBufferFormat);
      context.LightBuffer1 = renderTargetPool.Obtain2D(lightBufferFormat);

      // Set the device render target to the light buffer.
      _renderTargetBindings[0] = new RenderTargetBinding(context.LightBuffer0); // Diffuse light accumulation
      _renderTargetBindings[1] = new RenderTargetBinding(context.LightBuffer1); // Specular light accumulation
      graphicsDevice.SetRenderTargets(_renderTargetBindings);
      context.RenderTarget = context.LightBuffer0;
      context.Viewport = graphicsDevice.Viewport;

      // Clear the light buffer. (The alpha channel is not used. We can set it to anything.)
      graphicsDevice.Clear(new Color(0, 0, 0, 255));

      // Restore the depth buffer (which XNA destroys in SetRenderTarget).
      // (This is only needed if lights can use a clip geometry (LightNode.Clip).)
      var rebuildZBufferRenderer = (RebuildZBufferRenderer)context.Data[RenderContextKeys.RebuildZBufferRenderer];
      rebuildZBufferRenderer.Render(context, true);

      // Render all lights into the light buffers.
      LightRenderer.Render(lights, context);

      if (aoRenderTarget != null)
      {
        // Render the ambient occlusion texture using multiplicative blending.
        // This will darken the light buffers depending on the ambient occlusion term.
        // Note: Theoretically, this should be done after the ambient light renderer 
        // and before the directional light renderer because AO should not affect 
        // directional lights. But doing this here has more impact.
        context.SourceTexture = aoRenderTarget;
        graphicsDevice.BlendState = GraphicsHelper.BlendStateMultiply;
        _copyFilter.Process(context);
      }

      // Clean up.
      graphicsService.RenderTargetPool.Recycle(aoRenderTarget);
      context.SourceTexture = null;
      context.RenderTarget = target;
      context.Viewport = viewport;

      _renderTargetBindings[0] = new RenderTargetBinding();
      _renderTargetBindings[1] = new RenderTargetBinding();
    }
    #endregion
  }
}
#endif
