// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRune
{
  /// <summary>
  /// Provides resource pools for reusable generic collections.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the collection.</typeparam>
  /// <remarks>
  /// <para>
  /// Please read <see cref="ResourcePool{T}"/> first, to learn more about resource pooling.
  /// The static class <see cref="ResourcePools{T}"/> provides global pools for collections that
  /// are frequently used in the DigitalRune applications and libraries, but they can also be used
  /// in your application to avoid unnecessary memory allocations.
  /// </para>
  /// <para>
  /// New resource pools may be added to <see cref="ResourcePools{T}"/> in future releases of the 
  /// DigitalRune libraries.
  /// </para>
  /// <para>
  /// <strong>Thread-Safety:</strong> It is safe to access the resource pool from multiple threads
  /// simultaneously.
  /// </para>
  /// </remarks>
  /// <example>
  /// The following example shows how to reuse a generic list.
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Obtain an empty list from the global resource pool.
  /// List<string> list = ResourcePools<string>.Lists.Obtain();
  ///   
  /// // Do something with the list.
  /// 
  /// // After use, recycle the list. (Note: It is not necessary to clear the list before
  /// // recycling. This will be handled automatically.)
  /// ResourcePools<string>.Lists.Recycle(list);
  /// list = null;
  /// ]]>
  /// </code>
  /// </example>
  public static class ResourcePools<T>
  {
    // OPTIMIZE: Here is a tip to minimize the number of different resource pools.
    // Instead of List<T> use List<object> and cast the items to T where needed.
    // This reduces the number of resource pools and increases reuse of the 
    // collections. (Only works for reference types and the necessary cast 
    // operations add a slight performance overhead.)


    // ReSharper disable StaticFieldInGenericType

    /// <summary>
    /// A resource pool containing collections of type <see cref="HashSet{T}"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
    public static readonly ResourcePool<HashSet<T>> HashSets =
      new ResourcePool<HashSet<T>>(
        () => new HashSet<T>(),
        null,
        set => set.Clear());


    /// <summary>
    /// A resource pool containing collections of type <see cref="List{T}"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
    public static readonly ResourcePool<List<T>> Lists =
      new ResourcePool<List<T>>(
        () => new List<T>(),
        null,
        list => list.Clear());


    /// <summary>
    /// A resource pool containing collections of type <see cref="Stack{T}"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
    public static readonly ResourcePool<Stack<T>> Stacks =
      new ResourcePool<Stack<T>>(
        () => new Stack<T>(), 
        null,
        stack => stack.Clear());

    // ReSharper restore StaticFieldInGenericType
  }
}