using System;
using NUnit.Framework;


namespace DigitalRune.CommandLine.Tests
{
    [TestFixture]
    public class EnumArgumentTest
    {
        enum InvalidEnum
        {
        }

        enum MyEnum
        {
            Value1,
            Value2,
            Value3
        }


        [Flags]
        enum MyFlags
        {
            Flag1 = 1,
            Flag2 = 2,
            Flag3 = 4
        }


        [Test]
        public void InvalidType()
        {
            Assert.Throws<ArgumentException>(() => new EnumArgument<int>("name", ""));
        }


        [Test]
        public void InvalidEnumeration()
        {
            Assert.Throws<ArgumentException>(() => new EnumArgument<InvalidEnum>("name", ""));
        }


        [Test]
        public void ParseEnumeration()
        {
            EnumArgument<MyEnum> argument = new EnumArgument<MyEnum>("name", "");

            Assert.AreEqual("name", argument.Name);
            Assert.IsFalse(argument.AllowMultiple);
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetSyntax()));
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetHelp()));

            int i = 1;
            string[] args = { "other", "value2", "other" };
            var result = (ArgumentResult<MyEnum>)argument.Parse(args, ref i);
            Assert.AreEqual(2, i);
            Assert.IsNotNull(result.Values);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(MyEnum.Value2, result.Values[0]);
        }


        [Test]
        public void DuplicateValue()
        {
            EnumArgument<MyEnum> argument = new EnumArgument<MyEnum>("name", "");

            Assert.AreEqual("name", argument.Name);
            Assert.IsFalse(argument.AllowMultiple);
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetSyntax()));
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetHelp()));

            int i = 1;
            string[] args = { "other", "value2", "value1", "other" };
            var result = (ArgumentResult<MyEnum>)argument.Parse(args, ref i);
            Assert.AreEqual(2, i);
            Assert.IsNotNull(result.Values);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(MyEnum.Value2, result.Values[0]);
        }


        //[Test]
        //[ExpectedException(typeof(InvalidArgumentValueException))]
        //public void MultipleValuesForEnumeration()
        //{
        //    EnumArgument<MyEnum> argument = new EnumArgument<MyEnum>("name", "");
        //    int i = 1;
        //    string[] args = { "other", "Value1", "value2", };
        //    argument.Parse(args, ref i);
        //}


        [Test]
        public void ParseFlags1()
        {
            EnumArgument<MyFlags> argument = new EnumArgument<MyFlags>("MyFlags", "");
            Assert.AreEqual("MyFlags", argument.Name);
            Assert.IsTrue(argument.AllowMultiple);

            int i = 1;
            string[] args = { "other", "Flag1", "flag3", };
            var result = (ArgumentResult<MyFlags>)argument.Parse(args, ref i);
            Assert.AreEqual(3, i);
            Assert.IsNotNull(result.Values);
            Assert.AreEqual(2, result.Values.Count);
            Assert.AreEqual(MyFlags.Flag1, result.Values[0]);
            Assert.AreEqual(MyFlags.Flag3, result.Values[1]);
        }


        //[Test]
        //public void ParseFlags2()
        //{
        //    EnumArgument<MyFlags> argument = new EnumArgument<MyFlags>("MyFlags", "") { IsOptional = true };
        //    Assert.AreEqual("MyFlags", argument.Name);
        //    Assert.IsTrue(argument.AllowMultiple);

        //    int i = 1;
        //    string[] args = { "other", "Flag1", ",", "flag3", };
        //    var result = (ArgumentResult<MyFlags>)argument.Parse(args, ref i);
        //    Assert.AreEqual(4, i);
        //    Assert.IsNotNull(result.Values);
        //    Assert.AreEqual(2, result.Values.Count);
        //    Assert.AreEqual(MyFlags.Flag1, result.Values[0]);
        //    Assert.AreEqual(MyFlags.Flag3, result.Values[1]);
        //}


        //[Test]
        //public void ParseFlags3()
        //{
        //    EnumArgument<MyFlags> argument = new EnumArgument<MyFlags>("MyFlags", "");
        //    Assert.AreEqual("MyFlags", argument.Name);
        //    Assert.IsTrue(argument.AllowMultiple);

        //    int i = 1;
        //    string[] args = { "other", "Flag1", ";", "flag3", "other" };
        //    var result = (ArgumentResult<MyFlags>)argument.Parse(args, ref i);
        //    Assert.AreEqual(4, i);
        //    Assert.IsNotNull(result.Values);
        //    Assert.AreEqual(2, result.Values.Count);
        //    Assert.AreEqual(MyFlags.Flag1, result.Values[0]);
        //    Assert.AreEqual(MyFlags.Flag3, result.Values[1]);
        //}


        //[Test]
        //public void ParseFlags4()
        //{
        //    EnumArgument<MyFlags> argument = new EnumArgument<MyFlags>("MyFlags", "");
        //    Assert.AreEqual("MyFlags", argument.Name);
        //    Assert.IsTrue(argument.AllowMultiple);

        //    int i = 1;
        //    string[] args = { "other", "Flag1|flag3", ",", "flag2", "other" };
        //    var result = (ArgumentResult<MyFlags>)argument.Parse(args, ref i);
        //    Assert.AreEqual(4, i);
        //    Assert.IsNotNull(result.Values);
        //    Assert.AreEqual(3, result.Values.Count);
        //    Assert.AreEqual(MyFlags.Flag1, result.Values[0]);
        //    Assert.AreEqual(MyFlags.Flag3, result.Values[1]);
        //    Assert.AreEqual(MyFlags.Flag2, result.Values[2]);
        //}
    }
}
