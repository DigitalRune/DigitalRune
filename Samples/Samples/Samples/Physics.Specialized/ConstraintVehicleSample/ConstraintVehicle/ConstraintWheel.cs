// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Physics.Specialized
{
  /// <summary>
  /// Represents a single wheel of a <see cref="ConstraintVehicle"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The wheel is modeled as a short ray that samples the ground. The ray is attached to the car 
  /// where the suspension is fixed. It shoots down (-y in vehicle space). The ray length is equal 
  /// to the suspension rest length + the wheel radius. If the <see cref="SteeringAngle"/> is 0, the 
  /// wheel rotates (rolls) around the x-axis in vehicle space.
  /// </para>
  /// <para>
  /// Each wheel can only be attached to one <see cref="ConstraintVehicle"/>
  /// </para>
  /// <para>
  /// The wheel has a lot of parameters. Careful tuning is necessary to achieve the desired driving 
  /// and sliding behavior.
  /// </para>
  /// </remarks>
  public partial class ConstraintWheel : IGeometricObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The chassis to which the wheel is attached.
    private RigidBody _chassis;

    private Pose _rayPose = Pose.Identity;
    private readonly RayShape _ray;

    internal WheelConstraint Constraint;
    internal int Tag;
    #endregion


    //--------------------------------------------------------------
    #region Constant Input Properties
    //--------------------------------------------------------------

    // 
    // Following properties are set once and define the wheel behavior.
    //

    /// <summary>
    /// Gets (or sets) the vehicle to which the wheel is attached.
    /// </summary>
    /// <value>The vehicle to which the wheel is attached.</value>
    public ConstraintVehicle Vehicle
    {
      get { return _vehicle; }
      internal set
      {
        _vehicle = value;
        OnChassisChanged();
      }
    }
    private ConstraintVehicle _vehicle;


    /// <summary>
    /// Gets or sets the radius of the wheel. 
    /// </summary>
    /// <value>The radius of the wheel.</value>
    public float Radius
    {
      get { return _radius; }
      set
      {
        if (_radius == value)
          return;

        _radius = value;
        _ray.Length = _radius + SuspensionRestLength;
        OnShapeChanged(this, ShapeChangedEventArgs.Empty);
      }
    }
    private float _radius;


    /// <summary>
    /// Gets or sets the rest length of the suspension.
    /// </summary>
    /// <value>The length of the suspension when no forces are acting on the wheel.</value>
    public float SuspensionRestLength
    {
      get { return _suspensionRestLength; }
      set
      {
        if (_suspensionRestLength == value)
          return;

        _suspensionRestLength = value;
        _ray.Length = Radius + _suspensionRestLength;
        OnShapeChanged(this, ShapeChangedEventArgs.Empty);
      }
    }
    private float _suspensionRestLength;


    /// <summary>
    /// Gets or sets the minimal suspension length.
    /// </summary>
    /// <value>
    /// The minimal suspension length. This can be any positive or negative value.
    /// If it is set to a large negative value, the minimal suspension length limit is effectively
    /// disabled. The default value is -infinity, which means the min suspension limit is disabled.
    /// </value>
    /// <remarks>
    /// <para>
    /// When a force (e.g. ground impact) pushes the wheel upwards, the suspension will dynamically
    /// compress. The suspension can be compressed up to this limit. This limit is a hard limit
    /// that stops the wheel from penetrating (moving into) the chassis body.  
    /// </para>
    /// </remarks>
    public float MinSuspensionLength { get; set; }


    /// <summary>
    /// Gets or sets the suspension stiffness.
    /// </summary>
    /// <value>The suspension stiffness.</value>
    /// <remarks>
    /// This value is independent of the car mass. (The car mass is internally multiplied to this
    /// value to compute the suspension spring constant.) Typical values are in the range [5, 50].
    /// Normal cars use lower values. Off road cars use higher values. Sports cars have the highest
    /// values.
    /// </remarks>
    public float SuspensionStiffness { get; set; }


    /// <summary>
    /// Gets or sets the suspension damping.
    /// </summary>
    /// <value>The suspension damping.</value>
    /// <remarks>
    /// Typical values are 10 - 50 % of the <see cref="SuspensionStiffness"/>. 
    /// </remarks>
    public float SuspensionDamping { get; set; }


    /// <summary>
    /// Gets or sets the max suspension force.
    /// </summary>
    /// <value>The max suspension force.</value>
    /// <remarks>
    /// When the suspension is compressed, it executes a force onto the chassis body. This 
    /// suspension force is limited by the <see cref="MaxSuspensionForce"/>. This limit 
    /// does not apply when the <see cref="MinSuspensionLength"/> is reached because then
    /// a larger force can be applied to the chassis to stop the wheel from moving further
    /// into the chassis body.
    /// </remarks>
    public float MaxSuspensionForce { get; set; }


    /// <summary>
    /// Gets or sets the rolling friction force.
    /// </summary>
    /// <value>The rolling friction force.</value>
    /// <remarks>
    /// If this value is 0, the car does not stop rolling when the motor is turned off.
    /// </remarks>
    public float RollingFrictionForce { get; set; }


    /// <summary>
    /// Gets or sets the friction.
    /// </summary>
    /// <value>The friction.</value>
    /// <remarks>
    /// This friction constant determines how easily this wheel starts sliding. Front and rear
    /// wheels can use different friction values to create "understeering" and "oversteering"
    /// behaviors.
    /// </remarks>
    public float Friction { get; set; }


    /// <summary>
    /// Gets or sets the roll reduction.
    /// </summary>
    /// <value>The roll reduction.</value>
    /// <remarks>
    /// This value is usually in the range [0, 1]. Values greater than 0 stop the car from rolling 
    /// in tight curves. 
    /// </remarks>
    public float RollReduction { get; set; }


    /// <summary>
    /// Gets or sets the wheel offset.
    /// </summary>
    /// <value>The wheel offset.</value>
    /// <remarks>
    /// This is a position offset in the local space of the vehicle. It determines where
    /// the suspension (the ray origin) is fixed on the car.
    /// </remarks>
    public Vector3F Offset
    {
      get { return _offset; }
      set
      {
        _offset = value;
        if (_chassis != null)
          OnChassisPoseChanged(null, null);
      }
    }
    private Vector3F _offset;

    #endregion


    //--------------------------------------------------------------
    #region Driving Input Properties
    //--------------------------------------------------------------

    //
    // Following properties must be updated each frame by the user that controls the vehicle.
    //

    /// <summary>
    /// Gets or sets the steering angle.
    /// </summary>
    /// <value>The steering angle.</value>
    /// <remarks>
    /// This angle is 0 to drive forward. It is greater than 0 to drive left and less than 0
    /// to drive right.
    /// </remarks>
    public float SteeringAngle
    {
      get { return _steeringAngle; }
      set
      {
        if (_steeringAngle == value)
          return;

        _steeringAngle = value;
        _chassis.WakeUp();
      }
    }
    private float _steeringAngle;


    /// <summary>
    /// Gets or sets the motor force.
    /// </summary>
    /// <value>The motor force.</value>
    /// <remarks>
    /// Values greater than 0 will make the car drive into the steering direction. Values less
    /// than 0 will make the car drive backwards.
    /// </remarks>
    public float MotorForce
    {
      get { return _motorForce; }
      set
      {
        if (_motorForce == value)
          return;

        _motorForce = value;
        _chassis.WakeUp();
      }
    }
    private float _motorForce;


    /// <summary>
    /// Gets or sets the brake force.
    /// </summary>
    /// <value>The brake force. (Must be positive.)</value>
    /// <remarks>
    /// A value greater than 0 makes the car slow down. 
    /// </remarks>
    public float BrakeForce
    {
      get { return _brakeForce; }
      set
      {
        if (_brakeForce == value)
          return;

        _brakeForce = value;
        _chassis.WakeUp();
      }
    }
    private float _brakeForce;
    #endregion


    //--------------------------------------------------------------
    #region Simulation Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the collision object that defines collision detection behavior of the ray.
    /// </summary>
    /// <value>The collision object of the ray.</value>
    public CollisionObject CollisionObject { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ConstraintWheel"/> is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// If the wheel is disabled, all simulation objects are removed from the simulation and
    /// collision domain. 
    /// </remarks>
    internal bool Enabled
    {
      get { return CollisionObject.Domain != null; }
      set
      {
        if (Enabled == value)
          return;

        var simulation = Vehicle.Simulation;
        if (value)
        {
          // Add collision object to collision domain.
          simulation.CollisionDomain.CollisionObjects.Add(CollisionObject);

          // Add wheel constraint. 
          // (Make sure BodyB is not be null. null is not allowed. BodyB will only be null
          // if the constraint is disabled.)
          if (Constraint.BodyB == null)
            Constraint.BodyB = simulation.World;
          simulation.Constraints.Add(Constraint);

          // Disable collision between wheels and chassis.
          var filter = Vehicle.Simulation.CollisionDomain.CollisionDetection.CollisionFilter as ICollisionFilter;
          if (filter != null)
            filter.Set(Vehicle.Chassis.CollisionObject, CollisionObject, false);
        }
        else
        {
          // To clean up, undo all the steps from above:

          // Enable collision between wheels and chassis.
          var filter = simulation.CollisionDomain.CollisionDetection.CollisionFilter as ICollisionFilter;
          if (filter != null)
            filter.Set(Vehicle.Chassis.CollisionObject, CollisionObject, true);

          // Remove wheel constraint.
          simulation.Constraints.Remove(Constraint);

          // Remove collision object.
          simulation.CollisionDomain.CollisionObjects.Remove(CollisionObject);
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Simulation Result Properties
    //--------------------------------------------------------------

    // Following properties are updated by the simulation each frame.

    /// <summary>
    /// Gets (or sets) the position where the wheel touches the ground.
    /// </summary>
    /// <value>The ground position.</value>
    /// <remarks>
    /// This value is only up-to-date and valid if <see cref="HasGroundContact"/> is 
    /// <see langword="true"/>.
    /// </remarks>
    public Vector3F GroundPosition { get; internal set; }

    /// <summary>
    /// Gets (or sets) the normal vector of the ground at the <see cref="GroundPosition"/>.
    /// </summary>
    /// <value>The ground normal vector.</value>
    /// <remarks>
    /// This value is only up-to-date and valid if <see cref="HasGroundContact"/> is 
    /// <see langword="true"/>.
    /// </remarks>
    public Vector3F GroundNormal { get; internal set; }

    /// <summary>
    /// Gets (or sets) the forward vector in the ground plane.
    /// </summary>
    /// <value>The forward vector in the ground plane.</value>
    /// <remarks>
    /// This value is only up-to-date and valid if <see cref="HasGroundContact"/> is 
    /// <see langword="true"/>.
    /// </remarks>
    public Vector3F GroundForward { get; internal set; }


    /// <summary>
    /// Gets (or sets) the right vector in the ground plane.
    /// </summary>
    /// <value>The right vector in the ground plane.</value>
    /// <remarks>
    /// This value is only up-to-date and valid if <see cref="HasGroundContact"/> is 
    /// <see langword="true"/>.
    /// </remarks>
    public Vector3F GroundRight { get; internal set; }


    /// <summary>
    /// Gets (or sets) a value indicating whether this wheel has ground contact.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this wheel has ground contact; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool HasGroundContact { get; internal set; }


    /// <summary>
    /// Gets the current suspension length.
    /// </summary>
    /// <value>The length of the suspension.</value>
    public float SuspensionLength { get; set; }


    /// <summary>
    /// Gets the touched body (= the ground object).
    /// </summary>
    /// <value>
    /// The touched body or <see langword="null"/> if <see cref="HasGroundContact"/> is 
    /// <see langword="false"/>.
    /// </value>
    public RigidBody TouchedBody { get; internal set; }


    /// <summary>
    /// Gets the pose of the wheel in world space.
    /// </summary>
    /// <value>The pose of the wheel in world space.</value>
    /// <remarks>
    /// This property returns the current pose of the wheel center.
    /// </remarks>
    public Pose Pose
    {
      get
      {
        if (_chassis == null)
          return Pose.Identity;

        var chassisPose = _chassis.Pose;

        // Point where suspension is fixed.
        var hardpointPosition = chassisPose.Position + chassisPose.ToWorldDirection(Offset);

        // Add current suspension length.
        var wheelPosition = hardpointPosition + SuspensionLength * chassisPose.ToWorldDirection(-Vector3F.UnitY);

        var wheelRotation = chassisPose.Orientation
                            * Matrix33F.CreateRotationY(SteeringAngle)
                            * Matrix33F.CreateRotationX(-RotationAngle);

        return new Pose(wheelPosition, wheelRotation);
      }
    }


    /// <summary>
    /// Gets or sets the angular velocity of the wheel about the rotation axis.
    /// </summary>
    /// <value>The angular velocity.</value>
    public float AngularVelocity { get; internal set; }


    /// <summary>
    /// Gets or sets the rotation angle about the rolling axis.
    /// </summary>
    /// <value>The rotation angle.</value>
    public float RotationAngle { get; internal set; }


    /// <summary>
    /// Gets or sets the skid energy.
    /// </summary>
    /// <value>The skid energy.</value>
    /// <remarks>
    /// <para>
    /// The skid energy represents the energy converted to heat if the car tire slides. A skid 
    /// energy of 0 indicates that the wheel has grip and is not sliding. Values greater than 0
    /// indicate that the wheel is sliding. The skid energy can be used to control skid related 
    /// effects (skid marks, smoke, sounds).
    /// </para>
    /// <para>
    /// In the currently implementation the skid energy is only valid when the car is moving (rapid 
    /// acceleration, braking, sliding in turns). It is not correct when external constraints are
    /// blocking the movement. For example, when a car is pushing against a wall the skid energy is
    /// 0 - even if all wheels are sliding.
    /// </para>
    /// </remarks>
    public float SkidEnergy { get; internal set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ConstraintWheel"/> class.
    /// </summary>
    public ConstraintWheel()
    {
      _radius = 0.4f;
      _suspensionRestLength = 0.6f;
      MinSuspensionLength = float.NegativeInfinity;
      SuspensionLength = SuspensionRestLength;
      SuspensionStiffness = 100;
      SuspensionDamping = 10;
      MaxSuspensionForce = float.PositiveInfinity;
      RollingFrictionForce = 500;
      Friction = 1.1f;
      RollReduction = 0.3f;

      Vector3F rayOrigin = Vector3F.Zero;
      Vector3F rayDirection = -Vector3F.UnitY;
      float rayLength = Radius + SuspensionRestLength;
      _ray = new RayShape(rayOrigin, rayDirection, rayLength)
      {
        StopsAtFirstHit = true,
      };
      CollisionObject = new CollisionObject(this);

      Constraint = new WheelConstraint(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Called when the Chassis RigidBody changes.
    internal void OnChassisChanged()
    {
      if (_chassis != null)
      {
        _chassis.PoseChanged -= OnChassisPoseChanged;
        Constraint.BodyA = null;
      }

      _chassis = Vehicle.Chassis;

      if (_chassis != null)
      {
        _chassis.PoseChanged += OnChassisPoseChanged;
        Constraint.BodyA = _chassis;
        OnChassisPoseChanged(null, null);
      }
    }


    // Called when the pose of the wheel must be updated.
    private void OnChassisPoseChanged(object sender, EventArgs eventArgs)
    {
      var chassisPose = _chassis.Pose;
      var wheelPosition = chassisPose.Position + chassisPose.ToWorldDirection(Offset);
      _rayPose = new Pose(wheelPosition, chassisPose.Orientation);
      OnPoseChanged(eventArgs);
    }
    #endregion
  }
}
