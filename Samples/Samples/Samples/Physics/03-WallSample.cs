using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "This sample demonstrates how to create a simple wall of boxes.",
    "",
    3)]
  public class WallSample : PhysicsSample
  {
    public WallSample(Microsoft.Xna.Framework.Game game)
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

      // ----- Create a wall of boxes.
      const int wallHeight = 10;
      const int wallWidth = 10;
      const float boxWidth = 1.0f;
      const float boxDepth = 0.5f;
      const float boxHeight = 0.5f;
      BoxShape boxShape = new BoxShape(boxWidth, boxHeight, boxDepth);

      // Optional: Use a small overlap between boxes to improve the stability.
      float overlap = Simulation.Settings.Constraints.AllowedPenetration * 0.5f;

      float x;
      float y = boxHeight / 2 - overlap;
      float z = -5;

      for (int i = 0; i < wallHeight; i++)
      {
        for (int j = 0; j < wallWidth; j++)
        {
          // Tip: Leave a small gap between neighbor bricks. If the neighbors on the same
          // row do not touch, the simulation has less work to do!
          x = -boxWidth * wallWidth / 2 + j * (boxWidth + 0.02f) + (i % 2) * boxWidth / 2;
          RigidBody brick = new RigidBody(boxShape)
          {
            Name = "Brick" + i,
            Pose = new Pose(new Vector3F(x, y, z)),
          };
          Simulation.RigidBodies.Add(brick);
        }

        y += boxHeight - overlap;
      }
    }
  }
}
