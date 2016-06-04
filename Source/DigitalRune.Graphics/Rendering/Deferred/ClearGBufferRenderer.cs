// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Clears the G-buffer. 
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Render Targets and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device. The
  /// render target should be the G-buffer.
  /// </para>
  /// </remarks>
  public class ClearGBufferRenderer
  {
    private readonly Effect _effect;
    private readonly EffectParameter _parameterDepth;
    private readonly EffectParameter _parameterNormal;
    private readonly EffectParameter _parameterSpecularPower;


    /// <summary>
    /// Initializes a new instance of the <see cref="ClearGBufferRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public ClearGBufferRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Deferred/ClearGBuffer");
      _parameterDepth = _effect.Parameters["Depth"];
      _parameterNormal = _effect.Parameters["Normal"];
      _parameterSpecularPower = _effect.Parameters["SpecularPower"];
    }


    /// <summary>
    /// Clears the current render target (which must be the G-buffer).
    /// </summary>
    /// <param name="context">The render context.</param>
    public void Render(RenderContext context)
    {
      if (context == null)
        throw new ArgumentNullException("context");

      context.Validate(_effect); 

      var graphicsDevice = _effect.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.BlendState = BlendState.Opaque;

      // Clear to maximum depth.
      _parameterDepth.SetValue(1.0f);

      // The environment is facing the camera.
      // --> Set normal = cameraBackward.
      var cameraNode = context.CameraNode;
      _parameterNormal.SetValue((cameraNode != null) ? (Vector3)cameraNode.ViewInverse.GetColumn(2).XYZ : Vector3.Backward);

      // Clear specular to arbitrary value.
      _parameterSpecularPower.SetValue(1.0f);

      _effect.CurrentTechnique.Passes[0].Apply();

      // Draw full-screen quad using clip space coordinates.
      graphicsDevice.DrawQuad(
        new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)), 
        new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)));

      savedRenderState.Restore();
    }
  }
}
#endif
