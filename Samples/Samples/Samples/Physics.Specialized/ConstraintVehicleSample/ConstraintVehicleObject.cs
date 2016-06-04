using System;
using System.Linq;
using DigitalRune.Game;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.Materials;
using DigitalRune.Physics.Specialized;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Physics.Specialized
{
  // Controls a vehicle.
  [Controls(@"Vehicle
  Use <W> + <A> + <S> + <D> or <Left Stick> + <Right Trigger> + <Left Trigger> to control car.
  Press <Space> or <GamePad A> to use the handbrake.")]
  public class ConstraintVehicleObject : GameObject
  {
    // Note:
    // To reset the vehicle position, simply call:
    //  _vehicle.Chassis.Pose = myPose;
    //  _vehicle.Chassis.LinearVelocity = Vector3F.Zero;
    //  _vehicle.Chassis.AngularVelocity = Vector3F.Zero;


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IServiceLocator _services;
    private readonly IInputService _inputService;

    private readonly Simulation _simulation;

    // Models for rendering.
    private readonly ModelNode _vehicleModelNode;
    private readonly ModelNode[] _wheelModelNodes;

    // Vehicle values.
    private float _steeringAngle;
    private float _motorForce;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    public ConstraintVehicle Vehicle { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public ConstraintVehicleObject(IServiceLocator services)
    {
      Name = "Vehicle";

      _services = services;
      _inputService = services.GetInstance<IInputService>();

      _simulation = services.GetInstance<Simulation>();

      // Load models for rendering.
      var contentManager = services.GetInstance<ContentManager>();
      _vehicleModelNode = contentManager.Load<ModelNode>("Car/Car").Clone();
      _wheelModelNodes = new ModelNode[4];
      _wheelModelNodes[0] = contentManager.Load<ModelNode>("Car/Wheel").Clone();
      _wheelModelNodes[1] = _wheelModelNodes[0].Clone();
      _wheelModelNodes[2] = _wheelModelNodes[0].Clone();
      _wheelModelNodes[3] = _wheelModelNodes[0].Clone();

      // Add wheels under the car model node.
      _vehicleModelNode.Children.Add(_wheelModelNodes[0]);
      _vehicleModelNode.Children.Add(_wheelModelNodes[1]);
      _vehicleModelNode.Children.Add(_wheelModelNodes[2]);
      _vehicleModelNode.Children.Add(_wheelModelNodes[3]);

      // ----- Create the chassis of the car.
      // The Vehicle needs a rigid body that represents the chassis. This can be any shape (e.g.
      // a simple BoxShape). In this example we will build a convex polyhedron from the car model.

      // 1. Extract the vertices from the car model.
      // The car model has ~10,000 vertices. It consists of a MeshNode for the glass
      // parts and a MeshNode "Car" for the chassis.
      var meshNode = _vehicleModelNode.GetDescendants()
                                      .OfType<MeshNode>()
                                      .First(mn => mn.Name == "Car");
      var mesh = MeshHelper.ToTriangleMesh(meshNode.Mesh);
      // Apply the transformation of the mesh node.
      mesh.Transform(meshNode.PoseWorld * Matrix44F.CreateScale(meshNode.ScaleWorld));

      // 2. (Optional) Create simplified convex hull from mesh.
      // We could also skip this step and directly create a convex polyhedron from the mesh using
      //    var chassisShape = new ConvexPolyhedron(mesh.Vertices);
      // However, the convex polyhedron would still have 500-600 vertices. 
      // We can reduce the number of vertices by using the GeometryHelper.
      // Create a convex hull for mesh with max. 64 vertices. Additional, shrink the hull by 4 cm.
      var convexHull = GeometryHelper.CreateConvexHull(mesh.Vertices, 64, -0.04f);

      // 3. Create convex polyhedron shape using the vertices of the convex hull.
      var chassisShape = new ConvexPolyhedron(convexHull.Vertices.Select(v => v.Position));

      // (Note: Building convex hulls and convex polyhedra are time-consuming. To save loading time 
      // we should build the shape in the XNA content pipeline. See other DigitalRune Physics 
      // Samples.)

      // The mass properties of the car. We use a mass of 800 kg.
      var mass = MassFrame.FromShapeAndMass(chassisShape, Vector3F.One, 800, 0.1f, 1);

      // Trick: We artificially modify the center of mass of the rigid body. Lowering the center
      // of mass makes the car more stable against rolling in tight curves. 
      // We could also modify mass.Inertia for other effects.
      var pose = mass.Pose;
      pose.Position.Y -= 0.5f; // Lower the center of mass.
      pose.Position.Z = -0.5f; // The center should be below the driver. 
      // (Note: The car model is not exactly centered.)
      mass.Pose = pose;

      // Material for the chassis.
      var material = new UniformMaterial
      {
        Restitution = 0.1f,
        StaticFriction = 0.2f,
        DynamicFriction = 0.2f
      };

      var chassis = new RigidBody(chassisShape, mass, material)
      {
        Pose = new Pose(new Vector3F(0, 2, 0)),  // Start position
        UserData = "NoDraw",                     // (Remove this line to render the collision model.)
      };

      // ----- Create the vehicle.
      Vehicle = new ConstraintVehicle(_simulation, chassis);

      // Add 4 wheels.
      Vehicle.Wheels.Add(new ConstraintWheel { Offset = new Vector3F(-0.9f, 0.6f, -2.0f), Radius = 0.36f, SuspensionRestLength = 0.55f, MinSuspensionLength = 0.25f, Friction = 2 });  // Front left
      Vehicle.Wheels.Add(new ConstraintWheel { Offset = new Vector3F(0.9f, 0.6f, -2.0f), Radius = 0.36f, SuspensionRestLength = 0.55f, MinSuspensionLength = 0.25f, Friction = 2 });   // Front right
      Vehicle.Wheels.Add(new ConstraintWheel { Offset = new Vector3F(-0.9f, 0.6f, 0.98f), Radius = 0.36f, SuspensionRestLength = 0.55f, MinSuspensionLength = 0.25f, Friction = 1.8f });// Back left
      Vehicle.Wheels.Add(new ConstraintWheel { Offset = new Vector3F(0.9f, 0.6f, 0.98f), Radius = 0.36f, SuspensionRestLength = 0.55f, MinSuspensionLength = 0.25f, Friction = 1.8f }); // Back right

      // Vehicles are disabled per default. This way we can create the vehicle and the simulation
      // objects are only added when needed.
      Vehicle.Enabled = false;

    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    protected override void OnLoad()
    {
      // Enable vehicle. (This adds the necessary objects to the physics simulation.)
      Vehicle.Enabled = true;

      // Add graphics model to scene graph.
      var scene = _services.GetInstance<IScene>();
      scene.Children.Add(_vehicleModelNode);
    }


    protected override void OnUnload()
    {
      // Disable vehicle. (This removes the vehicle objects from the physics simulation.)
      Vehicle.Enabled = false;

      // Remove graphics model from scene graph.
      _vehicleModelNode.Parent.Children.Remove(_vehicleModelNode);
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Mouse centering (controlled by the MenuComponent) is disabled if the game
      // is inactive or if the GUI is active. In these cases, we do not want to move
      // the player.
      if (!_inputService.EnableMouseCentering)
        return;

      float deltaTimeF = (float)deltaTime.TotalSeconds;

      // Update steering direction from left/right arrow keys.
      UpdateSteeringAngle(deltaTimeF);

      // Update acceleration from up/down arrow keys.
      UpdateAcceleration(deltaTimeF);

      // Pressing <Space> activates handbrake
      if (_inputService.IsDown(Keys.Space) || _inputService.IsDown(Buttons.A, LogicalPlayerIndex.One))
      {
        // Braking force from handbrake needs to be applied to back wheels.
        const float brakeForce = 6000;
        Vehicle.Wheels[2].MotorForce = 0;
        Vehicle.Wheels[3].MotorForce = 0;
        Vehicle.Wheels[2].BrakeForce = brakeForce;
        Vehicle.Wheels[3].BrakeForce = brakeForce;
      }
      else
      {
        Vehicle.Wheels[2].BrakeForce = 0;
        Vehicle.Wheels[3].BrakeForce = 0;
      }

      // Update poses of graphics models.
      // Update SceneNode.LastPoseWorld (required for optional effects, like motion blur).
      _vehicleModelNode.SetLastPose(true);
      _vehicleModelNode.PoseWorld = Vehicle.Chassis.Pose;
      for (int i = 0; i < _wheelModelNodes.Length; i++)
      {
        var pose = Vehicle.Wheels[i].Pose;
        if (Vehicle.Wheels[i].Offset.X < 0)
        {
          // Left wheel.
          pose.Orientation = pose.Orientation * Matrix33F.CreateRotationY(ConstantsF.Pi);
        }
        _wheelModelNodes[i].SetLastPose(true);
        _wheelModelNodes[i].PoseWorld = pose;
      }
    }


    private void UpdateSteeringAngle(float deltaTime)
    {
      // TODO: Reduce max steering angle at high speeds.

      const float MaxAngle = 0.5f;
      const float SteeringRate = 3;

      // We limit the amount of change per frame.
      float change = SteeringRate * deltaTime;

      float direction = 0;
      if (_inputService.IsDown(Keys.A))
        direction += 1;
      if (_inputService.IsDown(Keys.D))
        direction -= 1;

      var gamePadState = _inputService.GetGamePadState(LogicalPlayerIndex.One);
      direction -= gamePadState.ThumbSticks.Left.X;

      if (direction != 0)
      {
        // Increase steering angle.
        _steeringAngle = MathHelper.Clamp(_steeringAngle + direction * change, -MaxAngle, +MaxAngle);
      }
      else
      {
        // Steer back to neutral position (angle 0).
        if (_steeringAngle > 0)
          _steeringAngle = MathHelper.Clamp(_steeringAngle - change, 0, +MaxAngle);
        else if (_steeringAngle < 0)
          _steeringAngle = MathHelper.Clamp(_steeringAngle + change, -MaxAngle, 0);

        // TODO: Maybe we steer back with half rate? 
        // (Pressing a button steers faster than not pressing a button?)
      }

      VehicleHelper.SetCarSteeringAngle(_steeringAngle, Vehicle.Wheels[0], Vehicle.Wheels[1], Vehicle.Wheels[2], Vehicle.Wheels[3]);
    }


    private void UpdateAcceleration(float deltaTime)
    {
      const float MaxForce = 2000;
      const float AccelerationRate = 10000;

      // We limit the amount of change per frame.
      float change = AccelerationRate * deltaTime;

      float direction = 0;
      if (_inputService.IsDown(Keys.W))
        direction += 1;
      if (_inputService.IsDown(Keys.S))
        direction -= 1;

      GamePadState gamePadState = _inputService.GetGamePadState(LogicalPlayerIndex.One);
      direction += gamePadState.Triggers.Right - gamePadState.Triggers.Left;

      if (direction != 0)
      {
        // Increase motor force.
        _motorForce = MathHelper.Clamp(_motorForce + direction * change, -MaxForce, +MaxForce);
      }
      else
      {
        // No acceleration. Bring motor force down to 0.
        if (_motorForce > 0)
          _motorForce = MathHelper.Clamp(_motorForce - change, 0, +MaxForce);
        else if (_motorForce < 0)
          _motorForce = MathHelper.Clamp(_motorForce + change, -MaxForce, 0);
      }

      // We can decide which wheels are motorized. Here we use an all wheel drive:
      Vehicle.Wheels[0].MotorForce = _motorForce;
      Vehicle.Wheels[1].MotorForce = _motorForce;
      Vehicle.Wheels[2].MotorForce = _motorForce;
      Vehicle.Wheels[3].MotorForce = _motorForce;
    }
    #endregion
  }
}
