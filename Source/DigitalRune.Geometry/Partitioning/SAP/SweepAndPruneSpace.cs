// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Sorts items using the <i>Sweep and Prune</i> method.
  /// </summary>
  /// <typeparam name="T">The type of items in this spatial partition.</typeparam>
  /// <remarks>
  /// <para>
  /// This method is also known as "Sort and Sweep", "coordinate sorting", etc. The method is good
  /// for detecting overlaps of all objects in the spatial partition - it is a typical collision
  /// broad phase algorithm.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public partial class SweepAndPruneSpace<T> : BasePartition<T>, ISupportBroadPhase<T>
  {
    // Notes:
    // Possible Improvements:
    // - We could use quantized integer positions. See Bullet SAP for an example. (But according
    //   to Pierre Terdiman (Opcode) this is a big disadvantage.
    // - Use batch updates (see Pierre Terdiman paper).
    // - Stabbing numbers (see Gino van den Bergen book)
    // - 3D-DDA for fast ray casting support.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Sorted lists of edges for the axes X, Y and Z. 
    private readonly List<Edge>[] _edges = new List<Edge>[3];

    // Stores SAP info for each item. Uses a dictionary for O(1) retrieval.
    private readonly ItemInfoCollection _itemInfos = new ItemInfoCollection();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    IBroadPhase<T> ISupportBroadPhase<T>.BroadPhase
    {
      get { return _broadPhase; }
      set { _broadPhase = value; }
    }
    private IBroadPhase<T> _broadPhase;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SweepAndPruneSpace{T}"/> class.
    /// </summary>
    public SweepAndPruneSpace()
    {
      _edges[0] = new List<Edge>();
      _edges[1] = new List<Edge>();
      _edges[2] = new List<Edge>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override BasePartition<T> CreateInstanceCore()
    {
      return new SweepAndPruneSpace<T>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(BasePartition<T> source)
    {
      base.CloneCore(source);
    }
    #endregion


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    internal override void OnUpdate(bool forceRebuild, HashSet<T> addedItems, HashSet<T> removedItems, HashSet<T> invalidItems)
    {
      if (forceRebuild)
      {
        // Total rebuild. Re-Add all Items.
        _edges[0].Clear();
        _edges[1].Clear();
        _edges[2].Clear();
        _itemInfos.Clear();
        if (_broadPhase != null)
          _broadPhase.Clear();
        if (EnableSelfOverlaps)
          SelfOverlaps.Clear();

        foreach (var item in Items)
          AddItem(item);
      }
      else
      {
        // Refit - the default case.
        // First, remove all old items.
        foreach (T removedItem in removedItems)
          RemoveItem(removedItem);

        // Second, update all invalid items before we add the new items.
        if (invalidItems == null)
        {
          // Update all items.
          foreach (ItemInfo itemInfo in _itemInfos)
            UpdateItem(itemInfo);
        }
        else
        {
          // Update items marked as invalid.
          foreach (T invalidItem in invalidItems)
            UpdateItem(invalidItem);
        }

        // Third, add all the new items.
        foreach (var addedItem in addedItems)
          AddItem(addedItem);
      }

      // Update AABB of whole space.
      UpdateAabb();
    }


    private void AddItem(T item)
    {
      // Add ItemInfo.
      Aabb aabb = GetAabbForItem(item);
      ItemInfo itemInfo = new ItemInfo
      {
        Aabb = aabb,
        Item = item,
      };
      _itemInfos.Add(itemInfo);

      // Create edge info for each axis and add it to the edge lists.
      for (int axisIndex = 0; axisIndex < 3; axisIndex++)
      {
        // Minimum edge
        Edge minEdge = new Edge
        {
          Info = itemInfo,
          IsMax = false,
          Position = aabb.Minimum[axisIndex]
        };

        // Maximum edge
        Edge maxEdge = new Edge
        {
          Info = itemInfo,
          IsMax = true,
          Position = aabb.Maximum[axisIndex]
        };

        // Append at the end of the edge list.
        var edgeList = _edges[axisIndex];
        itemInfo.MinEdgeIndices[axisIndex] = edgeList.Count;
        edgeList.Add(minEdge);
        itemInfo.MaxEdgeIndices[axisIndex] = edgeList.Count;
        edgeList.Add(maxEdge);
      }

      // Sort down.
      for (int axisIndex = 0; axisIndex < 3; axisIndex++)
      {
        var edgeList = _edges[axisIndex];
        SortMinEdgeDown(edgeList, axisIndex, itemInfo.MinEdgeIndices[axisIndex]);
        SortMaxEdgeDown(edgeList, axisIndex, itemInfo.MaxEdgeIndices[axisIndex]);
      }
    }


    private void UpdateItem(T item)
    {
      UpdateItem(_itemInfos.Get(item));
    }


    private void UpdateItem(ItemInfo itemInfo)
    {
      // Get new AABB.
      Aabb aabb = GetAabbForItem(itemInfo.Item);
      itemInfo.Aabb = aabb;

      // Update edge lists.
      for (int axisIndex = 0; axisIndex < 3; axisIndex++)
      {
        List<Edge> edgeList = _edges[axisIndex];

        // New positions of the min/max edges.
        float newMinEdgePosition = aabb.Minimum[axisIndex];
        float newMaxEdgePosition = aabb.Maximum[axisIndex];

        // Old indices of the edges.
        int oldMinEdgeIndex = itemInfo.MinEdgeIndices[axisIndex];
        int oldMaxEdgeIndex = itemInfo.MaxEdgeIndices[axisIndex];

        // Update edge structures and compute difference of edge positions. 
        var minEdge = edgeList[oldMinEdgeIndex];
        float minimumPositionChange = newMinEdgePosition - minEdge.Position;
        minEdge.Position = newMinEdgePosition;
        //edgeList[oldMinEdgeIndex] = minEdge;    // This is needed if Edge is a struct.

        var maxEdge = edgeList[oldMaxEdgeIndex];
        float maximumPositionChange = newMaxEdgePosition - maxEdge.Position;
        maxEdge.Position = newMaxEdgePosition;
        //edgeList[oldMaxEdgeIndex] = maxEdge;    // This is needed if Edge is a struct.

        // Sort the edges up or down.
        if (minimumPositionChange < 0)
          SortMinEdgeDown(edgeList, axisIndex, oldMinEdgeIndex);

        if (maximumPositionChange > 0)
          SortMaxEdgeUp(edgeList, axisIndex, oldMaxEdgeIndex);

        if (minimumPositionChange > 0)
          SortMinEdgeUp(edgeList, axisIndex, oldMinEdgeIndex);

        if (maximumPositionChange < 0)
          SortMaxEdgeDown(edgeList, axisIndex, oldMaxEdgeIndex);
      }
    }


    private void RemoveItem(T item)
    {
      // Remove broad phase info
      var itemInfo = _itemInfos.Get(item);
      if (itemInfo == null)  // Abort if the object was not in the broad phase.
        return;

      // Remove from _itemInfos.
      _itemInfos.Remove(item);

      // Remove edges from edge lists.
      for (int axisIndex = 0; axisIndex < 3; axisIndex++)
      {
        // Get indices in edge list.
        int minEdgeIndex = itemInfo.MinEdgeIndices[axisIndex];
        int maxEdgeIndex = itemInfo.MaxEdgeIndices[axisIndex];

        // Update edge indices in itemInfo that point into edge list for edges above the min edge.
        List<Edge> edgeList = _edges[axisIndex];
        int numberOfEdges = edgeList.Count;
        for (int i = minEdgeIndex + 1; i < numberOfEdges; i++)
        {
          // Subtract one for edges that were before the max edge to remove, and subtract 2 for the rest.
          // TODO: We could split the loop into two parts. The part before maxEdgeIndex and the part after.
          int decrement = (i > maxEdgeIndex) ? 2 : 1;

          var currentItemInfo = edgeList[i].Info;
          if (edgeList[i].IsMax == false)
            currentItemInfo.MinEdgeIndices[axisIndex] = i - decrement;
          else
            currentItemInfo.MaxEdgeIndices[axisIndex] = i - decrement;
        }

        // Remove from edge list.
        edgeList.RemoveAt(maxEdgeIndex);
        edgeList.RemoveAt(minEdgeIndex);
      }

      // Remove related self-overlaps.
      if (_broadPhase != null)
        _broadPhase.Remove(item);

      if (EnableSelfOverlaps)
      {
        // TODO: Lambda method produces garbage.
        SelfOverlaps.RemoveWhere(overlap => Comparer.Equals(overlap.First, item) || Comparer.Equals(overlap.Second, item));
      }
    }


    /// <summary>
    /// Updates the AABB of the whole spatial partition.
    /// </summary>
    private void UpdateAabb()
    {
      if (Count == 0)
      {
        // AABB is undefined.
        Aabb = new Aabb();
      }
      else
      {
        // Get minimum and maximum from the edge lists.
        var minimum = new Vector3F(
          _edges[0][0].Position,
          _edges[1][0].Position,
          _edges[2][0].Position);

        var maxEdgeIndex = _edges[0].Count - 1;
        var maximum = new Vector3F(
          _edges[0][maxEdgeIndex].Position,
          _edges[1][maxEdgeIndex].Position,
          _edges[2][maxEdgeIndex].Position);

        Aabb = new Aabb(minimum, maximum);
      }
    }


    // Can only add overlapping pairs.
    // Sort edge down and add overlapping pairs.
    private void SortMinEdgeDown(List<Edge> edgeList, int axisIndex, int edgeIndex)
    {
      if (edgeIndex == 0)
        return;

      var edge = edgeList[edgeIndex];
      var itemInfo = edge.Info;

      int previousEdgeIndex = edgeIndex - 1;
      Edge previousEdge = edgeList[previousEdgeIndex];

      while (edge.Position <= previousEdge.Position)
      {
        var previousItemInfo = previousEdge.Info;

        if (previousEdge.IsMax)
        {
          // New overlapping pair!
          if (_broadPhase != null || EnableSelfOverlaps)
          {
            if (AreOverlapping(itemInfo, previousItemInfo, axisIndex))
            {
              var overlap = new Pair<T>(itemInfo.Item, previousItemInfo.Item);
              if (Filter == null || Filter.Filter(overlap))
              {
                if (_broadPhase != null)
                  _broadPhase.Add(overlap);

                if (EnableSelfOverlaps)
                  SelfOverlaps.Add(overlap);
              }
            }
          }

          previousItemInfo.MaxEdgeIndices[axisIndex] = edgeIndex; // Max edge was swapped up.
        }
        else
        {
          previousItemInfo.MinEdgeIndices[axisIndex] = edgeIndex; // Min edge was swapped up.
        }

        itemInfo.MinEdgeIndices[axisIndex] = previousEdgeIndex;   // Min edge was swapped down.

        // Swap edges in edge list.
        edgeList[edgeIndex] = previousEdge;
        edgeList[previousEdgeIndex] = edge;

        edgeIndex--;
        if (edgeIndex == 0)
          return;

        previousEdgeIndex--;
        previousEdge = edgeList[previousEdgeIndex];
      }
    }


    // Can only remove overlapping pairs.
    // Sort edge up and remove pairs that are not overlapping anymore.
    // Needs to be called after SortMaxEdgeUp.
    private void SortMinEdgeUp(List<Edge> edgeList, int axisIndex, int edgeIndex)
    {
      var edge = edgeList[edgeIndex];
      var itemInfo = edge.Info;

      int nextEdgeIndex = edgeIndex + 1;
      var nextEdge = edgeList[nextEdgeIndex];

      while (edge.Position > nextEdge.Position)
      {
        var nextItemInfo = nextEdge.Info;

        if (nextEdge.IsMax)
        {
          // Remove overlapping pair.
          var overlap = new Pair<T>(itemInfo.Item, nextItemInfo.Item);
          if (_broadPhase != null)
            _broadPhase.Remove(overlap);

          if (EnableSelfOverlaps)
            SelfOverlaps.Remove(overlap);

          nextItemInfo.MaxEdgeIndices[axisIndex] = edgeIndex; // Max edge was swapped down.
        }
        else
        {
          nextItemInfo.MinEdgeIndices[axisIndex] = edgeIndex; // Min edge was swapped down.
        }

        itemInfo.MinEdgeIndices[axisIndex] = nextEdgeIndex;   // Min edge was swapped up.

        // Swap edges in edge list.
        edgeList[edgeIndex] = nextEdge;
        edgeList[nextEdgeIndex] = edge;

        edgeIndex++;
        nextEdgeIndex++;
        nextEdge = edgeList[nextEdgeIndex];
      }
    }


    // Can only add overlapping pairs.
    // Sort edge up and add overlapping pairs
    private void SortMaxEdgeUp(List<Edge> edgeList, int axisIndex, int edgeIndex)
    {
      int maxIndex = edgeList.Count - 1;
      if (edgeIndex == maxIndex)
        return;

      var edge = edgeList[edgeIndex];
      var itemInfo = edge.Info;

      int nextEdgeIndex = edgeIndex + 1;
      var nextEdge = edgeList[nextEdgeIndex];

      while (edge.Position >= nextEdge.Position)
      {
        var nextItemInfo = nextEdge.Info;

        if (nextEdge.IsMax == false)
        {
          // New overlapping pair!
          if (_broadPhase != null || EnableSelfOverlaps)
          {
            if (AreOverlapping(itemInfo, nextItemInfo, axisIndex))
            {
              var overlap = new Pair<T>(itemInfo.Item, nextItemInfo.Item);
              if (Filter == null || Filter.Filter(overlap))
              {
                if (_broadPhase != null)
                  _broadPhase.Add(overlap);

                if (EnableSelfOverlaps)
                  SelfOverlaps.Add(overlap);
              }
            }
          }

          nextItemInfo.MinEdgeIndices[axisIndex] = edgeIndex; // Min edge was swapped down.
        }
        else
        {
          nextItemInfo.MaxEdgeIndices[axisIndex] = edgeIndex; // Max edge was swapped down.
        }

        itemInfo.MaxEdgeIndices[axisIndex] = nextEdgeIndex;   // Max edge was swapped up.

        // Swap edges in edge list.
        edgeList[edgeIndex] = nextEdge;
        edgeList[nextEdgeIndex] = edge;

        edgeIndex++;
        if (edgeIndex == maxIndex)
          return;

        nextEdgeIndex++;
        nextEdge = edgeList[nextEdgeIndex];
      }
    }


    // Can only remove overlapping pairs.
    // Sort edge down and remove pairs that are not overlapping anymore.
    // Needs to called after SortMinEdgeDown.
    private void SortMaxEdgeDown(List<Edge> edgeList, int axisIndex, int edgeIndex)
    {
      var edge = edgeList[edgeIndex];
      var itemInfo = edge.Info;

      int previousEdgeIndex = edgeIndex - 1;
      var previousEdge = edgeList[previousEdgeIndex];

      while (edge.Position < previousEdge.Position)
      {
        var previousItemInfo = previousEdge.Info;

        if (previousEdge.IsMax == false)
        {
          // Remove overlapping pair.
          var overlap = new Pair<T>(itemInfo.Item, previousItemInfo.Item);
          if (_broadPhase != null)
            _broadPhase.Remove(overlap);

          if (EnableSelfOverlaps)
            SelfOverlaps.Remove(overlap);

          previousItemInfo.MinEdgeIndices[axisIndex] = edgeIndex;  // Min edge was swapped up.
        }
        else
        {
          previousItemInfo.MaxEdgeIndices[axisIndex] = edgeIndex;  // Max edge was swapped up.
        }

        itemInfo.MaxEdgeIndices[axisIndex] = previousEdgeIndex;    // Max edge was swapped down.

        // Swap edges in edge list.
        edgeList[edgeIndex] = previousEdge;
        edgeList[previousEdgeIndex] = edge;

        edgeIndex--;
        previousEdgeIndex--;
        previousEdge = edgeList[previousEdgeIndex];
      }
    }


    // The axis with index axisToIgnore is not tested. We assume that the objects are overlapping on 
    // this axis. The AABB test is done by comparing the indices in the edge lists - not the actual 
    // positions.
    private static bool AreOverlapping(ItemInfo broadPhaseInfoA, ItemInfo broadPhaseInfoB, int axisToIgnore)
    {
      if (axisToIgnore != 0)
      {
        if (broadPhaseInfoA.MaxEdgeIndices[0] < broadPhaseInfoB.MinEdgeIndices[0]
            || broadPhaseInfoA.MinEdgeIndices[0] > broadPhaseInfoB.MaxEdgeIndices[0])
        {
          return false;
        }
      }

      if (axisToIgnore != 1)
      {
        if (broadPhaseInfoA.MaxEdgeIndices[1] < broadPhaseInfoB.MinEdgeIndices[1]
            || broadPhaseInfoA.MinEdgeIndices[1] > broadPhaseInfoB.MaxEdgeIndices[1])
        {
          return false;
        }
      }

      if (axisToIgnore != 2)
      {
        if (broadPhaseInfoA.MaxEdgeIndices[2] < broadPhaseInfoB.MinEdgeIndices[2]
            || broadPhaseInfoA.MinEdgeIndices[2] > broadPhaseInfoB.MaxEdgeIndices[2])
        {
          return false;
        }
      }

      return true;
    }
    #endregion
  }
}
