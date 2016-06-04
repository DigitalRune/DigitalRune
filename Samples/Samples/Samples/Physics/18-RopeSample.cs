using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"Several capsules are connected with UniversalJoints to create an object similar to a
rope or a chain.",
    @"",
    18)]
  public class RopeSample : PhysicsSample
  {
    public RopeSample(Microsoft.Xna.Framework.Game game)
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

      const float capsuleHeight = 0.6f;
      CapsuleShape shape = new CapsuleShape(0.15f, capsuleHeight);
      for (int i = 0; i < 20; i++)
      {
        // A segment of the rope:
        RigidBody body = new RigidBody(shape)
        {
          Pose = new Pose(new Vector3F(0, 1 + i * capsuleHeight, 0)),
        };
        Simulation.RigidBodies.Add(body);

        if (i > 0)
        {
          // Connect the last body with the current body using a UniversalJoint that
          // allows rotations on two axes (no twist).
          RigidBody lastBody = Simulation.RigidBodies[i];
          UniversalJoint ballJoint = new UniversalJoint
          {
            BodyA = lastBody,
            // This attachment point is a the top of the first capsule.
            // The universal joint allows rotations around the first and the second axis.
            // The last axis is the twist axis where no rotation is allowed.
            // --> To define the constraint anchor orientation:
            // The columns are the axes. We set the local x axis in the first column and the
            // -z axis in the second column. The third column is y axis about which no rotation
            // is allowed. 
            // (All three columns are orthonormal and form a valid rotation matrix.)
            AnchorPoseALocal = new Pose(new Vector3F(0, capsuleHeight / 2, 0),
                                        new Matrix33F(1, 0, 0,
                                                      0, 0, 1,
                                                      0, -1, 0)),

            BodyB = body,
            // This attachment point is at the bottom of the second capsule.
            // The anchor orientation is defined as above.
            AnchorPoseBLocal = new Pose(new Vector3F(0, -capsuleHeight / 2, 0),
                                        new Matrix33F(1, 0, 0,
                                                      0, 0, 1,
                                                      0, -1, 0)),

            // Disable collision between body A and B.
            CollisionEnabled = false,

            // ErrorReduction and Softness are tweaked to create an appropriate amount
            // of springiness and damping.
            ErrorReduction = 0.1f,
            Softness = 0.0001f,

            // We allow a +/- 60° rotation around the first and the second constraint axis.
            Minimum = -new Vector2F(MathHelper.ToRadians(60), MathHelper.ToRadians(60)),
            Maximum = new Vector2F(MathHelper.ToRadians(60), MathHelper.ToRadians(60))
          };
          Simulation.Constraints.Add(ballJoint);
        }
        else
        {
          // This is the first body. There is no former body to link too. 
          // We set a velocity for this body to give the rope an initial movement.
          body.LinearVelocity = new Vector3F(1, 0, 0);
        }
      }
    }
  }
}
