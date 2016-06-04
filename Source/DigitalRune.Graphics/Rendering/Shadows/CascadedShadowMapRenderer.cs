// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Creates the shadow map of a <see cref="CascadedShadow"/>.
  /// </summary>
  /// <inheritdoc cref="ShadowMapRenderer"/>
  internal class CascadedShadowMapRenderer : SceneNodeRenderer, IShadowMapRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // A cached array which is reused in GetBoundingSphere().
    private readonly Vector3F[] _frustumCorners = new Vector3F[8];

    // The near and far limits of all shadow maps.
    private readonly float[] _csmSplitDistances = new float[5];

    private readonly PerspectiveViewVolume _splitVolume;
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
    /// Initializes a new instance of the <see cref="CascadedShadowMapRenderer"/> class.
    /// </summary>
    /// <param name="renderCallback">
    /// The method which renders the scene into the shadow map. Must not be <see langword="null"/>. 
    /// See <see cref="RenderCallback"/> for more information.
    /// </param>
    public CascadedShadowMapRenderer(Func<RenderContext, bool> renderCallback)
    {
      if (renderCallback == null)
        throw new ArgumentNullException("renderCallback");

      RenderCallback = renderCallback;
      _splitVolume = new PerspectiveViewVolume();
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
      return lightNode != null && lightNode.Shadow is CascadedShadow;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
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

      // Camera properties
      var cameraNode = context.CameraNode;
      var cameraPose = cameraNode.PoseWorld;
      var projection = cameraNode.Camera.Projection;
      if (!(projection is PerspectiveProjection))
        throw new NotImplementedException(
          "Cascaded shadow maps not yet implemented for scenes with orthographic camera.");

      float fieldOfViewY = projection.FieldOfViewY;
      float aspectRatio = projection.AspectRatio;

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      // The scene node renderer should use the light camera instead of the player camera.
      context.CameraNode = _orthographicCameraNode;
      context.Technique = "Directional";

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var shadow = lightNode.Shadow as CascadedShadow;
        if (shadow == null)
          continue;

        // LightNode is visible in current frame.
        lightNode.LastFrame = frame;

        var format = new RenderTargetFormat(
          shadow.PreferredSize * shadow.NumberOfCascades,
          shadow.PreferredSize,
          false,
          shadow.Prefer16Bit ? SurfaceFormat.HalfSingle : SurfaceFormat.Single,
          DepthFormat.Depth24);

        bool allLocked = shadow.IsCascadeLocked[0] && shadow.IsCascadeLocked[1] && shadow.IsCascadeLocked[2] && shadow.IsCascadeLocked[3];

        if (shadow.ShadowMap == null)
        {
          shadow.ShadowMap = graphicsService.RenderTargetPool.Obtain2D(format);
          allLocked = false;   // Need to render shadow map.
        }

        // If we can reuse the whole shadow map texture, abort early.
        if (allLocked)
          continue;

        _csmSplitDistances[0] = projection.Near;
        _csmSplitDistances[1] = shadow.Distances.X;
        _csmSplitDistances[2] = shadow.Distances.Y;
        _csmSplitDistances[3] = shadow.Distances.Z;
        _csmSplitDistances[4] = shadow.Distances.W;

        // (Re-)Initialize the array for cached matrices in the CascadedShadow.
        if (shadow.ViewProjections == null || shadow.ViewProjections.Length < shadow.NumberOfCascades)
          shadow.ViewProjections = new Matrix[shadow.NumberOfCascades];

        // Initialize the projection matrices to an empty matrix.
        // The unused matrices should not contain valid projections because 
        // CsmComputeSplitOptimized in CascadedShadowMask.fxh should not choose 
        // the wrong cascade.
        for (int j = 0; j < shadow.ViewProjections.Length; j++)
        {
          if (!shadow.IsCascadeLocked[j])    // Do not delete cached info for cached cascade.
            shadow.ViewProjections[j] = new Matrix();
        }

        // If some cascades are cached, we have to create a new shadow map and copy
        // the old cascades into the new shadow map.
        if (shadow.IsCascadeLocked[0] || shadow.IsCascadeLocked[1] || shadow.IsCascadeLocked[2] || shadow.IsCascadeLocked[3])
        {
          var oldShadowMap = shadow.ShadowMap;
          shadow.ShadowMap = graphicsService.RenderTargetPool.Obtain2D(new RenderTargetFormat(oldShadowMap));

          graphicsDevice.SetRenderTarget(shadow.ShadowMap);
          graphicsDevice.Clear(Color.White);

          var spriteBatch = graphicsService.GetSpriteBatch();
          spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
          for (int cascade = 0; cascade < shadow.NumberOfCascades; cascade++)
          {
            if (shadow.IsCascadeLocked[cascade])
            {
              var viewport = GetViewport(shadow, cascade);
              var rectangle = new Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height);
              spriteBatch.Draw(oldShadowMap, rectangle, rectangle, Color.White);
            }
          }
          spriteBatch.End();

          graphicsService.RenderTargetPool.Recycle(oldShadowMap);
        }
        else
        {
          graphicsDevice.SetRenderTarget(shadow.ShadowMap);
          graphicsDevice.Clear(Color.White);
        }

        context.RenderTarget = shadow.ShadowMap;
        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
        graphicsDevice.BlendState = BlendState.Opaque;

        context.ReferenceNode = lightNode;
        context.Object = shadow;
        context.ShadowNear = 0;           // Obsolete: Only kept for backward compatibility.

        bool shadowMapContainsSomething = false;
        for (int split = 0; split < shadow.NumberOfCascades; split++)
        {
          if (shadow.IsCascadeLocked[split])
            continue;

          context.Data[RenderContextKeys.ShadowTileIndex] = CubeMapShadowMapRenderer.BoxedIntegers[split];

          // near/far of this split.
          float near = _csmSplitDistances[split];
          float far = Math.Max(_csmSplitDistances[split + 1], near + Numeric.EpsilonF);

          // Create a view volume for this split.
          _splitVolume.SetFieldOfView(fieldOfViewY, aspectRatio, near, far);

          // Find the bounding sphere of the split camera frustum.
          Vector3F center;
          float radius;
          GetBoundingSphere(_splitVolume, out center, out radius);

          // Extend radius to get enough border for filtering.
          int shadowMapSize = shadow.ShadowMap.Height;

          // We could extend by (ShadowMapSize + BorderTexels) / ShadowMapSize;
          // Add at least 1 texel. (This way, shadow mask shader can clamp uv to 
          // texture rect in without considering half texel border to avoid sampling outside..)
          radius *= (float)(shadowMapSize + 1) / shadowMapSize;

          // Convert center to light space.
          Pose lightPose = lightNode.PoseWorld;
          center = cameraPose.ToWorldPosition(center);
          center = lightPose.ToLocalPosition(center);

          // Snap center to texel positions to avoid shadow swimming.
          SnapPositionToTexels(ref center, 2 * radius, shadowMapSize);

          // Convert center back to world space.
          center = lightPose.ToWorldPosition(center);

          Matrix33F orientation = lightPose.Orientation;
          Vector3F backward = orientation.GetColumn(2);
          var orthographicProjection = (OrthographicProjection)_orthographicCameraNode.Camera.Projection;

          // Create a tight orthographic frustum around the cascade's bounding sphere.
          orthographicProjection.SetOffCenter(-radius, radius, -radius, radius, 0, 2 * radius);
          Vector3F cameraPosition = center + radius * backward;
          Pose frustumPose = new Pose(cameraPosition, orientation);
          Pose view = frustumPose.Inverse;
          shadow.ViewProjections[split] = (Matrix)view * orthographicProjection;
          
          // Convert depth bias from "texel" to light space [0, 1] depth.
          // Minus sign to move receiver depth closer to light. Divide by depth to normalize.
          float unitsPerTexel = orthographicProjection.Width / shadow.ShadowMap.Height;
          shadow.EffectiveDepthBias[split] = -shadow.DepthBias[split] * unitsPerTexel / orthographicProjection.Depth;

          // Convert normal offset from "texel" to world space.
          shadow.EffectiveNormalOffset[split] = shadow.NormalOffset[split] * unitsPerTexel;

          // For rendering the shadow map, move near plane back by MinLightDistance 
          // to catch occluders in front of the cascade.
          orthographicProjection.Near = -shadow.MinLightDistance;
          _orthographicCameraNode.PoseWorld = frustumPose;
          
          // Set a viewport to render a tile in the texture atlas.
          graphicsDevice.Viewport = GetViewport(shadow, split);
          context.Viewport = graphicsDevice.Viewport;

          shadowMapContainsSomething |= RenderCallback(context);
        }

        // Recycle shadow map if empty.
        if (!shadowMapContainsSomething)
        {
          graphicsService.RenderTargetPool.Recycle(shadow.ShadowMap);
          shadow.ShadowMap = null;
        }
      }

      graphicsDevice.SetRenderTarget(null);
      savedRenderState.Restore();

      context.CameraNode = cameraNode;
      context.ShadowNear = float.NaN;
      context.Technique = null;
      context.RenderTarget = originalRenderTarget;
      context.Viewport = originalViewport;
      context.ReferenceNode = originalReferenceNode;
      context.Object = null;
      context.Data[RenderContextKeys.ShadowTileIndex] = null;
    }


    private static Viewport GetViewport(CascadedShadow shadow, int split)
    {
      return new Viewport
      {
        X = split * shadow.ShadowMap.Width / shadow.NumberOfCascades,
        Y = 0,
        Width = shadow.ShadowMap.Width / shadow.NumberOfCascades,
        Height = shadow.ShadowMap.Height,
        MinDepth = 0,
        MaxDepth = 1,
      };
    }


    private void GetBoundingSphere(ViewVolume viewVolume, out Vector3F center, out float radius)
    {
      float left = viewVolume.Left;
      float top = viewVolume.Top;
      float right = viewVolume.Right;
      float bottom = viewVolume.Bottom;
      float near = viewVolume.Near;
      float far = viewVolume.Far;

      _frustumCorners[0] = new Vector3F(left, top, -near);
      _frustumCorners[1] = new Vector3F(right, top, -near);
      _frustumCorners[2] = new Vector3F(left, bottom, -near);
      _frustumCorners[3] = new Vector3F(right, bottom, -near);

      float farOverNear = far / near;
      left *= farOverNear;
      top *= farOverNear;
      right *= farOverNear;
      bottom *= farOverNear;

      _frustumCorners[4] = new Vector3F(left, top, -far);
      _frustumCorners[5] = new Vector3F(right, top, -far);
      _frustumCorners[6] = new Vector3F(left, bottom, -far);
      _frustumCorners[7] = new Vector3F(right, bottom, -far);

      GeometryHelper.ComputeBoundingSphere(_frustumCorners, out radius, out center);
    }


    private static void SnapPositionToTexels(ref Vector3F position, float sizeWorld, int sizeTexels)
    {
      // The size of one texel in world units.
      float texelSize = sizeWorld / sizeTexels;

      // Clamp the position to the texel size.
      position.X = (float)Math.Ceiling(position.X / texelSize) * texelSize;
      position.Y = (float)Math.Ceiling(position.Y / texelSize) * texelSize;
      position.Z = (float)Math.Ceiling(position.Z / texelSize) * texelSize;
    }
    #endregion
  }
}
