// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if SILVERLIGHT
using System;
using System.Collections.Generic;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Provides extension methods for <see cref="IList{T}"/>.
  /// </summary>
  internal static class ListHelper
  {
    /// <summary>
    /// Removes the all the elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    /// <param name="list">The list.</param>
    /// <param name="match">
    /// The <see cref="Predicate{T}"/> delegate that defines the conditions of the elements to 
    /// remove.
    /// </param>
    /// <returns>The number of elements removed from the <see cref="IList{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="match"/> is <see langword="null"/>.
    /// </exception>
    public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
    {
      if (match == null)
        throw new ArgumentNullException("match");

      int numberOfRemovedItems = 0;
      for (int i = list.Count - 1; i >= 0; i--)
      {
        if (match(list[i]))
        {
          list.RemoveAt(i);
          numberOfRemovedItems++;
        }
      }

      return numberOfRemovedItems;
    }
  }
}
#endif
