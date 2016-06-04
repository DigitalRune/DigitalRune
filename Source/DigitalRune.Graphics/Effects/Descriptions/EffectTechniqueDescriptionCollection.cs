// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Stores an <see cref="EffectTechniqueDescription"/> for all techniques of an effect.
  /// </summary>
  /// <remarks>
  /// The <see cref="EffectTechniqueDescriptionCollection"/> is a read-only collection! Attempts to 
  /// manipulate the collection will cause exceptions.
  /// </remarks>
  public sealed class EffectTechniqueDescriptionCollection : Collection<EffectTechniqueDescription>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isReadOnly;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="EffectTechniqueDescription"/> for the specified technique.
    /// </summary>
    /// <value>
    /// The <see cref="EffectTechniqueDescription"/>, or <see langword="null"/> if the specified
    /// technique is not found in the collection.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
    public EffectTechniqueDescription this[EffectTechnique technique]
    {
      get
      {
        int numberOfTechniques = Items.Count;
        for (int i = 0; i < numberOfTechniques; i++)
        {
          var description = Items[i];
          if (description.Technique == technique)
            return description;
        }

        return null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterDescriptionCollection"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="effect">The effect.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> or <paramref name="effect"/> is <see langword="null"/>.
    /// </exception>
    internal EffectTechniqueDescriptionCollection(IGraphicsService graphicsService, Effect effect)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");
      if (effect == null)
        throw new ArgumentNullException("effect");

      Initialize(graphicsService, effect);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes the <see cref="EffectParameterDescriptionCollection"/> for the specified effect.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <param name="effect">The effect.</param>
    private void Initialize(IGraphicsService graphicsService, Effect effect)
    {
      foreach (var technique in effect.Techniques)
        InterpretTechnique(effect, technique, graphicsService.EffectInterpreters);

      // Make collection read-only!
      _isReadOnly = true;
    }


    /// <summary>
    /// Interprets the specified effect technique and adds an 
    /// <see cref="EffectTechniqueDescription"/> to the collection.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="technique">The technique.</param>
    /// <param name="interpreters">The effect interpreters.</param>
    private void InterpretTechnique(Effect effect, EffectTechnique technique, EffectInterpreterCollection interpreters)
    {
      foreach (var interpreter in interpreters)
      {
        var description = interpreter.GetDescription(effect, technique);
        if (description != null)
        {
          Add(description);
          return;
        }
      }

      // No description found. Set default description.
      Add(new EffectTechniqueDescription(effect, technique));
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="EffectTechniqueDescriptionCollection"/>.
    /// </returns>
    public new List<EffectTechniqueDescription>.Enumerator GetEnumerator()
    {
      return ((List<EffectTechniqueDescription>)Items).GetEnumerator();
    }


    /// <summary>
    /// Removes all elements from the collection.
    /// </summary>
    protected override void ClearItems()
    {
      ThrowIfReadOnly();
      base.ClearItems();
    }


    /// <summary>
    /// Inserts an element into the collection at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">The object to insert.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void InsertItem(int index, EffectTechniqueDescription item)
    {
      ThrowIfReadOnly();
      base.InsertItem(index, item);
    }


    /// <summary>
    /// Removes the element at the specified index of the collection.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero or <paramref name="index"/> is equal to or 
    /// greater than <see cref="Collection{T}.Count"/>.
    /// </exception>
    protected override void RemoveItem(int index)
    {
      ThrowIfReadOnly();
      base.RemoveItem(index);
    }


    /// <summary>
    /// Replaces the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="item">The new value for the element at the specified index.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void SetItem(int index, EffectTechniqueDescription item)
    {
      ThrowIfReadOnly();
      base.SetItem(index, item);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void ThrowIfReadOnly()
    {
      if (_isReadOnly)
        throw new NotSupportedException("The EffectTechniqueDescriptionCollection is read-only.");
    }
    #endregion
  }
}
