// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRune.Game.States
{
  /// <summary>
  /// Defines a transition between two states.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A transition changes the active state from <see cref="SourceState"/> to 
  /// <see cref="TargetState"/>. It "fires" if <see cref="Fire()"/> was called and the 
  /// <see cref="Guard"/> condition evaluates to <see langword="true"/>. The 
  /// <see cref="Fire(object,EventArgs)"/> method has an event handler signature, so it can be
  /// connect to a standard .NET event.
  /// </para>
  /// <para>
  /// The <see cref="Action"/> event is raised when a transition is performed. Transitions are
  /// performed at the beginning of <see cref="StateMachine"/>.<see cref="StateMachine.Update"/>.
  /// </para>
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public class Transition : INamedObject
  {
    // Note: The Action is not executed in Update() because the action must be called 
    // between the Exit and Enter events.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Remember if Fire() was called.
    private bool _fired;

    // The rest time delay until the transition fires. 
    // (The transitions needs to be fired when _restDelay becomes 0 or negative in
    // the current update.)
    private TimeSpan _restDelay;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the time delay.
    /// </summary>
    /// <value>The time delay.</value>
    /// <remarks>
    /// If the transition was triggered, the transition is delayed for this time span.
    /// </remarks>
    public TimeSpan Delay { get; set; }


    /// <summary>
    /// Gets or sets the guard predicate.
    /// </summary>
    /// <value>
    /// The guard predicate. Per default, this value is <see langword="null"/> - which is treated as
    /// "always true".
    /// </value>
    /// <remarks>
    /// A transition can fire if this predicate returns <see langword="true"/>. 
    /// </remarks>
    public Func<bool> Guard { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this transition should fire always.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if transition fires always; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Setting this value to <see langword="true"/> creates a transition that does not need a
    /// <see cref="Fire()"/> call to be triggered. Instead it acts as if <see cref="Fire()"/> is 
    /// called in each time step. Even if this value is <see langword="true"/>, the 
    /// <see cref="Guard"/> condition and the <see cref="Delay"/> are still active. 
    /// </remarks>
    public bool FireAlways { get; set; }


    /// <summary>
    /// Gets or sets the name of the state.
    /// </summary>
    /// <value>The name of the state.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the source state.
    /// </summary>
    /// <value>The source state.</value>
    /// <remarks>
    /// Setting/resetting this property automatically updates the <see cref="State.Transitions"/> 
    /// collection of the source state. 
    /// </remarks>
    public State SourceState
    {
      get { return _sourceState; }
      set 
      { 
        if (_sourceState == value)
          return;

        if (_sourceState != null)
        {
          // The user manually re-assigns the transitions to a different source state.
          Debug.Assert(_sourceState.Transitions.Contains(this));

          // Create local copy and reset _sourceState. (The method State.Transitions.Remove() 
          // below also sets the SourceState to null - recursive call! By setting the
          // property to null immediately, we avoid doing unnecessary work in the recursive
          // call.)
          var previousSourceState = _sourceState; 
          _sourceState = null;

          previousSourceState.Enter -= SourceStateEntered;
          previousSourceState.Transitions.Remove(this);
        }
        
        _sourceState = value;

        if (_sourceState != null)
        {
          if (!_sourceState.Transitions.Contains(this))
            _sourceState.Transitions.Add(this);

          _sourceState.Enter += SourceStateEntered;
        }
      }
    }    
    private State _sourceState;


    /// <summary>
    /// Gets or sets the target state.
    /// </summary>
    /// <value>The target state.</value>
    public State TargetState { get; set; }


    /// <summary>
    /// Occurs when the transition is performed.
    /// </summary>
    public event EventHandler<StateEventArgs> Action;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Fires this transition.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Fires this transition.
    /// </summary>
    /// <remarks>
    /// If this method is called, the transition's <see cref="Guard"/> condition is evaluated
    /// in the next time step of the <see cref="StateMachine"/>. If the <see cref="Guard"/>
    /// condition is <see langword="true"/>, the transition is performed.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
    public void Fire()
    {
      _fired = true;
    }


    /// <summary>
    /// Fires this transition. (Event handler signature)
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="EventArgs"/> instance containing the event data.
    /// </param>
    /// <remarks>
    /// <para>
    /// If this method is called, the transition's <see cref="Guard"/> condition is evaluated
    /// in the next time step of the <see cref="StateMachine"/>. If the <see cref="Guard"/>
    /// condition is <see langword="true"/>, the transition is performed.
    /// </para>
    /// <para>
    /// This method can be used as an event handler.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
    public void Fire(object sender, EventArgs eventArgs)
    {
      _fired = true;
    }


    /// <summary>
    /// Checks if the transition should be performed.
    /// </summary>
    /// <param name="deltaTime">The time step.</param>
    /// <returns>
    /// <see langword="true"/> if the transition should be performed; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    internal bool Update(TimeSpan deltaTime)
    {
      Debug.Assert(SourceState != null);

      if (_restDelay > TimeSpan.Zero)
      {
        // ----- Transition was fired but execution is delayed.
        _restDelay -= deltaTime;

        if (_restDelay <= TimeSpan.Zero)
        {
          // Delay time has passed. Transition should be executed.
          return true;
        }

        // Still delaying.
        return false;
      }

      if (!_fired && !FireAlways)
        return false;       // No Fire(), abort.

      if (!_fired)
      {
        // FireAlways was used. In this case we only fire if the source state is finished.
        foreach(var stateCollection in SourceState.ParallelSubStates)
        {
          if (stateCollection.FinalState != null                                // State collection has a FinalState.
              && stateCollection.ActiveState != stateCollection.FinalState)     // But the active state is not the final --> abort.
          {
            return false;
          }
        }
      }

      if (Guard != null && !Guard())
        return false;       // Fire() but Guard returns false.

      _fired = false;

      if (Delay <= deltaTime)
        return true;        // No delay. Transition should be executed.

      // Delay time must pass. Remember rest of delay time.
      _restDelay = Delay - deltaTime;
      return false;
    }


    private void SourceStateEntered(object sender, EventArgs eventArgs)
    {
      // State was entered --> reset transition.
      _restDelay = TimeSpan.Zero;
      _fired = false;
    }


    /// <summary>
    /// Raises the <see cref="Action"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="StateEventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnAction"/> in a derived
    /// class, be sure to call the base class's <see cref="OnAction"/> method so that registered
    /// delegates receive the event.
    /// </remarks>
    internal protected virtual void OnAction(StateEventArgs eventArgs)
    {
      var handler = Action;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
