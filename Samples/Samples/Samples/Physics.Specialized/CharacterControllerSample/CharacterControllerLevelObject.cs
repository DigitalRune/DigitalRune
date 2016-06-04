using System;
using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.Materials;
using Microsoft.Practices.ServiceLocation;


namespace Samples.Physics.Specialized
{
  /// <summary>
  /// Builds and updates a level with several test obstacles.
  /// </summary>
  public class CharacterControllerLevelObject : GameObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly IServiceLocator _services;

    // An platform that moves up-down.
    private RigidBody _elevator;

    // A platform that moves sideways.
    private RigidBody _pusher;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public CharacterControllerLevelObject(IServiceLocator services)
    {
      _services = services;
      Name = "CharacterControllerLevel";
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    protected override void OnLoad()
    {
      // Add rigid bodies to simulation.
      var simulation = _services.GetInstance<Simulation>();

      // We use a random number generator with a custom seed.
      RandomHelper.Random = new Random(123);

      // ----- Add a ground plane.
      AddBody(simulation, "GroundPlane", Pose.Identity, new PlaneShape(Vector3F.UnitY, 0), MotionType.Static);

      // ----- Create a small flying sphere.
      AddBody(simulation, "Sphere", new Pose(new Vector3F(0, 1f, 0)), new SphereShape(0.2f), MotionType.Static);

      // ----- Create small walls at the level boundary.
      AddBody(simulation, "WallLeft", new Pose(new Vector3F(-30, 1, 0)), new BoxShape(0.3f, 2, 60), MotionType.Static);
      AddBody(simulation, "WallRight", new Pose(new Vector3F(30, 1, 0)), new BoxShape(0.3f, 2, 60), MotionType.Static);
      AddBody(simulation, "WallFront", new Pose(new Vector3F(0, 1, -30)), new BoxShape(60, 2, 0.3f), MotionType.Static);
      AddBody(simulation, "WallBack", new Pose(new Vector3F(0, 1, 30)), new BoxShape(60, 2, 0.3f), MotionType.Static);

      // ----- Create a few bigger objects.
      // We position the boxes so that we have a few corners we can run into. Character controllers
      // should be stable when the user runs into corners.
      AddBody(simulation, "House0", new Pose(new Vector3F(10, 1, -10)), new BoxShape(8, 2, 8f), MotionType.Static);
      AddBody(simulation, "House1", new Pose(new Vector3F(13, 1, -4)), new BoxShape(2, 2, 4), MotionType.Static);
      AddBody(simulation, "House2", new Pose(new Vector3F(10, 2, -15), Matrix33F.CreateRotationY(-0.3f)), new BoxShape(8, 4, 2), MotionType.Static);

      // ----- Create stairs with increasing step height.
      // Each step is a box. With this object we can test if our character can climb up
      // stairs. The character controller has a step height limit. Increasing step heights
      // let us test if the step height limit works.
      float startHeight = 0;
      const float stepDepth = 1f;
      for (int i = 0; i < 10; i++)
      {
        float stepHeight = 0.1f + i * 0.05f;
        Vector3F position = new Vector3F(0, startHeight + stepHeight / 2, -2 - i * stepDepth);
        AddBody(simulation, "Step" + i, new Pose(position), new BoxShape(2, stepHeight, stepDepth), MotionType.Static);

        startHeight += stepHeight;
      }

      // ----- V obstacle to test if we get stuck.
      AddBody(simulation, "V0", new Pose(new Vector3F(-5.5f, 0, 10), QuaternionF.CreateRotationZ(0.2f)), new BoxShape(1f, 2f, 2), MotionType.Static);
      AddBody(simulation, "V1", new Pose(new Vector3F(-4, 0, 10), QuaternionF.CreateRotationZ(-0.2f)), new BoxShape(1f, 2f, 2), MotionType.Static);

      // ----- Create a height field.
      // Terrain that is uneven is best modeled with a height field. Height fields are faster
      // than general triangle meshes.
      // The height direction is the y direction. 
      // The height field lies in the x/z plane.
      var numberOfSamplesX = 20;
      var numberOfSamplesZ = 20;
      var samples = new float[numberOfSamplesX * numberOfSamplesZ];
      // Create arbitrary height values. 
      for (int z = 0; z < numberOfSamplesZ; z++)
      {
        for (int x = 0; x < numberOfSamplesX; x++)
        {
          if (x == 0 || z == 0 || x == 19 || z == 19)
          {
            // Set this boundary elements to a low height, so that the height field is connected
            // to the ground.
            samples[z * numberOfSamplesX + x] = -1;
          }
          else
          {
            // A sine/cosine function that creates some interesting waves.
            samples[z * numberOfSamplesX + x] = 1.0f + (float)(Math.Cos(z / 2f) * Math.Sin(x / 2f) * 1.0f);
          }
        }
      }
      var heightField = new HeightField(0, 0, 20, 20, samples, numberOfSamplesX, numberOfSamplesZ);
      AddBody(simulation, "HeightField", new Pose(new Vector3F(10, 0, 10)), heightField, MotionType.Static);

      // ----- Create rubble on the floor (small random objects on the floor).
      // Our character should be able to move over small bumps on the ground.
      for (int i = 0; i < 50; i++)
      {
        Vector3F position = new Vector3F(RandomHelper.Random.NextFloat(-5, 5), 0, RandomHelper.Random.NextFloat(10, 20));
        QuaternionF orientation = RandomHelper.Random.NextQuaternionF();
        Vector3F size = RandomHelper.Random.NextVector3F(0.05f, 0.8f);
        AddBody(simulation, "Stone" + i, new Pose(position, orientation), new BoxShape(size), MotionType.Static);
      }

      // ----- Create some slopes to see how our character performs on/under sloped surfaces.
      // Here we can test how the character controller behaves if the head touches an inclined
      // ceiling.
      AddBody(simulation, "SlopeGround", new Pose(new Vector3F(-2, 1.8f, -12), QuaternionF.CreateRotationX(0.4f)), new BoxShape(2, 0.5f, 10), MotionType.Static);
      AddBody(simulation, "SlopeRoof", new Pose(new Vector3F(-2, 5.6f, -12), QuaternionF.CreateRotationX(-0.4f)), new BoxShape(2, 0.5f, 10), MotionType.Static);

      // Create slopes with increasing tilt angles.
      // The character controller has a slope limit. Only flat slopes should be climbable. 
      // Movement between slopes should be smooth.
      Vector3F slopePosition = new Vector3F(-17, -0.25f, 6);
      BoxShape slopeShape = new BoxShape(8, 0.5f, 5);
      for (int i = 1; i < 8; i++)
      {
        Matrix33F oldRotation = Matrix33F.CreateRotationX((i - 1) * MathHelper.ToRadians(10));
        Matrix33F rotation = Matrix33F.CreateRotationX(i * MathHelper.ToRadians(10));

        slopePosition += (oldRotation * new Vector3F(0, 0, -slopeShape.WidthZ)) / 2
                         + (rotation * new Vector3F(0, 0, -slopeShape.WidthZ)) / 2;

        AddBody(simulation, "Slope" + i, new Pose(slopePosition, rotation), slopeShape, MotionType.Static);
      }

      // Create slopes with decreasing tilt angles.
      slopePosition = new Vector3F(-8, -2, 5);
      slopeShape = new BoxShape(8f, 0.5f, 5);
      for (int i = 1; i < 8; i++)
      {
        Matrix33F oldRotation = Matrix33F.CreateRotationX(MathHelper.ToRadians(40) - (i - 1) * MathHelper.ToRadians(10));
        Matrix33F rotation = Matrix33F.CreateRotationX(MathHelper.ToRadians(40) - i * MathHelper.ToRadians(10));

        slopePosition += (oldRotation * new Vector3F(0, 0, -slopeShape.WidthZ)) / 2
                         + (rotation * new Vector3F(0, 0, -slopeShape.WidthZ)) / 2;
        Vector3F position = slopePosition - rotation * new Vector3F(0, slopeShape.WidthY / 2, 0);

        AddBody(simulation, "Slope2" + i, new Pose(position, rotation), slopeShape, MotionType.Static);
      }

      // ----- Create a slope with a wall on one side.
      // This objects let's us test how the character controller behaves while falling and
      // sliding along a vertical wall. (Run up the slope and then jump down while moving into
      // the wall.)
      AddBody(simulation, "LongSlope", new Pose(new Vector3F(-24, 3, -10), Matrix33F.CreateRotationX(0.4f)), new BoxShape(4, 5f, 30), MotionType.Static);
      AddBody(simulation, "LongSlopeWall", new Pose(new Vector3F(-26, 5, -10)), new BoxShape(0.5f, 10f, 25), MotionType.Static);

      // ----- Create a trigger object that represents a ladder.
      var ladder = AddBody(simulation, "Ladder", new Pose(new Vector3F(-25.7f, 5, 0)), new BoxShape(0.5f, 10f, 1), MotionType.Static);
      ladder.CollisionObject.Type = CollisionObjectType.Trigger;

      // ----- Create a mesh object to test walking on triangle meshes.
      // Normally, the mesh would be loaded from a file. Here, we make a composite shape and 
      // let DigitalRune Geometry compute a mesh for it. Then we throw away the composite
      // shape and use only the mesh. (We do this to test triangle meshes. Using the composite
      // shape instead of the triangle mesh would be a lot faster.)
      CompositeShape compositeShape = new CompositeShape();
      compositeShape.Children.Add(new GeometricObject(heightField, Pose.Identity));
      compositeShape.Children.Add(new GeometricObject(new CylinderShape(1, 2), new Pose(new Vector3F(10, 1, 10))));
      compositeShape.Children.Add(new GeometricObject(new SphereShape(3), new Pose(new Vector3F(15, 0, 15))));
      compositeShape.Children.Add(new GeometricObject(new BoxShape(4, 4, 3), new Pose(new Vector3F(15, 1.5f, 5))));
      compositeShape.Children.Add(new GeometricObject(new BoxShape(4, 4, 3), new Pose(new Vector3F(15, 1.5f, 0))));
      ITriangleMesh mesh = compositeShape.GetMesh(0.01f, 3);
      TriangleMeshShape meshShape = new TriangleMeshShape(mesh);

      // Collision detection speed for triangle meshes can be improved by using a spatial 
      // partition. Here, we assign an AabbTree to the triangle mesh shape. The tree is
      // built automatically when needed and it stores triangle indices (therefore the generic 
      // parameter of the AabbTree is int).
      meshShape.Partition = new AabbTree<int>()
      {
        // The tree is automatically built using a mixed top-down/bottom-up approach. Bottom-up
        // building is slower but produces better trees. If the tree building takes too long,
        // we can lower the BottomUpBuildThreshold (default is 128).
        BottomUpBuildThreshold = 0,
      };

      // Contact welding creates smoother contact normals - but it costs a bit of performance.
      meshShape.EnableContactWelding = true;

      AddBody(simulation, "Mesh", new Pose(new Vector3F(-30, 0, 10)), meshShape, MotionType.Static);

      // ----- Create a seesaw.
      var seesawBase = AddBody(simulation, "SeesawBase", new Pose(new Vector3F(5, 0.5f, 0)), new BoxShape(0.2f, 1, 1), MotionType.Static);
      var seesaw = AddBody(simulation, "Seesaw", new Pose(new Vector3F(5, 1.05f, 0)), new BoxShape(5, 0.1f, 1), MotionType.Dynamic);

      // Attach the seesaw to the base using a hinge joint.
      simulation.Constraints.Add(new HingeJoint
      {
        BodyA = seesaw,
        BodyB = seesawBase,
        AnchorPoseALocal = new Pose(new Vector3F(0, 0, 0),
                                    new Matrix33F(0, 0, -1,
                                                  0, 1, 0,
                                                  1, 0, 0)),
        AnchorPoseBLocal = new Pose(new Vector3F(0, 0.5f, 0),
                                    new Matrix33F(0, 0, -1,
                                                  0, 1, 0,
                                                  1, 0, 0)),
        CollisionEnabled = false,
      });

      // ----- A platform that is moving up/down.
      _elevator = AddBody(simulation, "Elevator", new Pose(new Vector3F(5, -1f, 5)), new BoxShape(3, 1f, 3), MotionType.Kinematic);
      _elevator.LinearVelocity = new Vector3F(2, 2, 0);

      // ----- A platform that is moving sideways.
      _pusher = AddBody(simulation, "Pusher", new Pose(new Vector3F(15, 0.5f, 0)), new BoxShape(3, 1f, 3), MotionType.Kinematic);
      _pusher.LinearVelocity = new Vector3F(0, 0, 2);

      // ----- Create conveyor belt with two static boxes on the sides.
      AddBody(simulation, "ConveyorSide0", new Pose(new Vector3F(19, 0.25f, 0)), new BoxShape(0.8f, 0.5f, 8f), MotionType.Static);
      AddBody(simulation, "ConveyorSide1", new Pose(new Vector3F(21, 0.25f, 0)), new BoxShape(0.8f, 0.5f, 8f), MotionType.Static);

      // The conveyor belt is a simple box with a special material.
      var conveyorBelt = AddBody(simulation, "ConveyorBelt", new Pose(new Vector3F(20, 0.25f, 0)), new BoxShape(1f, 0.51f, 8f), MotionType.Static);
      UniformMaterial materialWithSurfaceMotion = new UniformMaterial("ConveyorBelt", true)  // Important: The second parameter enables the surface
      {                                                                                      // motion. It has to be set to true in the constructor!
        SurfaceMotion = new Vector3F(0, 0, 1),   // The surface motion relative to the object.
      };
      conveyorBelt.Material = materialWithSurfaceMotion;

      // ----- Distribute a few dynamic spheres and boxes across the landscape.
      SphereShape sphereShape = new SphereShape(0.5f);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-15, 15);
        position.Y = 20;

        AddBody(simulation, "Sphere" + i, new Pose(position), sphereShape, MotionType.Dynamic);
      }

