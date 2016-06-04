// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Collections;


namespace DigitalRune.Linq
{
  /// <summary>
  /// Provides new extension methods for LINQ.
  /// </summary>
  public static class LinqHelper
  {
    // Important Notes:
    // Methods that use "return yield" are encapsulated and separated from the null-argument
    // checks! Reason: The whole method is evaluated lazy in Enumerator.MoveNext(). But the 
    // ArgumentNullExceptions should be thrown right away. Therefore 2 separate methods!

    /// <overloads>
    /// <summary>
    /// Performs the given action on each element in a sequence when it is enumerated.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Performs the given action on each element in a sequence when it is enumerated.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="source">
    /// A sequence that contains the elements on which to perform <paramref name="action"/>.
    /// </param>
    /// <param name="action">The action to execute on each element.</param>
    /// <returns>The sequence of elements.</returns>
    /// <remarks>
    /// <see cref="Do{T}(IEnumerable{T},Action{T})"/> is similar to 
    /// <see cref="ForEach{T}(IEnumerable{T},Action{T})"/>. The difference is that 
    /// <see cref="ForEach{T}(IEnumerable{T},Action{T})"/> immediately enumerates the given 
    /// sequence. <see cref="Do{T}(IEnumerable{T},Action{T})"/> does not automatically trigger the 
    /// enumeration. Instead it returns the <see cref="IEnumerable{T}"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> or <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (action == null)
        throw new ArgumentNullException("action");

      return DoImpl(source, action);
    }


    private static IEnumerable<T> DoImpl<T>(IEnumerable<T> source, Action<T> action)
    {
      foreach (T element in source)
      {
        action(element);
        yield return element;
      }
    }


    /// <summary>
    /// Returns an empty <see cref="IEnumerable{T}"/>  that has the specified type argument.
    /// </summary>
    /// <typeparam name="T">
    /// The type to assign to the type parameter of the returned generic 
    /// <see cref="IEnumerable{T}"/>.
    /// </typeparam>
    /// <returns>
    /// An empty <see cref="IEnumerable{T}"/> whose type argument is <typeparamref name="T"/>. 
    /// </returns>
    /// <remarks>
    /// Unlike <see cref="Enumerable.Empty{TResult}"/>, this method returns a instance which does
    /// not create heap allocations ("garbage") when it is enumerated.
    /// </remarks>
    public static IEnumerable<T> Empty<T>()
    {
      return EmptyEnumerable<T>.Instance;
    }



    /// <summary>
    /// Performs the given action on each element (incorporating its index) in a sequence when it is 
    /// enumerated.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="source">
    /// A sequence that contains the elements on which to perform <paramref name="action"/>.
    /// </param>
    /// <param name="action">
    /// The action to execute on each element; the second parameter of the function represents the 
    /// index of the element.
    /// </param>
    /// <returns>The sequence of elements.</returns>
    /// <remarks>
    /// <see cref="Do{T}(IEnumerable{T},Action{T,int})"/> is similar to 
    /// <see cref="ForEach{T}(IEnumerable{T},Action{T,int})"/>. The difference is that 
    /// <see cref="ForEach{T}(IEnumerable{T},Action{T,int})"/> immediately enumerates the given 
    /// sequence. <see cref="Do{T}(IEnumerable{T},Action{T,int})"/> does not automatically trigger 
    /// the enumeration. Instead it returns the <see cref="IEnumerable{T}"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> or <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T, int> action)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (action == null)
        throw new ArgumentNullException("action");

      return DoImpl(source, action);
    }


    private static IEnumerable<T> DoImpl<T>(IEnumerable<T> source, Action<T, int> action)
    {
      int index = 0;
      foreach (T element in source)
      {
        action(element, index);
        yield return element;
        index++;
      }
    }


    ///// <summary>
    ///// Returns the first element of a <see cref="Dictionary{TKey,TValue}"/>.
    ///// </summary>
    ///// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    ///// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    ///// <param name="dictionary">The dictionary.</param>
    ///// <returns>The first element in the hash set.</returns>
    ///// <exception cref="ArgumentNullException">
    ///// <paramref name="dictionary"/> is <see langword="null"/>.
    ///// </exception>
    ///// <exception cref="InvalidOperationException">
    ///// The source sequence is empty.
    ///// </exception>
    //internal static TValue First<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    //{
    //  // This method can be used instead of Enumerable.First<T>() to avoid unnecessary
    //  // memory allocations.
    //  if (dictionary == null)
    //    throw new ArgumentNullException("dictionary");

    //  var enumerator = dictionary.Values.GetEnumerator();
    //  if (!enumerator.MoveNext())
    //    throw new InvalidOperationException("The source sequence is empty.");

    //  return enumerator.Current;
    //}


    /// <summary>
    /// Returns the first element of a <see cref="HashSet{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    /// <param name="set">The hash set.</param>
    /// <returns>The first element in the hash set.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="set"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The source sequence is empty.
    /// </exception>
    internal static T First<T>(this HashSet<T> set)
    {
      // This method can be used instead of Enumerable.First<T>() to avoid unnecessary
      // memory allocations.
      if (set == null)
        throw new ArgumentNullException("set");

      var enumerator = set.GetEnumerator();
      if (!enumerator.MoveNext())
        throw new InvalidOperationException("The source sequence is empty.");

      return enumerator.Current;
    }


    /// <overloads>
    /// <summary>
    /// Immediately performs the given action on each element in a sequence.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Immediately performs the given action on each element in a sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="source">
    /// A sequence that contains the elements on which to perform <paramref name="action"/>.
    /// </param>
    /// <param name="action">The action to execute on each element.</param>
    /// <inheritdoc cref="Do{T}(IEnumerable{T},Action{T})"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (action == null)
        throw new ArgumentNullException("action");

      foreach (T element in source)
        action(element);
    }


    /// <summary>
    /// Immediately performs the given action on each element (incorporating its index) in a 
    /// sequence.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="source">
    /// A sequence that contains the elements on which to perform <paramref name="action"/>.
    /// </param>
    /// <param name="action">
    /// The action to execute on each element; the second parameter of the function represents the 
    /// index of the element.
    /// </param>
    /// <inheritdoc cref="Do{T}(IEnumerable{T},Action{T,int})"/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    public static void ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (action == null)
        throw new ArgumentNullException("action");

      int index = 0;
      foreach (T element in source)
      {
        action(element, index);
        index++;
      }
    }


    /// <summary>
    /// Returns the index of the first element in a sequence that satisfies the specified condition.
    /// </summary>
    /// <typeparam name="T">The type of elements.</typeparam>
    /// <param name="source">A sequence of elements.</param>
    /// <param name="predicate">A predicate to test each element.</param>
    /// <returns>
    /// The zero-based index of the first element in the sequence that passed test; -1 if no element
    /// passed the test.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source"/> or <paramref name="predicate"/> is <see langword="null"/>.
    /// </exception>
    public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (predicate == null)
        throw new ArgumentNullException("predicate");

      int index = 0;
      foreach (var element in source)
      {
        if (predicate(element))
          return index;

        index++;
      }

      return -1;
    }


    /// <summary>
    /// Returns an <see cref="IEnumerable{T}"/> that returns a single element.
    /// </summary>
    /// <typeparam name="T">The type of the element.</typeparam>
    /// <param name="value">The first and only element in the sequence.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> that returns a single element.
    /// </returns>
    public static IEnumerable<T> Return<T>(T value)
    {
      yield return value;
    }
  }
}
