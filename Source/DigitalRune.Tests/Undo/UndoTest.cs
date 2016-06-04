using System;
using System.Collections.Generic;
using System.Text;
using DigitalRune.Collections;
using NUnit.Framework;


namespace DigitalRune.Undo.Tests
{
  [TestFixture]
  public class UndoTest
  {
    internal class AddCharOperation : IUndoableOperation
    {
      private readonly StringBuilder _textBuffer;
      private readonly char _c;

      
      public object Description
      {
        get { return "Add '" + _c + "'"; }
      }


      public AddCharOperation(StringBuilder textBuffer, char c)
      {
        _textBuffer = textBuffer;
        _c = c;
      }


      public void Undo()
      {
        _textBuffer.Remove(_textBuffer.Length - 1, 1);
      }


      public void Do()
      {
        _textBuffer.Append(_c);
      }
    }


    private UndoBuffer _undoBuffer;
    private StringBuilder _textBuffer;


    [SetUp]
    public void SetUp()
    {
      _undoBuffer = new UndoBuffer();
      _textBuffer = new StringBuilder();
    }


    private void AddChar(char c)
    {
      AddCharOperation addCharOperation = new AddCharOperation(_textBuffer, c);
      _undoBuffer.Add(addCharOperation);
      addCharOperation.Do();
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
	  public void OperationIsNull()
	  {
      _undoBuffer.Add(null);
    }


    [Test]
    public void EnabledTest()
    {
      Assert.IsTrue(_undoBuffer.AcceptChanges);
      _undoBuffer.AcceptChanges = false;
      Assert.IsFalse(_undoBuffer.AcceptChanges);

      AddChar('a');
      AddChar('b');
      AddChar('c');
      Assert.AreEqual("abc", _textBuffer.ToString());
      Assert.IsFalse(_undoBuffer.AcceptChanges);
      Assert.IsFalse(_undoBuffer.CanUndo);
      Assert.AreEqual(0, _undoBuffer.UndoStack.Count);

      _undoBuffer.AcceptChanges = true;

      AddChar('A');
      AddChar('B');
      AddChar('C');
      Assert.AreEqual("abcABC", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.AcceptChanges);
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.AreEqual(3, _undoBuffer.UndoStack.Count);
    }


    [Test]
    public void CanUndoRedoTest()
    {
      Assert.IsFalse(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      
      AddChar('a');
      Assert.AreEqual("a", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      
      _undoBuffer.Undo();
      Assert.AreEqual("", _textBuffer.ToString());
      Assert.IsFalse(_undoBuffer.CanUndo);
      Assert.IsTrue(_undoBuffer.CanRedo);

      _undoBuffer.Redo();
      Assert.AreEqual("a", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
    }


    [Test]
    public void UndoRedoTest()
    {
      Assert.IsFalse(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(0, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);

      AddChar('a');
      AddChar('b');
      AddChar('c');
      Assert.AreEqual("abc", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(3, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);

      _undoBuffer.Undo();
      _undoBuffer.Undo();
      Assert.AreEqual("a", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsTrue(_undoBuffer.CanRedo);
      Assert.AreEqual(1, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(2, _undoBuffer.RedoStack.Count);

      _undoBuffer.Undo();
      Assert.AreEqual("", _textBuffer.ToString());
      Assert.IsFalse(_undoBuffer.CanUndo);
      Assert.IsTrue(_undoBuffer.CanRedo);
      Assert.AreEqual(0, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(3, _undoBuffer.RedoStack.Count);

      _undoBuffer.Redo();
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsTrue(_undoBuffer.CanRedo);
      Assert.AreEqual("a", _textBuffer.ToString());
      Assert.AreEqual(1, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(2, _undoBuffer.RedoStack.Count);

      AddChar('A');
      Assert.AreEqual("aA", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(2, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);

      _undoBuffer.Undo();
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsTrue(_undoBuffer.CanRedo);
      Assert.AreEqual("a", _textBuffer.ToString());
      Assert.AreEqual(1, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(1, _undoBuffer.RedoStack.Count);

      _undoBuffer.ClearAll();
      Assert.AreEqual("a", _textBuffer.ToString());
      Assert.IsFalse(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(0, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);
    }



    [Test]
    public void UndoEvents()
    {
      bool operationUndoneRaised = false;
      bool operationRedoneRaised = false;
      _undoBuffer.OperationUndone += (sender, eventArgs) => operationUndoneRaised = true;
      _undoBuffer.OperationRedone += (sender, eventArgs) => operationRedoneRaised = true;

      AddChar('a');
      Assert.AreEqual("a", _textBuffer.ToString());
      Assert.IsFalse(operationUndoneRaised);
      Assert.IsFalse(operationRedoneRaised);

      _undoBuffer.Undo();
      Assert.IsTrue(operationUndoneRaised);
      Assert.IsFalse(operationRedoneRaised);
      operationUndoneRaised = false;

      _undoBuffer.Redo();
      Assert.IsFalse(operationUndoneRaised);
      Assert.IsTrue(operationRedoneRaised);
      operationRedoneRaised = false;
    }


    [Test]
    public void UndoGroup()
    {
      Assert.IsFalse(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(0, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);

      AddChar('a');
      Assert.AreEqual("a", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(1, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);

      _undoBuffer.BeginUndoGroup();
      AddChar('A');
      AddChar('B');
      AddChar('C');
      _undoBuffer.EndUndoGroup("Add ABC");
      Assert.AreEqual("aABC", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(2, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);

      _undoBuffer.Undo();
      Assert.AreEqual("a", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsTrue(_undoBuffer.CanRedo);
      Assert.AreEqual(1, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(1, _undoBuffer.RedoStack.Count);

      _undoBuffer.Redo();
      Assert.AreEqual("aABC", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(2, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);
    }


    [Test]
    public void NestedUndoGroups()
    {
      Assert.IsFalse(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(0, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);

      AddChar('a');
      Assert.AreEqual("a", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(1, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);

      _undoBuffer.BeginUndoGroup();
      AddChar('A');
      AddChar('B');
      AddChar('C');
      _undoBuffer.BeginUndoGroup();
      AddChar('x');
      AddChar('y');
      _undoBuffer.EndUndoGroup("Add xy");
      _undoBuffer.EndUndoGroup("Add ABC");
      Assert.AreEqual("aABCxy", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(2, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);

      _undoBuffer.Undo();
      Assert.AreEqual("a", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsTrue(_undoBuffer.CanRedo);
      Assert.AreEqual(1, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(1, _undoBuffer.RedoStack.Count);

      _undoBuffer.Redo();
      Assert.AreEqual("aABCxy", _textBuffer.ToString());
      Assert.IsTrue(_undoBuffer.CanUndo);
      Assert.IsFalse(_undoBuffer.CanRedo);
      Assert.AreEqual(2, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(0, _undoBuffer.RedoStack.Count);
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void UndoGroupMismatch()
    {
      _undoBuffer.EndUndoGroup("Invalid undo group"); 
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void UndoGroupMismatch2()
    {
      _undoBuffer.BeginUndoGroup();
      _undoBuffer.BeginUndoGroup();
      _undoBuffer.EndUndoGroup(null);
      _undoBuffer.EndUndoGroup(null);
      _undoBuffer.EndUndoGroup(null);
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void UndoWithinUndoGroup()
    {
      _undoBuffer.BeginUndoGroup();
      AddChar('a');
      _undoBuffer.Undo();
    }


    [Test]
    [ExpectedException(typeof(InvalidOperationException))]
    public void RedoWithinUndoGroup()
    {
      AddChar('a');
      _undoBuffer.Undo();
      _undoBuffer.BeginUndoGroup();
      _undoBuffer.Redo();
    }


    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void SizeLimitShouldThrowIfOutOfRange()
    {
      new UndoBuffer().SizeLimit = -1;
    }


    [Test]
    public void SizeLimit()
    {
      Assert.AreEqual(Int32.MaxValue, _undoBuffer.SizeLimit);
      _undoBuffer.SizeLimit = 1;
      Assert.AreEqual(1, _undoBuffer.SizeLimit);
      AddChar('A');
      AddChar('B');
      AddChar('C');
      Assert.AreEqual(1, _undoBuffer.UndoStack.Count);

      _undoBuffer.ClearAll();
      _undoBuffer.SizeLimit = 0;
      AddChar('A');
      Assert.AreEqual(0, _undoBuffer.UndoStack.Count);

      _undoBuffer.ClearAll();
      _undoBuffer.SizeLimit = 5;
      AddChar('A');
      AddChar('B');
      AddChar('C');
      AddChar('D');
      _undoBuffer.Undo();
      _undoBuffer.Undo();
      Assert.AreEqual(2, _undoBuffer.UndoStack.Count);
      Assert.AreEqual(2, _undoBuffer.RedoStack.Count);
      _undoBuffer.SizeLimit = 1;
      Assert.AreEqual(1, _undoBuffer.UndoStack.Count); 
      Assert.AreEqual(1, _undoBuffer.RedoStack.Count);
    }


    [Test]
    public void PropertyChanged()
    {
      List<string> propertiesChanged = new List<string>();
      _undoBuffer.PropertyChanged += (s, e) => propertiesChanged.Add(e.PropertyName);
      
      AddChar('D');
      Assert.AreEqual(1, propertiesChanged.Count);
      Assert.IsTrue(propertiesChanged.Contains(_undoBuffer.GetPropertyName(x => x.CanUndo)));

      propertiesChanged.Clear();
      _undoBuffer.Undo();
      Assert.AreEqual(2, propertiesChanged.Count);
      Assert.IsTrue(propertiesChanged.Contains(_undoBuffer.GetPropertyName(x => x.CanUndo)));
      Assert.IsTrue(propertiesChanged.Contains(_undoBuffer.GetPropertyName(x => x.CanRedo)));

      propertiesChanged.Clear();
      _undoBuffer.Redo();
      Assert.AreEqual(2, propertiesChanged.Count);
      Assert.IsTrue(propertiesChanged.Contains(_undoBuffer.GetPropertyName(x => x.CanUndo)));
      Assert.IsTrue(propertiesChanged.Contains(_undoBuffer.GetPropertyName(x => x.CanRedo)));
    }


    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void UndoGroupConstructorShouldThrowIfUndoStackIsNull()
    {
      new UndoGroup(null, 3, "Description");
    }


    [Test]
    public void UndoGroupWithWrongNumberOfOperations()
    {
      var undoStack = new Deque<IUndoableOperation>();
      _textBuffer.Append('a');
      undoStack.EnqueueHead(new AddCharOperation(_textBuffer, 'a'));
      var undoGroup = new UndoGroup(undoStack, 3, "Description");   // undoStack has 1 element but we use 3!
      undoGroup.Undo();
      Assert.AreEqual(0, _textBuffer.Length);
    }


    // Class to test UndoBuffer.OnPropertyChanged
    class MyUndoBuffer : UndoBuffer
    {
      public void Test()
      {
        OnPropertyChanged(null);
      }
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException))]
    public void UndoBufferOnPropertyChangedShouldThrowArgumentNullException()
    {
      new MyUndoBuffer().Test();
    }
  }
}