// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Linq;


namespace DigitalRune.Geometry.Partitioning
{
  partial class AabbTree<T>
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
    /// The <see cref="AabbTree{T}"/> uses a mixed approach: It starts with a top-down approach.
    /// When the number of nodes for an internal subtree is less than or equal to
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
      _root = null;
      _leaves = null;
      _height = -1;

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
        _root = new Node
        {
          Aabb = GetAabbForItem(item),
          Item = item,
        };
        _leaves = new[] { _root };
        _height = 0;
      }
      else
      {
        // Default case: Several items. (Data is stored in the leaves.)

        // (Fix for Xbox 360: Create a temporary list of leaves and pass the temporary list to the
        // AabbTreeBuilder. Cannot use the leaves array directly, because, the .NET CF has troubles
        // with arrays that are cast to IList<T>.)
        var leaves = DigitalRune.ResourcePools<IAabbTreeNode<T>>.Lists.Obtain();

        foreach (T item in Items)
        {
          Aabb aabb = GetAabbForItem(item);
          leaves.Add(new Node { Aabb = aabb, Item = item });
        }

        // Build tree.
        _root = (Node)AabbTreeBuilder.Build(leaves, () => new Node(), BottomUpBuildThreshold);

        //_root = CompactNodes(_root, null);
        //GC.Collect(0);

        // Copy leaves from temporary list.
        _leaves = new Node[numberOfItems];
        for (int i = 0; i < numberOfItems; i++)
          _leaves[i] = (Node)leaves[i];

        _height = GetHeight(_root);

        // Recycle temporary list.
        DigitalRune.ResourcePools<IAabbTreeNode<T>>.Lists.Recycle(leaves);
      }
    }


    private Node CompactNodes(Node node, Node parent)
    {
      if (node == null)
        return null;

      var newNode = new Node
      {
        Aabb = node.Aabb,
        Item = node.Item,
        Parent = parent
      };

      newNode.LeftChild = CompactNodes(node.LeftChild, newNode);
      newNode.RightChild = CompactNodes(node.RightChild, newNode);
      return newNode;
    }


    private int GetHeight(Node node)
    {
      if (node == null || node.IsLeaf)   // The height of a leaf is 0 (not 1).
        return 0;

      return Math.Max(GetHeight(node.LeftChild), GetHeight(node.RightChild)) + 1;
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
          // Update inner AABB.
          Aabb aabb = node.LeftChild.Aabb;
          aabb.Grow(node.RightChild.Aabb);
          node.Aabb = aabb;
        }
      }

      return updated;
    }
  }
}
