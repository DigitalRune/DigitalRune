// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;

#if !POOL_ENUMERABLES
using DigitalRune.Mathematics;
#endif


namespace DigitalRune.Geometry.Partitioning
{
  public partial class BasePartition<T>
  {
    /// <inheritdoc/>
    public abstract IEnumerable<T> GetOverlaps(Aabb aabb);


    /// <summary>
    /// Gets the items that touch the given item.
    /// </summary>
    /// <param name="item">
    /// The item. (Whether the given item must be a part of the spatial partition or whether it can
    /// be an external object depends on the <see cref="GetAabbForItem"/> callback. The 
    /// <see cref="GetAabbForItem"/> must be able to compute the AABB for the given item.)
    /// </param>
    /// <returns>All items that touch the given item.</returns>
    /// <remarks>
    /// Filtering (see <see cref="Filter"/>) is applied to filter overlaps.
    /// </remarks>
    public IEnumerable<T> GetOverlaps(T item)
    {
      // Note: We could make this virtual, then in derived classes
      // the equality and filter tests could be made before the Aabb test.
      // --> No big advantage...or?

#if !POOL_ENUMERABLES
      Aabb aabb = GetAabbForItem(item);

      foreach (var touchedItem in GetOverlaps(aabb))
      {
        if (FilterSelfOverlap(new Pair<T>(touchedItem, item)))
          yield return touchedItem;
      }
#else
      // Avoiding garbage:
      return GetOverlapsWithItemWork.Create(this, item);
#endif
    }


    /// <inheritdoc/>
    public virtual IEnumerable<T> GetOverlaps(Ray ray)
    {
#if !POOL_ENUMERABLES
      Aabb rayAabb = new Aabb(ray.Origin, ray.Origin);
      rayAabb.Grow(ray.Origin + ray.Direction * ray.Length);

      var rayDirectionInverse = new Vector3F(
            1 / ray.Direction.X,
            1 / ray.Direction.Y,
            1 / ray.Direction.Z);

      float epsilon = Numeric.EpsilonF * (1 + Aabb.Extent.Length);

      foreach (var candidate in GetOverlaps(rayAabb))
        if (GeometryHelper.HaveContact(GetAabbForItem(candidate), ray.Origin, rayDirectionInverse, ray.Length, epsilon))
          yield return candidate;
#else
      // Avoiding garbage:
      return GetOverlapsWithRayWork.Create(this, ref ray);
#endif
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public IEnumerable<Pair<T>> GetOverlaps()
    {
      if (!EnableSelfOverlaps)
        throw new GeometryException("GetOverlaps() can only be used if EnableSelfOverlaps is set to true.");

      // Make sure we are up-to-date.
      UpdateInternal();

      return SelfOverlaps;
    }


    /// <inheritdoc/>
    public virtual IEnumerable<Pair<T>> GetOverlaps(ISpatialPartition<T> otherPartition)
    {
      if (otherPartition == null)
        throw new ArgumentNullException("otherPartition");

      var otherBasePartition = otherPartition as BasePartition<T>;
      if (otherBasePartition != null)
        otherBasePartition.UpdateInternal();
      else
        otherPartition.Update(false);

      UpdateInternal();

#if !POOL_ENUMERABLES
      // Get all items that touch the other partition's AABB.
      var candidates = GetOverlaps(otherPartition.Aabb);

      // Now, we test each candidate against the other partition.
      foreach (var candidate in candidates)
      {
        Aabb candidateAabb = GetAabbForItem(candidate);
        var otherCandidates = otherPartition.GetOverlaps(candidateAabb);

        // We return one pair for each candidate vs. otherItem overlap.
        foreach (var otherCandidate in otherCandidates)
        {
          var overlap = new Pair<T>(candidate, otherCandidate);
          if (Filter == null || Filter.Filter(overlap))
            yield return overlap;
        }
      }
#else
      // Avoiding garbage:
      return GetOverlapsWithPartitionWork.Create(this, otherPartition);
#endif
    }


    /// <inheritdoc/>
    public virtual IEnumerable<Pair<T>> GetOverlaps(Vector3F scale, Pose pose, ISpatialPartition<T> otherPartition, Vector3F otherScale, Pose otherPose)
    {
      if (otherPartition == null)
        throw new ArgumentNullException("otherPartition");

      var otherBasePartition = otherPartition as BasePartition<T>;
      if (otherBasePartition != null)
        otherBasePartition.UpdateInternal();
      else
        otherPartition.Update(false);

      UpdateInternal();

      // Compute transformations.
      Vector3F scaleInverse = Vector3F.One / scale;
      Vector3F otherScaleInverse = Vector3F.One / otherScale;
      Pose toLocal = pose.Inverse * otherPose;
      Pose toOther = toLocal.Inverse;

      // Transform the AABB of the other partition into space of the this partition.
      var otherAabb = otherPartition.Aabb;
      otherAabb = otherAabb.GetAabb(otherScale, toLocal); // Apply local scale and transform to scaled local space of this partition.
      otherAabb.Scale(scaleInverse);                      // Transform to unscaled local space of this partition.

      var candidates = GetOverlaps(otherAabb);

#if !POOL_ENUMERABLES
      foreach (var candidate in candidates)
      {
        // Transform AABB of this partition into space of the other partition.
        var aabb = GetAabbForItem(candidate);
        aabb = aabb.GetAabb(scale, toOther);  // Apply local scale and transform to scaled local space of other partition.
        aabb.Scale(otherScaleInverse);        // Transform to unscaled local space of other partition.

        foreach (var otherCandidate in otherPartition.GetOverlaps(aabb))
        {
          var overlap = new Pair<T>(candidate, otherCandidate);
          if (Filter == null || Filter.Filter(overlap))
            yield return overlap;
        }
      }
#else
      // Avoiding garbage:
      return GetOverlapsWithTransformedPartitionWork.Create(this, otherPartition, candidates, ref scale, ref otherScaleInverse, ref toOther);
#endif
    }
  }
}
