// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if POOL_ENUMERABLES
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  partial class CompressedAabbTree
  {
    private sealed class GetOverlapsWork : PooledEnumerable<int>
    {
      private static readonly ResourcePool<GetOverlapsWork> Pool = new ResourcePool<GetOverlapsWork>(() => new GetOverlapsWork(), x => x.Initialize(), null);
      private CompressedAabbTree _compressedAabbTree;
      private Aabb _aabb;
      private int _index;

      public static IEnumerable<int> Create(CompressedAabbTree compressedAabbTree, ref Aabb aabb)
      {
        var enumerable = Pool.Obtain();
        enumerable._compressedAabbTree = compressedAabbTree;
        enumerable._aabb = aabb;
        enumerable._index = 0;
        return enumerable;
      }

      protected override bool OnNext(out int current)
      {
        var nodes = _compressedAabbTree._nodes;
        if (nodes != null)
        {
          while (_index < nodes.Length)
          {
            Node node = nodes[_index];
            bool haveContact = GeometryHelper.HaveContact(_compressedAabbTree.GetAabb(node), _aabb);

            if (haveContact || node.IsLeaf)
              _index++;
            else
              _index += node.EscapeOffset;

            if (haveContact && node.IsLeaf)
            {
              current = node.Item;
              return true;
            }
          }
        }
        current = 0;
        return false;
      }

      protected override void OnRecycle()
      {
        _compressedAabbTree = null;
        Pool.Recycle(this);
      }
    }


    private sealed class GetLeafNodesWork : PooledEnumerable<Node>
    {
      private static readonly ResourcePool<GetLeafNodesWork> Pool = new ResourcePool<GetLeafNodesWork>(() => new GetLeafNodesWork(), x => x.Initialize(), null);
      private CompressedAabbTree _compressedAabbTree;
      private Aabb _aabb;
      private int _index;

      public static IEnumerable<Node> Create(CompressedAabbTree compressedAabbTree, ref Aabb aabb)
      {
        var enumerable = Pool.Obtain();
        enumerable._compressedAabbTree = compressedAabbTree;
        enumerable._aabb = aabb;
        enumerable._index = 0;
        return enumerable;
      }

      protected override bool OnNext(out Node current)
      {
        var nodes = _compressedAabbTree._nodes;
        if (nodes != null)
        {
          while (_index < nodes.Length)
          {
            Node node = nodes[_index];
            bool haveContact = GeometryHelper.HaveContact(_compressedAabbTree.GetAabb(node), _aabb);

            if (haveContact || node.IsLeaf)
              _index++;
            else
              _index += node.EscapeOffset;

            if (haveContact && node.IsLeaf)
            {
              current = node;
              return true;
            }
          }
        }
        current = default(Node);
        return false;
      }

      protected override void OnRecycle()
      {
        _compressedAabbTree = null;
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithItemWork : PooledEnumerable<int>
    {
      private static readonly ResourcePool<GetOverlapsWithItemWork> Pool = new ResourcePool<GetOverlapsWithItemWork>(() => new GetOverlapsWithItemWork(), x => x.Initialize(), null);
      private CompressedAabbTree _compressedAabbTree;
      private int _item;
      private IEnumerator<int> _enumerator;

      public static IEnumerable<int> Create(CompressedAabbTree compressedAabbTree, int item)
      {
        var enumerable = Pool.Obtain();
        enumerable._compressedAabbTree = compressedAabbTree;
        enumerable._item = item;
        Aabb aabb = compressedAabbTree.GetAabbForItem(item);
        enumerable._enumerator = compressedAabbTree.GetOverlaps(aabb).GetEnumerator();
        return enumerable;
      }

      protected override bool OnNext(out int current)
      {
        while (_enumerator.MoveNext())
        {
          int touchedItem = _enumerator.Current;
          if (_compressedAabbTree.FilterSelfOverlap(new Pair<int>(touchedItem, _item)))
          {
            current = touchedItem;
            return true;
          }
        }
        current = 0;
        return false;
      }

      protected override void OnRecycle()
      {
        _compressedAabbTree = null;
        _enumerator.Dispose();
        _enumerator = null;
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithRayWork : PooledEnumerable<int>
    {
      private static readonly ResourcePool<GetOverlapsWithRayWork> Pool = new ResourcePool<GetOverlapsWithRayWork>(() => new GetOverlapsWithRayWork(), x => x.Initialize(), null);
      private CompressedAabbTree _compressedAabbTree;
      private Ray _ray;
      private Vector3F _rayDirectionInverse;
      private float _epsilon;
      private int _index;

      public static IEnumerable<int> Create(CompressedAabbTree compressedAabbTree, ref Ray ray)
      {
        var enumerable = Pool.Obtain();
        enumerable._compressedAabbTree = compressedAabbTree;
        enumerable._ray = ray;
        enumerable._rayDirectionInverse = new Vector3F(1 / ray.Direction.X,
                                                       1 / ray.Direction.Y,
                                                       1 / ray.Direction.Z);
        enumerable._epsilon = Numeric.EpsilonF * (1 + compressedAabbTree.Aabb.Extent.Length);
        enumerable._index = 0;
        return enumerable;
      }

      protected override bool OnNext(out int current)
      {
        var nodes = _compressedAabbTree._nodes;
        if (nodes != null)
        {
          while (_index < nodes.Length)
          {
            Node node = nodes[_index];
            bool haveContact = GeometryHelper.HaveContact(_compressedAabbTree.GetAabb(node), _ray.Origin, _rayDirectionInverse, _ray.Length, _epsilon);

            if (haveContact || node.IsLeaf)
              _index++;
            else
              _index += node.EscapeOffset;

            if (haveContact && node.IsLeaf)
            {
              current = node.Item;
              return true;
            }
          }
        }
        current = 0;
        return false;
      }

      protected override void OnRecycle()
      {
        _compressedAabbTree = null;
        Pool.Recycle(this);
      }
    }
  }
}
#endif
