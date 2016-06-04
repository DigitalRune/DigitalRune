// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Physics.Materials;
using DigitalRune.Threading;


namespace DigitalRune.Physics.Settings
{
  /// <summary>
  /// Defines simulation settings that control the physics simulation.
  /// </summary>
  public class SimulationSettings
  {
    // Notes:
    // This is a separate class so that it can be shared between simulations.
    // or it can be serialized.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the constraint-related simulation settings.
    /// </summary>
    /// <value>The constraint settings.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public ConstraintSettings Constraints
    {
      get { return _constraints; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _constraints = value;
      }
    }
    private ConstraintSettings _constraints;


    /// <summary>
    /// Gets or sets the motion-related simulation settings.
    /// </summary>
    /// <value>The motion settings.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public MotionSettings Motion
    {
      get { return _motion; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _motion = value;
      }
    }
    private MotionSettings _motion;


    /// <summary>
    /// Gets or sets the sleeping-related simulation settings.
    /// </summary>
    /// <value>The sleeping settings.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public SleepingSettings Sleeping
    {
      get { return _sleeping; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _sleeping = value;
      }
    }
    private SleepingSettings _sleeping;


    /// <summary>
    /// Gets or sets a value indicating whether multithreading is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if multithreading is enabled; otherwise, <see langword="false"/>. The
    /// default value is <see langword="true"/> if the current system has more than one CPU cores.
    /// </value>
    /// <remarks>
    /// <para>
    /// When multithreading is enabled the simulation will distribute the workload across multiple 
    /// processors (CPU cores) to improve the performance. 
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
    /// The simulation internally uses the class <see cref="Parallel"/> for parallelization.
    /// <see cref="Parallel"/> is a static class that defines how many worker threads are created, 
    /// how the workload is distributed among the worker threads and more. (See 
    /// <see cref="Parallel"/> to find out more on how to configure parallelization.)
    /// </para>
    /// </remarks>
    /// <seealso cref="Parallel"/>
    public bool EnableMultithreading { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the collision domain is kept up-to-date.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if collision domain is kept up-to-date; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The simulation updates the internal collision domain (see 
    /// <see cref="Simulation.CollisionDomain"/>) only if necessary. In 
    /// <see cref="Simulation.Update(TimeSpan)"/> the collision domain is updated to compute the 
    /// new contacts at the beginning of each time step. Then the new positions and orientations of 
    /// the simulation objects are computed. At the end of <see cref="Simulation.Update(TimeSpan)"/> 
    /// the contact information of the collision domain is not up-to-date because the simulation 
    /// objects have moved.
    /// </para>
    /// <para>
    /// By setting <see cref="SynchronizeCollisionDomain"/> to <see langword="true"/> the 
    /// <see cref="Simulation"/> explicitly updates the collision domain at the end of 
    /// <see cref="Simulation.Update(TimeSpan)"/> to ensure that the contact information is valid. 
    /// This additional update costs a bit of performance, therefore the property 
    /// <see cref="SynchronizeCollisionDomain"/> should only be set if other parts of the 
    /// application need to access the contact information of the collision domain.
    /// </para>
    /// <para>
    /// Alternatively, the user can explicitly update the collision domain by calling:
    /// <code lang="csharp">
    /// <![CDATA[
    /// // Ensure that the contact information is up-to-date.
    /// simulation.CollisionDomain.Update(0);
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public bool SynchronizeCollisionDomain { get; set; }


    /// <summary>
    /// Gets or sets the timing-related simulation settings.
    /// </summary>
    /// <value>The timing settings.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public TimingSettings Timing
    {
      get { return _timing; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _timing = value;
      }
    }
    private TimingSettings _timing;


    /// <summary>
    /// Gets or sets the material property combiner.
    /// </summary>
    /// <value>
    /// The material property combiner. The default is a new instance of type 
    /// <see cref="Materials.MaterialPropertyCombiner"/>.
    /// </value>
    /// <remarks>
    /// The material combiner is used to compute material settings of touching bodies.
    /// See <see cref="IMaterialPropertyCombiner"/> for more information. This object can 
    /// be replaced to use a custom material combination strategy.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public IMaterialPropertyCombiner MaterialPropertyCombiner
    {
      get { return _materialPropertyCombiner; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _materialPropertyCombiner = value;
      }
    }
    private IMaterialPropertyCombiner _materialPropertyCombiner;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationSettings"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2140:TransparentMethodsMustNotReferenceCriticalCodeFxCopRule", Justification = "Accessing Environment.ProcessorCount on Xbox 360 works.")]
    public SimulationSettings()
    {
      Constraints = new ConstraintSettings();
      Motion = new MotionSettings();
      Sleeping = new SleepingSettings();
      Timing = new TimingSettings();
      MaterialPropertyCombiner = new MaterialPropertyCombiner();
#if WP7 || UNITY
      // Cannot access Environment.ProcessorCount in phone app. (Security issue).
      EnableMultithreading = false;
#else
      // Enable multithreading by default if the current system has multiple processors.
      EnableMultithreading = Environment.ProcessorCount > 1;

      // Multithreading works but Parallel.For of Xamarin.Android/iOS is very inefficient.
      if (GlobalSettings.PlatformID == PlatformID.Android || GlobalSettings.PlatformID == PlatformID.iOS)
        EnableMultithreading = false;
#endif
    }


    //public SimulationSettings(SimulationSettings settings)
    //{
    //  Constraints = settings.Constraints;
    //  Motion = settings.Motion;
    //  Sleeping = settings.Sleeping;
    //  Timing = settings.Timing;
    //  MaterialCombiner = settings.MaterialCombiner;
    //  EnableMultithreading = settings.EnableMultithreading;
    //}
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }

}
