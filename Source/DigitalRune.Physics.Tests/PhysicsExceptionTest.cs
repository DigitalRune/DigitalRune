using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;


namespace DigitalRune.Physics.Tests
{
  [TestFixture]
  public class PhysicsExceptionTest
  {
    [Test]
    public void ConstructorTest()
    {
      new PhysicsException();
    }


    [Test]
    public void ConstructorTest1()
    {
      string message = "message";
      var exception = new PhysicsException(message);
      Assert.AreEqual("message", exception.Message);
    }


    [Test]
    public void ConstructorTest2()
    {
      string message = "message";
      Exception inner = new Exception("Inner exception");
      var exception = new PhysicsException(message, inner);
      Assert.AreEqual("message", exception.Message);
      Assert.AreEqual(inner, exception.InnerException);
    }


    //[Test]
    //public void ConstructorTest3()
    //{
    //  string message = "message";
    //  var exception = new PhysicsException(message);

    //  string fileName = "SerializationGraphicsScreenNotFoundException.bin";

    //  if (File.Exists(fileName))
    //    File.Delete(fileName);

    //  FileStream fileStream = new FileStream(fileName, FileMode.Create);

    //  BinaryFormatter formatter = new BinaryFormatter();
    //  formatter.Serialize(fileStream, exception);
    //  fileStream.Close();

    //  fileStream = new FileStream(fileName, FileMode.Open);
    //  formatter = new BinaryFormatter();
    //  PhysicsException exception2 = (PhysicsException)formatter.Deserialize(fileStream);
    //  fileStream.Close();

    //  Assert.AreEqual(message, exception2.Message);
    //}
  }
}
