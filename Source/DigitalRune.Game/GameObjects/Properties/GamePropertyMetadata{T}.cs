// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Diagnostics;
using DigitalRune.Collections;


namespace DigitalRune.Game
{
  // Factory method for use in the GamePropertyCollection.
  internal interface IGamePropertyFactory
  {
    IGameProperty CreateGameProperty(GameObject owner);
  }


  /// <summary>
  /// Identifies and describes a game object property.
  /// </summary>
  /// <typeparam name="T">The type of the property value.</typeparam>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public class GamePropertyMetadata<T> : IGamePropertyMetadata, IGamePropertyFactory
  {
    //--------------------------------------------------------------
    #region Static Fields
    //--------------------------------------------------------------

    // The global store for all property metadata for properties of type T.
    internal static NamedObjectCollection<GamePropertyMetadata<T>> Properties { get; private set; }
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


    // A reusable instance of PropertyChangedEventArgs.
    // (PropertyChangedEventArgs.Name can only be set in the constructor, so we cannot use
    // normal resource pooling.)
    internal PropertyChangedEventArgs PropertyChangedEventArgs
    {
      get
      {
        if (_propertyChangedEventArgs == null)
          _propertyChangedEventArgs = new PropertyChangedEventArgs(Name);

        return _propertyChangedEventArgs;
      }
    }
    private PropertyChangedEventArgs _propertyChangedEventArgs;


    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    /// <value>The default value.</value>
    public T DefaultValue { get; set; }


    /// <inheritdoc/>
    object IGamePropertyMetadata.DefaultValue
    {
      get { return DefaultValue; }
      set { DefaultValue = (T)value; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="GamePropertyMetadata{T}"/> class.
    /// </summary>
    static GamePropertyMetadata()
    {
      Properties = new NamedObjectCollection<GamePropertyMetadata<T>>();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GamePropertyMetadata{T}"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="id">The ID.</param>
    /// <remarks>
    /// The property metadata is automatically added to the <see cref="Properties"/> collection!
    /// </remarks>
    internal GamePropertyMetadata(string name, int id)
    {
      Debug.Assert(!string.IsNullOrEmpty(name), "Name must not be null or an empty string.");

      Name = name;
      Id = id;
      Properties.Add(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    IGameProperty IGamePropertyFactory.CreateGameProperty(GameObject owner)
    {
      return new GameProperty<T>(owner, this);
    }
    #endregion
  }
}
