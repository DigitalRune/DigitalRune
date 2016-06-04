using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Geometry.Meshes.Tests
{
  [TestFixture]
  public class ConvexHullTest
  {
    [SetUp]
    public void SetUp()
    {
      RandomHelper.Random = new Random(1234567);
    }

    [Test]
    public void EmptyConvexHull()
    {
      //Assert.AreEqual(null, ConvexHullHelper.CreateConvexHull(null));
      Assert.AreEqual(null, GeometryHelper.CreateConvexHull(null));
      //Assert.AreEqual(null, ConvexHullHelper.CreateConvexHullNew(new List<Vector3F>()));
      Assert.AreEqual(null, GeometryHelper.CreateConvexHull(new List<Vector3F>()));
    }


    [Test]
    public void PointConvexHull()
    {
      var points = new[] { new Vector3F(1, 2, 3) };
      var mesh = GeometryHelper.CreateConvexHull(points);
      Assert.AreEqual(1, mesh.Vertices.Count);
      Assert.AreEqual(points[0], mesh.Vertex.Position);
      Assert.AreEqual(null, mesh.Vertex.Edge);

      points = new[] { new Vector3F(1, 2, 3), new Vector3F(1, 2, 3), };
      mesh = GeometryHelper.CreateConvexHull(points);
      Assert.AreEqual(1, mesh.Vertices.Count);
      Assert.AreEqual(points[0], mesh.Vertex.Position);
      Assert.AreEqual(null, mesh.Vertex.Edge);

      points = new[]
      {
        new Vector3F(1, 2, 3), 
        new Vector3F(1, 2, 3), 
        new Vector3F(1.000001f, 2, 3),
        new Vector3F(1.000001f, 1.999999f, 3),
        new Vector3F(1.000001f, 2, 3.000001f),
      };
      mesh = GeometryHelper.CreateConvexHull(points);
      Assert.AreEqual(1, mesh.Vertices.Count);
      //Assert.AreEqual(points[0], mesh.Vertex.Position);
      Assert.AreEqual(null, mesh.Vertex.Edge);
    }


    [Test]
    public void LinearConvexHull()
    {
      var start = new Vector3F(1, 10, 20);
      var end = new Vector3F(10, 10, 20);
      var points = new[] { start, end };
      var mesh = GeometryHelper.CreateConvexHull(points);
      Assert.AreEqual(2, mesh.Vertices.Count);
      Assert.IsTrue(mesh.Vertices.Any(v => v.Position == start));
      Assert.IsTrue(mesh.Vertices.Any(v => v.Position == end));
      Assert.AreEqual(null, mesh.Vertex.Edge.Face);

      points = new[] { start, end, start, end, start, new Vector3F(1.00001f, 10.00001f, 20.00001f),  };
      mesh = GeometryHelper.CreateConvexHull(points);
      Assert.AreEqual(2, mesh.Vertices.Count);
      Assert.IsTrue(mesh.Vertices.Any(v => v.Position == start));
      Assert.IsTrue(mesh.Vertices.Any(v => v.Position == end));
      Assert.AreEqual(null, mesh.Vertex.Edge.Face);

      points = new[] { new Vector3F(2, 10, 20), new Vector3F(9.00001f, 10, 20), start, end, start };
      mesh = GeometryHelper.CreateConvexHull(points);
      Assert.AreEqual(2, mesh.Vertices.Count);
      Assert.IsTrue(mesh.Vertices.Any(v => v.Position == start));
      Assert.IsTrue(mesh.Vertices.Any(v => v.Position == end));
      Assert.AreEqual(null, mesh.Vertex.Edge.Face);
    }


    [Test]
    public void PlanarConvexHull()
    {
      var a = new Vector3F(1, 10, 20);
      var b = new Vector3F(10, 10, 20);
      var c = new Vector3F(10, 20, 20);
      var d = new Vector3F(1, 20, 20);

      var points = new[] { a, b, c, d };
      var mesh = GeometryHelper.CreateConvexHull(points);
      Assert.AreEqual(4, mesh.Vertices.Count);
      Assert.AreEqual(8, mesh.Edges.Count);
      Assert.AreEqual(2, mesh.Faces.Count);
      Assert.IsTrue(mesh.Vertices.Any(v => v.Position == a));
      Assert.IsTrue(mesh.Vertices.Any(v => v.Position == b));

      points = new[]
      {
        new Vector3F(2, 10.1f, 20), 
        new Vector3F(4, 13, 20), 
        new Vector3F(2, 10.1f, 20), 
        a,
        new Vector3F(2, 15, 20), 
        new Vector3F(1.000001f, 10f, 20.000001f), 
        new Vector3F(2, 10.1f, 20), 
        new Vector3F(9, 17, 20), 
        a, 
        b, 
        c, 
        d
      };
      mesh = GeometryHelper.CreateConvexHull(points);
      Assert.AreEqual(4, mesh.Vertices.Count);
      Assert.AreEqual(8, mesh.Edges.Count);
      Assert.AreEqual(2, mesh.Faces.Count);
      Assert.IsTrue(mesh.Vertices.Any(v => v.Position == a));
      Assert.IsTrue(mesh.Vertices.Any(v => v.Position == b));

      var triangleMesh = mesh.ToTriangleMesh();
      Assert.AreEqual(4, triangleMesh.NumberOfTriangles);
    }


    [Test]
    public void SpatialConvexHull()
    {
      var a = new Vector3F(0, 0, 0);
      var b = new Vector3F(10, 0, 0);
      var c = new Vector3F(0, 20, 0);
      var d = new Vector3F(10, 20, 0);
      var e = new Vector3F(11, 21, 4);

      var points = new[] { a, b, c, d, e };
      var mesh = GeometryHelper.CreateConvexHull(points);
      Assert.AreEqual(5, mesh.Vertices.Count);
      // The input vertices can be sorted in different sorting orders. So the bottom of the
      // Pyramid (abcde) can 1 quad or 2 triangles.
      Assert.IsTrue(mesh.Edges.Count == 16 && mesh.Faces.Count == 5
                    || mesh.Edges.Count == 18 && mesh.Faces.Count == 6);
    }



    
    [Test]
    public void CreateConvexHull1()
    {
      DcelMesh mesh = new DcelMesh();
      mesh = GeometryHelper.CreateConvexHull(new[] { new Vector3F(0, 0, 0), 
                                                       new Vector3F(0, 0, 0),
                                                       new Vector3F(1, 0, 0), 
                                                       new Vector3F(1, 1, 0),
                                                       new Vector3F(0, 0, 1),
                                                       new Vector3F(0, 0, -1),
                                                       new Vector3F(-1, 1, 0),
                                                       new Vector3F(-1, -1, 0)
                                                     });

      Assert.AreEqual(6, mesh.Vertices.Count);
      Assert.AreEqual(24, mesh.Edges.Count);
      Assert.AreEqual(8, mesh.Faces.Count);
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SampleConvexHullThrowsArgumentNullException()
    {
      GeometryHelper.SampleConvexShape(null, 1, 1);
    }

    [Test]
    public void RandomPoints()
    {
      var points = new List<Vector3F>();
      for (int i = 0; i < 1000; i++)
      {
        points.Add(RandomHelper.Random.NextVector3F(100, 300));
      }

      Stopwatch watch = new Stopwatch();
      watch.Start();
      for (int i = 0; i < 10; i++)
      {
        var mesh = GeometryHelper.CreateConvexHull(points);

        for (int j = 0; j < points.Count; j++)
        {
          Assert.IsTrue(mesh.Contains(points[j], 0.01f));
        }

        Assert.IsTrue(mesh.IsConvex());
      }
      Trace.WriteLine("Time: " + watch.Elapsed.TotalSeconds);
    }
  }
}
