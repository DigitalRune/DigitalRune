using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Creates a shadow using Variance Shadow Mapping (VSM). This shadow can be used for
  /// <see cref="DirectionalLight"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Variance shadow maps are computed using statistical properties. This allows the shadow map
  /// to be blurred. The result is a very smooth shadow which is prone to light bleeding artifacts
  /// (bright edges in the shadows where occluders overlap). The shadow map requires two channels
  /// to store depth and depth².
  /// </para>
  /// <para>
  /// VSM shadows are best used to create smooth shadows of distant hills.
  /// </para>
  /// </remarks>
  public class VarianceShadow : Shadow
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Cached values:
    internal Matrix ViewProjection;   // Transformation from world space to shadow map projection space.
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the shadow map.
    /// </summary>
    /// <value>The shadow map.</value>
    public new RenderTarget2D ShadowMap
    {
      get { return (RenderTarget2D)base.ShadowMap; }
      set { base.ShadowMap = value; }
    }


    /// <summary>
    /// Gets or sets the minimal distance of the light projection to the camera frustum of a 
    /// cascade.
    /// </summary>
    /// <value>The minimum light distance from the camera frustum. The default value is 1000.</value>
    /// <remarks>
    /// To compute the shadow map an orthographic projection is fitted to the partial frustum of a
    /// cascade. The near plane of this orthographic projection should be moved as close as possible
    /// to the cascade - but not too close in order to catch occluders in front of the cascade.
    /// <see cref="MinLightDistance"/> defines the minimum allowed distance of the shadow projection
    /// near plane from the cascade.
    /// </remarks>
    public float MinLightDistance { get; set; }


    /// <summary>
    /// Gets or sets the maximum distance from the camera up to which shadows are rendered.
    /// (Only used if <see cref="TargetArea"/> is<see langword="null"/>.)
    /// </summary>
    /// <value>The maximum distance.</value>
    public float MaxDistance { get; set; }


    /// <summary>
    /// Gets or sets the relative distance where shadows are faded out.
    /// </summary>
    /// <value>
    /// The relative distance where shadows are faded out. The value is in the range [0, 1]. The
    /// default is 0.1 (= 10 %).
    /// </value>
    /// <remarks>
    /// Near the maximum shadow distance defined by <see cref="MaxDistance"/> or the
    /// <see cref="TargetArea"/>, shadows are faded towards the <see cref="ShadowFog"/> value.
    /// <see cref="FadeOutRange"/> defines the fade out interval relative to the shadow map size.
    /// </remarks>
    public float FadeOutRange { get; set; }


    /// <summary>
    /// Gets or sets the shadow factor that is used beyond the max shadow distance.
    /// </summary>
    /// <value>
    /// The shadow factor that is used beyond <see cref="MaxDistance"/> or beyond
    /// <see cref="TargetArea"/>. If this value is 0, then objects beyond outside the shadow map
    /// are not shadowed. If this value is 1, then objects outside the shadow map are fully shadowed. 
    /// The default value is 0. 
    /// </value>
    public float ShadowFog { get; set; }


    /// <summary>
    /// Gets or sets the blur post-processor which is used to filter the shadow map.
    /// </summary>
    /// <value>
    /// The blur post-processor which is used to filter the shadow map.
    /// The default value is <see langword="null"/>.
    /// </value>
    public Blur Filter { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this shadow map is locked.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the shadow map is locked; otherwise, <see langword="false" />.
    /// </value>
    /// <remarks>
    /// If the shadow map is locked, the shadow map is not updated. The shadow map of the last
    /// frame is reused. This improves performance if the shadow map contains only static objects. 
    /// </remarks>
    public bool IsLocked { get; set; }


    /// <summary>
    /// Gets or sets the minimum variance.
    /// </summary>
    /// <value>The minimum variance in the range [0, 1]. The default value is 0.</value>
    public float MinVariance { get; set; }


    /// <summary>
    /// Gets or sets the light bleeding reduction.
    /// </summary>
    /// <value>The light bleeding reduction in the range [0, 1]. The default value is 0.</value>
    /// <remarks>
    /// VSM can create light bleeding artifacts (bright edges in the shadows where occluders
    /// overlap). If <see cref="LightBleedingReduction"/> is greater than 0, the shadows is darkened
    /// to hide those artifacts. The disadvantage is that the shadows will appear less smooth.
    /// </remarks>
    public float LightBleedingReduction { get; set; }


    /// <summary>
    /// Gets or sets the target area.
    /// </summary>
    /// <value>The target area. The default value is <see langword="null"/>.</value>
    public Aabb? TargetArea { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="VarianceShadow"/> class.
    /// </summary>
    public VarianceShadow()
    {
      MinLightDistance = 1000;
      MaxDistance = 100;
      FadeOutRange = 0.1f;
      ShadowFog = 0;
      MinVariance = 0;
      LightBleedingReduction = 0;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shadow CreateInstanceCore()
    {
      return new VarianceShadow();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shadow source)
    {
      // Clone Shadow properties.
      base.CloneCore(source);

      // Clone CascadedShadow properties.
      var sourceTyped = (VarianceShadow)source;
      MinLightDistance = sourceTyped.MinLightDistance;
      MaxDistance = sourceTyped.MaxDistance;
      FadeOutRange = sourceTyped.FadeOutRange;
      ShadowFog = sourceTyped.ShadowFog;
      Filter = sourceTyped.Filter;
      IsLocked = sourceTyped.IsLocked;
      MinVariance = sourceTyped.MinVariance;
      LightBleedingReduction = sourceTyped.LightBleedingReduction;
      TargetArea = sourceTyped.TargetArea;

      // ShadowMap is not cloned!
    }
    #endregion
    #endregion
  }
}
