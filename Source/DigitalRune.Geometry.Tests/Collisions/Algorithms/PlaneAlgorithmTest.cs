using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class PlaneAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestException0()
    {
      new PlaneConvexAlgorithm(new CollisionDetection()).HaveContact(
        new CollisionObject(new GeometricObject(new BoxShape())),
        new CollisionObject(new GeometricObject(new SphereShape())));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestException1()
    {
      new PlaneConvexAlgorithm(new CollisionDetection()).HaveContact(
        new CollisionObject(new GeometricObject(new BoxShape())),
        new CollisionObject(new GeometricObject(new CylinderShape())));
    }


    [Test]
    public void TestContainment()
    {
      PlaneConvexAlgorithm algo = new PlaneConvexAlgorithm(new CollisionDetection());

      CollisionObject a = new CollisionObject(new GeometricObject
                  {
                    Shape = new BoxShape(1, 2, 3),
                    Pose = new Pose(new Vector3F(0, -1, 0)),
                  });
      CollisionObject b = new CollisionObject(new GeometricObject
                  {
                    Shape = new PlaneShape(new Vector3F(0, 1, 0), 0),
                  });

      Assert.AreEqual(true, algo.HaveContact(a, b));
      Assert.AreEqual(true, algo.HaveContact(b, a));

      // Test contact set update.
      ContactSet cs = ContactSet.Create(a, b);
      cs.Add(Contact.Create());
      algo.UpdateContacts(cs, 0);
      Assert.AreEqual(1, cs.Count);
      Assert.AreEqual(2, cs[0].PenetrationDepth);
    }


    [Test]
    public void TestSeparated()
    {
      PlaneConvexAlgorithm algo = new PlaneConvexAlgorithm(new CollisionDetection());

      CollisionObject a = new CollisionObject(new GeometricObject
      {
        Shape = new BoxShape(1, 2, 3),
        Pose = new Pose(new Vector3F(0, 2, 0)),
      });
      CollisionObject b = new CollisionObject(new GeometricObject
      {
        Shape = new PlaneShape(new Vector3F(0, 1, 0), 0),
      });

      Assert.AreEqual(false, algo.HaveContact(a, b));      
      Assert.AreEqual(1,algo.GetClosestPoints(a, b).Count);
      Assert.AreEqual(new Vector3F(0, -1, 0), algo.GetClosestPoints(a, b)[0].Normal);
      Assert.AreEqual(new Vector3F(0.5f, 0.5f, 1.5f), algo.GetClosestPoints(a, b)[0].Position);
      Assert.IsTrue(Numeric.AreEqual(-1, algo.GetClosestPoints(a, b)[0].PenetrationDepth));

      // Test swapped.
      Assert.AreEqual(false, algo.HaveContact(b, a));
      Assert.AreEqual(1, algo.GetClosestPoints(b, a).Count);
      Assert.AreEqual(new Vector3F(0, 1, 0), algo.GetClosestPoints(b, a)[0].Normal);
      Assert.AreEqual(new Vector3F(0.5f, 0.5f, 1.5f), algo.GetClosestPoints(b, a)[0].Position);
      Assert.IsTrue(Numeric.AreEqual(-1, algo.GetClosestPoints(b, a)[0].PenetrationDepth));

      Assert.AreEqual(0, algo.GetContacts(a, b).Count);
    }   

  }
}
