// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="AmbientLight"/>s into the light buffer.
  /// </summary>
  /// <inheritdoc cref="LightRenderer"/>
  internal class AmbientLightRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _parameterWorldViewProjection;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterLightColor;
    private readonly EffectParameter _parameterHemisphericAttenuation;
    private readonly EffectParameter _parameterUp;
    private readonly EffectParameter _parameterGBuffer0;
    private readonly EffectParameter _parameterGBuffer1;
    private readonly EffectPass _passClip;
    private readonly EffectPass _passLight;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbientLightRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public AmbientLightRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Deferred/AmbientLight");
      _parameterWorldViewProjection = _effect.Parameters["WorldViewProjection"];
      _parameterViewportSize = _effect.Parameters["ViewportSize"];
      _parameterLightColor = _effect.Parameters["AmbientLight"];
      _parameterHemisphericAttenuation = _effect.Parameters["AmbientLightAttenuation"];
      _parameterUp = _effect.Parameters["AmbientLightUp"];
      _parameterGBuffer0 = _effect.Parameters["GBuffer0"];
      _parameterGBuffer1 = _effect.Parameters["GBuffer1"];
      _passClip = _effect.CurrentTechnique.Passes["Clip"];
      _passLight = _effect.CurrentTechnique.Passes["Light"];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var lightNode = node as LightNode;
      return lightNode != null && lightNode.Light is AmbientLight;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      context.Validate(_effect);
      context.ThrowIfCameraMissing();

      var graphicsDevice = _effect.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.BlendState = GraphicsHelper.BlendStateAdd;

      var viewport = graphicsDevice.Viewport;
      _parameterViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
      _parameterGBuffer0.SetValue(context.GBuffer0);
      _parameterGBuffer1.SetValue(context.GBuffer1);

      var cameraNode = context.CameraNode;
      Matrix viewProjection = (Matrix)cameraNode.View * cameraNode.Camera.Projection;

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      var isHdrEnabled = context.IsHdrEnabled();

      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var light = lightNode.Light as AmbientLight;
        if (light == null)
          continue;

        // LightNode is visible in current frame.
        lightNode.LastFrame = frame;
        
        float hdrScale = isHdrEnabled ? light.HdrScale : 1;
        _parameterLightColor.SetValue((Vector3)light.Color * light.Intensity * hdrScale);
        _parameterHemisphericAttenuation.SetValue(light.HemisphericAttenuation);

        Vector3F upWorld = lightNode.PoseWorld.ToWorldDirection(Vector3F.Up);
        _parameterUp.SetValue((Vector3)upWorld);

        if (lightNode.Clip != null)
        {
          var data = lightNode.RenderData as LightRenderData;
          if (data == null)
          {
            data = new LightRenderData();
            lightNode.RenderData = data;
          }

          data.UpdateClipSubmesh(context.GraphicsService, lightNode);

          graphicsDevice.DepthStencilState = GraphicsHelper.DepthStencilStateOnePassStencilFail;
          graphicsDevice.BlendState = GraphicsHelper.BlendStateNoColorWrite;

          _parameterWorldViewProjection.SetValue((Matrix)data.ClipMatrix * viewProjection);
          _passClip.Apply();
          data.ClipSubmesh.Draw();

          graphicsDevice.DepthStencilState = lightNode.InvertClip
            ? GraphicsHelper.DepthStencilStateStencilEqual0
            : GraphicsHelper.DepthStencilStateStencilNotEqual0;
          graphicsDevice.BlendState = GraphicsHelper.BlendStateAdd;
        }
        else
        {
          graphicsDevice.DepthStencilState = DepthStencilState.None;
        }

        _passLight.Apply();
        graphicsDevice.DrawFullScreenQuad();
      }

      savedRenderState.Restore();
    }
    #endregion
  }
}
#endif
