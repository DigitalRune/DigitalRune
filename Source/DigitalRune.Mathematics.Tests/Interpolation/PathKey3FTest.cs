using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class PathKey3FTest
  {


    [Test]
    public void SerializationXml()
    {
      PathKey3F pathKey1 = new PathKey3F
      {
        Interpolation = SplineInterpolation.Bezier,
        Parameter = 56.7f,
        Point = new Vector3F(1.2f, 3.4f, 5.6f),
        TangentIn = new Vector3F(0.7f, 2.6f, 5.1f),
        TangentOut = new Vector3F(1.9f, 3.3f, 5.9f)
      };
      PathKey3F pathKey2;

      const string fileName = "SerializationPath3FKey.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(PathKey3F));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, pathKey1);
      writer.Close();

      serializer = new XmlSerializer(typeof(PathKey3F));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      pathKey2 = (PathKey3F)serializer.Deserialize(fileStream);
      MathAssert.AreEqual(pathKey1, pathKey2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      PathKey3F pathKey1 = new PathKey3F
      {
        Interpolation = SplineInterpolation.Bezier,
        Parameter = 56.7f,
        Point = new Vector3F(1.2f, 3.4f, 5.6f),
        TangentIn = new Vector3F(0.7f, 2.6f, 5.1f),
        TangentOut = new Vector3F(1.9f, 3.3f, 5.9f)
      };
      PathKey3F pathKey2;

      const string fileName = "SerializationPath3FKey.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, pathKey1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      pathKey2 = (PathKey3F)formatter.Deserialize(fs);
      fs.Close();

      MathAssert.AreEqual(pathKey1, pathKey2);
    }
  }
}
