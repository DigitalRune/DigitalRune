// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Particles
{
  /// <summary>
  /// Manages a collection of particle systems.
  /// </summary>
  public interface IParticleSystemService
  {
    /// <summary>
    /// Gets the particle systems.
    /// </summary>
    /// <value>The particle systems.</value>
    ParticleSystemCollection ParticleSystems { get; }
  }
}
