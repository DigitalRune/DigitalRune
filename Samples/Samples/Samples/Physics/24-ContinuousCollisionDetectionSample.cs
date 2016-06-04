using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"This sample demonstrates Discrete and Continuous Collision Detection (CCD).",
    @"Two fast bodies are shot onto a wall. The left object does not use CCD and it ""tunnels""
through the wall. The right object uses CCD and collides with the wall.",
    24)]
  public class ContinuousCollisionDetectionSample : PhysicsSample
  {
    public ContinuousCollisionDetectionSample(Microsoft.Xna.Framework.Game game)
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

      // Add a thin wall.
      RigidBody body = new RigidBody(new BoxShape(10, 10, 0.1f))
      {
        MotionType = MotionType.Static,
        Pose = new Pose(new Vector3F(0, 5, -5))
      };
      Simulation.RigidBodies.Add(body);

      // ----- Add two fast bodies that move to the wall.
      // The first object does not use CCD. (Per default, RigidBody.CcdEnabled is false.)
      // The second object uses CCD.
      SphereShape bulletShape = new SphereShape(0.2f);
      body = new RigidBody(bulletShape)
      {
        Pose = new Pose(new Vector3F(-2, 5, 5.5f)),
        LinearVelocity = new Vector3F(0, 0, -100),
      };
      Simulation.RigidBodies.Add(body);

      body = new RigidBody(bulletShape)
      {
        Pose = new Pose(new Vector3F(2, 5, 5.5f)),
        LinearVelocity = new Vector3F(0, 0, -100),

        // Enable CCD for this body.
        CcdEnabled = true,
      };
      Simulation.RigidBodies.Add(body);

      // Note:
      // Global CCD settings can be changed in Simulation.Settings.Motion.
      // CCD can be globally enabled or disabled. 
      // Per default, Simulation.Settings.Motion.CcdEnabled is true. But RigidBody.CcdEnabled
      // is false. RigidBody.CcdEnabled should be set for critical game objects.
    }
  }
}