      BoxShape boxShape = new BoxShape(1, 1, 1);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-15, 15);
        position.Y = 20;

        AddBody(simulation, "Box" + i, new Pose(position), boxShape, MotionType.Dynamic);
      }

    }


    protected override void OnUnload()
    {
      // A perfect game object would remove all its rigid bodies in OnUnload. - 
      // But we are lazy and it is not necessary in this sample...
    }


    // Creates a new rigid body and adds it to the simulation.
    private static RigidBody AddBody(Simulation simulation, string name, Pose pose, Shape shape, MotionType motionType)
    {
      var rigidBody = new RigidBody(shape)
      {
        Name = name,
        Pose = pose,
        MotionType = motionType,
      };

      simulation.RigidBodies.Add(rigidBody);
      return rigidBody;
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
      // Reverse velocities of moving platforms when they reach a limit.
      if (_elevator.Pose.Position.Y > 8 && _elevator.LinearVelocity.Y > 0
          || _elevator.Pose.Position.Y < -1 && _elevator.LinearVelocity.Y < 0)
      {
        _elevator.LinearVelocity *= -1;
      }

      if (_pusher.Pose.Position.Z > 9 && _pusher.LinearVelocity.Z > 0
          || _pusher.Pose.Position.Z < 0 && _pusher.LinearVelocity.Z < 0)
      {
        _pusher.LinearVelocity *= -1;
      }
    }
    #endregion
  }
}
