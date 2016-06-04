// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game.States
{
  /// <summary>
  /// Represents a state machine.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This is an implementation of the UML state machine view.
  /// </para>
  /// <para>
  /// <see cref="StateMachine.Update"/> must be called to let the state machine advance a single
  /// time step. All transitions and state updates are performed only when the method 
  /// <see cref="Update"/> is called. Transition events between <see cref="Update"/> calls are 
  /// stored and executed at the beginning of the next <see cref="Update"/> call.
  /// </para>
  /// </remarks>
  public class StateMachine
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Re-use StateEventArgs instance to avoid unnecessary memory allocations.
    private readonly StateEventArgs StateEventArgs = new StateEventArgs();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the states.
    /// </summary>
    /// <value>The states.</value>
    public StateCollection States { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="StateMachine"/> class.
    /// </summary>
    public StateMachine()
    {
      States = new StateCollection();
      //States.StateMachine = this;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Updates the state machine.
    /// </summary>
    /// <param name="deltaTime">The elapsed time since the last update.</param>
    public void Update(TimeSpan deltaTime)
    {
      StateEventArgs.DeltaTime = deltaTime;

      // First, perform transitions. 
      // If this is the first call, then the initial states are entered.
      States.UpdateTransitions(StateEventArgs);

      // Update active state. 
      States.UpdateState(StateEventArgs);
    }
    #endregion
  }
}
