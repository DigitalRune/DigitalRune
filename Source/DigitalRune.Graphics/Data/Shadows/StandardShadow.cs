// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a standard shadow that can be used for <see cref="Spotlight"/>s or 
  /// <see cref="ProjectorLight"/>s.
  /// </summary>
  public class StandardShadow : Shadow
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Cached values.
    internal float Near;
    internal float Far;
    internal Matrix View;
    internal Matrix Projection;
    internal float EffectiveDepthBias;      // Bias in world space. Usually negative.
    internal float EffectiveNormalOffset;   // Offset in world space.
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
    /// Gets or sets the default near plane distance for the shadow projection.
    /// </summary>
    /// <value>The default near plane distance for the shadow projection.</value>
    /// <remarks>
    /// Some light sources define their own projection that is used to compute the shadows (e.g.
    /// <see cref="ProjectorLight"/>s). Some lights (e.g. <see cref="Spotlight"/>s) do not
    /// explicitly define a projection. In these cases this value defines the near plane distance
    /// that should be used for the shadow projection matrix.
    /// </remarks>
    public float DefaultNear { get; set; }


    /// <summary>
    /// Gets or sets the depth bias used to remove "surface acne".
    /// </summary>
    /// <value>The depth bias in shadow map texels. The default value is 2.</value>
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
    /// the shadow map resolution.
    /// </para>
    /// <para>
    /// The <see cref="DepthBias"/> is used to remove "surface acne" at surfaces facing the light
    /// source. The <see cref="NormalOffset"/> is used to remove "surface acne" at steep angles,
    /// i.e. surface parallel to the light direction. In practice a combination of
    /// <see cref="DepthBias"/> and <see cref="NormalOffset"/> is required to remove shadow
    /// artifacts.
    /// </para>
    /// </remarks>
    public float DepthBias { get; set; }


    /// <summary>
    /// Gets or sets the normal offset used to remove "surface acne".
    /// </summary>
    /// <value>The normal offset in shadow map texels. The default value is 2.</value>
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
    /// with the shadow map resolution.
    /// </para>
    /// <para>
    /// The <see cref="DepthBias"/> is used to remove "surface acne" at surfaces facing the light
    /// source. The <see cref="NormalOffset"/> is used to remove "surface acne" at steep angles,
    /// i.e. surface parallel to the light direction. In practice a combination of
    /// <see cref="DepthBias"/> and <see cref="NormalOffset"/> is required to remove shadow
    /// artifacts.
    /// </para>
    /// </remarks>
    public float NormalOffset { get; set; }


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
    /// <value>The filter radius in texels. The default value is 1.</value>
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


    #region ----- Obsolete -----

    /// <summary>
    /// Gets or sets the depth bias scale used to remove surface acne.
    /// </summary>
    /// <value>The depth bias scale. The default value is 0.99f.</value>
    /// <remarks>
    /// The depth value of the lit pixel is multiplied with this value. Use values lower than 1 to
    /// remove surface acne. If the value is too low, the shadow becomes visually disconnected from
    /// the occluder (a.k.a. "Peter Panning").
    /// </remarks>
    [Obsolete("The properties DepthBiasScale and DepthBiasOffset have been replaced by DepthBias.")]
    public float DepthBiasScale { get; set; }


    /// <summary>
    /// Gets or sets the depth bias offset used to remove surface acne.
    /// </summary>
    /// <value>The depth bias offset. The default value is -0.001f.</value>
    /// <remarks>
    /// This value is added to the depth value of the lit pixel. Use values lower than 0 to remove
    /// surface acne. If the value is too low, the shadow becomes visually disconnected from the
    /// occluder (a.k.a. "Peter Panning").
    /// </remarks>
    [Obsolete("The properties DepthBiasScale and DepthBiasOffset have been replaced by DepthBias.")]
    public float DepthBiasOffset { get; set; }
    #endregion
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="StandardShadow"/> class.
    /// </summary>
    public StandardShadow()
    {
      DefaultNear = 0.01f;
      DepthBias = 2;
      NormalOffset = 2;
      NumberOfSamples = -1;
      FilterRadius = 1;
      JitterResolution = 2048;
#pragma warning disable 618
      DepthBiasScale = 0.99f;
      DepthBiasOffset = -0.001f;
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
      return new StandardShadow();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shadow source)
    {
      // Clone Shadow properties.
      base.CloneCore(source);

      // Clone StandardShadow properties.
      var sourceTyped = (StandardShadow)source;
      DefaultNear = sourceTyped.DefaultNear;
      DepthBias = sourceTyped.DepthBias;
      NormalOffset = sourceTyped.NormalOffset;
      NumberOfSamples = sourceTyped.NumberOfSamples;
      FilterRadius = sourceTyped.FilterRadius;
      JitterResolution = sourceTyped.JitterResolution;
#pragma warning disable 618
      DepthBiasScale = sourceTyped.DepthBiasScale;
      DepthBiasOffset = sourceTyped.DepthBiasOffset;
#pragma warning restore 618

      // ShadowMap is not cloned!
    }
    #endregion

    #endregion
  }
}
