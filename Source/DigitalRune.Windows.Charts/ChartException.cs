// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if !SILVERLIGHT
using System.Runtime.Serialization;
#endif


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// The exception that is raised for charts-specific errors.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class ChartException : Exception
    {
        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartException"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartException"/> class.
        /// </summary>
        public ChartException()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ChartException"/> class with a specified
        /// error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ChartException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ChartException"/> class with a specified
        /// error message and a reference to the inner exception that is the cause of this
        /// exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or <see langword="null"/> if
        /// no inner exception is specified.
        /// </param>
        public ChartException(string message, Exception innerException)
            : base(message, innerException)
        {
        }


#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartException"/> class with serialized
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
        protected ChartException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
