// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Renders a cloud layer into the distant sky.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="CloudLayerNode"/> is a <see cref="SkyNode"/> that renders a distant layer of 
  /// clouds. <see cref="CloudLayerNode"/> are rendered by the <see cref="SkyRenderer"/>. The clouds
  /// are alpha-blended over the background. (The renderer does not draw the background sky, only 
  /// clouds.) 
  /// </para>
  /// <para>
  /// Clouds are defined by a <see cref="CloudMap"/>, which provides a cloud texture. The cloud 
  /// texture stores the transmittance of the sky (see <see cref="Graphics.CloudMap.Texture"/> for
  /// more information). It is possible to use user-defined textures (see 
  /// <see cref="UserDefinedCloudMap"/>), or dynamically generate clouds at runtime (see 
  /// <see cref="LayeredCloudMap"/>).
  /// </para>
  /// <para>
  /// The clouds are projected into the sky. The property <see cref="SkyCurvature"/> can be used to 
  /// blend between a paraboloid projection (<see cref="SkyCurvature"/> = 1) or a planar projection 
  /// (<see cref="SkyCurvature"/> = 0). A paraboloid projection makes optimal use of the texture
  /// resolution. Therefore, high values like 0.9, should be used for <see cref="SkyCurvature"/>. If
  /// a true paraboloid projection is used (<see cref="SkyCurvature"/> = 1), then the texture fills
  /// the whole sky without any tiling. If the <see cref="SkyCurvature"/> is less than 1, then the 
  /// texture does not cover the whole sky and tiling (texture wrapping) is used to fill the sky. If
  /// the cloud texture is not a seamlessly tiling texture, then <see cref="TextureMatrix"/> must be 
  /// used to change the scale of the texture and hide the seams.
  /// </para>
  /// <para>
  /// The clouds are lit by <see cref="SunLight"/> and <see cref="AmbientLight"/>. A forward
  /// scattering effect is visible in the <see cref="SunDirection"/>. 
  /// <see cref="ForwardScatterExponent"/>, <see cref="ForwardScatterScale"/> and 
  /// <see cref="ForwardScatterOffset"/> define the range and strength of this effect. The clouds 
  /// are also lit in other directions and <see cref="NumberOfSamples"/>/
  /// <see cref="SampleDistance"/> define the quality of the lighting effect.
  /// </para>
  /// <para>
  /// Clouds can be faded out to disappear near the horizon. <see cref="HorizonFade"/> defines the
  /// height where clouds start to fade out.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="CloudLayerNode"/> is cloned the 
  /// <see cref="CloudMap"/> is copied by reference (shallow copy). The original and the cloned
  /// node will reference the same <see cref="Graphics.CloudMap"/> instance.
  /// </para>
  /// </remarks>
  public class CloudLayerNode : SkyNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    internal OcclusionQuery OcclusionQuery;
    internal float QuerySize;       // The total size of the occlusion query in pixels.
    internal bool IsQueryPending;   // true if query was started and result was not yet retrieved.
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------    

    /// <summary>
    /// Gets or sets the cloud map that provides the cloud texture.
    /// </summary>
    /// <value>The cloud map that provides the cloud texture.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public CloudMap CloudMap
    {
      get { return _cloudMap; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _cloudMap = value;
      }
    }
    private CloudMap _cloudMap;


    /// <summary>
    /// Gets or sets the sky curvature.
    /// </summary>
    /// <value>
    /// The sky curvature in the range [0, 1]. If this value is 0, the clouds are projected onto a
    /// plane in the sky (with a lot of foreshortening near the horizon). If this value is 1, the
    /// clouds are projected onto a hemisphere in the sky (no foreshortening near the horizon). The
    /// default value is 0.9.
    /// </value>
    public float SkyCurvature { get; set; }


    /// <summary>
    /// Gets or sets the matrix used to transform the cloud texture.
    /// </summary>
    /// <value>
    /// The matrix used to transform the texture coordinates. The default value is 
    /// <see cref="Matrix33F.Identity"/>.
    /// </value>
    public Matrix33F TextureMatrix { get; set; }


    /// <summary>
    /// Gets or sets the direction to the sun.
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
    /// Gets or sets the sun light intensity used to shade the clouds.
    /// </summary>
    /// <value>The intensity of the sun light.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public Vector3F SunLight { get; set; }


    /// <summary>
    /// Gets or sets the ambient light intensity used to shade the clouds.
    /// </summary>
    /// <value>The intensity of the ambient light used to shade the clouds.</value>
    public Vector3F AmbientLight { get; set; }


    /// <summary>
    /// Gets or sets the number of samples used to compute cloud lighting in the shader.
    /// </summary>
    /// <value>
    /// The number of samples used to compute cloud lighting in the shader. The default value is 8.
    /// </value>
    public int NumberOfSamples { get; set; }


    /// <summary>
    /// Gets or sets the sample distance for cloud lighting.
    /// </summary>
    /// <value>The sample distance for cloud lighting. The default value is 0.004.</value>
    public float SampleDistance { get; set; }


    /// <summary>
    /// Gets or sets the forward scatter exponent used to define the range of the forward scatter 
    /// effect.
    /// </summary>
    /// <value>The forward scatter exponent. The default value is 5.</value>
    /// <remarks>
    /// <para>
    /// When a cloud is in front of the sun, a lot of light is scattered forward to the observer.
    /// Forward scattering can be used to create a nice glow in the sun direction and rim lights 
    /// ("silver linings") at the cloud borders. 
    /// </para>
    /// <para>
    /// The <see cref="ForwardScatterExponent"/> defines the angular range of the forward scatter 
    /// effect. If <see cref="ForwardScatterExponent"/> is large, the effect is limited to areas 
    /// near the sun. If <see cref="ForwardScatterExponent"/> is small, the effect is visible in a 
    /// larger area.
    /// </para>
    /// <para>
    /// <see cref="ForwardScatterScale"/> scales the intensity of the scattered sun light when 
    /// looking straight at the sun. <see cref="ForwardScatterOffset"/> defines how bright the dark 
    /// cloud parts are when looking straight at the sun.
    /// </para>
    /// </remarks>
    public float ForwardScatterExponent { get; set; }


    /// <summary>
    /// Gets or sets the forward scatter exponent used to define the intensity of forward scattered 
    /// sun light.
    /// </summary>
    /// <value>
    /// The forward scatter scale. The default value is 1.
    /// </value>
    /// <inheritdoc cref="ForwardScatterExponent"/>
    public float ForwardScatterScale { get; set; }


    /// <summary>
    /// Gets or sets the forward scatter offset used to define the brightness of dark cloud parts 
    /// when looking straight at the sun.
    /// </summary>
    /// <value>The forward scatter offset. The default value is 0.5.</value>
    /// <inheritdoc cref="ForwardScatterExponent"/>
    public float ForwardScatterOffset { get; set; }


    /// <summary>
    /// Gets or sets a value which determines where the clouds start to fade out towards the 
    /// horizon.
    /// </summary>
    /// <value>
    /// A value which determines where the clouds start to fade out towards the horizon. If this
    /// value is 0, clouds do not fade out towards the horizon. If this value is 1, clouds start to 
    /// fade out at the zenith. The default value is 0.05. 
    /// </value>
    public float HorizonFade { get; set; }


    /// <summary>
    /// Gets or sets the horizon bias which moves the horizon down.
    /// </summary>
    /// <value>
    /// The horizon bias. Positive values move the horizon down. The default value is 0.
    /// </value>
    public float HorizonBias { get; set; }


    /// <summary>
    /// Gets or sets the opacity of the clouds.
    /// </summary>
    /// <value>The opacity of the clouds. The default value is 1.</value>
    public float Alpha { get; set; }


    /// <summary>
    /// Gets or sets the size of the <see cref="SunOcclusion"/> query.
    /// </summary>
    /// <value>The size of the <see cref="SunOcclusion"/> query. The default value is 0.05.</value>
    /// <inheritdoc cref="SunOcclusion"/>
    public float SunQuerySize { get; set; }


    /// <summary>
    /// Gets the sun occlusion.
    /// </summary>
    /// <value>The sun occlusion.</value>
    /// <remarks>
    /// <para>
    /// If <see cref="SunQuerySize"/> is greater than 0, then a hardware occlusion query is used to 
    /// compute how much of the sun is occluded by clouds. The query size is the approximate size of
    /// the sun on screen. This value is used in the occlusion query to determine the visibility of 
    /// the sun. The query size is the height of the sun relative to the viewport. Example: 
    /// <c>QuerySize = 0.1</c> means that the light source is approximately 1/10 of the viewport.
    /// </para>
    /// <para>
    /// The resulting occlusion value is stored in <see cref="SunOcclusion"/>. An occlusion value
    /// of 0 means the sun is not occluded by clouds. An occlusion value of 1 means the sun is 
    /// totally hidden by clouds. This information can be used to change the intensity of sun lens 
    /// flares and similar effects that depend on the visibility of the sun.
    /// </para>
    /// <para>
    /// Hardware occlusion queries usually require one or more frames to complete. This means that 
    /// the value stored in <see cref="SunOcclusion"/> may be one or more frames old.
    /// </para>
    /// <para>
    /// The <see cref="SunOcclusion"/> is not computed in Reach graphics profile. (The occlusion
    /// value is always 0.)
    /// </para>
    /// </remarks>
    public float SunOcclusion
    {
      get
      {
        TryUpdateSunOcclusion();
        return _sunOcclusion;
      }
      internal set { _sunOcclusion = value; }
    }
    private float _sunOcclusion;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudLayerNode" /> class.
    /// </summary>
    /// <param name="cloudMap">The cloud map.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="cloudMap"/> is <see langword="null"/>.
    /// </exception>
    public CloudLayerNode(CloudMap cloudMap)
    {
      if (cloudMap == null)
        throw new ArgumentNullException("cloudMap");

      _cloudMap = cloudMap;
      SkyCurvature = 0.9f;
      TextureMatrix = Matrix33F.Identity;
      SunDirection = new Vector3F(1, 1, 1);
      SunLight = new Vector3F(0.6f);
      AmbientLight = new Vector3F(0.8f);
      NumberOfSamples = 8;
      SampleDistance = 0.004f;
      ForwardScatterExponent = 5;
      ForwardScatterScale = 1;
      ForwardScatterOffset = 0.5f;
      HorizonFade = 0.05f;
      Alpha = 1;
      SunQuerySize = 0.05f;
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing, bool disposeData)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          OcclusionQuery.SafeDispose();
          OcclusionQuery = null;
          SunOcclusion = 0;

          if (disposeData)
            CloudMap.SafeDispose();
        }

        base.Dispose(disposing, disposeData);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new CloudLayerNode Clone()
    {
      return (CloudLayerNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new CloudLayerNode(CloudMap);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone scene node properties.
      base.CloneCore(source);

      // Clone CloudLayerNode properties.
      var sourceTyped = (CloudLayerNode)source;
      SkyCurvature = sourceTyped.SkyCurvature;
      TextureMatrix = sourceTyped.TextureMatrix;
      SunDirection = sourceTyped.SunDirection;
      SunLight = sourceTyped.SunLight;
      AmbientLight = sourceTyped.AmbientLight;
      NumberOfSamples = sourceTyped.NumberOfSamples;
      SampleDistance = sourceTyped.SampleDistance;
      ForwardScatterExponent = sourceTyped.ForwardScatterExponent;
      ForwardScatterScale = sourceTyped.ForwardScatterScale;
      ForwardScatterOffset = sourceTyped.ForwardScatterOffset;
      HorizonFade = sourceTyped.HorizonFade;
      HorizonBias = sourceTyped.HorizonBias;
      Alpha = sourceTyped.Alpha;
      SunQuerySize = sourceTyped.SunQuerySize;
      SunOcclusion = sourceTyped.SunOcclusion;
    }


    /// <summary>
    /// Gets the texture coordinates of the cloud texture in the specified direction.
    /// </summary>
    /// <param name="direction">The normalized direction.</param>
    /// <returns>
    /// The texture coordinates of the cloud texture. (The result is undefined if 
    /// <paramref name="direction"/> does not point towards the sky.)
    /// </returns>
    public Vector2F GetTextureCoordinates(Vector3F direction)
    {
      float x = direction.X;
      float y = direction.Y + HorizonBias;
      float z = direction.Z;

      // We have to map the direction vector to texture coordinates.
      // fPlane(x) = x / y creates texture coordinates for a plane (= a lot of foreshortening).
      // fSphere(x) = x / (2 + 2 * y) creates texture coordinates for a paraboloid mapping (= almost no foreshortening). 
      // fPlane(x) = x / (4 * y) is similar to fSphere(x) = x / (2 + 2 * y) for y near 1.
      Vector2F texCoord = InterpolationHelper.Lerp(
        new Vector2F(x / (4 * y), z / (4 * y)),
        new Vector2F(x / (2 + 2 * y), z / (2 + 2 * y)),
        SkyCurvature);

      Vector3F texCoord3F = new Vector3F(texCoord.X, texCoord.Y, 1);
      texCoord3F = TextureMatrix * texCoord3F;
      return new Vector2F(texCoord3F.X + 0.5f, texCoord3F.Y + 0.5f);
    }


    internal void TryUpdateSunOcclusion()
    {
      if (!IsQueryPending)
        return;

      if (OcclusionQuery != null && OcclusionQuery.IsComplete)
      {
        IsQueryPending = false;
        SunOcclusion = 1 - MathHelper.Clamp(OcclusionQuery.PixelCount / QuerySize, 0, 1);
      }
    }
    #endregion
  }
}
