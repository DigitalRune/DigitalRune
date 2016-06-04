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
    /// Describes a command line argument that has a value which lies in an interval.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value. Must implement <see cref="IComparable{T}"/>
    /// </typeparam>
    public class BoundedValueArgument<T> : ValueArgument<T> where T : struct, IComparable<T>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the minimal value.
        /// </summary>
        /// <value>The minimal value.</value>
        public T Min { get; set; }


        /// <summary>
        /// Gets or sets the maximal value.
        /// </summary>
        /// <value>The maximal value.</value>
        public T Max { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundedValueArgument{T}"/> class.
        /// </summary>
        /// <param name="name">
        /// The name of the argument. Must not be <see langword="null"/> or an empty string.
        /// </param>
        /// <param name="description">
        /// The argument description that will be printed as help text.
        /// Can be <see langword="null"/>.
        /// </param>
        /// <param name="min">The minimal value.</param>
        /// <param name="max">The maximal value.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> or <paramref name="description"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="name"/> is an empty string.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="min"/> greater than <paramref name="max"/>.
        /// </exception>
        public BoundedValueArgument(string name, string description, T min, T max)
          : base(name, description)
        {
            if (min.CompareTo(max) > 0)
                throw new ArgumentException("min must be less than or equal to max.");

            Min = min;
            Max = max;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public override ArgumentResult Parse(IReadOnlyList<string> args, ref int index)
        {
            if (Min.CompareTo(Max) > 0)
                throw new ArgumentException("min must be less than or equal to max.");

            return base.Parse(args, ref index);
        }


        /// <inheritdoc/>
        protected override T Parse(string value)
        {
            var result = base.Parse(value);

            // Check parsed values against boundaries.
            if (result.CompareTo(Min) < 0 || result.CompareTo(Max) > 0)
                throw new InvalidArgumentValueException(
                    this,
                    value,
                    Invariant($"Value '{Name}' is outside of allowed bounds (Min = {Min}, Max = {Max}, actual value = {value})."));

            return result;
        }


        /// <inheritdoc/>
        public override string GetHelp()
        {
            StringBuilder sb = new StringBuilder();

            var baseHelp = base.GetHelp();
            if (!string.IsNullOrEmpty(baseHelp))
                sb.AppendLine(baseHelp);

            // Print description for allowed values
            sb.Append("Min allowed value: ");
            sb.AppendLine(Min.ToString());
            sb.Append("Max allowed value: ");
            sb.AppendLine(Max.ToString());

            return sb.ToString();
        }
        #endregion
    }
}
