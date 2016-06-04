using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class RayTriangleAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestException0()
    {
      new RayTriangleAlgorithm(new CollisionDetection()).HaveContact(
        new CollisionObject(new GeometricObject(new RayShape())),
        new CollisionObject(new GeometricObject(new SphereShape())));
    }


    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestException1()
    {
      new RayTriangleAlgorithm(new CollisionDetection()).HaveContact(new CollisionObject(
        new GeometricObject(new SphereShape())),
        new CollisionObject(new GeometricObject(new TriangleShape())));
    }


    [Test]
    public void ComputeCollision()
    {
      RayTriangleAlgorithm algo = new RayTriangleAlgorithm(new CollisionDetection());

      CollisionObject ray = new CollisionObject(new GeometricObject
      {
        Shape = new RayShape(new Vector3F(0, 0, 0), new Vector3F(-1, 0, 0), 10),
        Pose = new Pose(new Vector3F(11, 0, 0)),
      });

      CollisionObject triangle = new CollisionObject(new GeometricObject
            {
              Shape = new TriangleShape(new Vector3F(0, 0, 0), new Vector3F(0, 1, 0), new Vector3F(0, 0, 1)),
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
      ((GeometricObject)ray.GeometricObject).Pose = new Pose(new Vector3F(5, 0, 0));
      set = algo.GetClosestPoints(triangle, ray);
      Assert.AreEqual(new Vector3F(0, 0, 0), set[0].PositionBWorld);
      Assert.AreEqual(new Vector3F(0, 0, 0), set[0].PositionAWorld);
      Assert.AreEqual(5, set[0].PenetrationDepth);
      Assert.AreEqual(true, algo.HaveContact(ray, triangle));
      Assert.AreEqual(true, algo.HaveContact(triangle, ray));

      ((GeometricObject)ray.GeometricObject).Pose = new Pose(new Vector3F(4, 0.1f, 0.1f));
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(new Vector3F(0, 0.1f, 0.1f), set[0].PositionBWorld);
      Assert.AreEqual(new Vector3F(0, 0.1f, 0.1f), set[0].PositionAWorld);
      Assert.AreEqual(new Vector3F(0, 0.1f, 0.1f), set[0].Position);
      Assert.AreEqual(4, set[0].PenetrationDepth);
      Assert.AreEqual(true, algo.HaveContact(ray, triangle));
      Assert.AreEqual(true, algo.HaveContact(triangle, ray));

      // Through triangle plane but separated.
      ((GeometricObject)ray.GeometricObject).Pose = new Pose(new Vector3F(5, 1.1f, 0.1f));
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(0, set.Count);
      algo.UpdateClosestPoints(set, 0);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1.1f, 0.1f), set[0].PositionBWorld));
      Assert.AreEqual(false, algo.HaveContact(ray, triangle));
      Assert.AreEqual(false, algo.HaveContact(triangle, ray));
    }


    [Test]
    public void DegenerateTriangle()
    {
      RayTriangleAlgorithm algo = new RayTriangleAlgorithm(new CollisionDetection());

      CollisionObject ray = new CollisionObject();
      ((GeometricObject)ray.GeometricObject).Shape = new RayShape(new Vector3F(0, 0, 0), new Vector3F(-1, 0, 0), 10);
      ((GeometricObject)ray.GeometricObject).Pose = new Pose(new Vector3F(11, 0, 0));

      CollisionObject triangle = new CollisionObject();
      ((GeometricObject)triangle.GeometricObject).Shape = new TriangleShape(new Vector3F(0, 0, 0), new Vector3F(0, 1, 0), new Vector3F(0, 1, 0));  // 2 identical vertices.
      ((GeometricObject)triangle.GeometricObject).Pose = new Pose(new Vector3F(0, 0, 0));

      ContactSet set;

      // Separated
      set = algo.GetClosestPoints(ray, triangle);
      Assert.AreEqual(new Vector3F(1, 0, 0), set[0].PositionAWorld);
      Assert.AreEqual(new Vector3F(0, 0, 0), set[0].PositionBWorld);
      Assert.AreEqual(-1, set[0].PenetrationDepth);
      Assert.AreEqual(false, algo.HaveContact(ray, triangle));
      Assert.AreEqual(false, algo.HaveContact(triangle, ray));

      // Touching
      ((GeometricObject)ray.GeometricObject).Pose = new Pose(new Vector3F(5, 0, 0));
      set = algo.GetClosestPoints(triangle, ray);
      Assert.AreEqual(new Vector3F(0, 0, 0), set[0].PositionBWorld);
      Assert.AreEqual(new Vector3F(0, 0, 0), set[0].PositionAWorld);
      Assert.AreEqual(5, set[0].PenetrationDepth);
      Assert.AreEqual(false, algo.HaveContact(ray, triangle));
      Assert.AreEqual(false, algo.HaveContact(triangle, ray));

      // Through triangle plane but separated.
      ((GeometricObject)ray.GeometricObject).Pose = new Pose(new Vector3F(5, 1.1f, 0.1f));
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(0, set.Count);
      algo.UpdateClosestPoints(set, 0);
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1.1f, 0.1f), set[0].PositionBWorld));
      Assert.AreEqual(false, algo.HaveContact(ray, triangle));
      Assert.AreEqual(false, algo.HaveContact(triangle, ray));
    }
  }
}
