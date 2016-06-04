using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;


namespace DigitalRune.Mathematics.Tests
{
  // These unit tests do not make much sense. They are only here to get a test coverage of 100% in NCover.
  [TestFixture]
  public class MathematicsExceptionTest
  {
    [Test]
    public void TestConstructors()
    {
      MathematicsException m = new MathematicsException();

      m = new MathematicsException("hallo");
      Assert.AreEqual("hallo", m.Message);

      m = new MathematicsException("hallo", new Exception("inner"));
      Assert.AreEqual("hallo", m.Message);
      Assert.AreEqual("inner", m.InnerException.Message);
    }

    [Test]
    [Ignore("Binary serialization not supported in PCL version.")]
    public void SerializationBinary()
    {
      MathematicsException m1 = new MathematicsException("hallo");

      string fileName = "SerializationMathematicsException.bin";

      if (File.Exists(fileName))
        File.Delete(fileName);

      FileStream fs = new FileStream(fileName, FileMode.Create);

      BinaryFormatter formatter = new BinaryFormatter();
      formatter.Serialize(fs, m1);
      fs.Close();

      fs = new FileStream(fileName, FileMode.Open);
      formatter = new BinaryFormatter();
      MathematicsException m2 = (MathematicsException) formatter.Deserialize(fs);
      fs.Close();

      Assert.AreEqual(m1.Message, m2.Message);
    }
  }
}
