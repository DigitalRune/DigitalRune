// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Renders the sky using simple color gradients.
  /// </summary>
  /// <remarks>
  /// This class represents a sky. Only the top hemisphere is rendered. The sky colors are 
  /// determined by a color gradient which is defined with the following properties: 
  /// <see cref="FrontColor"/>, <see cref="ZenithColor"/>, <see cref="BackColor"/>, 
  /// <see cref="GroundColor"/>, <see cref="FrontZenithShift"/>, <see cref="BackZenithShift"/>, 
  /// <see cref="FrontGroundShift"/>, <see cref="BackGroundShift"/>.
  /// </remarks>
  public class GradientSkyNode : SkyNode
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
    /// Gets or sets the color of the horizon below the sun.
    /// </summary>
    /// <value>The color of the horizon below the sun (using premultiplied alpha).</value>
    public Vector4F FrontColor { get; set; }


    /// <summary>
    /// Gets or sets the color of the horizon opposite the sun.
    /// </summary>
    /// <value>The color of the horizon opposite the sun (using premultiplied alpha).</value>
    public Vector4F BackColor { get; set; }


    /// <summary>
    /// Gets or sets the color at the zenith.
    /// </summary>
    /// <value>The color at the zenith (using premultiplied alpha).</value>
    public Vector4F ZenithColor { get; set; }


    /// <summary>
    /// Gets or sets the color at the ground.
    /// </summary>
    /// <value>The color at the ground (using premultiplied alpha).</value>
    public Vector4F GroundColor { get; set; }


    /// <summary>
    /// Gets or sets the relative height where the sky color is the average of the
    /// <see cref="FrontColor"/> and the <see cref="ZenithColor"/>.
    /// </summary>
    /// <value>
    /// The relative height where the sky color is the average of the <see cref="FrontColor"/> and
    /// the <see cref="ZenithColor"/>, in the range [0, 1]. The default value is 0.5.
    /// </value>
    /// <remarks>
    /// <para>
    /// The sky color in the direction of the sun is created by interpolating the 
    /// <see cref="FrontColor"/> and the <see cref="ZenithColor"/>. If this value is 0.5, the
    /// average color of the gradient (= average of <see cref="FrontColor"/> and 
    /// <see cref="ZenithColor"/>) is in the middle. If this value is less than 0.5, then the
    /// average color is shifted down to the horizon. If this value is greater than 0.5, then the
    /// average color is shifted up to the zenith.
    /// </para>
    /// </remarks>
    public float FrontZenithShift { get; set; }


    /// <summary>
    /// Gets or sets the relative height where the sky color is the average of the
    /// <see cref="BackColor"/> and the <see cref="ZenithColor"/>.
    /// </summary>
    /// <value>
    /// The relative height where the sky color is the average of the <see cref="BackColor"/> and
    /// the <see cref="ZenithColor"/>, in the range [0, 1]. The default value is 0.5.
    /// </value>
    /// <remarks>
    /// <para>
    /// The sky color opposite to the sun is created by interpolating the <see cref="BackColor"/> 
    /// and the <see cref="ZenithColor"/>. If this value is 0.5, the average color of the gradient
    /// (= average of <see cref="BackColor"/> and the <see cref="ZenithColor"/>) is in the middle.
    /// If this value is less than 0.5, then the average color is shifted down to the horizon. If
    /// this value is greater than 0.5, then the average color is shifted up to the zenith.
    /// </para>
    /// </remarks>
    public float BackZenithShift { get; set; }


    /// <summary>
    /// Gets or sets the relative height where the sky color is the average of the
    /// <see cref="FrontColor"/> and the <see cref="GroundColor"/>.
    /// </summary>
    /// <value>
    /// The relative height where the sky color is the average of the <see cref="FrontColor"/> and
    /// the <see cref="GroundColor"/>, in the range [0, 1]. The default value is 0.5.
    /// </value>
    /// <remarks>
    /// <para>
    /// The sky color in the direction of the sun is created by interpolating the 
    /// <see cref="FrontColor"/> and the <see cref="GroundColor"/>. If this value is 0.5, the
    /// average color of the gradient (= average of <see cref="FrontColor"/> and 
    /// <see cref="GroundColor"/>) is in the middle. If this value is less than 0.5, then the
    /// average color is shifted up to the horizon. If this value is greater than 0.5, then the
    /// average color is shifted down to the ground.
    /// </para>
    /// </remarks>
    public float FrontGroundShift { get; set; }


    /// <summary>
    /// Gets or sets the relative height where the sky color is the average of the
    /// <see cref="BackColor"/> and the <see cref="GroundColor"/>.
    /// </summary>
    /// <value>
    /// The relative height where the sky color is the average of the <see cref="BackColor"/> and
    /// the <see cref="GroundColor"/>, in the range [0, 1]. The default value is 0.5.
    /// </value>
    /// <remarks>
    /// <para>
    /// The sky color opposite to the sun is created by interpolating the <see cref="BackColor"/> 
    /// and the <see cref="GroundColor"/>. If this value is 0.5, the average color of the gradient
    /// (= average of <see cref="BackColor"/> and <see cref="GroundColor"/>) is in the middle. If
    /// this value is less than 0.5, then the average color is shifted up to the horizon. If this
    /// value is greater than 0.5, then the average color is shifted down to the ground.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public float BackGroundShift { get; set; }


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
    /// Initializes a new instance of the <see cref="GradientSkyNode" /> class.
    /// </summary>
    public GradientSkyNode()
    {
      SunDirection = new Vector3F(1, 1, 1);
      FrontColor = new Vector4F(0.9f, 0.5f, 0, 1);
      ZenithColor = new Vector4F(0, 0.4f, 0.9f, 1);
      BackColor = new Vector4F(0.4f, 0.6f, 0.9f, 1);
      GroundColor = new Vector4F(1, 0.8f, 0.6f, 1);
      FrontZenithShift = 0.5f;
      BackZenithShift = 0.5f;
      FrontGroundShift = 0.5f;
      BackGroundShift = 0.5f;

      CieSkyParameters = CieSkyParameters.Type12;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new GradientSkyNode Clone()
    {
      return (GradientSkyNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new GradientSkyNode();
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SkyNode properties.
      base.CloneCore(source);

      // Clone GradientSkyNode properties.
      var sourceTyped = (GradientSkyNode)source;
      SunDirection = sourceTyped.SunDirection;
      FrontColor = sourceTyped.FrontColor;
      BackColor = sourceTyped.BackColor;
      ZenithColor = sourceTyped.ZenithColor;
      GroundColor = sourceTyped.GroundColor;
      FrontZenithShift = sourceTyped.FrontZenithShift;
      BackZenithShift = sourceTyped.BackZenithShift;
      FrontGroundShift = sourceTyped.FrontGroundShift;
      BackGroundShift = sourceTyped.BackGroundShift;
      CieSkyParameters = sourceTyped.CieSkyParameters;
      CieSkyStrength = sourceTyped.CieSkyStrength;
    }
    #endregion
  }
}
