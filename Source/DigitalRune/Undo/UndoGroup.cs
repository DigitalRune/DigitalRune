// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;


namespace DigitalRune.Undo
{
  /// <summary>
  /// Groups the last <i>n</i> operation into one operation that can be undone with a single Undo
  /// command.
  /// </summary>
  internal sealed class UndoGroup : IUndoableOperation
  {
    private readonly List<IUndoableOperation> _undoList = new List<IUndoableOperation>();


    /// <summary>
    /// Gets or sets the description of the operation.
    /// </summary>
    /// <value>The description of the operation.</value>
    public object Description { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="UndoGroup"/> class.
    /// </summary>
    /// <param name="undoStack">The stack of undo operations.</param>
    /// <param name="numberOfOperations">The number of operations to combine.</param>
    /// <param name="description">The description of the operation.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="undoStack"/> is <see langword="null"/>.
    /// </exception>
    public UndoGroup(Deque<IUndoableOperation> undoStack, int numberOfOperations, object description)
    {
      if (undoStack == null)
        throw new ArgumentNullException("undoStack");

      Description = description;

      Debug.Assert(numberOfOperations > 0, "numberOfOperations should be greater than 0.");
      if (numberOfOperations > undoStack.Count)
        numberOfOperations = undoStack.Count;

      for (int i = 0; i < numberOfOperations; ++i)
        _undoList.Add(undoStack.DequeueHead());
    }


    /// <summary>
    /// Undoes the operation.
    /// </summary>
    public void Undo()
    {
      for (int i = 0; i < _undoList.Count; ++i)
        _undoList[i].Undo();
    }


    /// <summary>
    /// Redoes the operation.
    /// </summary>
    public void Do()
    {
      for (int i = _undoList.Count - 1; i >= 0; --i)
        _undoList[i].Do();
    }
  }
}
