// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Helper class which can be used with the <see cref="DebuggerTypeProxyAttribute"/>.
  /// </summary>
  /// <typeparam name="T">The type of items.</typeparam>
  internal sealed class CollectionDebugView<T>
  {
    private readonly ICollection<T> _collection;


    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionDebugView{T}" /> class.
    /// </summary>
    /// <param name="collection">The collection.</param>
    public CollectionDebugView(ICollection<T> collection)
    {
      _collection = collection;
    }


    /// <summary>
    /// Gets the items of the collection.
    /// </summary>
    /// <value>The items of the collection.</value>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items
    {
      get { return _collection.ToArray(); }
    }
  }
}
