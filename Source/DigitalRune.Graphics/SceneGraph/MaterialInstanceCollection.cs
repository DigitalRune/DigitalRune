// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Manages a read-only collection of <see cref="MaterialInstance"/> objects.
  /// </summary>
  [DebuggerDisplay("{GetType().Name,nq}(Count = {Count})")]
  [DebuggerTypeProxy(typeof(MaterialInstanceCollectionView))]
  public class MaterialInstanceCollection : IList<MaterialInstance>
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // This view is used as DebuggerTypeProxy. With this, the debugger will display 
    // a readable list of material instances for the MaterialInstanceCollection.
    internal sealed class MaterialInstanceCollectionView
    {
      private readonly MaterialInstanceCollection _collection;
      public MaterialInstanceCollectionView(MaterialInstanceCollection collection)
      {
        _collection = collection;
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public MaterialInstance[] MaterialInstances
      {
        get { return _collection.ToArray(); }
      }
    }


    /// <summary>
    /// Enumerates the elements of a <see cref="MaterialInstanceCollection"/>. 
    /// </summary>
    public struct Enumerator : IEnumerator<MaterialInstance>
    {
      private readonly MaterialInstance[] _array;
      private int _index;
      private MaterialInstance _current;


      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <value>The element in the collection at the current position of the enumerator.</value>
      public MaterialInstance Current
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
            else
              throw new InvalidOperationException("The enumerator is positioned after the last element of the collection.");
          }

          return _current;
        }
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="MaterialInstanceCollection.Enumerator"/> struct.
      /// </summary>
      /// <param name="array">The material instances to be enumerated.</param>
      internal Enumerator(MaterialInstance[] array)
      {
        _array = array;
        _index = -1;
        _current = null;
      }


      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting 
      /// unmanaged resources.
      /// </summary>
      public void Dispose()
      {
        _index = -2;
        _current = null;
      }


      /// <summary>
      /// Advances the enumerator to the next element of the collection.
      /// </summary>
      /// <returns>
      /// <see langword="true"/> if the enumerator was successfully advanced to the next element; 
      /// <see langword="false"/> if the enumerator has passed the end of the collection.
      /// </returns>
      public bool MoveNext()
      {
        if (_index == -2)
          return false;

        _index++;
        if (_index < _array.Length)
        {
          _current = _array[_index];
          return true;
        }

        _index = -2;
        _current = null;
        return false;
      }


      /// <summary>
      /// Sets the enumerator to its initial position, which is before the first element in the 
      /// <see cref="MaterialInstanceCollection"/>.
      /// </summary>
      public void Reset()
      {
        _index = -1;
        _current = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly MaterialInstance[] _array;

    // Optimization: Store the hash values of all render passes for fast lookup.
    internal int[] PassHashes;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the number of material instances contained in the <see cref="MaterialInstanceCollection"/>.
    /// </summary>
    /// <value>
    /// The number of material instances contained in the <see cref="MaterialInstanceCollection"/>.
    /// </value>
    public int Count
    {
      get { return _array.Length; }
    }


    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <value>Always returns <see langword="true"/>.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection<MaterialInstance>.IsReadOnly
    {
      get { return true; }
    }


    /// <summary>
    /// Gets the material instance at the specified index.
    /// </summary>
    /// <value>The material instance at the specified index.</value>
    /// <param name="index">The zero-based index of the material instance to get.</param>
    /// <remarks>
    /// This indexer is an O(1) operation.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or equal to or greater than <see cref="Count"/>.
    /// </exception>
    public MaterialInstance this[int index]
    {
      get
      {
        if (index < 0 || _array.Length <= index)
          throw new ArgumentOutOfRangeException("index", "Index is out of range.");

        return _array[index];
      }
      set
      {
        ThrowNotSupportedException();
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialInstanceCollection"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialInstanceCollection"/> class.
    /// </summary>
    /// <param name="materials">The materials to be instantiated.</param>
    internal MaterialInstanceCollection(MaterialCollection materials)
    {
      InitializePassHashes(materials);

      int numberOfMaterials = materials.Count;
      _array = new MaterialInstance[numberOfMaterials];
      for (int i = 0; i < numberOfMaterials; i++)
        _array[i] = new MaterialInstance(materials[i]);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialInstanceCollection"/> class by cloning
    /// the specified material instances.
    /// </summary>
    /// <param name="materials">The material instances to be cloned.</param>
    internal MaterialInstanceCollection(MaterialInstanceCollection materials)
    {
      PassHashes = materials.PassHashes;

      int numberOfMaterials = materials.Count;
      _array = new MaterialInstance[numberOfMaterials];
      for (int i = 0; i < numberOfMaterials; i++)
        _array[i] = materials[i].Clone();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Stores the hash values of all supported render passes.
    /// </summary>
    /// <param name="materials">The materials to be instantiated.</param>
    private void InitializePassHashes(MaterialCollection materials)
    {
      var hashes = ResourcePools<int>.Lists.Obtain();
      foreach (var material in materials)
      {
        foreach (var pass in material.Passes)
        {
          int hash = pass.GetHashCode();
          if (!hashes.Contains(hash))
            hashes.Add(hash);
        }
      }

      PassHashes = hashes.ToArray();
      ResourcePools<int>.Lists.Recycle(hashes);

#if DEBUG
      // Just for debugging: 
      // For optimal performance hash values should not collide. 
      var dictionary = new Dictionary<int, string>();
      foreach (var material in materials)
      {
        foreach (var pass in material.Passes)
        {
          int hash = pass.GetHashCode();
          string otherPass;
          if (dictionary.TryGetValue(hash, out otherPass) && pass != otherPass)
          {
            Debug.WriteLine("The render passes \"{0}\" and \"{1}\" have the same hash value.", pass, otherPass);
            return;
          }

          dictionary[hash] = pass;
        }
      }
#endif
    }


    #region ----- IEnumerable, IEnumerable<T> -----

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_array);
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<MaterialInstance> IEnumerable<MaterialInstance>.GetEnumerator()
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


    #region ----- ICollection, ICollection<T> -----

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static void ThrowNotSupportedException()
    {
      throw new NotSupportedException("The MaterialInstanceCollection is read-only.");
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
    /// <exception cref="NotSupportedException">
    /// The <see cref="MaterialInstanceCollection"/> is read-only.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void ICollection<MaterialInstance>.Add(MaterialInstance item)
    {
      ThrowNotSupportedException();
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ICollection{T}"/> is read-only.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void ICollection<MaterialInstance>.Clear()
    {
      ThrowNotSupportedException();
    }


    /// <summary>
    /// Determines whether the <see cref="MaterialInstanceCollection"/> contains a specific value.
    /// </summary>
    /// <param name="materialInstance">
    /// The material instance to locate in the <see cref="MaterialInstanceCollection"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="materialInstance"/> is found in the 
    /// <see cref="MaterialInstanceCollection"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method performs a linear search; therefore, this method is an O(n) operation, where n 
    /// is <see cref="Count"/>. 
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
    public bool Contains(MaterialInstance materialInstance)
    {
      return Array.IndexOf(_array, materialInstance) != -1;
    }


    /// <summary>
    /// Copies the elements of the <see cref="MaterialInstanceCollection"/> to an 
    /// <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="MaterialInstanceCollection"/>. The <see cref="Array"/> must have zero-based 
    /// indexing.
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
    /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal to 
    /// or greater than the length of <paramref name="array"/>. Or the number of elements in the 
    /// source <see cref="MaterialInstanceCollection"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method is an O(n) operation, where n is <see cref="Count"/>.
    /// </para>
    /// </remarks>
    public void CopyTo(MaterialInstance[] array, int arrayIndex)
    {
      _array.CopyTo(array, arrayIndex);
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the 
    /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>. This method also returns 
    /// <see langword="false"/> if <paramref name="item"/> is not found in the original 
    /// <see cref="ICollection{T}"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ICollection{T}"/> is read-only.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection<MaterialInstance>.Remove(MaterialInstance item)
    {
      ThrowNotSupportedException();
      return false;
    }
    #endregion


    #region ----- IList<T> -----

    /// <summary>
    /// Determines the index of a specific material instance in the 
    /// <see cref="MaterialInstanceCollection"/>.
    /// </summary>
    /// <param name="materialInstance">
    /// The material instance to locate in the <see cref="MaterialInstanceCollection"/>.
    /// </param>
    /// <returns>
    /// The index of <paramref name="materialInstance"/> if found in the collection; otherwise, -1.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration")]
    public int IndexOf(MaterialInstance materialInstance)
    {
      return Array.IndexOf(_array, materialInstance);
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">
    /// The object to insert into the <see cref="IList{T}"/>.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// The <see cref="IList{T}"/> is read-only.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void IList<MaterialInstance>.Insert(int index, MaterialInstance item)
    {
      ThrowNotSupportedException();
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    /// <exception cref="NotSupportedException">
    /// The <see cref="IList{T}"/> is read-only.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void IList<MaterialInstance>.RemoveAt(int index)
    {
      ThrowNotSupportedException();
    }
    #endregion

    #endregion
  }
}
