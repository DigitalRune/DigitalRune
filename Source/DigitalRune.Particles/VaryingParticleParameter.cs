// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;

#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Particles
{
  /// <summary>
  /// Represents a varying particle parameter. (Each particles has its own parameter value.)
  /// </summary>
  /// <typeparam name="T">The type of the particle parameter.</typeparam>
  /// <remarks>
  /// Each particle has an individual parameter value stored in <see cref="Values"/>.
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  internal sealed class VaryingParticleParameter<T> : IParticleParameter<T>, IParticleParameterInternal
  {
    /// <summary>
    /// Gets the name of the particle parameter.
    /// </summary>
    /// <value>The name of the particle parameter.</value>
#if XNA || MONOGAME
    [ContentSerializer]
#endif
    public string Name { get; private set; }


    /// <inheritdoc/>
    T IParticleParameter<T>.DefaultValue { get; set; }


    /// <inheritdoc/>
    public T[] Values
    {
      get { return _values; }
      private set { _values = value; }
    }
    private T[] _values;


    /// <inheritdoc/>
    public bool IsUniform { get { return false; } }


    /// <inheritdoc/>
    bool IParticleParameterInternal.IsInitialized { get { return false; } }


    // For serialization.
    internal VaryingParticleParameter()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="VaryingParticleParameter{T}"/> class.
    /// </summary>
    /// <param name="name">The name of this particle parameter.</param>
    /// <param name="numberOfParticles">The maximal number of particles.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfParticles"/> is negative.
    /// </exception>
    public VaryingParticleParameter(string name, int numberOfParticles)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (numberOfParticles < 0)
        throw new ArgumentOutOfRangeException("numberOfParticles", "The number of particles must not be negative.");

      Name = name;
      Values = new T[numberOfParticles];
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfParticles"/> is 0 or negative.
    /// </exception>
    public void UpdateArrayLength(int numberOfParticles)
    {
      if (numberOfParticles < 0)
        throw new ArgumentOutOfRangeException("numberOfParticles", "The number of particles must not be negative.");

      if (numberOfParticles == Values.Length)
        return;

      Array.Resize(ref _values, numberOfParticles);
    }


    /// <inheritdoc/>
    public void AddCopyToCollection(ParticleParameterCollection collection)
    {
      // Add a parameter and copy the default value.
      collection.AddVarying<T>(Name).DefaultValue = ((IParticleParameter<T>)this).DefaultValue;
    }
  }
}
