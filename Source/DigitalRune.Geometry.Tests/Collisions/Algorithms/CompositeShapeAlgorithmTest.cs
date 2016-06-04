using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class CompositeShapeAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestException()
    {
      new CompositeShapeAlgorithm(new CollisionDetection()).GetClosestPoints(new CollisionObject(), new CollisionObject());
    }


    [Test]
    public void ComputeCollision()
    {
      CollisionObject a = new CollisionObject();
      CompositeShape compo = new CompositeShape();
      compo.Children.Add(new GeometricObject(new SphereShape(2), Pose.Identity));
      compo.Children.Add(new GeometricObject(new SphereShape(1), new Pose(new Vector3F(0, 3, 0))));
      a.GeometricObject = new GeometricObject(compo, Pose.Identity);

      CollisionObject b = new CollisionObject(new GeometricObject
                  {
                    Shape = new PlaneShape(new Vector3F(0, 1, 0), 1),
                    Pose = new Pose(new Vector3F(0, -1, 0)),
                  });

      ContactSet set;
      CompositeShapeAlgorithm algo = new CompositeShapeAlgorithm(new CollisionDetection());

      set = algo.GetClosestPoints(a, b);
      Assert.AreEqual(true, algo.HaveContact(a, b));
      Assert.AreEqual(true, algo.HaveContact(b, a));
      Assert.AreEqual(1, set.Count);
      Assert.AreEqual(2, set[0].PenetrationDepth);
      Assert.AreEqual(new Vector3F(0, -2, 0), set[0].PositionAWorld);

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 2, 3), a.GeometricObject.Pose.Orientation);
      set = set.Swapped;
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(true, algo.HaveContact(a, b));
      Assert.AreEqual(true, algo.HaveContact(b, a));
      Assert.AreEqual(1, set.Count);
      Assert.AreEqual(0, set[0].PenetrationDepth);
      Assert.AreEqual(new Vector3F(0, 0, 3), set[0].PositionBWorld);

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0), QuaternionF.CreateRotationZ(ConstantsF.PiOver2));
      set = set.Swapped; // new order: (a, b)
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(true, algo.HaveContact(a, b));
      Assert.AreEqual(2, set.Count);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-2, 0, 0), set[0].PositionALocal));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-1, 3, 0), set[1].PositionALocal));
      Assert.AreEqual(new Vector3F(0, -1, 0), set[0].Normal);
      Assert.AreEqual(new Vector3F(0, -1, 0), set[1].Normal);
      Assert.AreEqual(2, set[0].PenetrationDepth);
      Assert.IsTrue(Numeric.AreEqual(1, set[1].PenetrationDepth));

      algo.UpdateClosestPoints(set, 0);
      Assert.AreEqual(1, set.Count);

      ((GeometricObject)a.GeometricObject).Pose = new Pose(new Vector3F(0, 2.1f, 0), a.GeometricObject.Pose.Orientation);
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(0, set.Count);
      algo.UpdateClosestPoints(set, 0);
      Assert.AreEqual(1, set.Count);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(-2, 0, 0), set[0].PositionALocal));
      Assert.AreEqual(new Vector3F(0, -1, 0), set[0].Normal);
    }
  }
}
