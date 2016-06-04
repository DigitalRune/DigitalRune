// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Runtime.Serialization;
using static System.FormattableString;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// The exception that is thrown when a value of a <see cref="ValueArgument{T}"/> 
    /// cannot be parsed.
    /// </summary>
    [Serializable]
    public class InvalidArgumentValueException : CommandLineParserException
    {
        /// <summary>
        /// Gets the value which could not be parsed.
        /// </summary>
        /// <value>The value which could not be parsed.</value>
        public string Value { get; }


        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidArgumentValueException"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidArgumentValueException"/> class.
        /// </summary>
        public InvalidArgumentValueException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidArgumentValueException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public InvalidArgumentValueException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidArgumentValueException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidArgumentValueException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidArgumentValueException" /> class.
        /// </summary>
        /// <param name="argument">The missing argument.</param>
        /// <param name="value">The value which  could not be parsed.</param>
        public InvalidArgumentValueException(Argument argument, string value)
            : base(argument, GetMessage(argument, value))
        {
            Value = value;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidArgumentValueException"/> class.
        /// </summary>
        /// <param name="argument">The missing argument.</param>
        /// <param name="message">The message.</param>
        /// <param name="value">The value which  could not be parsed.</param>
        public InvalidArgumentValueException(Argument argument, string value, string message)
            : base(argument, message)
        {
            Value = value;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidArgumentValueException"/> class.
        /// </summary>
        /// <param name="argument">The missing argument.</param>
        /// <param name="value">The value which  could not be parsed.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidArgumentValueException(Argument argument, string value, Exception innerException)
            : base(argument, GetMessage(argument, value), innerException)
        {
            Value = value;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidArgumentValueException"/> class.
        /// </summary>
        /// <param name="argument">The missing argument.</param>
        /// <param name="value">The value which  could not be parsed.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidArgumentValueException(Argument argument, string value, string message, Exception innerException)
            : base(argument, message, innerException)
        {
            Value = value;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidArgumentValueException"/> class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> that holds the serialized object data about the
        /// exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/> that contains contextual information about the source
        /// or destination.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="info"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="SerializationException">
        /// The class name is <see langword="null"/> or <see cref="Exception.HResult"/> is zero (0).
        /// </exception>
        protected InvalidArgumentValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Value = info.GetString("Value");
        }


        /// <summary>
        /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with
        /// information about the exception.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> that holds the serialized object data about the
        /// exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/> that contains contextual information about the source
        /// or destination.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
            info.AddValue(nameof(Value), Value, typeof(string));
        }


        private static string GetMessage(Argument argument, string value)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            return Invariant($"The value '{value}' of the command line argument '{argument.Name}' cannot be parsed.");
        }
    }
}
