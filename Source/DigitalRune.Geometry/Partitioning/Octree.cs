// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

/*
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  public class Octree<T> : BaseAabbPartition<T>
  {
    //
    // Note:
    // This simple Octree implementation provides many opportunities for
    // improvements. See Christer Ericsson's Real Time Collision Detection 
    // book for ideas.
    //
    // Ideas:
    // We could let the user define the Root AABB size (= world size in 
    // collision detection broad-phase.
    //
    // For node order see, Real-Time Collision Detection p. 308.
    //

    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private sealed class Node
    {
      public Aabb Aabb;
      public Node[] Children = new Node[8];  // Note: We could also use ushort indices and store all nodes in one list.
      public List<T> Items = new List<T>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private Node Root;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------


    public Aabb? MinimumRootAabb
    {
      get { return _minimumRootAabb; }
      set
      {
        if (value != _minimumRootAabb)
        {
          _minimumRootAabb = value;

          if (_minimumRootAabb.HasValue)
          {
            if (!IsContained(Aabb, _minimumRootAabb.Value))
              Invalidate();
          }
        }
      }
    }
    private Aabb? _minimumRootAabb;


    public float MinimumCellSize { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public Octree(Func<T, Aabb> getAabb)
      : base(getAabb)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    public override IEnumerable<T> GetOverlaps(Aabb aabb)
    {
      Update(false, null);

      if (Root == null)
        yield break;

      var stack = new Stack<Node>();
      stack.Push(Root);

      while (stack.Count > 0)
      {
        var node = stack.Pop();

        if (GeometryHelper.HaveContact(node.Aabb, aabb))
        {
          foreach (var item in node.Items)
          {
            if (GeometryHelper.HaveContact(GetAabbForItem(item), aabb))
              yield return item;
          }


          for (int i = 0; i < 8; i++)
          {
            if (node.Children[i] != null)
              stack.Push(node.Children[i]);
          }
        }
      }
    }


    protected override void OnUpdate(bool forceRebuild, HashSet<T> addedItems, HashSet<T> removedItems, HashSet<T> invalidItems, Action<T, T> overlapCallback)
    {
      // TODO: Only total rebuild supported.

      if (Count == 0)
      {
        Root = null;
        Aabb = new Aabb();
        if (EnableSelfOverlaps)
          SelfOverlaps.Clear();
        return;
      }

      var aabbs = new Aabb[Count];
      aabbs[0] = GetAabbForItem(Items[0]);
      Aabb = aabbs[0];
      for (int i = 1; i < Count; i++)
      {
        aabbs[i] = GetAabbForItem(Items[i]);
        Aabb.Grow(aabbs[i]);
      }

      if (MinimumRootAabb.HasValue)
        Aabb.Grow(MinimumRootAabb.Value);


      Root = new Node { Aabb = Aabb, };

      for (int i = 0; i < Items.Count; i++)
      {
        var item = Items[i];
        var node = Root;
        var nodeAabb = Aabb;
        var itemAabb = aabbs[i];

        while (IsContained(nodeAabb, itemAabb))
        {
          int childIndex = -1;
          Aabb childNodeAabb = new Aabb();

          if (nodeAabb.Extent.LargestComponent >= 2 * MinimumCellSize)
          {
            for (int j = 0; j < 8; j++)
            {
              childNodeAabb = CreateChildAabb(nodeAabb, j);
              if (IsContained(childNodeAabb, itemAabb))
              {
                childIndex = j;
                break;
              }
            }
          }

          if (childIndex == -1)
          {
            node.Items.Add(item);
            break;
          }
          else
          {
            if (node.Children[childIndex] == null)
            {
              node.Children[childIndex] = new Node { Aabb = childNodeAabb, };
            }

            node = node.Children[childIndex];
          }
        }
      }

      if (EnableSelfOverlaps)
      {
        for (int i = 0; i < Items.Count; i++)
        {
          var item = Items[i];
          var itemAabb = aabbs[i];

          foreach (var touchedItem in GetOverlaps(itemAabb))
          {
            if (Comparer.Equals(item, touchedItem))
              continue;

            if (Filter != null && !Filter.Filter(item, touchedItem))
              continue;

            var overlap = new Overlap<T>(item, touchedItem);
            bool isNew = SelfOverlaps.Add(overlap);

            if (overlapCallback != null && isNew)
              overlapCallback(item, touchedItem);
          }
        }
      }
    }

    private bool IsContained(Aabb container, Aabb aabb)
    {
      // TODO: What about numerical tolerances?
      return container.Minimum <= aabb.Minimum && aabb.Maximum <= container.Maximum;
    }



    private Aabb CreateChildAabb(Aabb parentAabb, int childIndex)
    {
      Vector3F offset;
      switch (childIndex % 4)
      {
        case 0: offset = new Vector3F(0, 0, 0); break;
        case 1: offset = new Vector3F(1, 0, 0); break;
        case 2: offset = new Vector3F(0, 1, 0); break;
        default:
          Debug.Assert(childIndex % 4 == 3);
          offset = new Vector3F(1, 1, 1);
          break;

      }
      if (childIndex > 3)
        offset.Z = 1;

      var childExtent = parentAabb.Extent * 0.5f;
      var minimum = parentAabb.Minimum + offset * childExtent;
      var maximum = minimum + childExtent;
      return new Aabb(minimum, maximum);
    }
    #endregion

  }
}
*/
