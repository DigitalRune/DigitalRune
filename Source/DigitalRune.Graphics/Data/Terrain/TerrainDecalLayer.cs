// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a decal which is rendered onto the terrain.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="TerrainDecalLayer"/> can be used to render decals, such as dirt, leaves,
  /// explosion marks, sewer grates, etc. onto the terrain.
  /// </para>
  /// <para>
  /// The position and orientation of the texture is determined by the <see cref="Pose"/>. The decal
  /// is projected in "forward" (-z) direction. That means that the orientation will usually be a
  /// downward facing orientation and the y value of the position will be ignored. For
  /// example:
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// var position = new Vector3F(positionX, 0, positionZ);
  /// var orientation = Matrix33F.CreateRotationY(rotationAngle) * 
  ///                   Matrix33F.CreateRotationX(-ConstantsF.PiOver2);
  /// myDecalLayer.Pose = new Pose(position, orientation);
  /// ]]>
  /// </code>
  /// <para>
  /// The decal is centered at the specified <see cref="Pose"/>. The extent is defined by
  /// <see cref="Width"/> and <see cref="Height"/>.
  /// </para>
  /// <para>
  /// Important: The decal is only rendered on tiles where the decal layer is added. If the decal
  /// overlaps multiple terrain tiles, the decal layer needs to be added to all tiles. (An instance
  /// of <see cref="TerrainDecalLayer"/> can be shared by multiple terrain tiles.
  /// </para>
  /// <para>
  /// <strong>Material:</strong><br/>
  /// Different <see cref="TerrainDecalLayer"/>s can share the same material.
  /// </para>
  /// </remarks>
  public class TerrainDecalLayer : TerrainLayer
  {

    // centered on pose

    // TODO:
    // - Add Depth property (see DecalNode.Depth).
    // - Use non-top down projection: Render projection box into the clipmap or a quad which
    //   covers this box and project each clipmap pixel back into the decal texture space - like 
    //   deferred decals.
    // - Add other deferred decal properties, like NormalThreshold.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly TerrainLayerVertex[] QuadVertices =
    {
      new TerrainLayerVertex(new Vector2(0, 0), new Vector2(0, 0)), 
      new TerrainLayerVertex(new Vector2(0, 0), new Vector2(1, 0)),
      new TerrainLayerVertex(new Vector2(0, 0), new Vector2(0, 1)),
      new TerrainLayerVertex(new Vector2(0, 0), new Vector2(1, 1)),
    };
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the pose of the decal.
    /// </summary>
    /// <value>
    /// The pose of the decal. The default orientation is downward facing (local z-axis points
    /// down).
    /// </value>
    /// <remarks>
    /// See <see cref="TerrainDecalLayer"/> for more details.
    /// </remarks>
    public Pose Pose
    {
      get { return _pose; }
      set
      {
        _pose = value;
        UpdateAabb();
      }
    }
    private Pose _pose;


    /// <summary>
    /// Gets or sets the width of the decal in world space.
    /// </summary>
    /// <value>The width of the decal in world space. The default value is 1.</value>
    /// <remarks>
    /// The width is measured along the local x-axis.
    /// </remarks>
    public float Width
    {
      get { return _width; }
      set
      {
        _width = value;
        UpdateAabb();
      }
    }
    private float _width;


    /// <summary>
    /// Gets or sets the height of the decal in world space.
    /// </summary>
    /// <value>The height of the decal in world space. The default value is 1.</value>
    /// <remarks>
    /// The height is measured along the local y-axis.
    /// </remarks>
    public float Height
    {
      get { return _height; }
      set
      {
        _height = value;
        UpdateAabb();
      }
    }
    private float _height;


    /// <summary>
    /// Gets or sets the diffuse color.(This is a material parameter - see remarks.)
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
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainDecalLayer"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainDecalLayer"/> class with the default
    /// material.
    /// </summary>
    /// <param name="graphicService">The graphic service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicService"/> is <see langword="null"/>.
    /// </exception>
    public TerrainDecalLayer(IGraphicsService graphicService)
    {
      if (graphicService == null)
        throw new ArgumentNullException("graphicService");

      // Use a down orientation per default.
      Pose = new Pose(Matrix33F.CreateRotationX(-ConstantsF.PiOver2));

      var effect = graphicService.Content.Load<Effect>("DigitalRune/Terrain/TerrainDecalLayer");
      Material = new Material
      {
        { "Detail", new EffectBinding(graphicService, effect, null, EffectParameterHint.Material) }
      };

      FadeOutStart = int.MaxValue;
      FadeOutEnd = int.MaxValue;
      Width = 1;
      Height = 1;
      DiffuseColor = new Vector3F(1, 1, 1);
      SpecularColor = new Vector3F(1, 1, 1);
      SpecularPower = 10;
      Alpha = 1;
      DiffuseTexture = graphicService.GetDefaultTexture2DWhite();
      SpecularTexture = graphicService.GetDefaultTexture2DBlack();
      NormalTexture = graphicService.GetDefaultNormalTexture();
      HeightTextureScale = 1;
      HeightTexture = graphicService.GetDefaultTexture2DBlack();

      UpdateAabb();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainDecalLayer"/> class with a custom
    /// material.
    /// </summary>
    /// <param name="material">The material.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="material"/> is <see langword="null"/>.
    /// </exception>
    public TerrainDecalLayer(Material material)
    {
      if (material == null)
        throw new ArgumentNullException("material");

      // Use a down orientation per default.
      Pose = new Pose(Matrix33F.CreateRotationX(-ConstantsF.PiOver2));

      Material = material;
      UpdateAabb();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void UpdateAabb()
    {
      // Note: This code was copied from BoxShape. See BoxShape.cs for detailed comments.

      Vector3F halfExtent = new Vector3F(Width / 2, Height / 2, 0);

      Matrix33F rotationMatrix = _pose.Orientation;
      Vector3F worldX = rotationMatrix.GetRow(0);
      Vector3F worldY = rotationMatrix.GetRow(1);
      Vector3F worldZ = rotationMatrix.GetRow(2);

      worldX = Vector3F.Absolute(worldX);
      worldY = Vector3F.Absolute(worldY);
      worldZ = Vector3F.Absolute(worldZ);

      Vector3F halfExtentWorld = new Vector3F(Vector3F.Dot(halfExtent, worldX),
                                              Vector3F.Dot(halfExtent, worldY),
                                              Vector3F.Dot(halfExtent, worldZ));

      Aabb = new Aabb(_pose.Position - halfExtentWorld, _pose.Position + halfExtentWorld);
    }


    /// <inheritdoc/>
    internal override void OnDraw(GraphicsDevice graphicsDevice, Rectangle rectangle, Vector2F topLeftPosition, Vector2F bottomRightPosition)
    {
      // The decal is rendered as rotated quad. A proper scissor rectangle is already set, so we
      // can draw outside the given rectangle.
      float halfWidth = Width / 2;
      float halfHeight = Height / 2;
      Vector3F decalTopLeftWorld = _pose.ToWorldPosition(new Vector3F(-halfWidth, -halfHeight, 0));
      Vector3F decalTopRightWorld = _pose.ToWorldPosition(new Vector3F(halfWidth, -halfHeight, 0));
      Vector3F decalBottomLeftWorld = _pose.ToWorldPosition(new Vector3F(-halfWidth, halfHeight, 0));
      Vector3F decalBottomRightWorld = _pose.ToWorldPosition(new Vector3F(halfWidth, halfHeight, 0));

      // We store XZ position in the POSITION attribute.
      // The TEXCOORD0 attribute already contains the decal texture coordinates.
      QuadVertices[0].Position.X = decalTopLeftWorld.X;
      QuadVertices[0].Position.Y = decalTopLeftWorld.Z;
      QuadVertices[1].Position.X = decalTopRightWorld.X;
      QuadVertices[1].Position.Y = decalTopRightWorld.Z;
      QuadVertices[2].Position.X = decalBottomLeftWorld.X;
      QuadVertices[2].Position.Y = decalBottomLeftWorld.Z;
      QuadVertices[3].Position.X = decalBottomRightWorld.X;
      QuadVertices[3].Position.Y = decalBottomRightWorld.Z;

      graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, QuadVertices, 0, 2);
    }
    #endregion
  }
}
