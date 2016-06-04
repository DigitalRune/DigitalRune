// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;


namespace DigitalRune.Game
{
  /// <summary>
  /// Manages a collection of <see cref="GameObject"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class has a collection of game objects. The game can add objects and the objects will be
  /// updated once per frame. Therefore, <see cref="GameObjectManager.Update"/> has to be called
  /// once per frame in the game loop.
  /// </para>
  /// <para>
  /// If a game objects removes another game object from the <see cref="Objects"/> list, the other
  /// object will be effectively removed at the end of <see cref="GameObjectManager.Update"/>
  /// method. That means <see cref="GameObject.Update"/> for the current frame will still be
  /// executed, and in the next frame the object is removed.
  /// </para>
  /// </remarks>
  public class GameObjectManager : IGameObjectService
  {
    // Note:
    // Potential memory leak: 
    // Create GameObjectManager. --> Add game objects. --> Call Update() --> Remove all 
    // game objects. --> Leak: _objectsCopy still has a reference to the game objects. 
    // This does only happen if Update is not called anymore and if the GameObjectManager is
    // not garbage collected - a very unlikely scenario.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<GameObject> _objectsCopy = new List<GameObject>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public GameObjectCollection Objects { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GameObjectManager"/> class.
    /// </summary>
    public GameObjectManager()
    {
      Objects = new GameObjectCollection();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Updates all game objects.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last frame.</param>
    public void Update(TimeSpan deltaTime)
    {
      // To update the objects, we loop over a copy of the collection. Thus, the original 
      // collection can be modified by the game objects. 
      if (Objects.IsDirty)
      {
        // Our copy must be updated whenever the original collection has changed.
        Objects.IsDirty = false;

        _objectsCopy.Clear();
        foreach (var gameObject in Objects)
          _objectsCopy.Add(gameObject);
      }

      // First reset the game object state that is time step dependent.
      foreach (var gameObject in _objectsCopy)
        gameObject.NewFrame();

      // TODO: For bindings. Update all bindings before the entities.

      // Update elements.
      foreach (var gameObject in _objectsCopy)
        gameObject.Update(deltaTime);

      // If an object has removed itself, remove all strong references.
      if (Objects.IsDirty)
        _objectsCopy.Clear();
    }
    #endregion
  }
}
