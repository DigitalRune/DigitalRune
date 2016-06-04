// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Particles
{
  /// <summary>
  /// Describes if a particle effector reads and/or writes a particle parameter.
  /// </summary>
  public enum ParticleParameterUsage
  {
    /// <summary>
    /// The particle parameter is read but not changed.
    /// </summary>
    In,
    /// <summary>
    /// The particle parameter is written. (Overwriting existing parameter values.)
    /// </summary>
    Out,
    /// <summary>
    /// The particle parameter is read and written.
    /// </summary>
    InOut,
  }


  /// <summary>
  /// Describes how a particle parameter is used.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property)]
  public sealed class ParticleParameterAttribute : Attribute
  {
    /// <summary>
    /// Gets or sets the particle parameter usage.
    /// </summary>
    /// <value>The particle parameter usage.</value>
    public ParticleParameterUsage Usage { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether this the particle parameter is optional.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if optional; otherwise, <see langword="false"/>.
    /// </value>
    public bool Optional { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleParameterAttribute"/> class.
    /// </summary>
    /// <param name="usage">The particle parameter usage.</param>
    public ParticleParameterAttribute(ParticleParameterUsage usage)
    {
      Usage = usage;
    }
  }
}
