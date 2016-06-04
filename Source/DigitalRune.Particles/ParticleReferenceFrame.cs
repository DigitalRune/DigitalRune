// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Particles
{
  /// <summary>
  /// Defines which 3D coordinate system is used for vectors in particle parameters and particle
  /// effector properties.
  /// </summary>
  public enum ParticleReferenceFrame
  {
    /// <summary>
    /// All parameters are relative to world space (which is the space in which the root particle
    /// system is placed).
    /// </summary>
    World,

    ///// <summary>
    ///// All parameters are relative to the coordinate system of the parent particle system.
    ///// If the particle system is not a child of another particle system, this is the same
    ///// as <see cref="World"/>.
    ///// </summary>
    //Parent,

    /// <summary>
    /// All parameters are relative to the local space of the particle system.
    /// </summary>
    Local,
  }
}
