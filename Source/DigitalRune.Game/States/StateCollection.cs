// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using DigitalRune.Collections;


namespace DigitalRune.Game.States
{
  /// <summary>
  /// Manages a collection of <see cref="State"/>s.
  /// </summary>
  /// <remarks>
  /// <see langword="null"/> items or duplicate items are not allowed in this collection. The 
  /// collection has a few special state references: <see cref="InitialState"/>, 
  /// <see cref="FinalState"/>, and <see cref="ActiveState"/>. These references must refer to states
  /// contained in the collection. If a referenced state is removed from the collection the 
  /// reference is set to <see langword="null"/>.
  /// </remarks>
  public class StateCollection : NamedObjectCollection<State>
  {
    // Note: The property StateMachine can be removed. It is only used for consistency checks at 
    // runtime. --> It has been commented out.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Threshold for creating a lookup dictionary in the base NamedObjectCollection<T>.
    private const int DictionaryCreationThreshold = 8;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the initial state.
    /// </summary>
    /// <value>The initial state.</value>
    /// <remarks>
    /// Per default and if this value is set to <see langword="null"/>, the first state in the 
    /// collection is the initial state. 
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public State InitialState
    {
      get 
      {
        if (_initialState == null && Count > 0)
          return this[0];

        Debug.Assert(_initialState == null || Contains(_initialState), "The initial state is not part of the StateCollection.");

        return _initialState; 
      }
      set 
      { 
        if (_initialState == value)
          return;

        if (value != null && !Contains(value))
          throw new InvalidOperationException("The initial state must be contained in the StateCollection.");

        _initialState = value; 
      }
    }
    private State _initialState;


    /// <summary>
    /// Gets or sets the final state.
    /// </summary>
    /// <value>The final state. The default value is <see langword="null"/>.</value>
    /// <remarks>
    /// <para>
    /// A final state is only relevant if this state collection contains the sub-states of another 
    /// state and the parent state has an event-less transition (= a transition where 
    /// <see cref="Transition.FireAlways"/> is set). An event-less transition usually fires
    /// immediately. But when a final state is set, the event-less transition can only fire when the
    /// final state of the sub-states is active.
    /// </para>
    /// <para>
    /// If a state has parallel sub-states with multiple final states, then all final states must be 
    /// active before any event-less transitions can fire.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public State FinalState
    {
      get
      {
        Debug.Assert(_finalState == null || Contains(_finalState), "The final state is not part of the StateCollection.");

        return _finalState;
      }
      set
      {
        if (_finalState == value)
          return;

        if (value != null && !Contains(value))
          throw new InvalidOperationException("The final state must be contained in the StateCollection.");

        _finalState = value;
      }
    }
    private State _finalState;


    /// <summary>
    /// Gets or sets a value indicating whether this state collection has a "history state".
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the collection saves its history; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// When a state collection is re-entered, the initial state becomes active. But if this 
    /// property is set to <see langword="true"/>, the state collection remembers its history and on
    /// re-enter the last active state is reactivated. This functionality implements the "history
    /// state" of standard UML state machine.
    /// </remarks>
    public bool SaveHistory { get; set; }


    /// <summary>
    /// Gets the active state.
    /// </summary>
    /// <value>The active state - or <see langword="null"/> if no state is active.</value>
    public State ActiveState
    {
      get { return _activeState; }
      private set { _activeState = value; }
    }
    private State _activeState;


    /// <summary>
    /// Gets or sets the last active state.
    /// </summary>
    /// <value>The last active state.</value>
    /// <remarks>
    /// This state is used as the initial state if the state collection is re-entered and 
    /// <see cref="SaveHistory"/> is enabled.
    /// </remarks>
    private State LastActiveState
    {
      get { return _lastActiveState; }
      set { _lastActiveState = value; }
    }
    private State _lastActiveState;


    /// <summary>
    /// Gets the state that owns this state collection.
    /// </summary>
    /// <value>
    /// The owner state - or <see langword="null"/> if the owner is the state machine itself.
    /// </value>
    public State Owner { get; internal set; }


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

