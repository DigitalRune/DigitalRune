// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Collections;


namespace DigitalRune
{
  /// <summary>
  /// Manages a pool of reusable items (base implementation).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A resource pool can be used to remove load from the .NET garbage collector: Instead of 
  /// allocating objects with <c>new</c>, objects are taken (see 
  /// <see cref="ResourcePool{T}.Obtain"/>) from a pool of objects when the are needed and returned
  /// (see <see cref="ResourcePool{T}.Recycle"/>) when they are no longer needed.
  /// </para>
  /// <para>
  /// It is safe to obtain multiple objects without recycling them. In this case the objects are
  /// simply collected by the .NET garbage collector. However, the benefits of resource pooling are 
  /// lost. To achieve optimal performance all objects, which are taken from the resource pool,
  /// should be returned to the resource pool.
  /// </para>
  /// <para>
  /// <strong>Performance Considerations:</strong> Resource pooling should only be applied if there
  /// is strong evidence that the .NET garbage collector poses a performance bottleneck. On 
  /// platforms, such as Windows, with a generational garbage collector it is in most cases 
  /// counterproductive to use a resource pool. By keeping a large pool of objects on the heap, the
  /// time of single garbage collection is increased and the overhead created by the resource 
  /// pooling is worse than allocating objects in .NET with <c>new</c>. But on some platforms, for 
  /// example the .NET Compact Framework on the Xbox 360, a resource pool can help to avoid frequent
  /// full collections by the garbage collector.
  /// </para>
  /// </remarks>
  /// <example>
  /// The following demonstrates how to create a resource pool.
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Resource pools are usually static members of a type.
  /// public static readonly ResourcePool<List<float>> MyPool = 
  ///   new ResourcePool<List<float>> (
  ///     () => new List<float>(),   // Create
  ///     null,                      // Initialize (not needed for List<float>)
  ///     list => list.Clear());     // Uninitialize
  /// ]]>
  /// </code>
  /// <para>
  /// Note: The example above creates a resource pools for <see cref="List{T}"/> for a certain type.
  /// DigitalRune already provides such a resource pool (see <see cref="ResourcePools{T}"/>), so it 
  /// is actually not required to create your own.
  /// </para>
  /// <para>
  /// The following demonstrates how to use the resource pool created in example above.
  /// </para>
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Instead of calling 
  /// //   List<float> myList = new List<float>(); 
  /// // reuse existing list from resource pool.
  /// List<float> myList = MyPool.Obtain();
  /// 
  /// // Do something with myList.
  /// ...
  /// 
  /// // Return myList to resource pool when no longer needed.
  /// MyPool.Recycle(myList);
  /// myList = null;
  /// ]]>
  /// </code>
  /// </example>
  public abstract class ResourcePool
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly WeakCollection<ResourcePool> PoolsInternal = new WeakCollection<ResourcePool>();

    private static readonly WeakEvent<EventHandler<EventArgs>> _clearingAllEvent = new WeakEvent<EventHandler<EventArgs>>();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether resource pooling is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This property can be set to <see langword="false"/> to disable resource pooling globally.
    /// When disabled, the resource pools will always return newly allocated items.
    /// </remarks>
    public static bool Enabled
    {
      get { return _enabled; }
      set
      {
        if (_enabled != value)
        {
          ClearAll();
          _enabled = value;
        }
      }
    }
    private static bool _enabled = true;


    /// <summary>
    /// Gets collection of all active <see cref="ResourcePool"/>s.
    /// </summary>
    /// <value>All active <see cref="ResourcePool"/>s.</value>
    /// <remarks>
    /// This property is intended for debugging purposes only.
    /// </remarks>
    public static IEnumerable<ResourcePool> Pools
    {
      get
      {
        lock (PoolsInternal)
        {
          return PoolsInternal.ToArray();
        }
      }
    }


    /// <summary>
    /// Occurs at the start of <see cref="ClearAll"/>.
    /// </summary>
    internal static event EventHandler<EventArgs> ClearingAll
    {
      add { _clearingAllEvent.Add(value); }
      remove { _clearingAllEvent.Remove(value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Registers the specified resource pool.
    /// </summary>
    /// <param name="pool">The resource pool to register.</param>
    internal static void Register(ResourcePool pool)
    {
      lock (PoolsInternal)
      {
        PoolsInternal.Add(pool);
      }
    }


    /// <summary>
    /// Unregisters the specified resource pool.
    /// </summary>
    /// <param name="pool">The resource pool to unregister.</param>
    internal static void Unregister(ResourcePool pool)
    {
      lock (PoolsInternal)
      {
        PoolsInternal.Remove(pool);
      }
    }


    /// <summary>
    /// Clears all resource pools.
    /// </summary>
    public static void ClearAll()
    {
      OnClearingAll(EventArgs.Empty);

      lock (PoolsInternal)
      {
        foreach (var pool in PoolsInternal)
          pool.Clear();
      }
    }


    /// <summary>
    /// Removes all items from the resource pool.
    /// </summary>
    public abstract void Clear();


    private static void OnClearingAll(EventArgs eventArgs)
    {
      _clearingAllEvent.Invoke(null, eventArgs);
    }  
    #endregion
  }
}
