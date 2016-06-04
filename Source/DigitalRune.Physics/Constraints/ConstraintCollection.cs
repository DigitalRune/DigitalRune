// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;


namespace DigitalRune.Physics.Constraints
{
  /// <summary>
  /// Stores the <see cref="Constraint"/> objects of a simulation.
  /// </summary>
  public class ConstraintCollection : NotifyingCollection<Constraint>
  {
    // Note: AllowDuplicates is true, because don't need to check for duplicates in the base class.
    // We check whether Constraint.Simulation is set to avoid duplicates.

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintCollection"/> class.
    /// </summary>
    internal ConstraintCollection() 
      : base(false, true)
    {
    }


    /// <inheritdoc/>
    protected override void InsertItem(int index, Constraint item)
    {
      if (item != null && item.Simulation != null)
        throw new InvalidOperationException("Cannot add constraint to simulation. The constraint is already part of a simulation.");

      base.InsertItem(index, item);
    }


    /// <inheritdoc/>
    protected override void SetItem(int index, Constraint item)
    {
      if (item != null && item.Simulation != null)
        throw new InvalidOperationException("Cannot add constraint to simulation. The constraint is already part of a simulation.");

      base.SetItem(index, item);
    }
  }
}
