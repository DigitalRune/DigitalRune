// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX
using System.Runtime.Serialization;
#endif


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// The exception that is thrown when a general error in the docking library occurs.
    /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX
    [Serializable]
#endif
    public class DockException : Exception
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DockException"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="DockException"/> class.
        /// </summary>
        public DockException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DockException"/> class with a specified
        /// error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DockException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DockException"/> class with a specified
        /// error message and a reference to the inner exception that is the cause of this
        /// exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or <see langword="null"/> if
        /// no inner exception is specified.
        /// </param>
        public DockException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX
        /// <summary>
        /// Initializes a new instance of the <see cref="DockException"/> class with serialized
        /// data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> that holds the serialized object data about the
        /// exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/> that contains contextual information about the source
        /// or destination.
        /// </param>
        /// <exception cref="SerializationException">
        /// The class name is <see langword="null"/> or <see cref="Exception.HResult"/> is zero (0).
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The info parameter is <see langword="null"/>.
        /// </exception>
        protected DockException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
