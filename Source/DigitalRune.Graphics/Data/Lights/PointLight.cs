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
  /// Represents a point light.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Point lights have color, intensity, position and range - but no direction. Point lights give 
  /// off light equally in all directions. The <see cref="PointLight"/> object defines the light 
  /// properties of a point light that is positioned at the origin (0, 0, 0). A 
  /// <see cref="LightNode"/> needs to be created to position a light within a 3D scene.
  /// </para>
  /// <para>
  /// <see cref="Color"/>, <see cref="DiffuseIntensity"/>/<see cref="SpecularIntensity"/>, 
  /// <see cref="HdrScale"/>, and a light distance attenuation factor are multiplied to get the 
  /// final diffuse and specular light intensities which can be used in the lighting equations. 
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
  /// The <see cref="Shape"/> of a point light is a <see cref="SphereShape"/> with a radius
  /// equal to <see cref="Range"/>.
  /// </para>
  /// <para>
  /// A cube map texture (see <see cref="Texture"/>) can be assigned to the point light. By default
  /// no texture is assigned. If a texture is set, the point light projects this texture on its
  /// surroundings (like a disco ball). 
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When the <see cref="PointLight"/> is cloned the 
  /// <see cref="Texture"/> is not duplicated. The <see cref="Texture"/> is copied by reference.
  /// </para>
  /// </remarks>
  public class PointLight : Light
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
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
    /// Gets or sets the range of the light.
    /// </summary>
    /// <value>The range the light. The default value is 2.</value>
    /// <remarks>
    /// The intensity of the light continually decreases from the origin up to range. At a distance 
    /// of range the light intensity is 0. <see cref="Attenuation"/> the shape of the attenuation 
    /// curve. See also <see cref="GraphicsHelper.GetDistanceAttenuation"/>.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// The range of point light cannot be negative.
    /// </exception>
    public float Range
    {
      get { return ((SphereShape)Shape).Radius; }
      set
      {
        if (value < 0)
          throw new ArgumentException("The range of point light cannot be negative.", "value");

        ((SphereShape)Shape).Radius = value;
      }
    }

    
    /// <summary>
    /// Gets or sets the exponent for the distance attenuation.
    /// </summary>
    /// <value>The exponent for the distance attenuation. The default value is 2.</value>
    /// <remarks>
    /// This exponent defines the shape of the distance attenuation curve. See also
    /// <see cref="GraphicsHelper.GetDistanceAttenuation"/>.
    /// </remarks>
    public float Attenuation { get; set; }


    /// <summary>
    /// Gets or sets the cube map texture which is projected by this point light.
    /// </summary>
    /// <value>The cube map texture. The default value is <see langword="null"/>.</value>
    public TextureCube Texture { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PointLight"/> class.
    /// </summary>
    public PointLight()
    {
      Color = Vector3F.One;
      DiffuseIntensity = 1;
      SpecularIntensity = 1;
      HdrScale = 1;
      Shape = new SphereShape(2);
      Attenuation = 2;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Light CreateInstanceCore()
    {
      return new PointLight();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Light source)
    {
      // Clone Light properties.
      base.CloneCore(source);

      // Clone PointLight properties.
      var sourceTyped = (PointLight)source;
      Color = sourceTyped.Color;
      DiffuseIntensity = sourceTyped.DiffuseIntensity;
      SpecularIntensity = sourceTyped.SpecularIntensity;
      HdrScale = sourceTyped.HdrScale;
      Range = sourceTyped.Range;
      Attenuation = sourceTyped.Attenuation;
      Texture = sourceTyped.Texture;

      // Shape does not need to be cloned. It is automatically set in the constructor and 
      // adjusted when the related properties change.
    }
    #endregion


    /// <inheritdoc/>
    public override Vector3F GetIntensity(float distance)
    {
      float attenuation = GraphicsHelper.GetDistanceAttenuation(distance, Range, Attenuation);
      return Vector3F.Max(Color * (DiffuseIntensity * HdrScale * attenuation), 
                          Color * (SpecularIntensity * HdrScale * attenuation));
    }
    #endregion
  }
}
