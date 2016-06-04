// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using DigitalRune.Animation;


namespace DigitalRune.Game
{
  /// <summary>
  /// Represents an object of a game.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="GameObject"/>s represent individual items of game logic that are updated once per 
  /// frame. Game objects are also called game components, game entities, controls, actors, etc.
  /// </para>
  /// <para>
  /// Important note: Each game object must be given a name that is unique within the 
  /// <see cref="IGameObjectService"/> or the <see cref="GameObjectCollection"/> in which it will 
  /// be used.
  /// </para>
  /// <para>
  /// <strong>Notes to Inheritors:</strong> Derived classes should override 
  /// <see cref="GameObject.OnUpdate"/>. <see cref="OnUpdate"/> is the place for the logic of the 
  /// game object.
  /// </para>
  /// <para>
  /// <strong>Interface IAnimatableObject:</strong> The game object implements the
  /// interface <see cref="IAnimatableObject"/>. All game object properties can be animated by the 
  /// DigitalRune Animation system! Any game property can be cast into an 
  /// <see cref="IAnimatableProperty{T}"/> by calling <see cref="GameProperty{T}.AsAnimatable"/>.
  /// </para>
  /// <para>
  /// <strong>Interface INotifyPropertyChanged:</strong> The game object implements 
  /// <see cref="INotifyPropertyChanged"/>. For <see cref="GameProperty{T}"/>s this is automatically
  /// handled by the <see cref="GameProperty{T}"/> itself. If derived classes need to raise the 
  /// event for "normal" properties, they have to call
  /// <see cref="OnPropertyChanged(PropertyChangedEventArgs)"/>.
  /// </para>
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public partial class GameObject : INotifyPropertyChanged, IAnimatableObject
  {
    // Notes:
    // OnLoad is not really needed. Loading could be done in the first call of OnUpdate. But
    // OnUnload is needed because the GameObject would otherwise not detect the unloading.
    //
    // In following situation GameProperty.Changing/Changed events are not triggered:
    // For local values that have the default value (HasLocalValue == false), when
    // - the template is changed,
    // - the template value is changed,
    // - the metadata default value is changed.
    // That means, we have following current restriction: Example:
    // GameObject A uses GameObject B as Template: A.Template = B;
    // If a listener handles A.Properties["X"].Changing/Changed, then the listener will not
    // get any events if B.Properties["X"].Value is changed. Until A.Properties["X"].Value is
    // changed, A.Properties["X"].Value will return B.Properties["X"].Value. But 
    // A.Properties["X"].Value will not trigger Changing/Changed even if B.Properties["X"].Value
    // is changed.
    // A practical example: 100 soldiers are instanced using a soldier template. The soldier's
    // shirt color will change with the template's shirt color. But no events are triggered on the
    // soldiers - only on the template. To handle the changing soldier shirt color, handle the
    // template shirt color events directly. Or, connect the properties:
    // template.ShirtColor.Changed += soldier.ShirtColor.Change;


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly PropertyChangedEventArgs IsLoadedChangedEventArgs = new PropertyChangedEventArgs("IsLoaded");
    private static readonly PropertyChangedEventArgs NameChangedEventArgs = new PropertyChangedEventArgs("Name");
    private static readonly PropertyChangedEventArgs TemplateChangedEventArgs = new PropertyChangedEventArgs("Template");

    private static int _lastId = -1;

    private bool _updated; // Indicates whether this object has been updated in the current frame.
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the content of this object was loaded.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the content of this instance is loaded; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This flag is automatically set and reset in <see cref="Load"/> and <see cref="Unload"/>.
    /// </remarks>
    public bool IsLoaded
    {
      get { return _isLoaded; }
      private set
      {
        if (_isLoaded == value)
          return;

        _isLoaded = value;
        OnPropertyChanged(IsLoadedChangedEventArgs);
      }
    }
    private bool _isLoaded;


    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name of the game object.</value>
    /// <remarks>
    /// The name should be unique and must not be changed when the game object is already loaded.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot change name of a game object because it is already loaded.
    /// </exception>
    public string Name
    {
      // Must be set before it is added to a game object service
      get { return _name; }
      set
      {
        if (_name == value)
          return;

        if (IsLoaded)
          throw new InvalidOperationException("Cannot change name of a game object that was already loaded.");

        _name = value;
        OnPropertyChanged(NameChangedEventArgs);
      }
    }
    private string _name;


    /// <summary>
    /// Gets or sets the template.
    /// </summary>
    /// <value>The template. The default is <see langword="null"/>.</value>
    /// <remarks>
    /// <para>
    /// If a template is set, this game object has the same properties and events as the 
    /// template object. If a game object property does not have a local value (see 
    /// <see cref="GameProperty{T}.HasLocalValue"/>), the property value of the template is used
    /// as the default value.
    /// </para>
    /// <para>
    /// A template object itself can also have a template.
    /// </para>
    /// </remarks>
    public GameObject Template
    {
      get { return _template; }
      set
      {
        if (_template == value)
          return;

        _template = value;

        OnTemplateChanged(EventArgs.Empty);
        OnPropertyChanged(TemplateChangedEventArgs);
      }
    }
    private GameObject _template;


    /// <summary>
    /// Occurs when a property value has changed.
    /// </summary>
    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
      add { _notifyPropertyChanged += value; }
      remove { _notifyPropertyChanged -= value; }
    }
    private event PropertyChangedEventHandler _notifyPropertyChanged;


