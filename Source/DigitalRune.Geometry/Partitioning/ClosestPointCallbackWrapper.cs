// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Wraps a closest-point callback to reduce calls of 
  /// <see cref="ISupportClosestPointQueries{T}.GetClosestPointCandidates(Vector3F,Pose,ISpatialPartition{T},Vector3F,Pose,Func{T,T,float})"/>
  /// to calls of <see cref="ISupportClosestPointQueries{T}.GetClosestPointCandidates(Aabb,float,Func{T,float})"/>.
  /// </summary>
  /// <typeparam name="T">The type of items in the spatial partition.</typeparam>
  /// <remarks>
  /// Note: We could simply use lambda expressions (closures) instead of 
  /// <see cref="ClosestPointCallbackWrapper{T}"/>, but we want to avoid unnecessary garbage.
  /// </remarks>
  internal sealed class ClosestPointCallbackWrapper<T> : IRecyclable
  {
    // ReSharper disable StaticFieldInGenericType
    private static readonly ResourcePool<ClosestPointCallbackWrapper<T>> Pool =
      new ResourcePool<ClosestPointCallbackWrapper<T>>(
        () => new ClosestPointCallbackWrapper<T>(),
        null,
        null);
    // ReSharper restore StaticFieldInGenericType


    /// <summary>
    /// Gets or sets the original callback.
    /// </summary>
    /// <value>
    /// The original callback used in 
    /// <see cref="ISupportClosestPointQueries{T}.GetClosestPointCandidates(Vector3F,Pose,ISpatialPartition{T},Vector3F,Pose,Func{T,T,float})"/>.
    /// </value>
    public Func<T, T, float> OriginalCallback { get; set; }


    /// <summary>
    /// Gets or sets the current item.
    /// </summary>
    /// <value>The current item.</value>
    public T Item { get; set; }


    /// <summary>
    /// Prevents a default instance of the <see cref="ClosestPointCallbackWrapper{T}"/> class from 
    /// being created.
    /// </summary>
    private ClosestPointCallbackWrapper()
    {
    }


    /// <summary>
    /// Creates an instance of the <see cref="ClosestPointCallbackWrapper{T}"/> class. (This method 
    /// reuses a previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="ClosestPointCallbackWrapper{T}"/> class.
    /// </returns>
    public static ClosestPointCallbackWrapper<T> Create()
    {
      return Pool.Obtain();
    }


    /// <summary>
    /// Recycles this instance of the <see cref="ClosestPointCallbackWrapper{T}"/> class.
    /// </summary>
    public void Recycle()
    {
      OriginalCallback = null;
      Item = default(T);
      Pool.Recycle(this);
    }


    /// <summary>
    /// The callback which can be used in <see cref="ISupportClosestPointQueries{T}.GetClosestPointCandidates(Aabb,float,Func{T,float})"/>.
    /// </summary>
    /// <param name="otherItem">The candidate item.</param>
    /// <returns>The closest point distance.</returns>
    public float Callback(T otherItem)
    {
      return OriginalCallback(Item, otherItem);
    }
  }
}
