// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Partitioning
{
  partial class AdaptiveAabbTree<T>
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Represents a node of an <see cref="AdaptiveAabbTree{T}"/>.
    /// </summary>
    /// <remarks>
    /// A node is also the root of a subtree. Each node can be either a leaf or an inner node.
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// If the node is a leaf <see cref="LeftChild"/> and <see cref="RightChild"/> are 
    /// <see langword="null"/> and the node contains an <see cref="Item"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// If the node is an inner node then <see cref="LeftChild"/> and <see cref="RightChild"/> are 
    /// not <see langword="null"/> and the node does not contain an <see cref="Item"/>.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    private sealed class Node : IAabbTreeNode<T>
    {
      /// <summary>
      /// The AABB of this node which contains the current subtree.
      /// </summary>
      public Aabb Aabb;


      /// <summary>
      /// The left child or <see langword="null"/> if the node is a leaf node.
      /// </summary>
      public Node LeftChild;


      /// <summary>
      /// The right child or <see langword="null"/> if the node is a leaf node.
      /// </summary>
      public Node RightChild;


      /// <summary>
      /// The data object.
      /// </summary>
      /// <remarks>
      /// Often this data is an integer index in a collection with the real data objects.
      /// </remarks>
      public T Item;


      /// <summary>
      /// The leaves of the current subtree. (Set if the subtree has been invalidated.)
      /// </summary>
      public List<Node> Leaves;


      /// <summary>
      /// <see langword="true"/> if the current node was visited in the collision detection queries
      /// since the last update; otherwise, <see langword="false"/>.
      /// </summary>
      public bool IsActive;


      /// <summary>
      /// <see langword="true"/> if the parent/child relationship of the current node is degenerate 
      /// and the node should be re-split; otherwise, <see langword="false"/>.
      /// </summary>
      public bool IsDegenerate;


      /// <summary>
      /// Gets a value indicating whether this instance is a leaf node.
      /// </summary>
      /// <value>
      /// <see langword="true"/> if this instance is a leaf node; otherwise, <see langword="false"/>
      /// if it is an internal node.
      /// </value>
      public bool IsLeaf
      {
        get { return Leaves == null && LeftChild == null && RightChild == null; }
      }


      /// <summary>
      /// Gets a value indicating whether this <see cref="AdaptiveAabbTree{T}.Node"/> is valid.
      /// </summary>
      /// <value>
      /// <see langword="true"/> if valid; otherwise, <see langword="false"/>.
      /// </value>
      /// <remarks>
      /// A node of an adaptive AABB tree is invalid if it is an non-leaf node that has items
      /// associated with it, but has no appropriate subtree yet.
      /// </remarks>
      public bool IsValid
      {
        get { return Leaves == null; }
      }


      #region ----- IAabbTreeNode<T> -----

      /// <summary>
      /// Gets or sets the AABB of this node which contains the current subtree.
      /// </summary>
      /// <value>The AABB of this node which contains the current subtree.</value>
      Aabb IAabbTreeNode<T>.Aabb
      {
        get { return Aabb; }
        set { Aabb = value; }
      }


      /// <summary>
      /// Gets or sets the left child node.
      /// </summary>
      /// <value>
      /// The left child node. (Or <see langword="null"/> if the node is a leaf node.)
      /// </value>
      IAabbTreeNode<T> IAabbTreeNode<T>.LeftChild
      {
        get { return LeftChild; }
        set { LeftChild = (Node)value; }
      }


      /// <summary>
      /// Gets or sets the right child node.
      /// </summary>
      /// <value>
      /// The right child node. (Or <see langword="null"/> if the node is a leaf node.)
      /// </value>
      IAabbTreeNode<T> IAabbTreeNode<T>.RightChild
      {
        get { return RightChild; }
        set { RightChild = (Node)value; }
      }


      /// <summary>
      /// Gets the parent node.
      /// </summary>
      /// <value>
      /// The parent node. (Or <see langword="null"/> if the node is a leaf node.)
      /// </value>
      /// <exception cref="NotSupportedException">
      /// <see cref="Node"/> does not store a reference to its parent node.
      /// </exception>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
      IAabbTreeNode<T> IAabbTreeNode<T>.Parent
      {
        get { throw new NotSupportedException("AdaptiveAabbTree<T>.Node does not store a reference to its parent node."); }
      }


      /// <summary>
      /// Gets (or sets) the data held in this node.
      /// </summary>
      /// <value>The data of this node.</value>
      T IAabbTreeNode<T>.Item
      {
        get { return Item; }
      }
      #endregion
    }
    #endregion
  }
}
