using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;
using DigitalRune.Mathematics;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class SpherePlaneAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestNullArgument()
    {
      PlaneSphereAlgorithm algo = new PlaneSphereAlgorithm(new CollisionDetection());
      algo.UpdateContacts(null, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestArgumentException1()
    {
      PlaneSphereAlgorithm algo = new PlaneSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      CollisionObject objectB = new CollisionObject();
      ContactSet cs = ContactSet.Create(objectA, objectB);

      algo.UpdateContacts(cs, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestArgumentException2()
    {
      PlaneSphereAlgorithm algo = new PlaneSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(1);

      ContactSet cs = ContactSet.Create(objectA, objectB);
      algo.UpdateContacts(cs, 0);
    }


    [Test]
    public void TestNoContact()
    {
      PlaneSphereAlgorithm algo = new PlaneSphereAlgorithm(new CollisionDetection());
        
        CollisionObject objectA = new CollisionObject();
        ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
        ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(5, 5, 5));

        CollisionObject objectB = new CollisionObject();
        ((GeometricObject)objectB.GeometricObject).Shape = new PlaneShape(new Vector3F(1, 1, 1).Normalized, 2);
        ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(1, 1, 1));

        ContactSet cs = ContactSet.Create(objectA, objectB);
        cs.Add(ContactHelper.CreateContact(cs, Vector3F.Zero, Vector3F.UnitX, 0, false));

        algo.UpdateContacts(cs, 0);
        Assert.AreEqual(objectA, cs.ObjectA);
        Assert.AreEqual(objectB, cs.ObjectB);
        Assert.AreEqual(0, cs.Count);
      }

    [Test]
    public void TestTouchingContact()
    {
      PlaneSphereAlgorithm algo = new PlaneSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(10, -3, 20));

      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new PlaneShape(new Vector3F(0, 1, 0).Normalized, 2);
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(1, 0, 0), QuaternionF.CreateRotationZ(-(float)Math.PI));

      ContactSet cs = ContactSet.Create(objectA, objectB);
      algo.UpdateContacts(cs, 0);

      Assert.AreEqual(objectA, cs.ObjectA);
      Assert.AreEqual(objectB, cs.ObjectB);
      Assert.AreEqual(1, cs.Count);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(10, -2, 20), cs[0].Position));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), cs[0].Normal));
      Assert.IsTrue(Numeric.AreEqual(0, cs[0].PenetrationDepth));
    }


    [Test]
    public void TestContainment()
    {
      PlaneSphereAlgorithm algo = new PlaneSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(0, -2, 0));

      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new PlaneShape(new Vector3F(0, 1, 0).Normalized, 0);
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));

      ContactSet cs = ContactSet.Create(objectA, objectB);
      algo.UpdateContacts(cs, 0);

      Assert.AreEqual(objectA, cs.ObjectA);
      Assert.AreEqual(objectB, cs.ObjectB);
      Assert.AreEqual(1, cs.Count);
      Assert.AreEqual(new Vector3F(0, -1.5f, 0), cs[0].Position);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -1, 0), cs[0].Normal));
      Assert.IsTrue(Numeric.AreEqual(3, cs[0].PenetrationDepth));

      // Test swapped case:
      cs = ContactSet.Create(objectB, objectA);
      algo.UpdateContacts(cs, 0);

      Assert.AreEqual(objectB, cs.ObjectA);
      Assert.AreEqual(objectA, cs.ObjectB);
      Assert.AreEqual(1, cs.Count);
      Assert.AreEqual(new Vector3F(0, -1.5f, 0), cs[0].Position);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), cs[0].Normal));
      Assert.IsTrue(Numeric.AreEqual(3, cs[0].PenetrationDepth));
    }


    [Test]
    public void HaveContact()
    {
      PlaneSphereAlgorithm algo = new PlaneSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(0, 2, 0));

      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new PlaneShape(new Vector3F(0, 1, 0).Normalized, 0);
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));

      Assert.AreEqual(false, algo.HaveContact(objectA, objectB));

      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(0, 1, 0));
      Assert.AreEqual(true, algo.HaveContact(objectA, objectB));
      Assert.AreEqual(true, algo.HaveContact(objectB, objectA));
    }
  }
}


