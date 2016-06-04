// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Captures a snapshot of the scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This node can be used to capture the scene as viewed from a specific camera (see property
  /// <see cref="CameraNode"/>). The render-to-texture operation is usually performed by a
  /// <see cref="SceneCaptureRenderer"/>. The <see cref="SceneCaptureRenderer"/> will render the
  /// scene for each <see cref="SceneCaptureNode"/> where a valid render target (see property
  /// <see cref="RenderToTextureNode.RenderToTexture"/>) and a valid <see cref="CameraNode"/> is
  /// set.
  /// </para>
  /// <para>
  /// <strong>2D and Cube Map Render Targets:</strong><br/>
  /// A valid render target must be set in the <see cref="RenderToTextureNode.RenderToTexture"/>
  /// property. The target texture can be a <see cref="RenderTarget2D"/> to capture a 2D image or a
  /// <see cref="RenderTargetCube"/> to capture an environment map. If the render target is a cube
  /// map, then the scene will be rendered 6 times, once for each cube map side.
  /// </para>
  /// <para>
  /// <strong>Shared RenderToTexture instances:</strong><br/>
  /// Several <see cref="SceneCaptureNode"/>s can reference the same <see cref="RenderToTexture"/>
  /// instance. In this case, the scene is rendered only once. - See the example below.
  /// </para>
  /// <para>
  /// <strong>Frustum Culling:</strong><br/>
  /// The <see cref="SceneCaptureNode"/> is a normal scene node which can be added to the scene
  /// graph. The default <see cref="Shape"/> is an infinite shape - which means that this node is
  /// always visible. However, it is recommended to set a smaller shape. Thus, the 
  /// <see cref="SceneCaptureNode"/> can be used for frustum culling - if it is culled, then no 
  /// image needs to be captured.
  /// </para>
  /// </remarks>
  /// <example>
  /// <para>
  /// A prison level of a game has a surveillance camera. The recorded image should be displayed on
  /// several 3D models which represent security monitors. In this case, create one 
  /// <see cref="CameraNode"/> for the surveillance camera. Create one <see cref="RenderToTexture"/>
  /// instance which is shared by several <see cref="SceneCaptureNode"/>s. Create a
  /// <see cref="SceneCaptureNode"/> for each monitor mesh node.  Add the 
  /// <see cref="SceneCaptureNode"/> to the children of the monitor mesh node. Set the shape of the 
  /// <see cref="SceneCaptureNode"/> to the shape of the monitor mesh node. - This way, the
  /// <see cref="SceneCaptureNode"/> has the same pose and shape as the model which uses captured
  /// texture; and if no <see cref="SceneCaptureNode"/> is visible from the player camera, then the
  /// scene does not need to be captured in this frame (because the player does not look at any
  /// monitor). If a <see cref="SceneCaptureNode"/> is visible, then the scene is captured once per
  /// frame.
  /// </para>
  /// <para>
  /// Note: How a captured texture is used, is not defined by the <see cref="SceneCaptureNode"/>.
  /// You can, for example, use an effect parameter binding to use the texture when rendering a
  /// mesh.
  /// </para>
  /// </example>
  public class SceneCaptureNode : RenderToTextureNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the camera node.
    /// </summary>
    /// <value>The camera node.</value>
    public CameraNode CameraNode { get; set; }


    /// <summary>
    /// Gets or sets the bounding shape of this scene node.
    /// </summary>
    /// <value>
    /// The bounding shape. The bounding shape contains only the current node - it does not include 
    /// the bounds of the children! The default value is an 
    /// <see cref="DigitalRune.Geometry.Shapes.Shape.Infinite"/> shape.
    /// </value>
    /// <inheritdoc cref="SceneNode.Shape"/>
    public new Shape Shape
    {
      get { return base.Shape; }
      set { base.Shape = value; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneCaptureNode" /> class.
    /// </summary>
    /// <param name="renderToTexture">The render texture target.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="renderToTexture"/> is <see langword="null"/>.
    /// </exception>
    public SceneCaptureNode(RenderToTexture renderToTexture)
      : base(renderToTexture)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new SceneCaptureNode Clone()
    {
      return (SceneCaptureNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new SceneCaptureNode(RenderToTexture);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone SceneCaptureNode properties.
      var sourceTyped = (SceneCaptureNode)source;
      CameraNode = sourceTyped.CameraNode;
    }
    #endregion

    #endregion
  }
}
