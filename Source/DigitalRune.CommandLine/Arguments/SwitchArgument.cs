// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Text;
using static System.FormattableString;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// Describes a command line switch (such as "--sort").
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="SwitchArgument"/> does not have a value. To define a switch that can have a value
    /// (such <c>"--sort=ascending"</c>) use <see cref="SwitchValueArgument{T}"/>.
    /// </para>
    /// <para>
    /// <strong>Case-Sensitivity:</strong> Switch names and <see cref="Aliases"/> are 
    /// case-insensitive. Only <see cref="ShortAliases"/> are case-sensitive.
    /// </para>
    /// <para>
    /// A switch can be specified using different a normal name or a short alias: "--switch", or "-s"
    /// </para>
    /// </remarks>
    [Serializable]
    public class SwitchArgument : Argument
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        /// <summary>A list of characters that are not allowed in the name of a switch</summary>
        private static readonly char[] BadCharacters = { '/', '-', ':', ',', ';', '=', ' ', '|' };
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the name aliases.
        /// </summary>
        /// <value>The aliases.</value>
        public IReadOnlyList<string> Aliases
        {
            get { return _aliases; }
        }
        private readonly List<string> _aliases = new List<string>();


        /// <summary>
        /// Gets the short-name aliases.
        /// </summary>
        /// <value>The short aliases.</value>
        public IReadOnlyList<char> ShortAliases
        {
            get { return _shortAliases; }
        }
        private readonly List<char> _shortAliases = new List<char>();
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchArgument"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the argument. Must not be <see langword="null"/> or an empty string.
        /// </param>
        /// <param name="description">
        /// The argument description that will be printed as help text.
        /// Must not be <see langword="null"/> but can be an empty string.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> or <paramref name="description"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="name"/> is an empty string.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The switch name has less than two characters. A single character is only allowed for 
        /// short aliases.
        /// </exception>
        public SwitchArgument(string name, string description)
          : base(name, description)
        {
            ValidateName(name);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchArgument"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the argument. Must not be <see langword="null"/> or an empty string.
        /// </param>
        /// <param name="description">
        /// The argument description that will be printed as help text.
        /// Can be <see langword="null"/>.
        /// </param>
        /// <param name="aliases">The aliases.</param>
        /// <param name="shortAliases">The short aliases.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> or <paramref name="description"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="name"/> is an empty string.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// A switch name or alias has less than two characters. A single character is only allowed 
        /// for the short aliases.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The switch name, alias contains invalid characters. A switch may only 
        /// consist of letters, digits, or '_'. 
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The short alias contains invalid characters. Valid characters are only letters and 
        /// selected characters (e.g. '_', '?'). Digits are not valid.
        /// </exception>
        public SwitchArgument(string name, string description, IEnumerable<string> aliases, IEnumerable<char> shortAliases)
          : base(name, description)
        {
            ValidateName(name);

            if (aliases != null)
            {
                foreach (var alias in aliases)
                {
                    ValidateName(alias);
                    _aliases.Add(alias);
                }
            }

            if (shortAliases != null)
            {
                foreach (var shortAlias in shortAliases)
                {
                    ValidateName(shortAlias);
                    _shortAliases.Add(shortAlias);
                }
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Validates the name of the switch-argument.
        /// </summary>
        /// <param name="name">The name.</param>
        private static void ValidateName(string name)
        {
            if (name.Length == 1)
                throw new ArgumentException(
                    Invariant($"Invalid switch name '{name}'. A switch must have at least two characters. A single character is only allowed for a short alias."),
                    nameof(name));

            if (name.Length < 2)
                throw new ArgumentException(
                    Invariant($"Invalid switch name '{name}'. A switch must be at least two characters."),
                    nameof(name));

            foreach (char c in name)
            {
                if (Array.IndexOf(BadCharacters, c) >= 0)
                    throw new ArgumentException(
                        Invariant($"Invalid switch name '{name}'. A switch may only consist of letters, digits, or '_'."),
                        nameof(name));
            }
        }


        /// <summary>
        /// Validates the short-name of a switch-argument.
        /// </summary>
        /// <param name="shortName">The short-name.</param>
        private static void ValidateName(char shortName)
        {
            if (!IsValidShortName(shortName))
                throw new ArgumentException(
                    Invariant($"Invalid short alias '{shortName}'. Use a different character instead"),
                    nameof(shortName));
        }


        /// <summary>
        /// Determines whether the specified short-name is valid..
        /// </summary>
        /// <param name="shortName">The short-name.</param>
        /// <returns>
        /// <see langword="true"/> if the short-name is valid; otherwise, <see langword="false"/>
        /// if the character cannot be used for a short-name.
        /// </returns>
        public static bool IsValidShortName(char shortName)
        {
            return (Array.IndexOf(BadCharacters, shortName) < 0) && !char.IsDigit(shortName);
        }


        ///// <overloads>
        ///// <summary>
        ///// Adds an alias.
        ///// </summary>
        ///// </overloads>
        ///// 
        ///// <summary>
        ///// Adds a short-name alias.
        ///// </summary>
        ///// <param name="shortName">The short-name alias (case-sensitive).</param>
        //public void AddAlias(char shortName)
        //{
        //    ValidateShortName(shortName);

        //    if (_shortAliases.Contains(shortName))
        //        return;

        //    _shortAliases.Add(shortName);
        //}


        ///// <summary>
        ///// Adds a name alias.
        ///// </summary>
        ///// <param name="name">The name alias.</param>
        ///// <exception cref="ArgumentException">
        ///// The switch-argument has less than two characters. A single character is only allowed for  
        ///// short-name aliases added using <see cref="AddAlias(char)"/>.
        ///// </exception>
        //public void AddAlias(string name)
        //{
        //    ValidateName(name);

        //    if (_aliases.Contains(name))
        //        return;

        //    _aliases.Add(name);
        //}


        /// <inheritdoc/>
        public override ArgumentResult Parse(IReadOnlyList<string> args, ref int index)
        {
            if (args == null || args.Count <= 0 || index >= args.Count)
            {
                // No more arguments left.
                return null;
            }

            bool switchFound = Parse(args[index]);
            if (!switchFound)
                return null;

            index++;
            return new ArgumentResult(this);
        }


        /// <summary>
        /// Parses the specified argument.
        /// </summary>
        /// <param name="argument">The command line argument to parse.</param>
        /// <returns>
        /// <see langword="true"/> if argument was parsed successfully; otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        protected bool Parse(string argument)
        {
            bool switchFound = false;

            // A switch must have at least 2 characters
            if (string.IsNullOrEmpty(argument) || argument.Length < 2)
                return false;

            // A switch can have different forms: "--switch", or "-s"
            if (argument[0] == '-' && argument[1] == '-')
            {
                // Argument has the form "--switch"
                if (argument.Length > 3)
                    switchFound = CompareWithNames(argument.Substring(2));
            }
            else if (argument[0] == '-')
            {
                // Argument has the form "-s"
                if (argument.Length == 2)
                    switchFound = CompareWithShortNames(argument[1]);
            }

            return switchFound;
        }


        private bool CompareWithNames(string name)
        {
            name = name.ToUpperInvariant();
            if (name == Name.ToUpperInvariant())
                return true;

            if (Aliases != null)
                foreach (string alias in Aliases)
                    if (name == alias.ToUpperInvariant())
                        return true;

            return false;
        }


        private bool CompareWithShortNames(char c)
        {
            if (ShortAliases != null)
                foreach (char alias in ShortAliases)
                    if (c == alias)
                        return true;

            return false;
        }


        /// <inheritdoc/>
        public override string GetHelp()
        {
            var sb = new StringBuilder();

            var baseHelp = base.GetHelp();
            sb.Append(baseHelp);

            if (Aliases.Count > 0 || ShortAliases.Count > 0)
            {
                if (!string.IsNullOrEmpty(baseHelp))
                    sb.AppendLine();

                sb.Append("Aliases: ");
                bool addComma = false;
                foreach (var alias in Aliases)
                {
                    if (addComma)
                        sb.Append(", ");
                    sb.Append("--");
                    sb.Append(alias);
                    addComma = true;
                }
                foreach (var alias in ShortAliases)
                {
                    if (addComma)
                        sb.Append(", ");
                    sb.Append("-");
                    sb.Append(alias);
                    addComma = true;
                }
            }

            return sb.ToString();
        }


        /// <inheritdoc/>
        public override string GetSyntax()
        {
            var sb = new StringBuilder();

            if (IsOptional)
                sb.Append('[');

            sb.Append("--");
            sb.Append(Name);

            if (IsOptional)
                sb.Append(']');

            return sb.ToString();
        }
        #endregion
    }
}
