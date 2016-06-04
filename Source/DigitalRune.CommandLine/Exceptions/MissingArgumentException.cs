// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Runtime.Serialization;
using static System.FormattableString;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// The exception that is thrown when a mandatory command line argument is missing.
    /// </summary>
    [Serializable]
    public class MissingArgumentException : CommandLineParserException
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingArgumentException"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="MissingArgumentException"/> class.
        /// </summary>
        public MissingArgumentException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MissingArgumentException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MissingArgumentException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MissingArgumentException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public MissingArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MissingArgumentException"/> class.
        /// </summary>
        /// <param name="argument">The missing argument.</param>
        public MissingArgumentException(Argument argument)
            : base(argument, GetMessage(argument))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MissingArgumentException"/> class.
        /// </summary>
        /// <param name="argument">The missing argument.</param>
        /// <param name="message">The message.</param>
        public MissingArgumentException(Argument argument, string message)
            : base(argument, message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MissingArgumentException"/> class.
        /// </summary>
        /// <param name="argument">The missing argument.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public MissingArgumentException(Argument argument, string message, Exception innerException)
            : base(argument, message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="MissingArgumentException"/> class.
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
        protected MissingArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        private static string GetMessage(Argument argument)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            return Invariant($"Mandatory command line argument '{argument.Name}' is missing.");
        }
    }
}