    //    foreach (var state in Items)
    //      state.StateMachine = value;
    //  }
    //}
    //private StateMachine _stateMachine;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="StateCollection"/> class.
    /// </summary>
    public StateCollection() : base(StringComparer.Ordinal, DictionaryCreationThreshold)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Determines whether the specified state is directly or indirectly contained in this 
    /// collection.
    /// </summary>
    internal bool ContainsRecursive(State subState)
    {
      while (subState != null)
      {
        Debug.Assert(subState.Owner != null, "Owner of state is not properly set. (Owner is null.)");
        if (subState.Owner == this)
          return true;

        subState = subState.Owner.Owner;
      }

      return false;
    }


    /// <summary>
    /// Gets the child that is or contains the given state.
    /// </summary>
    /// <param name="state">The state or sub-state.</param>
    /// <returns>
    /// The child that is or contains <paramref name="state"/>.
    /// </returns>
    internal State GetChild(State state)
    {
      while (state != null)
      {
        if (state.Owner == this)
          return state;

        state = state.Owner.Owner;
      }

      return null;
    }



    /// <summary>
    /// Removes all elements from the <see cref="Collection{T}"/>.
    /// </summary>
    protected override void ClearItems()
    {
      // Reset Owner and StateMachine properties.
      foreach (State state in this)
      {
        state.Owner = null;
        //state.StateMachine = null;
      }

      // Reset special state references.
      _initialState = null;
      _activeState = null;
      _lastActiveState = null;
      _finalState = null;
      
      base.ClearItems();
    }


    /// <summary>
    /// Inserts an element into the <see cref="Collection{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">
    /// The object to insert. The value can be <see langword="null"/> for reference types.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is already contained in the collection. Duplicate states are not 
    /// allowed in the collection.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero. Or <paramref name="index"/> is greater than 
    /// <see cref="Collection{T}.Count"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override void InsertItem(int index, State item)
    {
      if (ReferenceEquals(item, null))
        throw new ArgumentNullException("item");
      if (Contains(item))
        throw new ArgumentException("Duplicate states are not allowed in the collection.");

      //if (item.StateMachine != null)
      //  throw new InvalidOperationException("The state is already part of a different state machine.");
      if (item.Owner != null && item.Owner != this)
        throw new InvalidOperationException("The state is already a part of a different StateCollection.");

      // Set Owner and StateMachine.
      item.Owner = this;
      //item.StateMachine = StateMachine;

      base.InsertItem(index, item);
    }


    /// <summary>
    /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero. Or <paramref name="index"/> is equal to or 
    /// greater than <see cref="Collection{T}.Count"/>.
    /// </exception>
    protected override void RemoveItem(int index)
    {
      State removedObject = this[index];

      // Update state references.
      if (_initialState == removedObject)
        _initialState = null;
      if (_finalState == removedObject)
        _finalState = null;
      if (_activeState == removedObject)
        _activeState = null;
      if (_lastActiveState == removedObject)
        _lastActiveState = null;

      // Unset Owner and StateMachine properties.
      removedObject.Owner = null;
      //removedObject.StateMachine = null;

      base.RemoveItem(index);
    }


