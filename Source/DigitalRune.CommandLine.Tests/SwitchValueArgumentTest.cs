using System;
using NUnit.Framework;


namespace DigitalRune.CommandLine.Tests
{
    [TestFixture]
    public class SwitchValueArgumentTest
    {
        [Test]
        public void ParseInteger1()
        {
            var valueArgument = new ValueArgument<int>("i", "") { AllowMultiple = true };
            var argument = new SwitchValueArgument<int>("Value", valueArgument, "Description", null, new[] { 'v' })
            {
                Category = "Category",
                IsOptional = true,
            };

            Assert.AreEqual("Value", argument.Name);
            Assert.AreEqual('v', argument.ShortAliases[0]);
            Assert.AreEqual("Description", argument.Description);
            Assert.AreEqual("Category", argument.Category);
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetSyntax()));
            Assert.IsFalse(string.IsNullOrEmpty(argument.GetHelp()));

            int i = 0;
            string[] args = { "--value:1", "23", "4", };
            var result = (ArgumentResult<int>)argument.Parse(args, ref i);
            Assert.AreEqual(3, i);
            Assert.IsNotNull(result.Values);
            Assert.AreEqual(3, result.Values.Count);
            Assert.AreEqual(1, result.Values[0]);
            Assert.AreEqual(23, result.Values[1]);
            Assert.AreEqual(4, result.Values[2]);
        }


        [Test]
        public void ParseInteger2()
        {
            var valueArg = new ValueArgument<int>("i", "") { AllowMultiple = false, IsOptional = true };
            var argument = new SwitchValueArgument<int>("Value", valueArg, "Description", null, new[] { 'v' })
            {
                Category = "Category",
                IsOptional = true,
            };

            int i = 0;
            string[] args = { "--value:1", "23", "4", "other" };
            var result = (ArgumentResult<int>)argument.Parse(args, ref i);
            Assert.AreEqual(1, i);
            Assert.AreEqual(1, result.Values.Count);
            Assert.AreEqual(1, result.Values[0]);
        }


        [Flags]
        enum MyFlags
        {
            Flag1 = 1,
            Flag2 = 2,
            Flag3 = 4
        }


        [Test]
        public void ParseFlags()
        {
            var valueArg = new EnumArgument<MyFlags>("MyFlags", "") { IsOptional = true };
            var argument = new SwitchValueArgument<MyFlags>("Flags", valueArg, "Description", null, new[] { 'f' })
            {
                Category = "Category",
                IsOptional = true,
            };

            int i = 0;
            string[] args = { "--flags:flag1", "flag3", };
            var result = (ArgumentResult<MyFlags>)argument.Parse(args, ref i);
            Assert.AreEqual(2, i);
            Assert.IsNotNull(result.Values);
            Assert.AreEqual(2, result.Values.Count);
            Assert.AreEqual(MyFlags.Flag1, result.Values[0]);
            Assert.AreEqual(MyFlags.Flag3, result.Values[1]);
        }


        [Test]
        public void ParseOptionalFlags()
        {
            var valueArg = new EnumArgument<MyFlags>("MyFlags", "") { IsOptional = true };
            var argument = new SwitchValueArgument<MyFlags>("Flags", valueArg, "Description", null, new[] { 'f' })
            {
                Category = "Category",
                IsOptional = true,
            };

            int i = 0;
            string[] args = { "-f", };
            var result = (ArgumentResult<MyFlags>)argument.Parse(args, ref i);
            Assert.AreEqual(1, i);
            Assert.IsTrue(result.Values == null || result.Values.Count == 0);
        }


        [Test]
        public void MissingFlag()
        {
            var valueArg = new EnumArgument<MyFlags>("MyFlags", "") { IsOptional = false };
            var argument = new SwitchValueArgument<MyFlags>("Flags", valueArg, "Description", null, new[] { 'f' })
            {
                Category = "Category",
                IsOptional = true,
            };

            int i = 0;
            string[] args = { "-f", };

            Assert.Throws<MissingArgumentException>(() => argument.Parse(args, ref i));
        }
    }
}
