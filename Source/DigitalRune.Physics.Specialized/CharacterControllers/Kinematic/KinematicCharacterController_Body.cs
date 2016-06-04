// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Materials;


namespace DigitalRune.Physics.Specialized
{
  public partial class KinematicCharacterController
  {
    // In this file: All members related to the RigidBody of the CC.

    // The CC is represented by capsule-shaped rigid body. The capsule is always upright and does
    // not rotate. The position of the CC is the bottom of the capsule.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the body.
    /// </summary>
    /// <value>The body.</value>
    /// <remarks>
    /// <para>
    /// The body is automatically added to or removed from the <see cref="Simulation"/> when the 
    /// character is enabled/disabled (see <see cref="Enabled"/> ).
    /// </para>
    /// </remarks>
    public RigidBody Body { get; private set; }


    /// <summary>
    /// Gets or sets the collision group.
    /// </summary>
    /// <value>The collision group.</value>
    public int CollisionGroup
    {
      get { return Body.CollisionObject.CollisionGroup; }
      set { Body.CollisionObject.CollisionGroup = value; }
    }


    /// <summary>
    /// Gets the vector that points into the "up" direction.
    /// </summary>
    /// <value>The normalized up vector.</value>
    /// <remarks>
    /// This vector is normalized and defines the direction of the character capsule. Gravity will
    /// act against this direction.
    /// </remarks>
    public Vector3F UpVector { get; private set; }

    
    /// <summary>
    /// Gets or sets the height.
    /// </summary>
    /// <value>The height. The default is 1.8.</value>
    /// <remarks>
    /// This property assumes that the character's shape is a <see cref="CapsuleShape"/>.
    /// </remarks>
    public float Height
    {
      get { return ((CapsuleShape)Body.Shape).Height; }
      set
      {
        // To crouch the capsule height is changed to 1 m.
        var position = Position;
        ((CapsuleShape)Body.Shape).Height = value;

        // Changing the shape also changes the character position. We want the character to stay
        // on the ground. 
        Position = position;
      }
    }

    
    /// <summary>
    /// Gets or sets the width.
    /// </summary>
    /// <value>The width. The default is 0.8.</value>
    /// <remarks>
    /// This property assumes that the character's shape is a <see cref="CapsuleShape"/>.
    /// </remarks>
    public float Width
    {
      get { return ((CapsuleShape)Body.Shape).Radius * 2; }
      set { ((CapsuleShape)Body.Shape).Radius = value / 2; }
    }


    /// <summary>
    /// Gets or sets the position of the character.
    /// </summary>
    /// <value>The position of the character.</value>
    /// <remarks>
    /// The <see cref="Position"/> is the bottom position (the lowest point of the character's body).
    /// </remarks>
    public Vector3F Position
    {
      get
      {
        return Body.Pose.Position - Height / 2 * UpVector;
      }
      set
      {
        var pose = Body.Pose;
        pose.Position = value + Height / 2 * UpVector;
        Body.Pose = pose;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void InitializeBody(Vector3F upVector)
    {
      if (!upVector.TryNormalize())
        throw new ArgumentException("The up vector must not be a zero vector.");

      UpVector = upVector;

      CapsuleShape shape = new CapsuleShape(0.4f, 1.8f);
      MassFrame mass = new MassFrame { Mass = 100 };

      UniformMaterial material = new UniformMaterial
      {
        // The body should be frictionless, so that it can be easily pushed by the simulation to
        // valid positions. 
        StaticFriction = 0.0f,
        DynamicFriction = 0.0f,

        // The body should not bounce when being hit or pushed.
        Restitution = 0
      };

      Body = new RigidBody(shape, mass, material)
      {
        // We set the mass explicitly and it should not automatically change when the 
        // shape is changed; e.g. a ducked character has a smaller shape, but still the same mass.
        AutoUpdateMass = false,

        // This body is under our control and should never be deactivated by the simulation.
        CanSleep = false,
        CcdEnabled = true,

        // The capsule does not rotate in any direction.
        LockRotationX = true,
        LockRotationY = true,
        LockRotationZ = true,

        Name = "CharacterController",
        
        Pose = new Pose(shape.Height / 2 * upVector, 
                        QuaternionF.CreateRotation(Vector3F.UnitY, upVector)),
      };

      // When the user changes the shape, we must re-compute all contacts.
      Body.ShapeChanged += (s, e) => UpdateContacts();
    }
    #endregion
  }
}
