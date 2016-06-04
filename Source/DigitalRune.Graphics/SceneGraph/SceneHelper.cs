// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Effects;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Provides helper methods for working with a scene graph and <see cref="SceneNode"/>s.
  /// </summary>
  public static class SceneHelper
  {
    /// <summary>
    /// Gets a shared collision detection instance.
    /// </summary>
    /// <value>
    /// The shared collision detection instance.
    /// </value>
    /// <remarks>
    /// This instance can be used for ad-hoc collision tests, e.g. in WaterNode.IsUnderwater.
    /// </remarks>
    internal static CollisionDetection CollisionDetection
    {
      get
      {
        if (_collisionDetection == null)
          _collisionDetection = new CollisionDetection();

        return _collisionDetection;
      }
    }
    private static CollisionDetection _collisionDetection;


    //--------------------------------------------------------------
    #region LINQ to Scene Graph
    //--------------------------------------------------------------

    private static readonly Func<SceneNode, SceneNode> GetParentCallback = node => node.Parent;
    private static readonly Func<SceneNode, IEnumerable<SceneNode>> GetChildrenCallback = GetChildren;


    /// <summary>
    /// Gets the children of the given scene node.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>
    /// The children of the given node or an empty <see cref="IEnumerable{T}"/> if 
    /// <paramref name="node"/> or <see cref="SceneNode.Children"/> is <see langword="null"/>.
    /// </returns>
    public static IEnumerable<SceneNode> GetChildren(this SceneNode node)
    {
      if (node == null || node.Children == null)
        return LinqHelper.Empty<SceneNode>();

      return node.Children;
    }


    /// <summary>
    /// Gets the root node.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <returns>The root node.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static SceneNode GetRoot(this SceneNode node)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      while (node.Parent != null)
        node = node.Parent;

      return node;
    }


    /// <summary>
    /// Gets the ancestors of the given scene node.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <returns>The ancestors of this scene node.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<SceneNode> GetAncestors(this SceneNode node)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      return TreeHelper.GetAncestors(node, GetParentCallback);
    }


    /// <summary>
    /// Gets the scene node and its ancestors scene.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <returns>The <paramref name="node"/> and its ancestors of the scene.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<SceneNode> GetSelfAndAncestors(this SceneNode node)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      return TreeHelper.GetSelfAndAncestors(node, GetParentCallback);
    }


    /// <overloads>
    /// <summary>
    /// Gets the descendants of the given scene node.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the descendants of the given scene node using a depth-first search.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <returns>The descendants of this scene node in depth-first order.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<SceneNode> GetDescendants(this SceneNode node)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      return TreeHelper.GetDescendants(node, GetChildrenCallback, true);
    }


    /// <summary>
    /// Gets the descendants of the given scene node using a depth-first or a breadth-first search.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>The descendants of this scene node.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<SceneNode> GetDescendants(this SceneNode node, bool depthFirst)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      return TreeHelper.GetDescendants(node, GetChildrenCallback, depthFirst);
    }


    /// <overloads>
    /// <summary>
    /// Gets the subtree (the given scene node and all of its descendants).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the subtree (the given scene node and all of its descendants) using a depth-first 
    /// search.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <returns>
    /// The subtree (the given scene node and all of its descendants) in depth-first order.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<SceneNode> GetSubtree(this SceneNode node)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      return TreeHelper.GetSubtree(node, GetChildrenCallback, true);
    }


    /// <summary>
    /// Gets the subtree (the given scene node and all of its descendants) using a depth-first or a 
    /// breadth-first search.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>The subtree (the given scene node and all of its descendants).</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<SceneNode> GetSubtree(this SceneNode node, bool depthFirst)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      return TreeHelper.GetSubtree(node, GetChildren, depthFirst);
    }


    /// <summary>
    /// Gets the leaves of the scene node.
    /// </summary>
    /// <param name="node">The scene node where to start the search.</param>
    /// <returns>The leaves of the scene node.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<SceneNode> GetLeaves(this SceneNode node)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      return TreeHelper.GetLeaves(node, GetChildren);
    }
    #endregion


    /// <overloads>
    /// <summary>
    /// Positions/rotates the scene node so that it faces a certain direction (in world space).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Rotates the scene node so that it faces a certain direction (in world space).
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <param name="target">
    /// The target coordinates in world space at which the scene node is "looking".
    /// </param>
    /// <param name="upVector">
    /// The direction that is "up" from the scene node's point of view given in world space. (Does 
    /// not need to be normalized.)
    /// </param>
    /// <remarks>
    /// A scene node uses the same coordinate system as the <strong>XNA Framework</strong>:
    /// By default, the positive x-axis points to the right, the positive y-axis points up, and the 
    /// positive z-axis points towards the viewer. This method rotates the scene node so that its
    /// local forward direction (0, 0, -1) is pointing towards <paramref name="target"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Current <see cref="Pose"/>.<see cref="Pose.Position"/> is the same as 
    /// <paramref name="target"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="upVector"/> is (0, 0, 0).
    /// </exception>
    /// <exception cref="DivideByZeroException">
    /// The direction (<paramref name="target"/> - <see cref="Pose"/>.<see cref="Pose.Position"/>) 
    /// is probably pointing in the same or opposite direction as <paramref name="upVector"/>. (The 
    /// two vectors must not be parallel.)
    /// </exception>
    public static void LookAt(this SceneNode node, Vector3F target, Vector3F upVector)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      LookAt(node, node.PoseWorld.Position, target, upVector);
    }


    /// <summary>
    /// Moves and rotates the scene node so that it faces a certain direction (in world space).
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <param name="position">The new position in world space.</param>
    /// <param name="target">
    /// The target coordinates in world space at which the scene node is "looking".
    /// </param>
    /// <param name="upVector">
    /// The direction that is "up" from the scene node's point of view given in world space. (Does 
    /// not need to be normalized.)
    /// </param>
    /// <remarks>
    /// A scene node uses the same coordinate system as the <strong>XNA Framework</strong>:
    /// By default, the positive x-axis points to the right, the positive y-axis points up, and the 
    /// positive z-axis points towards the viewer. This method moves the scene node to 
    /// <paramref name="position"/> and rotates it so that its local forward direction (0, 0, -1) is 
    /// pointing towards <paramref name="target"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="position"/> is the same as <paramref name="target"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="upVector"/> is (0, 0, 0).
    /// </exception>
    /// <exception cref="DivideByZeroException">
    /// The camera direction (<paramref name="target"/> - <paramref name="position"/>) is probably 
    /// pointing in the same or opposite direction as <paramref name="upVector"/>. (The two vectors 
    /// must not be parallel.)
    /// </exception>
    public static void LookAt(this SceneNode node, Vector3F position, Vector3F target, Vector3F upVector)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      Matrix44F view = Matrix44F.CreateLookAt(position, target, upVector);
      Matrix44F viewInverse = view.Inverse;
      QuaternionF orientation = QuaternionF.CreateRotation(viewInverse.Minor);
      node.PoseWorld = new Pose(position, orientation);
    }



    /// <summary>
    /// Gets a scene node by name from the subtree of the specified scene node.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <param name="name">The name.</param>
    /// <returns>
    /// The first scene node with the given name; or <see langword="null"/> if no matching scene 
    /// node is found.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static SceneNode GetSceneNode(this SceneNode node, string name)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      if (node.Name == name)
        return node;

      if (node.Children != null)
      {
        foreach (var child in node.Children)
        {
          var result = child.GetSceneNode(name);
          if (result != null)
            return result;
        }
      }

      return null;
    }



    /// <summary>
    /// Determines whether whether a scene node contains another scene node in its subtree.
    /// </summary>
    /// <param name="containingNode">The containing node.</param>
    /// <param name="containedNode">The contained node.</param>
    /// <returns>
    /// <see langword="true"/> if the subtree where the root is <paramref name="containingNode"/>
    /// contains <paramref name="containedNode"/> (in other words, <paramref name="containingNode"/>
    /// is an ancestor of <paramref name="containedNode"/> or both nodes are equal); 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="containingNode"/> is <see langword="null"/>.
    /// </exception>
    public static bool Contains(this SceneNode containingNode, SceneNode containedNode)
    {
      if (containingNode == null)
        throw new ArgumentNullException("containingNode");

      if (containedNode == null)
        return false;

      var parent = containedNode.Parent;
      while (parent != null)
      {
        if (parent == containingNode)
          return true;

        parent = containedNode.Parent;
      }
      return false;
    }


    /// <summary>
    /// Clears the <see cref="SceneNode.LastPoseWorld"/> property of the current scene node (and its 
    /// descendants).
    /// </summary>
    /// <param name="sceneNode">The scene node.</param>
    /// <param name="recursive">
    /// If set to <see langword="true"/> this method is executed recursively on the specified node 
    /// and all descendants nodes; otherwise, this method is executed only on the specified node.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sceneNode"/> is <see langword="null"/>.
    /// </exception>
    public static void ClearLastPose(this SceneNode sceneNode, bool recursive)
    {
      if (sceneNode == null)
        throw new ArgumentNullException("sceneNode");

      sceneNode.LastPoseWorld = null;

      if (recursive && sceneNode.Children != null)
        foreach (var child in sceneNode.Children)
          child.ClearLastPose(true);
    }


    /// <summary>
    /// Clears the <see cref="SceneNode.LastScaleWorld"/> property of the current scene node (and 
    /// its descendants).
    /// </summary>
    /// <param name="sceneNode">The scene node.</param>
    /// <param name="recursive">
    /// If set to <see langword="true"/> this method is executed recursively on the specified node 
    /// and all descendants nodes; otherwise, this method is executed only on the specified node.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sceneNode"/> is <see langword="null"/>.
    /// </exception>
    public static void ClearLastScale(this SceneNode sceneNode, bool recursive)
    {
      if (sceneNode == null)
        throw new ArgumentNullException("sceneNode");

      sceneNode.LastScaleWorld = null;

      if (recursive && sceneNode.Children != null)
        foreach (var child in sceneNode.Children)
          child.ClearLastScale(true);
    }


    /// <summary>
    /// Sets <see cref="SceneNode.LastPoseWorld"/> to the current 
    /// <see cref="SceneNode.PoseWorld"/>.
    /// </summary>
    /// <param name="sceneNode">The scene node.</param>
    /// <param name="recursive">
    /// If set to <see langword="true"/> this method is executed recursively on the specified node 
    /// and all descendants nodes; otherwise, this method is executed only on the specified node.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sceneNode"/> is <see langword="null"/>.
    /// </exception>
    public static void SetLastPose(this SceneNode sceneNode, bool recursive)
    {
      if (sceneNode == null)
        throw new ArgumentNullException("sceneNode");

      sceneNode.LastPoseWorld = sceneNode.PoseWorld;

      if (recursive && sceneNode.Children != null)
        foreach (var child in sceneNode.Children)
          child.SetLastPose(true);
    }


    /// <summary>
    /// Sets <see cref="SceneNode.LastScaleWorld"/> to the current 
    /// <see cref="SceneNode.ScaleWorld"/>.
    /// </summary>
    /// <param name="sceneNode">The scene node.</param>
    /// <param name="recursive">
    /// If set to <see langword="true"/> this method is executed recursively on the specified node 
    /// and all descendants nodes; otherwise, this method is executed only on the specified node.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sceneNode"/> is <see langword="null"/>.
    /// </exception>
    public static void SetLastScale(this SceneNode sceneNode, bool recursive)
    {
      if (sceneNode == null)
        throw new ArgumentNullException("sceneNode");

      sceneNode.LastScaleWorld = sceneNode.ScaleWorld;

      if (recursive && sceneNode.Children != null)
        foreach (var child in sceneNode.Children)
          child.SetLastScale(true);
    }


    /// <summary>
    /// Gets the AABB of the current subtree.
    /// </summary>
    /// <param name="sceneNode">The scene node (= root of subtree).</param>
    /// <returns>
    /// The AABB of the subtree rooted at <paramref name="sceneNode"/>, or <see langword="null"/> if
    /// the subtree is empty.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Aabb? GetSubtreeAabb(this SceneNode sceneNode)
    {
      if (sceneNode == null)
        return null;

      Aabb? aabb = null;
      if (GetSubtreeAabbInternal(sceneNode, ref aabb))
      {
        // The extent of subtree is infinite in one or more dimensions.
        aabb = new Aabb(new Vector3F(float.NegativeInfinity), new Vector3F(float.PositiveInfinity));
      }

      return aabb;
    }


    /// <summary>
    /// Gets the AABB of the current subtree.
    /// </summary>
    /// <param name="sceneNode">The scene node (= root of subtree).</param>
    /// <param name="aabb">
    /// The AABB of the subtree rooted at <paramref name="sceneNode"/>, or <see langword="null"/> if
    /// the subtree is empty. If the input parameter is non-null, the method grows the specified 
    /// AABB to include the subtree.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the extent of the current subtree is infinite in one or more 
    /// dimensions; otherwise, <see langword="false"/> if the subtree is empty or has finite size.
    /// </returns>
    internal static bool GetSubtreeAabbInternal(SceneNode sceneNode, ref Aabb? aabb)
    {
      Debug.Assert(sceneNode != null, "sceneNode must not be null.");

      if (sceneNode.Shape is InfiniteShape)
        return true;

      if (!(sceneNode.Shape is EmptyShape))
      {
        if (aabb.HasValue)
          aabb.Value.Grow(sceneNode.Aabb);
        else
          aabb = sceneNode.Aabb;
      }

      if (sceneNode.Children != null)
        foreach (var childNode in sceneNode.Children)
          if (GetSubtreeAabbInternal(childNode, ref aabb))
            return true;

      return false;
    }


    /// <summary>
    /// Determines whether the opacity of the scene node can be changed using 
    /// <see cref="SetInstanceAlpha"/>.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <returns>
    /// <see langword="true"/> if the scene node has an alpha parameter that can be changed using
    /// <see cref="SetInstanceAlpha"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static bool SupportsInstanceAlpha(this SceneNode node)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      return node.GetFlag(SceneNodeFlags.HasAlpha);
    }

    
    /// <summary>
    /// Sets the opacity (alpha) of a scene node - see remarks.
    /// </summary>
    /// <param name="node">The scene node.</param>
    /// <param name="alpha">The alpha value.</param>
    /// <remarks>
    /// This method can be called for all types of <see cref="SceneNode"/>s. If the node is a
    /// <see cref="MeshNode"/>, the method sets the 
    /// <see cref="DefaultEffectParameterSemantics.InstanceAlpha"/> effect parameter bindings in 
    /// the material instances. Currently, this method does nothing for all other types of scene 
    /// nodes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="node"/> is <see langword="null"/>.
    /// </exception>
    public static void SetInstanceAlpha(this SceneNode node, float alpha)
    {
      if (node == null)
        throw new ArgumentNullException("node");

      // ReSharper disable CompareOfFloatsByEqualityOperator
      if (alpha == 1)
      {
        // Abort if node's alpha is already equal to 1.
        if (!node.GetFlag(SceneNodeFlags.IsAlphaSet))
          return;

        // Remember that node's alpha is reset to 1.
        node.ClearFlag(SceneNodeFlags.IsAlphaSet);
      }
      else
      {
        // Remember that node's alpha is not 1.
        node.SetFlag(SceneNodeFlags.IsAlphaSet);
      }
      // ReSharper restore CompareOfFloatsByEqualityOperator

      // The only node which sets the HasAlpha flag is a MeshNode!
      // Cast to MeshNode and update the InstanceAlpha parameter bindings.
      // See also MeshNode.SetHasAlpha().
      var meshNode = node as MeshNode;
      if (meshNode != null)
      {
        foreach (var materialInstance in meshNode.MaterialInstances)
        {
          foreach (var effectBinding in materialInstance.EffectBindings)
          {
            foreach (var parameterBinding in effectBinding.ParameterBindings)
            {
              if (ReferenceEquals(parameterBinding.Description.Semantic, DefaultEffectParameterSemantics.InstanceAlpha))
              {
                var constParameterBinding = parameterBinding as ConstParameterBinding<float>;
                if (constParameterBinding != null)
                  constParameterBinding.Value = alpha;

                break;
              }
            }
          }
        }
      }
    }
  }
}
