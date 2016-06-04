// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Game.Input;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Game.UI.Controls
{
  public partial class TextBox
  {
#if !WP7 && !XBOX
    /// <summary>
    /// Inserts a newline, but only for multiline text boxes and if <see cref="MaxLength"/> is not 
    /// yet reached.
    /// </summary>
    private void Enter()
    {
      if (IsReadOnly)
        return;

      DeleteSelection();

      string text = Text ?? string.Empty;
      if (IsMultiline && text.Length < MaxLength)
      {
        Text = text.Insert(CaretIndex, "\n");
        CaretIndex++;
      }
    }


    /// <summary>
    /// Deletes the current selection or the previous character.
    /// </summary>
    private void Backspace()
    {
      if (IsReadOnly)
        return;

      if (_selectionStart < 0)
      {
        // No selection: Delete previous character.
        string text = Text ?? string.Empty;
        if (text.Length > 0 && CaretIndex > 0)
        {
          Text = text.Remove(CaretIndex - 1, 1);
          CaretIndex--;
        }
      }
      else
      {
        // Text selected.
        DeleteSelection();
      }
    }


    /// <summary>
    /// Deletes the current selection or the current character.
    /// </summary>
    private void Delete()
    {
      if (IsReadOnly)
        return;

      if (_selectionStart < 0)
      {
        // No selection: Delete current character.
        string text = Text ?? string.Empty;
        if (CaretIndex < text.Length)
        {
          Text = text.Remove(CaretIndex, 1);
        }
      }
      else
      {
        // Text selected.
        DeleteSelection();
      }
    }


    /// <summary>
    /// Moves the caret left.
    /// </summary>
    /// <param name="modifierKeys">The modifier keys.</param>
    private void MoveLeft(ModifierKeys modifierKeys)
    {
      if ((modifierKeys & ModifierKeys.Shift) != 0)
      {
        // Start or append selection.
        if (_selectionStart < 0)
          _selectionStart = CaretIndex;
      }
      else if (_selectionStart >= 0)
      {
        // Clear selection.
        CaretIndex = Math.Min(_selectionStart, CaretIndex);
        _selectionStart = -1;
        InvalidateArrange();
        return;
      }
 
      if (!IsPassword && (modifierKeys & ModifierKeys.Control) != 0)
      {
        // Skip word.
        if (CaretIndex > 0)
        {
          string text = Text ?? string.Empty;
          if (char.IsWhiteSpace(text[CaretIndex - 1]))
            while (CaretIndex > 0 && char.IsWhiteSpace(text[CaretIndex - 1]))
              CaretIndex--;
          else
            while (CaretIndex > 0 && !char.IsWhiteSpace(text[CaretIndex - 1]))
              CaretIndex--;
        }
      }
      else
      {
        // Skip character.
        CaretIndex--;
      }
    }


    /// <summary>
    /// Moves the caret right.
    /// </summary>
    /// <param name="modifierKeys">The modifier keys.</param>
    private void MoveRight(ModifierKeys modifierKeys)
    {
      if ((modifierKeys & ModifierKeys.Shift) != 0)
      {
        // Start or append selection.
        if (_selectionStart < 0)
          _selectionStart = CaretIndex;
      }
      else if (_selectionStart >= 0)
      {
        // Clear selection.
        CaretIndex = Math.Max(_selectionStart, CaretIndex);
        _selectionStart = -1;
        InvalidateArrange();
        return;
      }

      if (!IsPassword && (modifierKeys & ModifierKeys.Control) != 0)
      {
        // Skip word.
        string text = Text ?? string.Empty;
        if (CaretIndex < text.Length)
        {
          if (char.IsWhiteSpace(text[CaretIndex]))
            while (CaretIndex < text.Length && char.IsWhiteSpace(text[CaretIndex]))
              CaretIndex++;
          else
            while (CaretIndex < text.Length && !char.IsWhiteSpace(text[CaretIndex]))
              CaretIndex++;
        }
      }
      else
      {
        // Skip character.
        CaretIndex++;
      }
    }


    /// <summary>
    /// Moves the caret up by one line.
    /// </summary>
    /// <param name="modifierKeys">The modifier keys.</param>
    private void MoveUp(ModifierKeys modifierKeys)
    {
      StartSelection(modifierKeys);

      var screen = Screen;
      var font = screen.Renderer.GetFont(Font);
      int lineHeight = font.LineSpacing;
      CaretIndex = GetIndex(VisualCaret - new Vector2F(0, lineHeight), screen);
    }


    /// <summary>
    /// Moves the caret down by one line.
    /// </summary>
    /// <param name="modifierKeys">The modifier keys.</param>
    private void MoveDown(ModifierKeys modifierKeys)
    {
      StartSelection(modifierKeys);

      var screen = Screen;
      var font = screen.Renderer.GetFont(Font);
      int lineHeight = font.LineSpacing;
      CaretIndex = GetIndex(VisualCaret + new Vector2F(0, lineHeight), screen);
    }


    /// <summary>
    /// Moves the caret to start of line or start of whole text.
    /// </summary>
    /// <param name="modifierKeys">The modifier keys.</param>
    private void Home(ModifierKeys modifierKeys)
    {
      StartSelection(modifierKeys);

      if (!IsMultiline || (modifierKeys & ModifierKeys.Control) != 0)
        CaretIndex = 0;
      else
        CaretIndex = Math.Abs(_lineStarts[GetLine(CaretIndex)]);
    }


    /// <summary>
    /// Moves the caret to end of line or end of whole text.
    /// </summary>
    /// <param name="modifierKeys">The modifier keys.</param>
    private void End(ModifierKeys modifierKeys)
    {
      StartSelection(modifierKeys);

      string text = Text ?? string.Empty;
      if (!IsMultiline || (modifierKeys & ModifierKeys.Control) != 0)
      {
        CaretIndex = text.Length;
      }
      else
      {
        int line = GetLine(CaretIndex);
        CaretIndex = (line + 1 < _lineStarts.Count) ? Math.Abs(_lineStarts[line + 1]) : text.Length;
        if (CaretIndex > 0 && text[CaretIndex - 1] == '\n')
          CaretIndex--;
      }
    }


    /// <summary>
    /// Moves the caret one page up.
    /// </summary>
    /// <param name="modifierKeys">The modifier keys.</param>
    private void PageUp(ModifierKeys modifierKeys)
    {
      StartSelection(modifierKeys);

      if (IsMultiline)
        CaretIndex = GetIndex(VisualCaret - new Vector2F(0, VisualClip.Height), Screen);
    }


    /// <summary>
    /// Moves the caret one page down.
    /// </summary>
    /// <param name="modifierKeys">The modifier keys.</param>
    private void PageDown(ModifierKeys modifierKeys)
    {
      StartSelection(modifierKeys);

      if (IsMultiline)
        CaretIndex = GetIndex(VisualCaret + new Vector2F(0, VisualClip.Height), Screen);
    }
#endif


    /// <summary>
    /// Clears the text selection.
    /// </summary>
    public void ClearSelection()
    {
      _selectionStart = -1;
      InvalidateArrange();
    }


    /// <summary>
    /// Selects a range of text in the text box.
    /// </summary>
    /// <param name="start">
    /// The zero-based index of the first character in the selection.
    /// </param>
    /// <param name="length">
    /// The length of the selection, in characters.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The selection defined by <paramref name="start"/> and <paramref name="length"/> is invalid.
    /// </exception>
    public void Select(int start, int length)
    {
      if (start < 0)
        throw new ArgumentOutOfRangeException("start", "The start index of the selection must not be negative.");
      if (length < 0)
        throw new ArgumentOutOfRangeException("length", "The length of the selection must not be negative.");

      // Clear selection if length is 0.
      if (length == 0)
      {
        _selectionStart = -1;
        return;
      }

      string text = Text ?? string.Empty;
      int end = start + length;
      if (end > text.Length)
        throw new ArgumentOutOfRangeException(null, "The selection exceeds the content of the text box.");

      _selectionStart = start;
      _caretIndex = end;

      // Scroll to the selection.
      _bringCaretIntoView = true;

      InvalidateArrange();
    }


    /// <summary>
    /// Selects all contents of the text box.
    /// </summary>
    public void SelectAll()
    {
      string text = Text ?? string.Empty;
      _selectionStart = 0;
      _caretIndex = text.Length;

      // Do not bring caret into view. This happens automatically when the user
      // moves the caret.

      InvalidateArrange();
    }


    /// <summary>
    /// Removes the current selection from the text box and copies it to the clipboard.
    /// </summary>
    public void Cut()
    {
      if (IsPassword)
        return;

      if (IsReadOnly)
        return;

      string selectedText = SelectedText;
      if (string.IsNullOrEmpty(selectedText))
        return;

      DeleteSelection();

      if (PlatformHelper.IsClipboardSupported)
        PlatformHelper.SetClipboardText(selectedText);
      else
        ClipboardData = selectedText;
    }


    /// <summary>
    /// Copies the current selection from the text box to the clipboard.
    /// </summary>
    public void Copy()
    {
      if (IsPassword)
        return;

      string selectedText = SelectedText;
      if (string.IsNullOrEmpty(selectedText))
        return;

      if (PlatformHelper.IsClipboardSupported)
        PlatformHelper.SetClipboardText(selectedText);
      else
        ClipboardData = selectedText;
    }


    /// <summary>
    /// Pastes the contents of the clipboard into the text box.
    /// </summary>
    public void Paste()
    {
      if (IsReadOnly)
        return;

      DeleteSelection();

      string text = Text ?? string.Empty;

      string data = null;
      if (PlatformHelper.IsClipboardSupported)
        data = PlatformHelper.GetClipboardText();
      else
        data = ClipboardData;

      if (!IsMultiline)
        data = data.Replace("\n", string.Empty);

      Text = text.Insert(CaretIndex, data);
      CaretIndex += data.Length;
    }


#if !WP7 && !XBOX
    /// <summary>
    /// Selects the word or white-space at the given index.
    /// </summary>
    /// <param name="index">The zero-based index of a character in <see cref="Text"/>.</param>
    private void SelectWordOrWhiteSpace(int index)
    {
      if (IsPassword)
      {
        SelectAll();
        return;
      }

      string text = Text ?? string.Empty;
      if (text.Length == 0)
        return;

      Debug.Assert(0 <= index && index <= text.Length, "Invalid index.");

      // The character at the specified index determines whether to select a word 
      // or white-space.
      bool selectWhiteSpace;
      if (index == text.Length)
      {
        // Caret is placed at end of text.
        selectWhiteSpace = char.IsWhiteSpace(text[text.Length - 1]);
      }
      else
      {
        // Caret is placed at a character.
        selectWhiteSpace = char.IsWhiteSpace(text[index]);
      }

      // Find start of word or white-space.
      int startIndex = index;
      while (startIndex > 0 && char.IsWhiteSpace(text[startIndex - 1]) == selectWhiteSpace)
        startIndex--;

      // Find end of word or white-space.
      int endIndex = index + 1;
      while (endIndex < text.Length && char.IsWhiteSpace(text[endIndex]) == selectWhiteSpace)
        endIndex++;

      _selectionStart = startIndex;
      CaretIndex = endIndex;
    }

    
    /// <summary>
    /// Starts, appends or clears the selection depending on the currently pressed modifier keys.
    /// </summary>
    /// <param name="modifierKeys">The modifier keys.</param>
    private void StartSelection(ModifierKeys modifierKeys)
    {
      if ((modifierKeys & ModifierKeys.Shift) != 0)
      {
        // Start or append selection.
        if (_selectionStart < 0)
          _selectionStart = CaretIndex;
      }
      else
      {
        // Clear selection.
        _selectionStart = -1;
        InvalidateArrange();
      }
    }
#endif


    /// <summary>
    /// Deletes the currently selected text.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the text has been changed; otherwise, <see langword="false"/>.
    /// </returns>
    private bool DeleteSelection()
    {
      if (_selectionStart < 0)
        return false;

      // Sort selection indices.
      int startIndex, endIndex;
      if (_selectionStart < _caretIndex)
      {
        startIndex = _selectionStart;
        endIndex = _caretIndex;
      }
      else
      {
        startIndex = _caretIndex;
        endIndex = _selectionStart;
      }

      string text = Text ?? string.Empty;
      Text = text.Remove(startIndex, endIndex - startIndex);
      CaretIndex = startIndex;
      return true;
    }
  }
}
