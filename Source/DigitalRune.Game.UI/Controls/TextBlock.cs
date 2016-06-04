// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;

#if WP7 || XBOX
using DigitalRune.Text;
#endif


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Provides a lightweight control for displaying small amounts of text, supporting text 
  /// wrapping at word boundaries.
  /// </summary>
  /// <example>
  /// The following examples shows how to create a button and handle the 
  /// <see cref="ButtonBase.Click"/> event.
  /// <code lang="csharp">
  /// <![CDATA[
  /// var button = new Button
  /// {
  ///   Content = new TextBlock { Text = "Click Me!" },
  ///   Margin = new Vector4F(4),
  ///   Padding = new Vector4F(6),
  ///   HorizontalAlignment = HorizontalAlignment.Stretch,
  /// };
  /// 
  /// // To show the button, add it to an existing content control or panel.
  /// panel.Children.Add(button);
  /// 
  /// // To handle button clicks simply add an event handler to the Click event.
  /// button.Click += OnButtonClicked;
  /// ]]>
  /// </code>
  /// </example>
  public class TextBlock : UIControl
  {
    // TODO: Possible new properties: TextAlignment (Left, Right, Center, Justify)

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the text exactly as it should be displayed (wrapping already applied).
    /// </summary>
    /// <value>The text, exactly as it should be displayed (wrapping already applied).</value>
    public StringBuilder VisualText { get; private set; }


    /// <summary>
    /// Gets a value indicating whether the renderer should clip the rendered 
    /// <see cref="VisualText"/>.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the renderer should clip the text; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// If this value is <see langword="true"/>, the renderer should clip the text rendering. This
    /// flag is set if text must be clipped within characters (e.g. if the whole text block is not
    /// high enough). The clipping rectangle is defined by the <see cref="UIControl.ActualBounds"/>
    /// and applying the <see cref="UIControl.Padding"/>.
    /// </remarks>
    public bool VisualClip { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="UseEllipsis"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int UseEllipsisPropertyId = CreateProperty(
      typeof(TextBlock), "UseEllipsis", GamePropertyCategories.Appearance, null, false,
      UIPropertyOptions.AffectsRender);

    /// <summary>
    /// Gets or sets a value indicating whether an ellipsis ("…") should be appended when the text
    /// must be clipped. This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if an ellipsis ("…") should be appended; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool UseEllipsis
    {
      get { return GetValue<bool>(UseEllipsisPropertyId); }
      set { SetValue(UseEllipsisPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="WrapText"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int WrapTextPropertyId = CreateProperty(
      typeof(TextBlock), "WrapText", GamePropertyCategories.Layout, null, false,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets a value indicating whether text is wrapped when the available space is not
    /// wide enough. This is a game object property.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if text is wrapped when the available space is not wide enough; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    public bool WrapText
    {
      get { return GetValue<bool>(WrapTextPropertyId); }
      set { SetValue(WrapTextPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="Text"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int TextPropertyId = CreateProperty(
      typeof(TextBlock), "Text", GamePropertyCategories.Common, null, string.Empty,
      UIPropertyOptions.AffectsMeasure);

    /// <summary>
    /// Gets or sets the text. 
    /// This is a game object property.
    /// </summary>
    /// <value>The text.</value>
    public string Text
    {
      get { return GetValue<string>(TextPropertyId); }
      set { SetValue(TextPropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TextBlock"/> class.
    /// </summary>
    public TextBlock()
    {
      Style = "TextBlock";
      VisualText = new StringBuilder();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    protected override Vector2F OnMeasure(Vector2F availableSize)
    {
      // This control can only measure itself if it is in a screen because it needs a font.
      var screen = Screen;
      if (screen == null)
        return base.OnMeasure(availableSize);

      // Clear old renderer info.
      VisualClip = false;
      VisualText.Clear();

      string text = Text;
      float width = Width;
      float height = Height;
      bool hasWidth = Numeric.IsPositiveFinite(width);
      bool hasHeight = Numeric.IsPositiveFinite(height);

      if (string.IsNullOrEmpty(text))
      {
        // No text --> Abort.
        return new Vector2F(
          hasWidth ? width : 0,
          hasHeight ? height : 0);
      }

      // Limit constraint size by user-defined width and height.
      if (hasWidth && width < availableSize.X)
        availableSize.X = width;
      if (hasHeight && height < availableSize.Y)
        availableSize.Y = height;

      // Remove padding from constraint size.
      Vector4F padding = Padding;
      Vector2F contentSize = availableSize;
      if (Numeric.IsPositiveFinite(availableSize.X))
        contentSize.X -= padding.X + padding.Z;
      if (Numeric.IsPositiveFinite(availableSize.Y))
        contentSize.Y -= padding.Y + padding.W;

      // Measure text size.
      var font = screen.Renderer.GetFont(Font);
      Vector2F size = (Vector2F)font.MeasureString(text);
      if (size < contentSize)
      {
        // All text is visible. (VisualText is equal to Text.)
        VisualText.Append(text);
        return new Vector2F(
          hasWidth ? width : size.X + padding.X + padding.Z,
          hasHeight ? height : size.Y + padding.Y + padding.W);
      }

      // Get number of lines.
      int numberOfLines = 1;
      for (int i = 0; i < text.Length - 1; i++)
        if (text[i] == '\n')
          numberOfLines++;

      if (numberOfLines == 1 && size.Y > contentSize.Y)
      {
        // Not enough space for a single line height. --> Keep all text and use clipping.
        VisualText.Append(text);
        VisualClip = true;
        return new Vector2F(
          hasWidth ? width : size.X + padding.X + padding.Z,
          hasHeight ? height : size.Y + padding.Y + padding.W);
      }

      if (!WrapText)
      {
        // Not using word wrapping.

        // Compute desired size.
        Vector2F desiredSize = new Vector2F(
          hasWidth ? width : availableSize.X,
          hasHeight ? height : availableSize.Y);

        desiredSize.X = Math.Min(desiredSize.X, size.X + padding.X + padding.Z);
        desiredSize.Y = Math.Min(desiredSize.Y, size.Y + padding.Y + padding.W);

        if (numberOfLines > 1            // 2 or more lines?
            || !UseEllipsis              // No ellipsis needed?
            || size.Y > contentSize.Y)   // Single line is already to high?
        {
          // Just clip the text.
          VisualClip = true;
          VisualText.Append(text);
        }
        else
        {
          // 1 line that is too long and we have to insert an ellipsis.
          VisualText.Append(text);
          TrimText(VisualText, font, contentSize.X);
        }

        return desiredSize;
      }

      // Get words.
      var words = SplitText();

      // Note: We can compute line heights without font.MeasureString(). But we cannot compute 
      // line widths without font.MeasureString() because we do not have kerning information.
      int lineHeight = font.LineSpacing;
      Debug.Assert(lineHeight <= contentSize.Y, "At least one line must fit into content");
      float currentHeight = lineHeight;

      // Add words to string builder until space runs out.
      // In each loop iteration one line is built.
      int index = 0;
      int lineWordCount = 0;
      var line = new StringBuilder();
      while (currentHeight < contentSize.Y  // Room for one more line?
             && index < words.Count)        // Words left?
      {
        if (index > 0)
        {
          VisualText.Append(line.ToString()); // Add line of last iteration.
          VisualText.Append("\n");

          // If the next word is a newline, then we can skip it because we start a new line
          // anyways.
          if (words[index] == null)
            index++;
        }

        // Start with empty line.
        line.Remove(0, line.Length);
        lineWordCount = 0;

        // Build line.
        while (index < words.Count             // As long as we have words.
               && words[index] != null)        // And as long the next word is not a newline.
        {
          // Add spaces after first word.
          if (lineWordCount > 0)
            line.Append(" ");

          // Add next word.
          line.Append(words[index]);
          lineWordCount++;
          index++;

          float lineWidth = font.MeasureString(line).X;
          if (lineWidth > contentSize.X)
          {
            // Line is too long! Remove last word + blank. But keep at least one word per line.
            if (lineWordCount > 1)
            {
              index--;
              lineWordCount--;
              int wordLength = words[index].Length;
              line.Remove(line.Length - wordLength - 1, wordLength + 1);
            }
            else
            {
              VisualClip = true;  // A single word is too long and must be clipped.
            }

            break;
          }
        }

        currentHeight += lineHeight;
      }

      // Nearly all visible text is in VisualText.
      // The last line is in "line" and was not yet added to VisualText.
      if (UseEllipsis)
      {
        if ((index < words.Count                            // Not enough space to print all words.
            || font.MeasureString(line).X > contentSize.X)) // The last line is too long and needs to be trimmed.
        {
          // Trim the last line and add an ellipsis.
          line.Append("…");
          while (lineWordCount > 0 && font.MeasureString(line).X > contentSize.X)
          {
            index--;

            // We have to remove one word before the ellipsis.
            int wordLength = words[index].Length;
            line.Remove(line.Length - 1 - wordLength, wordLength);

            // Remove ' ' before ellipsis.
            int indexBeforeEllipsis = line.Length - 2;
            if (indexBeforeEllipsis > 0 && line[indexBeforeEllipsis] == ' ')
              line.Remove(indexBeforeEllipsis, 1);

            lineWordCount--;
          }
        }
      }

      // Add last line.
      VisualText.Append(line);

      size = (Vector2F)font.MeasureString(VisualText);
      return new Vector2F(
        hasWidth ? width : size.X + padding.X + padding.Z,
        hasHeight ? height : size.Y + padding.Y + padding.W);
    }


    private static void TrimText(StringBuilder text, SpriteFont font, float maxWidth)
    {
      // Remove last character because we know that we have to trim at least 1 but probably more.
      text.Remove(text.Length - 1, 1);

      // Add ellipsis.
      text.Append("…");

      // Remove characters before "…" until the string is shorter than or equal to maxWidth.
      // (Note: The ellipsis "…" is also removed if there is not enough space for single 
      // character.)
      while (font.MeasureString(text).X > maxWidth && text.Length > 0)
      {
        int index = Math.Max(0, text.Length - 2);
        text.Remove(index, 1);
      }
    }


    /// <summary>
    /// Gets the list of words for the current <see cref="Text"/>.
    /// </summary>
    /// <remarks>
    /// Words are separated by spaces. <see langword="null"/> is added to the list for newline 
    /// symbols.
    /// </remarks>
    private List<string> SplitText()
    {
      var words = new List<string>();

      int i = 0;
      string text = Text;
      while (i < text.Length)
      {
        // Skip all blanks.
        while (i < text.Length && text[i] == ' ')
          i++;

        // A new word starts.
        int wordStart = i;

        // Move index to the next blank, newline or end of text.
        while (i < text.Length && text[i] != ' ' && text[i] != '\n')
          i++;

        // Have we found a word?
        if (wordStart < i)
        {
          // Add word.
          words.Add(text.Substring(wordStart, i - wordStart));
        }

        // Is the next char a newline symbol?
        if (i < text.Length && text[i] == '\n')
        {
          // null indicates a newline.
          words.Add(null);
          i++;
        }
      }

      return words;
    }
    #endregion
  }
}
