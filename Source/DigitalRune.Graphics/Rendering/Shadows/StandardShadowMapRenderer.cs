// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Creates the shadow map of a <see cref="StandardShadow"/>.
  /// </summary>
  /// <inheritdoc cref="ShadowMapRenderer"/>
  internal class StandardShadowMapRenderer : SceneNodeRenderer, IShadowMapRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly CameraNode _perspectiveCameraNode;
    private readonly CameraNode _orthographicCameraNode;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public Func<RenderContext, bool> RenderCallback
    {
      get { return _renderCallback; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _renderCallback = value;
      }
    }
    private Func<RenderContext, bool> _renderCallback;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="StandardShadowMapRenderer"/> class.
    /// </summary>
    /// <param name="renderCallback">
    /// The method which renders the scene into the shadow map. Must not be <see langword="null"/>.
    /// See <see cref="RenderCallback"/> for more information.
    /// </param>
    public StandardShadowMapRenderer(Func<RenderContext, bool> renderCallback)
    {
      if (renderCallback == null)
        throw new ArgumentNullException("renderCallback");

      RenderCallback = renderCallback;
      _perspectiveCameraNode = new CameraNode(new Camera(new PerspectiveProjection()));
      _orthographicCameraNode = new CameraNode(new Camera(new OrthographicProjection()));
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var lightNode = node as LightNode;
      return lightNode != null && lightNode.Shadow is StandardShadow;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      // Note: The camera node is not used by the StandardShadowMapRenderer.
      // Still throw an exception if null for consistency. (All other shadow map
      // renderers need a camera node.)
      context.ThrowIfCameraMissing();
      context.ThrowIfSceneMissing();

      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;
      var originalReferenceNode = context.ReferenceNode;

      var cameraNode = context.CameraNode;

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      context.Technique = "Default";

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var shadow = lightNode.Shadow as StandardShadow;
        if (shadow == null)
          continue;

        // LightNode is visible in current frame.
        lightNode.LastFrame = frame;

        // Get a new shadow map if necessary.
        if (shadow.ShadowMap == null)
        {
          shadow.ShadowMap = graphicsService.RenderTargetPool.Obtain2D(
            new RenderTargetFormat(
              shadow.PreferredSize,
              shadow.PreferredSize,
              false,
              shadow.Prefer16Bit ? SurfaceFormat.HalfSingle : SurfaceFormat.Single,
              DepthFormat.Depth24));
        }

        // Create a suitable shadow camera.
        CameraNode lightCameraNode;
        if (lightNode.Light is ProjectorLight)
        {
          var light = (ProjectorLight)lightNode.Light;
          if (light.Projection is PerspectiveProjection)
          {
            var lp = (PerspectiveProjection)light.Projection;
            var cp = (PerspectiveProjection)_perspectiveCameraNode.Camera.Projection;
            cp.SetOffCenter(lp.Left, lp.Right, lp.Bottom, lp.Top, lp.Near, lp.Far);

            lightCameraNode = _perspectiveCameraNode;
          }
          else //if (light.Projection is OrthographicProjection)
          {
            var lp = (OrthographicProjection)light.Projection;
            var cp = (OrthographicProjection)_orthographicCameraNode.Camera.Projection;
            cp.SetOffCenter(lp.Left, lp.Right, lp.Bottom, lp.Top, lp.Near, lp.Far);

            lightCameraNode = _orthographicCameraNode;
          }
        }
        else if (lightNode.Light is Spotlight)
        {
          var light = (Spotlight)lightNode.Light;
          var cp = (PerspectiveProjection)_perspectiveCameraNode.Camera.Projection;
          cp.SetFieldOfView(2 * light.CutoffAngle, 1, shadow.DefaultNear, light.Range);

          lightCameraNode = _perspectiveCameraNode;
        }
        else
        {
          throw new GraphicsException("StandardShadow can only be used with a Spotlight or a ProjectorLight.");
        }

        lightCameraNode.PoseWorld = lightNode.PoseWorld;

        // Store data for use in StandardShadowMaskRenderer.
        shadow.Near = lightCameraNode.Camera.Projection.Near;
        shadow.Far = lightCameraNode.Camera.Projection.Far;
        shadow.View = lightCameraNode.PoseWorld.Inverse;
        shadow.Projection = lightCameraNode.Camera.Projection;

        // World units per texel at a planar distance of 1 world unit.
        float unitsPerTexel = lightCameraNode.Camera.Projection.Width / (shadow.ShadowMap.Height * shadow.Near);

        // Convert depth bias from "texel" to world space.
        // Minus to move receiver depth closer to light.
        shadow.EffectiveDepthBias = -shadow.DepthBias * unitsPerTexel;

        // Convert normal offset from "texel" to world space.
        shadow.EffectiveNormalOffset = shadow.NormalOffset * unitsPerTexel;

        graphicsDevice.SetRenderTarget(shadow.ShadowMap);
        context.RenderTarget = shadow.ShadowMap;
        context.Viewport = graphicsDevice.Viewport;

        graphicsDevice.Clear(Color.White);

        // The scene node renderer should use the light camera instead of the player camera.
        context.CameraNode = lightCameraNode;
        context.ReferenceNode = lightNode;
        context.Object = shadow;

        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        graphicsDevice.BlendState = BlendState.Opaque;

        bool shadowMapContainsSomething = RenderCallback(context);
        if (!shadowMapContainsSomething)
        {
          // Shadow map is empty. Recycle it.
          graphicsService.RenderTargetPool.Recycle(shadow.ShadowMap);
          shadow.ShadowMap = null;
        }
      }

      graphicsDevice.SetRenderTarget(null);
      savedRenderState.Restore();

      context.CameraNode = cameraNode;
      context.Technique = null;
      context.RenderTarget = originalRenderTarget;
      context.Viewport = originalViewport;
      context.ReferenceNode = originalReferenceNode;
      context.Object = null;
    }
    #endregion
  }
}
