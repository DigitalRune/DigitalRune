#if !WP7 && !WP8
using System;
using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  /// <summary>
  /// Renders <see cref="FogSphereNode"/>s into the current render target.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  public class FogSphereRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterWorld;
    private readonly EffectParameter _parameterWorldInverse;
    private readonly EffectParameter _parameterView;
    private readonly EffectParameter _parameterProjection;
    private readonly EffectParameter _parameterCameraPosition;
    private readonly EffectParameter _parameterCameraFar;
    private readonly EffectParameter _parameterGBuffer0;
    private readonly EffectParameter _parameterColor;
    private readonly EffectParameter _parameterDensity;
    private readonly EffectParameter _parameterBlendMode;
    private readonly EffectParameter _parameterFalloff;
    private readonly EffectParameter _parameterIntersectionSoftness;

    private readonly Submesh _submesh;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FogSphereRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public FogSphereRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      // Load effect.
      _effect = graphicsService.Content.Load<Effect>("FogSphere");
      _parameterViewportSize = _effect.Parameters["ViewportSize"];
      _parameterWorld = _effect.Parameters["World"];
      _parameterWorldInverse = _effect.Parameters["WorldInverse"];
      _parameterView = _effect.Parameters["View"];
      _parameterProjection = _effect.Parameters["Projection"];
      _parameterCameraPosition = _effect.Parameters["CameraPosition"];
      _parameterCameraFar = _effect.Parameters["CameraFar"];
      _parameterGBuffer0 = _effect.Parameters["GBuffer0"];
      _parameterColor = _effect.Parameters["Color"];
      _parameterBlendMode = _effect.Parameters["BlendMode"];
      _parameterDensity = _effect.Parameters["Density"];
      _parameterFalloff = _effect.Parameters["Falloff"];
      _parameterIntersectionSoftness = _effect.Parameters["IntersectionSoftness"];

      // Get a sphere mesh.
      _submesh = MeshHelper.CreateIcosphere(graphicsService.GraphicsDevice, 2);
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
          _submesh.Dispose();

        base.Dispose(disposing);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is FogSphereNode;
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      var cameraNode = context.CameraNode;
      var projection = cameraNode.Camera.Projection;
      Pose view = cameraNode.PoseWorld.Inverse;

      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      // Save render state.
      var originalRasterizerState = graphicsDevice.RasterizerState;
      var originalDepthStencilState = graphicsDevice.DepthStencilState;
      var originalBlendState = graphicsDevice.BlendState;

      // We render only backsides with no depth test.
      // OPTIMIZE: When camera is outside a sphere, we can render front sides with depth-read.
      graphicsDevice.RasterizerState = RasterizerState.CullClockwise;
      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.BlendState = BlendState.AlphaBlend;

      // Set global effect parameters.
      var viewport = graphicsDevice.Viewport;
      _parameterViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
      _parameterView.SetValue(view);
      _parameterProjection.SetValue(projection);
      _parameterCameraPosition.SetValue((Vector3)cameraNode.PoseWorld.Position);
      _parameterCameraFar.SetValue(projection.Far);
      _parameterGBuffer0.SetValue(context.GBuffer0);
      
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as FogSphereNode;
        if (node == null)
          continue;

        // FogSphereNode is visible in current frame.
        node.LastFrame = frame;

        Matrix world = Matrix.CreateScale((Vector3)node.ScaleWorld) * node.PoseWorld;
        _parameterWorld.SetValue(world);
        _parameterWorldInverse.SetValue(Matrix.Invert(world));
        _parameterColor.SetValue((Vector3)node.Color);
        _parameterBlendMode.SetValue(node.BlendMode);
        _parameterDensity.SetValue(node.Density);
        _parameterFalloff.SetValue(node.Falloff);
        _parameterIntersectionSoftness.SetValue(node.IntersectionSoftness);

        _effect.CurrentTechnique.Passes[0].Apply();
        _submesh.Draw();
      }

      // Restore render states.
      graphicsDevice.RasterizerState = originalRasterizerState;
      graphicsDevice.DepthStencilState = originalDepthStencilState;
      graphicsDevice.BlendState = originalBlendState;
    }
    #endregion
  }
}
#endif