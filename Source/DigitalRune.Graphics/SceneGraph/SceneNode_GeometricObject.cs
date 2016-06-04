// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  partial class SceneNode
  {
    // Notes:
    // PoseWorld is updated lazy. ScaleWorld is always updated immediately because 
    // if it has changed, we must trigger BoundingShapeChanged.

    // Scaling:
    // Parent nodes scale their child nodes. If a child node is rotated, then this 
    // could create a shearing. - This is not allowed because our collision algorithms 
    // do not support sheared primitives (sheared box, sheared sphere, ...). Therefore, 
    // we combine scales under the assumption that no shearing occurs.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the axis-aligned bounding box (AABB) in world space.
    /// </summary>
    /// <value>The axis-aligned bounding box (AABB) in world space.</value>
    /// <remarks>
    /// The AABB is automatically determined based on <see cref="Shape"/> and 
    /// <see cref="PoseWorld"/>.
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Geometry")]
#endif
    public Aabb Aabb
    {
      get
      {
        if (GetFlag(SceneNodeFlags.IsAabbDirty))
        {
          _aabb = _shape.GetAabb(ScaleWorld, PoseWorld);
          ClearFlag(SceneNodeFlags.IsAabbDirty);
        }

        return _aabb;
      }
    }
    private Aabb _aabb;


    /// <summary>
    /// Gets the total effective scale (which incorporates the scale factors of parent scene nodes).
    /// </summary>
    /// <value>
    /// The total effective scale (which incorporates the scale factors of parent scene nodes).
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    Vector3F IGeometricObject.Scale
    {
      get { return ScaleWorld; }
    }


    /// <summary>
    /// Gets the pose (position and orientation) in world space.
    /// </summary>
    /// <value>The pose (position and orientation) in world space.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    Pose IGeometricObject.Pose
    {
      get {  return PoseWorld; }
    }


    /// <summary>
    /// Gets the total effective scale (which incorporates the scale factors of parent scene nodes).
    /// </summary>
    /// <value>
    /// The total effective scale (which incorporates the scale factors of parent scene nodes).
    /// </value>
#if !PORTABLE && !NETFX_CORE
    [Category("Geometry")]
#endif
    public Vector3F ScaleWorld
    {
      get
      {
        if (GetFlag(SceneNodeFlags.IsScaleWorldDirty))
          UpdateScaleWorldFromLocal();

        return _scaleWorld;
      }
    }
    private Vector3F _scaleWorld;


    /// <summary>
    /// Gets or sets the pose (position and orientation) in world space.
    /// </summary>
    /// <value>The pose (position and orientation) in world space.</value>
    /// <remarks>
    /// <para>
    /// Changing this property raises the <see cref="IGeometricObject.PoseChanged"/> event.
    /// </para>
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Geometry")]
#endif
    public Pose PoseWorld
    {
      get
      {
        if (GetFlag(SceneNodeFlags.IsPoseWorldDirty))
          UpdatePoseWorldFromLocal();

        return _poseWorld;
      }
      set
      {
        if (GetFlag(SceneNodeFlags.IsPoseWorldDirty) || _poseWorld != value)
        {
          _poseWorld = value;
          ClearFlag(SceneNodeFlags.IsPoseWorldDirty);
          UpdatePoseLocalFromWorld();

          // Rotating the object modifies ScaleWorld if the parent contains a scaling!
          UpdateScaleWorldFromLocal();

          InvalidateChildren();
          OnPoseChanged(EventArgs.Empty);
        }
      }
    }
    private Pose _poseWorld;


    /// <summary>
    /// Gets or sets the scale relative to the parent scene node. 
    /// </summary>
    /// <value>The scale relative to the parent scene node.</value>
    /// <remarks>
    /// <para>
    /// All scale factors should be positive. Zero or negative scale factors can lead to unexpected
    /// results.
    /// </para>
    /// <para>
    /// Changing this property raises the <see cref="IGeometricObject.ShapeChanged"/> event.
    /// </para>
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Geometry")]
#endif
    public Vector3F ScaleLocal
    {
      get { return _scaleLocal; }
      set
      {
        if (_scaleLocal != value)
        {
          _scaleLocal = value;
          UpdateScaleWorldFromLocal();  // This calls OnShapeChanged if required.
          InvalidateChildren();
        }
      }
    }
    private Vector3F _scaleLocal;


    /// <summary>
    /// Gets or sets the pose (position and orientation) relative to the parent scene node.
    /// </summary>
    /// <value>The pose (position and orientation) relative to the parent scene node.</value>
    /// <remarks>
    /// <para>
    /// Changing this property raises the <see cref="IGeometricObject.PoseChanged"/> event.
    /// </para>
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Geometry")]
#endif
    public Pose PoseLocal
    {
      get { return _poseLocal; }
      set
      {
        if (_poseLocal != value)
        {
          _poseLocal = value;
          Invalidate();

          // (We do not need to call OnPoseChanged here.
          // It will be called automatically in Invalidate() above.)
        }
      }
    }
    private Pose _poseLocal;


    /// <summary>
    /// Gets or sets the <see cref="ScaleWorld"/> of the last frame.
    /// </summary>
    /// <value>
    /// <see cref="ScaleWorld"/> of the last frame in world space. Can be <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// <see cref="LastScaleWorld"/> and <see cref="LastPoseWorld"/> are optional properties. These 
    /// properties define the scene node's transformation of the last frame that was rendered. This 
    /// information is required by certain effects, such as object motion blur or camera motion 
    /// blur. 
    /// </para>
    /// <para>
    /// <strong>Important:</strong> These properties are not updated automatically! 
    /// <see cref="LastScaleWorld"/> and <see cref="LastPoseWorld"/> need to be set by the 
    /// application logic whenever the transformation of the scene node is changed.
    /// </para>
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Geometry")]
#endif
    public Vector3F? LastScaleWorld
    {
      get { return GetFlag(SceneNodeFlags.HasLastScaleWorld) ? _lastScaleWorld : (Vector3F?)null; }
      set
      {
        if (value.HasValue)
        {
          _lastScaleWorld = value.Value;
          SetFlag(SceneNodeFlags.HasLastScaleWorld);
        }
        else
        {
          ClearFlag(SceneNodeFlags.HasLastScaleWorld);
        }
      }
    }
    private Vector3F _lastScaleWorld;


    /// <summary>
    /// Gets or sets the <see cref="PoseWorld"/> of the last frame.
    /// </summary>
    /// <value>
    /// <see cref="PoseWorld"/> of the last frame in world space. Can be <see langword="null"/>.
    /// </value>
    /// <inheritdoc cref="LastScaleWorld"/>
#if !PORTABLE && !NETFX_CORE
    [Category("Geometry")]
#endif
    public Pose? LastPoseWorld
    {
      get { return GetFlag(SceneNodeFlags.HasLastPoseWorld) ? _lastPoseWorld : (Pose?)null; }
      set
      {
        if (value.HasValue)
        {
          _lastPoseWorld = value.Value;
          SetFlag(SceneNodeFlags.HasLastPoseWorld);
        }
        else
        {
          ClearFlag(SceneNodeFlags.HasLastPoseWorld);
        }
      }
    }
    private Pose _lastPoseWorld;


    /// <summary>
    /// Gets (or sets) the bounding shape of this scene node.
    /// </summary>
    /// <value>
    /// The bounding shape. The bounding shape contains only the current node - it does not include 
    /// the bounds of the children!
    /// </value>
    /// <remarks>
    /// <para>
    /// This property can be used as a bounding shape for frustum culling and similar operations. A 
    /// suitable bounding shape must be set manually. The default value is 
    /// <see cref="DigitalRune.Geometry.Shapes.Shape.Empty"/>.
    /// </para>
    /// <para>
    /// Changing this property raises the <see cref="IGeometricObject.ShapeChanged"/> event.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The <see cref="SceneNode"/> implements the interface 
    /// <see cref="IGeometricObject"/>. An <see cref="IGeometricObject"/> instance registers event 
    /// handlers for the <see cref="DigitalRune.Geometry.Shapes.Shape.Changed"/> event of the 
    /// contained <see cref="DigitalRune.Geometry.Shapes.Shape"/>. Therefore, a 
    /// <see cref="DigitalRune.Geometry.Shapes.Shape"/> will have an indirect reference to the 
    /// <see cref="IGeometricObject"/>. This is no problem if the geometric object exclusively owns 
    /// the shape. However, this could lead to problems ("life extension bugs" a.k.a. "memory 
    /// leaks") when multiple geometric objects share the same shape: One geometric object is no 
    /// longer used, but it cannot be collected by the garbage collector because the shape still 
    /// holds a reference to the object.
    /// </para>
    /// <para>
    /// Therefore, when <see cref="DigitalRune.Geometry.Shapes.Shape"/>s are shared between multiple 
    /// <see cref="IGeometricObject"/>s: Always set the shape to 
    /// <see cref="DigitalRune.Geometry.Shapes.Shape.Empty"/> or 
    /// <see cref="DigitalRune.Geometry.Shapes.Shape.Infinite"/> when the 
    /// <see cref="IGeometricObject"/> is no longer used. Those are special immutable shapes that 
    /// never raises any <see cref="DigitalRune.Geometry.Shapes.Shape.Changed"/> events. Setting 
    /// the shape to <see cref="DigitalRune.Geometry.Shapes.Shape.Empty"/> or 
    /// <see cref="DigitalRune.Geometry.Shapes.Shape.Infinite"/> ensures that the internal event 
    /// handlers are unregistered and the objects can be garbage-collected properly.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
#if !PORTABLE && !NETFX_CORE
    [Category("Geometry")]
#endif
    public Shape Shape
    {
      get { return _shape; }
      protected set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (_shape == value)
          return;

        _shape.Changed -= OnShapeChanged;
        _shape = value;
        _shape.Changed += OnShapeChanged;

        OnShapeChanged(ShapeChangedEventArgs.Empty);
      }
    }
    private Shape _shape;


    /// <summary>
    /// Occurs when the pose was changed.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    event EventHandler<EventArgs> IGeometricObject.PoseChanged
    {
      add { _poseChanged += value; }
      remove { _poseChanged -= value; }
    }
    private event EventHandler<EventArgs> _poseChanged;


    /// <summary>
    /// Occurs when the <see cref="Shape"/> or <see cref="ScaleWorld"/> was changed.
    /// </summary>
    public event EventHandler<ShapeChangedEventArgs> ShapeChanged;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    private void InitializeGeometricObject()
    {
      _poseWorld = Pose.Identity;
      _poseLocal = Pose.Identity;

      _scaleWorld = Vector3F.One;
      _scaleLocal = Vector3F.One;

      _shape = Shape.Empty;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Invalidates this scene node and all children.
    /// </summary>
    /// <remarks>
    /// When a scene node is invalid, its world pose needs to be recomputed when it is retrieved the
    /// next time.
    /// </remarks>
    public void Invalidate()
    {
      SetFlag(SceneNodeFlags.IsPoseWorldDirty);
      SetFlag(SceneNodeFlags.IsScaleWorldDirty);

      OnPoseChanged(EventArgs.Empty);
      UpdateScaleWorldFromLocal();  // This calls OnShapeChanged if required.

      InvalidateChildren();
    }


    /// <summary>
    /// Invalidates the children of this scene nodes.
    /// </summary>
    /// <remarks>
    /// When a scene node is invalid, its world pose needs to be recomputed when it is retrieved the
    /// next time.
    /// </remarks>
    private void InvalidateChildren()
    {
      if (_children != null)
        foreach (SceneNode child in _children)
          child.Invalidate();
    }


    /// <summary>
    /// Calculates the local pose based on the current world pose.
    /// </summary>
    private void UpdatePoseLocalFromWorld()
    {
      Debug.Assert(!GetFlag(SceneNodeFlags.IsPoseWorldDirty), "Cannot update local pose from world pose. World pose is invalid!");

      if (Parent != null)
      {
        _poseLocal = Parent.PoseWorld.Inverse * _poseWorld;
        _poseLocal.Position /= Parent.ScaleWorld;
      }
      else
      {
        _poseLocal = _poseWorld;
      }
    }


    /// <summary>
    /// Calculates the world pose based on the current local pose.
    /// </summary>
    private void UpdatePoseWorldFromLocal()
    {
      if (Parent != null)
      {
        if (Parent.ScaleWorld == Vector3F.One)
        {
          _poseWorld = Parent.PoseWorld * _poseLocal;
        }
        else
        {
          // Need to consider scale.
          Pose pose = _poseLocal;
          pose.Position *= Parent.ScaleWorld;
          _poseWorld = Parent.PoseWorld * pose;
        }
      }
      else
      {
        _poseWorld = _poseLocal;
        _scaleWorld = _scaleLocal;
      }

      ClearFlag(SceneNodeFlags.IsPoseWorldDirty);
    }


    private void UpdateScaleWorldFromLocal()
    {
      var oldScaleWorld = _scaleWorld;

      if (Parent != null)
      {
        Vector3F sParent = Parent.ScaleWorld;
        if (sParent.X == sParent.Y && sParent.Y == sParent.Z)
        {
          // ----- Uniform scaling
          _scaleWorld = sParent * _scaleLocal;
        }
        else
        {
          // ----- Non-uniform scaling

          // Compute ScaleWorld from Parent.ScaleWorld and ScaleLocal:
          // We are looking for a scale factor that can be applied in local space 
          // because this is the first transformation in the SRT order. 
          //
          //   Sw, Rw, Tw ... scale, rotation, translation of current node in world space.
          //   Sp, Rp, Tp ... scale, rotation, translation of parent node in world space.
          //   S, R, T ...... scale, rotation, translation of current node in local space.
          //
          //   Tw Rw Sw = Tp Rp Sp T R S
          //
          // We assume that the translation does not influence the scale, so we can 
          // set T = Identity.
          //
          //   Rw Sw = Rp Sp R S
          //   Sw = Rw^-1 Rp Sp R S
          //
          // Next, we assume that scale does not influence rotations. 
          //
          //   Rw = Rp R
          //   => Sw = R^-1 Rp^-1 Rp Sp R S
          //         = (R^-1 Sp R) S
          //
          // This means, if Sp applies a transformation in parent space ...
          //Matrix33F scaleParent = Matrix33F.CreateScale(sParent);

          // ... then (R^-1 Sp R) applies the same transformation in local space
          // (= similarity transformation).
          //scaleParent = _poseLocal.Orientation.Transposed * scaleParent * _poseLocal.Orientation;  // TODO: Optimize.

          // Assuming that the result is again a pure scaling matrix.
          //_scaleWorld.X = scaleParent.M00 * _scaleLocal.X;
          //_scaleWorld.Y = scaleParent.M11 * _scaleLocal.Y;
          //_scaleWorld.Z = scaleParent.M22 * _scaleLocal.Z;

          // ----- Optimized:
          Matrix33F o = _poseLocal.Orientation;
          _scaleWorld.X = (o.M00 * o.M00 * sParent.X + o.M10 * o.M10 * sParent.Y + o.M20 * o.M20 * sParent.Z) * _scaleLocal.X;
          _scaleWorld.Y = (o.M01 * o.M01 * sParent.X + o.M11 * o.M11 * sParent.Y + o.M21 * o.M21 * sParent.Z) * _scaleLocal.Y;
          _scaleWorld.Z = (o.M02 * o.M02 * sParent.X + o.M12 * o.M12 * sParent.Y + o.M22 * o.M22 * sParent.Z) * _scaleLocal.Z;
        }
      }
      else
      {
        _scaleWorld = _scaleLocal;
      }
      
      ClearFlag(SceneNodeFlags.IsScaleWorldDirty);
      
      // If the effective scale has changed, then we have to raise the ShapeChanged event.
      if (_scaleWorld != oldScaleWorld)
        OnShapeChanged(this, ShapeChangedEventArgs.Empty);
    }


    /// <summary>
    /// Raises the <see cref="IGeometricObject.PoseChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnPoseChanged(EventArgs)"/> 
    /// in a derived class, be sure to call the base class' <see cref="OnPoseChanged(EventArgs)"/> 
    /// method so that registered delegates receive the event.
    /// </remarks>
    protected virtual void OnPoseChanged(EventArgs eventArgs)
    {
      SetFlag(SceneNodeFlags.IsAabbDirty | SceneNodeFlags.IsDirty | SceneNodeFlags.IsDirtyScene);

      var handler = _poseChanged;
      if (handler != null)
        handler(this, eventArgs);

      OnSceneChanged(this, SceneChanges.PoseChanged);
    }


    /// <summary>
    /// Called when the shape stored in <see cref="Shape"/> has changed.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="ShapeChangedEventArgs"/> instance containing the event data.
    /// </param>
    private void OnShapeChanged(object sender, ShapeChangedEventArgs eventArgs)
    {
      OnShapeChanged(eventArgs);
    }


    /// <summary>
    /// Raises the <see cref="ShapeChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding 
    /// <see cref="OnShapeChanged(ShapeChangedEventArgs)"/> in a derived class, be sure to call the 
    /// base class' <see cref="OnShapeChanged(ShapeChangedEventArgs)"/> method so that registered 
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnShapeChanged(ShapeChangedEventArgs eventArgs)
    {
      SetFlag(SceneNodeFlags.IsAabbDirty | SceneNodeFlags.IsDirty | SceneNodeFlags.IsDirtyScene);

      var handler = ShapeChanged;
      if (handler != null)
        handler(this, eventArgs);

      OnSceneChanged(this, SceneChanges.ShapeChanged);
    }
    #endregion
  }
}
