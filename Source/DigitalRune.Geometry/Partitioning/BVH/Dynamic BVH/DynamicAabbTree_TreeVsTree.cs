// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  partial class DynamicAabbTree<T>
  {
    // TODO: Add DynamicAabbTree<T> vs. AabbTree<T>.


    /// <inheritdoc/>
    public override IEnumerable<Pair<T>> GetOverlaps(ISpatialPartition<T> otherPartition)
    {
      if (otherPartition == null)
        throw new ArgumentNullException("otherPartition");

      var otherBasePartition = otherPartition as BasePartition<T>;
      if (otherBasePartition != null)
        otherBasePartition.UpdateInternal();
      else
        otherPartition.Update(false);

      UpdateInternal();

      if (_root == null)
        return LinqHelper.Empty<Pair<T>>();

      var otherTree = otherPartition as DynamicAabbTree<T>;
      if (otherTree == null)
      {
        // DynamicAabbTree<T> vs. ISpatialPartition<T>.
        return GetOverlapsImpl(otherPartition);
      }
      else
      {
        // DynamicAabbTree<T> vs. DynamicAabbTree<T>
        if (otherTree._root == null)
          return LinqHelper.Empty<Pair<T>>();

        return GetOverlapsImpl(otherTree);
      }
    }


    private IEnumerable<Pair<T>> GetOverlapsImpl(ISpatialPartition<T> otherPartition)
    {
#if !POOL_ENUMERABLES
      // Test all leaf nodes that touch the other partition's AABB.
      foreach (var leaf in GetLeafNodes(otherPartition.Aabb))
      {
        var otherCandidates = otherPartition.GetOverlaps(leaf.Aabb);

        // We return one pair for each candidate vs. otherItem overlap.
        foreach (var otherCandidate in otherCandidates)
        {
          var overlap = new Pair<T>(leaf.Item, otherCandidate);
          if (Filter == null || Filter.Filter(overlap))
            yield return overlap;
        }
      }
#else
      // Avoiding garbage:
      return GetOverlapsWithPartitionWork.Create(this, otherPartition);
#endif
    }


    private IEnumerable<Pair<T>> GetOverlapsImpl(DynamicAabbTree<T> otherTree)
    {
#if !POOL_ENUMERABLES
      var stack = DigitalRune.ResourcePools<Pair<Node, Node>>.Stacks.Obtain();
      stack.Push(new Pair<Node, Node>(_root, otherTree._root));
      while (stack.Count > 0)
      {
        var nodePair = stack.Pop();
        var nodeA = nodePair.First;
        var nodeB = nodePair.Second;

        if (nodeA == nodeB)
        {
          if (!nodeA.IsLeaf)
          {
            stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeA.RightChild));
            stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeA.RightChild));
            stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeA.LeftChild));
          }
        }
        else if (GeometryHelper.HaveContact(nodeA.Aabb, nodeB.Aabb))
        {
          if (!nodeA.IsLeaf)
          {
            if (!nodeB.IsLeaf)
            {
              stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB.RightChild));
              stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB.RightChild));
              stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB.LeftChild));
              stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB.LeftChild));
            }
            else
            {
              stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB));
              stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB));
            }
          }
          else
          {
            if (!nodeB.IsLeaf)
            {
              stack.Push(new Pair<Node, Node>(nodeA, nodeB.RightChild));
              stack.Push(new Pair<Node, Node>(nodeA, nodeB.LeftChild));
            }
            else
            {
              // Leaf overlap.
              var overlap = new Pair<T>(nodeA.Item, nodeB.Item);
              if (Filter == null || Filter.Filter(overlap))
                yield return overlap;
            }
          }
        }
      }

      DigitalRune.ResourcePools<Pair<Node, Node>>.Stacks.Recycle(stack);
#else
      // Avoiding garbage:
      return GetOverlapsWithTreeWork.Create(this, otherTree);
