// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;


namespace DigitalRune.Linq
{
  /// <summary>
  /// Provides new extension methods for traversing trees using LINQ.
  /// </summary>
  public static class TreeHelper
  {
    // Important Notes:
    // Methods that use "return yield" are encapsulated and separated from the null-argument
    // checks! Reason: The whole method is evaluated lazy in Enumerator.MoveNext(). But the 
    // ArgumentNullExceptions should be thrown right away. Therefore 2 separate methods!


    /// <summary>
    /// Gets the ancestors of a certain node.
    /// </summary>
    /// <typeparam name="T">The type of node that is enumerated.</typeparam>
    /// <param name="node">
    /// The reference node where to start the search. (The reference node will not be included in 
    /// the resulting enumeration.)
    /// </param>
    /// <param name="getParent">
    /// <para>
    /// A method that retrieves the parent object for a node of type <typeparamref name="T"/>.
    /// </para>
    /// <para>
    /// The method should return <see langword="null"/> to indicate that a node does not have a
    /// parent. <see cref="GetAncestors{T}"/> guarantees that <paramref name="getParent"/> is never 
    /// called with <see langword="null"/> as parameter.
    /// </para>
    /// </param>
    /// <returns>
    /// The ancestors of <paramref name="node"/> (along the path from the node to the root).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getParent"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<T> GetAncestors<T>(T node, Func<T, T> getParent) where T : class
    {
      if (node == null)
        throw new ArgumentNullException("node");
      if (getParent == null)
        throw new ArgumentNullException("getParent");

      return GetAncestorsImpl(node, getParent);
    }


    private static IEnumerable<T> GetAncestorsImpl<T>(T node, Func<T, T> getParent) where T : class
    {
      T parent = getParent(node);
      while (parent != null)
      {
        yield return parent;
        parent = getParent(parent);
      }
    }


    /// <summary>
    /// Gets the given node and its ancestors.
    /// </summary>
    /// <typeparam name="T">The type of node that is enumerated.</typeparam>
    /// <param name="node">
    /// The reference node where to start the search. (The reference node will be the first node in 
    /// the resulting enumeration.)
    /// </param>
    /// <param name="getParent">
    /// <para>
    /// A method that retrieves the parent object for a node of type <typeparamref name="T"/>.
    /// </para>
    /// <para>
    /// The method should return <see langword="null"/> to indicate that a node does not have a
    /// parent. <see cref="GetSelfAndAncestors{T}"/> guarantees that <paramref name="getParent"/> is 
    /// never called with <see langword="null"/> as parameter.
    /// </para>
    /// </param>
    /// <returns>
    /// The <paramref name="node"/> and its ancestors (along the path from the node to the root).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getParent"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<T> GetSelfAndAncestors<T>(T node, Func<T, T> getParent) where T : class
    {
      if (node == null)
        throw new ArgumentNullException("node");
      if (getParent == null)
        throw new ArgumentNullException("getParent");

      return GetSelfAndAncestorsImpl(node, getParent);
    }


    private static IEnumerable<T> GetSelfAndAncestorsImpl<T>(T node, Func<T, T> getParent) where T : class
    {
      // Start with current node.
      yield return node;

      // Same as GetAncestorsImpl().
      T parent = getParent(node);
      while (parent != null)
      {
        yield return parent;
        parent = getParent(parent);
      }
    }


    /// <summary>
    /// Gets the root of a tree.
    /// </summary>
    /// <typeparam name="T">The type of node that is enumerated.</typeparam>
    /// <param name="node">The reference node where to start the search.</param>
    /// <param name="getParent">
    /// <para>
    /// A method that retrieves the parent object for a node of type <typeparamref name="T"/>.
    /// </para>
    /// <para>
    /// The method should return <see langword="null"/> to indicate that a node does not have a
    /// parent. <see cref="GetRoot{T}"/> guarantees that <paramref name="getParent"/> is never 
    /// called with <see langword="null"/> as parameter.
    /// </para>
    /// </param>
    /// <returns>The root node.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getParent"/> is <see langword="null"/>.
    /// </exception>
    public static T GetRoot<T>(T node, Func<T, T> getParent) where T : class
    {
      if (node == null)
        throw new ArgumentNullException("node");
      if (getParent == null)
        throw new ArgumentNullException("getParent");

      return GetSelfAndAncestorsImpl(node, getParent).Last();
    }


