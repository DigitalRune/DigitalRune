// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents fog.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="Fog"/> can be used to define a height-based fog where the fog density 
  /// increases/decreases with height. Height-based fog can be used to create thick layer of fog in 
  /// a valley or on a dungeon floor. The fog is relative to the <see cref="SceneNode.PoseWorld"/>
  /// of a <see cref="FogNode"/>. That means, the height fog moves up/down when the 
  /// <see cref="FogNode"/> moves up/down.
  /// </para>
  /// <para>
  /// The fog density can be specified using either <see cref="Density"/> and 
  /// <see cref="HeightFalloff"/> or <see cref="Density0"/> and <see cref="Density1"/>. 
  /// <see cref="Density"/> defines the fog density at height 0 and the <see cref="HeightFalloff"/>
  /// determines whether the fog density increases or decrease with height. Alternatively, the 
  /// fog density can be set at two reference heights using <see cref="Density0"/> and 
  /// <see cref="Density1"/>. The properties are coupled: If <see cref="Density"/> or 
  /// <see cref="HeightFalloff"/> are changed, <see cref="Density0"/> and <see cref="Density1"/> are
  /// updated automatically and vice versa. In practice, you will use either <see cref="Density"/> 
  /// and <see cref="HeightFalloff"/>, or <see cref="Density0"/> and <see cref="Density1"/> to 
  /// control the fog settings.
  /// </para>
  /// <para>
  /// To disable the fog, simply set <see cref="Density"/> to 0. To create a height-independent fog,
  /// set <see cref="HeightFalloff"/> to 0.
  /// </para>
  /// <para>
  /// The fog color can be specified using <see cref="Color0"/> and <see cref="Color1"/>, where
  /// <see cref="Color0"/> defines the fog color at a height of <see cref="Height0"/>, and 
  /// <see cref="Color1"/> defines the fog color at a height of <see cref="Height1"/>. Colors
  /// between <see cref="Height0"/> and <see cref="Height1"/> are interpolated. 
  /// </para>
  /// <para>
  /// The fog colors, <see cref="Color0"/> and <see cref="Color1"/>, use premultiplied alpha. The
  /// alpha value is usually 1, but lower values can be used to reduce the overall fog intensity.
  /// </para>
  /// <para>
  /// The properties <see cref="Start"/> and <see cref="End"/> define a ramp over which the fog 
  /// fades in.
  /// </para>
  /// </remarks>
  public class Fog : INamedObject
  {
    // Notes:
    // - CryEngine supports a ramp influence which can be used to disable ramp defined 
    //   by Start/End 
    // - If you want to fog out the horizon, use a height-fog and move it with the
    //   camera. This way the fog is relative to the horizon and not the ground.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the fog.
    /// </summary>
    /// <value>The name of the fog.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the density of the fog at a height of 0.
    /// </summary>
    /// <value>The density of the fog at a height of 0.</value>
    /// <remarks>
    /// <para>
    /// The fog density can be specified using either <see cref="Density"/> and 
    /// <see cref="HeightFalloff"/> or <see cref="Density0"/> and <see cref="Density1"/>. 
    /// <see cref="Density"/> defines the fog density at height 0 and the 
    /// <see cref="HeightFalloff"/>determines whether the fog density increases or decrease with
    /// height. Alternatively, the  fog density can be set at two reference heights using 
    /// <see cref="Density0"/> and <see cref="Density1"/>. The properties are coupled: If 
    /// <see cref="Density"/> or <see cref="HeightFalloff"/> are changed, <see cref="Density0"/> and
    /// <see cref="Density1"/> are updated automatically and vice versa. In practice, you will use
    /// either <see cref="Density"/> and <see cref="HeightFalloff"/>, or <see cref="Density0"/> and 
    /// <see cref="Density1"/> to control the fog settings.
    /// </para>
    /// <para>
    /// To disable the fog, simply set <see cref="Density"/> to 0. To create a height-independent
    /// fog, set <see cref="HeightFalloff"/> to 0.
    /// </para>
    /// </remarks>
    public float Density
    {
      get { return _density; }
      set
      {
        if (_density == value)
          return;

        _density = value;
        UpdateDensities();
      }
    }
    private float _density;


    /// <summary>
    /// Gets or sets the height falloff.
    /// </summary>
    /// <value>The height falloff.</value>
    /// <remarks>
    /// <para>
    /// The fog density can be specified using either <see cref="Density"/> and 
    /// <see cref="HeightFalloff"/> or <see cref="Density0"/> and <see cref="Density1"/>. 
    /// <see cref="Density"/> defines the fog density at height 0 and the 
    /// <see cref="HeightFalloff"/>determines whether the fog density increases or decrease with
    /// height. Alternatively, the  fog density can be set at two reference heights using 
    /// <see cref="Density0"/> and <see cref="Density1"/>. The properties are coupled: If 
    /// <see cref="Density"/> or <see cref="HeightFalloff"/> are changed, <see cref="Density0"/> and
    /// <see cref="Density1"/> are updated automatically and vice versa. In practice, you will use
    /// either <see cref="Density"/> and <see cref="HeightFalloff"/>, or <see cref="Density0"/> and 
    /// <see cref="Density1"/> to control the fog settings.
    /// </para>
    /// <para>
    /// If this value is greater than 0, the fog density decreases with height. Higher 
    /// <see cref="HeightFalloff"/> values let the fog density decrease faster.
    /// </para>
    /// <para>
    /// If this value is 0, then the fog density is height-independent and increases only with
    /// the distance from the camera.
    /// </para>
    /// <para>
    /// If the value is less than 0, the fog density increases with height. This reverses the
    /// typical height fog effect: The fog gathers at the ceiling instead of the ground.
    /// </para>
    /// <para>
    /// To avoid numerical problems, the absolute value of <see cref="HeightFalloff"/> should be a 
    /// small value (e.g. between 0 and 10).
    /// </para>
    /// </remarks>
    public float HeightFalloff
    {
      get { return _heightFalloff; }
      set
      {
        if (_heightFalloff == value)
          return;

        _heightFalloff = value;
        UpdateDensities();
      }
    }
    private float _heightFalloff;


    /// <summary>
    /// Gets or sets the reference height for <see cref="Color0"/> and <see cref="Density0"/>.
    /// </summary>
    /// <value>The reference height for <see cref="Color0"/> and <see cref="Density0"/>.</value>
    public float Height0 { get; set; }


    /// <summary>
    /// Gets or sets the color of the fog at <see cref="Height0"/>.
    /// </summary>
    /// <value>The color of the fog at <see cref="Height0"/> (using premultiplied alpha).</value>
    public Vector4F Color0 { get; set; }


    /// <summary>
    /// Gets or sets the density of the fog at <see cref="Height0"/>.
    /// </summary>
    /// <value>The density of the fog at <see cref="Height0"/>.</value>
    /// <inheritdoc cref="Density"/>
    public float Density0
    {
      get { return _density0; }
      set
      {
        if (_density0 == value)
          return;

        _density0 = value;
        UpdateDensityAndHeightFalloff();
      }
    }
    private float _density0;


    /// <summary>
    /// Gets or sets the reference height for <see cref="Color1"/> and <see cref="Density1"/>.
    /// </summary>
    /// <value>The reference height for <see cref="Color1"/> and <see cref="Density1"/>.</value>
    public float Height1 { get; set; }


    /// <summary>
    /// Gets or sets the color of the fog at <see cref="Height1"/>.
    /// </summary>
    /// <value>The color of the fog at <see cref="Height1"/> (using premultiplied alpha).</value>
    public Vector4F Color1 { get; set; }


    /// <summary>
    /// Gets or sets the density of the fog at <see cref="Height1"/>.
    /// </summary>
    /// <value>The density of the fog at <see cref="Height1"/>.</value>
    /// <inheritdoc cref="Density"/>
    public float Density1
    {
      get { return _density1; }
      set
      {
        if (_density1 == value)
          return;

        _density1 = value;
        UpdateDensityAndHeightFalloff();
      }
    }
    private float _density1;


    /// <summary>
    /// Gets or sets the distance from the camera where the fog starts.
    /// </summary>
    /// <value>The distance from the camera where the fog starts.</value>
    /// <remarks>
    /// <see cref="Start"/> and <see cref="End"/> define a ramp over which the fog fades in.
    /// </remarks>
    public float Start { get; set; }


    /// <summary>
    /// Gets or sets the distance from the camera where the fog reaches its full intensity.
    /// </summary>
    /// <value>The distance from the camera where the fog reaches its full intensity.</value>
    /// <inheritdoc cref="Start"/>
    public float End { get; set; }


    /// <summary>
    /// Gets or sets the scattering symmetry constant.
    /// </summary>
    /// <value>
    /// The scattering symmetry constant for red, green and blue. Each component must be in the
    /// range ]-1, 1[. The default value is (0, 0, 0).
    /// </value>
    /// <remarks>
    /// If this value is (0, 0, 0), then the fog color is uniform. Any other values create a 
    /// non-uniform color where the color depends on the most important 
    /// <see cref="DirectionalLight"/> (usually the sunlight). Positive values create more forward
    /// scattering which make the fog brighter in the direction to the directional light. Different
    /// values for scattering of red, green and blue can be set. This can be used to create more
    /// forward scattering for red and green than for blue. This will give the fog color an
    /// orange/reddish appearance when looking into the sun and a blueish appearance opposite the
    /// sun.
    /// </remarks>
    public Vector3F ScatteringSymmetry { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Fog"/> class.
    /// </summary>
    public Fog()
    {
      Density = 1f;
      Height0 = 0;
      Color0 = new Vector4F(0.5f, 0.5f, 0.5f, 1);
      Height1 = 100;
      Color1 = new Vector4F(0.5f, 0.5f, 0.5f, 1);
      HeightFalloff = 0;
      Start = 0;
      End = 50;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="Fog"/> that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="Fog"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="Fog"/> derived class and <see cref="CloneCore"/> to create a copy of the 
    /// current instance. Classes that derive from <see cref="Fog"/> need to implement 
    /// <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </para>
    /// </remarks>
    public Fog Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Fog"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a private method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="Fog"/> method, which this 
    /// method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone <see cref="Fog"/>. A derived class does not implement 
    /// <see cref="CreateInstanceCore"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private Fog CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone Fog. The derived class {0} does not implement CreateInstanceCore().",
          GetType());
        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the <see cref="Fog"/>
    /// derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="Fog"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Fog"/> derived class must 
    /// implement this method. A typical implementation is to simply call the default constructor 
    /// and return the result. 
    /// </para>
    /// </remarks>
    protected virtual Fog CreateInstanceCore()
    {
      return new Fog();
    }


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="Fog"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="Fog"/> derived class must implement
    /// this method. A typical implementation is to call <c>base.CloneCore(this)</c> to copy all 
    /// properties of the base class and then copy all properties of the derived class.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(Fog source)
    {
      Name = source.Name;
      Density = source.Density;
      HeightFalloff = source.HeightFalloff;
      Height0 = source.Height0;
      Color0 = source.Color0;
      Height1 = source.Height1;
      Color1 = source.Color1;
      Start = source.Start;
      End = source.End;
      ScatteringSymmetry = source.ScatteringSymmetry;
      // Density0 and Density1 are computed from Density and HeightFalloff.
    }
    #endregion


    // Computes Density0/1 from Density and HeightFalloff.
    private void UpdateDensities()
    {
      _density0 = _density * (float)Math.Pow(2, -_heightFalloff * Height0);
      _density1 = _density * (float)Math.Pow(2, -_heightFalloff * Height1);
    }


    // Computes Density and HeightFalloff from Density0/1.
    private void UpdateDensityAndHeightFalloff()
    {
      _heightFalloff = ((float)Math.Log(_density1, 2) - (float)Math.Log(_density0, 2)) / (Height0 - Height1);
      if (!Numeric.IsFinite(_heightFalloff))
        _heightFalloff = 0;

      _density = _density0 * (float)Math.Pow(2, _heightFalloff * Height0);
    }


    /// <summary>
    /// Gets the fog intensity at the specified target position.
    /// </summary>
    /// <param name="fogNode">The fog node.</param>
    /// <param name="cameraNode">The camera node.</param>
    /// <param name="targetPosition">The target position.</param>
    /// <returns>The fog intensity (0 = no fog; 1 = full fog, nothing else visible).</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="fogNode"/> or <paramref name="cameraNode"/> is <see langword="null"/>.
    /// </exception>
    public float GetIntensity(FogNode fogNode, CameraNode cameraNode, Vector3F targetPosition)
    {
      if (fogNode == null)
        throw new ArgumentNullException("fogNode");
      if (cameraNode == null)
        throw new ArgumentNullException("cameraNode");

      return OnGetIntensity(fogNode, cameraNode, targetPosition);
    }


    /// <summary>
    /// Called to compute the fog intensity for <see cref="GetIntensity"/>.
    /// </summary>
    /// <param name="fogNode">The fog node. (Is never <see langword="null"/>.)</param>
    /// <param name="cameraNode">The camera node. (Is never <see langword="null"/>.)</param>
    /// <param name="targetPosition">The target position.</param>
    /// <returns>The fog intensity (0 = no fog; 1 = full fog, nothing else visible).</returns>
    /*protected virtual*/
    private float OnGetIntensity(FogNode fogNode, CameraNode cameraNode, Vector3F targetPosition)
    {
      // These computations are the same as in FogRenderer and the Fog shader files.

      if (Numeric.IsZero(Density))
        return 0;

      Vector3F cameraToPosition = targetPosition - cameraNode.PoseWorld.Position;
      float distance = cameraToPosition.Length; // The distance traveled inside the fog.
      Vector3F cameraToPositionDirection = cameraToPosition / distance;
      
      // Compute a value that is 0 at Start and 1 at End.
      float ramp = (distance - Start) / (End - Start);

      // Smoothstep distance fog
      float smoothRamp = InterpolationHelper.HermiteSmoothStep(ramp);

      // Exponential Fog
      float referenceHeight = cameraNode.PoseWorld.Position.Y - fogNode.PoseWorld.Position.Y 
                              + cameraToPositionDirection.Y * Start;
      float distanceInFog = distance - Start;
      var fogDirection = cameraToPositionDirection;
      if (HeightFalloff * fogDirection.Y < 0)
      {
        referenceHeight += fogDirection.Y * distanceInFog;
        fogDirection = -fogDirection;
      }

      float referenceDensity = Density * (float)Math.Pow(2, -HeightFalloff * referenceHeight);
      float opticalLength = GetOpticalLengthInHeightFog(distanceInFog, referenceDensity, fogDirection * distanceInFog, HeightFalloff);
      float heightFog = (1 - (float)Math.Pow(2, -opticalLength * 1));

      // Get alpha from BottomColor and TopColor.
      // (Note: We have to avoid division by zero.)
      float height = targetPosition.Y - fogNode.PoseWorld.Position.Y;
      float p = MathHelper.Clamp((height - Height0) / Math.Max(Height1 - Height0, 0.0001f), 0, 1);
      float alpha = InterpolationHelper.Lerp(Color0.W, Color1.W, p);

      return smoothRamp * heightFog * alpha;
    }


    private static float GetOpticalLengthInHeightFog(float dist, float cameraDensity, Vector3F cameraToPosition, float heightFalloff)
    {
      float opticalLength = dist * cameraDensity;
      const float SlopeThreshold = 0.00001f;
      if (Math.Abs(cameraToPosition.Y) > SlopeThreshold && !Numeric.IsZero(heightFalloff))
      {
        // This part is only computed if t cannot be 0 (division by zero).
        float t = heightFalloff * cameraToPosition.Y;
        opticalLength *= (1.0f - (float)Math.Pow(2, -t)) / t;
      }
      opticalLength = MathHelper.Clamp(opticalLength, 0, 1e16f);
      return opticalLength;
    }
    #endregion
  }
}
