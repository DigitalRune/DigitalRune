// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Linq;


namespace DigitalRune.Geometry.Partitioning
{
  partial class AdaptiveAabbTree<T>
  {
    /// <summary>
    /// Gets or sets the threshold that determines when a bottom-up tree build method is used.
    /// </summary>
    /// <value>
    /// The threshold that determines when the tree is built using a bottom-up method. The default
    /// value is 128.
    /// </value>
    /// <remarks>
    /// <para>
    /// AABB trees can be built using top-down or bottom-up methods. Top-down methods are faster but
    /// less optimal. Bottom-up methods are slower but produce more balanced trees. 
    /// </para>
    /// <para>
    /// The <see cref="AdaptiveAabbTree{T}"/> uses a mixed approach: It starts with a top-down
    /// approach. When the number of nodes for an internal subtree is less than or equal to
    /// <see cref="BottomUpBuildThreshold"/> it uses a bottom-up method for the subtree.
    /// </para>
    /// <para>
    /// Increasing <see cref="BottomUpBuildThreshold"/> produces a better tree but (re)building the
    /// tree takes more time. Decreasing <see cref="BottomUpBuildThreshold"/> decreases the build
    /// time but produces less optimal trees.
    /// </para>
    /// <para>
    /// Changing <see cref="BottomUpBuildThreshold"/> does not change the tree structure 
    /// immediately. It takes effect the next time the tree is rebuilt.
    /// </para>
    /// </remarks>
    public int BottomUpBuildThreshold
    {
      get { return _bottomUpBuildThreshold; }
      set { _bottomUpBuildThreshold = value; }
    }
    private int _bottomUpBuildThreshold = 128;


    /// <summary>
    /// Builds the complete AABB tree.
    /// </summary>
    private void Build()
    {
      if (_root != null)
      {
        // Recycle old nodes.
        RemoveSubtree(_root, null);

        _root = null;
        _leaves.Clear();
      }

      int numberOfItems = Count;

      // No items?
      if (numberOfItems == 0)
      {
        // Nothing to do.
        return;
      }

      if (numberOfItems == 1)
      {
        // AABB tree contains exactly one item. (One leaf, no internal nodes.)
        T item = Items.First();
        _root = Nodes.Obtain();
        _root.Aabb = GetAabbForItem(item);
        _root.Item = item;
        _leaves.Add(_root);
      }
      else
      {
        // Default case: Several items. (Data is stored in the leaves.)
        foreach (T item in Items)
        {
          Node node = Nodes.Obtain();
          node.Aabb = GetAabbForItem(item);
          node.Item = item;
          _leaves.Add(node);
        }

        // (Fix for Xbox 360: Create a temporary list of leaves and pass the temporary list to the
        // AabbTreeBuilder. Cannot use the leaves array directly, because, the .NET CF has troubles
        // with arrays that are cast to IList<T>.)
        List<IAabbTreeNode<T>> leaves = DigitalRune.ResourcePools<IAabbTreeNode<T>>.Lists.Obtain();
        for (int i = 0; i < numberOfItems; i++)
          leaves.Add(_leaves[i]);

        // Build tree.
        _root = (Node)AabbTreeBuilder.Build(leaves, () => Nodes.Obtain(), BottomUpBuildThreshold);

        // Recycle temporary lists.
        DigitalRune.ResourcePools<IAabbTreeNode<T>>.Lists.Recycle(leaves);
      }
    }


    /// <summary>
    /// Performs a full refit of the specified subtree.
    /// </summary>
    /// <param name="node">The root node of the subtree.</param>
    private void FullRefit(Node node)
    {
      Debug.Assert(node != null, "Refit should not be called with node == null.");

      if (node.IsLeaf)
      {
        // Update leaf AABB.
        node.Aabb = GetAabbForItem(node.Item);
      }
      else
      {
        // Internal node.
        if (node.IsValid && node.IsActive && !node.LeftChild.IsActive)
        {
          Debug.Assert(!node.RightChild.IsActive, "When the left child is inactive the right child must also be inactive.");

          // Node was front node in the last collision detection query.
          // Invalidate node: 
          //   - Remove left and right subtrees
          //   - Gather all leaf nodes locally.
          InvalidateSubtree(node);
        }

        // Update AABBs.
        if (!node.IsValid)
        {
          // Leaf nodes are stored locally.
          Node leaf = node.Leaves[0];
          leaf.IsActive = false;
          leaf.Aabb = GetAabbForItem(leaf.Item);
          node.Aabb = leaf.Aabb;

          int numberOfLeaves = node.Leaves.Count;
          for (int index = 1; index < numberOfLeaves; index++)
          {
            leaf = node.Leaves[index];
            leaf.IsActive = false;
            leaf.Aabb = GetAabbForItem(leaf.Item);
            node.Aabb.Grow(leaf.Aabb);
          }
        }
        else
        {
          // Valid internal node. (Node was visited in the last collision detection query, but 
          // was not a front node.)
          FullRefit(node.LeftChild);
          FullRefit(node.RightChild);
          node.Aabb = Aabb.Merge(node.LeftChild.Aabb, node.RightChild.Aabb);

          // Check whether parent/children relationship is degenerate.
          node.IsDegenerate = IsDegenerate(node);
        }
      }

      node.IsActive = false;
    }


