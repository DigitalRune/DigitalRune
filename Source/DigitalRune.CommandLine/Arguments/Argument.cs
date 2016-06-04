// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// Describes a command line argument (also known as 'command line parameter', 'program option', 
    /// or 'application switch').
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class defines an argument. That means it describes the name, the option category, the 
    /// description and whether the argument is optional or mandatory. Derived classes can define 
    /// additional properties (for example: <see cref="SwitchArgument"/>, 
    /// <see cref="ValueArgument{T}"/>, etc.).
    /// </para>
    /// <para>
    /// The <see cref="CommandLineParser"/> uses the <see cref="Argument"/> classes to parse the 
    /// command line string and to automatically produce the help text for all arguments.
    /// </para>
    /// </remarks>
    [Serializable]
    [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
    public abstract class Argument : INamedObject
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the option category.
        /// </summary>
        /// <value>The option category. (Can be <see langword="null"/> or empty.)</value>
        public string Category { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Argument"/> is optional
        /// or mandatory.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if optional; <see langword="false"/> if mandatory. The default value
        /// is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// The <see cref="CommandLineParser"/> raises the <see cref="MissingArgumentException"/> when 
        /// an argument is marked as mandatory, but cannot be found in the command line string.
        /// </remarks>
        public bool IsOptional { get; set; }


        /// <summary>
        /// Gets the argument description that will be printed as a help text.
        /// </summary>
        /// <value>The argument description.</value>
        public string Description { get; set; }


        /// <summary>
        /// Gets the name of the argument.
        /// </summary>
        /// <value>The name of the argument.</value>
        /// <remarks>
        /// The name has several uses:
        /// <list type="number">
        /// <item>
        /// <description>
        /// If the argument is a switch (such as "--sort"), then <see cref="Name"/> defines the 
        /// switch text (e.g. <c>Name = "sort"</c>). 
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// If the argument is not a switch (such as the arguments in "del file1.txt file2.txt") 
        /// then <see cref="Name"/> is just a designator that will be printed in the usage/help 
        /// text. For example: If <c>Name = "file"</c> then the usage text would be 
        /// "USAGE: DEL &lt;file&gt; {&lt;file&gt;}".
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        public string Name { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Argument"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the argument. Must not be <see langword="null"/> or an empty string.
        /// </param>
        /// <param name="description">
        /// The argument description that will be printed as help text.
        /// Can be <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> or <paramref name="description"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="name"/> is an empty string.
        /// </exception>
        protected Argument(string name, string description)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name must not be an empty string.", nameof(name));

            Name = name;
            Description = description;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Parses the specified command line argument.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <param name="index">The index of the argument to parse.</param>
        /// <returns>
        /// A new <see cref="ArgumentResult"/> or <see langword="null"/> if the argument does not
        /// match.
        /// </returns>
        /// <remarks>
        /// This method tries to parse the next argument 
        /// <paramref name="args"/>[<paramref name="index"/>]. On success <paramref name="index"/> is
        /// incremented and <see cref="Parse"/> returns an <see cref="ArgumentResult"/>.
        /// On failure, when the next argument does not match, <see langword="null"/> is returned.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
        public abstract ArgumentResult Parse(IReadOnlyList<string> args, ref int index);


        /// <summary>
        /// Gets the help information for this command line argument.
        /// </summary>
        /// <returns>
        /// The help text.
        /// </returns>
        /// <remarks>
        /// The default implementation in <see cref="Argument"/> returns only the 
        /// <see cref="Description"/>. Derived classes can add additional information.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual string GetHelp()
        {
            return Description;
        }


        /// <summary>
        /// Gets the short syntax description.
        /// </summary>
        /// <value>The short syntax description.</value>
        /// <remarks>
        /// <para>
        /// The syntax string is printed in <see cref="CommandLineParser.GetSyntax"/> and 
        /// <see cref="CommandLineParser.GetHelp"/>. For example, if the argument is an optional
        /// argument "include" with 1 or more values "path", the syntax would be 
        /// "[--include &lt;path&gt; {&lt;path&gt;}]".
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public abstract string GetSyntax();
        #endregion
    }
}
