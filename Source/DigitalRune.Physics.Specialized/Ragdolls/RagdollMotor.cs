// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Animation;
using DigitalRune.Animation.Character;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Constraints;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Drives a body of a ragdoll to a target position.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Ragdolls without motors react to collisions, but are passive. Motors can be used to actively 
  /// move the limbs to a target position. 
  /// </para>
  /// <para>
  /// A ragdoll motor is either a velocity motor or a constraint motor, see <see cref="Mode"/>. See 
  /// the description of <see cref="RagdollMotorMode"/> for more information.
  /// </para>
  /// <para>
  /// A constraint motor controls the orientation of a bone relative to its parent bone. Usually,
  /// the parent of a bone can be determined using the <see cref="Skeleton"/> (see method 
  /// <see cref="Skeleton.GetParent"/>). But often a ragdoll does not have a rigid body for each
  /// bone of the skeleton - some bones are "skipped" for performance reasons. For a ragdoll motor
  /// the parent bone should be set to the bone to which the controlled bone is connected.
  /// </para>
  /// <para>
  /// <strong>Damping only:</strong><br/>
  /// Constraint motors with a positive <see cref="ConstraintDamping"/> value and a 
  /// <see cref="ConstraintSpring"/> of 0 can be used to create a passive, damped ragdoll. Damping 
  /// is very helpful to bring jittering ragdolls to rest.
  /// </para>
  /// </remarks>
  public class RagdollMotor
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    
    private readonly QuaternionMotor _quaternionMotor;
    #endregion
      
      
    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the ragdoll.
    /// </summary>
    /// <value>The ragdoll.</value>
    /// <remarks>
    /// This value is automatically set when the <see cref="RagdollMotor"/> is added to a 
    /// <see cref="Ragdoll"/>.
    /// </remarks>
    public Ragdoll Ragdoll
    {
      get { return _ragdoll; }
      internal set
      {
        if (_ragdoll == value)
          return;

        _ragdoll = value;

        RemoveFromSimulation();
        AddToSimulation();
      }
    }
    private Ragdoll _ragdoll;


    /// <summary>
    /// Gets or sets the index of the controlled bone.
    /// </summary>
    /// <value>The index of the controlled bone.</value>
    public int BoneIndex
    {
      get { return _boneIndex; }
      set
      {
        if (_boneIndex == value)
          return;

        _boneIndex = value;

        RemoveFromSimulation();
        AddToSimulation();
      }
    }
    private int _boneIndex;


    /// <summary>
    /// Gets or sets the index of the parent bone to which the controlled bone is connected.
    /// </summary>
    /// <value>The index of the parent bone to which the controlled bone is connected.</value>
    public int ParentIndex
    {
      get { return _parentIndex; }
      set
      {
        if (_parentIndex == value)
          return;

        _parentIndex = value;

        RemoveFromSimulation();
        AddToSimulation();
      }
    }
    private int _parentIndex;


    /// <summary>
    /// Gets or sets the motor mode.
    /// </summary>
    /// <value>The motor mode. The default value is <see cref="RagdollMotorMode.Velocity"/>.</value>
    public RagdollMotorMode Mode
    {
      get { return _mode; }
      set
      {
        if (_mode == value)
          return;

        _mode = value;

        RemoveFromSimulation();
        AddToSimulation();
      }
    }
    private RagdollMotorMode _mode;


    /// <summary>
    /// Gets or sets a value indicating whether this motor is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>.
    /// The default is <see langword="true"/>.
    /// </value>
    public bool Enabled
    {
      get { return _quaternionMotor.Enabled; }
      set { _quaternionMotor.Enabled = value; }
    }

    
    // TODO: Stress property = energy or impulse of obstacles to decide when the ragdoll has to fall.
    //public float Stress 
    //{ 
    //  get
    //  {
    //    return 0;
    //  }
    //}


    /// <summary>
    /// Gets or sets the spring constant of a constraint motor.
    /// </summary>
    /// <value>The spring constant of a constraint motor. The default value is 1e7.</value>
    /// <remarks>
    /// This property is not used by velocity motors.
    /// </remarks>
    public float ConstraintSpring
    {
      get { return _quaternionMotor.SpringConstant; }
      set { _quaternionMotor.SpringConstant = value; }
    }


    /// <summary>
    /// Gets or sets the damping constant of a constraint motor.
    /// </summary>
    /// <value>The damping constant of a constraint motor. The default value is 1e6.</value>
    /// <remarks>
    /// This property is not used by velocity motors.
    /// </remarks>
    public float ConstraintDamping
    {
      get { return _quaternionMotor.DampingConstant; }
      set { _quaternionMotor.DampingConstant = value; }
    }

    /// <summary>
    /// Gets or sets the maximal force that is applied by a constraint motor.
    /// </summary>
    /// <value>
    /// The maximal force of a constraint motor. The default value is +∞.
    /// </value>
    /// <remarks>
    /// This property defines the maximal force that the constraint motor can apply.
    /// This property is not used by velocity motors.
    /// </remarks>
    public float MaxConstraintForce
    {
      get { return _quaternionMotor.MaxForce; }
      set { _quaternionMotor.MaxForce = value; }
    }

    // TODO: VelocityFactor to lessen the strength of the velocity motor. 
    // If < 1 then interpolate desired velocity with actual velocity.
    //public float VelocityFactor { get; set; }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="RagdollMotor"/> class.
    /// </summary>
    /// <param name="boneIndex">The index of the controlled bone.</param>
    /// <param name="parentIndex">
    /// The index of the parent bone to which the controlled bone is connected.
    /// (Only relevant for constraint motors.)
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="boneIndex"/> is negative.
    /// </exception>
    public RagdollMotor(int boneIndex, int parentIndex)
    {
      if (boneIndex < 0)
        throw new ArgumentOutOfRangeException("boneIndex", "boneIndex must not be negative.");

      _boneIndex = boneIndex;
      _parentIndex = parentIndex;

      _quaternionMotor = new QuaternionMotor
      {
        DampingConstant = 1000000,
        SpringConstant = 10000000,
        MaxForce = float.PositiveInfinity,
        AnchorOrientationALocal = Matrix33F.Identity,
        AnchorOrientationBLocal = Matrix33F.Identity,

        // Single-axis mode is faster but less stable. In single-axis mode, 1 DOF (degree of 
        // freedom) constraint is applied along the rotation axis. If single-axis mode is turned 
        // off, a 3 DOF is applied: The bones rotate around the rotation axis and 2 more constraints
        // stabilize the bone in the 2 axes orthogonal to the rotation axis.
        UseSingleAxisMode = false,

      };
    }
    #endregion
      
      
    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    
    internal void AddToSimulation()
    {
      if (Ragdoll.Simulation == null)
      {
        // The ragdoll is not yet added to a simulation.
        return;
      }

      if (Mode == RagdollMotorMode.Velocity)
      {
        // Velocity motors don't need to be added to the simulation.
        return;
      }

      if (_quaternionMotor.Simulation != null)
      {
        // The constraint is already registered in simulation.
        return;
      }

      if (BoneIndex >= Ragdoll.Bodies.Count || ParentIndex < 0 || ParentIndex >= Ragdoll.Bodies.Count)
      {
        // Invalid bone index!
        return;
      }

      // Get bodies.
      var childBody = Ragdoll.Bodies[BoneIndex];
      var parentBody = Ragdoll.Bodies[ParentIndex];
      if (childBody == null || parentBody == null)
      {
        // No bodies?
        return; 
      }

      if (childBody.Simulation != Ragdoll.Simulation || parentBody.Simulation != Ragdoll.Simulation)
      {
        // Bodies are not in the simulation.
        return;
      }

      // Set bodies.
      _quaternionMotor.BodyA = parentBody;
      _quaternionMotor.BodyB = childBody;

      // Add to simulation.
      Ragdoll.Simulation.Constraints.Add(_quaternionMotor);
    }

    
    internal void RemoveFromSimulation()
    {
      if (_quaternionMotor.Simulation != null)
      {
        // Remove from simulation.
        Ragdoll.Simulation.Constraints.Remove(_quaternionMotor);

        // Reset bodies.
        _quaternionMotor.BodyA = null;
        _quaternionMotor.BodyB = null;
      }
    }


    /// <summary>
    /// Drives the controlled body.
    /// </summary>
    /// <param name="skeletonPose">The target skeleton pose.</param>
    /// <param name="deltaTime">The time step.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose" /> is <see langword="null"/>.
    /// </exception>
    internal void DriveToPose(SkeletonPose skeletonPose, float deltaTime)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      Debug.Assert(Ragdoll != null, "Motor must not be called when Ragdoll property is null.");
      Debug.Assert(Ragdoll.Simulation != null, "Ragdoll was not added to a simulation.");
      
      if (!Enabled)
        return;

      if (Mode == RagdollMotorMode.Velocity)
      {
        // ----- Velocity motor
        Debug.Assert(_quaternionMotor.Simulation == null, "Velocity motors should not be added to the simulation.");
        if (BoneIndex >= Ragdoll.Bodies.Count)
          return;

        var body = Ragdoll.Bodies[BoneIndex];
        if (body == null)
          return;

        var childOffset = (BoneIndex < Ragdoll.BodyOffsets.Count) ? Ragdoll.BodyOffsets[BoneIndex] : Pose.Identity;
        var childPose = Ragdoll.Pose * ((Pose)skeletonPose.GetBonePoseAbsolute(BoneIndex)) * childOffset;

        // Wake the body up. It should move, even if we move it very slowly.
        body.WakeUp();

        // Set velocities.
        body.LinearVelocity = AnimationHelper.ComputeLinearVelocity(body.Pose.Position, childPose.Position, deltaTime);
        body.AngularVelocity = AnimationHelper.ComputeAngularVelocity(body.Pose.Orientation, childPose.Orientation, deltaTime);
      }
      else
      {
        // ----- Constraint motor
        if (_quaternionMotor.Simulation == null)
        {
          // The motor was not added to the simulation. (Invalid configuration)
          return;
        }

        var parentOffset = (ParentIndex < Ragdoll.BodyOffsets.Count) ? Ragdoll.BodyOffsets[ParentIndex] : Pose.Identity;
        var parentPose = Ragdoll.Pose * ((Pose)skeletonPose.GetBonePoseAbsolute(ParentIndex)) * parentOffset;

        var childOffset = (BoneIndex < Ragdoll.BodyOffsets.Count) ? Ragdoll.BodyOffsets[BoneIndex] : Pose.Identity;
        var childPose = Ragdoll.Pose * ((Pose)skeletonPose.GetBonePoseAbsolute(BoneIndex)) * childOffset;

        // Set the relative motor target.
        var rotationMatrix = parentPose.Orientation.Transposed * childPose.Orientation;
        _quaternionMotor.TargetOrientation = QuaternionF.CreateRotation(rotationMatrix);
      }
    }
    #endregion
  }
}
