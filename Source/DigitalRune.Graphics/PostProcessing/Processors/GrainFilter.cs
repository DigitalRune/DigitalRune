// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Adds a film grain effect.
  /// </summary>
  /// <remarks>
  /// This processor adds noise to the source texture.
  /// </remarks>
  public class GrainFilter : PostProcessor
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Random _random;
    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _strengthParameter;
    private readonly EffectParameter _grainScaleParameter;
    private readonly EffectParameter _timeParameter;
    private readonly EffectParameter _luminanceThresholdParameter;
    private readonly EffectPass _scaledNoisePass;
    private readonly EffectPass _equalNoisePass;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether noise is blended equally to all pixels or whether 
    /// more noise is blended to dark pixels.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if more noise is added to darker pixels; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    public bool ScaleWithLuminance { get; set; }


    /// <summary>
    /// Gets or sets the luminance threshold. (Noise is only added to pixels with a luminance below 
    /// this threshold. This value is ignored if <see cref="ScaleWithLuminance"/> is 
    /// <see langword="false"/>.)
    /// </summary>
    /// <value>The luminance threshold. The default value is 1.</value>
    public float LuminanceThreshold { get; set; }


    /// <summary>
    /// Gets or sets the strength of the grain effect.
    /// </summary>
    /// <value>The effect strength. The default value is 0.1.</value>
    public float Strength { get; set; }


    /// <summary>
    /// Gets or sets the grain scale.
    /// </summary>
    /// <value>
    /// The grain scale. The default value is 1. Larger values than 1 make the noise pixels larger.
    /// </value>
    public float GrainScale { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the noise is animated.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the noise is animated; otherwise, <see langword="false" />
    /// if the noise is static. The default value is <see langword="true" />.
    /// </value>
    public bool IsAnimated { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GrainFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public GrainFilter(IGraphicsService graphicsService) : base(graphicsService)
    {
      _random = new Random();

      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/GrainFilter");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _strengthParameter = _effect.Parameters["Strength"];
      _grainScaleParameter = _effect.Parameters["GrainScale"];
      _timeParameter = _effect.Parameters["Time"];
      _luminanceThresholdParameter = _effect.Parameters["LuminanceThreshold"];
      _scaledNoisePass = _effect.CurrentTechnique.Passes["ScaledNoise"];
      _equalNoisePass = _effect.CurrentTechnique.Passes["EqualNoise"];

      ScaleWithLuminance = true;
      Strength = 0.1f;
      LuminanceThreshold = 1;
      GrainScale = 1;
      IsAnimated = true;
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
      _strengthParameter.SetValue(Strength);
      _grainScaleParameter.SetValue(GrainScale);
      _timeParameter.SetValue(IsAnimated ? _random.NextFloat(0, 1) : 0);
      if (ScaleWithLuminance)
      {
        _luminanceThresholdParameter.SetValue(LuminanceThreshold);
        _scaledNoisePass.Apply();
      }
      else
      {
        _equalNoisePass.Apply();
      }

      graphicsDevice.DrawFullScreenQuad();

      _sourceTextureParameter.SetValue((Texture2D)null);
    }
    #endregion
  }
}
#endif
