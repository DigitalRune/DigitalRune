// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
using System.Runtime.Serialization;
#endif


namespace DigitalRune.Game.UI.Consoles
{
  /// <summary>
  /// Is raised when a console command needs to report errors.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
  [Serializable]
#endif
  public class ConsoleCommandException : Exception
  {
    /// <summary>The default error message for an invalid argument.</summary>
    public readonly static string ErrorInvalidArgument = "Invalid argument.";

    /// <summary>The default error message for an invalid number of arguments.</summary>
    public readonly static string ErrorInvalidNumberOfArguments = "Invalid number of arguments.";

    /// <summary>The default error message for a missing argument.</summary>
    public readonly static string ErrorMissingArgument = "Missing argument(s).";

    /// <summary>The default error message for an invalid command.</summary>
    public readonly static string ErrorInvalidCommand = "Invalid command.";


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCommandException"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCommandException"/> class.
    /// </summary>
    public ConsoleCommandException()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCommandException"/> class with a
    /// specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ConsoleCommandException(string message)
      : base(message)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCommandException"/> class with a
    /// specified error message and the argument that caused the error.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="argument">The argument that caused the error.</param>
    public ConsoleCommandException(string message, string argument)
      : base(string.IsNullOrEmpty(argument) ? message : "'" + argument + "' - " + message)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCommandException"/> class with a
    /// specified error message and a reference to the inner exception that is the cause of this
    /// exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception, or <see langword="null"/> if no
    /// inner exception is specified.
    /// </param>
    public ConsoleCommandException(string message, Exception innerException)
      : base(message, innerException)
    {
    }


#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !PORTABLE
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCommandException"/> class with
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
    /// The <paramref name="info"/> parameter is <see langword="null"/>.
    /// </exception>
    /// <exception cref="SerializationException">
    /// The class name is <see langword="null"/> or <see cref="Exception.HResult"/> is zero (0).
    /// </exception>
    protected ConsoleCommandException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
#endif
  }
}
