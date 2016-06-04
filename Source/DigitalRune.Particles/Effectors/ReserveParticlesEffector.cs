// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Kills old particles if the particle system reaches the 
  /// <see cref="ParticleSystem.MaxNumberOfParticles"/> limit.
  /// </summary>
  /// <remarks>
  /// The number of particles in each <see cref="ParticleSystem"/> is limited (see 
  /// <see cref="ParticleSystem.MaxNumberOfParticles"/>). If the limit is reached, no more new
  /// particles can be emitted until existing particles die. The <see cref="ReserveParticleEffector"/>
  /// monitors <see cref="ParticleSystem.NumberOfActiveParticles"/> and if less than 
  /// <see cref="Reserve"/> particles are left, it kills the oldest particles (by setting their 
  /// "NormalizedAge" to 1) in its <see cref="OnUpdateParticles"/> method. This effector ensures 
  /// that there are always free particles ready to be emitted. Usually, this effector should be 
  /// added as the last effector to the <see cref="ParticleSystem.Effectors"/> collection of the 
  /// particle system because it should be updated after all other effectors.
  /// </remarks>
  public class ReserveParticleEffector : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private int _numberOfFreeParticles;
    private IParticleParameter<float> _normalizedAgeParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the number of particles to reserve.
    /// </summary>
    /// <value>
    /// The number of particles to reserve. The default value is 5.
    /// </value>
    public int Reserve { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ReserveParticleEffector"/> class.
    /// </summary>
    public ReserveParticleEffector()
    {
      Reserve = 5;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new ReserveParticleEffector();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone ReserveParticleEffector properties.
      var sourceTyped = (ReserveParticleEffector)source;
      Reserve = sourceTyped.Reserve;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _normalizedAgeParameter = ParticleSystem.Parameters.Get<float>(ParticleParameterNames.NormalizedAge);
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _normalizedAgeParameter = null;
    }


    /// <inheritdoc/>
    protected override void OnBeginUpdate(TimeSpan deltaTime)
    {
      _numberOfFreeParticles = ParticleSystem.MaxNumberOfParticles - ParticleSystem.NumberOfActiveParticles;
    }


    /// <inheritdoc/>
    protected override void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      // Number of free slots to reserve.
      int reserve = Reserve;

      // Kill particles until there are enough free slots.
      for (int i = startIndex; i < startIndex + count && _numberOfFreeParticles < reserve; i++)
      {
        if (_normalizedAgeParameter.Values[i] < 1.0f)
        {
          // Particle is not dead yet? - Kill it!!!
          _normalizedAgeParameter.Values[i] = 1f;
          _numberOfFreeParticles++;
        }
      }
    }
    #endregion
  }
}
