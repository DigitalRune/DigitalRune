using System;
using System.Diagnostics;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class TriangleMeshShapeTest
  {
    TriangleMesh _mesh; 
    TriangleMeshShape _meshShape;

    [SetUp]
    public void Setup()
    {
      // Make a unit cube.
      _mesh = new TriangleMesh(); 
      // Bottom
      _mesh.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(1, 1, 0), new Vector3F(1, 0, 0)), true);
      _mesh.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(0, 1, 0), new Vector3F(1, 1, 0)), true);
      // Top
      _mesh.Add(new Triangle(new Vector3F(0, 0, 1), new Vector3F(1, 0, 1), new Vector3F(1, 1, 1)), true);
      _mesh.Add(new Triangle(new Vector3F(0, 0, 1), new Vector3F(1, 1, 1), new Vector3F(0, 1, 1)), true);
      // Left
      _mesh.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(1, 0, 0), new Vector3F(1, 0, 1)), true);
      _mesh.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(1, 0, 1), new Vector3F(0, 0, 1)), true);
      // Right
      _mesh.Add(new Triangle(new Vector3F(0, 1, 0), new Vector3F(0, 1, 1), new Vector3F(1, 1, 1)), true);
      _mesh.Add(new Triangle(new Vector3F(0, 1, 0), new Vector3F(1, 1, 1), new Vector3F(1, 1, 0)), true);
      // Front
      _mesh.Add(new Triangle(new Vector3F(1, 0, 0), new Vector3F(1, 1, 0), new Vector3F(1, 1, 1)), true);
      _mesh.Add(new Triangle(new Vector3F(1, 0, 0), new Vector3F(1, 1, 1), new Vector3F(1, 0, 1)), true);
      // Back
      _mesh.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(0, 0, 1), new Vector3F(0, 1, 1)), true);
      _mesh.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(0, 1, 1), new Vector3F(0, 1, 0)), true);

      _meshShape = new TriangleMeshShape { Mesh = _mesh };

    }

    [Test]
    public void Constructor()
    {
      Assert.AreEqual(0, new TriangleMeshShape().Mesh.NumberOfTriangles);
      Assert.AreEqual(12, new TriangleMeshShape { Mesh = _mesh }.Mesh.NumberOfTriangles);
    }


    [Test]
    public void PropertiesTest()
    {
      TriangleMeshShape shape = new TriangleMeshShape();
      Assert.AreNotEqual(_mesh, shape.Mesh);
      shape.Mesh = _mesh;
      Assert.AreEqual(_mesh, shape.Mesh);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void PropertiesException()
    {
      TriangleMeshShape shape = new TriangleMeshShape();
      shape.Mesh = null;
    }



    [Test]
    public void InnerPoint()
    {
      Assert.AreEqual(new Vector3F(), new TriangleMeshShape().InnerPoint);
      Assert.AreEqual(new Vector3F(0, 1, 1), _meshShape.InnerPoint);
    }


    //[Test]
    //public void GetAabb()
    //{
    //  Assert.AreEqual(new Aabb(), new ConvexHullOfPoints().GetAabb(Pose.Identity));
    //  Assert.AreEqual(new Aabb(new Vector3F(10, 100, -13), new Vector3F(10, 100, -13)),
    //                 new ConvexHullOfPoints().GetAabb(new Pose(new Vector3F(10, 100, -13),
    //                                                                     QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
    //  Assert.AreEqual(new Aabb(new Vector3F(11, 102, 1003), new Vector3F(11, 102, 1003)),
    //                 new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetAabb(new Pose(new Vector3F(10, 100, 1000),
    //                                                                     QuaternionF.Identity)));
    //  QuaternionF rotation = QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f);
    //  Vector3F worldPos = rotation.Rotate(new Vector3F(1, 2, 3)) + new Vector3F(10, 100, 1000);
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(worldPos, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetAabb(new Pose(new Vector3F(10, 100, 1000), rotation)).Minimum));
    //  Assert.IsTrue(Vector3F.AreNumericallyEqual(worldPos, new ConvexHullOfPoints(new Vector3F(1, 2, 3)).GetAabb(new Pose(new Vector3F(10, 100, 1000), rotation)).Maximum));
    //}


    [Test]
    public void ToStringTest()
    {
      Assert.AreEqual("TriangleMeshShape { NumberOfTriangles = 12 }", _meshShape.ToString());
    }


    [Test]
    public void Clone()
    {
      TriangleMeshShape clone = _meshShape.Clone() as TriangleMeshShape;
      Assert.IsNotNull(clone);
      Assert.IsNotNull(clone.Mesh);
      Assert.IsTrue(clone.Mesh is TriangleMesh);
      Assert.AreSame(_mesh, clone.Mesh);
      Assert.AreEqual(_mesh.NumberOfTriangles, clone.Mesh.NumberOfTriangles);
      for (int i = 0; i < _mesh.NumberOfTriangles; i++)
      {
        Triangle t = _mesh.GetTriangle(i);

        Triangle tCloned = clone.Mesh.GetTriangle(i);

        Assert.AreEqual(t.Vertex0, tCloned.Vertex0);
        Assert.AreEqual(t.Vertex1, tCloned.Vertex1);
        Assert.AreEqual(t.Vertex2, tCloned.Vertex2);
      }
      Assert.AreEqual(_meshShape.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(_meshShape.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void CloneMeshWithPartition()
    {
      CloneMeshWithPartition(new AabbTree<int>());
      CloneMeshWithPartition(new AdaptiveAabbTree<int>());
      CloneMeshWithPartition(new CompressedAabbTree());
      CloneMeshWithPartition(new DynamicAabbTree<int>());
      CloneMeshWithPartition(new SweepAndPruneSpace<int>());
    }


    private void CloneMeshWithPartition(ISpatialPartition<int> partition)
    {
      TriangleMeshShape meshShape = (TriangleMeshShape)_meshShape.Clone();
      meshShape.Partition = partition;

      TriangleMeshShape clone = meshShape.Clone() as TriangleMeshShape;
      Assert.IsNotNull(clone);
      Assert.IsNotNull(clone.Mesh);
      Assert.IsTrue(clone.Mesh is TriangleMesh);
      Assert.AreSame(_mesh, clone.Mesh);
      Assert.AreEqual(_mesh.NumberOfTriangles, clone.Mesh.NumberOfTriangles);
      for (int i = 0; i < _mesh.NumberOfTriangles; i++)
      {
        Triangle t = _mesh.GetTriangle(i);

        Triangle tCloned = clone.Mesh.GetTriangle(i);

        Assert.AreEqual(t.Vertex0, tCloned.Vertex0);
        Assert.AreEqual(t.Vertex1, tCloned.Vertex1);
        Assert.AreEqual(t.Vertex2, tCloned.Vertex2);
      }

      Assert.IsNotNull(clone.Partition);
      Assert.IsInstanceOf(partition.GetType(), clone.Partition);
      Assert.AreEqual(_mesh.NumberOfTriangles, clone.Partition.Count);
      Assert.AreNotSame(partition, clone.Partition);
    }



    [Test]
    public void ContactWelding()
    {
      var mesh = new SphereShape(0.5f).GetMesh(0.0001f, 7);

      Stopwatch watch = Stopwatch.StartNew();
      var meshShape = new TriangleMeshShape(mesh, true, null);
      watch.Stop();
      //Assert.AreEqual(0, watch.Elapsed.TotalMilliseconds);

      Assert.AreEqual(mesh.NumberOfTriangles * 3, meshShape.TriangleNeighbors.Count);

      for (int i = 0; i < mesh.NumberOfTriangles; i++)
      {
        Assert.AreNotEqual(-1, meshShape.TriangleNeighbors[i * 3 + 0]);
        Assert.AreNotEqual(-1, meshShape.TriangleNeighbors[i * 3 + 1]);
        Assert.AreNotEqual(-1, meshShape.TriangleNeighbors[i * 3 + 2]);

        var triangle = mesh.GetTriangle(i);
        
        // Check if each edge neighbor shares two vertices with this triangle.
        for (int e = 0; e < 3; e++)
        {
          var vertex0 = triangle[(e + 1) % 3];
          var vertex1 = triangle[(e + 2) % 3];

          var neighbor = mesh.GetTriangle(meshShape.TriangleNeighbors[i * 3 + e]);
          
          int sharedCount = 0;

          // Count shared vertices.
          for (int j = 0; j < 3; j++)
          {
            if (Vector3F.AreNumericallyEqual(vertex0, neighbor[j]))
              sharedCount++;
            if (Vector3F.AreNumericallyEqual(vertex1, neighbor[j]))
              sharedCount++;
          }

          Assert.AreEqual(2, sharedCount);
        }
      }
    }
  }
}

