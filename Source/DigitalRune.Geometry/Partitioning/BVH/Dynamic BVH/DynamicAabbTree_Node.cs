// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Partitioning
{
  partial class DynamicAabbTree<T>
  {
    /// <summary>
    /// Represents a node of an <see cref="AabbTree{T}"/>.
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
      /// The parent of the node or <see langword="null"/> if the current node is the root of the 
      /// tree.
      /// </summary>
      public Node Parent;


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
      /// Gets a value indicating whether this instance is leaf node.
      /// </summary>
      /// <value>
      /// <see langword="true"/> if this instance is leaf; otherwise, <see langword="false"/> if it
      /// is an internal node.
      /// </value>
      public bool IsLeaf
      {
        get { return LeftChild == null && RightChild == null; }
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
        set
        {
          var node = (Node)value;

          Debug.Assert(value == null || node.Parent == null, "Cannot insert child node. The node is already the child of another node.");

          // Detach previous child.
          if (LeftChild != null)
            LeftChild.Parent = null;

          LeftChild = node;

          // Attach new child.
          if (LeftChild != null)
            LeftChild.Parent = this;
        }
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
        set
        {
          var node = (Node)value;

          Debug.Assert(value == null || node.Parent == null, "Cannot insert child node. The node is already the child of another node.");

          // Detach previous child.
          if (RightChild != null)
            RightChild.Parent = null;

          RightChild = node;

          // Attach new child.
          if (RightChild != null)
            RightChild.Parent = this;
        }
      }


      /// <summary>
      /// Gets the parent node.
      /// </summary>
      /// <value>
      /// The parent node. (Or <see langword="null"/> if the node is a leaf node.)
      /// </value>
      IAabbTreeNode<T> IAabbTreeNode<T>.Parent
      {
        get { return Parent; }
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
  }
}
