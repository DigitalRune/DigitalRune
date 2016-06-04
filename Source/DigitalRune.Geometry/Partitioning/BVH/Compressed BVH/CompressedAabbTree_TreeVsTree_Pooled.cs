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
  partial class CompressedAabbTree
  {
    // ReSharper disable StaticFieldInGenericType
    private sealed class GetOverlapsWithPartitionWork : PooledEnumerable<Pair<int>>
    {
      private static readonly ResourcePool<GetOverlapsWithPartitionWork> Pool = new ResourcePool<GetOverlapsWithPartitionWork>(() => new GetOverlapsWithPartitionWork(), x => x.Initialize(), null);
      private CompressedAabbTree _partition;
      private ISpatialPartition<int> _otherPartition;
      private IEnumerator<Node> _leafNodes;
      private IEnumerator<int> _otherCandidates;

      public static IEnumerable<Pair<int>> Create(CompressedAabbTree partition, ISpatialPartition<int> otherPartition)
      {
        var enumerable = Pool.Obtain();
        enumerable._partition = partition;
        enumerable._otherPartition = otherPartition;
        enumerable._leafNodes = partition.GetLeafNodes(otherPartition.Aabb).GetEnumerator();
        return enumerable;
      }

      protected override bool OnNext(out Pair<int> current)
      {
        while (true)
        {
          if (_otherCandidates == null)
          {
            if (_leafNodes.MoveNext())
            {
              var leaf = _leafNodes.Current;
              Aabb leafAabb = _partition.GetAabb(leaf);
              _otherCandidates = _otherPartition.GetOverlaps(leafAabb).GetEnumerator();
            }
            else
            {
              current = default(Pair<int>);
              return false;
            }
          }

          while (_otherCandidates.MoveNext())
          {
            var leaf = _leafNodes.Current;
            var otherCandidate = _otherCandidates.Current;
            var overlap = new Pair<int>(leaf.Item, otherCandidate);
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


    private sealed class GetOverlapsWithTransformedPartitionWork : PooledEnumerable<Pair<int>>
    {
      private static readonly ResourcePool<GetOverlapsWithTransformedPartitionWork> Pool = new ResourcePool<GetOverlapsWithTransformedPartitionWork>(() => new GetOverlapsWithTransformedPartitionWork(), x => x.Initialize(), null);
      private CompressedAabbTree _partition;
      private ISpatialPartition<int> _otherPartition;
      private IEnumerator<Node> _leafNodes;
      private Aabb _leafAabb;
      private IEnumerator<int> _otherCandidates;
      private Vector3F _scale;
      private Vector3F _otherScaleInverse;
      private Pose _toOther;

      public static IEnumerable<Pair<int>> Create(CompressedAabbTree partition,
        ISpatialPartition<int> otherPartition, IEnumerable<Node> leafNodes,
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

      protected override bool OnNext(out Pair<int> current)
      {
        while (true)
        {
          if (_otherCandidates == null)
          {
            if (_leafNodes.MoveNext())
            {
              var leaf = _leafNodes.Current;
              _leafAabb = _partition.GetAabb(leaf);
              _leafAabb = _leafAabb.GetAabb(_scale, _toOther);
              _leafAabb.Scale(_otherScaleInverse);
              _otherCandidates = _otherPartition.GetOverlaps(_leafAabb).GetEnumerator();
            }
            else
            {
              current = default(Pair<int>);
              return false;
            }
          }

          while (_otherCandidates.MoveNext())
          {
            var leaf = _leafNodes.Current;
            var otherCandidate = _otherCandidates.Current;
            var overlap = new Pair<int>(leaf.Item, otherCandidate);
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
    // ReSharper restore StaticFieldInGenericType
  }
}
#endif
