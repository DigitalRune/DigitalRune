// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Particles
{
  /// <summary>
  /// Represents a named parameter of a particle, like "Color" or "Position".
  /// </summary>
  /// <remarks>
  /// Particles have several parameters, like "Color", "Position", "Mass", etc. All particle
  /// parameters must implement <see cref="IParticleParameter"/>. The name of a parameter (see 
  /// property <see cref="INamedObject.Name"/>) must be unique within a particle system. See 
  /// <see cref="ParticleParameterNames"/> for standard parameter names.
  /// </remarks>
  public interface IParticleParameter : INamedObject
  {
    /// <summary>
    /// Gets a value indicating whether this particle parameter is a uniform parameter or a varying parameter.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this parameter is uniform; otherwise, <see langword="false"/>
    /// if this parameters is varying.
    /// </value>
    bool IsUniform { get; }
  }


  /// <summary>
  /// Represents a named, typed parameter of a particle, like "Color" or "Position".
  /// </summary>
  /// <typeparam name="T">The type of the particle parameter.</typeparam>
  /// <remarks>
  /// <para>
  /// Particles have several parameters, like "Color", "Position", "Mass", etc. All particle
  /// parameters must implement <see cref="IParticleParameter"/>. The name of a parameter (see 
  /// property <see cref="INamedObject.Name"/>) must be unique within a particle system. See 
  /// <see cref="ParticleParameterNames"/> for standard parameter names.
  /// </para>
  /// <para>
  /// <strong>Uniform vs. Varying Particle Parameters:</strong>
  /// A particle parameter has a default value (see <see cref="DefaultValue"/>) and an optional 
  /// array of values (see <see cref="Values"/>). If <see cref="Values"/> is <see langword="null"/>,
  /// the <see cref="DefaultValue"/> applies to all particles. This kind of particle parameter is 
  /// called a "uniform" parameter. If <see cref="Values"/> is not <see langword="null"/>, each 
  /// particle has its own individual value. This kind of particle parameter is called a "varying" 
  /// parameter.
  /// </para>
  /// <para>
  /// Particle effectors or renderers should not store a direct reference to the 
  /// <see cref="Values"/> array because the particle system can replace the array. Instead, 
  /// effectors or renderers should only store references to the <see cref="IParticleParameter{T}"/>
  /// and use the property <see cref="Values"/> to retrieve the array when needed.
  /// </para>
  /// </remarks>
  public interface IParticleParameter<T> : IParticleParameter
  {
    /// <summary>
    /// Gets or sets the default value.
    /// </summary>
    /// <value>The default value.</value>
    /// <remarks>
    /// <para>
    /// If the <see cref="Values"/> array is <see langword="null"/>, then the 
    /// <see cref="DefaultValue"/> applies to all particles. This type of particle parameter is 
    /// called a "uniform" parameter.
    /// </para>
    /// <para>
    /// If the <see cref="Values"/> array is set, then each particles has its own value. This type
    /// of particle parameter is called a "varying" parameter.
    /// </para>
    /// </remarks>
    T DefaultValue { get; set; }


    /// <summary>
    /// Gets the particle parameter array that contains one value per particle.
    /// </summary>
    /// <value>The particle parameter array. Can be <see langword="null"/>.</value>
    /// <remarks>
    /// <para>
    /// If the <see cref="Values"/> array is <see langword="null"/>, then the 
    /// <see cref="DefaultValue"/> applies to all particles. This type of particle parameter is 
    /// called a "uniform" parameter.
    /// </para>
    /// <para>
    /// If the <see cref="Values"/> array is set, then each particles has its own value. This type
    /// of particle parameter is called a "varying" parameter.
    /// </para>
    /// <para>
    /// Other objects should not store a direct reference to this array because the particle system
    /// can replace the array. Instead, store a reference to the <see cref="IParticleParameter{T}"/>
    /// and use this property to retrieve the current array when needed.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Performance critical")]
    T[] Values { get; }
  }


  /// <exclude/>
  internal interface IParticleParameterInternal
  {
    /// <summary>
    /// Gets a value indicating whether this instance is a uniform particle parameter.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is a uniform particle parameter; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    bool IsUniform { get; }


    /// <summary>
    /// Gets a value indicating whether this particle parameter is initialized (the default value is 
    /// not <c>null</c> or <c>default(T)</c>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this particle parameter is initialized; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    bool IsInitialized { get; }


    /// <summary>
    /// Updates the length of the array.
    /// </summary>
    /// <param name="numberOfParticles">The new number of particles.</param>
    /// <remarks>
    /// This method resizes the existing array if the <paramref name="numberOfParticles"/> has 
    /// changed.
    /// </remarks>
    void UpdateArrayLength(int numberOfParticles);


    /// <summary>
    /// Adds a parameter to the collection that has the same type and name as this instance.
    /// </summary>
    /// <param name="collection">The collection to add a copy to.</param>
    void AddCopyToCollection(ParticleParameterCollection collection);
  }
}
