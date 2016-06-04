// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Creates the shadow map of a <see cref="CubeMapShadow"/>.
  /// </summary>
  /// <inheritdoc cref="ShadowMapRenderer"/>
  internal class CubeMapShadowMapRenderer : SceneNodeRenderer, IShadowMapRenderer
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    private static readonly CubeMapFace[] CubeMapFaces =
    { 
      CubeMapFace.PositiveX, CubeMapFace.NegativeX, 
      CubeMapFace.PositiveY, CubeMapFace.NegativeY,
      CubeMapFace.PositiveZ, CubeMapFace.NegativeZ 
    };

    // Note: Cube map faces are left-handed! Therefore +Z is actually -Z.
    private static readonly Vector3F[] CubeMapForwardVectors =
    { 
      Vector3F.UnitX, -Vector3F.UnitX, 
      Vector3F.UnitY, -Vector3F.UnitY,
      -Vector3F.UnitZ, Vector3F.UnitZ   // Switch Z because cube maps are left handed
    };

    private static readonly Vector3F[] CubeMapUpVectors =
    { 
      Vector3F.UnitY, Vector3F.UnitY,
      Vector3F.UnitZ, -Vector3F.UnitZ,
      Vector3F.UnitY, Vector3F.UnitY
    };


    // Boxed integers to avoid allocation.
    internal static object[] BoxedIntegers = { 0, 1, 2, 3, 4, 5 };
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly CameraNode _perspectiveCameraNode;
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
    /// Initializes a new instance of the <see cref="CubeMapShadowMapRenderer"/> class.
    /// </summary>
    /// <param name="renderCallback">
    /// The method which renders the scene into the shadow map. Must not be <see langword="null"/>. 
    /// See <see cref="RenderCallback"/> for more information.
    /// </param>
    public CubeMapShadowMapRenderer(Func<RenderContext, bool> renderCallback)
    {
      if (renderCallback == null)
        throw new ArgumentNullException("renderCallback");

      RenderCallback = renderCallback;
      _perspectiveCameraNode = new CameraNode(new Camera(new PerspectiveProjection()));
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var lightNode = node as LightNode;
      return lightNode != null && lightNode.Shadow is CubeMapShadow;
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

      context.ThrowIfCameraMissing();
      context.ThrowIfSceneMissing();

      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;
      var originalReferenceNode = context.ReferenceNode;

      var cameraNode = context.CameraNode;

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      // The scene node renderer should use the light camera instead of the player camera.
      context.CameraNode = _perspectiveCameraNode;
      context.Technique = "Omnidirectional";

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var renderTargetPool = graphicsService.RenderTargetPool;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var shadow = lightNode.Shadow as CubeMapShadow;
        if (shadow == null)
          continue;

        var light = lightNode.Light as PointLight;
        if (light == null)
          throw new GraphicsException("CubeMapShadow can only be used with a PointLight.");

        // LightNode is visible in current frame.
        lightNode.LastFrame = frame;

        if (shadow.ShadowMap == null)
        {
          shadow.ShadowMap = renderTargetPool.ObtainCube(
            new RenderTargetFormat(
              shadow.PreferredSize, 
              null,
              false,
              shadow.Prefer16Bit ? SurfaceFormat.HalfSingle : SurfaceFormat.Single,
              DepthFormat.Depth24));
        }

        ((PerspectiveProjection)_perspectiveCameraNode.Camera.Projection).SetFieldOfView(
          ConstantsF.PiOver2, 1, shadow.Near, light.Range);

        // World units per texel at a planar distance of 1 world unit.
        float unitsPerTexel = _perspectiveCameraNode.Camera.Projection.Width / (shadow.ShadowMap.Size * shadow.Near);

        // Convert depth bias from "texel" to  world space.
        // Minus to move receiver closer to light.
        shadow.EffectiveDepthBias = -shadow.DepthBias * unitsPerTexel;

        // Convert normal offset from "texel" to world space.
        shadow.EffectiveNormalOffset = shadow.NormalOffset * unitsPerTexel;

        var pose = lightNode.PoseWorld;

        context.ReferenceNode = lightNode;
        context.Object = shadow;

        bool shadowMapContainsSomething = false;
        for (int side = 0; side < 6; side++)
        {
          context.Data[RenderContextKeys.ShadowTileIndex] = BoxedIntegers[side];

          graphicsDevice.SetRenderTarget(shadow.ShadowMap, CubeMapFaces[side]);
          // context.RenderTarget = shadow.ShadowMap;   // TODO: Support cube maps targets in the render context.
          context.Viewport = graphicsDevice.Viewport;

          graphicsDevice.Clear(Color.White);

          _perspectiveCameraNode.View = Matrix44F.CreateLookAt(
            pose.Position,
            pose.ToWorldPosition(CubeMapForwardVectors[side]),
            pose.ToWorldDirection(CubeMapUpVectors[side]));

          // Abort if this cube map frustum does not touch the camera frustum.
          if (!context.Scene.HaveContact(cameraNode, _perspectiveCameraNode))
            continue;

          graphicsDevice.DepthStencilState = DepthStencilState.Default;
          graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
          graphicsDevice.BlendState = BlendState.Opaque;

          shadowMapContainsSomething |= RenderCallback(context);
        }

        // Recycle shadow map if empty.
        if (!shadowMapContainsSomething)
        {
          renderTargetPool.Recycle(shadow.ShadowMap);
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
      context.Data[RenderContextKeys.ShadowTileIndex] = null;
    }
    #endregion
  }
}
