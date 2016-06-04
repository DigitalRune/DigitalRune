// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Defines the semantics for effect parameters used by the <see cref="Terrain"/>.
  /// </summary>
  /// <inheritdoc cref="SceneEffectParameterSemantics"/>
  public static class TerrainEffectParameterSemantics
  {
    #region ----- Terrain -----

    /// <summary>
    /// The clear values written into the <see cref="TerrainClipmap"/> (array of
    /// <see cref="Vector4"/>).
    /// </summary>
    public const string TerrainClearValues = "TerrainClearValues";
    #endregion


    #region ----- TerrainTile -----

    /// <summary>
    /// The world space origin (<see cref="TerrainTile.OriginX"/>, <see cref="TerrainTile.OriginZ"/>)
    /// of the terrain tile given as <see cref="Vector2"/>.
    /// </summary>
    public const string TerrainTileOrigin = "TerrainTileOrigin";


    /// <summary>
    /// The world space size (<see cref="TerrainTile.WidthX"/>, <see cref="TerrainTile.WidthZ"/>)
    /// of the terrain tile given as <see cref="Vector2"/>.
    /// </summary>
    public const string TerrainTileSize = "TerrainTileSize";


    /// <summary>
    /// The height texture of the terrain tile which stores absolute height values in the Red
    /// channel (<see cref="Texture2D"/>).
    /// </summary>
    public const string TerrainTileHeightTexture = "TerrainTileHeightTexture";


    /// <summary>
    /// The size of the <see cref="TerrainTileHeightTexture"/> in texels (<see cref="Vector2"/>).
    /// </summary>
    public const string TerrainTileHeightTextureSize = "TerrainTileHeightTextureSize";


    /// <summary>
    /// The normal texture of the terrain tile which store normal vectors (<see cref="Texture2D"/>).
    /// </summary>
    public const string TerrainTileNormalTexture = "TerrainTileNormalTexture";


    /// <summary>
    /// The size of the <see cref="TerrainTileNormalTexture"/> in texels (<see cref="Vector2"/>).
    /// </summary>
    public const string TerrainTileNormalTextureSize = "TerrainTileNormalTextureSize";


    /// <summary>
    /// The hole texture of the terrain tile which stores hole information in the Alpha channel
    /// (<see cref="Texture2D"/>).
    /// </summary>
    public const string TerrainTileHoleTexture = "TerrainTileHoleTexture";


    /// <summary>
    /// The size of the <see cref="TerrainTileHoleTexture"/> in texels (<see cref="Vector2"/>).
    /// </summary>
    public const string TerrainTileHoleTextureSize = "TerrainTileHoleTextureSize";
    #endregion


    #region ----- TerrainNode -----

    /// <summary>
    /// The threshold used to check for holes in the terrain (<see cref="float"/>).
    /// </summary>
    public const string TerrainHoleThreshold = "TerrainHoleThreshold";


    /// <summary>
    /// The n-th texture in the <see cref="TerrainNode.BaseClipmap"/> (<see cref="Texture2D"/>).
    /// </summary>
    public const string TerrainBaseClipmap = "TerrainBaseClipmap";


    /// <summary>
    /// The cell size of the <see cref="TerrainNode.BaseClipmap"/> (<see cref="float"/>).
    /// </summary>
    public const string TerrainBaseClipmapCellSize = "TerrainBaseClipmapCellSize";


    /// <summary>
    /// The <see cref="TerrainClipmap.CellsPerLevel"/> of the <see cref="TerrainNode.BaseClipmap"/>
    /// (<see cref="float"/>).
    /// </summary>
    public const string TerrainBaseClipmapCellsPerLevel = "TerrainBaseClipmapCellsPerLevel";


    /// <summary>
    /// The <see cref="TerrainClipmap.NumberOfLevels"/> in the <see cref="TerrainNode.BaseClipmap"/>
    /// (<see cref="float"/>).
    /// </summary>
    public const string TerrainBaseClipmapNumberOfLevels = "TerrainBaseClipmapNumberOfLevels";


    /// <summary>
    /// The number of texture atlas columns in the  <see cref="TerrainNode.BaseClipmap"/> 
    /// (<see cref="float"/>).
    /// </summary>
    public const string TerrainBaseClipmapNumberOfColumns = "TerrainBaseClipmapNumberOfColumns";


    /// <summary>
    /// The <see cref="TerrainClipmap.LevelBias"/> of the <see cref="TerrainNode.BaseClipmap"/>
    /// (<see cref="float"/>).
    /// </summary>
    public const string TerrainBaseClipmapLevelBias = "TerrainBaseClipmapLevelBias";


    /// <summary>
    /// The <see cref="TerrainClipmap.Origins"/> of the <see cref="TerrainNode.BaseClipmap"/>
    /// (array of <see cref="float"/>).
    /// </summary>
    public const string TerrainBaseClipmapOrigins = "TerrainBaseClipmapOrigins";


    /// <summary>
    /// The n-th texture in the <see cref="TerrainNode.DetailClipmap"/> (<see cref="Texture2D"/>).
    /// </summary>
    public const string TerrainDetailClipmap = "TerrainDetailClipmap";


    /// <summary>
    /// The cell sizes of the <see cref="TerrainNode.DetailClipmap"/> (array of <see cref="float"/>).
    /// </summary>
    public const string TerrainDetailClipmapCellSizes = "TerrainDetailClipmapCellSizes";


    /// <summary>
    /// The <see cref="TerrainClipmap.CellsPerLevel"/> of the <see cref="TerrainNode.DetailClipmap"/>
    /// (<see cref="float"/>).
    /// </summary>
    public const string TerrainDetailClipmapCellsPerLevel = "TerrainDetailClipmapCellsPerLevel";


    /// <summary>
    /// The <see cref="TerrainClipmap.NumberOfLevels"/> in the <see cref="TerrainNode.DetailClipmap"/>
    /// (<see cref="float"/>).
    /// </summary>
    public const string TerrainDetailClipmapNumberOfLevels = "TerrainDetailClipmapNumberOfLevels";


    /// <summary>
    /// The number of texture atlas columns in the <see cref="TerrainNode.DetailClipmap"/> 
    /// (<see cref="float"/>).
    /// </summary>
    public const string TerrainDetailClipmapNumberOfColumns = "TerrainDetailClipmapNumberOfColumns";


    /// <summary>
    /// The <see cref="TerrainClipmap.LevelBias"/> of the <see cref="TerrainNode.DetailClipmap"/>
    /// (<see cref="float"/>).
    /// </summary>
    public const string TerrainDetailClipmapLevelBias = "TerrainDetailClipmapLevelBias";


    /// <summary>
    /// The <see cref="TerrainClipmap.Origins"/> of the <see cref="TerrainNode.DetailClipmap"/>
    /// (array of <see cref="float"/>).
    /// </summary>
    public const string TerrainDetailClipmapOrigins = "TerrainDetailClipmapOrigins";


    /// <summary>
    /// The offsets (for toroidal wrapping) of the <see cref="TerrainNode.DetailClipmap"/> 
    /// (array of <see cref="float"/>).
    /// </summary>
    public const string TerrainDetailClipmapOffsets = "TerrainDetailClipmapOffsets";


    /// <summary>
    /// The <see cref="TerrainNode.DetailFadeRange"/> (<see cref="float"/>).
    /// </summary>
    public const string TerrainDetailFadeRange = "TerrainDetailFadeRange";


    /// <summary>
    /// The <see cref="TerrainClipmap.EnableAnisotropicFiltering"/> flag of the
    /// <see cref="TerrainNode.DetailClipmap"/> (<see cref="float"/>).
    /// </summary>
    public const string TerrainEnableAnisotropicFiltering = "TerrainEnableAnisotropicFiltering";
    #endregion
  }
}
