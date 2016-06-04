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
    /// Describes a command line argument that is an enumeration.
    /// </summary>
    /// <typeparam name="T">The enumeration (must be a .NET <see cref="Enum"/>).</typeparam>
    /// <remarks>
    /// <para>
    /// <typeparamref name="T"/> must be a .NET enumeration class. The values specified at the 
    /// command line are automatically converted to the matching enumeration value. 
    /// </para>
    /// <para>
    /// An <see cref="EnumArgument{T}"/> can be combination of multiple enumeration values. To 
    /// enable multiple values the enumeration must have the <see cref="FlagsAttribute"/>. 
    /// <see cref="ValueArgument{T}.AllowMultiple"/> will be automatically set to 
    /// <see langword="true"/>. However, the parsed values are not automatically ORed together. 
    /// Instead they are stored as individual values in the <see cref="ArgumentResult{T}"/>.
    /// </para>
    /// </remarks>
    /// <example>
    /// Here is an example of an enumeration:
    /// <code>
    /// [Flags]
    /// public enum MyEnum {
    ///   Flag1,
    ///   Flag2,
    ///   Flag3,
    ///   Flag4
    /// }
    /// 
    /// ...
    /// 
    /// CommandLineParser parser = new CommandLineParer();
    /// parser.AddArgument(new EnumArgument&lt;MyEnum&gt;("MyEnum", "A custom enumeration");
    /// 
    /// // The values on the command line can be specified in various ways, for example:
    /// // "--MyEnum:Flag1 Flag2 Flag3"
    /// // "--MyEnum = flag1 Flag2 Flag3"
    /// // "--MyEnum flag1 Flag2 Flag3"
    /// </code>
    /// </example>
    public class EnumArgument<T> : ValueArgument<T>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly Type _type;
        private Dictionary<string, object> _namesAndValues;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumArgument{T}"/> class.
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
        /// The argument type of the <see cref="EnumArgument{T}"/> is not an <see cref="Enum"/>. Or 
        /// the <see cref="Enum"/> does not have any values.
        /// </exception>
        public EnumArgument(string name, string description)
          : base(name, description)
        {
            _type = typeof(T);

            // Check whether T is an enumeration
            if (!_type.IsEnum)
                throw new ArgumentException("Type T of EnumArgument needs to be an enumeration.");

            if (Enum.GetValues(_type).Length < 1)
                throw new ArgumentException("Enumeration T does not have any values.");

            // Set AllowMultiple to true if enum has FlagsAttribute
            object[] attributes = _type.GetCustomAttributes(typeof(FlagsAttribute), false);
            if (attributes != null && attributes.Length > 0 && attributes[0] is FlagsAttribute)
                AllowMultiple = true;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public override ArgumentResult Parse(IReadOnlyList<string> args, ref int index)
        {
            if (_namesAndValues == null)
            {
                // Caches names and values in dictionary
                string[] enumNames = Enum.GetNames(_type);
                Array enumValues = Enum.GetValues(_type);
                _namesAndValues = new Dictionary<string, object>(enumNames.Length);
                for (int i = 0; i < enumNames.Length; i++)
                    _namesAndValues.Add(enumNames[i].ToUpperInvariant(), enumValues.GetValue(i));
            }

            return base.Parse(args, ref index);
        }


        /// <inheritdoc/>
        protected override T Parse(string value)
        {
            if (value == null)
                value = string.Empty;

            var upperValue = value.ToUpperInvariant();
            object result;
            if (_namesAndValues.TryGetValue(upperValue, out result))
                return (T)result;

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(Invariant($"Value '{value}' of argument '{Name}' is invalid. Allowed values are: "));

            bool addSeparator = false;
            foreach (var e in Enum.GetNames(_type))
            {
                if (addSeparator)
                    stringBuilder.Append(", ");

                stringBuilder.Append(e);

                addSeparator = true;
            }

            throw new InvalidArgumentValueException(this, value, stringBuilder.ToString());
        }


        /// <inheritdoc/>
        public override string GetHelp()
        {
            StringBuilder sb = new StringBuilder();

            var baseHelp = base.GetHelp();
            if (!string.IsNullOrEmpty(baseHelp))
                sb.AppendLine(baseHelp);

            // Print description for allowed values
            string[] names = Enum.GetNames(_type);
            sb.Append("Allowed values: ");
            if (names.Length > 0)
                sb.Append(names[0]);
            for (int i = 1; i < names.Length; i++)
            {
                sb.Append(',');
                sb.Append(' ');
                sb.Append(names[i]);
            }

            if (AllowMultiple)
                sb.Append("\n(Multiple values can be specified.)");

            return sb.ToString();
        }
        #endregion
    }
}
