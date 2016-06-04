// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
#if !UNITY
using System.Collections.ObjectModel;
#else
using DigitalRune.Collections.ObjectModel;
#endif
using System.Diagnostics;
using System.Linq;
using DigitalRune.Animation.Traits;


namespace DigitalRune.Animation
{
  /// <summary>
  /// Manages a collection of animations that are combined and applied to a certain property.
  /// </summary>
  /// <typeparam name="T">The type of animation value.</typeparam>
  /// <remarks>
  /// The animated property is stored using a weak reference. Animation composition chains will be 
  /// automatically removed from the animation system if either the target object is garbage 
  /// collected or the composition chain is empty.
  /// </remarks>
  internal sealed class AnimationCompositionChain<T> : Collection<AnimationInstance<T>>, IAnimationCompositionChain, IRecyclable
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // ReSharper disable StaticFieldInGenericType
    private static readonly ResourcePool<AnimationCompositionChain<T>> Pool =
      new ResourcePool<AnimationCompositionChain<T>>(
        () => new AnimationCompositionChain<T>(),
        null,
        null);
    // ReSharper restore StaticFieldInGenericType

    // The animated property stored as a weak reference.
    private readonly WeakReference _weakReference;

    // The Animation value traits.
    private IAnimationValueTraits<T> _traits;

    // Snapshot of the last animation value.
    private T _snapshot;
    private bool _hasSnapshot;

    // The cached animation value.
    private T _value;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public bool IsEmpty
    {
      get { return _weakReference.Target == null || Items.Count == 0; }
    }


    /// <inheritdoc/>
    IAnimatableProperty IAnimationCompositionChain.Property
    {
      get { return (IAnimatableProperty)_weakReference.Target; }
    }


    /// <inheritdoc cref="IAnimationCompositionChain.Property"/>
    public IAnimatableProperty<T> Property
    {
      get { return (IAnimatableProperty<T>)_weakReference.Target; }
    }


