// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Undo
{
  /// <summary>
  /// Represents an operation that supports Undo/Redo.
  /// </summary>
  public interface IUndoableOperation
  {
    /// <summary>
    /// Gets the description of the operation.
    /// </summary>
    /// <value>The description of the operation.</value>
    /// <remarks>
    /// The description is an object that identifies the operation that is performed. The object is 
    /// typically a static or dynamically generated string (such as "Insert 'abc'", "Backspace",
    /// etc.). The object can be listed in the drop-down menu of an Undo or Redo button.
    /// </remarks>
    object Description { get; }


    /// <summary>
    /// Undoes operation.
    /// </summary>
    void Undo();


    /// <summary>
    /// Performs/Redoes operation.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Do")]
    void Do();
  }
}
