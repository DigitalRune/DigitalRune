using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"A custom force effect creates a gravity effect that pulls objects to the origin. Similar 
to the gravity of a planet.",
    @"",
    14)]
  public class CustomGravitySample : PhysicsSample
  {
    public CustomGravitySample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new CustomGravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a static sphere that represents the planet.
      RigidBody planet = new RigidBody(new SphereShape(5))
      {
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(planet);

      // ----- Add a few cylinder and sphere bodies at random positions above the planet.
      Shape cylinderShape = new CylinderShape(0.3f, 1);
      for (int i = 0; i < 10; i++)
      {
        // A random position 10 m above the planet center.
        Vector3F randomPosition = RandomHelper.Random.NextVector3F(-1, 1);
        randomPosition.Length = 10;

        RigidBody body = new RigidBody(cylinderShape)
        {
          Pose = new Pose(randomPosition),
        };
        Simulation.RigidBodies.Add(body);
      }

      Shape sphereShape = new SphereShape(0.5f);
      for (int i = 0; i < 10; i++)
      {
        Vector3F randomPosition = RandomHelper.Random.NextVector3F(-1, 1);
        randomPosition.Length = 10;

        RigidBody body = new RigidBody(sphereShape)
        {
          Pose = new Pose(randomPosition),
        };
        Simulation.RigidBodies.Add(body);
      }
    }
  }
}
