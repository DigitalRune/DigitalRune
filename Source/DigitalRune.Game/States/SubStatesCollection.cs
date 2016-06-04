// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;


namespace DigitalRune.Game.States
{
  /// <summary>
  /// Manages the parallel sub-states of a <see cref="State"/>.
  /// </summary>
  /// <remarks>
  /// <see langword="null"/> or duplicate items are not allowed in this collection.
  /// </remarks>
  public class SubStatesCollection : NotifyingCollection<StateCollection>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SubStatesCollection"/> class.
    /// </summary>
    public SubStatesCollection() : base(false, false)
    {
    }
  }
}
