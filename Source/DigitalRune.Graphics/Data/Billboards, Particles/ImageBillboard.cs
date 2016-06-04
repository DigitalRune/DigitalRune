// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents an oriented, textured quad used for drawing impostors, particles, and other 
  /// effects.
  /// </summary>
  /// <inheritdoc/>
  public class ImageBillboard : Billboard
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the billboard texture (with premultiplied alpha).
    /// </summary>
    /// <value>The billboard texture (with premultiplied alpha).</value>
    public PackedTexture Texture { get; set; }


    /// <summary>
    /// Gets or sets the size of the billboard in world space.
    /// </summary>
    /// <value>The size of the billboard in world space. The default value is (1, 1).</value>
    public Vector2F Size
    {
      get { return _size; }
      set
      {
        _size = value;
        Shape.Radius = value.Length / 2;
      }
    }
    private Vector2F _size;


    /// <summary>
    /// Gets or sets the normalized animation time.
    /// </summary>
    /// <value>
    /// The normalized animation time where 0 marks the start of the animation and 1 marks the end 
    /// of the animation. The default value is 0.
    /// </value>
    /// <remarks>
    /// The <see cref="Texture"/> can contain multiple animation frames. The normalized animation 
    /// time determines the current frame. (See <see cref="PackedTexture"/> for more information.)
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
    /// Gets or sets a reference value for alpha testing.
    /// </summary>
    /// <value>
    /// The reference value used in the alpha test. The reference value is a value in the range
    /// [0, 1]. If the alpha of a pixel is less than the reference alpha, the pixel is discarded. 
    /// The default value is 0 (= alpha test disabled).
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or greater than 1.
    /// </exception>
    public float AlphaTest
    {
      get { return _alphaTest; }
      set
      {
        if (value < 0 || value > 1)
          throw new ArgumentOutOfRangeException("value", "The alpha test value must be in the range [0, 1].");

        _alphaTest = value;
      }
    }
    private float _alphaTest;


    /// <summary>
    /// Gets or sets the blend mode.
    /// </summary>
    /// <value>
    /// The blend mode of the billboard: 0 = additive blending, 1 = alpha blending. Intermediate 
    /// values between 0 and 1 are allowed. The default value is 1 (alpha blending).
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or greater than 1.
    /// </exception>
    public float BlendMode
    {
      get { return _blendMode; }
      set
      {
        if (value < 0 || value > 1)
          throw new ArgumentOutOfRangeException("value", "The blend mode must be a value in the range [0, 1].");

        _blendMode = value;
      }
    }
    private float _blendMode;


    /// <summary>
    /// Gets or sets the softness - see remarks.
    /// </summary>
    /// <value>
    /// <para>
    /// The softness of the billboard:<br/>
    /// 0 ... Disabled: The billboard is rendered with hard edges.<br/>
    /// -1 or NaN ... Automatic: The thickness of the billboard is determined automatically.<br/>
    /// &gt;0 ... Manual: The value defines the thickness of the billboard (= soft particle distance
    /// threshold).
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </value>
    /// <remarks>
    /// A regular billboard is rendered using a textured quad, which creates hard edges when it
    /// intersects with other geometry in the scene. A soft billboard (same as "soft particles") 
    /// has a volume and creates soft transitions when it intersects with other geometry.
    /// </remarks>
    public float Softness { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBillboard"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBillboard"/> class.
    /// </summary>
    public ImageBillboard()
      : this (null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBillboard"/> class.
    /// </summary>
    /// <param name="texture">The texture.</param>
    public ImageBillboard(PackedTexture texture)
    {
      Texture = texture;
      Size = new Vector2F(1, 1);
      _alphaTest = 0;
      _blendMode = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="Billboard.Clone"/>
    public new ImageBillboard Clone()
    {
      return (ImageBillboard)base.Clone();
    }


    /// <inheritdoc/>
    protected override Billboard CreateInstanceCore()
    {
      return new ImageBillboard();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Billboard source)
    {
      // Clone Billboard properties.
      base.CloneCore(source);

      // Clone ImageBillboard properties.
      var sourceTyped = (ImageBillboard)source;
      Texture = sourceTyped.Texture;
      Size = sourceTyped.Size;
      _animationTime = sourceTyped.AnimationTime;
      _alphaTest = sourceTyped._alphaTest;
      _blendMode = sourceTyped._blendMode;
      Softness = sourceTyped.Softness;
    }
    #endregion

    #endregion
  }
}
