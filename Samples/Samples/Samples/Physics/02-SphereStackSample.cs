using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to create a stack of spheres.",
    "",
    2)]
  public class SphereStackSample : PhysicsSample
  {
    public SphereStackSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        Name = "GroundPlane",           // Names are not required but helpful for debugging.
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);

      // ----- Add a stack of spheres.
      const int numberOfSpheres = 10;
      const float sphereRadius = 0.4f;
      Shape sphereShape = new SphereShape(sphereRadius);

      // Optional: Use a small overlap between spheres to improve the stability.
      float overlap = Simulation.Settings.Constraints.AllowedPenetration * 0.5f;
      Vector3F position = new Vector3F(0, sphereRadius - overlap, 0);
      for (int i = 0; i < numberOfSpheres; i++)
      {
        RigidBody sphere = new RigidBody(sphereShape)
        {
          Name = "Sphere" + i,
          Pose = new Pose(position),
        };
        Simulation.RigidBodies.Add(sphere);
        position.Y += 2 * sphereRadius - overlap;
      }
    }
  }
}
