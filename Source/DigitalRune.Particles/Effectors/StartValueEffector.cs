// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Statistics;

#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Initializes a particle parameter.
  /// </summary>
  /// <typeparam name="T">The type of the particle parameter.</typeparam>
  /// <remarks>
  /// <para>
  /// This effector initializes the start value of a specific particle parameter (see property 
  /// <see cref="Parameter"/>) for new particles. The start value is chosen from a given 
  /// <see cref="Distribution"/>. If <see cref="Distribution"/> is <see langword="null"/>, 
  /// <see cref="DefaultValue"/> is used as the start value for all particles.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When an instance is of this class is cloned, the clone references 
  /// the same <see cref="Distribution"/>. The <see cref="Distribution"/> is not cloned.
  /// </para>
  /// </remarks>
  public class StartValueEffector<T> : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<T> _parameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the particle parameter that is initialized.
    /// (A varying or uniform parameter of type <typeparamref name="T"/>.)
    /// </summary>
    /// <value>
    /// The name of the particle parameter that is initialized.
    /// (Parameter type: varying or uniform, value type: <typeparamref name="T"/>) <br/>
    /// The default value is <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// This property needs to be set before the particle system is running. The particle effector 
    /// might ignore any changes that occur while the particle system is running.
    /// </remarks>
    [ParticleParameter(ParticleParameterUsage.Out)]
    public string Parameter { get; set; }


    /// <summary>
    /// Gets or sets the random value distribution that is used to choose a start value for the 
    /// parameter of a new particle. 
    /// </summary>
    /// <value>
    /// The random value distribution that determines the start value for the parameter of new 
    /// particles. The default is <see langword="null"/>, which means that the start value is set to 
    /// the <see cref="DefaultValue"/>.
    /// </value>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public Distribution<T> Distribution { get; set; }


    /// <summary>
    /// Gets or sets the start value that is set if <see cref="Distribution"/> is 
    /// <see langword="null"/>.
    /// </summary>
    /// <value>
    /// The start value that is set if <see cref="Distribution"/> is <see langword="null"/>.
    /// </value>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public T DefaultValue { get; set; }


    // TODO: Add Emitter parameter if only particles of a certain emitter should be initialized?
    //public IParticleEmitter Emitter { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new StartValueEffector<T>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone StartValueEffector<T> properties.
      var sourceTyped = (StartValueEffector<T>)source;
      Parameter = sourceTyped.Parameter;
      Distribution = sourceTyped.Distribution;
      DefaultValue = sourceTyped.DefaultValue;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _parameter = ParticleSystem.Parameters.Get<T>(Parameter);
    }


    /// <inheritdoc/>
    protected override void OnInitialize()
    {
      if (_parameter != null && _parameter.Values == null)
      {
        // Initialize uniform parameter.
        var distribution = Distribution;
        if (distribution != null)
          _parameter.DefaultValue = distribution.Next(ParticleSystem.Random);
        else
          _parameter.DefaultValue = DefaultValue;
      }
    }


    /// <inheritdoc/>
    protected override void OnUninitialize()
    {
      _parameter = null;
    }


    /// <inheritdoc/>
    protected override void OnInitializeParticles(int startIndex, int count, object emitter)
    {
      if (_parameter == null)
        return;

      T[] values = _parameter.Values;
      if (values == null)
      {
        // Parameter is a uniform. Uniform parameters are handled in OnInitialize().
        return;
      }

      // TODO: Only initialize particles of a certain emitter?
      //if (Emitter == null || Emitter == emitter)

      var distribution = Distribution;
      if (distribution != null)
      {
        var random = ParticleSystem.Random;
        for (int i = startIndex; i < startIndex + count; i++)
          values[i] = distribution.Next(random);
      }
      else
      {
        var startValue = DefaultValue;
        for (int i = startIndex; i < startIndex + count; i++)
          values[i] = startValue;
      }
    }
    #endregion
  }
}
