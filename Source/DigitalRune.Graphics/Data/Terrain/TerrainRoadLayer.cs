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
  /// Represents a road which is rendered onto the terrain.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="TerrainRoadLayer"/> renders a mesh which represents a road (or other road-like
  /// structures, e.g. skid marks). The road is represented by a <see cref="Submesh"/> which can be
  /// set using <see cref="SetMesh"/>.
  /// </para>
  /// <para>
  /// Material textures <see cref="DiffuseTexture"/>, <see cref="SpecularTexture"/>,
  /// <see cref="NormalTexture"/> and <see cref="HeightTexture"/> are rendered along the road. The
  /// u-axis of the texture is mapped to the width of the road; the v-axis is mapped to the
  /// direction of the road. The textures are repeated along the road direction. The property
  /// <seealso cref="TileSize"/> determines the scale of tiling. (Textures are not tiled along the
  /// u-axis.)
  /// </para>
  /// <para>
  /// The helper methods <see cref="CreateMesh"/>, <see cref="ClampRoadToTerrain"/> and
  /// <see cref="ClampTerrainToRoad"/> can be used to create a road mesh and to clamp a road mesh to
  /// a terrain or to carve a road into a terrain.
  /// </para>
  /// <para>
  /// Important: The terrain road layer is only rendered on tiles where the layer is added. If the
  /// road stretches over multiple tiles, it needs to be added to all tiles. (An instance of
  /// <see cref="TerrainRoadLayer"/> can be shared by multiple terrain tiles.)
  /// </para>
  /// </remarks>
  public partial class TerrainRoadLayer : TerrainLayer
  {
    // Notes: 
    // - Alpha is useful for "roads" which should disappear, like skid marks!


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _disposeMesh;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the mesh that represents the road.
    /// </summary>
    /// <value>
    /// The mesh that represents the road. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// Use <see cref="SetMesh"/> to set this mesh.
    /// </remarks>
    public Submesh Submesh { get; private set; }


    /// <summary>
    /// Gets or sets the tile size of the road textures in world space.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The tile size of the road textures in world space. The default value is 1.</value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// The road textures repeat along the road. The road direction is mapped to the v-axis texture
    /// space. The default value is 1, which means that the textures repeat every 1 world space unit
    /// along the road.
    /// </para>
    /// </remarks>
    public float TileSize
    {
      get { return GetParameter<float>(true, "TileSize"); }
      set { SetParameter(true, "TileSize", value); }
    }


    /// <summary>
    /// Gets or sets the diffuse color. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The diffuse color. The default value is (1, 1, 1).</value>
    /// <remarks>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </remarks>
    public Vector3F DiffuseColor
    {
      get { return (Vector3F)GetParameter<Vector3>(true, "DiffuseColor"); }
      set { SetParameter(true, "DiffuseColor", (Vector3)value); }
    }


    /// <summary>
    /// Gets or sets the specular color. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The specular color. The default value is (1, 1, 1).</value>
    /// <remarks>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </remarks>
    public Vector3F SpecularColor
    {
      get { return (Vector3F)GetParameter<Vector3>(true, "SpecularColor"); }
      set { SetParameter(true, "SpecularColor", (Vector3)value); }
    }


    /// <summary>
    /// Gets or sets the specular color exponent. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The specular color exponent. The default value is 10.</value>
    /// <remarks>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </remarks>
    public float SpecularPower
    {
      get { return GetParameter<float>(true, "SpecularPower"); }
      set { SetParameter(true, "SpecularPower", value); }
    }


    /// <summary>
    /// Gets or sets the opacity (alpha). (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The opacity (alpha). The default value is 1.</value>
    /// <remarks>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </remarks>
    public float Alpha
    {
      get { return GetParameter<float>(true, "Alpha"); }
      set { SetParameter(true, "Alpha", value); }
    }


    /// <summary>
    /// Gets or sets the diffuse texture. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The diffuse texture.</value>
    /// <remarks>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </remarks>
    public Texture2D DiffuseTexture
    {
      get { return GetParameter<Texture2D>(true, "DiffuseTexture"); }
      set { SetParameter(true, "DiffuseTexture", value); }
    }


    /// <summary>
    /// Gets or sets the specular texture. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The specular texture.</value>
    /// <remarks>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </remarks>
    public Texture2D SpecularTexture
    {
      get { return GetParameter<Texture2D>(true, "SpecularTexture"); }
      set { SetParameter(true, "SpecularTexture", value); }
    }


    /// <summary>
    /// Gets or sets the normal texture. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The normal texture.</value>
    /// <remarks>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </remarks>
    public Texture2D NormalTexture
    {
      get { return GetParameter<Texture2D>(true, "NormalTexture"); }
      set { SetParameter(true, "NormalTexture", value); }
    }


    /// <summary>
    /// Gets or sets the scale that is multiplied with samples of the height texture.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The scale that is multiplied with samples of the height texture. The default value is 1.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// The <see cref="HeightTextureScale"/> and the <see cref="HeightTextureBias"/> can be used to
    /// modify the height samples read from the <see cref="HeightTexture"/>. The resulting height
    /// values is:
    /// </para>
    /// <para>
    /// <c>HeightTextureScale * value + HeightTextureBias</c>
    /// </para>
    /// <para>
    /// If the standard <see cref="TerrainNode"/> with a <see cref="TerrainNode.DetailClipmap"/> is
    /// used, the resulting height values need to be in the range [0, 1]. Values outside this range
    /// will be clamped.
    /// </para>
    /// </remarks>
    /// <seealso cref="HeightTexture"/>
    /// <seealso cref="HeightTextureBias"/>
    public float HeightTextureScale
    {
      get { return GetParameter<float>(true, "HeightTextureScale"); }
      set { SetParameter(true, "HeightTextureScale", value); }
    }


    /// <summary>
    /// Gets or sets the bias that is added to samples of the height texture.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The bias that is added to samples of the height texture. The default value is 0.
    /// </value>
    /// <inheritdoc cref="HeightTextureScale"/>
    /// <seealso cref="HeightTexture"/>
    /// <seealso cref="HeightTextureScale"/>
    public float HeightTextureBias
    {
      get { return GetParameter<float>(true, "HeightTextureBias"); }
      set { SetParameter(true, "HeightTextureBias", value); }
    }


    /// <summary>
    /// Gets or sets the height texture. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The height texture.</value>
    /// <remarks>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </remarks>
    /// <seealso cref="HeightTextureBias"/>
    /// <seealso cref="HeightTextureScale"/>
    public Texture2D HeightTexture
    {
      get { return GetParameter<Texture2D>(true, "HeightTexture"); }
      set { SetParameter(true, "HeightTexture", value); }
    }


    /// <summary>
    /// Gets or sets a value which determines how the sides of the road mesh fade out.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// A value which determines how the sides of the road mesh fade out.
    /// <see cref="BorderBlendRange"/> controls the fade-out ranges for the 4 sides of the road:
    /// (left, start, right, end). The values determines the range in texture coordinates where the
    /// opacity of the road texture fades out. The default value is (0, 0, 0, 0).
    /// </value>
    /// <remarks>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </remarks>
    public Vector4F BorderBlendRange
    {
      get { return (Vector4F)GetParameter<Vector4>(true, "BorderBlendRange"); }
      set { SetParameter(true, "BorderBlendRange", (Vector4)value); }
    }


    /// <summary>
    /// Gets the length of the road in world space units.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The length of the road in world space units. The default value is 1.</value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// The road length needs to be precomputed. The property affects the tiling of the textures
    /// along the road direction.
    /// </para>
    /// </remarks>
    public float RoadLength
    {
      get { return GetParameter<float>(true, "RoadLength"); }
      set { SetParameter(true, "RoadLength", value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainRoadLayer"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainRoadLayer"/> class with the default
    /// material.
    /// </summary>
    /// <param name="graphicService">The graphic service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicService"/> is <see langword="null"/>.
    /// </exception>
    public TerrainRoadLayer(IGraphicsService graphicService)
    {
      if (graphicService == null)
        throw new ArgumentNullException("graphicService");

      var effect = graphicService.Content.Load<Effect>("DigitalRune/Terrain/TerrainRoadLayer");
      Material = new Material
      {
        { "Detail", new EffectBinding(graphicService, effect, null, EffectParameterHint.Material) }
      };

      FadeOutStart = int.MaxValue;
      FadeOutEnd = int.MaxValue;
      TileSize = 1;
      DiffuseColor = new Vector3F(1, 1, 1);
      SpecularColor = new Vector3F(1, 1, 1);
      SpecularPower = 10;
      Alpha = 1;
      DiffuseTexture = graphicService.GetDefaultTexture2DWhite();
      SpecularTexture = graphicService.GetDefaultTexture2DBlack();
      NormalTexture = graphicService.GetDefaultNormalTexture();
      HeightTextureScale = 1;
      HeightTexture = graphicService.GetDefaultTexture2DBlack();
      RoadLength = 1;
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
          if (_disposeMesh)
            Submesh.SafeDispose();
        }

        // Release unmanaged resources.
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Sets the road mesh and related properties.
    /// </summary>
    /// <param name="submesh">The submesh that represents the road.</param>
    /// <param name="aabb">The axis-aligned bounding box of the mesh.</param>
    /// <param name="roadLength">The length of the road in world space units.</param>
    /// <param name="disposeWithRoadLayer">
    /// <see langword="true" /> to automatically dispose of the mesh when the
    /// <see cref="TerrainRoadLayer"/> is disposed of; otherwise, <see langword="false"/>.
    /// </param>
    /// <remarks>
    /// <see cref="CreateMesh"/> can be used to create a suitable mesh.
    /// </remarks>
    public void SetMesh(Submesh submesh, Aabb aabb, float roadLength, bool disposeWithRoadLayer)
    {
      Submesh = submesh;
      Aabb = aabb;
      RoadLength = roadLength;
      _disposeMesh = disposeWithRoadLayer;
    }


    /// <inheritdoc/>
    internal override void OnDraw(GraphicsDevice graphicsDevice, Rectangle rectangle, Vector2F topLeftPosition, Vector2F bottomRightPosition)
    {
      if (Submesh != null)
        Submesh.Draw();
    }
    #endregion
  }
}
