// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a height field based terrain.
  /// (Not available on these platforms: Xbox 360, mobile platforms)
  /// </summary>
  /// <remarks>
  /// <para>
  /// This type is not available on the following platforms: Xbox 360, mobile platforms
  /// </para>
  /// <para>
  /// A terrain is split into one or more tiles. The terrain tiles defines the geometry (height,
  /// normals, holes) of the terrain. Each tile has a set of material layers (dirt, grass, decals,
  /// roads) that define the appearance. The material layers are applied (blended) one after the
  /// other, which means that a layer can override previous layers.
  /// </para>
  /// <para>
  /// See <see cref="TerrainTile"/> and <see cref="TerrainLayer"/> for more information.
  /// </para>
  /// <para>
  /// <strong>Cache invalidation:</strong><br/>
  /// When the <see cref="Terrain"/> is used with the <see cref="TerrainNode"/>, then the terrain
  /// data is cached in clipmaps. Therefore, it is important to notify the terrain system when a
  /// tile or layer has changed and the cached data is invalid. When tiles or layers are added to or
  /// removed from the terrain, this happens automatically. But when the properties or the contents
  /// of tiles/layers are changed, the affected region needs to be invalidated explicitly by calling
  /// the appropriate <see cref="Terrain.Invalidate()"/> method of the <see cref="Terrain"/> or the
  /// <see cref="TerrainTile"/>. For example, when the contents of a height map is changed, the
  /// affected region on the terrain needs to be invalidated by calling
  /// <see cref="Terrain.Invalidate(DigitalRune.Geometry.Shapes.Aabb)"/> or
  /// <see cref="Terrain.Invalidate(TerrainTile)"/>.
  /// </para>
  /// </remarks>
  public class Terrain : IDisposable
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// A large value which can be used in the AABBs if the AABB should cover "everything".
    /// </summary>
    internal const float TerrainLimit = int.MaxValue;
    // We use int.MaxValue because some internal calculations are done in int.
    #endregion

    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
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


    // The invalid regions for the clipmaps.
    // These lists are read and reset by the TerrainClipmapRenderer.
    internal List<Aabb> InvalidBaseRegions { get; private set; }
    internal List<Aabb> InvalidDetailRegions { get; private set; }

    // Flag indicating whether the AABBs have been clipped against each other to remove any overlap.
    internal bool AreInvalidBaseRegionsClipped { get; set; }
    internal bool AreInvalidDetailRegionsClipped { get; set; }


    // This Shape reference always points to the same shape, the TerrainNode does not need to
    // check for shape changes.
    internal Shape Shape { get; private set; }


    /// <summary>
    /// Gets or sets the values written into the <see cref="TerrainNode.BaseClipmap"/> textures when
    /// it is cleared.
    /// </summary>
    /// <value>
    /// The clear values for the <see cref="TerrainNode.BaseClipmap"/>. The default values are
    /// (-10000, 0, 0, 1) for the first clipmap texture and (0, 0, 0, 0) for the remaining clipmap
    /// textures.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public Vector4F[] BaseClearValues { get; private set; }


    /// <summary>
    /// Gets or sets the values written into the <see cref="TerrainNode.DetailClipmap"/> textures
    /// when it is cleared.
    /// </summary>
    /// <value>
    /// The clear values for the <see cref="TerrainNode.DetailClipmap"/>. The default values are (0,
    /// 0, 0, 0).
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public Vector4F[] DetailClearValues { get; private set; }


    /// <summary>
    /// Gets the terrain tiles which define the terrain geometry and materials.
    /// </summary>
    /// <value>The terrain tiles which define the terrain geometry and materials.</value>
    public TerrainTileCollection Tiles { get; private set; }


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
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Terrain"/> class.
    /// </summary>
    public Terrain()
    {
      InvalidBaseRegions = new List<Aabb>();
      InvalidDetailRegions = new List<Aabb>();
      Shape = Shape.Infinite;
      BaseClearValues = new Vector4F[4];
      DetailClearValues = new Vector4F[4];
      BaseClearValues[0] = new Vector4F(-10000, 0, 0, 1);

      Tiles = new TerrainTileCollection(this);

      Aabb = new Aabb();
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="Terrain"/> class.
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
    /// Releases the unmanaged resources used by an instance of the <see cref="Terrain"/> class
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
          // Dispose managed resources.
          foreach (var tile in Tiles)
            tile.Dispose();
        }

        // Release unmanaged resources.

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Invalidates the data cached by the renderer.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Invalidates all data cached by the renderer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method notifies the renderers that cached data (e.g. terrain clipmaps) needs to be
    /// updated.
    /// </para>
    /// <para>
    /// The <see cref="Invalidate()"/> method or its overloads are called automatically when terrain
    /// tiles or layers are added to/removed from the terrain. If any other data that affects the
    /// appearance of the terrain is changed, the method <see cref="Invalidate()"/> needs to be
    /// called manually.
    /// </para>
    /// </remarks>
    public void Invalidate()
    {
      UpdateAabb();

      InvalidBaseRegions.Clear();
      InvalidBaseRegions.Add(Aabb);
      AreInvalidBaseRegionsClipped = true;

      InvalidDetailRegions.Clear();
      InvalidDetailRegions.Add(Aabb);
      AreInvalidDetailRegionsClipped = true;
    }


    /// <summary>
    /// Invalidates the specified terrain tile in the data cached by the renderer.
    /// </summary>
    /// <param name="tile">The terrain tile which should be invalidated.</param>
    /// <inheritdoc cref="Invalidate()"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="tile"/> is <see langword="null"/>.
    /// </exception>
    public void Invalidate(TerrainTile tile)
    {
      if (tile == null)
        throw new ArgumentNullException("tile");

      UpdateAabb();
      Invalidate(tile.Aabb);
    }


    /// <summary>
    /// Invalidates the specified terrain layer in the data cached by the renderer.
    /// </summary>
    /// <param name="tile">The terrain tile owning the terrain layer.</param>
    /// <param name="layer">The terrain layer which should be invalidated.</param>
    /// <inheritdoc cref="Invalidate()"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="tile"/> or <paramref name="layer"/> is <see langword="null"/>.
    /// </exception>
    public void Invalidate(TerrainTile tile, TerrainLayer layer)
    {
      if (tile == null)
        throw new ArgumentNullException("tile");
      if (layer == null)
        throw new ArgumentNullException("layer");

      UpdateAabb();

      Aabb aabb = layer.Aabb ?? tile.Aabb;

      if (layer.Material.Contains(TerrainClipmapRenderer.RenderPassBase)
          && !RegionsContain(InvalidBaseRegions, aabb))
      {
        InvalidBaseRegions.Add(aabb);

        // If there are 2 or more AABBs, then they need clipping.
        AreInvalidBaseRegionsClipped = (InvalidBaseRegions.Count == 1);
      }

      if (layer.Material.Contains(TerrainClipmapRenderer.RenderPassDetail)
          && !RegionsContain(InvalidDetailRegions, aabb))
      {
        InvalidDetailRegions.Add(aabb);

        // If there are 2 or more AABBs, then they need clipping.
        AreInvalidDetailRegionsClipped = (InvalidDetailRegions.Count == 1);
      }
    }


    /// <summary>
    /// Invalidates the specified terrain layer in the data cached by the renderer.
    /// </summary>
    /// <param name="layer">The terrain layer which should be invalidated.</param>
    /// <inheritdoc cref="Invalidate()"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="layer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="GraphicsException">
    /// <paramref name="layer"/> does not have a valid AABB. This method cannot be used. Use one of
    /// the other method overloads instead.
    /// </exception>
    public void Invalidate(TerrainLayer layer)
    {
      if (layer == null)
        throw new ArgumentNullException("layer");
      if (!layer.Aabb.HasValue)
        throw new GraphicsException("The specified terrain layer does not have a valid AABB.");

      Aabb aabb = layer.Aabb.Value;

      if (layer.Material.Contains(TerrainClipmapRenderer.RenderPassBase)
          && !RegionsContain(InvalidBaseRegions, aabb))
      {
        InvalidBaseRegions.Add(aabb);

        // If there are 2 or more AABBs, then they need clipping.
        AreInvalidBaseRegionsClipped = (InvalidBaseRegions.Count == 1);
      }

      if (layer.Material.Contains(TerrainClipmapRenderer.RenderPassDetail)
          && !RegionsContain(InvalidDetailRegions, aabb))
      {
        InvalidDetailRegions.Add(aabb);

        // If there are 2 or more AABBs, then they need clipping.
        AreInvalidDetailRegionsClipped = (InvalidDetailRegions.Count == 1);
      }
    }


    /// <summary>
    /// Invalidates the specified region in the data cached by the renderer.
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box of the invalid region.</param>
    /// <inheritdoc cref="Invalidate()"/>
    public void Invalidate(Aabb aabb)
    {
      if (!RegionsContain(InvalidBaseRegions, aabb))
      {
        InvalidBaseRegions.Add(aabb);

        // If there are 2 or more AABBs, then they need clipping.
        AreInvalidBaseRegionsClipped = (InvalidBaseRegions.Count == 1);
      }


      if (!RegionsContain(InvalidDetailRegions, aabb))
      {
        InvalidDetailRegions.Add(aabb);

        // If there are 2 or more AABBs, then they need clipping.
        AreInvalidDetailRegionsClipped = (InvalidDetailRegions.Count == 1);
      }
    }


    private static bool RegionsContain(List<Aabb> regions, Aabb aabb)
    {
      foreach (var region in regions)
        if (region.Contains(aabb))
          return true;

      return false;
    }


    private void UpdateAabb()
    {
      if (Tiles.Count == 0)
        Aabb = new Aabb();

      var aabb = Tiles[0].Aabb;
      for (int i = 1; i < Tiles.Count; i++)
        aabb.Grow(Tiles[i].Aabb);

      Aabb = aabb;
    }


    //private void UpdateShape()
    //{
    //  // Go through layers and collect AABBs.
    //  // AABB y values must be good conservative bounds. E.g. if a terrain layer is used to add
    //  // a hill to the base terrain, then the AABB must not be the AABB of the hill. It must
    //  // be the AABB of the terrain + the hill?!

    //  // The shape is always a transformed shape with a box.
    //  if (Shape == null)
    //  {
    //    Shape = new TransformedShape(new GeometricObject(new BoxShape(1, 1, 1)));
    //  }

    //  var transformedShape = (TransformedShape)Shape;

    //  var childObject = transformedShape.Child as GeometricObject;
    //  if (childObject == null)
    //  {
    //    // User has changed child object!?
    //    childObject = new GeometricObject(new BoxShape(1, 1, 1));
    //    transformedShape.Child = childObject;
    //  }

    //  var boxShape = childObject.Shape as BoxShape;
    //  if (boxShape == null)
    //  {
    //    // User has changed child shape!?
    //    boxShape = new BoxShape(1, 1, 1);
    //    childObject.Shape = boxShape;
    //  }

    //  // Compute AABB of all layers
    //  bool aabbInitialized = false;
    //  Aabb aabb = new Aabb();
    //  foreach (var layer in Layers)
    //  {
    //    // Ignore clear layers. They will usually have an unlimited AABB.
    //    if (layer is TerrainClearLayer)
    //      continue;

    //    if (!aabbInitialized)
    //      aabb = layer.Aabb;
    //    else
    //      aabb.Grow(layer.Aabb);
        
    //    aabbInitialized = true;
    //  }

    //  if (!aabbInitialized)
    //  {
    //    // No layer or all clear layers...
    //    // Create a dummy shape: Move shape "far away" to always be culled.
    //    childObject.Pose = new Pose(new Vector3F(TerrainLayer.TerrainLimit));
    //    return;
    //  }

    //  // Make y extent sufficiently large.
    //  aabb.Minimum.Y = -TerrainLayer.TerrainLimit;
    //  aabb.Maximum.Y = TerrainLayer.TerrainLimit;

    //  boxShape.Extent = aabb.Extent;
    //  childObject.Pose = new Pose(aabb.Center);
    //}
    #endregion
  }
}
