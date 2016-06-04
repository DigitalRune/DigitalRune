// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game
{
  /// <summary>
  /// Stores the event handlers of a game event.
  /// </summary>
  /// <typeparam name="T">The type of the <see cref="EventArgs"/>.</typeparam>
  internal sealed class GameEventData<T> : IGameEventData where T : EventArgs
  {
    // The actual event.
    public event EventHandler<T> Event;
      
      
    // Factory method of IGameEventData.
    public IGameEvent CreateGameEvent(GameObject owner, int eventId)
    {
      var metadata = (GameEventMetadata<T>) GameObject.EventMetadata[eventId];
      return new GameEvent<T>(owner, metadata);
    }


    public void Raise(object sender, T eventArgs)
    {
      var handler = Event;

      if (handler != null)
        handler(sender, eventArgs);
    }
  }
}
