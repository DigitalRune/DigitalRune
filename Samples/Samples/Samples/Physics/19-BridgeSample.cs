using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    "Several boxes are connected with HingeJoints to create a suspension bridge.",
    "",
    19)]
  public class BridgeSample : PhysicsSample
  {
    public BridgeSample(Microsoft.Xna.Framework.Game game)
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

      // We add another damping effect that acts only on the suspension bridge parts.
      // This damping uses higher damping factors than the standard damping. It makes the
      // bridge movement smoother and more stable.
      // We use a ListAreaOfEffect. So the additional damping acts only on bodies in this list.
      ListAreaOfEffect boardList = new ListAreaOfEffect(new List<RigidBody>());
      Damping damping = new Damping
      {
        AreaOfEffect = boardList,
        AngularDamping = 1f,
        LinearDamping = 0.5f
      };
      Simulation.ForceEffects.Add(damping);

      const int numberOfBoards = 20;
      BoxShape boardShape = new BoxShape(0.8f, 0.1f, 1.5f);
      RigidBody lastBoard = null;
      for (int i = 0; i < numberOfBoards; i++)
      {
        // A single plank of the bridge.
        RigidBody body = new RigidBody(boardShape)
        {
          Pose = new Pose(new Vector3F(-10 + boardShape.WidthX * i, 4, 0))
        };
        Simulation.RigidBodies.Add(body);

        // Add the body to the list of the additional damping force effect.
        boardList.RigidBodies.Add(body);

        if (lastBoard != null)
        {
          // Connect the last body with current body using a hinge.
          HingeJoint hinge = new HingeJoint
          {
            BodyA = lastBoard,
            // The attachment point is at the right side of the board.
            // --> To define the constraint anchor orientation:
            // The columns are the axes. We set the local z axis in the first column. This is
            // the hinge axis. In the other two columns we set two orthonormal vectors.
            // (All three columns are orthonormal and form a valid rotation matrix.)
            AnchorPoseALocal = new Pose(new Vector3F(boardShape.WidthX / 2, 0, 0),
                                        new Matrix33F(0, 0, -1,
                                                      0, 1, 0,
                                                      1, 0, 0)),
            BodyB = body,
            // The attachment point is at the left side of the board.
            // The anchor orientation is defined as above.
            AnchorPoseBLocal = new Pose(new Vector3F(-boardShape.WidthX / 2, 0, 0),
                                        new Matrix33F(0, 0, -1,
                                                      0, 1, 0,
                                                      1, 0, 0)),
            CollisionEnabled = false,
            // ErrorReduction and Softness are tweaked to get a stable and smooth bridge 
            // movement.
            ErrorReduction = 0.3f,
            Softness = 0.00005f,
          };
          Simulation.Constraints.Add(hinge);
        }
        else if (i == 0)
        {
          // To attach the bridge somewhere, connect the the first board to a fixed position in the 
          // world.
          HingeJoint hinge = new HingeJoint
          {
            BodyA = Simulation.World,
            AnchorPoseALocal = new Pose(new Vector3F(-9, 3, 0),
                                        new Matrix33F(0, 0, -1,
                                                      0, 1, 0,
                                                      1, 0, 0)),
            BodyB = body,
            AnchorPoseBLocal = new Pose(new Vector3F(-boardShape.WidthX / 2, 0, 0),
                                        new Matrix33F(0, 0, -1,
                                                      0, 1, 0,
                                                      1, 0, 0)),
          };
          Simulation.Constraints.Add(hinge);
        }

        if (i == numberOfBoards - 1)
        {
          // To attach the bridge somewhere, connect the the last board to a fixed position in the 
          // world.
          HingeJoint hinge = new HingeJoint
          {
            BodyA = Simulation.World,
            AnchorPoseALocal = new Pose(new Vector3F(9, 3, 0),
                                        new Matrix33F(0, 0, -1,
                                                      0, 1, 0,
                                                      1, 0, 0)),
            BodyB = body,
            AnchorPoseBLocal = new Pose(new Vector3F(boardShape.WidthX / 2, 0, 0),
                                        new Matrix33F(0, 0, -1,
                                                      0, 1, 0,
                                                      1, 0, 0)),
          };
          Simulation.Constraints.Add(hinge);
        }

        lastBoard = body;
      }

      // The bridge is ready.
      // Now, add some ramps so that the character controller can walk up to the bridge.
      BoxShape rampShape = new BoxShape(10, 10, 2);
      RigidBody ramp0 = new RigidBody(rampShape)
      {
        Pose = new Pose(new Vector3F(-12.5f, -3f, 0), Matrix33F.CreateRotationZ(0.3f)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(ramp0);
      RigidBody ramp1 = new RigidBody(rampShape)
      {
        Pose = new Pose(new Vector3F(12.5f, -3f, 0), Matrix33F.CreateRotationZ(-0.3f)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(ramp1);

      // Drop a few light boxes onto the bridge.
      BoxShape boxShape = new BoxShape(1, 1, 1);
      MassFrame boxMass = MassFrame.FromShapeAndDensity(boxShape, Vector3F.One, 100, 0.01f, 3);
      for (int i = 0; i < 10; i++)
      {
        Vector3F randomPosition = new Vector3F(RandomHelper.Random.NextFloat(-10, 10), 5, 0);
        QuaternionF randomOrientation = RandomHelper.Random.NextQuaternionF();
        RigidBody body = new RigidBody(boxShape, boxMass, null)
        {
          Pose = new Pose(randomPosition, randomOrientation),
        };
        Simulation.RigidBodies.Add(body);
      }
    }
  }
}
