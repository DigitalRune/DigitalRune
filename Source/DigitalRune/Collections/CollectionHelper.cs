// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Provides extension methods for working with collections.
  /// </summary>
  public static class CollectionHelper
  {
    internal static readonly WeakReference[] EmptyWeakReferenceArray = new WeakReference[0];


    /// <summary>
    /// Adds the specified items to the <see cref="ICollection{T}"/>. 
    /// </summary>
    ///<typeparam name="T">The type of items in the collection.</typeparam>
    ///<param name="collection">The collection to which the items should be added.</param>
    /// <param name="items">TThe items to be added.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is <see langword="null"/>.
    /// </exception>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");
      if (items == null)
        throw new ArgumentNullException("items");

      var list = items as IList<T>;
      if (list != null)
      {
        int numberOfItems = list.Count;
        for (int i = 0; i < numberOfItems; i++)
          collection.Add(list[i]);
      }
      else
      {
        foreach (T item in items)
          collection.Add(item);
      }
    }
  }
}
