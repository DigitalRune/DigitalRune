// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static System.FormattableString;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// Stores the parser result for an <see cref="Argument"/> which was found in the command line
    /// arguments.
    /// </summary>
    public class ArgumentResult
    {
        /// <summary>
        /// Gets the argument.
        /// </summary>
        /// <value>The argument.</value>
        public Argument Argument { get; }


        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        /// <remarks>
        /// This property is never <see langword="null"/> but the list can be empty.
        /// </remarks>
        public IEnumerable Values { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentResult"/> class.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        public ArgumentResult(Argument argument)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            Argument = argument;
            Values = Array.Empty<object>();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentResult"/> class.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="values">The values. Can be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        protected ArgumentResult(Argument argument, IEnumerable values)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            Argument = argument;
            Values = values ?? Array.Empty<object>();
        }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Invariant($"--{Argument.Name}");
        }
    }


    /// <summary>
    /// Stores the parsed values of a <see cref="ValueArgument{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    public class ArgumentResult<T> : ArgumentResult
    {
        /// <summary>
        /// Gets the values.
        /// </summary>
        /// <value>The values.</value>
        /// <remarks>
        /// This property is never <see langword="null"/> but the list can be empty.
        /// </remarks>
        public new IReadOnlyList<T> Values
        {
            get { return (IReadOnlyList<T>)base.Values; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentResult{T}" /> class.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <param name="values">The values. Can be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="argument"/> is <see langword="null"/>.
        /// </exception>
        public ArgumentResult(Argument argument, IReadOnlyList<T> values)
            : base(argument, EnsureValues(values))
        {
        }


        private static IEnumerable EnsureValues(IReadOnlyList<T> values)
        {
            return (values ?? Array.Empty<T>());
        }


        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("--");
            stringBuilder.Append(Argument.Name);
            foreach (var value in Values)
            {
                stringBuilder.Append(" ");
                stringBuilder.Append(value != null ? Invariant($"{value}") : "null");
            }
            return stringBuilder.ToString();
        }
    }
}
