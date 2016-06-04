// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  partial class CompressedAabbTree
  {
    /// <inheritdoc/>
    public IEnumerable<Pair<int>> GetOverlaps(ISpatialPartition<int> otherPartition)
    {
      if (otherPartition == null)
        throw new ArgumentNullException("otherPartition");

      var otherBasePartition = otherPartition as BasePartition<int>;
      if (otherBasePartition != null)
        otherBasePartition.UpdateInternal();
      else
        otherPartition.Update(false);

      Update(false);

#if !POOL_ENUMERABLES
      // Test all leaf nodes that touch the other partition's AABB.
      foreach (var leaf in GetLeafNodes(otherPartition.Aabb))
      {
        var otherCandidates = otherPartition.GetOverlaps(GetAabb(leaf));

        // We return one pair for each candidate vs. otherItem overlap.
        foreach (var otherCandidate in otherCandidates)
        {
          var overlap = new Pair<int>(leaf.Item, otherCandidate);
          if (Filter == null || Filter.Filter(overlap))
            yield return overlap;
        }
      }
#else
      return GetOverlapsWithPartitionWork.Create(this, otherPartition);
#endif
    }


    /// <inheritdoc/>
    public IEnumerable<Pair<int>> GetOverlaps(Vector3F scale, Pose pose, ISpatialPartition<int> otherPartition, Vector3F otherScale, Pose otherPose)
    {
      if (otherPartition == null)
        throw new ArgumentNullException("otherPartition");

      var otherBasePartition = otherPartition as BasePartition<int>;
      if (otherBasePartition != null)
        otherBasePartition.UpdateInternal();
      else
        otherPartition.Update(false);

      Update(false);

      // Compute transformations.
      Vector3F scaleInverse = Vector3F.One / scale;
      Vector3F otherScaleInverse = Vector3F.One / otherScale;
      Pose toLocal = pose.Inverse * otherPose;
      Pose toOther = toLocal.Inverse;

      // Transform the AABB of the other partition into space of the this partition.
      var otherAabb = otherPartition.Aabb;
      otherAabb = otherAabb.GetAabb(otherScale, toLocal); // Apply local scale and transform to scaled local space of this partition.
      otherAabb.Scale(scaleInverse);                      // Transform to unscaled local space of this partition.

      var leafNodes = GetLeafNodes(otherAabb);

#if !POOL_ENUMERABLES
      foreach (var leaf in leafNodes)
      {
        // Transform AABB of this partition into space of the other partition.
        var aabb = GetAabb(leaf);
        aabb = aabb.GetAabb(scale, toOther);  // Apply local scale and transform to scaled local space of other partition.
        aabb.Scale(otherScaleInverse);        // Transform to unscaled local space of other partition.

        foreach (var otherCandidate in otherPartition.GetOverlaps(aabb))
        {
          var overlap = new Pair<int>(leaf.Item, otherCandidate);
          if (Filter == null || Filter.Filter(overlap))
            yield return overlap;
        }
      }
#else
      return GetOverlapsWithTransformedPartitionWork.Create(this, otherPartition, leafNodes, ref scale, ref otherScaleInverse, ref toOther);
#endif
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    public void GetClosestPointCandidates(Vector3F scale, Pose pose, ISpatialPartition<int> otherPartition, Vector3F otherScale, Pose otherPose, Func<int, int, float> callback)
    {
      if (otherPartition == null)
        throw new ArgumentNullException("otherPartition");

      if (callback == null)
        throw new ArgumentNullException("callback");

      // Make sure we are up-to-date.
      var otherBasePartition = otherPartition as BasePartition<int>;
      if (otherBasePartition != null)
        otherBasePartition.UpdateInternal();
      else
        otherPartition.Update(false);

      Update(false);

      if (_numberOfItems == 0)
        return;

      if (otherPartition is ISupportClosestPointQueries<int>)
      {
        // ----- CompressedAabbTree vs. ISupportClosestPointQueries<int>
        GetClosestPointCandidatesImpl(scale, pose, (ISupportClosestPointQueries<int>)otherPartition, otherScale, otherPose, callback);
      }
      else
      {
        // ----- CompressedAabbTree vs. *
        GetClosestPointCandidatesImpl(otherPartition, callback);
      }
    }


    private void GetClosestPointCandidatesImpl(Vector3F scale, Pose pose, ISupportClosestPointQueries<int> otherPartition, Vector3F otherScale, Pose otherPose, Func<int, int, float> callback)
    {
      // Test leaf nodes against other partition.

      // Use a wrapper for the callback to reduce the parameters from Func<T, T, float> to 
      // Func<T, float>.
      ClosestPointCallbackWrapper<int> callbackWrapper = ClosestPointCallbackWrapper<int>.Create();
      callbackWrapper.OriginalCallback = callback;

      // Prepare transformation to transform leaf AABBs into local space of other partition.
      Pose toOther = otherPose.Inverse * pose;
      Vector3F otherScaleInverse = Vector3F.One / otherScale;

      float closestPointDistanceSquared = float.PositiveInfinity;
      foreach (Node node in _nodes)
      {
        if (node.IsLeaf)
        {
          callbackWrapper.Item = node.Item;

          // Transform AABB into local space of other partition.
          Aabb aabb = GetAabb(node).GetAabb(scale, toOther);
          aabb.Scale(otherScaleInverse);

          closestPointDistanceSquared = otherPartition.GetClosestPointCandidates(aabb, closestPointDistanceSquared, callbackWrapper.Callback);
          if (closestPointDistanceSquared < 0)
          {
            // closestPointDistanceSquared == -1 indicates early exit.
            break;
          }
        }
      }

      callbackWrapper.Recycle();
    }


    private void GetClosestPointCandidatesImpl(ISpatialPartition<int> otherPartition, Func<int, int, float> callback)
    {
      // Return all possible pairs.
      foreach (Node node in _nodes)
      {
        if (node.IsLeaf)
        {
          foreach (var otherItem in otherPartition)
          {
            // TODO: We could compute the AABBs, the minDistance of the AABBs and ignore
            // this pair if the minDistance of the AABBs is greater than the current 
            // closestPointDistance.

            float closestPointDistanceSquared = callback(node.Item, otherItem);
            if (closestPointDistanceSquared < 0)
            {
              // closestPointDistanceSquared == -1 indicates early exit.
              return;
            }
          }
        }
      }
    }

/*
    // TODO: Use ordered pair Pair<T, T> instead of unordered pair Pair<T>.
    // TODO: See also implementation of AabbTree<T>.
    public void GetOverlaps(Vector3F scale, Pose pose, CompressedAabbTree otherTree, Vector3F otherScale, Pose otherPose, List<Pair<int>> overlaps)
    {
      // Compute transformations.
      Vector3F scaleInverse = Vector3F.One / scale;
      Vector3F otherScaleInverse = Vector3F.One / otherScale;
      Pose toLocal = pose.Inverse * otherPose;
      Pose toOther = otherPose.Inverse * pose;

      var otherAabb = otherTree.Aabb;
      otherAabb = otherAabb.GetAabb(otherScale, toLocal);
      otherAabb.Scale(scaleInverse);                      

      int index = 0;
      while (index < _nodes.Length)
      {
        Node node = _nodes[index];
        bool haveContact = GeometryHelper.HaveContact(GetAabb(node), otherAabb);

        if (haveContact && node.IsLeaf)
        {
          var aabb = GetAabb(node);
          Shapes.Aabb.GetAabb(ref aabb, scale, toOther);
          aabb.ScalePositive(otherScaleInverse);
          
          int otherIndex = 0;
          while (otherIndex < otherTree._nodes.Length)
          {
            Node otherNode = otherTree._nodes[otherIndex];
            bool otherHaveContact = GeometryHelper.HaveContact(otherTree.GetAabb(otherNode), aabb);

            if (otherHaveContact && otherNode.IsLeaf)
            {
              var overlap = new Pair<int>(node.Item, otherNode.Item);
              overlaps.Add(overlap);
            }

            if (otherHaveContact || otherNode.IsLeaf)
              otherIndex++;
            else
              otherIndex += otherNode.EscapeOffset;
          }
        }

        if (haveContact || node.IsLeaf)
          index++;
        else
          index += node.EscapeOffset;
      }
    }
*/
  }
}
