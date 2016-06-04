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
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders reflection images for the <see cref="PlanarReflectionNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="PlanarReflectionRenderer"/> handles <see cref="PlanarReflectionNode"/>s. It
  /// renders the scene into the render target of the <see cref="PlanarReflectionNode"/> (see 
  /// <see cref="RenderToTextureNode.RenderToTexture"/>).
  /// </para>
  /// <para>
  /// <strong>Render Callback:</strong><br/>
  /// The renderer requires a callback method to render the scene. The callback method needs to
  /// render the scene using the camera and the information given in the
  /// <see cref="RenderContext"/>.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer changes the current render target of the graphics device because it uses the 
  /// graphics device to render to off-screen render targets. The render target and the viewport of
  /// the graphics device are undefined after rendering.
  /// </para>
  /// </remarks>
  public class PlanarReflectionRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the method which renders the scene.
    /// </summary>
    /// <value>
    /// The callback method that renders the scene. Must not be <see langword="null"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Action<RenderContext> RenderCallback
    {
      get { return _renderCallback; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _renderCallback = value;
      }
    }
    private Action<RenderContext> _renderCallback;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PlanarReflectionRenderer"/> class.
    /// </summary>
    /// <param name="renderCallback">
    /// The method which renders the scene. Must not be <see langword="null"/>.
    /// </param>
    public PlanarReflectionRenderer(Action<RenderContext> renderCallback)
    {
      if (renderCallback == null)
        throw new ArgumentNullException("renderCallback");

      _renderCallback = renderCallback;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is PlanarReflectionNode;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");
      if (context.Scene == null)
        throw new ArgumentException("Scene needs to be set in render context.", "context");
      if (context.CameraNode == null)
        throw new ArgumentException("Camera needs to be set in render context.", "context");
      if (!(context.CameraNode.Camera.Projection is PerspectiveProjection))
        throw new ArgumentException("The camera in the render context must use a perspective projection.", "context");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      int frame = context.Frame;

      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;
      var originalCameraNode = context.CameraNode;
      var originalLodCameraNode = context.LodCameraNode;
      float originalLodBias = context.LodBias;
      var originalReferenceNode = context.ReferenceNode;

      Pose originalCameraPose = originalCameraNode.PoseWorld;
      Vector3F originalCameraPosition = originalCameraPose.Position;
      Matrix33F originalCameraOrientation = originalCameraPose.Orientation;

      Vector3F right = originalCameraOrientation.GetColumn(0);
      Vector3F up = originalCameraOrientation.GetColumn(1);
      Vector3F back = originalCameraOrientation.GetColumn(2);

      try
      {
        // Use foreach instead of for-loop to catch InvalidOperationExceptions in
        // case the collection is modified.
        for (int i = 0; i < numberOfNodes; i++)
        {
          var node = nodes[i] as PlanarReflectionNode;
          if (node == null)
            continue;

          // Update each node only once per frame.
          if (node.LastFrame == frame)
            continue;

          node.LastFrame = frame;

          var texture = node.RenderToTexture.Texture;
          if (texture == null)
            continue;

          var renderTarget = texture as RenderTarget2D;
          if (renderTarget == null)
            throw new GraphicsException(
              "PlanarReflectionNode.RenderToTexture.Texture is invalid. The texture must be a RenderTarget2D.");

          // RenderToTexture instances can be shared. --> Update them only once per frame.
          if (node.RenderToTexture.LastFrame == frame)
            continue;

          // Do not render if we look at the back of the reflection plane.
          Vector3F planeNormal = node.NormalWorld;
          Vector3F planePosition = node.PoseWorld.Position;
          Vector3F planeToCamera = originalCameraPosition - planePosition;
          if (Vector3F.Dot(planeNormal, planeToCamera) < 0)
            continue;

          var cameraNode = node.CameraNode;

          // Reflect camera pose.
          Pose cameraPose;
          cameraPose.Position = planePosition + Reflect(planeToCamera, planeNormal);
          cameraPose.Orientation = new Matrix33F();
          cameraPose.Orientation.SetColumn(0, Reflect(right, planeNormal));
          cameraPose.Orientation.SetColumn(1, -Reflect(up, planeNormal));
          cameraPose.Orientation.SetColumn(2, Reflect(back, planeNormal));
          cameraNode.PoseWorld = cameraPose;

          // The projection of the player camera.
          var originalProjection = originalCameraNode.Camera.Projection;
          // The projection of the reflected camera.
          var projection = (PerspectiveProjection)cameraNode.Camera.Projection;

          // Choose optimal projection. We get the screen-space bounds of the reflection node.
          // Then we make the FOV so small that it exactly contains the node.
          projection.Set(originalProjection);

          var bounds = GraphicsHelper.GetBounds(cameraNode, node);

          // Abort if the bounds are empty.
          if (Numeric.AreEqual(bounds.X, bounds.Z) || Numeric.AreEqual(bounds.Y, bounds.W))
            continue;

          // Apply FOV scale to bounds.
          float fovScale = node.FieldOfViewScale;
          float deltaX = (bounds.Z - bounds.X) * (fovScale - 1) / 2;
          bounds.X -= deltaX;
          bounds.Z += deltaX;
          float deltaY = (bounds.W - bounds.Y) * (fovScale - 1) / 2;
          bounds.Y -= deltaY;
          bounds.W += deltaY;

          // Update projection to contain only the node bounds.
          projection.Left = projection.Left + bounds.X * projection.Width;
          projection.Right = projection.Left + bounds.Z * projection.Width;
          projection.Top = projection.Top - bounds.Y * projection.Height;
          projection.Bottom = projection.Top - bounds.W * projection.Height;

          // Set far clip plane.
          if (node.Far.HasValue)
            projection.Far = node.Far.Value;

          // Set near clip plane.
          Vector3F planeNormalCamera = cameraPose.ToLocalDirection(-node.NormalWorld);
          Vector3F planePointCamera = cameraPose.ToLocalPosition(node.PoseWorld.Position);
          projection.NearClipPlane = new Plane(planeNormalCamera, planePointCamera);

          context.CameraNode = cameraNode;
          context.LodCameraNode = cameraNode;
          context.LodBias = node.LodBias ?? originalLodBias;
          context.ReferenceNode = node;

          context.RenderTarget = renderTarget;
          context.Viewport = new Viewport(0, 0, renderTarget.Width, renderTarget.Height);

          RenderCallback(context);

          // Update other properties of RenderToTexture.
          node.RenderToTexture.LastFrame = frame;
          node.RenderToTexture.TextureMatrix = GraphicsHelper.ProjectorBiasMatrix
                                               * cameraNode.Camera.Projection
                                               * cameraNode.PoseWorld.Inverse;
        }
      }
      catch (InvalidOperationException exception)
      {
        throw new GraphicsException(
          "InvalidOperationException was raised in PlanarReflectionRenderer.Render(). "
          + "This can happen if a SceneQuery instance that is currently in use is modified in the "
          + "RenderCallback. --> Use different SceneQuery types in the method which calls "
          + "SceneCaptureRenderer.Render() and in the RenderCallback method.",
          exception);
      }

      graphicsDevice.SetRenderTarget(null);
      savedRenderState.Restore();

      context.RenderTarget = originalRenderTarget;
      context.Viewport = originalViewport;
      context.CameraNode = originalCameraNode;
      context.LodCameraNode = originalLodCameraNode;
      context.LodBias = originalLodBias;
      context.ReferenceNode = originalReferenceNode;
    }


    private static Vector3F Reflect(Vector3F vector, Vector3F normal)
    {
      return vector - 2 * Vector3F.Dot(normal, vector) * normal;
    }
    #endregion
  }
}
