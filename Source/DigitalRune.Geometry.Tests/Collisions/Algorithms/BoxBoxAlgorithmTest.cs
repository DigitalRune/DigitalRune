using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class BoxBoxAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(GeometryException))]
    public void TestException()
    {
      // Closest points not supported.
      new BoxBoxAlgorithm(new CollisionDetection()).GetClosestPoints(
        new CollisionObject(new GeometricObject { Shape = new BoxShape() }),
        new CollisionObject(new GeometricObject { Shape = new BoxShape() }));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestException2()
    {
      // Needs two boxes.
      new BoxBoxAlgorithm(new CollisionDetection()).GetContacts(
        new CollisionObject(new GeometricObject { Shape = new BoxShape() }),
        new CollisionObject());
    }


    [Test]
    public void ComputeCollision()
    {
      CollisionObject a = new CollisionObject(
        new GeometricObject
        {
          Pose = new Pose(new Vector3F(0, 0, 0)),
          Shape = new BoxShape(2, 2, 2),
        });

      CollisionObject b = new CollisionObject(
        new GeometricObject
        {
          Shape = new BoxShape(2, 2, 2),
          Pose = Pose.Identity,
        });

      ContactSet set;
      BoxBoxAlgorithm algo = new BoxBoxAlgorithm(new CollisionDetection());

      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, 3f, 0));
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      set = algo.GetContacts(a, b);
      Assert.AreEqual(0, set.Count);

      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, -3f, 0));
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(0, set.Count);

      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(3, 0f, 0));
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(0, set.Count);

      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(-3, 0f, 0));
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(0, set.Count);

      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, 0f, 3));
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(0, set.Count);

      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0, 0f, -3));
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(0, set.Count);

      // TODO: More unit tests required...
    }


    [Test]
    public void GetOutcode()
    {
      BoxShape box = new BoxShape(1, 2, 3);

      Assert.AreEqual(0, GeometryHelper.GetOutcode(box.Extent, new Vector3F(0.1f, 0.1f, 0.1f)));
      Assert.AreEqual(1 | 8, GeometryHelper.GetOutcode(box.Extent, new Vector3F(-1.1f, 2.1f, 0.1f)));
      Assert.AreEqual(2 | 4 | 32, GeometryHelper.GetOutcode(box.Extent, new Vector3F(1.1f, -2.1f, 3.1f)));
      Assert.AreEqual(16, GeometryHelper.GetOutcode(box.Extent, new Vector3F(0.1f, 0.9f, -3.1f)));
    }
  }
}