    /// <summary>
    /// Gets the animation value traits.
    /// </summary>
    /// <value>The animation value traits.</value>
    public IAnimationValueTraits<T> Traits
    {
      get { return _traits; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Prevents a default instance of the <see cref="AnimationCompositionChain{T}"/> class from 
    /// being created.
    /// </summary>
    private AnimationCompositionChain()
    {
      _weakReference = new WeakReference(null);
    }


    /// <summary>
    /// Creates a new instance of the <see cref="AnimationCompositionChain{T}"/> class.
    /// </summary>
    /// <param name="property">The property that should be animated.</param>
    /// <param name="traits">The animation value traits.</param>
    /// <exception cref="ArgumentNullException">
    /// Either <paramref name="property"/> or <paramref name="traits"/> is <see langword="null"/>.
    /// </exception>
    public static AnimationCompositionChain<T> Create(IAnimatableProperty<T> property, IAnimationValueTraits<T> traits)
    {
      var compositionChain = Pool.Obtain();
      compositionChain.Initialize(property, traits);
      return compositionChain;
    }


    /// <inheritdoc/>
    public void Recycle()
    {
      Reset();
      Pool.Recycle(this);
    }


    /// <summary>
    /// Initializes the <see cref="AnimationCompositionChain{T}"/>.
    /// </summary>
    /// <param name="property">The property that should be animated.</param>
    /// <param name="traits">The animation value traits.</param>
    /// <exception cref="ArgumentNullException">
    /// Either <paramref name="property"/> or <paramref name="traits"/> is <see langword="null"/>.
    /// </exception>
    public void Initialize(IAnimatableProperty<T> property, IAnimationValueTraits<T> traits)
    {
      Debug.Assert(Count == 0, "Animation composition chain has not been reset properly.");
      Debug.Assert(_weakReference.Target == null, "Animation composition chain has not been reset properly.");
      Debug.Assert(_hasSnapshot == false, "Animation composition chain has not been reset properly.");
      Debug.Assert(_traits == null, "Animation composition chain has not been reset properly.");

      if (property == null)
        throw new ArgumentNullException("property");
      if (traits == null)
        throw new ArgumentNullException("traits");

      _weakReference.Target = property;
      _traits = traits;

      var reference = (property.HasBaseValue) ? property.BaseValue : property.AnimationValue;
      traits.Create(ref reference, out _value);
    }


    private void Reset()
    {
      Clear();
      ClearSnapshot();
      _weakReference.Target = null;
      _traits.Recycle(ref _value);
      _traits = null;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public void Update(AnimationManager animationManager)
    {
      IAnimatableProperty<T> property = Property;
      if (property == null)
      {
        // The animated object has been garbage collected.
        return;
      }

      int numberOfAnimations = Items.Count;
      if (numberOfAnimations == 0)
      {
        // All animations have stopped or were removed.
        if (property is ImmediateAnimatableProperty<T>)
        {
          // Special: Certain properties need to be set immediately. 
          // All other properties are set in Apply();
          ResetProperty(property);
        }

        return;
      }

      // Get the property's base value.
      T baseValue;
      if (property.HasBaseValue)
      {
        // The animatable property has a base value.
        baseValue = property.BaseValue;
      }
      else
      {
        // The animatable property does not have a base value. 
        // Use identity. (Create a temporary object if necessary.)
        _traits.Create(ref _value, out baseValue);
        _traits.SetIdentity(ref baseValue);
      }

      // Get the start value of the composition chain.
      if (_hasSnapshot)
      {
        // An animation snapshot has been taken previously.
        // Start composition chain with animation snapshot.
        _traits.Copy(ref _snapshot, ref _value);
      }
      else
      {
        // Start the composition chain with the base value.
        _traits.Copy(ref baseValue, ref _value);
      }

      // Compose the animations in the composition chain.
      for (int i = 0; i < numberOfAnimations; i++)
      {
        var animationInstance = Items[i];

        // The source value is the output of the previous stage in the composition chain: _value.
        // The target value is the base value of the animated property: baseValue.
        // Store the output of the current stage in _value.
        animationInstance.GetValue(ref _value, ref baseValue, ref _value);
      }

      if (!property.HasBaseValue)
      {
        // Recycle baseValue which was created previously.
        _traits.Recycle(ref baseValue);
      }

      if (property is ImmediateAnimatableProperty<T>)
      {
        // Special: Certain properties need to be set immediately. 
        // All other properties are set in Apply();
        SetProperty(property);
      }
    }


    /// <inheritdoc/>
    public void Apply()
    {
      IAnimatableProperty<T> property = Property;
      if (property == null)
      {
        // The animated object has been garbage collected.
        return;
      }

      if (property is ImmediateAnimatableProperty<T>)
      {
        // Special: ImmediateAnimatableProperty<T> are set in Update(). 
        // All other properties are set in Apply();
        return;
      }

      if (Items.Count > 0)
      {
        // Apply output of chain to property.
        SetProperty(property);
      }
      else
      {
        // All animations have stopped or were removed.
        ResetProperty(property);
      }
    }


    /// <summary>
    /// Writes the property value.
    /// </summary>
    /// <param name="property">The property.</param>
    private void SetProperty(IAnimatableProperty<T> property)
    {
      // Important: AnimationValue should be set BEFORE IsAnimated.
      // This is relevant because a property (such as a DigitalRune.Game.GameProperty<T>) 
      // could raise Changed-events. If we call the setters in the wrong order the 
      // property might raise too many Changed-events.
      Traits.Set(ref _value, property);
      property.IsAnimated = true;
    }


    /// <summary>
    /// Resets the property.
    /// </summary>
    /// <param name="property">The property.</param>
    private void ResetProperty(IAnimatableProperty<T> property)
    {
      // Important: AnimationValue should be set AFTER IsAnimated.
      // Again this is relevant to avoid too many Changed-events.
      property.IsAnimated = false;
      if (property.HasBaseValue)
      {
        // If the property has a base value we should reset the animation value
        // to avoid memory leaks.
        Traits.Reset(property);
      }
    }


    /// <summary>
    /// Takes a snapshot of the current animation value.
    /// </summary>
    /// <remarks>
    /// The first animation instance in the composition chain will receive snapshot as its source
    /// value instead of the properties' base value.
    /// </remarks>
    /// <seealso cref="ClearSnapshot"/>
    public void TakeSnapshot()
    {
      IAnimatableProperty<T> property = Property;
      if (property != null)
      {
        if (Items.Count > 0)
        {
          // Take snapshot of last animation value.
          if (!_hasSnapshot)
          {
            // Allocate object, if necessary.
            _traits.Create(ref _value, out _snapshot);
            _hasSnapshot = true;
          }

          _traits.Copy(ref _value, ref _snapshot);
        }
        else if (property.HasBaseValue)
        {
          // No animations: Take snapshot of base value.
          if (!_hasSnapshot)
          {
            // Allocate object, if necessary.
            _traits.Create(ref _value, out _snapshot);
            _hasSnapshot = true;
          }

          var baseValue = property.BaseValue;
          _traits.Copy(ref baseValue, ref _snapshot);
        }
        else
        {
          // The property does not have animations nor a base value.
          // Clear the snapshot. The animations will use the default 
          // value (AnimationValueTraits.Identity).
          ClearSnapshot();
        }
      }
      else
      {
        // The animatable property is recycled while the animation is started. 
        // Clear snapshot just to be safe. (Should never occur in practice.
        ClearSnapshot();
      }
    }


    /// <summary>
    /// Clears any previously taken snapshot.
    /// </summary>
    /// <see cref="TakeSnapshot"/>
    public void ClearSnapshot()
    {
      if (_hasSnapshot)
      {
        _traits.Recycle(ref _snapshot);
        _hasSnapshot = false;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Collection<AnimationInstance<T>>
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="AnimationCompositionChain{T}"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="AnimationCompositionChain{T}"/>.
    /// </returns>
    public new List<AnimationInstance<T>>.Enumerator GetEnumerator()
    {
      return ((List<AnimationInstance<T>>)Items).GetEnumerator();
    }
    #endregion


    //--------------------------------------------------------------
    #region IList<AnimationInstance>
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<AnimationInstance> IEnumerable<AnimationInstance>.GetEnumerator()
    {
      throw new NotImplementedException("The AnimationCompositionChain does not implement IEnumerable<AnimationInstance>. The enumerator should not be used!");
    }


    /// <summary>
    /// Adds an item to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to add to the <see cref="ICollection{T}"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is not of type <see cref="AnimationInstance{T}"/>.
    /// </exception>
    void ICollection<AnimationInstance>.Add(AnimationInstance item)
    {
      var animationInstance = item as AnimationInstance<T>;
      if (animationInstance == null)
        throw new ArgumentException("Cannot add animation instance to animation composition chain. The instance is not of the correct type.", "item");

      Add(animationInstance);
    }


    /// <summary>
    /// Determines whether the <see cref="ICollection{T}"/> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="ICollection{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> is found in the 
    /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>.
    /// </returns>
    bool ICollection<AnimationInstance>.Contains(AnimationInstance item)
    {
      var animationInstance = item as AnimationInstance<T>;
      if (animationInstance == null)
        return false;

      return Contains(animationInstance);
    }


    /// <summary>
    /// Copies the elements of the <see cref="ICollection{T}"/> to an <see cref="Array"/>, starting 
    /// at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="ICollection{T}"/>. The <see cref="Array"/> must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> is multidimensional.<br/>
    /// Or, <paramref name="arrayIndex"/> is equal to or greater than the length of
    /// <paramref name="array"/>.<br/>
    /// Or, the number of elements in the source <see cref="ICollection{T}"/> is greater than the
    /// available space from <paramref name="arrayIndex"/> to the end of the destination
    /// <paramref name="array"/>.
    /// </exception>
    /// 
    void ICollection<AnimationInstance>.CopyTo(AnimationInstance[] array, int arrayIndex)
    {
      Items.OfType<AnimationInstance>()
           .ToArray()
           .CopyTo(array, arrayIndex);
    }


    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the 
    /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>. This method also returns 
    /// <see langword="false"/> if <paramref name="item"/> is not found in the original 
    /// <see cref="ICollection{T}"/>.
    /// </returns>
    bool ICollection<AnimationInstance>.Remove(AnimationInstance item)
    {
      var animationInstance = item as AnimationInstance<T>;
      if (animationInstance == null)
        return false;

      return Remove(animationInstance);
    }


    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the <see cref="ICollection{T}"/> is read-only; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    bool ICollection<AnimationInstance>.IsReadOnly
    {
      get { return false; }
    }


    /// <summary>
    /// Determines the index of a specific item in the <see cref="IList{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to locate in the <see cref="IList{T}"/>.
    /// </param>
    /// <returns>
    /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is not of type <see cref="AnimationInstance{T}"/>.
    /// </exception>
    int IList<AnimationInstance>.IndexOf(AnimationInstance item)
    {
      var animationInstance = item as AnimationInstance<T>;
      if (animationInstance == null)
        return -1;

      return IndexOf(animationInstance);
    }


    /// <summary>
    /// Inserts an item to the <see cref="IList{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">
    /// The object to insert into the <see cref="IList{T}"/>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="IList{T}"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="IList{T}"/> is read-only.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is not of type <see cref="AnimationInstance{T}"/>.
    /// </exception>
    void IList<AnimationInstance>.Insert(int index, AnimationInstance item)
    {
      var animationInstance = item as AnimationInstance<T>;
      if (animationInstance == null)
        throw new ArgumentException("Cannot add animation instance to animation composition chain. The instance is not of the correct type.", "item");

      Insert(index, animationInstance);
    }


    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="IList{T}"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The property is set and the <see cref="IList{T}"/> is read-only.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is not of type <see cref="AnimationInstance{T}"/>.
    /// </exception>
    AnimationInstance IList<AnimationInstance>.this[int index]
    {
      get { return Items[index]; }
      set
      {
        var animationInstance = value as AnimationInstance<T>;
        if (animationInstance == null)
          throw new ArgumentException("Cannot add animation instance to animation composition chain. The instance is not of the correct type.", "value");

        this[index] = animationInstance;
      }
    }
    #endregion
  }
}
