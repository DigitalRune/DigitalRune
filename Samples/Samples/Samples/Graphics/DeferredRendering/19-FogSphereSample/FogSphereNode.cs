#if !WP7 && !WP8
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Graphics
{
  /// <summary>
  /// Describes a sphere of fog.
  /// </summary>
  public class FogSphereNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the fog color.
    /// </summary>
    /// <value>The fog color. The default value is (1, 1, 1).</value>
    public Vector3F Color { get; set; }


    /// <summary>
    /// Gets or sets the fog density.
    /// </summary>
    /// <value>The fog density. The default value is 0.6.</value>
    public float Density { get; set; }


    /// <summary>
    /// Gets or sets the blend mode.
    /// </summary>
    /// <value>
    /// The blend mode of the billboard: 0 = additive blending, 1 = alpha blending. Intermediate 
    /// values between 0 and 1 are allowed. The default value is 0 (additive blending).
    /// </value>
    public float BlendMode { get; set; }


    /// <summary>
    /// Gets or sets the falloff of the fog intensity.
    /// </summary>
    /// <value>
    /// The falloff of the fog intensity. Use 1 to create fog that is linear relative to the
    /// distance traveled inside the fog. Use values greater than 1 to create non-linear fog.
    /// </value>
    public float Falloff { get; set; }


    /// <summary>
    /// Gets or sets the intersection softness.
    /// </summary>
    /// <value>The intersection softness. The default value is 1.</value>
    /// <remarks>
    /// The fog effect fades out when the fog is intersected by geometry. This avoids
    /// sharp fog edges at the front side of the fog sphere. The fade out effect depends on the 
    /// distance from the water surface to the geometry. The <see cref="IntersectionSoftness"/> 
    /// defines the distance where the fog effect fades out.  If <see cref="IntersectionSoftness"/> 
    /// is 1, the fog is fully visible when the distance to the geometry is 1 world space unit.
    /// </remarks>
    public float IntersectionSoftness { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FogSphereNode"/> class.
    /// </summary>
    public FogSphereNode()
    {
      IsRenderable = true;
      Shape = new SphereShape(1);

      Color = new Vector3F(1, 1, 1);
      Density = 0.6f;
      BlendMode = 0;
      Falloff = 5;
      IntersectionSoftness = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone()" />
    public new FogSphereNode Clone()
    {
      return (FogSphereNode)base.Clone();
    }


    /// <inheritdoc />
    protected override SceneNode CreateInstanceCore()
    {
      return new FogSphereNode();
    }


    /// <inheritdoc />
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone FogSphereNode properties.
      var sourceTyped = (FogSphereNode)source;
      Color = sourceTyped.Color;
      Density = sourceTyped.Density;
      BlendMode = sourceTyped.BlendMode;
      Falloff = sourceTyped.Falloff;
      IntersectionSoftness = sourceTyped.IntersectionSoftness;
    }
    #endregion

    #endregion
  }
}
#endif