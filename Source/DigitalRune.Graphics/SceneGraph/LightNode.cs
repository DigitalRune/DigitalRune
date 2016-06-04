// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a light in a scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="LightNode"/> positions a <see cref="Light"/> in a 3D scene. A <see cref="Light"/>
  /// itself does not define position or direction. Position and orientation are defined by the 
  /// <see cref="LightNode"/>. Multiple <see cref="LightNode"/>s can reference the same 
  /// <see cref="Light"/> object.
  /// </para>
  /// <para>
  /// The position and orientation (light direction) is defined by setting either
  /// <see cref="SceneNode.PoseLocal"/> or <see cref="SceneNode.PoseWorld"/>. The light direction is 
  /// the local forward direction (0, 0, -1) of the scene node. The <see cref="SceneHelper"/> class 
  /// provides helper methods to direct the light at a certain target: see methods 
  /// <see cref="SceneHelper.LookAt(SceneNode,Vector3F,Vector3F)"/> and 
  /// <see cref="SceneHelper.LookAt(SceneNode,Vector3F,Vector3F,Vector3F)"/>.
  /// </para>
  /// <para>
  /// The <see cref="Shadow"/> property defines whether the light creates a shadow. The property is
  /// <see langword="null"/> by default and needs to be set explicitly.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="LightNode"/> is cloned the <see cref="Light"/>
  /// is not cloned. The <see cref="Light"/> is copied by reference (shallow copy). The original 
  /// <see cref="LightNode"/> and the cloned <see cref="LightNode"/> will reference the same
  /// <see cref="Graphics.Light"/> object. The <see cref="Clip"/> geometry is copied by reference.
  /// The <see cref="Shadow"/> data is cloned.
  /// </para>
  /// </remarks>
  public class LightNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the light.
    /// </summary>
    /// <value>The light.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Light Light
    {
      get { return _light; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _light = value;
        Shape = value.Shape;
      }
    }
    private Light _light;


    /// <summary>
    /// Gets or sets the clip geometry.
    /// </summary>
    /// <value>The clip geometry. The default value is <see langword="null"/> (no clipping).</value>
    /// <remarks>
    /// <para>
    /// A clip geometry is an <see cref="IGeometricObject"/> (e.g. a <see cref="GeometricObject"/>
    /// or another <see cref="SceneNode"/>) which defines the volume that can be lit by this light.
    /// Objects outside the clip geometry are not lit. This can be used to avoid that a light inside
    /// a room illuminates objects in the neighbor room, without using shadow mapping.
    /// </para>
    /// <para>
    /// Per default, the light affects only objects inside the clip geometry. If
    /// <see cref="InvertClip"/> is set, the light affects only objects outside the clip geometry.
    /// </para>
    /// <para>
    /// The clip geometry is positioned in world space. Its shape is typically a simple shape, like
    /// a box or a composite shape containing several boxes. But the shape can also be a triangle
    /// mesh.
    /// </para>
    /// <para>
    /// The clip geometry can be shared by several lights. For example, all lights in a room can
    /// reference one clip geometry that describes the interior of the room.
    /// </para>
    /// <para>
    /// When the <see cref="LightNode"/> is cloned, <see cref="Clip"/> is copied by reference!
    /// </para>
    /// </remarks>
    public IGeometricObject Clip
    {
      get { return _clip; }
      set
      {
        if (Clip == value)
          return;

        if (_clip != null)
        {
          Clip.PoseChanged -= OnClipChanged;
          Clip.ShapeChanged -= OnClipChanged;
        }

        _clip = value;
        OnClipChanged(null, null);

        if (_clip != null)
        {
          Clip.PoseChanged += OnClipChanged;
          Clip.ShapeChanged += OnClipChanged;
        }
      }
    }
    private IGeometricObject _clip;


    /// <summary>
    /// Gets or sets a value indicating whether <see cref="Clip"/> geometry determines the volume
    /// that can be lit or the volume that should be ignored by the light.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if objects in the <see cref="Clip"/> are not lit; otherwise, 
    /// <see langword="false" /> if only objects in the <see cref="Clip"/> geometry are lit.
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool InvertClip
    {
      get { return GetFlag(SceneNodeFlags.InvertClip); }
      set { SetFlag(SceneNodeFlags.InvertClip, value); }
    }


    /// <summary>
    /// Gets or sets the shadow data of this light node.
    /// </summary>
    /// <value>
    /// The shadow data. The default value is <see langword="null"/> (no shadow).
    /// </value>
    /// <remarks>
    /// <para>
    /// If this property is <see langword="null"/>, this light node does not have a shadow.To enable
    /// shadows for this light node, <see cref="Shadow"/> must be set. 
    /// </para>
    /// <para>
    /// The <see cref="Shadow"/> type (e.g. <see cref="StandardShadow"/>, 
    /// <see cref="CascadedShadow"/> or <see cref="CubeMapShadow"/>) must fit the light type. 
    /// For instance, a <see cref="DirectionalLight"/> cannot have a <see cref="CubeMapShadow"/>. 
    /// </para>
    /// <para>
    /// <see cref="Shadow"/> instances must not be shared between <see cref="LightNode"/>s. If 
    /// several <see cref="LightNode"/>s reference the same <see cref="Shadow"/> object, the last 
    /// rendered light will overwrite the shadow maps of the other lights.
    /// </para>
    /// </remarks>
    public Shadow Shadow { get; set; }


    /// <summary>
    /// Gets or sets the priority.
    /// </summary>
    /// <value>The priority. The default value is 0.</value>
    /// <remarks>
    /// If the rendering system cannot render an arbitrary number of lights, then it might skip 
    /// lights with a lower priority.
    /// </remarks>
    public int Priority { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="LightNode"/> class.
    /// </summary>
    /// <param name="light">The light.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="light"/> is <see langword="null"/>.
    /// </exception>
    public LightNode(Light light)
    {
      if (light == null)
        throw new ArgumentNullException("light");

      _light = light;
      Shape = light.Shape;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new LightNode Clone()
    {
      return (LightNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new LightNode(Light);
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone LightNode properties.
      var sourceTyped = (LightNode)source;
      Clip = sourceTyped.Clip;
      InvertClip = sourceTyped.InvertClip;
      Priority = sourceTyped.Priority;
      if (sourceTyped.Shadow != null)
        Shadow = sourceTyped.Shadow.Clone();
    }
    #endregion


    private void OnClipChanged(object sender, EventArgs eventArgs)
    {
      RenderData.SafeDispose();
    }
    #endregion
  }
}
