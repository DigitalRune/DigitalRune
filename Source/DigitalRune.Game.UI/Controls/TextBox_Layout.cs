// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;

#if WP7 || XBOX || PORTABLE
using DigitalRune.Text;
#endif


namespace DigitalRune.Game.UI.Controls
{
  partial class TextBox
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the offset which is the horizontal offset for single-line text boxes or the vertical
    /// offset for multi-line text boxes (see also <see cref="MaxLines"/> and 
    /// <see cref="IsMultiline"/>).
    /// </summary>
    /// <value>
    /// The offset which is the horizontal offset for single-line text boxes or the vertical offset
    /// for multi-line text boxes (see also <see cref="MaxLines"/> and <see cref="IsMultiline"/>).
    /// </value>
    public float VisualOffset { get; private set; }


    /// <summary>
    /// Gets the text exactly as it should be displayed (wrapping already applied).
    /// </summary>
    /// <value>The text, exactly as it should be displayed (wrapping already applied).</value>
    public StringBuilder VisualText { get; private set; }


    /// <summary>
    /// Gets the position of the top left corner of the caret rectangle/line.
    /// </summary>
    /// <value>The position of the top left corner of the caret rectangle/line.</value>
    public Vector2F VisualCaret { get; private set; }


    /// <summary>
    /// Gets the clipping rectangle.
    /// </summary>
    /// <value>
    /// The clipping rectangle.
    /// </value>
    public RectangleF VisualClip { get; private set; }


