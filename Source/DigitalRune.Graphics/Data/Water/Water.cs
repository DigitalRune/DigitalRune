// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines the visual properties of a body of water, e.g. a river, a lake or an ocean.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class defines the appearance of a body of water. Two normal maps
  /// (<see cref="NormalMap0"/> and <see cref="NormalMap1"/>) define the wave ripple structure on
  /// the water surface. The normal maps are scrolled over the water surface to create the sense of
  /// an animated water surface.
  /// </para>
  /// <para>
  /// The water surface is rendered using a specular highlight, a reflection and the refracted
  /// scene under the water surface. Underwater is lit using light extinction and scattering,
  /// similar to exponential fog. <see cref="UnderwaterFogDensity"/> defines the extinction
  /// properties which create the water color.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When the <see cref="Water"/> is cloned the normal map textures
  /// are not duplicated. The textures are copied by reference.
  /// </para>
  /// </remarks>
  /// <seealso cref="WaterNode"/>
  public class Water
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the first normal map.
    /// </summary>
    /// <value>The first normal map.</value>
    public Texture2D NormalMap0 { get; set; }


    /// <summary>
    /// Gets or sets the second normal map.
    /// </summary>
    /// <value>The second normal map.</value>
    public Texture2D NormalMap1 { get; set; }


    /// <summary>
    /// Gets or sets the scale of the first normal map.
    /// </summary>
    /// <value>The scale of the first normal map. The default value is 1.</value>
    public float NormalMap0Scale { get; set; }


    /// <summary>
    /// Gets or sets the scale of the second normal map.
    /// </summary>
    /// <value>The scale of the second normal map. The default value is 1.</value>
    public float NormalMap1Scale { get; set; }


    /// <summary>
    /// Gets or sets the scroll velocity of the first normal map.
    /// </summary>
    /// <value>The scroll velocity of the first normal map.</value>
    /// <remarks>
    /// This velocity defines the movement direction and speed of the normal map relative to the
    /// local space of the <see cref="WaterNode"/>. The y component of this vector is usually
    /// ignored because water waves are only moving horizontally.
    /// </remarks>
    public Vector3F NormalMap0Velocity { get; set; }


    /// <summary>
    /// Gets or sets the scroll velocity of the second normal map.
    /// </summary>
    /// <value>The scroll velocity of the second normal map.</value>
    /// <inheritdoc cref="NormalMap0Velocity"/>
    public Vector3F NormalMap1Velocity { get; set; }


    /// <summary>
    /// Gets or sets the strength of the first normal map.
    /// </summary>
    /// <value>The strength of the first normal map. The default value is 1.</value>
    /// <remarks>
    /// Modify this value to change the intensity of the normal maps. Lower values make the water
    /// surface smoother. The water surface is not influenced by the normal map when this value is
    /// 0.
    /// </remarks>
    public float NormalMap0Strength { get; set; }


    /// <summary>
    /// Gets or sets the strength of the second normal map.
    /// </summary>
    /// <value>The strength of the second normal map. The default value is 1.</value>
    /// <inheritdoc cref="NormalMap0Strength"/>
    public float NormalMap1Strength { get; set; }


    /// <summary>
    /// Gets or sets the tint color of the specular highlight.
    /// </summary>
    /// <value>The tint color of the specular highlight. The default value is (1, 1, 1).</value>
    public Vector3F SpecularColor { get; set; }


    /// <summary>
    /// Gets or sets the specular exponent which defines the size of the specular highlight.
    /// </summary>
    /// <value>The specular exponent which defines the size of the specular highlight.</value>
    public float SpecularPower { get; set; }


    /// <summary>
    /// Gets or sets the tint color of the reflection.
    /// </summary>
    /// <value>The tint color of the reflection. The default value is (1, 1, 1).</value>
    public Vector3F ReflectionColor { get; set; }


    /// <summary>
    /// Gets or sets the intensity of distortion effects for any reflections.
    /// </summary>
    /// <value>
    /// The intensity of distortion effects for any reflections. The default value is 0.1.
    /// </value>
    public float ReflectionDistortion { get; set; }


    /// <summary>
    /// Gets or sets the tint color of the refraction.
    /// </summary>
    /// <value>The tint color of the refraction. The default value is (1, 1, 1).</value>
    public Vector3F RefractionColor { get; set; }


    /// <summary>
    /// Gets or sets the intensity of distortion effects for any refractions.
    /// </summary>
    /// <value>
    /// The intensity of distortion effects for any refractions. The default value is 0.1.
    /// </value>
    public float RefractionDistortion { get; set; }


    /// <summary>
    /// Gets or sets the underwater fog density, which defines how far you can see underwater.
    /// </summary>
    /// <value>The underwater fog density for red, green and blue.</value>
    /// <remarks>
    /// If this value is high, the light extinction in the water is high. If this value is low,
    /// the water is more transparent. Natural water has a higher extinction of red, which creates
    /// blue/green water. 
    /// </remarks>
    public Vector3F UnderwaterFogDensity { get; set; }


    /// <summary>
    /// Gets or sets the bias for the Fresnel effect approximation.
    /// </summary>
    /// <value>The bias for the Fresnel effect approximation. The default value is 0.02.</value>
    /// <remarks>
    /// <see cref="FresnelBias"/> is usually equal to 1 - <see cref="FresnelScale"/>.
    /// </remarks>
    public float FresnelBias { get; set; }


    /// <summary>
    /// Gets or sets the scale for the Fresnel effect approximation.
    /// </summary>
    /// <value>The scale for the Fresnel effect approximation. The default value is 0.98.</value>
    /// <inheritdoc cref="FresnelBias"/>
    public float FresnelScale { get; set; }


    /// <summary>
    /// Gets or sets the exponent for the Fresnel effect.
    /// </summary>
    /// <value>The exponent for the Fresnel effect. The default value is 5.</value>
    public float FresnelPower { get; set; }


    /// <summary>
    /// Gets or sets the intersection softness.
    /// </summary>
    /// <value>The intersection softness. The default value is 0.5.</value>
    /// <remarks>
    /// The water effect fades out when the water surface is intersected by geometry. This avoids
    /// sharp water edges. The fade out effect depends on the distance from the water surface to
    /// the geometry. The <see cref="IntersectionSoftness"/> defines the distance where the water
    /// effect fades out. If <see cref="IntersectionSoftness"/> is 0.5, the water is fully visible
    /// when the distance to the underwater geometry is 0.5 world space units.
    /// </remarks>
    public float IntersectionSoftness { get; set; }


    /// <summary>
    /// Gets or sets the color of the water.
    /// </summary>
    /// <value>The color of the water.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public Vector3F WaterColor { get; set; }


    /// <summary>
    /// Gets or sets the color/intensity of subsurface scattering effect.
    /// </summary>
    /// <value>The color/intensity of subsurface scattering effect.</value>
    /// <remarks>
    /// The upper parts of waves are usually lighter because some light enters the back side of the
    /// wave and comes back out at the front side. <see cref="ScatterColor"/> determines the 
    /// intensity of this effect.
    /// </remarks>
    public Vector3F ScatterColor { get; set; }


    /// <summary>
    /// Gets or sets the foam map.
    /// </summary>
    /// <value>The foam map.</value>
    /// <remarks>
    /// Foam is automatically blended to the water when the distance between the water surface and
    /// intersecting geometry is low (see <see cref="FoamShoreIntersection"/> or at the top of high
    /// waves (see <see cref="FoamCrestMin"/> and <see cref="FoamCrestMax"/>).
    /// </remarks>
    public Texture2D FoamMap { get; set; }


    /// <summary>
    /// Gets or sets the scale of foam map.
    /// </summary>
    /// <value>
    /// The scale of the foam map.
    /// </value>
    public float FoamMapScale { get; set; }


    /// <summary>
    /// Gets or sets a factor indicating how much foam is distorted by waves.
    /// </summary>
    /// <value>
    /// The factor indicating how much foam is distorted by waves.
    /// </value>
    public float FoamDistortion { get; set; }


    /// <summary>
    /// Gets or sets the color of the foam.
    /// </summary>
    /// <value>
    /// The color of the foam.
    /// </value>
    public Vector3F FoamColor { get; set; }


    /// <summary>
    /// Gets or sets the amount of foam where the water intersects geometry (e.g. the shore).
    /// </summary>
    /// <value>
    /// The amount of foam where the water intersects geometry (e.g. the shore).
    /// </value>
    /// <remarks>
    /// Foam is automatically rendered where water intersects geometry, e.g. at the shore or
    /// where rocks intersect the water. <see cref="FoamShoreIntersection"/> defines the 
    /// distance from the intersection up to which foam is rendered. For example, if 
    /// <see cref="FoamShoreIntersection"/> is 0.5, then foam starts when the distance between
    /// the water surface an the geometry is 0.5 world space units.
    /// </remarks>
    public float FoamShoreIntersection { get; set; }


    /// <summary>
    /// Gets or sets the wave crest height where foam starts.
    /// </summary>
    /// <value>
    /// The wave crest height where foam starts
    /// </value>
    /// <remarks>
    /// </remarks>
    public float FoamCrestMin { get; set; }


    /// <summary>
    /// Gets or sets the wave crest height where foam is fully visible.
    /// </summary>
    /// <value>
    /// The wave crest height where foam is fully visible.
    /// </value>
    public float FoamCrestMax { get; set; }


    /// <summary>
    /// Gets or sets the number of samples used to compute caustics.
    /// </summary>
    /// <value>
    /// The number of samples used to compute caustics.
    /// </value>
    /// <remarks>
    /// <para>
    /// <strong>Important:</strong> Currently, the <see cref="WaterRenderer"/> renders caustics only 
    /// for <see cref="WaterNode"/>s with <see cref="WaterWaves"/>.
    /// </para>
    /// <para>
    /// Caustics are computed by sampling the water normal map of <see cref="WaterWaves"/>. The
    /// shader takes N x N samples where N is <see cref="CausticsSampleCount"/>. 
    /// <see cref="CausticsSampleOffset"/> defines the distance between samples in world space 
    /// units. <see cref="CausticsDistortion"/> defines how much the normal maps influence the
    /// caustics computation. <see cref="CausticsPower"/> defines the sharpness of the caustics;
    /// very similar to the specular power/exponent of Phong shading.
    /// <see cref="CausticsIntensity"/> defines max brightness of the caustics.
    /// </para>
    /// </remarks>
    public int CausticsSampleCount { get; set; }

    /// <summary>
    /// Gets or sets the caustics sample offset.
    /// </summary>
    /// <value>
    /// The caustics sample offset.
    /// </value>
    /// <inheritdoc cref="CausticsSampleCount"/>
    public float CausticsSampleOffset { get; set; }


    /// <summary>
    /// Gets or sets the caustics distortion.
    /// </summary>
    /// <value>
    /// The caustics distortion.
    /// </value>
    /// <inheritdoc cref="CausticsSampleCount"/>
    public float CausticsDistortion { get; set; }


    /// <summary>
    /// Gets or sets the sharpness of caustics
    /// </summary>
    /// <value>
    /// The sharpness of caustics. 
    /// </value>
    /// <inheritdoc cref="CausticsSampleCount"/>
    public float CausticsPower { get; set; }


    /// <summary>
    /// Gets or sets the maximal brightness of caustics.
    /// </summary>
    /// <value>
    /// The maximal brightness of caustics.
    /// </value>
    /// <inheritdoc cref="CausticsSampleCount"/>
    public float CausticsIntensity { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Water"/> class.
    /// </summary>
    public Water()
    {
      NormalMap0Scale = 1;
      NormalMap1Scale = 1;
      NormalMap0Velocity = new Vector3F(0.1f, 0, 0.3f);
      NormalMap1Velocity = new Vector3F(-0.1f, 0, 0.3f);
      NormalMap0Strength = 1;
      NormalMap1Strength = 1;

      SpecularColor = new Vector3F(1);
      SpecularPower = 1000;

      ReflectionDistortion = 0.1f;
      ReflectionColor = new Vector3F(1);

      RefractionDistortion = 0.1f;
      RefractionColor = new Vector3F(1);

      UnderwaterFogDensity = new Vector3F(0.5f, 0.4f, 0.3f) * 2;

      FresnelBias = 0.02f;
      FresnelScale = 1 - FresnelBias;
      FresnelPower = 5;

      IntersectionSoftness = 0.5f;

      WaterColor = new Vector3F(0.2f, 0.4f, 0.5f);
      ScatterColor = new Vector3F(0.05f, 0.1f, 0.05f) / 2;

      FoamMapScale = 1;
      FoamDistortion = 0.01f;
      FoamColor = new Vector3F(1);
      FoamShoreIntersection = 1;
      FoamCrestMin = 0;
      FoamCrestMax = 1f;

      CausticsSampleCount = 3;
      CausticsSampleOffset = 0.015f;
      CausticsDistortion = 0.5f;
      CausticsPower = 100;
      CausticsIntensity = 2;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="Water"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="Water"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// See class documentation of <see cref="Water"/> (Section "Cloning") for more information 
    /// about cloning.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="Water"/> derived class and <see cref="CloneCore"/> to create a copy of the 
    /// current instance. Classes that derive from <see cref="Water"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </para>
    /// </remarks>
    public Water Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Water"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="Water"/> method, which this 
    /// method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="Water"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private Water CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone Water. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the <see cref="Water"/>
    /// derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="Water"/> derived class must implement this method. A typical implementation is to
    /// simply call the default constructor and return the result.
    /// </para>
    /// </remarks>
    protected virtual Water CreateInstanceCore()
    {
      return new Water();
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="Water"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Water"/> derived class must implement
    /// this method. A typical implementation is to call <c>base.CloneCore(this)</c> to copy all 
    /// properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(Water source)
    {
      NormalMap0 = source.NormalMap0;
      NormalMap1 = source.NormalMap1;
      NormalMap0Scale = source.NormalMap0Scale;
      NormalMap1Scale = source.NormalMap1Scale;
      NormalMap0Velocity = source.NormalMap0Velocity;
      NormalMap1Velocity = source.NormalMap1Velocity;
      NormalMap0Strength = source.NormalMap0Strength;
      NormalMap1Strength = source.NormalMap1Strength;
      SpecularColor = source.SpecularColor;
      SpecularPower = source.SpecularPower;
      ReflectionColor = source.ReflectionColor;
      ReflectionDistortion = source.ReflectionDistortion;
      RefractionColor = source.RefractionColor;
      RefractionDistortion = source.RefractionDistortion;
      UnderwaterFogDensity = source.UnderwaterFogDensity;
      FresnelBias = source.FresnelBias;
      FresnelScale = source.FresnelScale;
      FresnelPower = source.FresnelPower;
      IntersectionSoftness = source.IntersectionSoftness;
      WaterColor = source.WaterColor;
      ScatterColor = source.ScatterColor;
      FoamMap = source.FoamMap;
      FoamMapScale = source.FoamMapScale;
      FoamDistortion = source.FoamDistortion;
      FoamColor = source.FoamColor;
      FoamShoreIntersection = source.FoamShoreIntersection;
      FoamCrestMin = source.FoamCrestMin;
      FoamCrestMax = source.FoamCrestMax;
      CausticsSampleCount = source.CausticsSampleCount;
      CausticsSampleOffset = source.CausticsSampleOffset;
      CausticsDistortion = source.CausticsDistortion;
      CausticsPower = source.CausticsPower;
      CausticsIntensity = source.CausticsIntensity;
    }
    #endregion

    #endregion
  }
}
