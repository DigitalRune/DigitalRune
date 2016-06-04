// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Animation.Character;
using DigitalRune.Collections;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Constraints;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Represents a ragdoll of a 3D animated character.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A ragdoll represents a 3D animated character in the physics engine. To define a ragdoll you
  /// need several <see cref="Bodies"/> that represent the limbs of a character. In many cases,
  /// rigid bodies are only created for the important bones of the character skeleton. The rigid
  /// bodies are connected using constraints, e.g. <see cref="BallJoint"/>s, 
  /// <see cref="HingeJoint"/>s, etc. See property <see cref="Joints"/>. <see cref="Limits"/> are
  /// constraints that restrict the relative movement of limbs to avoid unrealistic poses.
  /// <see cref="Motors"/> can be used to control the ragdoll pose. 
  /// </para>
  /// <para>
  /// The <see cref="Ragdoll"/> class is a container for all the parts of a ragdoll (bodies, joints,
  /// limits, motors, etc.). When <see cref="AddToSimulation"/> is called, all relevant parts are
  /// added to the physics <see cref="Simulation"/> and the ragdoll simulation starts. 
  /// <see cref="RemoveFromSimulation"/> must be called to stop the ragdoll simulation and/or when
  /// the ragdoll is no longer needed. 
  /// </para>
  /// <para>
  /// <see cref="UpdateBodiesFromSkeleton(SkeletonPose)"/> takes a skeleton and moves the ragdoll
  /// bodies so that they match the skeleton pose. This method instantly moves ("teleports") the 
  /// bodies to their new positions. When this method is used, the bodies do not smoothly interact
  /// with other physics objects. This method is usually only used to initialize the rigid body
  /// positions when the ragdoll is added to the simulation.
  /// </para>
  /// <para>
  /// <see cref="UpdateSkeletonFromBodies"/> animates a <see cref="SkeletonPose"/> so that it
  /// matches the ragdoll posture. If a skeleton should be animated by the physics system, then this
  /// method must be called in each frame.
  /// </para>
  /// <para>
  /// <see cref="DriveToPose(SkeletonPose,float)"/> uses motors to control the movement of the 
  /// bodies. This method must be used if the ragdoll should interact with other physics objects, or
  /// if an animation should be blended with the physically-based movement.
  /// </para>
  /// <para>
  /// While a ragdoll is added to a simulation, it is not allowed to add or remove
  /// <see cref="Bodies"/>, <see cref="Joints"/>, <see cref="Limits"/> or <see cref="Motors"/>.
  /// </para>
  /// <para>
  /// <strong>Ragdoll creation:</strong><br/>
  /// The ragdoll does not contain helper methods for ragdoll creation. To create the ragdoll rigid
  /// bodies must be added to <see cref="Bodies"/>. The order of the rigid bodies is important
  /// because the index in this list determines with which skeleton bone the body will be 
  /// associated. This list can contain <see langword="null"/> entries (often bodies are only
  /// created for important bones). It is allowed that this list has less or more entries than the
  /// number of bones. Offsets can be added to <see cref="BodyOffsets"/>. The order of 
  /// <see cref="BodyOffsets"/> is the same as for <see cref="Bodies"/>. If no offsets are set the
  /// bodies are centered at the bone origins.
  /// </para>
  /// <para>
  /// Constraints that connect the rigid bodies should be added to the <see cref="Joints"/> list.
  /// The joints in this list can have any order. Typically, a <see cref="BallJoint"/> is created at
  /// each bone origin to connect the body of a bone with the body of the parent bone.
  /// </para>
  /// <para>
  /// Constraints that restrict the allowed relative body movement should be added to the
  /// <see cref="Limits"/> list. The limits in this list can have any order.
  /// </para>
  /// <para>
  /// Motors that control body movement should be added to <see cref="Motors"/>. The motors in this
  /// list can have any order.
  /// </para>
  /// <para>
  /// <strong>Ragdoll usage scenarios:</strong><br/>
  /// <list type="bullet">
  /// <item>
  /// <i>Collision detection only:</i> A ragdoll can be used to detect collisions with an animated
  /// character. In this scenario, <see cref="UpdateBodiesFromSkeleton(SkeletonPose)"/> 
  /// is called in each frame to set the rigid bodies to the pose of the skeleton. The physics 
  /// <see cref="Simulation"/> can be used to detect collision of other rigid bodies with the
  /// ragdoll.
  /// </item>
  /// <item>
  /// <i>Death animations:</i> The ragdoll is activated when the character is dead. The bodies are
  /// simulated to create a falling animation. <see cref="UpdateSkeletonFromBodies"/> is called in
  /// each frame. The physics simulation controls the skeleton animation.
  /// </item>
  /// <item>
  /// <i>Character can push other bodies:</i> The ragdoll bodies are 
  /// <see cref="MotionType.Kinematic"/>. Motors are used to move the rigid bodies to the skeleton
  /// position. In each frame <see cref="DriveToPose(SkeletonPose,float)"/> must be called. If the 
  /// rigid bodies collide with other obstacles, they move the obstacles. This is a one way 
  /// interaction - the ragdoll does not react to collisions with other objects.
  /// </item>
  /// <item>
  /// <i>Blending animation and physics:</i> The ragdoll is controlled by the simulation and
  /// constraint motors are used to drive the bodies to a target skeleton pose. This can be used to 
  /// let the ragdoll fall (simulated by the physics engine) and at the same time the character 
  /// tries to obtain a defensive posture. In this scenario, 
  /// <see cref="DriveToPose(SkeletonPose,float)"/> must be called to set the motor target position.
  /// <see cref="UpdateSkeletonFromBodies"/> must be called in each frame to update the skeleton of 
  /// the visible model. 
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  public partial class Ragdoll
  {
    // TODO:
    // - Is a Clone() method useful?
    // - Serialization
    // - Test position motors instead of rotation motors.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the pose (position and orientation) of the character in world space.
    /// </summary>
    /// <value>The pose (position and orientation) of the character in world space.</value>
    /// <remarks>
    /// <para>
    /// This pose is used in <see cref="UpdateBodiesFromSkeleton(SkeletonPose)"/> and 
    /// <see cref="UpdateSkeletonFromBodies"/>. A <see cref="SkeletonPose"/> is relative to model 
    /// space, but rigid bodies are always positioned relative to world space. Basically, the 
    /// <see cref="Pose"/> converts from model space to world space. It defines the offset of the 
    /// root bone/body.
    /// </para>
    /// <para>
    /// Changing the pose does not have an immediate effect. The new pose will be used in the next 
    /// <see cref="UpdateBodiesFromSkeleton(SkeletonPose)"/> or 
    /// <see cref="UpdateSkeletonFromBodies"/> call. The ragdoll only reads this value but will
    /// never modify it.
    /// </para>
    /// </remarks>
    public Pose Pose
    {
      get { return _pose; }
      set { _pose = value; }
    }
    private Pose _pose = Pose.Identity;


    /// <summary>
    /// Gets the simulation to which this ragdoll was added.
    /// </summary>
    /// <value>
    /// The simulation to which this ragdoll was added. Can be <see langword="null"/> if the
    /// ragdoll has not been added to a simulation yet.
    /// </value>
    public Simulation Simulation { get; private set; }


    /// <summary>
    /// Gets the rigid bodies that represent the ragdoll limbs.
    /// </summary>
    /// <value>The bodies. Per default this collection is empty.</value>
    /// <remarks>
    /// This collection can have any number of entries. It can contain <see langword="null"/> 
    /// entries. The order of the bodies in this collection is important. The index in this list 
    /// determines the bone index that will be used for the body.
    /// </remarks>
    public NotifyingCollection<RigidBody> Bodies { get; private set; }


    /// <summary>
    /// Gets the body offsets.
    /// </summary>
    /// <value>The body offsets. Per default this collection is empty.</value>
    /// <remarks>
    /// Each entry in this collection is associated with an entry in <see cref="Bodies"/>. The
    /// offset is a pose that converts from local body space to local bone space. If an offset is 
    /// <see cref="Geometry.Pose.Identity"/>, the associated body is centered at the bone origin. 
    /// </remarks>
    public NotifyingCollection<Pose> BodyOffsets { get; private set; }   // Body to bone transformation


    /// <summary>
    /// Gets the joints.
    /// </summary>
    /// <value>The joints. Per default this collection is empty.</value>
    public NotifyingCollection<Constraint> Joints { get; private set; }


    /// <summary>
    /// Gets the limits.
    /// </summary>
    /// <value>The limits. Per default this collection is empty.</value>
    public NotifyingCollection<Constraint> Limits { get; private set; }


    /// <summary>
    /// Gets the motors.
    /// </summary>
    /// <value>The motors. Per default this collection is empty.</value>
    public NotifyingCollection<RagdollMotor> Motors { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Ragdoll"/> class.
    /// </summary>
    public Ragdoll()
    {
      Bodies = new NotifyingCollection<RigidBody>(true, false);
      Bodies.CollectionChanged += OnBodiesChanged;

      BodyOffsets = new NotifyingCollection<Pose>(true, true);

      Joints = new NotifyingCollection<Constraint>(true, false);
      Joints.CollectionChanged += OnJointsChanged;

      Limits = new NotifyingCollection<Constraint>(true, false);
      Limits.CollectionChanged += OnLimitsChanged;

      Motors = new NotifyingCollection<RagdollMotor>(true, false);
      Motors.CollectionChanged += OnMotorsChanged;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnBodiesChanged(object sender, CollectionChangedEventArgs<RigidBody> eventArgs)
    {
      if (Simulation != null)
        throw new NotSupportedException("Changing the rigid bodies of a ragdoll while the ragdoll is added to a simulation is not supported.");
    }


    private void OnJointsChanged(object sender, CollectionChangedEventArgs<Constraint> eventArgs)
    {
      if (Simulation != null)
        throw new NotSupportedException("Changing the joints of a ragdoll while the ragdoll is added to a simulation is not supported.");
    }


    private void OnLimitsChanged(object sender, CollectionChangedEventArgs<Constraint> eventArgs)
    {
      if (Simulation != null)
        throw new NotSupportedException("Changing the limits of a ragdoll while the ragdoll is added to a simulation is not supported.");
    }


    private void OnMotorsChanged(object sender, CollectionChangedEventArgs<RagdollMotor> eventArgs)
    {
      if (Simulation != null)
        throw new NotSupportedException("Changing the motors of a ragdoll while the ragdoll is added to a simulation is not supported.");

      //----- Set/Unset RagdollMotor.Ragdoll.
      foreach (RagdollMotor motor in eventArgs.OldItems)
        if (motor != null)
          motor.Ragdoll = null;

      foreach (RagdollMotor motor in eventArgs.NewItems)
        if (motor != null)
          motor.Ragdoll = this;
    }


    /// <summary>
    /// Adds all parts of the ragdoll to a simulation. 
    /// </summary>
    /// <param name="simulation">The simulation.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="simulation"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The ragdoll cannot be added to the simulation because it has already been added to another 
    /// simulation.
    /// </exception>
    public void AddToSimulation(Simulation simulation)  // a.k.a. Start(), Enable()
    {
      if (simulation == null)
        throw new ArgumentNullException("simulation");

      if (Simulation != null)
      {
        if (simulation == Simulation)
          return;

        throw new InvalidOperationException("The ragdoll cannot be added to simulation because it was already added to another simulation.");
      }

      Simulation = simulation;

      foreach (var body in Bodies)
        if (body != null)
          simulation.RigidBodies.Add(body);

      foreach (var joint in Joints)
        if (joint != null)
          simulation.Constraints.Add(joint);

      foreach (var limit in Limits)
        if (limit != null)
          simulation.Constraints.Add(limit);

      foreach (var motor in Motors)
        if (motor != null)
          motor.AddToSimulation();
    }


    /// <summary>
    /// Removes all ragdoll parts from the simulation.
    /// </summary>
    public void RemoveFromSimulation()  // a.k.a. Stop(), Disable()
    {
      if (Simulation == null)
        return;

      foreach (var motor in Motors)
        if (motor != null)
          motor.RemoveFromSimulation();

      foreach (var limits in Limits)
        if (limits != null)
          Simulation.Constraints.Remove(limits);

      foreach (var joint in Joints)
        if (joint != null)
          Simulation.Constraints.Remove(joint);

      foreach (var body in Bodies)
        if (body != null)
          Simulation.RigidBodies.Remove(body);

      Simulation = null;
    }


    /// <summary>
    /// Enables all joints.
    /// </summary>
    public void EnableJoints()
    {
      foreach (var joint in Joints)
        if (joint != null)
          joint.Enabled = true;
    }


    /// <summary>
    /// Disables all joints.
    /// </summary>
    public void DisableJoints()
    {
      foreach (var joint in Joints)
        if (joint != null)
          joint.Enabled = false;
    }


    /// <summary>
    /// Enables all limits.
    /// </summary>
    public void EnableLimits()
    {
      foreach (var limit in Limits)
        if (limit != null)
          limit.Enabled = true;
    }


    /// <summary>
    /// Disables all limits.
    /// </summary>
    public void DisableLimits()
    {
      foreach (var limit in Limits)
        if (limit != null)
          limit.Enabled = false;
    }


    /// <summary>
    /// Enables all motors.
    /// </summary>
    public void EnableMotors()
    {
      foreach (var motor in Motors)
        if (motor != null)
          motor.Enabled = true;
    }


    /// <summary>
    /// Disables all motors.
    /// </summary>
    public void DisableMotors()
    {
      foreach (var motor in Motors)
        if (motor != null)
          motor.Enabled = false;
    }


    /// <overloads>
    /// <summary>
    /// Drives the ragdoll bodies to the target pose using the <see cref="Motors"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Drives the ragdoll bodies to the target pose using the <see cref="Motors"/>.
    /// </summary>
    /// <param name="skeletonPose">The target skeleton pose.</param>
    /// <param name="deltaTime">The current time step.</param>
    /// <remarks>
    /// This method controls the motors. If the ragdoll does not have any motors, this method does 
    /// nothing. The ragdoll bodies are not changed by this method. The bodies will move the next 
    /// time the simulation is updated (see method <see cref="Physics.Simulation.Update(TimeSpan)"/>)
    /// is called.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Ragdoll was not added to a simulation.
    /// </exception>
    public void DriveToPose(SkeletonPose skeletonPose, TimeSpan deltaTime)
    {
      DriveToPose(skeletonPose, (float)deltaTime.TotalSeconds);
    }


    /// <summary>
    /// Drives the ragdoll bodies to the target pose using the <see cref="Motors"/>.
    /// </summary>
    /// <param name="skeletonPose">The target skeleton pose.</param>
    /// <param name="deltaTime"> The time step (in seconds). See remarks.</param>
    /// <remarks>
    /// <para>
    /// This method controls the motors. If the ragdoll does not have any motors, this method does 
    /// nothing. The ragdoll bodies are not changed by this method. The bodies will move the next 
    /// time the simulation is updated (see method <see cref="Physics.Simulation.Update(TimeSpan)"/>)
    /// is called.
    /// </para>
    /// <para>
    /// The parameter <paramref name="deltaTime"/> is only necessary for velocity motors 
    /// (<see cref="RagdollMotorMode.Velocity"/>). The parameter must specify the time step size
    /// of the next physics update. 
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Ragdoll was not added to a simulation.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public void DriveToPose(SkeletonPose skeletonPose, float deltaTime)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");
      if (Simulation == null)
        throw new InvalidOperationException("Ragdoll was not added to a simulation. Call AddToSimulation() before calling DriveToPose().");

      foreach(var motor in Motors)
        if (motor != null)
          motor.DriveToPose(skeletonPose, deltaTime);
    }


    /// <summary>
    /// Updates the bone transforms of the skeleton pose, so that the bones match the ragdoll 
    /// bodies.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose that is modified.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose" /> is <see langword="null"/>.
    /// </exception>
    public void UpdateSkeletonFromBodies(SkeletonPose skeletonPose)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      var skeleton = skeletonPose.Skeleton;
      for (int i = 0; i < Bodies.Count && i < skeleton.NumberOfBones; i++)
      {
        var body = Bodies[i];
        if (body == null)
          continue;

        Pose offset = (i < BodyOffsets.Count) ? BodyOffsets[i] : Pose.Identity;
        Pose bonePoseAbsolute = Pose.Inverse * body.Pose * offset.Inverse;
        skeletonPose.SetBonePoseAbsolute(i, bonePoseAbsolute);
      }
    }


    /// <summary>
    /// Updates the poses of the bodies, so that the bodies match the bone transforms of the given 
    /// skeleton pose.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <remarks>
    /// The poses of the rigid bodies are changed instantly. The bodies will "teleport" instantly to
    /// the target positions. They will not interact correctly with other physics objects. The 
    /// velocities of the rigid bodies are set to zero. The bodies will be positioned relative to 
    /// the world space pose defined by <see cref="Pose"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose" /> is <see langword="null"/>.
    /// </exception>
    public void UpdateBodiesFromSkeleton(SkeletonPose skeletonPose)   // = Teleport of bodies.
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      var skeleton = skeletonPose.Skeleton;
      for (int i = 0; i < Bodies.Count && i < skeleton.NumberOfBones; i++)
      {
        var body = Bodies[i];
        if (body == null)
          continue;

        Pose offset = (i < BodyOffsets.Count) ? BodyOffsets[i] : Pose.Identity;
        Pose bodyPose = Pose * ((Pose)skeletonPose.GetBonePoseAbsolute(i)) * offset;

        body.Pose = bodyPose;
        body.LinearVelocity = Vector3F.Zero;
        body.AngularVelocity = Vector3F.Zero;
      }
    }


    /// <summary>
    /// Updates the pose of a single body, so that the bodies match the bone transforms of the given
    /// bone.
    /// </summary>
    /// <param name="skeletonPose">The skeleton pose.</param>
    /// <param name="boneIndex">The index of the bone.</param>
    /// <remarks>
    /// See also <see cref="UpdateBodiesFromSkeleton(SkeletonPose)"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="skeletonPose" /> is <see langword="null"/>.
    /// </exception>
    public void UpdateBodyFromSkeleton(SkeletonPose skeletonPose, int boneIndex)
    {
      if (skeletonPose == null)
        throw new ArgumentNullException("skeletonPose");

      if (boneIndex < 0 || boneIndex >= Bodies.Count)
        return;

      var body = Bodies[boneIndex];
      if (body == null)
        return;

      Pose offset = (boneIndex < BodyOffsets.Count) ? BodyOffsets[boneIndex] : Pose.Identity;
      Pose bodyPose = Pose * ((Pose)skeletonPose.GetBonePoseAbsolute(boneIndex)) * offset;

      body.Pose = bodyPose;
      body.LinearVelocity = Vector3F.Zero;
      body.AngularVelocity = Vector3F.Zero;
    }
    #endregion
  }
}
