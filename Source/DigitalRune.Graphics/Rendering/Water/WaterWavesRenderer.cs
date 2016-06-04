// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Generates the <see cref="OceanWaves"/> for <see cref="WaterNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="WaterWavesRenderer"/> is a <see cref="SceneNodeRenderer"/> that handles
  /// <see cref="WaterNode"/>s. If a <see cref="WaterNode"/> uses <see cref="OceanWaves"/>, the
  /// renderer creates the wave textures and stores the result in the properties
  /// <see cref="WaterWaves.DisplacementMap"/> and <see cref="WaterWaves.NormalMap"/>.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer changes the current render target of the graphics device because it uses the 
  /// graphics device to render the wave maps into internal render targets. The render target and
  /// the viewport of the graphics device are undefined after rendering.
  /// </para>
  /// </remarks>
  public class WaterWavesRenderer : SceneNodeRenderer
  {
    // TODO: Run CPU FFT as parallel task.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IGraphicsService _graphicsService;
    private readonly RenderTargetBinding[] _renderTargetBindings = new RenderTargetBinding[2];

    private readonly Effect _effect;
    private readonly EffectParameter _parameterSize;
    private readonly EffectParameter _parameterSpectrumParameters;
    private readonly EffectParameter _parameterSourceTexture;
    private readonly EffectPass _passSpectrum;

    private readonly OceanFft _fft;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudMapRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public WaterWavesRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");
      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        throw new NotSupportedException("The WaterWavesRenderer does not support the Reach profile.");

      _graphicsService = graphicsService;

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Water/Ocean");
      _parameterSize = _effect.Parameters["Size"];
      _parameterSpectrumParameters = _effect.Parameters["SpectrumParameters"];
      _parameterSourceTexture = _effect.Parameters["SourceTexture0"];
      _passSpectrum = _effect.Techniques[0].Passes["Spectrum"];

      _fft = new OceanFft(graphicsService);
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          _fft.Dispose();
        }
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var waterNode = node as WaterNode;
      return waterNode != null && waterNode.Waves is OceanWaves;
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (nodes.Count == 0)
        return;

      context.Validate(_effect);

      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      //var renderTargetPool = graphicsService.RenderTargetPool;

      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.BlendState = BlendState.Opaque;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.DepthStencilState = DepthStencilState.None;

      int frame = context.Frame;

      for (int nodeIndex = 0; nodeIndex < numberOfNodes; nodeIndex++)
      {
        var node = nodes[nodeIndex] as WaterNode;
        if (node == null)
          continue;

        var waves = node.Waves as OceanWaves;
        if (waves == null)
          continue;

        // We update the waves only once per frame.
        if (waves.LastFrame == frame)
          continue;

        waves.LastFrame = frame;

        float time = (float)context.Time.TotalSeconds;

        // Initialize h0 spectrum. Perform CPU FFT.
        waves.Update(graphicsDevice, time);

        int n = waves.TextureSize;

        // Allocate textures in the first frame and when the TextureSize was changed.
        if (waves.DisplacementSpectrum == null || waves.DisplacementSpectrum.Width != n)
        {
          waves.DisplacementSpectrum.SafeDispose();
          waves.NormalSpectrum.SafeDispose();
          waves.DisplacementMap.SafeDispose();
          waves.NormalMap.SafeDispose();

          waves.DisplacementSpectrum = new RenderTarget2D(_graphicsService.GraphicsDevice, n, n, false, SurfaceFormat.Vector4, DepthFormat.None);
          waves.NormalSpectrum = new RenderTarget2D(_graphicsService.GraphicsDevice, n, n, false, SurfaceFormat.Vector4, DepthFormat.None);
          waves.DisplacementMap = new RenderTarget2D(_graphicsService.GraphicsDevice, n, n, false, SurfaceFormat.Vector4, DepthFormat.None);
          waves.NormalMap = new RenderTarget2D(
            _graphicsService.GraphicsDevice, 
            n, 
            n,
            true,
            SurfaceFormat.Color, 
            DepthFormat.None);
        }

        // Create spectrum (h, D, N) for current time from h0.
        _renderTargetBindings[0] = new RenderTargetBinding(waves.DisplacementSpectrum);
        _renderTargetBindings[1] = new RenderTargetBinding(waves.NormalSpectrum);
        graphicsDevice.SetRenderTargets(_renderTargetBindings);
        _parameterSize.SetValue((float)n);
        _parameterSpectrumParameters.SetValue(new Vector4(
          waves.TileSize,
          waves.Gravity,
          time,
          waves.HeightScale));
        _parameterSourceTexture.SetValue(waves.H0Spectrum);
        _passSpectrum.Apply();
        graphicsDevice.DrawFullScreenQuad();

        // Do inverse FFT.
        _fft.Process(
          context,
          false,
          waves.DisplacementSpectrum,
          waves.NormalSpectrum,
          (RenderTarget2D)waves.DisplacementMap,
          (RenderTarget2D)waves.NormalMap,
          waves.Choppiness);

        #region ----- Old Debugging Code -----

        // Create textures from CPU FFT data for debug visualization.
        //n = waves.CpuSize;
        //var s0Data = new Vector4[n * n];
        //var s1Data = new Vector4[n * n];
        //var s0 = new RenderTarget2D(_graphicsService.GraphicsDevice, n, n, false, SurfaceFormat.Vector4, DepthFormat.None);
        //var s1 = new RenderTarget2D(_graphicsService.GraphicsDevice, n, n, false, SurfaceFormat.Vector4, DepthFormat.None);
        //for (int y = 0; y < n; y++)
        //{
        //  for (int x = 0; x < n; x++)
        //  {
        //s0Data[y * n + x] = new Vector4(
        //  -waves._D[x, y].X * waves.Choppiness,
        //  waves._h[x, y].X * 1,
        //  -waves._D[x, y].Y * waves.Choppiness,
        //  1);

        //s1Data[y * n + x] = new Vector4(
        //  waves._N[x, y].X,
        //  waves._N[x, y].Y,
        //  0,
        //  0);
        //  }
        //}
        //s0.SetData(s0Data);
        //s1.SetData(s1Data);
        //WaterSample._t0 = s0;
        //WaterSample._t1 = waves.DisplacementMap;
        #endregion
      }

      savedRenderState.Restore();
      graphicsDevice.SetRenderTarget(null);
      context.RenderTarget = originalRenderTarget;
      context.Viewport = originalViewport;

      _renderTargetBindings[0] = default(RenderTargetBinding);
      _renderTargetBindings[1] = default(RenderTargetBinding); 

      // Reset the texture stages. If a floating point texture is set, we get exceptions
      // when a sampler with bilinear filtering is set.
#if !MONOGAME
      graphicsDevice.ResetTextures();
#endif
    }
    #endregion
  }
}
#endif
