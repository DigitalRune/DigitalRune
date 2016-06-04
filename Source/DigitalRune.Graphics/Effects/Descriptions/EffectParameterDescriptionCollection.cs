// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Stores an <see cref="EffectParameterDescription"/> for all parameters of an effect.
  /// </summary>
  /// <remarks>
  /// The <see cref="EffectParameterDescriptionCollection"/> is a read-only collection! Attempts to 
  /// manipulate the collection will cause exceptions.
  /// </remarks>
  public sealed class EffectParameterDescriptionCollection : KeyedCollection<EffectParameter, EffectParameterDescription>
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
    /// Gets the internal <see cref="List{T}" /> ot the <see cref="Collection{T}" />.
    /// </summary>
    private new List<EffectParameterDescription> Items
    {
      get { return (List<EffectParameterDescription>)base.Items; }
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
    internal EffectParameterDescriptionCollection(IGraphicsService graphicsService, Effect effect)
      : base(null, 4)
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
      // Interpret parameters.
      foreach (var parameter in effect.Parameters)
        InterpretParameter(effect, parameter, graphicsService.EffectInterpreters);

      // The indices might be unassigned or messed up.
      ValidateIndices(effect);

      // Make collection read-only!
      _isReadOnly = true;
    }


    /// <summary>
    /// Interprets the specified effect parameter and adds an 
    /// <see cref="EffectParameterDescription"/> to the collection.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="interpreters">The effect interpreters.</param>
    private void InterpretParameter(Effect effect, EffectParameter parameter, EffectInterpreterCollection interpreters)
    {
      Debug.Assert(!Contains(parameter), "Effect binding already contains a description for the given effect parameter.");

      // Try all interpreters to find a description.
      foreach (var interpreter in interpreters)
      {
        var description = interpreter.GetDescription(effect, parameter);
        if (description != null)
        {
          Add(description);
          return;
        }
      }

      // No description found. If the parameter is a struct or array of structs
      // we check the structure members.
      if (parameter.ParameterClass == EffectParameterClass.Struct)
      {
        if (parameter.Elements.Count > 0)
        {
          // Effect parameter is an array of structs. 
          foreach (EffectParameter element in parameter.Elements)
            foreach (EffectParameter member in element.StructureMembers)
              InterpretParameter(effect, member, interpreters);
        }
        else
        {
          // Effect parameter is a struct. 
          foreach (EffectParameter member in parameter.StructureMembers)
            InterpretParameter(effect, member, interpreters);
        }
      }
      else
      {
        // No description found and this parameter is no struct.
        // Try to get hint from annotations and create default description (= "Unknown usage").
        var hint = EffectHelper.GetHintFromAnnotations(parameter) ?? EffectParameterHint.Material;
        Add(new EffectParameterDescription(parameter, null, 0, hint));
      }
    }


    /// <summary>
    /// Validates the indices of all effect parameters.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <remarks>
    /// <see cref="ValidateIndices"/> ensures that all the indices (see 
    /// <see cref="EffectParameterDescription.Index"/>) are set correctly (no duplicate indices, all 
    /// indices 0 or positive). Indices with a value of -1 are automatically set to the next free 
    /// index.
    /// </remarks>
    internal void ValidateIndices(Effect effect)
    {
      // Check for missing usage indices.
      foreach (var description in Items)
      {
        if (description == null || description.Semantic == null)
          continue;

        if (description.Index == -1)
        {
          // The index is not set.
          // --> Assign next available number.
          description.Index = GetNextIndex(description.Semantic, description.Parameter.ParameterType);
        }
      }

      // Check for duplicate indices.
      foreach (var description in Items)
      {
        if (description == null || description.Semantic == null)
          continue;

        foreach (var otherDescription in Items)
        {
          if (description == otherDescription)
            continue;

          if (description.Semantic == otherDescription.Semantic
              && description.Index == otherDescription.Index
              && description.Parameter.ParameterType == otherDescription.Parameter.ParameterType)
          {
            string message = String.Format(CultureInfo.InvariantCulture, "Cannot load effect (.fx). Duplicate effect parameter description (parameter = \"{0}\", semantic = \"{1}\").", description.Parameter.Name, description.Semantic);
            throw new EffectBindingException(message, effect, description.Parameter);
          }
        }
      }
    }


    private int GetNextIndex(string semantic, EffectParameterType parameterType)
    {
      int nextIndex = 0;
      while (IsIndexAlreadyUsed(semantic, nextIndex, parameterType))
        nextIndex++;

      return nextIndex;
    }


    private bool IsIndexAlreadyUsed(string semantic, int index, EffectParameterType parameterType)
    {
      foreach (var otherDescription in Items)
      {
        if (otherDescription == null
            || otherDescription.Semantic != semantic
            || otherDescription.Index == -1
            || otherDescription.Parameter.ParameterType != parameterType)
        {
          // Wrong semantic, index not set, or incompatible parameter.
          // --> Skip binding.
          continue;
        }

        if (otherDescription.Parameter.Elements.Count > 0)
        {
          // Effect parameter is an array, hence it has multiple indices.
          int startIndex = otherDescription.Index;
          int nextIndex = startIndex + otherDescription.Parameter.Elements.Count;
          if (startIndex <= index && index < nextIndex)
            return true;
        }
        else
        {
          if (otherDescription.Index == index)
            return true;
        }
      }

      return false;
    }


    /// <summary>
    /// Gets the description for the specified effect parameter.
    /// </summary>
    /// <param name="parameter">The effect parameter.</param>
    /// <param name="description">
    /// The description for the specified effect parameter, if the parameter is found; otherwise, 
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the collection contains a description for the effect parameter; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    public bool TryGet(EffectParameter parameter, out EffectParameterDescription description)
    {
      if (parameter == null)
        throw new ArgumentNullException("parameter");

      if (Dictionary != null)
        return Dictionary.TryGetValue(parameter, out description);

      // Linear search.
      foreach (var item in Items)
      {
        if (parameter == item.Parameter)
        {
          description = item;
          return true;
        }
      }

      description = null;
      return false;
    }


    /// <summary>
    /// When implemented in a derived class, extracts the key from the specified element.
    /// </summary>
    /// <param name="item">The element from which to extract the key.</param>
    /// <returns>The key for the specified element.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override EffectParameter GetKeyForItem(EffectParameterDescription item)
    {
      return item.Parameter;
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="EffectParameterDescriptionCollection"/>.
    /// </returns>
    public new List<EffectParameterDescription>.Enumerator GetEnumerator()
    {
      return Items.GetEnumerator();
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
    protected override void InsertItem(int index, EffectParameterDescription item)
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
    protected override void SetItem(int index, EffectParameterDescription item)
    {
      ThrowIfReadOnly();
      base.SetItem(index, item);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void ThrowIfReadOnly()
    {
      if (_isReadOnly)
        throw new NotSupportedException("The EffectParameterDescriptionCollection is read-only.");
    }
    #endregion
  }
}
