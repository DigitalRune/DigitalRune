using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Meshes.Tests
{
  [TestFixture]
  public class DcelTest
  {
    [Test]
    public void TestDcelVertex()
    {
      Assert.AreEqual(Vector3F.Zero, new DcelVertex().Position);
      Assert.AreEqual(0, new DcelVertex().Tag);
      Assert.AreEqual(null, new DcelVertex().Edge);

      DcelEdge edge = new DcelEdge();

      Assert.AreEqual(new Vector3F(3, 2, 1), new DcelVertex(new Vector3F(3, 2, 1), edge).Position);
      Assert.AreEqual(0, new DcelVertex(new Vector3F(3, 2, 1), edge).Tag);
      Assert.AreEqual(edge, new DcelVertex(new Vector3F(3, 2, 1), edge).Edge);
    }


    [Test]
    public void TestDcelFace()
    {
      DcelEdge edge = new DcelEdge();
      Assert.AreEqual(edge, new DcelFace(edge).Boundary);
    }


    [Test]
    public void TestDcelMeshDirty()
    {
      DcelMesh mesh = new DcelMesh();

      DcelVertex v0 = new DcelVertex();
      DcelVertex v1 = new DcelVertex();
      DcelVertex v2 = new DcelVertex();

      DcelEdge e0 = new DcelEdge();
      DcelEdge e1 = new DcelEdge();
      DcelEdge e2 = new DcelEdge();

      DcelFace f = new DcelFace();

      // Link features
      v0.Edge = e0;
      e0.Origin = v0;
      mesh.Vertex = v0;

      Assert.AreEqual(1, mesh.Vertices.Count);
      Assert.AreEqual(1, mesh.Edges.Count);
      Assert.AreEqual(0, mesh.Faces.Count);

      // Link more
      e0.Next = e1;
      e1.Next = e2;
      mesh.Dirty = true;
      Assert.AreEqual(3, mesh.Edges.Count);

      e1.Origin = v1;
      e2.Origin = v2;
      mesh.Dirty = true;
      Assert.AreEqual(3, mesh.Vertices.Count);

      e0.Face = f;
      f.Boundary = e1;
      mesh.Dirty = true;
      Assert.AreEqual(1, mesh.Faces.Count);
    }


    [Test]
    public void CopyConstructor()
    {
      // Try copying empty mesh.
      var a = new DcelMesh(null);
      var b = new DcelMesh(new DcelMesh()); 

      // Create a mesh.
      var mesh = DcelMesh.CreateCube();
      mesh.CutConvex(new Plane(new Vector3F(1, 2, 3).Normalized, 0.6f));
      mesh.CutConvex(new Plane(new Vector3F(-2, -3, 1).Normalized, 0.8f));

      // Create clone with copy constructor and compare.
      var clone = new DcelMesh(mesh);
      Assert.AreEqual(mesh.Vertices.Count, clone.Vertices.Count);
      Assert.AreEqual(mesh.Edges.Count, clone.Edges.Count);
      Assert.AreEqual(mesh.Faces.Count, clone.Faces.Count);
      var tm = mesh.ToTriangleMesh();
      var tmc = clone.ToTriangleMesh();
      for (int i = 0; i < tm.Vertices.Count; i++)
        Assert.AreEqual(tm.Vertices[i], tmc.Vertices[i]);
      for (int i = 0; i < tm.Indices.Count; i++)
        Assert.AreEqual(tm.Indices[i], tmc.Indices[i]);
    }


    [Test]
    public void ToTriangleMesh()
    {
      // Build DCEL mesh for tetrahedron.
      var vertices = new[] { new Vector3F(0, 0, 0), 
                             new Vector3F(1, 0, 0), 
                             new Vector3F(0, 1, 0), 
                             new Vector3F(0, 0, 1)};
      DcelMesh dcelMesh = GeometryHelper.CreateConvexHull(vertices);

      TriangleMesh triangleMesh = dcelMesh.ToTriangleMesh();
      Assert.AreEqual(4, triangleMesh.NumberOfTriangles);
      Assert.IsTrue(new List<Vector3F>(triangleMesh.Vertices).Contains(new Vector3F(0, 0, 0)));
      Assert.IsTrue(new List<Vector3F>(triangleMesh.Vertices).Contains(new Vector3F(1, 0, 0)));
      Assert.IsTrue(new List<Vector3F>(triangleMesh.Vertices).Contains(new Vector3F(0, 1, 0)));
      Assert.IsTrue(new List<Vector3F>(triangleMesh.Vertices).Contains(new Vector3F(0, 0, 1)));
    }


    [Test]
    public void FromTriangleMesh()
    {
      var mesh = new BoxShape(1, 2, 3).GetMesh(0.01f, 1);
      var dcel = DcelMesh.FromTriangleMesh((ITriangleMesh)mesh);

      Assert.IsTrue(dcel.IsValid());
      Assert.IsTrue(dcel.IsClosed());
      Assert.IsTrue(dcel.IsTriangleMesh());
      Assert.AreEqual(8, dcel.Vertices.Count);
      Assert.AreEqual(12, dcel.Faces.Count);
      Assert.AreEqual(36, dcel.Edges.Count);
    }


    [Test]
    public void FromTriangleMesh2()
    {
      var mesh = new TriangleMesh();
      mesh.Vertices.Add(new Vector3F(0, 0, 0));
      mesh.Vertices.Add(new Vector3F(1, 1, 1));
      mesh.Vertices.Add(new Vector3F(2, 2, 2));
      mesh.Vertices.Add(new Vector3F(3, 3, 3));
      //mesh.Vertices.Add(new Vector3F(405, 322, 0));

      mesh.Indices.Add(0);
      mesh.Indices.Add(1);
      mesh.Indices.Add(2);
      
      mesh.Indices.Add(1);
      mesh.Indices.Add(3);
      mesh.Indices.Add(2);

      var dcel = DcelMesh.FromTriangleMesh((ITriangleMesh)mesh);

      Assert.AreEqual(4, dcel.Vertices.Count);
      Assert.AreEqual(2, dcel.Faces.Count);
      Assert.AreEqual(10, dcel.Edges.Count);
      Assert.IsTrue(dcel.IsValid());
      Assert.IsFalse(dcel.IsClosed());
      Assert.IsTrue(dcel.IsTriangleMesh());      
    }


    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void FromTriangleMeshNotSupported()
    {
      var mesh = new BoxShape(1, 2, 3).GetMesh(0.01f, 1);

      // Add a stray triangle. - Only fully connected meshes are supported!
      mesh.Add(new Triangle(new Vector3F(10, 10, 10), new Vector3F(20, 20, 20), new Vector3F(30, 30, 30)), false);

      var dcel = DcelMesh.FromTriangleMesh((ITriangleMesh)mesh);
    }


    [Test]
    public void FromTriangleMeshWithOpenMesh()
    {
      var mesh = new BoxShape(1, 2, 3).GetMesh(0.01f, 1);

      // Remove fifth triangle.
      mesh.Indices.RemoveRange(4 * 3, 3);

      var dcel = DcelMesh.FromTriangleMesh((ITriangleMesh)mesh);

      Assert.IsTrue(dcel.IsValid());
      Assert.IsFalse(dcel.IsClosed());
      Assert.IsTrue(dcel.IsTriangleMesh());
      Assert.AreEqual(8, dcel.Vertices.Count);
      Assert.AreEqual(11, dcel.Faces.Count);
      Assert.AreEqual(36, dcel.Edges.Count);
    }


    [Test]
    public void IsConvex()
    {
      DcelMesh mesh = new DcelMesh();
      Assert.IsFalse(mesh.IsConvex());

      mesh.Vertex = new DcelVertex(new Vector3F(1, 2, 3), null);
      Assert.IsTrue(mesh.IsConvex());

      mesh = DcelMesh.FromTriangleMesh(new BoxShape(1, 2, 3).GetMesh(0.01f, 1));
      Assert.IsTrue(mesh.IsConvex());

      // Remove fifth triangle.
      var triangleMesh = new BoxShape(1, 2, 3).GetMesh(0.01f, 1);
      triangleMesh.Indices.RemoveRange(4 * 3, 3);
      mesh = DcelMesh.FromTriangleMesh(triangleMesh);
      Assert.IsFalse(mesh.IsConvex());

      mesh = DcelMesh.FromTriangleMesh(new SphereShape(10).GetMesh(0.01f, 3));
      Assert.IsTrue(mesh.IsConvex());

      // Move any sphere vertex more inside.
      var v = mesh.Vertices[10];
      v.Position = v.Position * 0.9f;
      Assert.IsFalse(mesh.IsConvex());
    }


    [Test]
    public void CutConvex()
    {
      var mesh = new BoxShape(1, 2, 3).GetMesh(0.01f, 1);
      var dcel = DcelMesh.FromTriangleMesh((ITriangleMesh)mesh);

      Assert.IsTrue(dcel.IsValid());
      Assert.IsTrue(dcel.IsConvex());
      Assert.IsTrue(dcel.IsClosed());

      bool result = dcel.CutConvex(new Plane(new Vector3F(0, 0, 1), 1.5f));
      Assert.IsFalse(result);

      result = dcel.CutConvex(new Plane(new Vector3F(0, 0, 1), -1.5f));
      Assert.IsTrue(result);
      Assert.IsNull(dcel.Vertex);
      Assert.AreEqual(0, dcel.Vertices.Count);

      dcel = DcelMesh.FromTriangleMesh((ITriangleMesh)mesh);
      result = dcel.CutConvex(new Plane(new Vector3F(0, 0, 1), 1f));
      Assert.IsTrue(result);
      Assert.IsTrue(dcel.IsValid());
      Assert.IsTrue(dcel.IsConvex());
      Assert.IsTrue(dcel.IsClosed());
      Assert.AreEqual(12, dcel.Vertices.Count);
      Assert.AreEqual(6 + 8 + 7 * 4, dcel.Edges.Count); // Bottom = 6 edges, new cap = 8 edges, cut sides = each 7 edges.
      Assert.AreEqual(12 - 1, dcel.Faces.Count);
    }


    [Test]
    public void CreateCube()
    {
      var dcel = DcelMesh.CreateCube();

      Assert.IsTrue(dcel.IsValid());
      Assert.IsTrue(dcel.IsConvex());
      Assert.IsTrue(dcel.IsClosed());
      Assert.AreEqual(8, dcel.Vertices.Count);
      Assert.AreEqual(4 * 6, dcel.Edges.Count); // Bottom = 6 edges, new cap = 8 edges, cut sides = each 7 edges.
      Assert.AreEqual(6, dcel.Faces.Count);
    }


    [Test]
    public void ModifyConvexVertexLimit()
    {
      var mesh = DcelMesh.CreateCube();
      mesh.CutConvex(new Plane(new Vector3F(1, 2, 3).Normalized, 0.6f));
      mesh.CutConvex(new Plane(new Vector3F(-2, -3, 1).Normalized, 0.8f));

      Assert.IsTrue(mesh.Vertices.Count > 10);

      mesh.ModifyConvex(10, 0.3f);

      Assert.IsTrue(mesh.IsConvex());
      Assert.IsTrue(mesh.Vertices.Count <= 10);
    }


    [Test]
    public void ModifyConvexSkinWidth()
    {
      // Create a mesh.
      var mesh = DcelMesh.CreateCube();
      mesh.CutConvex(new Plane(new Vector3F(1, 2, 3).Normalized, 0.6f));
      mesh.CutConvex(new Plane(new Vector3F(-2, -3, 1).Normalized, 0.8f));

      var aabb = mesh.GetAabb();

      var skinWidth = 0.3f;
      mesh.ModifyConvex(100, skinWidth);

      var aabb2 = mesh.GetAabb();

      Assert.IsTrue(mesh.IsConvex());
      Assert.IsTrue(Numeric.AreEqual(aabb.Minimum.X - skinWidth, aabb2.Minimum.X));
      Assert.IsTrue(Numeric.AreEqual(aabb.Minimum.Y - skinWidth, aabb2.Minimum.Y));
      Assert.IsTrue(Numeric.AreEqual(aabb.Minimum.Z - skinWidth, aabb2.Minimum.Z));
      Assert.IsTrue(Numeric.AreEqual(aabb.Maximum.X + skinWidth, aabb2.Maximum.X));
      Assert.IsTrue(Numeric.AreEqual(aabb.Maximum.Y + skinWidth, aabb2.Maximum.Y));
      Assert.IsTrue(Numeric.AreEqual(aabb.Maximum.Z + skinWidth, aabb2.Maximum.Z));
    }
  }
}
