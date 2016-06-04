using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using DigitalRune.Physics.Materials;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to create bodies with different restitution or friction.",
    "",
    6)]
  public class MaterialSample : PhysicsSample
  {
    public MaterialSample(Microsoft.Xna.Framework.Game game)
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

      // Adjust the coefficient of restitution of the ground plane.
      // (By default, the material of a rigid body is of type UniformMaterial.)
      ((UniformMaterial)groundPlane.Material).Restitution = 1;  // Max. bounciness. (The simulation actually 
                                                                // accepts higher values, but these usually
                                                                // lead to unnatural bounciness.)

      Simulation.RigidBodies.Add(groundPlane);

      // Add a static inclined ground plane.
      RigidBody inclinedPlane = new RigidBody(new PlaneShape(new Vector3F(-0.3f, 1f, 0).Normalized, 0))
      {
        Name = "InclinedPlane",
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(inclinedPlane);

      // Create a few boxes with different coefficient of friction.
      BoxShape boxShape = new BoxShape(1, 1, 1);
      for (int i = 0; i < 5; i++)
      {
        // Each box gets a different friction value.
        UniformMaterial material = new UniformMaterial
        {
          DynamicFriction = i * 0.2f,
          StaticFriction = i * 0.2f,
        };

        RigidBody box = new RigidBody(boxShape, null, material) // The second argument (the mass frame) is null. The 
        {                                                       // simulation will automatically compute a default mass.
          Name = "Box" + i,
          Pose = new Pose(new Vector3F(5, 6, -5 + i * 2)),
        };

        Simulation.RigidBodies.Add(box);
      }

      // Create a few balls with different coefficient of restitution (= bounciness).
      Shape sphereShape = new SphereShape(0.5f);
      for (int i = 0; i < 6; i++)
      {
        // Vary restitution between 0 and 1.
        UniformMaterial material = new UniformMaterial
        {
          Restitution = i * 0.2f
        };

        RigidBody body = new RigidBody(sphereShape, null, material)
        {
          Name = "Ball" + i,
          Pose = new Pose(new Vector3F(-1 - i * 2, 5, 0)),
        };

        Simulation.RigidBodies.Add(body);
      }
    }
  }
}