    /// <summary>
    /// Performs a partial refit of the current subtree.
    /// </summary>
    /// <param name="node">The root node of the subtree.</param>
    /// <param name="invalidItems">The set of invalid items.</param>
    /// <returns>
    /// <see langword="true"/> if the AABB of <paramref name="node"/> was updated; otherwise,
    /// <see langword="false"/> if the AABB has not changed.
    /// </returns>
    private bool PartialRefit(Node node, HashSet<T> invalidItems)
    {
      Debug.Assert(node != null);
      Debug.Assert(invalidItems != null);

      bool updated = false;
      if (node.IsLeaf)
      {
        // Update leaf AABB if necessary.
        if (invalidItems.Contains(node.Item))
        {
          node.Aabb = GetAabbForItem(node.Item);
          updated = true;
        }
      }
      else
      {
        // Refit children.
        if (!node.IsValid)
        {
          // Leaf nodes are stored locally.
          Node leaf = node.Leaves[0];
          leaf.IsActive = false;
          if (invalidItems.Contains(leaf.Item))
          {
            leaf.Aabb = GetAabbForItem(leaf.Item);
            updated = true;
          }

          node.Aabb = leaf.Aabb;

          int numberOfLeaves = node.Leaves.Count;
          for (int index = 1; index < numberOfLeaves; index++)
          {
            leaf = node.Leaves[index];
            leaf.IsActive = false;
            if (invalidItems.Contains(leaf.Item))
            {
              leaf.Aabb = GetAabbForItem(leaf.Item);
              updated = true;
            }

            node.Aabb.Grow(leaf.Aabb);
          }
        }
        else
        {
          // Valid internal node.
          bool leftUpdated = PartialRefit(node.LeftChild, invalidItems);
          bool rightUpdated = PartialRefit(node.RightChild, invalidItems);
          updated = leftUpdated || rightUpdated;
          if (updated)
          {
            // Update internal AABB.
            node.Aabb = Aabb.Merge(node.LeftChild.Aabb, node.RightChild.Aabb);

            // Check whether parent/children relationship is degenerate.
            node.IsDegenerate = IsDegenerate(node);
          }
        }
      }

      node.IsActive = false;
      return updated;
    }


    /// <summary>
    /// Invalidates the specified subtree.
    /// </summary>
    /// <param name="node">The root node of the subtree to be invalidated.</param>
    /// <remarks>
    /// This methods removes the left and right subtrees of the current node and stores all leaf 
    /// nodes locally.
    /// </remarks>
    private static void InvalidateSubtree(Node node)
    {
      if (!node.IsValid)
        return;

      // Fetch all leaf nodes and remove left and right subtrees.
      node.Leaves = DigitalRune.ResourcePools<Node>.Lists.Obtain();
      RemoveSubtree(node.LeftChild, node.Leaves);
      RemoveSubtree(node.RightChild, node.Leaves);
      node.LeftChild = null;
      node.RightChild = null;
      node.IsDegenerate = true;
    }


    /// <summary>
    /// Removes the current subtree and fetches the leaf nodes.
    /// </summary>
    /// <param name="node">The root node of the subtree.</param>
    /// <param name="leaves">A list where the leaf nodes will be added.</param>
    /// <remarks>
    /// The removed leaves are stored in an internal node. They are not completely
    /// removed from the spatial partition.
    /// </remarks>
    private static void RemoveSubtree(Node node, List<Node> leaves)
    {
      if (node.IsLeaf)
      {
        if (leaves != null)
          leaves.Add(node);
      }
      else
      {
        if (!node.IsValid)
        {
          // Current node is already invalid and has no subtree.
          if (leaves != null)
          {
            int numberOfLeaves = node.Leaves.Count;
            for (int i = 0; i < numberOfLeaves; i++)
              leaves.Add(node.Leaves[i]);
          }
        }
        else
        {
          // Recursively fetch leaf nodes and remove left and right subtrees.
          RemoveSubtree(node.LeftChild, leaves);
          RemoveSubtree(node.RightChild, leaves);
        }

        Nodes.Recycle(node);
      }
    }


