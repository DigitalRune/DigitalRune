// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.CommandLine;


namespace DigitalRune.Tools
{
    partial class Program
    {
        // Command-line arguments.
        private static ValueArgument<string> _inputArgument;
        private static SwitchValueArgument<string> _directoryArgument;
        private static SwitchArgument _recursiveArgument;
        private static SwitchValueArgument<string> _outputArgument;
        private static SwitchValueArgument<string> _passwordArgument;
        private static SwitchValueArgument<PackageEncryption> _encryptionArgument;
        private static SwitchArgument _testArgument;
        private static SwitchArgument _helpArgument;


        private static CommandLineParser ConfigureCommandLine()
        {
            var parser = new CommandLineParser
            {
                AllowUnknownArguments = false,

                Description =
@"This command-line tool can be used to pack files and directories into a single
package. The resulting package file uses the ZIP format and can be read with
standard ZIP tools.",

                HelpHeader =
@"If no package exists at the target location, a new file is created.
When the package already exists, it is updated.The input files are compared
with the packaged files. New files are added, modified files are updated, and
missing files are removed from the package. Files are compared by checking the
""Last Modified"" time and the file size.",

                HelpFooter =
@"Credits
-------
Pack.exe - Copyright (C) 2014 DigitalRune GmbH. All rights reserved.
DotNetZip - Copyright (C) 2006-2011 Dino Chiesa. All rights reserved.
jzlib - Copyright (C) 2000-2003 ymnk, JCraft,Inc. All rights reserved.
zlib - Copyright (C) 1995-2004 Jean-loup Gailly and Mark Adler."
            };

            var categoryMandatory = "Mandatory";
            var categoryOptional = "Optional";

            // ----- Mandatory arguments

            // Input files
            _inputArgument = new ValueArgument<string>(
                "files",
@"Defines the files and directories to add to the package.
The specified files and directories are relative to the current working copy or the base directory, which can be specified with /directory.
Wildcards ('?', '*') are supported.")
            {
                Category = categoryMandatory,
                AllowMultiple = true,
            };
            parser.Arguments.Add(_inputArgument);

            // --output, --out, -o
            _outputArgument = new SwitchValueArgument<string>(
                "output",
                new ValueArgument<string>("package", "The filename incl. path of package."),
                "Defines the package to create or update.",
                new[] { "out" },
                new[] { 'o' })
            {
                Category = categoryMandatory,
            };
            parser.Arguments.Add(_outputArgument);

            // ----- Optional arguments

            // --directory, --dir, -d
            _directoryArgument = new SwitchValueArgument<string>(
                "directory",
                new ValueArgument<string>("directory", "The base directory."),
                "Specifies the base directory where to search for files.",
                new[] { "dir" },
                new[] { 'd' })
            {
                Category = categoryOptional,
                IsOptional = true,
            };
            parser.Arguments.Add(_directoryArgument);

            // --recursive, --rec, -r
            _recursiveArgument = new SwitchArgument(
                "recursive",
                "Adds subdirectories to package.",
                new[] { "rec" },
                new[] { 'r' })
            {
                Category = categoryOptional,
                IsOptional = true,
            };
            parser.Arguments.Add(_recursiveArgument);

            // --password, --pwd, -p
            _passwordArgument = new SwitchValueArgument<string>(
                "password",
                new ValueArgument<string>("password", "The password to use."),
                "Encrypts the package with a password.",
                new[] { "pwd" },
                new[] { 'p' })
            {
                Category = categoryOptional,
                IsOptional = true,
            };
            parser.Arguments.Add(_passwordArgument);

            // --encryption, --enc, -e
            _encryptionArgument = new SwitchValueArgument<PackageEncryption>(
                "encryption",
                new EnumArgument<PackageEncryption>("method", "The default encryption method is ZipCrypto."),
                "Defines the encryption method in case a password is set.",
                new[] { "enc" },
                new[] { 'e' })
            {
                Category = categoryOptional,
                IsOptional = true,
            };
            parser.Arguments.Add(_encryptionArgument);

            // --test, -t
            _testArgument = new SwitchArgument(
                "test",
                "Makes a test run without creating/updating the actual package.",
                null,
                new[] { 't' })
            {
                Category = categoryOptional,
                IsOptional = true,
            };
            parser.Arguments.Add(_testArgument);

            // --help, -h, -?
            _helpArgument = new SwitchArgument(
                "help",
                "Shows help information.",
                null,
                new[] { 'h', '?' })
            {
                Category = "Help",
                IsOptional = true,
            };
            parser.Arguments.Add(_helpArgument);

            return parser;
        }
    }
}
