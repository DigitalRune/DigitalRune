// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Renders the sky using lookup textures which contain color gradients.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class represents a sky. Only the top hemisphere is rendered. The sky colors are 
  /// determined by two lookup textures: <see cref="FrontTexture"/> and <see cref="BackTexture"/>. 
  /// The <see cref="FrontTexture"/> determines the sky colors in the direction of the sun. The 
  /// <see cref="BackTexture"/> determines the sky colors opposite to the sun.
  /// </para>
  /// <para>
  /// The lookup texture are used like this: The columns of the textures represent the sky colors of
  /// a specific time of the day. The first column is for the hour 0 (midnight). The middle column
  /// is for the hour 12 (noon). The last column is for the hour 24 (midnight). Each column contains
  /// the sky colors. The bottom color is the horizon color. The top color is the zenith color.
  /// </para>
  /// <para>
  /// To render the sky, the <see cref="TimeOfDay"/> is used to determine which column of the
  /// textures should be sampled. Then the gradient of the <see cref="FrontTexture"/> is used to
  /// render the sky in the direction of the sun. The gradient of the <see cref="BackTexture"/> is
  /// used to render the sky opposite to the sun. The other sky colors are interpolated from both
  /// gradients.
  /// </para>
  /// <para>
  /// To avoid artifacts in the zenith, the colors of the top row of both textures should be
  /// identical.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="GradientTextureSkyNode"/> is cloned the lookup 
  /// textures are not duplicated. The textures are copied by reference (shallow copy). The original
  /// <see cref="GradientTextureSkyNode"/> and the cloned instance will reference the same textures.
  /// </para>
  /// </remarks>
  public class GradientTextureSkyNode : SkyNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

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
    /// Gets or sets the time of day.
    /// </summary>
    /// <value>The time of day.</value>
    public TimeSpan TimeOfDay { get; set; }


    /// <summary>
    /// Gets or sets the gradient lookup texture for the side facing to the sun.
    /// </summary>
    /// <value>
    /// The gradient lookup texture for the side facing to the sun (using premultiplied alpha).
    /// </value>
    public Texture2D FrontTexture { get; set; }


    /// <summary>
    /// Gets or sets the gradient lookup texture for the side opposite to the sun.
    /// </summary>
    /// <value>
    /// The gradient lookup texture for the side opposite to the sun (using premultiplied alpha).
    /// </value>
    public Texture2D BackTexture { get; set; }


    /// <summary>
    /// Gets or sets the tint color of the sky.
    /// </summary>
    /// <value>
    /// The tint color (using premultiplied alpha) which is multiplied to the color (and alpha) 
    /// from the gradient textures. The default value is opaque white (1, 1, 1, 1).
    /// </value>
    /// <remarks>
    /// This property can be used to tint the sky, change the brightness or change its opacity.
    /// </remarks>
    public Vector4F Color { get; set; }


    /// <summary>
    /// The parameters of the CIE sky luminance distribution.
    /// </summary>
    /// <remarks>
    /// The CIE sky model defines how the luminance is distributed across the sky depending on the
    /// sun direction. When <see cref="CieSkyStrength"/> is greater than 0, this luminance 
    /// distribution is used to attenuate the sky pixels to create a realistic sky brightness.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Allow direct access to struct fields.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public CieSkyParameters CieSkyParameters;


    /// <summary>
    /// Gets or sets the strength of the CIE sky luminance distribution (see 
    /// <see cref="CieSkyParameters"/>).
    /// </summary>
    /// <value>
    /// The strength of the CIE sky luminance distribution in the range [0, 1]. The default value is
    /// 0.
    /// </value>
    /// <remarks>
    /// If this value is 1, the original CIE sky luminance distribution is used (see 
    /// <see cref="CieSkyParameters"/>). This makes only sense if the render pipeline uses HDR. For
    /// LDR the effect of the CIE sky luminance distribution should be lessened using lower values
    /// for <see cref="CieSkyStrength"/>. If <see cref="CieSkyStrength"/> is 0, then the CIE 
    /// luminance distribution is not applied.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float CieSkyStrength { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GradientTextureSkyNode" /> class.
    /// </summary>
    public GradientTextureSkyNode()
    {
      SunDirection = new Vector3F(1, 1, 1);
      TimeOfDay = new TimeSpan(12, 0, 0);
      Color = new Vector4F(1, 1, 1, 1);
      CieSkyParameters = CieSkyParameters.Type12;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new GradientTextureSkyNode Clone()
    {
      return (GradientTextureSkyNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new GradientTextureSkyNode();
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SkyNode properties.
      base.CloneCore(source);

      // Clone GradientTextureSkyNode properties.
      var sourceTyped = (GradientTextureSkyNode)source;
      SunDirection = sourceTyped.SunDirection;
      TimeOfDay = sourceTyped.TimeOfDay;
      FrontTexture = sourceTyped.FrontTexture;
      BackTexture = sourceTyped.BackTexture;
      Color = sourceTyped.Color;
      CieSkyParameters = sourceTyped.CieSkyParameters;
      CieSkyStrength = sourceTyped.CieSkyStrength;
    }
    #endregion
  }
}
