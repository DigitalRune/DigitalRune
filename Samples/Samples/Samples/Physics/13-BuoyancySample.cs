using DigitalRune.Geometry;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates the buoyancy effect (bodies swimming in water).",
    "",
    13)]
  public class BuoyancySample : PhysicsSample
  {
    // The collision object that represents the area filled with water.
    private CollisionObject _waterCollisionObject;


    public BuoyancySample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // ----- Buoyancy Force Effect
      // Buoyancy is a force effect that lets bodies swim in water. The water area is 
      // defined by two properties: 
      //  - Buoyancy.AreaOfEffect defines which objects are affected.
      //  - Buoyancy.Surface defines the water level within this area.

      // The area of effect can be defined in different ways. In this sample we will use 
      // a geometric object ("trigger volume").

      // First, define the shape of the water area. We will create simple pool.
      Shape poolShape = new BoxShape(16, 10, 16);
      Vector3F poolCenter = new Vector3F(0, -5, 0);

      // Then create a geometric object for the water area. (A GeometricObject is required
      // to position the shape in the world. A GeometricObject stores shape, scale, position,
      // orientation, ...)
      GeometricObject waterGeometry = new GeometricObject(poolShape, new Pose(poolCenter));

      // Then create a collision object for the geometric object. (A CollisionObject required
      // because the geometry should be used for collision detection with other objects.)
      _waterCollisionObject = new CollisionObject(waterGeometry)
      {
        // Assign the object to a different collision group:
        // The Grab component (see Grab.cs) uses a ray to perform hit tests. We don't want the ray
        // to collide with the water. Therefore, we need to assign the water collision object to a 
        // different collision group. The general geometry is in collision group 0. The rays are in 
        // collision group 2. Add the water to collision group 1. Collision between 0 and 2 are 
        // enabled. Collision between 1 and 2 need to be disabled - this collision filter was set 
        // in PhysicsGame.cs.
        CollisionGroup = 1,

        // Set the type to "Trigger". This improves the performance because the collision 
        // detection does not need to compute detailed contact information. The collision
        // detection only returns whether an objects has contact with the water.
        Type = CollisionObjectType.Trigger,
      };

      // The collision object needs to be added into the collision domain of the simulation.
      Simulation.CollisionDomain.CollisionObjects.Add(_waterCollisionObject);

      // Now we can add the buoyancy effect.
      Buoyancy buoyancy = new Buoyancy
      {
        AreaOfEffect = new GeometricAreaOfEffect(_waterCollisionObject),
        Surface = new Plane(Vector3F.Up, 0),

        Density = 1000f,    // The density of water (1000 kg/m³).
        AngularDrag = 0.4f,
        LinearDrag = 4f,

        // Optional: Let the objects drift in the water by setting a flow velocity. 
        //Velocity = new Vector3F(-0.5f, 0, 0.5f),
      };
      Simulation.ForceEffects.Add(buoyancy);


      // Add static area around the pool.
      RigidBody bottom = new RigidBody(new BoxShape(36, 2, 36))
      {
        MotionType = MotionType.Static,
        Pose = new Pose(new Vector3F(0, -11, 0)),
      };
      Simulation.RigidBodies.Add(bottom);
      RigidBody left = new RigidBody(new BoxShape(10, 10, 36))
      {
        MotionType = MotionType.Static,
        Pose = new Pose(new Vector3F(-13, -5, 0)),
      };
      Simulation.RigidBodies.Add(left);
      RigidBody right = new RigidBody(new BoxShape(10, 10, 36))
      {
        MotionType = MotionType.Static,
        Pose = new Pose(new Vector3F(13, -5, 0)),
      };
      Simulation.RigidBodies.Add(right);
      RigidBody front = new RigidBody(new BoxShape(16, 10, 10))
      {
        MotionType = MotionType.Static,
        Pose = new Pose(new Vector3F(0, -5, 13)),
      };
      Simulation.RigidBodies.Add(front);
      RigidBody back = new RigidBody(new BoxShape(16, 10, 10))
      {
        MotionType = MotionType.Static,
        Pose = new Pose(new Vector3F(0, -5, -13)),
      };
      Simulation.RigidBodies.Add(back);

      // ----- Add some random objects to test the effect.
      // Note: Objects swim if their density is less than the density of water. They sink
      // if the density is greater than the density of water.
      // We can define the density of objects by explicitly setting the mass.

      // Add a swimming board.
      BoxShape raftShape = new BoxShape(4, 0.3f, 4);
      MassFrame raftMass = MassFrame.FromShapeAndDensity(raftShape, Vector3F.One, 700, 0.01f, 3);
      RigidBody raft = new RigidBody(raftShape, raftMass, null)
      {
        Pose = new Pose(new Vector3F(0, 4, 0)),
      };
      Simulation.RigidBodies.Add(raft);

      // Add some boxes on top of the swimming board.
      BoxShape boxShape = new BoxShape(1, 1, 1);
      MassFrame boxMass = MassFrame.FromShapeAndDensity(boxShape, Vector3F.One, 700, 0.01f, 3);
      for (int i = 0; i < 5; i++)
      {
        RigidBody box = new RigidBody(boxShape, boxMass, null)
        {
          Pose = new Pose(new Vector3F(0, 5 + i * 1.1f, 0)),
        };
        Simulation.RigidBodies.Add(box);
      }

      // Add some "heavy stones" represented as spheres.
      SphereShape stoneShape = new SphereShape(0.5f);
      MassFrame stoneMass = MassFrame.FromShapeAndDensity(stoneShape, Vector3F.One, 2500, 0.01f, 3);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-9, 9);
        position.Y = 5;

        RigidBody stone = new RigidBody(stoneShape, stoneMass, null)
        {
          Pose = new Pose(position),
        };
        Simulation.RigidBodies.Add(stone);
      }

      // Add some very light objects.
      CylinderShape cylinderShape = new CylinderShape(0.3f, 1);
      MassFrame cylinderMass = MassFrame.FromShapeAndDensity(cylinderShape, Vector3F.One, 500, 0.01f, 3);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-9, 9);
        position.Y = 5;
        QuaternionF orientation = RandomHelper.Random.NextQuaternionF();

        RigidBody cylinder = new RigidBody(cylinderShape, cylinderMass, null)
        {
          Pose = new Pose(position, orientation),
        };
        Simulation.RigidBodies.Add(cylinder);
      }
    }
  }
}
