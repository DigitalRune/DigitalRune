// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;

#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX
using System.Runtime.Serialization;
#endif


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// The exception is thrown by <see cref="Palette"/> when a operation fails.
    /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX
    [Serializable]
#endif
    public class PaletteException : Exception
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteException"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteException"/> class.
        /// </summary>
        public PaletteException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteException"/> class with a specified
        /// error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PaletteException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteException"/> class with a specified
        /// error message and a reference to the inner exception that is the cause of this
        /// exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or <see langword="null"/> if
        /// no inner exception is specified.
        /// </param>
        public PaletteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX
        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteException"/> class with serialized
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
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="info"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="SerializationException">
        /// The class name is <see langword="null"/> or <see cref="System.Exception.HResult"/> is
        /// zero (0).
        /// </exception>
        protected PaletteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
