// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Threading;
using DigitalRune.Collections;


namespace DigitalRune.Game.States
{
  /// <summary>
  /// Defines a state in a state machine.
  /// </summary>
  /// <remarks>
  /// Important note: Each state must be given a name that is unique within the 
  /// <see cref="StateCollection"/> in which it will be used.
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public class State : INamedObject
  {
    // Note: The property StateMachine can be removed. It is only used for consistency checks at 
    // runtime. --> It has been commented out.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static int _lastId = -1;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this state is active.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this state is active; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsActive
    {
      get
      {
        return Owner != null && Owner.ActiveState == this;
      }
    }
    

    /// <summary>
    /// Gets or sets the name of the state.
    /// </summary>
    /// <value>The name of the state.</value>
    /// <exception cref="InvalidOperationException">
    /// Cannot change name of a state because it is already part of <see cref="StateCollection"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public string Name
    {
      get { return _name; }
      set
      {
        if (value == _name)
          return;

        if (Owner != null)
          throw new InvalidOperationException("Cannot change name while the state is part of StateCollection.");

        _name = value;
      }
    }
    private string _name;


    /// <summary>
    /// Gets the state collection that owns this state.
    /// </summary>
    /// <value>The state collection that owns this state.</value>
    public StateCollection Owner { get; internal set; }


    /// <summary>
    /// Gets the parallel sub-state collections.
    /// </summary>
    /// <value>The parallel sub-state collections.</value>
    /// <remarks>
    /// If there are no sub-states, this collection is empty. If this state has sub-states
    /// but no concurrent sub-states, then this collection contains exactly one 
    /// <see cref="StateCollection"/>. If this state has several concurrent sub-states, this 
    /// collection contains several <see cref="StateCollection"/>s. The states in each 
    /// <see cref="StateCollection"/> are executed in parallel. ("Parallel" in this context means
    /// that the sub-states are executed "independently from each other". It does not necessarily
    /// mean that the sub-states are executed concurrently on different threads.)
    /// </remarks>
    public SubStatesCollection ParallelSubStates
    {
      get
      {
        if (_parallelSubStates == null)
        {
          _parallelSubStates = new SubStatesCollection();
          _parallelSubStates.CollectionChanged += OnParallelSubStatesChanged;
        }
        
        return _parallelSubStates;
      }      
    }
    private SubStatesCollection _parallelSubStates;


    ///// <summary>
    ///// Gets the state machine.
    ///// </summary>
    ///// <value>The state machine.</value>
    //public StateMachine StateMachine
    //{
    //  get { return _stateMachine; }
    //  internal set
    //  {
    //    if (_stateMachine == value)
    //      return;

    //    _stateMachine = value;

    //    if (_parallelSubStates != null)
    //      foreach (var stateCollection in _parallelSubStates)
    //        stateCollection.StateMachine = value;
    //  }
    //}
    //private StateMachine _stateMachine;


    /// <summary>
    /// Gets the transitions.
    /// </summary>
    /// <value>The transitions.</value>
    /// <remarks>
    /// If a <see cref="Transition.SourceState"/> of a <see cref="Transition"/> is set, the 
    /// transition is automatically added to the <see cref="Transitions"/> collection of the source
    /// state. In most cases it is not necessary to manually add transitions to this collection. -
    /// However, if a transition is added to this collection, the 
    /// <see cref="Transition.SourceState"/> property of the transitions is set automatically.
    /// </remarks>
    public TransitionCollection Transitions { get; private set; }


    /// <summary>
    /// Occurs when the state is entered.
    /// </summary>
    public event EventHandler<StateEventArgs> Enter;


    /// <summary>
    /// Occurs when the state is updated.
    /// </summary>
    public event EventHandler<StateEventArgs> Update;


    /// <summary>
    /// Occurs when the state is exited.
    /// </summary>
    public event EventHandler<StateEventArgs> Exit;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="State"/> class.
    /// </summary>
    public State()
    {
      // Set a unique default name.
      _name = "State" + (uint)Interlocked.Increment(ref _lastId);

      Transitions = new TransitionCollection();
      Transitions.CollectionChanged += OnTransitionsChanged;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="State"/> class.
    /// </summary>
    /// <param name="name">The name of this state.</param>
    public State(string name)
    {
      _name = name;

      Transitions = new TransitionCollection();
      Transitions.CollectionChanged += OnTransitionsChanged;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Determines whether the given state is a sub-state.
    /// </summary>
    /// <param name="subState">The other state.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="subState"/> is a sub-state of this state; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    internal bool Contains(State subState)
    {
      // Note: States do not contain themselves: stateX.Contains(stateX) == false.

      while (subState != null)
      {
        Debug.Assert(subState.Owner != null, "Owner of state is not properly set. (Owner is null.)");
        subState = subState.Owner.Owner;
        if (subState == this)
          return true;
      }

      return false;
    }


    private void OnParallelSubStatesChanged(object sender, CollectionChangedEventArgs<StateCollection> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      // Set/reset Owner and StateMachine properties.
      foreach (StateCollection stateCollection in eventArgs.OldItems)
      {
        stateCollection.Owner = null;
        //stateCollection.StateMachine = null;
      }

      foreach (StateCollection stateCollection in eventArgs.NewItems)
      {
        //if (stateCollection.StateMachine != null)
        //  throw new InvalidOperationException("The state collection is already part of a different state machine.");
        if (stateCollection.Owner != null && stateCollection.Owner != this)
          throw new InvalidOperationException("The state collection is already part of a different state.");

        stateCollection.Owner = this;
        //stateCollection.StateMachine = StateMachine;
      }
    }


    private void OnTransitionsChanged(object sender, CollectionChangedEventArgs<Transition> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      // Set/reset SourceState properties.
      foreach (Transition transition in eventArgs.OldItems)
      {
        transition.SourceState = null;
      }

      foreach (Transition transition in eventArgs.NewItems)
      {
        if (transition.SourceState != null && transition.SourceState != this)
          throw new InvalidOperationException("The transition is already bound to a different source state.");

        transition.SourceState = this;
      }
    }


    /// <summary>
    /// Performs state transitions.
    /// </summary>
    /// <param name="eventArgs">
    /// The <see cref="StateEventArgs"/> instance containing the required data.
    /// </param>
    /// <returns>
    /// The firing transition; or <see langword="null"/> if no transition is firing.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    internal Transition UpdateTransitions(StateEventArgs eventArgs)
    {
      Transition firingTransition = null;

      // Update sub-state transitions first.
      if (_parallelSubStates != null)
      {
        foreach (var stateCollection in _parallelSubStates)
        {
          firingTransition = stateCollection.UpdateTransitions(eventArgs);
          if (firingTransition != null && !stateCollection.ContainsRecursive(firingTransition.TargetState))
          {
            // The transition target is outside the sub-state collection.
            // More parallel transition updates are only allowed for internal transitions.
            break;
          }

          // There was no transition or the transition was an internal transition.
          // We can check the next parallel state set.
        }
      }

      // Abort if a transition was performed.
      if (firingTransition != null)
        return firingTransition;

      // Check transitions of this state.
      foreach (var transition in Transitions)
      {
        if (transition.TargetState == null)
          throw new InvalidOperationException("TargetState of transition must not be null.");
        //if (transition.TargetState.StateMachine != StateMachine)
        //  throw new InvalidOperationException("TargetState of transition must not belong to different state machine.");

        // Check transition.
        bool fired = transition.Update(eventArgs.DeltaTime);
        if (fired)
        {
          // Get state collection that contains this state and the target state.
          var stateCollection = StateCollection.GetCollection(this, transition.TargetState);

          // Exit states.
          stateCollection.ExitState(transition, eventArgs);

          // Execute transition action.
          transition.OnAction(eventArgs);

          // Enter states.
          stateCollection.EnterState(transition, eventArgs);

          // Do not update other transitions after first transition has fired.
          return transition;
        }
      }

      return null;
    }


    /// <summary>
    /// Enters the state.
    /// </summary>
    /// <param name="transition">The firing transition.</param>
    /// <param name="eventArgs">
    /// The <see cref="StateEventArgs"/> instance containing the required data.
    /// </param>
    internal void EnterState(Transition transition, StateEventArgs eventArgs)
    {
      if (!IsActive)
      {
        // State is not active. Raise Enter event.
        OnEnter(eventArgs);
      }

      // Call Enter for sub-states.
      if (_parallelSubStates != null)
        foreach (var stateCollection in _parallelSubStates)
          stateCollection.EnterState(transition, eventArgs);
    }


    /// <summary>
    /// Updates the state.
    /// </summary>
    /// <param name="eventArgs">
    /// The <see cref="StateEventArgs"/> instance containing the required data.
    /// </param>
    internal void UpdateState(StateEventArgs eventArgs)
    {
      // Sub-states are updated first.
      if (_parallelSubStates != null)
        foreach (var stateCollection in _parallelSubStates)
          stateCollection.UpdateState(eventArgs);

      // Raise Update event.
      OnUpdate(eventArgs);
    }


    /// <summary>
    /// Exits to the specified target state.
    /// </summary>
    /// <param name="transition">The firing transition.</param>
    /// <param name="eventArgs">
    /// The <see cref="StateEventArgs"/> instance containing the required data.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if this state was exited. <see langword="false"/> if this state is 
    /// still active because the target state is a sub-state of this state.
    /// </returns>
    internal bool ExitState(Transition transition, StateEventArgs eventArgs)
    {
      Debug.Assert(IsActive, "Exit should only be called for active states.");

      if (transition != null && !Transitions.Contains(transition) && Contains(transition.TargetState))
      {
        // The transition is not from this state and the target is a sub-state
        // --> We do not leave the current state!
        // We have to find the sub-state collection that contains the target. In this sub-state
        // collection the active state will exit (and the target state will be entered later).
        if (_parallelSubStates != null)
          foreach (var stateCollection in _parallelSubStates)
            if (stateCollection.ContainsRecursive(transition.TargetState))
              stateCollection.ExitState(transition, eventArgs);

        return false;

        // This approach assumes that there are no transitions from a state to a state in a 
        // parallel set. This is not explicitly forbidden (it is never verified in the state
        // machine).
        // Programmers must not create such transitions and graphical state machine editors 
        // should prohibit such constructs.
      }

      // The target state is no sub-state. --> Exit all sub-state collections.
      if (_parallelSubStates != null)
        foreach (var stateCollection in _parallelSubStates)
          stateCollection.ExitState(null, eventArgs);

      // Raise Exit event.
      OnExit(eventArgs);

      return true;
    }


    /// <summary>
    /// Raises the <see cref="Enter"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="StateEventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnEnter"/> in a derived
    /// class, be sure to call the base class's <see cref="OnEnter"/> method so that registered
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnEnter(StateEventArgs eventArgs)
    {
      var handler = Enter;

      if (handler != null)
        handler(this, eventArgs);
    }


    /// <summary>
    /// Raises the <see cref="Update"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="StateEventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnUpdate"/> in a derived
    /// class, be sure to call the base class's <see cref="OnUpdate"/> method so that registered
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnUpdate(StateEventArgs eventArgs)
    {
      var handler = Update;

      if (handler != null)
        handler(this, eventArgs);
    }


    /// <summary>
    /// Raises the <see cref="Exit"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="StateEventArgs"/> object that provides the arguments for the event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnExit"/> in a derived
    /// class, be sure to call the base class's <see cref="OnExit"/> method so that registered
    /// delegates receive the event.
    /// </remarks>
    protected virtual void OnExit(StateEventArgs eventArgs)
    {
      var handler = Exit;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
