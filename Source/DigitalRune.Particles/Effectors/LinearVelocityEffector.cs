// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Applies a linear velocity to a 3D position parameter.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This effector changes a position parameter by applying a linear velocity.
  /// </para>
  /// <para>
  /// Used particle parameters:
  /// <list type="table">
  /// <listheader><term>Particle Parameter</term><description>Description</description></listheader>
  /// <item>
  /// <term><see cref="PositionParameter"/></term>
  /// <description>
  /// A <see cref="Vector3F"/> parameter that defines the position. This parameter is modified by 
  /// the effector. Per default, the parameter "Position" is used.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="DirectionParameter"/></term>
  /// <description>
  /// A normalized <see cref="Vector3F"/> parameter that defines the movement direction (direction 
  /// of the linear velocity vector). Per default, the parameter "Direction" is used.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="SpeedParameter"/></term>
  /// <description>
  /// A <see cref="float"/> parameter that defines the movement speed (magnitude of the linear 
  /// velocity vector). Per default, the parameter "LinearSpeed" is used.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  public class LinearVelocityEffector : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<Vector3F> _positionParameter;
    private IParticleParameter<Vector3F> _directionParameter;
    private IParticleParameter<float> _linearSpeedParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the parameter that defines the position.
    /// (A varying or uniform parameter of type <see cref="Vector3F"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that defines the position.
    /// (Parameter type: varying or uniform, value type: <see cref="Vector3F"/>) <br/>
    /// The default value is "Position".
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.InOut)]
    public string PositionParameter { get; set; }


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
    [ParticleParameter(ParticleParameterUsage.In)]
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
    [ParticleParameter(ParticleParameterUsage.In)]
    public string SpeedParameter { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="LinearVelocityEffector"/> class.
    /// </summary>
    public LinearVelocityEffector()
    {
      PositionParameter = "Position";
      DirectionParameter = "Direction";
      SpeedParameter = "LinearSpeed";
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new LinearVelocityEffector();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone LinearVelocityEffector properties.
      var sourceTyped = (LinearVelocityEffector)source;
      PositionParameter = sourceTyped.PositionParameter;
      DirectionParameter = sourceTyped.DirectionParameter;
      SpeedParameter = sourceTyped.SpeedParameter;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _positionParameter = ParticleSystem.Parameters.Get<Vector3F>(PositionParameter);
      _directionParameter = ParticleSystem.Parameters.Get<Vector3F>(DirectionParameter);
      _linearSpeedParameter = ParticleSystem.Parameters.Get<float>(SpeedParameter);
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _linearSpeedParameter = null;
      _positionParameter = null;
      _directionParameter = null;
    }


    /// <inheritdoc/>
    protected override void OnBeginUpdate(TimeSpan deltaTime)
    {
      if (_linearSpeedParameter == null
          || _positionParameter == null
          || _directionParameter == null)
      {
        return;
      }

      Vector3F[] positions = _positionParameter.Values;
      if (positions == null)
      {
        // Position is a uniform parameter.
        float dt = (float)deltaTime.TotalSeconds;
        _positionParameter.DefaultValue += _directionParameter.DefaultValue * _linearSpeedParameter.DefaultValue * dt;
      }
    }


    /// <inheritdoc/>
    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (_linearSpeedParameter == null
          || _positionParameter == null
          || _directionParameter == null)
      {
        return;
      }

      var positions = _positionParameter.Values;
      if (positions == null)
      {
        // Position is a uniform parameter. Uniform parameters are handled in OnBeginUpdate().
        return;
      }

      // Value is a varying parameter.
      Vector3F[] directions = _directionParameter.Values;
      float[] speeds = _linearSpeedParameter.Values;
      float dt = (float)deltaTime.TotalSeconds;

      if (directions != null && speeds != null)
      {
        // Optimized case: Direction and Speed are varying parameters.
        for (int i = startIndex; i < startIndex + count; i++)
          positions[i] += directions[i] * speeds[i] * dt;
      }
      else if (directions != null)
      {
        // Optimized case: Direction is varying parameter and Speed is uniform parameter.
        Debug.Assert(speeds == null);
        float speed = _linearSpeedParameter.DefaultValue;
        for (int i = startIndex; i < startIndex + count; i++)
          positions[i] += directions[i] * speed * dt;
      }
      else
      {
        // General case:
        for (int i = startIndex; i < startIndex + count; i++)
          positions[i] += _directionParameter.GetValue(i) * _linearSpeedParameter.GetValue(i) * dt;
      }
    }
    #endregion
  }
}
