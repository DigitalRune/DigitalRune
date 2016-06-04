// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a cascaded shadow that can be used for <see cref="DirectionalLight"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A cascaded shadow map splits the camera frustum into separate slices (up to 4). For each slice
  /// a shadow map is computed.
  /// </para>
  /// <para>
  /// A "slice" is also called "cascade interval", "sub-frustum" or simply "cascade". The word
  /// "cascade" actually means "a series of stages" - however, it is common to use the term
  /// "cascade" to describe an individual slice and this term is used here too.
  /// </para>
  /// <para>
  /// The term "split" could refer to the border between two slices (4 cascades = 3 splits) or to
  /// the slices (cascade = split). Therefore, the term "split" has to be used with care.
  /// </para>
  /// <para>
  /// <see cref="Distances"/> defines the cascade split distances and the maximum shadow distance.
  /// </para>
  /// <para>
  /// <see cref="Shadow.PreferredSize"/> defines the size of a single cascade of the cascaded shadow
  /// map.
  /// </para>
  /// </remarks>
  public class CascadedShadow : Shadow
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Cached values.
    // Transformation from world space to shadow map.
    internal Matrix[] ViewProjections = new Matrix[4];
    internal Vector4F EffectiveDepthBias;      // Bias in light space [0, 1] range. Usually negative.
    internal Vector4F EffectiveNormalOffset;   // Offset in world space.
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc cref="Shadow.ShadowMap"/>
    public new RenderTarget2D ShadowMap
    {
      get { return (RenderTarget2D)base.ShadowMap; }
      set { base.ShadowMap = value; }
    }


    /// <summary>
    /// Gets or sets the number of cascades.
    /// </summary>
    /// <value>The number of cascades (1 - 4). The default value is 4.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is less than 1 or greater than 4.
    /// </exception>
    public int NumberOfCascades
    {
      get { return _numberOfCascades; }
      set
      {
        if (value < 1 || value > 4)
          throw new ArgumentOutOfRangeException("value");

        _numberOfCascades = value;
      }
    }
    private int _numberOfCascades;


    /// <summary>
    /// Gets or sets the cascade split distances.
    /// </summary>
    /// <value>
    /// The cascade split distances in world space. The default value is (4, 12, 20, 80).
    /// </value>
    /// <remarks>
    /// <para>
    /// This vector contains the distances where the camera frustum is split in world space in
    /// following order: (split 0-1, split 1-2, split 2-3, max shadow distance). If the
    /// <see cref="NumberOfCascades"/> is less than 4, then the last components are ignored.
    /// </para>
    /// <para>
    /// For example, if <see cref="Distances"/> is (4, 12, 20, 80), then the first cascade covers an
    /// area 4 units in front of the camera. The second cascade covers an area up to 12 units. The
    /// third cascade covers the area up to 20 units. The last cascade always covers the remaining
    /// area. The max. shadow distance is 80. The renderer does not need to render shadows beyond
    /// 80 units in front of the camera.
    /// </para>
    /// </remarks>
    public Vector4F Distances { get; set; }


    /// <summary>
    /// Gets or sets the minimal distance of the light projection to the camera frustum of a
    /// cascade.
    /// </summary>
    /// <value>The minimum light distance from the camera frustum. The default value is 100.</value>
    /// <remarks>
    /// To compute the shadow map an orthographic projection is fitted to the partial frustum of a
    /// cascade. The near plane of this orthographic projection should be moved as close as possible
    /// to the cascade - but not too close in order to catch occluders in front of the cascade.
    /// <see cref="MinLightDistance"/> defines the minimum allowed distance of the shadow projection
    /// near plane from the cascade.
    /// </remarks>
    public float MinLightDistance { get; set; }


    /// <summary>
    /// Gets or sets the depth bias used to remove "surface acne".
    /// </summary>
    /// <value>
    /// The depth bias for each of the 4 cascades in shadow map texels.
    /// The default value is (5, 5, 5, 5).
    /// </value>
    /// <remarks>
    /// <para>
    /// This value is used to modify the depth value of the shadow-receiving pixel. A positive value
    /// moves the receiver closer to the light source (into the light), a negative value moves the
    /// receiver farther away from the light source (into the shadow). If this value is too large,
    /// the shadow becomes visually disconnected from the occluder (a.k.a. "Peter Panning").
    /// </para>
    /// <para>
    /// This value is relative to the shadow map resolution. A depth bias of 1 changes the depth by
    /// the size of one shadow map texel. Therefore, the depth bias values automatically scale with
    /// the shadow map resolution of each cascade. You can set 4 different values to tune the depth
    /// bias for each cascade.
    /// </para>
    /// <para>
    /// The <see cref="DepthBias"/> is used to remove "surface acne" at surfaces facing the light
    /// source. The <see cref="NormalOffset"/> is used to remove "surface acne" at steep angles,
    /// i.e. surface parallel to the light direction. In practice a combination of
    /// <see cref="DepthBias"/> and <see cref="NormalOffset"/> is required to remove shadow
    /// artifacts.
    /// </para>
    /// </remarks>
    public Vector4F DepthBias { get; set; }


    /// <summary>
    /// Gets or sets the normal offset used to remove "surface acne".
    /// </summary>
    /// <value>
    /// The normal offset for each of the 4 cascades in shadow map texels. 
    /// The default value is (2, 2, 2, 2).
    /// </value>
    /// <remarks>
    /// <para>
    /// This value is used to modify the position of shadow receivers. This has the effect of moving
    /// the receiver into the direction of the receiver's surface normal. This helps to remove
    /// "surface acne" especially on steep slopes. If this value is too high, the shadow becomes
    /// visually disconnected from the occluder (a.k.a. "Peter Panning").
    /// </para>
    /// <para>
    /// This value is relative to the shadow map resolution. A normal offset of 1 moves the position
    /// by the size of one shadow map texel. Therefore, the normal offset values automatically scale
    /// with the shadow map resolution of each cascade. You can set 4 different values to tune the
    /// normal offset for each cascade.
    /// </para>
    /// <para>
    /// The <see cref="DepthBias"/> is used to remove "surface acne" at surfaces facing the light
    /// source. The <see cref="NormalOffset"/> is used to remove "surface acne" at steep angles,
    /// i.e. surface parallel to the light direction. In practice a combination of
    /// <see cref="DepthBias"/> and <see cref="NormalOffset"/> is required to remove shadow
    /// artifacts.
    /// </para>
    /// </remarks>
    public Vector4F NormalOffset { get; set; }


    /// <summary>
    /// Gets or sets the number of filter samples.
    /// </summary>
    /// <value>The number of PCF samples. The default value is -1 (see remarks).</value>
    /// <remarks>
    /// If this value is -1 (default), the shadow mask renderer will use predefined and optimized
    /// sampling pattern. If this value is 0, the shadow map is sampled once without any PCF
    /// (percentage closer filtering). If this value is 1, the shadow map is sampled with one
    /// jittered sample without any PCF. If this value is greater than 1, the shadow map is sampled
    /// with the given number of PCF samples.
    /// </remarks>
    public int NumberOfSamples { get; set; }


    /// <summary>
    /// Gets or sets the filter radius.
    /// </summary>
    /// <value>The filter radius in texels. The default value of is 1.</value>
    public float FilterRadius { get; set; }


    /// <summary>
    /// Gets or sets the jitter resolution (for jitter sampling).
    /// </summary>
    /// <value>
    /// The jitter resolution. The jitter resolution is the number of noise texels per world space
    /// unit. The default value is 2048.
    /// </value>
    /// <remarks>
    /// This value is only used when jitter sampling is applied to filter shadow edges. Jitter
    /// sampling uses a noise pattern to choose which shadow map texels are sampled. This noise is
    /// stable in world space. The <see cref="JitterResolution"/> defines the size of the jitter
    /// pattern relative to the world. If the jitter resolution is too high, then the noise becomes
    /// visually unstable when the camera moves (because there are too many noise pixels per screen
    /// pixel). Low jitter resolutions can cause a coarse blocky noise patterns.
    /// </remarks>
    public float JitterResolution { get; set; }


    /// <summary>
    /// Gets or sets the relative distance over which shadows are faded out.
    /// </summary>
    /// <value>
    /// The relative distance over which shadows are faded out. The value is in the range
    /// [0, 1]. The default is 0.1 (= 10 %).
    /// </value>
    /// <remarks>
    /// Near the maximum shadow distance defined by <see cref="Distances"/> shadows are faded
    /// towards the <see cref="ShadowFog"/> value. <see cref="FadeOutRange"/> defines the fade out
    /// interval relative to the size of the last cascade.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public float FadeOutRange { get; set; }


    /// <summary>
    /// Gets or sets the shadow factor that is used beyond the max shadow distance.
    /// </summary>
    /// <value>
    /// The shadow factor that is used beyond the maximum shadow distance defined by
    /// <see cref="Distances"/>. If this value is 0, then objects beyond the maximum shadow distance
    /// are not shadowed. If this value is 1, then objects beyond the max. distance are fully
    /// shadowed. The default value is 0.
    /// </value>
    /// <inheritdoc cref="FadeOutRange"/>
    public float ShadowFog { get; set; }


    /// <summary>
    /// Gets or sets the cascade selection mode.
    /// </summary>
    /// <value>
    /// The cascade selection mode. The default is <see cref="ShadowCascadeSelection.Fast"/> on Xbox
    /// 360 and <see cref="ShadowCascadeSelection.Best"/> on PC.
    /// </value>
    public ShadowCascadeSelection CascadeSelection { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether cascades are visualized for debugging.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if cascades are visualized for debugging; otherwise, 
    /// <see langword="false" />.
    /// </value>
    public bool VisualizeCascades { get; set; }


    /// <summary>
    /// Gets or sets the flags which determine if a cascade is locked.
    /// </summary>
    /// <value>
    /// A 4-elements array which determines if a cascade is locked. The default value is { false,
    /// false, false, false }, which means all 4 cascades are updated every time.
    ///  </value>
    /// <remarks>
    /// <para>
    /// These flags can be used to control shadow map caching. <c>IsCascadeLocked[i]</c> determines
    /// if the <see cref="ShadowMapRenderer"/> updates the cascade with index <c>i</c> or if it
    /// reuses the cached shadow map. Per default, all flags are <see langword="false"/> and the
    /// shadow maps of all cascades are updated every frame. If the flag of a cascade is set to
    /// <see langword="true"/>, the shadow map of the cascade is not rendered; the result of the
    /// last frame is used instead. The application can use this flags to determine when cascades
    /// need to be updated. (Note: If a flag is changed, it keeps this value until it is changed
    /// again. The flags are not automatically reset.)
    /// </para>
    /// <para>
    /// Example usages for shadow map caching:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// Update distant cascades less often.
    /// </item>
    /// <item>
    /// Do not update a cascade if it does not contain any dynamic objects and if the camera has not
    /// move.
    /// </item>
    /// <item>
    /// Distribute cascade updates over several frames. For example: Update cascade 0 and 1 every
    /// frame. Update cascade 2 in every odd numbered frame. Update cascade 3 in every even numbered
    /// frame.
    /// </item>
    /// </list>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public bool[] IsCascadeLocked { get; private set; }


    #region ----- Obsolete -----

    /// <summary>
    /// Gets or sets the split distribution parameter.
    /// </summary>
    /// <value>The split distribution parameter. The default value is 0.9f.</value>
    /// <remarks>
    /// If this value is 0, the camera frustum is split using a uniform splitting scheme (the camera
    /// frustum is split at regular intervals). If this value is 1, the camera frustum is split 
    /// using a logarithmic splitting scheme. <see cref="SplitDistribution"/> can be set to values 
    /// between 0 and 1 to interpolate between uniform and logarithmic splitting.
    /// </remarks>
    [Obsolete("Properties SplitDistribution and MaxDistance have been replaced by the property Distances.")]
    public float SplitDistribution { get; set; }


    /// <summary>
    /// Gets or sets the distance from the camera where the shadow starts to fade out.
    /// </summary>
    /// <value>The fade out distance.</value>
    /// <remarks>
    /// Shadows between [<see cref="FadeOutDistance"/>, <see cref="MaxDistance"/>] are faded out 
    /// towards the <see cref="ShadowFog"/> value.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    [Obsolete("The property FadeOutDistance has been replaced by FadeOutRange.")]
    public float FadeOutDistance { get; set; }


    /// <summary>
    /// Gets or sets the maximum distance from the camera up to which shadows are rendered. 
    /// </summary>
    /// <value>The maximum distance.</value>
    /// <inheritdoc cref="FadeOutDistance"/>
    [Obsolete("Properties SplitDistribution and MaxDistance have been replaced by the property Distances.")]
    public float MaxDistance { get; set; }


    /// <summary>
    /// Gets or sets the depth bias scale of each cascade used to remove surface acne.
    /// </summary>
    /// <value>
    /// The depth bias scale of each cascade. The default value of each cascade is 0.99f. 
    /// </value>
    /// <remarks>
    /// The depth value of the lit pixel is multiplied with this value. Use values lower than 1 to
    /// remove surface acne. If the value is too low, the shadow becomes visually disconnected from
    /// the occluder (a.k.a. "Peter Panning").
    /// </remarks>
    [Obsolete("The properties DepthBiasScale and DepthBiasOffset have been replaced by DepthBias.")]
    public Vector4F DepthBiasScale { get; set; }


    /// <summary>
    /// Gets or sets the depth bias offset of each cascade used to remove surface acne.
    /// </summary>
    /// <value>
    /// The depth bias offset of each cascade. The default value of each cascade is -0.001f.
    /// </value>
    /// <remarks>
    /// This value is added to the depth value of the lit pixel. Use values lower than 0 to remove
    /// surface acne. If the value is too low, the shadow becomes visually disconnected from the
    /// occluder (a.k.a. "Peter Panning").
    /// </remarks>
    [Obsolete("The properties DepthBiasScale and DepthBiasOffset have been replaced by DepthBias.")]
    public Vector4F DepthBiasOffset { get; set; }
    #endregion
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CascadedShadow"/> class.
    /// </summary>
    public CascadedShadow()
    {
      NumberOfCascades = 4;
      Distances = new Vector4F(4, 12, 20, 80);
      MinLightDistance = 100;
      DepthBias = new Vector4F(5);
      NormalOffset = new Vector4F(2);
      NumberOfSamples = -1;
      FilterRadius = 1;
      JitterResolution = 2048;
      FadeOutRange = 0.1f;
      ShadowFog = 0;
#if XBOX
      CascadeSelection = ShadowCascadeSelection.Fast;
#else
      CascadeSelection = ShadowCascadeSelection.Best;
#endif
      IsCascadeLocked = new[] { false, false, false, false };
#pragma warning disable 618
      SplitDistribution = 0.9f;
      FadeOutDistance = 50;
      MaxDistance = 70;
      DepthBiasScale = new Vector4F(0.99f);
      DepthBiasOffset = new Vector4F(-0.001f);
#pragma warning restore 618
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shadow CreateInstanceCore()
    {
      return new CascadedShadow();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shadow source)
    {
      // Clone Shadow properties.
      base.CloneCore(source);

      // Clone CascadedShadow properties.
      var sourceTyped = (CascadedShadow)source;
      NumberOfCascades = sourceTyped.NumberOfCascades;
      Distances = sourceTyped.Distances;
      MinLightDistance = sourceTyped.MinLightDistance;
      DepthBias = sourceTyped.DepthBias;
      NormalOffset = sourceTyped.NormalOffset;
      NumberOfSamples = sourceTyped.NumberOfSamples;
      FilterRadius = sourceTyped.FilterRadius;
      JitterResolution = sourceTyped.JitterResolution;
      FadeOutRange = sourceTyped.FadeOutRange;
      ShadowFog = sourceTyped.ShadowFog;
      CascadeSelection = sourceTyped.CascadeSelection;
      VisualizeCascades = sourceTyped.VisualizeCascades;
      IsCascadeLocked[0] = sourceTyped.IsCascadeLocked[0];
      IsCascadeLocked[1] = sourceTyped.IsCascadeLocked[1];
      IsCascadeLocked[2] = sourceTyped.IsCascadeLocked[2];
      IsCascadeLocked[3] = sourceTyped.IsCascadeLocked[3];
#pragma warning disable 618
      SplitDistribution = sourceTyped.SplitDistribution;
      FadeOutDistance = sourceTyped.FadeOutDistance;
      MaxDistance = sourceTyped.MaxDistance;
      DepthBiasScale = sourceTyped.DepthBiasScale;
      DepthBiasOffset = sourceTyped.DepthBiasOffset;
#pragma warning restore 618

      // ShadowMap is not cloned!
    }
    #endregion


    /// <summary>
    /// Computes the <see cref="Distances"/> for a <see cref="CascadedShadow"/>.
    /// </summary>
    /// <param name="near">The camera near plane distance.</param>
    /// <param name="maxDistance">The maximum shadow distance.</param>
    /// <param name="numberOfCascades">The number of cascades (2, 3 or 4).</param>
    /// <param name="splitDistribution">
    /// The split distribution parameter in the range [0, 1]. If this value is 0, the camera frustum
    /// is split using a uniform splitting scheme (the camera frustum is split at regular
    /// intervals). If this value is 1, the camera frustum is split using a logarithmic splitting
    /// scheme. A value between 0 and 1 can be used to interpolate between uniform and logarithmic
    /// splitting.
    /// </param>
    /// <returns>The split distances, which can be assigned to <see cref="Distances"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfCascades"/> is greater than 4. Or,
    /// <paramref name="splitDistribution"/> is not in the range [0, 1].
    /// </exception>
    public static Vector4F ComputeSplitDistances(float near, float maxDistance,
      int numberOfCascades, float splitDistribution)
    {
      // lambda = 0 --> Uniform split scheme
      // lambda = 1 --> Logarithmic split scheme
      float lambda = splitDistribution;

      if (lambda < 0f || lambda > 1.0f)
        throw new ArgumentOutOfRangeException("splitDistribution", "lambda must be in the range [0, 1].");
      if (numberOfCascades > 4)
        throw new ArgumentOutOfRangeException("numberOfCascades", "The number of cascades must be 4 or less.");

      var splits = new Vector4F();

      // Compute distances using the practical split scheme:
      //   SplitUniform(i) = near + (maxDistance - near) * i / numberOfSplits;
      //   SplitLogarithmic(i) = near * (maxDistance / near)^(i / numberOfSplits);
      //   Split(i) = lambda * SplitLogarithmic(i) + (1 - lambda) * SplitUniform(i)
      for (int i = 1; i < numberOfCascades; i++)
      {
        float splitUniform = near + (maxDistance - near) * i / numberOfCascades;
        float splitLogarithmic = near * (float)Math.Pow(maxDistance / near, i / (float)numberOfCascades);
        splits[i - 1] = lambda * splitLogarithmic + (1 - lambda) * splitUniform;
      }

      splits[numberOfCascades - 1] = maxDistance;
      return splits;
    }
    #endregion
  }
}
