// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if NETFX_CORE || NET45
using System.Reflection;
#endif


namespace DigitalRune
{
  /// <summary>
  /// Represents a <see cref="Delegate"/> of a specific type that stores the target object as a weak 
  /// reference.
  /// </summary>
  /// <typeparam name="T">The type of delegate.</typeparam>
  /// <remarks>
  /// <strong>Important:</strong> In Silverlight, the target of a <see cref="WeakDelegate"/> needs 
  /// to be a public method (not a private, protected or anonymous method). This is necessary 
  /// because of security restrictions in Silverlight.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
  public class WeakDelegate<T> : WeakDelegate where T : class 
  {
    /// <summary>
    /// Initializes static members of the <see cref="WeakDelegate{T}"/> class.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <typeparamref name="T"/> must be a delegate type.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
    static WeakDelegate()
    {
#if !NETFX_CORE && !NET45
      if (!typeof(T).IsSubclassOf(typeof(Delegate)))
        throw new ArgumentException("T must be a delegate type");
#else
      if (!typeof(T).GetTypeInfo().IsSubclassOf(typeof(Delegate)))
        throw new ArgumentException("T must be a delegate type");
#endif

    }


    /// <summary>
    /// Initializes a new instance of the <see cref="WeakDelegate{T}"/> class.
    /// </summary>
    /// <param name="target">
    /// The original <see cref="System.Delegate"/> to create a weak reference for.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="target"/> is <see langword="null"/>.
    /// </exception>
    public WeakDelegate(T target) : base((Delegate)(object)target)
    {
    }
  }
}
