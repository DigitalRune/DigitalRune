// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;

#if !POOL_ENUMERABLES
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
#endif


namespace DigitalRune.Geometry.Partitioning
{
  partial class CompressedAabbTree
  {
    /// <inheritdoc/>
    public IEnumerable<int> GetOverlaps(Aabb aabb)
    {
      Update(false);

      return GetOverlapsImpl(aabb);
    }


    private IEnumerable<int> GetOverlapsImpl(Aabb aabb)
    {
      // This method avoids the Update() call!

#if !POOL_ENUMERABLES
      if (_numberOfItems == 0)
        yield break;
      
      // ----- Stackless traversal of tree:
      // The AABB tree nodes are stored in preorder traversal order. We can visit them in linear
      // order. The EscapeOffset of each node can be used to skip a subtree.
      int index = 0;
      while (index < _nodes.Length)
      {
        Node node = _nodes[index];
        bool haveContact = GeometryHelper.HaveContact(GetAabb(node), aabb);

        if (haveContact && node.IsLeaf)
          yield return node.Item;

        if (haveContact || node.IsLeaf)
        {
          // Given AABB intersects the internal AABB tree node or the node is a leaf.
          // Continue with next item in preorder traversal order.
          index++;
        }
        else
        {
          // Given AABB does not touch the internal AABB tree node.
          // --> Skip the subtree.
          index += node.EscapeOffset;
        }
      }
#else
      // Avoiding garbage:
      return GetOverlapsWork.Create(this, ref aabb);
#endif
    }


    /// <summary>
    /// Gets the leaf nodes that touch the given AABB. (Same as 
    /// <see cref="GetOverlaps(Shapes.Aabb)"/> except we directly return the AABB tree node.
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box.</param>
    /// <returns>All items that touch the given AABB.</returns>
    /// <remarks>
    /// Filtering (see <see cref="Filter"/>) is not applied.
    /// </remarks>
    private IEnumerable<Node> GetLeafNodes(Aabb aabb)
    {
      // Note: This methods is the same as GetOverlaps(Aabb), but instead of returning items we 
      // return the nodes directly. This is used in tree vs. tree tests, so we do not have to 
      // recompute the AABBs of each leaf node.

      Update(false);

#if !POOL_ENUMERABLES
      if (_numberOfItems == 0)
        yield break;

      // Stackless traversal of tree.
      // The AABB tree nodes are stored in preorder traversal order. We can visit them in linear
      // order. The EscapeOffset of each node can be used to skip a subtree.
      int index = 0;
      while (index < _nodes.Length)
      {
        Node node = _nodes[index];
        bool haveContact = GeometryHelper.HaveContact(GetAabb(node), aabb);

        if (haveContact && node.IsLeaf)
          yield return node;

        if (haveContact || node.IsLeaf)
        {
          // Given AABB intersects the internal AABB tree node or the node is a leaf.
          // Continue with next item in preorder traversal order.
          index++;
        }
        else
        {
          // Given AABB does not touch the internal AABB tree node.
          // --> Skip the subtree.
          index += node.EscapeOffset;
        }
      }
#else
      // Avoiding garbage:
      return GetLeafNodesWork.Create(this, ref aabb);
#endif
    }


    /// <summary>
    /// Gets the items that touch the given item.
    /// </summary>
    /// <param name="item">
    /// The item. (The given item must be part of the spatial partition. External items are not 
    /// supported.)
    /// </param>
    /// <returns>All items that touch the given item.</returns>
    /// <remarks>
    /// Filtering (see <see cref="Filter"/>) is applied to filter overlaps.
    /// </remarks>
    public IEnumerable<int> GetOverlaps(int item)
    {
#if !POOL_ENUMERABLES
      var aabb = GetAabbForItem(item);

      foreach (var touchedItem in GetOverlaps(aabb))
      {
        if (FilterSelfOverlap(new Pair<int>(touchedItem, item)))
          yield return touchedItem;
      }
#else
      // Avoiding garbage:
      return GetOverlapsWithItemWork.Create(this, item);
#endif
    }


