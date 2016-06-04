// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines the type of particles in a particle system.
  /// </summary>
  public enum ParticleType
  {
    /// <summary>
    /// All particles in the particle system are rendered as individual billboards (quads).
    /// </summary>
    Billboard,

    /// <summary>
    /// Subsequent living particles in the particle system are rendered as connected ribbons 
    /// (quad strips). At least two living particles are required to create a ribbon. Dead particles 
    /// ("NormalizedAge" ≥ 1) can be used as delimiters to terminate one ribbon and start the next 
    /// ribbon.
    /// </summary>
    Ribbon,
  }
}
