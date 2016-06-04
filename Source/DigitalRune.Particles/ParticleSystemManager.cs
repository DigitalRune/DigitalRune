// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Threading;


namespace DigitalRune.Particles
{
  /// <summary>
  /// Manages a collection of particle systems.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Particle systems can be added to the <see cref="ParticleSystems"/> collection. All added
  /// particle systems are updated when <see cref="Update"/> is called. Usually, 
  /// <see cref="Update"/> should be called once per frame. 
  /// </para>
  /// <para>
  /// <strong>Multithreading Support:</strong> By default <see cref="EnableMultithreading"/> is set 
  /// on systems with multiple CPU cores and all particles systems are updated in parallel.
  /// </para>
  /// </remarks>
  public class ParticleSystemManager : IParticleSystemService
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly List<ParticleSystem> _particleSystemsCopy = new List<ParticleSystem>();
    private bool _collectionDirty;

    private readonly Action<int> _updateParticleSystem;
    private TimeSpan _deltaTime;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the particle systems.
    /// </summary>
    /// <value>The particle systems.</value>
    /// <remarks>
    /// Note that this collection does not include nested particle systems. A nested particle system
    /// (a particle system owned by another particle system) is automatically updated when the 
    /// parent particle system is updated. Only the root particle system needs to be added to the
    /// <see cref="ParticleSystems"/> collection.
    /// </remarks>
    public ParticleSystemCollection ParticleSystems { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether multithreading is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if multithreading is enabled; otherwise, <see langword="false"/>. The
    /// default value is <see langword="true"/> if the current system has more than one CPU cores.
    /// </value>
    /// <remarks>
    /// <para>
    /// When multithreading is enabled the particle system manager will distribute the workload 
    /// across multiple processors (CPU cores) to improve the performance. But multithreading also 
    /// adds an additional overhead, therefore it should only be enabled if the current system has 
    /// more than one CPU core and if the other cores are not fully utilized by the application. 
    /// Multithreading should be disabled if the system has only one CPU core or if all other CPU 
    /// cores are busy. In some cases it might be necessary to run a benchmark of the application 
    /// and compare the performance with and without multithreading to decide whether multithreading
    /// should be enabled or not.
    /// </para>
    /// <para>
    /// The particle system manager internally uses the class <see cref="Parallel"/> for 
    /// parallelization. <see cref="Parallel"/> is a static class that defines how many worker 
    /// threads are created, how the workload is distributed among the worker threads and more. 
    /// (See <see cref="Parallel"/> to find out more on how to configure parallelization.)
    /// </para>
    /// </remarks>
    /// <seealso cref="Parallel"/>
    public bool EnableMultithreading { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ParticleSystemManager"/> class.
    /// </summary>
    public ParticleSystemManager()
    {
      _updateParticleSystem = UpdateParticleSystem;

#if WP7 || UNITY
      // Cannot access Environment.ProcessorCount in phone app. (Security issue.)
      EnableMultithreading = false;
#else
      // Enable multithreading by default if the current system has multiple processors.
      EnableMultithreading = Environment.ProcessorCount > 1;

      // Multithreading works but Parallel.For of Xamarin.Android/iOS is very inefficient.
      if (GlobalSettings.PlatformID == PlatformID.Android || GlobalSettings.PlatformID == PlatformID.iOS)
        EnableMultithreading = false;
#endif

      ParticleSystems = new ParticleSystemCollection();
      ParticleSystems.CollectionChanged += OnParticleSystemsChanged;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void OnParticleSystemsChanged(object sender, CollectionChangedEventArgs<ParticleSystem> eventArgs)
    {
      _collectionDirty = true;

      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      if (eventArgs.NewItems != null)
      {
        foreach (var item in eventArgs.NewItems)
        {
          if (item.Service != null)
          {
            throw new ParticleSystemException("Cannot add particle system. The particle system has already been added to another particle system service.")
            {
              ParticleSystem = item,
            };  
          }

          item.Service = this;
        }
      }

      if (eventArgs.OldItems != null)
      {
        foreach (var item in eventArgs.OldItems)
        {
          item.Service = null;
        }
      }
    }


    /// <summary>
    /// Updates all particle systems.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    public void Update(TimeSpan deltaTime)
    {
      // Store deltaTime for use in the UpdateParticleSystems delegate.
      _deltaTime = deltaTime;

      // To update the objects, we loop over a copy of the collection. Thus, the original 
      // collection can be modified by the particle systems.
      UpdateParticleSystemsCopy();

      if (EnableMultithreading && _particleSystemsCopy.Count > 1)
      {
        Parallel.For(0, _particleSystemsCopy.Count, _updateParticleSystem);
      }
      else      
      {
        foreach (var system in _particleSystemsCopy)
          system.Update(deltaTime);
      }

      if (_collectionDirty)
      {
        // A particle system has removed itself. --> Remove all references.
        _particleSystemsCopy.Clear();
      }    
    }


    private void UpdateParticleSystemsCopy()
    {
      if (_collectionDirty)
      {
        _collectionDirty = false;

        _particleSystemsCopy.Clear();
        foreach (var item in ParticleSystems)
          _particleSystemsCopy.Add(item);
      }
    }


    private void UpdateParticleSystem(int index)
    {
      _particleSystemsCopy[index].Update(_deltaTime);
    }
    #endregion
  }
}
