using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class BoxSphereAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestException0()
    {
      new BoxSphereAlgorithm(new CollisionDetection()).HaveContact(
        new CollisionObject(new GeometricObject(new PlaneShape())),
        new CollisionObject(new GeometricObject(new SphereShape())));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestException1()
    {
      new BoxSphereAlgorithm(new CollisionDetection()).HaveContact(
        new CollisionObject(new GeometricObject(new BoxShape())),
        new CollisionObject(new GeometricObject(new PointShape())));
    }


    [Test]
    public void TestContainment()
    {
      BoxSphereAlgorithm algo = new BoxSphereAlgorithm(new CollisionDetection());

      CollisionObject a = new CollisionObject();
      CollisionObject b = new CollisionObject();
      ((GeometricObject)a.GeometricObject).Shape = new BoxShape(1, 2, 3);
      ((GeometricObject)b.GeometricObject).Shape = new SphereShape(1);

      Assert.AreEqual(true, algo.HaveContact(a, b));
      Assert.AreEqual(true, algo.HaveContact(b, a));
    }


    [Test]
    public void TestSeparated()
    {
      BoxSphereAlgorithm algo = new BoxSphereAlgorithm(new CollisionDetection());

      CollisionObject a = new CollisionObject(new GeometricObject
            {
              Shape = new BoxShape(1, 2, 3),
              Pose = new Pose(new Vector3F(0, 0, 0)),
            });
      CollisionObject b = new CollisionObject(new GeometricObject
            {
              Shape = new SphereShape(1),
              Pose = new Pose(new Vector3F(1.6f, 0, 0)),
            });

      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      Assert.AreEqual(1,algo.GetClosestPoints(a, b).Count);
      Assert.AreEqual(new Vector3F(1, 0, 0), algo.GetClosestPoints(a, b)[0].Normal);
      Assert.AreEqual(new Vector3F(0.55f, 0, 0), algo.GetClosestPoints(a, b)[0].Position);
      Assert.IsTrue(Numeric.AreEqual(-0.1f, algo.GetClosestPoints(a, b)[0].PenetrationDepth));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(-1.6f, 0, 0));
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      Assert.AreEqual(1, algo.GetClosestPoints(a, b).Count);
      Assert.AreEqual(new Vector3F(-1, 0, 0), algo.GetClosestPoints(a, b)[0].Normal);
      Assert.AreEqual(new Vector3F(-0.55f, 0, 0), algo.GetClosestPoints(a, b)[0].Position);
      Assert.IsTrue(Numeric.AreEqual(-0.1f, algo.GetClosestPoints(a, b)[0].PenetrationDepth));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, 2.1f, 0));
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      Assert.AreEqual(1, algo.GetClosestPoints(a, b).Count);
      Assert.AreEqual(new Vector3F(0, 1, 0), algo.GetClosestPoints(a, b)[0].Normal);
      Assert.AreEqual(new Vector3F(0, 1.05f, 0), algo.GetClosestPoints(a, b)[0].Position);
      Assert.IsTrue(Numeric.AreEqual(-0.1f, algo.GetClosestPoints(a, b)[0].PenetrationDepth));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, -2.1f, 0));
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      Assert.AreEqual(1, algo.GetClosestPoints(a, b).Count);
      Assert.AreEqual(new Vector3F(0, -1, 0), algo.GetClosestPoints(a, b)[0].Normal);
      Assert.AreEqual(new Vector3F(0, -1.05f, 0), algo.GetClosestPoints(a, b)[0].Position);
      Assert.IsTrue(Numeric.AreEqual(-0.1f, algo.GetClosestPoints(a, b)[0].PenetrationDepth));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 2.6f));
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      Assert.AreEqual(1, algo.GetClosestPoints(a, b).Count);
      Assert.AreEqual(new Vector3F(0, 0, 1), algo.GetClosestPoints(a, b)[0].Normal);
      Assert.AreEqual(new Vector3F(0, 0, 1.55f), algo.GetClosestPoints(a, b)[0].Position);
      Assert.IsTrue(Numeric.AreEqual(-0.1f, algo.GetClosestPoints(a, b)[0].PenetrationDepth));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, 0, -2.6f));
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      Assert.AreEqual(1, algo.GetClosestPoints(a, b).Count);
      Assert.AreEqual(new Vector3F(0, 0, -1), algo.GetClosestPoints(a, b)[0].Normal);
      Assert.AreEqual(new Vector3F(0, 0, -1.55f), algo.GetClosestPoints(a, b)[0].Position);
      Assert.IsTrue(Numeric.AreEqual(-0.1f, algo.GetClosestPoints(a, b)[0].PenetrationDepth));

      Assert.AreEqual(0, algo.GetContacts(a, b).Count);
    }


    [Test]
    public void TestSphereCenterInBox()
    {
      BoxSphereAlgorithm algo = new BoxSphereAlgorithm(new CollisionDetection());

      CollisionObject a = new CollisionObject();
      CollisionObject b = new CollisionObject();
      ((GeometricObject)a.GeometricObject).Shape = new BoxShape(1, 2, 3);
      ((GeometricObject)b.GeometricObject).Shape = new SphereShape(0.2f);

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0.4f, 0, 0));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(1, 0, 0), algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0.35f, 0, 0), algo.GetContacts(a, b)[0].Position));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(-0.4f, 0, 0));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(-1, 0, 0), algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.35f, 0, 0), algo.GetContacts(a, b)[0].Position));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, 0.9f, 0));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(0, 1, 0), algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0.85f, 0), algo.GetContacts(a, b)[0].Position));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, -0.9f, 0));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(0, -1, 0), algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -0.85f, 0), algo.GetContacts(a, b)[0].Position));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 1.4f));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(0, 0, 1), algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 1.35f), algo.GetContacts(a, b)[0].Position));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, 0, -1.4f));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(0, 0, -1), algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, -1.35f), algo.GetContacts(a, b)[0].Position));

      // Test swapping.
      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, 0, -1.4f));
      Assert.AreEqual(1, algo.GetContacts(b, a).Count);
      Assert.AreEqual(new Vector3F(0, 0, 1), algo.GetContacts(b, a)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, -1.35f), algo.GetContacts(b, a)[0].Position));
    }


    [Test]
    public void TestSphereOnSurface()
    {
      BoxSphereAlgorithm algo = new BoxSphereAlgorithm(new CollisionDetection());

      CollisionObject a = new CollisionObject(new GeometricObject
            {
              Shape = new BoxShape(1, 2, 3),
              Pose = Pose.Identity,
            });
      CollisionObject b = new CollisionObject(new GeometricObject
            {
              Shape = new SphereShape(0.2f),
              Pose = Pose.Identity,
            });

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0.5f, 0, 0));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(1, 0, 0).Normalized, algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0.4f, 0, 0f), algo.GetContacts(a, b)[0].Position));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0f, 1, 0));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(0, 1, 0).Normalized, algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0.0f, 0.9f, 0f), algo.GetContacts(a, b)[0].Position));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0f, 0, 1.5f));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(0, 0, 1).Normalized, algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 1.4f), algo.GetContacts(a, b)[0].Position));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(-0.5f, -1, -1.5f));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(-1, 0, 0).Normalized, algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-0.4f, -1, -1.5f), algo.GetContacts(a, b)[0].Position));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0f, -1, 1.5f));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(0, -1, 0).Normalized, algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0.0f, -0.9f, 1.5f), algo.GetContacts(a, b)[0].Position));

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0f, 0, -1.5f));
      Assert.AreEqual(1, algo.GetContacts(a, b).Count);
      Assert.AreEqual(new Vector3F(0, 0, -1).Normalized, algo.GetContacts(a, b)[0].Normal);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, -1.4f), algo.GetContacts(a, b)[0].Position));
    }
  }
}
