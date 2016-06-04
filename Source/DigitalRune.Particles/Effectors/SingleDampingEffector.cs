// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Applies a damping to a particle parameter of type <see cref="float"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// All parameters must be of type <see cref="float"/>.
  /// </para>
  /// <para>
  /// This effector reduces a parameter value over time by applying a damping.
  /// </para>
  /// <para>
  /// Used particle parameters:
  /// <list type="table">
  /// <listheader><term>Particle Parameter</term><description>Description</description></listheader>
  /// <item>
  /// <term><see cref="ValueParameter"/></term>
  /// <description>
  /// A <see cref="float"/> parameter that is dampened. Per default, the parameter "LinearSpeed" is 
  /// used.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="DampingParameter"/></term>
  /// <description>
  /// A <see cref="float"/> parameter that defines the damping factor. A value of 0 means no 
  /// damping. Values greater than 0 mean higher damping. Per default, the parameter "Damping" is 
  /// used.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  public class SingleDampingEffector : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<float> _valueParameter;
    private IParticleParameter<float> _dampingParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the particle parameter that is damped.
    /// (A varying or uniform parameter of type <see cref="float"/>.)
    /// </summary>
    /// <value>
    /// The name of the particle parameter that is damped.
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>) <br/>
    /// The default value is "LinearSpeed".
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.InOut)]
    public string ValueParameter { get; set; }


    /// <summary>
    /// Gets or sets the name of the particle parameter that defines the strength of the damping.
    /// (A varying or uniform parameter of type <see cref="float"/>.)
    /// </summary>
    /// <value>
    /// The name of the particle parameter that defines the strength of the damping.
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>) <br/>
    /// The default value is "Damping".
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.In)]
    public string DampingParameter { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleDampingEffector"/> class.
    /// </summary>
    public SingleDampingEffector()
    {
      ValueParameter = ParticleParameterNames.LinearSpeed;
      DampingParameter = ParticleParameterNames.Damping;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new SingleDampingEffector();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone SingleDampingEffector properties.
      var sourceTyped = (SingleDampingEffector)source;
      ValueParameter = sourceTyped.ValueParameter;
      DampingParameter = sourceTyped.DampingParameter;
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _valueParameter = null;
      _dampingParameter = null;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _valueParameter = ParticleSystem.Parameters.Get<float>(ValueParameter);
      _dampingParameter = ParticleSystem.Parameters.Get<float>(DampingParameter);
    }


    /// <inheritdoc/>
    protected override void OnBeginUpdate(TimeSpan deltaTime)
    {
      if (_valueParameter != null && _valueParameter.Values == null && _dampingParameter != null)
      {
        // Value is a uniform parameter.
        float dt = (float)deltaTime.TotalSeconds;
        _valueParameter.DefaultValue *= (1 - _dampingParameter.DefaultValue * dt);
      }
    }


    /// <inheritdoc/>
    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (_valueParameter == null || _dampingParameter == null)
        return;

      float[] values = _valueParameter.Values;
      float[] dampings = _dampingParameter.Values;

      if (values == null)
      {
        // Value is a uniform parameter. Uniform parameters are handled in OnBeginUpdate().
        return;
      }

      float dt = (float)deltaTime.TotalSeconds;
      if (dampings == null)
      {
        // Optimized case: Damping is uniform parameter.
        float damping = _dampingParameter.DefaultValue;
        for (int i = startIndex; i < startIndex + count; i++)
          values[i] *= (1 - damping * dt);
      }
      else
      {
        // Optimized case: Damping is varying parameter.
        for (int i = startIndex; i < startIndex + count; i++)
          values[i] *= (1 - dampings[i] * dt);
      }
    }
    #endregion
  }
}
