//using System;
//using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;
//using NUnit.Framework;


//namespace DigitalRune.ServiceLocation.Tests
//{
//  // These unit tests do not make much sense. They are only here to get a test coverage of 100% in NCover.
//  [TestFixture]
//  public class ServiceNotFoundExceptionTest
//  {
//    [Test]
//    public void TestConstructors()
//    {
//      var e = new ServiceNotFoundException();

//      e = new ServiceNotFoundException("hallo");
//      Assert.AreEqual("hallo", e.Message);

//      e = new ServiceNotFoundException("hallo", new Exception("inner"));
//      Assert.AreEqual("hallo", e.Message);
//      Assert.AreEqual("inner", e.InnerException.Message);
//    }

//    [Test]
//    public void SerializationBinary()
//    {
//      ServiceNotFoundException r1 = new ServiceNotFoundException("hallo");

//      string fileName = "SerializationServiceNotFoundException.bin";

//      if (File.Exists(fileName))
//        File.Delete(fileName);

//      FileStream fs = new FileStream(fileName, FileMode.Create);

//      BinaryFormatter formatter = new BinaryFormatter();
//      formatter.Serialize(fs, r1);
//      fs.Close();

//      fs = new FileStream(fileName, FileMode.Open);
//      formatter = new BinaryFormatter();
//      var r2 = (ServiceNotFoundException)formatter.Deserialize(fs);
//      fs.Close();

//      Assert.AreEqual(r1.Message, r2.Message);
//    }
//  }
//}
