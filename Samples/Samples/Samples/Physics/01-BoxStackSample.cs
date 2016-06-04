using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to simulate a stack of boxes.",
    "",
    1)]
  public class BoxStackSample : PhysicsSample
  {
    public BoxStackSample(Microsoft.Xna.Framework.Game game)
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

      // ----- Add a stack of boxes.
      const int numberOfBoxes = 5;
      const float boxSize = 0.8f;
      BoxShape boxShape = new BoxShape(boxSize, boxSize, boxSize);

      // Optional: Use a small overlap between boxes to improve the stability.
      float overlap = Simulation.Settings.Constraints.AllowedPenetration * 0.5f;
      Vector3F position = new Vector3F(0, boxSize / 2 - overlap, 0);
      for (int i = 0; i < numberOfBoxes; i++)
      {
        RigidBody box = new RigidBody(boxShape)
        {
          Name = "Box" + i,
          Pose = new Pose(position),
        };
        Simulation.RigidBodies.Add(box);
        position.Y += boxSize - overlap;
      }
    }
  }
}
