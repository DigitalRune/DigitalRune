// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents an object in a 3D scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="SceneNode"/> usually represents an instance of a graphics object in a 3D scene. 
  /// See derived classes such as <see cref="MeshNode"/>, <see cref="CameraNode"/>, 
  /// <see cref="LightNode"/>, etc.
  /// </para>
  /// <para>
  /// A scene node can have a transformation (see <see cref="ScaleLocal"/>/<see cref="PoseLocal"/>,
  /// <see cref="ScaleWorld"/>/<see cref="PoseWorld"/>), a bounding shape (see <see cref="Shape"/>), 
  /// a parent and children (see <see cref="Parent"/>, <see cref="Children"/>). 
  /// </para>
  /// <para>
  /// <strong>Scene Graph:</strong><br/>
  /// The scene node hierarchy, defined by the properties <see cref="Parent"/> and 
  /// <see cref="Children"/>, is a tree (a graph without cycles). A scene node can only have zero or
  /// one parent - it cannot be the child of multiple other nodes. Scene nodes are attached to their
  /// parent: When the parent node is transformed (rotated or translated) in a scene, all descendant
  /// nodes move together with the parent node.
  /// </para>
  /// <para>
  /// <strong>Local Transformation vs. World Transformation:</strong><br/>
  /// <see cref="ScaleLocal"/> and <see cref="PoseLocal"/> describe the local transformation of a 
  /// node relative to its parent node. <see cref="ScaleWorld"/> and <see cref="PoseWorld"/> 
  /// describe the absolute transformation of a node in world space.
  /// </para>
  /// <para>
  /// The transformation can be set either in local space (using <see cref="ScaleLocal"/> and 
  /// <see cref="PoseLocal"/>) or in world space (using <see cref="PoseWorld"/>). The other 
  /// properties will be updated automatically. (Note: <see cref="ScaleWorld"/> cannot be set 
  /// directly - the property is read-only.)
  /// </para>
  /// <para>
  /// The local transformation is the dominant transformation: When a scene node is detached from 
  /// its parent and attached to another scene node, it will be placed relative to the new parent. 
  /// The local transformation will remain the same, but the world transformation will change - 
  /// except when the parent's transformation is the identity transform.
  /// </para>
  /// <para>
  /// <strong>Non-uniform Scaling:</strong><br/>
  /// Non-uniform scaling of scene nodes or subtrees is supported, but it should be used with care. 
  /// Non-uniform scalings can be expensive because they require special treatment. (Imagine a scene
  /// node which has a bounding sphere. When the node is scaled non-uniformly, the sphere becomes an
  /// ellipsoid. An ellipsoid requires a different collision algorithm than a simple sphere...)
  /// </para>
  /// <para>
  /// <strong>No Shearing:</strong><br/>
  /// Certain combinations of non-uniform scalings and rotations can create shear transformations. 
  /// Shearing complicates the scene management and prevents certain optimizations. Shearing is 
  /// therefore not supported! The scene graph automatically eliminates any shearing.
  /// </para>
  /// <para>
  /// <para>
  /// <strong>Transformation of Previous Frame:</strong><br/>
  /// A scene node additionally has two optional properties: <see cref="LastScaleWorld"/> and
  /// <see cref="LastPoseWorld"/>. These properties define the scene node's transformation of the 
  /// last frame that was rendered. This information is required by certain effects, such as object
  /// motion blur or camera motion blur. <strong>Important:</strong> These properties are not set 
  /// automatically! <see cref="LastScaleWorld"/> and <see cref="LastPoseWorld"/> need to be updated
  /// by the application logic whenever the transformation of the scene node is changed.
  /// </para>
  /// <strong>Bounding Shape:</strong><br/>
  /// The property <see cref="Shape"/> contains the bounding shape of the scene node. The 
  /// bounding shape is used by the <see cref="Scene"/> for frustum culling and other optimizations.
  /// Be aware that the bounding shape of a scene node is <strong>not</strong> a hierarchical 
  /// bounding shape. It defines only the bounds of the current node. The bounding shape does 
  /// <strong>not</strong> include the bounds of the children!
  /// </para>
  /// <para>
  /// The <see cref="Shape"/> can be set to <see cref="Geometry.Shapes.Shape.Infinite"/>. In this 
  /// case the scene node is never culled during frustum culling and is always visible.
  /// </para>
  /// <para>
  /// Some scene nodes have an <see cref="Geometry.Shapes.Shape.Empty"/> bounding shape. These 
  /// scene nodes are ignored in scene queries. I.e. they do not show up in the query results!
  /// Newly created scene nodes have an <see cref="Geometry.Shapes.Shape.Empty"/> bounding shape.
  /// The <see cref="Scene"/> and <see cref="ModelNode"/> also have an 
  /// <see cref="Geometry.Shapes.Shape.Empty"/> shape. (These types of nodes are just used to group 
  /// other nodes in the scene graph. They can be ignored during rendering.)
  /// </para>
  /// <para>
  /// The root of a scene hierarchy is usually the <see cref="Scene"/>. However, 
  /// <see cref="SceneNode"/> objects can also be used independently from a <see cref="Scene"/> 
  /// object. For example, when a <see cref="ModelNode"/> is loaded the model hierarchy is also 
  /// defined as a tree of scene nodes. <see cref="ModelNode"/>s can be placed inside a 
  /// <see cref="Scene"/>, but they can also be used and rendered independently from a 
  /// <see cref="Scene"/> object.
  /// </para>
  /// <para>
  /// <strong>Tree Traversal:</strong><br/>
  /// <see cref="SceneHelper"/> provides various helper methods to traverse the scene tree using 
  /// LINQ: See 
  /// <see cref="SceneHelper.GetRoot"/>, 
  /// <see cref="SceneHelper.GetAncestors"/>, 
  /// <see cref="SceneHelper.GetSelfAndAncestors"/>, 
  /// <see cref="SceneHelper.GetChildren(SceneNode)"/>, 
  /// <see cref="SceneHelper.GetDescendants(SceneNode)"/>, 
  /// <see cref="SceneHelper.GetSubtree(SceneNode)"/>, 
  /// <see cref="SceneHelper.GetLeaves"/>
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong><br/>
  /// Scene nodes are cloneable. <see cref="Clone"/> makes a copy of the current scene node and 
  /// recursively clones all children. The purpose of <see cref="Clone"/> is to replicate a single 
  /// scene node or an entire tree of scene nodes in a scene. For example, by repeatedly calling 
  /// <see cref="Clone"/> on a <see cref="MeshNode"/> multiple copies ("instances") of a 
  /// <see cref="Mesh"/> object can be placed within a scene.
  /// </para>
  /// <para>
  /// A scene node contains <i>instance data</i>, which is specific to a certain scene node object 
  /// and <i>shared data</i>, which can be shared by multiple scene nodes. For example, a 
  /// <see cref="MeshNode"/> contains <see cref="MeshNode.MaterialInstances"/>, which are unique for
  /// each mesh node, and a <see cref="MeshNode.Mesh"/>, which can be shared by multiple mesh nodes.
  /// By cloning a scene node all instance data is duplicated (deep copy), but the shared data is 
  /// only copied by reference (shallow copy). So, when a <see cref="MeshNode"/> is cloned, the 
  /// original and the cloned mesh node will have a unique set of material instances 
  /// (<see cref="MeshNode.MaterialInstances"/>), but both will share the same 
  /// <see cref="MeshNode.Mesh"/>.
  /// </para>
  /// <para>
  /// Any object stored in <see cref="UserData"/> is copied per reference (shallow copy). 
  /// <see cref="SceneData"/> and <see cref="RenderData"/> are never copied when the scene node
  /// is cloned.
  /// </para>
  /// <para>
  /// <strong>Scene Node Sorting:</strong><br/>
  /// In many cases scene nodes need to be sorted. For example: 
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// When rendering opaque objects the scene nodes need to be sorted front-to-back for efficient 
  /// rendering.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// When rendering transparent objects the scene nodes need to be sorted back-to-front for correct
  /// alpha transparency.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// When multiple light shine on an object, it can be helpful to sort the lights by their light
  /// contribution in order to identify the lights that should be rendered.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Etc.
  /// </description>
  /// </item>
  /// </list>
  /// The property <see cref="SortTag"/> can be used for sorting scene nodes. For example, when 
  /// sorting scene nodes by distance, the distance can be computed and stored in 
  /// <see cref="SortTag"/>.
  /// </para>
  /// <para>
  /// <strong>Scene Node Disposal (Potential Memory Leaks!)</strong><br/>
  /// Scene nodes implement <see cref="IDisposable"/>. The <see cref="Dispose(bool)"/> method should
  /// be called when the scene node is no longer needed. This is necessary in order to prevent 
  /// potential memory leaks. Once the method has been called, the scene node is no longer usable.
  /// Reusing a previously disposed scene node may result in undefined behavior!
  /// </para>
  /// <para>
  /// When calling <see cref="Dispose(bool)"/>, the parameter determines whether data objects (such 
  /// as vertex buffers, index buffers, etc.) are disposed or preserved. Disposing scene nodes 
  /// including data objects can be dangerous because resources might be shared and still used by 
  /// other scene nodes. It is therefore recommended to pass <see langword="false"/> as parameter - 
  /// unless it is certain that the resources are no longer needed.
  /// </para>
  /// <para>
  /// Any data stored in <see cref="RenderData"/>, <see cref="SceneData"/>, or 
  /// <see cref="UserData"/> is disposed together with the scene node.
  /// </para>
  /// </remarks>
  /// <seealso cref="IScene"/>
  /// <seealso cref="ISceneQuery"/>
  /// <seealso cref="Scene"/>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public partial class SceneNode : IGeometricObject, INamedObject, IDisposable
  {
    // Notes: The position has a local pose (relative to parent node) and a world pose.
    // Both poses can directly be manipulated by the user. The local pose is the dominant 
    // transformation: When a node is attached to another node, the currently stored world 
    // pose becomes invalid. World poses are calculated on demand, whereas the local poses 
    // are always up-to-date. When a world pose is set by the user the local pose is updated 
    // immediately.
    // 
    // When a world pose is changed, the local pose of this node and the world poses of 
    // all descendant nodes need to be updated.
    // When a local pose is changed, the world poses of this node and the world poses all 
    // descendant nodes needs to be updated.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private SceneNodeFlags _flags;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of this scene node.
    /// </summary>
    /// <value>The name of the scene node. The default value is <see langword="null"/>.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Common")]
#endif
    public string Name { get; set; }


    /// <summary>
    /// Gets the parent scene node.
    /// </summary>
    /// <value>The parent scene node.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Hierarchy")]
#endif
    public SceneNode Parent
    {
      get { return _parent; }
      internal set
      {
        Debug.Assert(value == null || _parent != value, "SceneNode.Parent should not be set when the new value is same as current.");

        // When a SceneNode was part of a Scene, we need to remove it from the Scene.
        // Raise SceneChanged event of old parent.
        if (_parent != null)
          _parent.OnSceneChanged(this, SceneChanges.NodeRemoved);

        var oldParent = _parent;
        _parent = value;
        OnParentChanged(oldParent, value);

        // Raise SceneChanged event of new parent.
        if (_parent != null)
          _parent.OnSceneChanged(this, SceneChanges.NodeAdded);

        // When the parent changes, all world poses need to be updated (this node and descendants).
        // (Invalidating this node and all descendants is costly. We don't need to update the 
        // subtree, if the world of the new parent is the same as of the old parent. However, since 
        // this case is very unlikely, we don't check for it.)
        Invalidate();
      }
    }
    private SceneNode _parent;


    /// <summary>
    /// Gets or sets the children of this scene node.
    /// </summary>
    /// <value>
    /// The collection of children of the scene node. The default value is <see langword="null"/> -
    /// see remarks. Null entries are not allowed in the children collection.
    /// </value>
    /// <remarks>
    /// <para>
    /// The property is <see langword="null"/> by default to minimize the memory consumption of the 
    /// scene node. If the value is <see langword="null"/>, a new <see cref="SceneNodeCollection"/>
    /// needs to be set before any scene nodes can be attached to the current node.
    /// </para>
    /// <example>
    /// The following example shows how to attach a scene node to another node.
    /// <code lang="csharp">
    /// <![CDATA[
    /// var node = new SceneNode();
    /// var childNode = new SceneNode();
    /// 
    /// // First initialize the Children collection. (The property is null by default.)
    /// node.Children = new SceneNodeCollection();
    /// 
    /// // Now attach the childNode to node.
    /// node.Children.Add(childNode);
    /// ]]>
    /// </code>
    /// </example>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
#if !PORTABLE && !NETFX_CORE
    [Category("Hierarchy")]
#endif
    public SceneNodeCollection Children
    {
      get { return _children; }
      set
      {
        if (_children == value)
          return;

        if (_children != null)
          _children.Parent = null;

        _children = value;

        if (_children != null)
          _children.Parent = this;
      }
    }
    private SceneNodeCollection _children;


    /// <summary>
    /// Gets a value indicating whether this scene node has been disposed of.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this scene node has been disposed of; otherwise, 
    /// <see langword="false"/>.
    /// </value>
#if !PORTABLE && !NETFX_CORE
    [Category("Common")]
#endif
    public bool IsDisposed
    {
      get { return GetFlag(SceneNodeFlags.IsDisposed); }
      private set { SetFlag(SceneNodeFlags.IsDisposed, value); }
    }


    /// <summary>
    /// Gets or sets a value indicating whether this scene node is dirty. (See remarks.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this scene node is dirty; otherwise, 
    /// <see langword="false"/>. The default value depends on the scene node type.
    /// </value>
    /// <remarks>
    /// The <see cref="IsDirty"/> flag is a general purpose flag that can be used in derived types
    /// to indicate that an update is required. The base class <see cref="SceneNode"/> automatically
    /// sets the flag when the <see cref="Pose"/> or <see cref="Shape"/> changes, but it does not
    /// use the flag otherwise.
    /// </remarks>
    internal bool IsDirty
    {
      get { return GetFlag(SceneNodeFlags.IsDirty); }
      set { SetFlag(SceneNodeFlags.IsDirty, value); }
    }


    /// <summary>
    /// Gets or sets a value indicating whether this scene node is enabled. (May override children - 
    /// see remarks.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this scene node is enabled; otherwise, <see langword="false"/>.
    /// The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The <see cref="IsEnabled"/> flag applies to the current scene node and its children: A scene
    /// node is only enabled, if its <see cref="IsEnabled"/> flag is <see langword="true"/> and
    /// all ancestors are enabled. When a parent node is disabled, all descendant nodes are 
    /// considered disabled!
    /// </para>
    /// <para>
    /// Disabled scene nodes are ignored in scene queries (see <see cref="ISceneQuery"/>) and do not
    /// show up in the query results.
    /// </para>
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Common")]
#endif
    public bool IsEnabled
    {
      get { return GetFlag(SceneNodeFlags.IsEnabled); }
      set
      {
        var oldFlags = _flags;
        SetFlag(SceneNodeFlags.IsEnabled, value);
        if (oldFlags != _flags)
          OnSceneChanged(this, SceneChanges.IsEnabledChanged);
      }
    }


    /// <summary>
    /// Gets a value indicating whether this scene node is actually enabled. (The method checks the 
    /// current scene node and its ancestors.)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this scene node and its ancestors are enabled; otherwise, 
    /// <see langword="false"/>.
    /// </value>
#if !PORTABLE && !NETFX_CORE
    [Category("Common")]
#endif
    public bool ActualIsEnabled
    {
      get
      {
        bool isEnabled = IsEnabled;
        var parent = Parent;
        while (isEnabled && parent != null)
        {
          isEnabled = parent.IsEnabled;
          parent = parent.Parent;
        }

        return isEnabled;
      }
    }


    /// <summary>
    /// Gets or sets a value indicating whether this scene node is static (immobile).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this scene node is static (immobile); otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Scene nodes can be marked as static, which enables certain effects and optimizations. Once a
    /// scene node is marked as static, its <see cref="PoseWorld"/> and <see cref="ScaleWorld"/> 
    /// should not be modified. It is possible to move static scene nodes, but it may a cause a 
    /// performance hit.
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Common")]
#endif
    public bool IsStatic
    {
      get { return GetFlag(SceneNodeFlags.IsStatic); }
      set { SetFlag(SceneNodeFlags.IsStatic, value); }
    }


    /// <summary>
    /// Gets or sets a value indicating whether this scene node can be rendered with a 
    /// <see cref="SceneNodeRenderer"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this scene node can be rendered; otherwise, 
    /// <see langword="false"/>. The default value depends on the scene node type.
    /// </value>
    /// <remarks>
    /// The property <see cref="IsRenderable"/> determines whether an object can be rendered. 
    /// Renderers should ignore the scene node if this property is <see langword="false"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public bool IsRenderable
    {
      get { return GetFlag(SceneNodeFlags.IsRenderable); }
      set { SetFlag(SceneNodeFlags.IsRenderable, value); }
    }


    /// <summary>
    /// Gets or sets a value indicating whether this scene node blocks the light and casts shadows.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this scene node casts shadows; otherwise, <see langword="false"/>.
    /// The default value depends on the scene node type.
    /// </value>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public bool CastsShadows
    {
      get { return GetFlag(SceneNodeFlags.CastsShadows); }
      set { SetFlag(SceneNodeFlags.CastsShadows, value); }
    }


    /// <summary>
    /// Gets or sets a value indicating whether the occlusion culling determined that this scene 
    /// node does not need to be rendered into the shadow map of the directional light.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this scene node does not need to be rendered into the shadow map
    /// of the directional light; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// The <see cref="OcclusionBuffer"/> can determine if shadow caster is in the shadow of another
    /// shadow caster. If this is the case, <see cref="IsShadowCasterCulled"/> is set to 
    /// <see langword="true"/> and the scene node does not need to be rendered into the shadow map.
    /// This is only valid for the directional light which set during occlusion culling.
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public bool IsShadowCasterCulled
    {
      get { return GetFlag(SceneNodeFlags.IsShadowCasterCulled); }
      set { SetFlag(SceneNodeFlags.IsShadowCasterCulled, value); }
    }


    /// <summary>
    /// Gets or sets a 16-bit value which can be used to store user-defined information or flags.
    /// </summary>
    /// <value>A 16-bit value containing user-defined information.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
#if !PORTABLE && !NETFX_CORE
    [Category("Misc")]
#endif
    public short UserFlags
    {
      get { return (short)((uint)_flags >> 16); }
      set { _flags = (SceneNodeFlags)(((uint)value << 16) | ((uint)_flags & 0x0000ffff)); }
    }


    /// <summary>
    /// Gets or sets the maximum distance up to which the scene node is rendered. (Needs to be
    /// normalized - see remarks.)
    /// </summary>
    /// <value>
    /// The <i>view-normalized</i> distance. The default value is 0, which means that distance
    /// culling is disabled and the scene node is visible from any distance.
    /// </value>
    /// <remarks>
    /// <para>
    /// The <see cref="MaxDistance"/> determines the distance up to which the scene node is visible.
    /// The value stored in this property is a <i>view-normalized distance</i> as described here:
    /// <see cref="GraphicsHelper.GetViewNormalizedDistance(SceneNode,CameraNode)"/>. The method
    /// <see cref="GraphicsHelper.GetViewNormalizedDistance(float, Matrix44F)"/> can be used to
    /// convert a distance to a view-normalized distance. The resulting value is independent of the
    /// current field-of-view and can be used for "distance culling".
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The <see cref="MaxDistance"/> only affects the current scene
    /// node. It does not affect the visibility of child nodes.
    /// </para>
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Level of detail")]
#endif
    public float MaxDistance { get; set; }


    /// <summary>
    /// Gets or sets the proxy node.
    /// </summary>
    /// <value>The proxy.</value>
    /// <remarks>
    /// <para>
    /// In certain cases scene nodes are referenced by other scene nodes, but they are not directly 
    /// registered in the scene. In such cases the scene node that is registered within the scene is
    /// called the <i>proxy node</i>. The scene node that is referenced by the proxy node is not 
    /// directly visible within the scene graph.
    /// </para>
    /// <para>
    /// Example: The <see cref="LodGroupNode"/> is a proxy node. It contains a scene node or tree of 
    /// scene nodes for each level of detail. Only the <see cref="LodGroupNode"/> is registered within 
    /// the scene. The individual levels of detail are referenced by the <see cref="LodGroupNode"/>, but
    /// they do not show up in the scene graph.
    /// </para>
    /// <para>
    /// Each scene node has a property <see cref="Proxy"/>. This property is usually automatically 
    /// set by the proxy node. 
    /// </para>
    /// <para>
    /// <strong>Scene Queries with Proxy Nodes:</strong><br/>
    /// When a scene query is executed, it always uses the proxy node if one is set. Scene nodes 
    /// that are only referenced are ignored by the scene query, only the proxy nodes are included 
    /// in the query results.
    /// </para>
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Hierarchy")]
#endif
    public SceneNode Proxy { get; set; }


    /// <summary>
    /// Gets or sets the number of the last frame in which the scene node was rendered.
    /// </summary>
    /// <value>The number of the frame in which the scene node was rendered.</value>
    /// <remarks>
    /// The property <see cref="LastFrame"/> can be used to determine when the scene node was 
    /// rendered the last time. Renderers need to update this property when they render the scene 
    /// node. The application logic can read this property and compare it with
    /// <see cref="IGraphicsService.Frame"/> to see if an object is visible and needs to be 
    /// updated in the next frame.
    /// </remarks>
    /// <seealso cref="IGraphicsService.Frame"/>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public int LastFrame { get; set; }


    /// <summary>
    /// Gets or sets the cached renderer data.
    /// </summary>
    /// <value>The cached renderer data.</value>
    /// <remarks>
    /// <para>
    /// This property is not used by the scene node itself; it is reserved for use by a renderer 
    /// that renders this scene node. The renderer can cache data in this property.
    /// </para>
    /// <para>
    /// The <see cref="RenderData"/> is <strong>not</strong> cloned or copied when the scene node is
    /// being cloned. If the object implements <see cref="IDisposable"/> it will be disposed
    /// together with the scene node.
    /// </para>
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public object RenderData { get; set; }


    /// <summary>
    /// Gets or sets scene data.
    /// </summary>
    /// <value>The scene data.</value>
    /// <remarks>
    /// <para>
    /// This property is not used by the scene node itself. It is reserved for a "scene" which 
    /// manages the scene nodes. In this property the scene can store additional data with the scene
    /// node; for example, culling information.
    /// </para>
    /// <para>
    /// The <see cref="SceneData"/> is <strong>not</strong> cloned or copied when the scene node is
    /// being cloned. If the object implements <see cref="IDisposable"/> it will be disposed
    /// together with the scene node.
    /// </para>
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Misc")]
#endif
    public object SceneData { get; set; }


    /// <summary>
    /// Gets or sets the sort tag.
    /// </summary>
    /// <value>The sort tag.</value>
    /// <remarks>
    /// <para>
    /// This property that can be used for sorting scene nodes. For example, when sorting scene 
    /// nodes by distance, the distance can be computed and stored in <see cref="SortTag"/>. The 
    /// sorting should occur immediately after assigning the sort tag. (Many renderers of 
    /// DigitalRune Graphics internally also use the sort tag. Hence, the property may be 
    /// overwritten by the graphics engine.)
    /// </para>
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Misc")]
#endif
    public float SortTag { get; set; }


    /// <summary>
    /// Gets or sets user-defined data.
    /// </summary>
    /// <value>User-defined data.</value>
    /// <remarks>
    /// <para>
    /// This property is intended for application-specific data and is not used by the scene graph
    /// itself.
    /// </para>
    /// <para>
    /// Any object stored in <see cref="UserData"/> is copied per reference (shallow copy) when the
    /// scene node is cloned. If the object implements <see cref="IDisposable"/> it will be disposed
    /// together with the scene node.
    /// </para>
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Misc")]
#endif
    public object UserData { get; set; }


    /// <summary>
    /// Occurs when the local subtree changed.
    /// </summary>
    public event EventHandler<SceneChangedEventArgs> SceneChanged;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneNode"/> class.
    /// </summary>
    public SceneNode()
    {
      _parent = null;
      _flags = SceneNodeFlags.IsDirty | SceneNodeFlags.IsEnabled;
      LastFrame = -1;
      InitializeGeometricObject();
    }


    /// <summary>
    /// Releases all resources used by the scene node and all descendant nodes.
    /// </summary>
    /// <remarks>
    /// This method calls the <see cref="Dispose(bool)"/> method, passing in <see langword="true"/>
    /// to dispose scene nodes including data objects.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
    void IDisposable.Dispose()
    {
      // The explicit interface implementation is called by the XNA ContentManager 
      // to unload assets. The scene nodes including data objects (vertex buffers, 
      // index buffers, etc.) are disposed and no longer usable.
      Dispose(true);
    }


    /// <summary>
    /// Releases all resources used by the scene node and all descendant nodes.
    /// </summary>
    /// <param name="disposeData">
    /// <see langword="true"/> to dispose scene nodes including data objects; 
    /// <see langword="false"/> to dispose only scene nodes but preserve the data objects.
    /// </param>
    /// <remarks>
    /// <para>
    /// Scene nodes can share data objects. The parameter <paramref name="disposeData"/> determines
    /// whether data objects are disposed or preserved when the scene node is disposed. For example, 
    /// multiple <see cref="MeshNode"/>s can share the same <see cref="Mesh"/>. When a 
    /// <see cref="MeshNode"/> is disposed and <paramref name="disposeData"/> is 
    /// <see langword="true"/> the <see cref="Mesh"/> is disposed. All resources (vertex buffers, 
    /// index buffers, etc.) are released and the mesh is no longer usable.
    /// </para>
    /// <para>
    /// This method calls the virtual <see cref="Dispose(bool, bool)" /> method and then suppresses
    /// finalization of the instance.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
    public void Dispose(bool disposeData)
    {
      Dispose(true, disposeData);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="SceneNode" /> class
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and unmanaged resources;
    /// <see langword="false" /> to release only unmanaged resources.
    /// </param>
    /// <param name="disposeData">
    /// <see langword="true"/> to dispose scene nodes including data objects; 
    /// <see langword="false"/> to dispose only scene nodes but preserve the data objects.
    /// </param>
    /// <remarks>
    /// Scene nodes can share data objects. The parameter <paramref name="disposeData"/> determines
    /// whether data objects are disposed or preserved when the scene node is disposed. For example, 
    /// multiple <see cref="MeshNode"/>s can share the same <see cref="Mesh"/>. When a 
    /// <see cref="MeshNode"/> is disposed and <paramref name="disposeData"/> is 
    /// <see langword="true"/> the <see cref="Mesh"/> is disposed. All resources (vertex buffers, 
    /// index buffers, etc.) are released and the mesh is no longer usable.
    /// </remarks>
    protected virtual void Dispose(bool disposing, bool disposeData)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Unregister event handlers to prevent "memory leak".
          _shape.Changed -= OnShapeChanged;

          // Dispose additional data.
          RenderData.SafeDispose();
          SceneData.SafeDispose();
          UserData.SafeDispose();

          // Remove any view-dependent information from cameras.
          CameraNode.RemoveViewDependentData(this);

          if (_children != null)
            foreach (var child in _children)
              child.Dispose(disposeData);
        }

        IsDisposed = true;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    internal bool GetFlag(SceneNodeFlags flag)
    {
      return (_flags & flag) == flag;
    }


    internal SceneNodeFlags GetFlags(SceneNodeFlags flags)
    {
      return _flags & flags;
    }


    internal void ClearFlag(SceneNodeFlags flag)
    {
      _flags &= ~flag;
    }


    internal void SetFlag(SceneNodeFlags flag)
    {
      _flags |= flag;
    }


    internal void SetFlag(SceneNodeFlags flag, bool value)
    {
      if (value)
        SetFlag(flag);
      else
        ClearFlag(flag);
    }


    /// <summary>
    /// Called when <see cref="Parent"/> was changed.
    /// </summary>
    /// <param name="oldParent">The old parent.</param>
    /// <param name="newParent">The new parent.</param>
    protected virtual void OnParentChanged(SceneNode oldParent, SceneNode newParent)
    {
    }


    /// <summary>
    /// Calls <see cref="OnSceneChanged(SceneChangedEventArgs)"/>.
    /// </summary>
    /// <param name="sceneNode">The scene node that was added/removed/modified.</param>
    /// <param name="changes">The changes.</param>
    private void OnSceneChanged(SceneNode sceneNode, SceneChanges changes)
    {
      var args = SceneChangedEventArgs.Create(sceneNode, changes);
      OnSceneChanged(args);
      args.Recycle();
    }


    /// <summary>
    /// Raises the <see cref="SceneChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="SceneChangedEventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method is called when a change in the local subtree occurred. For example, when a scene
    /// node was added/removed, or a scene node's pose or shape changed.
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> When overriding 
    /// <see cref="OnSceneChanged(SceneChangedEventArgs)"/> in a derived class, be sure to call the
    /// base class's <see cref="OnSceneChanged(SceneChangedEventArgs)"/> method so that registered 
    /// delegates receive the event.
    /// </para>
    /// </remarks>
    protected virtual void OnSceneChanged(SceneChangedEventArgs eventArgs)
    {
      var handler = SceneChanged;

      if (handler != null)
        handler(this, eventArgs);

      if (_parent != null)
        _parent.OnSceneChanged(eventArgs);
    }
    #endregion
  }
}
