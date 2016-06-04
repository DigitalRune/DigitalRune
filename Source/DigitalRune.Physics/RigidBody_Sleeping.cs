// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Physics.Settings;


namespace DigitalRune.Physics
{
  public partial class RigidBody
  {
    // Notes:
    // Static bodies are always sleeping.
    // Kinematic bodies and dynamic bodies fall asleep after a certain _noMovementTime.
    // Dynamic bodies are kept awake by the simulation as long as a body in a simulation island
    // is no sleeping candidate.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // How long the body's velocities were below the sleeping thresholds [s].
    private float _noMovementTime;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether this body can sleep.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this rigid body can sleep; otherwise, <see langword="false"/>. 
    /// The default is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// Normally, all rigid bodies can sleep (see also <see cref="SleepingSettings"/>). For 
    /// critical bodies sleeping can be disabled, e.g. for rigid bodies of the player character. If 
    /// <see cref="CanSleep"/> is <see langword="false"/>, this body will always be awake and
    /// actively simulated.
    /// </remarks>
    public bool CanSleep
    {
      get { return _canSleep; }
      set
      {
        _canSleep = value;
        if (!_canSleep)
          IsSleeping = false;
      }
    }
    private bool _canSleep;


    /// <summary>
    /// Gets a value indicating whether the rigid body is a sleeping candidate, i.e. it is already 
    /// sleeping or will be sleeping in the next time step.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this rigid body is a sleeping candidate; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    internal bool IsSleepingCandidate
    {
      get
      {
        // Note: Do not change the comparison. We need to compare with ≥.
        // (The simulation first updates velocities which calls UpdateSleeping where the 
        // _noMovementTime is increased. If _noMovementTime >= TimeThreshold the body will start 
        // sleeping in the next frame. After updating the velocity the simulation processes islands
        // which checks IsSleepingCandidate. If an island contains a rigid body which is no sleeping
        // candidate then sleeping of all rigid bodies is deferred.
        // If we would check with > instead of ≥ then we would continually defer the sleeping since
        // the _noMovementTime has already been updated.)
        return _noMovementTime >= Simulation.Settings.Sleeping.TimeThreshold;
      }
    }


    /// <summary>
    /// Gets a value indicating whether this rigid body is sleeping.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is sleeping; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsSleeping { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether the simulation island touches a static or resting
    /// kinematic body. (Experimental)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the simulation island touches a static or resting kinematic body; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    internal bool IslandTouchesImmovable { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Makes sure the body is not sleeping for the given duration.
    /// </summary>
    /// <param name="duration">The duration.</param>
    internal void DeferSleep(float duration)
    {
      if (MotionType != MotionType.Static)
      {
        // We set _noMovementTime to a value below the sleeping threshold.
        IsSleeping = false;
        _noMovementTime = Math.Min(_noMovementTime, Simulation.Settings.Sleeping.TimeThreshold - duration);
      }
    }


    /// <summary>
    /// Deactivates this rigid body. 
    /// </summary>
    /// <remarks>
    /// The simulation will automatically put rigid bodies to sleep if they do not move for some
    /// time. This method can be called manually if rigid bodies are already initialized in a stable
    /// resting configuration. For example, if a stack of bodies is initialized in a game, 
    /// <see cref="Sleep"/> can be called for all bodies in the stack after the first 
    /// <see cref="Physics.Simulation.Update(TimeSpan)"/>. The bodies will wake up when other 
    /// objects interact with the stacked bodies. (Note: 
    /// <see cref="Physics.Simulation.Update(TimeSpan)"/> needs to be called at least once to detect
    /// all collision and initialize all constraints. After the 
    /// <see cref="Physics.Simulation.Update(TimeSpan)"/> the bodies can be put to sleep.)
    /// </remarks>
    public void Sleep()
    {
      if (CanSleep)
      {
        IsSleeping = true;
        _linearVelocity = Vector3F.Zero;
        _angularVelocity = Vector3F.Zero;
        _noMovementTime = float.PositiveInfinity;
      }
    }


    /// <summary>
    /// Wakes the rigid body up from sleeping.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public void WakeUp()
    {
      if (MotionType != MotionType.Static)
      {
        IsSleeping = false;
        _noMovementTime = 0;
      }
    }


    /// <summary>
    /// Updates the sleeping state. Is called in <see cref="UpdateVelocity"/>.
    /// </summary>
    /// <param name="deltaTime">The time step.</param>
    private void UpdateSleeping(float deltaTime)
    {
      if (IsSleeping || !CanSleep)
        return;

      if (MotionType == MotionType.Dynamic)
      {
        if (LinearVelocity.LengthSquared < Simulation.Settings.Sleeping.LinearVelocityThresholdSquared
           && AngularVelocity.LengthSquared < Simulation.Settings.Sleeping.AngularVelocityThresholdSquared)
        {
          // Movement is below threshold. Increase counter.
          _noMovementTime += deltaTime;

          // Static bodies sleep immediately. Kinematic and dynamic bodies are handled here.
          // Dynamic bodies can only sleep if their whole island is sleeping. (Note: When the island 
          // is processed the sleeping of the dynamic body is deferred if the island is awake.)
          if (_noMovementTime > Simulation.Settings.Sleeping.TimeThreshold)
            IsSleeping = true;
        }
        else
        {
          // Movement detected.
          _noMovementTime = 0;
        }
      }
      else
      {
        if (LinearVelocity.LengthSquared < Numeric.EpsilonFSquared
           && AngularVelocity.LengthSquared < Numeric.EpsilonFSquared)
        {
          // Kinematic bodies are set to sleep immediately!
          IsSleeping = true;
          _linearVelocity = Vector3F.Zero;
          _angularVelocity = Vector3F.Zero;
          _noMovementTime = float.PositiveInfinity;
        }
        else
        {
          // Movement detected.
          _noMovementTime = 0;
        }
      }
    }
    #endregion
  }
}