    /// <summary>
    /// Occurs when a property value has changed.
    /// </summary>
    public event EventHandler<GamePropertyEventArgs> PropertyChanged;


    /// <summary>
    /// <see langword="true"/> if event handlers are registered for a <see cref="PropertyChanged"/> 
    /// event.
    /// </summary>
    internal bool NeedToCallPropertyChanged
    {
      get
      {
        return _notifyPropertyChanged != null || PropertyChanged != null;
      }
    }


    /// <summary>
    /// Occurs when the <see cref="Template"/> changed.
    /// </summary>
    public event EventHandler TemplateChanged;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GameObject"/> class.
    /// </summary>
    public GameObject()
      : this(null)
    {
      // Force execution of static constructor which forces the execution of
      // static field initializers. This is necessary to execute all CreateProperty
      // and CreateEvent methods when they are used like this:
      //   public static readonly int FooPropertyId = CreateProperty(...).Id;
      // This is necessary because a .NET runtime can defer the static field 
      // initialization until first use of a static field (e.g. in NETFX_CORE).
      System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(GetType().TypeHandle);

      _name = "GameObject" + (uint)Interlocked.Increment(ref _lastId);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GameObject"/> class.
    /// </summary>
    /// <param name="name">The unique name.</param>
    public GameObject(string name)
    {
      _name = name;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Loads the content of the game object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is automatically called when the game object is added to a 
    /// <see cref="IGameObjectService"/>. 
    /// </para>
    /// <para>
    /// <see cref="OnLoad"/> will be called automatically in this method. 
    /// </para>
    /// </remarks>
    public void Load()
    {
      if (IsLoaded)
        return;

      IsLoaded = true;
      OnLoad();
    }


    /// <summary>
    /// Called when the game object should load its content.
    /// </summary>
    /// <remarks>
    /// This method is automatically called after the game object was added to a 
    /// <see cref="IGameObjectService"/>.
    /// </remarks>
    protected virtual void OnLoad()
    {
    }


    /// <summary>
    /// Unloads the content of the game object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is automatically called when the game object is removed from a 
    /// <see cref="IGameObjectService"/>. 
    /// </para>
    /// <para>
    /// <see cref="OnUnload"/> will be called automatically in this method.
    /// </para>
    /// </remarks>
    public void Unload()
    {
      if (!IsLoaded)
        return;

      IsLoaded = false;
      OnUnload();
    }


    /// <summary>
    /// Called when the game object should unload its content.
    /// </summary>
    /// <remarks>
    /// This method is automatically called before the game object is removed from its
    /// <see cref="IGameObjectService"/>.
    /// </remarks>
    protected virtual void OnUnload()
    {
    }


    /// <summary>
    /// Tells the game object to prepare itself for the next time step.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method has to be called at the beginning of a time step. This is normally done
    /// automatically by the <see cref="IGameObjectService"/>. Do not call this method manually
    /// unless you have to emulate the function of a <see cref="IGameObjectService"/>.
    /// </para>
    /// </remarks>
    public void NewFrame()
    {
      _updated = false;
    }


    /// <summary>
    /// Updates this game object.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last frame.</param>
    /// <remarks>
    /// <para>
    /// Normally, this method is called by the <see cref="IGameObjectService"/> each frame. It can
    /// be called manually, for example, in <see cref="OnUpdate"/> to make sure that another game
    /// object is updated before this game object. - But in general it is not required to call this
    /// method.
    /// </para>
    /// <para>
    /// It is safe to call this method multiple times. It will only be executed once per time step.
    /// </para>
    /// <para>
    /// <see cref="OnUpdate"/> will be called automatically in this method.
    /// </para>
    /// </remarks>
    public void Update(TimeSpan deltaTime)
    {
      if (_updated)
        return;  // Already updated.

      _updated = true;
      OnUpdate(deltaTime);
    }


    /// <summary>
    /// Called when the game object should be updated.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last frame.</param>
    /// <remarks>
    /// This method is automatically called in <see cref="GameObject.Update"/> if the game object
    /// hasn't already been updated in the current time step. 
    /// </remarks>
    protected virtual void OnUpdate(TimeSpan deltaTime)
    {
    }


    /// <overloads>
    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="GamePropertyEventArgs{T}"/> object that provides the arguments for the event.
    /// </param>
    internal void OnPropertyChanged<T>(GamePropertyEventArgs<T> eventArgs)
    {
      var handler = PropertyChanged;

      if (handler != null)
        handler(this, eventArgs);
    }


    /// <summary>
    /// Is called after a game object property was changed.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="gameProperty">The game object property.</param>
    /// <param name="oldValue">The old value.</param>
    /// <param name="newValue">The new value.</param>
    protected internal virtual void OnPropertyChanged<T>(GameProperty<T> gameProperty, T oldValue, T newValue)
    {
      // This method does not use GamePropertyEventArgs<T> to avoid creation/recycling of this
      // data structure when it is not really needed. OnPropertyChanged<T> should be called for
      // all property changes. But the creation of GamePropertyEventArgs<T> is only needed if
      // someone has attached to the PropertyChanged events.
    }


    /// <summary>
    /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="PropertyChangedEventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnPropertyChanged"/> in a 
    /// derived class, be sure to call the base class's <see cref="OnPropertyChanged"/> method so 
    /// that registered delegates receive the event.
    /// </remarks>
    protected internal virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
    {
      PropertyChangedEventHandler handler = _notifyPropertyChanged;

      if (handler != null)
        handler(this, eventArgs);
    }


    /// <summary>
    /// Raises the <see cref="TemplateChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnTemplateChanged"/> in a 
    /// derived class, be sure to call the base class's <see cref="OnTemplateChanged"/> method so 
    /// that registered delegates receive the event.
    /// </remarks>
    protected virtual void OnTemplateChanged(EventArgs eventArgs)
    {
      EventHandler handler = TemplateChanged;

      if (handler != null)
        handler(this, eventArgs);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IEnumerable<IAnimatableProperty> IAnimatableObject.GetAnimatedProperties()
    {
      var count = _propertyData.Count;
      for (int i = 0; i < count; i++)
      {
        var property = _propertyData.GetByIndex(i) as IAnimatableProperty;
        if (property != null && property.IsAnimated)
          yield return property;
      }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IAnimatableProperty<T> IAnimatableObject.GetAnimatableProperty<T>(string name)
    {
      GamePropertyMetadata<T> metadata;
      if (!GamePropertyMetadata<T>.Properties.TryGet(name, out metadata))
        return null;

      return new GameProperty<T>(this, metadata).AsAnimatable();
    }
    #endregion
  }
}
