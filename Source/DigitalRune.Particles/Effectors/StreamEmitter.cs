// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;

#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Creates particles at a configurable emission rate.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This effectors reads a uniform particle parameter (see <see cref="EmissionRateParameter"/>) to
  /// define the emission rate. If no emission rate parameter is found, the 
  /// <see cref="DefaultEmissionRate"/> is used.
  /// </para>
  /// <para>
  /// The emitter emits a stream of particles until <see cref="EmissionLimit"/> number of particles 
  /// have been created. Then it will stop to emit particles. The internal particle counter is reset 
  /// when <see cref="ParticleEffector.Initialize"/> is called (which is automatically called when a
  /// particle system is <see cref="ParticleSystem.Reset"/>).
  /// </para>
  /// <para>
  /// This emitter can be configured to create particles in bursts: Simply set a high enough 
  /// emission rate and limit the number of particles with <see cref="EmissionLimit"/>. To trigger 
  /// another burst, simply increase <see cref="EmissionLimit"/> by the burst size.
  /// </para>
  /// </remarks>
  public class StreamEmitter : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    
    private IParticleParameter<float> _emissionRateParameter;
    private float _leftoverParticles;
    private int _emittedParticles;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the particle parameter that defines the emission rate (particles 
    /// per second). (A uniform parameter of type <see cref="float"/>.)
    /// </summary>
    /// <value>
    /// The name of the particle parameter that defines emission rate. 
    /// (Parameter type: uniform, value type: <see cref="float"/>) <br/>
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer (Optional = true)]
#endif
    [ParticleParameter(ParticleParameterUsage.In, Optional = true)]
    public string EmissionRateParameter { get; set; }


    /// <summary>
    /// Gets or sets the default emission rate that is used if the 
    /// <see cref="EmissionRateParameter"/> is not found.
    /// </summary>
    /// <value>
    /// The default emission rate (particles per second). The default value is 10.
    /// </value>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public float DefaultEmissionRate { get; set; }


    /// <summary>
    /// Gets or sets the maximum number of emitted particles.
    /// </summary>
    /// <value>
    /// The maximum number of emitted particles. If this property is -1, the emission is unlimited.
    /// The default value is -1.
    /// </value>
    public int EmissionLimit { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamEmitter"/> class.
    /// </summary>
    public StreamEmitter()
    {
      DefaultEmissionRate = 10;
      EmissionLimit = -1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new StreamEmitter();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone StreamEmitter properties.
      var sourceTyped = (StreamEmitter)source;
      EmissionRateParameter = sourceTyped.EmissionRateParameter;
      DefaultEmissionRate = sourceTyped.DefaultEmissionRate;
      EmissionLimit = sourceTyped.EmissionLimit;
    }

    
    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _emissionRateParameter = ParticleSystem.Parameters.Get<float>(EmissionRateParameter);
    }


    /// <inheritdoc/>
    protected override void OnInitialize()
    {
      _leftoverParticles = 0;
      _emittedParticles = 0;
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _emissionRateParameter = null;
    }


    /// <inheritdoc/>
    protected override void OnBeginUpdate(TimeSpan deltaTime)
    {
      float dt = (float)deltaTime.TotalSeconds;
      float emissionRate = (_emissionRateParameter != null) ? _emissionRateParameter.DefaultValue : DefaultEmissionRate;
      float numberOfParticles = emissionRate * dt + _leftoverParticles;

      if (EmissionLimit >= 0)
      {
        float totalCount = _emittedParticles + (int)numberOfParticles;
        if (totalCount > EmissionLimit)
          numberOfParticles = EmissionLimit - _emittedParticles;
      }
     
      if (numberOfParticles >= 1)
      {
        ParticleSystem.AddParticles((int)numberOfParticles, this);
        _emittedParticles += (int)numberOfParticles;
      }

      // The decimal fraction of numberOfParticles is truncated, so not the whole
      // deltaTime is really used. We store the unused fraction for the next frame.
      _leftoverParticles = numberOfParticles - (float)Math.Floor(numberOfParticles);
    }
    #endregion
  }
}
