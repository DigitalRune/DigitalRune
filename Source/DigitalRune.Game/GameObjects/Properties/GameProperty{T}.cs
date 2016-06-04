// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Animation;


namespace DigitalRune.Game
{
  /// <summary>
  /// Represents a property of a <see cref="GameObject"/>
  /// </summary>
  /// <typeparam name="T">The type of the property value.</typeparam>
  /// <remarks>
  /// <strong>Animation:</strong> Game objects and their game object properties can be animated 
  /// using the DigitalRune Animation system. In order to animate a specific property
  /// <see cref="AsAnimatable"/> needs to be called. This method returns an 
  /// <see cref="IAnimatableProperty{T}"/> for the given property which can be passed to the 
  /// animation system.
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name}, HasLocalValue = {HasLocalValue}, Value = {Value})")]
  public struct GameProperty<T> : IGameProperty, IEquatable<GameProperty<T>>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly GameObject _owner;
    private readonly GamePropertyMetadata<T> _metadata;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public GameObject Owner
    {
      get { return _owner; }
    }


    /// <summary>
    /// Gets the property metadata.
    /// </summary>
    /// <value>The property metadata.</value>
    public GamePropertyMetadata<T> Metadata
    {
      get { return _metadata; }
    }


    /// <inheritdoc/>
    IGamePropertyMetadata IGameProperty.Metadata
    {
      get { return _metadata; }
    }


    /// <inheritdoc/>
    public string Name { get { return _metadata.Name; } }


    /// <inheritdoc/>
    public bool HasLocalValue
    {
      get
      {
        IGamePropertyData data = Owner.PropertyData.Get(_metadata.Id);
        return data != null && data.HasLocalValue;
      }
    }


    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    /// <value>The value.</value>
    public T Value
    {
      get
      {
        IGamePropertyData data = Owner.PropertyData.Get(_metadata.Id);

        // Animatable data handles getValue itself.
        var animatableData = data as AnimatableGamePropertyData<T>;
        if (animatableData != null)
          return animatableData.Value;

        // Local value
        if (data != null && data.HasLocalValue)
          return ((GamePropertyData<T>)data).Value;

        // Template value
        if (Owner.Template != null)
          return new GameProperty<T>(Owner.Template, _metadata).Value;

        // Metadata default value
        return _metadata.DefaultValue;
      }
      set
      {
        IGamePropertyData untypedData = Owner.PropertyData.Get(_metadata.Id);
        if (untypedData != null)
        {
          // We have found data.

          // Animatable data handles setValue itself.
          var animatableData = untypedData as AnimatableGamePropertyData<T>;
          if (animatableData != null)
          {
            animatableData.Value = value;
            return;
          }
          
          var data = ((GamePropertyData<T>)untypedData);

          // Remember old value (= local value, template value or default value).
          T oldValue;
          if (data.HasLocalValue)
            oldValue = data.Value;
          else if (Owner.Template != null)
            oldValue = new GameProperty<T>(Owner.Template, _metadata).Value;
          else
            oldValue = _metadata.DefaultValue;

          T newValue = value;

          // Now we have a local value, even if the new value is the same as the default value!
          data.HasLocalValue = true;
          data.Value = oldValue;

          // Skip events if oldValue == newValue.
          if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
            return;

          // Raise Changing event.
          data.OnChanging(this, oldValue, ref newValue);

          // Raise Changed event if value has changed.
          if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
          {
            // Set coerced value.
            data.Value = newValue;
            data.OnChanged(this, oldValue, newValue);
          }
        }
        else
        {
          // ----- No local data yet.

          // Remember old value (= template value or default value).
          T oldValue;
          if (Owner.Template != null)
            oldValue = new GameProperty<T>(Owner.Template, _metadata).Value;
          else
            oldValue = _metadata.DefaultValue;

          var newValue = value;

          var data = new GamePropertyData<T>();

          // Raise Changing event.
          if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            data.OnChanging(this, oldValue, ref newValue);

          // Now we have a local value, even if the new value is the same as the default value!
          data.HasLocalValue = true;
          data.Value = newValue;
          Owner.PropertyData.Set(_metadata.Id, data);

          // Raise Changed event if value has changed.
          if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
            data.OnChanged(this, oldValue, newValue);
        }
      }
    }


    /// <inheritdoc/>
    object IGameProperty.Value
    {
      get { return Value; }
      set { Value = (T)value; }
    }


    /// <summary>
    /// Event handler that automatically changes the value of the property.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This event handler can be used to connect the property to the property of another game 
    /// object. It can, for example, be used to connect the <see cref="GameProperty{T}.Changed"/>
    /// event of a <see cref="GameProperty{T}"/> to this event.
    /// </para>
    /// <code lang="csharp">
    /// <![CDATA[
    /// GameObject.CreateProperty<Vector3F>(
    ///   "Position", 
    ///   GamePropertyCategories.Common, 
    ///   "Defines the 3D position.", 
    ///   new Vector3F());
    /// 
    /// var gameObject1 = new GameObject();
    /// var gameObject2 = new GameObject();
    /// 
    /// var property1 = gameObject1.Properties.Get<Vector3F>("Position");
    /// var property2 = gameObject2.Properties.Get<Vector3F>("Position");
    /// property1.Changed += property2.Change;
    /// ]]>
    /// </code>
    /// <para>
    /// Now, whenever the position of the first game object changes the position of the second game 
    /// object will be updated and set to the same value.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public EventHandler<GamePropertyEventArgs<T>> Change
    {
      get
      {
        var eventHandler = Owner.ChangeEventHandlers.Get(_metadata.Id) as EventHandler<GamePropertyEventArgs<T>>;
        if (eventHandler == null)
        {
          eventHandler = new GamePropertyChangeHandler<T>(this).Change;
          Owner.ChangeEventHandlers.Set(_metadata.Id, eventHandler);
        }

        return eventHandler;
      }
    }


    /// <summary>
    /// Occurs when the <see cref="GameProperty{T}.Value"/> is about to change.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <i>sender</i> of the event will be the <see cref="GameObject"/> that owns this
    /// <see cref="GameProperty{T}"/>.
    /// </para>
    /// <para>
    /// This event can be used to coerce the new property value. See 
    /// <see cref="GamePropertyEventArgs{T}"/> for more information.
    /// </para>
    /// <para>
    /// Event handlers should not change the value of the property, as this would cause a new 
    /// <see cref="Changing"/> event to be raised.
    /// </para>
    /// </remarks>
    public event EventHandler<GamePropertyEventArgs<T>> Changing
    {
      add
      {
        GamePropertyData<T> data = GetOrCreateLocalData();
        data.Changing += value;
      }
      remove
      {
        GamePropertyData<T> data = GetOrCreateLocalData();
        data.Changing -= value;
      }
    }


    /// <summary>
    /// Occurs when the <see cref="GameProperty{T}.Value"/> changed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <i>sender</i> of the event will be the <see cref="GameObject"/> that owns this
    /// <see cref="GameProperty{T}"/>.
    /// </para>
    /// <para>
    /// Event handlers should not change the value of the property, as this would cause new 
    /// <see cref="Changing"/> and <see cref="Changed"/> events to be raised.
    /// </para>
    /// </remarks>
    public event EventHandler<GamePropertyEventArgs<T>> Changed
    {
      add
      {
        GamePropertyData<T> data = GetOrCreateLocalData();
        data.Changed += value;
      }
      remove
      {
        GamePropertyData<T> data = GetOrCreateLocalData();
        data.Changed -= value;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GameProperty{T}"/> struct.
    /// </summary>
    /// <param name="owner">The game object that owns this property.</param>
    /// <param name="metadata">The metadata.</param>
    internal GameProperty(GameObject owner, GamePropertyMetadata<T> metadata)
    {
      Debug.Assert(owner != null, "owner must not be null.");
      Debug.Assert(metadata != null, "metadata must not be null.");

      _owner = owner;
      _metadata = metadata;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private GamePropertyData<T> GetOrCreateLocalData()
    {
      IGamePropertyData untypedData = Owner.PropertyData.Get(_metadata.Id);
      GamePropertyData<T> data;
      if (untypedData != null)
      {
        // data found!
        data = (GamePropertyData<T>)untypedData;
      }
      else
      {
        // No data found! Create new data.
        data = new GamePropertyData<T>();
        Owner.PropertyData.Set(_metadata.Id, data);
      }
      return data;
    }


    /// <inheritdoc/>
    public void Parse(string value)
    {
      Value = ObjectHelper.Parse<T>(value);
    }


    /// <inheritdoc/>
    public void Reset()
    {
      // Get local data.
      IGamePropertyData untypedData = Owner.PropertyData.Get(_metadata.Id);

      // Nothing to do if we have no local data.
      if (untypedData == null)
        return;

      // Nothing to do if the local data uses the default value.
      if (!untypedData.HasLocalValue)
        return;

      // Assert: We have a local value and must remove it.

      var data = ((GamePropertyData<T>)untypedData);

      // If the value is animated, changing the BaseValue does not cause events.
      var animatableData = untypedData as AnimatableGamePropertyData<T>;

      // Remember current value.
      T oldValue;
      bool isAnimated;
      if (animatableData != null)
      {
        oldValue = animatableData.BaseValue;
        isAnimated = animatableData.IsAnimated;
      }
      else
      {
        oldValue = data.Value;
        isAnimated = false;
      }

      // Get the target default value from template or metadata.
      T defaultValue;
      if (Owner.Template != null)
        defaultValue = new GameProperty<T>(Owner.Template, _metadata).Value;
      else
        defaultValue = _metadata.DefaultValue;
      
      if (isAnimated || EqualityComparer<T>.Default.Equals(oldValue, defaultValue))
      {
        // oldValue and defaultValue are the same. We only have to reset the flag.
        data.HasLocalValue = false;
        data.Value = default(T);
        return;
      }

      var newValue = defaultValue;

      // Raise Changing event.
      data.OnChanging(this, oldValue, ref newValue);

      if (EqualityComparer<T>.Default.Equals(defaultValue, newValue))
      {
        // After coercion in Changing, the target value is still the default value.

        // Value has been reset.
        data.HasLocalValue = false;
        data.Value = default(T);

        // Raise Changed event if oldValue is different from the newValue.
        if (!EqualityComparer<T>.Default.Equals(oldValue, defaultValue))
          data.OnChanged(this, oldValue, defaultValue);
      }
      else
      {
        // Value was overwritten by Changing event handler! - We still have a local value!
        // Raise Changed event if oldValue is different from the newValue.
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
          data.Value = newValue;
          data.OnChanged(this, oldValue, newValue);
        }
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
      return obj is GameProperty<T> && Equals((GameProperty<T>)obj);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other"/> 
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(GameProperty<T> other)
    {
      if (_owner != other._owner)
        return false;

      return _metadata == other._metadata;
    }


    /// <summary>
    /// Compares two <see cref="GameProperty{T}"/>s to determine whether they are the same.
    /// </summary>
    /// <param name="property1">The first <see cref="GameProperty{T}"/>.</param>
    /// <param name="property2">The second <see cref="GameProperty{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="property1"/> and 
    /// <paramref name="property2"/> are the same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(GameProperty<T> property1, GameProperty<T> property2)
    {
      return property1.Equals(property2);
    }


    /// <summary>
    /// Compares two <see cref="GameProperty{T}"/>s to determine whether they are different.
    /// </summary>
    /// <param name="property1">The first <see cref="GameProperty{T}"/>.</param>
    /// <param name="property2">The second <see cref="GameProperty{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="property1"/> and 
    /// <paramref name="property2"/> are different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(GameProperty<T> property1, GameProperty<T> property2)
    {
      return !property1.Equals(property2);
    }


    /// <summary>
    /// Returns the value of the specified <see cref="GameProperty{T}"/>.
    /// </summary>
    /// <param name="property">The game object property.</param>
    /// <returns>The value of the game object property.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Users can simply read Value instead.")]
    public static explicit operator T(GameProperty<T> property)
    {
      return property.Value;
    }


    /// <summary>
    /// Returns an <see cref="IAnimatableProperty{T}"/> that can be used to animate this
    /// <see cref="GameProperty{T}"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="IAnimatableProperty{T}"/> instance that animates this game object property.
    /// </returns>
    /// <remarks>
    /// If this method is called more than once on the same game object property, it always returns
    /// the same <see cref="IAnimatableProperty{T}"/> instance and the
    /// <see cref="IAnimatableProperty{T}"/> instances are kept alive by the game object. Only if
    /// the game object property is removed from a game object (using e.g. 
    /// <see cref="GamePropertyCollection.Remove(int)"/>), the <see cref="IAnimatableProperty{T}"/> 
    /// instance is removed too, and the next call of <see cref="AsAnimatable"/> will return a new 
    /// <see cref="IAnimatableProperty{T}"/> instance.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public IAnimatableProperty<T> AsAnimatable()
    {
      // Try to get existing animatable.
      IGamePropertyData untypedData = Owner.PropertyData.Get(_metadata.Id);
      AnimatableGamePropertyData<T> animatableData = untypedData as AnimatableGamePropertyData<T>;

      if (animatableData != null)
        return animatableData;

      // Create new animatable data.
      animatableData = new AnimatableGamePropertyData<T>(_owner, _metadata, untypedData as GamePropertyData<T>);
      Owner.PropertyData.Set(_metadata.Id, animatableData);
      return animatableData;
    }
    #endregion
  }
}
