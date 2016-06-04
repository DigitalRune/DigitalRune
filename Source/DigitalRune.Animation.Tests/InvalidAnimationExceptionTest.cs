using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using NUnit.Framework;


namespace DigitalRune.Animation.Tests
{
  [TestFixture]
  public class InvalidAnimationExceptionTest
  {
    [Test]
    public void ConstructorTest()
    {
      var exception = new InvalidAnimationException();
    }


    [Test]
    public void ConstructorTest1()
    {
      const string message = "message";
      var exception = new InvalidAnimationException(message);
      Assert.AreEqual(message, exception.Message);
    }


    [Test]
    public void ConstructorTest2()
    {
      const string message = "message";
      var innerException = new Exception();
      var exception = new InvalidAnimationException(message, innerException);
      Assert.AreEqual(message, exception.Message);
      Assert.AreEqual(innerException, exception.InnerException);
    }


    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void ConstructorTest3()
    {
      const string message = "message";
      const string innerMessage = "Inner exception";
      var innerException = new Exception(innerMessage);
      var exception = new InvalidAnimationException(message, innerException);

      const string fileName = "SerializationInvalidAnimationException.bin";
      if (File.Exists(fileName))
        File.Delete(fileName);

      // Serialize exception.
      var fileStream = new FileStream(fileName, FileMode.Create);
      var formatter = new BinaryFormatter();
      formatter.Serialize(fileStream, exception);
      fileStream.Close();

      // Deserialize exception.
      fileStream = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      var deserializedException = (InvalidAnimationException)formatter.Deserialize(fileStream);
      fileStream.Close();

      Assert.AreEqual(message, deserializedException.Message);
      Assert.AreEqual(innerMessage, deserializedException.InnerException.Message);
    }
  }
}
