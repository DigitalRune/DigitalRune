// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Collections;


namespace DigitalRune.Game
{
  // Factory method for use in the GameEventCollection.
  internal interface IGameEventFactory
  {
    IGameEvent CreateGameEvent(GameObject owner);
  }


  /// <summary>
  /// Identifies and describes a game object event.
  /// </summary>
  /// <typeparam name="T">The type of the event arguments.</typeparam>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public class GameEventMetadata<T> : IGameEventMetadata, IGameEventFactory where T : EventArgs
  {
    //--------------------------------------------------------------
    #region Static Fields
    //--------------------------------------------------------------
    
    // The global store for all event metadata for events of type T.
    internal static NamedObjectCollection<GameEventMetadata<T>> Events { get; private set; }
    #endregion
    

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion
      
      
    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public string Name { get; private set; }


    /// <inheritdoc/>
    public int Id { get; private set; }


    /// <inheritdoc/>
    public string Category { get; set; }


    /// <inheritdoc/>
    public string Description { get; set; }


    /// <summary>
    /// Gets or sets the default event arguments.
    /// </summary>
    /// <value>The default event arguments.</value>
    /// <remarks>
    /// If the event is raised without user-defined event arguments, the default arguments are
    /// passed to the event handlers.
    /// </remarks>
    public T DefaultEventArgs { get; set; }


    /// <inheritdoc/>
    EventArgs IGameEventMetadata.DefaultEventArgs
    {
      get { return DefaultEventArgs; }
    }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="GameEventMetadata{T}"/> class.
    /// </summary>
    static GameEventMetadata()
    {
      Events = new NamedObjectCollection<GameEventMetadata<T>>();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GameEventMetadata{T}"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="id">The ID.</param>
    /// <remarks>
    /// The event metadata is automatically added to the <see cref="Events"/> collection!
    /// </remarks>
    internal GameEventMetadata(string name, int id)
    {
      Debug.Assert(!string.IsNullOrEmpty(name), "Name must not be null or an empty string.");

      Name = name;
      Id = id;
      Events.Add(this);
    }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    IGameEvent IGameEventFactory.CreateGameEvent(GameObject owner)
    {
      return new GameEvent<T>(owner, this);
    }
    #endregion
  }
}
