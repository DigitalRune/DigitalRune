using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"Several bodies are created with random positions and orientations. The rotations of the
bodies are locked. - Just to demonstrate the effect of the RigidBody.LockRotation flags.",
    "",
    23)]
  public class LockRotationSample : PhysicsSample
  {
    public LockRotationSample(Microsoft.Xna.Framework.Game game)
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

      // Next, we add boxes and capsules in random positions and orientations.
      // For all bodies the flags LockRotationX/Y/Z are set. This will prevent all
      // rotation that would be caused by forces. (It is still allowed to manually 
      // change the rotation or to set an angular velocity.)

      BoxShape boxShape = new BoxShape(0.5f, 0.8f, 1.2f);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-10, 10);
        position.Y = 5;
        QuaternionF orientation = RandomHelper.Random.NextQuaternionF();

        RigidBody body = new RigidBody(boxShape)
        {
          Pose = new Pose(position, orientation),
          LockRotationX = true,
          LockRotationY = true,
          LockRotationZ = true,
        };
        Simulation.RigidBodies.Add(body);
      }

      CapsuleShape capsuleShape = new CapsuleShape(0.3f, 1.2f);
      for (int i = 0; i < 10; i++)
      {
        Vector3F randomPosition = RandomHelper.Random.NextVector3F(-10, 10);
        randomPosition.Y = 5;
        QuaternionF randomOrientation = RandomHelper.Random.NextQuaternionF();

        RigidBody body = new RigidBody(capsuleShape)
        {
          Pose = new Pose(randomPosition, randomOrientation),
          LockRotationX = true,
          LockRotationY = true,
          LockRotationZ = true,
        };
        Simulation.RigidBodies.Add(body);
      }
    }
  }
}
