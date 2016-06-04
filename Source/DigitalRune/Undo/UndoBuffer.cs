// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using DigitalRune.Collections;


namespace DigitalRune.Undo
{
  /// <summary>
  /// Implements an undo/redo buffer.
  /// </summary>
  public class UndoBuffer : INotifyPropertyChanged
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private int _groupDepth;
    private int _numberOfOperationsInGroup;
    #endregion


    //--------------------------------------------------------------
    #region Events
    //--------------------------------------------------------------

    /// <summary>
    /// Occurs after an operation is undone.
    /// </summary>
    public event EventHandler<EventArgs> OperationUndone;


    /// <summary>
    /// Occurs after an operation is redone.
    /// </summary>
    public event EventHandler<EventArgs> OperationRedone;


    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="UndoBuffer"/> accepts changes.
    /// </summary>
    /// <value><see langword="true"/> if this <see cref="UndoBuffer"/> accepts changes; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// This property can be used to disable the <see cref="UndoBuffer"/> temporarily while undoing
    /// an operation. Any operations added (see <see cref="Add"/>) while this property is 
    /// <see langword="false"/> will be ignored and will not be recorded in the undo buffer.
    /// </remarks>
    /// <seealso cref="Add"/>
    public bool AcceptChanges
    {
      get { return _acceptChanges; }
      set
      {
        if (_acceptChanges != value)
        {
          _acceptChanges = value;
          OnPropertyChanged(new PropertyChangedEventArgs("AcceptChanges"));
        }
      }
    }
    private bool _acceptChanges = true;


    /// <summary>
    /// Gets a value indicating whether there are operations on the undo stack.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if an operation can be undone; otherwise, <see langword="false"/>.
    /// </value>
    public bool CanUndo
    {
      get { return InternalUndoStack.Count > 0; }
    }


    /// <summary>
    /// Gets a value indicating whether there are operations on the redo stack.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if an operation can be redone; otherwise, <see langword="false"/>.
    /// </value>
    public bool CanRedo
    {
      get { return InternalRedoStack.Count > 0; }
    }


    /// <summary>
    /// Gets a value indicating whether an undo group is open.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if an undo group open; otherwise, <see langword="false"/>.
    /// </value>
    /// <seealso cref="BeginUndoGroup"/>
    /// <seealso cref="EndUndoGroup"/>
    public bool IsUndoGroupOpen
    {
      get { return _groupDepth != 0; }
    }


    /// <summary>
    /// Gets or sets the max number of undo steps stored in the undo buffer.
    /// </summary>
    /// <value>
    /// The max number of undo steps stored in the undo buffer. The default value is 
    /// <see cref="Int32.MaxValue"/>.
    /// </value>
    /// <remarks>
    /// Multiple operations grouped in an undo group by using 
    /// <see cref="BeginUndoGroup"/>/<see cref="EndUndoGroup"/> 
    /// count as a single undo step.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public int SizeLimit
    {
      get { return _sizeLimit; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value", "UndoBuffer.SizeLimit must not be negative.");

        if (_sizeLimit != value)
        {
          _sizeLimit = value;
          OnPropertyChanged(new PropertyChangedEventArgs("SizeLimit"));
          if (!IsUndoGroupOpen)
            EnforceSizeLimit();
        }
      }
    }
    private int _sizeLimit = int.MaxValue;


    /// <summary>
    /// Gets or sets the undo stack.
    /// </summary>
    /// <value>The undo stack.</value>
    /// <remarks>
    /// <para>
    /// The undo stack contains the operations in last-in-first-out order: The first element (see 
    /// <see cref="Deque{T}.Head"/>) contains the operation that was added last and that needs to be
    /// undone first.
    /// </para>
    /// <para>
    /// The number of operations stored in the undo stack can be limited by setting the 
    /// <see cref="SizeLimit"/>. If the number of items in the undo stack exceed the limit, the 
    /// oldest operations are removed from the end of the undo stack (<see cref="Deque{T}.Tail"/>).
    /// </para>
    /// </remarks>
    private Deque<IUndoableOperation> InternalUndoStack { get; set; }


    /// <summary>
    /// Gets or sets the redo stack.
    /// </summary>
    /// <value>The redo stack.</value>
    /// <remarks>
    /// <para>
    /// The redo stack contains the operations in last-in-first-out order: The most-recently undone 
    /// operation is added at the beginning of the redo stack (see <see cref="Deque{T}.Head"/>). 
    /// When operation should be redone they need to be processed in the order from 
    /// <see cref="Deque{T}.Head"/> to <see cref="Deque{T}.Tail"/>.
    /// </para>
    /// <para>
    /// The redo stack is cleared every time a new operation is undone.
    /// </para>
    /// <para>
    /// The number of operations stored in the redo stack can be limited by setting the 
    /// <see cref="SizeLimit"/>. If the number of items in the redo stack exceed the limit, the 
    /// oldest operations are removed from the end of the redo stack (<see cref="Deque{T}.Tail"/>).
    /// </para>
    /// </remarks>
    private Deque<IUndoableOperation> InternalRedoStack { get; set; }