#endif
    }


    /// <inheritdoc/>
    public override IEnumerable<Pair<T>> GetOverlaps(Vector3F scale, Pose pose, ISpatialPartition<T> otherPartition, Vector3F otherScale, Pose otherPose)
    {
      if (otherPartition == null)
        throw new ArgumentNullException("otherPartition");

      var otherBasePartition = otherPartition as BasePartition<T>;
      if (otherBasePartition != null)
        otherBasePartition.UpdateInternal();
      else
        otherPartition.Update(false);

      UpdateInternal();

      if (_root == null)
        return LinqHelper.Empty<Pair<T>>();

      var otherTree = otherPartition as DynamicAabbTree<T>;
      if (otherTree == null)
      {
        // DynamicAabbTree<T> vs. ISpatialPartition<T>.
        return GetOverlapsImpl(scale, otherPartition, otherScale, pose.Inverse * otherPose);
      }
      else
      {
        // DynamicAabbTree<T> vs. DynamicAabbTree<T>
        if (otherTree._root == null)
          return LinqHelper.Empty<Pair<T>>();

        return GetOverlapsImpl(scale, otherTree, otherScale, pose.Inverse * otherPose);
      }
    }


    private IEnumerable<Pair<T>> GetOverlapsImpl(Vector3F scale, ISpatialPartition<T> otherPartition, Vector3F otherScale, Pose otherPose)
    {
      // Compute transformations.
      Vector3F scaleInverse = Vector3F.One / scale;
      Vector3F otherScaleInverse = Vector3F.One / otherScale;
      Pose toLocal = otherPose;
      Pose toOther = otherPose.Inverse;

      // Transform the AABB of the other partition into space of the this partition.
      var otherAabb = otherPartition.Aabb;
      otherAabb = otherAabb.GetAabb(otherScale, toLocal); // Apply local scale and transform to scaled local space of this partition.
      otherAabb.Scale(scaleInverse);                      // Transform to unscaled local space of this partition.

      var leafNodes = GetLeafNodes(otherAabb);

#if !POOL_ENUMERABLES
      foreach (var leaf in leafNodes)
      {
        // Transform AABB of this partition into space of the other partition.
        Aabb aabb = leaf.Aabb.GetAabb(scale, toOther);    // Apply local scale and transform to scaled local space of other partition.
        aabb.Scale(otherScaleInverse);                    // Transform to unscaled local space of other partition.

        foreach (var otherCandidate in otherPartition.GetOverlaps(aabb))
        {
          var overlap = new Pair<T>(leaf.Item, otherCandidate);
          if (Filter == null || Filter.Filter(overlap))
            yield return overlap;
        }
      }
#else
      // Avoiding garbage:
      return GetOverlapsWithTransformedPartitionWork.Create(this, otherPartition, leafNodes, ref scale, ref otherScaleInverse, ref toOther);
#endif
    }


    private IEnumerable<Pair<T>> GetOverlapsImpl(Vector3F scale, DynamicAabbTree<T> otherTree, Vector3F otherScale, Pose otherPose)
    {
      // Compute transformations.
      Vector3F scaleA = scale;      // Rename scales for readability.
      Vector3F scaleB = otherScale;
      Pose bToA = otherPose;

#if !POOL_ENUMERABLES
      var stack = DigitalRune.ResourcePools<Pair<Node, Node>>.Stacks.Obtain();
      stack.Push(new Pair<Node, Node>(_root, otherTree._root));
      while (stack.Count > 0)
      {
        var nodePair = stack.Pop();
        var nodeA = nodePair.First;
        var nodeB = nodePair.Second;

        if (HaveAabbContact(nodeA, scaleA, nodeB, scaleB, bToA))
        {
          if (!nodeA.IsLeaf)
          {
            if (!nodeB.IsLeaf)
            {
              stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB.RightChild));
              stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB.RightChild));
              stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB.LeftChild));
              stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB.LeftChild));
            }
            else
            {
              stack.Push(new Pair<Node, Node>(nodeA.RightChild, nodeB));
              stack.Push(new Pair<Node, Node>(nodeA.LeftChild, nodeB));
            }
          }
          else
          {
            if (!nodeB.IsLeaf)
            {
              stack.Push(new Pair<Node, Node>(nodeA, nodeB.RightChild));
              stack.Push(new Pair<Node, Node>(nodeA, nodeB.LeftChild));
            }
            else
            {
              // Leaf overlap.
              var overlap = new Pair<T>(nodeA.Item, nodeB.Item);
              if (Filter == null || Filter.Filter(overlap))
                yield return overlap;
            }
          }
        }
      }

      DigitalRune.ResourcePools<Pair<Node, Node>>.Stacks.Recycle(stack);
