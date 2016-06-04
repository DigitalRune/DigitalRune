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
  /// Represents an instance of a height-field-based terrain.
  /// (Not available on these platforms: Xbox 360, mobile platforms)
  /// </summary>
  /// <remarks>
  /// <para>
  /// This type is not available on the following platforms: Xbox 360, mobile platforms
  /// </para>
  /// <para>
  /// The <see cref="TerrainNode"/> represents a terrain in the <see cref="Scene"/>. The terrain
  /// itself is defined by <see cref="Terrain"/>.
  /// </para>
  /// <para>
  /// <see cref="Terrain"/>s can be shared, i.e. multiple <see cref="TerrainNode"/>s can reference
  /// the same <see cref="Terrain"/> instance.
  /// </para>
  /// <para>
  /// <strong>Material:</strong><br/>
  /// The terrain node is rendered using a <see cref="Material"/> (similar to a normal
  /// <see cref="Mesh"/>). If no custom material is set, a default material is used. The default
  /// material supports the render passes "GBuffer" and "Material" used in the deferred lighting
  /// samples.
  /// </para>
  /// <para>
  /// <strong>Clipmaps:</strong><br/>
  /// The default material renders the terrain using height and material information stored in
  /// clipmaps. <see cref="BaseClipmap"/> stores information at the terrain vertex level. It usually
  /// provides height, normal and hole information which define the terrain mesh.
  /// <see cref="DetailClipmap"/> stores more detailed information which is used to shade the
  /// terrain. It usually stores detail normals (for normal mapping), diffuse colors, specular
  /// colors, heights (for parallax occlusion mapping) and other material information.
  /// </para>
  /// <para>
  /// <strong>Renderers:</strong><br/>
  /// Terrain clipmaps are created and updated (when the camera has moved) by the
  /// <see cref="TerrainClipmapRenderer"/>. The <see cref="TerrainRenderer"/> renders
  /// <see cref="TerrainNode"/>s to the screen.
  /// </para>
  /// <para>
  /// <strong>Level of detail (LOD):</strong><br/>
  /// When the terrain is rendered, the terrain mesh and texture resolution depends on the distance
  /// from the camera. When the <see cref="TerrainRenderer"/> renders the terrain, it uses the
  /// <see cref="RenderContext.LodCameraNode"/> in the <see cref="RenderContext"/>. (If no
  /// <see cref="RenderContext.LodCameraNode"/> is set, the normal
  /// <see cref="RenderContext.CameraNode"/> of the render context is used. 
  /// </para>
  /// <para>
  /// A terrain node should only be rendered for a single camera node because the renderer might
  /// cache camera-dependent LOD data. If a scene contains two camera nodes (e.g. for 2 player
  /// split screen rendering), the <see cref="RenderContext.LodCameraNode"/> should be one of these
  /// two cameras. It could also be a "virtual" camera, which is e.g. between both player cameras.
  /// Switching the cameras within one frame would be inefficient.
  /// </para>
  /// <para>
  /// Alternatively, each camera could use a separate <see cref="TerrainNode"/>. When the image of
  /// a camera is rendered only one terrain node should be rendered. Several terrain nodes can
  /// reference the same <see cref="Terrain"/> instance. 
  /// </para>
  /// <para>
  /// <strong>Terrain shadows:</strong><br/>
  /// Terrain nodes can be rendered into the shadow maps. <see cref="SceneNode.CastsShadows"/> is
  /// <see langword="true"/> by default. When using the standard <see cref="ShadowCasterQuery"/>,
  /// terrain nodes are only rendered into the shadow maps of <see cref="DirectionalLight"/>s.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// When a <see cref="TerrainNode"/> is cloned the properties <see cref="Terrain"/> and
  /// <see cref="Material"/> are copied by reference (shallow copy). The original and the cloned
  /// node will reference the same instances.
  /// </para>
  /// </remarks>
  /// <seealso cref="DigitalRune.Graphics.Terrain"/>
  public class TerrainNode : SceneNode
  {
    // Notes:
    // - Base clipmap:
    //   Format HalfVector4
    //   (abs. height, world space normal x, world space normal z, hole flag (1 = no hole, like opacity))
    // - Detail clipmap:
    //   Format Color
    //   (world space normal.x, world space normal.z, specular exponent, hole alpha)
    //   (diffuse rgb, alpha)
    //   (specular intensity, height, -, alpha)
    //   The unused slot could store emissive color. Using emissive for terrain is rare (e.g. could 
    //   be used for glowing lava veins or SciFy-Tron-like glowing lines). Specular color is only 
    //   needed for metals. In most cases specular intensity is enough.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the terrain definition.
    /// </summary>
    /// <value>The terrain definition.</value>
    /// <remarks>
    /// <para>
    /// Multiple <see cref="TerrainNode"/>s can reference the same <see cref="Terrain"/> instance.
    /// </para>
    /// </remarks>
    public Terrain Terrain { get; private set; }


    /// <summary>
    /// Gets or sets the base clipmap which stores geometry information at the terrain mesh vertex
    /// level.
    /// </summary>
    /// <value>
    /// The base clipmap which stores geometry information at the terrain mesh vertex level.
    /// </value>
    /// <remarks>
    /// <see cref="TerrainNode"/> for more information.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public TerrainClipmap BaseClipmap
    {
      get { return _baseClipmap; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _baseClipmap = value;
      }
    }
    private TerrainClipmap _baseClipmap;


    /// <summary>
    /// Gets or sets the detail clipmap which stores material information used to shade the terrain.
    /// </summary>
    /// <value>
    /// The detail clipmap which stores material information used to shade the terrain.
    /// </value>
    /// <remarks>
    /// <see cref="TerrainNode"/> for more information.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public TerrainClipmap DetailClipmap
    {
      get { return _detailClipmap; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _detailClipmap = value;
      }
    }
    private TerrainClipmap _detailClipmap;


    /// <summary>
    /// Gets or sets the terrain material.
    /// </summary>
    /// <value>The terrain material.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public Material Material
    {
      get { return _material; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _material = value;
        MaterialInstance = new MaterialInstance(Material);
      }
    }
    private Material _material;


    /// <summary>
    /// Gets the material instance.
    /// </summary>
    /// <value>The material instance.</value>
    public MaterialInstance MaterialInstance { get; private set; }


    /// <summary>
    /// Gets or sets the threshold used to check for holes.
    /// </summary>
    /// <value>The threshold used to check for holes. The default value is 0.3.</value>
    /// <remarks>
    /// <para>
    /// Holes are usually marked using 0 values in the <see cref="BaseClipmap"/>. Solid terrain
    /// parts have a value of 1. However, in the distance the terrain geometry is downsampled, which
    /// also means that the hole values are averaged. <see cref="HoleThreshold"/> is used to decide
    /// when a value still counts as a hole. A hole threshold of 0 disables holes.
    /// </para>
    /// </remarks>
    public float HoleThreshold { get; set; }


    /// <summary>
    /// Gets or sets the detail fade range which defines the transition between two clipmap levels
    /// of the <see cref="DetailClipmap"/>.
    /// </summary>
    /// <value>The detail fade range in the range [0, 1]. The default value is 0.3.</value>
    /// <remarks>
    /// To hide transitions between clipmap levels of the <see cref="DetailClipmap"/>, the lower
    /// clipmap level fades to the higher clipmap level. <see cref="DetailFadeRange"/> defines the
    /// range over which this transition occurs. If this value is 0, then there is no transition.
    /// The smoothest transition is created by setting the value to 1, but this wastes a lot of
    /// texture resolution. Small values like 0.3 are usually better.
    /// </remarks>
    public float DetailFadeRange { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainNode"/> class.
    /// </summary>
    /// <param name="terrain">The terrain.</param>
    /// <param name="material">The material.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="terrain"/> or <paramref name="material"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public TerrainNode(Terrain terrain, Material material)
    {
      if (terrain == null)
        throw new ArgumentNullException("terrain");
      if (material == null)
        throw new ArgumentNullException("material");

      Terrain = terrain;
      Material = material;

      IsRenderable = true;
      CastsShadows = true;
      Shape = terrain.Shape;

      _baseClipmap = new TerrainClipmap(1, SurfaceFormat.HalfVector4)
      {
        LevelBias = 0.1f,
      };

      _detailClipmap = new TerrainClipmap(3, SurfaceFormat.Color)
      {
        CellsPerLevel = 1024,
        NumberOfLevels = 6,
      };
      DetailClipmap.CellSizes[0] = 0.005f;
      DetailClipmap.Invalidate();

      HoleThreshold = 0.3f;
      DetailFadeRange = 0.3f;
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing, bool disposeData)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          BaseClipmap.Dispose();
          DetailClipmap.Dispose();
          Shape = Shape.Empty;

          if (disposeData)
            Terrain.Dispose();
        }

        base.Dispose(disposing, disposeData);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new TerrainNode Clone()
    {
      return (TerrainNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new TerrainNode(Terrain, Material);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone the SceneNode properties (base class).
      base.CloneCore(source);

      // Clone the TerrainNode properties.
      var sourceTyped = (TerrainNode)source;
      HoleThreshold = sourceTyped.HoleThreshold;
    }
    #endregion

    #endregion
  }
}
