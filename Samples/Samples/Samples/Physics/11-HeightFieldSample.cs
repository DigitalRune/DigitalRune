using System;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to create a landscape using a height field.",
    "",
    11)]
  public class HeightFieldSample : PhysicsSample
  {
    public HeightFieldSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Create a height field.
      // The height data consists of height samples with a resolution of 20 entries per dimension.
      // (Height samples are stored in a 1-dimensional array.)
      var numberOfSamplesX = 20;
      var numberOfSamplesZ = 20;
      var samples = new float[numberOfSamplesX * numberOfSamplesZ];
      for (int z = 0; z < numberOfSamplesZ; z++)
      {
        for (int x = 0; x < numberOfSamplesX; x++)
        {
          // Set the y values (height).
          samples[z * numberOfSamplesX  + x] = (float)(Math.Cos(z / 2f) * Math.Sin(x / 2f) * 5f + 5f);
        }
      }

      // The height field has a size of 100 m x 100 m.
      float widthX = 100;
      float widthZ = 100;

      // The origin is at (-50, -50) to center the height field at the world space origin.
      float originX = -50;
      float originZ = -50;

      // Create the height field shape.
      HeightField heightField = new HeightField(
        originX, originZ, widthX, widthZ, samples, numberOfSamplesX, numberOfSamplesZ);

      // We can set following flag to get a significant performance gain - but the collision
      // detection will be less accurate. For smooth height fields this flag can be set.
      heightField.UseFastCollisionApproximation = true;

      // Create a static rigid body using the height field and add it to the simulation.
      RigidBody landscape = new RigidBody(heightField)
      {
        Pose = Pose.Identity,
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(landscape);

      // Distribute a few spheres and boxes across the landscape.
      SphereShape sphereShape = new SphereShape(0.5f);
      for (int i = 0; i < 30; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-30, 30);
        position.Y = 20;

        RigidBody body = new RigidBody(sphereShape)
        {
          Pose = new Pose(position),
        };
        Simulation.RigidBodies.Add(body);
      }

      BoxShape boxShape = new BoxShape(1, 1, 1);
      for (int i = 0; i < 30; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-30, 30);
        position.Y = 20;

        RigidBody body = new RigidBody(boxShape)
        {
          Pose = new Pose(position),
        };
        Simulation.RigidBodies.Add(body);
      }
    }
  }
}
