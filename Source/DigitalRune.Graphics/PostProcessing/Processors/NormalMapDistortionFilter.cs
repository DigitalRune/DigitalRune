// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  // TODO:
  // This class is currently internal. We need to make a better DistortionFilter for
  // local heat distortion effects.
  // Should we rename NormalMap to NormalTexture for consistency?

  /// <summary>
  /// Distorts the image using information from a normal map.
  /// </summary>
  internal class NormalMapDistortionFilter : PostProcessor
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _scaleParameter;
    private readonly EffectParameter _offsetParameter;
    private readonly EffectParameter _strengthParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _normalMapParameter;
    private readonly EffectPass _basicPass;
    private readonly EffectPass _blur4Pass;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the normal map that defines the perturbation.
    /// </summary>
    /// <value>The normal map.</value>
    public Texture2D NormalMap { get; set; }


    /// <summary>
    /// Gets or sets the offset that is applied to texture lookups in the <see cref="NormalMap"/>.
    /// </summary>
    /// <value>The offset for normal map lookups.</value>
    public Vector2F Offset { get; set; }


    /// <summary>
    /// Gets or sets the scale applied to the <see cref="NormalMap"/>.
    /// </summary>
    /// <value>The scale. The default value is (1,1).</value>
    public Vector2F Scale { get; set; }


    /// <summary>
    /// Gets or sets the strength of the distortion.
    /// </summary>
    /// <value>
    /// The strength. 0 means no distortion. Distortion increases with values greater than 0.
    /// The default value is 10.
    /// </value>
    public float Strength { get; set; }


    /// <summary>
    /// Gets or sets the blur level.
    /// </summary>
    /// <value>The blur level. Set to 0 for no blur. Set to 1 for a 4 tap blur.</value>
    public int BlurLevel
    {
      get { return _blurLevel; }
      set
      {
        if (value < 0 || value > 1)
          throw new ArgumentOutOfRangeException("value", "Allowed blur levels are 0 and 1. Other blur levels are currently not supported.");

        _blurLevel = value;
      }
    }
    private int _blurLevel;      // TODO: We could add more levels with Poisson blur, etc.
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="NormalMapDistortionFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public NormalMapDistortionFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/NormalMapDistortionFilter");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _scaleParameter = _effect.Parameters["Scale"];
      _offsetParameter = _effect.Parameters["Offset"];
      _strengthParameter = _effect.Parameters["Strength"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _normalMapParameter = _effect.Parameters["NormalMap"];
      _basicPass = _effect.CurrentTechnique.Passes["Basic"];
      _blur4Pass = _effect.CurrentTechnique.Passes["Blur4"];

      Scale = new Vector2F(1);
      Strength = 10;
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

      if (NormalMap == null || TextureHelper.IsFloatingPointFormat(NormalMap.Format))
        graphicsDevice.SamplerStates[1] = SamplerState.PointWrap;
      else
        graphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

      graphicsDevice.SetRenderTarget(context.RenderTarget);
      graphicsDevice.Viewport = context.Viewport;

      var viewportSize = new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);
      _viewportSizeParameter.SetValue(viewportSize);
      _scaleParameter.SetValue(new Vector2(1 / Scale.X, 1 / Scale.Y));
      _offsetParameter.SetValue((Vector2)Offset);
      _strengthParameter.SetValue(new Vector2(Strength / viewportSize.X, Strength / viewportSize.Y));
      _sourceTextureParameter.SetValue(context.SourceTexture);
      _normalMapParameter.SetValue(NormalMap);
      if (BlurLevel == 0)
        _basicPass.Apply();
      else
        _blur4Pass.Apply();

      graphicsDevice.DrawFullScreenQuad();

      _sourceTextureParameter.SetValue((Texture2D)null);
      _normalMapParameter.SetValue((Texture2D)null);
    }
    #endregion
  }
}
#endif
