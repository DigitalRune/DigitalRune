// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Collections;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Threading;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Manages a scene of 3D objects represented by scene nodes (a.k.a the "scene graph"). 
  /// </summary>
  /// <remarks>
  /// <para>
  /// See <see cref="IScene"/> for general information about scenes. The <see cref="Scene"/> is the 
  /// default implementation of <see cref="IScene"/>. It internally uses a 
  /// <see cref="DigitalRune.Geometry.Collisions.CollisionDomain"/> to accelerate scene queries 
  /// (such as frustum culling).
  /// </para>
  /// <para>
  /// A <see cref="Scene"/> is derived from <see cref="SceneNode"/> and therefore provides the same
  /// helper methods for manipulation and traversal. But a <see cref="Scene"/> object can only be 
  /// the root node of the scene graph - it cannot be attached to another scene node. See 
  /// <see cref="SceneNode"/> for more details regarding the scene hierarchy.
  /// </para>
  /// <para>
  /// <strong>Scene Transformations:</strong><br/>
  /// A <see cref="Scene"/> is derived from <see cref="SceneNode"/> and therefore also has a 
  /// transformation (see <see cref="SceneNode.ScaleLocal"/>/<see cref="SceneNode.PoseLocal"/> or 
  /// <see cref="SceneNode.ScaleWorld"/>/<see cref="SceneNode.PoseWorld"/>) and a bounding shape 
  /// (see <see cref="SceneNode.Shape"/>). The bounding shape is always empty (see 
  /// <see cref="EmptyShape"/>) - it is not used. Since a <see cref="Scene"/> is always the root of 
  /// a 3D scene <see cref="SceneNode.PoseLocal"/> and <see cref="SceneNode.PoseWorld"/> are always 
  /// identical. The default scale is <see cref="Vector3F.One"/>, the default position is 
  /// <see cref="Vector3F.Zero"/> and the default orientation is <see cref="QuaternionF.Identity"/>.
  /// By setting a different values the entire scene is moved or rotated in world space.
  /// </para>
  /// <para>
  /// <strong>Scene Queries and Scene Node Filtering:</strong><br/>
  /// <see cref="Query{T}"/> can be used to query all scene nodes that touch a given reference node:
  /// For example, to get all mesh nodes within the camera frustum, or all light nodes that affect a
  /// mesh node. <see cref="Query{T}"/> uses <see cref="Filter"/> to further filter the query 
  /// result. See <see cref="Filter"/> for more information.
  /// </para>
  /// <para>
  /// <strong>Scene Node Groups:</strong><br/>
  /// <see cref="GetGroup"/> and <see cref="SetGroup"/> can be used to put scene nodes into groups,
  /// where a group is simply an integer ID. This group can be used for filtering. Per default, 
  /// meshes are in group 1, lights are in group 2, cameras are in group 3, all other objects are 
  /// in group 0. The default grouping can be changed by overriding <see cref="GetDefaultGroup"/>.
  /// The default groups and <see cref="Filter"/> settings are: 
  /// </para>
  /// <list type="table">
  /// <listheader><term>Group</term><description>Scene Nodes</description></listheader>
  /// <item>
  /// <term>0</term>
  /// <description>
  /// Scene nodes which can touch all other nodes including scene nodes of the same type. This is 
  /// the default group for custom scene node types derived from <see cref="SceneNode"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <term>1</term>
  /// <description>
  /// <see cref="CameraNode"/>
  /// </description>
  /// </item>
  /// <item>
  /// <term>2</term>
  /// <description>
  /// <see cref="LightNode"/>
  /// </description>
  /// </item>
  /// <item>
  /// <term>3</term>
  /// <description>  
  /// Scene nodes which can be rendered: <see cref="BillboardNode"/>, <see cref="FogNode"/>,
  /// <see cref="LensFlareNode"/>, <see cref="FigureNode"/>, <see cref="ParticleSystemNode"/>,
  /// <see cref="SpriteNode"/><br/>
  /// Nodes in this group can only touch cameras (group 1).
  /// </description>
  /// </item>
  /// <item>
  /// <term>4</term>
  /// <description>
  /// Scene nodes which can be rendered and lit: <see cref="MeshNode"/><br/>
  /// Nodes in this group can only touch cameras (group 1) and lights (group 2).
  /// </description>
  /// </item>
  /// </list>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// A <see cref="Scene"/> cannot be cloned. Calling <see cref="SceneNode.Clone"/> will raise an 
  /// exception.
  /// </para>
  /// </remarks>
  /// <seealso cref="IScene"/>
  /// <seealso cref="ISceneQuery"/>
  /// <seealso cref="SceneNode"/>
  public class Scene : SceneNode, IScene
  {
    // TODO: Add/remove AttachedSceneData when a node is added/removed and use a resource pool?

    // KNOWN ISSUES:
    // Note that the CollisionObject.Enabled flag is not updated, when the scene
    // node is not registered in the scene. This can lead to the following problems
    // with Query() and HaveContact():
    //
    // Problem #1:
    //  1. SceneNode is added to Scene and a CollisionObject is created.
    //  2. SceneNode is removed from Scene, CollisionObject is still cached.
    //  3. User disables SceneNode (SceneNode.IsEnabled = false).
    //  4. User uses SceneNode for HaveContact query.
    //  --> The CollisionObject is still cached, but SceneNode.IsEnabled does not
    //      match CollisionObject.Enabled. HaveContact() may return a wrong contact.
    //
    // Problem #2:
    //  1. Scene node is not registered in Scene.
    //  2. User disables SceneNode (SceneNode.IsEnabled = false).
    //  3. User makes HaveContact query, CollisionObject gets created with Enabled = false.
    //  4. User enables SceneNode (SceneNode.IsEnabled = true).
    //  5. User makes HaveContact query.
    //  --> The CollisionObject is still cached, but SceneNode.IsEnabled is false.
    //      HaveContact() does not return any contacts.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private class NullFilter : IPairFilter<SceneNode>
    {
      public static readonly NullFilter Instance = new NullFilter();
      public bool Filter(Pair<SceneNode> pair) { return true; }
      public event EventHandler<EventArgs> Changed { add { } remove { } }
    }


    private enum ShapeType
    {
      Empty,
      Infinite,
      Other
    }

    /// Additional data stored in SceneNode.SceneData.
    private class AttachedSceneData
    {
      public ShapeType ShapeType;
      public int Group;
      public CollisionObject CollisionObject;

      // Note: New properties must be initialized in GetAttachedSceneData().
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The collision object filter of the collision domain. This filter is never
    // exchanged. Only the contained IPairFilter<SceneNode> can be exchanged.
    private readonly SceneCollisionObjectFilter _collisionObjectFilter;

    // The scene differentiates between scene nodes with:
    //  - EmptyShape    --> ignored
    //  - InfiniteShape --> stored in _infiniteNodes
    //  - other shape   --> stored in _collisionDomain
    // (Note: CameraNodes are also ignored.)
    private readonly List<SceneNode> _infiniteNodes = new List<SceneNode>();
    internal readonly CollisionDomain _collisionDomain;  // internal for unit tests.

    // A list of all queries - max. one entry per query type.
    private readonly List<ISceneQuery> _queries = new List<ISceneQuery>();

    // Reusable lists to avoid memory allocations.
    private readonly List<SceneNode> _tempNodes = new List<SceneNode>();
    private readonly List<Plane> _tempPlanes = new List<Plane>(6);
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a filter which is used in <see cref="Query{T}"/>.
    /// </summary>
    /// <value>
    /// The filter which decides which scene nodes can touch each other. 
    /// </value>
    /// <remarks>
    /// <para>
    /// The default value is a <see cref="SceneNodeCollisionFilter"/> that disables collisions
    /// between two objects of group 1 (two mesh nodes) and two objects of group 3 (two camera 
    /// nodes). If you do not use the default group IDs (see <see cref="GetDefaultGroup"/>) or if
    /// your rendering pipeline has different requirements, this filter must be changed.
    /// </para>
    /// <para>
    /// Changing the filter or the settings of a filter invalidates the currently cached collision 
    /// data. For performance reasons it is not recommended to change the filter at runtime after
    /// the scene has been loaded.
    /// </para>
    /// </remarks>
    public IPairFilter<SceneNode> Filter
    {
      get { return (_filter == NullFilter.Instance) ? null : _filter; }
      set
      {
        _filter = value ?? NullFilter.Instance;
        _collisionObjectFilter.SceneNodeFilter = _filter;
      }
    }
    private IPairFilter<SceneNode> _filter;


    /// <summary>
    /// Gets or sets a value indicating whether multithreading is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if multithreading is enabled; otherwise, <see langword="false"/>. The
    /// default value is <see langword="true"/> if the current system has more than one CPU cores.
    /// </value>
    /// <remarks>
    /// <para>
    /// When multithreading is enabled the scene will distribute the workload across multiple
    /// processors (CPU cores) to improve the performance. 
    /// </para>
    /// <para>
    /// Multithreading adds an additional overhead, therefore it should only be enabled if the 
    /// current system has more than one CPU core and if the other cores are not fully utilized by
    /// the application. Multithreading should be disabled if the system has only one CPU core or if
    /// all other CPU cores are busy. In some cases it might be necessary to run a benchmark of the
    /// application and compare the performance with and without multithreading to decide whether
    /// multithreading should be enabled or not.
    /// </para>
    /// <para>
    /// The scene internally uses the class <see cref="Parallel"/> for parallelization.
    /// <see cref="Parallel"/> is a static class that defines how many worker threads are created, 
    /// how the workload is distributed among the worker threads and more. (See 
    /// <see cref="Parallel"/> to find out more on how to configure parallelization.)
    /// </para>
    /// </remarks>
    /// <seealso cref="Parallel"/>
    public bool EnableMultithreading
    {
      get { return _collisionDomain.EnableMultithreading; }
      set { _collisionDomain.EnableMultithreading = value; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Scene"/> class.
    /// </summary>
    public Scene()
    {
      Children = new SceneNodeCollection();

      if ((GlobalSettings.ValidationLevelInternal & GlobalSettings.ValidationLevelUserHighExpensive) != 0)
        SceneChanged += OnSceneChangedValidation;

      // Create a default collision filter.
      var filter = new SceneNodeCollisionFilter(this);
      filter.Set(1, 1, false);  // Ignore camera vs. camera.
      filter.Set(2, 3, false);  // Ignore light vs. lens flare.
      filter.Set(3, 3, false);  // Ignore lens flare vs. lens flare.
      filter.Set(3, 4, false);  // Ignore lens flare vs. mesh.
      filter.Set(4, 4, false);  // Ignore mesh vs. mesh.
      _filter = filter;
      _collisionObjectFilter = new SceneCollisionObjectFilter(filter);
      var collisionDetection = new CollisionDetection { CollisionFilter = null, };

      // DualPartition is better for frustum culling.
      _collisionDomain = new CollisionDomain(collisionDetection)
      {
        BroadPhase = new DualPartition<CollisionObject> { Filter = _collisionObjectFilter },
      };
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing, bool disposeData)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          _collisionDomain.CollisionObjects.Clear();
        }

        base.Dispose(disposing, disposeData);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <returns>The new instance.</returns>
    protected override SceneNode CreateInstanceCore()
    {
      throw new NotSupportedException("A scene cannot be cloned.");
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    protected override void CloneCore(SceneNode source)
    {
      throw new NotSupportedException("A scene cannot be cloned.");
    }
    #endregion


    /// <inheritdoc/>
    protected override void OnParentChanged(SceneNode oldParent, SceneNode newParent)
    {
      throw new InvalidOperationException("The Scene cannot have a parent node.");
    }


    private static void OnSceneChangedValidation(object sender, SceneChangedEventArgs eventArgs)
    {
      if (eventArgs.Changes == SceneChanges.NodeAdded)
      {
        ValidateShape(eventArgs.SceneNode);
        ValidatePose(eventArgs.SceneNode);
      }
      else if (eventArgs.Changes == SceneChanges.ShapeChanged)
      {
        ValidateShape(eventArgs.SceneNode);
      }
      else if (eventArgs.Changes == SceneChanges.PoseChanged)
      {
        ValidatePose(eventArgs.SceneNode);
      }
    }


    private static void ValidateShape(SceneNode node)
    {
      if (node == null)
        return;

      // Check for NaN.
      Vector3F scale = node.ScaleLocal;
      if (!Numeric.IsFinite(scale.X) || !Numeric.IsFinite(scale.Y) || !Numeric.IsFinite(scale.Z))
      {
        var message = string.Format(
          CultureInfo.InvariantCulture,
          "Invalid scale for scene node '{0}' (parent = '{1}')! If a scene node is part of a scene, the scale must not contain invalid values, e.g. NaN or infinity.",
          node.Name ?? "<unnamed>",
          (node.Parent != null) ? (node.Parent.Name ?? "<unnamed>") : "<no parent>");
        throw new GraphicsException(message);
      }

      // Check for NaN.
      Aabb aabb = node.Shape.GetAabb();
      if (Numeric.IsNaN(aabb.Extent.X) || Numeric.IsNaN(aabb.Extent.Y) || Numeric.IsNaN(aabb.Extent.Z))
      {
        var message =
          string.Format(
            CultureInfo.InvariantCulture,
            "Invalid shape for scene node '{0}', parent = '{1}')! If a scene node is part of a scene, the shape must not contain NaN values.",
            node.Name ?? "<unnamed>",
            (node.Parent != null) ? (node.Parent.Name ?? "<unnamed>") : "<no parent>");
        throw new GraphicsException(message);
      }

      // Check for infinity. (Only some shapes are infinite per definition.)
      if (!(node.Shape is InfiniteShape) && !(node.Shape is PlaneShape) && !(node.Shape is LineShape))
      {
        if (!Numeric.IsFinite(aabb.Extent.X) || !Numeric.IsFinite(aabb.Extent.Y) || !Numeric.IsFinite(aabb.Extent.Z))
        {
          var message =
            string.Format(
              CultureInfo.InvariantCulture,
              "Invalid shape for scene node '{0}', parent = '{1}')! If a scene node is part of a scene, the shape must not contain infinite values.",
              node.Name ?? "<unnamed>",
              (node.Parent != null) ? (node.Parent.Name ?? "<unnamed>") : "<no parent>");
          throw new GraphicsException(message);
        }
      }
    }


    private static void ValidatePose(SceneNode node)
    {
      if (node == null)
        return;

      Pose pose = node.PoseLocal;

      // Check if pose is valid. (User might have set pose to 'new Pose()' which is invalid.)
      if (pose.Orientation == Matrix33F.Zero)
      {
        var message = string.Format(
          CultureInfo.InvariantCulture,
          "Invalid pose for scene node '{0}' (parent = '{1}')! If a scene node is part of a scene, the orientation must not be a zero matrix.",
          node.Name ?? "<unnamed>",
          (node.Parent != null) ? (node.Parent.Name ?? "<unnamed>") : "<no parent>");
        throw new GraphicsException(message);
      }

      // Check for NaN.
      float value = pose.Position.X + pose.Position.Y + pose.Position.Z
                    + pose.Orientation.M00 + pose.Orientation.M01 + pose.Orientation.M02
                    + pose.Orientation.M10 + pose.Orientation.M11 + pose.Orientation.M12
                    + pose.Orientation.M20 + pose.Orientation.M21 + pose.Orientation.M22;
      if (!Numeric.IsFinite(value))
      {
        var message = string.Format(
          CultureInfo.InvariantCulture,
          "Invalid pose for scene node '{0}' (parent = '{1}')! If a scene node is part of a scene, the pose must not contain invalid values, e.g. NaN or infinity.",
          node.Name ?? "<unnamed>",
          (node.Parent != null) ? (node.Parent.Name ?? "<unnamed>") : "<no parent>");
        throw new GraphicsException(message);
      }
    }


    /// <summary>
    /// Gets the <see cref="AttachedSceneData"/> of scene node. If the scene node does not contain 
    /// scene data, a new <see cref="AttachedSceneData"/> object is created and stored in the scene 
    /// node.
    /// </summary>
    /// <param name="sceneNode">The scene node.</param>
    /// <returns>The attached scene data.</returns>
    private AttachedSceneData GetAttachedSceneData(SceneNode sceneNode)
    {
      var sceneData = (sceneNode.SceneData as AttachedSceneData);
      if (sceneData == null)
      {
        sceneData = new AttachedSceneData
        {
          ShapeType = GetShapeType(sceneNode),
          Group = GetDefaultGroup(sceneNode)
        };
        sceneNode.SceneData = sceneData;
      }

      return sceneData;
    }


    private static ShapeType GetShapeType(SceneNode sceneNode)
    {
      ShapeType type;
      if (sceneNode.Shape is EmptyShape)
        type = ShapeType.Empty;
      else if (sceneNode.Shape is InfiniteShape)
        type = ShapeType.Infinite;
      else
        type = ShapeType.Other;

      return type;
    }


    /// <summary>
    /// Gets the group ID of a scene node.
    /// </summary>
    /// <param name="sceneNode">The scene node.</param>
    /// <returns>The group ID.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sceneNode"/> is <see langword="null"/>.
    /// </exception>
    public int GetGroup(SceneNode sceneNode)
    {
      if (sceneNode == null)
        throw new ArgumentNullException("sceneNode");

      return GetAttachedSceneData(sceneNode).Group;
    }


    /// <summary>
    /// Sets the group ID of a scene node.
    /// </summary>
    /// <param name="sceneNode">The scene node.</param>
    /// <param name="group">The group ID.</param>
    /// <remarks>
    /// The group is usually used for internal optimization. It is not allowed to change the group 
    /// of a scene node that has already been added to the scene. The group must be set BEFORE the 
    /// scene node is added to the scene graph.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sceneNode"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="GraphicsException">
    /// The group of the scene node cannot be changed because the scene node has already been added
    /// to a scene.
    /// </exception>
    public void SetGroup(SceneNode sceneNode, int group)
    {
      if (sceneNode == null)
        throw new ArgumentNullException("sceneNode");

      var sceneData = GetAttachedSceneData(sceneNode);
      if (sceneData.CollisionObject != null && sceneData.CollisionObject.Domain != null)
        throw new GraphicsException("Cannot change group of a scene node if the scene node has already been added to a scene.");

      sceneData.Group = group;
    }


    /// <summary>
    /// Called when the default group of a new scene node is set.
    /// </summary>
    /// <param name="sceneNode">The scene node.</param>
    /// <returns>The default group of a scene node.</returns>
    /// <remarks>
    /// <para>
    /// Derived classes can override this method to change the default groups. The groups are used
    /// by the <see cref="Filter"/> to determine which scene nodes can touch each other. See remarks
    /// of the <see cref="Scene"/> class for more information.
    /// </para>
    /// </remarks>
    protected virtual int GetDefaultGroup(SceneNode sceneNode)
    {
      // Checks roughly sorted by frequency.
      if (sceneNode is MeshNode)
        return 4;

      if (sceneNode is LightNode)
        return 2;

      if (sceneNode is CameraNode)
        return 1;

      if (sceneNode is DecalNode
#if PARTICLES
          || sceneNode is ParticleSystemNode
#endif
          || sceneNode is BillboardNode
          || sceneNode is LensFlareNode
          || sceneNode is FogNode
          || sceneNode is SpriteNode
          || sceneNode is FigureNode
          || sceneNode is SkyNode
          || sceneNode is OccluderNode
          || sceneNode is RenderToTextureNode
          || sceneNode is WaterNode)
      {
        return 3;
      }

      return 0;
    }


    /// <summary>
    /// Gets the collision object from the <see cref="AttachedSceneData" />. If none exists, a new
    /// collision object is created.
    /// </summary>
    /// <param name="sceneNode">The scene node.</param>
    /// <param name="updateEnabledFlag">
    /// <see langword="true"/> to update the collision objects <see cref="CollisionObject.Enabled"/>
    /// flag.
    /// </param>
    /// <returns>The collision object.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sceneNode" /> is <see langword="null" />.
    /// </exception>
    private CollisionObject GetCollisionObject(SceneNode sceneNode, bool updateEnabledFlag)
    {
      if (sceneNode == null)
        throw new ArgumentNullException("sceneNode");

      var sceneData = GetAttachedSceneData(sceneNode);
      var collisionObject = sceneData.CollisionObject;
      if (collisionObject == null)
      {
        collisionObject = new CollisionObject(sceneNode)
        {
          Enabled = sceneNode.ActualIsEnabled,
          Type = CollisionObjectType.Trigger
        };
        sceneData.CollisionObject = collisionObject;
      }
      else if (updateEnabledFlag)
      {
        collisionObject.Enabled = sceneNode.ActualIsEnabled;
      }

      return collisionObject;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnSceneChanged(SceneChangedEventArgs eventArgs)
    {
      base.OnSceneChanged(eventArgs);

      var sceneNode = eventArgs.SceneNode;
      if (eventArgs.Changes == SceneChanges.NodeAdded)
      {
        // Recursively add scene nodes to collision domain.
        Register(sceneNode);
      }
      else if (eventArgs.Changes == SceneChanges.NodeRemoved)
      {
        // Recursively remove scene nodes to collision domain.
        Unregister(sceneNode);
      }
      else if (eventArgs.Changes == SceneChanges.IsEnabledChanged)
      {
        // Recursively enable/disable collision objects.
        HandleIsEnabledChanged(sceneNode);
      }
      else if (eventArgs.Changes == SceneChanges.ShapeChanged)
      {
        // Shape changes are usually handled by the collision domain, except for 
        // EmptyShapes: EmptyShapes should not be registered in the collision domain.
        HandleShapeChange(sceneNode);
      }
    }


    private void Register(SceneNode sceneNode)
    {
      Debug.Assert(sceneNode != null, "Register() expects non-null argument.");

      var sceneData = GetAttachedSceneData(sceneNode);

      // Note: ShapeType changes are handled by HandleShapeChange(). But the ShapeType
      // does not get updated when the scene node is detached from the scene.
      // --> ShapeType needs to be updated when the sceneNode is added to the scene.
      sceneData.ShapeType = GetShapeType(sceneNode);

      Debug.Assert(
        !_infiniteNodes.Contains(sceneNode)
        && (sceneData.CollisionObject == null || sceneData.CollisionObject.Domain == null),
        "Register() must not be called when the scene node is already registered.");

      if (!(sceneNode is CameraNode)) // CameraNodes are excluded from the collections.
      {
        switch (sceneData.ShapeType)
        {
          case ShapeType.Empty:
            // Ignore.
            break;
          case ShapeType.Infinite:
            _infiniteNodes.Add(sceneNode);
            break;
          case ShapeType.Other:
            var collisionObject = GetCollisionObject(sceneNode, true);
            _collisionDomain.CollisionObjects.Add(collisionObject);
            break;
        }
      }

      // Add descendant nodes.
      if (sceneNode.Children != null)
        foreach (var child in sceneNode.Children)
          Register(child);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    private void Unregister(SceneNode sceneNode)
    {
      Debug.Assert(sceneNode != null, "Unregister() expects non-null argument.");

      var sceneData = GetAttachedSceneData(sceneNode);

      Debug.Assert(
        sceneNode.Shape is EmptyShape
        || sceneNode.Shape is InfiniteShape && _infiniteNodes.Contains(sceneNode)
        || sceneNode is CameraNode
        || sceneData.CollisionObject != null && sceneData.CollisionObject.Domain == _collisionDomain,
        "Unregister() must not be called when the scene node is not registered.");

      if (!(sceneNode is CameraNode)) // CameraNodes are excluded from the collections.
      {
        switch (sceneData.ShapeType)
        {
          case ShapeType.Empty:
            // Ignore.
            break;
          case ShapeType.Infinite:
            _infiniteNodes.Remove(sceneNode);
            break;
          case ShapeType.Other:
            var collisionObject = sceneData.CollisionObject;
            _collisionDomain.CollisionObjects.Remove(collisionObject);

            // Enable collision object. (The user should be able to use the external
            // object for scene queries, even when it is not in the scene! Note that
            // CollisionObject.Enabled is not updated when SceneNode.IsEnabled is 
            // changed and the scene node is not in the scene.)
            collisionObject.Enabled = true;
            break;
        }
      }

      // Remove descendant nodes.
      if (sceneNode.Children != null)
        foreach (var child in sceneNode.Children)
          Unregister(child);
    }


    private void HandleIsEnabledChanged(SceneNode sceneNode)
    {
      Debug.Assert(sceneNode != null, "HandleIsEnabledChanged() expects non-null argument.");

      // Recursively update Enabled flag of collision objects.
      // (Note: A scene node is enabled when its IsEnabled flag is set and all 
      // ancestors are enabled.)
      HandleIsEnabledChanged(sceneNode, sceneNode.ActualIsEnabled);
    }


    private void HandleIsEnabledChanged(SceneNode sceneNode, bool isParentEnabled)
    {
      bool isEnabled = sceneNode.IsEnabled && isParentEnabled;
      var sceneData = GetAttachedSceneData(sceneNode);
      switch (sceneData.ShapeType)
      {
        case ShapeType.Empty:
        case ShapeType.Infinite:
          // Do nothing.
          break;
        case ShapeType.Other:
          var collisionObject = sceneData.CollisionObject;
          if (collisionObject != null)      // CameraNodes don't have a CollisionObject usually.
            collisionObject.Enabled = isEnabled;
          break;
      }

      if (sceneNode.Children != null)
        foreach (var child in sceneNode.Children)
          HandleIsEnabledChanged(child, isEnabled);
    }


    private void HandleShapeChange(SceneNode sceneNode)
    {
      Debug.Assert(sceneNode != null, "HandleShapeChange() expects non-null argument.");

      var sceneData = GetAttachedSceneData(sceneNode);
      var oldShapeType = sceneData.ShapeType;
      var newShapeType = GetShapeType(sceneNode);
      sceneData.ShapeType = newShapeType;

      // Ensure that scene node is registered in the correct collection.
      if (oldShapeType != newShapeType)
      {
        switch (oldShapeType)
        {
          case ShapeType.Empty:
            // Ignore.
            break;
          case ShapeType.Infinite:
            _infiniteNodes.Remove(sceneNode);
            break;
          case ShapeType.Other:
            var collisionObject = sceneData.CollisionObject;
            if (collisionObject != null)   // CameraNodes don't have a CollisionObject usually.
              _collisionDomain.CollisionObjects.Remove(collisionObject);
            break;
        }

        if (!(sceneNode is CameraNode)) // CameraNodes are excluded from the collections.
        {
          switch (sceneData.ShapeType)
          {
            case ShapeType.Empty:
              // Ignore.
              break;
            case ShapeType.Infinite:
              _infiniteNodes.Add(sceneNode);
              break;
            case ShapeType.Other:
              var collisionObject = GetCollisionObject(sceneNode, true);
              _collisionDomain.CollisionObjects.Add(collisionObject);
              break;
          }
        }
      }
    }


    /// <summary>
    /// Updates the scene.
    /// </summary>
    /// <param name="deltaTime">The time step size in seconds.</param>
    /// <remarks>
    /// A scene needs to be updated once per frame. The method recomputes the internal information
    /// that is used for scene queries (such as frustum culling) and may perform other
    /// optimizations.
    /// </remarks>
    public void Update(TimeSpan deltaTime)
    {
      // Clear bins.
      foreach (var query in _queries)
      {
        var referenceNode = query.ReferenceNode;
        if (referenceNode != null)
          referenceNode.ClearFlag(SceneNodeFlags.IsDirtyScene);

        query.Reset();
      }

      // Update collisions.
      _collisionDomain.Update(deltaTime);
    }


    /// <inheritdoc/>
    public bool HaveContact(SceneNode nodeA, SceneNode nodeB)
    {
      if (nodeA == null)
        throw new ArgumentNullException("nodeA");
      if (nodeB == null)
        throw new ArgumentNullException("nodeB");

      var shapeA = nodeA.Shape;
      var shapeB = nodeB.Shape;
      if (shapeA is EmptyShape || shapeB is EmptyShape)
        return false;

      if (shapeA is InfiniteShape || shapeB is InfiniteShape)
        return nodeA.ActualIsEnabled && nodeB.ActualIsEnabled;

      var collisionObjectA = GetCollisionObject(nodeA, false);
      var collisionObjectB = GetCollisionObject(nodeB, false);
      return _collisionDomain.HaveContact(collisionObjectA, collisionObjectB);
    }


    /// <inheritdoc/>
    public T Query<T>(SceneNode referenceNode, RenderContext context) where T : class, ISceneQuery, new()
    {
      if (referenceNode == null)
        throw new ArgumentNullException("referenceNode");

      // Use proxy node instead of specified node, if set.
      while (referenceNode.Proxy != null)
        referenceNode = referenceNode.Proxy;

      // Check if the node was modified since Scene.Update(). This can happen, for
      // example, if a CameraNode is rotated to capture 6 sides of a cube map.
      if (referenceNode.GetFlag(SceneNodeFlags.IsDirtyScene))
      {
        // Reset all queries that use this node.
        int numberOfQueries = _queries.Count;
        for (int i = 0; i < numberOfQueries; i++)
        {
          if (_queries[i].ReferenceNode == referenceNode)
            _queries[i].Reset();
        }

        referenceNode.ClearFlag(SceneNodeFlags.IsDirtyScene);
      }

      try
      {
        // ----- Get query of type T.
        T query = null;
        int numberOfQueries = _queries.Count;
        for (int i = 0; i < numberOfQueries; i++)
        {
          query = _queries[i] as T;
          if (query != null)
            break;
        }

        if (query != null)
        {
          // Query exists.
          // Return cached result if the reference node is the same.
          if (query.ReferenceNode == referenceNode)
            return query;
        }
        else
        {
          // Create new query.
          query = new T();
          _queries.Add(query);
        }

        if (referenceNode.Shape is InfiniteShape)
        {
          // ----- Infinite shape queries.
          // Return all scene nodes.
          GetSceneNodes(this, referenceNode, _filter, _tempNodes);
          query.Set(referenceNode, _tempNodes, context);
          return query;
        }

        // Add infinite nodes to temporary list.
        foreach (var node in _infiniteNodes)
          if (node.ActualIsEnabled && _filter.Filter(new Pair<SceneNode>(referenceNode, node)))
            _tempNodes.Add(node);

        var cameraNode = referenceNode as CameraNode;
        if (cameraNode != null)
        {
          // ----- Camera frustum queries.
          // Extract frustum planes.
          Matrix44F viewProjection = cameraNode.Camera.Projection * cameraNode.View;
          GeometryHelper.ExtractPlanes(viewProjection, _tempPlanes, false);

          // Use broad phase to do frustum culling.
          foreach (var collisionObject in ((DualPartition<CollisionObject>)_collisionDomain.BroadPhase).GetOverlaps(_tempPlanes))
          {
            // ISupportFrustumCulling.GetOverlaps() does not apply filtering.
            // --> Manually check if collision object is enabled.
            if (collisionObject.Enabled)
            {
              //if (collisionObject.GeometricObject.Shape is PlaneShape
              //    && !HaveAabbPlaneContact(referenceNode, collisionObject))
              //  continue;

              var sceneNode = collisionObject.GeometricObject as SceneNode;
              if (sceneNode != null && _filter.Filter(new Pair<SceneNode>(referenceNode, sceneNode)))
                _tempNodes.Add(sceneNode);
            }
          }
        }
        else
        {
          // ----- Normal object queries.
          var collisionObject = GetCollisionObject(referenceNode, false);
          foreach (var contactSet in _collisionDomain.ContactSets.GetContacts(collisionObject))
          {
            SceneNode sceneNode;
            if (contactSet.ObjectA == collisionObject)
              sceneNode = contactSet.ObjectB.GeometricObject as SceneNode;
            else
              sceneNode = contactSet.ObjectA.GeometricObject as SceneNode;

            Debug.Assert(sceneNode != null);
            _tempNodes.Add(sceneNode);
          }
        }

        query.Set(referenceNode, _tempNodes, context);
        return query;
      }
      finally
      {
        _tempPlanes.Clear();
        _tempNodes.Clear();
      }
    }


    /// <summary>
    /// Recursively gets all scene nodes and adds them to the specified collection in depth-first
    /// order. (Scene node filtering is applied, EmptyShapes are ignored.)
    /// </summary>
    /// <param name="node">The root node.</param>
    /// <param name="referenceNode">The reference node.</param>
    /// <param name="filter">The scene node filter.</param>
    /// <param name="list">The collection to which all scene nodes are added.</param>
    private static void GetSceneNodes(SceneNode node, SceneNode referenceNode, IPairFilter<SceneNode> filter, List<SceneNode> list)
    {
      if (node.IsEnabled)
      {
        if (!(node.Shape is EmptyShape) && filter.Filter(new Pair<SceneNode>(node, referenceNode)))
          list.Add(node);

        if (node.Children != null)
          foreach (var child in node.Children)
            GetSceneNodes(child, referenceNode, filter, list);
      }
    }


    //private static bool HaveAabbPlaneContact(SceneNode referenceNode, CollisionObject collisionObject)
    //{
    //  // AABB in world space.
    //  var aabb = referenceNode.Aabb;

    //  // Plane in world space.
    //  var planeShape = (PlaneShape)collisionObject.GeometricObject.Shape;
    //  var plane = new Plane(planeShape);
    //  var scale = collisionObject.GeometricObject.Scale;
    //  plane.Scale(ref scale);
    //  var pose = collisionObject.GeometricObject.Pose;
    //  plane.ToWorld(ref pose);

    //  return GeometryHelper.HaveContact(aabb, plane);
    //}
    #endregion
  }
}
