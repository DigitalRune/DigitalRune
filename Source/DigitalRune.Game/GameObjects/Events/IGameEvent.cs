// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game
{
  /// <summary>
  /// Base interface for <see cref="GameEvent{T}"/>.
  /// </summary>
  public interface IGameEvent : INamedObject
  {
    /// <summary>
    /// Gets the game object that owns this event.
    /// </summary>
    /// <value>The <see cref="GameObject"/> that owns this event.</value>
    GameObject Owner { get; }


    /// <summary>
    /// Gets the event metadata.
    /// </summary>
    /// <value>The event metadata.</value>
    IGameEventMetadata Metadata { get; }
  }
}
