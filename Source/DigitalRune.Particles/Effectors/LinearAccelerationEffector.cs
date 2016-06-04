// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Applies an acceleration to a particle parameter.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This effector changes a linear velocity by applying an acceleration.
  /// </para>
  /// <para>
  /// Used particle parameters:
  /// <list type="table">
  /// <listheader><term>Particle Parameter</term><description>Description</description></listheader>
  /// <item>
  /// <term><see cref="DirectionParameter"/></term>
  /// <description>
  /// A normalized <see cref="Vector3F"/> parameter that defines the movement direction (direction 
  /// of the linear velocity vector). This parameter is modified by applying the acceleration.
  /// Per default, the parameter "Direction" is used.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="SpeedParameter"/></term>
  /// <description>
  /// A <see cref="float"/> parameter that defines the movement speed (magnitude of the linear 
  /// velocity vector). This parameter is modified by applying the acceleration. 
  /// Per default, the parameter "LinearSpeed" is used.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="AccelerationParameter"/></term>
  /// <description>
  /// A <see cref="Vector3F"/> parameter that defines the acceleration vector. The direction of the 
  /// vector defines the acceleration direction. The length of the vector defines the magnitude of 
  /// the acceleration. Per default, the parameter "LinearAcceleration" is used.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  public class LinearAccelerationEffector : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<Vector3F> _directionParameter;
    private IParticleParameter<float> _linearSpeedParameter;
    private IParticleParameter<Vector3F> _linearAccelerationParameter;
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
    /// Gets or sets the name of the parameter that defines the acceleration vector.
    /// (A varying or uniform parameter of type <see cref="Vector3F"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that defines the acceleration vector.
    /// (Parameter type: varying or uniform, value type: <see cref="Vector3F"/>) <br/>
    /// The default value is "LinearAcceleration".
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.In)]
    public string AccelerationParameter { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearAccelerationEffector"/> class.
    /// </summary>
    public LinearAccelerationEffector()
    {
      DirectionParameter = ParticleParameterNames.Direction;
      SpeedParameter = ParticleParameterNames.LinearSpeed;
      AccelerationParameter = ParticleParameterNames.LinearAcceleration;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new LinearAccelerationEffector();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone LinearAccelerationEffector properties.
      var sourceTyped = (LinearAccelerationEffector)source;
      DirectionParameter = sourceTyped.DirectionParameter;
      SpeedParameter = sourceTyped.SpeedParameter;
      AccelerationParameter = sourceTyped.AccelerationParameter;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _directionParameter = ParticleSystem.Parameters.Get<Vector3F>(DirectionParameter);
      _linearSpeedParameter = ParticleSystem.Parameters.Get<float>(SpeedParameter);
      _linearAccelerationParameter = ParticleSystem.Parameters.Get<Vector3F>(AccelerationParameter);
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _directionParameter = null;
      _linearSpeedParameter = null;
      _linearAccelerationParameter = null;
    }


    /// <inheritdoc/>
    protected override void OnBeginUpdate(TimeSpan deltaTime)
    {
      if (_linearSpeedParameter == null
          || _directionParameter == null
          || _linearAccelerationParameter == null)
      {
        return;
      }

      // Update uniform parameters.
      if (_linearSpeedParameter.Values == null || _directionParameter.Values == null)
      {
        float dt = (float)deltaTime.TotalSeconds;
        Vector3F velocity = _directionParameter.DefaultValue * _linearSpeedParameter.DefaultValue;
        velocity += _linearAccelerationParameter.DefaultValue * dt;

        // Set new speed.
        var newSpeed = velocity.Length;
        _linearSpeedParameter.DefaultValue = newSpeed;

        // Set new direction.
        if (!Numeric.IsZero(newSpeed))
          _directionParameter.DefaultValue = velocity / newSpeed;
      }
    }


    /// <inheritdoc/>
    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (_linearSpeedParameter == null
          || _directionParameter == null
          || _linearAccelerationParameter == null)
      {
        return;
      }

      // Update varying parameters.
      Vector3F[] directions = _directionParameter.Values;
      float[] speeds = _linearSpeedParameter.Values;
      Vector3F[] accelerations = _linearAccelerationParameter.Values;
      float dt = (float)deltaTime.TotalSeconds;

      if (directions != null && speeds != null && accelerations != null)
      {
        // Optimized case: Direction, Speed, and Acceleration are varying parameters.
        for (int i = startIndex; i < startIndex + count; i++)
        {
          Vector3F velocity = directions[i] * speeds[i];
          velocity += accelerations[i] * dt;
          speeds[i] = velocity.Length;
          if (!Numeric.IsZero(speeds[i]))
            directions[i] = velocity / speeds[i];
        }
      }
      else if (directions != null && speeds != null)
      {
        // Optimized case: Direction and Speed are varying parameters, Acceleration is uniform.
        Vector3F acceleration = _linearAccelerationParameter.DefaultValue;
        for (int i = startIndex; i < startIndex + count; i++)
        {
          Vector3F velocity = directions[i] * speeds[i];
          velocity += acceleration * dt;
          speeds[i] = velocity.Length;
          if (!Numeric.IsZero(speeds[i]))
            directions[i] = velocity / speeds[i];
        }
      }      
      else if (directions != null || speeds != null)
      {
        // General case: Either Direction or Speed is varying parameter.
        // This path does not make sense much sense. - But maybe someone has an idea how to use it.
        Vector3F acceleration = _linearAccelerationParameter.DefaultValue;
        for (int i = startIndex; i < startIndex + count; i++)
        {
          Vector3F velocity = _directionParameter.GetValue(i) * _linearSpeedParameter.GetValue(i);
          velocity += acceleration * dt;
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