    /// <summary>
    /// Gets the bounds of the text selection (for rendering).
    /// </summary>
    /// <value>
    /// The bounds of the text selection (for rendering).
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<RectangleF> VisualSelectionBounds { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override Vector2F OnMeasure(Vector2F availableSize)
    {
      // This control can only measure itself if it is in a screen because it needs 
      // a font.
      var screen = Screen;
      if (screen == null)
        return base.OnMeasure(availableSize);

      // The code assumes that text is never null but it can be "".
      string text = Text ?? string.Empty;
      if (IsPassword)
      {
        // Replace text with password characters.
        text = new String(PasswordCharacter, text.Length);
      }

      float width = Width;
      float height = Height;
      bool hasWidth = Numeric.IsPositiveFinite(width);
      bool hasHeight = Numeric.IsPositiveFinite(height);

      // Limit constraint size by user-defined width and height.
      if (hasWidth && width < availableSize.X)
        availableSize.X = width;
      if (hasHeight && height < availableSize.Y)
        availableSize.Y = height;

      // Remove padding from constraint size.
      Vector4F padding = Padding;
      Vector2F contentSize = availableSize; // Text area size
      if (Numeric.IsPositiveFinite(availableSize.X))
        contentSize.X -= padding.X + padding.Z;
      if (Numeric.IsPositiveFinite(availableSize.Y))
        contentSize.Y -= padding.Y + padding.W;

      var font = screen.Renderer.GetFont(Font);

      // Limit height if MaxLines is set.
      if (!hasHeight)
      {
        float maxHeight = MaxLines * font.LineSpacing;
        if (contentSize.Y > maxHeight)
          contentSize.Y = maxHeight;
      }

      // Determine visibility of vertical scroll bar. (_verticalScrollBar.IsVisible)
      bool visualTextUpToDate = UpdateScrollBarVisibility(text, contentSize, font);

      // If the scroll bar is visible, it reduces the available width.
      float scrollBarWidth = 0.0f;
      if (_verticalScrollBar != null && _verticalScrollBar.IsVisible)
      {
        _verticalScrollBar.Measure(availableSize);
        scrollBarWidth = _verticalScrollBar.DesiredWidth;
        contentSize.X -= scrollBarWidth;
      }

      // Now we know the exact space for the text and can wrap the text.
      if (!visualTextUpToDate)
        UpdateVisualText(text, contentSize.X, font);

      // Determine desired size of text box.
      Vector2F desiredSize = contentSize;
      if (!hasWidth)
      {
        // Automatic width.
        float textWidth = font.MeasureString(VisualText).X;
        desiredSize.X = Math.Min(contentSize.X, textWidth);
      }

      if (!hasHeight)
      {
        // Automatic height.
        // Don't use font.MeasureString(VisualText).Y. The measured height may vary
        // by some pixels depending on the content. Instead calculate height with
        // font.LineSpacing, which is constant.
        int numberOfLines = (IsMultiline) ? _lineStarts.Count : 1;
        float textHeight = numberOfLines * font.LineSpacing;
        desiredSize.Y = Math.Min(contentSize.Y, textHeight);

        // Ensure minimum height if MinLines is set.
        float minHeight = MinLines * font.LineSpacing;
        if (desiredSize.Y < minHeight && minHeight <= contentSize.Y)
          desiredSize.Y = minHeight;
      }

      // Add padding and scroll bar.
      desiredSize.X += padding.X + padding.Z + scrollBarWidth;
      desiredSize.Y += padding.Y + padding.W;

      return desiredSize;
    }


    /// <inheritdoc/>
    protected override void OnArrange(Vector2F position, Vector2F size)
    {
      // This method updates/arranges the scroll bar and computes the VisualText, 
      // VisualClip, etc. It does not arrange any other visual children!

      // This control can only arrange itself if it is in a screen because it needs 
      // a font.
      var screen = Screen;
      if (screen == null)
      {
        base.OnArrange(position, size);
        return;
      }

      // The code assumes that text is never null but it can be "".
      string text = Text ?? string.Empty;
      if (IsPassword)
      {
        // Replace text with password characters.
        text = new String(PasswordCharacter, text.Length);
      }

      var font = screen.Renderer.GetFont(Font);

      // Coerce caret index.
      if (CaretIndex > text.Length)
        CaretIndex = text.Length;

      // The bounds of the control.
      RectangleF controlBounds = new RectangleF(position.X, position.Y, size.X, size.Y);

      // Get content bounds by subtracting the padding.
      Vector4F padding = Padding;
      RectangleF contentBounds = controlBounds;
      contentBounds.X += padding.X;
      contentBounds.Y += padding.Y;
      contentBounds.Width -= padding.X + padding.Z;
      contentBounds.Height -= padding.Y + padding.W;

      // Determine whether the actual size is different than the desired size, which
      // has been calculated in OnMeasure().
      Vector4F margin = Margin;
      bool sizeChanged = (DesiredWidth != size.X + margin.X + margin.Z);

      // If the actual size is different, we need to reevaluate the visibility of
      // the scroll bar!
      bool visualTextUpToDate = false;
      if (sizeChanged)
        visualTextUpToDate = UpdateScrollBarVisibility(text, contentBounds.Size, font);

      // If the scroll bar is visible, its size is subtracted from the available text area.
      float scrollBarWidth = 0.0f;
      bool isScrollBarVisible = _verticalScrollBar != null && _verticalScrollBar.IsVisible;
      if (isScrollBarVisible)
      {
        _verticalScrollBar.Measure(size);
        scrollBarWidth = _verticalScrollBar.DesiredWidth;
        contentBounds.Width -= scrollBarWidth;
      }

      // Update the text wrapping, if necessary.
      if (!visualTextUpToDate)
        UpdateVisualText(text, contentBounds.Width, font);

      // The scroll offset depends on actual size of control. Make sure scroll offset
      // is within the limits.
      int numberOfLines = (IsMultiline) ? _lineStarts.Count : 1;
      float textHeight = numberOfLines * font.LineSpacing;
      float maximum = Math.Max(0, textHeight - contentBounds.Height);
      if (IsMultiline)
      {
        VisualOffset = MathHelper.Clamp(VisualOffset, 0, maximum);
      }
      else
      {
        if (VisualOffset > 0)
        {
          float textWidth = font.MeasureString(VisualText).X;
          VisualOffset = MathHelper.Clamp(VisualOffset, 0, textWidth - contentBounds.Width + 4);
        }
      }

      // Update caret position.
      UpdateVisualCaret(text, contentBounds, font);

      // Update text selection bounds.
      UpdateSelectionBounds(text, contentBounds, font);

      // Update scroll bar properties and arrange the scroll bar.
      if (isScrollBarVisible)
      {
        _verticalScrollBar.Minimum = 0;
        _verticalScrollBar.Maximum = maximum;
        _verticalScrollBar.ViewportSize = Math.Min(1, contentBounds.Height / textHeight);
        _verticalScrollBar.SmallChange = font.LineSpacing;
        _verticalScrollBar.LargeChange = contentBounds.Height;
        _verticalScrollBar.Value = VisualOffset;
        _verticalScrollBar.Arrange(
          new Vector2F(controlBounds.Right - scrollBarWidth, controlBounds.Top),
          new Vector2F(scrollBarWidth, controlBounds.Height));
      }

      VisualClip = contentBounds;
    }


    /// <summary>
    /// Updates the visibility of the vertical scroll bar.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="contentSize">The size of the text area.</param>
    /// <param name="font">The font.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="VisualText"/> has been updated; otherwise,
    /// <see langword="false"/> when the <see cref="VisualText"/> still needs to be updated.
    /// </returns>
    private bool UpdateScrollBarVisibility(string text, Vector2F contentSize, SpriteFont font)
    {
      bool visualTextUpdated = false;
      if (_verticalScrollBar != null)
      {
        if (IsMultiline)
        {
          // ----- Multi-line
          var scrollBarVisibility = VerticalScrollBarVisibility;
          if (scrollBarVisibility == ScrollBarVisibility.Auto)
          {
            // Scroll bar is visible if the text is too large for the available space.
            UpdateVisualText(text, contentSize.X, font);
            float textHeight = _lineStarts.Count * font.LineSpacing;
            bool isScrollBarVisible = (textHeight > contentSize.Y);
            _verticalScrollBar.IsVisible = isScrollBarVisible;

            // If the scroll bar is hidden, then VisualText is up-to-date. But if the 
            // scroll bar is visible, VisualText needs to be recalculated.
            visualTextUpdated = !isScrollBarVisible;
          }
          else if (scrollBarVisibility == ScrollBarVisibility.Visible)
          {
            _verticalScrollBar.IsVisible = true;
          }
          else // Disabled or Hidden
          {
            _verticalScrollBar.IsVisible = false;
          }
        }
        else
        {
          // ----- Single-line
          _verticalScrollBar.IsVisible = false;
        }
      }

      return visualTextUpdated;
    }


    /// <summary>
    /// Updates the <see cref="VisualText"/> including text wrapping.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="textWidth">The available width.</param>
    /// <param name="font">The font.</param>
    private void UpdateVisualText(string text, float textWidth, SpriteFont font)
    {
      VisualText.Clear();
      WrapText(textWidth, font);

      if (_lineStarts == null || _lineStarts.Count == 0)
      {
        // Single-line: Just add all text. The renderer has to clip the text.
        VisualText.Append(text);
      }
      else
      {
        // Multiline: Add line by line.
        for (int i = 0; i < _lineStarts.Count; i++)
        {
          int start = _lineStarts[i];
          int end = (i + 1 < _lineStarts.Count) ? _lineStarts[i + 1] : -text.Length;

          VisualText.Append(text, Math.Abs(start), Math.Abs(end) - Math.Abs(start));

          // A positive index indicates that a newline character needs to be added
          // for text wrapping.
          if (end > 0)
            VisualText.Append('\n');
        }
      }
    }


    /// <summary>
    /// Computes the <see cref="_lineStarts"/> array that indicates where new lines have to start.
    /// </summary>
    /// <param name="maxWidth">The max width in pixels.</param>
    /// <param name="font">The font.</param>
    private void WrapText(float maxWidth, SpriteFont font)
    {
      if (!IsMultiline)
      {
        // Single-line text box - no wrapping.
        _lineStarts = null;
        return;
      }

      // Possibly more than one line. Compute wrapping indices.
      if (_lineStarts == null)
        _lineStarts = new List<int>();
      else
        _lineStarts.Clear();

      // The first line always starts at index 0.
      _lineStarts.Add(0);

      string text = Text ?? string.Empty;
      StringBuilder line = new StringBuilder();
      int words = 0;
      int i = 0;
      while (i < text.Length)
      {
        // Add word to line until we find a whitespace or newline.
        int wordLength = 0;
        while (i < text.Length && !char.IsWhiteSpace(text[i]) && text[i] != '\n')
        {
          line.Append(text[i]);
          i++;
          wordLength++;
        }

        // Trailing spaces belong to the word because a new line should not start with a space.
        while (i < text.Length && char.IsWhiteSpace(text[i]) && text[i] != '\n')
        {
          line.Append(text[i]);
          i++;
          wordLength++;
        }

        // Ok, we have one more word.
        words++;

        // Check if we have to start a new line.
        float width = font.MeasureString(line).X;
        if (width > maxWidth || (i < text.Length && text[i] == '\n'))
        {
          // We have to start a new line, either because the line is too long or the
          // last character was a newline.
          if (width > maxWidth)
          {
            // Ups, line is too long we have to remove something.
            if (words > 1)
            {
              // There are several words. --> Remove the last word.
              line.Remove(line.Length - wordLength, wordLength);
              i -= wordLength;
            }
            else
            {
              // Only one word - we have to cut the word.
              while (line.Length > 1)
              {
                line.Remove(line.Length - 1, 1);
                i--;

                if (font.MeasureString(line).X <= maxWidth)
                  break;
              }
            }
          }

          // We have the final line.
          words = 0;
          line.Clear();

          if (i < text.Length)
          {
            if (text[i] == '\n')
            {
              // When a line was started by a user-entered newline, we indicate this with a 
              // negative index.
              i++;
              _lineStarts.Add(-i);
            }
            else
            {
              // The new line was started because the line was too long and had to be wrapped.
              _lineStarts.Add(i);
            }
          }
        }
      }
    }


    /// <summary>
    /// Updates the visual caret position.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="textBounds">The text bounds.</param>
    /// <param name="font">The font.</param>
    private void UpdateVisualCaret(string text, RectangleF textBounds, SpriteFont font)
    {
      if (IsReadOnly)
      {
        // Read-only: Hide caret.
        VisualCaret = new Vector2F(float.NaN);
        return;
      }

      // Get visual caret position from CaretIndex.
      Vector2F caret = GetPosition(CaretIndex, text, textBounds, font);
      if (_bringCaretIntoView)
      {
        _bringCaretIntoView = false;

        if (!IsMultiline)
        {
          // Single line text box --> Compute horizontal offset in pixels.
          if (caret.X < textBounds.X)
          {
            VisualOffset -= textBounds.X - caret.X;
            caret.X = textBounds.X;
          }
          else if (caret.X > textBounds.Right)
          {
            VisualOffset += caret.X - textBounds.Right + 4;
            caret.X = textBounds.Right - 4;
          }
        }
        else
        {
          // Multi line text box --> Compute vertical offset in pixels.
          if (caret.Y < textBounds.Y)
          {
            VisualOffset -= textBounds.Y - caret.Y;
            caret.Y = textBounds.Y;
          }
          else if (caret.Y + font.LineSpacing > textBounds.Bottom)
          {
            VisualOffset += caret.Y - textBounds.Bottom + font.LineSpacing;
            caret.Y = textBounds.Bottom - font.LineSpacing;
          }
        }
      }

      if (GlobalSettings.PlatformID == PlatformID.WindowsPhone7
          || GlobalSettings.PlatformID == PlatformID.WindowsPhone8)
      {
        // Hide caret on Windows Phone.
        VisualCaret = new Vector2F(float.NaN);
      }
      else
      {
        VisualCaret = caret;
      }
    }


    /// <summary>
    /// Updates the text selection bounds.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <param name="textBounds">The text bounds.</param>
    /// <param name="font">The font.</param>
    private void UpdateSelectionBounds(string text, RectangleF textBounds, SpriteFont font)
    {
      VisualSelectionBounds.Clear();

      if (_selectionStart < 0)
        return;

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

      if (startIndex == endIndex)
        return;

      // Determine lines containing start and end of selection.
      int startLine = GetLine(startIndex);
      int endLine = GetLine(endIndex);

      // Determine the screen position of the selection. 
      // (Upper, left corner of start and end index.)
      Vector2F selectionStartPosition = GetPosition(startIndex, startLine, text, textBounds, font);
      Vector2F selectionEndPosition = GetPosition(endIndex, endLine, text, textBounds, font);

      // The selection bounds contain one RectangleF per line.
      if (startLine == endLine)
      {
        // ----- Single-line selection.
        VisualSelectionBounds.Add(new RectangleF(selectionStartPosition.X, selectionStartPosition.Y, selectionEndPosition.X - selectionStartPosition.X, font.LineSpacing));
      }
      else
      {
        // ----- Multi-line selection.
        int lineStartIndex, lineEndIndex;
        Vector2F lineStartPosition, lineEndPosition;

        // First line.
        lineEndIndex = Math.Abs(_lineStarts[startLine + 1]) - 1;
        lineEndPosition = GetPosition(lineEndIndex, startLine, text, textBounds, font);
        VisualSelectionBounds.Add(new RectangleF(selectionStartPosition.X, selectionStartPosition.Y, lineEndPosition.X - selectionStartPosition.X, font.LineSpacing));

        // Intermediate lines.
        for (int line = startLine + 1; line < endLine; line++)
        {
          lineStartIndex = lineEndIndex + 1;
          lineEndIndex = Math.Abs(_lineStarts[line + 1]) - 1;
          lineStartPosition = GetPosition(lineStartIndex, line, text, textBounds, font);
          lineEndPosition = GetPosition(lineEndIndex, line, text, textBounds, font);
          VisualSelectionBounds.Add(new RectangleF(lineStartPosition.X, lineStartPosition.Y, lineEndPosition.X - lineStartPosition.X, font.LineSpacing));
        }

        // Last line.
        lineStartIndex = lineEndIndex + 1;
        lineStartPosition = GetPosition(lineStartIndex, endLine, text, textBounds, font);
        VisualSelectionBounds.Add(new RectangleF(lineStartPosition.X, lineStartPosition.Y, selectionEndPosition.X - lineStartPosition.X, font.LineSpacing));
      }
    }


#if !WP7 && !XBOX
    /// <summary>
    /// Gets the nearest index of the caret for a given screen position (e.g. for a mouse click).
    /// </summary>
    /// <param name="position">The absolute screen position.</param>
    /// <param name="screen">The screen.</param>
    /// <returns>The index of the caret.</returns>
    private int GetIndex(Vector2F position, UIScreen screen)
    {
      string text = Text ?? string.Empty;
      var buffer = new StringBuilder();
      var font = screen.Renderer.GetFont(Font);

      if (!IsMultiline)
      {
        // ----- Single-line 
        // Measure substrings until we now the number of characters before the position.
        for (int i = 0; i < VisualText.Length; i++)
        {
          buffer.Clear();
          buffer.Append(text, 0, i + 1);

          float x = VisualClip.X - VisualOffset + font.MeasureString(buffer).X;
          if (x > position.X)
            return i;
        }
        return VisualText.Length;
      }

      // ----- Multi-line
      // Find line number.
      float textY = VisualClip.Y - VisualOffset;
      int line = (int)(position.Y - textY) / font.LineSpacing;
      if (line < 0)
        return 0;
      if (line >= _lineStarts.Count)
        return text.Length;

      // Get info for this line.
      int lineStartIndex = Math.Abs(_lineStarts[line]);
      int lineEndIndex = (line + 1 < _lineStarts.Count) ? Math.Abs(_lineStarts[line + 1]) : text.Length;
      int lineLength = lineEndIndex - lineStartIndex;

      // Measure substrings until we know the column.
      for (int i = 0; i < lineLength; i++)
      {
        buffer.Clear();
        buffer.Append(text, lineStartIndex, i + 1);

        float x = VisualClip.X + font.MeasureString(buffer).X;
        if (x > position.X)
          return lineStartIndex + i;
      }

      // If the last character is a newline, then we want to position the caret before
      // the newline, otherwise the caret is displayed on the next line.
      if (lineEndIndex > lineStartIndex && text[lineEndIndex - 1] == '\n')
        return lineEndIndex - 1;

      return lineEndIndex;
    }
#endif


    // ----- Not used.
    //private int GetColumn(int index)
    //{
    //  if (!IsMultiline)
    //    return index;

    //  int line = GetLine(index);
    //  return index - Math.Abs(_lineStarts[line]);
    //}


    private int GetLine(int index)
    {
      // Compute the line number of the character index using _lineStarts.
      int line = 0;
      if (_lineStarts != null && _lineStarts.Count > 0)
      {
        for (int i = 1; i < _lineStarts.Count; i++)  // Starting at second line (i = 1)!
        {
          if (Math.Abs(_lineStarts[i]) <= index)
            line++;
          else
            break;
        }
      }

      return line;
    }


    /// <summary>
    /// Gets the position of the given index.
    /// </summary>
    /// <param name="index">The zero-based index of a character in <paramref name="text"/>.</param>
    /// <param name="text">The text.</param>
    /// <param name="textBounds">The text bounds.</param>
    /// <param name="font">The font.</param>
    /// <returns>The upper, left corner of the character</returns>
    private Vector2F GetPosition(int index, string text, RectangleF textBounds, SpriteFont font)
    {
      // Find line number using _lineStarts.
      int line = GetLine(index);

      return GetPosition(index, line, text, textBounds, font);
    }


    // Same as GetPosition() above except that line number is already known. 
    // (Avoids recomputation of line number.)
    private Vector2F GetPosition(int index, int line, string text, RectangleF textBounds, SpriteFont font)
    {
      Debug.Assert(line == GetLine(index), "The line does not contain the given character index.");

      int lineStartIndex = (line == 0) ? 0 : Math.Abs(_lineStarts[line]);
      float y = textBounds.Y + line * font.LineSpacing;

      // Find column using sprite font.
      string textBeforeCaret = text.Substring(lineStartIndex, index - lineStartIndex);
      float x = textBounds.X + font.MeasureString(textBeforeCaret).X;

      // Handle VisualOffset (different interpretation for single-line vs. multi-line).
      if (IsMultiline)
        y -= VisualOffset;
      else
        x -= VisualOffset;

      return new Vector2F(x, y);
    }
    #endregion
  }
}
