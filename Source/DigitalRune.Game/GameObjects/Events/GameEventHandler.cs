// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game
{
  /// <summary>
  /// Provides the <see cref="EventHandler{TEventArgs}"/> that raises a game event when another
  /// event occurs.
  /// </summary>
  /// <typeparam name="T">The type of the event args.</typeparam>
  internal sealed class GameEventHandler<T> where T : EventArgs
  {
    private GameEvent<T> _gameEvent;


    /// <summary>
    /// Initializes a new instance of the <see cref="GameEventHandler{T}"/> class.
    /// </summary>
    /// <param name="gameEvent">The game event.</param>
    public GameEventHandler(GameEvent<T> gameEvent)
    {
      _gameEvent = gameEvent;
    }


    /// <summary>
    /// Raises game event.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="EventArgs"/> instance containing the event data.
    /// </param>
    public void Raise(object sender, EventArgs eventArgs)
    {
      var eventArgsAsT = eventArgs as T;
      if (eventArgsAsT != null)
      {
        // The event args of the game event and the original event are compatible.
        _gameEvent.Raise(eventArgsAsT);
      }
      else
      {
        // Incompatible event args. Pass the default event args.
        _gameEvent.Raise();
      }
    }
  }
}
