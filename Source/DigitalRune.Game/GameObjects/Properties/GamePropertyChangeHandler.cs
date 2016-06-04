// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game
{
  /// <summary>
  /// Provides the <see cref="EventHandler{TEventArgs}"/> that connects a game property with
  /// another game property.
  /// </summary>
  /// <typeparam name="T">The type of the property value.</typeparam>
  internal sealed class GamePropertyChangeHandler<T> 
  {
    private GameProperty<T> _gameProperty;


    /// <summary>
    /// Initializes a new instance of the <see cref="GamePropertyChangeHandler{T}"/> class.
    /// </summary>
    /// <param name="gameProperty">The game property.</param>
    public GamePropertyChangeHandler(GameProperty<T> gameProperty)
    {
      _gameProperty = gameProperty;
    }


    /// <summary>
    /// Updates the value of the game property.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="GamePropertyEventArgs{T}"/> instance containing the event data.
    /// </param>
    public void Change(object sender, GamePropertyEventArgs<T> eventArgs)
    {
      _gameProperty.Value = eventArgs.NewValue;
    }
  }
}
