// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DigitalRune.Animation;
using DigitalRune.Collections;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Linq;
using DigitalRune.Mathematics;
using DigitalRune.Threading;

#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Particles
{
  /// <summary>
  /// Represents a system of particles.
  /// </summary>
  /// <remarks>
  /// <para>
  /// For general information about particle systems, see 
  /// <see href="http://en.wikipedia.org/wiki/Particle_system"/>
  /// </para>
  /// <para>
  /// <strong>Particle Parameters:</strong> <br/>
  /// Normally, particle systems use a "Particle" class to store the state of a particle, and all 
  /// "Particle" instances are stored in an array. This <i>Array of Structures (AoS)</i> approach 
  /// has the disadvantages that the particle parameters are hard-coded as the properties of the 
  /// Particle class. For example, the Particle class could contain the properties "Position", 
  /// "Direction", "Speed", "Size". If a new particle effector wants to store the property "Mass" 
  /// with the particle, an new "Particle" class has to be defined. And if all particles in a 
  /// specific particle system should have the same size, then it is a waste of memory to store 
  /// "Size" per particle. 
  /// </para>
  /// <para>
  /// The DigitalRune Particle System uses a different approach. Particle data is stored in a set of
  /// separate arrays. This is a <i>Structure of Arrays (SoA)</i> approach. Further, each particle 
  /// system can decide if a specific particle parameter is constant for all particles 
  /// (<i>uniform particle parameters</i>), or if each particle can have a different parameter value
  /// (<i>varying particle parameters</i>). For example: In a smoke particle system, the particle 
  /// parameter "Color" will be uniform - all particles use the same color (e.g brown for a dirt 
  /// cloud effect). In a fireworks particle system, the particle parameter "Color" will be varying 
  /// to give each particle a unique color to create a colorful effect.
  /// </para>
  /// <para>
  /// The <see cref="ParticleSystem"/> has a property <see cref="Parameters"/> which manages a 
  /// collection of <see cref="IParticleParameter{T}"/> instances. Each particle parameter has a 
  /// default value and an optional array. If the particle parameter is uniform, only the 
  /// <see cref="IParticleParameter{T}.DefaultValue"/> is used and the 
  /// <see cref="IParticleParameter{T}.Values"/> array is <see langword="null"/>. If the particle 
  /// parameter is varying, the <see cref="IParticleParameter{T}.Values"/> is used and its length is
  /// equal to <see cref="MaxNumberOfParticles"/>. <see cref="MaxNumberOfParticles"/> defines the 
  /// number of particles that can be alive at any given time in the particle system. The particle 
  /// parameter arrays are used like a circular buffer. The <see cref="ParticleStartIndex"/> defines 
  /// the array element that contains the first active particle. The current number of active 
  /// particles is stored in <see cref="NumberOfActiveParticles"/>.
  /// </para>
  /// <para>
  /// <strong>Particle Age and Lifetime:</strong> <br/>
  /// The <see cref="ParticleSystem"/> itself defines and manages only one particle parameter:
  /// "NormalizedAge". (All other particle parameters, like "Position", "Color", "Size", "Alpha", 
  /// etc., must be manually added when needed. <see cref="ParticleEffector"/>s must be used to 
  /// initialize and update these particle parameters.)
  /// </para>
  /// <para>
  /// This "NormalizedAge" parameter is varying and stores the relative lifetime of each particle.
  /// New particles have a normalized age of 0. If the normalized age is equal to or greater than 1,
  /// the particle is dead. Per default, all particles live 3 seconds, but you can add a uniform or 
  /// varying particle parameter "Lifetime" to change the lifetime or to give each particle an 
  /// individual lifetime (measured in seconds).
  /// </para>
  /// <para>
  /// <strong>Killing Particles:</strong> <br/>
  /// Particles can be killed by setting their "NormalizedAge" to 1 or higher. Dead particles are 
  /// usually ignored by particle systems and their effectors. But the space is not immediately 
  /// freed. Particles are created and removed in a first-in-first-out order. That means, in order 
  /// to free up space for new particles the oldest particles need to be killed first.
  /// </para>
  /// <para>
  /// <strong>Particle Effectors:</strong> <br/>
  /// A particle system manages a set of <see cref="ParticleEffector"/>s (see property 
  /// <see cref="Effectors"/>). These objects define the behavior of the particle system. A particle
  /// effector can manipulate the particle system, create particles, or change the particle 
  /// parameters. See <see cref="ParticleEffector"/> for more information.
  /// </para>
  /// <para>
  /// <strong>Child Particle Systems:</strong> <br/>
  /// A particle system can own other particle systems (see property <see cref="Children"/>). This 
  /// allows to create complex layered particle system. For instance, an explosion effect that 
  /// combines smoke, fire and debris particle systems.
  /// </para>
  /// <para>
  /// <strong>Creating a Particle System:</strong> <br/>
  /// To create the particle system: Add the required particle parameters to the 
  /// <see cref="Parameters"/> collection. Then add the particle effectors that define the behavior
  /// of particle system to the <see cref="Effectors"/> collection. - It is also possible to clone 
  /// an existing particle system as described below.
  /// </para>
  /// <para>
  /// <strong>Using a Particle System:</strong> <br/>
  /// To use a configured particle system, you only have to call <see cref="Update"/> once per 
  /// frame. Particle systems can be added to an <see cref="IParticleSystemService"/>, in which case 
  /// the particle system service updates the particle system automatically. (Child particle 
  /// systems, which are owned by another particle system, are also updated automatically.)
  /// </para>
  /// <para>
  /// <strong>Rendering a Particle System:</strong> <br/>
  /// The particle system does not draw itself because rendering functionality is application and 
  /// graphics-engine specific. The particle system only manages the particle data. 
  /// </para>
  /// <para>
  /// <strong>IAnimatableObject:</strong> <br/>
  /// The <see cref="ParticleSystem"/> implements <see cref="IAnimatableObject"/>. All uniform 
  /// particle parameters of a particle system are <see cref="IAnimatableProperty"/>s. That means 
  /// they can be animated using DigitalRune Animation.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> <br/>
  /// The <see cref="ParticleSystem"/> can be cloned. When a clone is created using 
  /// <see cref="Clone"/>, the whole particle system including the particle parameters, particle 
  /// effectors and child particle systems is cloned. The returned particle system is in its initial 
  /// state, which means the runtime state of particle system and the particle states are never 
  /// copied - each clone starts at <see cref="Time"/> 0 with no particles.
  /// </para>
  /// <para>
  /// The properties <see cref="Random"/>, <see cref="RenderData"/> and <see cref="UserData"/> are 
  /// not copied.
  /// </para>
  /// <para>
  /// <strong>IGeometricObject:</strong><br/>
  /// The <see cref="ParticleSystem"/> implements <see cref="IGeometricObject"/>, which means the 
  /// particle system has a <see cref="Pose"/>, and a <see cref="Shape"/>. These properties are not 
  /// used by the particle system class itself. The <see cref="Pose"/> can be used to position the 
  /// particle system in a 3D world or relative to its parent system. The <see cref="Shape"/> can be
  /// used as the bounding shape for frustum culling. (Please note: The default shape is an 
  /// <see cref="InfiniteShape"/>. A better shape should be chosen and updated manually.)
  /// </para>
  /// </remarks>
  public partial class ParticleSystem : IGeometricObject, IAnimatableObject
  {
    // Note: Buffer indices cannot be compared with <= because it is a ring buffer. Use != instead.
    // Note: Currently all particles have the same MaxLifetime. So the particles are always sorted 
    // by their age. Order of indices in buffer: _firstLiving <= _firstFree <= _firstLiving <= ...

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _isInitialized;
    private bool _parametersAreQueried;

    private bool _effectorsDirty = true;
    private readonly List<ParticleEffector> _effectorsCopy = new List<ParticleEffector>();

    private bool _childrenDirty;
    private List<ParticleSystem> _childrenCopy;

    private readonly Action<int> _updateParticleSystem;

    private IParticleParameter<float> _lifetimeParameter;
    private IParticleParameter<float> _normalizedAgeParameter;

    // Possible future changes: 
    // Order of indices in buffer: _firstLiving <= _firstNew <= _firstFree <= _firstDead <= _firstLiving <= ...
    //private int _firstDeadParticleIndex;
    //private int _firstNewParticleIndex;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the random number generator used by the particle system.
    /// </summary>
    /// <value>The random number generator. (Must not be <see langword="null"/>.)</value>
    /// <remarks>
    /// <para>
    /// The random number generator is created on demand when the property getter is accessed for 
    /// the first time. When particle systems are updated in parallel (see 
    /// <see cref="ParticleSystemManager.EnableMultithreading"/>) each particle system needs to have
    /// its own random number generator because the class <see cref="Random"/> is not thread-safe!
    /// </para>
    /// <para>
    /// When multiple particle systems are created in close succession, they may be initialized with
    /// the same random seed. This can lead to particle systems that behave similar and do not look 
    /// random. If this is a problem, consider setting a random number generator with a carefully 
    /// chosen seed.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public Random Random
    {
      get
      {
        if (_random == null)
          _random = new Random();

        return _random;
      }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _random = value;
      }
    }
    private Random _random;


    /// <summary>
    /// Gets the particle system service.
    /// </summary>
    /// <value>
    /// The particle system service if this instance or the <see cref="Parent"/> particle system was
    /// added to a particle system service; otherwise, <see langword="null"/>.
    /// </value>
    public IParticleSystemService Service
    {
      get { return _service; }
      internal set
      {
        if (_service == value)
          return;

        _service = value;

        if (Children != null)
        {
          foreach (var child in Children)
            child.Service = _service;
        }
      }
    }
    private IParticleSystemService _service;


    /// <summary>
    /// Gets the parent particle systems.
    /// </summary>
    /// <value>
    /// The parent particle system if this instance is the child of another particle system;
    /// otherwise, <see langword="null"/>.
    /// </value>
    public ParticleSystem Parent
    {
      get { return _parent; }
      internal set
      {
        if (_parent == value)
          return;

        _parent = value;
        _parametersAreQueried = false; // (Parameters from the parent could be inherited!)
      }
    }
    private ParticleSystem _parent;


    /// <summary>
    /// Gets the name of this particle system.
    /// </summary>
    /// <value>
    /// The name of the particle system. The default value is <see langword="null"/>.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Cannot change the name because the particle system has already been added to a particle 
    /// system service or another particle system.
    /// </exception>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public string Name
    {
      get { return _name; }
      set
      {
        if (_name == value)
          return;

        if (Service != null || Parent != null)
          throw new InvalidOperationException("Cannot change name of a particle system that has already been added to a particle system service or another particle system.");

        _name = value;
      }
    }
    private string _name;


    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ParticleSystem"/> is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>. The default value is
    /// <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// If <see cref="Enabled"/> is <see langword="false"/> the particle system is paused and does 
    /// not change its state when <see cref="Update"/> is called.
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public bool Enabled { get; set; }


    /// <summary>
    /// Gets the maximum number of particles.
    /// </summary>
    /// <value>The maximum number of particles. The default value is 100.</value>
    /// <remarks>
    /// This value determines the maximum number of particles which can be alive at a single moment.
    /// This value is equal to the length of the particle parameter arrays. If the maximum allowed 
    /// number of particles is reached, no new particles can be created. Old particles have to be 
    /// removed before new particles can be added. (Particles can be killed by setting their 
    /// "NormalizedAge" to 1 or greater. When killing particles the oldest particles need to be 
    /// killed first.)
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public int MaxNumberOfParticles
    {
      get { return _maxNumberOfParticles; }
      set
      {
        if (_maxNumberOfParticles == value)
          return;

        if (_maxNumberOfParticles < 0)
          throw new ArgumentOutOfRangeException("value", "MaxNumberOfParticles must not be negative.");

        _maxNumberOfParticles = value;
        Parameters.UpdateArrayLength();
      }
    }
    private int _maxNumberOfParticles = 100;


    /// <summary>
    /// Gets index of the first active particle in the particle parameter arrays.
    /// </summary>
    /// <value>The index of the first active particle.</value>
    /// <remarks>
    /// <see cref="ParticleStartIndex"/> points to the first active particle in the particle 
    /// parameter arrays. <see cref="NumberOfActiveParticles"/> counts how many particles are 
    /// active. 
    /// </remarks>
    public int ParticleStartIndex { get; private set; }


    /// <summary>
    /// Gets the number of active particles.
    /// </summary>
    /// <value>The number of active particles.</value>
    /// <remarks>
    /// <para>
    /// <see cref="ParticleStartIndex"/> points to the first active particle in the particle 
    /// parameter arrays. <see cref="NumberOfActiveParticles"/> counts how many particles are 
    /// active. 
    /// </para>
    /// <para>
    /// Usually, <see cref="NumberOfActiveParticles"/> is equal to 
    /// <see cref="NumberOfLivingParticles"/> particles because all particles have the same maximal 
    /// "Lifetime". Only if a custom particle effector randomly kills particles (by setting the 
    /// particle parameter "NormalizedAge" to a value equal to or greater than 1) or if "Lifetime" 
    /// is a varying particle parameter, then <see cref="NumberOfLivingParticles"/> can be less than 
    /// <see cref="NumberOfActiveParticles"/>; that means that some particles are active but have 
    /// already died.
    /// </para>
    /// </remarks>
    public int NumberOfActiveParticles { get; private set; }


    /// <summary>
    /// Gets the number of living particles.
    /// </summary>
    /// <value>The number of living particles.</value>
    /// <remarks>
    /// <para>
    /// <see cref="NumberOfLivingParticles"/> counts the number of particles where the 
    /// "NormalizedAge" parameter is less than 1. This number does not include the living particles 
    /// of the child particle systems (see <see cref="Children"/>).
    /// </para>
    /// <para>
    /// Usually, <see cref="NumberOfActiveParticles"/> is equal to 
    /// <see cref="NumberOfLivingParticles"/> particles because all particles have the same maximal 
    /// "Lifetime". Only if a custom particle effector randomly kills particles (by setting the 
    /// particle parameter "NormalizedAge" to a value equal to or greater than 1) or if "Lifetime" 
    /// is a varying particle parameter, then <see cref="NumberOfLivingParticles"/> can be less than 
    /// <see cref="NumberOfActiveParticles"/>; that means that some particles are active but have 
    /// already died.
    /// </para>
    /// </remarks>
    public int NumberOfLivingParticles { get; private set; }


    /// <summary>
    /// Gets the simulation time of the particle system.
    /// </summary>
    /// <value>The simulation time of the particle system.</value>
    public TimeSpan Time { get; private set; }


    /// <summary>
    /// Gets or sets the time scaling.
    /// </summary>
    /// <value>The time scaling. The default value is 1.</value>
    /// <remarks>
    /// <see cref="TimeScaling"/> is a factor that is used to scale the <i>deltaTime</i> in 
    /// <see cref="Update"/>. If <see cref="TimeScaling"/> is larger than 1, the particle system 
    /// will advance faster. If <see cref="TimeScaling"/> is less than 1, the particle system 
    /// advances in slow-motion. If <see cref="TimeScaling"/> is 0, the particle system is paused. 
    /// The particle system behavior is undefined if the value is negative. (Some particle system 
    /// might support a negative <see cref="TimeScaling"/> to simulate backwards in time.)
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public float TimeScaling { get; set; }


    /// <summary>
    /// Gets or sets the initial delay.
    /// </summary>
    /// <value>The initial delay. The default value is 0.</value>
    /// <remarks>
    /// <para>
    /// The <see cref="InitialDelay"/> is used by the particle system when the system is updated for
    /// the first time, or after it was <see cref="Reset"/>. Changing <see cref="InitialDelay"/> of 
    /// an already running particle system does not have an immediate effect.
    /// </para>
    /// <para>
    /// If <see cref="InitialDelay"/> is greater than zero, the particle system will wait the 
    /// specified time until it starts to create and simulate particles. 
    /// </para>
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public TimeSpan InitialDelay { get; set; }


    /// <summary>
    /// Gets or sets the current delay.
    /// </summary>
    /// <value>The current delay. The default value is 0.</value>
    /// <remarks>
    /// <para>
    /// When the particle system is updated for the first time or when it is <see cref="Reset"/>,
    /// <see cref="CurrentDelay"/> is set to the <see cref="InitialDelay"/> (if it is positive). If 
    /// this value is positive, the particle system will be paused for the given duration. With each
    /// <see cref="Update"/> the <see cref="CurrentDelay"/> is reduced, and the particle system 
    /// continues to simulate particles as soon as <see cref="CurrentDelay"/> is 0 or negative. 
    /// </para>
    /// <para>
    /// Unlike <see cref="InitialDelay"/>, changing <see cref="CurrentDelay"/> of a running particle
    /// system has an immediate effect (in the next <see cref="Update"/> calls). Each time the 
    /// particle system is <see cref="Reset"/>, <see cref="CurrentDelay"/> is set to 
    /// <see cref="InitialDelay"/>
    /// </para>
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public TimeSpan CurrentDelay { get; set; }


    /// <summary>
    /// Gets or sets the preload duration.
    /// </summary>
    /// <value>The preload duration. The default value is 0 (no preloading).</value>
    /// <remarks>
    /// <para>
    /// Usually, particle systems start with no particles and particles are added over time. But for
    /// some particle systems in a game, the start should not be visible to the user. For example, 
    /// when the player encounters a waterfall, the waterfall should already be running and not 
    /// slowly start in front of the player. Such particle systems need to be preloaded.
    /// </para>
    /// <para>
    /// If <see cref="Update"/> is called for the first time or after a <see cref="Reset"/> and 
    /// <see cref="PreloadDuration"/> is greater than 0, the particle system will automatically
    /// advance its state by executing <see cref="Update"/> internally several times. For each 
    /// internal <see cref="Update"/> call the <see cref="PreloadDeltaTime"/> is used as the time
    /// step size. 
    /// </para>
    /// <para>
    /// Preloading during the first <see cref="Update"/> can take a significant amount of time. It
    /// might be better to make the first <see cref="Update"/> call while a game level is loading to
    /// avoid any slow-downs at runtime. Setting <see cref="PreloadDeltaTime"/> to a larger value 
    /// will also reduce the needed time for preloading.
    /// </para>
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public TimeSpan PreloadDuration { get; set; }


    /// <summary>
    /// Gets or sets the preload delta time.
    /// </summary>
    /// <value>
    /// The preload delta time. The default is 1/60 seconds.
    /// </value>
    /// <remarks>
    /// <see cref="PreloadDeltaTime"/> is the time step size used by the particle system while 
    /// preloading. See <see cref="PreloadDuration"/> for more information.
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public TimeSpan PreloadDeltaTime { get; set; }


    /// <summary>
    /// Gets the <i>deltaTime</i> parameter of the last <see cref="Update"/> call.
    /// </summary>
    /// <value>The <i>deltaTime</i> parameter of the last <see cref="Update"/> call.</value>
    internal TimeSpan CurrentDeltaTime { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether multithreading is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if multithreading is enabled; otherwise, <see langword="false"/>. The
    /// default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// When multithreading is enabled the particle system will update child particle systems (see 
    /// <see cref="Children"/>) on multiple threads to improve the performance. 
    /// </para>
    /// <para>
    /// Multithreading adds an additional overhead, therefore it should only be enabled if the 
    /// current system has more than one CPU core and if the other cores are not fully utilized by
    /// the application. Multithreading should be disabled if the system has only one CPU core or
    /// if all other CPU cores are busy. In some cases it might be necessary to run a benchmark of
    /// the application and compare the performance with and without multithreading to decide 
    /// whether multithreading should be enabled or not.
    /// </para>
    /// <para>
    /// The particle system internally uses the class <see cref="Parallel"/> for parallelization.
    /// <see cref="Parallel"/> is a static class that defines how many worker threads are created, 
    /// how the workload is distributed among the worker threads and more. (See 
    /// <see cref="Parallel"/> to find out more on how to configure parallelization.)
    /// </para>
    /// </remarks>
    /// <seealso cref="Parallel"/>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public bool EnableMultithreading { get; set; }


    /// <summary>
    /// Gets or sets which 3D reference frame is used for particle parameters and particle effector 
    /// properties.
    /// </summary>
    /// <value>
    /// The used reference frame. The default is <see cref="ParticleReferenceFrame.World"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// Each particle system has a <see cref="Pose"/>. This is like a transformation matrix that
    /// defines how the particle system is positioned relative to the <see cref="Parent"/> particle 
    /// system, or relative to world space if the particle system does not have a 
    /// <see cref="Parent"/>. 
    /// </para>
    /// <para>
    /// Usually, all particle positions, all rotation parameters, all direction parameters, etc. are
    /// relative to world space. A renderer which draws particles, can render them at the position 
    /// specified by the position particle parameter. If the particle system moves (by changing its 
    /// <see cref="Pose"/>), the particles do not move with the particle system. 
    /// </para>
    /// <para>
    /// In some cases, the particle should always be relative to particle system, for example, if 
    /// the particles are used to render light-points that are fixed on a vehicle. In this case the 
    /// particle positions must be relative to the <see cref="ParticleReferenceFrame.Local"/> space 
    /// of the particle system. A renderer which draws particles must transform particle positions 
    /// by the <see cref="Pose"/> of this particle system and all parent particle systems.
    /// </para>
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public ParticleReferenceFrame ReferenceFrame { get; set; }


    /// <summary>
    /// Gets the particle parameters.
    /// </summary>
    /// <value>The particle parameters.</value>
    public ParticleParameterCollection Parameters { get; private set; }


#if XNA || MONOGAME
    [ContentSerializer(ElementName = "Parameters", Optional = true, CollectionItemName = "Parameter")]
    internal IParticleParameter[] ParametersInternal    // For serialization only.
    {
      get
      {
        // Return all parameters except the NormalizedAge.
        var normalizedAgeParameter = Parameters.GetUnchecked<float>(ParticleParameterNames.NormalizedAge, true);
        var excludedParameters = LinqHelper.Return<IParticleParameter>(normalizedAgeParameter);
        return Parameters.Except(excludedParameters).ToArray();
      }
      set
      {
        if (value == null)
          return;

        Parameters.Clear();
        foreach (var parameter in value.OfType<IParticleParameterInternal>())
          parameter.AddCopyToCollection(Parameters);
      }
    }
#endif


    /// <summary>
    /// Gets the particle effectors.
    /// </summary>
    /// <value>The particle effectors.</value>
    public ParticleEffectorCollection Effectors { get; private set; }


#if XNA || MONOGAME
    [ContentSerializer(ElementName = "Effectors", Optional = true, CollectionItemName = "Effector")]
    internal ParticleEffector[] EffectorsInternal    // For serialization only.
    {
      get
      {
        return Effectors.ToArray();
      }
      set
      {
        if (value == null)
          return;

        Effectors.Clear();
        foreach (var effector in value)
          Effectors.Add(effector);
      }
    }
#endif


    /// <summary>
    /// Gets or sets the collection of child particle systems.
    /// </summary>
    /// <value>
    /// The collection of child particle systems. The default value is <see langword="null"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
#if XNA || MONOGAME
    [ContentSerializerIgnore]
#endif
    public ParticleSystemCollection Children
    {
      get { return _children; }
      set
      {
        if (_children == value)
          return;

        _childrenDirty = true;

        if (_children != null)
        {
          _children.CollectionChanged -= OnChildrenChanged;
          foreach (var child in _children)
          {
            child.Service = null;
            child.Parent = null;
          }
        }

        _children = value;

        if (_children != null)
        {
          _children.CollectionChanged += OnChildrenChanged;
          foreach (var child in _children)
          {
            child.Service = Service;
            child.Parent = this;
          }
        }
      }
    }
    private ParticleSystemCollection _children;


#if XNA || MONOGAME
    [ContentSerializer(ElementName = "Children", Optional = true, CollectionItemName = "ParticleSystem")]
    internal ParticleSystem[] ChildrenInternal    // For serialization only.
    {
      get
      {
        return (Children != null) ? Children.ToArray() : null;
      }
      set
      {
        if (value == null)
          return;

        Children = new ParticleSystemCollection();
        foreach (var child in value)
          Children.Add(child);
      }
    }
#endif


    /// <summary>
    /// Gets or sets the render data.
    /// </summary>
    /// <value>The render data. The default value is <see langword="null"/>.</value>
    /// <remarks>
    /// This property is not used by the particle system itself. A particle renderer can use this
    /// property to store data.
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public object RenderData { get; set; }


    /// <summary>
    /// Gets or sets the user data.
    /// </summary>
    /// <value>The user data. The default value is <see langword="null"/>.</value>
    /// <remarks>
    /// This property is not used by the particle system itself. This property can be used to store 
    /// application specific data.
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public object UserData { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystem"/> class.
    /// </summary>
    public ParticleSystem()
    {
      InitializeGeometricObject();

      _updateParticleSystem = UpdateParticleSystem;
      Enabled = true;

      Effectors = new ParticleEffectorCollection();
      Effectors.CollectionChanged += OnEffectorsChanged;

      Parameters = new ParticleParameterCollection(this);
      Parameters.Changed += OnParametersChanged;

      PreloadDeltaTime = new TimeSpan(166666);
      TimeScaling = 1;

      _normalizedAgeParameter = Parameters.AddVarying<float>(ParticleParameterNames.NormalizedAge);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnParametersChanged(object sender, EventArgs eventArgs)
    {
      // The particle parameter references need to be requeried, later.
      _parametersAreQueried = false;
    }


    private void OnEffectorsChanged(object sender, CollectionChangedEventArgs<ParticleEffector> eventArgs)
    {
      // The effectors copy needs to be recreated, later.
      _effectorsDirty = true;

      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      if (eventArgs.NewItems != null)
      {
        foreach (var item in eventArgs.NewItems)
        {
          // Check if effector.ParticleSystem is already set. 
          // (Note: effector.ParticleSystem cannot be "this" because the Effector 
          // collection does not allow duplicates.)
          if (item.ParticleSystem != null)
          {
            throw new ParticleSystemException("Cannot add particle effector to particle system. The effector has already been added to another particle system.")
            {
              ParticleEffector = item,
              ParticleSystem = this,
            };
          }

          item.ParticleSystem = this;

          // If effectors are added while the particle system is already in use, we 
          // initialize the effectors immediately. - This can fail if necessary 
          // particle parameters are missing or have the wrong type.
          try
          {
            if (_isInitialized)
            {
              item.RequeryParameters();
              item.Initialize();
            }
          }
          catch (ParticleSystemException exception)
          {
            // Add more information to the exception.
            exception.ParticleSystem = this;
            exception.ParticleEffector = item;
            throw;
          }
        }
      }

      if (eventArgs.OldItems != null)
      {
        foreach (var item in eventArgs.OldItems)
        {
          // Clean up.
          item.Uninitialize();
          item.ParticleSystem = null;
        }
      }
    }


    private void OnChildrenChanged(object sender, CollectionChangedEventArgs<ParticleSystem> eventArgs)
    {
      _childrenDirty = true;

      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      if (eventArgs.NewItems != null)
      {
        foreach (var item in eventArgs.NewItems)
        {
          if (item.Service != null)
          {
            throw new ParticleSystemException("Cannot add particle system. The particle system has already been added to a particle system service.")
            {
              ParticleSystem = item,
            };
          }
          if (item.Parent != null)
          {
            throw new ParticleSystemException("Cannot add particle system. The particle system has already been added to another particle system.")
            {
              ParticleSystem = item,
            };
          }

          item.Service = Service;
          item.Parent = this;
        }
      }

      if (eventArgs.OldItems != null)
      {
        foreach (var item in eventArgs.OldItems)
        {
          item.Service = null;
          item.Parent = null;
        }
      }
    }


    internal void InvalidateParameters()
    {
      _parametersAreQueried = false;
    }


    private void RequeryParameters()
    {
      // Call RequeryParameters() for all effectors if this is the first call or 
      // the parameter collection was modified.
      if (_parametersAreQueried)
        return;

      // Requery the base parameters. 
      _lifetimeParameter = Parameters.Get<float>(ParticleParameterNames.Lifetime);

      // The "NormalizedAge" parameter is required. Use AddVarying<T>() instead of Get<T>():
      // AddVarying<T>() might add a missing "NormalizedAge" or replace any uniform NormalizedAge 
      // parameters.)
      _normalizedAgeParameter = Parameters.AddVarying<float>(ParticleParameterNames.NormalizedAge);

      foreach (var effector in Effectors)
      {
        try
        {
          effector.RequeryParameters();
        }
        catch (ParticleSystemException exception)
        {
          // RequeryParameters() can fail if necessary parameters are missing or 
          // have the/ wrong type. Add additional exception information.
          exception.ParticleSystem = this;
          exception.ParticleEffector = effector;
          throw;
        }
      }

      // Children might use inherited parameters and must be informed of new parameters too.
      if (Children != null)
        foreach (var child in Children)
          child.InvalidateParameters();

      _parametersAreQueried = true;
    }


    private void Initialize()
    {
      if (!_isInitialized)
      {
        _isInitialized = true;
        CurrentDelay = InitialDelay;

        CopyEffectors();
        int numberOfEffectors = _effectorsCopy.Count;
        for (int i = 0; i < numberOfEffectors; i++)
        {
          var effector = _effectorsCopy[i];
          effector.Initialize();
        }
      }
    }


    /// <summary>
    /// Resets the particle system to its initial state. All particle states are cleared.
    /// </summary>
    /// <remarks>
    /// <see cref="Reset"/> does not change the <see cref="Enabled"/> flag.
    /// </remarks>
    public void Reset()
    {
      OnReset();
    }


    /// <summary>
    /// Called when <see cref="Reset"/> was called.
    /// </summary>
    protected virtual void OnReset()
    {
      RequeryParameters();

      _isInitialized = true;
      ParticleStartIndex = 0;
      NumberOfActiveParticles = 0;
      NumberOfLivingParticles = 0;

      CurrentDelay = InitialDelay;
      Time = TimeSpan.Zero;

      // Initialize effectors.
      CopyEffectors();
      int numberOfEffectors = _effectorsCopy.Count;
      for (int i = 0; i < numberOfEffectors; i++)
      {
        var effector = _effectorsCopy[i];
        effector.Initialize();
      }

      // Reset child particle systems.
      if (Children != null)
      {
        CopyChildren();
        foreach (var system in _childrenCopy)
          system.Reset();
      }
    }


    /// <summary>
    /// Advances the state of the particle system.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    /// <remarks>
    /// This method calls <see cref="OnUpdate"/> which increases the particle ages and calls the 
    /// <see cref="Effectors"/> and the child <see cref="Children"/>.
    /// </remarks>
    public void Update(TimeSpan deltaTime)
    {
      if (!Enabled)
        return;

      // ----- First time initialization
      RequeryParameters();
      Initialize();

      // ----- Preloading
      if (Time == TimeSpan.Zero && PreloadDuration != TimeSpan.Zero)
      {
        var absolutePreloadDuration = PreloadDuration.Duration();

        while (Time < absolutePreloadDuration)
        {
          var preloadDeltaTime = PreloadDeltaTime;
          if (Time + deltaTime > absolutePreloadDuration)
            preloadDeltaTime = absolutePreloadDuration - Time;

          OnUpdate(preloadDeltaTime);
        }
      }

      deltaTime = new TimeSpan((long)(deltaTime.Ticks * (double)TimeScaling));
      if (deltaTime == TimeSpan.Zero)
        return;

      if (CurrentDelay > TimeSpan.Zero)
      {
        if (CurrentDelay >= deltaTime)
        {
          CurrentDelay -= deltaTime;
          return;
        }

        deltaTime -= CurrentDelay;
        CurrentDelay = TimeSpan.Zero;
      }

      OnUpdate(deltaTime);

      // If an item has removed itself, then we remove all strong references.
      if (_effectorsDirty)
        _effectorsCopy.Clear();

      if (_childrenDirty && _childrenCopy != null)
        _childrenCopy.Clear();
    }


    /// <summary>
    /// Called when <see cref="Update"/> was called.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> <br/>
    /// This method is called by <see cref="Update"/> to do the actual work. This method is only
    /// called if the particle system is <see cref="Enabled"/> and not delaying. The base 
    /// implementation calls <see cref="ParticleEffector.BeginUpdate"/> of the effectors. Then the 
    /// particle ages (particle parameter "NormalizedAge") are updated. Next 
    /// <see cref="ParticleEffector.UpdateParticles"/> of the effectors is called to update the 
    /// particle states. After that the child <see cref="Children"/> are updated. Finally 
    /// <see cref="ParticleStartIndex"/>, <see cref="NumberOfActiveParticles"/> and
    /// <see cref="NumberOfLivingParticles"/> are updated and 
    /// <see cref="ParticleEffector.EndUpdate"/> of the effectors is called.
    /// </remarks>
    protected virtual void OnUpdate(TimeSpan deltaTime)
    {
      CurrentDeltaTime = deltaTime;
      CopyEffectors();

      // ----- Effector.BeginUpdate()
      int numberOfEffectors = _effectorsCopy.Count;
      for (int i = 0; i < numberOfEffectors; i++)
      {
        var effector = _effectorsCopy[i];
        effector.BeginUpdate(deltaTime);
      }

      // ----- Particle aging
      if (_lifetimeParameter == null || _lifetimeParameter.Values == null)
        AgeWithUniformLifetime(deltaTime);
      else
        AgeWithVaryingLifetime(deltaTime);

      // ----- Effector.UpdateParticles()
      for (int i = 0; i < numberOfEffectors; i++)
      {
        var effector = _effectorsCopy[i];
        effector.UpdateParticles(deltaTime, ParticleStartIndex, NumberOfActiveParticles);
      }

      // ----- ParticleSystem.Update()
      if (Children != null)
      {
        CopyChildren();

        if (EnableMultithreading && _childrenCopy.Count > 1)
        {
          Parallel.For(0, _childrenCopy.Count, _updateParticleSystem);
        }
        else
        {
          foreach (var particleSystem in _childrenCopy)
            particleSystem.Update(deltaTime);
        }
      }

      // ----- Update ParticleStartIndex, NumberOfLivingParticles and NumberOfActiveParticles
      // We do this after we have updated effectors because some effectors might want to
      // react to dead particles. They could also revive particles.
      CountLivingParticles();

      // ----- Effector.EndUpdate()
      for (int i = 0; i < numberOfEffectors; i++)
      {
        var effector = _effectorsCopy[i];
        effector.EndUpdate(deltaTime);
      }

      // Update time - but only if the service is still there. Special effectors
      // like a "particle system recycler" can remove the particle system from the
      // parent or the service in EndUpdate()!
      if (Service != null)
        Time += deltaTime;
    }


    // Age the particles when Lifetime is a uniform parameter.
    private void AgeWithUniformLifetime(TimeSpan deltaTime)
    {
      var ages = _normalizedAgeParameter.Values;
      var lifetime = (_lifetimeParameter != null) ? _lifetimeParameter.DefaultValue : 3;   // 3 seconds is the default if the user does not add a Lifetime parameter.
      float normalizedAgeChange = (float)(deltaTime.TotalSeconds) / lifetime;

      // We update the age array in one or two batches to avoid any wrap-around 
      // computations in the for loop.
      // Exclusive end indices for the two batches:
      int endIndex0 = ParticleStartIndex + NumberOfActiveParticles; // Batch #1
      int endIndex1 = 0;                                            // Batch #2
      if (endIndex0 > MaxNumberOfParticles)
      {
        endIndex0 = MaxNumberOfParticles;
        endIndex1 = NumberOfActiveParticles - (endIndex0 - ParticleStartIndex);
      }

      // ----- Update ages in up to 2 batches.
      // (Update batch #2 before batch #1 to avoid potential cache miss at end of array.)
      for (int i = 0; i < endIndex1; i++)
        ages[i] += normalizedAgeChange;

      for (int i = ParticleStartIndex; i < endIndex0; i++)
        ages[i] += normalizedAgeChange;
    }


    // Age the particles when Lifetime is a varying parameter.
    private void AgeWithVaryingLifetime(TimeSpan deltaTime)
    {
      var ageArray = _normalizedAgeParameter.Values;
      float dt = (float)deltaTime.TotalSeconds;
      float[] lifetimes = _lifetimeParameter.Values;

      // We update the age array in one or two batches to avoid any wrap-around 
      // computations in the for loop.
      // Exclusive end indices for the two batches:
      int endIndex0 = ParticleStartIndex + NumberOfActiveParticles; // Batch #1
      int endIndex1 = 0;                                            // Batch #2
      if (endIndex0 > MaxNumberOfParticles)
      {
        endIndex0 = MaxNumberOfParticles;
        endIndex1 = NumberOfActiveParticles - (endIndex0 - ParticleStartIndex);
      }

      // ----- Update ages in up to 2 batches.
      // (Update batch #2 before batch #1 to avoid potential cache miss at end of array.)
      for (int i = 0; i < endIndex1; i++)
        ageArray[i] += dt / lifetimes[i];

      for (int i = ParticleStartIndex; i < endIndex0; i++)
        ageArray[i] += dt / lifetimes[i];
    }


    private void CountLivingParticles()
    {
      var ages = _normalizedAgeParameter.Values;

      // Exclusive end indices for the two batches:
      int endIndex0 = ParticleStartIndex + NumberOfActiveParticles; // Batch #1
      int endIndex1 = 0;                                            // Batch #2
      if (endIndex0 > MaxNumberOfParticles)
      {
        endIndex0 = MaxNumberOfParticles;
        endIndex1 = NumberOfActiveParticles - (endIndex0 - ParticleStartIndex);
      }

      // ----- Count living particles and find new ParticleStartIndex.
      int numberOfLivingParticles = 0;
      bool particleStartIndexFound = false;

      // Go through Batch #1 until new ParticleStartIndex is found.
      int index = ParticleStartIndex;
      for (; index < endIndex0; index++)
      {
        if (ages[index] < 1.0f)
        {
          numberOfLivingParticles++;
          ParticleStartIndex = index;
          particleStartIndexFound = true;
          index++;
          break;
        }

        NumberOfActiveParticles--;
      }

      // Go through rest of Batch #1.
      for (; index < endIndex0; index++)
      {
        if (ages[index] < 1.0f)
          numberOfLivingParticles++;
      }

      index = 0;
      if (!particleStartIndexFound)
      {
        // Go through Batch #2 until ParticleStartIndex is found.
        for (; index < endIndex1; index++)
        {
          if (ages[index] < 1.0f)
          {
            numberOfLivingParticles++;
            ParticleStartIndex = index;
            index++;
            break;
          }

          NumberOfActiveParticles--;
        }
      }

      // Go through rest of Batch #2.
      for (; index < endIndex1; index++)
      {
        if (ages[index] < 1.0f)
          numberOfLivingParticles++;
      }

      NumberOfLivingParticles = numberOfLivingParticles;
    }


    private void UpdateParticleSystem(int index)
    {
      _childrenCopy[index].Update(CurrentDeltaTime);
    }


    /// <overloads>
    /// <summary>
    /// Creates a number of new particles.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates a number of new particles.
    /// </summary>
    /// <param name="numberOfNewParticles">The number of particles to create.</param>
    /// <returns>
    /// The number of actually created particles. This number will be less than 
    /// <paramref name="numberOfNewParticles"/> if the total number of particles in the particle
    /// system would exceed <see cref="MaxNumberOfParticles"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The "NormalizedAge" of new particles is set to 0. The particle system will automatically 
    /// call <see cref="ParticleEffector.InitializeParticles"/> of all effectors to initialize other
    /// particle parameters.
    /// </para>
    /// </remarks>
    public int AddParticles(int numberOfNewParticles)
    {
      return AddParticles(numberOfNewParticles, null);
    }


    /// <summary>
    /// Creates a number of new particles from the specified emitter.
    /// </summary>
    /// <param name="numberOfNewParticles">The number of particles to create.</param>
    /// <param name="emitter">
    /// Optional: The emitter that triggered the particle creation. This can be an effector, another
    /// object, or <see langword="null"/>. 
    /// </param>
    /// <returns>
    /// The number of actually created particles. This number will be less than 
    /// <paramref name="numberOfNewParticles"/> if the total number of particles in the particle
    /// system would exceed <see cref="MaxNumberOfParticles"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The "NormalizedAge" of new particles is set to 0. The particle system will automatically 
    /// call <see cref="ParticleEffector.InitializeParticles"/> of all effectors to initialize other
    /// particle parameters.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public int AddParticles(int numberOfNewParticles, object emitter)
    {
      if (!Enabled || numberOfNewParticles <= 0)
        return 0;

      // Find out how many free particle array slots are left.
      int numberOfAvailableParticles = MaxNumberOfParticles - NumberOfActiveParticles;
      if (numberOfAvailableParticles <= 0)
        return 0;

      // First time initialization
      RequeryParameters();
      Initialize();

      // Clamp the number of actually created particles to the valid range.
      int numberOfCreatedParticles = MathHelper.Clamp(numberOfNewParticles, 0, numberOfAvailableParticles);

      // The first free slot:
      int startIndex = ParticleStartIndex + NumberOfActiveParticles;
      if (startIndex >= MaxNumberOfParticles)
        startIndex -= MaxNumberOfParticles;

      // We update the age array in one or two batches to avoid any wrap-around 
      // computations in the for loop.
      // Exclusive end indices for the two batches:
      int endIndex0 = startIndex + numberOfCreatedParticles;  // Batch #1
      int endIndex1 = 0;                                      // Batch #2
      if (endIndex0 > MaxNumberOfParticles)
      {
        endIndex0 = MaxNumberOfParticles;
        endIndex1 = numberOfCreatedParticles - (endIndex0 - startIndex);
      }

      // Check if normalized age parameter is varying.
      var ages = _normalizedAgeParameter.Values;
      if (ages == null)
      {
        string message = string.Format(CultureInfo.InvariantCulture, "The required varying particle parameter '{0}' was not found.", ParticleParameterNames.NormalizedAge);
        throw new ParticleSystemException(message)
        {
          ParticleSystem = this,
          ParticleParameter = ParticleParameterNames.NormalizedAge,
        };
      }

      // ----- Initialize ages. 
      // (Update batch #2 before batch #1 to avoid potential cache miss at end of array.)
      for (int i = 0; i < endIndex1; i++)
        ages[i] = 0;
      for (int i = startIndex; i < endIndex0; i++)
        ages[i] = 0;

      NumberOfActiveParticles += numberOfCreatedParticles;
      NumberOfLivingParticles += numberOfCreatedParticles;

      // Let each effector initialize the particle.
      int numberOfEffectors = Effectors.Count;
      for (int i = 0; i < numberOfEffectors; i++)
      {
        var effector = Effectors[i];
        effector.InitializeParticles(startIndex, numberOfCreatedParticles, emitter);
      }

      return numberOfCreatedParticles;
    }


    private void CopyEffectors()
    {
      if (_effectorsDirty)
      {
        _effectorsDirty = false;

        _effectorsCopy.Clear();
        foreach (var item in Effectors)
          _effectorsCopy.Add(item);
      }
    }


    private void CopyChildren()
    {
      if (_childrenDirty)
      {
        _childrenDirty = false;

        if (_childrenCopy == null)
          _childrenCopy = new List<ParticleSystem>();
        else
          _childrenCopy.Clear();

        foreach (var item in Children)
          _childrenCopy.Add(item);
      }
    }


    #region ----- IAnimatabelObject -----

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IEnumerable<IAnimatableProperty> IAnimatableObject.GetAnimatedProperties()
    {
      foreach (var property in Parameters)
      {
        var animatable = property as IAnimatableProperty;
        if (animatable != null && animatable.IsAnimated)
          yield return animatable;
      }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    IAnimatableProperty<T> IAnimatableObject.GetAnimatableProperty<T>(string name)
    {
      IParticleParameter<T> parameter = Parameters.GetUnchecked<T>(name, true);
      return parameter as IAnimatableProperty<T>;
    }
    #endregion
    #endregion
  }
}
