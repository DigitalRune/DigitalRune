using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"This sample demonstrates how to change simulation settings if the game objects use a 
different scale.",
    @"Look closely - there are small objects in the middle of the scene.",
    27)]
  public class SmallerScaleSample : PhysicsSample
  {
    public SmallerScaleSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // The default simulation settings are optimized for game objects that are 1 unit in size.
      // These default settings are useful for standard game objects: crates, barrels, furniture, 
      // humans, rocks, etc. 
      // The simulation settings should be adapted if the average game object is more than 10 times
      // bigger or smaller. (Note: Future versions of DigitalRune.Physics will automatically 
      // adapt the simulation settings.)
      // If you are unsure which settings are relevant for your game scenario, just ask in our
      // support forums: http://www.digitalrune.com/Support/Forum.aspx
      // Nevertheless, it is strongly recommended to scale game objects so that the average game 
      // object is about 1 unit in size.

      // In this sample, the average game object is 0.05 units. 

      // Simulating small objects is a lot more difficult. Therefore, we decrease the time
      // step size to improve the simulation accuracy.
      Simulation.Settings.Timing.FixedTimeStep /= 5;
      Simulation.Settings.Timing.MaxNumberOfSteps *= 5;

      // Rigid bodies have an allowed penetration. Errors in this range are acceptable and not
      // corrected to improve stability and remove jittering. For small objects this limit
      // must be reduced.
      Simulation.Settings.Constraints.AllowedPenetration /= 5;

      // The collision detection settings are defined in the CollisionDetection class.
      // The collision detection uses a tolerance to define when two contacts near each other
      // can be considered the same contact. For small objects this limit must be reduced.
      Simulation.CollisionDomain.CollisionDetection.ContactPositionTolerance /= 5;

      // To improve stacking of small objects:
      Simulation.Settings.Constraints.StackingFactor = 10;

      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping { AngularDamping = 0.9f });

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        Name = "GroundPlane",
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);

      // ----- Add a stack of small boxes.
      const float boxSize = 0.05f;
      float overlap = Simulation.Settings.Constraints.AllowedPenetration * 0.5f;
      float yPosition = boxSize / 2 - overlap;
      BoxShape boxShape = new BoxShape(boxSize, boxSize, boxSize);
      for (int i = 0; i < 10; i++)
      {
        RigidBody stackBox = new RigidBody(boxShape)
        {
          Pose = new Pose(new Vector3F(0, yPosition, 0)),
        };
        Simulation.RigidBodies.Add(stackBox);
        yPosition += boxSize - overlap;
      }

      // ------ Add spheres at random positions.
      SphereShape sphereShape = new SphereShape(boxSize * 0.5f);
      for (int i = 0; i < 10; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-0.5f, 0.5f);
        position.Y = 1;

        RigidBody body = new RigidBody(sphereShape)
        {
          Pose = new Pose(position),
        };
        Simulation.RigidBodies.Add(body);
      }
    }
  }
}
