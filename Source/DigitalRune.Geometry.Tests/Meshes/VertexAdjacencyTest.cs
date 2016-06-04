using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DigitalRune.Mathematics.Statistics;
using NUnit.Framework;


namespace DigitalRune.Geometry.Meshes.Tests
{
  [TestFixture]
  public class VertexAdjacencyTest
  {
    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      // Create a dummy mesh.
      var randomPoints = Enumerable.Range(0, 100)
                                   .Select(i => RandomHelper.Random.NextVector3F(-100, 100));
      var mesh = GeometryHelper.CreateConvexHull(randomPoints);
      VertexAdjacency vertexAdjacency = new VertexAdjacency(mesh);
        
      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, vertexAdjacency);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      var vertexAdjacency2 = (VertexAdjacency)deserializer.Deserialize(stream);

      Assert.That(vertexAdjacency2.ListIndices, Is.EqualTo(vertexAdjacency.ListIndices));
      Assert.That(vertexAdjacency2.Lists, Is.EqualTo(vertexAdjacency.Lists));
    }
  }
}
