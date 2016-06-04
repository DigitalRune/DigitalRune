using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;


namespace DigitalRune.CommandLine.Tests
{
    [TestFixture]
    public class CommandLineParserExceptionTest
    {
        [Test]
        public void ConstructorTest0()
        {
            var exception = new CommandLineParserException();
        }


        [Test]
        public void ConstructorTest1()
        {
            const string message = "message";
            var exception = new CommandLineParserException(message);
            Assert.AreEqual(message, exception.Message);
        }


        [Test]
        public void ConstructorTest2()
        {
            const string argument = "arg";
            const string message = "message";
            var exception = new CommandLineParserException(argument, message);
            Assert.AreEqual(argument, exception.Argument);
            Assert.AreEqual(message, exception.Message);
        }


        [Test]
        public void ConstructorTest3()
        {
            var argument = new ValueArgument<string>("arg", "");
            const string message = "message";
            var exception = new CommandLineParserException(argument, message);
            Assert.AreEqual(argument.Name, exception.Argument);
            Assert.AreEqual(message, exception.Message);
        }


        [Test]
        public void ConstructorTest4()
        {
            const string message = "message";
            var innerException = new Exception();
            var exception = new CommandLineParserException(message, innerException);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }


        [Test]
        public void ConstructorTest5()
        {
            const string argument = "arg";
            const string message = "message";
            var innerException = new Exception();
            var exception = new CommandLineParserException(argument, message, innerException);
            Assert.AreEqual(argument, exception.Argument);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }


        [Test]
        public void ConstructorTest6()
        {
            var argument = new ValueArgument<string>("arg", "");
            const string message = "message";
            var innerException = new Exception();
            var exception = new CommandLineParserException(argument, message, innerException);
            Assert.AreEqual(argument.Name, exception.Argument);
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(innerException, exception.InnerException);
        }


        [Test]
        public void SerializationTest()
        {
            const string argument = "arg01";
            const string message = "message";
            const string innerMessage = "Inner exception";
            var innerException = new Exception(innerMessage);
            var exception = new CommandLineParserException(argument, message, innerException);

            using (var memoryStream = new MemoryStream())
            {
                // Serialize exception.
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, exception);

                memoryStream.Position = 0;

                // Deserialize exception.
                formatter = new BinaryFormatter();
                var deserializedException = (CommandLineParserException)formatter.Deserialize(memoryStream);

                Assert.AreEqual(argument, deserializedException.Argument);
                Assert.AreEqual(message, deserializedException.Message);
                Assert.AreEqual(innerMessage, deserializedException.InnerException.Message);
            }
        }
    }
}