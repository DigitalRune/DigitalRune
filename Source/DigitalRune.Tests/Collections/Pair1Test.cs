using System.IO;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;

#if !SILVERLIGHT
using System.Runtime.Serialization.Formatters.Binary;
#endif


namespace DigitalRune.Collections.Tests
{
  [TestFixture]
  public class Pair1Test
  {
    [Test]
    public void ConstructorEmpty()
    {
      var p1 = new Pair<int>();
      Assert.AreEqual(0, p1.First);
      Assert.AreEqual(0, p1.Second);

      var p2 = new Pair<object>();
      Assert.AreEqual(null, p2.First);
      Assert.AreEqual(null, p2.Second);
    }


    [Test]
    public void ConstructorWithArguments()
    {
      Pair<int> p = new Pair<int>(1, 2);
      Assert.AreEqual(1, p.First);
      Assert.AreEqual(2, p.Second);
    }


    [Test]
    public void GetHashCodeTest()
    {
      Pair<int> p1 = new Pair<int>();
      Pair<int> p2 = new Pair<int>();
      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
      p1.First = 12;
      Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());
      p2.First = 12;
      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
      p1.Second = 34;
      Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());
      p2.Second = 34;
      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
      p1.First = 13;
      Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());
      p2.First = 13;
      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
      p1.First = p2.Second;
      p1.Second = p2.First;
      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
    }


    [Test]
    public void EqualsTest()
    {
      Pair<int> p1 = new Pair<int>();
      Pair<int> p2 = new Pair<int>();
      Assert.IsTrue(p1.Equals(p1));
      Assert.IsTrue(p1.Equals((object)p1));
      Assert.IsFalse(p1.Equals(null));
      Assert.IsFalse(p1.Equals((object)null));
      Assert.IsTrue(p1.Equals(p2));
      Assert.IsTrue(p1 == p2);
      Assert.IsFalse(p1 != p2);
      p1.First = 12;
      Assert.IsFalse(p1.Equals(p2));
      Assert.IsFalse(p1 == p2);
      Assert.IsTrue(p1 != p2);
      p2.First = 12;
      Assert.IsTrue(p1.Equals(p2));
      p1.Second = 34;
      Assert.IsFalse(p1.Equals(p2));
      p2.Second = 34;
      Assert.IsTrue(p1.Equals(p2));
      p1.First = 13;
      Assert.IsFalse(p1.Equals(p2));
      p2.First = 13;
      Assert.IsTrue(p1.Equals(p2));

      p1.First = p2.Second;
      p1.Second = p2.First;
      Assert.IsTrue(p1.Equals(p2));
    }
    

    [Test]
    public void PropertiesTest()
    {
      Pair<int> p = new Pair<int>();

      p.First = 1;
      Assert.AreEqual(1, p.First);
      Assert.AreEqual(0, p.Second);

      p.Second = 2;
      Assert.AreEqual(1, p.First);
      Assert.AreEqual(2, p.Second);
    }


#if !SILVERLIGHT
    [Test]
    public void BinarySerialization()
    {
      Pair<float> p = new Pair<float>(1.0f, 2.0f);

      MemoryStream stream = new MemoryStream();
      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(stream, p);
      
      
      stream.Seek(0, SeekOrigin.Begin);
      Pair<float> p2 = (Pair<float>)formatter.Deserialize(stream);
      Assert.AreEqual(p, p2);
    }
#endif


    [Test]
    public void ToStringTest()
    {
      Pair<int> p = new Pair<int>();
      Assert.AreEqual("(0; 0)", p.ToString());
      p.First = 10;
      p.Second = 20;
      Assert.AreEqual("(10; 20)", p.ToString());
    }


    [Test]
    public void XmlSerialization()
    {
      Pair<float> p = new Pair<float>(1.0f, 2.0f);

      StringBuilder buffer = new StringBuilder();
      StringWriter writer = new StringWriter(buffer);
      XmlSerializer serializer = new XmlSerializer(typeof(Pair<float>));
      serializer.Serialize(writer, p);
      writer.Close();

      StringReader reader = new StringReader(buffer.ToString());
      Pair<float> p2 = (Pair<float>) serializer.Deserialize(reader);
      Assert.AreEqual(p, p2);
    }
  }
}
