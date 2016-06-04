// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Animation;

#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Particles
{  
  /// <summary>
  /// Represents a uniform particle parameter. (All particles have the same parameter value.)
  /// </summary>
  /// <typeparam name="T">The type of the particle parameter.</typeparam>
  /// <remarks>
  /// All particles use <see cref="DefaultValue"/> as the parameter value. This particle parameter 
  /// implements <see cref="IAnimatableProperty{T}"/>, so it can be easily animated.
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  internal sealed class UniformParticleParameter<T> : IParticleParameter<T>, IAnimatableProperty<T>, IParticleParameterInternal
  {
    private T _baseValue;
    private T _animationValue;
    private bool _isAnimated;


    /// <summary>
    /// Gets the name of the particle parameter.
    /// </summary>
    /// <value>The name of the particle parameter.</value>
#if XNA || MONOGAME
    [ContentSerializer]
#endif
    public string Name { get; private set; }


    /// <inheritdoc/>
    public T DefaultValue
    {
      get { return _isAnimated ? _animationValue : _baseValue; }
      set { _baseValue = value; }
    }

    
    /// <summary>
    /// Returns <see langword="null"/>.
    /// </summary>
    /// <value>Always <see langword="null"/>.</value>
    T[] IParticleParameter<T>.Values
    {
      get { return null; }
    }


    /// <inheritdoc/>
    public bool IsUniform { get { return true; } }


    /// <inheritdoc/>
    bool IParticleParameterInternal.IsInitialized { get { return !EqualityComparer<T>.Default.Equals(DefaultValue, default(T)); } }


    // For serialization.
    internal UniformParticleParameter()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="UniformParticleParameter{T}"/> class.
    /// </summary>
    /// <param name="name">The name of this particle parameter.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    public UniformParticleParameter(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");

      Name = name;
    }


    /// <inheritdoc/>
    void IParticleParameterInternal.UpdateArrayLength(int numberOfParticles)
    {
    }


    /// <inheritdoc/>
    public void AddCopyToCollection(ParticleParameterCollection collection)
    {
      // Add a parameter and copy the default value.
      collection.AddUniform<T>(Name).DefaultValue = DefaultValue;
    }


    //--------------------------------------------------------------
    #region IAnimatableProperty
    //--------------------------------------------------------------

    /// <inheritdoc/>
    bool IAnimatableProperty.HasBaseValue
    {
      get { return true; }
    }


    /// <inheritdoc/>
    object IAnimatableProperty.BaseValue
    {
      get { return _baseValue; }
    }


    /// <inheritdoc/>
    bool IAnimatableProperty.IsAnimated
    {
      get { return _isAnimated; }
      set { _isAnimated = value; }
    }


    /// <inheritdoc/>
    object IAnimatableProperty.AnimationValue
    {
      get { return _animationValue; }
    }


    /// <inheritdoc/>
    T IAnimatableProperty<T>.BaseValue
    {
      get { return _baseValue; }
    }


    /// <inheritdoc/>
    T IAnimatableProperty<T>.AnimationValue
    {
      get { return _animationValue; }
      set { _animationValue = value; }
    }
    #endregion
  }
}
