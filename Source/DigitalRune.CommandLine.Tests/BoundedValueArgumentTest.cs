using System;
using NUnit.Framework;


namespace DigitalRune.CommandLine.Tests
{
    [TestFixture]
    public class BoundedValueArgumentTest
    {
        [Test]
        public void InvalidConstructor()
        {
            Assert.Throws<ArgumentException>(() => new BoundedValueArgument<int>("value", "description", 2, 1));
        }


        [Test]
        public void ParseMultiple1()
        {
            BoundedValueArgument<int> argument = new BoundedValueArgument<int>("value", "description", 1, 10) { AllowMultiple = true };
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetSyntax()));
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetHelp()));

            int i = 1;
            var result = (ArgumentResult<int>)argument.Parse(new[] { "/switch1", "3", "1", "10", }, ref i);
            Assert.AreEqual(4, i);
            Assert.AreEqual(3, result.Values.Count);
            Assert.AreEqual(3, result.Values[0]);
            Assert.AreEqual(1, result.Values[1]);
            Assert.AreEqual(10, result.Values[2]);
        }


        [Test]
        public void ParseSingle()
        {
            BoundedValueArgument<int> argument = new BoundedValueArgument<int>("value", "description", 1, 10);

            int i = 1;
            var result = (ArgumentResult<int>)argument.Parse(new[] { "/switch1", "3", "1", "10", }, ref i);
            Assert.AreEqual(2, i);
            Assert.IsNotNull(result.Values);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(3, result.Values[0]);
        }


        [Test]
        public void ParseMultiple2()
        {
            BoundedValueArgument<long> argument = new BoundedValueArgument<long>("value", "description", -10, -1) { AllowMultiple = true };

            int i = 1;
            var result = (ArgumentResult<long>)argument.Parse(new[] { "/switch1", "-3", "-1", "-10", }, ref i);
            Assert.AreEqual(4, i);
            Assert.IsNotNull(result.Values);
            Assert.AreEqual(3, result.Values.Count);
            Assert.AreEqual(-3, result.Values[0]);
            Assert.AreEqual(-1, result.Values[1]);
            Assert.AreEqual(-10, result.Values[2]);
        }


        [Test]
        public void ValueOutOfBounds1()
        {
            BoundedValueArgument<int> argument = new BoundedValueArgument<int>("value", "description", 1, 10);

            int i = 0;
            Assert.Throws<InvalidArgumentValueException>(() => argument.Parse(new[] { "0" }, ref i));
        }


        [Test]
        public void ValueOutOfBounds2()
        {
            BoundedValueArgument<int> argument = new BoundedValueArgument<int>("value", "description", -10, -1);

            int i = 0;
            Assert.Throws<InvalidArgumentValueException>(() => argument.Parse(new[] { "0" }, ref i));
        }


        [Test]
        public void ParseInvalidValue()
        {
            BoundedValueArgument<int> argument = new BoundedValueArgument<int>("value", "description", 1, 10);

            int i = 1;
            Assert.Throws<InvalidArgumentValueException>(() => argument.Parse(new[] { "/switch1", "3A", "1", "10", "x" }, ref i));
        }
    }
}