// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Constraints;


namespace DigitalRune.Physics
{
  public partial class RigidBody
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the mass frame which defines the mass properties of this body.
    /// </summary>
    /// <value>
    /// The mass frame. Per default a mass frame is computed for the current <see cref="Shape"/>,
    /// <see cref="Scale"/> and a density of 1000.
    /// </value>
    /// <remarks>
    /// If the mass frame is modified, related properties, like <see cref="PoseCenterOfMass"/> are
    /// updated automatically.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public MassFrame MassFrame
    {
      get { return _massFrame; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (_massFrame != value)
        {
          _massFrame = value;
          WakeUp();
          UpdatePoseCenterOfMass();
          UpdateInverseMass();
        }
      }
    }
    private MassFrame _massFrame;


    /// <summary>
    /// Gets the effective inverse mass.
    /// </summary>
    /// <value>The effective inverse mass.</value>
    /// <remarks>
    /// <para>
    /// This property stores the inverse of the mass defined in the <see cref="MassFrame"/>. This
    /// value is modified by other simulation properties. For example, if the body is 
    /// <see cref="Physics.MotionType.Static"/> or <see cref="Physics.MotionType.Kinematic"/> this
    /// value is set to 0.
    /// </para>
    /// <para>
    /// This property is mostly only of interest if you are implementing new 
    /// <see cref="Constraint"/>s.
    /// </para>
    /// </remarks>
    public float MassInverse
    {
      get { return _massInverse; }
      private set { _massInverse = value; }
    }
    private float _massInverse;


    /// <summary>
    /// Gets the world space inertia matrix.
    /// </summary>
    /// <value>The world space inertia matrix.</value>
    internal Matrix33F InertiaWorld
    {
      get
      {
        Matrix33F orientationCOM = PoseCenterOfMass.Orientation;
        return orientationCOM * Matrix33F.CreateScale(MassFrame.Inertia) * orientationCOM.Transposed;
      }
    }


