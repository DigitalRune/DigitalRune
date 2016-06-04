// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game
{
  public partial class GameObject 
  {
    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    // The local property data of this game object.
    internal DataStore<IGamePropertyData> PropertyData
    {
      get
      {
        if (_propertyData == null)
          _propertyData = new DataStore<IGamePropertyData>();

        return _propertyData;
      }
    }
    private DataStore<IGamePropertyData> _propertyData;


    // Event handlers used to connect game properties.
    internal DataStore<object> ChangeEventHandlers
    {
      get
      {
        if (_changeEventHandlers == null)
          _changeEventHandlers = new DataStore<object>();

        return _changeEventHandlers;
      }
    }
    private DataStore<object> _changeEventHandlers;


    // The local event data of this game object.
    internal DataStore<IGameEventData> EventData
    {
      get
      {
        if (_eventData == null)
          _eventData = new DataStore<IGameEventData>();

        return _eventData;
      }
    }
    private DataStore<IGameEventData> _eventData;


    // Event handlers used to connect game events with other events.
    internal DataStore<EventHandler<EventArgs>> RaiseEventHandlers
    {
      get
      {
        if (_raiseEventHandlers == null)
          _raiseEventHandlers = new DataStore<EventHandler<EventArgs>>();

        return _raiseEventHandlers;
      }
    }
    private DataStore<EventHandler<EventArgs>> _raiseEventHandlers;


    /// <summary>
    /// Gets the game object properties.
    /// </summary>
    /// <value>The properties of the game object.</value>
    /// <remarks>
    /// The property collection is a collection of all properties of the game object and user
    /// defined properties. The user and other game object can add additional properties to this 
    /// collection.
    /// </remarks>
    public GamePropertyCollection Properties
    {
      // GamePropertyCollection is a struct and only used to make the API look as if there is
      // property collection. Return a new struct to safe memory.
      get { return new GamePropertyCollection(this); }
    }


    /// <summary>
    /// Gets the game object events.
    /// </summary>
    /// <value>The events of the game object.</value>
    /// <remarks>
    /// <para>
    /// The event collection is a collection of all events of the game object and user defined
    /// events. The user and other game elements can add additional events to this collection.
    /// </para>
    /// </remarks>
    public GameEventCollection Events
    {
      get { return new GameEventCollection(this); }
    }
    #endregion
    

    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary> 
    /// Gets the value of a game object property.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the value of the property with the given name.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>
    /// The value of the property.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyName"/> is an empty string.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The property is not defined. Properties must be defined with 
    /// <see cref="GameObject.CreateProperty{T}"/> before they can be used.
    /// </exception>
    public T GetValue<T>(string propertyName)
    {
      return Properties.Get<T>(propertyName).Value;
    }


    /// <summary>
    /// Gets the value of the property with the given ID.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="propertyId">The ID of the property.</param>
    /// <returns>
    /// The value of the property.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The <paramref name="propertyId"/> is invalid. Properties must be defined with 
    /// <see cref="GameObject.CreateProperty{T}"/> before they can be used.
    /// </exception>
    public T GetValue<T>(int propertyId)
    {
      return Properties.Get<T>(propertyId).Value;
    }


    /// <summary>
    /// Gets the value of the property with the given metadata.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="propertyMetadata">The metadata of the property.</param>
    /// <returns>
    /// The value of the property.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyMetadata"/> is <see langword="null"/>.
    /// </exception>
    public T GetValue<T>(GamePropertyMetadata<T> propertyMetadata)
    {
      return Properties.Get(propertyMetadata).Value;
    }


    /// <overloads>
    /// <summary>
    /// Sets the value of a game object property.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Sets the value of the property with the given name.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The new value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyName"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="propertyName"/> is an empty string.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The property is not defined. Properties must be defined with
    /// <see cref="GameObject.CreateProperty{T}"/> before they can be used.
    /// </exception>
    public void SetValue<T>(string propertyName, T value)
    {
      var property = Properties.Get<T>(propertyName);
      property.Value = value;
    }


    /// <summary>
    /// Sets the value of the property with the given ID.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="propertyId">The ID of the property.</param>
    /// <param name="value">The new value.</param>
    /// <exception cref="ArgumentException">
    /// The <paramref name="propertyId"/> is invalid. Properties must be defined with 
    /// <see cref="GameObject.CreateProperty{T}"/> before they can be used.
    /// </exception>
    public void SetValue<T>(int propertyId, T value)
    {
      var property = Properties.Get<T>(propertyId);
      property.Value = value;
    }


    /// <summary>
    /// Set the value of the property for the given metadata.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="propertyMetadata">The metadata of the property.</param>
    /// <param name="value">The new value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="propertyMetadata"/> is <see langword="null"/>.
    /// </exception>
    public void SetValue<T>(GamePropertyMetadata<T> propertyMetadata, T value)
    {
      var property = Properties.Get(propertyMetadata);
      property.Value = value;
    }
    #endregion
  }
}
