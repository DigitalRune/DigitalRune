// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;

#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Particles
{
  partial class ParticleSystem
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public Aabb Aabb
    {
      get
      {
        if (_aabbIsValid == false)
        {
          _aabb = Shape.GetAabb(Vector3F.One, Pose);
          _aabbIsValid = true;
        }

        return _aabb;
      }
    }
    private Aabb _aabb;
    private bool _aabbIsValid;


    /// <summary>
    /// Gets or sets the pose (position and orientation) of the particle system relative to the
    /// <see cref="Parent"/> or the world.
    /// </summary>
    /// <value>The pose (position and orientation) relative to the <see cref="Parent"/>.</value>
    /// <remarks>
    /// <para>
    /// This property specifies the positions and orientation of the particle system, relative to 
    /// the <see cref="Parent"/> particle system. If <see cref="Parent"/> is <see langword="null"/>,
    /// the pose describes where the particle system is placed in world space.
    /// </para>
    /// <para>
    /// Changing this property raises the <see cref="PoseChanged"/> event.
    /// </para>
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public Pose Pose
    {
      get { return _pose; }
      set
      {
        if (_pose != value)
        {
          _pose = value;
          OnPoseChanged(EventArgs.Empty);
        }
      }
    }
    private Pose _pose;


    /// <summary>
    /// Gets or sets the bounding shape of the particle system.
    /// </summary>
    /// <value>
    /// The bounding shape. The default value is <see cref="Geometry.Shapes.Shape.Infinite"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property can be used as a bounding shape for frustum culling and similar operations. A 
    /// suitable bounding shape must be set manually. The default value is 
    /// <see cref="Geometry.Shapes.Shape.Infinite"/>.
    /// </para>
    /// <para>
    /// Changing this property raises the <see cref="ShapeChanged"/> event.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The <see cref="ParticleSystem"/> implements the interface 
    /// <see cref="IGeometricObject"/>. An <see cref="IGeometricObject"/> instance registers event 
    /// handlers for the <see cref="DigitalRune.Geometry.Shapes.Shape.Changed"/> event of the 
    /// contained <see cref="Shape"/>. Therefore, a <see cref="DigitalRune.Geometry.Shapes.Shape"/> 
    /// will have an indirect reference to the <see cref="IGeometricObject"/>. This is no problem if
    /// the geometric object exclusively owns the shape. However, this could lead to problems 
    /// ("life extension bugs" a.k.a. "memory leaks") when multiple geometric objects share the same
    /// shape: One geometric object is no longer used but it cannot be collected by the garbage 
    /// collector because the shape still holds a reference to the object.
    /// </para>
    /// <para>
    /// Therefore, when <see cref="DigitalRune.Geometry.Shapes.Shape"/>s are shared between multiple 
    /// <see cref="IGeometricObject"/>s: Always set the property <see cref="Shape"/> to 
    /// <see cref="DigitalRune.Geometry.Shapes.Shape.Empty"/> or
    /// <see cref="DigitalRune.Geometry.Shapes.Shape.Infinite"/> when the 
    /// <see cref="IGeometricObject"/> is no longer used. Those are special immutable shapes that 
    /// never raises any <see cref="DigitalRune.Geometry.Shapes.Shape.Changed"/> events. Setting 
    /// <see cref="Shape"/> to <see cref="DigitalRune.Geometry.Shapes.Shape.Empty"/> or 
    /// <see cref="DigitalRune.Geometry.Shapes.Shape.Infinite"/> ensures that the internal event 
    /// handlers are unregistered and the objects can be garbage-collected properly.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
#if XNA || MONOGAME
    [ContentSerializer(SharedResource = true, Optional = true)]
#endif
    public Shape Shape
    {
      get { return _shape; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");
        
        _shape.Changed -= OnShapeChanged;
        _shape = value;
        _shape.Changed += OnShapeChanged;
        OnShapeChanged(ShapeChangedEventArgs.Empty);
      }
    }
    private Shape _shape;


    /// <summary>
    /// Gets the scale. - Always returns (1, 1, 1).
    /// </summary>
    /// <value>Always returns (1, 1, 1).
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    Vector3F IGeometricObject.Scale
    {
      get { return Vector3F.One; }
    }


    /// <inheritdoc/>
    public event EventHandler<EventArgs> PoseChanged;


    /// <inheritdoc/>
    public event EventHandler<ShapeChangedEventArgs> ShapeChanged;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    private void InitializeGeometricObject()
    {
      _shape = Shape.Infinite;
      _pose = Pose.Identity;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnShapeChanged(object sender, ShapeChangedEventArgs eventArgs)
    {
      OnShapeChanged(eventArgs);
    }


    /// <summary>
    /// Raises the <see cref="PoseChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnPoseChanged(EventArgs)"/> 
    /// in a derived class, be sure to call the base class's <see cref="OnPoseChanged(EventArgs)"/> 
    /// method so that registered delegates receive the event.
    /// </remarks>
    protected virtual void OnPoseChanged(EventArgs eventArgs)
    {
      _aabbIsValid = false;

      var handler = PoseChanged;
      if (handler != null)
        handler(this, eventArgs);
    }


    /// <summary>
    /// Raises the <see cref="ShapeChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="ShapeChangedEventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnShapeChanged(ShapeChangedEventArgs)"/> 
    /// in a derived class, be sure to call the base class's <see cref="OnShapeChanged(ShapeChangedEventArgs)"/> 
    /// method so that registered delegates receive the event.
    /// </remarks>
    protected virtual void OnShapeChanged(ShapeChangedEventArgs eventArgs)
    {
      _aabbIsValid = false;

      var handler = ShapeChanged;
      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
