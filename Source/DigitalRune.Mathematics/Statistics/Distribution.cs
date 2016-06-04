// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Mathematics.Statistics
{
  /// <summary>
  /// Base class of probability distributions.
  /// </summary>
  /// <typeparam name="T">The type of the random value.</typeparam>
  public abstract class Distribution<T>
  {
    /// <summary>
    /// Gets or sets the random number generator.
    /// </summary>
    /// <value>The random number generator.</value>
    /// <remarks>
    /// Per default, the global random number generator of the <see cref="RandomHelper"/> is used. A
    /// different random number generator can be set. Set this value to <see langword="null"/> to 
    /// use the default random number generator (see <see cref="RandomHelper.Random"/>).
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializerIgnore]
#endif
    [Obsolete(
      "The properties Random and NextValue have been declared obsolete because the .NET class "
      + "Random is not thread-safe, which can lead to problems in multithreaded applications. "
      + "Use the method Next(Random) instead, which is thread-safe if used properly.")]
    public Random Random
    {
      get { return _random ?? RandomHelper.Random; }
      set { _random = value; }
    }
    private Random _random; 


    /// <summary>
    /// Gets a new random value for the underlying probability distribution.
    /// </summary>
    /// <value>A random value.</value>
    [Obsolete(
      "The properties Random and NextValue have been declared obsolete because the .NET class "
      + "Random is not thread-safe, which can lead to problems in multithreaded applications. "
      + "Use the method Next(Random) instead, which is thread-safe if used properly.")]
    public T NextValue
    {
      get { return Next(Random); }
    }


    /// <summary>
    /// Gets a new random value for the underlying probability distribution.
    /// </summary>
    /// <param name="random">
    /// The random number generator. (Must not be <see langword="null"/>.)
    /// </param>
    /// <returns>A random value.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="random"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
    public abstract T Next(Random random);
  }
}