    /// <summary>
    /// Gets the effective inverse inertia.
    /// </summary>
    /// <value>The effective inverse inertia.</value>
    /// <remarks>
    /// <para>
    /// This property stores the inverse of the inertia matrix (defined in <see cref="MassFrame"/>)
    /// in world space. This value is modified by other simulation properties. For example, if the
    /// body is <see cref="Physics.MotionType.Static"/> or <see cref="Physics.MotionType.Kinematic"/>
    /// this value is set to 0. 
    /// </para>
    /// <para>
    /// This property is mostly only of interest if you are implementing new 
    /// <see cref="Constraint"/>s.
    /// </para>
    /// </remarks>
    public Matrix33F InertiaInverseWorld
    {
      get { return _inertiaInverseWorld; }
      private set { _inertiaInverseWorld = value; }
    }
    private Matrix33F _inertiaInverseWorld;


    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="MassFrame"/> is automatically updated
    /// if the <see cref="Shape"/> or <see cref="Scale"/> of this body is changed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the <see cref="MassFrame"/> is automatically updated when the 
    /// <see cref="Shape"/> or <see cref="Scale"/> is changed; otherwise, <see langword="false"/>.
    /// The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If the <see cref="Shape"/> or the <see cref="Scale"/> of a rigid body is changed, the mass
    /// properties should usually change too. If this property is <see langword="true"/>
    /// <see cref="MassFrame"/> is automatically updated. If the current <see cref="MassFrame"/> was
    /// computed for a certain density (see <see cref="Physics.MassFrame.FromShapeAndDensity"/>),
    /// the new <see cref="MassFrame"/> will use the same density. If the current 
    /// <see cref="MassFrame"/> was computed for a certain total target mass (see 
    /// <see cref="Physics.MassFrame.FromShapeAndMass"/>), the new <see cref="MassFrame"/> will use
    /// the same total target mass. (If the <see cref="MassFrame"/> is not set manually, rigid
    /// bodies use an automatic computed mass for a target density of 1000.) If this property is 
    /// <see langword="false"/>, the <see cref="MassFrame"/> is not adjusted when the shape or scale
    /// is changed.
    /// </para>
    /// </remarks>
    public bool AutoUpdateMass { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the body can rotate around its local mass frame 
    /// x-axis.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if rotations around the local x-axis (of the mass frame) are locked; 
    /// otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If this flag is set, forces and collision impacts will not rotate the body around the locked
    /// axis. The body can still be rotated by setting an angular velocity or by changing the 
    /// <see cref="Pose"/> (<see cref="PoseCenterOfMass"/>) of the body directly.
    /// </para>
    /// <para>
    /// The lock axis refers to an axis of the <see cref="MassFrame"/>. If 
    /// <see cref="MassFrame"/>.<see cref="Physics.MassFrame.Pose"/> does not contain a rotation,
    /// the lock axis is equal to the local coordinate axis of the body. If
    /// <see cref="MassFrame"/>.<see cref="Physics.MassFrame.Pose"/> contains a rotation, the lock
    /// axis is not identical to the local space axis of the body. In other words: The lock axes are
    /// the local axes of the <see cref="PoseCenterOfMass"/> and not the local axes of the 
    /// <see cref="Pose"/> of this body.
    /// </para>
    /// </remarks>
    public bool LockRotationX
    {
      get { return _lockRotationX; }
      set
      {
        _lockRotationX = value;
        UpdateInverseMass();
      }
    }
    private bool _lockRotationX;


    /// <summary>
    /// Gets or sets a value indicating whether the body can rotate around its local mass frame 
    /// y-axis.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if rotations around the local y-axis (of the mass frame) are locked; 
    /// otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If this flag is set, forces and collision impacts will not rotate the body around the locked
    /// axis. The body can still be rotated by setting an angular velocity or by changing the 
    /// <see cref="Pose"/> (<see cref="PoseCenterOfMass"/>) of the body directly.
    /// </para>
    /// <para>
    /// The lock axis refers to an axis of the <see cref="MassFrame"/>. If 
    /// <see cref="MassFrame"/>.<see cref="Physics.MassFrame.Pose"/> does not contain a rotation,
    /// the lock axis is equal to the local coordinate axis of the body. If
    /// <see cref="MassFrame"/>.<see cref="Physics.MassFrame.Pose"/> contains a rotation, the lock
    /// axis is not identical to the local space axis of the body. In other words: The lock axes are
    /// the local axes of the <see cref="PoseCenterOfMass"/> and not the local axes of the 
    /// <see cref="Pose"/> of this body.
    /// </para>
    /// </remarks>
    public bool LockRotationY
    {
      get { return _lockRotationY; }
      set
      {
        _lockRotationY = value;
        UpdateInverseMass();
      }
    }
    private bool _lockRotationY;


    /// <summary>
    /// Gets or sets a value indicating whether the body can rotate around its local mass frame 
    /// z-axis.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if rotations around the local z-axis (of the mass frame) are locked; 
    /// otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If this flag is set, forces and collision impacts will not rotate the body around the locked
    /// axis. The body can still be rotated by setting an angular velocity or by changing the 
    /// <see cref="Pose"/> (<see cref="PoseCenterOfMass"/>) of the body directly.
    /// </para>
    /// <para>
    /// The lock axis refers to an axis of the <see cref="MassFrame"/>. If 
    /// <see cref="MassFrame"/>.<see cref="Physics.MassFrame.Pose"/> does not contain a rotation,
    /// the lock axis is equal to the local coordinate axis of the body. If
    /// <see cref="MassFrame"/>.<see cref="Physics.MassFrame.Pose"/> contains a rotation, the lock
    /// axis is not identical to the local space axis of the body. In other words: The lock axes are
    /// the local axes of the <see cref="PoseCenterOfMass"/> and not the local axes of the 
    /// <see cref="Pose"/> of this body.
    /// </para>
    /// </remarks>
    public bool LockRotationZ
    {
      get { return _lockRotationZ; }
      set
      {
        _lockRotationZ = value;
        UpdateInverseMass();
      }
    }
    private bool _lockRotationZ;
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Updates the mass frame for a new shape and scale.
    /// </summary>
    private void UpdateMassFrame()
    {
      if (MassFrame == null)
      {
        MassFrame = MassFrame.FromShapeAndDensity(Shape, Scale, 1000, 0.01f, 3);
        return;
      }

      if (AutoUpdateMass)
      {
        // Compute mass for the same density as last time or the same target mass as last time.
        if (MassFrame.Density > 0)
          MassFrame = MassFrame.FromShapeAndDensity(Shape, Scale, MassFrame.Density, 0.01f, 3);
        else
          MassFrame = MassFrame.FromShapeAndMass(Shape, Scale, MassFrame.Mass, 0.01f, 3);
      }
    }


    /// <summary>
    /// Updates the cached inverse mass and inertia.
    /// </summary>
    private void UpdateInverseMass()
    {
      // TODO: Maybe set MassInverse to dirty and do lazy evaluation?

      MassInverse = (MotionType == MotionType.Dynamic) ? MassFrame.MassInverse : 0;
      Vector3F inertiaInverse = (MotionType == MotionType.Dynamic) ? MassFrame.InertiaInverse : Vector3F.Zero;

      if (LockRotationX)
        inertiaInverse.X = 0;
      if (LockRotationY)
        inertiaInverse.Y = 0;
      if (LockRotationZ)
        inertiaInverse.Z = 0;

      // TODO: make faster multiplication. Do not convert inertia vector to diagonal matrix with a lot of 0s.
      Matrix33F orientationCOM = _poseCenterOfMass.Orientation;
      InertiaInverseWorld = orientationCOM * Matrix33F.CreateScale(inertiaInverse) * orientationCOM.Transposed;
    }
    #endregion
  }
}
