// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX
using System.Runtime.Serialization;
#endif


namespace DigitalRune.Graphics
{
  /// <summary>
  /// The exception that is raised when an error occurs in DigitalRune Graphics.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  public class GraphicsException : Exception
  {
    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsException"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsException"/> class.
    /// </summary>
    public GraphicsException()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsException"/> class with a specified
    /// error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public GraphicsException(string message)
      : base(message)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsException"/> class with a specified
    /// error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <see langword="null"/> if no
    /// inner exception is specified.
    /// </param>
    public GraphicsException(string message, Exception innerException)
      : base(message, innerException)
    {
    }


#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
    /// <summary>
    /// Initializes a new instance of the <see cref="GraphicsException"/> class with serialized
    /// data.
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
    /// The class name is <see langword="null"/> or <see cref="Exception.HResult"/> is zero (0).
    /// </exception>
    protected GraphicsException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
#endif
  }
}
