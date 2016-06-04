// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Manages a collection of <see cref="EffectParameterBinding"/>s.
  /// </summary>
  public class EffectParameterBindingCollection : Collection<EffectParameterBinding>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating which effect parameters can be added to this collection.
    /// </summary>
    /// <value>
    /// A bitwise combination of <see cref="EffectParameterHint"/> values. The value defines which
    /// parameter bindings can be added to the collection.
    /// </value>
    public EffectParameterHint Hints { get; private set; }


    /// <overloads>
    /// <summary>
    /// Gets the <see cref="EffectParameterBinding"/> of a specific effect parameter.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the <see cref="EffectParameterBinding"/> for the specified effect parameter.
    /// </summary>
    /// <param name="parameter"> The effect parameter.</param>
    /// <value>
    /// The <see cref="EffectParameterBinding"/> for the specified effect parameter. Or 
    /// <see langword="null"/> if no matching <see cref="EffectParameterBinding"/> is found.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
    public EffectParameterBinding this[EffectParameter parameter]
    {
      get
      {
        int index = IndexOf(parameter);
        if (index == -1)
          return null;

        return Items[index];
      }
    }


    /// <summary>
    /// Gets the <see cref="EffectParameterBinding"/> for the effect parameter with the specified 
    /// name.
    /// </summary>
    /// <param name="name">The name of the effect parameter.</param>
    /// <value>
    /// The <see cref="EffectParameterBinding"/> for the effect parameter the specified name. Or 
    /// <see langword="null"/> if no matching <see cref="EffectParameterBinding"/> is found.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    public EffectParameterBinding this[string name]
    {
      get
      {
        int index = IndexOf(name);
        if (index == -1)
          return null;

        return Items[index];
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectParameterBindingCollection"/> class.
    /// </summary>
    /// <param name="hints">
    /// A bitwise combination of <see cref="EffectParameterHint"/> values. The value defines which
    /// parameter bindings can be added to the collection.
    /// </param>
    internal EffectParameterBindingCollection(EffectParameterHint hints)
    {
      Hints = hints;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through the 
    /// <see cref="EffectParameterBindingCollection"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="EffectParameterBindingCollection"/>.
    /// </returns>
    public new List<EffectParameterBinding>.Enumerator GetEnumerator()
    {
      return ((List<EffectParameterBinding>)Items).GetEnumerator();
    }


    // ReSharper disable UnusedParameter.Local
    private void CheckHint(EffectParameterBinding binding)
    {
      if ((Hints & binding.Description.Hint) == 0)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Cannot add EffectParameterBinding to collection. The collection does not supported EffectParameters that have the sort hint \"{0}\".",
          binding.Description.Hint);
        throw new ArgumentException(message);
      }
    }
    // ReSharper restore UnusedParameter.Local


    /// <summary>
    /// Inserts the <see cref="EffectParameterBinding"/> at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="item">The new effect parameter binding.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> does not belong to the same <see cref="Effect"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// An <see cref="EffectParameterBinding"/> for the same <see cref="EffectParameter"/> already
    /// exists.
    /// </exception>
    protected override void InsertItem(int index, EffectParameterBinding item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "An EffectParameterBinding in an EffectParameterBindingCollection must not be null.");

      CheckHint(item);

      if (Contains(item.Parameter))
      {
        string message = string.Format(CultureInfo.InvariantCulture, "An EffectParameterBinding for the given EffectParameter \"{0}\" already exists.", item.Parameter.Name);
        throw new ArgumentException(message);
      }

      base.InsertItem(index, item);
    }


    /// <summary>
    /// Sets the <see cref="EffectParameterBinding"/> at the specified index.
    /// </summary>
    /// <param name="index">The index of the effect parameter binding.</param>
    /// <param name="item">The new effect parameter binding.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> does not belong to the same <see cref="Effect"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// An <see cref="EffectParameterBinding"/> for the same <see cref="EffectParameter"/> already
    /// exists.
    /// </exception>
    protected override void SetItem(int index, EffectParameterBinding item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "An EffectParameterBinding in an EffectParameterBindingCollection must not be null.");

      CheckHint(item);

      int indexOfExistingBinding = IndexOf(item.Parameter);
      if (indexOfExistingBinding >= 0 && indexOfExistingBinding != index)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "An EffectParameterBinding for the given EffectParameter \"{0}\" already exists at index {1}.",
          item.Parameter.Name,
          indexOfExistingBinding);
        throw new ArgumentException(message);
      }

      base.SetItem(index, item);
    }


    /// <overloads>
    /// <summary>
    /// Determines whether the <see cref="EffectParameterBindingCollection"/> contains a certain
    /// <see cref="EffectParameterBinding"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the <see cref="EffectParameterBindingCollection"/> contains an 
    /// <see cref="EffectParameterBinding"/> for the specified effect parameter.
    /// </summary>
    /// <param name="parameter">
    /// The <see cref="EffectParameter"/> of the <see cref="EffectParameterBinding"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the collection contains a binding for the specified effect 
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    public bool Contains(EffectParameter parameter)
    {
      return IndexOf(parameter) >= 0;
    }


    /// <summary>
    /// Determines whether the <see cref="EffectParameterBindingCollection"/> contains an 
    /// <see cref="EffectParameterBinding"/> for the effect parameter with the specified name.
    /// </summary>
    /// <param name="name">
    /// The name of the <see cref="EffectParameter"/> in the <see cref="EffectParameterBinding"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the collection contains a binding for the effect parameter with 
    /// the given name; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    public bool Contains(string name)
    {
      return IndexOf(name) >= 0;
    }


    /// <overloads>
    /// <summary>
    /// Searches for the specified <see cref="EffectParameterBinding"/> and returns the zero-based 
    /// index. 
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Searches for the <see cref="EffectParameterBinding"/> for the effect parameter with the 
    /// specified name and returns the zero-based index. 
    /// </summary>
    /// <param name="name">
    /// The name of the <see cref="EffectParameter"/> in the <see cref="EffectParameterBinding"/>.
    /// </param>
    /// <returns>
    /// The zero-based index of the <see cref="EffectParameterBinding"/> within the entire
    /// <see cref="EffectParameterBindingCollection"/>, if found; otherwise, -1. 
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    public int IndexOf(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("String must not be empty.", "name");

      int count = Items.Count;
      for (int index = 0; index < count; index++)
        if (Items[index].Parameter.Name == name)
          return index;

      return -1;
    }


    /// <summary>
    /// Searches for the <see cref="EffectParameterBinding"/> with the specified effect parameter 
    /// and returns the zero-based index. 
    /// </summary>
    /// <param name="parameter">
    /// The <see cref="EffectParameter"/> of the <see cref="EffectParameterBinding"/>.
    /// </param>
    /// <returns>
    /// The zero-based index of the <see cref="EffectParameterBinding"/> within the entire
    /// <see cref="EffectParameterBindingCollection"/>, if found; otherwise, -1. 
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    public int IndexOf(EffectParameter parameter)
    {
      if (parameter == null)
        throw new ArgumentNullException("parameter");

      int count = Items.Count;
      for (int index = 0; index < count; index++)
        if (Items[index].Parameter == parameter)
          return index;

      return -1;
    }


    /// <overloads>
    /// <summary>
    /// Removes an <see cref="EffectParameterBinding"/> from the collection.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Removes the <see cref="EffectParameterBinding"/> with the specified effect parameter.
    /// </summary>
    /// <param name="parameter">
    /// The <see cref="EffectParameter"/> of the <see cref="EffectParameterBinding"/> to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="EffectParameterBinding"/> was removed successfully; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parameter"/> is <see langword="null"/>.
    /// </exception>
    public bool Remove(EffectParameter parameter)
    {
      int index = IndexOf(parameter);
      if (index == -1)
        return false;

      RemoveAt(index);
      return true;
    }


    /// <summary>
    /// Removes the <see cref="EffectParameterBinding"/> for the effect parameter with the specified 
    /// name.
    /// </summary>
    /// <param name="name">
    /// The name of the <see cref="EffectParameter"/> in the <see cref="EffectParameterBinding"/> to
    /// remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="EffectParameterBinding"/> was removed successfully; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    public bool Remove(string name)
    {
      int index = IndexOf(name);
      if (index == -1)
        return false;

      RemoveAt(index);
      return true;
    }
    #endregion
  }
}
