using System;
using System.Collections.Generic;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Plane = DigitalRune.Geometry.Shapes.Plane;


namespace Samples.Graphics
{
  /// <summary>
  /// Renders planar projected shadows of <see cref="MeshNode"/>s.
  /// </summary>
  /// <remarks>
  /// This renderer supports <see cref="MeshNode"/>. It renders projected shadows onto a
  /// <see cref="Plane"/> using the <see cref="BasicEffect"/>. This means, mesh skinning and other
  /// special shader effects are not supported.
  /// </remarks>
  public class ProjectedShadowRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // To avoid overdraw: Only draw if stencil buffer contains 0, write 1 to stencil buffer.
    private readonly DepthStencilState StencilNoOverdraw = new DepthStencilState
    {
      DepthBufferEnable = true,
      DepthBufferWriteEnable = false,
      StencilEnable = true,
      StencilPass = StencilOperation.Replace,
      StencilFunction = CompareFunction.Greater,
      ReferenceStencil = 1
    };
    
    private readonly BasicEffect _effect;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the plane which receives the shadows.
    /// </summary>
    /// <value>
    /// The plane which receives the shadows. 
    /// </value>
    /// <remarks>
    /// Tip: Place this plane above the actual ground plane to avoid z-fighting.
    /// </remarks>
    public Plane ShadowedPlane { get; set; }


    /// <summary>
    /// Gets or sets the light position (or light direction, see remarks).
    /// </summary>
    /// <value>The light position in homogeneous coordinates.</value>
    /// <remarks>
    /// <para>
    /// The light position is specified in 4D homogeneous coordinates. By setting the 4th element
    /// to 0, the light is positioned at infinity. This can be used to create a directional light.
    /// </para>
    /// <para>
    /// If the light is a local light (e.g. a point light; rays are not parallel), the set the 
    /// first 3 elements to the light position and the 4th element to 1. If the light is a 
    /// directional light (light rays are parallel), then set the first 3 elements to the inverse
    /// direction of the light rays and the 4th element to 0.
    /// </para>
    /// </remarks>
    public Vector4F LightPosition { get; set; }


    /// <summary>
    /// Gets or sets the color of the shadow (including alpha value).
    /// </summary>
    /// <value>
    /// The color of the shadow (including alpha value, non-premultiplied).
    /// </value>
    /// <remarks>
    /// For example, a value of (0, 0, 0, 0.5) creates a half-transparent black shadow.
    /// </remarks>
    public Vector4F ShadowColor
    {
      get
      {
        return new Vector4F(
          (Vector3F)_effect.DiffuseColor,
          _effect.Alpha);
      }
      set
      {
        _effect.DiffuseColor = value.XYZ.ToXna();
        _effect.Alpha = value.W;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectedShadowRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    public ProjectedShadowRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _effect = new BasicEffect(graphicsService.GraphicsDevice)
      {
        DiffuseColor = Vector3.Zero,
        LightingEnabled = false,
        TextureEnabled = false
      };

      ShadowedPlane = new Plane(new Vector3F(0, 1, 0), 0.01f);
      LightPosition = new Vector4F(1, 1, 1, 0);
    }


    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          _effect.Dispose();
        }

        base.Dispose(disposing);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Determines whether this instance can render the specified node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <param name="context">The context.</param>
    /// <returns>
    /// <see langword="true"/> if this instance can render the specified node; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is MeshNode;
    }


    /// <summary>
    /// Renders the specified scene nodes.
    /// </summary>
    /// <param name="nodes">The scene nodes. The list may contain null entries.</param>
    /// <param name="context">
    /// The render context. (The property <see cref="RenderContext.CameraNode"/> selects the
    /// currently active camera. Some renderers require additional information in the render
    /// context. See remarks.)
    /// </param>
    /// <param name="order">The render order.</param>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var cameraNode = context.CameraNode;
      if (cameraNode == null)
        return; // No camera set.

      // Update view and projection matrix.
      _effect.View = cameraNode.PoseWorld.Inverse;
      _effect.Projection = cameraNode.Camera.Projection;

      // Save original render state.
      var originalDepthStencilState = graphicsDevice.DepthStencilState;
      var originalRasterizerState = graphicsDevice.RasterizerState;
      var originalBlendState = graphicsDevice.BlendState;

      // We use stencil to avoid overdraw (which is a problem for transparent shadows).
      graphicsDevice.DepthStencilState = StencilNoOverdraw;
      // Cull back faces as usual.
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
      // Use a alpha blending, because we shadows are usually transparent.
      graphicsDevice.BlendState = BlendState.AlphaBlend;

      var shadowMatrix = CreateShadowMatrix(ShadowedPlane, LightPosition);

      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as MeshNode;
        if (node == null)
          continue;

        // Apply shadow matrix to world matrix.
        Matrix world = Matrix.CreateScale((Vector3)node.ScaleWorld) * node.PoseWorld;
        _effect.World = world * shadowMatrix;

        _effect.CurrentTechnique.Passes[0].Apply();

        foreach (var submesh in node.Mesh.Submeshes)
          submesh.Draw();
      }

      // Restore render state.
      graphicsDevice.DepthStencilState = originalDepthStencilState;
      graphicsDevice.RasterizerState = originalRasterizerState;
      graphicsDevice.BlendState = originalBlendState;
    }


    /// <summary>
    /// Creates the projection matrix for planar projected shadows.
    /// </summary>
    /// <param name="plane">The plane which receives the shadows.</param>
    /// <param name="lightPosition">
    /// The light position in homogeneous coordinates.
    /// Set W to 1, to create a local light. Set W to 0, to create a directional light.
    /// </param>
    /// <returns>The shadow projection matrix.</returns>
    public static Matrix CreateShadowMatrix(Plane plane, Vector4F lightPosition)
    {
      var planeEquation = new Vector4F(plane.Normal, -plane.DistanceFromOrigin);

      float a = planeEquation.X;
      float b = planeEquation.Y;
      float c = planeEquation.Z;
      float d = planeEquation.W;
      float lx = lightPosition.X;
      float ly = lightPosition.Y;
      float lz = lightPosition.Z;
      float lw = lightPosition.W;

      // Note: XNA matrix layout, not DigitalRune layout.
      Matrix shadowMatrix = new Matrix(
        b * ly + c * lz + d * lw, -a * ly, -a * lz, -a * lw,
        -b * lx, a * lx + c * lz + d * lw, -b * lz, -b * lw,
        -c * lx, -c * ly, a * lx + b * ly + d * lw, -c * lw,
        -d * lx, -d * ly, -d * lz, a * lx + b * ly + c * lz);

      return shadowMatrix;
    }
    #endregion
  }
}
