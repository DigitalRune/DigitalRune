using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to create bodies with different mass.",
    "",
    5)]
  public class MassSample : PhysicsSample
  {
    public MassSample(Microsoft.Xna.Framework.Game game)
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

      // Add a static body that serves as the base of the see-saw.
      RigidBody body = new RigidBody(new BoxShape(0.1f, 1, 2))
      {
        MotionType = MotionType.Static,
        Pose = new Pose(new Vector3F(0, 0.5f, 0))
      };
      Simulation.RigidBodies.Add(body);

      // Create a plank.
      body = new RigidBody(new BoxShape(5, 0.1f, 1.3f))
      {
        Pose = new Pose(new Vector3F(0, 1.05f, 0))
      };
      Simulation.RigidBodies.Add(body);

      // ----- Create a few light bodies on the left.
      Shape boxShape = new BoxShape(0.7f, 0.7f, 0.7f);

      // The light bodies have a density of 200.
      // (The first three parameters of FromShapeAndDensity are: shape, scale, density.
      // The last two parameters are required for shapes where the mass properties can only
      // be approximated using an iterative procedure: 0.01 --> The shape is approximated
      // up to approx. 1%. The procedure aborts after 3 iterations. 
      // Since the shape is a box FromShapeAndDensity computes the exact mass and the last two
      // parameters are irrelevant in this case.)
      MassFrame mass = MassFrame.FromShapeAndDensity(boxShape, Vector3F.One, 200, 0.01f, 3);

      body = new RigidBody(boxShape, mass, null)
      {
        Pose = new Pose(new Vector3F(-1.5f, 2f, 0))
      };
      Simulation.RigidBodies.Add(body);

      body = new RigidBody(boxShape, mass, null)
      {
        Pose = new Pose(new Vector3F(-1.5f, 2.7f, 0))
      };
      Simulation.RigidBodies.Add(body);

      body = new RigidBody(boxShape, mass, null)
      {
        Pose = new Pose(new Vector3F(-1.5f, 3.4f, 0))
      };
      Simulation.RigidBodies.Add(body);

      // ----- Create a heavy body on the right.
      // The heavy body has a density of 2000.
      mass = MassFrame.FromShapeAndDensity(boxShape, Vector3F.One, 2000, 0.01f, 3);
      body = new RigidBody(boxShape, mass, null)
      {
        Pose = new Pose(new Vector3F(1.5f, 3, 0))
      };
      Simulation.RigidBodies.Add(body);
    }
  }
}
