// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRune.Game
{
  /// <summary>
  /// Represents event of a <see cref="GameObject"/>.
  /// </summary>
  /// <typeparam name="T">The type of the <see cref="EventArgs"/>.</typeparam>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public struct GameEvent<T> : IGameEvent, IEquatable<GameEvent<T>> where T : EventArgs
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly GameObject _owner;
    private readonly GameEventMetadata<T> _metadata;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public GameObject Owner
    {
      get { return _owner; }
    }


    /// <summary>
    /// Gets the event metadata.
    /// </summary>
    /// <value>The event metadata.</value>
    public GameEventMetadata<T> Metadata
    {
      get { return _metadata; }
    }


    /// <inheritdoc/>
    IGameEventMetadata IGameEvent.Metadata
    {
      get { return _metadata; }
    }


    /// <inheritdoc/>
    public string Name { get { return _metadata.Name; } }


    /// <summary>
    /// Event handler that automatically raises this game object event when another event occurs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This event handler can be used to raise this game event in response to another event. It 
    /// can, for example, be used to connect the <see cref="GameProperty{T}.Changed"/> event of a 
    /// <see cref="GameProperty{T}"/> to this event.
    /// </para>
    /// <code lang="csharp">
    /// <![CDATA[
    /// myFloatProperty.Changed += new EventHandler<GamePropertyEventArgs<float>>(myEvent.RaiseOnEvent);
    /// ]]>
    /// </code>
    /// <para>
    /// <c>myEvent</c> is fired every time when <c>myFloatProperty</c> changes its value.
    /// </para>
    /// </remarks>
    public EventHandler<EventArgs> RaiseOnEvent
    {
      get
      {
        var eventHandler = Owner.RaiseEventHandlers.Get(_metadata.Id);
        if (eventHandler == null)
        {
          eventHandler = new GameEventHandler<T>(this).Raise;
          Owner.RaiseEventHandlers.Set(_metadata.Id, eventHandler);
        }

        return eventHandler;
      }
    }


    /// <summary>
    /// The event.
    /// </summary>
    public event EventHandler<T> Event
    {
      add
      {
        GameEventData<T> data = GetOrCreateLocalData();
        data.Event += value;
      }
      remove
      {
        GameEventData<T> data = GetOrCreateLocalData();
        data.Event -= value;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEvent{T}"/> struct.
    /// </summary>
    /// <param name="owner">The game object that owns this event.</param>
    /// <param name="metadata">The metadata.</param>
    internal GameEvent(GameObject owner, GameEventMetadata<T> metadata)
    {
      Debug.Assert(owner != null, "Owner must not be null.");
      Debug.Assert(metadata != null, "Metadata must not be null.");

      _owner = owner;
      _metadata = metadata;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private GameEventData<T> GetOrCreateLocalData()
    {
      IGameEventData untypedData = Owner.EventData.Get(_metadata.Id);
      GameEventData<T> data;
      if (untypedData != null)
      {
        // data found!
        data = (GameEventData<T>)untypedData;
      }
      else
      {
        // No data found! Create new data.
        data = new GameEventData<T>();
        Owner.EventData.Set(_metadata.Id, data);
      }

      return data;
    }


    /// <overloads>
    /// <summary>
    /// Raises the game object event.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Raises the event with default arguments.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
    public void Raise()
    {
      IGameEventData untypedData = Owner.EventData.Get(_metadata.Id);
      if (untypedData != null)
      {
        var data = (GameEventData<T>)untypedData;
        data.Raise(Owner, Metadata.DefaultEventArgs);
      }
    }


    /// <summary>
    /// Raises the event with the given arguments.
    /// </summary>
    /// <param name="eventArgs">The event arguments.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
    public void Raise(T eventArgs)
    {
      IGameEventData untypedData = Owner.EventData.Get(_metadata.Id);
      if (untypedData != null)
      {
        var data = (GameEventData<T>)untypedData;
        data.Raise(Owner, eventArgs);
      }
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
      return _owner.GetHashCode() ^ _metadata.GetHashCode();
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
      return obj is GameEvent<T> && Equals((GameEvent<T>)obj);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> 
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(GameEvent<T> other)
    {
      if (_owner != other._owner)
        return false;

      return _metadata == other._metadata;
    }


    /// <summary>
    /// Compares two <see cref="GameEvent{T}"/>s to determine whether they are the same.
    /// </summary>
    /// <param name="event1">The first <see cref="GameEvent{T}"/>.</param>
    /// <param name="event2">The second <see cref="GameEvent{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="event1"/> and 
    /// <paramref name="event2"/> are the same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(GameEvent<T> event1, GameEvent<T> event2)
    {
      return event1.Equals(event2);
    }


    /// <summary>
    /// Compares two <see cref="GameEventCollection"/>s to determine whether they are different.
    /// </summary>
    /// <param name="event1">The first <see cref="GameEvent{T}"/>.</param>
    /// <param name="event2">The second <see cref="GameEvent{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="event1"/> and 
    /// <paramref name="event2"/> are different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(GameEvent<T> event1, GameEvent<T> event2)
    {
      return !event1.Equals(event2);
    }
    #endregion
  }
}
