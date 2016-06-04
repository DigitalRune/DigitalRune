using System;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;
using DigitalRune.Mathematics;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class SphereSphereAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestNullArgument()
    {
      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());
      algo.UpdateContacts(null, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestArgumentException1()
    {
      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      CollisionObject objectB = new CollisionObject();
      ContactSet cs = ContactSet.Create(objectA, objectB);

      algo.UpdateContacts(cs, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestArgumentException2()
    {
      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
      CollisionObject objectB = new CollisionObject();

      ContactSet cs = ContactSet.Create(objectA, objectB);
      algo.UpdateContacts(cs, 0);
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestArgumentException3()
    {
      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();      
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(1);

      ContactSet cs = ContactSet.Create(objectA, objectB);
      algo.UpdateContacts(cs, 0);
    }


    [Test]
    public void TestNoContact()
    {
      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);      
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(0.5f);

      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(1.6f, 0, 0));

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
      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(0.5f);

      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(1.5f, 0, 0));

      ContactSet cs = ContactSet.Create(objectA, objectB);

      algo.UpdateContacts(cs, 0);

      Assert.AreEqual(objectA, cs.ObjectA);
      Assert.AreEqual(objectB, cs.ObjectB);
      Assert.AreEqual(1, cs.Count);
      Assert.AreEqual(new Vector3F(1, 0, 0), cs[0].Position);
      Assert.AreEqual(new Vector3F(1, 0, 0), cs[0].Normal);
      Assert.IsTrue(Numeric.AreEqual(0, cs[0].PenetrationDepth));
    }


    [Test]
    public void TestInterpenetration()
    {
      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(0.5f);

      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(0, 1.2f, 0));

      ContactSet cs = ContactSet.Create(objectA, objectB);

      algo.UpdateContacts(cs, 0);

      Assert.AreEqual(objectA, cs.ObjectA);
      Assert.AreEqual(objectB, cs.ObjectB);
      Assert.AreEqual(1, cs.Count);
      Assert.AreEqual(new Vector3F(0, 0.85f, 0), cs[0].Position);
      Assert.AreEqual(new Vector3F(0, 1, 0), cs[0].Normal);
      Assert.IsTrue(Numeric.AreEqual(0.3f, cs[0].PenetrationDepth));
    }


    [Test]
    public void TestInfiniteSphere()
    {
      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(float.PositiveInfinity);
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(1f);

      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(0, 2f, 0));

      ContactSet cs = ContactSet.Create(objectA, objectB);

      algo.UpdateContacts(cs, 0);

      Assert.AreEqual(1, cs.Count);
      //Assert.AreEqual(new Vector3F(0, 1f, 0), cs.Contacts[0].Position);     // Undefined when a sphere is infinite.
      Assert.AreEqual(new Vector3F(0, 1, 0), cs[0].Normal);
      Assert.IsTrue(float.IsPositiveInfinity(cs[0].PenetrationDepth));
    }


    [Test]
    public void TestZeroSphere1()
    {
      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(0);
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(0);

      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(0, 2f, 0));

      ContactSet cs = ContactSet.Create(objectA, objectB);

      algo.UpdateContacts(cs, 0);

      Assert.AreEqual(0, cs.Count);
    }


    [Test]
    public void TestZeroSphere()
    {
      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(0);
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(0);

      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 1));
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 1));

      ContactSet cs = ContactSet.Create(objectA, objectB);

      algo.UpdateContacts(cs, 0);

      Assert.AreEqual(1, cs.Count);
      Assert.AreEqual(new Vector3F(0, 0, 1), cs[0].Position);
      //Assert.AreEqual(new Vector3F(0, 0, 1), cs.Contacts[0].Normal);
      Assert.IsTrue(Numeric.AreEqual(0, cs[0].PenetrationDepth));
    }


    [Test]
    public void TestInterpenetration2()
    {
      // Center of the second sphere is within the first sphere.

      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(0.5f);

      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0.75f));

      ContactSet cs = ContactSet.Create(objectA, objectB);

      algo.UpdateContacts(cs, 0);

      Assert.AreEqual(1, cs.Count);
      Assert.AreEqual(new Vector3F(0, 0, 0.625f), cs[0].Position);
      Assert.AreEqual(new Vector3F(0, 0, 1), cs[0].Normal);
      Assert.IsTrue(Numeric.AreEqual(0.75f, cs[0].PenetrationDepth));
    }

    [Test]
    public void TestContainment1()
    {
      // Second sphere is within first sphere.

      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(0.5f);

      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0.5f));

      ContactSet cs = ContactSet.Create(objectA, objectB);

      algo.UpdateContacts(cs, 0);

      Assert.AreEqual(1, cs.Count);
      Assert.AreEqual(new Vector3F(0, 0, 0.5f), cs[0].Position);
      Assert.AreEqual(new Vector3F(0, 0, 1), cs[0].Normal);
      Assert.IsTrue(Numeric.AreEqual(1, cs[0].PenetrationDepth));
    }


    [Test]
    public void TestContainment2()
    {
      // Second sphere is within first sphere on same position.

      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(0.5f);

      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(1, 1, 1));
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(1, 1, 1));

      ContactSet cs = ContactSet.Create(objectA, objectB);

      algo.UpdateContacts(cs, 0);

      Assert.AreEqual(1, cs.Count);
      Assert.AreEqual(new Vector3F(1, 1.25f, 1), cs[0].Position);
      Assert.AreEqual(new Vector3F(0, 1, 0), cs[0].Normal);
      Assert.IsTrue(Numeric.AreEqual(1.5f, cs[0].PenetrationDepth));
    }


    [Test]
    public void HaveContact()
    {
      SphereSphereAlgorithm algo = new SphereSphereAlgorithm(new CollisionDetection());

      CollisionObject objectA = new CollisionObject();
      ((GeometricObject)objectA.GeometricObject).Shape = new SphereShape(1);
      CollisionObject objectB = new CollisionObject();
      ((GeometricObject)objectB.GeometricObject).Shape = new SphereShape(0.5f);

      ((GeometricObject)objectA.GeometricObject).Pose = new Pose(new Vector3F(1, 1, 1));
      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(1, 1, 1));

      Assert.AreEqual(true, algo.HaveContact(objectA, objectB));

      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(1, 1, 2.5f));
      Assert.AreEqual(true, algo.HaveContact(objectA, objectB));

      ((GeometricObject)objectB.GeometricObject).Pose = new Pose(new Vector3F(1, 1, 2.6f));
      Assert.AreEqual(false, algo.HaveContact(objectA, objectB));
    }
  }
}


