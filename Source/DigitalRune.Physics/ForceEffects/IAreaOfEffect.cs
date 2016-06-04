// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Defines the area of effect of a <see cref="ForceField"/>. Only rigid bodies in the area of
  /// effect are subject to forces.
  /// </summary>
  public interface IAreaOfEffect
  {
    /// <summary>
    /// Calls <see cref="ForceField.Apply(RigidBody)"/> of the given force field for all objects
    /// in the area of effect.
    /// </summary>
    /// <param name="forceField">The force field.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="forceField"/> is <see langword="null"/>.
    /// </exception>
    void Apply(ForceField forceField);
  }
}
