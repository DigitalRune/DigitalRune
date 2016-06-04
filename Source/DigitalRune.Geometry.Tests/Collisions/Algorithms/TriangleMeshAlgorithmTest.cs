using System;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Collisions.Algorithms.Tests
{
  [TestFixture]
  public class TriangleMeshAlgorithmTest
  {
    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestException()
    {
      new TriangleMeshAlgorithm(new CollisionDetection()).GetClosestPoints(new CollisionObject(), new CollisionObject());
    }


    [Test]
    public void ComputeCollision()
    {
      CollisionObject a = new CollisionObject();
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(0, 0, 1), new Vector3F(1, 0, 0)), false);
      mesh.Add(new Triangle(new Vector3F(1, 0, 0), new Vector3F(0, 0, 1), new Vector3F(1, 0, 1)), false);
      TriangleMeshShape meshShape = new TriangleMeshShape();
      meshShape.Mesh = mesh;
      a.GeometricObject = new GeometricObject(meshShape, Pose.Identity);

      CollisionObject b = new CollisionObject(new GeometricObject
                  {
                    Shape = new SphereShape(1),
                    Pose = new Pose(new Vector3F(0, 0.9f, 0)),
                  });

      ContactSet set;
      TriangleMeshAlgorithm algo = new TriangleMeshAlgorithm(new CollisionDetection());

      set = algo.GetClosestPoints(a, b);
      Assert.AreEqual(true, algo.HaveContact(a, b));
      Assert.AreEqual(true, algo.HaveContact(b, a));
      Assert.AreEqual(1, set.Count);
      Assert.IsTrue(Numeric.AreEqual(0.1f, set[0].PenetrationDepth, 0.001f));
      //Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 0, 0), set[0].PositionALocal, 0.01f));  // MPR will not return the perfect contact point.
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), set[0].Normal, 0.1f));

      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0.1f, 0.9f, 0.1f));
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(true, algo.HaveContact(a, b));
      Assert.AreEqual(true, algo.HaveContact(b, a));
      Assert.AreEqual(1, set.Count);
      Assert.IsTrue(Numeric.AreEqual(0.1f, set[0].PenetrationDepth, 0.001f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0.1f, 0, 0.1f), set[0].PositionALocal, 0.01f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), set[0].Normal, 0.1f));

      // The same with a swapped set.
      set = set.Swapped;
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(1, set.Count);
      Assert.IsTrue(Numeric.AreEqual(0.1f, set[0].PenetrationDepth, 0.001f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0.1f, 0, 0.1f), set[0].PositionBLocal, 0.01f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, -1, 0), set[0].Normal, 0.1f));

      // Separation.
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(0.2f, 1.2f, 0.3f));
      set = set.Swapped;
      Assert.AreEqual(false, algo.HaveContact(a, b));
      Assert.AreEqual(false, algo.HaveContact(b, a));
      algo.UpdateClosestPoints(set, 0);
      Assert.IsTrue(Numeric.AreEqual(-0.2f, set[0].PenetrationDepth, 0.001f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0.2f, 0, 0.3f), set[0].PositionALocal, 0.01f));
      Assert.IsTrue(Vector3F.AreNumericallyEqual(new Vector3F(0, 1, 0), set[0].Normal, 0.1f));
      algo.UpdateContacts(set, 0);
      Assert.AreEqual(0, set.Count);      
    }


    [Test]
    public void ComputeCollisionBvh()
    {
      CollisionObject a = new CollisionObject();
      TriangleMesh meshA = new TriangleMesh();
      meshA.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(0, 0, 1), new Vector3F(1, 0, 0)), false);      
      var meshShapeA = new TriangleMeshShape();
      meshShapeA.Mesh = meshA;
      meshShapeA.Partition = new AabbTree<int>();
      ((GeometricObject)a.GeometricObject).Shape = meshShapeA;

      CollisionObject b = new CollisionObject();
      TriangleMesh meshB = new TriangleMesh();
      meshB.Add(new Triangle(new Vector3F(2, 0, 0), new Vector3F(2, 0, 1), new Vector3F(3, 0, 0)), false);
      TriangleMeshShape meshShapeB = new TriangleMeshShape();
      meshShapeB.Mesh = meshB;
      meshShapeB.Partition = new AabbTree<int>();
      ((GeometricObject)b.GeometricObject).Shape = meshShapeB;
      ((GeometricObject)b.GeometricObject).Pose = new Pose(new Vector3F(-2, 0, 0));

      CollisionAlgorithm algo = new TriangleMeshAlgorithm(new CollisionDetection());
      Assert.AreEqual(true, algo.HaveContact(a, b));
    }
  }
}
