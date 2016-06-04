using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class RayAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestException0()
    {
      new RayConvexAlgorithm(new CollisionDetection()).HaveContact(
        new CollisionObject(new GeometricObject(new SphereShape())),
        new CollisionObject(new GeometricObject(new SphereShape())));
    }


    [Test]
    public void ComputeCollision()
    {
      RayConvexAlgorithm algo = new RayConvexAlgorithm(new CollisionDetection());

      CollisionObject ray = new CollisionObject(new GeometricObject
      {
        Shape = new RayShape(new Vector3F(0, 0, 0), new Vector3F(-1, 0, 0), 10),
        Pose = new Pose(new Vector3F(11, 0, 0))
      });

      CollisionObject triangle = new CollisionObject(new GeometricObject
      {
        Shape = new TriangleShape(new Vector3F(0, 0, 0), new Vector3F(0, 1, 0), new Vector3F(0, 0, 1)),
        Pose = Pose.Identity,
      });

      ContactSet set;

      // Separated
      set = algo.GetClosestPoints(ray, triangle);
      Assert.AreEqual(new Vector3F(1, 0, 0), set[0].PositionAWorld);
      Assert.AreEqual(new Vector3F(0, 0, 0), set[0].PositionBWorld);
      Assert.AreEqual(-1, set[0].PenetrationDepth);
      Assert.AreEqual(false, algo.HaveContact(ray, triangle));
      Assert.AreEqual(false, algo.HaveContact(triangle, ray));
      Assert.AreEqual(0, algo.GetContacts(ray, triangle).Count);

      // Touching
      Pose newPose = ray.GeometricObject.Pose;
      newPose.Position = new Vector3F(5, 0, 0);
      ((GeometricObject)ray.GeometricObject).Pose = newPose;
      set = algo.GetClosestPoints(triangle, ray);
      Assert.AreEqual(new Vector3F(0, 0, 0), set[0].PositionBWorld);
      Assert.AreEqual(new Vector3F(0, 0, 0), set[0].PositionAWorld);
      Assert.AreEqual(5, set[0].PenetrationDepth);
      Assert.AreEqual(true, algo.HaveContact(ray, triangle));
      Assert.AreEqual(true, algo.HaveContact(triangle, ray));

      newPose = ray.GeometricObject.Pose;
      newPose.Position = new Vector3F(4, 0.1f, 0.1f);
      ((GeometricObject)ray.GeometricObject).Pose = newPose;
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(new Vector3F(0, 0.1f, 0.1f), set[0].PositionBWorld);
      Assert.AreEqual(new Vector3F(0, 0.1f, 0.1f), set[0].PositionAWorld);
      Assert.AreEqual(new Vector3F(0, 0.1f, 0.1f), set[0].Position);
      Assert.AreEqual(4, set[0].PenetrationDepth);
      Assert.AreEqual(true, algo.HaveContact(ray, triangle));
      Assert.AreEqual(true, algo.HaveContact(triangle, ray));

      // Through triangle plane but separated.
      newPose = ray.GeometricObject.Pose;
      newPose.Position = new Vector3F(5, 1.1f, 0.1f);
      ((GeometricObject)ray.GeometricObject).Pose = newPose;
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(0, set.Count);
      algo.UpdateClosestPoints(set, 0);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1.1f, 0.1f), set[0].PositionBWorld));
      Assert.AreEqual(false, algo.HaveContact(ray, triangle));
      Assert.AreEqual(false, algo.HaveContact(triangle, ray));
    }
  }
}
