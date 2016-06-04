// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using DigitalRune.CommandLine;


namespace DigitalRune.Editor.About
{
    partial class AboutExtension
    {
        private SwitchArgument _aboutArgument;
        private SwitchArgument _versionArgument;


        private void AddCommandLineArguments()
        {
            if (!Editor.CommandLineParser.Arguments.Contains("about"))
            {
                // Add command-line argument: "--about"
                _aboutArgument = new SwitchArgument(
                    "about", 
                    "Show information about the application and installed components and copy the information into the clipboard.")
                {
                    Category = "Info",
                    IsOptional = true,
                };
                Editor.CommandLineParser.Arguments.Add(_aboutArgument);
            }
            else
            {
                Logger.Warn("AboutExtension tried to register command line argument 'about' but this argument was already registered.");
            }

            if (!Editor.CommandLineParser.Arguments.Contains("version"))
            {
                // Add command-line argument: "--version"
                _versionArgument = new SwitchArgument("version", "Show the version information.")
                {
                    Category = "Info",
                    IsOptional = true,
                };
                Editor.CommandLineParser.Arguments.Add(_versionArgument);
            }
            else
            {
                Logger.Warn("AboutExtension tried to register command line argument 'version' but this argument was already registered.");
            }
        }


        private void RemoveCommandLineArguments()
        {
            Editor.CommandLineParser.Arguments.Remove(_aboutArgument);
            Editor.CommandLineParser.Arguments.Remove(_versionArgument);
        }


        /// <summary>
        /// Parses the command line arguments.
        /// </summary>
        private void ParseCommandLineArguments()
        {
            Logger.Debug("Parsing command-line arguments.");

            if (Editor.CommandLineResult.ParsedArguments[_aboutArgument] != null)
            {
                Logger.Debug("Printing about information.");
                using (ConsoleHelper.AttachConsole())
                {
                    string information = GetInformation();
                    Logger.Info(CultureInfo.InvariantCulture, "About information:\n{0}", information);
                    Console.WriteLine(information);
                }

                CopyInformationToClipboard();

                Editor.Exit();
            }
            else if (Editor.CommandLineResult.ParsedArguments[_versionArgument] != null)
            {
                Logger.Debug("Printing version information.");
                using (ConsoleHelper.AttachConsole())
                {
                    Console.WriteLine("{0} {1}", ApplicationName, Version);
                    foreach (var extensionDescription in ExtensionDescriptions)
                        Console.WriteLine("{0} {1}", extensionDescription.Name, extensionDescription.Version);
                }

                Editor.Exit();
            }
        }
    }
}
