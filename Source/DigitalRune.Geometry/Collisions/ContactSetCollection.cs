// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Collections;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// A collection of contact sets.
  /// </summary>
  /// <remarks>
  /// The contact sets in the collection are not necessarily stored in the same order as they were
  /// added. Duplicate contact sets and <see langword="null"/> values must not be added to the
  /// collection.
  /// </remarks>
  [DebuggerTypeProxy(typeof(ContactSetCollectionView))]
  public class ContactSetCollection : ICollection<ContactSet>
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // This view is used as DebuggerTypeProxy. With this, the debugger will display 
    // a readable list of contact sets for the ContactSetCollection.
    internal class ContactSetCollectionView
    {
      private readonly ContactSetCollection _collection;
      public ContactSetCollectionView(ContactSetCollection collection)
      {
        _collection = collection;
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public ContactSet[] ContactSets
      {
        get { return _collection.ToArray(); }
      }
    }


    private struct Slot
    {
      // If the slot is used, the index of the next slot in the chain.
      // If the slot has been freed, the index of the next slot in the free list.
      // -1 indicates the end of the chain/free list.
      public int Next;

      // Collision objects are stored locally to prevent cache misses.
      public CollisionObject ObjectA;
      public CollisionObject ObjectB;

      // The contact set stored in the slot.
      public ContactSet ContactSet;
    }


    /// <summary>
    /// Enumerates the contact sets in a <see cref="ContactSetCollection"/>. 
    /// </summary>
    public struct Enumerator : IEnumerator<ContactSet>
    {
      private ContactSetCollection _collection;
      private int _index;
      private readonly int _version;
      private ContactSet _current;

      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <value>The element in the collection at the current position of the enumerator.</value>
      public ContactSet Current
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
          CheckState();
          if (_index <= 0)
            throw new InvalidOperationException("The enumerator is positioned before the first element of the collection or after the last element.");

          return _current;
        }
      }

      /// <summary>
      /// Initializes a new instance of the <see cref="ContactSetCollection.Enumerator"/> struct.
      /// </summary>
      /// <param name="collection">The <see cref="ContactSetCollection"/> to be enumerated.</param>
      internal Enumerator(ContactSetCollection collection)
      {
        _collection = collection;
        _index = 0;
        _version = collection._version;
        _current = null;
      }

      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting 
      /// unmanaged resources.
      /// </summary>
      public void Dispose()
      {
        _collection = null;
        _current = null;
      }

      void CheckState()
      {
        if (_collection == null)
          throw new ObjectDisposedException(GetType().FullName);
        if (_collection._version != _version)
          throw new InvalidOperationException("The collection has been modified while it was iterated over.");
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
        CheckState();

        if (_index < 0)
          return false;

        while (_index < _collection._touchedSlots)
        {
          var contactSet = _collection._slots[_index].ContactSet;
          if (contactSet != null)
          {
            _current = contactSet;
            _index++;
            return true;
          }

          // Slot is empty, try next slot.
          _index++;
        }

        _index = -1;
        _current = null;
        return false;
      }

      /// <summary>
      /// Sets the enumerator to its initial position, which is before the first element in the 
      /// <see cref="ContactSetCollection"/>.
      /// </summary>
      /// <exception cref="InvalidOperationException">
      /// The <see cref="ContactSetCollection"/> was modified after the enumerator was created.
      /// </exception>
      void IEnumerator.Reset()
      {
        CheckState();
        _index = 0;
        _current = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The hash table stores the index into the slots array plus 1.
    // A value of 0 indicates that there is no entry.
    //  tableIndex = hashCode % _table.Length
    //  _table[tableIndex] = slotIndex + 1
    //  slotIndex = _table[tableIndex] - 1
    private int[] _table;

    private Slot[] _slots;

    // The number of slot that are in use (i.e. filled with data) or have been used
    // and are not in the "free list". The index of the first untouched slot.
    private int _touchedSlots;

    // The index of the first free slot in the free list.
    // Remove() prepends the cleared slots to the free list.
    // Add() takes the first slot from the free list, or increases _touchedSlots
    // if the free list is empty.
    private int _freeList;

    // The number of items in the collection.
    private int _count;

    // The version number is incremented with every change. Used by the enumerator
    // to detect changes.
    private int _version;

    // true, if the collection is owned by a collision domain. In this case the 
    // contact sets can be stored in linked lists for fast access.
    private readonly bool _ownedByDomain;

    // Array used in Synchronize() method. (Only used by CollisionDetectionBroadPhase.)
    private bool[] _used;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the number of contact sets in the collection. 
    /// </summary>
    /// <value>The number of contact sets in the collection.</value>
    public int Count
    {
      get { return _count; }
    }


    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this <see cref="ICollection{T}"/> is read-only; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection<ContactSet>.IsReadOnly
    {
      get { return false; }
    }


    /// <summary>
    /// Gets the number of entries in the internal array.
    /// </summary>
    /// <value>
    /// The number of used entries in the internal array. <see cref="InternalCount"/> is equal to
    /// or greater than <see cref="Count"/> because the internal array may contain empty slots!
    /// </value>
    internal int InternalCount
    {
      get { return _touchedSlots; }
    }


    /// <summary>
    /// Gets the contact set at the specified index in the internal array.
    /// </summary>
    /// <param name="index">The index into the internal array.</param>
    /// <value>
    /// The contact set. Returns <see langword="null"/> if the slot at the specified index is
    /// empty!
    /// </value>
    internal ContactSet this[int index]
    {
      get { return _slots[index].ContactSet; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ContactSetCollection"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ContactSetCollection"/> class.
    /// </summary>
    public ContactSetCollection()
    {
      Initialize(4);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ContactSetCollection"/> class that manages the
    /// contact sets of the specified collision domain.
    /// </summary>
    internal ContactSetCollection(CollisionDomain domain)
    {
      _ownedByDomain = (domain != null);
      Initialize(4);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ContactSetCollection"/> class with the given
    /// contact sets.
    /// </summary>
    /// <param name="contactSets">
    /// The contact sets which are initially added to the collection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="contactSets"/> is <see langword="null"/>.
    /// </exception>
    public ContactSetCollection(IEnumerable<ContactSet> contactSets)
    {
      if (contactSets == null)
        throw new ArgumentNullException("contactSets");

      var collection = contactSets as ICollection<ContactSet>;
      Initialize((collection != null) ? collection.Count : 4);

      foreach (var contactSet in contactSets)
        Add(contactSet);
    }


    private void Initialize(int capacity)
    {
      capacity = PrimeHelper.NextPrime(capacity);

      _table = new int[capacity];
      _slots = new Slot[capacity];
      _touchedSlots = 0;
      _freeList = -1;
      _count = 0;
      _version = 0;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    internal static int GetHashCode(CollisionObject objectA, CollisionObject objectB)
    {
      int hashA = objectA.GetHashCode();
      int hashB = objectB.GetHashCode();
      int hash = (hashA != hashB) ? hashA ^ hashB : hashA;
      return hash & int.MaxValue;
    }


    private int GetSlotIndex(int tableIndex, CollisionObject objectA, CollisionObject objectB)
    {
      // Find head of chain.
      int slotIndex = _table[tableIndex] - 1;

      // Check items in chain.
      while (slotIndex != -1)
      {
        var slot = _slots[slotIndex];
        if (slot.ObjectA != null 
            && (slot.ObjectA == objectA && slot.ObjectB == objectB
                || slot.ObjectA == objectB && slot.ObjectB == objectA))
        {
          return slotIndex;
        }

        slotIndex = slot.Next;
      }

      return -1;
    }


    /// <summary>
    /// Adds the specified contact set to the collection.
    /// </summary>
    /// <param name="item">The contact set to add to the collection.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is already contained in the collection.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    public void Add(ContactSet item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      var objectA = item.ObjectA;
      var objectB = item.ObjectB;
      int hashCode = GetHashCode(objectA, objectB);
      int tableIndex = hashCode % _table.Length;

      if (GetSlotIndex(tableIndex, objectA, objectB) >= 0)
        throw new ArgumentException("The contact set with the given collision objects is already contained in the collection.");

      Add(tableIndex, hashCode, objectA, objectB, item);
    }


    private int Add(int tableIndex, int hashCode, CollisionObject objectA, CollisionObject objectB, ContactSet contactSet)
    {
      // Use free slot, if available.
      int slotIndex = _freeList;
      if (slotIndex == -1)
      {
        // Free list is empty. Use new slot.
        slotIndex = _touchedSlots;
        if (slotIndex == _slots.Length)
        {
          // Increase capacity.
          Resize(2 * _count);
          tableIndex = hashCode % _table.Length;
        }
        _touchedSlots++;
      }
      else
      {
        // Remove slot from head of free list.
        _freeList = _slots[slotIndex].Next;
      }

      // Prepend the new item to the linked list and update the hash table.
      // (The hash table points to the head of the linked list.)
      var slot = new Slot
      {
        Next = _table[tableIndex] - 1,
        ObjectA = objectA,
        ObjectB = objectB,
        ContactSet = contactSet,
      };
      _slots[slotIndex] = slot;
      _table[tableIndex] = slotIndex + 1;

      // In addition, add the contact set to the linked lists of the collision objects.
      if (_ownedByDomain)
        AddToLinkedLists(contactSet);

      _count++;
      _version++;
      return slotIndex;
    }


    /// <summary>
    /// Removes all contact sets from the collection.
    /// </summary>
    public void Clear()
    {
      if (_touchedSlots > 0)
      {
        if (_ownedByDomain)
          ClearLinkedLists();

        Array.Clear(_table, 0, _table.Length);
        Array.Clear(_slots, 0, _touchedSlots);
        _touchedSlots = 0;
        _freeList = -1;
        _count = 0;
        _version++;

        if (_used != null)
          Array.Clear(_used, 0, _touchedSlots);
      }
    }


    /// <overloads>
    /// <summary>
    /// Determines whether the collection contains a contact set.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the collection contains the specified contact set.
    /// </summary>
    /// <param name="item">The contact set to locate in the collection.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> is found; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Contains(ContactSet item)
    {
      if (item == null)
        return false;

      int hashCode = GetHashCode(item.ObjectA, item.ObjectB);
      int tableIndex = hashCode % _table.Length;

      // Find head of chain.
      int slotIndex = _table[tableIndex] - 1;

      // Check items in chain.
      while (slotIndex != -1)
      {
        if (_slots[slotIndex].ContactSet == item)
          return true;

        slotIndex = _slots[slotIndex].Next;
      }

      return false;
    }


    /// <summary>
    /// Determines whether the collection contains a contact set for the given pair of collision 
    /// objects.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <returns>
    /// <see langword="true"/> if the collection contains a contact set with contacts between 
    /// <paramref name="objectA"/> and <paramref name="objectB"/>; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Contains(CollisionObject objectA, CollisionObject objectB)
    {
      if (objectA == null || objectB == null)
        return false;

      ContactSet contactSet;
      return TryGet(objectA, objectB, out contactSet);
    }


    /// <summary>
    /// Determines whether the collection contains a contact set for the given pair of
    /// collision objects.
    /// </summary>
    /// <param name="collisionObjectPair">The collision object pair.</param>
    /// <returns>
    /// <see langword="true"/> if the collection contains a contact set with contacts between
    /// the given pair of objects; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Pair<CollisionObject> collisionObjectPair)
    {
      return Contains(collisionObjectPair.First, collisionObjectPair.Second);
    }


    /// <summary>
    /// Determines whether the collection contains a contact set for the given 
    /// collision object.
    /// </summary>
    /// <param name="collisionObject">The collision object.</param>
    /// <returns>
    /// <see langword="true"/> if the collection contains a contact set with 
    /// <paramref name="collisionObject"/>; otherwise, <see langword="false"/>.
    /// </returns>
    internal bool Contains(CollisionObject collisionObject)
    {
      bool found = false;
      if (_ownedByDomain)
      {
        // Contact sets are stored in linked lists per collision object.
        // --> Check whether list is empty.
        found = (collisionObject.ContactSets != null);
      }
      else
      {
        // Go through all contact sets in the collection.
        for (int i = 0; i < _touchedSlots; i++)
        {
          if (_slots[i].ObjectA == collisionObject || _slots[i].ObjectB == collisionObject)
            found = true;
        }
      }

      return found;
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
    /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal to 
    /// or greater than the length of <paramref name="array"/>. Or the number of elements in the 
    /// source <see cref="ICollection{T}"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    public void CopyTo(ContactSet[] array, int arrayIndex)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (arrayIndex < 0)
        throw new ArgumentOutOfRangeException("arrayIndex");
      if (arrayIndex > array.Length)
        throw new ArgumentException("index larger than largest valid index of array");
      if (array.Length - arrayIndex < _count)
        throw new ArgumentException("Destination array cannot hold the requested elements!");

      for (int i = 0; i < _touchedSlots; i++)
      {
        var contactSet = _slots[i].ContactSet;
        if (contactSet != null)
        {
          array[arrayIndex] = contactSet;
          arrayIndex++;
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Gets the contact set with the contacts between two collision objects.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the contact set with the contacts between the specified collision objects.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <returns>
    /// <para>
    /// A <see cref="ContactSet"/> with contacts between <paramref name="objectA"/> and 
    /// <paramref name="objectB"/>. The collision objects in the returned <see cref="ContactSet"/>
    /// can be swapped! See <see cref="ContactSet"/> for more information on <i>swapped contact 
    /// sets</i>.
    /// </para>
    /// <para>
    /// If the collection does not contain a suitable <see cref="ContactSet"/>, 
    /// <see langword="null"/> is returned. 
    /// </para>
    /// </returns>
    public ContactSet GetContacts(CollisionObject objectA, CollisionObject objectB)
    {
      ContactSet contactSet;
      TryGet(objectA, objectB, out contactSet);
      return contactSet;
    }


    /// <summary>
    /// Gets the contact set with the contacts between the specified pair of collision objects.
    /// </summary>
    /// <param name="collisionObjectPair">The collision object pair.</param>
    /// <returns>
    /// <para>
    /// A <see cref="ContactSet"/> with contacts between the given pair of objects. 
    /// The collision objects in the returned <see cref="ContactSet"/>
    /// can be swapped! See <see cref="ContactSet"/> for more information on <i>swapped contact
    /// sets</i>.
    /// </para>
    /// <para>
    /// If the collection does not contain a suitable <see cref="ContactSet"/>,
    /// <see langword="null"/> is returned.
    /// </para>
    /// </returns>
    public ContactSet GetContacts(Pair<CollisionObject> collisionObjectPair)
    {
      ContactSet contactSet;
      TryGet(collisionObjectPair.First, collisionObjectPair.Second, out contactSet);
      return contactSet;
    }


    /// <summary>
    /// Gets the contact sets for the specified collision object.
    /// </summary>
    /// <param name="collisionObject">The collision object.</param>
    /// <returns>
    /// All <see cref="ContactSet"/>s that include <paramref name="collisionObject"/>.
    /// </returns>
    public IEnumerable<ContactSet> GetContacts(CollisionObject collisionObject)
    {
#if !POOL_ENUMERABLES
      if (_ownedByDomain)
      {
        // Contact sets are stored in linked lists per collision object.
        // --> Return all contact sets in linked list.
        var current = collisionObject.ContactSets;
        while (current != null)
        {
          yield return current;
          current = (current.ObjectA == collisionObject) ? current.NextA : current.NextB;
        }
      }
      else
      {
        // Go through all contact sets in the collection.
        for (int i = 0; i < _touchedSlots; i++)
        {
          if (_slots[i].ObjectA == collisionObject || _slots[i].ObjectB == collisionObject)
            yield return _slots[i].ContactSet;
        }
      }
#else
      if (_ownedByDomain)
        return GetContactsWork0.Create(collisionObject);

      return GetContactsWork1.Create(this, collisionObject);
#endif
    }


#if POOL_ENUMERABLES
    private sealed class GetContactsWork0 : PooledEnumerable<ContactSet>
    {
      private static readonly ResourcePool<GetContactsWork0> Pool = new ResourcePool<GetContactsWork0>(() => new GetContactsWork0(), x => x.Initialize(), null);
      private CollisionObject _collisionObject;
      private ContactSet _next;

      public static IEnumerable<ContactSet> Create(CollisionObject collisionObject)
      {
        var enumerable = Pool.Obtain();
        enumerable._collisionObject = collisionObject;
        enumerable._next = collisionObject.ContactSets;
        return enumerable;
      }

      protected override bool OnNext(out ContactSet current)
      {
        if (_next == null)
        {
          current = null;
          return false;
        }

        current = _next;
        _next = (_next.ObjectA == _collisionObject) ? _next.NextA : _next.NextB;
        return true;
      }

      protected override void OnRecycle()
      {
        _collisionObject = null;
        _next = null;
        Pool.Recycle(this);
      }
    }


    private sealed class GetContactsWork1 : PooledEnumerable<ContactSet>
    {
      private static readonly ResourcePool<GetContactsWork1> Pool = new ResourcePool<GetContactsWork1>(() => new GetContactsWork1(), x => x.Initialize(), null);
      private ContactSetCollection _collection;
      private CollisionObject _collisionObject;
      private int _index;

      public static IEnumerable<ContactSet> Create(ContactSetCollection collection, CollisionObject collisionObject)
      {
        var enumerable = Pool.Obtain();
        enumerable._collection = collection;
        enumerable._collisionObject = collisionObject;
        enumerable._index = 0;
        return enumerable;
      }

      protected override bool OnNext(out ContactSet current)
      {
        if (_index < 0)
        {
          current = null;
          return false;
        }

        var slots = _collection._slots;
        while (_index < _collection._touchedSlots)
        {
          if (slots[_index].ObjectA == _collisionObject || slots[_index].ObjectB == _collisionObject)
          {
            current = slots[_index].ContactSet;
            _index++;
            return true;
          }

          _index++;
        }

        _index = -1;
        current = null;
        return false;
      }

      protected override void OnRecycle()
      {
        _collection = null;
        _collisionObject = null;
        Pool.Recycle(this);
      }
    }
#endif


    /// <overloads>
    /// <summary>
    /// Removes one or more contact sets from the collection.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if item was successfully removed from the 
    /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>. This method also returns 
    /// <see langword="false"/> if item is not found in the original <see cref="ICollection{T}"/>.
    /// </returns>
    public bool Remove(ContactSet item)
    {
      if (item == null)
        return false;

      int hashCode = GetHashCode(item.ObjectA, item.ObjectB);
      int tableIndex = hashCode % _table.Length;
      int slotIndex = _table[tableIndex] - 1;

      if (slotIndex == -1)
        return false;

      // Walk linked list until right slot (and its predecessor) is found or end 
      // is reached.
      int prevSlotIndex = -1;
      do
      {
        var slot = _slots[slotIndex];
        if (slot.ContactSet == item)
          break;

        prevSlotIndex = slotIndex;
        slotIndex = slot.Next;
      } while (slotIndex != -1);

      // If we reached the end of the chain, return false.
      if (slotIndex == -1)
        return false;

      // Remove slot from linked list.
      if (prevSlotIndex == -1)
      {
        // The slot is the head of the linked list.
        _table[tableIndex] = _slots[slotIndex].Next + 1;
      }
      else
      {
        // The slot is in the middle or end of the linked list.
        _slots[prevSlotIndex].Next = _slots[slotIndex].Next;
      }

      // Clear slot and prepend it to the free list.
      _slots[slotIndex] = new Slot { Next = _freeList };
      _freeList = slotIndex;

      // In addition, remove the contact set from the linked lists.
      if (_ownedByDomain)
        RemoveFromLinkedLists(item);

      _count--;
      _version++;
      return true;
    }


    private ContactSet Remove(CollisionObject objectA, CollisionObject objectB, int hashCode)
    {
      int tableIndex = hashCode % _table.Length;
      int slotIndex = _table[tableIndex] - 1;

      if (slotIndex == -1)
        return null;

      // Walk linked list until right slot (and its predecessor) is found or end 
      // is reached.
      int prevSlotIndex = -1;
      ContactSet contactSet = null;
      do
      {
        var slot = _slots[slotIndex];
        if (slot.ObjectA != null
            && (slot.ObjectA == objectA && slot.ObjectB == objectB
                || slot.ObjectA == objectB && slot.ObjectB == objectA))
        {
          contactSet = slot.ContactSet;
          break;
        }

        prevSlotIndex = slotIndex;
        slotIndex = slot.Next;
      } while (slotIndex != -1);

      // If we reached the end of the chain, return false.
      if (slotIndex == -1)
        return null;

      // Remove slot from linked list.
      if (prevSlotIndex == -1)
      {
        // The slot is the head of the linked list.
        _table[tableIndex] = _slots[slotIndex].Next + 1;
      }
      else
      {
        // The slot is in the middle or end of the linked list.
        _slots[prevSlotIndex].Next = _slots[slotIndex].Next;
      }

      // Clear slot and prepend it to the free list.
      _slots[slotIndex] = new Slot { Next = _freeList };
      _freeList = slotIndex;

      // In addition, remove the contact set from the linked lists.
      if (_ownedByDomain)
        RemoveFromLinkedLists(contactSet);

      _count--;
      _version++;
      return contactSet;
    }


    /// <summary>
    /// Removes the contact sets for the given collision object.
    /// </summary>
    /// <param name="collisionObject">The collision object.</param>
    /// <returns>
    /// <see langword="true"/> if an item was successfully removed; otherwise, 
    /// <see langword="false"/>. This method also returns <see langword="false"/> if item is not 
    /// found.
    /// </returns>
    public bool Remove(CollisionObject collisionObject)
    {
      return Remove(collisionObject, (List<ContactSet>)null);
    }


    /// <summary>
    /// Removes the contact sets for the given collision object and stores them in a list.
    /// </summary>
    /// <param name="collisionObject">The collision object.</param>
    /// <param name="removedContactSets">
    /// A list to which the removed contact sets are added. Can be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an item was successfully removed; otherwise, 
    /// <see langword="false"/>. This method also returns <see langword="false"/> if item is not 
    /// found.
    /// </returns>
    internal bool Remove(CollisionObject collisionObject, List<ContactSet> removedContactSets)
    {
      if (collisionObject == null)
        return false;

      bool removed = false;
      if (_ownedByDomain)
      {
        // Contact sets are stored in linked lists per collision object.
        // --> Remove head of linked list until list is empty.
        while (collisionObject.ContactSets != null)
        {
          if (removedContactSets != null)
            removedContactSets.Add(collisionObject.ContactSets);

          Remove(collisionObject.ContactSets);
          removed = true;
        }
      }
      else
      {
        // Go through all contact sets in the collection. 
        for (int i = 0; i < _touchedSlots; i++)
        {
          if (_slots[i].ObjectA == collisionObject || _slots[i].ObjectB == collisionObject)
          {
            var contactSet = _slots[i].ContactSet;
            if (removedContactSets != null)
              removedContactSets.Add(contactSet);

            Remove(contactSet);
            removed = true;
          }
        }
      }

      return removed;
    }


    /// <summary>
    /// Removes the contact set for the given pair of collision objects.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <returns>
    /// The <see cref="ContactSet"/> that was removed; <see langword="null"/> if no matching 
    /// <see cref="ContactSet"/> was found.
    /// </returns>
    public ContactSet Remove(CollisionObject objectA, CollisionObject objectB)
    {
      if (objectA == null || objectB == null)
        return null;

      int hashCode = GetHashCode(objectA, objectB);
      return Remove(objectA, objectB, hashCode);
    }


    /// <summary>
    /// Removes the contact set for the given pair of <see cref="CollisionObject"/>s.
    /// </summary>
    /// <param name="collisionObjectPair">The collision object pair.</param>
    /// <returns>
    /// The <see cref="ContactSet"/> that was removed; <see langword="null"/> if no matching 
    /// <see cref="ContactSet"/> was found.
    /// </returns>
    public ContactSet Remove(Pair<CollisionObject> collisionObjectPair)
    {
      return Remove(collisionObjectPair.First, collisionObjectPair.Second);
    }


    private void Resize(int newSize)
    {
      // Hash table size needs to be prime.
      newSize = PrimeHelper.NextPrime(newSize);

      var newTable = new int[newSize];
      var newSlots = new Slot[newSize];

      // Copy current slots.
      Array.Copy(_slots, 0, newSlots, 0, _touchedSlots);

      // Update hash table and rebuild chains.
      for (int slotIndex = 0; slotIndex < _touchedSlots; slotIndex++)
      {
        int hashCode = GetHashCode(newSlots[slotIndex].ObjectA, newSlots[slotIndex].ObjectB);
        int tableIndex = hashCode % newSize;
        newSlots[slotIndex].Next = newTable[tableIndex] - 1;
        newTable[tableIndex] = slotIndex + 1;
      }

      _table = newTable;
      _slots = newSlots;

      if (_used != null)
      {
        var newUsed = new bool[newSize];
        Array.Copy(_used, 0, newUsed, 0, _touchedSlots);
        _used = newUsed;
      }
    }


    /// <summary>
    /// Copies the contact sets of the collection to a new array.
    /// </summary>
    /// <returns>
    /// An array containing the contact sets.
    /// </returns>
    public ContactSet[] ToArray()
    {
      var array = new ContactSet[_count];
      CopyTo(array, 0);
      return array;
    }


    /// <overloads>
    /// <summary>
    /// Gets the contact sets for the given pair of collision objects.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the contact sets for the given pair of collision objects.
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <param name="contactSet">
    /// The contact set with contacts between <paramref name="objectA"/> and 
    /// <paramref name="objectB"/>, if such a contact set exists in the collection - otherwise, 
    /// <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the collections contains a contact set for the specified pair of 
    /// collision objects; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet(CollisionObject objectA, CollisionObject objectB, out ContactSet contactSet)
    {
      if (objectA == null || objectB == null)
      {
        contactSet = null;
        return false;
      }

      int hashCode = GetHashCode(objectA, objectB);
      int tableIndex = hashCode % _table.Length;
      int slotIndex = GetSlotIndex(tableIndex, objectA, objectB);
      if (slotIndex >= 0)
      {
        contactSet = _slots[slotIndex].ContactSet;
        return true;
      }

      contactSet = null;
      return false;
    }


    /// <summary>
    /// Gets the contact set for the given pair of collision objects.
    /// </summary>
    /// <param name="collisionObjectPair">The collision object pair.</param>
    /// <param name="contactSet">
    /// The contact set with contacts between the given collision object pair, if such a contact set
    /// exists in the collection - otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the collections contains a contact set for the specified pair of 
    /// collision objects; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet(Pair<CollisionObject> collisionObjectPair, out ContactSet contactSet)
    {
      return TryGet(collisionObjectPair.First, collisionObjectPair.Second, out contactSet);
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "ContactSetCollection {{ Count = {0} }}", _count);
    }


    #region ----- IEnumerable, IEnumerable<T> -----

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<ContactSet> IEnumerable<ContactSet>.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    public Enumerator GetEnumerator()
    {
      // Use struct Enumerator.
      return new Enumerator(this);
    }
    #endregion

    #endregion


    //--------------------------------------------------------------
    #region Linked Lists
    //--------------------------------------------------------------

    // Contact sets are stored in linked lists for fast access (one linked list 
    // per collision object):
    // - CollisionObject.ContactSets is the head of the list.
    // - ContactSets.NextA stores the next ContactSet of ObjectA.
    // - ContactSets.NextB stores the next ContactSet of ObjectB.
    // - Linked lists are only used when the collection is exclusively owned by 
    //   the collision domain! (See _ownedByDomain.)

    private void ClearLinkedLists()
    {
      Debug.Assert(_ownedByDomain, "No linked lists. Collection does not belong to a collision domain.");

      for (int i = 0; i < _touchedSlots; i++)
      {
        var contactSet = _slots[i].ContactSet;
        if (contactSet != null)
        {
          contactSet.ObjectA.ContactSets = null;
          contactSet.ObjectB.ContactSets = null;
          contactSet.NextA = null;
          contactSet.NextB = null;
        }
      }
    }


    private static void AddToLinkedLists(ContactSet contactSet)
    {
      Debug.Assert(contactSet != null, "Null values are not allowed in the collection.");
      Debug.Assert(contactSet.ObjectA != null, "Invalid contact set. The first collision object is null.");
      Debug.Assert(contactSet.ObjectB != null, "Invalid contact set. The second collision object is null.");
      Debug.Assert(contactSet.NextA == null, "The contact set is already stored in the collection.");
      Debug.Assert(contactSet.NextB == null, "The contact set is already stored in the collection.");

      // Prepend contact set to linked lists.
      contactSet.NextA = contactSet.ObjectA.ContactSets;
      contactSet.ObjectA.ContactSets = contactSet;
      contactSet.NextB = contactSet.ObjectB.ContactSets;
      contactSet.ObjectB.ContactSets = contactSet;
    }


    private static void RemoveFromLinkedLists(ContactSet contactSet)
    {
      Debug.Assert(contactSet != null, "Null values are not allowed in the linked lists.");
      Debug.Assert(contactSet.ObjectA != null, "Invalid contact set. The first collision object is null.");
      Debug.Assert(contactSet.ObjectB != null, "Invalid contact set. The second collision object is null.");

      Unlink(contactSet.ObjectA, contactSet, contactSet.NextA);
      Unlink(contactSet.ObjectB, contactSet, contactSet.NextB);
      contactSet.NextA = null;
      contactSet.NextB = null;
    }


    private static void Unlink(CollisionObject collisionObject, ContactSet contactSet, ContactSet next)
    {
      Debug.Assert(collisionObject != null, "The collision object must not be null.");
      Debug.Assert(collisionObject.ContactSets != null, "The linked list should not be empty.");

      // Locate contact set in linked list.
      ContactSet previous = null;
      ContactSet current = collisionObject.ContactSets; // Head of list.
      while (current != contactSet)
      {
        previous = current;
        if (current.ObjectA == collisionObject)
        {
          current = current.NextA;
        }
        else
        {
          Debug.Assert(current.ObjectB == collisionObject);
          current = current.NextB;
        }
      }

      Debug.Assert(current == contactSet, "Contact set not found in linked list.");

      // Unlink contact set.
      if (previous == null)
      {
        // The contact set is the head of the list.
        collisionObject.ContactSets = next;
      }
      else
      {
        // The contact set is in the middle or at the end of the list.
        if (previous.ObjectA == collisionObject)
        {
          previous.NextA = next;
        }
        else
        {
          Debug.Assert(previous.ObjectB == collisionObject);
          previous.NextB = next;
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Broad Phase Support
    //--------------------------------------------------------------

    // The following methods are used by the broad phase to synchronize the self-overlaps
    // of the spatial partition with the candidate pairs.

    /// <summary>
    /// Adds a new contact set for the specified pair, bypassing any checks.
    /// </summary>
    /// <param name="pair">The pair of collision objects.</param>
    internal void Add(Pair<CollisionObject> pair)
    {
      var objectA = pair.First;
      var objectB = pair.Second;

      Debug.Assert(objectA != null && objectB != null, "Collision objects must not be null.");

      int hashCode = GetHashCode(objectA, objectB);
      int tableIndex = hashCode % _table.Length;

      Debug.Assert(GetSlotIndex(tableIndex, objectA, objectB) == -1, "The contact set with the given collision objects is already contained in the collection.");

      var contactSet = ContactSet.Create(objectA, objectB);
      Add(tableIndex, hashCode, objectA, objectB, contactSet);
    }


    /// <summary>
    /// Adds a new contact set to the collection, or marks the contact set as used if it already
    /// exists.
    /// </summary>
    /// <param name="overlap">The overlap.</param>
    internal void AddOrMarkAsUsed(Pair<CollisionObject> overlap)
    {
      var objectA = overlap.First;
      var objectB = overlap.Second;
      int hashCode = GetHashCode(objectA, objectB);
      int tableIndex = hashCode % _table.Length;
      int slotIndex = GetSlotIndex(tableIndex, objectA, objectB);
      if (slotIndex == -1)
      {
        // No matching entry found. Add new contact set to collection.
        var contactSet = ContactSet.Create(objectA, objectB);
        slotIndex = Add(tableIndex, hashCode, objectA, objectB, contactSet);
      }

      // Mark entry as used.
      // Use additional array to mark entries which are in the overlaps collection.
      if (_used == null)
        _used = new bool[_table.Length];

      _used[slotIndex] = true;
    }


    /// <summary>
    /// Removes all contact sets which are not marked as used..
    /// </summary>
    /// <param name="removedContactSets">The removed contact sets.</param>
    internal void RemoveUnused(List<ContactSet> removedContactSets)
    {
      if (_used == null)
        _used = new bool[_table.Length];

      // Remove all obsolete contact sets.
      for (int i = 0; i < _touchedSlots; i++)
      {
        var contactSet = _slots[i].ContactSet;
        if (contactSet != null && !_used[i])
        {
          // Slot contains contact set, which is no longer used.
          removedContactSets.Add(contactSet);
          Remove(contactSet);
        }
      }

      Array.Clear(_used, 0, _touchedSlots);
    }
    #endregion
  }
}
