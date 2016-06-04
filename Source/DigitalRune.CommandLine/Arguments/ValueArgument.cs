// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using static System.FormattableString;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// Describes a command line argument that has a value.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value. <typeparamref name="T"/> must provide a proper 
    /// <see cref="TypeConverter"/> that allows conversion from <see cref="string"/> or implement 
    /// <see cref="IConvertible"/>.
    /// </typeparam>
    [Serializable]
    public class ValueArgument<T> : Argument
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether multiple values are allowed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if multiple values are allowed; otherwise,
        /// <see langword="false"/>. The default is <see langword="false"/>.
        /// </value>
        public bool AllowMultiple { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueArgument{T}"/> class.
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
        /// <exception cref="ArgumentException">
        /// The value type is an enumeration. Use <see cref="EnumArgument{T}"/> instead.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The value type is not supported. The type must provide a <see cref="TypeConverter"/> that
        /// allows conversions from <see cref="string"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TypeConverter"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ValueArguments")]
        public ValueArgument(string name, string description)
          : base(name, description)
        {
            var type = typeof(T);

            if (type == typeof(Enum))
                throw new ArgumentException("ValueArgument does not support enumeration as value type. Use EnumArgument instead.");

            if (!ObjectHelper.CanParse(type))
                throw new NotSupportedException(Invariant($"ValueArguments of type {type.Name} are not supported. The type must provide a TypeConverter that allows conversions from String."));
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

            List<T> values = null;
            while (index < args.Count && Parse(args[index], ref values))
            {
                index++;

                if (!AllowMultiple)
                    break;
            }

            if (values == null || values.Count == 0)
                return null;

            return new ArgumentResult<T>(this, values);
        }


        private bool Parse(string argument, ref List<T> values)
        {
            if (string.IsNullOrEmpty(argument))
                return false;

            // Ignore argument when it starts like a switch ("--switch")
            if (argument.StartsWith("--", StringComparison.Ordinal))
                return false;

            // Ignore argument when it starts like a short-form switch ("-S')
            // Attention: Values starting with '-' can also be numbers such as "-123".
            if (argument.Length > 1 && argument[0] == '-' 
                && (SwitchArgument.IsValidShortName(argument[1]) && !char.IsDigit(argument[1])))
                return false;

            // Parse the string.
            T value = Parse(argument);

            // Lazy allocation.
            if (values == null)
                values = new List<T>();

            values.Add(value);
            return true;
        }


        /// <summary>
        /// Parses the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The value as type <typeparamref name="T"/>.</returns>
        /// <exception cref="DigitalRune.CommandLine.InvalidArgumentValueException">
        /// The value cannot be parsed. See inner exception for more details.
        /// </exception>
        protected virtual T Parse(string value)
        {
            try
            {
                // Try to parse the string.
                return ObjectHelper.Parse<T>(value);
            }
            catch (NotSupportedException exception)
            {
                // Conversion is not supported.
                throw new InvalidArgumentValueException(this, value, exception);
            }
            catch (FormatException exception)
            {
                // Conversion is not supported.
                throw new InvalidArgumentValueException(this, value, exception);
            }
        }


        /// <inheritdoc/>
        public override string GetSyntax()
        {
            StringBuilder argument = new StringBuilder();

            if (IsOptional && AllowMultiple)
                argument.Append('{');
            else if (IsOptional)
                argument.Append('[');

            argument.Append('<');
            argument.Append(Name);
            argument.Append('>');

            if (IsOptional && AllowMultiple)
            {
                argument.Append('}');
            }
            else if (IsOptional)
            {
                argument.Append(']');
            }
            else if (AllowMultiple)
            {
                string s = argument.ToString();
                argument.Append(' ');
                argument.Append('{');
                argument.Append(s);
                argument.Append('}');
            }

            return argument.ToString();
        }
        #endregion
    }
}
