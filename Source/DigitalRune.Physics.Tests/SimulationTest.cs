using NUnit.Framework;


namespace DigitalRune.Physics.Tests
{
  [TestFixture]
  public class SimulationTest
  {
    [Test]
    public void EmptySimulation()
    {
      Simulation simulation = new Simulation();
      Assert.IsNotNull(simulation.CollisionDomain);
      Assert.IsEmpty(simulation.CollisionDomain.CollisionObjects);
      Assert.IsNotNull(simulation.Constraints);
      Assert.IsEmpty(simulation.Constraints);
      Assert.IsNotNull(simulation.ContactConstraints);
      Assert.IsEmpty(simulation.ContactConstraints);
      Assert.IsNotNull(simulation.ForceEffects);
      Assert.IsEmpty(simulation.ForceEffects);
      Assert.IsNotNull(simulation.IslandManager);
      Assert.IsEmpty(simulation.IslandManager.Islands);
      Assert.IsNotNull(simulation.RigidBodies);
      Assert.IsEmpty(simulation.RigidBodies);
      Assert.IsNotNull(simulation.Settings);
      Assert.IsNotNull(simulation.World);

      simulation.Update(0);
      simulation.Update(1);

      Assert.IsEmpty(simulation.CollisionDomain.CollisionObjects);
      Assert.IsEmpty(simulation.Constraints);
      Assert.IsEmpty(simulation.ContactConstraints);
      Assert.IsEmpty(simulation.ForceEffects);
      Assert.IsEmpty(simulation.IslandManager.Islands);
      Assert.IsEmpty(simulation.RigidBodies);
    }
  }
}
