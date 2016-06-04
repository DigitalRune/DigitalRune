// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;


namespace DigitalRune.Particles
{
  /// <summary>
  /// Represents a collection of named <see cref="ParticleSystem"/>s.
  /// </summary>
  public class ParticleSystemCollection : NotifyingCollection<ParticleSystem>
  {
    // This collection is used by the IParticleSystemService and by ParticleSystem.

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystemCollection"/> class.
    /// </summary>
    public ParticleSystemCollection() 
      : base(false, false)
    {
    }
  }
}
