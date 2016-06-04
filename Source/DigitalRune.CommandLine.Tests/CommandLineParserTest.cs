using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;


namespace DigitalRune.CommandLine.Tests
{
    [TestFixture]
    public class CommandLineParserTest
    {
        private static bool IsNullOrEmpty(ICollection collection)
        {
            return (collection == null || collection.Count == 0);
        }


        private static bool AreListsEqual<T>(IReadOnlyList<T> a, IReadOnlyList<T> b)
        {
            if (a == null && b == null)
                return true;

            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
                if (!a[i].Equals(b[i]))   // This code does not handle a[i] == null!
                    return false;

            return true;
        }


        [Test]
        public void DefaultConstructor()
        {
            CommandLineParser parser = new CommandLineParser();
            Assert.IsFalse(parser.AllowUnknownArguments);
            Assert.AreEqual(0, parser.Arguments.Count);
            Assert.IsTrue(string.IsNullOrEmpty(parser.Description));
            Assert.IsTrue(string.IsNullOrEmpty(parser.HelpHeader));
            Assert.IsTrue(string.IsNullOrEmpty(parser.HelpFooter));
        }


        [Test]
        public void HelpText()
        {
            string description = "Description";
            string header = "Header";
            string footer = "Footer";

            CommandLineParser parser = new CommandLineParser();
            Assert.IsTrue(string.IsNullOrEmpty(parser.Description));
            Assert.IsTrue(string.IsNullOrEmpty(parser.HelpHeader));
            Assert.IsTrue(string.IsNullOrEmpty(parser.HelpFooter));

            parser.GetSyntax();
            parser.GetHelp();

            parser.Description = description;

            parser.GetSyntax();
            parser.GetHelp();

            parser.HelpHeader = header;

            parser.GetSyntax();
            parser.GetHelp();

            parser.HelpFooter = footer;

            parser.GetSyntax();
            parser.GetHelp();

            Assert.AreEqual(description, parser.Description);
            Assert.AreEqual(header, parser.HelpHeader);
            Assert.AreEqual(footer, parser.HelpFooter);

            AddTestArguments(parser);
            parser.GetSyntax();
            parser.GetHelp();
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


        private void AddTestArguments(CommandLineParser parser)
        {
            SwitchArgument helpArgument = new SwitchArgument("help", "Show help.", null, new[] { 'h', '?' })
            {
                Category = "Help",
                IsOptional = true,
            };
            parser.Arguments.Add(helpArgument);

            ValueArgument<string> fileArgument = new ValueArgument<string>("file", "The file to load.")
            {
                Category = "",
                IsOptional = true,
                AllowMultiple = true,
            };
            parser.Arguments.Add(fileArgument);

            SwitchArgument recursiveArgument = new SwitchArgument("Recursive", "Enables recursive mode.", null, new[] { 'R' })
            {
                Category = null,
                IsOptional = true
            };
            parser.Arguments.Add(recursiveArgument);

            SwitchArgument switchArgument1 = new SwitchArgument("Switch", "Another test switch.", null, new[] { 'S' })
            {
                Category = "Test Category",
                IsOptional = true,
            };
            parser.Arguments.Add(switchArgument1);

            SwitchValueArgument<int> valueArgument = new SwitchValueArgument<int>(
                "value",
                new ValueArgument<int>("value", "The value.") { AllowMultiple = true },
                "This switch has a value.",
                null,
                new[] { 'v' })
            {
                Category = "Test Category 2",
                IsOptional = true,
            };
            parser.Arguments.Add(valueArgument);

            SwitchValueArgument<float> boundedArgument = new SwitchValueArgument<float>(
                "bounded",
                new BoundedValueArgument<float>("value", "The value.", 1, 5) { AllowMultiple = true },
                "This is a bounded value.",
                null,
                new[] { 'b' })
            {
                Category = "Test Category 2",
                IsOptional = true,
            };
            parser.Arguments.Add(boundedArgument);

            SwitchValueArgument<MyEnum> enumArgument = new SwitchValueArgument<MyEnum>(
                "Enum",
                new EnumArgument<MyEnum>("MyEnum", "The value."),
                "This is an enumeration.",
                null,
                new[] { 'e' })
            {
                Category = "Test Category 2",
                IsOptional = true,
            };
            parser.Arguments.Add(enumArgument);

            SwitchValueArgument<MyFlags> flagsArgument = new SwitchValueArgument<MyFlags>(
                "Flags",
                new EnumArgument<MyFlags>("MyFlags", "The value."),
                "This is a combination of flags (= enumeration with FlagsAttribute).",
                null,
                new[] { 'f' })
            {
                Category = "Test Category 2",
                IsOptional = true,
            };
            parser.Arguments.Add(flagsArgument);
        }

        [Test]
        public void ParseTest()
        {
            CommandLineParser parser = new CommandLineParser();
            AddTestArguments(parser);
            string[] args =
            {
                "-?", "file XYZ", "file2", "--VALUE:100", "-12345", "-S", "--recursive", "--enum", "Value1",
                "--flags=","flag1", "flag3", "-b=+2.3",
            };
            var r = parser.Parse(args);
            Assert.IsTrue(AreListsEqual(args, r.RawArguments));

            var file = (ArgumentResult<string>)r.ParsedArguments["file"];
            Assert.IsNotNull(file);
            Assert.AreEqual(2, file.Values.Count);
            Assert.AreEqual("file XYZ", file.Values[0]);
            Assert.AreEqual("file2", file.Values[1]);

            var help = r.ParsedArguments[parser.Arguments["help"]];
            Assert.IsNotNull(help);

            var recursive = r.ParsedArguments["recursive"];
            Assert.IsNotNull(recursive);

            var value = (ArgumentResult<int>)r.ParsedArguments["value"];
            Assert.IsNotNull(value);
            Assert.AreEqual(2, value.Values.Count);
            Assert.AreEqual(100, value.Values[0]);
            Assert.AreEqual(-12345, value.Values[1]);

            var boundedValue = (ArgumentResult<float>)r.ParsedArguments["bounded"];
            Assert.IsNotNull(boundedValue);
            Assert.AreEqual(1, boundedValue.Values.Count);
            Assert.AreEqual(+2.3f, boundedValue.Values[0]);

            var enumArgument = (ArgumentResult<MyEnum>)r.ParsedArguments["Enum"];
            Assert.IsNotNull(enumArgument);
            Assert.AreEqual(1, enumArgument.Values.Count);
            Assert.AreEqual(MyEnum.Value1, enumArgument.Values[0]);

            var flags = (ArgumentResult<MyFlags>)r.ParsedArguments["Flags"];
            Assert.IsNotNull(flags);
            Assert.AreEqual(2, flags.Values.Count);
            Assert.AreEqual(MyFlags.Flag1, flags.Values[0]);
            Assert.AreEqual(MyFlags.Flag3, flags.Values[1]);
        }


        [Test]
        public void DuplicateArgumentsDefinitions()
        {
            CommandLineParser parser = new CommandLineParser();
            SwitchArgument switchArgument1 = new SwitchArgument("Switch", "Another test switch.", null, new[] { 'S' })
            {
                Category = "Test Category",
                IsOptional = true,
            };
            parser.Arguments.Add(switchArgument1);
            SwitchArgument switchArgument2 = new SwitchArgument("SWITCH", "Another test switch.", null, new[] { 'T' })
            {
                Category = "Test Category",
                IsOptional = true,
            };

            Assert.Throws<ArgumentException>(() => parser.Arguments.Add(switchArgument2));
        }


        [Test]
        public void MissingArgument()
        {
            CommandLineParser parser = new CommandLineParser();

            // Add mandatory argument
            SwitchArgument switchArgument = new SwitchArgument("Switch", "Another test switch.", null, new[] { 'S' })
            {
                Category = "Test Category",
                IsOptional = false,
            };
            parser.Arguments.Add(switchArgument);

            Assert.Throws<MissingArgumentException>(() => parser.Parse(new string[] { }));
        }


        [Test]
        public void UnknownArgument1()
        {
            CommandLineParser parser = new CommandLineParser();

            // Add mandatory argument
            SwitchArgument switchArgument = new SwitchArgument("Switch", "Another test switch.", null, new[] { 'S' })
            {
                Category = "Test Category",
                IsOptional = false,
            };
            parser.Arguments.Add(switchArgument);

            Assert.Throws<UnknownArgumentException>(() => parser.Parse(new[] { "unknown" }));
        }


        [Test]
        public void UnknownArgument2()
        {
            CommandLineParser parser = new CommandLineParser
            {
                AllowUnknownArguments = true
            };

            // Add mandatory argument
            SwitchArgument switchArgument = new SwitchArgument("Switch", "Another test switch.", null, new[] { 'S' })
            {
                Category = "Test Category",
                IsOptional = false,
            };
            parser.Arguments.Add(switchArgument);

            var r = parser.Parse(new[] { "unknown1", "--switch", "unknown2" });
            Assert.AreEqual(1, r.ParsedArguments.Count);
            Assert.AreEqual(switchArgument, r.ParsedArguments[0].Argument);
            Assert.AreEqual(2, r.UnknownArguments.Count);
            Assert.AreEqual("unknown1", r.UnknownArguments[0]);
            Assert.AreEqual("unknown2", r.UnknownArguments[1]);
        }
    }
}