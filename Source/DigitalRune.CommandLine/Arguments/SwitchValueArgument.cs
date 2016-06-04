// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static System.FormattableString;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// Describes a command line switch with a value (such as "--sort=ascending").
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    [Serializable]
    public class SwitchValueArgument<T> : SwitchArgument
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the value of the command line switch.
        /// </summary>
        /// <value>The value of the command line switch.</value>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public ValueArgument<T> ValueArgument
        {
            get { return _valueArgument; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _valueArgument = value;
            }
        }
        private ValueArgument<T> _valueArgument;
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchValueArgument{T}"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchValueArgument{T}"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the argument. Must not be <see langword="null"/> or an empty string.
        /// </param>
        /// <param name="value">The value argument.</param>
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
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        public SwitchValueArgument(string name, ValueArgument<T> value, string description)
            : base(name, description)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _valueArgument = value;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchValueArgument{T}"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the argument. Must not be <see langword="null"/> or an empty string.
        /// </param>
        /// <param name="value">The value argument.</param>
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
        /// The switch name, alias or short alias contains invalid characters. A switch may only 
        /// consist of letters, digits, or '_'. 
        /// </exception>
        public SwitchValueArgument(string name, ValueArgument<T> value, string description,
            IEnumerable<string> aliases, IEnumerable<char> shortAliases)
            : base(name, description, aliases, shortAliases)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _valueArgument = value;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public override ArgumentResult Parse(IReadOnlyList<string> args, ref int index)
        {
            if (args == null || args.Count <= 0 || index >= args.Count)
            {
                // No more arguments left.
                return null;
            }

            // args[index] may contain a switch and value (e.g. "--switch=value" or "--switch:value").
            // Therefore, we need split the string.
            string switchArgument;
            string valueArgument;

            var argument = args[index];
            if (string.IsNullOrEmpty(argument))
                return null;

            int indexOfDelimiter = argument.IndexOfAny(new[] { ':', '=' });
            if (indexOfDelimiter < 0)
            {
                // No delimiter such as ':' or '=' found.
                // Check if args[i] matches the switch
                switchArgument = argument;
                valueArgument = null;
            }
            else
            {
                // args[i] contains a delimiter (':' or '=').
                switchArgument = argument.Substring(0, indexOfDelimiter);
                valueArgument = argument.Substring(indexOfDelimiter + 1);
            }

            // Parse switch
            if (!Parse(switchArgument))
                return null;

            index++;

            ArgumentResult<T> firstResult = null;
            if (!string.IsNullOrEmpty(valueArgument))
            {
                // Parse value contained in args[index]
                int i = 0;
                firstResult = (ArgumentResult<T>)_valueArgument.Parse(new[] { valueArgument }, ref i);

                if (firstResult == null)
                    throw new InvalidArgumentValueException(this, valueArgument);
            }
            else
            {
                // Skip delimiters
                while (index < args.Count && (args[index] == "=" || args[index] == ":"))
                    index++;
            }

            // Parse remaining values.
            ArgumentResult<T> otherResults = null;
            if (_valueArgument.AllowMultiple || firstResult == null)
                otherResults = (ArgumentResult<T>)_valueArgument.Parse(args, ref index);

            if (firstResult == null && otherResults == null)
            {
                if (ValueArgument.IsOptional)
                    return new ArgumentResult<T>(this, null);

                throw new MissingArgumentException(
                    this,
                    Invariant($"Mandatory value of argument '{Name}' is missing"));
            }
            if (firstResult == null)
                return new ArgumentResult<T>(this, otherResults.Values);
            if (otherResults == null)
                return new ArgumentResult<T>(this, firstResult.Values);

            return new ArgumentResult<T>(this, firstResult.Values.Concat(otherResults.Values).ToArray());
        }


        /// <inheritdoc/>
        public override string GetHelp()
        {
            var sb = new StringBuilder();

            var baseHelp = base.GetHelp();
            if (!string.IsNullOrEmpty(baseHelp))
                sb.AppendLine(baseHelp);

            if (ValueArgument != null)
                sb.Append(ValueArgument.GetHelp());

            return sb.ToString();
        }


        /// <inheritdoc/>
        public override string GetSyntax()
        {
            StringBuilder s = new StringBuilder();

            if (IsOptional)
                s.Append('[');

            s.Append("--");
            s.Append(Name);
            s.Append(' ');
            s.Append(_valueArgument.GetSyntax());

            if (IsOptional)
                s.Append(']');

            return s.ToString();
        }
        #endregion
    }
}
