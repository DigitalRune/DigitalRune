using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Geometry.Meshes.Tests
{
  [TestFixture]
  public class TriangleMeshTest
  {
    [Test]
    public void GetTriangle()
    {
      Triangle t1 = new Triangle(new Vector3F(0, 0, 0), new Vector3F(1, 1, 1), new Vector3F(1, 0, 1));
      
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(t1, false);

      var t2 = mesh.GetTriangle(0);
      Assert.AreEqual(t1.Vertex0, t2.Vertex0);
      Assert.AreEqual(t1.Vertex1, t2.Vertex1);
      Assert.AreEqual(t1.Vertex2, t2.Vertex2);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetTriangleException()
    {
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(1, 1, 1), new Vector3F(1, 0, 1)), false);
      mesh.GetTriangle(-1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void GetTriangleException2()
    {
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(1, 1, 1), new Vector3F(1, 0, 1)), false);
      mesh.GetTriangle(1);
    }



    [Test]
    public void AddMesh()
    {
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(1, 1, 1), new Vector3F(1, 0, 2)), true);
      mesh.Add(new Triangle(new Vector3F(0, 1, 0), new Vector3F(1, 2, 1), new Vector3F(1, 1, 1)), true);

      var mesh2 = new TriangleMesh();
      mesh2.Add(new Triangle(new Vector3F(0, 0, 0), new Vector3F(1, 1, 1), new Vector3F(1, 0, 1)), true);

      mesh2.Add(mesh, true);
      Assert.AreEqual(3, mesh2.NumberOfTriangles);
      Assert.AreEqual(new Triangle(new Vector3F(0, 1, 0), new Vector3F(1, 2, 1), new Vector3F(1, 1, 1)), mesh2.GetTriangle(2));
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void AddTriangleToleranceMustBeGreaterNull()
    {
      TriangleMesh mesh = new TriangleMesh();
      mesh.Add(new Triangle(), true, 0, true);
    }


    [Test]
    public void AddTriangleShouldRemoveDegenerateTriangle()
    {
      var mesh = new TriangleMesh();
      mesh.Add(new Triangle(new Vector3F(1, 1, 1), new Vector3F(1, 1, 1), new Vector3F(1, 1, 1.000001f)), true, 0.001f, true);
      Assert.AreEqual(0, mesh.NumberOfTriangles);
    }


    [Test]
    public void AddTriangleShouldRemoveDegenerateTriangle2()
    {
      var mesh = new TriangleMesh();
      mesh.Add(new Triangle(new Vector3F(1, 1, 1), new Vector3F(1, 1, 1), new Vector3F(1, 1, 1.000001f)), false, 0.001f, true);
      Assert.AreEqual(0, mesh.NumberOfTriangles);
    }


    [Test]
    public void AddTriangleWithoutMergingAndWithDestroyedLists()
    {
      var mesh = new TriangleMesh();

      // Destroy lists.
      mesh.Vertices = null;
      mesh.Indices = null;
      Assert.AreEqual(0, mesh.NumberOfTriangles);

      mesh.Add(new Triangle(new Vector3F(1, 1, 1), new Vector3F(1, 1, 1), new Vector3F(1, 1, 1.000001f)), false, 0.001f, false);
      Assert.AreEqual(3, mesh.Vertices.Count);
    }



    [Test]
    public void Transform()
    {
      var mesh = new TriangleMesh();
      mesh.Add(new Triangle(new Vector3F(0, 1, 2), new Vector3F(3, 4, 5), new Vector3F(6, 7, 8)), true);
      mesh.Add(new Triangle(new Vector3F(-0, -1, -2), new Vector3F(-3, -4, -5), new Vector3F(-6, -7, -8)), true);

      var trans = RandomHelper.Random.NextMatrix44F(-1, 1);
      mesh.Transform(trans);

      Assert.AreEqual(trans.TransformPosition(new Vector3F(0, 1, 2)), mesh.Vertices[0]);
      Assert.AreEqual(trans.TransformPosition(new Vector3F(-6, -7, -8)), mesh.Vertices[5]);
    }


    [Test]
    public void Clone()
    {
      var mesh = new TriangleMesh();
      mesh.Add(new Triangle(new Vector3F(0, 1, 2), new Vector3F(3, 4, 5), new Vector3F(6, 7, 8)), true);
      mesh.Add(new Triangle(new Vector3F(-0, -1, -2), new Vector3F(-3, -4, -5), new Vector3F(-6, -7, -8)), true);
      mesh.Tag = new SphereShape(3);

      var clone = mesh.Clone();
      
      Assert.AreEqual(mesh.NumberOfTriangles, clone.NumberOfTriangles);
      Assert.AreEqual(mesh.GetTriangle(0), clone.GetTriangle(0));
      Assert.AreEqual(mesh.GetTriangle(1), clone.GetTriangle(1));
      Assert.AreSame(mesh.Tag, clone.Tag);

      mesh.Tag = new MemoryStream();
      clone = mesh.Clone();
      Assert.AreSame(mesh.Tag, clone.Tag);
    }


    [Test]
    public void SerializationXml()
    {
      var a = new TriangleMesh();
      a.Add(new Triangle(new Vector3F(0, 1, 2), new Vector3F(3, 4, 5), new Vector3F(6, 7, 8)), true);
      a.Add(new Triangle(new Vector3F(-0, -1, -2), new Vector3F(-3, -4, -5), new Vector3F(-6, -7, -8)), true);


      // Serialize object.
      var stream = new MemoryStream();
      var serializer = new XmlSerializer(typeof(TriangleMesh));
      serializer.Serialize(stream, a);

      // Output generated xml. Can be manually checked in output window.
      stream.Position = 0;
      var xml = new StreamReader(stream).ReadToEnd();
      Trace.WriteLine("Serialized Object:\n" + xml);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new XmlSerializer(typeof(TriangleMesh));
      var b = (TriangleMesh)deserializer.Deserialize(stream);

      Assert.AreEqual(a.NumberOfTriangles, b.NumberOfTriangles);
      for (int i = 0; i < a.Vertices.Count; i++)
        Assert.AreEqual(a.Vertices[i], b.Vertices[i]);
      for (int i = 0; i < a.Indices.Count; i++)
        Assert.AreEqual(a.Indices[i], b.Indices[i]);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = new TriangleMesh();
      a.Add(new Triangle(new Vector3F(0, 1, 2), new Vector3F(3, 4, 5), new Vector3F(6, 7, 8)), false);
      a.Add(new Triangle(new Vector3F(-0, -1, -2), new Vector3F(-3, -4, -5), new Vector3F(-6, -7, -8)), false);

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (TriangleMesh)deserializer.Deserialize(stream);

      Assert.AreEqual(a.NumberOfTriangles, b.NumberOfTriangles);
      for (int i = 0; i < a.Vertices.Count; i++)
        Assert.AreEqual(a.Vertices[i], b.Vertices[i]);
      for (int i = 0; i < a.Indices.Count; i++)
        Assert.AreEqual(a.Indices[i], b.Indices[i]);
    }


    [Test]
    public void ReverseWindingOrder()
    {
      SphereShape sphere = new SphereShape(1);
      var mesh = sphere.GetMesh(0.1f, 3);

      var clone = mesh.Clone();
      clone.ReverseWindingOrder();

      // Test if all normal are inverted.
      for (int i = 0; i < mesh.NumberOfTriangles; i++)
      {
        var n0 = mesh.GetTriangle(i).Normal;
        var n1 = clone.GetTriangle(i).Normal;
        Assert.IsTrue(Vector3F.AreNumericallyEqual(n0, -n1));
      }
    }


    [Test]
    public void WeldVertices()
    {
      TriangleMesh mesh = new TriangleMesh();
      Assert.AreEqual(0, mesh.WeldVertices());

      mesh.Add(new Triangle(new Vector3F(1, 2, 3), new Vector3F(3, 4, 5), new Vector3F(1.00001f, 2.00001f, 3f)), false);

      Assert.AreEqual(3, mesh.Vertices.Count);

      Assert.Throws(typeof(ArgumentOutOfRangeException), () => mesh.WeldVertices(-0.1f));

      Assert.AreEqual(1, mesh.WeldVertices(0.0001f));
      Assert.AreEqual(2, mesh.Vertices.Count);

      var w = Stopwatch.StartNew();
      mesh = new SphereShape(0.5f).GetMesh(0.001f, 7);
      w.Stop();
      //Assert.AreEqual(0, w.Elapsed.TotalMilliseconds);

      for (int i = 0; i < mesh.Vertices.Count; i++)
      {
        for (int j = i + 1; j < mesh.Vertices.Count; j++)
        {
          Assert.IsFalse(Vector3F.AreNumericallyEqual(mesh.Vertices[i], mesh.Vertices[j]));
        }
      }

      // Second time does nothing.
      Assert.AreEqual(0, mesh.WeldVertices());
    }
  }
}
