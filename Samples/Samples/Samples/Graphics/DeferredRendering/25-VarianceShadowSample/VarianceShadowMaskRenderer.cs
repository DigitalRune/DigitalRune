#if !WP7
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Creates the shadow mask from the shadow map of a light node with a
  /// <see cref="VarianceShadow"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The shadow mask is an image as seen from the camera where for each pixel the shadow info is
  /// stored. A value of 0 means the pixel is in the shadow. A value of 1 means the pixel is fully
  /// lit. (The shadow mask is rendered into the current render target.)
  /// </para>
  /// </remarks>
  internal class VarianceShadowMaskRenderer : SceneNodeRenderer
  {
    // This renderer uses the effect VarianceShadowMask.fx.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Vector3[] _frustumFarCorners = new Vector3[4];

    private readonly Effect _effect;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterFrustumCorners;
    private readonly EffectParameter _parameterGBuffer0;
    private readonly EffectParameter _parameterShadowMatrix;
    private readonly EffectParameter _parameterShadowMap;
    private readonly EffectParameter _parameterParameters0;
    private readonly EffectParameter _parameterParameters1;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="VarianceShadowMaskRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public VarianceShadowMaskRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _effect = graphicsService.Content.Load<Effect>("VarianceShadowMask");
      _parameterViewportSize = _effect.Parameters["ViewportSize"];
      _parameterFrustumCorners = _effect.Parameters["FrustumCorners"];
      _parameterGBuffer0 = _effect.Parameters["GBuffer0"];
      _parameterShadowMatrix = _effect.Parameters["ShadowMatrix"];
      _parameterShadowMap = _effect.Parameters["ShadowMap"];
      _parameterParameters0 = _effect.Parameters["Parameters0"];
      _parameterParameters1 = _effect.Parameters["Parameters1"];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var lightNode = node as LightNode;
      return lightNode != null && lightNode.Shadow is VarianceShadow;
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

      Debug.Assert(context.CameraNode != null, "A camera node has to be set in the render context.");
      Debug.Assert(context.Scene != null, "A scene has to be set in the render context.");

      var graphicsDevice = _effect.GraphicsDevice;

      var cameraNode = context.CameraNode;
      var cameraPose = cameraNode.PoseWorld;
      var viewInverse = (Matrix)cameraPose;

      var originalBlendState = graphicsDevice.BlendState;
      var originalDepthStencilState = graphicsDevice.DepthStencilState;
      var originalRasterizerState = graphicsDevice.RasterizerState;

      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;

      Viewport viewport = context.Viewport;
      _parameterViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));

      GraphicsHelper.GetFrustumFarCorners(cameraNode.Camera.Projection, _frustumFarCorners);
      _parameterFrustumCorners.SetValue(_frustumFarCorners);

      _parameterGBuffer0.SetValue(context.GBuffer0);

      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var shadow = lightNode.Shadow as VarianceShadow;
        if (shadow == null)
          continue;

        if (shadow.ShadowMap == null || shadow.ShadowMask == null)
          continue;

        // The effect must only render into a specific channel.
        // Do not change blend state if the correct write channels is already set, e.g. if this 
        // shadow is part of a CompositeShadow, the correct blend state is already set.
        if ((int)graphicsDevice.BlendState.ColorWriteChannels != (1 << shadow.ShadowMaskChannel))
        {
          switch (shadow.ShadowMaskChannel)
          {
            case 0:
              graphicsDevice.BlendState = GraphicsHelper.BlendStateWriteRed;
              break;
            case 1:
              graphicsDevice.BlendState = GraphicsHelper.BlendStateWriteGreen;
              break;
            case 2:
              graphicsDevice.BlendState = GraphicsHelper.BlendStateWriteBlue;
              break;
            case 3:
              graphicsDevice.BlendState = GraphicsHelper.BlendStateWriteAlpha;
              break;
          }
        }

        _parameterShadowMatrix.SetValue(viewInverse * shadow.ViewProjection);
        _parameterShadowMap.SetValue(shadow.ShadowMap);
        _parameterParameters0.SetValue(new Vector4(
          shadow.ShadowMap.Width,
          shadow.ShadowMap.Height,
          shadow.TargetArea.HasValue ? 0 : shadow.MaxDistance,
          0));
        _parameterParameters1.SetValue(new Vector4(
          shadow.FadeOutRange,
          shadow.MinVariance,
          shadow.LightBleedingReduction,
          shadow.ShadowFog));

        _effect.CurrentTechnique.Passes[0].Apply();

        graphicsDevice.DrawFullScreenQuad();
      }

      graphicsDevice.BlendState = originalBlendState;
      graphicsDevice.DepthStencilState = originalDepthStencilState;
      graphicsDevice.RasterizerState = originalRasterizerState;
    }
    #endregion
  }
}
#endif
