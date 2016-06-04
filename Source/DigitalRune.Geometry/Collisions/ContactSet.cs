// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Geometry.Collisions.Algorithms;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// A collection of <see cref="Contact"/>s that describe the contact points or closest points
  /// between to collision objects.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="ContactSet"/> collects <see cref="Contact"/>s between two
  /// <see cref="CollisionObject"/>s. A <see cref="CollisionDomain"/> creates one
  /// <see cref="ContactSet"/> for each pair of touching objects.
  /// </para>
  /// <para>
  /// <strong>Swapped Contact Sets:</strong> The order of <see cref="ObjectA"/> and
  /// <see cref="ObjectB"/> is determined in the collision detection when the contact set is
  /// created. When the collision detection returns a contact set with a method like
  /// <c>GetContacts(objectA, objectB)</c>, the objects in the contact set could be swapped such
  /// that <c>contactSet.ObjectA == objectB</c> and <c>contactSet.ObjectB == objectA</c>. If your
  /// algorithms rely on the order of <see cref="ObjectA"/> and <see cref="ObjectB"/>, for example
  /// when the contact normal vectors are used, you need to manually check whether the objects are
  /// in the expected order. By calling the property <see cref="Swapped"/> you can get a copy of the
  /// contact set where <see cref="ObjectA"/> and <see cref="ObjectB"/> are swapped and all contacts
  /// are updated accordingly.
  /// </para>
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(HaveContact = {HaveContact}, Count = {Count})")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public sealed class ContactSet : IList<Contact>, ICollection, IRecyclable
  {
    // Notes on contact swapping:
    // Allowing the user to call ContactSet.Swap() is a bad idea if others rely on the order of a and 
    // b, especially cached physics data or similar things. It is possible that same contact set is 
    // referenced from different places, therefore the order of a and b should be constant.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Dummy CollisionObjects required for un-/initialization of ContactSets.
    private static readonly List<Contact> Empty = new List<Contact>(0);
    private static readonly ResourcePool<ContactSet> Pool =
      new ResourcePool<ContactSet>(
        () => new ContactSet(),
        null,
        null);

    private List<Contact> _contacts;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    #region ----- IList<Contact>, ICollection -----

    /// <summary>
    /// Gets the number of <see cref="Contact"/>s contained in the <see cref="ContactSet"/>.
    /// </summary>
    /// <value>
    /// The number of <see cref="Contact"/>s contained in the <see cref="ContactSet"/>.
    /// </value>
    public int Count
    {
      get { return (_contacts == null) ? 0 : _contacts.Count; }
    }


    /// <summary>
    /// Gets or sets the <see cref="Contact"/> at the specified index.
    /// </summary>
    /// <value>The contact at the specified index.</value>
    /// <param name="index">The zero-based index of the contact to get or set.</param>
    /// <remarks>
    /// <para>
    /// This indexer is an O(1) operation.
    /// </para>
    /// </remarks>
    /// <exception cref="NullReferenceException">
    /// This <see cref="ContactSet"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or equal to or greater than <see cref="Count"/>.
    /// </exception>
    public Contact this[int index]
    {
      // Note: We explicitly do not check whether _contacts is null, because of performance.
      // Calling the indexer when list is empty is wrong anyways, so let the runtime just throw
      // a NullReferenceException.
      get { return _contacts[index]; }
      set { _contacts[index] = value; }
    }


    /// <summary>
    /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
    /// </summary>
    /// <value>
    /// An object that can be used to synchronize access to the <see cref="ICollection"/>.
    /// </value>
    object ICollection.SyncRoot
    {
      get { return this; }
    }


    /// <summary>
    /// Gets a value indicating whether access to the <see cref="ICollection"/> is synchronized 
    /// (thread safe).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if access to the <see cref="ICollection"/> is synchronized (thread 
    /// safe); otherwise, <see langword="false"/>.
    /// </value>
    bool ICollection.IsSynchronized
    {
      get { return false; }
    }


    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the <see cref="ICollection{T}"/> is read-only; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    bool ICollection<Contact>.IsReadOnly
    {
      get { return false; }
    }
    #endregion


    /// <summary>
    /// Gets or sets a value indicating whether <see cref="ObjectA"/> and <see cref="ObjectB"/> are
    /// in contact.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if <see cref="ObjectA"/> and <see cref="ObjectB"/> are touching or 
    /// intersecting; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Normally, if this value is <see langword="true"/>, the <see cref="ContactSet"/> contains one
    /// or more <see cref="Contact"/>s that indicate that the objects are touching or intersecting.
    /// Two objects are touching or intersecting when the 
    /// <see cref="Contact.PenetrationDepth"/> of <see cref="Contact"/> is equal to or greater than 
    /// 0. 
    /// </para>
    /// <para>
    /// When <see cref="HaveContact"/> is <see langword="false"/> the <see cref="ContactSet"/>
    /// contains no <see cref="Contact"/> - except when the <see cref="ContactSet"/> is created by a
    /// closest point query. A closest-point query (see 
    /// <see cref="CollisionDetection.GetClosestPoints"/> or
    /// <see cref="CollisionDetection.UpdateClosestPoints"/>) returns a 
    /// <see cref="ContactSet"/> that has a single <see cref="Contact"/> where the 
    /// <see cref="Contact.PenetrationDepth"/> indicates the closest-point distance. (When the 
    /// <see cref="Contact.PenetrationDepth"/> is negative then the two objects are separated and
    /// the absolute value of the <see cref="Contact.PenetrationDepth"/> is the distance between the
    /// closest points. When <see cref="Contact.PenetrationDepth"/> is 0 the objects are touching
    /// and when the <see cref="Contact.PenetrationDepth"/> is positive the objects are 
    /// intersecting.)
    /// </para>
    /// <para>
    /// In certain cases <see cref="HaveContact"/> is <see langword="true"/>, but the 
    /// <see cref="ContactSet"/> is empty and does not contain any <see cref="Contact"/>s. This is 
    /// the case if either <see cref="ObjectA"/> or <see cref="ObjectB"/> is a trigger 
    /// (<see cref="CollisionObjectType"/>) or if no useful contact information could be computed
    /// because of numerical errors or other exceptions.
    /// </para>
    /// </remarks>
    public bool HaveContact { get; set; }


    /// <summary>
    /// Gets collision object A.
    /// </summary>
    /// <value>Collision object A.</value>
    public CollisionObject ObjectA { get; private set; }


    /// <summary>
    /// Gets collision object B.
    /// </summary>
    /// <value>Collision object B.</value>
    public CollisionObject ObjectB { get; private set; }


    /// <summary>
    /// Gets a copy of the contact set where <see cref="ObjectA"/> and <see cref="ObjectB"/> are
    /// swapped.
    /// </summary>
    /// <remarks>
    /// This method copies the contact set, exchanges <see cref="ObjectA"/> and 
    /// <see cref="ObjectB"/> and updates the contact information accordingly.
    /// </remarks>
    public ContactSet Swapped
    {
      get
      {
        ContactSet swapped = Create(ObjectB, ObjectA);
        swapped.PreferredNormal = PreferredNormal;
        swapped.HaveContact = HaveContact;
        swapped.IsPerturbationTestAllowed = IsPerturbationTestAllowed;
        swapped.IsValid = IsValid;

        int numberOfContacts = Count;
        for (int i = 0; i < numberOfContacts; i++)
          swapped.Add(_contacts[i].Swapped);

        return swapped;
      }
    }


    /// <summary>
    /// Gets or sets a value indicating whether this instance is valid.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This flag can be set to <see langword="false"/> to tell the collision domain, that it has to 
    /// recompute this contact set. The flag will be set to <see langword="true"/> automatically 
    /// when the contact set is updated.
    /// </remarks>
    internal bool IsValid { get; set; }


    /// <summary>
    /// Gets or sets the preferred normal direction.
    /// </summary>
    /// <value>
    /// The preferred normal direction. The default value is <see cref="Vector3F.Zero"/>. If the 
    /// value is a vector other than <see cref="Vector3F.Zero"/> it needs to be normalized.
    /// </value>
    /// <remarks>
    /// If this value is a vector other than <see cref="Vector3F.Zero"/>, the vector is used as the
    /// preferred normal direction. The preferred normal direction is a hint that can be used by 
    /// collision algorithms to return better contact points. Collision algorithms which support 
    /// this feature will try to return a contact point with a normal that is close to 
    /// <see cref="PreferredNormal"/>. Currently, only the MPR collision algorithm supports this 
    /// feature (see <see cref="MinkowskiPortalRefinement"/> for details).
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// <paramref name="value"/> is not normalized. The vector needs to be either (0, 0, 0) or a 
    /// normalized vector.
    /// </exception>
    internal Vector3F PreferredNormal
    {
      get { return _preferredNormal; }
      set
      {
        if (value != Vector3F.Zero && !value.IsNumericallyNormalized)
          throw new ArgumentException("Preferred normal must be normalized.", "value");

        _preferredNormal = value;
      }
    }
    private Vector3F _preferredNormal;


    /// <summary>
    /// Gets a value indicating whether <see cref="PreferredNormal"/> is set.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if <see cref="PreferredNormal"/> is available; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    internal bool IsPreferredNormalAvailable
    {
      get { return _preferredNormal != Vector3F.Zero; }
    }


    internal bool IsPerturbationTestAllowed { get; set; }


    // Caches the collision filter result. This flag is set by the collision domain.
    // -1 = not set, 0 = do not collide, 1 = do collide
    internal int CanCollide;

    // Caches the CollisionAlgorithm from the CollisionAlgorithmMatrix.
    internal CollisionAlgorithm CollisionAlgorithm;

    // Contact sets of a collision object are stored in linked lists. The collision 
    // object stores the head of the list. The contact set itself is oblivious to 
    // this list. The list is managed completely by the ContactSetCollection!
    internal ContactSet NextA;  // The next ContactSet containing ObjectA.
    internal ContactSet NextB;  // The next ContactSet containing ObjectB.
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Prevents a default instance of the <see cref="ContactSet"/> class from being created.
    /// </summary>
    private ContactSet()
    {
    }


    /// <summary>
    /// Creates an instance of the <see cref="ContactSet"/> class. (This method 
    /// reuses a previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <param name="objectA">The object A.</param>
    /// <param name="objectB">The object B.</param>
    /// <returns>A new or reusable instance of the <see cref="ContactSet"/> class.</returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap. 
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle()"/> when the instance is no longer 
    /// needed.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="objectA"/> and <paramref name="objectB"/> are the same.
    /// </exception>
    public static ContactSet Create(CollisionObject objectA, CollisionObject objectB)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");
      if (objectA == objectB)
        throw new ArgumentException("The collision objects of the contact set must not be identical.");

      var contactSet = Pool.Obtain();
      contactSet.ObjectA = objectA;
      contactSet.ObjectB = objectB;
      contactSet.IsValid = true;

      contactSet.IsPerturbationTestAllowed = true;
      contactSet.CanCollide = -1;

      return contactSet;
    }


    /// <overloads>
    /// <summary>
    /// Recycles this instance of the <see cref="ContactSet"/> class.
    /// </summary>
    /// </overloads>
    ///
    /// <summary>
    /// Recycles this instance of the <see cref="ContactSet"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// <para>
    /// This method does not recycle the contained <see cref="Contact"/>s.
    /// </para>
    /// </remarks>
    public void Recycle()
    {
      Debug.Assert(NextA == null && NextB == null, "Cannot recycle contact set. The object is still in use.");
      Reset(null, null);
      Pool.Recycle(this);
    }


    /// <summary>
    /// Recycles this instance of the <see cref="ContactSet" /> class.
    /// </summary>
    /// <param name="recycleContacts">
    /// If set to <see langword="true" />, the contained <see cref="Contact"/>s are also recycled; 
    /// If set to <see langword="false"/>, the contacts are not recycled.</param>
    /// <remarks>
    /// This method resets this instance and returns it to a resource pool if resource pooling is
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </remarks>
    public void Recycle(bool recycleContacts)
    {
      Debug.Assert(NextA == null && NextB == null, "Cannot recycle contact set. The object is still in use.");

      if (recycleContacts && _contacts != null)
      {
        var numberOfContacts = _contacts.Count;
        for (int i = 0; i < numberOfContacts; i++)
          _contacts[i].Recycle();
      }

      Reset(null, null);
      Pool.Recycle(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void EnsureList()
    {
      if (_contacts == null)
        _contacts = DigitalRune.ResourcePools<Contact>.Lists.Obtain();
    }


    /// <summary>
    /// Resets this contact set to default values. 
    /// </summary>
    /// <param name="objectA">The first collision object.</param>
    /// <param name="objectB">The second collision object.</param>
    /// <remarks>
    /// This method allows to re-use an existing contact set instead of allocating a new instance on 
    /// the heap. This avoids garbage on the heap. In general, this method must only be used by the
    /// creator of this instance. If the <see cref="ContactSet"/> was created by the collision
    /// detection, this method should not be used.
    /// </remarks>
    public void Reset(CollisionObject objectA, CollisionObject objectB)
    {
      ObjectA = objectA;
      ObjectB = objectB;
      IsValid = true;
      PreferredNormal = Vector3F.Zero;
      HaveContact = false;
      IsPerturbationTestAllowed = true;
      CanCollide = -1;
      CollisionAlgorithm = null;
      Clear();
    }


    #region ----- IList<Contact>, ICollection -----

    /// <summary>
    /// Adds a <see cref="Contact"/> to the end of the <see cref="ContactSet"/>.
    /// </summary>
    /// <param name="item">
    /// The contact to add to the <see cref="ContactSet"/>.
    /// </param>
    public void Add(Contact item)
    {
      EnsureList();
      _contacts.Add(item);
    }


    /// <summary>
    /// Returns a read-only <see cref="IList{T}"/> wrapper for the current <see cref="ContactSet"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="ReadOnlyCollection{T}"/> that acts as a read-only wrapper around the current
    /// <see cref="ContactSet"/>. 
    /// </returns>
    public ReadOnlyCollection<Contact> AsReadOnly()
    {
      return new ReadOnlyCollection<Contact>(this);
    }


    /// <summary>
    /// Removes and recycles all <see cref="Contact"/>s from the <see cref="ContactSet"/>.
    /// </summary>
    public void Clear()
    {
      if (_contacts != null)
      {
        DigitalRune.ResourcePools<Contact>.Lists.Recycle(_contacts);
        _contacts = null;
      }
    }


    /// <summary>
    /// Determines whether the <see cref="ContactSet"/> contains a specific <see cref="Contact"/>.
    /// </summary>
    /// <param name="item">The contact to locate in the <see cref="ContactSet"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> is found in the 
    /// <see cref="ContactSet"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Contact item)
    {
      return _contacts != null && _contacts.Contains(item);
    }


    /// <summary>
    /// Copies the <see cref="Contact"/>s of the <see cref="ContactSet"/> to an <see cref="Array"/>,
    /// starting at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the 
    /// <see cref="Contact"/>s copied from this <see cref="ContactSet"/>. The <see cref="Array"/>
    /// must have zero-based indexing.
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
    /// or greater than the length of <paramref name="array"/>. Or the number of contacts in the 
    /// source <see cref="ContactSet"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>. Or 
    /// type <see cref="Contact"/> cannot be cast automatically to the type of the destination 
    /// <paramref name="array"/>.
    /// </exception>
    public void CopyTo(Contact[] array, int arrayIndex)
    {
      if (_contacts != null)
        _contacts.CopyTo(array, arrayIndex);
    }


    /// <summary>
    /// Copies the <see cref="Contact"/>s of the <see cref="ContactSet"/> to an <see cref="Array"/>,
    /// starting at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the 
    /// <see cref="Contact"/>s copied from this <see cref="ContactSet"/>. The <see cref="Array"/>
    /// must have zero-based indexing.
    /// </param>
    /// <param name="index">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> is multidimensional. Or <paramref name="index"/> is equal to or
    /// greater than the length of <paramref name="array"/>. Or the number of contacts in the 
    /// source <see cref="ContactSet"/> is greater than the available space from 
    /// <paramref name="index"/> to the end of the destination <paramref name="array"/>. Or type 
    /// <see cref="Contact"/> cannot be cast automatically to the type of the destination 
    /// <paramref name="array"/>.
    /// </exception>
    void ICollection.CopyTo(Array array, int index)
    {
      if (_contacts != null)
        ((ICollection)_contacts).CopyTo(array, index);
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<Contact> IEnumerable<Contact>.GetEnumerator()
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

    
    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ContactSet"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="ContactSet"/>.
    /// </returns>
    public List<Contact>.Enumerator GetEnumerator()
    {
      return (_contacts == null) ? Empty.GetEnumerator() : _contacts.GetEnumerator();
    }


    /// <summary>
    /// Determines the index of a specific <see cref="Contact"/> in the <see cref="ContactSet"/>.
    /// </summary>
    /// <param name="item">The contact to locate in the <see cref="ContactSet"/>.</param>
    /// <returns>
    /// The index of <paramref name="item"/> if found in the contact set; otherwise, -1.
    /// </returns>
    public int IndexOf(Contact item)
    {
      return (_contacts == null) ? -1 : _contacts.IndexOf(item);
    }


    /// <summary>
    /// Inserts an <see cref="Contact"/> in the <see cref="ContactSet"/> at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">The contact to insert into the <see cref="ContactSet"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="ContactSet"/>.
    /// </exception>
    public void Insert(int index, Contact item)
    {
      EnsureList();
      _contacts.Insert(index, item);
    }


    /// <summary>
    /// Removes the first occurrence of a specific <see cref="Contact"/> from the 
    /// <see cref="ContactSet"/>.
    /// </summary>
    /// <param name="item">The contact to remove from the <see cref="ContactSet"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the 
    /// <see cref="ContactSet"/>; otherwise, <see langword="false"/>. This method also returns 
    /// <see langword="false"/> if <paramref name="item"/> is not found in the original 
    /// <see cref="ContactSet"/>.
    /// </returns>
    public bool Remove(Contact item)
    {
      return _contacts != null && _contacts.Remove(item);
    }


    /// <summary>
    /// Removes the <see cref="Contact"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the contact to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="ContactSet"/>.
    /// </exception>
    public void RemoveAt(int index)
    {
      if (_contacts == null)
        throw new ArgumentOutOfRangeException("index", "Index is not a valid index in the contact set.");

      _contacts.RemoveAt(index);
    }
    #endregion
    

    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(
        CultureInfo.InvariantCulture,
        "ContactSet {{ ObjectA = \"{0}\", ObjectB = \"{1}\", Count = {2} }}", 
        (ObjectA != null) ? ObjectA.GeometricObject : null, 
        (ObjectB != null) ? ObjectB.GeometricObject : null, 
        Count);
    }
    #endregion
  }
}
