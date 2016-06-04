using DigitalRune.Game;
using DigitalRune.Graphics.Effects;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using System;
using System.Linq;


namespace Samples.Graphics
{
  /// <summary>
  /// Controls the wind (direction and speed) in the scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This game object defines the wind velocity (property <see cref="Wind"/>). The wind can be
  /// controlled using the properties <see cref="MaxSpeed"/>, <see cref="DirectionVariation"/>
  /// and <see cref="SpeedVariation"/>.
  /// </para>
  /// <para>
  /// <strong>Important:</strong>
  /// Only one <see cref="WindObject"/> must be used at any time in the game! The wind object must
  /// be added to the game object service before any meshes are loaded which use an effect with a
  /// "Wind" parameter!
  /// </para>
  /// <para>
  /// This game object creates an effect parameter binding for the parameter "Wind". If an effect
  /// has an effect parameter with the name or semantic "Wind", the graphics service will
  /// automatically update the effect parameter value.
  /// </para>
  /// <para>
  /// The wind simulation is very simple: A random wind change vector is chosen at regular
  /// intervals. The wind smoothly changes over the interval.
  /// </para>
  /// </remarks>
  public sealed class WindObject : GameObject
  {
    private readonly IServiceLocator _services;

    private float _lastAngle;
    private float _lastSpeed;
    private float _nextAngle;
    private float _nextSpeed;
    private float _timeSinceWindChange = float.MaxValue;


    /// <summary>
    /// Gets or sets the max wind speed.
    /// </summary>
    /// <value>The max speed wind speed. The default value is 10 ("fresh breeze").</value>
    /// <remarks>
    /// <para>
    /// Common wind speeds are between 0 (no wind) and 30 (violent storm). Hurricanes have wind
    /// speeds above 30.
    /// </para>
    /// <para>
    /// For more information on wind speeds, have a look at the Beaufort scale: 
    /// http://en.wikipedia.org/wiki/Beaufort_scale
    /// </para>
    /// </remarks>
    public float MaxSpeed { get; set; }


    /// <summary>
    /// Gets or sets the allowed direction variation.
    /// </summary>
    /// <value>The allowed direction variation in the range [0, 1].</value>
    /// <remarks>
    /// If this value is 0, then the wind will always blow in the exact same direction. If this
    /// value is greater than 0, the wind can blow in several directions.
    /// </remarks>
    public float DirectionVariation { get; set; }


    /// <summary>
    /// Gets or sets the allowed speed variation.
    /// </summary>
    /// <value>The allowed speed variation in the range [0,1].</value>
    /// <remarks>
    /// If this value is 0, then the wind will always have the <see cref="MaxSpeed"/>. If this
    /// value is greater than 0, the wind speed can deviate from <see cref="MaxSpeed"/>. If this
    /// value is 1, the wind speed can even become 0 (no wind).
    /// </remarks>
    public float SpeedVariation { get; set; }


    /// <summary>
    /// Gets the simulated wind velocity (direction and speed).
    /// </summary>
    /// <value>The wind velocity.</value>
    public Vector3F Wind { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="WindObject"/> class.
    /// </summary>
    /// <param name="services">The service provider.</param>
    public WindObject(IServiceLocator services)
    {
      _services = services;
      MaxSpeed = 10;
    }


    protected override void OnLoad()
    {
      // ----- Register info for "Wind" effect parameter.
      var graphicsService = _services.GetInstance<IGraphicsService>();

      // Tell the graphics service some information about effect parameters with
      // the name or semantic "Wind". The hint "Global" tells the engine that this
      // parameter is the same for all scene nodes.
      var defaultEffectInterpreter = graphicsService.EffectInterpreters.OfType<DefaultEffectInterpreter>().First();
      defaultEffectInterpreter.ParameterDescriptions.Add(
        "Wind",
        (parameter, i) => new EffectParameterDescription(parameter, "Wind", i, EffectParameterHint.Global));

      // Tell the engine how to create an effect parameter binding for "Wind" which
      // automatically sets the effect parameter value.
      var defaultEffectBinder = graphicsService.EffectBinders.OfType<DefaultEffectBinder>().First();
      defaultEffectBinder.Vector3Bindings.Add(
        "Wind",
        (effect, parameter, data) => new DelegateParameterBinding<Vector3>(effect, parameter,
          (binding, context) => (Vector3)Wind));  // The delegate returns the Wind property.


      // ----- Add GUI controls to the Options window.
      var sampleFramework = _services.GetInstance<SampleFramework>();
      var optionsPanel = sampleFramework.AddOptions("Game Objects");
      var panel = SampleHelper.AddGroupBox(optionsPanel, "Wind");

      SampleHelper.AddSlider(
        panel,
        "Max speed",
        "F2",
        0,
        30,
        MaxSpeed,
        value => MaxSpeed = value,
        "0 = no wind, 30 = violent storm");

      SampleHelper.AddSlider(
        panel,
        "Speed variation",
        "F2",
        0,
        1,
        SpeedVariation,
        value => SpeedVariation = value,
        "0 wind blows only with max speed; > 0 wind vary the speed.");

      SampleHelper.AddSlider(
        panel,
        "Direction variation",
        "F2",
        0,
        1,
        DirectionVariation,
        value => DirectionVariation = value,
        "0 wind blows only in one direction; > 0 wind can change direction");
    }


    protected override void OnUnload()
    {
      // ----- Unregister effect parameter info for "Wind".
      var graphicsService = _services.GetInstance<IGraphicsService>();
      var defaultEffectInterpreter = graphicsService.EffectInterpreters.OfType<DefaultEffectInterpreter>().First();
      defaultEffectInterpreter.ParameterDescriptions.Remove("Wind");
      var defaultEffectBinder = graphicsService.EffectBinders.OfType<DefaultEffectBinder>().First();
      defaultEffectBinder.Vector3Bindings.Remove("Wind");
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // A random new wind change is chosen at regular intervals.
      const float WindInterval = 3;
      _timeSinceWindChange += (float)deltaTime.TotalSeconds;
      if (_timeSinceWindChange > WindInterval)
      {
        _lastAngle = _nextAngle;
        _lastSpeed = _nextSpeed;
        _timeSinceWindChange = 0;

        // Get random target angle.
        float a = RandomHelper.Random.NextFloat(-1, 1);
        // Apply non-linear curve to make smaller changes more likely.
        a = (float)Math.Pow(a, 3);
        // Convert to angle and limit variation.
        _nextAngle = _lastAngle + ConstantsF.PiOver2 * a * DirectionVariation;

        // Get random target speed.
        float s = RandomHelper.Random.NextFloat(0, 1);
        _nextSpeed = MaxSpeed * (1 - s * SpeedVariation);
      }

      // Update current wind.
      float p = _timeSinceWindChange / WindInterval;
      float speed = InterpolationHelper.Lerp(_lastSpeed, _nextSpeed, p);
      float angle = InterpolationHelper.Lerp(_lastAngle, _nextAngle, p);
      Wind = speed * Matrix33F.CreateRotationY(angle) * Vector3F.UnitX;
    }
  }
}
