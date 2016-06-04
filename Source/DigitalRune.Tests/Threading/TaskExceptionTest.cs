using System;
using NUnit.Framework;

#if !NETFX_CORE && !SILVERLIGHT
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#endif


namespace DigitalRune.Threading.Tests
{
  [TestFixture]
  public class TaskExceptionTest
  {
    [Test]
    public void TestConstructors()
    {
      var exception0 = new Exception("First exception");
      var exception1 = new Exception("Second exception");
      var exception = new TaskException(new [] { exception0, exception1 });

      Assert.IsFalse(String.IsNullOrEmpty(exception.Message));
      Assert.IsNotNull(exception.InnerExceptions);
      Assert.AreEqual(2, exception.InnerExceptions.Length);
      Assert.AreEqual(exception0, exception.InnerExceptions[0]);
      Assert.AreEqual(exception1, exception.InnerExceptions[1]);
    }


#if !NETFX_CORE && !SILVERLIGHT && !ANDROID
    [Test]
    public void SerializationBinary()
    {
      var exception0 = new Exception("First exception");
      var exception1 = new Exception("Second exception");
      var exception = new TaskException(new[] { exception0, exception1 });

      const string fileName = "SerializationTaskException.bin";
      if (File.Exists(fileName))
        File.Delete(fileName);

      var fileStream = new FileStream(fileName, FileMode.Create);

      var formatter = new BinaryFormatter();
      formatter.Serialize(fileStream, exception);
      fileStream.Close();

      fileStream = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      var deserializedException = (TaskException)formatter.Deserialize(fileStream);
      fileStream.Close();

      Assert.AreEqual(exception.Message, deserializedException.Message);
      Assert.AreEqual(exception0.Message, deserializedException.InnerExceptions[0].Message);
      Assert.AreEqual(exception1.Message, deserializedException.InnerExceptions[1].Message);
    }
#endif
  }
}
