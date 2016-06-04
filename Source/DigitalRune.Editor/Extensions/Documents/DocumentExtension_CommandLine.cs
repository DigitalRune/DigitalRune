// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using DigitalRune.CommandLine;


namespace DigitalRune.Editor.Documents
{
    partial class DocumentExtension
    {
        private ValueArgument<string> _fileArgument;


        private void AddCommandLineArguments()
        {
            // Add command-line argument "<file>"
            _fileArgument = new ValueArgument<string>("file", "Specifies the file to open on startup.")
            {
                IsOptional = true,
                AllowMultiple = true,
                Category = "Documents"
            };
            Editor.CommandLineParser.Arguments.Add(_fileArgument);
        }


        private void RemoveCommandLineArguments()
        {
            Editor.CommandLineParser.Arguments.Remove(_fileArgument);
        }


        /// <summary>
        /// Opens the documents that were specified using command-line arguments.
        /// </summary>
        /// <param name="commandLineResult">The command line parse result.</param>
        /// <remarks>
        /// This method is called automatically when the applications starts. However, in a
        /// single-instance application it can be necessary to call this method manually: For
        /// example, when the single-instance application is started the method is called
        /// automatically to load all files specified on the command line. When a new instance of
        /// the same application is started the command-line arguments are redirected to the
        /// existing instance. The existing instance needs to parse the new command line arguments
        /// and then call <see cref="OpenFromCommandLineAsync"/> explicitly.
        /// </remarks>
        public async Task OpenFromCommandLineAsync(ParseResult commandLineResult)
        {
            var fileArgument = commandLineResult.ParsedArguments["file"];
            if (fileArgument != null)
            {
                // The user has specified files on the command-line.
                var files = new List<string>();
                foreach (string filePattern in fileArgument.Values)
                {
                    Logger.Debug(CultureInfo.InvariantCulture, "Opening file \"{0}\" from command-line.", filePattern);

                    try
                    {
                        bool anyFileFound = false;

                        var searchFolder = ".";
                        var searchPattern = filePattern;
                        if (Path.IsPathRooted(filePattern))
                        {
                            searchFolder = Path.GetDirectoryName(filePattern);
                            searchPattern = Path.GetFileName(filePattern);
                        }
                        
                        foreach (var file in Directory.EnumerateFiles(searchFolder, searchPattern))
                        {
                            anyFileFound = true;
                            files.Add(Path.GetFullPath(file));
                        }

                        if (!anyFileFound)
                            Logger.Error(CultureInfo.InvariantCulture, "Cannot open file \"{0}\" from command-line. File does not exist.", filePattern);
                    }
                    catch (Exception exception)
                    {
                        Logger.Error(exception, CultureInfo.InvariantCulture, "Cannot open file \"{0}\" from command-line.", filePattern);

                        // Ignore the error. The user just gets an empty application window, but no error message.
                        // TODO: Collect all invalid files and show a message box.
                    }
                }

                await OpenAsync(files);
            }
        }
    }
}
