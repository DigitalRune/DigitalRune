using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Geometry.Tests
{
  [TestFixture]
  public class DirectionalLookupTableTest
  {
    [Test]
    public void DirectionalLookup()
    {
      DirectionalLookupTableF<int> lookupTable = new DirectionalLookupTableF<int>(4);
      Assert.AreEqual(4, ((dynamic)lookupTable.Internals).Width);

      // Store 6 * 4 * 4 values.
      int value = 0;
      foreach (Vector3F direction in lookupTable.GetSampleDirections())
      {
        lookupTable[direction] = value;
        value++;
      }

      Assert.AreEqual(6 * 4 * 4, value);

      // Check values.
      value = 0;
      foreach (Vector3F direction in lookupTable.GetSampleDirections())
      {
        Assert.AreEqual(value, lookupTable[direction]);
        value++;
      }

      Assert.AreEqual(6 * 4 * 4, value);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      var lookupTable = new DirectionalLookupTableF<int>(4);

      // Store 6 * 4 * 4 values.
      int value = 0;
      foreach (Vector3F direction in lookupTable.GetSampleDirections())
      {
        lookupTable[direction] = value;
        value++;
      }

      // Serialize object.
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      formatter.Serialize(stream, lookupTable);

      // Deserialize object.
      stream.Position = 0;
      var deserializer = new BinaryFormatter();
      lookupTable = (DirectionalLookupTableF<int>)deserializer.Deserialize(stream);

      // Check values.
      value = 0;
      foreach (Vector3F direction in lookupTable.GetSampleDirections())
      {
        Assert.AreEqual(value, lookupTable[direction]);
        value++;
      }
    }
  }
}
