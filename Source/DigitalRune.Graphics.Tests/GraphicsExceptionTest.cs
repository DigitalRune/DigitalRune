using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;


namespace DigitalRune.Graphics.Tests
{
  [TestFixture]
  public class GraphicsExceptionTest
  {
    [Test]
    public void TestConstructors()
    {
      var exception = new GraphicsException();

      exception = new GraphicsException("message");
      Assert.AreEqual("message", exception.Message);

      exception = new GraphicsException("message", new Exception("inner"));
      Assert.AreEqual("message", exception.Message);
      Assert.AreEqual("inner", exception.InnerException.Message);
    }


    [Test]
    public void SerializationBinary()
    {
      GraphicsException exception1 = new GraphicsException("message");

      string fileName = "SerializationGraphicsException.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fileStream = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fileStream, exception1);
      fileStream.Close();

      fileStream = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      GraphicsException exception2 = (GraphicsException)formatter.Deserialize(fileStream);
      fileStream.Close();

      Assert.AreEqual(exception1.Message, exception2.Message);
    }
  }
}