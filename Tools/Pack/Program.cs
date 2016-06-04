// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Threading;
using DigitalRune.CommandLine;
using Ionic.Zip;


namespace DigitalRune.Tools
{
    partial class Program
    {
        // See system error codes: http://msdn.microsoft.com/en-us/library/windows/desktop/ms681382.aspx
        private const int ERROR_SUCCESS = 0;
        private const int ERROR_BAD_ARGUMENTS = 160;        // 0x0A0
        private const int ERROR_UNHANDLED_EXCEPTION = 574;  // 0x23E
        private const int ERROR_OPERATION_ABORTED = 995;    // 0x3E3


        private static CancellationTokenSource _cancellationTokenSource;


        static int Main(string[] args)
        {
            // ----- Evaluate command-line arguments.
            var commandLineParser = ConfigureCommandLine();

            ParseResult parseResult;
            try
            {
                parseResult = commandLineParser.Parse(args);

                if (parseResult.ParsedArguments[_helpArgument] != null)
                {
                    // Show help and exit.
                    Console.WriteLine(commandLineParser.GetHelp());
                    return ERROR_SUCCESS;
                }

                commandLineParser.ThrowIfMandatoryArgumentIsMissing(parseResult);
            }
            catch (CommandLineParserException exception)
            {
                Console.Error.WriteLine("ERROR");
                Console.Error.WriteLineIndented(exception.Message, 4);
                Console.Error.WriteLine();
                Console.Out.WriteLine("SYNTAX");
                Console.Out.WriteLineIndented(commandLineParser.GetSyntax(), 4);
                Console.Out.WriteLine();
                Console.Out.WriteLine("Try 'Pack --help' for more information.");
                return ERROR_BAD_ARGUMENTS;
            }

            // Mandatory arguments.
            var files = ((ArgumentResult<string>)parseResult.ParsedArguments[_inputArgument]).Values;
            var output = ((ArgumentResult<string>)parseResult.ParsedArguments[_outputArgument]).Values[0];

            // Optional arguments.
            string directory = (parseResult.ParsedArguments[_directoryArgument] as ArgumentResult<string>)?.Values[0];
            bool isTestRun = parseResult.ParsedArguments[_testArgument] != null;
            bool isRecursive = parseResult.ParsedArguments[_recursiveArgument] != null;
            string password = (parseResult.ParsedArguments[_passwordArgument] as ArgumentResult<string>)?.Values[0];
            PackageEncryption? encryption = (parseResult.ParsedArguments[_encryptionArgument] as ArgumentResult<PackageEncryption>)?.Values[0];

            // ----- Pack files.
            try
            {
                Console.CancelKeyPress += OnCancelKeyPressed;

                // In case the output includes UTF8 text.
                // (Entries and text in ZIP archives can include UTF8.)
                //Console.OutputEncoding = new System.Text.UTF8Encoding();

                _cancellationTokenSource = new CancellationTokenSource();
                var packageHelper = new PackageHelper
                {
                    MessageWriter = Console.Out,
                    IsTestRun = isTestRun,
                    Password = password,
                    Encryption = ToEncryptionAlgorithm(encryption ?? PackageEncryption.ZipCrypto),
                };

                packageHelper.Pack(directory, files, isRecursive, output, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine("ERROR");
                Console.Error.WriteLineIndented("Operation has been canceled.", 4);
                Console.Error.WriteLine();
                return ERROR_OPERATION_ABORTED;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERROR");
                Console.Error.WriteLineIndented(FormattableString.Invariant($"Error: {ex}"), 4);
                Console.Error.WriteLine();
                return ERROR_UNHANDLED_EXCEPTION;
            }
            finally
            {
                Console.CancelKeyPress -= OnCancelKeyPressed;
            }

            return ERROR_SUCCESS;
        }


        private static void OnCancelKeyPressed(object sender, ConsoleCancelEventArgs eventArgs)
        {
            Console.WriteLine("\nCtrl+C pressed. Canceling operation...");

            _cancellationTokenSource?.Cancel();

            // Set the Cancel property to true to prevent the process from terminating.
            eventArgs.Cancel = true;
        }


        private static EncryptionAlgorithm ToEncryptionAlgorithm(PackageEncryption encryption)
        {
            switch (encryption)
            {
                case PackageEncryption.ZipCrypto:
                    return EncryptionAlgorithm.PkzipWeak;
                case PackageEncryption.Aes256:
                    return EncryptionAlgorithm.WinZipAes256;
                default:
                    throw new NotSupportedException("The specified encryption method is not supported.");
            }
        }
    }
}
