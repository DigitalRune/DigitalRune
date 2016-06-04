// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// Stores the result of a <see cref="CommandLineParser"/>.<see cref="CommandLineParser.Parse"/>
    /// call.
    /// </summary>
    public sealed class ParseResult
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the original command line arguments which were parsed.
        /// </summary>
        /// <value>The original command line arguments which were parsed.</value>
        /// <remarks>
        /// <para>
        /// These are the original arguments that were the input for the 
        /// <see cref="CommandLineParser.Parse"/> call.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public IReadOnlyList<string> RawArguments { get; }


        /// <summary>
        /// Gets the unknown arguments.
        /// </summary>
        /// <value>The unknown arguments.</value>
        /// <remarks>
        /// <para>
        /// Unknown arguments are arguments that are detected on the command line, but do
        /// not match any of the arguments definitions.
        /// </para>
        /// <para>
        /// By default, the parser throws an <see cref="UnknownArgumentException"/> if it 
        /// encounters any unknown arguments. But when 
        /// <see cref="CommandLineParser.AllowUnknownArguments"/> is set to <see langword="true"/>, 
        /// all encountered unknown arguments are stored and can be accessed using this property.
        /// </para>
        /// </remarks>
        public IReadOnlyList<string> UnknownArguments { get; }


        /// <summary>
        /// Gets the arguments that were detected by the parser.
        /// </summary>
        /// <value>The arguments detected by the parser.</value>
        /// <remarks>
        /// This property contains the result of <see cref="CommandLineParser.Parse"/>. This 
        /// collection contains one item for each <see cref="Argument"/> that was found in the 
        /// parsed command line arguments. For argument types, like <see cref="SwitchArgument"/>, 
        /// the collection item is an <see cref="ArgumentResult"/>, which only indicates the 
        /// presence of the argument in the command line arguments. For 
        /// <see cref="ValueArgument{T}"/>s an <see cref="ArgumentResult{T}"/> is stored in 
        /// this collection to provide additional information about the parsed argument values.
        /// </remarks>
        public ArgumentResultCollection ParsedArguments { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseResult" /> class.
        /// </summary>
        /// <param name="args">The original input arguments.</param>
        /// <param name="parsedArguments">
        /// The <see cref="ParsedArguments"/>. Can be <see langword="null"/>.
        /// </param>
        /// <param name="unknownArguments">
        /// The <see cref="UnknownArguments"/>. Can be <see langword="null"/>.
        /// </param>
        public ParseResult(string[] args, IEnumerable<ArgumentResult> parsedArguments, IEnumerable<string> unknownArguments)
        {
            RawArguments = args?.ToArray() ?? Array.Empty<string>();

            ParsedArguments = new ArgumentResultCollection(parsedArguments?.ToList() ?? new List<ArgumentResult>());

            UnknownArguments = unknownArguments?.ToArray() ?? Array.Empty<string>();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------
        #endregion
    }
}