    /// <summary>
    /// Checks whether the parent/children relationship of the specified node should be considered 
    /// as degenerate and the node should be re-split in the next collision detection query.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>
    /// <see langword="true"/> if the parent/children relationship of <paramref name="node"/> is 
    /// degenerate; otherwise, <see langword="false"/>.
    /// </returns>
    private static bool IsDegenerate(Node node)
    {
      Debug.Assert(node.IsValid, "Cannot check node. Node is invalid.");
      Debug.Assert(!node.IsLeaf, "Cannot check node. Node is a leaf node.");

      if (node.LeftChild.IsLeaf && node.RightChild.IsLeaf)
      {
        // A parent node containing two leaf nodes does not need to be re-split.
        return false;
      }

      float volumeParent = node.Aabb.Volume;
      float volumeLeftChild = node.LeftChild.Aabb.Volume;
      float volumeRightChild = node.RightChild.Aabb.Volume;

      const float resplitThreshold = 0.9f;  // This value is recommended by Larsson et al.
      // The value is confirmed by our test cases:
      // Other values such as 0.8 or 1.0 perform worse.

      return volumeParent / (volumeLeftChild + volumeRightChild) < resplitThreshold;
    }


    /// <summary>
    /// Adds the specified item to the AABB tree.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    private void AddLeaf(T item)
    {
      Node leaf = Nodes.Obtain();
      leaf.Aabb = GetAabbForItem(item);
      leaf.Item = item;
      _leaves.Add(leaf);

      if (_root == null)
      {
        // Leaf node is the new root.
        _root = leaf;
      }
      else if (_root.IsLeaf)
      {
        // Split at root node.
        Node sibling = _root;

        _root = Nodes.Obtain();
        _root.LeftChild = sibling;
        _root.RightChild = leaf;
      }
      else
      {
        // Recursively add leaf node to tree.
        AddLeaf(_root, leaf);
      }
    }


    /// <summary>
    /// Adds the leaf node to the specified subtree.
    /// </summary>
    /// <param name="node">The root of the subtree.</param>
    /// <param name="leaf">The leaf node.</param>
    private void AddLeaf(Node node, Node leaf)
    {
      Debug.Assert(!node.IsLeaf);

      if (node.IsValid)
      {
        // Choose closest node among the children.
        int selection = AabbTreeHelper.SelectClosest(leaf.Aabb, node.LeftChild.Aabb, node.RightChild.Aabb);
        if (selection == 0)
        {
          // Insert into left subtree.
          if (node.LeftChild.IsLeaf)
          {
            // Invalidate the node above the leaf node. Insert the leaf node (see below) and 
            // re-split in next collision detection query as needed.
            InvalidateSubtree(node);
          }
          else
          {
            // Recursively add leaf node to subtree.
            AddLeaf(node.LeftChild, leaf);
          }
        }
        else
        {
          // Insert into right subtree.
          if (node.RightChild.IsLeaf)
          {
            // Invalidate the node above the leaf node. Insert the leaf node (see below) and 
            // re-split in next collision detection query as needed.
            InvalidateSubtree(node);
          }
          else
          {
            // Recursively add leaf node to subtree.
            AddLeaf(node.RightChild, leaf);
          }
        }
      }

      if (!node.IsValid)
      {
        // All nodes are stored locally.
        node.Leaves.Add(leaf);
      }

      RecomputeAabb(node);
    }


    /// <summary>
    /// Removes the specified item from the AABB tree.
    /// </summary>
    /// <param name="item">The item to be removed.</param>
    private void RemoveLeaf(T item)
    {
      Debug.Assert(_root != null);

      if (_leaves.Count == 1)
      {
        // Tree consists of only 1 leaf node.
        Debug.Assert(_root.IsLeaf);
        Debug.Assert(_root.IsValid);
        Debug.Assert(Comparer.Equals(_root.Item, item));

        _leaves.RemoveAt(0);
        Nodes.Recycle(_root);
        _root = null;
      }
      else
      {
        // Tree consists of ≥2 leaf nodes.
        RemoveLeaf(_root, item);

        // Check whether root is still valid.
        if (!_root.IsValid && _root.Leaves.Count == 1)
        {
          // The remaining leaf node is the new root of the tree.
          Node newRoot = _root.Leaves[0];
          Nodes.Recycle(_root);
          _root = newRoot;
        }
      }
    }


