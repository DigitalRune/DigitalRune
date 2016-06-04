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
  /// Manipulates a particle system and/or its particles.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The particle effectors need to be added to a <see cref="ParticleSystem"/>. They define the 
  /// behavior of the particle system and/or its particles. An effector has several properties and 
  /// methods that are automatically set or called by the <see cref="ParticleSystem"/>. The members 
  /// are used in the following order:
  /// </para>
  /// <list type="number">
  /// <item>
  /// <see cref="ParticleSystem"/> is automatically set when the effector is added to a particle 
  /// system (see property <see cref="DigitalRune.Particles.ParticleSystem.Effectors"/>).
  /// </item>
  /// <item>
  /// <see cref="RequeryParameters"/> is called next. <see cref="RequeryParameters"/> calls
  /// <see cref="OnRequeryParameters"/>. Derived particle effectors can override this method to get 
  /// all required particle parameters from the parameter collection of the particle system (see 
  /// <see cref="Particles.ParticleSystem.Parameters"/>). The particle effector can store references
  /// to these parameters. Anytime the particle parameters are modified, 
  /// <see cref="RequeryParameters"/> is called again.
  /// </item>
  /// <item>
  /// <see cref="Initialize"/> is called when the particle system is updated for the first time and 
  /// anytime the particle system is reset (see <see cref="Particles.ParticleSystem.Reset"/>).
  /// </item>
  /// <item>
  /// Every time the particle system is updated, it calls <see cref="BeginUpdate"/> of all particle 
  /// effectors. <see cref="BeginUpdate"/> calls <see cref="OnBeginUpdate"/>. Derived particle 
  /// effectors can override this method to create particles (see 
  /// <see cref="Particles.ParticleSystem.AddParticles(int, object)"/>), to change uniform particle 
  /// parameters or to do other work.
  /// </item>
  /// <item>
  /// After <see cref="BeginUpdate"/> the particle system calls <see cref="UpdateParticles"/> of all
  /// particle effectors. <see cref="UpdateParticles"/> calls <see cref="OnUpdateParticles"/>. 
  /// Derived particle effectors can override this method to manipulate the particles.
  /// </item>
  /// <item>
  /// When the update of a particle system ends, it calls <see cref="EndUpdate"/> of all particle 
  /// effectors. <see cref="EndUpdate"/> calls <see cref="OnEndUpdate"/>. Derived particle effectors
  /// can override this method to perform additional checks, a clean up, or other tasks.
  /// </item>
  /// <item>
  /// Each time a set of new particles is created, the particle system calls 
  /// <see cref="InitializeParticles"/>. <see cref="InitializeParticles"/> calls 
  /// <see cref="OnInitializeParticles"/>. Derived particle effectors can override this method to 
  /// initialize the particle parameters of newly created particles.
  /// </item>
  /// <item>
  /// When a particle effectors is removed from a particle system, <see cref="Uninitialize"/> is 
  /// called. <see cref="Uninitialize"/> calls <see cref="OnUninitialize"/>. Derived classes
  /// can override this method to clean up any held resources.
  /// </item>
  /// </list>
  /// <para>
  /// <strong>Cloning:</strong> A particle effector can be cloned. When a clone is created using 
  /// <see cref="Clone"/>, a new particle effectors is returned for use in another particle system. 
  /// <see cref="Clone"/> does not set the property <see cref="ParticleSystem"/> of the clone. See 
  /// also remarks of derived classes.
  /// </para>
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public abstract partial class ParticleEffector : INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    
    /// <summary>
    /// Gets the name of this particle effector.
    /// </summary>
    /// <value>
    /// The name of the particle effector. The default value is <see langword="null"/>.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Cannot change the name because the particle effector has already been added to a particle 
    /// system.
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

        if (ParticleSystem != null)
          throw new InvalidOperationException("Cannot change name of a particle effector that has already been added to a particle system.");

        _name = value;
      }
    }
    private string _name;


    /// <summary>
    /// Gets or sets the particle system.
    /// </summary>
    /// <value>The particle system that owns this particle effector.</value>
    /// <remarks>
    /// This property is automatically set when the effector is added to a particle system (see
    /// property <see cref="DigitalRune.Particles.ParticleSystem.Effectors"/>).
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializerIgnore]
#endif
    public ParticleSystem ParticleSystem { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ParticleEffector"/> is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if enabled; otherwise, <see langword="false"/>. The default value is 
    /// <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// When disabled the following methods are not executed: 
    /// <see cref="OnBeginUpdate"/>, <see cref="OnUpdateParticles"/>, <see cref="OnEndUpdate"/> and 
    /// <see cref="OnInitializeParticles"/>.
    /// </remarks>
#if XNA || MONOGAME
    [ContentSerializer(Optional = true)]
#endif
    public bool Enabled { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleEffector"/> class.
    /// </summary>
    protected ParticleEffector()
    {
      Enabled = true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Requeries the particle parameters.
    /// </summary>
    /// <remarks>
    /// This method is automatically called by the particle system before the effector is used for 
    /// the first time and when its particle parameter collection was modified.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Requery")]
    public void RequeryParameters()
    {
      if (ParticleSystem != null)
        OnRequeryParameters();
    }


    /// <summary>
    /// Called when <see cref="RequeryParameters"/> was called.
    /// </summary>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> <br/>
    /// When this method is called, <see cref="ParticleEffector.ParticleSystem"/> is already
    /// initialized and can be used.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Requery")]
    protected virtual void OnRequeryParameters()
    {
    }


    /// <summary>
    /// Initializes this particle effector.
    /// </summary>
    /// <remarks>
    /// This method is automatically called by the particle system before the effector is used for 
    /// the first time and when the particle system is reset. (Particle systems call 
    /// <see cref="RequeryParameters"/> before <see cref="Initialize"/>, therefore the parameters
    /// are already available.)
    /// </remarks>
    public void Initialize()
    {
      if (ParticleSystem != null)
        OnInitialize();
    }


    /// <summary>
    /// Called when <see cref="Initialize"/> was called.
    /// </summary>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> <br/>
    /// When this method is called, <see cref="ParticleEffector.ParticleSystem"/> is already
    /// initialized and <see cref="OnRequeryParameters"/> was already called.
    /// </remarks>
    protected virtual void OnInitialize()
    {
    }


    /// <summary>
    /// Uninitializes this particle effector.
    /// </summary>
    /// <remarks>
    /// This method is automatically called by the particle system before the effector is removed
    /// from the particle system.
    /// </remarks>
    public void Uninitialize()
    {
      OnUninitialize();
    }


    /// <summary>
    /// Called when <see cref="Uninitialize"/> was called.
    /// </summary>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> <br/>
    /// When this method is called, the property <see cref="ParticleEffector.ParticleSystem"/> is 
    /// still set. The property is automatically set to <see langword="null"/> after this method was
    /// executed.
    /// </remarks>
    protected virtual void OnUninitialize()
    {
    }


    /// <summary>
    /// Called when the particle system begins its update.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    /// <remarks>
    /// This method is automatically called by the particle system at the beginning of its
    /// <see cref="Particles.ParticleSystem.Update"/> method.
    /// </remarks>
    public void BeginUpdate(TimeSpan deltaTime)
    {
      if (Enabled && ParticleSystem != null)
        OnBeginUpdate(deltaTime);
    }


    /// <summary>
    /// Called when <see cref="BeginUpdate"/> was called.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> <br/>
    /// When this method is called, <see cref="ParticleEffector.ParticleSystem"/> is already
    /// initialized and the particle effector is <see cref="Enabled"/>.
    /// </remarks>
    protected virtual void OnBeginUpdate(TimeSpan deltaTime)
    {
    }


    /// <summary>
    /// Initializes new particles.
    /// </summary>
    /// <param name="startIndex">The index of the first particle to initialize.</param>
    /// <param name="count">The number of particles to initialize.</param>
    /// <param name="emitter">
    /// The emitter that triggered the particle creation. This can be an effector, another object or
    /// <see langword="null"/>.
    /// </param>
    /// <remarks>
    /// This method is automatically called by the particle system after new particles have been 
    /// created. 
    /// </remarks>
    public void InitializeParticles(int startIndex, int count, object emitter)
    {
      if (startIndex < 0)
        throw new ArgumentOutOfRangeException("startIndex");
      
      if (!Enabled || ParticleSystem == null || count <= 0)
        return;

      var maxNumberOfParticles = ParticleSystem.MaxNumberOfParticles;

      // Ensure limit. (Usually not necessary, except when called directly with 
      // invalid parameters. We could also throw an exception.)
      if (count > maxNumberOfParticles)
        count = maxNumberOfParticles;

      if (startIndex + count <= maxNumberOfParticles)
      {
        OnInitializeParticles(startIndex, count, emitter);
      }
      else
      {
        // Range exceeds end of array. Restart at beginning. (Circular buffer)
        int count0 = maxNumberOfParticles - startIndex; // Batch #1
        int count1 = count - count0;                    // Batch #2

        OnInitializeParticles(startIndex, count0, emitter);
        OnInitializeParticles(0, count1, emitter);
      }
    }


    /// <summary>
    /// Called when <see cref="InitializeParticles"/> was called.
    /// </summary>
    /// <param name="startIndex">The index of the first particle to initialize.</param>
    /// <param name="count">The number of particles to initialize.</param>
    /// <param name="emitter">
    /// The emitter that triggered the particle creation. This can be an effector, another object or
    /// <see langword="null"/>.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> <br/>
    /// When this method is called, <see cref="ParticleEffector.ParticleSystem"/> is already
    /// initialized and the particle effector is <see cref="Enabled"/>. <paramref name="startIndex"/>
    /// plus <paramref name="count"/> will never exceed the maximal number of particles (= the
    /// length of the particle parameter arrays). This method does not have to make any index 
    /// checks. (When <paramref name="startIndex"/> plus <paramref name="count"/> would exceed the 
    /// length of the particle parameter arrays, <see cref="InitializeParticles"/> calls 
    /// <see cref="OnInitializeParticles"/> twice.)
    /// </remarks>
    protected virtual void OnInitializeParticles(int startIndex, int count, object emitter)
    {
    }


    /// <summary>
    /// Updates particles.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    /// <param name="startIndex">The index of the first particle to initialize.</param>
    /// <param name="count">The number of particles to initialize.</param>
    /// <remarks>
    /// This method is automatically called when the particle system is updated (see
    /// <see cref="Particles.ParticleSystem.Update"/>).
    /// </remarks>
    public void UpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
      if (startIndex < 0)
        throw new ArgumentOutOfRangeException("startIndex");

      if (!Enabled || ParticleSystem == null || count <= 0)
        return;

      var maxNumberOfParticles = ParticleSystem.MaxNumberOfParticles;

      // Ensure limit. (Usually not necessary, except when called directly with 
      // invalid parameters. We could also throw an exception.)
      if (count > maxNumberOfParticles)
        count = maxNumberOfParticles;

      if (startIndex + count <= maxNumberOfParticles)
      {
        OnUpdateParticles(deltaTime, startIndex, count);
      }
      else
      {
        // Range exceeds end of array. Restart at beginning. (Circular buffer)
        int count0 = maxNumberOfParticles - startIndex; // Batch #1
        int count1 = count - count0;                    // Batch #2

        OnUpdateParticles(deltaTime, startIndex, count0);
        OnUpdateParticles(deltaTime, 0, count1);
      }
    }


    /// <summary>
    /// Called when <see cref="UpdateParticles"/> was called.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    /// <param name="startIndex">The index of the first particle to initialize.</param>
    /// <param name="count">The number of particles to initialize.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> <br/>
    /// When this method is called, <see cref="ParticleEffector.ParticleSystem"/> is already
    /// initialized and the particle effector is <see cref="Enabled"/>. <paramref name="startIndex"/>
    /// plus <paramref name="count"/> will never exceed the maximal number of particles (= the
    /// length of the particle parameter arrays). This method does not have to make any index 
    /// checks. (When <paramref name="startIndex"/> plus <paramref name="count"/> would exceed the 
    /// length of the particle parameter arrays, <see cref="UpdateParticles"/> calls 
    /// <see cref="OnUpdateParticles"/> twice.)
    /// </remarks>
    protected virtual void OnUpdateParticles(TimeSpan deltaTime, int startIndex, int count)
    {
    }


    /// <summary>
    /// Called when the particle system finishes its update.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    /// <remarks>
    /// This method is automatically called by the particle system at the end of its
    /// <see cref="Particles.ParticleSystem.Update"/> method.
    /// </remarks>
    public void EndUpdate(TimeSpan deltaTime)
    {
      if (Enabled && ParticleSystem != null)
        OnEndUpdate(deltaTime);
    }


    /// <summary>
    /// Called when <see cref="BeginUpdate"/> was called.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> <br/>
    /// When this method is called, <see cref="ParticleEffector.ParticleSystem"/> is already
    /// initialized and the particle effector is <see cref="Enabled"/>.
    /// </remarks>
    protected virtual void OnEndUpdate(TimeSpan deltaTime)
    {
    }
    #endregion
  }
}
