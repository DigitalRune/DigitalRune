// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// A simple brute-force partitioning method used for debugging.
  /// </summary>
  /// <typeparam name="T">The type of the items in the spatial partition.</typeparam>
  /// <remarks>
  /// This spatial partition is only for debugging. It performs a simple exhaustive test of all 
  /// items against all items without acceleration structures.
  /// </remarks>
  internal sealed class DebugSpatialPartition<T> : BasePartition<T>
  {
    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override BasePartition<T> CreateInstanceCore()
    {
      return new DebugSpatialPartition<T>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(BasePartition<T> source)
    {
      base.CloneCore(source);
    }
    #endregion


    /// <inheritdoc/>
    public override IEnumerable<T> GetOverlaps(Aabb aabb)
    {
      return Items.Where(item => GeometryHelper.HaveContact(GetAabbForItem(item), aabb));
    }


    /// <inheritdoc/>
    internal override void OnUpdate(bool forceRebuild, HashSet<T> addedItems, HashSet<T> removedItems, HashSet<T> invalidItems)
    {
      if (EnableSelfOverlaps)
      {
        // Need to find all self-overlaps. 
        SelfOverlaps.Clear();

        // Test all items against all items.
        HashSet<T>.Enumerator outerEnumerator = Items.GetEnumerator();
        while (outerEnumerator.MoveNext())
        {
          T item = outerEnumerator.Current;
          Aabb aabb = GetAabbForItem(item);

          // Duplicate struct enumerator at current position.
          HashSet<T>.Enumerator innerEnumerator = outerEnumerator;
          while (innerEnumerator.MoveNext())
          {
            T otherItem = innerEnumerator.Current;
            Aabb otherAabb = GetAabbForItem(otherItem);

            Pair<T> overlap = new Pair<T>(item, otherItem);
            if (Filter == null || Filter.Filter(overlap))
              if (GeometryHelper.HaveContact(aabb, otherAabb))
                SelfOverlaps.Add(overlap);
          }
        }

        // If Items is a IList<T>, which has an indexer, we can use the following code.
        //for (int i = 0; i < Items.Count; i++)
        //{
        //  var itemI = Items[i];
        //  var aabbI = GetAabbForItem(itemI);

        //  for (int j = i + 1; j < Items.Count; j++)
        //  {
        //    var itemJ = Items[j];
        //    var aabbJ = GetAabbForItem(itemJ);

        //    var overlap = new Pair<T>(itemI, itemJ);
        //    if (Filter == null || Filter.Filter(overlap))
        //      if (GeometryHelper.HaveContact(aabbI, aabbJ))
        //        SelfOverlaps.Add(overlap);
        //  }
        //}
      }
    }
  }
}
