// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if XBOX || WP7
using DigitalRune.Collections;


namespace System.Collections.Generic
{
  /// <summary>
  /// Represents a set of values. 
  /// </summary>
  /// <typeparam name="T">The type of the elements in this set.</typeparam>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public class HashSet<T> : HashSetEx<T>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="HashSet{T}" /> class.
    /// </summary>
    public HashSet()
    {      
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="HashSet{T}" /> class.
    /// </summary>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> used for calculating hash codes and for comparing 
    /// values of type <typeparamref name="T"/>.
    /// </param>
    public HashSet(IEqualityComparer<T> comparer) : base(comparer)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="HashSet{T}" /> class.
    /// </summary>
    /// <param name="collection">The initial content of the set.</param>
    public HashSet(IEnumerable<T> collection)
      : base(collection)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="HashSet{T}" /> class.
    /// </summary>
    /// <param name="collection">The initial content of the set.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> used for calculating hash codes and for comparing 
    /// values of type <typeparamref name="T"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
      : base(collection, comparer)
    {
    }
  }
}
#endif
