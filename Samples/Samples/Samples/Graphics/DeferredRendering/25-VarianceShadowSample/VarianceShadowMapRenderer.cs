using System;
using System.Collections.Generic;
using System.Diagnostics;
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
  /// Creates the shadow map of a <see cref="VarianceShadow"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Render Callback:</strong><br/>
  /// The shadow map renderer requires a <see cref="RenderCallback"/> method to render the scene.
  /// The callback method needs to render the scene using the camera and the information given in
  /// the <see cref="RenderContext"/>. 
  /// </para>
  /// </remarks>
  internal class VarianceShadowMapRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // A cached array which is reused in GetBoundingSphere().
    private readonly Vector3F[] _frustumCorners = new Vector3F[8];

    private readonly PerspectiveViewVolume _cameraVolume;
    private readonly CameraNode _orthographicCameraNode;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the method which renders the scene into the shadow map.
    /// </summary>
    /// <value>
    /// The callback method that renders the scene into the shadow map.
    /// Must not be <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// The render callback renders the scene for the shadow map using the camera and the
    /// information currently set in the render context. It returns <see langword="true"/> if
    /// any objects were rendered and <see langword="false"/> if no objects were rendered.
    /// </remarks>
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
    /// Initializes a new instance of the <see cref="VarianceShadowMapRenderer"/> class.
    /// </summary>
    /// <param name="render">
    /// The method which renders the scene into the shadow map. Must not be <see langword="null"/>. 
    /// See <see cref="RenderCallback"/> for more information.
    /// </param>
    public VarianceShadowMapRenderer(Func<RenderContext, bool> render)
    {
      if (render == null)
        throw new ArgumentNullException("render");

      RenderCallback = render;
      _cameraVolume = new PerspectiveViewVolume();
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

      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;
      var originalReferenceNode = context.ReferenceNode;

      // Camera properties
      var cameraNode = context.CameraNode;
      var cameraPose = cameraNode.PoseWorld;
      var projection = cameraNode.Camera.Projection;
      if (!(projection is PerspectiveProjection))
        throw new NotImplementedException("VSM shadow maps not yet implemented for scenes with perspective camera.");

      float fieldOfViewY = projection.FieldOfViewY;
      float aspectRatio = projection.AspectRatio;

      // Update SceneNode.LastFrame for all rendered nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      // The scene node renderer should use the light camera instead of the player camera.
      context.CameraNode = _orthographicCameraNode;

      // The shadow map is rendered using the technique "DirectionalVsm".
      // See ShadowMap.fx in the DigitalRune source code folder.
      context.Technique = "DirectionalVsm";

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var originalBlendState = graphicsDevice.BlendState;
      var originalDepthStencilState = graphicsDevice.DepthStencilState;
      var originalRasterizerState = graphicsDevice.RasterizerState;

      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var shadow = lightNode.Shadow as VarianceShadow;
        if (shadow == null)
          continue;

        // LightNode is visible in current frame.
        lightNode.LastFrame = frame;

        // The format of the shadow map:
        var format = new RenderTargetFormat(
          shadow.PreferredSize,
          shadow.PreferredSize,
          false,
          shadow.Prefer16Bit ? SurfaceFormat.HalfVector2 : SurfaceFormat.Vector2,  // VSM needs two channels!
          DepthFormat.Depth24);

        if (shadow.ShadowMap != null && shadow.IsLocked)
          continue;

        if (shadow.ShadowMap == null)
          shadow.ShadowMap = graphicsService.RenderTargetPool.Obtain2D(format);

        graphicsDevice.DepthStencilState = DepthStencilState.Default;
        graphicsDevice.BlendState = BlendState.Opaque;

        // Render front and back faces for VSM due to low shadow map texel density.
        // (VSM is usually used for distant geometry.)
        graphicsDevice.RasterizerState = RasterizerState.CullNone;

        graphicsDevice.SetRenderTarget(shadow.ShadowMap);
        context.RenderTarget = shadow.ShadowMap;
        context.Viewport = graphicsDevice.Viewport;

        graphicsDevice.Clear(Color.White);

        // Compute an orthographic camera for the light.
        // If Shadow.TargetArea is null, the shadow map should cover the area in front of the player camera.
        // If Shadow.TargetArea is set, the shadow map should cover this static area.
        if (shadow.TargetArea == null)
        {
          // near/far of this shadowed area.
          float near = projection.Near;
          float far = shadow.MaxDistance;

          // Abort if near-far distances are invalid.
          if (Numeric.IsGreaterOrEqual(near, far))
            continue;

          // Create a view volume for frustum part that is covered by the shadow map.
          _cameraVolume.SetFieldOfView(fieldOfViewY, aspectRatio, near, far);

          // Find the bounding sphere of the frustum part.
          Vector3F center;
          float radius;
          GetBoundingSphere(_cameraVolume, out center, out radius);

          // Convert center to light space.
          Pose lightPose = lightNode.PoseWorld;
          center = cameraPose.ToWorldPosition(center);
          center = lightPose.ToLocalPosition(center);

          // Snap center to texel positions to avoid shadow swimming.
          SnapPositionToTexels(ref center, 2 * radius, shadow.ShadowMap.Height);

          // Convert center back to world space.
          center = lightPose.ToWorldPosition(center);

          Matrix33F orientation = lightPose.Orientation;
          Vector3F backward = orientation.GetColumn(2);
          var orthographicProjection = (OrthographicProjection)_orthographicCameraNode.Camera.Projection;

          // Create a tight orthographic frustum around the target bounding sphere.
          orthographicProjection.SetOffCenter(-radius, radius, -radius, radius, 0, 2 * radius);
          Vector3F cameraPosition = center + radius * backward;
          Pose frustumPose = new Pose(cameraPosition, orientation);
          Pose view = frustumPose.Inverse;
          shadow.ViewProjection = (Matrix)view * orthographicProjection;

          // For rendering the shadow map, move near plane back by MinLightDistance
          // to catch occluders in front of the camera frustum.
          orthographicProjection.Near = -shadow.MinLightDistance;
          _orthographicCameraNode.PoseWorld = frustumPose;
        }
        else
        {
          // Get bounding sphere of static target area.
          Aabb targetAabb = shadow.TargetArea.Value;
          Vector3F center = targetAabb.Center;
          float radius = (targetAabb.Maximum - center).Length;

          // Convert center to light space.
          Matrix33F orientation = lightNode.PoseWorld.Orientation;
          Vector3F backward = orientation.GetColumn(2);
          var orthographicProjection = (OrthographicProjection)_orthographicCameraNode.Camera.Projection;

          // Create a tight orthographic frustum around the target bounding sphere.
          orthographicProjection.SetOffCenter(-radius, radius, -radius, radius, 0, 2 * radius);
          Vector3F cameraPosition = center + radius * backward;
          Pose frustumPose = new Pose(cameraPosition, orientation);
          Pose view = frustumPose.Inverse;
          shadow.ViewProjection = (Matrix)view * orthographicProjection;

          // For rendering the shadow map, move near plane back by MinLightDistance
          // to catch occluders in front of the camera frustum.
          orthographicProjection.Near = -shadow.MinLightDistance;
          _orthographicCameraNode.PoseWorld = frustumPose;
        }

        context.ReferenceNode = lightNode;
        context.Object = shadow;

        // Render objects into shadow map.
        bool shadowMapContainsSomething = RenderCallback(context);

        if (shadowMapContainsSomething)
        {
          // Blur shadow map.
          if (shadow.Filter != null && shadow.Filter.Scale > 0)
          {
            context.SourceTexture = shadow.ShadowMap;
            shadow.Filter.Process(context);
            context.SourceTexture = null;
          }
        }
        else
        {
          // Shadow map is empty. Recycle it.
          graphicsService.RenderTargetPool.Recycle(shadow.ShadowMap);
          shadow.ShadowMap = null;
        }
      }

      graphicsDevice.SetRenderTarget(null);

      graphicsDevice.BlendState = originalBlendState;
      graphicsDevice.DepthStencilState = originalDepthStencilState;
      graphicsDevice.RasterizerState = originalRasterizerState;

      context.CameraNode = cameraNode;
      context.Technique = null;
      context.RenderTarget = originalRenderTarget;
      context.Viewport = originalViewport;
      context.ReferenceNode = originalReferenceNode;
      context.Object = null;
    }


    // Computes the bounding sphere of the given view frustum.
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


    // Clamps the given position to shadow map texels to avoid "shadow swimming".
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
