// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;


namespace DigitalRune
{
  /// <summary>
  /// Base class for types to restrict instantiation to one object ("singleton pattern").
  /// </summary>
  /// <typeparam name="T">The type of the object that is instantiated.</typeparam>
  /// <remarks>
  /// <para>
  /// <strong>Important:</strong> In Silverlight the singleton type <typeparamref name="T"/> needs 
  /// to be a public type (not a private or internal). This is necessary because of security 
  /// restrictions in Silverlight.
  /// </para>
  /// <para>
  /// <strong>Thread-Safety:</strong> This class is thread-safe. The property 
  /// <see cref="Instance"/> can be accessed by multiple threads simultaneously.
  /// </para>
  /// </remarks>
  /// <example>
  /// A class can derive from <see cref="Singleton{T}"/> like the class <strong>Log</strong> in this
  /// example:
  /// <code>
  /// <![CDATA[
  /// public class Log : Singleton<Log>
  /// {
  ///   ...
  /// }
  /// ]]>
  /// </code>
  /// The singleton can be instantiated/accessed by using the property <see cref="Instance"/>.
  /// <code>
  /// <![CDATA[
  /// var log = Log.Instance;
  /// var log = Singleton<Log>.Instance;
  /// ]]>
  /// </code>
  /// </example>
  public abstract class Singleton<T> where T : Singleton<T>, new()
  {
    // Credit: This code is partially based on the singleton class written by Nick Gravelyn.
    // (See http://nickgravelyn.com/2009/05/my-singleton-base-class/)
    
    // Note: In .NET 4.0 we could use System.Lazy<T> to which is automatically thread-safe.
    // But System.Lazy<T> is currently not supported on Windows Phone and Xbox 360.
    // (See http://geekswithblogs.net/BlackRabbitCoder/archive/2010/05/19/c-system.lazylttgt-and-the-singleton-design-pattern.aspx)


    // ReSharper disable StaticFieldInGenericType

    private static readonly object SyncRoot = new object();


    /// <summary>
    /// Gets the singleton of type <typeparamref name="T"/>.
    /// </summary>
    /// <value>The instance of type <typeparamref name="T"/>.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
    public static T Instance
    {
      get
      {
        // Optional check to see if _instance is null to avoid unnecessary locking.
        if (_instance == null)
        {
          lock (SyncRoot)
          {
            // If _instance is null, instantiate the singleton.
            if (_instance == null)
            {
              _instance = new T();
            }
          }
        }

        return _instance;
      }
    }
    private static volatile T _instance;
    // ReSharper restore StaticFieldInGenericType


    /// <summary>
    /// Initializes a new instance of the <see cref="Singleton{T}"/> class.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Constructor is called several times, but <see cref="Singleton{T}"/> can only be instantiated 
    /// once.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The class which derives from <see cref="Singleton{T}"/> is not of type 
    /// <typeparamref name="T"/>.
    /// </exception>
    protected Singleton()
    {
      // Make sure we only have one instance.
      if (_instance != null)
        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Singleton of type {0} already instantiated.", typeof(T)));

      _instance = this as T;

      // Validate that the cast worked.
      if (_instance == null)
        throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Singleton of type {0} failed to be instantiated.", GetType()));
    }
  }
}
