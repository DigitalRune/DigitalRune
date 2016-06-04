// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Manages a collection of <see cref="IAnimationCompositionChain"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The composition chains are stored in a list. All composition chains that handle 
  /// <see cref="IImmediateAnimatableProperty"/>s are stored in the front of the list. These 
  /// composition chains are not sorted. 
  /// </para>
  /// <para>
  /// All other composition chains are stored in the back of the list. These chains are sorted using
  /// the hash code of the <see cref="IAnimatableProperty"/>. 
  /// </para>
  /// </remarks>
  internal sealed class AnimationCompositionChainCollection : IEnumerable<IAnimationCompositionChain>
  {
    // Notes:
    // Animations controlling animation weights need to be inserted at the start 
    // of the list because animation weights affect other animations and need to 
    // be updated first. (Animation weights are of type IImmediateAnimatableProperty.)

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Dummy hash code which is stored for IImmediateAnimatableProperties instead of the real hash
    // code. (The composition chains for IImmediateAnimatableProperties are not sorted.)
    private const uint ImmediateHashCode = 0;

    // Number of composition chains that handle IImmediateAnimatableProperties.
    private int _numberOfImmediates;

    // The list of all composition chains. 
    private readonly List<IAnimationCompositionChain> _compositionChains;

    // The hash codes of the animatable properties of the chains.
    private readonly List<uint> _hashCodes;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the total number of composition chains in this collection.
    /// </summary>
    /// <value>The total number of composition chains.</value>
    public int NumberOfChains { get { return _compositionChains.Count; } }


    /// <summary>
    /// Gets the number of composition chains that handle <see cref="IImmediateAnimatableProperty"/>s. 
    /// These chains are stored at the start of the collection.
    /// </summary>
    /// <value>
    /// The number of composition chains that handle <see cref="IImmediateAnimatableProperty"/>s.
    /// </value>
    public int NumberOfImmediateChains { get { return _numberOfImmediates; } }


    /// <summary>
    /// Gets the <see cref="IAnimationCompositionChain"/> at the specified index.
    /// </summary>
    /// <value>
    /// The <see cref="IAnimationCompositionChain"/> at the specified index.
    /// </value>
    public IAnimationCompositionChain this[int index]
    {
      get { return _compositionChains[index]; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationCompositionChainCollection"/> class.
    /// </summary>
    public AnimationCompositionChainCollection()
    {
      _compositionChains = new List<IAnimationCompositionChain>();
      _hashCodes = new List<uint>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return _compositionChains.GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the 
    /// <see cref="AnimationCompositionChainCollection"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{IAnimationCompositionChain}"/> for 
    /// <see cref="AnimationCompositionChainCollection"/>.
    /// </returns>
    IEnumerator<IAnimationCompositionChain> IEnumerable<IAnimationCompositionChain>.GetEnumerator()
    {
      return _compositionChains.GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the 
    /// <see cref="AnimationCompositionChainCollection"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="AnimationCompositionChainCollection"/>.
    /// </returns>
    public List<IAnimationCompositionChain>.Enumerator GetEnumerator()
    {
      return _compositionChains.GetEnumerator();
    }


    /// <summary>
    /// Gets the composition chain for the specified <paramref name="property"/>.
    /// </summary>
    /// <param name="property">The animatable property.</param>
    /// <param name="index">
    /// The index of the composition chain in this collection. If no suitable composition chain is 
    /// found, this is the index where a new composition chain should be inserted.
    /// </param>
    /// <param name="chain">
    /// The found composition chain. <see langword="null"/> if no suitable chain was found.
    /// </param>
    public void Get(IAnimatableProperty property, out int index, out IAnimationCompositionChain chain)
    {
      // Abort if property is null.
      if (property == null)
      {
        index = -1;
        chain = null;
        return;
      }

      // Composition chains for immediate animatable properties are stored at the front - unsorted.
      if (property is IImmediateAnimatableProperty)
      {
        // Search immediates in sequential order.
        for (int i = 0; i < _numberOfImmediates; i++)
        {
          var candidate = _compositionChains[i];
          if (candidate.Property == property)
          {
            index = i;
            chain = candidate;
            return;
          }
        }

        index = 0;
        chain = null;
        return;
      }

      // The other chains are stored after the immediates, sorted by the hash codes of the 
      // properties.
      uint hashCode = (uint)property.GetHashCode();

      // Search for non-immediates using binary search.
      int start = _numberOfImmediates;
      var numberOfCompositionChains = _compositionChains.Count;
      int end = numberOfCompositionChains - 1;
      while (start <= end)
      {
        index = start + (end - start >> 1);
        long comparison = (long)_hashCodes[index] - (long)hashCode;
        if (comparison == 0)
        {
          chain = _compositionChains[index];
          if (chain.Property == property)
            return;

          // Found the correct hash value. Must search vicinity because there could be more 
          // properties with this hash code.

          // Search forward.
          int savedIndex = index;
          index++;
          while (index < numberOfCompositionChains && _hashCodes[index] == hashCode)
          {
            chain = _compositionChains[index];
            if (chain.Property == property)
              return;
            index++;
          }

          // Search backward.
          index = savedIndex - 1;
          while (index >= _numberOfImmediates && _hashCodes[index] == hashCode)
          {
            chain = _compositionChains[index];
            if (chain.Property == property)
              return;
            index--;
          }

          index = savedIndex;
          chain = null;
          return;
        }

        if (comparison < 0)
        {
          Debug.Assert(hashCode > _hashCodes[index]);
          start = index + 1;
        }
        else
        {
          Debug.Assert(hashCode < _hashCodes[index]);
          end = index - 1;
        }
      }

      index = start;
      chain = null;
      return;
    }


    /// <summary>
    /// Inserts the composition chain at the specified index. 
    /// </summary>
    /// <param name="index">
    /// The index. (Use <see cref="Get"/> to find the correct index.)
    /// </param>
    /// <param name="compositionChain">The composition chain to insert.</param>
    public void Insert(int index, IAnimationCompositionChain compositionChain)
    {
      var property = compositionChain.Property;
      if (property == null)
        return;

      if (property is IImmediateAnimatableProperty)
      {
        // Insert at front.
        _compositionChains.Insert(0, compositionChain);
        _hashCodes.Insert(0, ImmediateHashCode);  // Insert dummy value.
        _numberOfImmediates++;
      }
      else
      {
        // Insert at given index.
        _hashCodes.Insert(index, (uint)property.GetHashCode());
        _compositionChains.Insert(index, compositionChain);
      }
    }


    /// <summary>
    /// Removes the composition chain at the given index.
    /// </summary>
    /// <param name="index">The index of the composition chain to remove.</param>
    public void RemoveAt(int index)
    {
      if (index < _numberOfImmediates)
        _numberOfImmediates--;

      _compositionChains.RemoveAt(index);
      _hashCodes.RemoveAt(index);
    }


    /// <summary>
    /// Executes a lot of assertions in DEBUG builds to validate the integrity of this collection.
    /// </summary>
    [Conditional("DEBUG")]
    public void Validate()
    {
      Debug.Assert(_hashCodes.Count == _compositionChains.Count, "The internal lists of the AnimationCompositionChainCollection are out-of-sync.");

      // Validate the composition chains of IImmediateAnimatableProperties.
      for (int i = 0; i < _numberOfImmediates; i++)
      {
        var property = _compositionChains[i].Property;
        uint hashCode = _hashCodes[i];
        Debug.Assert(property == null || property is IImmediateAnimatableProperty, "The composition chains are not sorted properly.");
        Debug.Assert(hashCode == ImmediateHashCode, "Wrong hash code stored in AnimationCompositionChainCollection.");
      }

      // Validate other composition chains.
      uint previousHash = 0;
      for (int i = _numberOfImmediates; i < _compositionChains.Count; i++)
      {
        var property = _compositionChains[i].Property;
        var hashCode = _hashCodes[i];
        Debug.Assert(property == null || !(property is IImmediateAnimatableProperty), "The composition chains are not sorted properly.");
        Debug.Assert(property == null || (uint)property.GetHashCode() == hashCode, "Wrong hash code stored in AnimationCompositionChainCollection.");
        Debug.Assert(hashCode >= previousHash, "Hash codes need to be sorted in ascending order.");
        previousHash = hashCode;
      }
    }
    #endregion
  }
}