    /// <inheritdoc/>
    public IEnumerable<int> GetOverlaps(Ray ray)
    {
      Update(false);

#if !POOL_ENUMERABLES
      if (_numberOfItems == 0)
        yield break;

      var rayDirectionInverse = new Vector3F(
            1 / ray.Direction.X,
            1 / ray.Direction.Y,
            1 / ray.Direction.Z);

      float epsilon = Numeric.EpsilonF * (1 + Aabb.Extent.Length);

      // ----- Stackless traversal of tree:
      // The AABB tree nodes are stored in preorder traversal order. We can visit them in linear
      // order. The EscapeOffset of each node can be used to skip a subtree.
      int index = 0;
      while (index < _nodes.Length)
      {
        Node node = _nodes[index];
        bool haveContact = GeometryHelper.HaveContact(GetAabb(node), ray.Origin, rayDirectionInverse, ray.Length, epsilon);

        if (haveContact && node.IsLeaf)
          yield return node.Item;

        if (haveContact || node.IsLeaf)
        {
          // Given AABB intersects the internal AABB tree node or the node is a leaf.
          // Continue with next item in preorder traversal order.
          index++;
        }
        else
        {
          // Given AABB does not touch the internal AABB tree node.
          // --> Skip the subtree.
          index += node.EscapeOffset;
        }
      }
#else
      // Avoiding garbage:
      return GetOverlapsWithRayWork.Create(this, ref ray);
#endif
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public IEnumerable<Pair<int>> GetOverlaps()
    {
      if (!EnableSelfOverlaps)
        throw new GeometryException("GetOverlaps() can only be used if EnableSelfOverlaps is set to true.");

      // Make sure we are up-to-date.
      Update(false);

      return _selfOverlaps;
    }


    /// <inheritdoc/>
    public float GetClosestPointCandidates(Aabb aabb, float maxDistanceSquared, Func<int, float> callback)
    {
      if (callback == null)
        throw new ArgumentNullException("callback");

      Update(false);

      if (_numberOfItems == 0)
        return -1;

      float closestPointDistanceSquared = maxDistanceSquared;
      GetClosestPointCandidatesImpl(0, aabb, callback, ref closestPointDistanceSquared);
      return closestPointDistanceSquared;
    }


    // Recursive traversal of tree.
    private void GetClosestPointCandidatesImpl(int index, Aabb aabb, Func<int, float> callback, ref float closestPointDistanceSquared)
    {
      // closestPointDistanceSquared == -1 can be returned by callback to abort the query.
      if (closestPointDistanceSquared < 0)
      {
        // Abort.
        return;
      }

      Node node = _nodes[index];

      // If we have a contact, it is not necessary to examine nodes with no AABB contact
      // because they cannot give a closer point pair.
      if (closestPointDistanceSquared == 0 && !GeometryHelper.HaveContact(GetAabb(node), aabb))
        return;

      if (node.IsLeaf)
      {
        // Node is leaf - call callback and updated closest-point distance.
        var leafDistanceSquared = callback(node.Item);
        closestPointDistanceSquared = Math.Min(leafDistanceSquared, closestPointDistanceSquared);
        return;
      }

      int leftIndex = index + 1;
      Node leftChild = _nodes[leftIndex];

      int rightIndex = (leftChild.IsLeaf) ? leftIndex + 1 : leftIndex + leftChild.EscapeOffset;
      Node rightChild = _nodes[rightIndex];

      if (closestPointDistanceSquared == 0)
      {
        // We have contact, so we must examine all children.
        GetClosestPointCandidatesImpl(leftIndex, aabb, callback, ref closestPointDistanceSquared);
        GetClosestPointCandidatesImpl(rightIndex, aabb, callback, ref closestPointDistanceSquared);
        return;
      }

      // No contact. Use lower bound estimates to search the best nodes first.
      float minDistanceLeft = GeometryHelper.GetDistanceSquared(GetAabb(leftChild), aabb);
      float minDistanceRight = GeometryHelper.GetDistanceSquared(GetAabb(rightChild), aabb);

      if (minDistanceLeft < minDistanceRight)
      {
        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceLeft is NaN.
        if (minDistanceLeft > closestPointDistanceSquared)
          return;

        // Handle left first.
        GetClosestPointCandidatesImpl(leftIndex, aabb, callback, ref closestPointDistanceSquared);

        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceRight is NaN.
        if (minDistanceRight > closestPointDistanceSquared)
          return;

        GetClosestPointCandidatesImpl(rightIndex, aabb, callback, ref closestPointDistanceSquared);
      }
      else
      {
        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceRight is NaN.
        if (minDistanceRight > closestPointDistanceSquared)
          return;

        // Handle right first.
        GetClosestPointCandidatesImpl(rightIndex, aabb, callback, ref closestPointDistanceSquared);

        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceLeft is NaN.
        if (minDistanceLeft > closestPointDistanceSquared)
          return;

        GetClosestPointCandidatesImpl(leftIndex, aabb, callback, ref closestPointDistanceSquared);
      }
    }
  }
}
