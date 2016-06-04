// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game
{
  /// <summary>
  /// Provides data for the <see cref="GameProperty{T}.Changing"/> and the 
  /// <see cref="GameProperty{T}.Changed"/> event of a game object property.
  /// </summary>
  /// <typeparam name="T">The type of the game object property value.</typeparam>
  /// <remarks>
  /// <para>
  /// These type of event arguments are passed to event handlers when the value of a game object 
  /// property changes. This class stores the <see cref="OldValue"/> and the <see cref="NewValue"/> 
  /// of the property. Event handlers of the <see cref="GameProperty{T}.Changing"/> can set the 
  /// <see cref="CoercedValue"/> to coerce the property to a corrected value.
  /// <see cref="CoercedValue"/> is not used in <see cref="GameProperty{T}.Changed"/> events.
  /// </para>
  /// <para>
  /// In detail: When the user calls a <strong>SetValue</strong> method of a 
  /// <see cref="GameObject"/> the <see cref="GameProperty{T}.Changing"/> event is raised (unless
  /// the new value is the same as the current property value). The <see cref="OldValue"/> of the
  /// event arguments is set to the current property value. <see cref="NewValue"/> and 
  /// <see cref="CoercedValue"/> are set to the value that was specified in 
  /// <strong>SetValue</strong>. The <see cref="GameProperty{T}.Changing"/> event handlers can
  /// change the <see cref="CoercedValue"/>. After all <see cref="GameProperty{T}.Changing"/> event
  /// handlers have been called the property value is set to the <see cref="CoercedValue"/>. After
  /// that, if the <see cref="CoercedValue"/> is different from the <see cref="OldValue"/>, the 
  /// <see cref="GameProperty{T}.Changed"/> event is raised. In the event arguments the 
  /// <see cref="OldValue"/> is the same as in the <see cref="GameProperty{T}.Changing"/> event. 
  /// <see cref="NewValue"/> is set to the new property value (which is the coerced value). 
  /// <see cref="CoercedValue"/> is not used in <see cref="GameProperty{T}.Changed"/> events.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> Event handlers of the <see cref="GameProperty{T}.Changing"/> and 
  /// <see cref="GameProperty{T}.Changed"/> events should not directly change the game object
  /// property using a <strong>SetValue</strong> method. This can result in unexpected behavior. The
  /// <see cref="CoercedValue"/> property of this event arguments can be used instead.
  /// </para>
  /// </remarks>
  public class GamePropertyEventArgs<T> : GamePropertyEventArgs
  {
    // Property OldValue:
    // The oldValue is not correct if an event handler changes the property. This can lead to a 
    // case were the oldValue is not the oldValue that an event handler has previously seen.


    //--------------------------------------------------------------
    #region Static Fields
    //--------------------------------------------------------------

    // ReSharper disable StaticFieldInGenericType
    private static readonly ResourcePool<GamePropertyEventArgs<T>> Pool = new ResourcePool<GamePropertyEventArgs<T>>(
      () => new GamePropertyEventArgs<T>(),      // Create
      null,                                      // Initialize
      null);                                     // Uninitialize
    // ReSharper restore StaticFieldInGenericType
    #endregion
    

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion
      
      
    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the game object property.
    /// </summary>
    /// <value>The game object property.</value>
    public new GameProperty<T> Property { get; private set; }


    internal override IGameProperty UntypedProperty
    {
      get { return Property; }
    }


    /// <summary>
    /// Gets the old value of the game object property.
    /// </summary>
    /// <value>
    /// The value that the game object property had before the 
    /// <see cref="GameProperty{T}.Changing"/> and the <see cref="GameProperty{T}.Changed"/> were
    /// raised.
    /// </value>
    public T OldValue { get; private set; }


    /// <summary>
    /// Gets the new value of the game object property.
    /// </summary>
    /// <value>
    /// The new value that should be stored in the game object property.
    /// </value>
    public T NewValue { get; private set; }


    /// <summary>
    /// Gets or sets the coerced value.
    /// </summary>
    /// <value> The coerced value.</value>
    public T CoercedValue { get; set; }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    // Constructor is private to force use of resource pooling!
    private GamePropertyEventArgs()
    {
    }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Creates a new instance using resource pooling.
    /// </summary>
    /// <param name="property">The property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    /// <returns>
    /// An <see cref="GamePropertyEventArgs{T}"/> instance.
    /// </returns>
    /// <remarks>
    /// This method tries to obtain a free instance from a resource pool. If no instance is
    /// available, a new instance is allocated on the heap. The caller of this method should call
    /// <see cref="Recycle"/> when the instance is no longer needed.
    /// </remarks>
    internal static GamePropertyEventArgs<T> Create(GameProperty<T> property, T oldValue, T newValue)
    {
      var args = Pool.Obtain();
      args.Property = property;
      args.OldValue = oldValue;
      args.NewValue = newValue;

      // Coerced value is initially equal to the new value.
      args.CoercedValue = newValue;
      return args;
    }


    /// <summary>
    /// Recycles the specified instance using resource pooling.
    /// </summary>
    /// <remarks>
    /// This method returns the instance to a resource pool from which it can later be obtained 
    /// again using <see cref="Create"/>.
    /// </remarks>
    internal void Recycle()
    {
      Property = new GameProperty<T>();
      OldValue = default(T);
      NewValue = default(T);
      CoercedValue = default(T);
      Pool.Recycle(this);
    }
    #endregion
  }
}
