// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a camera in a scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="CameraNode"/> positions a <see cref="Camera"/> object in a 3D scene. The 
  /// <see cref="CameraNode"/> defines the view transformation, whereas the <see cref="Camera"/> 
  /// object defines the projection transformation and imaging properties. Multiple 
  /// <see cref="CameraNode"/>s can share the same <see cref="Camera"/> object. 
  /// </para>
  /// <para>
  /// The view transformation is defined by the following properties: 
  /// <see cref="SceneNode.PoseLocal"/>, <see cref="SceneNode.PoseWorld"/>, <see cref="View"/>, or
  /// <see cref="ViewInverse"/>. A new view transformation can be set in several ways:
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// By setting the property <see cref="SceneNode.PoseLocal"/> to define a position and orientation
  /// in a scene relative to a parent node.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// By setting the property <see cref="SceneNode.PoseWorld"/> to define an absolute position and 
  /// orientation in world space.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// By setting the property <see cref="View"/> or <see cref="ViewInverse"/> which sets the
  /// absolute transformation in world space.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// By calling the method <see cref="SceneHelper.LookAt(SceneNode,Vector3F,Vector3F)"/>.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// <strong>View Space (View Coordinate System):</strong> View space is the local coordinate 
  /// system of a camera. In view space x-axis points to the right, the y-axis points up, and the 
  /// z-axis points towards the viewer.
  /// </para>
  /// <para>
  /// <strong>View-Dependent Information:</strong> In special cases other objects need to store 
  /// information which is only valid for a certain view. This data can be stored in the 
  /// <see cref="ViewDependentData"/> dictionary. The method 
  /// <see cref="InvalidateViewDependentData()"/> can be called to reset any data in the dictionary.
  /// This is necessary when there is an abrupt change ("cut") in the scene.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="CameraNode"/> is cloned the 
  /// <see cref="Camera"/> is not duplicated. The <see cref="Camera"/> is copied by reference 
  /// (shallow copy). The original <see cref="CameraNode"/> and the cloned 
  /// <see cref="CameraNode"/> will reference the same <see cref="Graphics.Camera"/> object.
  /// </para>
  /// </remarks>
  /// <seealso cref="DigitalRune.Graphics.Camera"/>
  public class CameraNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>A (weak) list of all camera nodes.</summary>
    private static readonly WeakCollection<CameraNode> Instances = new WeakCollection<CameraNode>();
    private static readonly ReadOnlyWeakCollection<CameraNode> InstancesReadOnly = Instances.AsReadOnly();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the camera.
    /// </summary>
    /// <value>The camera.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Camera Camera
    {
      get { return _camera; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _camera = value;
        Shape = value.Projection.ViewVolume;
      }
    }
    private Camera _camera;


    /// <summary>
    /// Gets or sets the LOD bias of the camera.
    /// </summary>
    /// <value>The camera's LOD bias in the range [0, ∞[. The default value is 1.</value>
    /// <inheritdoc cref="RenderContext.LodBias"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float LodBias
    {
      get { return _lodBias; }
      set
      {
        if (!(value >= 0))
          throw new ArgumentOutOfRangeException("value", "The LOD bias must be in the range [0, ∞[");

        _lodBias = value;
      }
    }
    private float _lodBias = 1;


    /// <summary>
    /// Gets or sets the view matrix.
    /// </summary>
    /// <value>
    /// The view transformation in world space. The default value is 
    /// <see cref="Matrix44F.Identity"/>.
    /// </value>
    /// <remarks>
    /// Setting <see cref="View"/> automatically updates <see cref="ViewInverse"/>.
    /// </remarks>
    public Matrix44F View
    {
      get
      {
        if (IsDirty)
          UpdateView();

        return _view;
      }
      set
      {
        PoseWorld = Pose.FromMatrix(value).Inverse;
      }
    }
    private Matrix44F _view = Matrix44F.Identity;


    /// <summary>
    /// Gets or sets the inverse of the view matrix.
    /// </summary>
    /// <value>
    /// The inverse view transformation in world space. The default value is 
    /// <see cref="Matrix44F.Identity"/>.
    /// </value>
    /// <remarks>
    /// Setting <see cref="ViewInverse"/> automatically updates <see cref="View"/>.
    /// </remarks>
    public Matrix44F ViewInverse
    {
      get
      {
        if (IsDirty)
          UpdateView();

        return _viewInverse;
      }
      set
      {
        PoseWorld = Pose.FromMatrix(value);
      }
    }
    private Matrix44F _viewInverse = Matrix44F.Identity;


    /// <summary>
    /// Gets a dictionary that can be used to store view-dependent information.
    /// </summary>
    /// <value>The dictionary that stores view-dependent information.</value>
    /// <remarks>
    /// <para>
    /// The <see cref="ViewDependentData"/> dictionary can be used to store view-dependent 
    /// information with the camera node.
    /// </para>
    /// <para>
    /// When there is an abrupt change ("cut") in the scene the method 
    /// <see cref="InvalidateViewDependentData()"/> needs to be called to reset any view-dependent 
    /// information. For example, this can be necessary when a new level is loaded or the view 
    /// changes significantly from one frame to the next. This method disposes any data in the
    /// <see cref="ViewDependentData"/> dictionary that implements the interface 
    /// <see cref="IDisposable"/>.
    /// </para>
    /// </remarks>
    public Dictionary<object, object> ViewDependentData
    {
      get
      {
        if (_viewDependentData == null)
          _viewDependentData = new Dictionary<object, object>();

        return _viewDependentData;
      }
    }
    private Dictionary<object, object> _viewDependentData;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CameraNode"/> class.
    /// </summary>
    /// <param name="camera">The camera.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="camera"/> is <see langword="null"/>.
    /// </exception>
    public CameraNode(Camera camera)
    {
      if (camera == null)
        throw new ArgumentNullException("camera");

      _camera = camera;
      Shape = camera.Projection.ViewVolume;

      // Register camera node in global list.
      Instances.Add(this);
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing, bool disposeData)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          InvalidateViewDependentData();

          // Unregister camera node from global list.
          Instances.Remove(this);
        }

        base.Dispose(disposing, disposeData);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new CameraNode Clone()
    {
      return (CameraNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new CameraNode(Camera);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      base.CloneCore(source);

      var sourceTyped = (CameraNode)source;
      LodBias = sourceTyped.LodBias;
    }
    #endregion


    /// <summary>
    /// Gets a read-only collection of all camera node instances.
    /// </summary>
    /// <returns>A read-only collection of all camera node instances.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public static ReadOnlyWeakCollection<CameraNode> GetInstances()
    {
      return InstancesReadOnly;
    }


    /// <summary>
    /// Updates <see cref="View"/> and <see cref="ViewInverse"/> when the <see cref="Pose"/> has 
    /// changed.
    /// </summary>
    private void UpdateView()
    {
      _viewInverse = PoseWorld;
      _view = PoseWorld.Inverse;
      IsDirty = false;
    }


    /// <summary>
    /// Resets any view-dependent information in the <see cref="ViewDependentData"/> dictionary.
    /// </summary>
    /// <inheritdoc cref="ViewDependentData"/>
    public void InvalidateViewDependentData()
    {
      if (_viewDependentData == null || _viewDependentData.Count == 0)
        return;

      // Make temporary copy because dictionary may be modified during enumeration.
      var tempList = ResourcePools<object>.Lists.Obtain();
      foreach (var data in _viewDependentData.Values)
        tempList.Add(data);

      foreach (var data in tempList)
        data.SafeDispose();

      ResourcePools<object>.Lists.Recycle(tempList);
    }


    /// <summary>
    /// Invalidates the view-dependent data of the specified object in all <see cref="CameraNode"/>
    /// instances.
    /// </summary>
    /// <param name="key">
    /// The key that identifies the view-dependent information - usually the scene node that owns 
    /// data.
    /// </param>
    internal static void InvalidateViewDependentData(object key)
    {
      foreach (var cameraNode in Instances)
      {
        object data;
        if (cameraNode.ViewDependentData.TryGetValue(key, out data))
          data.SafeDispose();
      }
    }


    /// <summary>
    /// Removes the view-dependent information of the specified object from all 
    /// <see cref="CameraNode"/> instances.
    /// </summary>
    /// <param name="key">
    /// The key that identifies the view-dependent information - usually the scene node that owns 
    /// data.
    /// </param>
    internal static void RemoveViewDependentData(object key)
    {
      foreach (var cameraNode in Instances)
      {
        object data;
        if (cameraNode.ViewDependentData.TryGetValue(key, out data))
        {
          cameraNode.ViewDependentData.Remove(key);
          data.SafeDispose();
        }
      }
    }
    #endregion
  }
}