    /// <summary>
    /// Replaces the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="item">
    /// The new value for the element at the specified index. The value can be 
    /// <see langword="null"/> for reference types.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is already contained in the collection. Duplicate states are not 
    /// allowed in the collection.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero.
    /// Or <paramref name="index"/> is greater than <see cref="Collection{T}.Count"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override void SetItem(int index, State item)
    {
      State removedObject = this[index];
      if (ReferenceEquals(item, removedObject))
        return;
      if (ReferenceEquals(item, null))
        throw new ArgumentNullException("item");
      if (Contains(item))
        throw new ArgumentException("Duplicate states are not allowed in the collection.");

      //if (item.StateMachine != null)
      //  throw new InvalidOperationException("The state is already part of a different state machine.");
      if (item.Owner != null && item.Owner != this)
        throw new InvalidOperationException("The state is already a part of a different StateCollection.");

      // Update state references.
      if (_initialState == removedObject)
        _initialState = null;
      if (_finalState == removedObject)
        _finalState = null;
      if (_activeState == removedObject)
        _activeState = null;
      if (_lastActiveState == removedObject)
        _lastActiveState = null;

      // Set/unset Owner and StateMachine properties.
      removedObject.Owner = null;
      //removedObject.StateMachine = null;
      item.Owner = this;
      //item.StateMachine = null;

      base.SetItem(index, item);
    }


    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="StateCollection"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="StateCollection"/>.
    /// </returns>
    public new List<State>.Enumerator GetEnumerator()
    {
      return ((List<State>)Items).GetEnumerator();
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
    internal Transition UpdateTransitions(StateEventArgs eventArgs)
    {
      if (ActiveState == null)
      {
        // No state active. --> Transition into a state.
        // This happens at the first step of a state machine.
        EnterState(null, eventArgs);
        return null;
      }
      
      // Check transitions of active state.
      return ActiveState.UpdateTransitions(eventArgs);
    }


    /// <summary>
    /// Enters a new state.
    /// </summary>
    /// <param name="transition">The transition. (Can be <see langword="null"/>.)</param>
    /// <param name="eventArgs">
    /// The <see cref="StateEventArgs"/> instance containing the required data.
    /// </param>
    internal void EnterState(Transition transition, StateEventArgs eventArgs)
    {
      if (Count == 0)
      {
        // Abort, nothing to do.
        return;
      }

      // Current active state (can be null).
      var newActiveState = ActiveState;

      if (transition != null && ContainsRecursive(transition.TargetState))
      {
        // The target state is a state or sub-state.
        // The new active state must be the state that contains the target state.
        newActiveState = GetChild(transition.TargetState);
        Debug.Assert(newActiveState != null, "New active state not found.");
      }

      if (newActiveState == null)
      {
        // We don't know which state to enter - enter history/initial state.
        if (SaveHistory && LastActiveState != null)
          newActiveState = LastActiveState;
        else
          newActiveState = InitialState;
      }

      // Enter the new state.
      newActiveState.EnterState(transition, eventArgs);

      // Now that state was entered we remember it as the active state.
      ActiveState = newActiveState;
      LastActiveState = newActiveState;
    }


    /// <summary>
    /// Updates the active state.
    /// </summary>
    /// <param name="eventArgs">
    /// The <see cref="StateEventArgs"/> instance containing the required data.
    /// </param>
    internal void UpdateState(StateEventArgs eventArgs)
    {
      if (Count == 0)
        return;   // Nothing to do.

      Debug.Assert(ActiveState != null, "Cannot update state. Active state is not set.");
      Debug.Assert(LastActiveState != null, "Last active state is not set.");

      ActiveState.UpdateState(eventArgs);
    }


    /// <summary>
    /// Exits to the specified target state.
    /// </summary>
    /// <param name="transition">The firing transition.</param>
    /// <param name="eventArgs">
    /// The <see cref="StateEventArgs"/> instance containing the required data.
    /// </param>
    internal void ExitState(Transition transition, StateEventArgs eventArgs)
    {
      if (Count == 0 || ActiveState == null)
        return;     // Nothing to do.

      // Call exit on current state. The method will call true if the
      // state was exited. If the ActiveState contains the target state,
      // the method will return false and the ActiveState will still be active.
      bool exited = ActiveState.ExitState(transition, eventArgs);
      if (exited)
        ActiveState = null;
    }


    /// <summary>
    /// Gets the collection that contains the two given states.
    /// </summary>
    internal static StateCollection GetCollection(State stateA, State stateB)
    {
      //Debug.Assert(stateA.StateMachine == stateB.StateMachine, "States are not in the same state machine.");

      if (stateA == stateB)          // Are states identical?
        return stateA.Owner;
      if (stateA.Contains(stateB))   // Is A parent of B?
        return stateA.Owner;
      if (stateB.Contains(stateA))   // Is B parent of A?
        return stateB.Owner;
      if (stateA.Owner.ContainsRecursive(stateB))     // Is B a sibling of A?
        return stateA.Owner;
      if (stateB.Owner.ContainsRecursive(stateA))     // Is A a sibling of B?
        return stateB.Owner;

      Debug.Assert(stateA.Owner.Owner != null);
      Debug.Assert(stateB.Owner.Owner != null);

      return GetCollection(stateA.Owner.Owner, stateB.Owner.Owner);
    }
    #endregion
  }
}
