using System;


namespace DigitalRune.Particles.Effectors
{
  /// <summary>
  /// Recycles the particle system after a certain time or when the particle system does not have 
  /// any living particles anymore.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This effector monitors the owning particle system. If the <see cref="MaxRuntime"/> is 
  /// exceeded, the particle system is recycled. If <see cref="CheckIfAlive"/> is set, the particle 
  /// system is also recycled if the particle system and its children do not have any living 
  /// particles. The particle system will not be recycled before its time reaches the 
  /// <see cref="MinRuntime"/>. (For particles that do not emit particles in the very first frame,
  /// <see cref="MinRuntime"/> must be set to a value greater than one frame time! Otherwise, the 
  /// particle system could be immediately recycled.)
  /// </para>
  /// <para>
  /// <strong>Recycling a particle system:</strong> <br/>
  /// First the event <see cref="Recycle"/> is raised to allow event handlers to do custom clean-up.
  /// Then the particle system is removed from the <see cref="IParticleSystemService"/> or any 
  /// parent particle system. If the particle system implements <see cref="IRecyclable"/>, 
  /// <see cref="IRecyclable.Recycle"/> is called. Otherwise, the particle system is 
  /// <see cref="ParticleSystem.Reset"/>. If a <see cref="ResourcePool"/> is specified, the 
  /// particle system is recycled into this resource pool. (If the particle system implements 
  /// <see cref="IRecyclable"/> these steps are all skipped and it is expected that the particle 
  /// system's <see cref="IRecyclable.Recycle"/> method performs the necessary actions.)
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> <br/>
  /// When the effector is cloned, the clone refers to the same resource pool.
  /// </para>
  /// </remarks>
  public class ParticleSystemRecycler : ParticleEffector
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // A lock objects for thread synchronization.
    private static readonly object _lock = new object();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the minimal time the effector will wait before trying to recycle.
    /// </summary>
    /// <value>The minimal runtime of the particle system. The default value is 0.</value>
    public TimeSpan MinRuntime { get; set; }


    /// <summary>
    /// Gets or sets the maximal time after which the effector will automatically recycle the 
    /// particle system.
    /// </summary>
    /// <value>
    /// The maximal runtime of the particle system in seconds. The default value is 
    /// <see cref="TimeSpan.MaxValue"/>.
    /// </value>
    public TimeSpan MaxRuntime { get; set; }


    /// <summary>
    /// Gets or sets the resource pool.
    /// </summary>
    /// <value>The resource pool. The default value is <see langword="null"/>.</value>
    public ResourcePool<ParticleSystem> ResourcePool { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the particle system should be automatically recycled
    /// if <see cref="MinRuntime"/> has passed and the number of living particles is 0.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if particle should be automatically recycled if 
    /// <see cref="MinRuntime"/> has passed and there are no living particles anymore; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    public bool CheckIfAlive { get; set; }


    /// <summary>
    /// Event raised before the particle system is recycled.
    /// </summary>
    public event EventHandler<EventArgs> Recycle;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystemRecycler"/> class.
    /// </summary>
    public ParticleSystemRecycler()
    {
      MinRuntime = TimeSpan.Zero;
      MaxRuntime = TimeSpan.MaxValue;
      CheckIfAlive = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override ParticleEffector CreateInstanceCore()
    {
      return new ParticleSystemRecycler();
    }


    /// <inheritdoc/>
    protected override void CloneCore(ParticleEffector source)
    {
      base.CloneCore(source);

      var sourceTyped = (ParticleSystemRecycler)source;
      MinRuntime = sourceTyped.MinRuntime;
      MaxRuntime = sourceTyped.MaxRuntime;
      CheckIfAlive = sourceTyped.CheckIfAlive;
      ResourcePool = sourceTyped.ResourcePool;
    }
    #endregion


    /// <inheritdoc/>
    protected override void OnEndUpdate(TimeSpan deltaTime)
    {
      if (ParticleSystem.Time < MinRuntime)
        return;

      if (ParticleSystem.Time >= MaxRuntime || (CheckIfAlive && !ParticleSystem.IsAlive()))
      {
        OnRecycle(EventArgs.Empty);

        // Remove the particle system from the service or the parent system.
        // Important: Particle systems can be updated in parallel. Access to
        // the ParticleSystems collections must be synchronized!
        // (This class assumes that it is the only effector which changes this 
        // collection. If there are others, we need to use a shared lock object.)
        if (ParticleSystem.Service != null)
          lock (_lock)
            ParticleSystem.Service.ParticleSystems.Remove(ParticleSystem);

        if (ParticleSystem.Parent != null)
          lock (_lock)
            ParticleSystem.Parent.Children.Remove(ParticleSystem);

        var recyclable = ParticleSystem as IRecyclable;
        if (recyclable != null)
        {
          recyclable.Recycle();
        }
        else
        {
          ParticleSystem.Reset();
          if (ResourcePool != null)
            ResourcePool.Recycle(ParticleSystem);
        }
      }
    }


    /// <summary>
    /// Raises the <see cref="Recycle"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="EventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnRecycle"/> in a derived 
    /// class, be sure to call the base class’s <see cref="OnRecycle"/> method so that registered 
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnRecycle(EventArgs eventArgs)
    {
      var handler = Recycle;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
