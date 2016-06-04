// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;

#if !POOL_ENUMERABLES
using DigitalRune.Collections;
using DigitalRune.Mathematics;
#endif


namespace DigitalRune.Geometry.Partitioning
{
  partial class DynamicAabbTree<T>
  {
    /// <inheritdoc/>
    public override IEnumerable<T> GetOverlaps(Aabb aabb)
    {
      UpdateInternal();

#if !POOL_ENUMERABLES
      if (_root == null)
        yield break;

      var stack = DigitalRune.ResourcePools<Node>.Stacks.Obtain();
      stack.Push(_root);
      while (stack.Count > 0)
      {
        Node node = stack.Pop();

        if (GeometryHelper.HaveContact(node.Aabb, aabb))
        {
          if (node.IsLeaf)
          {
            yield return node.Item;
          }
          else
          {
            stack.Push(node.RightChild);
            stack.Push(node.LeftChild);
          }
        }
      }

      DigitalRune.ResourcePools<Node>.Stacks.Recycle(stack);
#else
      // Avoiding garbage:
      return GetOverlapsWork.Create(this, ref aabb);
#endif
    }


    private IEnumerable<T> GetOverlapsImpl(Node leaf)
    {
      // Note: This method is the same as GetOverlapsImpl(Aabb), except that before 
      // checking the AABBs we compare the nodes. This removes some unnecessary AABB
      // checks when computing self-overlaps. Filtering is not applied.

#if !POOL_ENUMERABLES
      if (_root == null)
        yield break;

      var stack = DigitalRune.ResourcePools<Node>.Stacks.Obtain();
      stack.Push(_root);
      while (stack.Count > 0)
      {
        Node node = stack.Pop();

        if (node != leaf && GeometryHelper.HaveContact(node.Aabb, leaf.Aabb))
        {
          if (node.IsLeaf)
          {
            yield return node.Item;
          }
          else
          {
            stack.Push(node.RightChild);
            stack.Push(node.LeftChild);
          }
        }
      }

      DigitalRune.ResourcePools<Node>.Stacks.Recycle(stack);
#else
      return GetOverlapsWithLeafWork.Create(this, leaf);
#endif
    }


    /// <summary>
    /// Gets the leaf nodes that touch the given AABB. (Same as <see cref="GetOverlaps(Aabb)"/>
    /// except we directly return the AABB tree node.
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box.</param>
    /// <returns>All leaf nodes that touch the given AABB.</returns>
    /// <remarks>
    /// Filtering (see <see cref="BasePartition{T}.Filter"/>) is not applied.
    /// </remarks>
    private IEnumerable<Node> GetLeafNodes(Aabb aabb)
    {
      // Note: This methods is the same as GetOverlaps(Aabb), but instead of returning items we 
      // return the nodes directly. This is used in tree vs. tree tests, so we do not have to 
      // recompute the AABBs of each leaf node.

      UpdateInternal();

#if !POOL_ENUMERABLES
      if (_root == null)
        yield break;

      var stack = DigitalRune.ResourcePools<Node>.Stacks.Obtain();
      stack.Push(_root);
      while (stack.Count > 0)
      {
        Node node = stack.Pop();

        if (GeometryHelper.HaveContact(node.Aabb, aabb))
        {
          if (node.IsLeaf)
          {
            yield return node;
          }
          else
          {
            stack.Push(node.RightChild);
            stack.Push(node.LeftChild);
          }
        }
      }

      DigitalRune.ResourcePools<Node>.Stacks.Recycle(stack);
#else
      // Avoiding garbage:
      return GetLeafNodesWork.Create(this, ref aabb);
#endif
    }


    /// <inheritdoc/>
    public override IEnumerable<T> GetOverlaps(Ray ray)
    {
      UpdateInternal();

#if !POOL_ENUMERABLES
      if (_root == null)
        yield break;

      var rayDirectionInverse = new Vector3F(
            1 / ray.Direction.X,
            1 / ray.Direction.Y,
            1 / ray.Direction.Z);

      float epsilon = Numeric.EpsilonF * (1 + Aabb.Extent.Length);

      var stack = DigitalRune.ResourcePools<Node>.Stacks.Obtain();
      stack.Push(_root);
      while (stack.Count > 0)
      {
        var node = stack.Pop();

        if (GeometryHelper.HaveContact(node.Aabb, ray.Origin, rayDirectionInverse, ray.Length, epsilon))
        {
          if (node.IsLeaf)
          {
            yield return node.Item;
          }
          else
          {
            stack.Push(node.RightChild);
            stack.Push(node.LeftChild);
          }
        }
      }

      DigitalRune.ResourcePools<Node>.Stacks.Recycle(stack);
#else
      // Avoiding garbage:
      return GetOverlapsWithRayWork.Create(this, ref ray);
#endif
    }


