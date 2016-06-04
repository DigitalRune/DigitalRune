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
  public partial class BasePartition<T>
  {
    // ReSharper disable StaticFieldInGenericType
    private sealed class GetOverlapsWithItemWork : PooledEnumerable<T>
    {
      private static readonly ResourcePool<GetOverlapsWithItemWork> Pool = new ResourcePool<GetOverlapsWithItemWork>(() => new GetOverlapsWithItemWork(), x => x.Initialize(), null);
      private BasePartition<T> _partition;
      private T _item;
      private IEnumerator<T> _enumerator;

      public static IEnumerable<T> Create(BasePartition<T> partition, T item)
      {
        var enumerable = Pool.Obtain();
        enumerable._partition = partition;
        enumerable._item = item;
        Aabb aabb = partition.GetAabbForItem(item);
        enumerable._enumerator = partition.GetOverlaps(aabb).GetEnumerator();
        return enumerable;
      }

      protected override bool OnNext(out T current)
      {
        while (_enumerator.MoveNext())
        {
          var touchedItem = _enumerator.Current;
          if (_partition.FilterSelfOverlap(new Pair<T>(touchedItem, _item)))
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
        _partition = null;
        _item = default(T);
        _enumerator.Dispose();
        _enumerator = null;
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithRayWork : PooledEnumerable<T>
    {
      private static readonly ResourcePool<GetOverlapsWithRayWork> Pool = new ResourcePool<GetOverlapsWithRayWork>(() => new GetOverlapsWithRayWork(), x => x.Initialize(), null);
      private BasePartition<T> _partition;
      private Ray _ray;
      private Vector3F _rayDirectionInverse;
      private float _epsilon;
      private IEnumerator<T> _enumerator;

      public static IEnumerable<T> Create(BasePartition<T> partition, ref Ray ray)
      {
        var enumerable = Pool.Obtain();
        enumerable._partition = partition;
        enumerable._ray = ray;
        enumerable._rayDirectionInverse = new Vector3F(1 / ray.Direction.X,
                                                       1 / ray.Direction.Y,
                                                       1 / ray.Direction.Z);
        enumerable._epsilon = Numeric.EpsilonF * (1 + partition.Aabb.Extent.Length);

        Aabb rayAabb = new Aabb(ray.Origin, ray.Origin);
        rayAabb.Grow(ray.Origin + ray.Direction * ray.Length);
        enumerable._enumerator = partition.GetOverlaps(rayAabb).GetEnumerator();
        return enumerable;
      }

      protected override bool OnNext(out T current)
      {
        while (_enumerator.MoveNext())
        {
          var candidate = _enumerator.Current;
          if (GeometryHelper.HaveContact(_partition.GetAabbForItem(candidate), _ray.Origin, _rayDirectionInverse, _ray.Length, _epsilon))
          {
            current = candidate;
            return true;
          }
        }
        current = default(T);
        return false;
      }

      protected override void OnRecycle()
      {
        _partition = null;
        _enumerator.Dispose();
        _enumerator = null;
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithPartitionWork : PooledEnumerable<Pair<T>>
    {
      private static readonly ResourcePool<GetOverlapsWithPartitionWork> Pool = new ResourcePool<GetOverlapsWithPartitionWork>(() => new GetOverlapsWithPartitionWork(), x => x.Initialize(), null);
      private BasePartition<T> _partition;
      private ISpatialPartition<T> _otherPartition;
      private IEnumerator<T> _candidates;
      private IEnumerator<T> _otherCandidates;

      public static IEnumerable<Pair<T>> Create(BasePartition<T> partition, ISpatialPartition<T> otherPartition)
      {
        var enumerable = Pool.Obtain();
        enumerable._partition = partition;
        enumerable._otherPartition = otherPartition;
        enumerable._candidates = partition.GetOverlaps(otherPartition.Aabb).GetEnumerator();
        return enumerable;
      }

      protected override bool OnNext(out Pair<T> current)
      {
        while (true)
        {
          if (_otherCandidates == null)
          {
            if (_candidates.MoveNext())
            {
              var candidate = _candidates.Current;
              Aabb candidateAabb = _partition.GetAabbForItem(candidate);
              _otherCandidates = _otherPartition.GetOverlaps(candidateAabb).GetEnumerator();
            }
            else
            {
              current = default(Pair<T>);
              return false;
            }
          }

          while (_otherCandidates.MoveNext())
          {
            var candidate = _candidates.Current;
            var otherCandidate = _otherCandidates.Current;
            var overlap = new Pair<T>(candidate, otherCandidate);
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
        _candidates.Dispose();
        _candidates = null;
        if (_otherCandidates != null)
        {
          _otherCandidates.Dispose();
          _otherCandidates = null;
        }
        Pool.Recycle(this);
      }
    }


    private sealed class GetOverlapsWithTransformedPartitionWork : PooledEnumerable<Pair<T>>
    {
      private static readonly ResourcePool<GetOverlapsWithTransformedPartitionWork> Pool = new ResourcePool<GetOverlapsWithTransformedPartitionWork>(() => new GetOverlapsWithTransformedPartitionWork(), x => x.Initialize(), null);
      private BasePartition<T> _partition;
      private ISpatialPartition<T> _otherPartition;
      private IEnumerator<T> _candidates;
      private IEnumerator<T> _otherCandidates;
      private Vector3F _scale;
      private Vector3F _otherScaleInverse;
      private Pose _toOther;

      public static IEnumerable<Pair<T>> Create(BasePartition<T> partition, 
        ISpatialPartition<T> otherPartition, IEnumerable<T> candidates, 
        ref Vector3F scale, ref Vector3F otherScaleInverse, ref Pose toOther)
      {
        var enumerable = Pool.Obtain();
        enumerable._partition = partition;
        enumerable._otherPartition = otherPartition;
        enumerable._candidates = candidates.GetEnumerator();
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
            if (_candidates.MoveNext())
            {
              var candidate = _candidates.Current;
              var aabb = _partition.GetAabbForItem(candidate);
              aabb = aabb.GetAabb(_scale, _toOther);
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
            var candidate = _candidates.Current;
            var otherCandidate = _otherCandidates.Current;
            var overlap = new Pair<T>(candidate, otherCandidate);
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
        _candidates.Dispose();
        _candidates = null;
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
