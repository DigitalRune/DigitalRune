// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a lens flare effect in a scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="LensFlareNode"/> position a <seealso cref="LensFlare"/> effect in a 3D scene.
  /// Lens flares can be caused by local lights or directional lights, such as the sun. If the lens 
  /// flare effect is caused by a directional light then the light direction is defined by the 
  /// local forward direction (0, 0, -1) of the scene node.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="LensFlareNode"/> is cloned the 
  /// <see cref="LensFlare"/> is not duplicated. The <see cref="LensFlare"/> is copied by reference 
  /// (shallow copy). The original <see cref="LensFlareNode"/> and the cloned instance will 
  /// reference the same <see cref="Graphics.LensFlare"/> object.
  /// </para>
  /// </remarks>
  /// <seealso cref="DigitalRune.Graphics.LensFlare"/>
  public class LensFlareNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the lens flare effect.
    /// </summary>
    /// <value>The lens flare effect.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public LensFlare LensFlare
    {
      get { return _lensFlare; } 
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _lensFlare = value;
        Shape = value.Shape;
      }
    }
    private LensFlare _lensFlare;


    /// <summary>
    /// Gets or sets the intensity of this lens flare node.
    /// </summary>
    /// <value>The intensity of this lens flare node. The default value is 1.</value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    /// <remarks>
    /// The intensity of the lens flare node is multiplied with the base intensity of the lens 
    /// flare.
    /// </remarks>
    public float Intensity
    {
      get { return _intensity; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value");

        _intensity = value;
      }
    }
    private float _intensity;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="LensFlareNode" /> class.
    /// </summary>
    /// <param name="lensFlare">The lens flare effect.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="lensFlare"/> is <see langword="null"/>.
    /// </exception>
    public LensFlareNode(LensFlare lensFlare)
    {
      if (lensFlare == null)
        throw new ArgumentNullException("lensFlare");

      _lensFlare = lensFlare;
      _intensity = 1;

      IsRenderable = true;
      Shape = lensFlare.Shape;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new LensFlareNode Clone()
    {
      return (LensFlareNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new LensFlareNode(LensFlare);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone LensFlareNode properties.
      var sourceTyped = (LensFlareNode)source;
      _intensity = sourceTyped.Intensity;
    }
    #endregion

    #endregion
  }
}
