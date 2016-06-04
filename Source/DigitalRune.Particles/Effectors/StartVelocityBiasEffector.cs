// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;

#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Adds a bias velocity to the start velocities of particles.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This effector changes the initial particle speed and direction by adding a bias velocity. 
  /// </para>
  /// <para>
  /// For example: When a particle emitter is moving, the emitter's velocity can be set as the bias 
  /// velocity to influence the particles when they are spawned. The <see cref="Strength"/> property
  /// is a factor, usually in the range [0, 1], that defines how strong the influence of the emitter
  /// velocity is.
  /// </para>
  /// <para>
  /// Used particle parameters:
  /// <list type="table">
  /// <listheader><term>Particle Parameter</term><description>Description</description></listheader>
  /// <item>
  /// <term><see cref="DirectionParameter"/></term>
  /// <description>
  /// A normalized <see cref="Vector3F"/> parameter that defines the movement direction (direction 
  /// of the linear velocity vector). This parameter is modified by applying the bias velocity.
  /// Per default, the parameter "Direction" is used.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="SpeedParameter"/></term>
  /// <description>
  /// A <see cref="float"/> parameter that defines the movement speed (magnitude of the linear 
  /// velocity vector). This parameter is modified by applying the acceleration. Per default, the 
  /// parameter "LinearSpeed" is used.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="BiasVelocityParameter"/></term>
  /// <description>
  /// A <see cref="Vector3F"/> parameter that defines the bias velocity. 
  /// Per default, the parameter "EmitterVelocity" is used.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  public class StartVelocityBiasEffector : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<Vector3F> _directionParameter;
    private IParticleParameter<float> _linearSpeedParameter;
    private IParticleParameter<Vector3F> _biasVelocityParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the parameter that defines the movement direction.
    /// (A varying or uniform parameter of type <see cref="Vector3F"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that defines the movement direction.
    /// (Parameter type: varying or uniform, value type: <see cref="Vector3F"/>) <br/>
    /// The default value is "Direction".
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.InOut)]
    public string DirectionParameter { get; set; }


    /// <summary>
    /// Gets or sets the name of the parameter that defines the movement speed.
    /// (A varying or uniform parameter of type <see cref="float"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that defines the movement speed.
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>) <br/>
    /// The default value is "LinearSpeed".
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.InOut)]
    public string SpeedParameter { get; set; }


    /// <summary>
    /// Gets or sets the name of the parameter that defines the bias velocity vector.
    /// (A varying or uniform parameter of type <see cref="Vector3F"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that defines the bias velocity vector.
    /// (Parameter type: varying or uniform, value type: <see cref="Vector3F"/>) <br/>
    /// The default value is "EmitterVelocity".
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.In)]
    public string BiasVelocityParameter { get; set; }


    /// <summary>
    /// Gets or sets a factor that is used to scale the bias velocity before adding it to the 
    /// particle velocity.
    /// </summary>
    /// <value>
    /// The strength of the bias, usually in the range [0, 1]. The default value is 1.
    /// </value>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public float Strength { get; set; }


    // TODO: Add Emitter parameter if only particles of a certain emitter should be initialized?
    //public IParticleEmitter Emitter { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="StartVelocityBiasEffector"/> class.
    /// </summary>
    public StartVelocityBiasEffector()
    {
      DirectionParameter = ParticleParameterNames.Direction;
      SpeedParameter = ParticleParameterNames.LinearSpeed;
      BiasVelocityParameter = ParticleParameterNames.EmitterVelocity;
      Strength = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new StartVelocityBiasEffector();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone StartVelocityBiasEffector properties.
      var sourceTyped = (StartVelocityBiasEffector)source;
      DirectionParameter = sourceTyped.DirectionParameter;
      SpeedParameter = sourceTyped.SpeedParameter;
      BiasVelocityParameter = sourceTyped.BiasVelocityParameter;
      Strength = sourceTyped.Strength;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _directionParameter = ParticleSystem.Parameters.Get<Vector3F>(DirectionParameter);
      _linearSpeedParameter = ParticleSystem.Parameters.Get<float>(SpeedParameter);
      _biasVelocityParameter = ParticleSystem.Parameters.Get<Vector3F>(BiasVelocityParameter);
    }


    /// <inheritdoc/>
    protected override void OnInitialize()
    {
      if (_linearSpeedParameter == null
          || _directionParameter == null
          || _biasVelocityParameter == null)
      {
        return;
      }

      if (_directionParameter.Values != null && _linearSpeedParameter.Values != null)
      {
        // Varying parameters are handled in OnInitializeParticles().
        return;
      }

      // Initialize uniform parameters.
      Vector3F velocity = _directionParameter.DefaultValue * _linearSpeedParameter.DefaultValue;
      velocity += _biasVelocityParameter.DefaultValue * Strength;
      var newSpeed = velocity.Length;
      _linearSpeedParameter.DefaultValue = newSpeed;
      if (!Numeric.IsZero(newSpeed))
        _directionParameter.DefaultValue = velocity / newSpeed;
    }

    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _directionParameter = null;
      _linearSpeedParameter = null;
      _biasVelocityParameter = null;
    }


    /// <inheritdoc/>
    protected override void OnInitializeParticles(int startIndex, int count, object emitter)
    {
      if (_linearSpeedParameter == null
          || _directionParameter == null
          || _biasVelocityParameter == null)
      {
        return;
      }

      Vector3F[] directions = _directionParameter.Values;
      float[] speeds = _linearSpeedParameter.Values;
      Vector3F[] biases = _biasVelocityParameter.Values;

      if (directions != null && speeds != null && biases != null)
      {
        // Optimized case: Direction, Speed, and Acceleration are varying parameters.
        for (int i = startIndex; i < startIndex + count; i++)
        {
          Vector3F velocity = directions[i] * speeds[i];
          velocity += biases[i] * Strength;
          speeds[i] = velocity.Length;
          if (!Numeric.IsZero(speeds[i]))
            directions[i] = velocity / speeds[i];
        }
      }
      else if (directions != null && speeds != null)
      {
        // Optimized case: Direction and Speed are varying parameters, Bias is uniform.
        Vector3F bias = _biasVelocityParameter.DefaultValue * Strength;
        for (int i = startIndex; i < startIndex + count; i++)
        {
          Vector3F velocity = directions[i] * speeds[i];
          velocity += bias;
          speeds[i] = velocity.Length;
          if (!Numeric.IsZero(speeds[i]))
            directions[i] = velocity / speeds[i];
        }
      }
      else if (directions != null || speeds != null)
      {
        // General case: Either Direction or Speed is varying parameter.
        // This path does not make sense much sense. - But maybe someone has an idea how to use it.
        Vector3F bias = _biasVelocityParameter.DefaultValue * Strength;
        for (int i = startIndex; i < startIndex + count; i++)
        {
          Vector3F velocity = _directionParameter.GetValue(i) * _linearSpeedParameter.GetValue(i);
          velocity += bias;
          var newSpeed = velocity.Length;
          _linearSpeedParameter.SetValue(i, newSpeed);
          if (!Numeric.IsZero(newSpeed))
            _directionParameter.SetValue(i, velocity / newSpeed);
        }
      }
    }
    #endregion
  }
}
