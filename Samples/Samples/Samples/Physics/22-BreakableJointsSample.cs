using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.Constraints;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"Several ragdolls are created. This game component monitors the constraint forces and
disables joints where the constraint force was too high - the joint breaks.",
    "Shake ragdoll or shoot at it to destroy it.",
    22)]
  public class BreakableJointsSample : PhysicsSample
  {
    public BreakableJointsSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add basic force effects.
      Simulation.ForceEffects.Add(new Gravity());
      Simulation.ForceEffects.Add(new Damping());

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        Name = "GroundPlane",            // Names are not required but helpful for debugging.
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(groundPlane);

      // Add ragdolls. We use the Ragdoll-creation method of Sample21.
      for (int i = 0; i < 5; i++)
      {
        Vector3F position = RandomHelper.Random.NextVector3F(-3, 3);
        position.Y = 1 + i;
        RagdollSample.AddRagdoll(Simulation, 2f, position, 0.0001f, false);
      }
    }


    public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
    {
      // We assume that joints break when the constraint force exceeds this limit:
      const float breakForceLimit = 50000;

      // The simulation uses impulses. Therefore, we compute an equivalent impulse:
      // impulse = force * deltaTime
      float breakImpulseLimit = breakForceLimit * Simulation.Settings.Timing.FixedTimeStep;

      // Loop over constraints.
      foreach (Constraint constraint in Simulation.Constraints)
      {
        // Skip joints that are already disabled. Or that involve the world. 
        // (Ragdoll joints are always between two dynamic bodies, and we don't want to 
        // break the mouse grab joint that is between a body and the world.)
        if (!constraint.Enabled
            || constraint.BodyA == Simulation.World
            || constraint.BodyB == Simulation.World)
        {
          continue;
        }

        // Disable constraint if the constraint impulse of the last time step was 
        // above limit.
        if (constraint.LinearConstraintImpulse.Length > breakImpulseLimit)
        {
          constraint.Enabled = false;
        }
      }

      base.Update(gameTime);
    }
  }
}
