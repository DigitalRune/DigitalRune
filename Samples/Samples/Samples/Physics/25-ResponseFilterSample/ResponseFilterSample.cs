using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"This sample demonstrates collision response filtering. Capsules and boxes are dropped
at random positions. Collision response between capsules and boxes is disabled by using
a custom filter class.",
    @"",
    25)]
  public class ResponseFilterSample : PhysicsSample
  {
    public ResponseFilterSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Only disable collision response if you need collision detection info but no collision
      // response. If you can disable collision detection too, use 
      //   Simulation.CollisionDomain.CollisionDetection.CollisionFilter 
      // instead - this is more efficient!
      // (In this sample, a custom filter implementation is used. DigitalRune.Physics provides
      // a standard filter implementation: DigitalRune.Physics.CollisionResponseFilter.)
      Simulation.ResponseFilter = new MyCollisionResponseFilter();

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        Name = "GroundPlane",           // Names are not required but helpful for debugging.
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);

      // ----- Add boxes at random poses.
      BoxShape boxShape = new BoxShape(0.5f, 0.8f, 1.2f);
      for (int i = 0; i < 20; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-3, 3);
        position.Y = 5;
        QuaternionF orientation = RandomHelper.Random.NextQuaternionF();

        RigidBody body = new RigidBody(boxShape)
        {
          Pose = new Pose(position, orientation),
        };
        Simulation.RigidBodies.Add(body);
      }

      // ----- Add capsules at random poses.
      CapsuleShape capsuleShape = new CapsuleShape(0.3f, 1.2f);
      for (int i = 0; i < 20; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-3, 3);
        position.Y = 5;
        QuaternionF orientation = RandomHelper.Random.NextQuaternionF();

        RigidBody body = new RigidBody(capsuleShape)
        {
          Pose = new Pose(position, orientation),
        };
        Simulation.RigidBodies.Add(body);
      }
    }
  }
}
