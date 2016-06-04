using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;


namespace DigitalRune.Geometry.Tests
{
  // These unit tests do not make much sense. They are only here to get a test coverage of 100% in NCover.
  [TestFixture]
  public class GeometryExceptionTest
  {
    [Test]
    public void TestConstructors()
    {
      GeometryException m = new GeometryException();

      m = new GeometryException("hallo");
      Assert.AreEqual("hallo", m.Message);

      m = new GeometryException("hallo", new Exception("inner"));
      Assert.AreEqual("hallo", m.Message);
      Assert.AreEqual("inner", m.InnerException.Message);
    }

    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      GeometryException m1 = new GeometryException("hallo");

      string fileName = "SerializationGeometryException.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, m1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      GeometryException m2 = (GeometryException) formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(m1.Message, m2.Message);
    }
  }
}
