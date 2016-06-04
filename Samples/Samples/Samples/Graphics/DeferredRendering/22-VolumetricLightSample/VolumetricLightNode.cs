#if !WP7 && !WP8
using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Graphics
{
  /// <summary>
  /// Represents a volumetric light effect (light shafts) for a single light.
  /// </summary>
  /// <remarks>
  /// Simply add the node to the children of LightNode. The light type must be 
  /// <see cref="PointLight"/>, <see cref="Spotlight"/> or <see cref="ProjectorLight"/>.
  /// </remarks>
  public class VolumetricLightNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the color (tint color and effect intensity).
    /// </summary>
    /// <value>The color (tint color and effect intensity). The default value is (1, 1, 1).</value>
    public Vector3F Color { get; set; }


    /// <summary>
    /// Gets or sets the number of samples.
    /// </summary>
    /// <value>The number of samples. The default value is 10.</value>
    public int NumberOfSamples
    {
      get { return _numberOfSamples; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "NumberOfSamples must be greater than 0.");

        _numberOfSamples = value;
      }
    }
    private int _numberOfSamples;


    /// <summary>
    /// Gets or sets the mipmap bias.
    /// </summary>
    /// <value>The mipmap bias. The default value is 0.</value>
    /// <remarks>
    /// Use <see cref="MipMapBias"/> greater than 0 to sample a higher (= lower resolution) mipmap
    /// level of the light texture.
    /// </remarks>
    public int MipMapBias
    {
      get { return _mipMapBias; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "MipMapBias must be 0 or positive.");

        _mipMapBias = value;
      }
    }
    private int _mipMapBias;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="VolumetricLightNode"/> class.
    /// </summary>
    public VolumetricLightNode()
    {
      IsRenderable = true;

      Shape = Shape.Empty;

      Color = new Vector3F(1, 1, 1);
      NumberOfSamples = 10;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone()" />
    public new VolumetricLightNode Clone()
    {
      return (VolumetricLightNode)base.Clone();
    }


    /// <inheritdoc />
    protected override SceneNode CreateInstanceCore()
    {
      return new VolumetricLightNode();
    }


    /// <inheritdoc />
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone VolumetricLightNode properties.
      var sourceTyped = (VolumetricLightNode)source;
      Color = sourceTyped.Color;
      NumberOfSamples = sourceTyped.NumberOfSamples;
      MipMapBias = sourceTyped.MipMapBias;
    }
    #endregion


    /// <inheritdoc/>
    protected override void OnParentChanged(SceneNode oldParent, SceneNode newParent)
    {
      if (newParent != null && !(newParent is LightNode))
        throw new GraphicsException("VolumetricLightNode can only be parented to a LightNode.");

      // Use same bounding shape as the parent light.
      Shape = (newParent != null) ? newParent.Shape : Shape.Empty;

      base.OnParentChanged(oldParent, newParent);
    }
    #endregion
  }
}
#endif