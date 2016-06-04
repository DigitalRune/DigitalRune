using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using DigitalRune.Mathematics.Algebra;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Interpolation.Tests
{
  [TestFixture]
  public class CurveKey2FTest
  {
    [Test]
    public void ParameterTest()
    {
      CurveKey2F curveKey = new CurveKey2F
      {
        Interpolation = SplineInterpolation.Bezier,
        Point = new Vector2F(1.2f, 3.4f),
        TangentIn = new Vector2F(0.7f, 2.6f),
        TangentOut = new Vector2F(1.9f, 3.3f)
      };

      Assert.AreEqual(1.2f, curveKey.Parameter);

      curveKey.Parameter = 0.2f;
      Assert.AreEqual(0.2f, curveKey.Point.X);

      curveKey.Point = new Vector2F(1, 2);
      Assert.AreEqual(1, curveKey.Parameter);
      Assert.AreEqual(1, curveKey.Point.X);
      Assert.AreEqual(2, curveKey.Point.Y);
    }


    [Test]
    public void SerializationXml()
    {
      CurveKey2F curveKey1 = new CurveKey2F
      {
       Interpolation = SplineInterpolation.Bezier,
       Point = new Vector2F(1.2f, 3.4f),
       TangentIn = new Vector2F(0.7f, 2.6f),
       TangentOut = new Vector2F(1.9f, 3.3f)
      };
      CurveKey2F curveKey2;

      const string fileName = "SerializationCurve2FKey.xml";

      if (File.Exists(fileName))
        File.Delete(fileName);

      XmlSerializer serializer = new XmlSerializer(typeof(CurveKey2F));
      StreamWriter writer = new StreamWriter(fileName);
      serializer.Serialize(writer, curveKey1);
      writer.Close();

      serializer = new XmlSerializer(typeof(CurveKey2F));
      FileStream fileStream = new FileStream(fileName, FileMode.Open);
      curveKey2 = (CurveKey2F)serializer.Deserialize(fileStream);
      MathAssert.AreEqual(curveKey1, curveKey2);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      CurveKey2F curveKey1 = new CurveKey2F
      {
        Interpolation = SplineInterpolation.Bezier,
        Point = new Vector2F(1.2f, 3.4f),
        TangentIn = new Vector2F(0.7f, 2.6f),
        TangentOut = new Vector2F(1.9f, 3.3f)
      };
      CurveKey2F curveKey2;

      const string fileName = "SerializationCurve2FKey.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, curveKey1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      curveKey2 = (CurveKey2F)formatter.Deserialize(fs);
      fs.Close();

      MathAssert.AreEqual(curveKey1, curveKey2);
    }
  }
}
