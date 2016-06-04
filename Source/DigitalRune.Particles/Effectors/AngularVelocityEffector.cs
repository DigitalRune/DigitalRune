// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Applies an angular velocity to an angle parameter.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This effector changes an angle parameter by applying an angular velocity.
  /// </para>
  /// <para>
  /// Used particle parameters:
  /// <list type="table">
  /// <listheader><term>Particle Parameter</term><description>Description</description></listheader>
  /// <item>
  /// <term><see cref="AngleParameter"/></term>
  /// <description>
  /// A <see cref="float"/> parameter that defines the angle in radians. This parameter is modified 
  /// by this effector. The angle is always kept in the interval [0, 2π[. Per default, the 
  /// parameter "Angle" is used.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="SpeedParameter"/></term>
  /// <description>
  /// A <see cref="float"/> parameter that defines the angular speed in radians per second. Per 
  /// default, the parameter "AngularSpeed" is used.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  public class AngularVelocityEffector : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<float> _angleParameter;
    private IParticleParameter<float> _angularSpeedParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the parameter that defines the angle.
    /// (A varying or uniform parameter of type <see cref="float"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that defines the angle.
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>) <br/>
    /// The default value is "Angle".
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.InOut)]
    public string AngleParameter { get; set; }


    /// <summary>
    /// Gets or sets the name of the parameter that defines the rotation speed.
    /// (A varying or uniform parameter of type <see cref="float"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that defines the rotation speed.
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>) <br/>
    /// The default value is "AngularSpeed".
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
    /// Initializes a new instance of the <see cref="AngularVelocityEffector"/> class.
    /// </summary>
    public AngularVelocityEffector()
    {
      AngleParameter = ParticleParameterNames.Angle;
      SpeedParameter = ParticleParameterNames.AngularSpeed;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new AngularVelocityEffector();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone AngularVelocityEffector properties.
      var sourceTyped = (AngularVelocityEffector)source;
      AngleParameter = sourceTyped.AngleParameter;
      SpeedParameter = sourceTyped.SpeedParameter;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _angleParameter = ParticleSystem.Parameters.Get<float>(AngleParameter);
      _angularSpeedParameter = ParticleSystem.Parameters.Get<float>(SpeedParameter);
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _angleParameter = null;
      _angularSpeedParameter = null;
    }


    /// <inheritdoc/>
    protected override void OnBeginUpdate(TimeSpan deltaTime)
    {
      if (_angleParameter == null || _angularSpeedParameter == null)
        return;

      var angles = _angleParameter.Values;
      if (angles == null)
      {
        // Angle is a uniform parameter.
        float dt = (float)deltaTime.TotalSeconds;
        _angleParameter.DefaultValue = Rotate(-1, dt, _angleParameter.DefaultValue);
      }
    }


    /// <inheritdoc/>
    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (_angleParameter == null || _angularSpeedParameter == null)
        return;

      var angles = _angleParameter.Values;
      if (angles == null)
      {
        // Angle is a uniform parameter. Uniform parameters are handled in OnBeginUpdate().
        return;
      }

      // Angle is a varying parameter.
      float[] speeds = _angularSpeedParameter.Values;
      float dt = (float)deltaTime.TotalSeconds;

      if (speeds != null)
      {
        // Optimized case: Speed is varying parameter.
        for (int i = startIndex; i < startIndex + count; i++)
          angles[i] = Rotate(dt, angles[i], speeds[i]);
      }
      else
      {
        // Optimized case: Speed is uniform parameter.
        float speed = _angularSpeedParameter.DefaultValue;
        for (int i = startIndex; i < startIndex + count; i++)
          angles[i] = Rotate(dt, angles[i], speed);
      }
    }


    private float Rotate(int index, float deltaTime, float angle)
    {
      float speed = _angularSpeedParameter.GetValue(index);
      return Rotate(deltaTime, angle, speed);
    }


    private static float Rotate(float deltaTime, float angle, float speed)
    {
      angle += speed * deltaTime;

      // Limit angle to [0, 2π[.
      if (angle >= ConstantsF.TwoPi)
        angle = angle % ConstantsF.TwoPi;
      else if (angle < 0)
        angle = angle % ConstantsF.TwoPi + ConstantsF.TwoPi;

      return angle;
    }
    #endregion
  }
}
