// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Partitioning;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Manages a collection of 3D objects represented by scene nodes (a.k.a the "scene graph"). 
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="IScene"/> manages a collection of <see cref="SceneNode"/>s. A scene node 
  /// usually represents an instance of a graphics object (a mesh, a camera, a light, etc.). 
  /// </para>
  /// <para>
  /// A scene has two important purposes:
  /// <list type="bullet">
  /// <item>
  /// <term>
  /// Scene Graph
  /// </term>
  /// <description>
  /// The main purpose is to organize the objects in a 3D scene. Graphics objects (such as meshes,
  /// cameras, lights, etc.) are represented by scene nodes. Scene nodes are organized in a 
  /// hierarchy: Each scene node can have a <see cref="SceneNode.Parent"/> and zero or more 
  /// <see cref="SceneNode.Children"/>. The resulting hierarchy is a tree (a graph without cycles) -
  /// usually called the <i>scene graph</i>. See class <see cref="SceneNode"/> to find out more on
  /// how to place objects within a scene.
  /// </description>
  /// </item>
  /// <item>
  /// <term>
  /// Scene Queries
  /// </term>
  /// <description>
  /// The second purpose is to execute queries against the scene. For example, when rendering a 
  /// scene it is important to quickly access all scene nodes that are within the camera frustum.
  /// When an object is lit, it is important to quickly get all lights that affect the object. A
  /// scene query is run by calling the method <see cref="Query{T}"/>. The type parameter of the 
  /// method specifies the type of the query. See method <see cref="Query{T}"/> and interface
  /// <see cref="ISceneQuery"/> for more information.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// The scene graph is the organization of the scene nodes that is visible to the application 
  /// logic. But internally, a scene can organize scene nodes in a way which is optimal for 
  /// rendering. Different types of scenes might require different implementations: For example, 
  /// indoor levels, outdoor levels, top-down views, side-scrolling games, etc. might require
  /// different data structures in order to enable efficient queries. Therefore, different 
  /// applications can use different implementations of <see cref="IScene"/>.
  /// </para>
  /// <para>
  /// The default implementation <see cref="Scene"/> internally uses a <see cref="CollisionDomain"/>
  /// with a <see cref="DualPartition{T}"/> to accelerate scene queries.
  /// </para>
  /// </remarks>
  /// <seealso cref="ISceneQuery"/>
  /// <seealso cref="Scene"/>
  /// <seealso cref="SceneNode"/>
  public interface IScene
  {
    /// <summary>
    /// Gets the scene nodes.
    /// </summary>
    /// <value>The scene nodes.</value>
    SceneNodeCollection Children { get; }


    /// <summary>
    /// Determines whether bounding shapes of two scene nodes overlap.
    /// </summary>
    /// <param name="nodeA">The first scene node.</param>
    /// <param name="nodeB">The second scene node.</param>
    /// <returns>
    /// <see langword="true"/> if the bounding shape of the scene nodes overlap; 
    /// <see langword="false"/> if the two nodes do not touch.
    /// </returns>
    bool HaveContact(SceneNode nodeA, SceneNode nodeB);


    /// <summary>
    /// Gets the scene nodes that touch the specified reference node.
    /// </summary>
    /// <typeparam name="T">
    /// The type of query (see <see cref="ISceneQuery"/>) that should be executed.
    /// </typeparam>
    /// <param name="referenceNode">
    /// The reference node. Must not be <see langword="null"/>.
    /// </param>
    /// <param name="context">The render context.</param>
    /// <returns>The <see cref="ISceneQuery"/> object containing the result of the query.</returns>
    /// <remarks>
    /// <para>
    /// This method can be used to query scene nodes, e.g. all scene nodes that touch the camera
    /// frustum, or all lights that shine on the reference node. This method performs efficient
    /// culling, e.g. if the reference node is a camera node it uses frustum culling to quickly
    /// find all nodes that touch the camera frustum and to reject nodes outside of the frustum.
    /// </para>
    /// <para>
    /// Disabled scene nodes (see property <see cref="SceneNode.IsEnabled"/>) are ignored in scene
    /// queries and should not show up in query results.
    /// </para>
    /// <para>
    /// The scene caches the most recent results of each query type: If <see cref="Query{T}"/> is
    /// called several times per frame for the same query type and reference node, then only the
    /// first call performs work and the other calls return the cached result of the first call. If
    /// the method is called again with the same query type but a different reference node, then the
    /// cache is cleared and the new query is executed. This also means that the result of the first
    /// call is no longer available.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="referenceNode"/> is <see langword="null"/>.
    /// </exception>
    T Query<T>(SceneNode referenceNode, RenderContext context) where T : class, ISceneQuery, new();
  }
}
