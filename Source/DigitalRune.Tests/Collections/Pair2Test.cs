using System;
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
  public class Pair2Test
  {
    [Test]
    public void ConstructorEmpty()
    {
      Pair<float, object> p = new Pair<float, object>();
      Assert.AreEqual(0, p.First);
      Assert.AreEqual(null, p.Second);
    }


    [Test]
    public void ConstructorWithArguments()
    {
      Pair<float, object> p = new Pair<float, object>(1.0f, "Hallo");
      Assert.AreEqual(1.0f, p.First);
      Assert.AreEqual("Hallo", p.Second);
    }


    [Test]
    public void GetHashCodeTest()
    {
      Pair<int, String> p1 = new Pair<int, string>();
      Pair<int, String> p2 = new Pair<int, string>();
      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
      p1.First = 12;
      Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());
      p2.First = 12;
      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
      p1.Second = "Hallo";
      Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());
      p2.Second = "Hallo";
      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
      p1.First = 13;
      Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());
      p2.First = 13;
      Assert.AreEqual(p1.GetHashCode(), p2.GetHashCode());
      p1.Second = "Hallo2";
      Assert.AreNotEqual(p1.GetHashCode(), p2.GetHashCode());
    }


    [Test]
    public void EqualsTest()
    {
      Pair<int, String> p1 = new Pair<int, string>();
      Pair<int, String> p2 = new Pair<int, string>();
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
      p1.Second = "Hallo";
      Assert.IsFalse(p1.Equals(p2));
      p2.Second = "Hallo";
      Assert.IsTrue(p1.Equals(p2));
      p1.First = 13;
      Assert.IsFalse(p1.Equals(p2));
      p2.First = 13;
      Assert.IsTrue(p1.Equals(p2));
      p1.Second = "Hallo2";
      Assert.IsFalse(p1.Equals(p2));
    }
    

    [Test]
    public void PropertiesTest()
    {
      Pair<float, object> p = new Pair<float, object>();
      Assert.AreEqual(0, p.First);
      Assert.AreEqual(null, p.Second);

      p.First = 12.3f;
      Assert.AreEqual(12.3f, p.First);
      Assert.AreEqual(null, p.Second);

      p.Second = "Test123";
      Assert.AreEqual(12.3f, p.First);
      Assert.AreEqual("Test123", p.Second);

      p.First = 0;
      p.Second = null;
      Assert.AreEqual(0, p.First);
      Assert.AreEqual(null, p.Second);
    }


#if !SILVERLIGHT
    [Test]
    public void BinarySerialization()
    {
      Pair<float, object> p = new Pair<float, object>(1.0f, "Hallo");

      MemoryStream stream = new MemoryStream();
      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(stream, p);
      
      
      stream.Seek(0, SeekOrigin.Begin);
      Pair<float, object> p2 = (Pair<float, object>)formatter.Deserialize(stream);
      Assert.AreEqual(p, p2);
    }
#endif


    [Test]
    public void ToStringTest()
    {
      Pair<int, object> p = new Pair<int, object>();
      Assert.AreEqual("(0; )", p.ToString());
      p.First = 10;
      p.Second = "Hallo";
      Assert.AreEqual("(10; Hallo)", p.ToString());
    }


    [Test]
    public void XmlSerialization()
    {
      Pair<float, object> p = new Pair<float, object>(1.0f, "Hallo");

      StringBuilder buffer = new StringBuilder();
      StringWriter writer = new StringWriter(buffer);
      XmlSerializer serializer = new XmlSerializer(typeof(Pair<float, object>));
      serializer.Serialize(writer, p);
      writer.Close();

      StringReader reader = new StringReader(buffer.ToString());
      Pair<float, object> p2 = (Pair<float, object>) serializer.Deserialize(reader);
      Assert.AreEqual(p, p2);
    }
  }
}
