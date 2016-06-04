// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;


namespace DigitalRune.Game
{
  /// <summary>
  /// Manages a collection of game object events.
  /// </summary>
  /// <remarks>
  /// This collection implements <see cref="IEnumerable{T}"/>, but enumerating the collection
  /// allocates heap memory (garbage!) and should only be used in game editors and not in
  /// performance critical paths of a game.
  /// </remarks>
  public struct GameEventCollection : IEnumerable<IGameEvent>, IEquatable<GameEventCollection>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the game object that owns this collection.
    /// </summary>
    /// <value>The game object that owns this collection.</value>
    public GameObject Owner
    {
      get { return _owner; }
    }
    private readonly GameObject _owner;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEventCollection"/> struct.
    /// </summary>
    /// <param name="owner">The game object that owns this collection.</param>
    internal GameEventCollection(GameObject owner)
    {
      Debug.Assert(owner != null, "Owner must not be null.");
      
      _owner = owner;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}"/> object that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<IGameEvent> GetEnumerator()
    {
      var events = new HashSet<IGameEvent>();
      if (Owner.Template != null)
      {
        // Add events from all templates.
        foreach (var gameEvent in Owner.Template.Events)
          events.Add(((IGameEventFactory)gameEvent.Metadata).CreateGameEvent(Owner));
      }

      var count = Owner.EventData.Count;
      for (int i = 0; i < count; i++)
      {
        var id = Owner.EventData.GetIdByIndex(i);
        var data = Owner.EventData.GetByIndex(i);

        // Add or replace entry from template.
        events.Add(data.CreateGameEvent(Owner, id));
      }

      return events.GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Gets the metadata by name and performs several parameter checks. Exceptions
    /// are thrown when the name is invalid or no metadata is found.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is an empty string.
    /// </exception>
    private static GameEventMetadata<T> GetMetadataChecked<T>(string name) where T : EventArgs
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("Name must not be an empty string.", "name");

      var metadata = GameObject.GetEventMetadata<T>(name);
      if (metadata == null)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "No game object event with the name '{0}' and type '{1}' was defined. "
          + "Game object events must be defined using GameObject.CreateEvent() before they can be used.",
          name,
          typeof(T).FullName);

