using System;
using DigitalRune.Game.Input;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using DigitalRune.Physics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"This sample demonstrates how to optimize simulation settings for better performance.",
    @"This example shows various settings that may influence performance and shows how to tune
the simulation for performance. Many settings that improve performance make the simulation
less stable. Which settings are acceptable depends on the simulation scenario.",
    28)]
  [Controls(@"Sample
  Press <Y> on keyboard or on gamepad to drop random bodies.")]
  public class PerformanceSample : PhysicsSample
  {
    // A list of rigid bodies that will be used to define new bodies.
    private RigidBody[] _prototypes;


    public PerformanceSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // ----- Multithreading
      // If the scene contains many active (= moving objects), using multithreading is 
      // recommended. Multithreading does not improve very static scenes where most bodies
      // are not moving or scenes with very little bodies. 
      // If multithreading is enabled in the collision domain, the narrow phase algorithms
      // (which determines contacts) are executed in parallel.
      Simulation.CollisionDomain.EnableMultithreading = true;
      // If multithreading is enabled in the simulation, the simulation islands are solved
      // in parallel and a few other things run in parallel.
      Simulation.Settings.EnableMultithreading = true;

      // The processor affinity and the number of threads can be controlled with these properties:
      //Parallel.ProcessorAffinity = new[] { 4, 3, 5 };    // Threads use the cores 3, 4, 5.
      //Parallel.Scheduler = new WorkStealingScheduler(3); // Use 3 worker threads.
      // The default settings should be ok for most scenarios.

      // ----- Collision Detection Settings
      // We disable the flag FullContactSetPerFrame. If the flag is disabled, the collision
      // detection is faster because in the narrow phase some algorithms will compute only
      // one new contact between two touching bodies.
      // If the flag is enabled, the simulation is more stable because the narrow phase computes
      // more contacts per pair of touching bodies.
      Simulation.CollisionDomain.CollisionDetection.FullContactSetPerFrame = false;

      // ----- Physics Settings
      // If SynchronizeCollisionDomain is false, the collision detection is run only at the 
      // beginning of Simulation.Update(). If SynchronizeCollisionDomain is set, the collision 
      // detection is also performed at the end. This is necessary in case you need to make manual 
      // collision detection queries and need up-to-date collision detection info.
      // Disable this flag if you do not need it.
      Simulation.Settings.SynchronizeCollisionDomain = false;
      // The MinConstraintImpulse defines when the constraint solver will stop its iterative
      // process. A higher limit will make the solver stop earlier (=> faster, but less stable).
      Simulation.Settings.Constraints.MinConstraintImpulse = 0.0001f;
      // NumberOfConstraintIterations defines how many iterations the solver performs at max.
      // Values from 4 to 20 are normal. Use higher values if stable stacking is required.
      Simulation.Settings.Constraints.NumberOfConstraintIterations = 4;
      // Randomization of constraints takes a tiny bit of time and helps to make stacks and
      // complex scenes more stable. For simple scenes we can disable it.
      Simulation.Settings.Constraints.RandomizeConstraints = false;
      // Continuous collision detection cost a bit performance. We are faster if we disable it
      // but with disabled CCD balls (right mouse button) will fly through objects because of 
      // their high speed.
      Simulation.Settings.Motion.CcdEnabled = false;
      // If RemoveBodiesOutsideWorld is set, the simulation automatically removes bodies that 
      // leave the simulation (defined with Simulation.World). Disable it if not needed.
      Simulation.Settings.Motion.RemoveBodiesOutsideWorld = false;
      // TimeThreshold defines how fast bodies are deactivated. Normal values are 1 or 2 seconds.
      // We can set it to a low value, e.g. 0.5 s, for a very aggressive sleeping. The negative
      // effects of this are that bodies that are slowly falling over, can freeze in a tilted
      // position. 
      // You can also try to disable sleeping by setting TimeThreshold to float.MaxValue. But the
      // simulation will run significantly slower. You can run the PhysicsSample and compare the 
      // simulation times with enabled and disabled sleeping.
      Simulation.Settings.Sleeping.TimeThreshold = 0.5f;
      // FixedTimeStep defines the size of a single simulation step. Per default, the smallest
      // step is 1 / 60 s (60 fps). In some cases it is ok to use an even larger time step
      // like 1 / 30. But with large time steps stacks and walls will not be stable.
      Simulation.Settings.Timing.FixedTimeStep = 1.0f / 60.0f;
      // If the simulation gets complex the game will need more time to compute each frame.
      // If the game becomes very slow, Simulation.Update(elapsedTime) will be called with 
      // a large elapsedTime. If our frame rate drops to 30 fps, Simulation.Update(1/30) will
      // internally make 2 sub-time steps (if FixedTimeStep = 1/60). This could make the problem
      // worse and if we expect such a situation we should limit the number of sub steps to 1.
      // Then, if the game is running slowly, the physics simulation will run in slow motion -
      // but at least it will not freeze the game.
      Simulation.Settings.Timing.MaxNumberOfSteps = 1;

      // ----- Force Effects.
      // Using a low gravity is common trick to make the simulation more stable:
      Simulation.ForceEffects.Add(new Gravity { Acceleration = new Vector3F(0, -5, 0) });
      // Using high damping coefficients helps to make your simulation faster and more stable
      // because objects will come the rest much quicker. - But too high values can create a
      // very unrealistic damped body movement.
      Simulation.ForceEffects.Add(new Damping { LinearDamping = 0.3f, AngularDamping = 0.3f });

      // ----- Rigid Body Prototypes
      // Here we create 3 rigid bodies that will serve as templates for the new random bodies
      // that are created in Update().
      // We use the same material instance for all rigid bodies to avoid the creation of several 
      // material instances.
      var material = new UniformMaterial();
      _prototypes = new RigidBody[3];
      _prototypes[0] = new RigidBody(new SphereShape(0.5f), null, material);
      _prototypes[1] = new RigidBody(new CylinderShape(0.4f, 0.9f), null, material);
      _prototypes[2] = new RigidBody(new BoxShape(0.9f, 0.9f, 0.9f), null, material);

      // ----- Height Field
      // Create a height field.
      var numberOfSamplesX = 30;
      var numberOfSamplesZ = 30;
      var samples = new float[numberOfSamplesX * numberOfSamplesZ];
      for (int z = 0; z < numberOfSamplesZ; z++)
        for (int x = 0; x < numberOfSamplesX; x++)
          samples[z * numberOfSamplesX + x] = (float)(Math.Cos(z / 2f) * Math.Sin(x / 2f) * 3f + 5f);
      var heightField = new HeightField(0, 0, 100, 100, samples, numberOfSamplesX, numberOfSamplesZ);

      // We can set following flag to get a significant performance gain - but the collision
      // detection will be less accurate. For smooth height fields this flag can be set.
      heightField.UseFastCollisionApproximation = true;

      // Create a static rigid body using the height field and add it to the simulation.
      // The mass of static rigid bodies is not relevant, therefore we use a default 
      // mass frame instance as the second constructor parameter. If we do not specify
      // the mass frame, the physics library will try to compute a suitable mass frame
      // which can take some time for large meshes.
      RigidBody landscape = new RigidBody(heightField, new MassFrame(), material)
      {
        Pose = new Pose(new Vector3F(-50, 0, -50f)),
        MotionType = MotionType.Static,
      };
      Simulation.RigidBodies.Add(landscape);
    }


    public override void Update(GameTime gameTime)
    {
      // If Y is pressed, add a random body.
      if (InputService.IsDown(Keys.Y) || InputService.IsDown(Buttons.Y, LogicalPlayerIndex.One))
      {
        // Choose one of the prototypes randomly.
        RigidBody prototype = _prototypes[RandomHelper.Random.NextInteger(0, _prototypes.Length - 1)];

        // Create a new body. 
        // The new body shares the shape, mass frame and material instances with the prototype.
        RigidBody newBody = new RigidBody(prototype.Shape, prototype.MassFrame, prototype.Material);
        Vector3F randomPosition = new Vector3F(
          RandomHelper.Random.NextInteger(-40, 40),
          10,
          RandomHelper.Random.NextInteger(-40, 40));
        QuaternionF randomOrientation = RandomHelper.Random.NextQuaternionF();
        newBody.Pose = new Pose(randomPosition, randomOrientation);

        Simulation.RigidBodies.Add(newBody);
      }

      base.Update(gameTime);
    }
  }
}
