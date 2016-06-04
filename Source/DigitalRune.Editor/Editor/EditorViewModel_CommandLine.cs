// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.CommandLine;
using static System.FormattableString;


namespace DigitalRune.Editor
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    partial class EditorViewModel
    {
        private SwitchArgument _helpArgument;


        /// <inheritdoc/>
        public CommandLineParser CommandLineParser { get; private set; }


        /// <inheritdoc/>
        public ParseResult CommandLineResult { get; private set; }


        /// <summary>
        /// Registers the default command line arguments.
        /// </summary>
        private void InitializeCommandLineParser()
        {
            CommandLineParser = new CommandLineParser();

            _helpArgument = new SwitchArgument("help", "Shows the command-line help.", null, new[] { 'h', '?' })
            {
                Category = "Info",
                IsOptional = true,
            };
            CommandLineParser.Arguments.Add(_helpArgument);
        }


        /// <summary>
        /// Parses the command line arguments.
        /// </summary>
        private void ParseCommandLineArguments()
        {
            Logger.Debug("Parsing command-line arguments.");

            CommandLineResult = null;

            var args = Environment.GetCommandLineArgs();

            Debug.Assert(args != null, "The command-line arguments should never be null.");
            Debug.Assert(args.Length > 0, "The command-line arguments should at least contain the name of the executable.");

            // Skip the first parameter because in Main(string[] args) the first parameter is not the 
            // program path, but in Environment.GetCommandLineArgs it is!
            args = args.Skip(1).ToArray();

            try
            {
                Logger.Info("Command-line arguments: {0}", args);

                CommandLineResult = CommandLineParser.Parse(args);

                if (CommandLineResult.ParsedArguments[_helpArgument] != null)
                {
                    // "help" is in the parsed arguments.
                    Logger.Debug("Showing command-line usage information.");
                    using (ConsoleHelper.AttachConsole())
                    {
                        Console.WriteLine(CommandLineParser.GetHelp());
                    }

                    Exit();
                    return;
                }

                CommandLineParser.ThrowIfMandatoryArgumentIsMissing(CommandLineResult);
            }
            catch (CommandLineParserException exception)
            {
                Logger.Warn(exception, "Error parsing command-line arguments: ");

                // Show error message and usage and exit application.
                using (ConsoleHelper.AttachConsole())
                {
                    Logger.Debug("Printing error message and showing command-line usage information.");

                    Console.Error.WriteLine("ERROR");
                    Console.Error.WriteLineIndented(exception.Message, 4);
                    Console.Error.WriteLine();
                    Console.Out.WriteLine("SYNTAX");
                    Console.Out.WriteLineIndented(CommandLineParser.GetSyntax(), 4);
                    Console.Out.WriteLine();
                    Console.Out.WriteLine(Invariant($"Try '{EditorHelper.GetExecutableName()} --help' for more information."));
                }

                Exit((int)Editor.ExitCode.ERROR_BAD_ARGUMENTS);
            }
        }
    }
}
