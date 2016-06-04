// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Partitioning
{
  partial class SweepAndPruneSpace<T>
  {
    /// <inheritdoc/>
    public override IEnumerable<T> GetOverlaps(Aabb aabb)
    {
      // Make sure we are up-to-date.
      UpdateInternal();

      // TODO: This could be vastly improved with stabbing numbers!
      // TODO: Choose better sweep direction.
      // TODO: Sweep along all 3 axis instead only x.
      // TODO: Add special support for ray collision objects vs the AABB.
      // Currently we sweep up along the x-axis. We could check the AABB center
      // versus the full SAP space to choose a better sweep axis and direction (up vs. down).

      var overlapCandidates = DigitalRune.ResourcePools<ItemInfo>.HashSets.Obtain();

      // The x-axis edge list.
      List<Edge> edgesX = _edges[0];
      int numberOfEdgesPerAxis = edgesX.Count;

      // First, sort max x position up.
      for (int i = 0; i < numberOfEdgesPerAxis; i++)
      {
        var edge = edgesX[i];
        if (edge.Position <= aabb.Maximum.X)
        {
          if (!edge.IsMax)                    // Crossed Min edge --> new overlap.
            overlapCandidates.Add(edge.Info);
        }
        else
        {
          // Search finished.
          break;
        }
      }

      // Next, sort min x position up and remove candidates.
      for (int i = 0; i < numberOfEdgesPerAxis; i++)
      {
        var edge = edgesX[i];
        if (edge.Position < aabb.Minimum.X)
        {
          if (edge.IsMax)                       // Crossed Max edge --> remove overlap.
            overlapCandidates.Remove(edge.Info);
        }
        else
        {
          // Search finished.
          break;
        }
      }

#if !POOL_ENUMERABLES
      foreach (var candidate in overlapCandidates)
      {
        if (GeometryHelper.HaveContact(candidate.Aabb, aabb))
          yield return candidate.Item;
      }

      DigitalRune.ResourcePools<ItemInfo>.HashSets.Recycle(overlapCandidates);        
#else
      // Avoiding garbage:
      return GetOverlapsWork.Create(overlapCandidates, ref aabb);
#endif
    }


#if POOL_ENUMERABLES
    private sealed class GetOverlapsWork : PooledEnumerable<T>
    {
      // ReSharper disable StaticFieldInGenericType
      private static readonly ResourcePool<GetOverlapsWork> Pool = new ResourcePool<GetOverlapsWork>(() => new GetOverlapsWork(), x => x.Initialize(), null);
      // ReSharper restore StaticFieldInGenericType
      private Aabb _aabb;
      private HashSet<ItemInfo> _candidates;
      private HashSet<ItemInfo>.Enumerator _enumerator;

      public static IEnumerable<T> Create(HashSet<ItemInfo> candidates, ref Aabb aabb)
      {
        var enumerable = Pool.Obtain();
        enumerable._aabb = aabb;
        enumerable._candidates = candidates;
        enumerable._enumerator = candidates.GetEnumerator();
        return enumerable;
      }

      protected override bool OnNext(out T current)
      {
        while (_enumerator.MoveNext())
        {
          ItemInfo candidate = _enumerator.Current;
          if (GeometryHelper.HaveContact(candidate.Aabb, _aabb))
          {
            current = candidate.Item;
            return true;
          }
        }

        current = default(T);
        return false;
      }

      protected override void OnRecycle()
      {
        DigitalRune.ResourcePools<ItemInfo>.HashSets.Recycle(_candidates);
        Pool.Recycle(this);
      }
    }
#endif
  }
}
