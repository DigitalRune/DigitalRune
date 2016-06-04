// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using DigitalRune.Collections;
using static System.FormattableString;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// Parses command line arguments.
    /// </summary>
    /// <remarks>
    /// To use the command line parser follow these step:
    /// <para>
    /// First, provide the definitions for all allowed command line arguments 
    /// (<see cref="Argument"/>). These definitions describe how the command line should be
    /// parsed. You need to add the argument definitions to the collection 
    /// <see cref="Arguments"/>.
    /// </para>
    /// <para>
    /// Second, call <see cref="Parse"/> specifying the arguments (which were received in the 
    /// application's <c>Main</c> method or using 
    /// <see cref="Environment"/>.<see cref="Environment.GetCommandLineArgs"/>).
    /// </para>
    /// <para>
    /// The command line parser then parses the input arguments and stores the detected arguments in
    /// a <see cref="ParseResult"/> instance.
    /// </para>
    /// </remarks>
    public sealed class CommandLineParser
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private const int IndentSpacing = 4;
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether to accept unknown arguments when parsing the 
        /// command line.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to accept unknown arguments; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// "Unknown arguments" are all arguments that are not described in 
        /// <see cref="Arguments"/>.
        /// </para>
        /// <para>
        /// If <see cref="AllowUnknownArguments"/> is set to <see langword="true"/> then 
        /// <see cref="Parse"/> stores all unknown arguments in 
        /// <see cref="ParseResult.UnknownArguments"/> or the <see cref="ParseResult"/>. But 
        /// when <see cref="AllowUnknownArguments"/> is set to <see langword="false"/>, 
        /// <see cref="Parse"/> will raise an <see cref="UnknownArgumentException"/> when it detects 
        /// an unknown argument.
        /// </para>
        /// </remarks>
        public bool AllowUnknownArguments { get; set; }


        /// <summary>
        /// Gets the arguments definitions.
        /// </summary>
        /// <value>The argument definitions.</value>
        /// <remarks>
        /// <para>
        /// This collection contains the definitions of the application's command line arguments. The 
        /// <see cref="CommandLineParser"/> can only interpret arguments that are listed here.
        /// </para>
        /// </remarks>
        public NamedObjectCollection<Argument> Arguments { get; private set; }


        /// <summary>
        /// Gets or sets the short application description which is displayed in the usage string
        /// (<see cref="GetSyntax"/>).
        /// </summary>
        /// <value>
        /// The short description of the application. (Can be <see langword="null"/> or empty.)
        /// </value>
        public string Description { get; set; }


        /// <summary>
        /// Gets or sets the header of the help text (<see cref="GetHelp"/>).
        /// </summary>
        /// <value>The header of the help text. (Can be <see langword="null"/> or empty.)</value>
        public string HelpHeader { get; set; }


        /// <summary>
        /// Gets or sets the footer of the help text (<see cref="GetHelp"/>).
        /// </summary>
        /// <value>The footer of the help text. (Can be <see langword="null"/> or empty.)</value>
        public string HelpFooter { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParser"/> class.
        /// </summary>
        public CommandLineParser()
        {
            Arguments = new NamedObjectCollection<Argument>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Parses the command line arguments.
        /// </summary>
        /// <param name="args">
        /// The actual command line arguments (which were received in the application's <c>Main</c> 
        /// method).
        /// </param>
        /// <returns>
        /// The <see cref="ParseResult"/> storing the detected arguments.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <paramref name="args"/> contains only the argument - not the name of the executing
        /// program. (Typically, in C/C++ and in 
        /// <see cref="Environment"/>.<see cref="Environment.GetCommandLineArgs"/>, the first 
        /// argument is the name of the executing program. Whereas the args parameter of the C# 
        /// <c>Main</c> does not contain the program name.)
        /// </para>
        /// <para><strong>Exceptions:</strong><br/>
        /// If the arguments cannot be parsed, an exception of type 
        /// <see cref="CommandLineParserException"/> or one its derived types is thrown. 
        /// </para>
        /// <para>
        /// Please note: <see cref="MissingArgumentException"/>s are only thrown if no arguments 
        /// where parsed at all and an argument is mandatory (<see cref="Argument.IsOptional"/> is 
        /// <see langword="false"/>). If any arguments are parsed, the 
        /// <see cref="MissingArgumentException"/> is not thrown. This is necessary because all
        /// command line apps should have a "--help" argument (or something similar). Such arguments
        /// can be used without all other mandatory arguments.
        /// </para>
        /// </remarks>
        public ParseResult Parse(string[] args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            // The index of the next argument to parse
            int i = 0;

            var parsedArguments = new List<ArgumentResult>();
            var unknownArguments = new List<string>();

            // Parse all command line arguments
            while (i < args.Length)
            {
                int currentIndex = i;

                // Let each Argument try to parse the next parameter.
                // (Remark: Calling argument.Parse for each argument over and over again is not 
                // very efficient. But parsing of command line arguments usually happens only once per
                // application and does not need to be fast. 
                // The performance could be improved by creating a lookup table for all arguments. But we 
                // have decided to go for simplicity instead of performance.)
                foreach (Argument argument in Arguments)
                {
                    var result = argument.Parse(args, ref i);

                    if (result != null)
                    {
                        if (Contains(parsedArguments, argument))
                            throw new DuplicateArgumentException(argument);

                        parsedArguments.Add(result);
                    }

                    if (i >= args.Length)
                        break;
                }

                if (i == currentIndex)
                {
                    // i has not changed. args[i] does not match any of the defined arguments.
                    if (AllowUnknownArguments)
                        unknownArguments.Add(args[i++]);
                    else
                        throw new UnknownArgumentException(args[i]);
                }
            }

            // Check mandatory arguments. (See also remarks of method documentation.)
            if (parsedArguments.Count == 0)
                foreach (Argument argument in Arguments)
                    if (!argument.IsOptional)
                        throw new MissingArgumentException(argument);

            return new ParseResult(args, parsedArguments, unknownArguments);
        }


        private static bool Contains(List<ArgumentResult> results, Argument argument)
        {
            foreach (var r in results)
                if (r.Argument == argument)
                    return true;

            return false;
        }


        /// <summary>
        /// Gets the string that describes the syntax of the command.
        /// </summary>
        /// <returns>The string that describes the syntax of the command.</returns>
        /// <remarks>
        /// The <see cref="CommandLineParser"/> automatically gathers the required information to
        /// create the syntax text. The application name is taken from the
        /// <see cref="AssemblyProductAttribute"/> of the assembly. So make sure that these
        /// attributes are set properly.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetSyntax()
        {
            Assembly application = Assembly.GetEntryAssembly();

            var usage = new StringBuilder();
            if (application == null)
            {
                // We cannot access the executing assembly.
                // This happens, for example, if we run PrintApplicationInfo() in a unit test.
                // --> Do not print application info.
                usage.Append("PROGRAM");
            }
            else
            {
                usage.Append(Path.GetFileNameWithoutExtension(application.Location));
            }

            foreach (Argument argument in Arguments)
            {
                usage.Append(" ");
                usage.Append(argument.GetSyntax());
            }

            return usage.ToString();
        }


        /// <summary>
        /// Gets the application info using the assembly attributes of the entry assembly.
        /// </summary>
        /// <returns>
        /// The application info text.
        /// </returns>
        /// <exception cref="CommandLineParserException">
        /// Cannot print product info. <see cref="AssemblyProductAttribute"/> is not defined for the 
        /// entry assembly.
        /// </exception>
        /// <exception cref="CommandLineParserException">
        /// Cannot print copyright info. <see cref="AssemblyCopyrightAttribute"/> is not defined for the 
        /// entry assembly.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private static string GetApplicationInfo()
        {
            Assembly application = Assembly.GetEntryAssembly();
            if (application == null)
            {
                // We cannot access the executing assembly.
                // This happens, for example, if we run GetApplicationInfo() in a unit test.
                // --> Do not print application info.
                return string.Empty;
            }

            StringBuilder stringBuilder = new StringBuilder();

            // Print product name
            object[] attributes = application.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (attributes != null && attributes.Length > 0)
                stringBuilder.Append(((AssemblyProductAttribute)attributes[0]).Product);
            else
                throw new CommandLineParserException("Cannot print product info. AssemblyProductAttribute is not defined for the entry assembly.");

            // Print version
            Version version = application.GetName().Version;
            stringBuilder.AppendLine(Invariant($" version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}"));

            // Print copyright info
            attributes = application.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes != null && attributes.Length > 0)
                stringBuilder.AppendLine(((AssemblyCopyrightAttribute)attributes[0]).Copyright);
            else
                throw new CommandLineParserException("Cannot print copyright info. AssemblyCopyrightAttribute is not defined for the entry assembly.");

            stringBuilder.AppendLine();
            return stringBuilder.ToString();
        }


        /// <summary>
        /// Gets the help text.
        /// </summary>
        /// <returns>
        /// The formatted help text.
        /// </returns>
        /// <remarks>
        /// The <see cref="CommandLineParser"/> automatically gathers the required information to
        /// create the help text. The application name is taken from the
        /// <see cref="AssemblyProductAttribute"/> of the assembly, the version is taken from the
        /// assembly version, and the copyright info is taken from the
        /// <see cref="AssemblyCopyrightAttribute"/>. So make sure that these attributes are set
        /// properly.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetHelp()
        {
            var windowWidth = ConsoleHelper.WindowWidth;

            var sb = new StringBuilder();

            sb.AppendLineIndented(GetApplicationInfo(), 0, windowWidth);

            // Print description
            if (!string.IsNullOrEmpty(Description))
            {
                sb.AppendLineIndented(Description, 0, windowWidth);
                sb.AppendLine();
            }

            // Print usage
            sb.AppendLine("SYNTAX");

            sb.AppendLineIndented(GetSyntax(), IndentSpacing, windowWidth);

            sb.AppendLine();

            // Print description
            if (!string.IsNullOrEmpty(HelpHeader))
            {
                sb.AppendLine(HelpHeader);
                sb.AppendLine();
            }

            // Print options
            if (Arguments.Count > 0)
            {
                sb.AppendLine("PARAMETERS");

                // Make a list of all argument categories
                List<string> categories = new List<string>();
                bool hasArgumentsWithoutCategory = false;
                foreach (Argument argument in Arguments)
                {
                    if (string.IsNullOrEmpty(argument.Category))
                        hasArgumentsWithoutCategory = true;
                    else if (!categories.Contains(argument.Category))
                        categories.Add(argument.Category);
                }

                // First print arguments that do not belong to a category
                if (hasArgumentsWithoutCategory)
                {
                    foreach (Argument argument in Arguments)
                    {
                        if (string.IsNullOrEmpty(argument.Category))
                        {
                            sb.AppendLineIndented(GetSyntaxWithoutBrackets(argument), IndentSpacing, windowWidth);
                            sb.AppendLineIndented(argument.GetHelp(), IndentSpacing * 2, windowWidth);
                            sb.AppendLine();
                        }
                    }

                    sb.AppendLine();
                }

                // Next print all other arguments sorted by category
                foreach (string category in categories)
                {
                    sb.AppendLineCentered(category.ToUpperInvariant(), windowWidth);
                    foreach (Argument argument in Arguments)
                    {
                        if (argument.Category == category)
                        {
                            sb.AppendLineIndented(GetSyntaxWithoutBrackets(argument), IndentSpacing, windowWidth);
                            sb.AppendLineIndented(argument.GetHelp(), IndentSpacing * 2, windowWidth);
                            sb.AppendLine();
                        }
                    }

                    sb.AppendLine();
                }
            }

            // Print description
            if (!string.IsNullOrEmpty(HelpFooter))
                sb.AppendLine(HelpFooter);

            return sb.ToString();
        }


        // Remove surrounding brackets.
        private static string GetSyntaxWithoutBrackets(Argument argument)
        {
            var syntax = argument.GetSyntax();

            if (syntax == null)
                return string.Empty;

            if (syntax.StartsWith("[", StringComparison.Ordinal))
                syntax = syntax.Remove(0, 1);

            if (syntax.EndsWith("]", StringComparison.Ordinal))
                syntax = syntax.Remove(syntax.Length - 1, 1);

            return syntax;
        }


        /// <summary>
        /// Throws a <see cref="MissingArgumentException"/> if a mandatory argument 
        /// (<see cref="Argument.IsOptional"/> is <see langword="false"/>) is missing.
        /// </summary>
        /// <param name="parseResult">The parse result.</param>
        /// <remarks>
        /// <see cref="Parse"/> throws a <see cref="MissingArgumentException"/> only if no arguments
        /// where parsed at all. The reason for this is that if an argument, like "help", is found
        /// you do not want to check for missing arguments. Therefore, 
        /// <see cref="ThrowIfMandatoryArgumentIsMissing"/> is a separate step which must be called
        /// manually.
        /// </remarks>
        public void ThrowIfMandatoryArgumentIsMissing(ParseResult parseResult)
        {
            if (parseResult == null)
                throw new ArgumentNullException(nameof(parseResult));

            foreach (var argument in Arguments)
                if (!argument.IsOptional && parseResult.ParsedArguments[argument] == null)
                    throw new MissingArgumentException(argument);
        }
        #endregion
    }
}
