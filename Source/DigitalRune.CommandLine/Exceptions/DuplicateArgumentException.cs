// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Runtime.Serialization;
using static System.FormattableString;


namespace DigitalRune.CommandLine
{
    /// <summary>
    /// Occurs when the <see cref="CommandLineParser"/> encounters an argument twice.
    /// </summary>
    [Serializable]
    public class DuplicateArgumentException : CommandLineParserException
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateArgumentException"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateArgumentException"/> class.
        /// </summary>
        public DuplicateArgumentException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateArgumentException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public DuplicateArgumentException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateArgumentException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DuplicateArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateArgumentException"/> class.
        /// </summary>
        /// <param name="argument">The missing argument.</param>
        public DuplicateArgumentException(Argument argument)
            : base(argument, GetMessage(argument))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateArgumentException"/> class.
        /// </summary>
        /// <param name="argument">The missing argument.</param>
        /// <param name="message">The message.</param>
        public DuplicateArgumentException(Argument argument, string message)
            : base(argument, message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateArgumentException"/> class.
        /// </summary>
        /// <param name="argument">The missing argument.</param>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DuplicateArgumentException(Argument argument, string message, Exception innerException)
            : base(argument, message, innerException)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicateArgumentException"/> class.
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
        protected DuplicateArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }


        private static string GetMessage(Argument argument)
        {
            if (argument == null)
                throw new ArgumentNullException(nameof(argument));

            return Invariant($"Command line argument '{argument.Name}' must not be specified more than once. If the argument accepts several values, they must not be separated by other arguments.");
        }
    }
}
