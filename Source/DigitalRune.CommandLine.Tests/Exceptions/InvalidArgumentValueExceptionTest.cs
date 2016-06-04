using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;


namespace DigitalRune.CommandLine.Tests
{
    [TestFixture]
    public class InvalidArgumentValueExceptionTest
    {
        [Test]
        public void ConstructorTest0()
        {
            var exception = new InvalidArgumentValueException();
        }


        [Test]
        public void ConstructorTest1()
        {
            const string message = "message";
            var exception = new InvalidArgumentValueException(message);
            Assert.AreEqual(message, exception.Message);
        }


        [Test]
        public void ConstructorTest2()
        {
            const string message = "message";
            var innerException = new Exception();
            var exception = new InvalidArgumentValueException(message, innerException);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }


        [Test]
        public void ConstructorTest3()
        {
            var argument = new ValueArgument<string>("arg", "");
            const string value = "value";
            var exception = new InvalidArgumentValueException(argument, value);
            Assert.AreEqual(argument.Name, exception.Argument);
            Assert.AreEqual(value, exception.Value);
        }


        [Test]
        public void ConstructorTest4()
        {
            var argument = new ValueArgument<string>("arg", "");
            const string value = "value";
            const string message = "message";
            var exception = new InvalidArgumentValueException(argument, value, message);
            Assert.AreEqual(argument.Name, exception.Argument);
            Assert.AreEqual(value, exception.Value);
            Assert.AreEqual(message, exception.Message);
        }


        [Test]
        public void ConstructorTest5()
        {
            var argument = new ValueArgument<string>("arg", "");
            const string value = "value";
            var innerException = new Exception();
            var exception = new InvalidArgumentValueException(argument, value, innerException);
            Assert.AreEqual(argument.Name, exception.Argument);
            Assert.AreEqual(value, exception.Value);
            Assert.AreEqual(innerException, exception.InnerException);
        }


        [Test]
        public void ConstructorTest6()
        {
            var argument = new ValueArgument<string>("arg", "");
            const string value = "value";
            const string message = "message";
            var innerException = new Exception();
            var exception = new InvalidArgumentValueException(argument, value, message, innerException);
            Assert.AreEqual(argument.Name, exception.Argument);
            Assert.AreEqual(value, exception.Value);
            Assert.AreEqual(innerException, exception.InnerException);
            Assert.AreEqual(message, exception.Message);
        }


        [Test]
        public void ConstructorExceptions()
        {
            Assert.Throws<ArgumentNullException>(() => new InvalidArgumentValueException(null, ""));
            Assert.Throws<ArgumentNullException>(() => new InvalidArgumentValueException((Argument)null, "", (Exception)null));
        }


        [Test]
        public void SerializationTest()
        {
            var argument = new ValueArgument<string>("arg", "");
            const string value = "Value01";
            const string message = "message";
            const string innerMessage = "Inner exception";
            var innerException = new Exception(innerMessage);
            var exception = new InvalidArgumentValueException(argument, value, message, innerException);

            using (var memoryStream = new MemoryStream())
            {
                // Serialize exception.
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, exception);

                memoryStream.Position = 0;

                // Deserialize exception.
                formatter = new BinaryFormatter();
                var deserializedException = (InvalidArgumentValueException)formatter.Deserialize(memoryStream);

                Assert.AreEqual(argument.Name, deserializedException.Argument);
                Assert.AreEqual(value, deserializedException.Value);
                Assert.AreEqual(message, deserializedException.Message);
                Assert.AreEqual(innerMessage, deserializedException.InnerException.Message);
            }
        }


        [Test]
        public void SerializationTest2()
        {
            var argument = new ValueArgument<string>("arg", "");
            const string value = null;
            var exception = new InvalidArgumentValueException(argument, value);

            using (var memoryStream = new MemoryStream())
            {
                // Serialize exception.
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, exception);

                memoryStream.Position = 0;

                // Deserialize exception.
                formatter = new BinaryFormatter();
                var deserializedException = (InvalidArgumentValueException)formatter.Deserialize(memoryStream);

                Assert.AreEqual(argument.Name, deserializedException.Argument);
                Assert.AreEqual(value, deserializedException.Value);
            }
        }
    }
}