    /// <inheritdoc/>
    public IEnumerable<T> GetOverlaps(IList<Plane> planes)
    {
      UpdateInternal();

#if !POOL_ENUMERABLES
      if (_root == null || planes == null)
        yield break;

      var numberOfPlanes = planes.Count;
      if (numberOfPlanes <= 0 || numberOfPlanes >= 32)
        throw new ArgumentException("The bounding volume (k-DOP) needs to have at least 1 but max 32 planes.", "planes");

      var stack0 = DigitalRune.ResourcePools<Pair<Node, int>>.Stacks.Obtain();
      var stack1 = DigitalRune.ResourcePools<Node>.Stacks.Obtain();
      var signs = DigitalRune.ResourcePools<int>.Lists.Obtain();
      for (int i = 0; i < numberOfPlanes; i++)
      {
        signs.Add(((planes[i].Normal.X >= 0) ? 1 : 0)
                  + ((planes[i].Normal.Y >= 0) ? 2 : 0)
                  + ((planes[i].Normal.Z >= 0) ? 4 : 0));
      }

      // Push entry: (node, mask)
      // The mask determines which planes need to be tested.
      //   0 ... Plane needs to be tested.
      //   1 ... Node is behind plane.
      stack0.Push(new Pair<Node, int>(_root, 0));

      // Mask when node is inside bounding volume.
      int inside = (1 << numberOfPlanes) - 1;

      while (stack0.Count > 0)
      {
        var entry = stack0.Pop();
        var node = entry.First;
        int mask = entry.Second;

        bool outside = false;
        for (int i = 0, j = 1; !outside && i < numberOfPlanes; i++, j <<= 1)
        {
          if ((mask & j) == 0)
          {
            var aabb = node.Aabb;
            var plane = planes[i];
            int side = Classify(ref aabb, ref plane, signs[i]);
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
          if (mask == inside || node.IsLeaf)
          {
            // Return all leaves.
            stack1.Push(node);
            while (stack1.Count > 0)
            {
              var n = stack1.Pop();
              if (n.IsLeaf)
              {
                yield return n.Item;
              }
              else
              {
                stack1.Push(n.RightChild);
                stack1.Push(n.LeftChild);
              }
            }
          }
          else
          {
            stack0.Push(new Pair<Node, int>(node.RightChild, mask));
            stack0.Push(new Pair<Node, int>(node.LeftChild, mask));
          }
        }
      }

      DigitalRune.ResourcePools<Pair<Node, int>>.Stacks.Recycle(stack0);
      DigitalRune.ResourcePools<Node>.Stacks.Recycle(stack1);
      DigitalRune.ResourcePools<int>.Lists.Recycle(signs);
#else
      if (_root == null || planes == null)
        return LinqHelper.Empty<T>();

      var numberOfPlanes = planes.Count;
      if (numberOfPlanes <= 0 || numberOfPlanes >= 32)
        throw new ArgumentException("The bounding volume (k-DOP) needs to have at least 1 but max 32 planes.", "planes");

      return GetOverlapsWithKDOPWork.Create(this, planes);
#endif
    }


    private static int Classify(ref Aabb aabb, ref Plane plane, int signs)
    {
      Vector3F min = aabb.Minimum;
      Vector3F max = aabb.Maximum;

      // Get near and far corners of the AABB.
      Vector3F pNear, pFar;
      switch (signs)
      {
        case (0 + 0 + 0): // normal = (-x, -y, -z)
          pNear = max;
          pFar  = min;
          break;
        case (1 + 0 + 0): // normal = (+x, -y, -z)
          pNear = new Vector3F(min.X, max.Y, max.Z);
          pFar  = new Vector3F(max.X, min.Y, min.Z);
          break;
        case (0 + 2 + 0):// normal = (-x, +y, -z)
          pNear = new Vector3F(max.X, min.Y, max.Z);
          pFar  = new Vector3F(min.X, max.Y, min.Z);
          break;
        case (1 + 2 + 0): // normal = (+x, +y, -z)
          pNear = new Vector3F(min.X, min.Y, max.Z);
          pFar  = new Vector3F(max.X, max.Y, min.Z);
          break;
        case (0 + 0 + 4): // normal = (-x, -y, +z)
          pNear = new Vector3F(max.X, max.Y, min.Z);
          pFar  = new Vector3F(min.X, min.Y, max.Z);
          break;
        case (1 + 0 + 4): // normal = (+x, -y, +z)
          pNear = new Vector3F(min.X, max.Y, min.Z);
          pFar  = new Vector3F(max.X, min.Y, max.Z);
          break;
        case (0 + 2 + 4): // normal = (-x, +y, +z)
          pNear = new Vector3F(max.X, min.Y, min.Z);
          pFar  = new Vector3F(min.X, max.Y, max.Z);
          break;
        case (1 + 2 + 4): // normal = (+x, +y, +z)
          pNear = min;
          pFar  = max;
          break;
        default:
          throw new ArgumentException("Invalid signs.", "signs");
      }

      if (Vector3F.Dot(plane.Normal, pNear) >= plane.DistanceFromOrigin)
        return +1;  // AABB is in positive halfspace.

      if (Vector3F.Dot(plane.Normal, pFar) <= plane.DistanceFromOrigin)
        return -1;  // AABB is in negative halfspace.

      // AABB intersects plane.
      return 0;
    }


    /// <inheritdoc/>
    public float GetClosestPointCandidates(Aabb aabb, float maxDistanceSquared, Func<T, float> callback)
    {
      if (callback == null)
        throw new ArgumentNullException("callback");

      UpdateInternal();

      if (_root == null)
        return -1;

      float closestPointDistanceSquared = maxDistanceSquared;
      GetClosestPointCandidatesImpl(_root, aabb, callback, ref closestPointDistanceSquared);
      return closestPointDistanceSquared;
    }


    private static void GetClosestPointCandidatesImpl(Node node, Aabb aabb, Func<T, float> callback, ref float closestPointDistanceSquared)
    {
      // closestPointDistanceSquared == -1 indicates early exit.
      if (closestPointDistanceSquared < 0)
      {
        // Abort.
        return;
      }

      // If we have a contact, it is not necessary to examine nodes with no AABB contact
      // because they cannot give a closer point pair.
      if (closestPointDistanceSquared == 0 && !GeometryHelper.HaveContact(aabb, node.Aabb))
        return;

      if (node.IsLeaf)
      {
        // Node is leaf - call callback and updated closest-point distance.
        var leafDistanceSquared = callback(node.Item);
        closestPointDistanceSquared = Math.Min(leafDistanceSquared, closestPointDistanceSquared);
        return;
      }

      Node leftChild = node.LeftChild;
      Node rightChild = node.RightChild;

      if (closestPointDistanceSquared == 0)
      {
        // We have contact, so we must examine all children.
        GetClosestPointCandidatesImpl(leftChild, aabb, callback, ref closestPointDistanceSquared);
        GetClosestPointCandidatesImpl(rightChild, aabb, callback, ref closestPointDistanceSquared);
        return;
      }

      // No contact. Use lower bound estimates to search the best nodes first.
      float minDistanceLeft = GeometryHelper.GetDistanceSquared(aabb, leftChild.Aabb);
      float minDistanceRight = GeometryHelper.GetDistanceSquared(aabb, rightChild.Aabb);

      if (minDistanceLeft < minDistanceRight)
      {
        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceLeft is NaN.
        if (minDistanceLeft > closestPointDistanceSquared)
          return;

        // Handle left first.
        GetClosestPointCandidatesImpl(leftChild, aabb, callback, ref closestPointDistanceSquared);

        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceRight is NaN.
        if (minDistanceRight > closestPointDistanceSquared)
          return;

        GetClosestPointCandidatesImpl(rightChild, aabb, callback, ref closestPointDistanceSquared);
      }
      else
      {
        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceRight is NaN.
        if (minDistanceRight > closestPointDistanceSquared)
          return;

        // Handle right first.
        GetClosestPointCandidatesImpl(rightChild, aabb, callback, ref closestPointDistanceSquared);

        // Stop if other child cannot improve result.
        // Note: Do not invert the "if" because this way it is safe if minDistanceLeft is NaN.
        if (minDistanceLeft > closestPointDistanceSquared)
          return;

        GetClosestPointCandidatesImpl(leftChild, aabb, callback, ref closestPointDistanceSquared);
      }
    }
  }
}
