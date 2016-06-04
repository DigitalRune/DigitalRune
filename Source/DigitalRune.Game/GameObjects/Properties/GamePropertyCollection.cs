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
  /// Manages a collection of game object properties.
  /// </summary>
  /// <remarks>
  /// This collection implements <see cref="IEnumerable{T}"/>, but enumerating the collection
  /// allocates heap memory (garbage!) and should only be used in game editors and not in
  /// performance critical paths of a game.
  /// </remarks>
  public struct GamePropertyCollection : IEnumerable<IGameProperty>, IEquatable<GamePropertyCollection>
  {
    // This is only a facade. It does not store any data other than the reference to the 
    // game object.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
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
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GamePropertyCollection"/> struct.
    /// </summary>
    /// <param name="owner">The game object that owns this collection.</param>
    internal GamePropertyCollection(GameObject owner)
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
    public IEnumerator<IGameProperty> GetEnumerator()
    {
      var properties = new HashSet<IGameProperty>();
      if (Owner.Template != null)
      {
        // Add properties from all templates.
        foreach (var property in Owner.Template.Properties)
          properties.Add(((IGamePropertyFactory)property.Metadata).CreateGameProperty(Owner));
      }

      var count = Owner.PropertyData.Count;
      for (int i = 0; i < count; i++)
      {
        int id = Owner.PropertyData.GetIdByIndex(i);
        var data = Owner.PropertyData.GetByIndex(i);
        
        // Add or replace entry from template.
        properties.Add(data.CreateGameProperty(Owner, id));
      }

      return properties.GetEnumerator();
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
    /// Gets the metadata by name and performs several parameter checks. Exceptions are thrown when
    /// the name is invalid or no metadata is found.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is an empty string.
    /// </exception>
    private static GamePropertyMetadata<T> GetMetadataChecked<T>(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("Name must not be an empty string.", "name");

      var metadata = GameObject.GetPropertyMetadata<T>(name);
      if (metadata == null)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "No game object property with the name '{0}' and type '{1}' was defined. "
          + "Game object properties must be defined using GameObject.CreateProperty() before they can be used.", 
          name, 
          typeof(T).FullName);

        throw new ArgumentException(message, "name");
      }

      return metadata;
    }


    /// <overloads>
    /// <summary>
    /// Adds a game object property with its default value.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Adds a property with with the given name.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="name">The property name.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is an empty string.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The property is not defined. Properties must be defined with 
    /// <see cref="GameObject.CreateProperty{T}"/> before they can be added.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public void Add<T>(string name)
    {
      GamePropertyMetadata<T> metadata = GetMetadataChecked<T>(name);
      Add(metadata);
    }


    /// <summary>
    /// Adds a property with the given ID.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="id">The ID of the property.</param>
    /// <exception cref="ArgumentException">
    /// The <paramref name="id"/> is invalid. Properties must be defined with 
    /// <see cref="GameObject.CreateProperty{T}"/> before they can be added.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public void Add<T>(int id)
    {
      if (Owner.PropertyData.Get(id) != null)
        return;

      if (id < 0 || id >= GameObject.PropertyMetadata.Count)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Unknown ID. No game object property with the ID '{0}' and type '{1}' was defined. "
          + "Game object properties must be defined using GameObject.CreateProperty() before they can be used.",
          id,
          typeof(T).FullName);
        throw new ArgumentException(message, "id");
      }

      var data = new GamePropertyData<T>();
      Owner.PropertyData.Set(id, data);
    }


    /// <summary>
    /// Adds a property for the given metadata.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="metadata">The metadata of the property.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="metadata"/> is <see langword="null"/>.
    /// </exception>
    public void Add<T>(GamePropertyMetadata<T> metadata)
    {
      if (metadata == null)
        throw new ArgumentNullException("metadata");

      Add<T>(metadata.Id);
    }


    /// <overloads>
    /// <summary> 
    /// Gets a game object property.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the property with the given name.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>The property.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is an empty string.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The property is not defined. Properties must be defined with 
    /// <see cref="GameObject.CreateProperty{T}"/> before they can be used.
    /// </exception>
    public GameProperty<T> Get<T>(string name)
    {
      GamePropertyMetadata<T> metadata = GetMetadataChecked<T>(name);
      return new GameProperty<T>(Owner, metadata);
    }


    /// <summary>
    /// Gets the property with the given ID.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="id">The ID of the property.</param>
    /// <returns>The property.</returns>
    /// <exception cref="ArgumentException">
    /// The <paramref name="id"/> is invalid. Note: Properties must be defined with 
    /// <see cref="GameObject.CreateProperty{T}"/> before they can be used.
    /// </exception>
    public GameProperty<T> Get<T>(int id)
    {
      if (id < 0 || id >= GameObject.PropertyMetadata.Count)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Unknown ID. No game object property with the ID '{0}' and type '{1}' was defined. "
          + "Game object properties must be defined using GameObject.CreateProperty() before they can be used.",
          id,
          typeof(T).FullName);
        throw new ArgumentException(message, "id");
      }

      GamePropertyMetadata<T> metadata = (GamePropertyMetadata<T>)GameObject.PropertyMetadata[id];
      return new GameProperty<T>(Owner, metadata);
    }


    /// <summary>
    /// Gets the property for the given metadata.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="metadata">The metadata of the property.</param>
    /// <returns>The property</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="metadata"/> is <see langword="null"/>.
    /// </exception>
    public GameProperty<T> Get<T>(GamePropertyMetadata<T> metadata)
    {
      if (metadata == null)
        throw new ArgumentNullException("metadata");

      return new GameProperty<T>(Owner, metadata);
    }


    /// <overloads>
    /// <summary> 
    /// Removes a game object property.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Removes the property with the given name.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>
    /// <see langword="true"/> if the property was removed; otherwise, <see langword="false"/>
    /// if the property was not found.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is an empty string.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The property is not defined. Properties must be defined with 
    /// <see cref="GameObject.CreateProperty{T}"/> before they can be used.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
    public bool Remove<T>(string name)
    {
      GamePropertyMetadata<T> metadata = GetMetadataChecked<T>(name);
      return Remove(metadata.Id);
    }


    /// <summary>
    /// Removes the property with the given ID.
    /// </summary>
    /// <param name="id">The ID of the property.</param>
    /// <returns>
    /// <see langword="true"/> if the property was removed; otherwise, <see langword="false"/>
    /// if the property was not found.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The <paramref name="id"/> is invalid. Note: Properties must be defined with 
    /// <see cref="GameObject.CreateProperty{T}"/> before they can be used.
    /// </exception>
    public bool Remove(int id)
    {
      Owner.ChangeEventHandlers.Remove(id);
      return Owner.PropertyData.Remove(id);
    }


    /// <summary>
    /// Removes the property for the given metadata.
    /// </summary>
    /// <param name="metadata">The metadata of the property.</param>
    /// <returns>
    /// <see langword="true"/> if the property was removed; otherwise, <see langword="false"/> if
    /// the property was not found.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="metadata"/> is <see langword="null"/>.
    /// </exception>
    public bool Remove(IGamePropertyMetadata metadata)
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
      return obj is GamePropertyCollection && Equals((GamePropertyCollection)obj);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> 
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(GamePropertyCollection other)
    {
      return _owner == other._owner;
    }


    /// <summary>
    /// Compares two <see cref="GamePropertyCollection"/>s to determine whether they are the same.
    /// </summary>
    /// <param name="collection1">The first <see cref="GamePropertyCollection"/>.</param>
    /// <param name="collection2">The second <see cref="GamePropertyCollection"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="collection1"/> and 
    /// <paramref name="collection2"/> are the same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(GamePropertyCollection collection1, GamePropertyCollection collection2)
    {
      return collection1._owner == collection2._owner;
    }


    /// <summary>
    /// Compares two <see cref="GamePropertyCollection"/>s to determine whether they are different.
    /// </summary>
    /// <param name="collection1">The first <see cref="GamePropertyCollection"/>.</param>
    /// <param name="collection2">The second <see cref="GamePropertyCollection"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="collection1"/> and 
    /// <paramref name="collection2"/> are different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(GamePropertyCollection collection1, GamePropertyCollection collection2)
    {
      return collection1._owner != collection2._owner;
    }
    #endregion
  }
}
