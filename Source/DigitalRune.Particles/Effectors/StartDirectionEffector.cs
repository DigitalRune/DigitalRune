// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;

#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Initializes a <see cref="Vector3F"/> particle parameter as a direction vector and applies the 
  /// rotation of the <see cref="ParticleSystem"/>'s <see cref="ParticleSystem.Pose"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This effector initializes the start value of a specific particle parameter (see property 
  /// <see cref="Parameter"/>) for new particles. The start value is chosen from a given 
  /// <see cref="Distribution"/>. If <see cref="Distribution"/> is <see langword="null"/>, 
  /// <see cref="DefaultValue"/> is used as the start value for all particles.
  /// </para>
  /// <para>
  /// This effector acts like a standard <see cref="StartValueEffector{T}"/>, except: The vector 
  /// created by the <see cref="Distribution"/> or the <see cref="DefaultValue"/> is treated as a 
  /// direction vector given in the local coordinate space of the particle system. That means, the 
  /// direction start value rotates with the particle system. If the 
  /// <see cref="ParticleSystem.ReferenceFrame"/> of the particle system is 
  /// <see cref="ParticleReferenceFrame.World"/>, the start value of each particle is multiplied 
  /// with the poses of the particle system (and its parent particle systems) to convert the 
  /// direction vector from local space to world space. If the 
  /// <see cref="ParticleSystem.ReferenceFrame"/> of the particle system is 
  /// <see cref="ParticleReferenceFrame.Local"/>, the pose of the particle system is ignored.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When an instance is of this class is cloned, the clone
  /// references the same <see cref="Distribution"/>. The <see cref="Distribution"/> is not cloned.
  /// </para>
  /// </remarks>
  public class StartDirectionEffector : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private IParticleParameter<Vector3F> _parameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the particle parameter that is initialized.
    /// (A varying or uniform parameter of type <see cref="Vector3F"/>.)
    /// </summary>
    /// <value>
    /// The name of the particle parameter that is initialized.
    /// (Parameter type: varying or uniform, value type: <see cref="Vector3F"/>) <br/>
    /// The default value is "Direction".
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
    /// <see cref="DefaultValue"/>.
    /// </value>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public Distribution<Vector3F> Distribution { get; set; }


    /// <summary>
    /// Gets or sets the start value that is used if <see cref="Distribution"/> is 
    /// <see langword="null"/>.
    /// </summary>
    /// <value>
    /// The default start value that is used if <see cref="Distribution"/> is <see langword="null"/>.
    /// </value>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public Vector3F DefaultValue { get; set; }


    // TODO: Add Emitter parameter if only particles of a certain emitter should be initialized?
    //public IParticleEmitter Emitter { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="StartDirectionEffector"/> class.
    /// </summary>
    public StartDirectionEffector()
    {
      Parameter = ParticleParameterNames.Direction;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new StartDirectionEffector();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      // Clone ParticleEffector properties.
      base.CloneCore(source);

      // Clone StartDirectionEffector properties.
      var sourceTyped = (StartDirectionEffector)source;
      Parameter = sourceTyped.Parameter;
      Distribution = sourceTyped.Distribution;
      DefaultValue = sourceTyped.DefaultValue;
    }


    /// <inheritdoc/>
    protected override void OnRequeryParameters()
    {
      _parameter = ParticleSystem.Parameters.Get<Vector3F>(Parameter);
    }


    /// <inheritdoc/>
    protected override void OnInitialize()
    {
      if (_parameter != null && _parameter.Values == null)
      {
        // Initialize uniform parameter.
        Vector3F startDirection = (Distribution != null) ? Distribution.Next(ParticleSystem.Random) : DefaultValue;

        if (ParticleSystem.ReferenceFrame == ParticleReferenceFrame.World)
        {
          // Apply rotation of particle system.
          var pose = ParticleSystem.GetPoseWorld();
          startDirection = pose.ToWorldDirection(startDirection);
        }

        _parameter.DefaultValue = startDirection;
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

      var array = _parameter.Values;
      if (array == null)
      {
        // Parameter is a uniform. Uniform parameters are handled in OnInitialize().
        return;
      }

      var distribution = Distribution;
      if (distribution != null)
      {
        var random = ParticleSystem.Random;
        if (ParticleSystem.ReferenceFrame == ParticleReferenceFrame.World)
        {
          var pose = ParticleSystem.GetPoseWorld();
          if (pose != Pose.Identity)
          {
            for (int i = startIndex; i < startIndex + count; i++)
              array[i] = pose.ToWorldDirection(distribution.Next(random));
          }
          else
          {
            for (int i = startIndex; i < startIndex + count; i++)
              array[i] = distribution.Next(random);
          }
        }
        else
        {
          for (int i = startIndex; i < startIndex + count; i++)
            array[i] = distribution.Next(random);
        }
      }
      else
      {
        Vector3F startDirection = DefaultValue;
        if (ParticleSystem.ReferenceFrame == ParticleReferenceFrame.World)
        {
          var pose = ParticleSystem.GetPoseWorld();
          startDirection = pose.ToWorldDirection(startDirection);
        }

        for (int i = startIndex; i < startIndex + count; i++)
          array[i] = startDirection;
      }
    }
    #endregion
  }
}
