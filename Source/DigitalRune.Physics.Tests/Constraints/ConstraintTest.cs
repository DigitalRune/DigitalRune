using DigitalRune.Collections;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using NUnit.Framework;


namespace DigitalRune.Physics.Constraints.Tests
{
  [TestFixture]
  public class ConstraintTest
  {
    [Test]
    public void CollisinFiltering()
    {
      var simulation = new Simulation();
      var filter = (ICollisionFilter)simulation.CollisionDomain.CollisionDetection.CollisionFilter;

      var bodyA = new RigidBody(new SphereShape(1));
      simulation.RigidBodies.Add(bodyA);
      
      var bodyB = new RigidBody(new SphereShape(1));
      simulation.RigidBodies.Add(bodyB);

      var bodyC = new RigidBody(new SphereShape(1));
      simulation.RigidBodies.Add(bodyC);

      var pairAB = new Pair<CollisionObject>(bodyA.CollisionObject, bodyB.CollisionObject);
      var pairAC = new Pair<CollisionObject>(bodyA.CollisionObject, bodyC.CollisionObject);
      var pairBC = new Pair<CollisionObject>(bodyB.CollisionObject, bodyC.CollisionObject);

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsTrue(filter.Filter(pairBC));

      var constraintA = new BallJoint() { BodyA = bodyB, BodyB = bodyC, CollisionEnabled = false };
      var constraintB = new BallJoint() { BodyA = bodyB, BodyB = bodyC };

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsTrue(filter.Filter(pairBC));

      simulation.Constraints.Add(constraintB);

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsTrue(filter.Filter(pairBC));

      simulation.Constraints.Add(constraintA);

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsFalse(filter.Filter(pairBC));

      simulation.Constraints.Remove(constraintB);

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsFalse(filter.Filter(pairBC));

      simulation.Constraints.Remove(constraintA);

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsTrue(filter.Filter(pairBC));

      simulation.Constraints.Add(constraintA);
      simulation.Constraints.Add(constraintB);
      constraintB.CollisionEnabled = false;

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsFalse(filter.Filter(pairBC));

      constraintA.CollisionEnabled = true;

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsFalse(filter.Filter(pairBC));

      constraintB.CollisionEnabled = true;

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsTrue(filter.Filter(pairBC));

      constraintA.CollisionEnabled = false;
      constraintB.CollisionEnabled = false;

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsFalse(filter.Filter(pairBC));

      simulation.Constraints.Remove(constraintA);

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsFalse(filter.Filter(pairBC));

      simulation.Constraints.Clear();

      Assert.IsTrue(filter.Filter(pairAB));
      Assert.IsTrue(filter.Filter(pairAC));
      Assert.IsTrue(filter.Filter(pairBC));
    }

  }
}