        throw new ArgumentException(message, "name");
      }

      return metadata;
    }


    /// <overloads>
    /// <summary>
    /// Adds an event.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Adds an event with the given name.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="name">The event name.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is an empty string.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The event is not defined. Events must be defined with 
    /// <see cref="GameObject.CreateEvent{T}"/> before they can be added.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public void Add<T>(string name) where T : EventArgs
    {
      GameEventMetadata<T> metadata = GetMetadataChecked<T>(name);
      Add<T>(metadata.Id);
    }


    /// <summary>
    /// Adds an event with the given ID.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="id">The ID of the event.</param>
    /// <exception cref="ArgumentException">
    /// The <paramref name="id"/> is invalid. Events must be defined with 
    /// <see cref="GameObject.CreateEvent{T}"/> before they can be added.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public void Add<T>(int id) where T : EventArgs
    {
      if (Owner.EventData.Get(id) != null)
        return;

      if (id < 0 || id >= GameObject.EventMetadata.Count)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Unknown ID. No game object event with the ID '{0}' and type '{1}' was defined. "
          + "Game object events must be defined using GameObject.CreateEvent() before they can be used.",
          id,
          typeof(T).FullName);
        throw new ArgumentException(message, "id");
      }

      var data = new GameEventData<T>();
      Owner.EventData.Set(id, data);
    }


    /// <summary>
    /// Adds an event for the given metadata.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="metadata">The metadata of the event.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="metadata"/> is <see langword="null"/>.
    /// </exception>
    public void Add<T>(GameEventMetadata<T> metadata) where T : EventArgs
    {
      if (metadata == null)
        throw new ArgumentNullException("metadata");

      Add<T>(metadata.Id);
    }


    /// <overloads>
    /// <summary> 
    /// Gets a game object event.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the event with the given name.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="name">The name of the event.</param>
    /// <returns>The game object event.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is an empty string.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The event is not defined. Events must be defined with 
    /// <see cref="GameObject.CreateEvent{T}"/> before they can be used.
    /// </exception>
    public GameEvent<T> Get<T>(string name) where T : EventArgs
    {
      var metadata = GetMetadataChecked<T>(name);
      return new GameEvent<T>(Owner, metadata);
    }


    /// <summary>
    /// Gets the event with the given ID.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="id">The ID of the event.</param>
    /// <returns>The event.</returns>
    /// <exception cref="ArgumentException">
    /// The <paramref name="id"/> is invalid. Events must be defined with 
    /// <see cref="GameObject.CreateEvent{T}"/> before they can be used.
    /// </exception>
    public GameEvent<T> Get<T>(int id) where T : EventArgs
    {
      if (id < 0 || id >= GameObject.EventMetadata.Count)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Unknown ID. No game object event with the ID '{0}' and type '{1}' was defined. "
          + "Game object events must be defined using GameObject.CreateEvent() before they can be used.",
          id,
          typeof(T).FullName);
        throw new ArgumentException(message, "id");
      }

      var metadata = (GameEventMetadata<T>)GameObject.EventMetadata[id];
      return new GameEvent<T>(Owner, metadata);
    }


    /// <summary>
    /// Gets the event for the given metadata.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="metadata">The metadata of the property.</param>
    /// <returns>The property</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="metadata"/> is <see langword="null"/>.
    /// </exception>
    public GameEvent<T> Get<T>(GameEventMetadata<T> metadata) where T : EventArgs
    {
      if (metadata == null)
        throw new ArgumentNullException("metadata");

      return new GameEvent<T>(Owner, metadata);
    }


    /// <overloads>
    /// <summary> 
    /// Removes a game object event.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Removes the event with the given name.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="name">The name of the event.</param>
    /// <returns>
    /// <see langword="true"/> if the event was removed; otherwise, <see langword="false"/>
    /// if the event was not found.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is an empty string.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The event is not defined. Events must be defined with 
    /// <see cref="GameObject.CreateEvent{T}"/> before they can be used.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public bool Remove<T>(string name) where T : EventArgs
    {
      GameEventMetadata<T> metadata = GetMetadataChecked<T>(name);
      return Remove(metadata.Id);
    }


    /// <summary>
    /// Removes the event with the given ID.
    /// </summary>
    /// <param name="id">The ID of the event.</param>
    /// <returns>
    /// <see langword="true"/> if the event was removed; otherwise, <see langword="false"/>
    /// if the event was not found.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The <paramref name="id"/> is invalid. Events must be defined with 
    /// <see cref="GameObject.CreateEvent{T}"/> before they can be used.
    /// </exception>
    public bool Remove(int id)
    {
      Owner.RaiseEventHandlers.Remove(id);
      return Owner.EventData.Remove(id);
    }


    /// <summary>
    /// Removes the event for the given metadata.
    /// </summary>
    /// <param name="metadata">The metadata of the event.</param>
    /// <returns>
    /// <see langword="true"/> if the event was removed; otherwise, <see langword="false"/>
    /// if the event was not found.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="metadata"/> is <see langword="null"/>.
    /// </exception>
    public bool Remove(IGameEventMetadata metadata)
    {
      if (metadata == null)
        throw new ArgumentNullException("metadata");

      return Remove(metadata.Id);
    }


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures 
    /// like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      return _owner.GetHashCode();
    }


    /// <summary>
    /// Determines whether the specified <see cref="Object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Object"/> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object"/> is equal to this instance; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is GameEventCollection && Equals((GameEventCollection)obj);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> 
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(GameEventCollection other)
    {
      return _owner == other._owner;
    }


    /// <summary>
    /// Compares two <see cref="GameEventCollection"/>s to determine whether they are the same.
    /// </summary>
    /// <param name="collection1">The first <see cref="GameEventCollection"/>.</param>
    /// <param name="collection2">The second <see cref="GameEventCollection"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="collection1"/> and 
    /// <paramref name="collection2"/> are the same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(GameEventCollection collection1, GameEventCollection collection2)
    {
      return collection1._owner == collection2._owner;
    }


    /// <summary>
    /// Compares two <see cref="GameEventCollection"/>s to determine whether they are different.
    /// </summary>
    /// <param name="collection1">The first <see cref="GameEventCollection"/>.</param>
    /// <param name="collection2">The second <see cref="GameEventCollection"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="collection1"/> and 
    /// <paramref name="collection2"/> are different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(GameEventCollection collection1, GameEventCollection collection2)
    {
      return collection1._owner != collection2._owner;
    }
    #endregion
  }
}
