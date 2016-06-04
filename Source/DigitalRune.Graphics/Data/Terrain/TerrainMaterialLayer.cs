// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a material with tiling textures that are rendered onto the terrain.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="TerrainMaterialLayer"/> renders a material, such as grass, dirt, rocks, etc.,
  /// onto the terrain.
  /// </para>
  /// <para>
  /// <strong>Material textures:</strong><br/>
  /// The material is defined by several tiling textures:
  /// <list type="bullet">
  /// <item><see cref="DiffuseTexture"/></item>
  /// <item><see cref="SpecularTexture"/></item>
  /// <item><see cref="NormalTexture"/></item>
  /// <item><see cref="HeightTexture"/></item>
  /// </list>
  /// These textures repeat within the bounds of terrain tile. The property <see cref="TileSize"/>
  /// defines the scale of the textures.
  /// </para>
  /// <para>
  /// <strong>Blend texture:</strong><br/>
  /// The <see cref="BlendTexture"/> contains blend weights. It is a non-tiling texture which covers
  /// the terrain tile. (In other tools or engines this texture is called <i>splat map</i>,
  /// <i>control map</i>, <i>alpha map</i>, <i>material map</i>, <i>weight map</i>, or <i>mask
  /// texture</i>.) A blend texture can contain several blend weights (e.g. an RGBA texture can
  /// contain 4 blend weights - one weight per channel). The property 
  /// <see cref="BlendTextureChannel"/> determines which channel is used by the material layer.
  /// </para>
  /// <para>
  /// The material reads the blend weight from the blend texture and if the blend weight is greater
  /// than <see cref="BlendThreshold"/>, the material is drawn. There is a small transition zone
  /// around the threshold where the material fades out. This transition zone is defined by
  /// <see cref="BlendRange"/>.
  /// </para>
  /// <para>
  /// The <see cref="HeightTexture"/> of the material can be used to modify the blending. This can
  /// be used to create more realistic transitions between two materials. For example: One material
  /// layer draws a dirt texture. The next material layer blends a stone texture over the dirt
  /// texture. The stone material includes a height texture. This is used to create a dirt-stone
  /// transition where more dirt is visible in the gaps between stones (low height values).
  /// <see cref="BlendHeightInfluence"/> controls how much the height texture influences the
  /// blending.
  /// </para>
  /// <para>
  /// Noise can also be used to make transitions between two material layers visually more
  /// interesting. If <see cref="BlendNoiseInfluence"/> is greater than 0, a noise value is added to
  /// the blend weight to make transitions less uniform. <see cref="NoiseTileSize"/> controls the
  /// size of the tiling noise texture.
  /// </para>
  /// <para>
  /// <strong>Tint texture:</strong><br/>
  /// The <see cref="TintTexture"/> contains a color that is multiplied with the material. Like the
  /// <see cref="BlendTexture"/>, the <see cref="TintTexture"/> is a non-tiling texture which covers
  /// the entire terrain tile. <see cref="TintStrength"/> defines the influence of the
  /// <see cref="TintTexture"/>.
  /// </para>
  /// <para>
  /// <strong>Height-based and slope-based blending based:</strong><br/>
  /// The properties <see cref="TerrainHeightMin"/>, <see cref="TerrainHeightMax"/>,
  /// <see cref="TerrainSlopeMin"/>, and <see cref="TerrainSlopeMax"/> can be used to apply the
  /// material only on terrain geometry with a certain height or slope. Near these limits the
  /// material fades out. The fade-out range is determined by <see cref="TerrainHeightBlendRange"/>
  /// and <see cref="TerrainSlopeBlendRange"/>.
  /// </para>
  /// <para>
  /// <strong>Triplanar texture mapping:</strong><br/>
  /// The material textures are usually projected top-down onto the material. This may lead to
  /// distorted textures on very steep slopes. Triplanar texture mapping can be used to reduce
  /// distortions. If the triplanar texturing is enabled, the terrain normals are checked and the
  /// texture is projected vertically (y direction) or horizontally (x or z direction) to minimize
  /// distortions.
  /// </para>
  /// <para>
  /// Triplanar texture mapping is disabled by default (<see cref="TriplanarTightening"/> = -1).
  /// Set <see cref="TriplanarTightening"/> to a positive value, e.g. 0.5, to enable triplanar
  /// texturing. Larger <see cref="TriplanarTightening"/> values make the transitions between
  /// different projection directions shorter.
  /// </para>
  /// </remarks>
  public class TerrainMaterialLayer : TerrainLayer
  {
    // TODOs:
    // - Allow to set control/tint map origin? Or use PackedTexture for control/tint map?


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the tile size of the textures in world space.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The tile size of the textures in world space. The default value is 1, which means
    /// that the textures repeat every 1 world space unit.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// The <see cref="TileSize"/> affects the size of the <see cref="DiffuseTexture"/>,
    /// <see cref="SpecularTexture"/>, <see cref="NormalTexture"/> and <see cref="HeightTexture"/>.
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
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
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
    /// The scale that is multiplied with samples of the height texture.
    /// The default value is 1.
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
    /// Gets or sets the height texture, which stores relative height values.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The height texture, which stores relative height values.</value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <seealso cref="HeightTextureBias"/>
    /// <seealso cref="HeightTextureScale"/>
    public Texture2D HeightTexture
    {
      get { return GetParameter<Texture2D>(true, "HeightTexture"); }
      set { SetParameter(true, "HeightTexture", value); }
    }


    /// <summary>
    /// Gets or sets the tightening factor for triplanar texture mapping. (Use -1 to disable
    /// triplanar texture mapping. This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The tightening factor for triplanar mapping. To use triplanar texture mapping this factor
    /// should be in the range [0, sqrt(3)]. (sqrt(3) is about 0.577.) If this value is -1,
    /// triplanar mapping is disabled. The default value is -1.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    public float TriplanarTightening
    {
      get { return GetParameter<float>(true, "TriplanarTightening"); }
      set { SetParameter(true, "TriplanarTightening", value); }
    }


    /// <summary>
    /// Gets or sets the influence of the <see cref="TintTexture"/>.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The influence of the <see cref="TintTexture"/>.</value>
    /// <remarks>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </remarks>
    /// <seealso cref="TintTexture"/>
    public float TintStrength
    {
      get { return GetParameter<float>(true, "TintStrength"); }
      set { SetParameter(true, "TintStrength", value); }
    }


    /// <summary>
    /// Gets or sets the tint texture. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The tint texture.</value>
    /// <remarks>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </remarks>
    /// <seealso cref="TintStrength"/>
    public Texture2D TintTexture
    {
      get { return GetParameter<Texture2D>(true, "TintTexture"); }
      set { SetParameter(true, "TintTexture", value); }
    }


    /// <summary>
    /// Gets or sets the threshold for the blend weights.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The threshold for the blend weights. The default value is 0.5.</value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <seealso cref="BlendRange"/>
    /// <seealso cref="BlendTexture"/>
    /// <seealso cref="BlendTextureChannel"/>
    public float BlendThreshold
    {
      get { return GetParameter<float>(true, "BlendThreshold"); }
      set { SetParameter(true, "BlendThreshold", value); }
    }


    /// <summary>
    /// Gets or sets the blend range. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The blend range. The default value is 1.</value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <seealso cref="BlendThreshold"/>
    /// <seealso cref="BlendTexture"/>
    /// <seealso cref="BlendTextureChannel"/>
    public float BlendRange
    {
      get { return GetParameter<float>(true, "BlendRange"); }
      set { SetParameter(true, "BlendRange", value); }
    }


    /// <summary>
    /// Gets or sets the influence of the <see cref="HeightTexture"/> on the blend weight.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// <para>
    /// The influence of the <see cref="HeightTexture"/> on the blend weight in the range [-1, 1].
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>= 0</term>
    /// <description>The height values do not influence blending.</description>
    /// </item>
    /// <item>
    /// <term>&gt; 0</term>
    /// <description>
    /// This material layer overwrites previous material layers where the height values are large.
    /// </description>
    /// </item>
    /// <item>
    /// <term>&lt; 0</term>
    /// <description>
    /// This material layer overwrites previous material layers where the height values are small.
    /// </description>
    /// </item>
    /// </list>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// <para>
    /// For optimal results, the <see cref="HeightTexture"/> should be normalized.
    /// </para>
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    public float BlendHeightInfluence
    {
      get { return GetParameter<float>(true, "BlendHeightInfluence"); }
      set { SetParameter(true, "BlendHeightInfluence", value); }
    }


    /// <summary>
    /// Gets or sets the influence of the noise on the blend weight.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The influence of the noise on the blend weight. The default value is 0.</value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <seealso cref="NoiseTileSize"/>
    public float BlendNoiseInfluence
    {
      get { return GetParameter<float>(true, "BlendNoiseInfluence"); }
      set { SetParameter(true, "BlendNoiseInfluence", value); }
    }


    /// <summary>
    /// Gets or sets the texture channel of the <see cref="BlendTexture"/> which contains the blend
    /// weight for this material. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// <para>
    /// The texture channel of the <see cref="BlendTexture"/> which contains the blend weight for
    /// this material. This values is in the range [0, 3]:
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Value</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>0</term>
    /// <description>Red channel</description>
    /// </item>
    /// <item>
    /// <term>1</term>
    /// <description>Green channel</description>
    /// </item>
    /// <item>
    /// <term>2</term>
    /// <description>Blue channel</description>
    /// </item>
    /// <item>
    /// <term>3</term>
    /// <description>Alpha channel</description>
    /// </item>
    /// </list>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <seealso cref="BlendRange"/>
    /// <seealso cref="BlendThreshold"/>
    /// <seealso cref="BlendTexture"/>
    public int BlendTextureChannel
    {
      get
      {
        var mask = GetParameter<Vector4>(true, "BlendTextureChannelMask");
        if (mask.X > 0.99f)
          return 0;
        if (mask.Y > 0.99f)
          return 1;
        if (mask.Z > 0.99f)
          return 2;
        return 3;
      }
      set
      {
        Vector4 blendMask;
        switch (value)
        {
          case 0: blendMask = new Vector4(1, 0, 0, 0); break;
          case 1: blendMask = new Vector4(0, 1, 0, 0); break;
          case 2: blendMask = new Vector4(0, 0, 1, 0); break;
          default: blendMask = new Vector4(0, 0, 0, 1); break;
        }

        SetParameter(true, "BlendTextureChannelMask", blendMask);
      }
    }


    /// <summary>
    /// Gets or sets the texture which contains the blend weights.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>The texture which contains the blend weights.</value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <seealso cref="BlendRange"/>
    /// <seealso cref="BlendThreshold"/>
    /// <seealso cref="BlendTextureChannel"/>
    public Texture2D BlendTexture
    {
      get { return GetParameter<Texture2D>(true, "BlendTexture"); }
      set { SetParameter(true, "BlendTexture", value); }
    }


    /// <summary>
    /// Gets or sets the size of the tiling noise map in world space units.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The size of the tiling noise map in world space units. The default value is 1.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// <seealso cref="BlendNoiseInfluence"/>
    public float NoiseTileSize
    {
      get { return GetParameter<float>(true, "NoiseTileSize"); }
      set { SetParameter(true, "NoiseTileSize", value); }
    }


    /// <summary>
    /// Gets or sets the min terrain height in world space. The material is not rendered below this
    /// height. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The min terrain height in world space. The material is not rendered below this height. The
    /// default value is -1e20.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    public float TerrainHeightMin
    {
      get { return GetParameter<float>(true, "TerrainHeightMin"); }
      set { SetParameter(true, "TerrainHeightMin", value); }
    }


    /// <summary>
    /// Gets or sets the max terrain height in world space. The material is not rendered above this
    /// height. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The min terrain height in world space. The material is not rendered above this height. The
    /// default value is 1e20.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    public float TerrainHeightMax
    {
      get { return GetParameter<float>(true, "TerrainHeightMax"); }
      set { SetParameter(true, "TerrainHeightMax", value); }
    }


    /// <summary>
    /// Gets or sets the range for terrain height-based blending in world space units.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The range for terrain height-based blending in world space units. The default value is 1.
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    public float TerrainHeightBlendRange
    {
      get { return GetParameter<float>(true, "TerrainHeightBlendRange"); }
      set { SetParameter(true, "TerrainHeightBlendRange", value); }
    }


    /// <summary>
    /// Gets or sets the min terrain slope in radians. The material is not rendered if the terrain
    /// is flatter than this slope. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The min terrain slope in radians. The material is not rendered if the terrain is flatter
    /// than this slope. The default value is -π rad (= 180°).
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    public float TerrainSlopeMin
    {
      get { return GetParameter<float>(true, "TerrainSlopeMin"); }
      set { SetParameter(true, "TerrainSlopeMin", value); }
    }


    /// <summary>
    /// Gets or sets the max terrain slope in radians. The material is not rendered if the terrain
    /// is steeper than this slope. (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The max terrain slope in radians. The material is not rendered if the terrain is steeper
    /// than this slope. The default value is π rad (= 180°).
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    /// </remarks>
    public float TerrainSlopeMax
    {
      get { return GetParameter<float>(true, "TerrainSlopeMax"); }
      set { SetParameter(true, "TerrainSlopeMax", value); }
    }


    /// <summary>
    /// Gets or sets the range for terrain slope-based blending in radians.
    /// (This is a material parameter - see remarks.)
    /// </summary>
    /// <value>
    /// The range for terrain slope-based blending in radians. The default value is 0.1745 rad (= 
    /// 10°).
    /// </value>
    /// <remarks>
    /// <para>
    /// This is material parameter. Changing this property affects all terrain layers that share
    /// the same material.
    /// </para>
    /// <para>
    /// See <see cref="TerrainMaterialLayer"/> for more details.
    /// </para>
    /// </remarks>
    public float TerrainSlopeBlendRange
    {
      get { return GetParameter<float>(true, "TerrainSlopeBlendRange"); }
      set { SetParameter(true, "TerrainSlopeBlendRange", value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainMaterialLayer"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainMaterialLayer"/> class with the default
    /// material.
    /// </summary>
    /// <param name="graphicService">The graphic service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicService"/> is <see langword="null"/>.
    /// </exception>
    public TerrainMaterialLayer(IGraphicsService graphicService)
    {
      if (graphicService == null)
        throw new ArgumentNullException("graphicService");

      var effect = graphicService.Content.Load<Effect>("DigitalRune/Terrain/TerrainMaterialLayer");
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
      HeightTextureBias = 0;
      HeightTexture = graphicService.GetDefaultTexture2DBlack();
      TriplanarTightening = -1;
      TintStrength = 1;
      TintTexture = graphicService.GetDefaultTexture2DWhite();
      BlendThreshold = 0.5f;
      BlendRange = 1f;
      BlendHeightInfluence = 0;
      BlendNoiseInfluence = 0;
      BlendTextureChannel = 0;
      BlendTexture = graphicService.GetDefaultTexture2DWhite();
      NoiseTileSize = 1;
      TerrainHeightMin = -1e20f;
      TerrainHeightMax = +1e20f;
      TerrainHeightBlendRange = 1f;
      TerrainSlopeMin = -ConstantsF.Pi;
      TerrainSlopeMax = ConstantsF.Pi;
      TerrainSlopeBlendRange = MathHelper.ToRadians(10);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainMaterialLayer"/> class with a custom
    /// material.
    /// </summary>
    /// <param name="material">The material.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="material"/> is <see langword="null"/>.
    /// </exception>
    public TerrainMaterialLayer(Material material)
    {
      if (material == null)
        throw new ArgumentNullException("material");

      Material = material;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
