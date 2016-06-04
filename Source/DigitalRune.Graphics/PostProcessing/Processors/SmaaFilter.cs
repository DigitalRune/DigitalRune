// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Applies <i>Enhanced Subpixel Morphological Anti-Aliasing</i> (SMAA).
  /// </summary>
  /// <remarks>
  /// SMAA should be applied to a texture in sRGB space (= gamma-corrected, non-linear color 
  /// values).
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class SmaaFilter : PostProcessor
  {
    // Note:
    // The original SMAA writes to the stencil buffer in the edge detection pass 
    // and the second pass is only applied to pixels that pass the stencil buffer.
    // This is not possible in XNA because the stencil buffer is not preserved.
    //
    // If possible sRGB reads should be enabled for the input texture (all passes)
    // and sRGB writes should be enabled for the render target in the final pass.
    // (But anti-aliasing also works in sRGB space.)


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    //private readonly DepthStencilState _stencilStateReplace;
    //private readonly DepthStencilState _stencilStateKeep;
    private readonly Effect _effect;
    private readonly EffectParameter _pixelSizeParameter;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _edgesTextureParameter;
    private readonly EffectParameter _areaLookupTextureParameter;
    private readonly EffectParameter _searchLookupTextureParameter;
    private readonly EffectParameter _blendTextureParameter;
    private readonly EffectPass _lumaEdgeDetectionPass;
    private readonly EffectPass _blendWeightCalculationPass;
    private readonly EffectPass _neighborhoodBlendingPass;

    private readonly Texture2D _searchLookupTexture;
    private readonly Texture2D _areaLookupTexture;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SmaaFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SmaaFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      //_stencilStateReplace = new DepthStencilState
      //{
      //  DepthBufferEnable = false,
      //  DepthBufferWriteEnable = false,
      //  StencilEnable = true,
      //  StencilPass = StencilOperation.Replace,
      //  ReferenceStencil = 1,

      //  TwoSidedStencilMode = true,
      //};

      //_stencilStateKeep = new DepthStencilState
      //{
      //  DepthBufferEnable = false,
      //  DepthBufferWriteEnable = false,
      //  StencilEnable = true,
      //  StencilPass = StencilOperation.Keep,
      //  StencilFunction = CompareFunction.Equal,
      //  ReferenceStencil = 1,
      //};

      var content = GraphicsService.Content;
      _effect = content.Load<Effect>("DigitalRune/PostProcessing/SmaaFilter");
      _pixelSizeParameter = _effect.Parameters["PixelSize"];
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _edgesTextureParameter = _effect.Parameters["EdgesTexture"];
      _areaLookupTextureParameter = _effect.Parameters["AreaLookupTexture"];
      _searchLookupTextureParameter = _effect.Parameters["SearchLookupTexture"];
      _blendTextureParameter = _effect.Parameters["BlendTexture"];
      _lumaEdgeDetectionPass = _effect.CurrentTechnique.Passes["LumaEdgeDetection"];
      _blendWeightCalculationPass = _effect.CurrentTechnique.Passes["BlendWeightCalculation"];
      _neighborhoodBlendingPass = _effect.CurrentTechnique.Passes["NeighborhoodBlending"];

      _areaLookupTexture = content.Load<Texture2D>("DigitalRune/PostProcessing/SmaaAreaTexDX9");
      _searchLookupTexture = content.Load<Texture2D>("DigitalRune/PostProcessing/SmaaSearchTex");
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        throw new GraphicsException("Source texture format must not be a floating-point format.");

      var graphicsDevice = GraphicsService.GraphicsDevice;

      // The target width/height.
      int targetWidth = context.Viewport.Width;
      int targetHeight = context.Viewport.Height;

      _pixelSizeParameter.SetValue(new Vector2(1.0f / targetWidth, 1.0f / targetHeight));
      _viewportSizeParameter.SetValue(new Vector2(targetWidth, targetHeight));

      // Cannot use render target from pool because we need a stencil buffer.
      //var edgeRenderTarget = graphicsService.RenderTargetPool.Obtain2D(targetWidth, targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
      //var blendRenderTarget = graphicsService.RenderTargetPool.Obtain2D(targetWidth, targetHeight, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
      var tempFormat = new RenderTargetFormat(targetWidth, targetHeight, false, SurfaceFormat.Color, DepthFormat.None);
      var edgeRenderTarget = GraphicsService.RenderTargetPool.Obtain2D(tempFormat);
      var blendRenderTarget = GraphicsService.RenderTargetPool.Obtain2D(tempFormat);

      //graphicsDevice.DepthStencilState = _stencilStateReplace;
      graphicsDevice.SetRenderTarget(edgeRenderTarget);
      // Clear color + stencil buffer.
      //graphicsDevice.Clear(ClearOptions.Target | ClearOptions.Stencil, new Color(0, 0, 0, 0), 1, 0);
      graphicsDevice.Clear(ClearOptions.Target, new Color(0, 0, 0, 0), 1, 0);
      _sourceTextureParameter.SetValue(context.SourceTexture);
      _lumaEdgeDetectionPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      //graphicsDevice.DepthStencilState = _stencilStateKeep;
      graphicsDevice.SetRenderTarget(blendRenderTarget);
      //graphicsDevice.Clear(ClearOptions.Target, new Color(0, 0, 0, 0), 1, 1);
      _edgesTextureParameter.SetValue(edgeRenderTarget);
      _areaLookupTextureParameter.SetValue(_areaLookupTexture);
      _searchLookupTextureParameter.SetValue(_searchLookupTexture);
      _blendWeightCalculationPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      //graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.SetRenderTarget(context.RenderTarget);
      graphicsDevice.Viewport = context.Viewport;
      _blendTextureParameter.SetValue(blendRenderTarget);
      _neighborhoodBlendingPass.Apply();
      graphicsDevice.DrawFullScreenQuad();

      _sourceTextureParameter.SetValue((Texture2D)null);
      _edgesTextureParameter.SetValue((Texture2D)null);
      _blendTextureParameter.SetValue((Texture2D)null);

      GraphicsService.RenderTargetPool.Recycle(blendRenderTarget);
      GraphicsService.RenderTargetPool.Recycle(edgeRenderTarget);
    }
    #endregion
  }
}
#endif
