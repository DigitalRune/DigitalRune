// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a directional light.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Directional lights have color, intensity, and direction - but no position or range. They emit
  /// light in the form of parallel rays. Directional lights can be used to model light sources 
  /// which are positioned at infinite distance, such as the sun. 
  /// </para>
  /// <para>
  /// The <see cref="DirectionalLight"/> object defines the light properties of a directional light
  /// that shines in forward direction (0, 0, -1) - see <see cref="Vector3F.Forward"/>. A 
  /// <see cref="LightNode"/> needs to be created to orient a light within a 3D scene.
  /// </para>
  /// <para>
  /// <see cref="Color"/>, <see cref="DiffuseIntensity"/>/<see cref="SpecularIntensity"/>, and
  /// <see cref="HdrScale"/> are multiplied to get the final diffuse and specular light intensities 
  /// which can be used in the lighting equations. 
  /// </para>
  /// <para>
  /// When using a low dynamic range lighting (LDR lighting) the light intensities are
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// Diffuse light intensity <i>L<sub>diffuse</sub></i> = 
  /// <i>Color<sub>RGB</sub></i> · <i>DiffuseIntensity</i>
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Specular light intensity <i>L<sub>specular</sub></i> = 
  /// <i>Color<sub>RGB</sub></i> · <i>SpecularIntensity</i>
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// When using a high dynamic range lighting (HDR lighting) the light intensities are
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// Diffuse light intensity <i>L<sub>diffuse</sub></i> = 
  /// <i>Color<sub>RGB</sub></i> · <i>DiffuseIntensity</i> · <i>HdrScale</i>
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Specular light intensity <i>L<sub>specular</sub></i> = 
  /// <i>Color<sub>RGB</sub></i> · <i>SpecularIntensity</i> · <i>HdrScale</i>
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// A pure diffuse light can be created by setting <see cref="DiffuseIntensity"/> &gt; 0 and 
  /// <see cref="SpecularIntensity"/> = 0. A pure specular light can be created by setting
  /// <see cref="DiffuseIntensity"/> = 0 and <see cref="SpecularIntensity"/> &gt; 0.
  /// </para>
  /// <para>
  /// The default <see cref="Shape"/> of a directional light is an <see cref="InfiniteShape"/> which
  /// covers the whole game world. It is allowed to set a different shape to create a local light.
  /// </para>
  /// <para>
  /// A 2D texture (see <see cref="Texture"/>) can be assigned to the directional light. By default
  /// no texture is assigned. If a texture is set, the directional light projects the texture
  /// onto the lit surroundings (using texture wrapping to get an "infinite" texture).
  /// <see cref="TextureOffset"/> and <see cref="TextureScale"/> can be used to change how the
  /// texture is projected.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When the <see cref="DirectionalLight"/> is cloned the 
  /// <see cref="Texture"/> is not duplicated. The <see cref="Texture"/> is copied by reference.
  /// </para>
  /// </remarks>
  public class DirectionalLight : Light
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the shape of the light volume.
    /// </summary>
    /// <value>
    /// A <see cref="Geometry.Shapes.Shape"/> that describes the light volume (the area that is
    /// hit by the light).
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <inheritdoc cref="Light.Shape"/>
    public new Shape Shape
    {
      get { return base.Shape; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        base.Shape = value;
      }
    }


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
    /// <see cref="Color"/> and <see cref="DiffuseIntensity"/> are separate properties so the values 
    /// can be adjusted independently.
    /// </remarks>
    public float DiffuseIntensity { get; set; }


    /// <summary>
    /// Gets or sets the specular intensity of the light.
    /// </summary>
    /// <value>The specular intensity of the light. The default value is 1.</value>
    /// <remarks>
    /// <see cref="Color"/> and <see cref="SpecularIntensity"/> are separate properties so the 
    /// values can be adjusted independently.
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float HdrScale { get; set; }


    /// <summary>
    /// Gets or sets the texture which is projected by this directional light.
    /// </summary>
    /// <value>The texture. The default value is <see langword="null"/>.</value>
    public Texture2D Texture { get; set; }


    /// <summary>
    /// Gets or sets the texture offset.
    /// </summary>
    /// <value>The texture offset. The default value is (0, 0).</value>
    public Vector2F TextureOffset { get; set; }


    /// <summary>
    /// Gets or sets the texture scale.
    /// </summary>
    /// <value>The texture scale. The default value is (1, 1).</value>
    public Vector2F TextureScale { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectionalLight"/> class.
    /// </summary>
    public DirectionalLight()
    {
      Color = Vector3F.One;
      DiffuseIntensity = 1;
      SpecularIntensity = 1;
      HdrScale = 1;
      TextureScale = Vector2F.One;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
 
    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Light CreateInstanceCore()
    {
      return new DirectionalLight();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void CloneCore(Light source)
    {
      // Clone Light properties.
      base.CloneCore(source);

      // Clone DirectionalLight properties.
      var sourceTyped = (DirectionalLight)source;
      Color = sourceTyped.Color;
      DiffuseIntensity = sourceTyped.DiffuseIntensity;
      SpecularIntensity = sourceTyped.SpecularIntensity;
      HdrScale = sourceTyped.HdrScale;
      Texture = sourceTyped.Texture;
      TextureOffset = sourceTyped.TextureOffset;
      TextureScale = sourceTyped.TextureScale;
      Shape = source.Shape.Clone();
    }
    #endregion


    /// <inheritdoc/>
    public override Vector3F GetIntensity(float distance)
    {
      return Vector3F.Max(Color * (DiffuseIntensity * HdrScale), 
                          Color * (SpecularIntensity * HdrScale));
    }
    #endregion
  }
}
