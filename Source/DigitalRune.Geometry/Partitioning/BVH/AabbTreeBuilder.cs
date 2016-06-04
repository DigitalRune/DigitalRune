// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Provides methods to build AABB trees.
  /// </summary>
  internal static class AabbTreeBuilder
  {
    // Note: The parameter leaves could be of type IList<IAabbTreeNode<T>> and we could pass a simple
    // array instead of a List<IAabbTreeNode<T>>. This works fine in Windows, but raises a 
    // NotSupportedException on Xbox 360. (The .NET CF seems to have problems dealing with arrays 
    // cast to IList<IAabbTreeNode<T>>.)

    /// <summary>
    /// Builds the AABB tree for the specified leaf nodes.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="leaves">The leaves of the tree.</param>
    /// <param name="createNode">A function that creates a new, empty node.</param>
    /// <returns>The root node of the subtree.</returns>
    /// <remarks>
    /// The order of the nodes in <paramref name="leaves"/> is changed.
    /// </remarks>
    public static IAabbTreeNode<T> Build<T>(List<IAabbTreeNode<T>> leaves, Func<IAabbTreeNode<T>> createNode)
    {
      //return BuildTopDownCenterSplit(leaves, 0, leaves.Count - 1, createNode);
      //return BuildTopDownVarianceBasedSplit(leaves, 0, leaves.Count - 1, createNode);
      //return BuildBottomUp(leaves, 0, leaves.Count - 1, createNode);
      return BuildMixed(leaves, 0, leaves.Count - 1, createNode);
    }


    /// <summary>
    /// Builds the AABB tree for the specified leaf nodes.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="leaves">The leaves of the tree.</param>
    /// <param name="createNode">A function that creates a new, empty node.</param>
    /// <param name="bottomUpThreshold">
    /// The threshold in number of nodes (for example 128). If the number of nodes in a subtree is
    /// less than or equal to this value, the subtree will be built bottom-up.
    /// </param>
    /// <returns>The root node of the subtree.</returns>
    /// <remarks>
    /// The order of the nodes in <paramref name="leaves"/> is changed.
    /// </remarks>
    public static IAabbTreeNode<T> Build<T>(List<IAabbTreeNode<T>> leaves, Func<IAabbTreeNode<T>> createNode, int bottomUpThreshold)
    {
      //return BuildTopDownCenterSplit(leaves, 0, leaves.Count - 1, createNode);
      //return BuildTopDownVarianceBasedSplit(leaves, 0, leaves.Count - 1, createNode);
      //return BuildBottomUp(leaves, 0, leaves.Count - 1, createNode);
      return BuildMixed(leaves, 0, leaves.Count - 1, createNode, bottomUpThreshold);
    }

    #region ----- Bottom-Up Approach -----

    // The following basic top-down approach is used in Bullet (btDbvt).

    /// <summary>
    /// Builds the subtree top-down.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="leaves">The leaves of the tree.</param>
    /// <param name="firstLeaf">The first leaf.</param>
    /// <param name="lastLeaf">The last leaf.</param>
    /// <param name="createNode">A function that creates a new, empty node.</param>
    /// <returns>The root node of the subtree.</returns>
    private static IAabbTreeNode<T> BuildBottomUp<T>(List<IAabbTreeNode<T>> leaves, int firstLeaf, int lastLeaf, Func<IAabbTreeNode<T>> createNode)
    {
      // Replace 'leaves' with new list we can work with.
      var nodes = DigitalRune.ResourcePools<IAabbTreeNode<T>>.Lists.Obtain();
      for (int i = firstLeaf; i <= lastLeaf; i++)
        nodes.Add(leaves[i]);

      // Iteratively merge nodes (subtrees) until we have a single tree.
      while (nodes.Count > 1)
      {
        float minSize = float.PositiveInfinity;
        Pair<int> minPair = new Pair<int>(-1, -1);
        for (int i = 0; i < nodes.Count; i++)
        {
          // Compare node with all subsequent nodes in list.
          for (int j = i + 1; j < nodes.Count; j++)
          {
            Aabb mergedAabb = Aabb.Merge(nodes[i].Aabb, nodes[j].Aabb);

            // Compute a "size" which can be used to estimate the fit of the new node. 
            // Here: volume + edges
            Vector3F edges = mergedAabb.Extent;
            float size = edges.X * edges.Y * edges.Z + edges.X + edges.Y + edges.Z;
            if (size <= minSize)  // Note: We compare with ≤ because size can be ∞.
            {
              minSize = size;
              minPair.First = i;
              minPair.Second = j;
            }
          }
        }

        if (minPair.First < 0 || minPair.Second < 0)
          throw new GeometryException("Could not build AABB tree because the AABB of an item is invalid (e.g. NaN).");

        // Create a new parent node that merges the two subtrees.
        IAabbTreeNode<T> leftChild = nodes[minPair.First];
        IAabbTreeNode<T> rightChild = nodes[minPair.Second];
        IAabbTreeNode<T> parent = createNode();
        parent.Aabb = Aabb.Merge(leftChild.Aabb, rightChild.Aabb);
        parent.LeftChild = leftChild;
        parent.RightChild = rightChild;

        // Remove subtrees from list and add the new node.
        nodes.RemoveAt(minPair.Second);
        nodes.RemoveAt(minPair.First);
        nodes.Add(parent);
      }

      IAabbTreeNode<T> root = nodes[0];
      DigitalRune.ResourcePools<IAabbTreeNode<T>>.Lists.Recycle(nodes);
      return root;
    }
    #endregion


    #region ----- Top-Down Approach: Center Split -----
    
    // The following basic top-down approach is described in many papers about AABB trees:
    //   - Determine combined AABB of nodes.
    //   - Split axis: largest AABB extent of combined AABB.
    //   - Split value: center of combined AABB.
    //   - If split fails, evenly distribute nodes.

    /// <summary>
    /// Builds the subtree top-down.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="leaves">The leaves of the tree.</param>
    /// <param name="firstLeaf">The first leaf.</param>
    /// <param name="lastLeaf">The last leaf.</param>
    /// <param name="createNode">A function that creates a new, empty node.</param>
    /// <returns>The root node of the subtree.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    private static IAabbTreeNode<T> BuildTopDownCenterSplit<T>(List<IAabbTreeNode<T>> leaves, int firstLeaf, int lastLeaf, Func<IAabbTreeNode<T>> createNode)
    {
      int numberOfNodes = lastLeaf - firstLeaf + 1;
      if (numberOfNodes == 1)
        return leaves[firstLeaf];

      IAabbTreeNode<T> node = createNode();
      node.Aabb = MergeLeaveAabbs(leaves, firstLeaf, lastLeaf);

      // Get max axis.
      int splitAxis = node.Aabb.Extent.IndexOfLargestComponent;

      // Split at center of AABB.
      float splitValue = node.Aabb.Center[splitAxis];

      // Sort indices in list.
      int rightLeaf;  // Leaf index where the right tree begins.
      SortLeaves(leaves, firstLeaf, lastLeaf, splitAxis, splitValue, out rightLeaf);

      // If one subtree is empty, we create two equal trees.
      if (rightLeaf == firstLeaf || rightLeaf > lastLeaf)
        rightLeaf = (firstLeaf + lastLeaf + 1) / 2;

      if (rightLeaf == firstLeaf + 1)
      {
        // Left child is leaf.
        node.LeftChild = leaves[firstLeaf];
      }
      else
      {
        // Build left subtree.
        node.LeftChild = BuildTopDownCenterSplit(leaves, firstLeaf, rightLeaf - 1, createNode);
      }

      if (rightLeaf == lastLeaf)
      {
        // Right child is leaf.
        node.RightChild = leaves[rightLeaf];
      }
      else
      {
        // Build right subtree.
        node.RightChild = BuildTopDownCenterSplit(leaves, rightLeaf, lastLeaf, createNode);
      }

      return node;
    }
    #endregion


    #region ----- Top-Down Approach: Variance-Based Split -----

    // The following basic top-down approach is used in Bullet (btQuantizedBvh):
    //   - Determine mean and variance of nodes.
    //   - Split axis: axis of largest variance
    //   - Split value: mean

    /// <summary>
    /// Builds the subtree top-down.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="leaves">The leaves of the tree.</param>
    /// <param name="firstLeaf">The first leaf.</param>
    /// <param name="lastLeaf">The last leaf.</param>
    /// <param name="createNode">A function that creates a new, empty node.</param>
    /// <returns>The root node of the subtree.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    private static IAabbTreeNode<T> BuildTopDownVarianceBasedSplit<T>(List<IAabbTreeNode<T>> leaves, int firstLeaf, int lastLeaf, Func<IAabbTreeNode<T>> createNode)
    {
      int numberOfNodes = lastLeaf - firstLeaf + 1;
      if (numberOfNodes == 1)
        return leaves[firstLeaf];

      // Compute mean of AABB centers.
      Vector3F mean = new Vector3F();
      for (int i = firstLeaf; i <= lastLeaf; i++)
        mean += leaves[i].Aabb.Center;

      mean /= numberOfNodes;

      // Compute variance of AABB centers.
      Vector3F variance = new Vector3F();
      for (int i = firstLeaf; i <= lastLeaf; i++)
      {
        Vector3F difference = leaves[i].Aabb.Center - mean;
        variance += difference * difference;
      }

      variance /= numberOfNodes;

      // Choose axis of max variance as split axis.
      int splitAxis = variance.IndexOfLargestComponent;
      float splitValue = mean[splitAxis];

      IAabbTreeNode<T> node = createNode();
      node.Aabb = MergeLeaveAabbs(leaves, firstLeaf, lastLeaf);

      // Sort indices in list.
      int rightLeaf;  // Leaf index where the right tree begins.
      SortLeaves(leaves, firstLeaf, lastLeaf, splitAxis, splitValue, out rightLeaf);

      // Avoid unbalanced trees. (Unbalanced trees could cause stack overflows.)
      int minNodesPerSubtree = numberOfNodes / 3;
      if (rightLeaf == firstLeaf                          // Left subtree is empty.
          || rightLeaf > lastLeaf                         // Right subtree is empty.
          || firstLeaf + minNodesPerSubtree >= rightLeaf  // Not enough nodes in right subtree.
          || rightLeaf + minNodesPerSubtree >= lastLeaf)  // Not enough nodes in left subtree.
      {
        // Evenly distribute nodes among subtree.
        rightLeaf = (firstLeaf + lastLeaf + 1) / 2;
      }

      // Build subtrees.
      node.LeftChild = BuildTopDownVarianceBasedSplit(leaves, firstLeaf, rightLeaf - 1, createNode);
      node.RightChild = BuildTopDownVarianceBasedSplit(leaves, rightLeaf, lastLeaf, createNode);
      return node;
    }
    #endregion


    #region ----- Mixed Approach -----

    // The following basic mixed approach is used in Bullet (btDbvt).
    //   - Determine mean and variance of nodes.
    //   - Split axis: axis of largest variance
    //   - Split value: mean

    /// <summary>
    /// Builds the subtree using a mixed bottom-up/top-down approach.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="leaves">The leaves of the tree.</param>
    /// <param name="firstLeaf">The first leaf.</param>
    /// <param name="lastLeaf">The last leaf.</param>
    /// <param name="createNode">A function that creates a new, empty node.</param>
    /// <returns>The root node of the subtree.</returns>
    private static IAabbTreeNode<T> BuildMixed<T>(List<IAabbTreeNode<T>> leaves, int firstLeaf, int lastLeaf, Func<IAabbTreeNode<T>> createNode)
    {
      const int DefaultBottomUpThreshold = 128;
      return BuildMixed(leaves, firstLeaf, lastLeaf, createNode, DefaultBottomUpThreshold);
    }


    /// <summary>
    /// Builds the subtree using a mixed bottom-up/top-down approach.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="leaves">The leaves of the tree.</param>
    /// <param name="firstLeaf">The first leaf.</param>
    /// <param name="lastLeaf">The last leaf.</param>
    /// <param name="createNode">A function that creates a new, empty node.</param>
    /// <param name="bottomUpThreshold">
    /// The threshold in number of nodes (for example 128). If the number of nodes in a subtree is
    /// less than or equal to this value, the subtree will be built bottom-up.
    /// </param>
    /// <returns>The root node of the subtree.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Body")]
    private static IAabbTreeNode<T> BuildMixed<T>(List<IAabbTreeNode<T>> leaves, int firstLeaf, int lastLeaf, Func<IAabbTreeNode<T>> createNode, int bottomUpThreshold)
    {
      int numberOfNodes = lastLeaf - firstLeaf + 1;
      if (numberOfNodes == 1)
        return leaves[firstLeaf];

      if (numberOfNodes <= bottomUpThreshold)
        return BuildBottomUp(leaves, firstLeaf, lastLeaf, createNode);

      IAabbTreeNode<T> node = createNode();
      node.Aabb = MergeLeaveAabbs(leaves, firstLeaf, lastLeaf);
      Vector3F center = node.Aabb.Center;

      // Check which split yields the most balanced tree.
      int[,] splitCount = new int[3,2];   // splitCount[number of axis, left or right]
      for (int i = firstLeaf; i <= lastLeaf; i++)
      {
        Vector3F offset = leaves[i].Aabb.Center - center;
        for (int axis = 0; axis < 3; axis++)
        {
          if (offset[axis] <= 0)
            splitCount[axis, 0]++;
          else
            splitCount[axis, 1]++;
        }
      }

      int minDifference = numberOfNodes;
      int splitAxis = -1;
      float splitValue = 0;
      for (int axis = 0; axis < 3; axis++)
      {
        int leftCount = splitCount[axis, 0];
        int rightCount = splitCount[axis, 1];
        if (leftCount > 0 && rightCount > 0)
        {
          int difference = Math.Abs(leftCount - rightCount);
          if (difference < minDifference)
          {
            minDifference = difference;
            splitAxis = axis;
            splitValue = center[axis];
          }
        }
      }

      int rightLeaf;  // Leaf index where the right tree begins.
      if (splitAxis >= 0)
      {
        // Sort indices in list.
        SortLeaves(leaves, firstLeaf, lastLeaf, splitAxis, splitValue, out rightLeaf);
      }
      else
      {
        // Evenly distribute nodes among subtree.
        rightLeaf = (firstLeaf + lastLeaf + 1) / 2;
      }

      // Build subtrees.
      node.LeftChild = BuildMixed(leaves, firstLeaf, rightLeaf - 1, createNode, bottomUpThreshold);
      node.RightChild = BuildMixed(leaves, rightLeaf, lastLeaf, createNode, bottomUpThreshold);
      return node;
    }
    #endregion


    /// <summary>
    /// Computes the combined AABB of the leaves.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="leaves">The leaves of the tree.</param>
    /// <param name="firstLeaf">The first leaf index (included).</param>
    /// <param name="lastLeaf">The last leaf index (included).</param>
    /// <returns>The AABB of the leaves.</returns>
    private static Aabb MergeLeaveAabbs<T>(List<IAabbTreeNode<T>> leaves, int firstLeaf, int lastLeaf)
    {
      Debug.Assert(leaves != null, "Array of leaves is empty.");
      Debug.Assert(firstLeaf < lastLeaf, "Invalid leaf indices.");

      Aabb aabb = leaves[firstLeaf].Aabb;

      // Compute union of all leaf AABBs.
      for (int i = firstLeaf + 1; i <= lastLeaf; i++)
        aabb.Grow(leaves[i].Aabb);

      return aabb;
    }


    /// <summary>
    /// Sorts the leaves such that objects of the left subtree come first.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the AABB tree.</typeparam>
    /// <param name="leaves">The leaves.</param>
    /// <param name="firstLeaf">The index of the first leaf.</param>
    /// <param name="lastLeaf">The index of the last leaf.</param>
    /// <param name="splitAxis">The index of the split axis.</param>
    /// <param name="splitValue">The split value.</param>
    /// <param name="rightLeaf">The index of the first leaf of the right subtree.</param>
    private static void SortLeaves<T>(List<IAabbTreeNode<T>> leaves, int firstLeaf, int lastLeaf, int splitAxis, float splitValue, out int rightLeaf)
    {
      int unhandledLeaf = firstLeaf; // Leaf index of first untested leaf.
      rightLeaf = lastLeaf + 1;

      // Go through leaves for this subtree and sort the indices such that
      // first are objects in the left half and then all objects in the right half.
      while (unhandledLeaf < rightLeaf)
      {
        if (leaves[unhandledLeaf].Aabb.Center[splitAxis] <= splitValue)
        {
          // Object of leaf is in left half. We can test the next.
          unhandledLeaf++;
        }
        else
        {
          // Object of leaf is in right half. Swap with a leaf at end.
          rightLeaf--;

          IAabbTreeNode<T> dummy = leaves[unhandledLeaf];
          leaves[unhandledLeaf] = leaves[rightLeaf];
          leaves[rightLeaf] = dummy;
        }
      }
    }
  }
}