    /// <summary>
    /// Recursively removes the specified item from the subtree.
    /// </summary>
    /// <param name="node">The root of the tree or subtree.</param>
    /// <param name="item">The item to be removed.</param>
    /// <returns>
    /// <see langword="true"/> if the item was found and removed from the subtree; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    private bool RemoveLeaf(Node node, T item)
    {
      Debug.Assert(!node.IsLeaf);

      bool found = false;

      if (node.IsValid)
      {
        // ----- Check left subtree.
        if (node.LeftChild.IsLeaf)
        {
          // The left child is a leaf node.
          if (Comparer.Equals(node.LeftChild.Item, item))
          {
            // Invalidate the current node. (Node needs to be re-split.)
            InvalidateSubtree(node);

            // Remove item from list of leaf nodes.
            found = RemoveLeaf(node.Leaves, item);
            Debug.Assert(found);
          }
        }
        else
        {
          // Recursively check the left subtree.
          found = RemoveLeaf(node.LeftChild, item);

          // Check whether the left child is still valid.
          if (found && !node.LeftChild.IsValid && node.LeftChild.Leaves.Count == 1)
          {
            // The left child node has become invalid.
            InvalidateSubtree(node);
          }
        }

        // ----- Check right subtree.
        if (!found)
        {
          if (node.RightChild.IsLeaf)
          {
            // The right child is a leaf node.
            if (Comparer.Equals(node.RightChild.Item, item))
            {
              // Invalidate the current node. (Node needs to be re-split.)
              InvalidateSubtree(node);

              // Remove item from list of leaf nodes.
              found = RemoveLeaf(node.Leaves, item);
              Debug.Assert(found);
            }
          }
          else
          {
            // Recursively check the right subtree.
            found = RemoveLeaf(node.RightChild, item);

            // Check whether the right child is still valid.
            if (found && !node.RightChild.IsValid && node.RightChild.Leaves.Count == 1)
            {
              // The right child node has become invalid.
              InvalidateSubtree(node);
            }
          }
        }
      }
      else
      {
        // Current node is invalid.
        found = RemoveLeaf(node.Leaves, item);
      }

      if (found)
        RecomputeAabb(node);

      return found;
    }


    /// <summary>
    /// Removes specified item from the collection of leaf nodes.
    /// </summary>
    /// <param name="leaves">The collection of leaf nodes.</param>
    /// <param name="item">The item to be removed.</param>
    /// <returns>
    /// <see langword="true"/> if the item was found and removed from the subtree; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    private bool RemoveLeaf(List<Node> leaves, T item)
    {
      bool found = false;
      Node leaf = null;
      int index;
      int numberOfLeaves = leaves.Count;
      for (index = 0; index < numberOfLeaves; index++)
      {
        leaf = leaves[index];
        if (Comparer.Equals(leaf.Item, item))
        {
          found = true;
          break;
        }
      }

      if (found)
      {
        leaves.RemoveAt(index);
        _leaves.Remove(leaf);
      }

      return found;
    }


    /// <summary>
    /// Recomputes the AABB of the specified node. (All leaf nodes need to be up-to-date.)
    /// </summary>
    /// <param name="node">The node.</param>
    private static void RecomputeAabb(Node node)
    {
      Debug.Assert(!node.IsLeaf);

      if (node.IsValid)
      {
        node.Aabb = Aabb.Merge(node.LeftChild.Aabb, node.RightChild.Aabb);
      }
      else
      {
        node.Aabb = node.Leaves[0].Aabb;
        int numberOfLeaves = node.Leaves.Count;
        for (int i = 1; i < numberOfLeaves; i++)
          node.Aabb.Grow(node.Leaves[i].Aabb);
      }
    }


