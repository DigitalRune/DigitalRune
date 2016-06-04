// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DigitalRune.Collections;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="TerrainNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="TerrainRenderer"/> renders <see cref="TerrainNode"/> using the
  /// <see cref="TerrainNode.Material"/> of the node. The default material uses clipmaps
  /// (<see cref="TerrainNode.BaseClipmap"/> and <see cref="TerrainNode.DetailClipmap"/>). The
  /// clipmaps are created by the <see cref="TerrainClipmapRenderer"/> - not by the
  /// <see cref="TerrainRenderer"/>!
  /// </para>
  /// <para>
  /// Each terrain node is rendered using an internal static geo-clipmap mesh. The mesh is
  /// represented by the class <see cref="TerrainRendererMesh"/>. An instance of
  /// <see cref="TerrainRendererMesh"/> is created automatically when needed and cached internally.
  /// However, the creation of the mesh can take up to several seconds. Therefore it is recommended
  /// to build the mesh offline and store the instance as a file using
  /// <see cref="TerrainRendererMesh.Save"/>. At runtime, the pre-built mesh can be loaded using
  /// <see cref="TerrainRendererMesh.Load"/> and passed to the <see cref="TerrainRenderer"/> using
  /// <see cref="SetMesh"/>.
  /// </para>
  /// <para>
  /// The mesh needs to match the <see cref="TerrainNode.BaseClipmap"/> of the
  /// <see cref="TerrainNode"/>. The properties <see cref="TerrainClipmap.NumberOfLevels"/> and
  /// <see cref="TerrainClipmap.CellsPerLevel"/> determine the size of the mesh. The terrain
  /// renderer automatically creates and caches one internal mesh for each configuration it
  /// encounters.
  /// </para>
  /// <para>
  /// <strong>Wire frame rendering:</strong><br/>
  /// <see cref="DrawWireFrame"/> can be set to render the wire frame of the terrain for debugging.
  /// This works only if the material of the terrain node provides a render pass "WireFrame".
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// <para>
  /// <strong>Disposing:</strong><br/>
  /// The internally stored <see cref="TerrainRendererMesh"/> instances are disposed when the
  /// <see cref="TerrainRenderer"/> is disposed!
  /// </para>
  /// </remarks>
  public class TerrainRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    //private readonly IGraphicsService _graphicsService;
    private readonly Dictionary<Pair<int, int>, TerrainRendererMesh> _meshes = new Dictionary<Pair<int, int>, TerrainRendererMesh>(1);
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether the wire frame of the terrain should be rendered for
    /// debugging.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to render the terrain wire frame; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// See <see cref="TerrainRenderer"/> for more details.
    /// </remarks>
    public bool DrawWireFrame { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The current graphics profile is Reach.
    /// </exception>
    [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public TerrainRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        throw new NotSupportedException("The Diagnostics does not support the Reach profile.");

      //_graphicsService = graphicsService;
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          foreach (var mesh in _meshes.Values)
            mesh.Dispose();
        }
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Clears the internally stored <see cref="TerrainRendererMesh"/>es.
    /// </summary>
    /// <param name="dispose">
    /// If set to <see langword="true" /> the internally stored meshes are disposed.
    /// If set to <see langword="false"/> the references to the meshes are removed but the meshes
    /// are not disposed.
    /// </param>
    public void ClearMeshes(bool dispose)
    {
      if (dispose)
        foreach (var mesh in _meshes.Values)
          mesh.Dispose();

      _meshes.Clear();
    }


    /// <summary>
    /// Adds a new <see cref="TerrainRendererMesh"/>.
    /// </summary>
    /// <param name="mesh">The terrain mesh.</param>
    /// <remarks>
    /// <para>
    /// The <see cref="TerrainRenderer"/> takes ownership of the <see cref="TerrainRendererMesh"/>
    /// instance. When the <see cref="TerrainRenderer"/> is disposed of, all internally stored
    /// <see cref="TerrainRendererMesh"/>es are disposed of.
    /// </para>
    /// <para>
    /// If the terrain renderer already has a mesh with the same settings
    /// (<see cref="TerrainRendererMesh.NumberOfLevels"/> and
    /// <see cref="TerrainRendererMesh.CellsPerLevel"/>), the old mesh is disposed of and replaced.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    public void SetMesh(TerrainRendererMesh mesh)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");
      if (mesh.IsDisposed)
        throw new ArgumentException("Mesh is disposed.", "mesh");

      var key = new Pair<int, int>(mesh.NumberOfLevels, mesh.CellsPerLevel);

      TerrainRendererMesh oldMesh;
      if (_meshes.TryGetValue(key, out oldMesh) && mesh != oldMesh)
        oldMesh.Dispose();

      _meshes[key] = mesh;
    }


    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is TerrainNode;
    }


    /// <inheritdoc/>
    [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode"), SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      context.ThrowIfCameraMissing();
      context.ThrowIfRenderPassMissing();

      var graphicsService = context.GraphicsService;
      var graphicsDevice = graphicsService.GraphicsDevice;

      var cameraNode = context.CameraNode;

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as TerrainNode;
        if (node == null)
          continue;

        // Node is visible in current frame.
        node.LastFrame = frame;

        if (!node.MaterialInstance.Contains(context.RenderPass))
          continue;

        TerrainRendererMesh mesh;
        var meshKey = new Pair<int, int>(node.BaseClipmap.NumberOfLevels, node.BaseClipmap.CellsPerLevel);
        bool meshExists = _meshes.TryGetValue(meshKey, out mesh);
        if (!meshExists || mesh.IsDisposed)
        {
          // Warning: This may take a few seconds!
          mesh = new TerrainRendererMesh(graphicsDevice, node.BaseClipmap.NumberOfLevels, node.BaseClipmap.CellsPerLevel);
          _meshes[meshKey] = mesh;
        }

        // Get the EffectBindings and the Effect for the current render pass.
        EffectBinding materialInstanceBinding = node.MaterialInstance[context.RenderPass];
        EffectBinding materialBinding = node.Material[context.RenderPass];
        Effect effect = materialBinding.Effect;

        // Add this info to the render context. This info will be needed by parameter bindings.
        context.SceneNode = node;
        context.MaterialInstanceBinding = materialInstanceBinding;
        context.MaterialBinding = materialBinding;

        // Update and apply global effect parameter bindings - these bindings set the 
        // effect parameter values for "global" parameters. For example, if an effect uses
        // a "ViewProjection" parameter, then a binding will compute this matrix from the
        // current CameraInstance in the render context and update the effect parameter.
        foreach (var binding in effect.GetParameterBindings())
        {
          if (binding.Description.Hint == EffectParameterHint.Global)
          {
            binding.Update(context);
            binding.Apply(context);
          }
        }

        // Update and apply material bindings - these are usually constant parameter values,
        // like textures or colors, that are defined in the fbx file or material description 
        // (XML files).
        //SetHoleParameter(materialBinding, context);
        foreach (var binding in materialBinding.ParameterBindings)
        {
          binding.Update(context);
          binding.Apply(context);
        }

        // Update and apply local, per-instance, and per-pass bindings - these are bindings
        // for parameters, like the "World" matrix or lighting parameters.
        foreach (var binding in materialInstanceBinding.ParameterBindings)
        {
          if (binding.Description.Hint != EffectParameterHint.PerPass)
          {
            binding.Update(context);
            binding.Apply(context);
          }
        }

        // Select and apply technique.
        var techniqueBinding = materialInstanceBinding.TechniqueBinding;
        techniqueBinding.Update(context);
        var technique = techniqueBinding.GetTechnique(effect, context);
        effect.CurrentTechnique = technique;

        var passBinding = techniqueBinding.GetPassBinding(technique, context);
        foreach (var pass in passBinding)
        {
          // Update and apply per-pass bindings.
          foreach (var binding in materialInstanceBinding.ParameterBindings)
          {
            if (binding.Description.Hint == EffectParameterHint.PerPass)
            {
              binding.Update(context);
              binding.Apply(context);
            }
          }

          pass.Apply();

          // The WireFrame pass is only rendered if DrawWireFrame is set.
          if ((!string.Equals(pass.Name, "WireFrame", StringComparison.OrdinalIgnoreCase) || DrawWireFrame)
              && !string.Equals(pass.Name, "Restore", StringComparison.OrdinalIgnoreCase))
          {
            mesh.Submesh.Draw();
          }
        }

        // Reset texture effect parameter:
        // This seems to be necessary because the vertex textures are not automatically
        // removed from the texture stage which causes exceptions later.
        ResetParameter(effect, "TerrainBaseClipmap0");
        ResetParameter(effect, "TerrainBaseClipmap1");
        ResetParameter(effect, "TerrainBaseClipmap2");
        ResetParameter(effect, "TerrainBaseClipmap3");
        foreach (var pass in passBinding)
        {
          pass.Apply();
          break;
        }
      }

      context.SceneNode = null;
      context.MaterialBinding = null;
      context.MaterialInstanceBinding = null;

      savedRenderState.Restore();
    }


    //private static void SetHoleParameter(EffectBinding effectBinding, RenderContext context)
    //{
      // To clip triangles in the vertex shader, we can set the vertex position to a special value
      // which causes the triangle to be clipped:
      // NaN and Infinity work on NVIDIA and AMD but not Intel HD4000.
      // positionProj.w = 0 work only if we set all triangle vertices = 0.
      // "Infinity behind the camera" seems to work everywhere except for orthographic projections
      // (sun shadow camera!).

      //var cameraNode = context.CameraNode;
      //var pose = cameraNode.PoseWorld;
      //var forward = pose.ToWorldDirection(Vector3F.Forward);
      //var forwardAbsolute = forward;
      //forwardAbsolute.Absolute();
      //var indexOfLargestComponent = forwardAbsolute.IndexOfLargestComponent;
      //Vector3 holePosition;
      //if (indexOfLargestComponent == 0)
      //{
      //  if (forward.X > 0)
      //    holePosition = new Vector3(float.NegativeInfinity, 0, 0);
      //  else
      //    holePosition = new Vector3(float.PositiveInfinity, 0, 0);
      //}
      //else if (indexOfLargestComponent == 1)
      //{
      //  if (forward.Y > 0)
      //    holePosition = new Vector3(0, float.NegativeInfinity, 0);
      //  else
      //    holePosition = new Vector3(0, float.PositiveInfinity, 0);
      //}
      //else 
      //{
      //  if (forward.Z > 0)
      //    holePosition = new Vector3(0, 0, float.NegativeInfinity);
      //  else
      //    holePosition = new Vector3(0, 0, float.PositiveInfinity);
      //}

      //UpdateAndApplyParameter(effectBinding, "TerrainHolePosition", new Vector3(float.NaN));

      // --> Current Effects use DefaultEffectParameterSemantics.NaN.
    //}


    private static void ResetParameter(Effect effect, string textureName)
    {
      var parameter = effect.Parameters[textureName];
      if (parameter != null)
        parameter.SetValue((Texture2D)null);
    }
    #endregion
  }
}
#endif
