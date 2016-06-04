// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Linq;


namespace DigitalRune.Geometry.Partitioning
{
  partial class DynamicAabbTree<T>
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
    /// The <see cref="DynamicAabbTree{T}"/> uses a mixed approach: It starts with a top-down
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
    /// Builds the AABB tree.
    /// </summary>
    private void Build()
    {
      if (_root != null)
      {
        // Recycle old nodes.
        RecycleNodes(_root);

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
        _leaves.Add(item, _root);
      }
      else
      {
        // Default case: Several items. (Data is stored in the leaves.)
        foreach (T item in Items)
        {
          Node node = Nodes.Obtain();
          node.Aabb = GetAabbForItem(item);
          node.Item = item;
          _leaves.Add(item, node);
        }

        // (Fix for Xbox 360: Create a temporary list of leaves and pass the temporary list to the
        // AabbTreeBuilder. Cannot use the leaves array directly, because, the .NET CF has troubles
        // with arrays that are cast to IList<T>.)
        List<IAabbTreeNode<T>> leaves = DigitalRune.ResourcePools<IAabbTreeNode<T>>.Lists.Obtain();
        foreach (var leaf in _leaves.Values)
          leaves.Add(leaf);

        // Build tree.
        _root = (Node)AabbTreeBuilder.Build(leaves, () => Nodes.Obtain(), BottomUpBuildThreshold);

        // Recycle temporary lists.
        DigitalRune.ResourcePools<IAabbTreeNode<T>>.Lists.Recycle(leaves);
      }
    }


    private static void RecycleNodes(Node node)
    {
      if (node != null)
      {
        RecycleNodes(node.RightChild);
        RecycleNodes(node.LeftChild);
        Nodes.Recycle(node);
      }
    }


    /// <summary>
    /// Refits the subtree.
    /// </summary>
    /// <param name="node">The subtree root.</param>
    /// <param name="invalidItems">The items that need to be updated.</param>
    /// <returns>
    /// <see langword="true"/> if the AABB of <paramref name="node"/> was updated; otherwise,
    /// <see langword="false"/> if the AABB has not changed.
    /// </returns>
    private bool Refit(Node node, HashSet<T> invalidItems)
    {
      Debug.Assert(node != null, "Refit should not be called with node == null.");

      bool updated = false;
      if (node.IsLeaf)
      {
        // Update leaf AABB if necessary.
        if (invalidItems == null || invalidItems.Contains(node.Item))
        {
          node.Aabb = GetAabbForItem(node.Item);
          updated = true;
        }
      }
      else
      {
        // Refit children.
        bool leftUpdated = Refit(node.LeftChild, invalidItems);
        bool rightUpdated = Refit(node.RightChild, invalidItems);
        updated = leftUpdated || rightUpdated;
        if (updated)
        {
          // Update internal AABB.
          Aabb aabb = node.LeftChild.Aabb;
          aabb.Grow(node.RightChild.Aabb);
          node.Aabb = aabb;
        }
      }

      return updated;
    }


    /// <summary>
    /// Adds a leaf node to the given tree.
    /// </summary>
    /// <param name="root">The root of the tree (or subtree).</param>
    /// <param name="leaf">The leaf node to be added.</param>
    private void AddLeaf(Node root, Node leaf)
    {
      if (_root == null)
      {
        // Insert leaf as new root.
        _root = leaf;
      }
      else
      {
        // Search for leaf node that is closest to 'leaf'. This node that will become the new 
        // sibling of 'leaf'.
        Node sibling = root;
        while (!sibling.IsLeaf)
        {
          int selection = AabbTreeHelper.SelectClosest(leaf.Aabb, sibling.LeftChild.Aabb, sibling.RightChild.Aabb);
          sibling = (selection == 0) ? sibling.LeftChild : sibling.RightChild;
        }

        // Add a new node as the parent of (sibling, leaf).
        Node parent = sibling.Parent;
        Node node = Nodes.Obtain();
        node.Aabb = Aabb.Merge(sibling.Aabb, leaf.Aabb);
        node.Parent = parent;
        node.LeftChild = sibling;
        node.RightChild = leaf;
        sibling.Parent = node;
        leaf.Parent = node;

        if (parent == null)
        {
          // node is the new root.
          _root = node;
        }
        else
        {
          // node is an internal node.
          if (parent.LeftChild == sibling)
            parent.LeftChild = node;
          else
            parent.RightChild = node;

          // Update AABBs of ancestor.
          do
          {
            if (!parent.Aabb.Contains(node.Aabb))
              parent.Aabb = Aabb.Merge(parent.LeftChild.Aabb, parent.RightChild.Aabb);
            else
              break;

            node = parent;
            parent = parent.Parent;
          } while (parent != null);
        }
      }
    }


    /// <summary>
    /// Removes the specified leaf node from the tree.
    /// </summary>
    /// <param name="leaf">The leaf node .</param>
    /// <returns>
    /// The closest ancestor of <paramref name="leaf"/> that was not resized during remove.
    /// </returns>
    private Node RemoveLeaf(Node leaf)
    {
      if (_root == leaf)
      {
        // Remove only node of tree.
        _root = null;
        return null;
      }
      else
      {
        Node parent = leaf.Parent;
        Node previous = parent.Parent;
        Node sibling = (parent.LeftChild == leaf) ? parent.RightChild : parent.LeftChild;
        if (previous == null)
        {
          // The sibling becomes the new root of the tree.
          _root = sibling;
          sibling.Parent = null;
          Nodes.Recycle(parent);
          return _root;
        }
        else
        {
          // Replace parent by sibling.
          if (previous.LeftChild == parent)
            previous.LeftChild = sibling;
          else
            previous.RightChild = sibling;

          sibling.Parent = previous;
          Nodes.Recycle(parent);

          // Update AABBs of ancestors.
          do
          {
            Aabb oldAabb = previous.Aabb;
            previous.Aabb = Aabb.Merge(previous.LeftChild.Aabb, previous.RightChild.Aabb);
            if (oldAabb != previous.Aabb)
              previous = previous.Parent;
            else
              break;

          } while (previous != null);

          return previous ?? _root;
        }
      }
    }


