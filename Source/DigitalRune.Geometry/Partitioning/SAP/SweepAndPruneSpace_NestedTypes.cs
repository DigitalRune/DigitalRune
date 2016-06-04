// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !UNITY
using System.Collections.ObjectModel;
#else
using DigitalRune.Collections.ObjectModel;
#endif
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Partitioning
{
  public partial class SweepAndPruneSpace<T>
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Represents a minimum or maximum AABB edge of an item for one axis (X, Y or Z).
    /// </summary>
    private sealed class Edge
    {
      /// <summary>
      /// The item information.
      /// </summary>
      public ItemInfo Info;


      /// <summary>
      /// The position of this AABB edge in world space.
      /// </summary>
      public float Position;


      /// <summary>
      /// A value indicating whether this edge is a maximum edge of the AABB.
      /// </summary>
      public bool IsMax;
    }


    /// <summary>
    /// Provides additional information for each item.
    /// </summary>
    private sealed class ItemInfo
    {
      /// <summary>
      /// The item.
      /// </summary>
      public T Item;


      /// <summary>
      /// The cached AABB. (Note: This is redundant because this info is also stored in the edges).
      /// </summary>
      public Aabb Aabb;


      /// <summary>
      /// The indices of the minimum AABB edges in the 3 edge lists.
      /// </summary>
      public readonly int[] MinEdgeIndices = new int[3];


      /// <summary>
      /// The indices of the maximum AABB edges in the 3 edge lists.
      /// </summary>
      public readonly int[] MaxEdgeIndices = new int[3];
    }


    /// <summary>
    /// Stores <see cref="ItemInfo"/>s for all items. Allows O(1) access of the 
    /// <see cref="ItemInfo"/> for a given item.
    /// </summary>
    private sealed class ItemInfoCollection : KeyedCollection<T, ItemInfo>
    {
      /// <summary>
      /// When implemented in a derived class, extracts the key from the specified element.
      /// </summary>
      /// <param name="info">The element from which to extract the key.</param>
      /// <returns>The key for the specified element.</returns>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
      protected override T GetKeyForItem(ItemInfo info)
      {
        return info.Item;
      }


      /// <summary>
      /// Gets the <see cref="ItemInfo"/> for the given item.
      /// </summary>
      /// <param name="item">The item.</param>
      /// <returns>
      /// The <see cref="ItemInfo"/> for <paramref name="item"/> or <see langword="null"/> if the 
      /// collection does not contain info for <paramref name="item"/>.
      /// </returns>
      public ItemInfo Get(T item)
      {
        ItemInfo itemInfo;
        Dictionary.TryGetValue(item, out itemInfo);
        return itemInfo;
      }
    }
    #endregion
  }
}
