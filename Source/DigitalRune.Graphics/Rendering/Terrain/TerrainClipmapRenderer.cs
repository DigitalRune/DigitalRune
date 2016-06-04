// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders the clipmaps of a <see cref="TerrainNode"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="TerrainClipmapRenderer"/> is a <see cref="SceneNodeRenderer"/> that handles
  /// <see cref="TerrainNode"/>s. It renders the <see cref="TerrainNode.BaseClipmap"/>
  /// and <see cref="TerrainNode.DetailClipmap"/> of the <see cref="TerrainNode"/>.
  /// </para>
  /// <para>
  /// <strong>Render passes:</strong><br/>
  /// This renderer uses the <see cref="Material"/>s stored in the <see cref="TerrainLayer"/>. The
  /// material needs a render pass called "Base" to render into the
  /// <see cref="TerrainNode.BaseClipmap"/> and a render pass called "Detail" to render into the
  /// <see cref="TerrainNode.DetailClipmap"/>. <see cref="TerrainLayer"/>s can have render passes
  /// for both, "Base" and "Detail".
  /// </para>
  /// <para>
  /// <strong>Clipmap data:</strong><br/>
  /// The content of the clipmaps depends on the used materials. The materials of the
  /// <see cref="TerrainLayer"/> material renders into the clipmap. The material of the
  /// <see cref="TerrainNode"/> reads from the clipmap. These materials can be modified to store
  /// different data in the clipmaps.
  /// </para>
  /// <para>
  /// Per default, the <see cref="TerrainNode.BaseClipmap"/> consists of one 
  /// <see cref="SurfaceFormat.HalfVector4"/> texture which stores:
  /// (absolute terrain height, world space normal x, world space normal z, hole flag)
  /// </para>
  /// <para>
  /// Per default, the <see cref="TerrainNode.DetailClipmap"/> consists of three 
  /// <see cref="SurfaceFormat.Color"/> textures which store: 
  /// (world space detail normal x, world space detail normal z, specular power, hole flag),
  /// (diffuse R, diffuse B, diffuse B, -), (specular intensity, height, -, -)
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer changes the current render target of the graphics device because it uses the 
  /// graphics device to render into the clipmap render targets. The render target
  /// and the viewport of the graphics device are undefined after rendering.
  /// </para>
  /// </remarks>
  public partial class TerrainClipmapRenderer : SceneNodeRenderer
  {
    // Notes:
    // A clipmap with toroidal wrapping consists of 4 rectangles:
    //   +-------+--------------+
    //   | new   |     new      |
    //   +-------o--------------+   clipmap.Offsets stores point o in [0, 1] relative to
    //   |       |              |   texture without any texture filter border.
    //   |       |              |
    //   | new   |     old      |
    //   |       |              |
    //   |       |              |
    //   +-------+--------------+
    // If the offset moves from (0, 0) to (m, n), then we have to update the L shape with three
    // rectangles. However, if the offset moves from (m, n) to (j, k), then we have to
    // update a cross shape - or even a ring shape if the offset moves over the border and wraps
    // around. --> To simplify this, we compute AABBs of the invalid regions. Then we handle the
    // 4 rectangles of the toroidally wrapped clipmap and update the rectangle parts that overlap
    // the invalid region AABBs.
    //
    // Since the TerrainClipmapRenderer clears Terrain.InvalidRegions, the same Terrain instance
    // cannot be used with different TerrainClipmapRenderer calls. --> The game should make only
    // one TerrainClipmapRenderer call - but that should never be a problem.
    //
    // TODO:
    // - OPTIMIZE: Use texture arrays instead of a texture atlas!
    // - OPTIMIZE: Use DXT compression for clipmaps.


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    // Render pass names.
    internal const string RenderPassBase = "Base";
    internal const string RenderPassDetail = "Detail";
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // 4 render target binding arrays for 1 to 4 render targets at once.
    private readonly RenderTargetBinding[][] _renderTargetBindings = new RenderTargetBinding[4][];

    /// <summary>
    /// Rasterizer state without culling and with enabled scissor test.
    /// </summary>
    private static readonly RasterizerState RasterizerStateCullNoneWithScissorTest = new RasterizerState
    {
      CullMode = CullMode.None,
      ScissorTestEnable = true
    };

    /// <summary>
    /// Blend state with RGB alpha blending but no change to the alpha channel.
    /// </summary>
    private static readonly BlendState BlendStateAlphaBlendRgb = new BlendState
    {
      ColorSourceBlend = Blend.One,
      ColorBlendFunction = BlendFunction.Add,
      ColorDestinationBlend = Blend.InverseSourceAlpha,

      // Do not destroy existing alpha.
      AlphaSourceBlend = Blend.Zero,
      AlphaBlendFunction = BlendFunction.Add,
      AlphaDestinationBlend = Blend.One,
    };

    // Temp collections.
    private readonly Stack<Aabb> _aabbStack = new Stack<Aabb>();
    private readonly List<Aabb> _aabbList = new List<Aabb>();

    private readonly TerrainClearLayer _clearLayer;

    private EffectBinding _previousMaterialBinding;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    // 8 pixel border supports 16x anisotropic filtering.
    private const int Border = 8;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainClipmapRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The current graphics profile is Reach.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TerrainClipmapRenderer")]
    public TerrainClipmapRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        throw new NotSupportedException("The TerrainClipmapRenderer does not support the Reach profile.");

      _clearLayer = new TerrainClearLayer(graphicsService);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is TerrainNode;
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (nodes.Count == 0)
        return;

      context.ThrowIfCameraMissing();

      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;
      var originalSceneNode = context.SceneNode;
      var originalTechnique = context.Technique;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      //int frame = context.Frame;
      //float deltaTime = (float)context.DeltaTime.TotalSeconds;

      for (int nodeIndex = 0; nodeIndex < numberOfNodes; nodeIndex++)
      {
        var node = nodes[nodeIndex] as TerrainNode;
        if (node == null)
          continue;

        context.SceneNode = node;

        context.RenderPass = RenderPassBase;
        ProcessClipmap(node, node.BaseClipmap, context);

        context.RenderPass = RenderPassDetail;
        ProcessClipmap(node, node.DetailClipmap, context);
      }

      context.RenderPass = null;

      // Clear invalid regions stored in terrain. (Note: Terrains can be shared.)
      for (int nodeIndex = 0; nodeIndex < numberOfNodes; nodeIndex++)
      {
        var node = nodes[nodeIndex] as TerrainNode;
        if (node == null)
          continue;

        node.Terrain.InvalidBaseRegions.Clear();
        node.Terrain.InvalidDetailRegions.Clear();
      }

      // The clipmap layers use a MipMapLodBias which must be reset.
      graphicsDevice.ResetSamplerStates();

      savedRenderState.Restore();
      graphicsDevice.SetRenderTarget(null);
      context.RenderTarget = originalRenderTarget;
      context.Viewport = originalViewport;
      context.SceneNode = originalSceneNode;
      context.MaterialBinding = null;
      context.MaterialInstanceBinding = null;
      context.Technique = originalTechnique;
    }


    private void ProcessClipmap(TerrainNode node, TerrainClipmap clipmap, RenderContext context)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var lodCameraNode = context.LodCameraNode ?? context.CameraNode;

      bool isBaseClipmap = (node.BaseClipmap == clipmap);

      // Update the clipmap render targets if necessary.
      InitializeClipmapTextures(graphicsDevice, clipmap);

      // Update other clipmap data (origins, offsets, ...). No rendering.
      // (Data is stored in TerrainClipmap class.)
      UpdateClipmapData(node, clipmap, lodCameraNode, isBaseClipmap);

      // Compute which rectangular regions need to be updated.
      // (Data is stored in TerrainClipmap class.)
      ComputeInvalidRegions(node, clipmap, isBaseClipmap);

      // Abort if there are no invalid regions.
      int numberOfInvalidRegions = 0;
      for (int level = 0; level < clipmap.NumberOfLevels; level++)
        numberOfInvalidRegions += clipmap.InvalidRegions[level].Count;

      Debug.Assert(numberOfInvalidRegions > 0 || clipmap.UseIncrementalUpdate,
        "If the clipmap update is not incremental, there must be at least one invalid region.");

      if (numberOfInvalidRegions == 0)
        return;

      // Set render target binding to render into all clipmap textures at once.
      int numberOfTextures = clipmap.Textures.Length;
      if (_renderTargetBindings[numberOfTextures] == null)
        _renderTargetBindings[numberOfTextures] = new RenderTargetBinding[numberOfTextures];

      for (int i = 0; i < numberOfTextures; i++)
        _renderTargetBindings[numberOfTextures][i] = new RenderTargetBinding((RenderTarget2D)clipmap.Textures[i]);

      switch (numberOfTextures)
      {
        case 1: context.Technique = "RenderTargets1"; break;
        case 2: context.Technique = "RenderTargets2"; break;
        case 3: context.Technique = "RenderTargets3"; break;
        case 4: context.Technique = "RenderTargets4"; break;
        default: context.Technique = null; break;
      }

      graphicsDevice.SetRenderTargets(_renderTargetBindings[numberOfTextures]);

      // The viewport covers the whole texture atlas.
      var viewport = graphicsDevice.Viewport;
      context.Viewport = viewport;

      Debug.Assert(_previousMaterialBinding == null);

      // Loop over all layers. Render each layer into all levels (if there is an invalid region).
      Aabb tileAabb = new Aabb(new Vector3F(-Terrain.TerrainLimit), new Vector3F(Terrain.TerrainLimit));
      ProcessLayer(graphicsDevice, context, clipmap, isBaseClipmap, _clearLayer, tileAabb);
      foreach (var tile in node.Terrain.Tiles)
      {
        tileAabb = tile.Aabb;
        context.Object = tile;
        ProcessLayer(graphicsDevice, context, clipmap, isBaseClipmap, tile, tileAabb);

        foreach (var layer in tile.Layers)
          ProcessLayer(graphicsDevice, context, clipmap, isBaseClipmap, layer, tileAabb);

        context.Object = null;
      }

      _previousMaterialBinding = null;

      ClearFlags(_clearLayer, context);
      foreach (var tile in node.Terrain.Tiles)
      {
        ClearFlags(tile, context);
        foreach (var layer in tile.Layers)
          ClearFlags(layer, context);
      }

      // All invalid regions handled.
      for (int i = 0; i < clipmap.NumberOfLevels; i++)
        clipmap.InvalidRegions[i].Clear();

      // The next time we can update incrementally.
      clipmap.UseIncrementalUpdate = true;
    }


    private static void UpdateClipmapData(TerrainNode node, TerrainClipmap clipmap, CameraNode lodCameraNode, bool isBaseClipmap)
    {
      int border = isBaseClipmap ? 0 : Border;

      var terrain = node.Terrain;
      var terrainAabb = terrain.Aabb;
      Vector2F terrainAabbMin = new Vector2F(terrainAabb.Minimum.X, terrainAabb.Minimum.Z);
      Vector2F terrainAabbMax = new Vector2F(terrainAabb.Maximum.X, terrainAabb.Maximum.Z);

      for (int level = clipmap.NumberOfLevels - 1; level >= 0; level--)
      {
        // Compute new origins.
        int texelsPerLevel = clipmap.CellsPerLevel - 2 * border;
        Vector3F referencePosition3D = lodCameraNode.PoseWorld.Position;
        Vector2F referencePosition2D = new Vector2F(referencePosition3D.X, referencePosition3D.Z);
        clipmap.LevelSizes[level] = clipmap.ActualCellSizes[level] * texelsPerLevel;
        Vector2F levelOrigin = new Vector2F(
          referencePosition2D.X - clipmap.LevelSizes[level] / 2,
          referencePosition2D.Y - clipmap.LevelSizes[level] / 2);

        // Do not move detail clipmap outside the terrain. This would only waste resources.
        if (!isBaseClipmap)
        {
          // The fade range is hardcoded to 15% of the radius in Terrain.fxh.
          float fadeRange = clipmap.LevelSizes[level] * node.DetailFadeRange / 2; // / 2 because its relative to the radius.

          // The last level does not need a fade range.
          if (level == clipmap.NumberOfLevels - 1)
            fadeRange = 0;

          levelOrigin.X = Math.Min(levelOrigin.X, terrainAabbMax.X - clipmap.LevelSizes[level] + fadeRange);
          levelOrigin.Y = Math.Min(levelOrigin.Y, terrainAabbMax.Y - clipmap.LevelSizes[level] + fadeRange);
          levelOrigin.X = Math.Max(levelOrigin.X, terrainAabbMin.X - fadeRange);
          levelOrigin.Y = Math.Max(levelOrigin.Y, terrainAabbMin.Y - fadeRange);
        }
        
        // Snap to grid.
        levelOrigin = RoundToGrid(levelOrigin, clipmap.ActualCellSizes[level]);

        if (isBaseClipmap)
        {
          // To align the base clipmap with the terrain tile cell raster, we have to move it by
          // half a cell size. E.g. if the tile origin is (0, 0), then the first mesh vertex
          // is over (0, 0). The vertices should sample the clipmap texel centers.
          levelOrigin.X -= clipmap.ActualCellSizes[0] / 2;
          levelOrigin.Y -= clipmap.ActualCellSizes[0] / 2;
        }

        // Simple movement threshold for debugging: Only update clipmaps if origin
        // has moved more than 10 units.
        //if ((levelOrigin - clipmap.OldOrigins[level]).Length < 10)
        //  continue;

        if (levelOrigin == clipmap.OldOrigins[level] && clipmap.UseIncrementalUpdate)
          continue;

        if (level < clipmap.MinLevel)
        {
          // We do not update this level.
          // Note: As long as the level is within the region of the next level we could still 
          // draw it - but we ignore this here:
          // Set to invalid origin, to make the shader ignore this level. (The value is still used
          // in the shader, so we must not set it to a totally extreme value.)
          clipmap.Origins[level] = referencePosition2D - new Vector2F(10000);
          clipmap.OldOrigins[level] = clipmap.Origins[level];
          continue;
        }

        clipmap.OldOrigins[level] = clipmap.Origins[level];
        clipmap.OldOffsets[level] = clipmap.Offsets[level];

        // Compute new toroidal wrap offset.
        if (!clipmap.UseIncrementalUpdate || isBaseClipmap)
        {
          // Full clipmap update needed or it is a base clipmap.

          clipmap.Origins[level] = levelOrigin;

          // Base clipmaps are usually super small (< 256²) and are updated infrequently.
          // Therefore we do not make a toroidal update for the base clipmap.
          clipmap.Offsets[level] = new Vector2F(0, 0);
        }
        else
        {
          clipmap.Origins[level] = levelOrigin;

          Vector2F levelSize = new Vector2F(clipmap.LevelSizes[level]);
          Vector2F newOffset = clipmap.OldOffsets[level] + (levelOrigin - clipmap.OldOrigins[level]) / levelSize;

          float cellsPerLevelWithoutBorder = clipmap.CellsPerLevel - 2 * border;
          //Debug.Assert(
          //  Numeric.AreEqual(newOffset.X * cellsPerLevelWithoutBorder,
          //    (float)Math.Floor(newOffset.X * cellsPerLevelWithoutBorder + 0.5f), 0.01f),
          //  "New clipmap offset is not snapped to texel grid.");
          //Debug.Assert(
          //  Numeric.AreEqual(newOffset.Y * cellsPerLevelWithoutBorder,
          //    (float)Math.Floor(newOffset.Y * cellsPerLevelWithoutBorder + 0.5f), 0.01f),
          //  "New clipmap offset is not snapped to texel grid.");

          // The offset should always correspond to clipmap texels, but if we compute the new offset 
          // from the old offset then we accumulate errors. --> Snap to texels to remove error.
          newOffset.X = (float)Math.Floor(newOffset.X * cellsPerLevelWithoutBorder + 0.5f) / cellsPerLevelWithoutBorder;
          newOffset.Y = (float)Math.Floor(newOffset.Y * cellsPerLevelWithoutBorder + 0.5f) / cellsPerLevelWithoutBorder;

          // Use "positive modulo" to wrap to [0, 1].
          newOffset.X = ((newOffset.X % 1) + 1) % 1;
          newOffset.Y = ((newOffset.Y % 1) + 1) % 1;

          Debug.Assert(Numeric.IsGreaterOrEqual(newOffset.X, 0), "New offset is not in [0,1]");
          Debug.Assert(Numeric.IsLessOrEqual(newOffset.X, 1), "New offset is not in [0,1]");
          Debug.Assert(Numeric.IsGreaterOrEqual(newOffset.Y, 0), "New offset is not in [0,1]");
          Debug.Assert(Numeric.IsLessOrEqual(newOffset.Y, 1), "New offset is not in [0,1]");

          // Note: clipmap.Offsets stores the offsets as if border is 0.
          clipmap.Offsets[level].X = newOffset.X;
          clipmap.Offsets[level].Y = newOffset.Y;
        }
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private void ComputeInvalidRegions(TerrainNode node, TerrainClipmap clipmap, bool isBaseClipmap)
    {
      // Note:
      // This method computes the AABBs of terrain parts that need to be updated.
      // Important: AABBs must not overlap because the materials can use alpha blending and they
      // must not blend twice into the same area.

      var terrain = node.Terrain;

      var invalidRegions = isBaseClipmap ? terrain.InvalidBaseRegions : terrain.InvalidDetailRegions;
      var areInvalidRegionsClipped = isBaseClipmap ? terrain.AreInvalidBaseRegionsClipped : terrain.AreInvalidDetailRegionsClipped;

      float border = isBaseClipmap ? 0 : Border;
      for (int level = 0; level < clipmap.NumberOfLevels; level++)
      {
        if (clipmap.InvalidRegions[level] == null)
          clipmap.InvalidRegions[level] = new List<Aabb>();

        // Compute AABB of whole level.
        float borderWorld = border * clipmap.ActualCellSizes[level];
        Vector2F newOrigin = clipmap.Origins[level];
        var aabb = new Aabb
        {
          Minimum =
          {
            X = newOrigin.X - borderWorld,
            Y = float.MinValue,
            Z = newOrigin.Y - borderWorld
          },
          Maximum =
          {
            X = newOrigin.X + clipmap.LevelSizes[level] + borderWorld,
            Y = float.MaxValue,
            Z = newOrigin.Y + clipmap.LevelSizes[level] + borderWorld
          }
        };

        // Store AABB of whole clipmap.
        if (level == clipmap.NumberOfLevels - 1)
          clipmap.Aabb = aabb;

        // For debugging:
        //var oldOrigin = clipmap.OldOrigins[level];
        //if (oldOrigin != newOrigin)
        //  clipmap.InvalidRegions[level].Add(aabb);

        if (!clipmap.UseIncrementalUpdate)
        {
          // Incremental update not possible. --> The whole clipmap is the invalid region.
          clipmap.InvalidRegions[level].Add(aabb);
        }
        else
        {
          // The clipmap contains info from the last frame and the layers where not changed.
          // We can use a toroidal update.

          Vector2F oldOrigin = clipmap.OldOrigins[level];
          float levelSize = clipmap.LevelSizes[level];
          if (oldOrigin != newOrigin)
          {
            // The camera or the clipmap level origin has moved.

            if (oldOrigin.X >= newOrigin.X + levelSize
                || oldOrigin.Y >= newOrigin.Y + levelSize
                || newOrigin.X >= oldOrigin.X + levelSize
                || newOrigin.Y >= oldOrigin.Y + levelSize
                || isBaseClipmap)   // Base clipmap does not (yet) use toroidal wrap.
            {
              // We moved a lot and we cannot reuse anything from the last frame.
              clipmap.InvalidRegions[level].Clear();
              clipmap.InvalidRegions[level].Add(aabb);
              clipmap.Offsets[level] = new Vector2F(0);
            }
            else
            {
              // The origin has moved. An L shape has to be updated. --> We need two AABBs.
              // To avoid an unnecessary overlap: The horizontal AABB covers the whole width.
              // The vertical AABB is shorter to avoid overlap with the horizontal AABB.
              Aabb horizontalAabb = new Aabb();
              horizontalAabb.Minimum.X = newOrigin.X;
              horizontalAabb.Maximum.X = newOrigin.X + levelSize;
              horizontalAabb.Minimum.Y = float.MinValue;
              horizontalAabb.Maximum.Y = float.MaxValue;
              if (oldOrigin.Y <= newOrigin.Y)
              {
                // Origin moved down.
                horizontalAabb.Minimum.Z = oldOrigin.Y + levelSize;
                horizontalAabb.Maximum.Z = newOrigin.Y + levelSize;
              }
              else
              {
                // Origin moved up.
                horizontalAabb.Minimum.Z = newOrigin.Y;
                horizontalAabb.Maximum.Z = oldOrigin.Y;
              }

              Aabb verticalAabb = new Aabb();
              verticalAabb.Minimum.Y = float.MinValue;
              verticalAabb.Maximum.Y = float.MaxValue;
              verticalAabb.Minimum.Z = newOrigin.Y;
              verticalAabb.Maximum.Z = newOrigin.Y + levelSize;
              if (oldOrigin.X <= newOrigin.X)
              {
                // Origin moved right.
                verticalAabb.Minimum.X = oldOrigin.X + levelSize;
                verticalAabb.Maximum.X = newOrigin.X + levelSize;
              }
              else
              {
                // Origin moved left.
                verticalAabb.Minimum.X = newOrigin.X;
                verticalAabb.Maximum.X = oldOrigin.X;
              }

              Debug.Assert(horizontalAabb.Minimum <= horizontalAabb.Maximum);
              Debug.Assert(verticalAabb.Minimum <= verticalAabb.Maximum);
              Debug.Assert(Numeric.AreEqual(horizontalAabb.Extent.X, levelSize));

              // (Assertions need larger epsilon.)
              //Debug.Assert(Numeric.AreEqual(horizontalAabb.Extent.Z, Math.Abs(oldOrigin.Y - newOrigin.Y)));
              //Debug.Assert(Numeric.AreEqual(verticalAabb.Extent.X, Math.Abs(oldOrigin.X - newOrigin.X)));

              // Add the border for texture filtering.
              horizontalAabb.Minimum.X -= borderWorld;
              horizontalAabb.Minimum.Z -= borderWorld;
              horizontalAabb.Maximum.X += borderWorld;
              horizontalAabb.Maximum.Z += borderWorld;
              verticalAabb.Minimum.X -= borderWorld;
              verticalAabb.Minimum.Z -= borderWorld;
              verticalAabb.Maximum.X += borderWorld;
              verticalAabb.Maximum.Z += borderWorld;

              // AABBs must not overlap.
              if (oldOrigin.Y <= newOrigin.Y)
                verticalAabb.Maximum.Z = horizontalAabb.Minimum.Z;
              else
                verticalAabb.Minimum.Z = horizontalAabb.Maximum.Z;

              Debug.Assert(horizontalAabb.Minimum.X >= verticalAabb.Maximum.X
                           || horizontalAabb.Maximum.X <= verticalAabb.Minimum.X
                           || horizontalAabb.Minimum.Y >= verticalAabb.Maximum.Y
                           || horizontalAabb.Maximum.Y <= verticalAabb.Minimum.Y
                           || horizontalAabb.Minimum.Z >= verticalAabb.Maximum.Z
                           || horizontalAabb.Maximum.Z <= verticalAabb.Minimum.Z,
                           "Invalid region AABBs must not overlap.");

              if (clipmap.InvalidRegions[level].Count == 0)
              {
                clipmap.InvalidRegions[level].Add(horizontalAabb);
                clipmap.InvalidRegions[level].Add(verticalAabb);
              }
              else
              {
                AddAabbWithClipping(clipmap.InvalidRegions[level], ref horizontalAabb);
                AddAabbWithClipping(clipmap.InvalidRegions[level], ref verticalAabb);
              }
            }
          }

          if (!areInvalidRegionsClipped && invalidRegions.Count > 1)
          {
            // Clip the invalid regions stored in the Terrain once.
            _aabbList.Clear();
            for (int i = 0; i < invalidRegions.Count; i++)
            {
              var invalidAabb = invalidRegions[i];
              AddAabbWithClipping(_aabbList, ref invalidAabb);
            }

            invalidRegions.Clear();
            invalidRegions.AddRange(_aabbList);

            // Update flag (also in terrain in case other TerrainNodes use the same Terrain).
            areInvalidRegionsClipped = true;
            if (isBaseClipmap)
              terrain.AreInvalidBaseRegionsClipped = true;
            else
              terrain.AreInvalidDetailRegionsClipped = true;
          }

          if (invalidRegions.Count > 0)
          {
            // Add all invalid regions - without overlap.
            for (int i = 0; i < invalidRegions.Count; i++)
            {
              var invalidAabb = invalidRegions[i];
              AddAabbWithClipping(clipmap.InvalidRegions[level], ref invalidAabb);
            }
          }
        }

#if DEBUG
        // Assert: Invalid regions do not overlap.
        for (int i = 0; i < clipmap.InvalidRegions[level].Count; i++)
        {
          var a = clipmap.InvalidRegions[level][i];
          for (int j = i + 1; j < clipmap.InvalidRegions[level].Count; j++)
          {
            var b = clipmap.InvalidRegions[level][j];
            Debug.Assert(!HaveContactXZ(ref a, ref b));
          }
        }
#endif

        // Compute the union of all invalid regions. We can use this to early out if a layer
        // does not overlap this combined region.
        var numberOfInvalidRegions = clipmap.InvalidRegions[level].Count;
        if (numberOfInvalidRegions == 0)
        {
          clipmap.CombinedInvalidRegionsAabbs[level] = new Aabb(new Vector3F(float.NaN), new Vector3F(float.NaN));
        }
        else
        {
          clipmap.CombinedInvalidRegionsAabbs[level] = clipmap.InvalidRegions[level][0];
          for (int i = 1; i < numberOfInvalidRegions; i++)
            clipmap.CombinedInvalidRegionsAabbs[level].Grow(clipmap.InvalidRegions[level][i]);
        }

        // For debugging:
        //clipmap.CombinedInvalidRegionsAabbs[level] = new Aabb(new Vector3F(float.MinValue), new Vector3F(float.MaxValue));
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private void ProcessLayer(GraphicsDevice graphicsDevice, RenderContext context, TerrainClipmap clipmap, bool isBaseClipmap, IInternalTerrainLayer layer, Aabb tileAabb)
    {
      // The material needs to have a "Base" or "Detail" render pass.
      if (!layer.Material.Contains(context.RenderPass))
        return;

      var layerAabb = layer.Aabb ?? tileAabb;
      //if (!HaveContactXZ(ref layerAabb, ref clipmap.Aabb))
      //  continue;

      bool isInInvalidRegion = false;
      for (int level = clipmap.NumberOfLevels - 1; level >= 0; level--)
      {
        for (int i = 0; i < clipmap.InvalidRegions[level].Count; i++)
        {
          var aabb = clipmap.InvalidRegions[level][i];
          if (HaveContactXZ(ref layerAabb, ref aabb))
          {
            isInInvalidRegion = true;
            break;
          }
        }
        if (isInInvalidRegion)
          break;
      }

      if (!isInInvalidRegion)
        return;

      // Reset render state for each layer because layers are allowed to change render state
      // without restoring it in a restore pass.
      // The base clipmap uses no blending. The detail clipmap uses alpha-blending (but only in
      // RGB, A is not changed, A is used e.g. for hole info).
      graphicsDevice.BlendState = isBaseClipmap ? BlendState.Opaque : BlendStateAlphaBlendRgb;
      graphicsDevice.RasterizerState = RasterizerStateCullNoneWithScissorTest;
      graphicsDevice.DepthStencilState = DepthStencilState.None;

      #region ----- Effect binding updates -----

      // Get the EffectBindings and the Effect for the current render pass.
      EffectBinding materialInstanceBinding = layer.MaterialInstance[context.RenderPass];
      EffectBinding materialBinding = layer.Material[context.RenderPass];
      EffectEx effectEx = materialBinding.EffectEx;
      Effect effect = materialBinding.Effect;

      context.MaterialInstanceBinding = materialInstanceBinding;
      context.MaterialBinding = materialBinding;

      if (effectEx.Id == 0)
      {
        effectEx.Id = 1;

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
      }

      // Update and apply material bindings. 
      // If this material is the same as in the last ProcessLayer() call, then we can skip this.
      if (_previousMaterialBinding != materialBinding)
      {
        _previousMaterialBinding = materialBinding;


        if (materialBinding.Id == 0)
        {
          materialBinding.Id = 1;
          foreach (var binding in materialBinding.ParameterBindings)
          {
            binding.Update(context);
            binding.Apply(context);
          }
        }
        else
        {
          // The material has already been updated in this frame.
          foreach (var binding in materialBinding.ParameterBindings)
            binding.Apply(context);
        }
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
      #endregion

      int border = isBaseClipmap ? 0 : Border;

      // Region covered by the layer.
      Vector2F layerStart = new Vector2F(layerAabb.Minimum.X, layerAabb.Minimum.Z);
      Vector2F layerEnd = new Vector2F(layerAabb.Maximum.X, layerAabb.Maximum.Z);

      // Clamp to tile AABB
      layerStart.X = Math.Max(layerStart.X, tileAabb.Minimum.X);
      layerStart.Y = Math.Max(layerStart.Y, tileAabb.Minimum.Z);
      layerEnd.X = Math.Min(layerEnd.X, tileAabb.Maximum.X);
      layerEnd.Y = Math.Min(layerEnd.Y, tileAabb.Maximum.Z);

      // Loop over clipmap levels.
      for (int level = 0; level < clipmap.NumberOfLevels; level++)
      {
        // If there are no invalid regions, the combined AABB is NaN.
        if (Numeric.IsNaN(clipmap.CombinedInvalidRegionsAabbs[level].Minimum.X))
          continue;

        if (layer.FadeInStart > level || layer.FadeOutEnd <= level)
          continue;

        if (!HaveContactXZ(ref layerAabb, ref clipmap.CombinedInvalidRegionsAabbs[level]))
          continue;

        Vector2F levelOrigin = clipmap.Origins[level];
        Vector2F levelSize = new Vector2F(clipmap.LevelSizes[level]); // without border!
        Vector2F levelEnd = levelOrigin + levelSize;

        float cellsPerLevelWithoutBorder = clipmap.CellsPerLevel - 2 * border;
        float cellSize = clipmap.ActualCellSizes[level];
        float texelsPerUnit = 1 / cellSize;

        // Following effect parameters change per clipmap level:
        UpdateAndApplyParameter(materialBinding, "TerrainClipmapLevel", (float)level, context);
        UpdateAndApplyParameter(materialBinding, "TerrainClipmapCellSize", cellSize, context);

        // The rectangle of the whole clipmap level (including border).
        var levelRect = GetScreenSpaceRectangle(clipmap, level);

        // The pixel position of the offset.
        int offsetX = levelRect.X + border + (int)(cellsPerLevelWithoutBorder * clipmap.Offsets[level].X + 0.5f);
        int offsetY = levelRect.Y + border + (int)(cellsPerLevelWithoutBorder * clipmap.Offsets[level].Y + 0.5f);

        // Handle the 4 rectangles of the toroidally wrapped clipmap.
        bool applyPass = true;
        for (int i = 0; i < 4; i++)
        {
          Rectangle quadrantRect;
          Vector2F offsetPosition;
          switch (i)
          {
            case 0:
              // Top left rectangle.
              quadrantRect = new Rectangle(levelRect.X, levelRect.Y, offsetX - levelRect.X, offsetY - levelRect.Y);
              offsetPosition = levelEnd;
              break;
            case 1:
              // Top right rectangle.
              quadrantRect = new Rectangle(offsetX, levelRect.Y, levelRect.Right - offsetX, offsetY - levelRect.Y);
              offsetPosition.X = levelOrigin.X;
              offsetPosition.Y = levelEnd.Y;
              break;
            case 2:
              // Bottom left rectangle.
              quadrantRect = new Rectangle(levelRect.X, offsetY, offsetX - levelRect.X, levelRect.Bottom - offsetY);
              offsetPosition.X = levelEnd.X;
              offsetPosition.Y = levelOrigin.Y;
              break;
            default:
              // Bottom right rectangle.
              quadrantRect = new Rectangle(offsetX, offsetY, levelRect.Right - offsetX, levelRect.Bottom - offsetY);
              offsetPosition = levelOrigin;
              break;
          }

          if (quadrantRect.Width == 0 || quadrantRect.Height == 0)
            continue;

          applyPass |= UpdateAndApplyParameter(materialBinding, "TerrainClipmapOffsetWorld", (Vector2)offsetPosition, context);
          applyPass |= UpdateAndApplyParameter(materialBinding, "TerrainClipmapOffsetScreen", new Vector2(offsetX, offsetY), context);

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
                applyPass = true;
              }
            }

            if (applyPass)
            {
              pass.Apply();
              applyPass = false;
            }

            foreach (var aabb in clipmap.InvalidRegions[level])
            {
              // Intersect layer AABB with invalid region AABB.
              Vector2F clippedLayerStart, clippedLayerEnd;
              clippedLayerStart.X = Math.Max(layerStart.X, aabb.Minimum.X);
              clippedLayerStart.Y = Math.Max(layerStart.Y, aabb.Minimum.Z);
              clippedLayerEnd.X = Math.Min(layerEnd.X, aabb.Maximum.X);
              clippedLayerEnd.Y = Math.Min(layerEnd.Y, aabb.Maximum.Z);

              // Nothing to do if layer AABB does not intersect invalid region.
              if (clippedLayerStart.X >= clippedLayerEnd.X || clippedLayerStart.Y >= clippedLayerEnd.Y)
                continue;

              // Compute screen space rectangle of intersection (relative to toroidal offset).
              var invalidRect = GetScissorRectangle(
                clippedLayerStart - offsetPosition,
                clippedLayerEnd - clippedLayerStart,
                texelsPerUnit);

              // Add toroidal offset screen position.
              invalidRect.X += offsetX;
              invalidRect.Y += offsetY;

              // Set a scissor rectangle to avoid drawing outside the current toroidal wrap
              // part and outside the invalid region.
              var scissorRect = Rectangle.Intersect(quadrantRect, invalidRect);
              if (scissorRect.Width <= 0 || scissorRect.Height <= 0)
                continue;

              graphicsDevice.ScissorRectangle = scissorRect;

              // Compute world space position of scissor rectangle corners.
              Vector2F start, end;
              start.X = offsetPosition.X + (scissorRect.X - offsetX) * cellSize;
              start.Y = offsetPosition.Y + (scissorRect.Y - offsetY) * cellSize;
              end.X = offsetPosition.X + (scissorRect.Right - offsetX) * cellSize;
              end.Y = offsetPosition.Y + (scissorRect.Bottom - offsetY) * cellSize;

              Debug.Assert(Numeric.IsLessOrEqual(start.X, end.X));
              Debug.Assert(Numeric.IsLessOrEqual(start.Y, end.Y));

              layer.OnDraw(graphicsDevice, scissorRect, start, end);
            }
          }
        }
      }
    }


    private static void ClearFlags(IInternalTerrainLayer layer, RenderContext context)
    {
      EffectBinding materialBinding;
      if (layer.Material.TryGet(context.RenderPass, out materialBinding))
      {
        materialBinding.EffectEx.Id = 0;
        materialBinding.Id = 0;
      }
    }


    private static Rectangle GetScissorRectangle(Vector2F topLeft, Vector2F size, float texelsPerUnit)
    {
      // Snap invalid regions to grid.
      // We could round the start values down and the end values up, but this could
      // create a 1 texel overlap of neighbor AABBs. Instead we use the same rounding
      // for start and end.
      int rectX0 = (int)RoundToGrid(topLeft.X * texelsPerUnit, 1);
      int rectY0 = (int)RoundToGrid(topLeft.Y * texelsPerUnit, 1);
      int rectX1 = (int)RoundToGrid((topLeft.X + size.X) * texelsPerUnit, 1);
      int rectY1 = (int)RoundToGrid((topLeft.Y + size.Y) * texelsPerUnit, 1);

      return new Rectangle(rectX0, rectY0, (rectX1 - rectX0), (rectY1 - rectY0));
    }
    #endregion
  }
}
#endif
