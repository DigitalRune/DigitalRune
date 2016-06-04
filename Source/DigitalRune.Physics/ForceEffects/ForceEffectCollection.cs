// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;


namespace DigitalRune.Physics.ForceEffects
{
  /// <summary>
  /// Stores the <see cref="ForceEffect"/>s of a simulation.
  /// </summary>
  public class ForceEffectCollection : NotifyingCollection<ForceEffect>
  {
    // Note: AllowDuplicates is true, because don't need to check for duplicates in the base class.
    // We check whether ForceEffect.Simulation is set to avoid duplicates.

    /// <summary>
    /// Initializes a new instance of the <see cref="ForceEffectCollection"/> class.
    /// </summary>
    internal ForceEffectCollection() 
      : base(false, true)
    {
    }


    /// <inheritdoc/>
    protected override void InsertItem(int index, ForceEffect item)
    {
      if (item != null && item.Simulation != null)
        throw new InvalidOperationException("Cannot add force effect to simulation. The force effect is already part of a simulation.");

      base.InsertItem(index, item);
    }


    /// <inheritdoc/>
    protected override void SetItem(int index, ForceEffect item)
    {
      if (item != null && item.Simulation != null)
        throw new InvalidOperationException("Cannot add force effect to simulation. The force effect is already part of a simulation.");

      base.SetItem(index, item);
    }
  }
}
