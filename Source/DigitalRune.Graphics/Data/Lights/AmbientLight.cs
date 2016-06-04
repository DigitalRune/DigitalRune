// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents an ambient light (indirect light).
  /// </summary>
  /// <remarks>
  /// <para>
  /// The ambient light represents indirect light in a scene. Indirect light has no origin, 
  /// direction or range, it has only color and intensity.
  /// </para>
  /// <para>
  /// <see cref="Color"/>, <see cref="Intensity"/>, and <see cref="HdrScale"/> are multiplied to get
  /// the final light intensity which can be used in the lighting equations. 
  /// </para>
  /// <para>
  /// When using a low dynamic range lighting (LDR lighting) the ambient light intensity is 
  /// <i>L<sub>ambient</sub></i> = <i>Color<sub>RGB</sub></i> · <i>Intensity</i>
  /// </para>
  /// <para>
  /// When using a high dynamic range lighting (HDR lighting) the ambient light intensity is 
  /// <i>L<sub>ambient</sub></i> = <i>Color<sub>RGB</sub></i> · <i>Intensity</i> · <i>HdrScale</i>
  /// </para>
  /// <para>
  /// Hemispheric Lighting: If <see cref="HemisphericAttenuation"/> is greater than 0, then the 
  /// <see cref="AmbientLight"/> acts as a hemispheric light with an up direction of (0, 1, 0) - see
  /// <see cref="Vector3F.Up"/>. (A <see cref="LightNode"/> needs to be created to orient a 
  /// light within a 3D scene.) A hemispheric light uses the normal vector of the lit surface to 
  /// attenuate the light intensity. See <see cref="HemisphericAttenuation"/> for more information.
  /// </para>
  /// <para>
  /// The default <see cref="Shape"/> of an ambient light is an <see cref="InfiniteShape"/> which 
  /// covers the whole game world. It is allowed to set a different shape to create a local light.
  /// </para>
  /// </remarks>
  public class AmbientLight : Light
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
    /// A <see cref="Geometry.Shapes.Shape"/> that describes the light volume (the area that is hit 
    /// by the light).
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
    /// Gets or sets the intensity of the light.
    /// </summary>
    /// <value>The intensity of the light. The default value is 1.</value>
    /// <remarks>
    /// <see cref="Color"/> and <see cref="Intensity"/> are separate properties so the values can be 
    /// adjusted independently.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
    public float Intensity { get; set; }


    /// <summary>
    /// Gets or sets the HDR scale of the light.
    /// </summary>
    /// <value>The HDR scale of the light. The default value is 1.</value>
    /// <remarks>
    /// The <see cref="HdrScale"/> is an additional intensity factor. The factor is applied to the 
    /// <see cref="Color"/> and <see cref="Intensity"/> when high dynamic range lighting (HDR 
    /// lighting) is enabled.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float HdrScale { get; set; }


    /// <summary>
    /// Gets or sets the hemispheric attenuation factor.
    /// </summary>
    /// <value>
    /// The hemispheric attenuation factor in the range [0, 1]. The default value is 0.7f.
    /// </value>
    /// <remarks>
    /// <para>
    /// Normal ambient lighting does not depend on the direction of the normal vector of the lit
    /// surface. Hemispheric lighting depends on the normal vector and the up vector of the light: 
    /// A lit surface point is brightest if the normal points is parallel to the up vector, and it 
    /// is darkest if the normal is orthogonal to the light's up vector or pointing in the down 
    /// direction. 
    /// </para>
    /// If <see cref="HemisphericAttenuation"/> is 0, this light is ambient light without 
    /// hemispheric attenuation. If <see cref="HemisphericAttenuation"/> is 1, this light is a 
    /// hemispheric light. Usually, <see cref="HemisphericAttenuation"/> is set to a value 
    /// in between. For example, if it is set to 0.7, then every surface point is lit with at least 
    /// 30% intensity and only surface points with up point normals are lit with 100% intensity.
    /// </remarks>
    public float HemisphericAttenuation { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="AmbientLight"/> class.
    /// </summary>
    public AmbientLight()
    {
      Color = Vector3F.One;
      Intensity = 1;
      HdrScale = 1;
      HemisphericAttenuation = 0.7f;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Light CreateInstanceCore()
    {
      return new AmbientLight();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void CloneCore(Light source)
    {
      // Clone Light properties.
      base.CloneCore(source);

      // Clone AmbientLight properties.
      var sourceTyped = (AmbientLight)source;
      Color = sourceTyped.Color;
      Intensity = sourceTyped.Intensity;
      HdrScale = sourceTyped.HdrScale;
      HemisphericAttenuation = sourceTyped.HemisphericAttenuation;
      Shape = source.Shape.Clone();
    }
    #endregion


    /// <inheritdoc/>
    public override Vector3F GetIntensity(float distance)
    {
      return Color * (Intensity * HdrScale);
    }
    #endregion
  }
}
