// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Runtime.Serialization;
using static System.FormattableString;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// Occurs when the <see cref="CommandLineParser"/> encounters an unknown argument.
    /// </summary>
    [Serializable]
    public class UnknownArgumentException : CommandLineParserException
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownArgumentException"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownArgumentException"/> class.
        /// </summary>
        public UnknownArgumentException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownArgumentException"/> class.
        /// </summary>
        /// <param name="argument">The unknown argument.</param>
        public UnknownArgumentException(string argument)
            : base(argument, GetMessage(argument))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownArgumentException"/> class with a 
        /// specified error message.
        /// </summary>
        /// <param name="argument">The unknown argument.</param>
        /// <param name="message">The message that describes the error.</param>
        public UnknownArgumentException(string argument, string message)
            : base(argument, message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownArgumentException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UnknownArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownArgumentException"/> class.
        /// </summary>
        /// <param name="argument">The unknown argument.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UnknownArgumentException(string argument, string message, Exception innerException)
            : base(argument, message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UnknownArgumentException"/> class.
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
        /// <exception cref="SerializationException">
        /// The class name is <see langword="null"/> or <see cref="P:System.Exception.HResult"/> is zero 
        /// (0).
        /// </exception>
        protected UnknownArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        private static string GetMessage(string argument)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            return Invariant($"Invalid command line argument '{argument}'.");
        }
    }
}
