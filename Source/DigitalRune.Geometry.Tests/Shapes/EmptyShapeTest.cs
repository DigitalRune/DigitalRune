using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Shapes.Tests
{
  [TestFixture]
  public class EmptyShapeTest
  {
    [Test]
    public void GetAxisAlignedBoundingBox()
    {
      Assert.AreEqual(new Aabb(), Shape.Empty.GetAabb(Pose.Identity));
      Assert.AreEqual(new Aabb(new Vector3F(11, 12, -13), new Vector3F(11, 12, -13)),
                      Shape.Empty.GetAabb(new Pose(new Vector3F(11, 12, -13), QuaternionF.CreateRotation(new Vector3F(1, 1, 1), 0.7f))));
    }


    [Test]
    public void Clone()
    {
      var emptyShape = Shape.Empty;
      var clone = emptyShape.Clone() as EmptyShape;
      Assert.IsNotNull(clone);
      Assert.AreEqual(emptyShape.GetAabb(Pose.Identity).Minimum, clone.GetAabb(Pose.Identity).Minimum);
      Assert.AreEqual(emptyShape.GetAabb(Pose.Identity).Maximum, clone.GetAabb(Pose.Identity).Maximum);
    }


    [Test]
    public void SerializationXml()
    {
      var a = Shape.Empty;

      // Serialize object.
      var stream = new MemoryStream();
      var serializer = new XmlSerializer(typeof(Shape));
      serializer.Serialize(stream, a);

      // Output generated xml. Can be manually checked in output window.
      stream.Position = 0;
      var xml = new StreamReader(stream).ReadToEnd();
      Trace.WriteLine("Serialized Object:\n" + xml);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new XmlSerializer(typeof(Shape));
      var b = (EmptyShape)deserializer.Deserialize(stream);

      Assert.IsNotNull(b);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var a = Shape.Empty;

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, a);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var b = (EmptyShape)deserializer.Deserialize(stream);

      Assert.IsNotNull(b);
    }


    [Test]
    public void GetMesh()
    {
      var s = Shape.Empty;
      var mesh = s.GetMesh(0.05f, 3);
      Assert.AreEqual(0, mesh.NumberOfTriangles);
    }
  }
}
