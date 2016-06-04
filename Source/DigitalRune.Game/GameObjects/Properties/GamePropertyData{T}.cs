// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game
{
  /// <summary>
  /// Stores the value of a game property.
  /// </summary>
  /// <typeparam name="T">The type of the property value.</typeparam>
  internal class GamePropertyData<T> : IGamePropertyData
  {
    //--------------------------------------------------------------
    #region Fields, Properties and Events
    //--------------------------------------------------------------

    // ----- The actual data that is stored for a property instance:

    public virtual T Value
    {
      get { return _value; }
      set { _value = value; }
    }
    private T _value;


    public virtual bool HasLocalValue
    {
      get { return _hasLocalValue; }
      set { _hasLocalValue = value; }
    }
    private bool _hasLocalValue;


    public event EventHandler<GamePropertyEventArgs<T>> Changing;
    public event EventHandler<GamePropertyEventArgs<T>> Changed;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public GamePropertyData()
    {
    }


    // Copy existing data.
    protected GamePropertyData(GamePropertyData<T> data)
    {
      if (data != null)
      {
        _value = data.Value;
        _hasLocalValue = data.HasLocalValue;
        Changing = data.Changing;
        Changed = data.Changed;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Factory method of IGamePropertyData.
    IGameProperty IGamePropertyData.CreateGameProperty(GameObject owner, int propertyId)
    {
      var metadata = (GamePropertyMetadata<T>)GameObject.PropertyMetadata[propertyId];
      return new GameProperty<T>(owner, metadata);
    }


    // Raises the Changing event.
    internal void OnChanging(GameProperty<T> property, T oldValue, ref T newValue)
    {
      var handler = Changing;

      if (handler != null)
      {
        // Get args from resource pool.
        var args = GamePropertyEventArgs<T>.Create(property, oldValue, newValue);

        // Call event handlers.
        handler(property.Owner, args);

        // Changing event handlers can coerce the value. Return coerced value to caller.
        newValue = args.CoercedValue;

        args.Recycle();
      }
    }


    // Raises the GameProperty.Changed and GameObject.PropertyChanged events.
    internal void OnChanged(GameProperty<T> property, T oldValue, T newValue)
    {
      var handler = Changed;
      var gameObject = property.Owner;

      if (handler != null || gameObject.NeedToCallPropertyChanged)
      {
        // GameProperty.Changed or GameOwner.PropertyChanged must be called because event handlers
        // are registered.

        // Get args from resource pool
        var args = GamePropertyEventArgs<T>.Create(property, oldValue, newValue);

        // Call GameProperty.Changed event handlers.
        if (handler != null)
          handler(gameObject, args);

        // Call the virtual OnPropertyChanged method of the GameObject.
        gameObject.OnPropertyChanged(property, oldValue, newValue);

        // Call GameObject to raise GameObject.PropertyChanged event.
        gameObject.OnPropertyChanged(args);
        gameObject.OnPropertyChanged(property.Metadata.PropertyChangedEventArgs);

        args.Recycle();
      }
      else
      {
        // Call the virtual OnPropertyChanged method of the GameObject. 
        // (Derived classes can override this method.)
        gameObject.OnPropertyChanged(property, oldValue, newValue);
      }
    }
    #endregion
  }
}
