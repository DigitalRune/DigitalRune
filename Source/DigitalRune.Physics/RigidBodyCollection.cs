// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;


namespace DigitalRune.Physics
{
  /// <summary>
  /// Stores the <see cref="RigidBody"/> objects of a simulation.
  /// </summary>
  public class RigidBodyCollection : NotifyingCollection<RigidBody>
  {
    // Note: AllowDuplicates is true, because don't need to check for duplicates in the base class.
    // We check whether RigidBody.Simulation is set to avoid duplicates.

    /// <summary>
    /// Initializes a new instance of the <see cref="RigidBodyCollection"/> class.
    /// </summary>
    internal RigidBodyCollection() 
      : base(false, true)
    {
    }


    /// <inheritdoc/>
    protected override void InsertItem(int index, RigidBody item)
    {
      if (item != null && item.Simulation != null)
        throw new InvalidOperationException("Cannot add rigid body to simulation. The rigid body is already part of a simulation.");

      base.InsertItem(index, item);
    }


    /// <inheritdoc/>
    protected override void SetItem(int index, RigidBody item)
    {
      if (item != null && item.Simulation != null)
        throw new InvalidOperationException("Cannot add rigid body to simulation. The rigid body is already part of a simulation.");

      base.SetItem(index, item);
    }
  }
}
