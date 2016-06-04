// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// Represents a collection of <see cref="CollisionObject"/>s.
  /// </summary>
  public class CollisionObjectCollection : NotifyingCollection<CollisionObject>
  {
    // Note: AllowDuplicates is true, because don't need to check for duplicates in the base class.
    // We check whether CollisionObject.Domain is set to avoid duplicates.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// A lookup table that maps <see cref="IGeometricObject"/> objects to their 
    /// <see cref="CollisionObject"/>s.
    /// </summary>
    private Dictionary<IGeometricObject, CollisionObject> _lookupTable;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether the internal lookup table is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the internal lookup table is enabled; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The <see cref="Get"/> method can be used to look up a <see cref="CollisionObject"/> by
    /// specifying its <see cref="IGeometricObject"/>. By default the lookup is done by performing a
    /// linear search through all <see cref="CollisionObject"/>s, which is O(n). By setting 
    /// <see cref="EnableLookupTable"/> to <see langword="true"/> an internal lookup table is 
    /// created to speedup the lookup at the cost of additional memory. The lookup using the 
    /// internal lookup table is close to O(1).
    /// </para>
    /// <para>
    /// The lookup table can be enabled or disabled at any time. (However, enabling the lookup table
    /// costs some time because all <see cref="CollisionObject"/>s need to be copied into the 
    /// lookup table.)
    /// </para>
    /// </remarks>
    public bool EnableLookupTable
    {
      get { return _enableLookupTable; }
      set
      {
        if (_enableLookupTable == value)
          return;

        _enableLookupTable = value;
        if (_enableLookupTable)
        {
          // Create lookup table and copy all CollisionObjects into the table.
          _lookupTable = new Dictionary<IGeometricObject, CollisionObject>();
          foreach (CollisionObject collisionObject in this)
            _lookupTable.Add(collisionObject.GeometricObject, collisionObject);
        }
        else
        {
          // Remove lookup table.
          _lookupTable = null;
        }
      }
    }
    private bool _enableLookupTable;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionObjectCollection"/> class.
    /// </summary>
    internal CollisionObjectCollection() : base(false, true)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void ClearItems()
    {
      base.ClearItems();

      if (_enableLookupTable)
        _lookupTable.Clear();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void InsertItem(int index, CollisionObject item)
    {
      if (item != null && item.Domain != null)
        throw new InvalidOperationException("Cannot add object to collision domain. The object is already part of a collision domain.");

      base.InsertItem(index, item);

      if (_enableLookupTable)
      {
        Debug.Assert(item != null, "Base class should throw exception if item is null");
        _lookupTable[item.GeometricObject] = item;
      }
    }


    /// <inheritdoc/>
    protected override void RemoveItem(int index)
    {
      if (_enableLookupTable)
      {
        CollisionObject removedItem = this[index];
        _lookupTable.Remove(removedItem.GeometricObject);
      }

      base.RemoveItem(index);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void SetItem(int index, CollisionObject item)
    {
      if (item != null && item.Domain != null)
        throw new InvalidOperationException("Cannot add object to collision domain. The object is already part of a collision domain.");

      CollisionObject removedItem = this[index];

      base.SetItem(index, item);

      if (_enableLookupTable)
      {
        Debug.Assert(item != null, "Base class should throw exception if item is null");
        _lookupTable[item.GeometricObject] = item;
        _lookupTable.Remove(removedItem.GeometricObject);
      }
    }


    /// <summary>
    /// Gets the collision object for the specified geometric object.
    /// </summary>
    /// <param name="geometricObject">The <see cref="IGeometricObject"/>.</param>
    /// <returns>
    /// The <see cref="CollisionObject"/> of <paramref name="geometricObject"/>, or 
    /// <see langword="null"/> if the collision domain does not contain a 
    /// <see cref="CollisionObject"/> for <paramref name="geometricObject"/>.
    /// </returns>
    /// <remarks>
    /// This method can be used to lookup a <see cref="CollisionObject"/> by specifying its 
    /// <see cref="IGeometricObject"/>. By default, this method performs a linear search over all 
    /// items to find the <see cref="CollisionObject"/> which is O(n). The lookup can be speed up by
    /// setting <see cref="EnableLookupTable"/> to <see langword="true"/>. In this case a internal
    /// lookup table is created that maps the <see cref="IGeometricObject"/> object to their 
    /// <see cref="CollisionObject"/>s. The lookup when using the internal lookup table is close to
    /// O(1).
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="geometricObject"/> is <see langword="true"/>.
    /// </exception>
    public CollisionObject Get(IGeometricObject geometricObject)
    {
      if (geometricObject == null)
        throw new ArgumentNullException("geometricObject");

      if (_enableLookupTable)
      {
        // Great, we use our lookup table.
        CollisionObject collisionObject;
        _lookupTable.TryGetValue(geometricObject, out collisionObject);
        return collisionObject;
      }

      // No lookup table: Let's perform a linear search.
      foreach (CollisionObject collisionObject in this)
        if (collisionObject.GeometricObject == geometricObject)
          return collisionObject;
      
      return null;
    }
    #endregion
  }
}
