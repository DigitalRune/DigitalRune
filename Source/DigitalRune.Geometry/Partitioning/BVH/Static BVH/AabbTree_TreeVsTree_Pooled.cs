// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if POOL_ENUMERABLES
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  public partial class AabbTree<T>
  {
    // ReSharper disable StaticFieldInGenericType
    private sealed class GetOverlapsWithPartitionWork : PooledEnumerable<Pair<T>>
    {
      private static readonly ResourcePool<GetOverlapsWithPartitionWork> Pool = new ResourcePool<GetOverlapsWithPartitionWork>(() => new GetOverlapsWithPartitionWork(), x => x.Initialize(), null);
      private AabbTree<T> _partition;
      private ISpatialPartition<T> _otherPartition;
      private IEnumerator<Node> _leafNodes;
      private IEnumerator<T> _otherCandidates;

      public static IEnumerable<Pair<T>> Create(AabbTree<T> partition, ISpatialPartition<T> otherPartition)
      {
        var enumerable = Pool.Obtain();
        enumerable._partition = partition;
        enumerable._otherPartition = otherPartition;
        enumerable._leafNodes = partition.GetLeafNodes(otherPartition.Aabb).GetEnumerator();
        return enumerable;
      }

      protected override bool OnNext(out Pair<T> current)
      {
        while (true)
        {
          if (_otherCandidates == null)
          {
            if (_leafNodes.MoveNext())
            {
              var leaf = _leafNodes.Current;
              _otherCandidates = _otherPartition.GetOverlaps(leaf.Aabb).GetEnumerator();
            }
            else
            {
              current = default(Pair<T>);
              return false;
            }
          }

          while (_otherCandidates.MoveNext())
          {
            var leaf = _leafNodes.Current;
            var otherCandidate = _otherCandidates.Current;
            var overlap = new Pair<T>(leaf.Item, otherCandidate);
            if (_partition.Filter == null || _partition.Filter.Filter(overlap))
            {
              current = overlap;
              return true;
            }
          }

          _otherCandidates.Dispose();
          _otherCandidates = null;
        }
      }

      protected override void OnRecycle()
      {
        _partition = null;
        _otherPartition = null;
        _leafNodes.Dispose();
        _leafNodes = null;
        if (_otherCandidates != null)
        {
          _otherCandidates.Dispose();
          _otherCandidates = null;
        }
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithTreeWork : PooledEnumerable<Pair<T>>
    {
      private static readonly ResourcePool<GetOverlapsWithTreeWork> Pool = new ResourcePool<GetOverlapsWithTreeWork>(() => new GetOverlapsWithTreeWork(), x => x.Initialize(), null);
      private AabbTree<T> _partition;
      private readonly Stack<Pair<Node, Node>> _stack = new Stack<Pair<Node, Node>>();

      public static IEnumerable<Pair<T>> Create(AabbTree<T> partition, AabbTree<T> otherPartition)
      {
        var enumerable = Pool.Obtain();
        enumerable._partition = partition;
        enumerable._stack.Push(new Pair<Node, Node>(partition._root, otherPartition._root));
        return enumerable;
      }

      protected override bool OnNext(out Pair<T> current)
      {
        while (_stack.Count > 0)
        {
          var nodePair = _stack.Pop();
          var nodeA = nodePair.First;
          var nodeB = nodePair.Second;

          if (nodeA == nodeB)
          {
            if (!nodeA.IsLeaf)
            {
              _stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeA.RightChild));
              _stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeA.RightChild));
              _stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeA.LeftChild));
            }
          }
          else if (GeometryHelper.HaveContact(nodeA.Aabb, nodeB.Aabb))
          {
            if (!nodeA.IsLeaf)
            {
              if (!nodeB.IsLeaf)
              {
                _stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB.RightChild));
                _stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB.RightChild));
                _stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB.LeftChild));
                _stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB.LeftChild));
              }
              else
              {
                _stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB));
                _stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB));
              }
            }
            else
            {
              if (!nodeB.IsLeaf)
              {
                _stack.Push(new Pair<Node, Node>(nodeA, nodeB.RightChild));
                _stack.Push(new Pair<Node, Node>(nodeA, nodeB.LeftChild));
              }
              else
              {
                // Leaf overlap.
                var overlap = new Pair<T>(nodeA.Item, nodeB.Item);
                if (_partition.Filter == null || _partition.Filter.Filter(overlap))
                {
                  current = overlap;
                  return true;
                }
              }
            }
          }
        }

        current = default(Pair<T>);
        return false;
      }

      protected override void OnRecycle()
      {
        _partition = null;
        _stack.Clear();
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithTransformedPartitionWork : PooledEnumerable<Pair<T>>
    {
      private static readonly ResourcePool<GetOverlapsWithTransformedPartitionWork> Pool = new ResourcePool<GetOverlapsWithTransformedPartitionWork>(() => new GetOverlapsWithTransformedPartitionWork(), x => x.Initialize(), null);
      private AabbTree<T> _partition;
      private ISpatialPartition<T> _otherPartition;
      private IEnumerator<Node> _leafNodes;
      private IEnumerator<T> _otherCandidates;
      private Vector3F _scale;
      private Vector3F _otherScaleInverse;
      private Pose _toOther;

      public static IEnumerable<Pair<T>> Create(AabbTree<T> partition, 
        ISpatialPartition<T> otherPartition, IEnumerable<Node> leafNodes,
        ref Vector3F scale, ref Vector3F otherScaleInverse, ref Pose toOther)
      {
        var enumerable = Pool.Obtain();
        enumerable._partition = partition;
        enumerable._otherPartition = otherPartition;
        enumerable._leafNodes = leafNodes.GetEnumerator();
        enumerable._scale = scale;
        enumerable._otherScaleInverse = otherScaleInverse;
        enumerable._toOther = toOther;
        return enumerable;
      }

      protected override bool OnNext(out Pair<T> current)
      {
        while (true)
        {
          if (_otherCandidates == null)
          {
            if (_leafNodes.MoveNext())
            {
              var leaf = _leafNodes.Current;
              Aabb aabb = leaf.Aabb.GetAabb(_scale, _toOther);
              aabb.Scale(_otherScaleInverse);
              _otherCandidates = _otherPartition.GetOverlaps(aabb).GetEnumerator();
            }
            else
            {
              current = default(Pair<T>);
              return false;
            }
          }

          while (_otherCandidates.MoveNext())
          {
            var leaf = _leafNodes.Current;
            var otherCandidate = _otherCandidates.Current;
            var overlap = new Pair<T>(leaf.Item, otherCandidate);
            if (_partition.Filter == null || _partition.Filter.Filter(overlap))
            {
              current = overlap;
              return true;
            }
          }

          _otherCandidates.Dispose();
          _otherCandidates = null;
        }
      }

      protected override void OnRecycle()
      {
        _partition = null;
        _otherPartition = null;
        _leafNodes.Dispose();
        _leafNodes = null;
        if (_otherCandidates != null)
        {
          _otherCandidates.Dispose();
          _otherCandidates = null;
        }
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithTransformedTreeWork : PooledEnumerable<Pair<T>>
    {
      private static readonly ResourcePool<GetOverlapsWithTransformedTreeWork> Pool = new ResourcePool<GetOverlapsWithTransformedTreeWork>(() => new GetOverlapsWithTransformedTreeWork(), x => x.Initialize(), null);
      private AabbTree<T> _partition;
      private readonly Stack<Pair<Node, Node>> _stack = new Stack<Pair<Node, Node>>();
      private Vector3F _scaleA;
      private Vector3F _scaleB;
      private Pose _bToA;

      public static IEnumerable<Pair<T>> Create(AabbTree<T> partition, AabbTree<T> otherPartition, 
        ref Vector3F scaleA, ref Vector3F scaleB, ref Pose bToA)
      {
        var enumerable = Pool.Obtain();
        enumerable._partition = partition;
        enumerable._stack.Push(new Pair<Node, Node>(partition._root, otherPartition._root));
        enumerable._scaleA = scaleA;
        enumerable._scaleB = scaleB;
        enumerable._bToA = bToA;
        return enumerable;
      }

      protected override bool OnNext(out Pair<T> current)
      {
        while (_stack.Count > 0)
        {
          var nodePair = _stack.Pop();
          var nodeA = nodePair.First;
          var nodeB = nodePair.Second;

          if (HaveAabbContact(nodeA, _scaleA, nodeB, _scaleB, _bToA))
          {
            if (!nodeA.IsLeaf)
            {
              if (!nodeB.IsLeaf)
              {
                _stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB.RightChild));
                _stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB.RightChild));
                _stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB.LeftChild));
                _stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB.LeftChild));
              }
              else
              {
                _stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB));
                _stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB));
              }
            }
            else
            {
              if (!nodeB.IsLeaf)
              {
                _stack.Push(new Pair<Node, Node>(nodeA, nodeB.RightChild));
                _stack.Push(new Pair<Node, Node>(nodeA, nodeB.LeftChild));
              }
              else
              {
                // Leaf overlap.
                var overlap = new Pair<T>(nodeA.Item, nodeB.Item);
                if (_partition.Filter == null || _partition.Filter.Filter(overlap))
                {
                  current = overlap;
                  return true;
                }
              }
            }
          }
        }

        current = default(Pair<T>);
        return false;
      }

      protected override void OnRecycle()
      {
        _partition = null;
        _stack.Clear();
        Pool.Recycle(this);
      }
    }
    // ReSharper restore StaticFieldInGenericType
  }
}
#endif
