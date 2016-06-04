// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="Spotlight"/>s into the light buffer.
  /// </summary>
  /// <inheritdoc cref="LightRenderer"/>
  internal class SpotlightRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Vector3[] _frustumFarCorners = new Vector3[4];

    private readonly Effect _effect;
    private readonly EffectParameter _parameterWorldViewProjection;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterFrustumCorners;
    private readonly EffectParameter _parameterDiffuseColor;
    private readonly EffectParameter _parameterSpecularColor;
    private readonly EffectParameter _parameterLightDirection;
    private readonly EffectParameter _parameterPosition;
    private readonly EffectParameter _parameterRange;
    private readonly EffectParameter _parameterAttenuation;
    private readonly EffectParameter _parameterCutoffAngle;
    private readonly EffectParameter _parameterFalloffAngle;
    private readonly EffectParameter _parameterTextureMatrix;
    private readonly EffectParameter _parameterTexture;
    private readonly EffectParameter _parameterGBuffer0;
    private readonly EffectParameter _parameterGBuffer1;
    private readonly EffectParameter _parameterShadowMaskChannel;
    private readonly EffectParameter _parameterShadowMask;
    private readonly EffectPass _passClip;
    private readonly EffectPass _passDefault;
    private readonly EffectPass _passShadowed;
    private readonly EffectPass _passTexturedRgb;
    private readonly EffectPass _passTexturedAlpha;
    private readonly EffectPass _passShadowedTexturedRgb;
    private readonly EffectPass _passShadowedTexturedAlpha;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SpotlightRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public SpotlightRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Deferred/Spotlight");
      _parameterWorldViewProjection = _effect.Parameters["WorldViewProjection"];
      _parameterViewportSize = _effect.Parameters["ViewportSize"];
      _parameterFrustumCorners = _effect.Parameters["FrustumCorners"];
      _parameterDiffuseColor = _effect.Parameters["SpotlightDiffuse"];
      _parameterSpecularColor = _effect.Parameters["SpotlightSpecular"];
      _parameterLightDirection = _effect.Parameters["SpotlightDirection"];
      _parameterPosition = _effect.Parameters["SpotlightPosition"];
      _parameterRange = _effect.Parameters["SpotlightRange"];
      _parameterAttenuation = _effect.Parameters["SpotlightAttenuation"];
      _parameterCutoffAngle = _effect.Parameters["SpotlightCutoffAngle"];
      _parameterFalloffAngle = _effect.Parameters["SpotlightFalloffAngle"];
      _parameterTextureMatrix = _effect.Parameters["SpotlightTextureMatrix"];
      _parameterTexture = _effect.Parameters["SpotlightTexture"];
      _parameterGBuffer0 = _effect.Parameters["GBuffer0"];
      _parameterGBuffer1 = _effect.Parameters["GBuffer1"];
      _parameterShadowMaskChannel = _effect.Parameters["ShadowMaskChannel"];
      _parameterShadowMask = _effect.Parameters["ShadowMask"];
      _passClip = _effect.CurrentTechnique.Passes["Clip"];
      _passDefault = _effect.CurrentTechnique.Passes["Default"];
      _passShadowed = _effect.CurrentTechnique.Passes["Shadowed"];
      _passTexturedRgb = _effect.CurrentTechnique.Passes["TexturedRgb"];
      _passTexturedAlpha = _effect.CurrentTechnique.Passes["TexturedAlpha"];
      _passShadowedTexturedRgb = _effect.CurrentTechnique.Passes["ShadowedTexturedRgb"];
      _passShadowedTexturedAlpha = _effect.CurrentTechnique.Passes["ShadowedTexturedAlpha"];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var lightNode = node as LightNode;
      return lightNode != null && lightNode.Light is Spotlight;
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
      var cameraPose = cameraNode.PoseWorld;
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

        var light = lightNode.Light as Spotlight;
        if (light == null)
          continue;

        // LightNode is visible in current frame.
        lightNode.LastFrame = frame;

        float hdrScale = isHdrEnabled ? light.HdrScale : 1;
        _parameterDiffuseColor.SetValue((Vector3)light.Color * light.DiffuseIntensity * hdrScale);
        _parameterSpecularColor.SetValue((Vector3)light.Color * light.SpecularIntensity * hdrScale);

        var lightPose = lightNode.PoseWorld;
        _parameterPosition.SetValue((Vector3)(lightPose.Position - cameraPose.Position));

        var lightDirectionWorld = lightPose.ToWorldDirection(Vector3F.Forward);
        _parameterLightDirection.SetValue((Vector3)lightDirectionWorld);

        _parameterRange.SetValue(light.Range);
        _parameterAttenuation.SetValue(light.Attenuation);
        _parameterCutoffAngle.SetValue(light.CutoffAngle);
        _parameterFalloffAngle.SetValue(Math.Min(light.FalloffAngle, light.CutoffAngle));

        bool hasShadow = (lightNode.Shadow != null && lightNode.Shadow.ShadowMask != null);
        if (hasShadow)
        {
          switch (lightNode.Shadow.ShadowMaskChannel)
          {
            case 0: _parameterShadowMaskChannel.SetValue(new Vector4(1, 0, 0, 0)); break;
            case 1: _parameterShadowMaskChannel.SetValue(new Vector4(0, 1, 0, 0)); break;
            case 2: _parameterShadowMaskChannel.SetValue(new Vector4(0, 0, 1, 0)); break;
            default: _parameterShadowMaskChannel.SetValue(new Vector4(0, 0, 0, 1)); break;
          }

          _parameterShadowMask.SetValue(lightNode.Shadow.ShadowMask);
        }

        bool hasTexture = (light.Texture != null);
        if (hasTexture)
        {
          var projection = Matrix44F.CreatePerspectiveFieldOfView(light.CutoffAngle * 2, 1, 0.1f, 100);
          _parameterTextureMatrix.SetValue((Matrix)(GraphicsHelper.ProjectorBiasMatrix * projection * (lightPose.Inverse * new Pose(cameraPose.Position))));
          _parameterTexture.SetValue(light.Texture);
        }

        var rectangle = GraphicsHelper.GetViewportRectangle(cameraNode, viewport, lightNode);
        var texCoordTopLeft = new Vector2F(rectangle.Left / (float)viewport.Width, rectangle.Top / (float)viewport.Height);
        var texCoordBottomRight = new Vector2F(rectangle.Right / (float)viewport.Width, rectangle.Bottom / (float)viewport.Height);
        GraphicsHelper.GetFrustumFarCorners(cameraNode.Camera.Projection, texCoordTopLeft, texCoordBottomRight, _frustumFarCorners);
        
        // Convert frustum far corners from view space to world space.
        for (int j = 0; j < _frustumFarCorners.Length; j++)
          _frustumFarCorners[j] = (Vector3)cameraPose.ToWorldDirection((Vector3F)_frustumFarCorners[j]);
        _parameterFrustumCorners.SetValue(_frustumFarCorners);

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

        if (hasShadow)
        {
          if (hasTexture)
          {
            if (light.Texture.Format == SurfaceFormat.Alpha8)
              _passShadowedTexturedAlpha.Apply();
            else
              _passShadowedTexturedRgb.Apply();
          }
          else
          {
            _passShadowed.Apply();
          }
        }
        else
        {
          if (hasTexture)
          {
            if (light.Texture.Format == SurfaceFormat.Alpha8)
              _passTexturedAlpha.Apply();
            else
              _passTexturedRgb.Apply();
          }
          else
          {
            _passDefault.Apply();
          }
        }

        graphicsDevice.DrawQuad(rectangle);
      }

      savedRenderState.Restore();
    }
    #endregion
  }
}
#endif
