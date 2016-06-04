// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game
{
  /// <summary>
  /// Provides data for the <see cref="GameProperty{T}.Changing"/> and the 
  /// <see cref="GameProperty{T}.Changed"/> event of a game object property.
  /// </summary>
  public abstract class GamePropertyEventArgs : EventArgs
  {
    /// <summary>
    /// Gets the game object property.
    /// </summary>
    /// <value>The game object property.</value>
    public IGameProperty Property
    {
      get { return UntypedProperty; }
    }


    // Trick: "Property" uses "UntypedProperty", which is implemented in the derived class.
    internal abstract IGameProperty UntypedProperty { get; }
  }
}
