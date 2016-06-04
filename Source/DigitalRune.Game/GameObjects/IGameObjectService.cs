// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game
{
  /// <summary>
  /// Manages <see cref="GameObject"/>s.
  /// </summary>
  /// <remarks>
  /// <see cref="GameObject"/>s in these list are automatically updated, usually once per frame.
  /// </remarks>
  public interface IGameObjectService
  {
    // TODO: (Optional) Add bindings.
    // Bindings create events before the entities are updated: InputCommandBinding, 
    // CollisionBinding, etc.


    /// <summary>
    /// Gets the game objects.
    /// </summary>
    /// <value>The game objects.</value>
    /// <remarks>
    /// Each game object is identified by its unique name.
    /// </remarks>
    GameObjectCollection Objects { get; }
  }
}