    /// <summary>
    /// Gets the undo stack.
    /// </summary>
    /// <value>The undo stack.</value>
    /// <remarks>
    /// <para>
    /// The undo stack contains the operations in last-in-first-out order: The first element
    /// is the operation that was executed last and that needs to be undone first.
    /// </para>
    /// <para>
    /// This is a read-only collection and should only be used to read information about the most 
    /// recently executed operations. The operations on the undo stack should not be manipulated
    /// directly. Call <see cref="Undo"/> or <see cref="Redo"/> to undo or redo operations.
    /// </para>
    /// </remarks>
    public ReadOnlyCollection<IUndoableOperation> UndoStack { get; private set; }


    /// <summary>
    /// Gets the redo stack.
    /// </summary>
    /// <value>The redo stack.</value>
    /// <remarks>
    /// <para>
    /// The redo stack contains the operations in last-in-first-out order: The most-recently undone 
    /// operation is at the begin of the redo stack.
    /// </para>
    /// <para>
    /// The redo stack is cleared every time a new operation is undone.
    /// </para>
    /// <para>
    /// This is a read-only collection and should only be used to read information about the most 
    /// recently undone operations. The operations on the redo stack should not be manipulated
    /// directly. Call <see cref="Undo"/> or <see cref="Redo"/> to undo or redo operations.
    /// </para>
    /// </remarks>
    public ReadOnlyCollection<IUndoableOperation> RedoStack { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="UndoBuffer"/> class.
    /// </summary>
    public UndoBuffer()
    {
      InternalUndoStack = new Deque<IUndoableOperation>();
      InternalRedoStack = new Deque<IUndoableOperation>();
      UndoStack = new ReadOnlyCollection<IUndoableOperation>(InternalUndoStack);
      RedoStack = new ReadOnlyCollection<IUndoableOperation>(InternalRedoStack);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Begins a new undo group.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An undo group is a group of operations that is combined into a single undo operation.
    /// </para>
    /// <para>
    /// Undo groups can be nested: New undo groups can be started within other undo groups. The
    /// number of <see cref="EndUndoGroup"/> calls need to match the number of 
    /// <see cref="BeginUndoGroup"/> calls. All operations between the outer 
    /// <see cref="BeginUndoGroup"/> and <see cref="EndUndoGroup"/> are combined and pushed onto the
    /// undo stack.
    /// </para>
    /// </remarks>
    /// <seealso cref="EndUndoGroup"/>
    /// <seealso cref="IsUndoGroupOpen"/>
    public void BeginUndoGroup()
    {
      if (_groupDepth == 0)
      {
        _numberOfOperationsInGroup = 0;
        OnPropertyChanged(new PropertyChangedEventArgs("IsUndoGroupOpen"));
      }

      _groupDepth++;
    }


    /// <summary>
    /// Ends an undo group and puts the group of operations onto the <see cref="UndoBuffer"/>.
    /// </summary>
    /// <param name="groupDescription">
    /// The description of the undo group. See <see cref="IUndoableOperation.Description"/> for a
    /// more detailed description. (The parameter is ignored if this is a nested undo group.)
    /// </param>
    /// <remarks>
    /// An undo group is a group of operations that is combined into a single
    /// undo operation.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// There are no undo groups. (<see cref="BeginUndoGroup"/> has not been called.)
    /// </exception>
    /// <seealso cref="BeginUndoGroup"/>
    /// <seealso cref="IsUndoGroupOpen"/>
    public void EndUndoGroup(object groupDescription)
    {
      if (_groupDepth == 0)
        throw new InvalidOperationException("There are no open undo groups.");

      _groupDepth--;
      if (_groupDepth == 0 && _numberOfOperationsInGroup > 1)
      {
        Add(new UndoGroup(InternalUndoStack, _numberOfOperationsInGroup, groupDescription));
        OnPropertyChanged(new PropertyChangedEventArgs("IsUndoGroupOpen"));
      }
    }


    /// <summary>
    /// Checks that no undo groups are open.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// An undo group is open.
    /// </exception>
    private void AssertNoUndoGroupOpen()
    {
      if (IsUndoGroupOpen)
      {
        _groupDepth = 0;
        throw new InvalidOperationException("Cannot Undo/Redo while undo group is open.");
      }
    }


    void EnforceSizeLimit()
    {
      AssertNoUndoGroupOpen();
      while (InternalUndoStack.Count > _sizeLimit)
        InternalUndoStack.DequeueTail();

      while (InternalRedoStack.Count > _sizeLimit)
        InternalRedoStack.DequeueTail();
    }


    /// <summary>
    /// Undoes the last operation.
    /// </summary>
    public void Undo()
    {
      AssertNoUndoGroupOpen();
      if (InternalUndoStack.Count > 0)
      {
        IUndoableOperation operation = InternalUndoStack.DequeueHead();
        InternalRedoStack.EnqueueHead(operation);
        operation.Undo();
        OnOperationUndone();

        if (InternalUndoStack.Count == 0)
          OnPropertyChanged(new PropertyChangedEventArgs("CanUndo"));

        if (InternalRedoStack.Count == 1)
          OnPropertyChanged(new PropertyChangedEventArgs("CanRedo"));

        EnforceSizeLimit();
      }
    }


    /// <summary>
    /// Redoes the last undone operation.
    /// </summary>
    public void Redo()
    {
      AssertNoUndoGroupOpen();
      if (InternalRedoStack.Count > 0)
      {
        IUndoableOperation operation = InternalRedoStack.DequeueHead();
        InternalUndoStack.EnqueueHead(operation);
        operation.Do();
        OnOperationRedone();

        if (InternalRedoStack.Count == 0)
          OnPropertyChanged(new PropertyChangedEventArgs("CanRedo"));

        if (InternalUndoStack.Count == 1)
          OnPropertyChanged(new PropertyChangedEventArgs("CanUndo"));

        EnforceSizeLimit();
      }
    }


    /// <summary>
    /// Adds an operation to the undo buffer.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <remarks>
    /// <para>
    /// This methods needs to be called for every <see cref="IUndoableOperation"/> before or after
    /// it is executed by calling its <see cref="IUndoableOperation.Do"/> method. Note: 
    /// <see cref="Add"/> does not execute the <see cref="IUndoableOperation"/> it will only record
    /// the operation.
    /// </para>
    /// <para>
    /// After the operation is recorded in the undo buffer it can be undone and redone by calling
    /// <see cref="Undo"/> and <see cref="Redo"/>.
    /// </para>
    /// <para>
    /// The recording of operations in the undo buffer can be temporarily disabled by setting
    /// <see cref="AcceptChanges"/> to <see langword="false"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="operation"/> is <see langword="null"/>.
    /// </exception>
    /// <seealso cref="AcceptChanges"/>
    public void Add(IUndoableOperation operation)
    {
      if (operation == null)
        throw new ArgumentNullException("operation");

      if (AcceptChanges)
      {
        InternalUndoStack.EnqueueHead(operation);

        if (IsUndoGroupOpen)
          _numberOfOperationsInGroup++;

        if (InternalUndoStack.Count == 1)
          OnPropertyChanged(new PropertyChangedEventArgs("CanUndo"));

        ClearRedoStack();

        if (!IsUndoGroupOpen)
          EnforceSizeLimit();
      }
    }


    /// <summary>
    /// Clears the undo buffer.
    /// </summary>
    public void ClearAll()
    {
      AssertNoUndoGroupOpen();
      ClearRedoStack();
      ClearUndoStack();
      _numberOfOperationsInGroup = 0;
    }


    private void ClearRedoStack()
    {
      if (InternalRedoStack.Count > 0)
      {
        InternalRedoStack.Clear();
        OnPropertyChanged(new PropertyChangedEventArgs("CanRedo"));
      }
    }


    private void ClearUndoStack()
    {
      if (InternalUndoStack.Count > 0)
      {
        InternalUndoStack.Clear();
        OnPropertyChanged(new PropertyChangedEventArgs("CanUndo"));
      }
    }


    /// <summary>
    /// Raises the <see cref="OperationUndone"/> event.
    /// </summary>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OperationUndone"/> 
    /// in a derived class, be sure to call the base class's <see cref="OperationUndone"/> 
    /// method so that registered delegates receive the event.
    /// </remarks>
    protected void OnOperationUndone()
    {
      var handler = OperationUndone;

      if (handler != null)
        handler(this, EventArgs.Empty);
    }


    /// <summary>
    /// Raises the <see cref="OperationRedone"/> event.
    /// </summary>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OperationRedone"/> 
    /// in a derived class, be sure to call the base class's <see cref="OperationRedone"/> 
    /// method so that registered delegates receive the event.
    /// </remarks>
    protected void OnOperationRedone()
    {
      var handler = OperationRedone;

      if (handler != null)
        handler(this, EventArgs.Empty);
    }


    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// The <see cref="PropertyChangedEventArgs"/> describing the property that has changed.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnPropertyChanged"/> 
    /// in a derived class, be sure to call the base class's <see cref="OnPropertyChanged"/> 
    /// method so that registered delegates receive the event.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="eventArgs"/> is <see langword="null"/>.
    /// </exception>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
    {
      if (eventArgs == null)
        throw new ArgumentNullException("eventArgs");

      var handler = PropertyChanged;

      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
