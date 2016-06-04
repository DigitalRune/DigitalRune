// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;


namespace DigitalRune.Game.States
{
  /// <summary>
  /// Manages the <see cref="Transition"/>s of a <see cref="State"/>.
  /// </summary>
  /// <remarks>
  /// <see langword="null"/> or duplicate items are not allowed in this collection.
  /// </remarks>
  public class TransitionCollection : NotifyingCollection<Transition>
  {
    /// <summary>
    /// Gets the <see cref="Transition"/> with the specified name.
    /// </summary>
    /// <param name="name">The name of the transition.</param>
    public Transition this[string name]
    {
      get
      {
        foreach (var transition in this)
          if (transition.Name == name)
            return transition;

        return null;
      }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TransitionCollection"/> class.
    /// </summary>
    public TransitionCollection() : base(false, false)
    {
    }
  }
}
