// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Provides extension methods for working with AABB trees.
  /// </summary>
  internal static class AabbTreeHelper
  {
    /// <summary>
    /// Gets the ancestors of the given node.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="node">The node.</param>
    /// <returns>
    /// The ancestors of <paramref name="node"/> starting with the direct parent of 
    /// <paramref name="node"/> going upwards to the root of the tree.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<IAabbTreeNode<T>> GetAncestors<T>(this IAabbTreeNode<T> node)
    {
      return TreeHelper.GetAncestors(node, n => n.Parent);
    }


    /// <summary>
    /// Gets the direct children of the given node.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="node">The node.</param>
    /// <returns>The direct children of <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<IAabbTreeNode<T>> GetChildren<T>(this IAabbTreeNode<T> node)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      return GetChildrenImpl(node);
    }


    private static IEnumerable<IAabbTreeNode<T>> GetChildrenImpl<T>(IAabbTreeNode<T> node)
    {
      if (node.IsLeaf)
        yield break;

      yield return node.LeftChild;
      yield return node.RightChild;
    }


    /// <overloads>
    /// <summary>
    /// Gets the descendants of a given node.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the descendants of the given node using a depth-first search.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="node">The node.</param>
    /// <returns>The descendants of <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method can be used to traverse a tree in depth-first order (pre-order).
    /// </remarks>
    public static IEnumerable<IAabbTreeNode<T>> GetDescendants<T>(this IAabbTreeNode<T> node)
    {
      return TreeHelper.GetDescendants(node, GetChildrenImpl);
    }


    /// <summary>
    /// Gets the descendants of the given node using a depth-first search or a breadth-first search.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="node">The node.</param>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>The descendants of <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method can be used to traverse a tree in either depth-first order (pre-order) or in 
    /// breadth-first order (level-order).
    /// </remarks>
    public static IEnumerable<IAabbTreeNode<T>> GetDescendants<T>(this IAabbTreeNode<T> node, bool depthFirst)
    {
      return TreeHelper.GetDescendants(node, GetChildrenImpl, depthFirst);
    }


    /// <overloads>
    /// <summary>
    /// Gets the subtree (the given node and all of its descendants).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the subtree (the given node and all of its descendants) using a depth-first search.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="node">
    /// The reference node where to start the search. (The reference node will be the first 
    /// element in the enumeration.)
    /// </param>
    /// <returns>The subtree of <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method can be used to traverse a tree in depth-first order (pre-order).
    /// </remarks>
    public static IEnumerable<IAabbTreeNode<T>> GetSubtree<T>(this IAabbTreeNode<T> node)
    {
      return TreeHelper.GetSubtree(node, GetChildrenImpl);
    }


    /// <summary>
    /// Gets the subtree (the given node and all of its descendants) using a depth-first search or a 
    /// breadth-first search.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="node">
    /// The reference node where to start the search. (The reference node will be the first 
    /// element in the enumeration.)
    /// </param>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>The descendants of <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method can be used to traverse a tree in either depth-first order (pre-order) or in 
    /// breadth-first order (also known as level-order).
    /// </remarks>
    public static IEnumerable<IAabbTreeNode<T>> GetSubtree<T>(this IAabbTreeNode<T> node, bool depthFirst)
    {
      return TreeHelper.GetSubtree(node, GetChildrenImpl, depthFirst);
    }


    /// <summary>
    /// Gets the leaves of a given tree.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="node">The reference node where to start the search.</param>
    /// <returns>The leaves of <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<IAabbTreeNode<T>> GetLeaves<T>(this IAabbTreeNode<T> node)
    {
      return TreeHelper.GetLeaves(node, GetChildrenImpl);
    }


    /// <summary>
    /// Gets the depth of the given node.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="node">The node.</param>
    /// <returns>The depth of the node.</returns>
    /// <remarks>
    /// The depth of a node is the length of the longest upward path to the root. Therefore, a root 
    /// node has a depth of 0.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static int GetDepth<T>(this IAabbTreeNode<T> node)
    {
      return TreeHelper.GetDepth(node, n => node.Parent);
    }


    /// <summary>
    /// Gets the height of the given tree or subtree.
    /// </summary>
    /// <typeparam name="T">The type of item stored in the tree.</typeparam>
    /// <param name="node">The tree.</param>
    /// <returns>The height of the tree.</returns>
    /// <remarks>
    /// The height of the tree is the length of the longest downward path to a leaf. Therefore, a 
    /// leaf node has a height of 0.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static int GetHeight<T>(this IAabbTreeNode<T> node)
    {
      return TreeHelper.GetHeight(node, GetChildrenImpl);
    }


    /// <summary>
    /// Gets a value that indicates the proximity between to AABBs.
    /// </summary>
    /// <param name="first">The first AABB.</param>
    /// <param name="second">The second AABB.</param>
    /// <returns>
    /// A value that indicates the proximity between <paramref name="first"/> and 
    /// <paramref name="second"/>. (A smaller value means a closer proximity.)
    /// </returns>
    private static float GetProximity(Aabb first, Aabb second)
    {
      // Compute same norm as in Bullet. (2 * Manhattan distance between the centers 
      // of the AABBs.)
      Vector3F distance = (first.Minimum + first.Maximum) - (second.Minimum + second.Maximum);
      return Math.Abs(distance.X) + Math.Abs(distance.Y) + Math.Abs(distance.Z);
    }


    /// <summary>
    /// Selects the AABB that is closest to a given AABB.
    /// </summary>
    /// <param name="reference">The reference AABB.</param>
    /// <param name="first">The first AABB.</param>
    /// <param name="second">The second AABB.</param>
    /// <returns>
    /// 0 if <paramref name="first"/> is closest; otherwise 1 if <paramref name="second"/> is 
    /// closest.
    /// </returns>
    public static int SelectClosest(Aabb reference, Aabb first, Aabb second)
    {
      float proximity0 = GetProximity(reference, first);
      float proximity1 = GetProximity(reference, second);
      return (proximity0 < proximity1) ? 0 : 1;
    }
  }
}
