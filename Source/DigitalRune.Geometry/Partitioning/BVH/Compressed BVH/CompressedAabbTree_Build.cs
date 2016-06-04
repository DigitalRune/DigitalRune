// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Partitioning
{
  partial class CompressedAabbTree
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
    /// The <see cref="CompressedAabbTree"/> uses a mixed approach: It starts with a top-down
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
    /// <exception cref="GeometryException">
    /// Cannot build AABB tree. The property <see cref="GetAabbForItem"/> of the spatial partition
    /// is not set.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void Build()
    {
      Debug.Assert(_items != null && _items.Count > 0, "Build should not be called for empty CompressedAabbTree.");
      if (GetAabbForItem == null)
        throw new GeometryException("Cannot build AABB tree. The property GetAabbForItem of the spatial partition is not set.");

      if (_items.Count == 1)
      {
        // AABB tree contains exactly one item. (One leaf, no internal nodes.)
        int item = _items[0];

        // Determine AABB of spatial partition and prepare factors for quantization.
        Aabb aabb = GetAabbForItem(item);
        SetQuantizationValues(aabb);

        // Create node.
        Node node = new Node();
        node.Item = _items[0];
        SetAabb(ref node, _aabb);

        _nodes = new[] { node };
        _numberOfItems = 1;
      }
      else
      {
        // Default case: Several items. (Data is stored in the leaves.)
        // First create a normal AabbTree<int> which is then compressed.
        _numberOfItems = _items.Count;

        List<IAabbTreeNode<int>> leaves = DigitalRune.ResourcePools<IAabbTreeNode<int>>.Lists.Obtain();
        for (int i = 0; i < _numberOfItems; i++)
        {
          int item = _items[i];
          Aabb aabb = GetAabbForItem(item);
          leaves.Add(new AabbTree<int>.Node { Aabb = aabb, Item = item });
        }

        // Build tree.
        AabbTree<int>.Node root = (AabbTree<int>.Node)AabbTreeBuilder.Build(leaves, () => new AabbTree<int>.Node(), BottomUpBuildThreshold);

        // Set AABB of spatial partition and prepare the factors for quantization.
        SetQuantizationValues(root.Aabb);

        // Compress AABB tree.
        var nodes = DigitalRune.ResourcePools<Node>.Lists.Obtain();
        CompressTree(nodes, root);
        _nodes = nodes.ToArray();

        // Recycle temporary lists.
        DigitalRune.ResourcePools<IAabbTreeNode<int>>.Lists.Recycle(leaves);
        DigitalRune.ResourcePools<Node>.Lists.Recycle(nodes);
      }

      // Recycle items list, now that we have a valid tree.
      DigitalRune.ResourcePools<int>.Lists.Recycle(_items);
      _items = null;
    }


    /// <summary>
    /// Compresses an AABB tree.
    /// </summary>
    /// <param name="compressedNodes">The list of compressed AABB nodes.</param>
    /// <param name="uncompressedNode">The root of the uncompressed AABB tree.</param>
    private void CompressTree(List<Node> compressedNodes, AabbTree<int>.Node uncompressedNode)
    {
      if (uncompressedNode.IsLeaf)
      {
        // Compress leaf node.
        Node node = new Node();
        node.Item = uncompressedNode.Item;
        SetAabb(ref node, uncompressedNode.Aabb);
        compressedNodes.Add(node);
      }
      else
      {
        // Node is internal node.
        int currentIndex = compressedNodes.Count;
        Node node = new Node();
        SetAabb(ref node, uncompressedNode.Aabb);
        compressedNodes.Add(node);

        // Compress child nodes.
        CompressTree(compressedNodes, uncompressedNode.LeftChild);
        CompressTree(compressedNodes, uncompressedNode.RightChild);

        // Set escape offset. (Escape offset = size of subtree)
        node.EscapeOffset = compressedNodes.Count - currentIndex;
        compressedNodes[currentIndex] = node;
      }
    }


    private void Refit()
    {
      // Compute new unquantized AABBs.
      Aabb[] buffer = new Aabb[_nodes.Length];
      int count = 0;
      ComputeAabbs(buffer, 0, ref count);

      // Update AABB of spatial partition and prepare the factors for quantization.
      SetQuantizationValues(buffer[0]);

      // Update compressed AABBs.
      for (int i = 0; i < _nodes.Length; i++)
      {
        Node node = _nodes[i];
        SetAabb(ref node, buffer[i]);
        _nodes[i] = node;
      }
    }


    private void ComputeAabbs(Aabb[] buffer, int index, ref int count)
    {
      // Increment the counter for each node visited.
      count++;

      Node node = _nodes[index];
      if (node.IsLeaf)
      {
        // Store unquantized AABB of leaf node.
        buffer[index] = GetAabbForItem(node.Item);
      }
      else
      {
        // Compute AABB of child nodes.
        int leftIndex = index + 1;
        ComputeAabbs(buffer, leftIndex, ref count);

        int rightIndex = count;
        ComputeAabbs(buffer, rightIndex, ref count);
        buffer[index] = Aabb.Merge(buffer[leftIndex], buffer[rightIndex]);
      }
    }
  }
}
