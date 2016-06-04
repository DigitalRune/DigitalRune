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
  public partial class AabbTree<T>
  {
    // ReSharper disable StaticFieldInGenericType
    private sealed class GetOverlapsWork : PooledEnumerable<T>
    {
      private static readonly ResourcePool<GetOverlapsWork> Pool = new ResourcePool<GetOverlapsWork>(() => new GetOverlapsWork(), x => x.Initialize(), null);
      private Aabb _aabb;
      private readonly Stack<Node> _stack = new Stack<Node>();

      public static IEnumerable<T> Create(AabbTree<T> aabbTree, ref Aabb aabb)
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


    private sealed class GetLeafNodesWork : PooledEnumerable<Node>
    {
      private static readonly ResourcePool<GetLeafNodesWork> Pool = new ResourcePool<GetLeafNodesWork>(() => new GetLeafNodesWork(), x => x.Initialize(), null);
      private Aabb _aabb;
      private readonly Stack<Node> _stack = new Stack<Node>();

      public static IEnumerable<Node> Create(AabbTree<T> aabbTree, ref Aabb aabb)
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


    private sealed class GetOverlapsWithRayWork : PooledEnumerable<T>
    {
      private static readonly ResourcePool<GetOverlapsWithRayWork> Pool = new ResourcePool<GetOverlapsWithRayWork>(() => new GetOverlapsWithRayWork(), x => x.Initialize(), null);
      private Ray _ray;
      private Vector3F _rayDirectionInverse;
      private float _epsilon;
      private readonly Stack<Node> _stack = new Stack<Node>();

      public static IEnumerable<T> Create(AabbTree<T> aabbTree, ref Ray ray)
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
    // ReSharper restore StaticFieldInGenericType
  }
}
#endif
