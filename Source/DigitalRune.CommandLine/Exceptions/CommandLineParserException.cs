// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Runtime.Serialization;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// The exception thrown by the <see cref="CommandLineParser"/>.
    /// </summary>
    [Serializable]
    public class CommandLineParserException : Exception
    {
        /// <summary>
        /// Gets the name of the command line argument which caused the exception.
        /// </summary>
        /// <value>
        /// The name of the command line argument which caused the exception.
        /// Can be <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// This property is only set if the exception was caused by a specific argument.
        /// </remarks>
        public string Argument { get; }


        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParserException"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParserException"/> class.
        /// </summary>
        public CommandLineParserException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CommandLineParserException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParserException"/> class.
        /// </summary>
        /// <param name="argument">
        /// The argument which caused the exception. Can be <see langword="null"/>.
        /// </param>
        /// <param name="message">The message.</param>
        public CommandLineParserException(string argument, string message)
            : base(message)
        {
            Argument = argument;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParserException"/> class.
        /// </summary>
        /// <param name="argument">
        /// The argument which caused the exception. Can be <see langword="null"/>.
        /// </param>
        /// <param name="message">The message.</param>
        public CommandLineParserException(Argument argument, string message)
            : base(message)
        {
            Argument = argument?.Name;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParserException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommandLineParserException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParserException" /> class.
        /// </summary>
        /// <param name="argument">
        /// The argument which caused the exception. Can be <see langword="null"/>.
        /// </param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommandLineParserException(string argument, string message, Exception innerException)
            : base(message, innerException)
        {
            Argument = argument;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParserException" /> class.
        /// </summary>
        /// <param name="argument">
        /// The argument which caused the exception. Can be <see langword="null"/>.
        /// </param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public CommandLineParserException(Argument argument, string message, Exception innerException)
            : base(message, innerException)
        {
            Argument = argument?.Name;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineParserException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> that holds the serialized object data about the 
        /// exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/> that contains contextual information about the source or 
        /// destination. 
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="info"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="SerializationException">
        /// The class name is <see langword="null"/> or <see cref="Exception.HResult"/> is zero (0).
        /// </exception>
        protected CommandLineParserException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Argument = info.GetString(nameof(Argument));
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
        /// The <see cref="StreamingContext"/> that contains contextual information about the source or
        /// destination.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="info"/> parameter is <see langword="null"/>.
        /// </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
            info.AddValue(nameof(Argument), Argument, typeof(string));
        }
    }
}
