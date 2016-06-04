// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using DigitalRune.Collections;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// A basic collision filter supporting pairwise filtering and collision groups.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Per default, all collisions are enabled. Collisions can be disabled for pairs of
  /// <see cref="CollisionObject"/>s, for a pair of collision groups, or for a whole collision
  /// group. Collision groups are identified by an <see cref="int"/> ID, stored in the
  /// <see cref="CollisionObject"/> (see <see cref="CollisionObject.CollisionGroup"/>).
  /// Per default, the collision filter supports only collision group IDs in the range 0-31. This
  /// limit can be changed in the constructor (see <see cref="CollisionFilter(int)"/>).
  /// </para>
  /// <para>
  /// Two collision objects A and B will NOT collide if one of the following conditions is met:
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// Collisions for the collision group of A are disabled (see <see cref="Set(int,bool)"/>).
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Collisions for the collision group of B are disabled (see <see cref="Set(int,bool)"/>).
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Collisions between the collision group of A and the collision group of B are disabled (see
  /// <see cref="Set(int,int,bool)"/>).
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Collisions between collision object A and B are disabled (see
  /// <see cref="Set(CollisionObject,CollisionObject,bool)"/>).
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  public class CollisionFilter : ICollisionFilter
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly bool[] _groupFlags;

    // Pairwise group flags: 
    // Use jagged array [i][j] for lower triangular matrix where i ≥ j.
    private readonly bool[][] _groupPairFlags;

    // Pairwise filtering. If object pair is contained, filtering is disabled.
    private readonly HashSet<Pair<CollisionObject>> _disabledPairs;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// The maximum number of supported collision groups.
    /// </summary>
    /// <remarks>
    /// Collision group numbers must be in the range 0 - (<see cref="MaxNumberOfGroups"/> - 1).
    /// This limit can be changed in the constructor (see <see cref="CollisionFilter(int)"/>).
    /// </remarks>
    public int MaxNumberOfGroups
    {
      get { return _groupFlags.Length; }
    }


    /// <summary>
    /// Occurs when the filter rules were changed.
    /// </summary>
    public event EventHandler<EventArgs> Changed;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionFilter"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionFilter"/> class for max. 32 
    /// different collision groups.
    /// </summary>
    public CollisionFilter()
      : this(32)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionFilter"/> class for the given
    /// max. number of collision groups.
    /// </summary>
    /// <param name="maxNumberOfGroups">
    /// The maximum number of groups (see <see cref="MaxNumberOfGroups"/>).
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxNumberOfGroups"/> is negative.
    /// </exception>
    public CollisionFilter(int maxNumberOfGroups)
    {
      if (maxNumberOfGroups < 0)
        throw new ArgumentOutOfRangeException("maxNumberOfGroups", "The max number of collision groups must be greater than or equal to 0.");

      _groupFlags = new bool[maxNumberOfGroups];
      _groupPairFlags = new bool[maxNumberOfGroups][];
      for (int i = 0; i < maxNumberOfGroups; i++)
        _groupPairFlags[i] = new bool[i + 1];

      _disabledPairs = new HashSet<Pair<CollisionObject>>();

      // Per default all collisions are enabled.
      ResetInternal();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Resets this filter. All collisions will be enabled.
    /// </summary>
    public virtual void Reset()
    {
      ResetInternal();
      OnChanged(EventArgs.Empty);
    }


    private void ResetInternal()
    {
      _disabledPairs.Clear();

      for (int i = 0; i < _groupFlags.Length; i++)
        _groupFlags[i] = true;

      for (int i = 0; i < _groupPairFlags.Length; i++)
      {
        var row = _groupPairFlags[i];
        for (int j = 0; j < row.Length; j++)
          row[j] = true;
      }
    }


    /// <inheritdoc/>
    public void Set(CollisionObject objectA, CollisionObject objectB, bool collisionsEnabled)
    {
      if (collisionsEnabled == false)
        _disabledPairs.Add(new Pair<CollisionObject>(objectA, objectB));
      else
        _disabledPairs.Remove(new Pair<CollisionObject>(objectA, objectB));

      OnChanged(EventArgs.Empty);
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="collisionGroup"/> is out of range.
    /// </exception>
    public void Set(int collisionGroup, bool collisionsEnabled)
    {
      if (collisionGroup < 0 || collisionGroup > MaxNumberOfGroups - 1)
        throw new ArgumentOutOfRangeException("collisionGroup", String.Format(CultureInfo.InvariantCulture, "The number of the collision group must be a value from 0 to {0}.", MaxNumberOfGroups - 1));

      _groupFlags[collisionGroup] = collisionsEnabled;

      OnChanged(EventArgs.Empty);
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="groupA"/> is out of range.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="groupB"/> is out of range.
    /// </exception>
    public void Set(int groupA, int groupB, bool collisionsEnabled)
    {
      if (groupA < 0 || groupA > MaxNumberOfGroups - 1)
        throw new ArgumentOutOfRangeException("groupA", String.Format(CultureInfo.InvariantCulture, "The number of the collision group must be a value from 0 to {0}.", MaxNumberOfGroups - 1));
      if (groupB < 0 || groupB > MaxNumberOfGroups - 1)
        throw new ArgumentOutOfRangeException("groupB", String.Format(CultureInfo.InvariantCulture, "The number of the collision group must be a value from 0 to {0}.", MaxNumberOfGroups - 1));

      if (groupA >= groupB)
        _groupPairFlags[groupA][groupB] = collisionsEnabled;
      else
        _groupPairFlags[groupB][groupA] = collisionsEnabled;

      OnChanged(EventArgs.Empty);
    }


    /// <inheritdoc/>
    public bool Get(CollisionObject objectA, CollisionObject objectB)
    {
      return !_disabledPairs.Contains(new Pair<CollisionObject>(objectA, objectB));
    }


    /// <inheritdoc/>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="collisionGroup"/> is out of range.
    /// </exception>
    public bool Get(int collisionGroup)
    {
      return _groupFlags[collisionGroup];
    }


    /// <inheritdoc/>
    /// <exception cref="IndexOutOfRangeException">
    /// <paramref name="groupA"/> or <paramref name="groupB"/> is out of range.
    /// </exception>
    public bool Get(int groupA, int groupB)
    {
      if (groupA >= groupB)
        return _groupPairFlags[groupA][groupB];
      else
        return _groupPairFlags[groupB][groupA];
    }


    /// <summary>
    /// Determines whether the given <see cref="CollisionObject"/>s can collide.
    /// </summary>
    /// <param name="pair">The pair of collision objects.</param>
    /// <returns>
    /// <see langword="true"/> if the pair of collision objects can collide; otherwise, 
    /// <see langword="false"/> if the objects cannot collide.
    /// </returns>
    public virtual bool Filter(Pair<CollisionObject> pair)
    {
      return Get(pair.First.CollisionGroup) && Get(pair.Second.CollisionGroup)   // Are collision groups enabled.
             && Get(pair.First.CollisionGroup, pair.Second.CollisionGroup)       // Collision group pair flag.
             && Get(pair.First, pair.Second);                                    // Object pair flag.
    }


    /// <summary>
    /// Raises the <see cref="Changed"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnChanged"/> in a derived
    /// class, be sure to call the base class's <see cref="OnChanged"/> method so that registered
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnChanged(EventArgs eventArgs)
    {
      var handler = Changed;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
