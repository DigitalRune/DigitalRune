// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Provides effect bindings for rendering a <see cref="Terrain"/>.
  /// </summary>
  public class TerrainEffectBinder : DictionaryEffectBinder
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainEffectBinder"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public TerrainEffectBinder()
    {
      var d = SingleBindings;
      d.Add(TerrainEffectParameterSemantics.TerrainHoleThreshold, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetHoleThreshold));
      d.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapCellSize, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetBaseClipmapCellSize));
      d.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapCellsPerLevel, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetBaseClipmapCellsPerLevel));
      d.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapNumberOfLevels, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetBaseClipmapNumberOfLevels));
      d.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapNumberOfColumns, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetBaseClipmapNumberOfColumns));
      d.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapLevelBias, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetBaseClipmapLevelBias));
      d.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapCellsPerLevel, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDetailClipmapCellsPerLevel));
      d.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapNumberOfLevels, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDetailClipmapNumberOfLevels));
      d.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapNumberOfColumns, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDetailClipmapNumberOfColumns));
      d.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapLevelBias, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDetailClipmapLevelBias));
      d.Add(TerrainEffectParameterSemantics.TerrainDetailFadeRange, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDetailFadeRange));
      d.Add(TerrainEffectParameterSemantics.TerrainEnableAnisotropicFiltering, (e, p, o) => CreateDelegateParameterBinding<float>(e, p, GetDetailEnableAnisotropicFiltering));

      d = SingleArrayBindings;
      d.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapCellSizes, (e, p, o) => CreateDelegateParameterArrayBinding<float>(e, p, GetDetailClipmapCellSizes));

      d = Vector2Bindings;
      d.Add(TerrainEffectParameterSemantics.TerrainTileOrigin, (e, p, o) => CreateDelegateParameterBinding<Vector2>(e, p, GetTerrainOrigin));
      d.Add(TerrainEffectParameterSemantics.TerrainTileSize, (e, p, o) => CreateDelegateParameterBinding<Vector2>(e, p, GetTerrainSize));
      d.Add(TerrainEffectParameterSemantics.TerrainTileHeightTextureSize, (e, p, o) => CreateDelegateParameterBinding<Vector2>(e, p, GetTerrainHeightTextureSize));
      d.Add(TerrainEffectParameterSemantics.TerrainTileNormalTextureSize, (e, p, o) => CreateDelegateParameterBinding<Vector2>(e, p, GetTerrainNormalTextureSize));
      d.Add(TerrainEffectParameterSemantics.TerrainTileHoleTextureSize, (e, p, o) => CreateDelegateParameterBinding<Vector2>(e, p, GetTerrainHoleTextureSize));

      d = Vector2ArrayBindings;
      d.Add(TerrainEffectParameterSemantics.TerrainBaseClipmapOrigins, (e, p, o) => CreateDelegateParameterArrayBinding<Vector2>(e, p, GetBaseClipmapOrigins));
      d.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapOrigins, (e, p, o) => CreateDelegateParameterArrayBinding<Vector2>(e, p, GetDetailClipmapOrigins));
      d.Add(TerrainEffectParameterSemantics.TerrainDetailClipmapOffsets, (e, p, o) => CreateDelegateParameterArrayBinding<Vector2>(e, p, GetDetailClipmapOffsets));

      d = Vector4ArrayBindings;
      d.Add(TerrainEffectParameterSemantics.TerrainClearValues, (e, p, o) => CreateDelegateParameterArrayBinding<Vector4>(e, p, GetTerrainClearValues));

      // TODO: TextureBindings and Texture2DBindings duplicated!
      d = TextureBindings;
      d.Add(TerrainEffectParameterSemantics.TerrainTileHeightTexture, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetTerrainHeightTexture));
      d.Add(TerrainEffectParameterSemantics.TerrainTileNormalTexture, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetTerrainNormalTexture));
      d.Add(TerrainEffectParameterSemantics.TerrainTileHoleTexture, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetTerrainHoleTexture));
      d.Add(TerrainEffectParameterSemantics.TerrainBaseClipmap, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetBaseClipmapTexture));
      d.Add(TerrainEffectParameterSemantics.TerrainDetailClipmap, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetDetailClipmapTexture));

      d = Texture2DBindings;
      d.Add(TerrainEffectParameterSemantics.TerrainTileHeightTexture, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetTerrainHeightTexture));
      d.Add(TerrainEffectParameterSemantics.TerrainTileNormalTexture, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetTerrainNormalTexture));
      d.Add(TerrainEffectParameterSemantics.TerrainTileHoleTexture, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetTerrainHoleTexture));
      d.Add(TerrainEffectParameterSemantics.TerrainBaseClipmap, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetBaseClipmapTexture));
      d.Add(TerrainEffectParameterSemantics.TerrainDetailClipmap, (e, p, o) => CreateDelegateParameterBinding<Texture2D>(e, p, GetDetailClipmapTexture));
    }


    #region ----- Terrain -----

    private static void GetTerrainClearValues(DelegateParameterArrayBinding<Vector4> binding, RenderContext context, Vector4[] values)
    {
      var terrain = GetTerrainNode(context).Terrain;
      var clearValues = (context.RenderPass == "Base") ? terrain.BaseClearValues : terrain.DetailClearValues;
      for (int i = 0; i < Math.Min(clearValues.Length, values.Length); i++)
        values[i] = (Vector4)clearValues[i];
    }

    #endregion

    #region ----- TerrainTile -----

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static TerrainTile GetTerrainTile(RenderContext context)
    {
      var terrainTile = context.Object as TerrainTile;
      if (terrainTile == null)
        throw new EffectBindingException("Cannot resolve terrain tile. TerrainTile needs to be set in RenderContext.Object.");

      return terrainTile;
    }


    private static Vector2 GetTerrainOrigin(DelegateParameterBinding<Vector2> binding, RenderContext context)
    {
      var terrainTile = GetTerrainTile(context);
      return new Vector2(terrainTile.OriginX, terrainTile.OriginZ);
    }


    private static Vector2 GetTerrainSize(DelegateParameterBinding<Vector2> binding, RenderContext context)
    {
      var terrainTile = GetTerrainTile(context);
      return new Vector2(terrainTile.WidthX, terrainTile.WidthZ);
    }


    private static Texture2D GetTerrainHeightTexture(DelegateParameterBinding<Texture2D> binding, RenderContext context)
    {
      return GetTerrainTile(context).HeightTexture ?? context.GraphicsService.GetDefaultTexture2DBlack();
    }


    private static Vector2 GetTerrainHeightTextureSize(DelegateParameterBinding<Vector2> binding, RenderContext context)
    {
      var texture = GetTerrainHeightTexture(null, context);
      return new Vector2(texture.Width, texture.Height);
    }


    private static Texture2D GetTerrainNormalTexture(DelegateParameterBinding<Texture2D> binding, RenderContext context)
    {
      return GetTerrainTile(context).NormalTexture ?? context.GraphicsService.GetDefaultNormalTexture();
    }


    private static Vector2 GetTerrainNormalTextureSize(DelegateParameterBinding<Vector2> binding, RenderContext context)
    {
      var texture = GetTerrainNormalTexture(null, context);
      return new Vector2(texture.Width, texture.Height);
    }


    private static Texture2D GetTerrainHoleTexture(DelegateParameterBinding<Texture2D> binding, RenderContext context)
    {
      return GetTerrainTile(context).HoleTexture ?? context.GraphicsService.GetDefaultTexture2DWhite();
    }


    private static Vector2 GetTerrainHoleTextureSize(DelegateParameterBinding<Vector2> binding, RenderContext context)
    {
      var texture = GetTerrainHoleTexture(null, context);
      return new Vector2(texture.Width, texture.Height);
    }

    #endregion

    #region ----- TerrainNode -----

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static TerrainNode GetTerrainNode(RenderContext context)
    {
      var terrainNode = context.SceneNode as TerrainNode;
      if (terrainNode == null)
        throw new EffectBindingException("Cannot resolve terrain node. TerrainNode needs to be set in RenderContext.SceneNode.");

      return terrainNode;
    }


    private static float GetHoleThreshold(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).HoleThreshold;
    }


    private static Texture2D GetBaseClipmapTexture(DelegateParameterBinding<Texture2D> binding, RenderContext context)
    {
      var usage = binding.Description;
      int index = (usage != null) ? usage.Index : 0;
      var textures = GetTerrainNode(context).BaseClipmap.Textures;
      if (index < textures.Length)
        return textures[index];

      return null;
    }


    private static Texture2D GetDetailClipmapTexture(DelegateParameterBinding<Texture2D> binding, RenderContext context)
    {
      var usage = binding.Description;
      int index = (usage != null) ? usage.Index : 0;
      var textures = GetTerrainNode(context).DetailClipmap.Textures;
      if (index < textures.Length)
        return textures[index];

      return null;
    }


    private static float GetBaseClipmapCellSize(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).BaseClipmap.CellSizes[0];
    }


    private static void GetDetailClipmapCellSizes(DelegateParameterArrayBinding<float> binding, RenderContext context, float[] values)
    {
      var cellSizes = GetTerrainNode(context).DetailClipmap.CellSizes;
      for (int i = 0; i < Math.Min(cellSizes.Length, values.Length); i++)
        values[i] = cellSizes[i];

      for (int i = 1; i < values.Length; i++)
        if (Numeric.IsNaN(values[i]))
          values[i] = values[i - 1] * 2;
    }


    private static float GetBaseClipmapCellsPerLevel(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).BaseClipmap.CellsPerLevel;
    }


    private static float GetBaseClipmapNumberOfLevels(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).BaseClipmap.NumberOfLevels;
    }


    private static float GetBaseClipmapNumberOfColumns(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).BaseClipmap.NumberOfTextureAtlasColumns;
    }


    private static float GetBaseClipmapLevelBias(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).BaseClipmap.LevelBias;
    }


    private static void GetBaseClipmapOrigins(DelegateParameterArrayBinding<Vector2> binding, RenderContext context, Vector2[] values)
    {
      var origins = GetTerrainNode(context).BaseClipmap.Origins;
      for (int i = 0; i < Math.Min(origins.Length, values.Length); i++)
        values[i] = (Vector2)origins[i];
    }


    private static float GetDetailClipmapCellsPerLevel(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).DetailClipmap.CellsPerLevel;
    }


    private static float GetDetailClipmapNumberOfLevels(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).DetailClipmap.NumberOfLevels;
    }


    private static float GetDetailClipmapNumberOfColumns(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).DetailClipmap.NumberOfTextureAtlasColumns;
    }


    private static float GetDetailClipmapLevelBias(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).DetailClipmap.LevelBias;
    }


    private static float GetDetailFadeRange(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).DetailFadeRange;
    }


    private static float GetDetailEnableAnisotropicFiltering(DelegateParameterBinding<float> binding, RenderContext context)
    {
      return GetTerrainNode(context).DetailClipmap.EnableAnisotropicFiltering ? 1 : 0;
    }


    private static void GetDetailClipmapOrigins(DelegateParameterArrayBinding<Vector2> binding, RenderContext context, Vector2[] values)
    {
      var origins = GetTerrainNode(context).DetailClipmap.Origins;
      for (int i = 0; i < Math.Min(origins.Length, values.Length); i++)
        values[i] = (Vector2)origins[i];
    }


    private static void GetDetailClipmapOffsets(DelegateParameterArrayBinding<Vector2> binding, RenderContext context, Vector2[] values)
    {
      var offsets = GetTerrainNode(context).DetailClipmap.Offsets;
      for (int i = 0; i < Math.Min(offsets.Length, values.Length); i++)
        values[i] = (Vector2)offsets[i];
    }
    #endregion
  }
}
