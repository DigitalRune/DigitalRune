using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;


namespace DigitalRune.CommandLine.Tests
{
    [TestFixture]
    public class UnknownArgumentExceptionTest
    {
        [Test]
        public void ConstructorTest0()
        {
            var exception = new UnknownArgumentException();
        }


        [Test]
        public void ConstructorTest1()
        {
            const string argument = "Argument";
            var exception = new UnknownArgumentException(argument);
            Assert.AreEqual(argument, exception.Argument);
        }


        [Test]
        public void ConstructorTest2()
        {
            const string argument = "Argument";
            const string message = "message";
            var exception = new UnknownArgumentException(argument, message);
            Assert.AreEqual(argument, exception.Argument);
            Assert.AreEqual(message, exception.Message);
        }


        [Test]
        public void ConstructorTest3()
        {
            const string message = "message";
            var innerException = new Exception();
            var exception = new UnknownArgumentException(message, innerException);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }


        [Test]
        public void ConstructorTest4()
        {
            const string argument = "Argument";
            const string message = "message";
            var innerException = new Exception();
            var exception = new UnknownArgumentException(argument, message, innerException);
            Assert.AreEqual(argument, exception.Argument);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }


        [Test]
        public void ConstructorExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => new UnknownArgumentException(null));
        }


        [Test]
        public void SerializationTest()
        {
            const string argument = "Argument";
            const string message = "message";
            const string innerMessage = "Inner exception";
            var innerException = new Exception(innerMessage);
            var exception = new UnknownArgumentException(argument, message, innerException);

            using (var memoryStream = new MemoryStream())
            {
                // Serialize exception.
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, exception);

                memoryStream.Position = 0;

                // Deserialize exception.
                formatter = new BinaryFormatter();
                var deserializedException = (UnknownArgumentException)formatter.Deserialize(memoryStream);

                Assert.AreEqual(argument, deserializedException.Argument);
                Assert.AreEqual(message, deserializedException.Message);
                Assert.AreEqual(innerMessage, deserializedException.InnerException.Message);
            }
        }
    }
}