    /// <summary>
    /// Splits the specified node if necessary.
    /// </summary>
    /// <param name="node">The node to be split.</param>
    private static void SplitIfNecessary(Node node)
    {
      if (node.IsValid && !node.IsDegenerate)
        return;

      // If only node.IsDegenerate is set, there is still a valid subtree. 
      // We need to remove the subtree first.
      InvalidateSubtree(node);

      int numberOfLeaves = node.Leaves.Count; // Number of leaf nodes.
      int leftleaves;                         // Number of leaf nodes in left sub-volume.
      int rightLeaves;                        // Number of leaf nodes in right sub-volume.
      int rightLeaf;                          // Start index of leaf nodes in the right sub-volume.

      if (numberOfLeaves == 2)
      {
        // Optimization: No splitting necessary if there are only 2 leaf nodes.
        leftleaves = 1;
        rightLeaves = 1;
        rightLeaf = 1;
      }
      else
      {
        // Get max axis.
        int splitAxis = node.Aabb.Extent.IndexOfLargestComponent;

        // Split at center of AABB.
        float splitValue = node.Aabb.Center[splitAxis];

        // Sort leaves according to split-plane.
        SortLeaves(node.Leaves, splitAxis, splitValue, out rightLeaf);

        // If more than 80% of the leaf nodes are in one subtree we perform a re-split.
        leftleaves = rightLeaf - 0;
        rightLeaves = numberOfLeaves - rightLeaf;
        float subtreeThreshold = 0.8f * numberOfLeaves;
        if (leftleaves > subtreeThreshold || rightLeaves > subtreeThreshold)
        {
          // Re-split
          if (leftleaves > rightLeaves)
          {
            // Move split-plane to midpoint of left sub-volume.
            splitValue = (node.Aabb.Minimum[splitAxis] + splitValue) / 2;
          }
          else
          {
            // Move split-plane to midpoint of right sub-volume.
            splitValue = (node.Aabb.Maximum[splitAxis] + splitValue) / 2;
          }

          // Sort again.
          SortLeaves(node.Leaves, splitAxis, splitValue, out rightLeaf);

          // Check if re-split has worked.
          leftleaves = rightLeaf - 0;
          rightLeaves = numberOfLeaves - rightLeaf;
          if (leftleaves > subtreeThreshold || rightLeaves > subtreeThreshold)
          {
            // Fallback: Create two equal trees.
            rightLeaf = numberOfLeaves / 2;
          }

          leftleaves = rightLeaf - 0;
          rightLeaves = numberOfLeaves - rightLeaf;
        }
      }

      if (leftleaves == 1)
      {
        // Left child is leaf.
        node.LeftChild = node.Leaves[0];
      }
      else
      {
        // Left child is a subtree.
        Node child = Nodes.Obtain();
        child.Leaves = DigitalRune.ResourcePools<Node>.Lists.Obtain();
        Node leaf = node.Leaves[0];
        child.Leaves.Add(leaf);
        child.Aabb = leaf.Aabb;
        for (int index = 1; index < rightLeaf; index++)
        {
          leaf = node.Leaves[index];
          child.Leaves.Add(leaf);
          child.Aabb.Grow(leaf.Aabb);
        }

        child.IsDegenerate = true;
        node.LeftChild = child;
      }

      if (rightLeaves == 1)
      {
        // Right child is leaf.
        node.RightChild = node.Leaves[rightLeaf];
      }
      else
      {
        // Right child is a subtree.
        Node child = Nodes.Obtain();
        child.Leaves = DigitalRune.ResourcePools<Node>.Lists.Obtain();
        Node leaf = node.Leaves[rightLeaf];
        child.Leaves.Add(leaf);
        child.Aabb = leaf.Aabb;
        for (int index = rightLeaf + 1; index < numberOfLeaves; index++)
        {
          leaf = node.Leaves[index];
          child.Leaves.Add(leaf);
          child.Aabb.Grow(leaf.Aabb);
        }

        child.IsDegenerate = true;
        node.RightChild = child;
      }

      DigitalRune.ResourcePools<Node>.Lists.Recycle(node.Leaves);
      node.Leaves = null;
      node.IsDegenerate = false;
    }


    /// <summary>
    /// Sorts the leaf nodes in the given list.
    /// </summary>
    /// <param name="leaves">The leaves.</param>
    /// <param name="splitAxis">The index of the split axis.</param>
    /// <param name="splitValue">The split value.</param>
    /// <param name="rightLeaf">The index of the first leaf in the right sub-volume.</param>
    private static void SortLeaves(List<Node> leaves, int splitAxis, float splitValue, out int rightLeaf)
    {
      int unhandledLeaf = 0;      // Leaf index of first untested leaf.
      rightLeaf = leaves.Count;   // Leaf index where the right tree begins.

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

          Node dummy = leaves[unhandledLeaf];
          leaves[unhandledLeaf] = leaves[rightLeaf];
          leaves[rightLeaf] = dummy;
        }
      }
    }
  }
}