    /// <overloads>
    /// <summary>
    /// Gets the descendants of a given node.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the descendants of a given node using a depth-first search.
    /// </summary>
    /// <typeparam name="T">The type of node that is enumerated.</typeparam>
    /// <param name="node">
    /// The reference node where to start the search. (The reference node will not be included in
    /// the enumeration.)
    /// </param>
    /// <param name="getChildren">
    /// <para>
    /// A method that retrieves the children of an object of type <typeparamref name="T"/>. 
    /// </para>
    /// <para>
    /// <see cref="GetDescendants{T}(T,Func{T,IEnumerable{T}})"/> guarantees that 
    /// <paramref name="getChildren"/> is never called with <see langword="null"/> as parameter. The
    /// enumeration returned by <paramref name="getChildren"/> may contain <see langword="null"/>.
    /// </para>
    /// </param>
    /// <returns>The descendants of <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getChildren"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method can be used to traverse a tree in depth-first order (preorder).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static IEnumerable<T> GetDescendants<T>(T node, Func<T, IEnumerable<T>> getChildren) where T : class
    {
      return GetDescendants(node, getChildren, true);
    }


    /// <summary>
    /// Gets the descendants of a given node using a depth-first search or a breadth-first 
    /// search.
    /// </summary>
    /// <typeparam name="T">The type of node that is enumerated.</typeparam>
    /// <param name="node">
    /// The reference node where to start the search. (The reference node will not be included in 
    /// the enumeration.)
    /// </param>
    /// <param name="getChildren">
    /// <para>
    /// A method that retrieves the children of an object of type <typeparamref name="T"/>. 
    /// </para>
    /// <para>
    /// <see cref="GetDescendants{T}(T,Func{T,IEnumerable{T}},bool)"/> guarantees that 
    /// <paramref name="getChildren"/> is never called with <see langword="null"/> as parameter. The
    /// enumeration returned by <paramref name="getChildren"/> may contain <see langword="null"/>.
    /// </para>
    /// </param>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>The descendants of <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getChildren"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method can be used to traverse a tree in either depth-first order (preorder) or in 
    /// breadth-first order (also known as level-order).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static IEnumerable<T> GetDescendants<T>(T node, Func<T, IEnumerable<T>> getChildren, bool depthFirst) where T : class
    {
      if (node == null)
        throw new ArgumentNullException("node");
      if (getChildren == null)
        throw new ArgumentNullException("getChildren");

      return GetDescendantsImpl(node, getChildren, depthFirst);
    }


