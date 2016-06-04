#region ----- Copyright -----
/*
  The class in this file is based on the TaskException from the ParallelTasks library (see 
  http://paralleltasks.codeplex.com/) which is licensed under Ms-PL (see below).


  Microsoft Public License (Ms-PL)

  This license governs use of the accompanying software. If you use the software, you accept this 
  license. If you do not accept the license, do not use the software.

  1. Definitions

  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same 
  meaning here as under U.S. copyright law.

  A "contribution" is the original software, or any additions or changes to the software.

  A "contributor" is any person that distributes its contribution under this license.

  "Licensed patents" are a contributor's patent claims that read directly on its contribution.

  2. Grant of Rights

  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  copyright license to reproduce its contribution, prepare derivative works of its contribution, and 
  distribute its contribution or any derivative works that you create.

  (B) Patent Grant- Subject to the terms of this license, including the license conditions and 
  limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free 
  license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or 
  otherwise dispose of its contribution in the software or derivative works of the contribution in 
  the software.

  3. Conditions and Limitations

  (A) No Trademark License- This license does not grant you rights to use any contributors' name, 
  logo, or trademarks.

  (B) If you bring a patent claim against any contributor over patents that you claim are infringed 
  by the software, your patent license from such contributor to the software ends automatically.

  (C) If you distribute any portion of the software, you must retain all copyright, patent, 
  trademark, and attribution notices that are present in the software.

  (D) If you distribute any portion of the software in source code form, you may do so only under 
  this license by including a complete copy of this license with your distribution. If you 
  distribute any portion of the software in compiled or object code form, you may only do so under a 
  license that complies with this license.

  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no 
  express warranties, guarantees or conditions. You may have additional consumer rights under your 
  local laws which this license cannot change. To the extent permitted under your local laws, the 
  contributors exclude the implied warranties of merchantability, fitness for a particular purpose 
  and non-infringement.  
*/
#endregion

using System;
#if NETFX_CORE || PORTABLE || USE_TPL
using System.Linq;
#endif
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY
using System.Runtime.Serialization;
#endif


namespace DigitalRune.Threading
{
  /// <summary>
  /// Occurs when an unhandled exception is thrown within a <see cref="Task"/>.
  /// </summary>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class TaskException : Exception
  {
    /// <summary>
    /// Gets an array containing any unhandled exceptions that were thrown by the task.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Breaking change. Fix in next version.")]
    public Exception[] InnerExceptions { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="TaskException"/> class.
    /// </summary>
    public TaskException()
      : this(null, null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TaskException"/> class with a specified error
    /// message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public TaskException(string message)
      : this(message, null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TaskException"/> class with a specified error
    /// message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public TaskException(string message, Exception innerException)
      : base(message, innerException)
    {
      InnerExceptions = new Exception[0];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TaskException"/> class.
    /// </summary>
    /// <param name="innerExceptions">The unhandled exceptions thrown by the task.</param>
    public TaskException(Exception[] innerExceptions)
      : base("One or more exceptions were thrown while executing a task.")
    {
      InnerExceptions = innerExceptions;
    }


#if NETFX_CORE || PORTABLE || USE_TPL
    internal TaskException(AggregateException aggregateException)
      : base(aggregateException.Message)
    {
      InnerExceptions = aggregateException.InnerExceptions.ToArray();
    }
#endif


#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskException"/> class.
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
    protected TaskException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
      InnerExceptions = (Exception[])(info.GetValue("InnerExceptions", typeof(Exception[])));
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
        throw new ArgumentNullException("info");

      base.GetObjectData(info, context);
      info.AddValue("InnerExceptions", InnerExceptions, typeof(Exception[]));
    }
#endif
  }
}
