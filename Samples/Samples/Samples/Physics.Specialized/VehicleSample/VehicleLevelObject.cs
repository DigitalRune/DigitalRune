using System;
using DigitalRune.Game;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using Microsoft.Practices.ServiceLocation;


namespace Samples.Physics.Specialized
{
  /// <summary>
  /// Builds a level with several test obstacles.
  /// </summary>
  public class VehicleLevelObject : GameObject
  {
    private readonly IServiceLocator _services;


    public VehicleLevelObject(IServiceLocator services)
    {
      _services = services;
      Name = "VehicleLevel";
    }


    protected override void OnLoad()
    {
      // Add rigid bodies to simulation.
      var simulation = _services.GetInstance<Simulation>();

      // ----- Add a ground plane.
      AddBody(simulation, "GroundPlane", Pose.Identity, new PlaneShape(Vector3F.UnitY, 0), MotionType.Static);

      // ----- Create a height field.
      var numberOfSamplesX = 20;
      var numberOfSamplesZ = 20;
      var samples = new float[numberOfSamplesX * numberOfSamplesZ];
      for (int z = 0; z < numberOfSamplesZ; z++)
      {
        for (int x = 0; x < numberOfSamplesX; x++)
        {
          if (x == 0 || z == 0 || x == 19 || z == 19)
          {
            samples[z * numberOfSamplesX + x] = -1;
          }
          else
          {
            samples[z * numberOfSamplesX + x] = 1.0f + (float)(Math.Cos(z / 2f) * Math.Sin(x / 2f) * 1.0f);
          }
        }
      }
      HeightField heightField = new HeightField(0, 0, 120, 120, samples, numberOfSamplesX, numberOfSamplesZ);
      //heightField.UseFastCollisionApproximation = true;
      AddBody(simulation, "HeightField", new Pose(new Vector3F(10, 0, 20)), heightField, MotionType.Static);

      // ----- Create rubble on the floor (small random objects on the floor).
      for (int i = 0; i < 60; i++)
      {
        Vector3F position = new Vector3F(RandomHelper.Random.NextFloat(-5, 5), 0, RandomHelper.Random.NextFloat(10, 20));
        QuaternionF orientation = RandomHelper.Random.NextQuaternionF();
        BoxShape shape = new BoxShape(RandomHelper.Random.NextVector3F(0.05f, 0.5f));
        AddBody(simulation, "Stone" + i, new Pose(position, orientation), shape, MotionType.Static);
      }

      // ----- Slopes with different tilt angles.
      // Create a loop.
      Vector3F slopePosition = new Vector3F(-20, -0.25f, -5);
      BoxShape slopeShape = new BoxShape(8, 0.5f, 2);
      for (int i = 1; i < 33; i++)
      {
        Matrix33F oldRotation = Matrix33F.CreateRotationX((i - 1) * MathHelper.ToRadians(10));
        Matrix33F rotation = Matrix33F.CreateRotationX(i * MathHelper.ToRadians(10));

        slopePosition += (oldRotation * new Vector3F(0, 0, -slopeShape.WidthZ)) / 2
                         + (rotation * new Vector3F(0, 0, -slopeShape.WidthZ)) / 2;

        AddBody(simulation, "Loop" + i, new Pose(slopePosition, rotation), slopeShape, MotionType.Static);
      }

      // Create an arched bridge.
      slopePosition = new Vector3F(-10, -2, -15);
      slopeShape = new BoxShape(8f, 0.5f, 5);
      for (int i = 1; i < 8; i++)
      {
        Matrix33F oldRotation = Matrix33F.CreateRotationX(MathHelper.ToRadians(40) - (i - 1) * MathHelper.ToRadians(10));
        Matrix33F rotation = Matrix33F.CreateRotationX(MathHelper.ToRadians(40) - i * MathHelper.ToRadians(10));

        slopePosition += (oldRotation * new Vector3F(0, 0, -slopeShape.WidthZ)) / 2
                         + (rotation * new Vector3F(0, 0, -slopeShape.WidthZ)) / 2;
        Vector3F position = slopePosition - rotation * new Vector3F(0, slopeShape.WidthY / 2, 0);

        AddBody(simulation, "Bridge" + i, new Pose(position, rotation), slopeShape, MotionType.Static);
      }

      // ----- Create a mesh object.
      // We first build a composite shape out of several primitives and then convert the 
      // composite shape to a triangle mesh. (Just for testing.)
      CompositeShape compositeShape = new CompositeShape();
      compositeShape.Children.Add(new GeometricObject(heightField, Pose.Identity));
      compositeShape.Children.Add(new GeometricObject(new CylinderShape(1, 2), new Pose(new Vector3F(10, 1, 10))));
      compositeShape.Children.Add(new GeometricObject(new SphereShape(3), new Pose(new Vector3F(15, 0, 15))));
      compositeShape.Children.Add(new GeometricObject(new BoxShape(1, 2, 3), new Pose(new Vector3F(15, 0, 5))));
      ITriangleMesh mesh = compositeShape.GetMesh(0.01f, 3);
      TriangleMeshShape meshShape = new TriangleMeshShape(mesh, true);
      meshShape.Partition = new AabbTree<int>() { BottomUpBuildThreshold = 0 };
      AddBody(simulation, "Mesh", new Pose(new Vector3F(-120, 0, 20)), meshShape, MotionType.Static);

      // ----- Create a seesaw.
      var seesawBase = AddBody(simulation, "SeesawBase", new Pose(new Vector3F(15, 0.5f, 0)), new BoxShape(0.2f, 1, 6), MotionType.Static);
      var seesaw = AddBody(simulation, "Seesaw", new Pose(new Vector3F(16, 1.05f, 0)), new BoxShape(15, 0.1f, 6), MotionType.Dynamic);
      seesaw.MassFrame.Mass = 500;
      seesaw.CanSleep = false;

      // Connect seesaw using a hinge joint.
      simulation.Constraints.Add(new HingeJoint
      {
        BodyA = seesaw,
        BodyB = seesawBase,
        AnchorPoseALocal = new Pose(new Vector3F(1.0f, 0, 0),
                                    new Matrix33F(0, 0, -1,
                                                  0, 1, 0,
                                                  1, 0, 0)),
        AnchorPoseBLocal = new Pose(new Vector3F(0, 0.5f, 0),
                                    new Matrix33F(0, 0, -1,
                                                  0, 1, 0,
                                                  1, 0, 0)),
        CollisionEnabled = false,
      });


      // ----- Distribute a few dynamic spheres and boxes across the landscape.
      SphereShape sphereShape = new SphereShape(0.5f);
      for (int i = 0; i < 40; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-60, 60);
        position.Y = 10;
        AddBody(simulation, "Sphere" + i, new Pose(position), sphereShape, MotionType.Dynamic);
      }

      BoxShape boxShape = new BoxShape(1.0f, 1.0f, 1.0f);
      for (int i = 0; i < 40; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-60, 60);
        position.Y = 1;
        AddBody(simulation, "Box" + i, new Pose(position), boxShape, MotionType.Dynamic);
      }
    }


    protected override void OnUnload()
    {
      // A perfect game object would remove all its rigid bodies and constraints in OnUnload. - 
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
  }
}
