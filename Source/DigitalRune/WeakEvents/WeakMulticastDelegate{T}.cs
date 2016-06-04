// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune
{
  /// <summary>
  /// Represents a <see cref="MulticastDelegate"/> that stores the target objects as weak 
  /// references.
  /// </summary>
  /// <typeparam name="T">The type of delegate.</typeparam>
  /// <remarks>
  /// <strong>Important:</strong> In Silverlight, the targets of a 
  /// <see cref="WeakMulticastDelegate"/> need to be public methods (no private, protected or
  /// anonymous methods). This is necessary because of security restrictions in Silverlight.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
  public class WeakMulticastDelegate<T> : WeakMulticastDelegate where T : class
  {
    /// <overloads>
    /// <summary>
    /// Adds a new <see cref="Delegate"/> to the <see cref="WeakMulticastDelegate{T}"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Adds a new <see cref="Delegate"/> of a given type to the 
    /// <see cref="WeakMulticastDelegate{T}"/>.
    /// </summary>
    /// <param name="delegate">The new <see cref="Delegate"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="delegate"/> is <see langword="null"/>.
    /// </exception>
    public void Add(T @delegate)
    {
      Add((Delegate)(object)@delegate);
    }


    /// <overloads>
    /// <summary>
    /// Removes a <see cref="Delegate"/> from the <see cref="WeakMulticastDelegate{T}"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Removes a <see cref="Delegate"/> of a given type from the 
    /// <see cref="WeakMulticastDelegate{T}"/>.
    /// </summary>
    /// <param name="delegate">The <see cref="Delegate"/> to remove.</param>
    public void Remove(T @delegate)
    {
      Remove((Delegate)(object)@delegate);
    }
  }
}
