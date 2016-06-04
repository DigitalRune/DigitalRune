// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;


namespace DigitalRune.Particles
{
  /// <summary>
  /// Manages a collection of <see cref="ParticleEffector"/>s.
  /// </summary>
  public class ParticleEffectorCollection : NotifyingCollection<ParticleEffector>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleEffectorCollection"/> class.
    /// </summary>
    public ParticleEffectorCollection() 
      : base(false, false)
    {
    }
  }
}
