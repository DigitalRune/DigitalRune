// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Reconstructs the hardware Z-buffer from the G-buffer.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This renderer reads the G-Buffer and outputs depth to the hardware Z-buffer. The resulting
  /// Z-buffer is not totally accurate but should be good enough for most operations.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  public class RebuildZBufferRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterProjection;
    private readonly EffectParameter _parameterCameraFar;
    private readonly EffectParameter _parameterGBuffer0;
    private readonly EffectParameter _parameterColor;
    private readonly EffectParameter _parameterSourceTexture;
    private readonly EffectTechnique _techniqueOrthographic;
    private readonly EffectTechnique _techniquePerspective;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the factor used to bias the camera near plane distance to avoid 
    /// z-fighting.
    /// </summary>
    /// <value>The near bias factor. The default value is 1 (no bias).</value>
    public float NearBias { get; set; }


    /// <summary>
    /// Gets or sets the factor used to bias the camera far plane distance to avoid 
    /// z-fighting.
    /// </summary>
    /// <value>The far bias factor. The default value is 0.995f.</value>
    public float FarBias { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="RebuildZBufferRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public RebuildZBufferRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      NearBias = 1;
      FarBias = 0.995f;

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Deferred/RebuildZBuffer");
      _parameterViewportSize = _effect.Parameters["ViewportSize"];
      _parameterProjection = _effect.Parameters["Projection"];
      _parameterCameraFar = _effect.Parameters["CameraFar"];
      _parameterGBuffer0 = _effect.Parameters["GBuffer0"];
      _parameterColor = _effect.Parameters["Color"];
      _parameterSourceTexture = _effect.Parameters["SourceTexture"];
      _techniqueOrthographic = _effect.Techniques["Orthographic"];
      _techniquePerspective = _effect.Techniques["Perspective"];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Rebuilds the current hardware Z-buffer from the G-Buffer and optionally writes a color or
    /// texture to the render target.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Rebuilds the current hardware Z-buffer from the G-Buffer and writes the specified color 
    /// value to the current render target.
    /// </summary>
    /// <param name="context">
    /// The render context. (<see cref="RenderContext.CameraNode"/> and 
    /// <see cref="RenderContext.GBuffer0"/> need to be set.)
    /// </param>
    /// <param name="color">The color to be written to the render target.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void Render(RenderContext context, Vector4F color)
    {
      Render(context, color, null, false);
    }


    /// <summary>
    /// Rebuilds the current hardware Z-buffer from the G-Buffer and copies the specified texture
    /// to the render target.
    /// </summary>
    /// <param name="context">
    /// The render context. (<see cref="RenderContext.CameraNode"/> and 
    /// <see cref="RenderContext.GBuffer0"/> need to be set.)
    /// </param>
    /// <param name="colorTexture">
    /// Optional: The color texture to be copied to the render target.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void Render(RenderContext context, Texture2D colorTexture)
    {
      Render(context, Vector4F.Zero, colorTexture, colorTexture == null);
    }


    /// <summary>
    /// Rebuilds the current hardware Z-buffer from the G-Buffer and clears or preserves the current
    /// render target.
    /// </summary>
    /// <param name="context">
    /// The render context. (<see cref="RenderContext.CameraNode"/> and 
    /// <see cref="RenderContext.GBuffer0"/> need to be set.)
    /// </param>
    /// <param name="preserveColor">
    /// If set to <see langword="true"/> color writes are disabled to preserve the current content;
    /// otherwise, <see langword="false"/> to clear the color target.
    /// </param>
    /// <remarks>
    /// Note that the option <paramref name="preserveColor"/> (to disable color writes) is not 
    /// supported by all render target formats.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="context"/> is <see langword="null"/>.
    /// </exception>
    public void Render(RenderContext context, bool preserveColor)
    {
      Render(context, Vector4F.Zero, null, preserveColor);
    }


    private void Render(RenderContext context, Vector4F color, Texture2D colorTexture, bool preserveColor)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      context.Validate(_effect);
      context.ThrowIfCameraMissing();
      context.ThrowIfGBuffer0Missing();

      var graphicsDevice = _effect.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.DepthStencilState = GraphicsHelper.DepthStencilStateAlways;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;

      if (preserveColor)
        graphicsDevice.BlendState = GraphicsHelper.BlendStateNoColorWrite;
      else
        graphicsDevice.BlendState = BlendState.Opaque;

      if (colorTexture != null)
      {
        if (TextureHelper.IsFloatingPointFormat(colorTexture.Format))
          graphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
        else
          graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
      }

      var projection = context.CameraNode.Camera.Projection;
      bool isPerspective = projection is PerspectiveProjection;
      float near = projection.Near * NearBias;
      float far = projection.Far * FarBias;
      var biasedProjection = isPerspective
                               ? Matrix44F.CreatePerspectiveOffCenter(
                                 projection.Left, projection.Right,
                                 projection.Bottom, projection.Top,
                                 near, far)
                               : Matrix44F.CreateOrthographicOffCenter(
                                 projection.Left, projection.Right,
                                 projection.Bottom, projection.Top,
                                 near, far);

      var viewport = graphicsDevice.Viewport;
      _parameterViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
      _parameterProjection.SetValue((Matrix)biasedProjection);
      _parameterCameraFar.SetValue(projection.Far);
      _parameterGBuffer0.SetValue(context.GBuffer0);
      _parameterColor.SetValue((Vector4)color);
      _parameterSourceTexture.SetValue(colorTexture);

      _effect.CurrentTechnique = isPerspective ? _techniquePerspective : _techniqueOrthographic;
      _effect.CurrentTechnique.Passes[(colorTexture == null) ? 0 : 1].Apply();

      graphicsDevice.DrawFullScreenQuad();

      graphicsDevice.ResetTextures();

      savedRenderState.Restore();
    }
    #endregion
  }
}
