// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Represents a binary node of an AABB tree.
  /// </summary>
  /// <typeparam name="T">The type of item stored in the tree.</typeparam>
  internal interface IAabbTreeNode<T>
  {
    /// <summary>
    /// Gets or sets the AABB of this node which contains the current subtree.
    /// </summary>
    /// <value>The AABB of this node which contains the current subtree.</value>
    Aabb Aabb { get; set; }


    /// <summary>
    /// Gets or sets the left child node.
    /// </summary>
    /// <value>The left child node. (Or <see langword="null"/> if the node is a leaf node.)</value>
    IAabbTreeNode<T> LeftChild { get; set; }


    /// <summary>
    /// Gets or sets the right child node.
    /// </summary>
    /// <value>The right child node. (Or <see langword="null"/> if the node is a leaf node.)</value>
    IAabbTreeNode<T> RightChild { get; set; }


    /// <summary>
    /// Gets the parent node.
    /// </summary>
    /// <value>The parent node. (Or <see langword="null"/> if the node is a leaf node.)</value>
    IAabbTreeNode<T> Parent { get; }


    /// <summary>
    /// Gets a value indicating whether this instance is a leaf node.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is a leaf; otherwise, <see langword="false"/> if it
    /// is an internal node.
    /// </value>
    bool IsLeaf { get; }


    /// <summary>
    /// Gets the data held in this node.
    /// </summary>
    /// <value>The data of this node.</value>
    T Item { get; }
  }
}
