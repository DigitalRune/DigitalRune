// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Converts the particle system's <see cref="ParticleSystem.Time"/> into a <see cref="float"/> 
  /// value and stores it in a particle parameter.
  /// </summary>
  public class TimeToSingleEffector : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<float> _parameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the parameter that should store the time value.
    /// (A varying or uniform parameter of type <see cref="float"/>.)
    /// </summary>
    /// <value>
    /// The name of the parameter that should store the time value.
    /// (Parameter type: varying or uniform, value type: <see cref="float"/>) <br/>
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.Out)]
    public string Parameter { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new TimeToSingleEffector();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone TimeToSingleEffector properties.
      var sourceTyped = (TimeToSingleEffector)source;
      Parameter = sourceTyped.Parameter;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _parameter = ParticleSystem.Parameters.Get<float>(Parameter);
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _parameter = null;
    }


    /// <inheritdoc/>
    protected override void OnBeginUpdate(TimeSpan deltaTime)
    {
      if (_parameter == null)
        return;

      float[] values = _parameter.Values;
      if (values == null)
      {
        // Value is a uniform parameter.
        _parameter.DefaultValue = (float)ParticleSystem.Time.TotalSeconds;
      }
    }


    /// <inheritdoc/>
    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (_parameter == null)
        return;

      float[] values = _parameter.Values;
      if (values == null)
      {
        // Value is a uniform parameter. Uniform parameters are handled in OnBeginUpdate().
        return;
      }

      // Value is a varying parameter.
      var time = (float)ParticleSystem.Time.TotalSeconds;
      for (int i = startIndex; i < startIndex + count; i++)
        values[i] = time;
    }
    #endregion
  }
}
