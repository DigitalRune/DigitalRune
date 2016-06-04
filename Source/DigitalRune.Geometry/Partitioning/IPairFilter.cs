// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Filters a pair of items.
  /// </summary>
  /// <typeparam name="T">The type of the items.</typeparam>
  /// <remarks>
  /// <para>
  /// A <see cref="IPairFilter{T}"/> is usually used to remove item pairs, so that they are not
  /// processed. <see cref="Filter"/> returns <see langword="true"/> if an item pair should be
  /// processed (item pair is accepted). <see cref="Filter"/> returns <see langword="false"/> if an
  /// item pair should not be processed (item pair is rejected).
  /// </para>
  /// <para>
  /// <strong>Notes to Implementors:</strong> The filter rules must be consistent. In most
  /// applications the order of the items in the pair should not matter. And <see cref="Filter"/>
  /// should always return the same result for the same pair. If the filter rules are changed, the 
  /// <see cref="Changed"/> event must be raised.
  /// </para>
  /// </remarks>
  public interface IPairFilter<T>
  {
    /// <summary>
    /// Filters the specified item pair.
    /// </summary>
    /// <param name="pair">The pair.</param>
    /// <returns>
    /// <see langword="true"/> if the pair should be processed (pair is accepted); otherwise,
    /// <see langword="false"/> if the pair should not be processed (pair is rejected).
    /// </returns>
    bool Filter(Pair<T> pair);


    /// <summary>
    /// Occurs when the filter rules were changed.
    /// </summary>
    event EventHandler<EventArgs> Changed;
  }  
}
