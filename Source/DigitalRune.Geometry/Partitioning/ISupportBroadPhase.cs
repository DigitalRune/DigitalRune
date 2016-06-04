// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Indicates that the <see cref="ISpatialPartition{T}"/> has special support for a collision 
  /// detection broad phase.
  /// </summary>
  /// <typeparam name="T">The type of objects managed in the spatial partition.</typeparam>
  /// <remarks>
  /// If a spatial partition that implements this interface is used for the broad phase of a 
  /// collision domain, the collision domain update will perform faster.
  /// </remarks>
  internal interface ISupportBroadPhase<T>
  {
    /// <summary>
    /// Gets or sets the collision detection broad phase.
    /// </summary>
    /// <value>The collision detection broad phase.
    /// </value>
    IBroadPhase<T> BroadPhase { get; set; }
  }


  /// <summary>
  /// Represents the collision detection broad phase which stores candidate pairs.
  /// </summary>
  /// <typeparam name="T">The type of the collision object.</typeparam>
  internal interface IBroadPhase<T>
  {
    /// <summary>
    /// Removes all candidate pairs.
    /// </summary>
    void Clear();


    /// <summary>
    /// Adds the specified candidate pair.
    /// </summary>
    /// <param name="overlap">The overlapping pair of collision objects.</param>
    void Add(Pair<T> overlap);


    /// <summary>
    /// Removes all candidate pair that include the specified collision object.
    /// </summary>
    /// <param name="collisionObject">The collision objects to remove.</param>
    void Remove(T collisionObject);


    /// <summary>
    /// Removes the specified candidate pair.
    /// </summary>
    /// <param name="overlap">The overlapping pair of collision objects.</param>
    void Remove(Pair<T> overlap);


    /// <summary>
    /// Adds a new candidate pair, or marks an existing pair as "used".
    /// </summary>
    /// <param name="overlap">The overlapping pair of collision objects.</param>
    void AddOrMarkAsUsed(Pair<T> overlap);


    /// <summary>
    /// Removes all candidate pairs which are not marked as "used".
    /// </summary>
    void RemoveUnused();
  }
}
