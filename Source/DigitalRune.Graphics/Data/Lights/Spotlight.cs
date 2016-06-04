// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a spotlight.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Spotlights have color, intensity, position, direction and range. The light emitted from a 
  /// spotlight is shaped like a cone. The light intensity is maximal at the center and diminishes 
  /// towards the outside of the cone. This effect is called "spotlight falloff" (see below). The 
  /// <see cref="Spotlight"/> object defines the light properties of a spotlight positioned at the 
  /// origin (0, 0, 0) that shines in forward direction (0, 0, -1) - see 
  /// <see cref="Vector3F.Forward"/>. A <see cref="LightNode"/> needs to be created to position and 
  /// orient a spotlight within a 3D scene.
  /// </para>
  /// <para>
  /// <see cref="Color"/>, <see cref="DiffuseIntensity"/>/<see cref="SpecularIntensity"/>, 
  /// <see cref="HdrScale"/>, the spotlight falloff (see 
  /// <see cref="GraphicsHelper.GetAngularAttenuation"/>) and the light distance attenuation factor 
  /// (see <see cref="GraphicsHelper.GetDistanceAttenuation"/>) are multiplied to get the final 
  /// diffuse and specular light intensities which can be used in the lighting equations. 
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
  /// A pure diffuse spotlight can be created by setting <see cref="DiffuseIntensity"/> &gt; 0 and 
  /// <see cref="SpecularIntensity"/> = 0. A pure specular spotlight can be created by setting
  /// <see cref="DiffuseIntensity"/> = 0 and <see cref="SpecularIntensity"/> &gt; 0.
  /// </para>
  /// <para>
  /// <strong>Spotlight Falloff (Dual-Cone Model):</strong> The light emitted from a spotlight is 
  /// made up of a bright inner cone and a larger outer cone. The amount of light emitted 
  /// continually diminishes from the inner cone to the outer cone. The angle at which the light 
  /// starts to fall off is defined as the <see cref="FalloffAngle"/>. The angle at which the light 
  /// is cut off (light intensity is 0) is defined as the <see cref="CutoffAngle"/>. So the size of 
  /// the inner cone is 2 · <see cref="FalloffAngle"/> and the size of the outer cone is 
  /// 2 · <see cref="CutoffAngle"/>. See also <see cref="GraphicsHelper.GetAngularAttenuation"/>.
  /// </para>
  /// <para>
  /// The <see cref="Shape"/> of a spotlight is a <see cref="PerspectiveViewVolume"/>. 
  /// (A <see cref="Geometry.Shapes.ConeShape"/> would be a better fit, but the 
  /// <see cref="PerspectiveViewVolume"/> was chosen because it is computationally more efficient.) 
  /// </para>
  /// <para>
  /// A 2D texture (see <see cref="Texture"/>) can be assigned to the spotlight. By default
  /// no texture is assigned. If a texture is set, the spotlight acts like a projector and
  /// projects this texture onto the lit surroundings.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When the <see cref="Spotlight"/> is cloned the <see cref="Texture"/>
  /// is not duplicated. The <see cref="Texture"/> is copied by reference.
  /// </para>
  /// </remarks>
  public class Spotlight : Light
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    private ConeShape ConeShape
    {
      get
      {
        var transformedShape = (TransformedShape)Shape;
        return (ConeShape)transformedShape.Child.Shape;
      }
    }


    /// <summary>
    /// Gets or sets the RGB color of the light.
    /// </summary>
    /// <value>The color of the light. The default value is (1, 1, 1).</value>
    /// <remarks>
    /// This property defines only the RGB color of the light source - not its intensity. 
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
    /// <value>The range the light. The default value is 5.</value>
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
      get { return ConeShape.Height; }
      set
      {
        if (value < 0)
          throw new ArgumentException("The range of spotlight cannot be negative.", "value");

        float oldCutoffAngle = CutoffAngle;

        var transformedShape = (TransformedShape)Shape;
        var cone = (ConeShape)transformedShape.Child.Shape;
        ((GeometricObject)transformedShape.Child).Pose = new Pose(new Vector3F(0, 0, -value), transformedShape.Child.Pose.Orientation);
        cone.Height = value;

        // Changing the height and keeping the radius constant changes the CutoffAngle!
        // We must make sure it stays the same:
        CutoffAngle = oldCutoffAngle;
 
      }
    }


    /// <summary>
    /// Gets or sets the falloff (umbra) angle.
    /// </summary>
    /// <value>
    /// The falloff (umbra) angle of the spotlight in radians.
    /// The default value is 0.349 radians (= 20°).
    /// </value>
    /// <remarks>
    /// The falloff angle is the angle between the spotlight direction and the direction at which
    /// the light begins to fall off. (The size of the inner light cone, in which the light is at
    /// full intensity, is 2 · <see cref="FalloffAngle"/>.)
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float FalloffAngle
    {
      get { return _falloffAngle; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "Spotlight falloff angle must not be negative.");

        _falloffAngle = value;
      }
    }
    private float _falloffAngle;


    /// <summary>
    /// Gets or sets the cutoff (penumbra) angle.
    /// </summary>
    /// <value>
    /// The cutoff (penumbra) angle of the spotlight in radians.
    /// The default value is 0.524 radians (= 30°).
    /// </value>
    /// <remarks>
    /// The cutoff angle is the angle between the spotlight direction and the direction at which 
    /// the light is totally cut off. (The size of the outer light cone is 
    /// 2 · <see cref="CutoffAngle"/>.)
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float CutoffAngle
    {
      get
      {
        var cone = ConeShape;
        return (float)Math.Atan(cone.Radius / cone.Height);
      }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "Spotlight cutoff angle must not be negative.");

        var cone = ConeShape;
        float tan = (float)Math.Tan(value);
        if (tan < 0)
          throw new ArgumentOutOfRangeException("value");
        cone.Radius = tan * cone.Height;
      }
    }


    /// <summary>
    /// Gets or sets the attenuation exponent for the distance attenuation.
    /// </summary>
    /// <value>The attenuation exponent. The default value is 2.</value>
    /// <remarks>
    /// This exponent defines the shape of the distance attenuation curve. See also
    /// <see cref="GraphicsHelper.GetDistanceAttenuation"/>.
    /// </remarks>
    public float Attenuation { get; set; }


    /// <summary>
    /// Gets or sets the texture which is projected by this spotlight.
    /// </summary>
    /// <value>The texture. The default value is <see langword="null"/>.</value>
    public Texture2D Texture { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Spotlight"/> class.
    /// </summary>
    public Spotlight()
    {
      Color = Vector3F.One;
      DiffuseIntensity = 1;
      SpecularIntensity = 1;
      HdrScale = 1;
      _falloffAngle = 20.0f * ConstantsF.Pi / 180;
      Shape = new TransformedShape(new GeometricObject(new ConeShape((float)Math.Tan(MathHelper.ToRadians(30)) * 5, 5), new Pose(new Vector3F(0, 0, -5), QuaternionF.CreateRotationX(ConstantsF.PiOver2))));
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
      return new Spotlight();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Light source)
    {
      // Clone Light properties.
      base.CloneCore(source);

      // Clone Spotlight properties.
      var sourceTyped = (Spotlight)source;
      Color = sourceTyped.Color;
      DiffuseIntensity = sourceTyped.DiffuseIntensity;
      SpecularIntensity = sourceTyped.SpecularIntensity;
      HdrScale = sourceTyped.HdrScale;
      Range = sourceTyped.Range;
      FalloffAngle = sourceTyped.FalloffAngle;
      CutoffAngle = sourceTyped.CutoffAngle; // Setting the CutoffAngle will automatically adjust the Shape.
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


    //public override Vector3F GetIntensity(Vector3F position)
    //{
    //  float distance = position.Length;
    //  float angle = !position.IsNumericallyZero ? Vector3F.GetAngle(Vector3F.Forward, position) : 0;
    //  float attenuation = GraphicsHelper.GetDistanceAttenuation(distance, Range, AttenuationExponent);
    //  float spotlightFalloff = GraphicsHelper.GetAngularAttenuation(angle, FalloffAngle, CutoffAngle);
    //  return Vector3F.Max(Color * (DiffuseIntensity * HdrScale * spotlightFalloff * attenuation),
    //                      Color * (SpecularIntensity * HdrScale * spotlightFalloff * attenuation));
    //}
    #endregion
  }
}
