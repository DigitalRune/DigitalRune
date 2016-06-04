// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Graphics.SceneGraph;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines the weights for a set of morph targets.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A mesh may include morph targets (see <see cref="Submesh.MorphTargets"/>). Each morph target
  /// is controlled by a <i>weight</i>, which is usually a value in the range [0, 1] or [-1, 1]. The
  /// <see cref="MorphWeightCollection"/> stores the weights for a set of morph targets.
  /// </para>
  /// <para>
  /// Morph targets are identified by their name. The extension method
  /// <see cref="MeshHelper.GetMorphTargetNames"/> can be used to get a list of all morph targets of a
  /// specific mesh.
  /// </para>
  /// <para>
  /// <strong>Animation:</strong><br/>
  /// The class <see cref="MorphWeightCollection"/> implements the interface
  /// <see cref="IAnimatableObject"/>, which means that the weights can be animated using
  /// DigitalRune Animation. The animatable properties can be accessed by calling the method
  /// <see cref="IAnimatableObject.GetAnimatableProperty{T}"/> passing the name of the morph target
  /// as the parameter.
  /// </para>
  /// <para>
  /// Note: The <see cref="IAnimatableProperty"/>s (the morph target weights) do not have a base
  /// value and therefore cannot be used in some from-to-by animations or similar animations that
  /// require a base value.
  /// </para>
  /// </remarks>
  /// <seealso cref="MorphTarget"/>
  /// <seealso cref="Submesh.MorphTargets"/>
  /// <seealso cref="MeshNode.MorphWeights"/>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  [DebuggerTypeProxy(typeof(MorphWeightCollectionView))]
  public sealed class MorphWeightCollection : IAnimatableObject, IEnumerable<KeyValuePair<string, float>>
  {
    // Notes:
    // Instead of the MorphWeightCollection we could simply use IDictionary<string, float>.
    // The renderer only needs TryGetValue(). Using IDictionary<string, float>:
    //  + Easy to use and understand.
    //  + The user can add/remove morph targets.
    //  - Memory overhead.
    //  - Does not implement IAnimatableObject.
    //  - No easy way to Reset() all morph target weights.
    //  - No Clone() mechanism.

    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    internal sealed class MorphWeightCollectionView
    {
      private readonly MorphWeightCollection _morphWeights;
      public MorphWeightCollectionView(MorphWeightCollection morphWeights)
      {
        _morphWeights = morphWeights;
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public KeyValuePair<string, float>[] Weights
      {
        get { return _morphWeights.ToArray(); }
      }
    }


    /// <summary>
    /// Enumerates the weights of a <see cref="MorphWeightCollection"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<KeyValuePair<string, float>>
    {
      private readonly MorphWeightCollection _morphWeights;
      private int _index;
      private KeyValuePair<string, float> _current;

      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <value>The element in the collection at the current position of the enumerator.</value>
      public KeyValuePair<string, float> Current
      {
        get { return _current; }
      }


      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <value>The element in the collection at the current position of the enumerator.</value>
      /// <exception cref="InvalidOperationException">
      /// The enumerator is positioned before the first element of the collection or after the last 
      /// element.
      /// </exception>
      object IEnumerator.Current
      {
        get
        {
          if (_index < 0)
          {
            if (_index == -1)
              throw new InvalidOperationException("The enumerator is positioned before the first element of the collection.");

            throw new InvalidOperationException("The enumerator is positioned after the last element of the collection.");
          }

          return _current;
        }
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="Enumerator"/> struct.
      /// </summary>
      /// <param name="morphWeights">The <see cref="MorphWeightCollection"/> to be enumerated.</param>
      internal Enumerator(MorphWeightCollection morphWeights)
      {
        _morphWeights = morphWeights;
        _index = -1;
        _current = new KeyValuePair<string, float>();
      }


      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting 
      /// unmanaged resources.
      /// </summary>
      public void Dispose()
      {
        _index = -2;
        _current = new KeyValuePair<string, float>();
      }


      /// <summary>
      /// Advances the enumerator to the next element of the collection.
      /// </summary>
      /// <returns>
      /// <see langword="true"/> if the enumerator was successfully advanced to the next element; 
      /// <see langword="false"/> if the enumerator has passed the end of the collection.
      /// </returns>
      /// <exception cref="InvalidOperationException">
      /// The collection was modified after the enumerator was created.
      /// </exception>
      public bool MoveNext()
      {
        if (_index == -2)
          return false;

        _index++;
        if (_index < _morphWeights.Count)
        {
          var names = _morphWeights._names;
          var weights = _morphWeights._weights;
          _current = new KeyValuePair<string, float>(names[_index], weights[_index].Value);
          return true;
        }

        _index = -2;
        _current = new KeyValuePair<string, float>();
        return false;
      }


      /// <summary>
      /// Sets the enumerator to its initial position, which is before the first element in the 
      /// collection.
      /// </summary>
      public void Reset()
      {
        _index = -1;
        _current = new KeyValuePair<string, float>();
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly string[] _names;
    private readonly MorphWeight[] _weights;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the <see cref="MorphWeightCollection"/>.
    /// </summary>
    /// <value>
    /// The name of the <see cref="MorphWeightCollection"/>. The default value is the name of the
    /// <see cref="Mesh"/> or <see langword="null"/>.
    /// </value>
    public string Name { get; set; }  // Needed for IAnimatableObject


    /// <summary>
    /// Gets the number of morph targets.
    /// </summary>
    /// <value>The number of morph targets.</value>
    public int Count
    {
      get { return _names.Length; }
    }


    /// <summary>
    /// Gets or sets the weight of the specified morph target.
    /// </summary>
    /// <param name="name">The name of the morph target.</param>
    /// <value>The weight of the morph target.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// No morph target with the given <paramref name="name"/> was found.
    /// </exception>
    public float this[string name]
    {
      get { return _weights[GetIndexOrThrow(name)].Value; }
      set { _weights[GetIndexOrThrow(name)].Value = value; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="MorphWeightCollection"/> class for the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="mesh"/> does not include any morph targets.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public MorphWeightCollection(Mesh mesh)
      : this(GetMorphTargetNames(mesh))
    {
      Name = mesh.Name;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="MorphWeightCollection"/> class for the specified morph
    /// targets.
    /// </summary>
    /// <param name="morphTargetNames">The names of the morph targets.</param>
    /// <exception cref="ArgumentException">
    /// No parameters specified. Or the name of a morph target is null or empty.
    /// </exception>
    public MorphWeightCollection(params string[] morphTargetNames)
    {
      if (morphTargetNames == null || morphTargetNames.Length == 0)
        throw new ArgumentException("The list of morph target names is empty.");

      for (int i = 0; i < morphTargetNames.Length; i++)
        if (string.IsNullOrEmpty(morphTargetNames[i]))
          throw new ArgumentException("The name of the morph target must not be null or empty.");

      _names = new string[morphTargetNames.Length];
      Array.Copy(morphTargetNames, _names, morphTargetNames.Length);
      Array.Sort(_names, StringComparer.Ordinal);

      _names = morphTargetNames;
      _weights = new MorphWeight[morphTargetNames.Length];
      for (int i = 0; i < morphTargetNames.Length; i++)
        _weights[i] = new MorphWeight();
    }


    /// <summary>
    /// Creates a copy of the specified <see cref="MorphWeightCollection"/> instance.
    /// </summary>
    /// <param name="source">The source <see cref="MorphWeightCollection"/>.</param>
    private MorphWeightCollection(MorphWeightCollection source)
    {
      Name = source.Name;
      _names = source._names;

      var weights = source._weights;
      _weights = new MorphWeight[weights.Length];
      for (int i = 0; i < weights.Length; i++)
        _weights[i] = new MorphWeight { Value = weights[i].Value };
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private static string[] GetMorphTargetNames(Mesh mesh)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      return mesh.GetMorphTargetNames();
    }


    /// <summary>
    /// Creates a new <see cref="MorphWeightCollection"/> that is a clone (deep copy) of the current 
    /// instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="MorphWeightCollection"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    public MorphWeightCollection Clone()
    {
      return new MorphWeightCollection(this);
    }


    private int GetIndexOrThrow(string name)
    {
      int index = GetIndex(name);
      if (index < 0)
      {
        if (name == null)
          throw new ArgumentNullException("name");

        throw new KeyNotFoundException("The specified morph target was not found.");
      }

      return index;
    }


    private int GetIndex(string name)
    {
      // Note: It is not guaranteed that the names are sorted.
      return Array.BinarySearch(_names, name, StringComparer.Ordinal);
    }


    /// <summary>
    /// Clears the weights of all morph targets.
    /// </summary>
    public void Reset()
    {
      for (int i = 0; i < _weights.Length; i++)
        _weights[i].Value = 0;
    }


    /// <summary>
    /// Determines whether the <see cref="MorphWeightCollection"/> contains a morph target with the specified
    /// name.
    /// </summary>
    /// <param name="name">The name of the morph target.</param>
    /// <returns>
    /// <see langword="true"/> if this instance contains a morph target with the given
    /// <paramref name="name"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(string name)
    {
      return GetIndex(name) >= 0;
    }


    /// <summary>
    /// Gets the weight for the specified morph target.
    /// </summary>
    /// <param name="name">The name of the morph target.</param>
    /// <param name="weight">The weight of the morph target.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="MorphWeightCollection"/> contains the specified morph target;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetValue(string name, out float weight)
    {
      int index = GetIndex(name);
      if (index >= 0)
      {
        weight = _weights[index].Value;
        return true;
      }

      weight = 0;
      return false;
    }


    #region ----- IAnimatableObject -----

    /// <inheritdoc/>
    IEnumerable<IAnimatableProperty> IAnimatableObject.GetAnimatedProperties()
    {
      foreach (var weight in _weights)
      {
        var property = (IAnimatableProperty)weight;
        if (property.IsAnimated)
          yield return weight;
      }
    }


    /// <inheritdoc/>
    IAnimatableProperty<T> IAnimatableObject.GetAnimatableProperty<T>(string name)
    {
      int index = GetIndex(name);
      if (index >= 0)
        return _weights[index] as IAnimatableProperty<T>;

      return null;
    }
    #endregion


    #region ----- IEnumerable<T> -----

    /// <summary>
    /// Returns an enumerator that iterates through the morph target weights.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the morph target weights.
    /// </returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<KeyValuePair<string, float>> IEnumerable<KeyValuePair<string, float>>.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion

    #endregion
  }
}
