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
  /// Creates a radial blur effect.
  /// </summary>
  /// <remarks>
  /// The screen is blurred radially from the center to the border. The blur increases from the 
  /// center to <see cref="MaxBlurRadius"/>. All pixels beyond <see cref="MaxBlurRadius"/> use the 
  /// maximal blur. Pixels in the screen center are blurred less.
  /// </remarks>
  public class RadialBlur : PostProcessor
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _numberOfSamplesParameter;
    private readonly EffectParameter _maxBlurRadiusParameter;
    private readonly EffectParameter _maxBlurAmountParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the radius in the range [0, 1] where the maximum blur is reached.
    /// </summary>
    /// <value>The max blur radius. The default value is 1.</value>
    /// <remarks>
    /// 0 means full blur starts in the screen center. 1 means full blur is reached at screen 
    /// border.
    /// </remarks>
    public float MaxBlurRadius { get; set; }


    /// <summary>
    /// Gets or sets the range of texels that are blurred at <see cref="MaxBlurRadius"/>.
    /// </summary>
    /// <value>
    /// The range of texels that are blurred at <see cref="MaxBlurRadius"/> relative to the viewport
    /// size. The default value is 0.04 (= 4% of viewport size).
    /// </value>
    public float MaxBlurAmount { get; set; }


    /// <summary>
    /// Gets or sets the number of samples that are used in the blur.
    /// </summary>
    /// <value>The number of samples. The default value is 5.</value>
    public int NumberOfSamples { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="RadialBlur"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public RadialBlur(IGraphicsService graphicsService) : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/RadialBlur");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _numberOfSamplesParameter = _effect.Parameters["NumberOfSamples"];
      _maxBlurRadiusParameter = _effect.Parameters["MaxBlurRadius"];
      _maxBlurAmountParameter = _effect.Parameters["MaxBlurAmount"];

      NumberOfSamples = 5;
      MaxBlurRadius = 1;
      MaxBlurAmount = 0.04f;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;

      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      graphicsDevice.SetRenderTarget(context.RenderTarget);
      graphicsDevice.Viewport = context.Viewport;

      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _sourceTextureParameter.SetValue(context.SourceTexture);
      _numberOfSamplesParameter.SetValue(NumberOfSamples);
      _maxBlurRadiusParameter.SetValue(MaxBlurRadius);
      _maxBlurAmountParameter.SetValue(MaxBlurAmount);
      _effect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawFullScreenQuad();

      _sourceTextureParameter.SetValue((Texture2D)null);
    }
    #endregion
  }
}
#endif
