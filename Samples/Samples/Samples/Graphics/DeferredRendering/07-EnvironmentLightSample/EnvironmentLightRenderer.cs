#if !WP7 && !WP8
using System;
using System.Collections.Generic;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // Renders EnvironmentLights into the light buffer.
  internal class EnvironmentLightRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Vector3[] _cameraFrustumFarCorners = new Vector3[4];

    private readonly Effect _effect;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterFrustumCorners;
    private readonly EffectParameter _parameterDiffuseColor;
    private readonly EffectParameter _parameterSpecularColor;
    private readonly EffectParameter _parameterTextureSize;
    private readonly EffectParameter _parameterMaxMipLevel;
    private readonly EffectParameter _parameterTexture;
    private readonly EffectParameter _parameterGBuffer0;
    private readonly EffectParameter _parameterGBuffer1;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentLightRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public EnvironmentLightRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _effect = graphicsService.Content.Load<Effect>("EnvironmentLight");
      _parameterViewportSize = _effect.Parameters["ViewportSize"];
      _parameterFrustumCorners = _effect.Parameters["FrustumCorners"];
      _parameterDiffuseColor = _effect.Parameters["DiffuseColor"];
      _parameterSpecularColor = _effect.Parameters["SpecularColor"];
      _parameterTextureSize = _effect.Parameters["TextureSize"];
      _parameterMaxMipLevel = _effect.Parameters["MaxMipLevel"];
      _parameterTexture = _effect.Parameters["EnvironmentMap"];
      _parameterGBuffer0 = _effect.Parameters["GBuffer0"];
      _parameterGBuffer1 = _effect.Parameters["GBuffer1"];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Returns true if node is a LightNode with an EnvironmentLight.
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var lightNode = node as LightNode;
      return lightNode != null && lightNode.Light is EnvironmentLight;
    }


    // Renders a fullscreen quad for each EnvironmentLight. All lights are accumulated in the
    // light buffer using additive alpha blending.
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      if (context.CameraNode == null)
        throw new GraphicsException("Camera node needs to be set in render context.");

      var graphicsDevice = _effect.GraphicsDevice;
      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.BlendState = GraphicsHelper.BlendStateAdd;

      var viewport = graphicsDevice.Viewport;
      _parameterViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
      _parameterGBuffer0.SetValue(context.GBuffer0);
      _parameterGBuffer1.SetValue(context.GBuffer1);

      var cameraNode = context.CameraNode;

      int frame = context.GraphicsService.Frame;
      cameraNode.LastFrame = frame;

      // Frustum corners are vectors which point from the camera to the far plane corners.
      GraphicsHelper.GetFrustumFarCorners(cameraNode.Camera.Projection, _cameraFrustumFarCorners);
      // Convert frustum far corners from view space to world space.
      for (int j = 0; j < _cameraFrustumFarCorners.Length; j++)
        _cameraFrustumFarCorners[j] = (Vector3)cameraNode.PoseWorld.ToWorldDirection((Vector3F)_cameraFrustumFarCorners[j]);
      _parameterFrustumCorners.SetValue(_cameraFrustumFarCorners);

      // The current render pipeline is a HDR pipeline if the light buffer is HdrBlendable.
      // (This will practically always be the case.)
      var isHdrEnabled = context.RenderTarget != null && context.RenderTarget.Format == SurfaceFormat.HdrBlendable;

      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var light = lightNode.Light as EnvironmentLight;
        if (light == null || light.EnvironmentMap == null || light.Color == new Vector3F(0)
            || (light.DiffuseIntensity == 0 && light.SpecularIntensity == 0))
          continue;

        lightNode.LastFrame = frame;

        float hdrScale = isHdrEnabled ? light.HdrScale : 1;
        _parameterDiffuseColor.SetValue((Vector3)light.Color * light.DiffuseIntensity * hdrScale);
        _parameterSpecularColor.SetValue((Vector3)light.Color * light.SpecularIntensity * hdrScale);
        _parameterTextureSize.SetValue(light.EnvironmentMap.Size);
        _parameterMaxMipLevel.SetValue(Math.Max(0, light.EnvironmentMap.LevelCount - 1));
        _parameterTexture.SetValue(light.EnvironmentMap);

        _effect.CurrentTechnique.Passes[0].Apply();

        graphicsDevice.DrawFullScreenQuad();
      }
    }
    #endregion
  }
}
#endif