using System;
using System.Linq;
using DigitalRune.CommandLine;


namespace CommandLineParserTest
{
    internal enum MyEnum
    {
        Value1,
        Value2,
        Value3
    }


    [Flags]
    internal enum MyFlags
    {
        Flag1 = 1,
        Flag2 = 2,
        Flag3 = 4
    }


    class Program
    {
        // See system error codes: http://msdn.microsoft.com/en-us/library/windows/desktop/ms681382.aspx
        private const int ERROR_SUCCESS = 0;
        private const int ERROR_BAD_ARGUMENTS = 0xA0;


        static int Main(string[] args)
        {
            CommandLineParser parser = new CommandLineParser
            {
                AllowUnknownArguments = true,
            };

            parser.Description = "This is just a test application that demonstrates the usage of the CommandLineParser.";

            parser.HelpHeader =
@"DESCRIPTION
    Here is a very detailed description of this app. Yadda, yadda, yadda...
    Yadda, yadda, yadda...
    Yadda, yadda, yadda...";

            parser.HelpFooter = 
@"EXAMPLES
    Show the help text:
        CommandLineApp --help
    
    Do something else:
        CommandLineApp --Enum Value1 Foo";

            var helpArgument = new SwitchArgument("help", "Show help.", null, new[] { 'h', '?' })
            {
                Category = "Help",
                IsOptional = true,
            };
            parser.Arguments.Add(helpArgument);

            var fileArgument = new ValueArgument<string>("file", "The file to load.")
            {
                IsOptional = false,
                AllowMultiple = true,
            };
            parser.Arguments.Add(fileArgument);

            var recursiveArgument = new SwitchArgument(
                "Recursive",
                "Enables recursive mode. \n This is just a demo text to test the formatting capabilities of the CommandLineParser.",
                null,
                new[] { 'R' })
            {
                IsOptional = true,
            };
            parser.Arguments.Add(recursiveArgument);

            var switchArgument1 = new SwitchArgument(
                "Switch1",
                "Another test switch. This is just a demo text to test the formatting capabilities of the CommandLineParser.")
            {
                Category = "Test Category",
                IsOptional = true,
            };
            parser.Arguments.Add(switchArgument1);

            var switchArgument2 = new SwitchArgument(
                "Switch2",
                "Yet another test switch. This is just a demo text to test the formatting capabilities of the CommandLineParser. abcdefghijklmnopqrstuvw0123456789ABCDEFRGHIJKLMNOPQRSTUVWXYZ0123456789",
                null,
                new[] { 'S' })
            {
                Category = "Test Category",
                IsOptional = true,
            };
            parser.Arguments.Add(switchArgument2);

            SwitchArgument longArgument = new SwitchArgument(
                "extremelyLongCommandLineArgumentToTestFormatting",
                "Extremely long argument. This is just a demo text to test the formatting capabilities of the CommandLineParser.",
                null,
                new[] { 'e' })
            {
                Category = "Test Category",
                IsOptional = true,
            };
            parser.Arguments.Add(longArgument);

            var echoArgument = new SwitchValueArgument<string>(
                "echo",
                new ValueArgument<string>("text", null),
                "Prints the given text.")
            {
                Category = null,
                IsOptional = true,
            };
            parser.Arguments.Add(echoArgument);

            var valueArgument = new SwitchValueArgument<int>(
                "value",
                new ValueArgument<int>("value", null) { AllowMultiple = true },
                "This switch has an integer value.",
                null,
                new[] { 'v' })
            {
                Category = "Test Category 2",
                IsOptional = true,
            };
            parser.Arguments.Add(valueArgument);

            var boundedArgument = new SwitchValueArgument<float>(
                "bounded",
                new BoundedValueArgument<float>("boundedValue", null, 1, 5) { AllowMultiple = true, IsOptional = true },
                "This is a bounded integer value.",
                null,
                new[] { 'b' })
            {
                Category = "Test Category 2",
                IsOptional = true,
            };
            parser.Arguments.Add(boundedArgument);

            var enumArgument = new SwitchValueArgument<MyEnum>(
                "Enum",
                new EnumArgument<MyEnum>("MyEnum", null) { IsOptional = true },
                "This is an enumeration.",
                null,
                new[] { 'e' })
            {
                Category = "Test Category 2",
                IsOptional = true,
            };
            parser.Arguments.Add(enumArgument);

            var flagsArgument = new SwitchValueArgument<MyFlags>(
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

            ParseResult parseResult;
            try
            {
                parseResult = parser.Parse(args);

                if (parseResult.ParsedArguments[helpArgument] != null)
                {
                    // Show help and exit.
                    Console.WriteLine(parser.GetHelp());
                    return ERROR_SUCCESS;
                }

                parser.ThrowIfMandatoryArgumentIsMissing(parseResult);
            }
            catch (CommandLineParserException exception)
            {
                Console.Error.WriteLine("ERROR");
                Console.Error.WriteLineIndented(exception.Message, 4);
                Console.Error.WriteLine();
                Console.Out.WriteLine("SYNTAX");
                Console.Out.WriteLineIndented(parser.GetSyntax(), 4);
                Console.Out.WriteLine();
                Console.Out.WriteLine("Try 'CommandLineApp --help' for more information.");
                return ERROR_BAD_ARGUMENTS;
            }

            var echo = parseResult.ParsedArguments["echo"] as ArgumentResult<string>;
            if (echo != null)
            {
                Console.Out.WriteLineWrapped(echo.Values[0]);
                return ERROR_SUCCESS;
            }

            Console.WriteLine("----- Raw arguments");
            foreach (var arg in parseResult.RawArguments)
                Console.WriteLine(arg);

            Console.WriteLine();

            Console.WriteLine("----- Unknown arguments");
            foreach (var arg in parseResult.UnknownArguments)
                Console.WriteLine(arg);

            Console.WriteLine();

            Console.WriteLine("----- Parsed arguments");
            foreach (var arg in parseResult.ParsedArguments)
            {
                Console.Write("--");
                Console.Write(arg.Argument.Name);

                var values = arg.Values.Cast<object>().ToArray();

                if (values.Length > 0)
                {
                    Console.WriteLine(":");
                    foreach (var value in values)
                    {
                        Console.Write("    ");
                        Console.WriteLine(value);
                    }
                }
                else
                {
                    Console.WriteLine();
                }
            }

            return ERROR_SUCCESS;
        }
    }
}