#else
      // Avoiding garbage:
      return GetOverlapsWithTransformedTreeWork.Create(this, otherTree, ref scaleA, ref scaleB, ref bToA);
#endif
    }


    /// <summary>
    /// Compares the sizes of two transformed AABB nodes.
    /// </summary>
    /// <param name="nodeA">The first AABB node.</param>
    /// <param name="scaleA">The scale of the first AABB node.</param>
    /// <param name="nodeB">The second AABB node.</param>
    /// <param name="scaleB">The scale of the second AABB node.</param>
    /// <returns>
    /// <see langword="true"/> if is <paramref name="nodeA"/> bigger than <paramref name="nodeB"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private static bool IsABiggerThanB(Node nodeA, Vector3F scaleA, Node nodeB, Vector3F scaleB)
    {
      Vector3F extentA = nodeA.Aabb.Extent * Vector3F.Absolute(scaleA);
      Vector3F extentB = nodeB.Aabb.Extent * Vector3F.Absolute(scaleB);
      return extentA.LargestComponent > extentB.LargestComponent;
    }


    /// <summary>
    /// Makes an AABB check for the two node AABBs where the second has a pose and scale.
    /// </summary>
    /// <param name="nodeA">The first AABB node.</param>
    /// <param name="scaleA">The scale of the first AABB node.</param>
    /// <param name="nodeB">The second AABB node.</param>
    /// <param name="scaleB">The scale of the second AABB node.</param>
    /// <param name="poseB">The pose of the second AABB node relative to the first.</param>
    /// <returns>
    /// <see langword="true"/> if the AABBs have contact; otherwise, <see langword="false"/>.
    /// </returns>
    private static bool HaveAabbContact(Node nodeA, Vector3F scaleA, Node nodeB, Vector3F scaleB, Pose poseB)
    {
      // Scale AABB of A.
      Aabb aabbA = nodeA.Aabb;
      aabbA.Scale(scaleA);

      // Scale AABB of B.
      Aabb aabbB = nodeB.Aabb;
      aabbB.Scale(scaleB);

      // Convert AABB of B to OBB in local space of A.
      Vector3F boxExtentB = aabbB.Extent;
      Pose poseBoxB = poseB * new Pose(aabbB.Center);

      // Test AABB of A against OBB.
      return GeometryHelper.HaveContact(aabbA, boxExtentB, poseBoxB,
                                        false);   // We do not make edge tests.
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    public void GetClosestPointCandidates(Vector3F scale, Pose pose, ISpatialPartition<T> otherPartition, Vector3F otherScale, Pose otherPose, Func<T, T, float> callback)
    {
      if (otherPartition == null)
        throw new ArgumentNullException("otherPartition");

      if (callback == null)
        throw new ArgumentNullException("callback");

      // Make sure we are up-to-date.
      var otherBasePartition = otherPartition as BasePartition<T>;
      if (otherBasePartition != null)
        otherBasePartition.UpdateInternal();
      else
        otherPartition.Update(false);

      UpdateInternal();

      if (_root == null)
        return;

      if (otherPartition is DynamicAabbTree<T>)
      {
        // ----- DynamicAabbTree<T> vs. DynamicAabbTree<T>
        // (Transform second partition into local space.)
        var otherTree = (DynamicAabbTree<T>)otherPartition;
        float closestPointDistanceSquared = float.PositiveInfinity;
        GetClosestPointCandidatesImpl(_root, scale, otherTree._root, otherScale, pose.Inverse * otherPose, callback, ref closestPointDistanceSquared);
      }
      else if (otherPartition is ISupportClosestPointQueries<T>)
      {
        // ----- DynamicAabbTree<T> vs. ISupportClosestPointQueries<T>
        GetClosestPointCandidatesImpl(scale, pose, (ISupportClosestPointQueries<T>)otherPartition, otherScale, otherPose, callback);
      }
      else
      {
        // ----- DynamicAabbTree<T> vs. *
        GetClosestPointCandidatesImpl(otherPartition, callback);
      }
    }


    /// <summary>
    /// Gets all items that are candidates for the smallest closest-point distance to items in a
    /// given partition. (Internal, recursive.)
    /// </summary>
    /// <param name="nodeA">The first AABB node.</param>
    /// <param name="scaleA">The scale of the first AABB node.</param>
    /// <param name="nodeB">The second AABB node.</param>
    /// <param name="scaleB">The scale of the second AABB node.</param>
    /// <param name="poseB">The pose of the second AABB node relative to the first.</param>
    /// <param name="callback">
    /// The callback that is called with each found candidate item. The method must compute the
    /// closest-point on the candidate item and return the squared closest-point distance.
    /// </param>
    /// <param name="closestPointDistanceSquared">
    /// The squared of the current closest-point distance.
    /// </param>
    private void GetClosestPointCandidatesImpl(Node nodeA, Vector3F scaleA, Node nodeB, Vector3F scaleB, Pose poseB, Func<T, T, float> callback, ref float closestPointDistanceSquared)
    {
      // closestPointDistanceSquared == -1 indicates early exit.
      if (nodeA == null || nodeB == null || closestPointDistanceSquared < 0)
      {
        // Abort.
        return;
      }

      // If we have a contact, it is not necessary to examine nodes with no AABB contact
      // because they cannot give a closer point pair.
      if (closestPointDistanceSquared == 0 && !HaveAabbContact(nodeA, scaleA, nodeB, scaleB, poseB))
        return;

      if (nodeA.IsLeaf && nodeB.IsLeaf)
      {
        // Leaf vs leaf.
        if (Filter == null || Filter.Filter(new Pair<T>(nodeA.Item, nodeB.Item)))
        {
          var leafDistanceSquared = callback(nodeA.Item, nodeB.Item);
          closestPointDistanceSquared = Math.Min(leafDistanceSquared, closestPointDistanceSquared);
        }
        return;
      }

      // Determine which one to split:
      // If B is a leaf, we have to split A. OR
      // If A can be split and is bigger than B, we split A.
      if (nodeB.IsLeaf || (!nodeA.IsLeaf && IsABiggerThanB(nodeA, scaleA, nodeB, scaleB)))
      {
        #region ----- Split A -----

        Node leftChild = nodeA.LeftChild;
        Node rightChild = nodeA.RightChild;

        if (closestPointDistanceSquared == 0)
        {
          // We have contact, so we must examine all children.
          GetClosestPointCandidatesImpl(leftChild, scaleA, nodeB, scaleB, poseB, callback, ref closestPointDistanceSquared);
          GetClosestPointCandidatesImpl(rightChild, scaleA, nodeB, scaleB, poseB, callback, ref closestPointDistanceSquared);
          return;
        }

        // No contact. Use lower bound estimates to search the best nodes first.
        // TODO: Optimize: We do not need to call GeometryHelper.GetDistanceLowerBoundSquared for OBBs. We have AABB + OBB.

        // Scale AABB of B.
        Aabb aabbB = nodeB.Aabb;
        aabbB.Scale(scaleB);

        // Convert AABB of B to OBB in local space of A.
        Vector3F boxExtentB = aabbB.Extent;
        Pose poseBoxB = poseB * new Pose(aabbB.Center);

        // Scale left child AABB of A.
        Aabb leftChildAabb = leftChild.Aabb;
        leftChildAabb.Scale(scaleA);

        // Convert left child AABB of A to OBB in local space of A.
        Vector3F leftChildBoxExtent = leftChildAabb.Extent;
        Pose leftChildBoxPose = new Pose(leftChildAabb.Center);

        // Compute lower bound for distance to left child.
        float minDistanceLeft = GeometryHelper.GetDistanceLowerBoundSquared(leftChildBoxExtent, leftChildBoxPose, boxExtentB, poseBoxB);

        // Scale right child AABB of A.
        Aabb rightChildAabb = rightChild.Aabb;
        rightChildAabb.Scale(scaleA);

        // Convert right child AABB of A to OBB in local space of A.
        Vector3F rightChildBoxExtent = rightChildAabb.Extent;
        Pose rightChildBoxPose = new Pose(rightChildAabb.Center);

        // Compute lower bound for distance to right child.
        float minDistanceRight = GeometryHelper.GetDistanceLowerBoundSquared(rightChildBoxExtent, rightChildBoxPose, boxExtentB, poseBoxB);

        if (minDistanceLeft < minDistanceRight)
        {
          // Stop if other child cannot improve result.
          // Note: Do not invert the "if" because this way it is safe if minDistanceLeft is NaN.
          if (minDistanceLeft > closestPointDistanceSquared)
            return;

          // Handle left first.
          GetClosestPointCandidatesImpl(leftChild, scaleA, nodeB, scaleB, poseB, callback, ref closestPointDistanceSquared);

          // Stop if other child cannot improve result.
          // Note: Do not invert the "if" because this way it is safe if minDistanceRight is NaN.
          if (minDistanceRight > closestPointDistanceSquared)
            return;

          GetClosestPointCandidatesImpl(rightChild, scaleA, nodeB, scaleB, poseB, callback, ref closestPointDistanceSquared);
        }
        else
        {
          // Stop if other child cannot improve result.
          // Note: Do not invert the "if" because this way it is safe if minDistanceRight is NaN.
          if (minDistanceRight > closestPointDistanceSquared)
            return;

          // Handle right first.
          GetClosestPointCandidatesImpl(rightChild, scaleA, nodeB, scaleB, poseB, callback, ref closestPointDistanceSquared);

          // Stop if other child cannot improve result.
          // Note: Do not invert the "if" because this way it is safe if minDistanceLeft is NaN.
          if (minDistanceLeft > closestPointDistanceSquared)
            return;

          GetClosestPointCandidatesImpl(leftChild, scaleA, nodeB, scaleB, poseB, callback, ref closestPointDistanceSquared);
        }
        #endregion
      }
      else
      {
        #region ----- Split B -----

        Node leftChildB = nodeB.LeftChild;
        Node rightChildB = nodeB.RightChild;

        if (closestPointDistanceSquared == 0)
        {
          // We have contact, so we must examine all children.
          GetClosestPointCandidatesImpl(nodeA, scaleA, leftChildB, scaleB, poseB, callback, ref closestPointDistanceSquared);
          GetClosestPointCandidatesImpl(nodeA, scaleA, rightChildB, scaleB, poseB, callback, ref closestPointDistanceSquared);
        }
        else
        {
          // No contact. Use lower bound estimates to search the best nodes first.
          // TODO: Optimize: We do not need to call GeometryHelper.GetDistanceLowerBoundSquared for OBBs. We have AABB + OBB.

          // Scale AABB of A.
          Aabb aabbA = nodeA.Aabb;
          aabbA.Scale(scaleA);

          // Convert AABB of A to OBB in local space of A.
          Vector3F boxExtentA = aabbA.Extent;
          Pose poseBoxA = new Pose(aabbA.Center);

          // Scale left child AABB of B.
          Aabb leftChildAabb = leftChildB.Aabb;
          leftChildAabb.Scale(scaleB);

          // Convert left child AABB of B to OBB in local space of A.
          Vector3F childBoxExtent = leftChildAabb.Extent;
          Pose poseLeft = poseB * new Pose(leftChildAabb.Center);

          // Compute lower bound for distance to left child.
          float minDistanceLeft = GeometryHelper.GetDistanceLowerBoundSquared(childBoxExtent, poseLeft, boxExtentA, poseBoxA);

          // Scale right child AABB of B.
          Aabb rightChildAabb = rightChildB.Aabb;
          rightChildAabb.Scale(scaleB);

          // Convert right child AABB of B to OBB in local space of A.
          childBoxExtent = rightChildAabb.Extent;
          Pose poseRight = poseB * new Pose(rightChildAabb.Center);

          // Compute lower bound for distance to right child.
          float minDistanceRight = GeometryHelper.GetDistanceLowerBoundSquared(childBoxExtent, poseRight, boxExtentA, poseBoxA);

          if (minDistanceLeft < minDistanceRight)
          {
            // Stop if other child cannot improve result.
            // Note: Do not invert the "if" because this way it is safe if minDistanceLeft is NaN.
            if (minDistanceLeft > closestPointDistanceSquared)
              return;

            // Handle left first.
            GetClosestPointCandidatesImpl(nodeA, scaleA, leftChildB, scaleB, poseB, callback, ref closestPointDistanceSquared);

            // Stop if other child cannot improve result.
            // Note: Do not invert the "if" because this way it is safe if minDistanceRight is NaN.
            if (minDistanceRight > closestPointDistanceSquared)
              return;

            GetClosestPointCandidatesImpl(nodeA, scaleA, rightChildB, scaleB, poseB, callback, ref closestPointDistanceSquared);
          }
          else
          {
            // Stop if other child cannot improve result.
            // Note: Do not invert the "if" because this way it is safe if minDistanceRight is NaN.
            if (minDistanceRight > closestPointDistanceSquared)
              return;

            // Handle right first.
            GetClosestPointCandidatesImpl(nodeA, scaleA, rightChildB, scaleB, poseB, callback, ref closestPointDistanceSquared);

            // Stop if other child cannot improve result.
            // Note: Do not invert the "if" because this way it is safe if minDistanceLeft is NaN.
            if (minDistanceLeft > closestPointDistanceSquared)
              return;

            GetClosestPointCandidatesImpl(nodeA, scaleA, leftChildB, scaleB, poseB, callback, ref closestPointDistanceSquared);
          }
        }
        #endregion
      }
    }


    private void GetClosestPointCandidatesImpl(Vector3F scale, Pose pose, ISupportClosestPointQueries<T> otherPartition, Vector3F otherScale, Pose otherPose, Func<T, T, float> callback)
    {
      // Test leaf nodes against other partition.

      // Use a wrapper for the callback to reduce the parameters from Func<T, T, float> to 
      // Func<T, float>.
      ClosestPointCallbackWrapper<T> callbackWrapper = ClosestPointCallbackWrapper<T>.Create();
      callbackWrapper.OriginalCallback = callback;

      // Prepare transformation to transform leaf AABBs into local space of other partition.
      Pose toOther = otherPose.Inverse * pose;
      Vector3F otherScaleInverse = Vector3F.One / otherScale;

      float closestPointDistanceSquared = float.PositiveInfinity;
      foreach (var leaf in _leaves.Values)
      {
        callbackWrapper.Item = leaf.Item;

        // Transform AABB into local space of other partition.
        Aabb aabb = leaf.Aabb.GetAabb(scale, toOther);
        aabb.Scale(otherScaleInverse);

        closestPointDistanceSquared = otherPartition.GetClosestPointCandidates(aabb, closestPointDistanceSquared, callbackWrapper.Callback);
        if (closestPointDistanceSquared < 0)
        {
          // closestPointDistanceSquared == -1 indicates early exit.
          break;
        }
      }

      callbackWrapper.Recycle();
    }


    private void GetClosestPointCandidatesImpl(ISpatialPartition<T> otherPartition, Func<T, T, float> callback)
    {
      // Return all possible pairs.
      foreach (var itemA in Items)
      {
        foreach (var itemB in otherPartition)
        {
          float closestPointDistanceSquared = callback(itemA, itemB);
          if (closestPointDistanceSquared < 0)
          {
            // closestPointDistanceSquared == -1 indicates early exit.
            return;
          }
        }
      }
    }
  }
}
