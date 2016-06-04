// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a billboard in a 3D scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="BillboardNode"/> positions a <see cref="Billboard"/> in a 3D scene. The 
  /// orientation is defined by the <see cref="Graphics.Billboard"/> object - see property 
  /// <see cref="Graphics.Billboard.Orientation"/>.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="BillboardNode"/> is cloned the 
  /// <see cref="Billboard"/> is not duplicated. The <see cref="Billboard"/> is copied by reference 
  /// (shallow copy). The original <see cref="BillboardNode"/> and the cloned instance will 
  /// reference the same <see cref="Graphics.Billboard"/> object.
  /// </para>
  /// </remarks>
  /// <seealso cref="DigitalRune.Graphics.Billboard"/>
  /// <seealso cref="ImageBillboard"/>
  /// <seealso cref="TextBillboard"/>
  public class BillboardNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the billboard.
    /// </summary>
    /// <value>The billboard.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Billboard Billboard
    {
      get { return _billboard; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _billboard = value;
        Shape = value.Shape;
      }
    }
    private Billboard _billboard;


    /// <summary>
    /// Gets or sets the tint color of the billboard instance.
    /// </summary>
    /// <value>The tint color (non-premultiplied). The default value is white (1, 1, 1).</value>
    public Vector3F Color { get; set; }


    /// <summary>
    /// Gets or sets the opacity of the billboard instance.
    /// </summary>
    /// <value>The opacity. The default value is 1 (opaque).</value>
    public float Alpha { get; set; }


    /// <summary>
    /// Gets or sets the normalized animation time. (Overrides the animation time of an 
    /// <see cref="ImageBillboard"/>.)
    /// </summary>
    /// <value>
    /// The normalized animation time where 0 marks the start of the animation and 1 marks the end 
    /// of the animation. <see cref="float.NaN"/> can be set to use the value set in the
    /// <see cref="ImageBillboard"/>; otherwise, the property overrides the value set in the
    /// <see cref="ImageBillboard"/>. The default value is <see cref="float.NaN"/>.
    /// </value>
    /// <remarks>
    /// The <see cref="ImageBillboard.Texture"/> can contain multiple animation frames. The 
    /// normalized animation time determines the current frame. (See <see cref="PackedTexture"/> 
    /// for more information.)
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or greater than 1.
    /// </exception>
    public float AnimationTime
    {
      get { return _animationTime; }
      set
      {
        if (value < 0 || value > 1)
          throw new ArgumentOutOfRangeException("value", "The normalized animation time must be a value in the range [0, 1].");

        _animationTime = value;
      }
    }
    private float _animationTime;


    /// <summary>
    /// Gets the normal vector of the billboard in world space.
    /// </summary>
    /// <value>The normal vector of the billboard in world space.</value>
    /// <remarks>
    /// The normal vector is the defined by the z-axis (0, 0, 1) in local space.
    /// </remarks>
    public Vector3F Normal
    {
      get 
      {
        // Z axis = 3rd column vector
        return PoseWorld.Orientation.GetColumn(2);
      }
    }


    /// <summary>
    /// Gets the axis vector of the billboard in world space.
    /// </summary>
    /// <value>The axis vector of the billboard in world space.</value>
    /// <remarks>
    /// The axis vector is the up direction (0, 1, 0) in local space.
    /// </remarks>
    public Vector3F Axis
    {
      get
      {
        // 2nd column vector
        return PoseWorld.Orientation.GetColumn(1);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="BillboardNode" /> class.
    /// </summary>
    /// <param name="billboard">The billboard.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="billboard"/> is <see langword="null"/>.
    /// </exception>
    public BillboardNode(Billboard billboard)
    {
      if (billboard == null)
        throw new ArgumentNullException("billboard");

      IsRenderable = true;
      _billboard = billboard;
      Shape = billboard.Shape;
      Color = new Vector3F(1, 1, 1);
      Alpha = 1.0f;
      _animationTime = float.NaN;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----
    
    /// <inheritdoc cref="SceneNode.Clone"/>
    public new BillboardNode Clone()
    {
      return (BillboardNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new BillboardNode(Billboard);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone BillboardNode properties.
      var sourceTyped = (BillboardNode)source;
      Color = sourceTyped.Color;
      Alpha = sourceTyped.Alpha;
      _animationTime = sourceTyped.AnimationTime;
    }
    #endregion

    #endregion
  }
}
