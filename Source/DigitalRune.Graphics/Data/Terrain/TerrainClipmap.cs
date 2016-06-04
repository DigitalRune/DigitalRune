// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a clipmap which stores terrain data.
  /// </summary>
  /// <remarks>
  /// <para>
  /// For a basic introduction to clipmaps, see <see href="https://en.wikipedia.org/wiki/Clipmap">
  /// Clipmaps (Wikipedia)</see>.
  /// </para>
  /// <para>
  /// The clipmap can store geometry information (e.g. heights, normals, holes) or material
  /// information (e.g. diffuse color, specular color). The usage of a clipmap can vary and is not
  /// defined by this class itself. The only restriction is that all textures of this clipmap use
  /// the same texture format.
  /// </para>
  /// <para>
  /// The clipmap consists of several levels, which are similar to mipmap levels. Level 0 has the
  /// finest resolution. All other levels have a lower resolution. The cell size (= texel size) is
  /// usually doubled between levels, but this can be configured using the property
  /// <see cref="CellSizes"/>. For example: A clipmap which stores terrain height values can use a
  /// resolution of 1 meter per cell at level 0, 2 meters per cell at level 1, 4 meters per cell at
  /// level 2, etc.
  /// </para>
  /// <para>
  /// There is one texture for each clipmap level. All texture have the same size which is defined
  /// by <see cref="CellsPerLevel"/>. The number of levels is defined by
  /// <see cref="NumberOfLevels"/>.
  /// </para>
  /// <para>
  /// Further a clipmap can consist of several textures per level, e.g. one texture to store detail
  /// normal vectors, a second texture to store diffuse color, a third texture to store specular
  /// color. Because of XNA limitations it might be necessary to combine several textures into a
  /// single texture atlas, where each texture atlas contains all levels with the same content. For
  /// example: One texture atlas contains all level textures with detail normal vectors, a second
  /// texture atlas contains all level textures with diffuse colors, the third texture atlas
  /// contains all level textures with specular colors. - All textures can be accessed using the
  /// <see cref="Textures"/> collection. (It can be helpful to visualize the textures for
  /// debugging).
  /// </para>
  /// </remarks>
  public sealed class TerrainClipmap : IDisposable
  {
    // Possible optimizations:
    // - Currently clipping invalid regions is O(n²). This could be made O(n*log(n)) using e.g. a 
    //   sweep line algorithm (similar to sweep-and-prune).
    // - Terrain layers are stored in a flat list. If there are many layers, we could store them
    //   in an AABB tree. The tree can be used to "get all layers that touch invalid region x".
    //   Note that the query must not change the render order of the layers!
    //   However, the number of layers is usually low. Many decals should be handled by 
    //   TerrainDecalInstancingLayer.

    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// The maximum number of clipmap levels.
    /// </summary>
    public const int MaxNumberOfLevels = 9;
    #endregion
    
   
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    internal SurfaceFormat Format { get; private set; }


    /// <summary>
    /// Gets the cell sizes of all clipmap levels.
    /// </summary>
    /// <value>The cell sizes of all clipmap levels.</value>
    /// <remarks>
    /// <para>
    /// This array contains <see cref="MaxNumberOfLevels"/> elements - where only the first
    /// <see cref="NumberOfLevels"/> elements are used.
    /// </para>
    /// <para>
    /// The first element <c>CellSizes[0]</c> has to be set. All other entries can contain NaN
    /// values, which means that subsequent cell sizes are chosen automatically. By default, each
    /// cell size is twice the cell size of the previous level.
    /// </para>
    /// <para>
    /// Example: The array contains the values { 1, NaN, NaN, NaN, NaN, ... }. In this case the
    /// first level uses a cell size of 1 unit. The second level has a cell size of 2 units. The
    /// third level has a cell size of 4 units per cell. Etc.
    /// </para>
    /// <para>
    /// The default terrain renderers ( <see cref="TerrainClipmapRenderer"/> and
    /// <see cref="TerrainRenderer"/>) use only <c>CellSizes[0]</c> for the
    /// <see cref="TerrainNode.BaseClipmap"/> (all other cell sizes are treated as if they are NaN
    /// or twice the size of the previous level). The <see cref="TerrainNode.DetailClipmap"/> can
    /// have user-defined cell sizes for all levels.
    /// </para>
    /// <para>
    /// <see cref="Invalidate"/> has to be called if the elements in this array are changed!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public float[] CellSizes { get; private set; }


    // The actual cell sizes (no NaN values).
    internal float[] ActualCellSizes = new float[MaxNumberOfLevels];


    /// <summary>
    /// Gets or sets the number of cells (texels) per clipmap level.
    /// </summary>
    /// <value>The number of cells (texels) per clipmap level.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public int CellsPerLevel
    {
      get { return _cellsPerLevel; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException("value", "The number of cells per level must be greater than 0.");

        if (_cellsPerLevel == value)
          return;

        _cellsPerLevel = value;
        Invalidate();
      }
    }
    private int _cellsPerLevel;


    /// <summary>
    /// Gets or sets the number of clipmap levels.
    /// </summary>
    /// <value>The number of clipmap levels.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is outside the range [1, <see cref="MaxNumberOfLevels"/>].
    /// </exception>
    public int NumberOfLevels
    {
      get { return _numberOfLevels; }
      set
      {
        if (value < 1 || value > MaxNumberOfLevels)
        {
          string message = string.Format(CultureInfo.InvariantCulture, "Number of levels must be a value in the range [1, {0}].", MaxNumberOfLevels);
          throw new ArgumentOutOfRangeException("value", message);
        }

        if (_numberOfLevels == value)
          return;

        _numberOfLevels = value;
        Invalidate();
      }
    }
    private int _numberOfLevels;


    /// <summary>
    /// Gets or sets the level bias.
    /// </summary>
    /// <value>The level bias.</value>
    /// <remarks>
    /// This value can be used to modify the distance of the level of detail (LOD) transitions. Use
    /// a positive value to make the LOD transitions appear closer to the camera (reduce quality).
    /// </remarks>
    public float LevelBias { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the clipmap textures use mipmaps.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the clipmap textures use mipmaps; otherwise,
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    public bool EnableMipMap
    {
      get { return _enableMipMap; }
      set
      {
        if (_enableMipMap == value)
          return;

        _enableMipMap = value;
        Invalidate();
      }
    }
    private bool _enableMipMap;


    /// <summary>
    /// Gets or sets a value indicating whether the clipmap is sampled using anisotropic
    /// filtering.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the clipmap is sampled using anisotropic filtering;
    /// otherwise, <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// On AMD GPUs it is also necessary to enable mipmaps (see <see cref="EnableMipMap"/>) for
    /// anisotropic filtering. This is not necessary on Intel or Nvidia GPUs. (Note that mipmap
    /// generation for terrain clipmaps is very expensive. Therefore, consider disabling anisotropic
    /// filtering on AMD GPUs.)
    /// </remarks>
    public bool EnableAnisotropicFiltering
    {
      get { return _enableAnisotropicFiltering; }
      set
      {
        if (_enableAnisotropicFiltering == value)
          return;

        _enableAnisotropicFiltering = value;
        Invalidate();
      }
    }
    private bool _enableAnisotropicFiltering;


    /// <summary>
    /// Gets or sets the index of the first level which is actively used.
    /// </summary>
    /// <value>
    /// The index of the first level which is actively used.
    /// The default value is 0.
    /// </value>
    /// <remarks>
    /// Per default, all clipmap levels from index 0 (most detailed level) to index 
    /// <see cref="NumberOfLevels"/> - 1 (least detailed level) are in use. In certain situations
    /// it makes sense to skip the most detailed levels, for example if player is high above 
    /// the ground or moving very fast. For this case <see cref="MinLevel"/> can be set to a value
    /// in [0, <see cref="NumberOfLevels"/> - 1]. The clipmap renderer will not update the clipmap
    /// levels with an index smaller than <see cref="MinLevel"/>.
    /// </remarks>
    public float MinLevel { get; set; }


    // Set in Invalidate. Reset by renderer.
    internal bool UseIncrementalUpdate;


    // ----- Set automatically by Renderer:

    /// <summary>
    /// Gets the clipmap textures.
    /// </summary>
    /// <value>The clipmap textures.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public Texture2D[] Textures { get; private set; }


    /// <summary>
    /// Gets the world space origin of each clipmap level.
    /// </summary>
    /// <value>The world space origin of each clipmap level.</value>
    /// <remarks>
    /// <para>
    /// This array contains <see cref="MaxNumberOfLevels"/> elements. Only the first
    /// <see cref="NumberOfLevels"/> elements are used.
    /// </para>
    /// <para>
    /// Unlike the origin of <see cref="TerrainTile"/>s, this origin corresponds to the texture
    /// coordinate (0, 0) (= the corner of the texture). The origin is not in the center of the 
    /// first texel. 
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public Vector2F[] Origins { get; private set; }


    // Origins of the last frame.
    internal Vector2F[] OldOrigins = new Vector2F[MaxNumberOfLevels];


    internal int NumberOfTextureAtlasColumns
    {
      get
      {
        if (Textures[0] == null)
          return 0;

        return Textures[0].Width / CellsPerLevel;
      }
    }

    // The offsets in texture coordinates ([0, 1]) for toroidal wrapping.
    // (Computes as if filter border is 0!)
    internal Vector2F[] Offsets { get; private set; }

    // Offsets of the last frame.
    internal Vector2F[] OldOffsets = new Vector2F[MaxNumberOfLevels];

    // The level sizes in world space units without a pixel border for texture filtering.
    internal float[] LevelSizes = new float[MaxNumberOfLevels];

    // AABBs of regions which need to be updated.
    internal List<Aabb>[] InvalidRegions = new List<Aabb>[MaxNumberOfLevels];

    // The combined AABB of all invalid regions.
    internal Aabb[] CombinedInvalidRegionsAabbs = new Aabb[MaxNumberOfLevels];

    // The AABB of the whole clipmap.
    internal Aabb Aabb;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainClipmap"/> class.
    /// </summary>
    /// <param name="numberOfTextures">The number of textures in the range [1, 4].</param>
    /// <param name="format">The texture surface format.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Invalid <paramref name="numberOfTextures"/>.
    /// </exception>
    public TerrainClipmap(int numberOfTextures, SurfaceFormat format)
    {
      if (numberOfTextures < 1 || numberOfTextures > 4)
        throw new ArgumentOutOfRangeException("numberOfTextures", "Number of textures must be in the range [1, 4].");

      CellSizes = new float[MaxNumberOfLevels];
      CellSizes[0] = 1;
      for (int i = 1; i < CellSizes.Length; i++)
        CellSizes[i] = float.NaN;

      CellsPerLevel = 64;
      NumberOfLevels = 6;
      EnableAnisotropicFiltering = true;
      Origins = new Vector2F[MaxNumberOfLevels];
      Textures = new Texture2D[numberOfTextures];
      Format = format;
      Offsets = new Vector2F[MaxNumberOfLevels];

      Invalidate();
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="TerrainClipmap"/> class.
    /// </summary>
    public void Dispose()
    {
      for (int i = 0; i < Textures.Length; i++)
      {
        Textures[i].SafeDispose();
        Textures[i] = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Invalidates the cached clipmap.
    /// </summary>
    /// <remarks>
    /// This method is called automatically when clipmap properties are modified. It is usually not
    /// necessary to call this method manually.
    /// </remarks>
    public void Invalidate()
    {
      UseIncrementalUpdate = false;

      // Compute actual cell sizes.
      float previousCellSize = 1;
      for (int level = 0; level < NumberOfLevels; level++)
      {
        ActualCellSizes[level] = CellSizes[level];
        if (Numeric.IsNaN(ActualCellSizes[level]))
          ActualCellSizes[level] = previousCellSize * 2;

        previousCellSize = ActualCellSizes[level];
      }
    }
    #endregion
  }
}