    private static IEnumerable<T> GetDescendantsImpl<T>(T node, Func<T, IEnumerable<T>> getChildren, bool depthFirst) where T : class
    {
      if (depthFirst)
      {
        Stack<T> stack = new Stack<T>();
        stack.Push(node);
        while (stack.Count > 0)
        {
          T descendant = stack.Pop();
          if (descendant != node)
            yield return descendant;

          foreach (T child in getChildren(descendant).Reverse())
            if (child != null)
              stack.Push(child);
        }
      }
      else
      {
        Queue<T> queue = new Queue<T>();
        queue.Enqueue(node);
        while (queue.Count > 0)
        {
          T descendant = queue.Dequeue();
          if (descendant != node)
            yield return descendant;

          foreach (T child in getChildren(descendant))
            if (child != null)
              queue.Enqueue(child);
        }
      }
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
    /// <typeparam name="T">The type of node that is enumerated.</typeparam>
    /// <param name="node">
    /// The reference node where to start the search. (The reference node will be the first element
    /// in the enumeration.)
    /// </param>
    /// <param name="getChildren">
    /// <para>
    /// A method that retrieves the children of an object of type <typeparamref name="T"/>. 
    /// </para>
    /// <para>
    /// <see cref="GetSubtree{T}(T,Func{T,IEnumerable{T}})"/> guarantees that 
    /// <paramref name="getChildren"/> is never called with <see langword="null"/> as parameter. The
    /// enumeration returned by <paramref name="getChildren"/> may contain <see langword="null"/>.
    /// </para>
    /// </param>
    /// <returns>The subtree of <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getChildren"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method can be used to traverse a tree in depth-first order (preorder).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static IEnumerable<T> GetSubtree<T>(T node, Func<T, IEnumerable<T>> getChildren) where T : class
    {
      return GetSubtree(node, getChildren, true);
    }


    /// <summary>
    /// Gets the subtree (the given node and all of its descendants) using a depth-first search or a
    /// breadth-first search.
    /// </summary>
    /// <typeparam name="T">The type of node that is enumerated.</typeparam>
    /// <param name="node">
    /// The reference node where to start the search. (The reference node will be the first element
    /// in the enumeration.)
    /// </param>
    /// <param name="getChildren">
    /// <para>
    /// A method that retrieves the children of an object of type <typeparamref name="T"/>. 
    /// </para>
    /// <para>
    /// <see cref="GetSubtree{T}(T,Func{T,IEnumerable{T}})"/> guarantees that 
    /// <paramref name="getChildren"/> is never called with <see langword="null"/> as parameter. The
    /// enumeration returned by <paramref name="getChildren"/> may contain <see langword="null"/>.
    /// </para>
    /// </param>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>The subtree of <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getChildren"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method can be used to traverse a tree in either depth-first order (preorder) or in 
    /// breadth-first order (also known as level-order).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static IEnumerable<T> GetSubtree<T>(T node, Func<T, IEnumerable<T>> getChildren, bool depthFirst) where T : class
    {
      if (node == null)
        throw new ArgumentNullException("node");
      if (getChildren == null)
        throw new ArgumentNullException("getChildren");

      return GetSubtreeImpl(node, getChildren, depthFirst);
    }


    private static IEnumerable<T> GetSubtreeImpl<T>(T node, Func<T, IEnumerable<T>> getChildren, bool depthFirst) where T : class
    {
      if (depthFirst)
      {
        Stack<T> stack = new Stack<T>();
        stack.Push(node);
        while (stack.Count > 0)
        {
          T descendant = stack.Pop();
          yield return descendant;

          foreach (T child in getChildren(descendant).Reverse())
            if (child != null)
              stack.Push(child);
        }
      }
      else
      {
        Queue<T> queue = new Queue<T>();
        queue.Enqueue(node);
        while (queue.Count > 0)
        {
          T descendant = queue.Dequeue();
          yield return descendant;

          foreach (T child in getChildren(descendant))
            if (child != null)
              queue.Enqueue(child);
        }
      }
    }


    /// <summary>
    /// Gets the leaves of a given tree.
    /// </summary>
    /// <typeparam name="T">The type of node that is enumerated.</typeparam>
    /// <param name="node">The reference node where to start the search.</param>
    /// <param name="getChildren">
    /// <para>
    /// A method that retrieves the children of an object of type <typeparamref name="T"/>.
    /// </para>
    /// <para>
    /// <see cref="GetLeaves{T}(T,Func{T,IEnumerable{T}})"/> guarantees that 
    /// <paramref name="getChildren"/> is never called with <see langword="null"/> as parameter. The
    /// enumeration returned by <paramref name="getChildren"/> may contain <see langword="null"/>.
    /// </para>
    /// </param>
    /// <returns>The leaves of <paramref name="node"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getChildren"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static IEnumerable<T> GetLeaves<T>(T node, Func<T, IEnumerable<T>> getChildren) where T : class
    {
      if (node == null)
        throw new ArgumentNullException("node");
      if (getChildren == null)
        throw new ArgumentNullException("getChildren");

      return GetLeavesImpl(node, getChildren);
    }


    private static IEnumerable<T> GetLeavesImpl<T>(T node, Func<T, IEnumerable<T>> getChildren) where T : class
    {
      Stack<T> stack = new Stack<T>();
      stack.Push(node);
      while (stack.Count > 0)
      {
        T descendant = stack.Pop();

        bool hasChildren = false;
        var children = getChildren(descendant);
        foreach (T child in children.Reverse())
        {
          hasChildren = true;
          if (child != null)
            stack.Push(child);
        }

        if (!hasChildren)
        {
          // Leaf node.
          yield return descendant;
        }
      }
    }


    /// <summary>
    /// Gets the depth of the specified node in a tree.
    /// </summary>
    /// <typeparam name="T">The type of tree node.</typeparam>
    /// <param name="node">The node.</param>
    /// <param name="getParent">
    /// <para>
    /// A method that retrieves the parent object for a node of type <typeparamref name="T"/>.
    /// </para>
    /// <para>
    /// The method should return <see langword="null"/> to indicate that a node does not have a
    /// parent. <see cref="GetDepth{T}"/> guarantees that <paramref name="getParent"/> is never 
    /// called with <see langword="null"/> as parameter.
    /// </para>
    /// </param>
    /// <returns>The depth of the node.</returns>
    /// <remarks>
    /// The depth of a node is the length of the longest upward path to the root. A root node has a 
    /// depth of 0.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="getParent"/> is <see langword="null"/>.
    /// </exception>
    public static int GetDepth<T>(T node, Func<T, T> getParent) where T : class
    {
      if (node == null)
        throw new ArgumentNullException("node");
      if (getParent == null)
        throw new ArgumentNullException("getParent");

      return GetDepthImpl(node, getParent);
    }


    private static int GetDepthImpl<T>(T node, Func<T, T> getParent) where T : class
    {
      T parent = getParent(node);
      if (parent == null)
        return 0;

      return 1 + GetDepthImpl(parent, getParent);
    }


    /// <summary>
    /// Gets the height of the specified tree or subtree.
    /// </summary>
    /// <typeparam name="T">The type of tree node.</typeparam>
    /// <param name="tree">The root of a tree or subtree.</param>
    /// <returns>The height of the tree or subtree.</returns>
    /// <param name="getChildren">
    /// <para>
    /// A method that retrieves the children of an object of type <typeparamref name="T"/>. 
    /// </para>
    /// <para>
    /// <see cref="GetHeight{T}"/> guarantees that <paramref name="getChildren"/> is never called 
    /// with <see langword="null"/> as parameter. The enumeration returned by 
    /// <paramref name="getChildren"/> may contain <see langword="null"/>.
    /// </para>
    /// </param>
    /// <remarks>
    /// The height of the tree is the length of the longest downward path to a leaf. A leaf node has 
    /// a height of 0.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="tree"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static int GetHeight<T>(T tree, Func<T, IEnumerable<T>> getChildren) where T : class
    {
      if (tree == null)
        throw new ArgumentNullException("tree");
      if (getChildren == null)
        throw new ArgumentNullException("getChildren");

      return GetHeightImpl(tree, getChildren);
    }


    private static int GetHeightImpl<T>(T tree, Func<T, IEnumerable<T>> getChildren) where T : class
    {
      var children = getChildren(tree);

      // Find the maximum height of the child nodes.
      bool hasChildren = false;
      int maxChildHeight = 0;
      foreach (T child in children)
      {
        hasChildren = true;
        int childHeight = GetHeightImpl(child, getChildren);
        if (childHeight > maxChildHeight)
          maxChildHeight = childHeight;
      }

      return hasChildren ? 1 + maxChildHeight : 0;
    }
  }
}
