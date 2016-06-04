using System;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Graphics
{
  /// <summary>
  /// References a scene node which is not in the scene graph.
  /// </summary>
  public class ProxyNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _ignoreChanges;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the scene node that is represented by this proxy node.
    /// </summary>
    /// <value>The scene node that is represented by this proxy node.</value>
    public SceneNode Node
    {
      get { return _node; }
      set
      {
        if (_node == value)
          return;

        SetNode(value);
        UpdateBoundingShape();
      }
    }
    private SceneNode _node;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyNode"/> class.
    /// </summary>
    /// <param name="node">The scene node.</param>
    public ProxyNode(SceneNode node)
    {
      IsRenderable = true;
      CastsShadows = true;
      Node = node;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new ProxyNode Clone()
    {
      return (ProxyNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new ProxyNode(null);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      base.CloneCore(source);

      var sourceTyped = (ProxyNode)source;
      SetNode((sourceTyped.Node != null) ? sourceTyped.Node.Clone() : null);
      Shape = sourceTyped.Shape.Clone();
    }
    #endregion


    // Sets the node without updating the bounding shape.
    private void SetNode(SceneNode node)
    {
      // Detach proxy.
      if (_node != null)
      {
        SetProxy(_node, null);
        _node.SceneChanged -= OnNodeSceneChanged;
        _node.PoseLocal = Pose.Identity;
        _node.ScaleLocal = Vector3F.One;
      }

      _node = node;

      // Attach proxy.
      if (_node != null)
      {
        SetProxy(_node, this);
        _node.PoseLocal = PoseWorld;
        _node.ScaleLocal = ScaleWorld;
        _node.SceneChanged += OnNodeSceneChanged;
      }
    }


    /// <inheritdoc/>
    protected override void OnPoseChanged(EventArgs eventArgs)
    {
      base.OnPoseChanged(eventArgs);

      if (_node != null)
      {
        _ignoreChanges = true;
        _node.PoseLocal = PoseWorld;
        _ignoreChanges = false;
      }
    }


    /// <inheritdoc/>
    protected override void OnShapeChanged(ShapeChangedEventArgs eventArgs)
    {
      base.OnShapeChanged(eventArgs);

      // (Remember: ShapeChanged is also triggered by scale changes.)
      if (_node != null)
      {
        _ignoreChanges = true;
        _node.ScaleLocal = ScaleWorld;
        _ignoreChanges = false;
      }
    }


    /// <summary>
    /// Called when the referenced node (or a node in its subtree) changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="SceneChangedEventArgs"/> instance containing the event data.
    /// </param>
    private void OnNodeSceneChanged(object sender, SceneChangedEventArgs eventArgs)
    {
      if (_ignoreChanges)
        return;

      switch (eventArgs.Changes)
      {
        case SceneChanges.NodeAdded:
          SetProxy(eventArgs.SceneNode, this);
          UpdateBoundingShape();
          break;
        case SceneChanges.NodeRemoved:
          SetProxy(eventArgs.SceneNode, null);
          UpdateBoundingShape();
          break;
        case SceneChanges.PoseChanged:
        case SceneChanges.ShapeChanged:
          UpdateBoundingShape();
          break;
      }
    }


    /// <summary>
    /// Sets the <see cref="SceneNode.Proxy"/> in all referenced nodes.
    /// </summary>
    /// <param name="referencedNode">The referenced node.</param>
    /// <param name="proxyNode">The proxy node.</param>
    private static void SetProxy(SceneNode referencedNode, SceneNode proxyNode)
    {
      Debug.Assert(referencedNode != null, "node must not be null.");

      referencedNode.Proxy = proxyNode;
      if (referencedNode.Children != null)
        foreach (var childNode in referencedNode.Children)
          SetProxy(childNode, proxyNode);
    }


    /// <summary>
    /// Updates the bounding shape of the proxy node.
    /// </summary>
    /// <remarks>
    /// The bounding shape needs to be large enough to contain all referenced nodes.
    /// </remarks>
    private void UpdateBoundingShape()
    {
      if (_node == null)
      {
        Shape = Shape.Empty;
        return;
      }

      // Traverse subtree and calculate AABB.
      Aabb? aabb = _node.GetSubtreeAabb();
      if (aabb.HasValue)
      {
        Vector3F extent = aabb.Value.Extent;
        if (float.IsInfinity(extent.X) || float.IsInfinity(extent.Y) || float.IsInfinity(extent.Z))
        {
          // The extent of the subtree is infinite in one or more dimensions.
          Shape = Shape.Infinite;
        }
        else
        {
          // The subtree has finite size.
          Shape = GetBoundingShape(aabb.Value);
        }
      }
      else
      {
        // The subtree is empty.
        Shape = Shape.Empty;
      }
    }


    /// <summary>
    /// Gets a bounding shape that matches the specified AABB.
    /// </summary>
    /// <param name="aabb">The AABB.</param>
    /// <returns>A box or transformed box that matches the specified AABB.</returns>
    private Shape GetBoundingShape(Aabb aabb)
    {
      // Get existing shape objects to avoid unnecessary memory allocation.
      BoxShape boxShape;
      GeometricObject geometricObject = null;
      TransformedShape transformedShape = null;
      if (Shape is BoxShape)
      {
        boxShape = (BoxShape)Shape;
      }
      else if (Shape is TransformedShape)
      {
        transformedShape = (TransformedShape)Shape;
        geometricObject = (GeometricObject)transformedShape.Child;
        boxShape = (BoxShape)geometricObject.Shape;
      }
      else
      {
        boxShape = new BoxShape();
      }

      // Make box the size of the AABB.
      boxShape.Extent = aabb.Extent;

      if (aabb.Center.IsNumericallyZero)
      {
        // Bounding box is centered at origin.
        return boxShape;
      }
      
      // Apply offset to bounding box.
      if (transformedShape == null)
      {
        geometricObject = new GeometricObject(boxShape, new Pose(aabb.Center));
        transformedShape = new TransformedShape(geometricObject);
      }
      else
      {
        geometricObject.Shape = boxShape;
        geometricObject.Pose = new Pose(aabb.Center);
      }

      return transformedShape;
    }
    #endregion
  }
}