    /// <summary>
    /// Optimizes the incrementally.
    /// </summary>
    /// <param name="numberOfPasses">
    /// The number of passes. In each pass one leaf node is updated.
    /// </param>
    private void OptimizeIncrementally(int numberOfPasses)
    {
      // Note: Bullet additional keeps all nodes in a contiguous array and sorts the nodes such that
      // the parent nodes lie before the child nodes to optimize for cache-locality. (We would have 
      // to convert nodes to structs or use preallocated arrays.)

      if (_root == null)
        return;

      if (numberOfPasses < 0)
        numberOfPasses = Count;

      for (int i = 0; i < numberOfPasses; i++)
      {
        // Find leaf node which should be optimized next. The _optimizationPath identifies the path 
        // from root to the next leaf node. Each bit selects the direction to take at each level of
        // the tree (0 == left, 1 == right).
        Node node = _root;
        int bit = 0;
        while (!node.IsLeaf)
        {
          if (((_optimizationPath >> bit) & 1) == 0)
            node = node.LeftChild;
          else
            node = node.RightChild;

          bit = (bit + 1) & 31; // (bit + 1) % 32;
        }

        UpdateLeaf(node);
        unchecked { _optimizationPath++; }
      }
    }


    /// <summary>
    /// Updates the specified node.
    /// </summary>
    /// <param name="node">The node to be updated.</param>
    private void UpdateLeaf(Node node)
    {
      // Temporarily remove node from tree.
      Node root = RemoveLeaf(node);

      // Re-insert node at closest ancestor that was not resized during remove.
      AddLeaf(root, node);
    }


    /// <summary>
    /// Updates the node and sets the specified AABB.
    /// </summary>
    /// <param name="node">The node to be updated.</param>
    /// <param name="newAabb">The new AABB of <paramref name="node"/>.</param>
    /// <remarks>
    /// <para>
    /// Motion prediction is used to reduce the number of tree updates: If motion prediction is 
    /// enabled, the AABB of the node is increased artificially to hopefully include future 
    /// positions of the item. If a future AABB is contained in the current AABB the tree is left 
    /// unchanged. See check in <see cref="Invalidate"/>.
    /// </para>
    /// </remarks>
    private void UpdateLeaf(Node node, Aabb newAabb)
    {
      if (_enableMotionPrediction && GeometryHelper.HaveContact(node.Aabb, newAabb))
      {
        // Old AABB overlaps with new AABB. 
        // Let's assume a linear motion.

        // Expand AABB by margin and along motion.
        Vector3F velocity = (newAabb.Center - node.Aabb.Center) * MotionPrediction;
        Expand(ref newAabb, RelativeMargin);  // Add margin to account for jiggling.
        Expand(ref newAabb, ref velocity);    // Expand in direction to account for linear motion.

        // Note: Bullet uses 
        //
        //   Vector3F delta = newAabb.Minimum - node.Aabb.Minimum;
        //
        // to estimate the direction (positive x or negative x, ...).
        // Then Bullet calculates the velocity as 
        //
        //   Vector3F velocity = newAabb.Extent / 2 * MotionPrediction;
        //   if (delta.X < 0) velocity.X = -velocity.X;
        //   if (delta.Y < 0) velocity.Y = -velocity.Y;
        //   if (delta.Z < 0) velocity.Z = -velocity.Z;
        //
        // Tests have shown that our method, besides being more understandable, also yields a much
        // better performance in typical scenarios where objects move continuously. Our methods 
        // creates smaller AABBs thereby yields fewer overlaps and reduces the work in the narrow
        // phase.
      }
      else
      {
        // Teleporting (or motion prediction disabled).
        // Do not change the AABB.
      }

      node.Aabb = newAabb;
      UpdateLeaf(node);
    }


    /// <summary>
    /// Expands the specified AABB in the given direction.
    /// </summary>
    /// <param name="aabb">The AABB to be expanded.</param>
    /// <param name="direction">The direction.</param>
    private static void Expand(ref Aabb aabb, ref Vector3F direction)
    {
      if (direction.X >= 0)
        aabb.Maximum.X += direction.X;
      else
        aabb.Minimum.X += direction.X;

      if (direction.Y >= 0)
        aabb.Maximum.Y += direction.Y;
      else
        aabb.Minimum.Y += direction.Y;

      if (direction.Z >= 0)
        aabb.Maximum.Z += direction.Z;
      else
        aabb.Minimum.Z += direction.Z;
    }


    /// <summary>
    /// Expands the specified AABB by a margin.
    /// </summary>
    /// <param name="aabb">The AABB to be expanded.</param>
    /// <param name="margin">The relative margin.</param>
    private static void Expand(ref Aabb aabb, float margin)
    {
      Vector3F delta = new Vector3F(aabb.Extent.LargestComponent * margin);
      aabb.Minimum -= delta;
      aabb.Maximum += delta;
    }
  }
}
