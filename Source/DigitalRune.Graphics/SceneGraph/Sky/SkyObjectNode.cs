// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using MathHelper = Microsoft.Xna.Framework.MathHelper;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents sky objects, like the sun and the moon.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class can render a texture, like a moon texture, and shade this texture to create moon
  /// phases. For correct shading, the object in the texture must be round and fill the entire
  /// texture (or the entire tile of a packed texture).
  /// </para>
  /// <para>
  /// This class can further render two glows, where each glow is very similar to a specular Phong
  /// highlight. The shape of the glow is defined using an exponent, similar to the specular
  /// exponent of Phong-shaded materials. A glow with a high exponent can be used to draw a sun
  /// disk. 
  /// </para>
  /// <para>
  /// The position on the sky is defined by the orientation (see <see cref="SceneNode.PoseWorld"/>) 
  /// of this scene node. The object will be drawn in the forward direction (-z) of this node. The
  /// glows are defined by the properties <see cref="GlowColor0"/>, <see cref="GlowExponent0"/>,
  /// <see cref="GlowColor1"/>, <see cref="GlowExponent1"/> and <see cref="GlowCutoffThreshold"/>.
  /// To disable a glow, set its glow color to 0. All other properties are only used to define the
  /// appearance of the texture. To render only glows, set the <see cref="Texture"/> to 
  /// <see langword="null"/>.
  /// </para>
  /// <para>
  /// Usage examples: To render a moon or a planet, use a moon texture and a single glow. To render
  /// a sun, use two glows - one glow with a high exponent to create the sun disk and one glow with
  /// a low exponent for a huge highlight. To render any other texture into the sky, use a texture
  /// without glows and without a <see cref="SunLight"/> to avoid moon-phase-like shading.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="SkyObjectNode"/> is cloned the 
  /// <see cref="Texture"/> is not duplicated. The <see cref="Texture"/> is copied by reference
  /// (shallow copy). The original <see cref="SkyObjectNode"/> and the cloned instance will
  /// reference the same <see cref="Texture"/>.
  /// </para>
  /// </remarks>
  public class SkyObjectNode : SkyNode
  {
    // Notes:
    // For the moon, the ambient light is actually the earthshine, which could be computed...

    // TODO:
    // - Add support for normal maps in addition to the current automatically 
    //   computed normals. - Since the sky objects are very far away we probably
    //   never need this. If we add normal maps, do we expect that the roundness
    //   of the object is in the normal map, or is the normal map combined with 
    //   the computed hemisphere normal directions?


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the size of the object using its angular diameter.
    /// </summary>
    /// <value>The angular diameter of the object in x and y (specified in radians).</value>
    public Vector2F AngularDiameter { get; set; }


    /// <summary>
    /// Gets or sets the texture.
    /// </summary>
    /// <value>The texture (using premultiplied alpha).</value>
    /// <remarks>
    /// If the object should be shaded to create "moon phases", then the object in the texture must
    /// round and fill the entire tile of the packed texture. The texels outside the circle must be
    /// transparent.
    /// </remarks>
    public PackedTexture Texture { get; set; }


    /// <summary>
    /// Gets or sets the opacity of the rendered texture.
    /// </summary>
    /// <value>The opacity of the rendered texture. The default value is 1.</value>
    public float Alpha { get; set; }


    /// <summary>
    /// Gets or sets the direction to the sun, used to shade the object to create "moon phases".
    /// </summary>
    /// <value>The direction to the sun. This vector is automatically normalized.</value>
    public Vector3F SunDirection
    {
      get { return _sunDirection; }
      set
      {
        _sunDirection = value;
        _sunDirection.TryNormalize();
      }
    }
    private Vector3F _sunDirection;


    /// <summary>
    /// Gets or sets the sun light intensity used to shade the object to create "moon phases".
    /// </summary>
    /// <value>
    /// The intensity of the sun light. If this value is (0, 0, 0), the texture is rendered without
    /// "moon phases". The default value is (1, 1, 1).
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public Vector3F SunLight { get; set; }


    /// <summary>
    /// Gets or sets the ambient light intensity used for the texture.
    /// </summary>
    /// <value>
    /// The intensity of the ambient light used to shade the texture. The ambient light has the same
    /// effect as a tint color.
    /// </value>
    public Vector3F AmbientLight { get; set; }


    /// <summary>
    /// Gets or sets the light wrap parameter.
    /// </summary>
    /// <value>The light wrap parameter in the range [0, 1]. The default value is 0.1.</value>
    /// <remarks>
    /// If this value is 0, then the sun light only lights the hemisphere of the moon/planet which
    /// is visible from the sun. If this value is greater than 0, then the light also lights part of
    /// the back side of the moon/planet. If this value is 1, then the sun light reaches back all
    /// the way to the point opposite of the sun.
    /// </remarks>
    public float LightWrap { get; set; }


    /// <summary>
    /// Gets or sets the smoothness of the light shading.
    /// </summary>
    /// <value>The light smoothness in the range [0, 1]. The default value is 1.</value>
    /// <remarks>
    /// If this value is 0, then the shading of the moon/planet is physically-based. This can lead
    /// to a hard border between the dark and the light side. If this value is greater than 0, the
    /// dark/light transition is softened.
    /// </remarks>
    public float LightSmoothness { get; set; }


    /// <summary>
    /// Gets or sets the color of the first glow.
    /// </summary>
    /// <value>The color of the first glow.</value>
    public Vector3F GlowColor0 { get; set; }


    /// <summary>
    /// Gets or sets the exponent of the first glow.
    /// </summary>
    /// <value>The exponent of the first glow.</value>
    /// <remarks>
    /// This value is like the specular power/exponent of materials with specular Phong shading.
    /// Higher values create sharper, smaller highlights. Lower values create smooth, large 
    /// highlights.
    /// </remarks>
    public float GlowExponent0 { get; set; }


    /// <summary>
    /// Gets or sets the color of the second glow.
    /// </summary>
    /// <value>The color of the second glow.</value>
    public Vector3F GlowColor1 { get; set; }


    /// <summary>
    /// Gets or sets the exponent of the second glow.
    /// </summary>
    /// <value>The exponent of the second glow.</value>
    /// <inheritdoc cref="GlowExponent0"/>
    public float GlowExponent1 { get; set; }


    /// <summary>
    /// Gets or sets the cutoff threshold for glows.
    /// </summary>
    /// <value>The glow cutoff threshold. The default value is 0.001.</value>
    /// <remarks>
    /// Mathematically, each highlight has an infinite size. As an optimization, the highlight is
    /// cut off when its intensity reaches a small value - controlled by the 
    /// <see cref="GlowCutoffThreshold"/>. For example, if the <see cref="GlowCutoffThreshold"/>is
    /// 0.01, then the glow is cut off where its intensity reaches less than 1% of its maximum 
    /// intensity. 
    /// </remarks>
    public float GlowCutoffThreshold { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SkyObjectNode" /> class.
    /// </summary>
    public SkyObjectNode()
    {
      AngularDiameter = new Vector2F(MathHelper.ToRadians(5));
      Alpha = 1;
      SunDirection = new Vector3F(1, 1, 1);
      SunLight = new Vector3F(1, 1, 1);
      AmbientLight = new Vector3F(0, 0, 0);
      LightWrap = 0.1f;
      LightSmoothness = 1;

      GlowColor0 = new Vector3F(0.1f);
      GlowExponent0 = 200;
      GlowColor1 = new Vector3F(0, 0, 0);
      GlowExponent0 = 4000;
      GlowCutoffThreshold = 0.001f;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new SkyObjectNode Clone()
    {
      return (SkyObjectNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new SkyObjectNode();
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SkyNode properties.
      base.CloneCore(source);

      // Clone SkyObjectNode properties.
      var sourceTyped = (SkyObjectNode)source;
      AngularDiameter = sourceTyped.AngularDiameter;
      Texture = sourceTyped.Texture;
      Alpha = sourceTyped.Alpha;
      SunDirection = sourceTyped.SunDirection;
      SunLight = sourceTyped.SunLight;
      AmbientLight = sourceTyped.AmbientLight;
      LightWrap = sourceTyped.LightWrap;
      LightSmoothness = sourceTyped.LightSmoothness;
      GlowColor0 = sourceTyped.GlowColor0;
      GlowExponent0 = sourceTyped.GlowExponent0;
      GlowColor1 = sourceTyped.GlowColor1;
      GlowExponent1 = sourceTyped.GlowExponent1;
      GlowCutoffThreshold = sourceTyped.GlowCutoffThreshold;
    }
    #endregion
  }
}
