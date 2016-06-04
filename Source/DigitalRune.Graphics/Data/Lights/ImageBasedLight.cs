// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Threading;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a light which uses an environment cube map to add diffuse lighting and specular
  /// lighting (reflections).
  /// </summary>
  /// <remarks>
  /// <para>
  /// Image-based lighting (IBL) uses a cube map of the environment to define the lighting in a
  /// scene.
  /// </para>
  /// <para>
  /// The property <see cref="Texture"/> defines the used environment map. This property must be set
  /// to valid cube map- otherwise, the <see cref="ImageBasedLight"/> is disabled. The texture can
  /// be a normal sRGB texture or an RGBM encoded HDR texture; see
  /// <see cref="Encoding"/>. The cube map must contain mipmaps.
  /// </para>
  /// <para>
  /// When a mesh is shaded, the normal vector is used to look up a color value in the environment
  /// map to use for diffuse lighting. Reflection vectors are used to look up color values for
  /// specular lighting. This allows a mesh to reflect the environment, like a mirror. The mipmap
  /// levels are used to create reflections on materials with different glossiness. Materials with a
  /// high <see cref="DefaultEffectParameterSemantics.SpecularPower"/>, like mirrors, use high
  /// resolution mipmap levels. Diffuse lighting and dull materials use low resolution mipmap
  /// levels.
  /// </para>
  /// <para>
  /// <see cref="ImageBasedLight"/>s have a <see cref="Shape"/>, which can be
  /// <see cref="Geometry.Shapes.Shape.Infinite"/> or a <see cref="BoxShape"/>. Lights with an
  /// infinite shape cover the whole scene. Usually a level contains only one image-based light with
  /// an infinite shape. Lights with a box shape cover only the volume inside the box. This allows
  /// to set different image-based lights for different rooms or zones of a level.
  /// </para>
  /// A <see cref="LightNode"/> must be used to add an <see cref="ImageBasedLight"/> to a scene. The
  /// <see cref="LightNode"/> is also used to define the position and the orientation of the light.
  /// <para>
  /// The <see cref="BlendMode"/> determines if the diffuse light contribution of an image-based
  /// light is added to the scene (additive blending, <see cref="BlendMode"/> = 0) or replaces the
  /// ambient lighting and other image-based lights (alpha blending, <see cref="BlendMode"/> = 1).
  /// The default is <see cref="BlendMode"/> = 1, which means that an diffuse light replaces the
  /// ambient lighting and other image-based lights.
  /// </para>
  /// <para>
  /// Note: Image-based lights are usually applied after the ambient light of the scene and before
  /// the other lights of the scene. Image-based lights never replace other lights, like directional
  /// lights or point lights.
  /// </para>
  /// <para>
  /// The property <see cref="FalloffRange"/> can be used to let an image-based light fade out to
  /// create smooth blending between different image-based lights or between an image-based light
  /// and an area which contains no image-based lights. (Only relevant for box-shaped lights.
  /// Infinite lights never fade out.)
  /// </para>
  /// <para>
  /// Image-based lights have color and intensity, which are used to tint and scale the colors of
  /// the environment map. The color of the <see cref="Texture"/>, the <see cref="Color"/>,
  /// <see cref="DiffuseIntensity"/>/<see cref="SpecularIntensity"/> and <see cref="HdrScale"/> are 
  /// multiplied to get the final diffuse and specular light intensities which can be used in the 
  /// lighting equations.
  /// </para>
  /// <para>
  /// <strong>Diffuse only:</strong> The <see cref="SpecularIntensity"/> can be set to
  /// <see cref="float.NaN"/> to disable the specular light contribution. This creates a pure
  /// diffuse light. As mentioned above, the <see cref="BlendMode"/> determines whether the diffuse
  /// light is added to the scene (<see cref="BlendMode"/> = 0) or replaces the ambient light and
  /// other image-based lights (<see cref="BlendMode"/> = 1).
  /// </para>
  /// <para>
  /// <strong>Specular only:</strong> The <see cref="DiffuseIntensity"/> can be set to
  /// <see cref="float.NaN"/> to disable the diffuse light contribution. This creates a pure
  /// specular light, which can be used for reflections only.
  /// </para>
  /// <para><strong>Localized reflections:</strong>Usually, environment maps are treated as if the
  /// scene in the environment map is infinitely far away. If an image-based light is used to create
  /// reflection of a box-shaped room, the reflections will not properly align with the walls of the
  /// room. To correct the reflections, <see cref="EnableLocalizedReflection"/> can be set to
  /// <see langword="true"/>. In this case the reflections are computed to fit a given box. This box
  /// is either equal to the <see cref="BoxShape"/> of the light (property <see cref="Shape"/>) or
  /// to an axis-aligned box defined by <see cref="LocalizedReflectionBox"/>. The box is always
  /// aligned with the local space of the <see cref="LightNode"/>.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong>When the <see cref="ImageBasedLight"/> is cloned the 
  /// <see cref="Encoding"/> and the <see cref="Texture"/> are not duplicated. These properties are
  /// copied by reference.
  /// </para>
  /// <para><strong>Usage tips:</strong>
  /// <list type="bullet">
  /// <item>
  /// If you only want to add environment map reflections to a game level, add one infinite
  /// image-based light with diffuse intensity set to <see cref="float.NaN"/>. This way the light
  /// only adds reflections and does not influence the ambient light of the scene.
  /// </item>
  /// <item>
  /// Image-based lights with a diffuse intensity can be used to replace ambient lighting. Ambient
  /// lighting is usually very "flat", e.g. an ambient light adds only one color. An image-based
  /// light can create more interesting ambient lighting. For example, if the environment map
  /// contains the blue sky and a brown ground, then objects lit by the image-based light will be
  /// blue on top and brown at the bottom.
  /// </item>
  /// <item>
  /// Image-based lights can also be used to add "light bounces". For example, if the environment
  /// map contains a yellow wall, objects near the yellow wall will receive a yellow "bounce light".
  /// </item>
  /// <item>
  /// Environment maps defined by image-based lights can be used in effects using the environment
  /// map semantics, see e.g. <see cref="SceneEffectParameterSemantics.EnvironmentMap"/>.
  /// </item>
  /// <item>
  /// To get proper reflections in rooms, create one image-based light per room. Set
  /// <see cref="EnableLocalizedReflection"/> to <see langword="true"/>. Set an AABB in
  /// <see cref="LocalizedReflectionBox"/> which is aligned with the floor, ceiling and walls of the
  /// room. <see cref="LightNode"/>s can be rotated to align with rooms which are not axis-aligned
  /// with the world space.
  /// </item>
  /// <item>
  /// It is recommended to position the image-based lights at the eye-level of the player and to
  /// capture the environment maps from this position.
  /// </item>
  /// <item>
  /// If you want to smoothly fade out an image-based light over time, you can fade the
  /// <see cref="BlendMode"/> from 1 (alpha-blend) to 0 (additive), and at the same time fade the
  /// <see cref="DiffuseIntensity"/> and <see cref="SpecularIntensity"/> to 0. This creates a smooth
  /// change in lighting.
  /// </item>
  /// </list>
  /// </para> 
  /// </remarks>
  public class ImageBasedLight : Light
  {
    // Notes:
    // Related keywords are: light probe, environment map, ...
    // Possible new properties:
    // - DiffuseMipLevelBias, SpecularMipLevelBias to allow user to choose higher 
    //   or lower mip levels.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // An ID for sorting, see ImageBasedLightRenderer.
    private static int NextId;
    internal int Id = Interlocked.Increment(ref NextId);
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the RGB color of the light.
    /// </summary>
    /// <value>The RGB color of the light. The default value is (1, 1, 1).</value>
    /// <remarks>
    /// This property defines only the color of the light source - not its intensity.
    /// </remarks>
    public Vector3F Color { get; set; }


    /// <summary>
    /// Gets or sets the diffuse intensity of the light.
    /// </summary>
    /// <value>The diffuse intensity of the light. The default value is 1.</value>
    /// <remarks>
    /// <para>
    /// Setting the diffuse intensity to <see cref="float.NaN"/> has a special meaning and is used
    /// to disable the diffuse contribution of this light. That means, if the intensity is
    /// <see cref="float.NaN"/>, the <see cref="ImageBasedLight"/> does not influence the diffuse
    /// light buffer of the scene.
    /// </para>
    /// <para>
    /// <see cref="Color"/> and <see cref="DiffuseIntensity"/> are separate properties so the values
    /// can be adjusted independently.
    /// </para>
    /// </remarks>
    public float DiffuseIntensity { get; set; }


    /// <summary>
    /// Gets or sets the specular intensity of the light.
    /// </summary>
    /// <value>The specular intensity of the light. The default value is 1.</value>
    /// <remarks>
    /// <para>
    /// Setting the specular intensity to <see cref="float.NaN"/> has a special meaning and is used
    /// to disable the specular contribution of this light. That means, if the intensity is
    /// <see cref="float.NaN"/>, the <see cref="ImageBasedLight"/> does not influence the specular
    /// light buffer of the scene.
    /// </para>
    /// <para>
    /// <see cref="Color"/> and <see cref="SpecularIntensity"/> are separate properties so the
    /// values can be adjusted independently.
    /// </para>
    /// </remarks>
    public float SpecularIntensity { get; set; }


    /// <summary>
    /// Gets or sets the HDR scale of the light.
    /// </summary>
    /// <value>The HDR scale of the light. The default value is 1.</value>
    /// <remarks>
    /// The <see cref="HdrScale"/> is an additional intensity factor. The factor is applied to the
    /// <see cref="Color"/> and <see cref="DiffuseIntensity"/>/<see cref="SpecularIntensity"/> when
    /// high dynamic range lighting (HDR lighting) is enabled.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Hdr")]
    public float HdrScale { get; set; }


    /// <summary>
    /// Gets or sets the bounding shape of the light volume.
    /// </summary>
    /// <value>
    /// A <see cref="Geometry.Shapes.Shape" /> that describes the light volume (the area that is hit
    /// by the light). The shape type must be <see cref="InfiniteShape"/> or <see cref="BoxShape"/>.
    /// Other shapes are not allowed. The default value is
    /// <see cref="Geometry.Shapes.Shape.Infinite"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <see cref="Shape"/> must not be <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <see cref="Shape"/> type must be <see cref="InfiniteShape"/> or <see cref="BoxShape"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public new Shape Shape
    {
      get { return base.Shape; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (!(value is InfiniteShape) && !(value is BoxShape))
          throw new GraphicsException("ImageBasedLight.Shape must be an InfiniteShape or a BoxShape.");

        base.Shape = value;
      }
    }


    /// <summary>
    /// Gets or sets the cube map texture.
    /// </summary>
    /// <value>
    /// The cube map texture. The default value is <see langword="null"/>, which means the
    /// <see cref="ImageBasedLight"/> is disabled.
    /// </value>
    public TextureCube Texture { get; set; }


    /// <summary>
    /// Gets or sets the color encoding used by the cube map texture.
    /// </summary>
    /// <value>
    /// The color encoding used by the <see cref="Texture"/>. The default value is
    /// <see cref="ColorEncoding.SRgb"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public ColorEncoding Encoding
    {
      get { return _encoding; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _encoding = value;
      }
    }
    private ColorEncoding _encoding;


    /// <summary>
    /// Gets or sets the blend mode for the diffuse light contribution.
    /// </summary>
    /// <value>
    /// The blend mode of the image-based light: 0 = additive blending, 1 = alpha blending.
    /// Intermediate values between 0 and 1 are allowed. The default value is 1 (alpha blending).
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or greater than 1.
    /// </exception>
    public float BlendMode
    {
      get { return _blendMode; }
      set
      {
        if (value < 0 || value > 1)
          throw new ArgumentOutOfRangeException("value", "The blend mode must be a value in the range [0, 1].");

        _blendMode = value;
      }
    }
    private float _blendMode;


    /// <summary>
    /// Gets or sets the relative distance over which light effect falls off.
    /// </summary>
    /// <value>
    /// The relative distance over which the light is falls off. The value is in the range [0, 1].
    /// The default is 0.1 (= 10 %).
    /// </value>
    /// <remarks>
    /// <para>
    /// This value is only used for <see cref="ImageBasedLight"/> where the <see cref="Shape"/> is a
    /// <see cref="BoxShape"/>. If this value is 0, the whole box is lit. If this value is 1, the
    /// light has full effect in the center and has no effect at the sides of the box. If the value
    /// is between 0 and 1, the falloff affects only the outer border; e.g if the 
    /// <see cref="FalloffRange"/> is 0.1, the falloff happens only in the outer 10% of the box.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public float FalloffRange { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the cube map reflection is localized.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the reflection is localized; otherwise, <see langword="false" />.
    /// The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If <see cref="EnableLocalizedReflection"/> is <see langword="false"/>, the cube map is
    /// treated as if the content of the cube map is infinitely far away. If
    /// <see cref="EnableLocalizedReflection"/> is <see langword="true"/>, the cube map is treated
    /// as if the content was captured from a finite box around the light.
    /// </para>
    /// <para>
    /// The projection box used to localize reflection is defined using
    /// <see cref="LocalizedReflectionBox"/>. (The <see cref="LocalizedReflectionBox"/> can be
    /// <see langword="null"/>. In this case the <see cref="Shape"/> is used to localize the
    /// reflections.)
    /// </para>
    /// </remarks>
    public bool EnableLocalizedReflection { get; set; }


    /// <summary>
    /// Gets or sets the axis-aligned bounding box used to localize the cube map reflection when 
    /// <see cref="EnableLocalizedReflection"/> is set.
    /// </summary>
    /// <value>
    /// The axis-aligned bounding box used to localize the cube map reflection. This value can be
    /// <see langword="null"/> in which case the <see cref="Shape"/> is used to localize the
    /// reflection. The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property is only used if the <see cref="EnableLocalizedReflection"/> is 
    /// <see langword="true"/>. The axis-aligned bounding box is defined in the local space of the 
    /// <see cref="LightNode"/>. The box can be scaled and rotated using the
    /// <see cref="LightNode"/> 's <see cref="SceneNode.ScaleLocal"/> and
    /// <see cref="SceneNode.PoseLocal"/> properties.
    /// </remarks>
    public Aabb? LocalizedReflectionBox { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBasedLight"/> class.
    /// </summary>
    /// <param name="texture">The cube map texture.</param>
    public ImageBasedLight(TextureCube texture)
    {
      Color = new Vector3F(1);
      DiffuseIntensity = 1;
      SpecularIntensity = 1;
      HdrScale = 1;
      Shape = Shape.Infinite;
      Texture = texture;
      _encoding = ColorEncoding.Rgbm;
      _blendMode = 1;
      EnableLocalizedReflection = false;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBasedLight"/> class.
    /// </summary>
    public ImageBasedLight()
      : this(null)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Light CreateInstanceCore()
    {
      return new ImageBasedLight();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Light source)
    {
      // Clone Light properties.
      base.CloneCore(source);

      // Clone ImageBasedLight properties.
      var sourceTyped = (ImageBasedLight)source;
      Color = sourceTyped.Color;
      DiffuseIntensity = sourceTyped.DiffuseIntensity;
      SpecularIntensity = sourceTyped.SpecularIntensity;
      HdrScale = sourceTyped.HdrScale;
      Shape = sourceTyped.Shape.Clone();
      Texture = sourceTyped.Texture;
      Encoding = sourceTyped.Encoding;
      BlendMode = sourceTyped.BlendMode;
      FalloffRange = sourceTyped.FalloffRange;
      EnableLocalizedReflection = sourceTyped.EnableLocalizedReflection;
      LocalizedReflectionBox = sourceTyped.LocalizedReflectionBox;
    }
    #endregion


    /// <inheritdoc/>
    public override Vector3F GetIntensity(float distance)
    {
      float diffuse = Numeric.IsNaN(DiffuseIntensity) ? 0.0f : DiffuseIntensity;
      float specular = Numeric.IsNaN(SpecularIntensity) ? 0.0f : SpecularIntensity;
      return Color * Math.Max(diffuse, specular) * HdrScale;
    }
    #endregion
  }
}
