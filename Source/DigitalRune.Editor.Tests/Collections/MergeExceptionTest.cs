using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;


namespace DigitalRune.Collections.Tests
{
    // These unit tests do not make much sense. They are only here to get a test coverage of 100% in NCover.
    [TestFixture]
    public class MergeExceptionTest
    {
        [Test]
        public void TestConstructors()
        {
            var e = new MergeException();

            e = new MergeException("hallo");
            Assert.AreEqual("hallo", e.Message);

            e = new MergeException("hallo", new Exception("inner"));
            Assert.AreEqual("hallo", e.Message);
            Assert.AreEqual("inner", e.InnerException.Message);
        }


        [Test]
        public void SerializationBinary()
        {
            MergeException r1 = new MergeException("hallo");

            using (var memoryStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, r1);

                memoryStream.Position = 0;

                formatter = new BinaryFormatter();
                var r2 = (MergeException)formatter.Deserialize(memoryStream);

                Assert.AreEqual(r1.Message, r2.Message);
            }
        }
    }
}
