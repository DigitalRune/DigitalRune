// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a visual object with multiple levels of detail (LODs). 
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="LodGroupNode"/> contains a collection of scene nodes sorted by distance (see 
  /// property <see cref="Levels"/>). Each entry represents a <i>level of detail</i> (LOD). A LOD 
  /// may consist of a single scene node or an entire tree of scene nodes. The <see cref="Levels"/> 
  /// collection of LODs is empty by default. LODs can be added or removed during runtime.
  /// </para>
  /// <para>
  /// Pose (position and orientation) and scale of the individual levels of detail are synchronized 
  /// with the <see cref="LodGroupNode"/>. When a scene node is assigned to the 
  /// <see cref="LodGroupNode"/> its local pose and scale is overwritten!
  /// </para>
  /// <para>
  /// <strong>LOD0, LOD1, ... LOD<i>n-1</i>:</strong><br/>
  /// LODs can be accessed by index. The LOD with index <i>i</i> is called LOD<i>i</i>. By 
  /// definition, LOD0 is the highest level of detail and LOD<i>n-1</i> is the lowest level of 
  /// detail.
  /// </para>
  /// <para>
  /// <strong>Nested LOD Groups:</strong><br/>
  /// LOD groups can be nested. That means a LOD may contain another <see cref="LodGroupNode"/> in 
  /// its subtree.
  /// </para>
  /// <para>
  /// <strong>LOD Distances:</strong><br/>
  /// LODs are selected based on the current camera and the distance to the camera (see method
  /// <see cref="SelectLod"/>). The LOD distances stored in the <see cref="LodCollection"/> are 
  /// <i>view-normalized</i> distances. This means that distance values are corrected based on the 
  /// camera's field-of-view. The resulting LOD distances are independent of the current 
  /// field-of-view. See <see cref="GraphicsHelper.GetViewNormalizedDistance(float, Matrix44F)"/> 
  /// for more information.
  /// </para>
  /// <para>
  /// The camera that serves as reference for LOD distance computations needs to be stored in the 
  /// render context: See property <see cref="RenderContext.LodCameraNode"/>.
  /// </para>
  /// <para>
  /// In addition, the render context provides additional LOD settings: see properties 
  /// <see cref="RenderContext.LodBias"/>, <see cref="RenderContext.LodBlendingEnabled"/>,
  /// <see cref="RenderContext.LodHysteresis"/>.
  /// </para>
  /// <para>
  /// <strong>IMPORTANT:</strong><br/>
  /// To avoid unnecessary computations the LOD selection is updated only once per frame and camera.
  /// If the position or projection of the camera is modified during rendering, the method 
  /// <see cref="CameraNode.InvalidateViewDependentData()"/> needs to be called. Likewise, if LODs 
  /// are modified, added, or removed during rendering, 
  /// <see cref="CameraNode.InvalidateViewDependentData()"/> needs to be called.
  /// </para>
  /// <para>
  /// However, <see cref="CameraNode.InvalidateViewDependentData()"/> may affect other scene nodes. 
  /// It is therefore not recommended to modify the camera or the LOD node during rendering of a 
  /// frame! Only call <see cref="CameraNode.InvalidateViewDependentData()"/> if absolutely 
  /// necessary.
  /// </para>
  /// </remarks>
  /// <seealso cref="ISceneQuery"/>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class LodGroupNode : SceneNode, IOcclusionProxy
  {
    // Note: During initialization UpdateBoundingShape() is called every time a 
    // LOD is added. The performance impact should be minimal, therefore we ignore
    // this issue. Alternatively we could 
    //  - make Begin/EndUpdate() public, 
    //  - or return an IDisposable (struct) and use a using-statement to capture updates.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private const int DefaultCapacity = 4;
    private bool _suppressUpdates;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the collection that stores all levels of detail (LODs).
    /// </summary>
    /// <value>The levels of detail (LODs).</value>
    public LodCollection Levels { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="LodGroupNode"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="LodGroupNode"/> class with the default 
    /// capacity.
    /// </summary>
    public LodGroupNode()
      : this(DefaultCapacity)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="LodGroupNode"/> class.
    /// </summary>
    /// <param name="capacity">The initial capacity of the LOD collection.</param>
    public LodGroupNode(int capacity)
    {
      // The actual value of the following flags does not matter. The LodGroupNodes 
      // need be handled explicitly in scene queries!
      IsRenderable = true;
      CastsShadows = true;

      Levels = new LodCollection(this, capacity);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new LodGroupNode Clone()
    {
      return (LodGroupNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new LodGroupNode();
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      BeginUpdate();

      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone LodGroupNode properties.
      var sourceTyped = (LodGroupNode)source;

      Levels.Clear();
      foreach (var lod in sourceTyped.Levels)
        Levels.Add(lod.Distance, lod.Node.Clone());

      EndUpdate();
    }
    #endregion


    /// <summary>
    /// Notifies the <see cref="LodGroupNode" /> that a series of changes is going to start. During 
    /// these changes the <see cref="LodGroupNode" /> will not be synchronized with the LODs.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="BeginUpdate"/> is called before <see cref="EndUpdate"/>. Each call to
    /// <see cref="BeginUpdate"/> needs to be matched with call to <see cref="EndUpdate"/>!
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    internal void BeginUpdate()
    {
      if (_suppressUpdates)
        throw new InvalidOperationException(
          "Recursive updates are not supported. Each call to BeginUpdate() needs to be matched with "
          + "a call to EndUpdate()!");

      _suppressUpdates = true;
    }


    /// <summary>
    /// Notifies the <see cref="LodGroupNode"/> that a series of changes has ended. During these
    /// changes the <see cref="LodGroupNode"/> will not be synchronized with the LODs.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// <see cref="BeginUpdate"/> has not been called.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    internal void EndUpdate()
    {
      if (!_suppressUpdates)
        throw new InvalidOperationException(
          "Mismatched call of EndUpdate(). BeginUpdate() has not been called.");

      _suppressUpdates = false;
      UpdateBoundingShape();
    }


    /// <inheritdoc/>
    protected override void OnPoseChanged(EventArgs eventArgs)
    {
      base.OnPoseChanged(eventArgs);

      // Synchronize absolute pose.
      Levels.SetPose(PoseWorld);
    }


    /// <inheritdoc/>
    protected override void OnShapeChanged(ShapeChangedEventArgs eventArgs)
    {
      base.OnShapeChanged(eventArgs);

      // Synchronize absolute scale.
      // (Remember: ShapeChanged is also triggered by scale changes.)
      Levels.SetScale(ScaleWorld);
    }


    /// <summary>
    /// Updates the bounding shape of the <see cref="LodGroupNode"/>. (Called automatically by the 
    /// <see cref="LodCollection"/>.)
    /// </summary>
    /// <remarks>
    /// This method sets a <see cref="Shape"/> that is large enough to contain all LODs.
    /// </remarks>
    internal void UpdateBoundingShape()
    {
      if (_suppressUpdates)
        return;

      // Traverse LODs and calculate AABB.
      Aabb? aabb = null;
      bool isInfinite = false;
      Levels.SetPose(Pose.Identity);     // AABB has to be relative to local space!
      Levels.SetScale(Vector3F.One);     // AABB has to be relative to local space!
      foreach (var lod in Levels)
        isInfinite |= SceneHelper.GetSubtreeAabbInternal(lod.Node, ref aabb);
      Levels.SetPose(PoseWorld);
      Levels.SetScale(ScaleWorld);

      if (isInfinite)
      {
        // The extent of the subtree is infinite in one or more dimensions.
        Shape = Shape.Infinite;
      }
      else if (aabb.HasValue)
      {
        // The subtree has finite size.
        Shape = GetBoundingShape(aabb.Value);
      }
      else
      {
        // No LODs or LODs are empty.
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
      // The AABB of the LOD is real world size including scaling. We have to undo
      // the scale because this LodGroupNode also applies the same scale.
      var unscaledCenter = aabb.Center / ScaleWorld;
      var unscaledExtent = aabb.Extent / ScaleWorld;

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

      // Make bounding box the size of the unscaled AABB.
      boxShape.Extent = unscaledExtent;

      if (unscaledCenter.IsNumericallyZero)
      {
        // Bounding box is centered at origin.
        return boxShape;
      }

      // Apply offset to bounding box.
      if (transformedShape == null)
      {
        geometricObject = new GeometricObject(boxShape, new Pose(unscaledCenter));
        transformedShape = new TransformedShape(geometricObject);
      }
      else
      {
        geometricObject.Shape = boxShape;
        geometricObject.Pose = new Pose(unscaledCenter);
      }

      return transformedShape;
    }


    /// <summary>
    /// Gets the LOD or LOD transitions for the specified distance.
    /// </summary>
    /// <param name="context">The render context.</param>
    /// <param name="distance"> The view-normalized distance (including any LOD bias). </param>
    /// <returns>
    /// An <see cref="LodSelection" /> that describes the current LOD or LOD transition.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public LodSelection SelectLod(RenderContext context, float distance)
    {
      return Levels.SelectLod(context, distance);
    }


    #region ----- IOcclusionProxy -----

    private IOcclusionProxy GetOcclusionProxy()
    {
      return (Levels.Count > 0) ? Levels[0].Node as IOcclusionProxy : null;
    }


    /// <inheritdoc/>
    bool IOcclusionProxy.HasOccluder 
    {
      get
      {
        var occlusionProxy = GetOcclusionProxy();
        return (occlusionProxy != null) && occlusionProxy.HasOccluder;
      }
    }


    /// <inheritdoc/>
    void IOcclusionProxy.UpdateOccluder()
    {
      var occlusionProxy = GetOcclusionProxy();
      Debug.Assert(occlusionProxy != null, "Check IOcclusionProxy.HasOccluder before calling UpdateOccluder().");
      occlusionProxy.UpdateOccluder();
    }


    /// <inheritdoc/>
    OccluderData IOcclusionProxy.GetOccluder()
    {
      var occlusionProxy = GetOcclusionProxy();
      Debug.Assert(occlusionProxy != null, "Check IOcclusionProxy.HasOccluder before calling GetOccluder().");
      return occlusionProxy.GetOccluder();
    }
    #endregion

    #endregion
  }
}
