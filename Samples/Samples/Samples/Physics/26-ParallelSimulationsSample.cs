using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics;
using DigitalRune.Physics.ForceEffects;
using Microsoft.Xna.Framework;


namespace Samples.Physics
{
  [Sample(SampleCategory.Physics,
    @"This sample demonstrates that several Simulation instances can run parallel.",
    @"A stack of boxes on the right uses a separate Simulation instance that is owned by this
game components. A stack of boxes on the left is simulated in the default Simulation of
the game.
Parallel simulations can be used to compare the effects of changed simulation settings.
The right box stack is simulated with bad simulation settings.",
    26)]
  public class ParallelSimulationsSample : PhysicsSample
  {
    // A second simulation that runs parallel to the normal simulation.
    private Simulation _secondSimulation;


    public ParallelSimulationsSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Initialize the default simulation with a box stack on the left.
      Simulation firstSimulation = Simulation;
      InitializeSimulation(firstSimulation, new Vector3F(-3, 0, 0));

      // Initialize a second simulation with a box stack on the right.
      _secondSimulation = new Simulation();
      InitializeSimulation(_secondSimulation, new Vector3F(3, 0, 0));

      // Disable sleeping so that we can see the result of changed simulation settings and
      // bodies are not automatically disabled.
      firstSimulation.Settings.Sleeping.TimeThreshold = float.PositiveInfinity;
      _secondSimulation.Settings.Sleeping.TimeThreshold = float.PositiveInfinity;

      // ----- Let's play around with simulation settings.
      // Reduce the number of constraint iterations. - This makes the simulation faster and
      // less accurate/stable.
      _secondSimulation.Settings.Constraints.NumberOfConstraintIterations = 4;

      // The "BaumgarteRatio" is an experimental parameter that changes error correction method.
      // Setting this value to 0 makes the simulation a bit slower but less "bouncy".
      //_secondSimulation.Settings.Constraints.BaumgarteRatio = 0;

      // If we set a stacking factor > 0, we can make right stack stable - even with reduced
      // constraint iteration count.
      //_secondSimulation.Settings.Constraints.StackingFactor = 5;
    }


    private void InitializeSimulation(Simulation simulation, Vector3F offset)
    {
      // Add default force effects.
      simulation.ForceEffects.Add(new Gravity());
      simulation.ForceEffects.Add(new Damping());

      // Add a ground plane.
      RigidBody groundPlane = new RigidBody(new PlaneShape(Vector3F.UnitY, 0))
      {
        Name = "GroundPlane",
        MotionType = MotionType.Static,
      };
      simulation.RigidBodies.Add(groundPlane);

      // Add a stack of boxes.
      const float boxSize = 0.8f;
      float overlap = simulation.Settings.Constraints.AllowedPenetration * 0.5f;
      float yPosition = boxSize / 2 - overlap;
      BoxShape boxShape = new BoxShape(boxSize, boxSize, boxSize);
      for (int i = 0; i < 15; i++)
      {
        RigidBody stackBox = new RigidBody(boxShape)
        {
          Name = "StackBox" + i,
          Pose = new Pose(new Vector3F(0, yPosition, 0) + offset),
        };
        simulation.RigidBodies.Add(stackBox);
        yPosition += boxSize - overlap;
      }
    }


    public override void Update(GameTime gameTime)
    {
      // The first simulation is owned by the game and updated in SampleGame.cs.
      // The second simulation is owned by this game component and must be updated here:
      _secondSimulation.Update(gameTime.ElapsedGameTime);

      // Let the base class render the rigid bodies of the first simulation.
      base.Update(gameTime);

      // Draw rigid bodies of the second simulation.
      foreach (var body in _secondSimulation.RigidBodies)
      {
        var color = Color.Gray;
        // Draw dynamic and kinematic/static and optionally sleeping bodies with different colors.
        if (body.MotionType == MotionType.Static)
          color = Color.LightGray;

        var debugRenderer = GraphicsScreen.DebugRenderer;
        debugRenderer.DrawObject(body, color, false, false);
      }
    }
  }
}
