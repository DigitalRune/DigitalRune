// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace DigitalRune.Game
{
  public partial class GameObject 
  {
    // All metadata are stored in these lists. The metadata IDs are the indices in these lists.
    // Metadata is also stored in lists in GamePropertyMetadata<T> for retrieval of the specialized
    // types.
    internal static int NextPropertyId;
    internal static List<IGamePropertyMetadata> PropertyMetadata = new List<IGamePropertyMetadata>();

    internal static int NextEventId;
    internal static List<IGameEventMetadata> EventMetadata = new List<IGameEventMetadata>();


    /// <summary>
    /// Defines a game object property.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="name">The name.</param>
    /// <param name="category">The category.</param>
    /// <param name="description">The description.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The property metadata.</returns>
    /// <remarks>
    /// Game object properties must be defined using this method before they can be used. Properties
    /// are identified by name and type. If a property with the given <paramref name="name"/>
    /// and type <typeparamref name="T"/> has already been defined, the metadata of the existing
    /// property is returned and the other parameters (<paramref name="category"/>, 
    /// <paramref name="description"/> and <paramref name="defaultValue"/>) are ignored!
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> must not be an empty string.
    /// </exception>
    public static GamePropertyMetadata<T> CreateProperty<T>(string name, string category, string description, T defaultValue)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("Name must not be an empty string.", "name");

      GamePropertyMetadata<T> metadata;
      if (GamePropertyMetadata<T>.Properties.TryGet(name, out metadata))
      {
        // Nothing to do - property was already defined.
        return metadata;
      }

      Debug.Assert(NextPropertyId == PropertyMetadata.Count);

      // Create and add new metadata.
      metadata = new GamePropertyMetadata<T>(name, NextPropertyId)
      {
        Category = category,
        Description = description,
        DefaultValue = defaultValue,
      };

      PropertyMetadata.Add(metadata);
      NextPropertyId++;

      return metadata;
    }


    /// <summary>
    /// Defines a game object event.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="name">The name.</param>
    /// <param name="category">The category.</param>
    /// <param name="description">The description.</param>
    /// <param name="defaultEventArgs">
    /// The default event arguments that are used when the event is raised without custom event
    /// arguments.
    /// </param>
    /// <returns>The event metadata.</returns>
    /// <remarks>
    /// Game object events must be defined using this method before they can be used. Events are
    /// identified by name and type. If an event with the given <paramref name="name"/> and type 
    /// <typeparamref name="T"/> has already been defined, the metadata of the existing event is
    /// returned and the other parameters (<paramref name="category"/>, 
    /// <paramref name="description"/> and <paramref name="defaultEventArgs"/>) are ignored!
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> must not be an empty string.
    /// </exception>
    public static GameEventMetadata<T> CreateEvent<T>(string name, string category, string description, T defaultEventArgs) where T : EventArgs
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("Name must not be an empty string.", "name");

      GameEventMetadata<T> metadata;
      if (GameEventMetadata<T>.Events.TryGet(name, out metadata))
      {
        // Nothing to do - event was already defined.
        return metadata;
      }

      Debug.Assert(NextEventId == EventMetadata.Count);

      // Create and add new metadata.
      metadata = new GameEventMetadata<T>(name, NextEventId)
      {
        Category = category,
        Description = description,
        DefaultEventArgs = defaultEventArgs,
      };

      EventMetadata.Add(metadata);
      NextEventId++;

      return metadata;
    }


    /// <overloads>
    /// <summary>
    /// Gets game object property metadata.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the metadata of the property with the given name.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="name">The property name.</param>
    /// <returns>
    /// The property metadata, or <see langword="null"/> if no property with the given 
    /// <paramref name="name"/> was defined with <see cref="CreateProperty{T}"/>.
    /// </returns>
    public static GamePropertyMetadata<T> GetPropertyMetadata<T>(string name)
    {
      if (name == null)
        return null;

      GamePropertyMetadata<T> metadata;
      GamePropertyMetadata<T>.Properties.TryGet(name, out metadata);

      return metadata;
    }


    /// <summary>
    /// Gets the metadata of the property with the given ID.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="id">The property ID.</param>
    /// <returns>
    /// The property metadata, or <see langword="null"/> if no property with the given 
    /// <paramref name="id"/> was defined with <see cref="CreateProperty{T}"/>.
    /// </returns>
    public static GamePropertyMetadata<T> GetPropertyMetadata<T>(int id)
    {
      if (id < 0 || id >= PropertyMetadata.Count)
        return null;

      return (GamePropertyMetadata<T>)PropertyMetadata[id];
    }


    /// <overloads>
    /// <summary>
    /// Gets the game object event metadata.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets metadata for the event with the given name.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="name">The event name.</param>
    /// <returns>
    /// The event metadata, or <see langword="null"/> if no event with the given 
    /// <paramref name="name"/> was defined with <see cref="CreateEvent{T}"/>.
    /// </returns>
    public static GameEventMetadata<T> GetEventMetadata<T>(string name) where T : EventArgs
    {
      if (name == null)
        return null;

      GameEventMetadata<T> metadata;
      GameEventMetadata<T>.Events.TryGet(name, out metadata);

      return metadata;
    }


    /// <summary>
    /// Gets the metadata of the event with the given ID.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    /// <param name="id">The event ID.</param>
    /// <returns>
    /// The event metadata, or <see langword="null"/> if no event with the given 
    /// <paramref name="id"/> was defined with <see cref="CreateEvent{T}"/>.
    /// </returns>
    public static GameEventMetadata<T> GetEventMetadata<T>(int id) where T : EventArgs
    {
      if (id < 0 || id >= EventMetadata.Count)
        return null;

      return (GameEventMetadata<T>)EventMetadata[id];
    }


    /// <summary>
    /// Gets the metadata of all game object properties that were created with 
    /// <see cref="CreateProperty{T}"/>.
    /// </summary>
    /// <returns>
    /// The global collection of all created game object property metadata.
    /// </returns>
    public static IEnumerable<IGamePropertyMetadata> GetPropertyMetadata()
    {
      return PropertyMetadata;
    }


    /// <summary>
    /// Gets the metadata of all game object events that were created with 
    /// <see cref="CreateEvent{T}"/>.
    /// </summary>
    /// <returns>
    /// The global collection of all created game object event metadata.
    /// </returns>
    public static IEnumerable<IGameEventMetadata> GetEventMetadata()
    {
      return EventMetadata;
    }
  }
}
