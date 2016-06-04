// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines the geometry (height, normals, holes) and material of a rectangular terrain region.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A terrain is split into one or more tiles. The terrain tiles defines the geometry (height,
  /// normals, holes) of the terrain. Each tile has a set of material layers (dirt, grass, decals,
  /// roads) that define the appearance. The material layers are applied (blended) one after the
  /// other, which means that a layer can override previous layers.
  /// </para>
  /// <para>
  /// <strong>Geometry textures:</strong><br/> The <see cref="HeightTexture"/> defines height values
  /// (absolute heights in world space) of the terrain. 
  /// </para>
  /// <para>
  /// <see cref="NormalTexture"/> contains the terrain normal vectors. This texture encodes the
  /// normal vectors like a standard "green-up" normal map, i.e. the world space +x component of the
  /// normal is stored in the red channel. The world space up (+y) component is stored in the blue
  /// channel. The <i>negative</i> world space z component is stored in the green channel.
  /// </para>
  /// <para>
  /// <see cref="HoleTexture"/> defines holes in the terrain. The texture is used like an alpha mask
  /// texture. If the alpha channel contains 0, then there is a hole in the terrain.
  /// </para>
  /// <para>
  /// <strong>Tile dimensions:</strong><br/><see cref="OriginX"/> and <see cref="OriginZ"/> define
  /// the tile origin in world space - which corresponds to center of the first texel of the
  /// textures. <see cref="CellSize"/> defines the horizontal distance between two height values.
  /// The texture coordinate u is aligned with the positive x-axis. The texture coordinate
  /// v is aligned with the positive z-axis. This means, if the cell/texel size is 1 world space
  /// unit and the texture is 1025 x 513 texels large, then the terrain tile covers the area between
  /// (OriginX, *, OriginZ) and (OriginX + 1024, *, OriginZ + 512).
  /// </para>
  /// <para>
  /// <strong>Mipmaps:</strong><br/> All textures should contain mipmaps. Ideally, the mipmaps are
  /// generated using 3 x 3 downsampling instead of the usual 2 x 2 downsampling. - Suitable
  /// textures can be created using the <see cref="TerrainHelper"/> class.
  /// </para>
  /// <para>
  /// <strong>Miscellaneous:</strong><br/> Please notes that the normal vectors depend on the scale
  /// of the terrain. That means, if height values in the <see cref="HeightTexture"/> are scaled,
  /// then the normal map has to be updated too. The normal map also needs to be updated if the
  /// <see cref="CellSize"/> is changed! (This is not done automatically. Normal textures can be
  /// created using the <see cref="TerrainHelper"/>.)
  /// </para>
  /// <para>
  /// <see cref="OriginX"/> and <see cref="OriginZ"/> should always be an integer multiple of the
  /// <see cref="CellSize"/>. For example, if the cell size is 0.5, valid origin values are -0.5, 0,
  /// 0.5, 1, etc.
  /// </para>
  /// <para>
  /// <strong>Cache invalidation:</strong><br/> When the <see cref="Terrain"/> is used with the
  /// <see cref="TerrainNode"/>, then the terrain data is cached in clipmaps. Therefore, it is
  /// important to notify the terrain system when a tile or layer has changed and the cached data is
  /// invalid. When tiles or layers are added to or removed from the terrain, this happens
  /// automatically. But when the properties or the contents of tiles/layers are changed, the
  /// affected region needs to be invalidated explicitly by calling the appropriate
  /// <see cref="Graphics.Terrain.Invalidate()"/> method of the <see cref="Terrain"/> or the
  /// <see cref="TerrainTile"/>. For example, when the contents of a height map is changed, the
  /// affected region on the terrain needs to be invalidated by calling
  /// <see cref="Graphics.Terrain.Invalidate(DigitalRune.Geometry.Shapes.Aabb)"/> or
  /// <see cref="Graphics.Terrain.Invalidate(TerrainTile)"/>.
  /// </para>
  /// <para>See <see cref="Terrain"/> and <see cref="TerrainLayer"/> for more information.</para>
  /// </remarks>
  public class TerrainTile : IDisposable, IInternalTerrainLayer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private const string DefaultMaterialKey = "DefaultTerrainTileMaterial";
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this instance has been disposed of.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance has been disposed of; otherwise,
    /// <see langword="false"/>.
    /// </value>
    public bool IsDisposed { get; private set; }


    /// <summary>
    /// Gets the terrain that owns this terrain tile.
    /// </summary>
    /// <value>The terrain that owns this terrain tile.</value>
    public Terrain Terrain { get; internal set; }


    /// <summary>
    /// Gets the axis-aligned bounding box of the terrain tile. 
    /// (Vertical min and max values are not set!)
    /// </summary>
    /// <value>The axis-aligned bounding box of the terrain tile.</value>
    /// <remarks>
    /// The min and max y values of this <see cref="Aabb"/> are 0 and should be ignored. Only the
    /// x and z values are set.
    /// </remarks>
    public Aabb Aabb { get; private set; }


    /// <inheritdoc/>
    Aabb? IInternalTerrainLayer.Aabb
    {
      get { return Aabb; }
    }


    /// <inheritdoc/>
    int IInternalTerrainLayer.FadeInStart
    {
      get { return 0; }
    }


    /// <inheritdoc/>
    int IInternalTerrainLayer.FadeOutEnd
    {
      get { return int.MaxValue; }
    }


    /// <summary>
    /// Gets or sets the material that is used to render the geometry of this terrain tile.
    /// </summary>
    /// <value>The material that is used to render the geometry of this terrain layer.</value>
    /// <remarks>
    /// If the <see cref="Material"/> is <see langword="null"/>, no geometry (heights, normals,
    /// holes) are rendered for this terrain tile.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public Material Material
    {
      get { return _material; }
      set
      {
        _material = value;
        MaterialInstance = (value != null) ? new MaterialInstance(value) : null;
      }
    }
    private Material _material;


    /// <summary>
    /// Gets the material instance.
    /// </summary>
    /// <value>The material instance.</value>
    /// <remarks>
    /// The <see cref="MaterialInstance"/> is unique to the terrain tile. When effect parameters
    /// in the material instance are changed, only the current terrain tile is affected.
    /// </remarks>
    public MaterialInstance MaterialInstance { get; private set; }


    /// <summary>
    /// Gets the terrain layers.
    /// </summary>
    /// <value>The terrain layers.</value>
    public TerrainLayerCollection Layers { get; private set; }


    /// <summary>
    /// Gets or sets the world space origin of this terrain tile on the x-axis.
    /// </summary>
    /// <value>
    /// The world space origin of this terrain tile on the x-axis. The default value is 0.
    /// </value>
    /// <remarks>
    /// The origin should be an integer multiple of the <see cref="CellSize"/>!
    /// </remarks>
    /// <seealso cref="OriginZ"/>
    public float OriginX
    {
      get { return _originX; }
      set
      {
        _originX = value;
        UpdateAabb();
      }
    }
    private float _originX;


    /// <summary>
    /// Gets or sets the world space origin of this terrain tile on the z-axis.
    /// </summary>
    /// <value>
    /// The world space origin of this terrain tile on the z-axis. The default value is 0.
    /// </value>
    /// <remarks>
    /// The origin should be an integer multiple of the <see cref="CellSize"/>!
    /// </remarks>
    /// <seealso cref="OriginX"/>
    public float OriginZ
    {
      get { return _originZ; }
      set
      {
        _originZ = value;
        UpdateAabb();
      }
    }
    private float _originZ;


    /// <summary>
    /// Gets or sets the world space size of one cell in the height texture.
    /// </summary>
    /// <value>
    /// The world space size of one cell in the height texture. The default value is 1.
    /// </value>
    public float CellSize
    {
      get { return _cellSize; }
      set
      {
        _cellSize = value;
        UpdateAabb();
      }
    }
    private float _cellSize = 1;


    /// <summary>
    /// Gets the world space size of this terrain tile along the x-axis.
    /// </summary>
    /// <value>The world space size of this terrain tile along the x-axis.</value>
    public float WidthX
    {
      get { return (HeightTexture != null) ? HeightTexture.Width * CellSize : 0; }
    }


    /// <summary>
    /// Gets the world space size of this terrain tile along the z-axis.
    /// </summary>
    /// <value>The world space size of this terrain tile along the z-axis.</value>
    public float WidthZ
    {
      get { return (HeightTexture != null) ? HeightTexture.Height * CellSize : 0; }
    }


    /// <summary>
    /// Gets or sets the height texture which stores absolute height values in the Red channel.
    /// </summary>
    /// <value>The height texture which stores absolute height values in the Red channel.</value>
    /// <remarks>
    /// See <see cref="TerrainTile"/> for more details.
    /// </remarks>
    public Texture2D HeightTexture
    {
      get { return _heightTexture; }
      set
      {
        _heightTexture = value;
        UpdateAabb();
      }
    }
    private Texture2D _heightTexture;


    /// <summary>
    /// Gets or sets the normal texture which stores normal vectors.
    /// </summary>
    /// <value>The normal texture which stores normal vectors.</value>
    /// <remarks>
    /// See <see cref="TerrainTile"/> for more details.
    /// </remarks>
    public Texture2D NormalTexture
    {
      get { return _normalTexture; }
      set
      {
        _normalTexture = value;
        UpdateAabb();
      }
    }
    private Texture2D _normalTexture;


    /// <summary>
    /// Gets or sets the hole texture which stores hole information in the Alpha channel.
    /// </summary>
    /// <value>The hole texture which stores hole information in the Alpha channel.</value>
    /// <remarks>
    /// See <see cref="TerrainTile"/> for more details.
    /// </remarks>
    public Texture2D HoleTexture
    {
      get { return _holeTexture; }
      set
      {
        _holeTexture = value;
        UpdateAabb();
      }
    }
    private Texture2D _holeTexture;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainTile"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainTile"/> class with a default
    /// material.
    /// </summary>
    /// <param name="graphicsService">The graphic service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public TerrainTile(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      // ReSharper disable once DoNotCallOverridableMethodsInConstructor
      Material = OnCreateMaterial(graphicsService);

      Layers = new TerrainLayerCollection(this);

      UpdateAabb();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainTile"/> class with the specified
    /// material.
    /// </summary>
    /// <param name="graphicsService">The graphic service.</param>
    /// <param name="material">The material. Can be <see langword="null"/> - see remarks.</param>
    /// <remarks>
    /// The specified material is used to render the terrain geometry (heights, normals, holes) into
    /// the clipmaps. When the <paramref name="material"/> is <see langword="null"/>, no geometry is
    /// rendered into the terrain tile.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public TerrainTile(IGraphicsService graphicsService, Material material)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      Material = material;
      Layers = new TerrainLayerCollection(this);

      UpdateAabb();
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="TerrainTile"/> class.
    /// </summary>
    /// <remarks>
    /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in
    /// <see langword="true"/>, and then suppresses finalization of the instance.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="TerrainTile"/> class
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          foreach (var layer in Layers)
            layer.SafeDispose();
        }

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Called when a new <see cref="TerrainTile"/> is created without explicitly specifying a
    /// <see cref="Material"/>.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>The default material.</returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong>
    /// This method may be executed in the constructor of the <see cref="TerrainTile"/> class.
    /// Therefore, the <see cref="TerrainTile"/> instance may not be fully initialized when this
    /// method is called!
    /// </remarks>
    protected virtual Material OnCreateMaterial(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      object data;
      graphicsService.Data.TryGetValue(DefaultMaterialKey, out data);

      var material = data as Material;
      if (material == null)
      {
        material = new Material
        {
          {
            "Base",
            new EffectBinding(
              graphicsService,
              graphicsService.Content.Load<Effect>("DigitalRune/Terrain/TerrainGeometryLayer"),
              null,
              EffectParameterHint.Material)
          },
          {
            "Detail",
            new EffectBinding(
              graphicsService,
              graphicsService.Content.Load<Effect>("DigitalRune/Terrain/TerrainHoleLayer"),
              null,
              EffectParameterHint.Material)
          }
        };

        graphicsService.Data[DefaultMaterialKey] = material;
      }

      return material;
    }


    private void UpdateAabb()
    {
      // Update AABB which depends on several properties.
      Aabb = new Aabb(new Vector3F(OriginX, 0, OriginZ),
                      new Vector3F(OriginX + WidthX, 0, OriginZ + WidthZ));
    }


    /// <overloads>
    /// <summary>
    /// Invalidates the data cached by the renderer.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Invalidates the terrain tile in the data cached by the renderer.
    /// </summary>
    /// <inheritdoc cref="Graphics.Terrain.Invalidate()"/>
    public void Invalidate()
    {
      if (Terrain != null)
        Terrain.Invalidate(this);
    }


    /// <summary>
    /// Invalidates the specified terrain layer in the data cached by the renderer.
    /// </summary>
    /// <param name="layer">The terrain layer which should be invalidated.</param>
    /// <inheritdoc cref="Graphics.Terrain.Invalidate()"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="layer"/> is <see langword="null"/>.
    /// </exception>
    public void Invalidate(TerrainLayer layer)
    {
      if (Terrain != null)
        Terrain.Invalidate(this, layer);
    }


    /// <inheritdoc/>
    void IInternalTerrainLayer.OnDraw(GraphicsDevice graphicsDevice, Rectangle rectangle, Vector2F topLeftPosition, Vector2F bottomRightPosition)
    {
      graphicsDevice.DrawQuad(rectangle, topLeftPosition, bottomRightPosition);
    }
    #endregion
  }
}
