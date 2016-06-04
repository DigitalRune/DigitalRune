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
  /// Renders scene images for the <see cref="SceneCaptureNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="SceneCaptureRenderer"/> handles <see cref="SceneCaptureNode"/>s. It renders the
  /// scene into the render target of the <see cref="SceneCaptureNode"/> (see 
  /// <see cref="RenderToTextureNode.RenderToTexture"/>). If several <see cref="SceneCaptureNode"/>s
  /// reference the same <see cref="RenderToTexture"/> instance, the scene is rendered only once.
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
  public class SceneCaptureRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private SpriteBatch _spriteBatch;
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
    /// Initializes a new instance of the <see cref="SceneCaptureRenderer"/> class.
    /// </summary>
    /// <param name="renderCallback">
    /// The method which renders the scene. Must not be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="renderCallback"/> is <see langword="null"/>.
    /// </exception>
    public SceneCaptureRenderer(Action<RenderContext> renderCallback)
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
      return node is SceneCaptureNode;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;
      var renderTargetPool = graphicsService.RenderTargetPool;
      int frame = context.Frame;

      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;
      var originalCameraNode = context.CameraNode;
      var originalLodCameraNode = context.LodCameraNode;
      var originalReferenceNode = context.ReferenceNode;

      try
      {
        // Use foreach instead of for-loop to catch InvalidOperationExceptions in
        // case the collection is modified.
        for (int i = 0; i < numberOfNodes; i++)
        {
          var node = nodes[i] as SceneCaptureNode;
          if (node == null)
            continue;

          // Update each node only once per frame.
          if (node.LastFrame == frame)
            continue;

          node.LastFrame = frame;

          var cameraNode = node.CameraNode;
          if (cameraNode == null)
            continue;

          var texture = node.RenderToTexture.Texture;
          if (texture == null)
            continue;

          // RenderToTexture instances can be shared. --> Update them only once per frame.
          if (node.RenderToTexture.LastFrame == frame)
            continue;

          context.CameraNode = cameraNode;
          context.LodCameraNode = cameraNode;
          context.ReferenceNode = node;

          var renderTarget2D = texture as RenderTarget2D;
          var projection = cameraNode.Camera.Projection;
          if (renderTarget2D != null)
          {
            context.RenderTarget = renderTarget2D;
            context.Viewport = new Viewport(0, 0, renderTarget2D.Width, renderTarget2D.Height);

            RenderCallback(context);

            // Update other properties of RenderToTexture.
            node.RenderToTexture.LastFrame = frame;
            node.RenderToTexture.TextureMatrix = GraphicsHelper.ProjectorBiasMatrix
                                                 * projection
                                                 * cameraNode.PoseWorld.Inverse;

            continue;
          }

          var renderTargetCube = texture as RenderTargetCube;
          if (renderTargetCube != null)
          {
            var format = new RenderTargetFormat(renderTargetCube) { Mipmap = false };

            renderTarget2D = renderTargetPool.Obtain2D(format);

            context.RenderTarget = renderTarget2D;
            context.Viewport = new Viewport(0, 0, renderTarget2D.Width, renderTarget2D.Height);

            if (_spriteBatch == null)
              _spriteBatch = graphicsService.GetSpriteBatch();

            var perspectiveProjection = projection as PerspectiveProjection;
            if (perspectiveProjection == null)
              throw new GraphicsException("The camera of the SceneCaptureNode must use a perspective projection.");

            // ReSharper disable CompareOfFloatsByEqualityOperator
            if (perspectiveProjection.FieldOfViewX != ConstantsF.PiOver2
                || perspectiveProjection.FieldOfViewY != ConstantsF.PiOver2)
              perspectiveProjection.SetFieldOfView(ConstantsF.PiOver2, 1, projection.Near, projection.Far);
            // ReSharper restore CompareOfFloatsByEqualityOperator

            var originalCameraPose = cameraNode.PoseWorld;
            for (int side = 0; side < 6; side++)
            {
              // Rotate camera to face the current cube map face.
              //var cubeMapFace = (CubeMapFace)side;
              // AMD problem: If we generate in normal order, the last cube map face contains 
              // garbage when mipmaps are created.
              var cubeMapFace = (CubeMapFace)(5 - side);
              var position = cameraNode.PoseWorld.Position;
              cameraNode.View = Matrix44F.CreateLookAt(
                position,
                position + originalCameraPose.ToWorldDirection(GraphicsHelper.GetCubeMapForwardDirection(cubeMapFace)),
                originalCameraPose.ToWorldDirection(GraphicsHelper.GetCubeMapUpDirection(cubeMapFace)));

              RenderCallback(context);

              // Copy RGBM texture into cube map face.
              graphicsDevice.SetRenderTarget(renderTargetCube, cubeMapFace);
              _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null);
              _spriteBatch.Draw(renderTarget2D, new Vector2(0, 0), Color.White);
              _spriteBatch.End();
            }
            cameraNode.PoseWorld = originalCameraPose;

            renderTargetPool.Recycle(renderTarget2D);

            // Update other properties of RenderToTexture.
            node.RenderToTexture.LastFrame = frame;
            node.RenderToTexture.TextureMatrix = GraphicsHelper.ProjectorBiasMatrix
                                                 * projection
                                                 * cameraNode.PoseWorld.Inverse;

            continue;
          }

          throw new GraphicsException(
            "SceneCaptureNode.RenderToTexture.Texture is invalid. The texture must be a RenderTarget2D or RenderTargetCube.");
        }
      }
      catch (InvalidOperationException exception)
      {
        throw new GraphicsException(
          "InvalidOperationException was raised in SceneCaptureRenderer.Render(). "
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
      context.ReferenceNode = originalReferenceNode;
    }
    #endregion
  }
}
