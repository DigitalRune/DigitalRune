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
  partial class DynamicAabbTree<T>
  {
    // ReSharper disable StaticFieldInGenericType
    private sealed class GetOverlapsWork : PooledEnumerable<T>
    {
      private static readonly ResourcePool<GetOverlapsWork> Pool = new ResourcePool<GetOverlapsWork>(() => new GetOverlapsWork(), x => x.Initialize(), null);
      private Aabb _aabb;
      private readonly Stack<Node> _stack = new Stack<Node>();

      public static IEnumerable<T> Create(DynamicAabbTree<T> aabbTree, ref Aabb aabb)
      {
        var enumerable = Pool.Obtain();
        enumerable._aabb = aabb;
        if (aabbTree._root != null)
          enumerable._stack.Push(aabbTree._root);
        return enumerable;
      }

      protected override bool OnNext(out T current)
      {
        while (_stack.Count > 0)
        {
          Node node = _stack.Pop();

          if (GeometryHelper.HaveContact(node.Aabb, _aabb))
          {
            if (node.IsLeaf)
            {
              current = node.Item;
              return true;
            }
            else
            {
              _stack.Push(node.RightChild);
              _stack.Push(node.LeftChild);
            }
          }
        }
        current = default(T);
        return false;
      }

      protected override void OnRecycle()
      {
        _stack.Clear();
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithLeafWork : PooledEnumerable<T>
    {
      private static readonly ResourcePool<GetOverlapsWithLeafWork> Pool = new ResourcePool<GetOverlapsWithLeafWork>(() => new GetOverlapsWithLeafWork(), x => x.Initialize(), null);
      private Node _leaf;
      private readonly Stack<Node> _stack = new Stack<Node>();

      public static IEnumerable<T> Create(DynamicAabbTree<T> aabbTree, Node leaf)
      {
        var enumerable = Pool.Obtain();
        enumerable._leaf = leaf;
        if (aabbTree._root != null)
          enumerable._stack.Push(aabbTree._root);
        return enumerable;
      }

      protected override bool OnNext(out T current)
      {
        while (_stack.Count > 0)
        {
          Node node = _stack.Pop();
          if (node != _leaf && GeometryHelper.HaveContact(node.Aabb, _leaf.Aabb))
          {
            if (node.IsLeaf)
            {
              current = node.Item;
              return true;
            }
            else
            {
              _stack.Push(node.RightChild);
              _stack.Push(node.LeftChild);
            }
          }
        }
        current = default(T);
        return false;
      }

      protected override void OnRecycle()
      {
        _leaf = null;
        _stack.Clear();
        Pool.Recycle(this);
      }
    }


    private sealed class GetLeafNodesWork : PooledEnumerable<Node>
    {
      private static readonly ResourcePool<GetLeafNodesWork> Pool = new ResourcePool<GetLeafNodesWork>(() => new GetLeafNodesWork(), x => x.Initialize(), null);
      private Aabb _aabb;
      private readonly Stack<Node> _stack = new Stack<Node>();

      public static IEnumerable<Node> Create(DynamicAabbTree<T> aabbTree, ref Aabb aabb)
      {
        var enumerable = Pool.Obtain();
        enumerable._aabb = aabb;
        if (aabbTree._root != null)
          enumerable._stack.Push(aabbTree._root);
        return enumerable;
      }

      protected override bool OnNext(out Node current)
      {
        while (_stack.Count > 0)
        {
          Node node = _stack.Pop();
          if (GeometryHelper.HaveContact(node.Aabb, _aabb))
          {
            if (node.IsLeaf)
            {
              current = node;
              return true;
            }
            else
            {
              _stack.Push(node.RightChild);
              _stack.Push(node.LeftChild);
            }
          }
        }
        current = default(Node);
        return false;
      }

      protected override void OnRecycle()
      {
        _stack.Clear();
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithItemWork : PooledEnumerable<T>
    {
      private static readonly ResourcePool<GetOverlapsWithItemWork> Pool = new ResourcePool<GetOverlapsWithItemWork>(() => new GetOverlapsWithItemWork(), x => x.Initialize(), null);
      private DynamicAabbTree<T> _dynamicAabbTree;
      private T _item;
      private IEnumerator<T> _enumerator;

      public static IEnumerable<T> Create(DynamicAabbTree<T> dynamicAabbTree, T item)
      {
        var enumerable = Pool.Obtain();
        enumerable._dynamicAabbTree = dynamicAabbTree;
        enumerable._item = item;
        Aabb aabb = dynamicAabbTree.GetAabbForItem(item);
        enumerable._enumerator = dynamicAabbTree.GetOverlaps(aabb).GetEnumerator();
        return enumerable;
      }

      protected override bool OnNext(out T current)
      {
        while (_enumerator.MoveNext())
        {
          T touchedItem = _enumerator.Current;
          if (_dynamicAabbTree.FilterSelfOverlap(new Pair<T>(touchedItem, _item)))
          {
            current = touchedItem;
            return true;
          }
        }
        current = default(T);
        return false;
      }

      protected override void OnRecycle()
      {
        _dynamicAabbTree = null;
        _enumerator.Dispose();
        _enumerator = null;
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithRayWork : PooledEnumerable<T>
    {
      private static readonly ResourcePool<GetOverlapsWithRayWork> Pool = new ResourcePool<GetOverlapsWithRayWork>(() => new GetOverlapsWithRayWork(), x => x.Initialize(), null);
      private Ray _ray;
      private Vector3F _rayDirectionInverse;
      private float _epsilon;
      private readonly Stack<Node> _stack = new Stack<Node>();

      public static IEnumerable<T> Create(DynamicAabbTree<T> aabbTree, ref Ray ray)
      {
        var enumerable = Pool.Obtain();
        enumerable._ray = ray;
        enumerable._rayDirectionInverse = new Vector3F(1 / ray.Direction.X,
                                                       1 / ray.Direction.Y,
                                                       1 / ray.Direction.Z);
        enumerable._epsilon = Numeric.EpsilonF * (1 + aabbTree.Aabb.Extent.Length);
        if (aabbTree._root != null)
          enumerable._stack.Push(aabbTree._root);
        return enumerable;
      }

      protected override bool OnNext(out T current)
      {
        while (_stack.Count > 0)
        {
          var node = _stack.Pop();
          if (GeometryHelper.HaveContact(node.Aabb, _ray.Origin, _rayDirectionInverse, _ray.Length, _epsilon))
          {
            if (node.IsLeaf)
            {
              current = node.Item;
              return true;
            }
            else
            {
              _stack.Push(node.RightChild);
              _stack.Push(node.LeftChild);
            }
          }
        }
        current = default(T);
        return false;
      }

      protected override void OnRecycle()
      {
        _stack.Clear();
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithKDOPWork : PooledEnumerable<T>
    {
      private static readonly ResourcePool<GetOverlapsWithKDOPWork> Pool = new ResourcePool<GetOverlapsWithKDOPWork>(() => new GetOverlapsWithKDOPWork(), x => x.Initialize(), null);
      private readonly Stack<Pair<Node, int>> _stack0 = new Stack<Pair<Node, int>>();
      private readonly Stack<Node> _stack1 = new Stack<Node>();
      private IList<Plane> _planes;
      private int[] _signs = new int[6];
      private int _inside;

      public static IEnumerable<T> Create(DynamicAabbTree<T> aabbTree, IList<Plane> planes)
      {
        var enumerable = Pool.Obtain();
        enumerable._planes = planes;
        int numberOfPlanes = planes.Count;
        if (enumerable._signs.Length < numberOfPlanes)
          enumerable._signs = new int[numberOfPlanes];
        for (int i = 0; i < numberOfPlanes; i++)
        {
          enumerable._signs[i] = ((planes[i].Normal.X >= 0) ? 1 : 0)
                                 + ((planes[i].Normal.Y >= 0) ? 2 : 0)
                                 + ((planes[i].Normal.Z >= 0) ? 4 : 0);
        }
        enumerable._inside = (1 << numberOfPlanes) - 1;
        if (aabbTree._root != null)
          enumerable._stack0.Push(new Pair<Node, int>(aabbTree._root, 0));
        return enumerable;
      }

      protected override bool OnNext(out T current)
      {
        while (_stack1.Count > 0)
        {
          var n = _stack1.Pop();
          if (n.IsLeaf)
          {
            current = n.Item;
            return true;
          }
          else
          {
            _stack1.Push(n.RightChild);
            _stack1.Push(n.LeftChild);
          }
        }

        var numberOfPlanes = _planes.Count;
        while (_stack0.Count > 0)
        {
          var entry = _stack0.Pop();
          var node = entry.First;
          int mask = entry.Second;
          bool outside = false;
          for (int i = 0, j = 1; !outside && i < numberOfPlanes; i++, j <<= 1)
          {
            if ((mask & j) == 0)
            {
              var aabb = node.Aabb;
              var plane = _planes[i];
              int side = Classify(ref aabb, ref plane, _signs[i]);
              switch (side)
              {
                case +1:
                  // Node is in positive halfspace (in front of plane).
                  outside = true;
                  break;
                case -1:
                  // Node is in negative halfspace (behind plane).
                  mask |= j;
                  break;
                case 0:
                  // Node intersect plane.
                  break;
              }
            }
          }
          if (!outside)
          {
            if (mask == _inside || node.IsLeaf)
            {
              // Return all leaves.
              _stack1.Push(node);
              while (_stack1.Count > 0)
              {
                var n = _stack1.Pop();
                if (n.IsLeaf)
                {
                  current = n.Item;
                  return true;
                }
                else
                {
                  _stack1.Push(n.RightChild);
                  _stack1.Push(n.LeftChild);
                }
              }
            }
            else
            {
              _stack0.Push(new Pair<Node, int>(node.RightChild, mask));
              _stack0.Push(new Pair<Node, int>(node.LeftChild, mask));
            }
          }
        }
        current = default(T);
        return false;
      }

      protected override void OnRecycle()
      {
        _stack0.Clear();
        _stack1.Clear();
        _planes = null;
        Pool.Recycle(this);
      }
    }    // ReSharper restore StaticFieldInGenericType
  }
}
#endif
