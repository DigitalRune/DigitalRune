// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI.Consoles;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Input;
#if WP7 || XBOX
using DigitalRune.Text;
#endif
#if SILVERLIGHT
using Keys = System.Windows.Input.Key;
#endif


namespace DigitalRune.Game.UI.Controls
{
  /// <summary>
  /// Provides an interactive console for debugging.
  /// </summary>
  /// <remarks>
  /// <para>
  /// If the user enters a command, the <see cref="CommandEntered"/> event is raised. If this event 
  /// is not handled (see <see cref="ConsoleCommandEventArgs.Handled"/>), the 
  /// <see cref="Interpreter"/> handles the command. 
  /// </para>
  /// <para>
  /// To add new commands, you can either handle the <see cref="CommandEntered"/> event, or add new 
  /// commands to the <see cref="Interpreter"/> (see 
  /// <see cref="ConsoleCommandInterpreter.Commands"/>).
  /// </para>
  /// <para>
  /// The console has a command history that can be accessed with the up/down keys. The console 
  /// content can be scrolled with the page up/down keys or a vertical scroll bar.
  /// </para>
  /// <para>
  /// " can be used to group several words. To write " in a command argument it must be escaped
  /// using \".
  /// </para>
  /// <para>
  /// The <see cref="Console"/> assumes that a fixed-width <see cref="UIControl.Font"/> is used.
  /// </para>
  /// </remarks>
  /// <example>
  /// The following example opens a window containing a console.
  /// <code lang="csharp">
  /// <![CDATA[
  /// void ShowConsoleWindow(UIScreen screen)
  /// {
  ///   var window = new Window
  ///   {
  ///     Title = "Console Window",
  ///     Width = 480,
  ///     Height = 240
  ///   };
  ///   var console = new Console
  ///   {
  ///     HorizontalAlignment = HorizontalAlignment.Stretch,
  ///     VerticalAlignment = VerticalAlignment.Stretch
  ///   };
  ///   window.Content = console;
  /// 
  ///   // Print a message in the console.
  ///   console.WriteLine("Enter 'help' to see all available commands.");
  /// 
  ///   // Register a new command 'close', which closes the console window.
  ///   var closeCommand = new ConsoleCommand("close", "Close console.", _ => window.Close());
  ///   console.Interpreter.Commands.Add(closeCommand);
  /// 
  ///   window.Show(_screen);
  /// }
  /// ]]>
  /// </code>
  /// </example>
  public class Console : UIControl, IConsole
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The index of the selected history index. -1 means no history entry is selected.
    private int _historyIndex = -1;

#if !SILVERLIGHT
    // Pressing ChatPadGreen changes the keyboard mode until another key is pressed.
    // Normally, the ChatPadGreen/Orange modes are time limited, but XNA cannot access this
    // info :-(.
    private bool _chatPadGreenIsActive;
    private bool _chatPadOrangeIsActive;
#endif

    // Measures of the current console layout.
    private float _charWidth;
    private int _numberOfLines;
    private int _numberOfColumns;

    // The wrapped text lines. Each entry is a single visual console line.
    private readonly List<string> _wrappedLines = new List<string>();

    private ScrollBar _verticalScrollBar;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the position of the caret.
    /// </summary>
    /// <value>The position of the caret (zero-based index).</value>
    public int CaretIndex
    {
      get { return _caretIndex; }
      set
      {
        if (_caretIndex == value)
          return; 

        _caretIndex = value;

        // Limit cursor position to allowed range.
        if (_caretIndex < 0)
          _caretIndex = 0;
        else if (_caretIndex > Text.Length)
          _caretIndex = Text.Length;

        // When the caret moves, the caret is brought into view.
        LineOffset = 0;
        InvalidateVisual();
      }
    }
    private int _caretIndex;


    /// <summary>
    /// Gets the text currently entered at the prompt.
    /// </summary>
    /// <value>The text currently entered at the prompt.</value>
    /// <remarks>
    /// <para>
    /// The current text is not in <see cref="TextLines"/>.
    /// </para>
    /// <para>
    /// Call <see cref="UIControl.InvalidateArrange"/> after changing the text to update the 
    /// console. 
    /// </para>
    /// </remarks>
    public StringBuilder Text { get; private set; }


    /// <inheritdoc/>
    public ConsoleCommandInterpreter Interpreter { get; private set; }


    /// <summary>
    /// Gets the command history.
    /// </summary>
    /// <value>The command history.</value>
    /// <remarks>
    /// This list contains the last commands (whole line of text including arguments) that were 
    /// entered into the console. The max number of history entries is limited by 
    /// <see cref="MaxHistoryEntries"/>. The entries are sorted. History[0] contains the oldest 
    /// entry. History[History.Count - 1] contains the newest entry. The list does not contain
    /// duplicate entries.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<string> History { get; private set; }


    /// <summary>
    /// Gets or sets the max number of entries in the command history.
    /// </summary>
    /// <value>The max number of history entries.</value>
    public int MaxHistoryEntries { get; set; }


    /// <inheritdoc/>
    public string Prompt
    {
      get { return _prompt; }
      set
      {
        _prompt = value;
        if (_prompt == null)
          _prompt = string.Empty;

        InvalidateArrange();
      }
    }
    private string _prompt;

    
    /// <summary>
    /// Gets the text content of the console line by line.
    /// </summary>
    /// <value>The text lines.</value>
    /// <remarks>
    /// <para>
    /// A single string in this list is the result of a single <see cref="WriteLine(string)"/>. To
    /// add text to the console, call <see cref="WriteLine(string)"/> and do not change this list
    /// directly.
    /// </para>
    /// <para>
    /// A single text line can create several <see cref="VisualLines"/> if the line is too long and
    /// must be wrapped at the max column limit of the console.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<string> TextLines { get; private set; }

    
    /// <summary>
    /// Gets the text lines, exactly as they should be displayed (wrapping already applied).
    /// </summary>
    /// <value>The text, exactly as it should be displayed (wrapping already applied).</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<String> VisualLines { get; private set; }
    
    
    /// <summary>
    /// Gets the x-position of the caret (in columns).
    /// </summary>
    /// <value>
    /// The x-position of the caret (in columns). This value can be -1 to indicate that the caret is
    /// not visible.
    /// </value>
    public int VisualCaretX { get; private set; }


    /// <summary>
    /// Gets the y-position of the caret (in rows).
    /// </summary>
    /// <value>The y-position of the caret (in rows).</value>
    public int VisualCaretY { get; private set; }


    /// <inheritdoc/>
    public event EventHandler<ConsoleCommandEventArgs> CommandEntered;
    #endregion


    //--------------------------------------------------------------
    #region Game Object Properties & Events
    //--------------------------------------------------------------

    /// <summary> 
    /// The ID of the <see cref="LineOffset"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int LineOffsetPropertyId = CreateProperty(
      typeof(Console), "LineOffset", GamePropertyCategories.Default, null, 0, 
      UIPropertyOptions.AffectsArrange);

    /// <summary>
    /// Gets or sets the line offset for scrolling. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The line offset. 0 means that the end of the console text is visible. A positive value
    /// indicates the amount of lines to scroll back up to older content.
    /// </value>
    public int LineOffset
    {
      get { return GetValue<int>(LineOffsetPropertyId); }
      set { SetValue(LineOffsetPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="MaxLines"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int MaxLinesPropertyId = CreateProperty(
      typeof(Console), "MaxLines", GamePropertyCategories.Default, null, 100, 
      UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the max number of text lines. 
    /// This is a game object property.
    /// </summary>
    /// <value>The max number of text lines.</value>
    /// <remarks>
    /// The console will "forget" the older <see cref="TextLines"/> if the number of
    /// <see cref="TextLines"/> exceeds this value.
    /// </remarks>
    public int MaxLines
    {
      get { return GetValue<int>(MaxLinesPropertyId); }
      set { SetValue(MaxLinesPropertyId, value); }
    }


    /// <summary> 
    /// The ID of the <see cref="VerticalScrollBarStyle"/> game object property.
    /// </summary>
#if !NETFX_CORE && !XBOX && !PORTABLE
    [Browsable(false)]
#endif
    public static readonly int VerticalScrollBarStylePropertyId = CreateProperty(
      typeof(Console), "VerticalScrollBarStyle", GamePropertyCategories.Style, null, 
      "VerticalScrollBar", UIPropertyOptions.None);

    /// <summary>
    /// Gets or sets the style that is applied to the vertical scroll bar. 
    /// This is a game object property.
    /// </summary>
    /// <value>
    /// The style that is applied to the vertical scroll bar. If this property is 
    /// <see langword="null"/> or an empty string no scroll bar is used.
    /// </value>
    public string VerticalScrollBarStyle
    {
      get { return GetValue<string>(VerticalScrollBarStylePropertyId); }
      set { SetValue(VerticalScrollBarStylePropertyId, value); }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes static members of the <see cref="Console"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static Console()
    {
      // Focusable per default.
      OverrideDefaultValue(typeof(Console), FocusablePropertyId, true);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Console"/> class.
    /// </summary>
    public Console()
    {
      Style = "Console";

      Text = new StringBuilder();
      History = new List<string>();
      TextLines = new List<string>();
      Interpreter = new ConsoleCommandInterpreter(this);
      MaxHistoryEntries = 25;
      Prompt = "> ";
      VisualLines = new List<string>();

      var lineOffset = Properties.Get<int>(LineOffsetPropertyId);
      lineOffset.Changing += OnCoerceLineOffset;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override void OnLoad()
    {
      base.OnLoad();

      // Create scroll bar.
      var verticalScrollBarStyle = VerticalScrollBarStyle;
      if (!string.IsNullOrEmpty(verticalScrollBarStyle))
      {
        _verticalScrollBar = new ScrollBar
        {
          Name = "ConsoleScrollBar",
          Style = verticalScrollBarStyle,
          SmallChange = 1, // 1 unit = 1 line.
        };
        VisualChildren.Add(_verticalScrollBar);

        var scrollBarValue = _verticalScrollBar.Properties.Get<float>(RangeBase.ValuePropertyId);
        scrollBarValue.Changed += (s, e) =>
        {
          // The line offset is the "inverse" of the scroll bar value:
          int maxLineOffset = _wrappedLines.Count - _numberOfLines;
          LineOffset = maxLineOffset - (int)e.NewValue;
        };
      }
    }

    
    /// <inheritdoc/>
    protected override void OnUnload()
    {
      VisualChildren.Remove(_verticalScrollBar);
      _verticalScrollBar = null;

      base.OnUnload();
    }


    /// <inheritdoc/>
    public void Clear()
    {
      TextLines.Clear();
      Text.Clear();
      CaretIndex = 0;
      _historyIndex = -1;
      InvalidateArrange();
    }


    private void HistoryDown()
    {
      if (_historyIndex >= 0 && _historyIndex < History.Count - 1)
      {
        // Select next newer entry.
        _historyIndex++;
        Text.Clear();
        Text.Append(History[_historyIndex]);
        CaretIndex = Text.Length;
        InvalidateArrange();
      }
      else if (_historyIndex == History.Count - 1)
      {
        // No newer entry exists. Set current text to empty string for new input.
        _historyIndex = -1;
        Text.Clear();
        CaretIndex = Text.Length;
        InvalidateArrange();
      }
    }


    private void HistoryUp()
    {
      if (History.Count > 0 && _historyIndex != 0)
      {
        if (_historyIndex < 0)
          _historyIndex = History.Count - 1;   // Select newest entry.
        else if (_historyIndex > 0)
          _historyIndex--;                     // Select next older entry.

        Text.Clear();
        Text.Append(History[_historyIndex]);
        CaretIndex = Text.Length;
        InvalidateArrange();
      }
    }


    private void PageUp()
    {
      LineOffset += _numberOfLines;
    }


    private void PageDown()
    {
      LineOffset -= _numberOfLines;
    }


    /// <inheritdoc/>
    public void WriteLine()
    {
      TextLines.Add(string.Empty);
      LineOffset = 0;
      InvalidateArrange();
    }


    /// <inheritdoc/>
    public void WriteLine(string text)
    {
      TextLines.Add(text);
      LineOffset = 0;
      InvalidateArrange();
    }


    private void OnCoerceLineOffset(object sender, GamePropertyEventArgs<int> eventArgs)
    {
      // Make sure LineOffset is in the allowed range.
      int maxLineOffset = Math.Max(0, _wrappedLines.Count - _numberOfLines);
      eventArgs.CoercedValue = MathHelper.Clamp(eventArgs.CoercedValue, 0, maxLineOffset);
    }


    private void LimitText()
    {
      // Limit entries in TextLines and entries in command history.
      while (TextLines.Count > MaxLines)
      {
        TextLines.RemoveAt(0);
        InvalidateArrange();
      }

      while (History.Count > MaxHistoryEntries)
      {
        History.RemoveAt(0);
        InvalidateArrange();
      }
    }

    
    /// <summary>
    /// Raises the <see cref="CommandEntered"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="ConsoleCommandEventArgs"/> object that provides the arguments for the event.
    /// </param>
    ///// <remarks>
    ///// <strong>Notes to Inheritors: </strong>When overriding <see cref="OnCommandEntered"/> in a 
    ///// derived class, be sure to call the base class's <see cref="OnCommandEntered"/> method so
    ///// that registered delegates receive the event.
    ///// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private void OnCommandEntered(ConsoleCommandEventArgs eventArgs)
    {
      var handler = CommandEntered;

      try
      {
        if (handler != null)
          handler(this, eventArgs);
      }
      catch (Exception exception)
      {
        // We catch all exceptions and print them.
        WriteLine(exception.Message);
      }

      // At last call default interpreter.
      Interpreter.Interpret(eventArgs);
    }


    /// <summary>
    /// Parses a command and returns the command arguments.
    /// </summary>
    /// <param name="text">The text.</param>
    /// <returns>The command arguments. The first argument is the command name itself.</returns>
    private static string[] ParseCommand(string text)
    {
      // "" is used to group several words into one argument.
      // " is escaped with \".

      List<string> args = new List<string>();
      StringBuilder builder = new StringBuilder();
      bool inString = false;                     // true if we are between " and ".

      // Go through text character by character.
      for (int i = 0; i < text.Length; i++)
      {
        char lastChar = (i == 0) ? '\0' : text[i - 1];
        char currentChar = text[i];
        char nextChar = (i < text.Length - 1) ? text[i + 1] : ' ';

        if (!inString)
        {
          // ----- We are not in a string.

          if (currentChar == ' ')
          {
            // The current char is a whitespace. --> Finish argument.
            if (builder.Length > 0)
            {
              args.Add(builder.ToString());
              builder.Clear();
            }
          }
          else if (currentChar == '"' && lastChar == ' ')
          {
            // We are entering a string.
            inString = true;
          }
          else
          {
            // Current char is a normal character.
            builder.Append(currentChar);
          }
        }
        else
        {
          // ---- We are in a string.

          // Test for \"
          if (currentChar == '\\' && nextChar == '"')
          {
            // An escaped ".
            builder.Append('"');
            i++;
          }
          else if (currentChar == '"' && nextChar == ' ')
          {
            // The end of the string.
            inString = false;
            args.Add(builder.ToString());
            builder.Clear();
          }
          else
          {
            // Another character within the string.
            builder.Append(currentChar);
          }
        }
      }

      // Don't forget current/last argument.
      if (builder.Length > 0)
        args.Add(builder.ToString());

      return args.ToArray();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    protected override void OnHandleInput(InputContext context)
    {
      base.OnHandleInput(context);

      if (!IsLoaded)
        return;

      var screen = Screen;
      var inputService = InputService;
      var uiService = UIService;

      // Limit the console content.
      LimitText();

      // Scroll with mouse wheel when mouse is over.
      if (IsMouseOver && !inputService.IsMouseOrTouchHandled)
      {
        if (inputService.MouseWheelDelta != 0)
        {
          inputService.IsMouseOrTouchHandled = true;
          LineOffset += (int)inputService.MouseWheelDelta / screen.MouseWheelScrollDelta * screen.MouseWheelScrollLines;
        }
      }

#if !SILVERLIGHT
      // Move caret with left stick and d-pad.
      // Move through history with left stick and d-pad.
      // Scroll with right stick.
      if (!inputService.IsGamePadHandled(context.AllowedPlayer))
      {
        if (inputService.IsPressed(Buttons.DPadLeft, true, context.AllowedPlayer) || inputService.IsPressed(Buttons.LeftThumbstickLeft, true, context.AllowedPlayer))
        {
          inputService.IsGamePadHandled(context.AllowedPlayer);
          CaretIndex--;
        }

        if (inputService.IsPressed(Buttons.DPadRight, true, context.AllowedPlayer) || inputService.IsPressed(Buttons.LeftThumbstickRight, true, context.AllowedPlayer))
        {
          inputService.IsGamePadHandled(context.AllowedPlayer);
          CaretIndex++;
        }

        if (inputService.IsPressed(Buttons.DPadUp, true, context.AllowedPlayer) || inputService.IsPressed(Buttons.LeftThumbstickUp, true, context.AllowedPlayer))
        {
          inputService.IsGamePadHandled(context.AllowedPlayer);
          HistoryUp();
        }

        if (inputService.IsPressed(Buttons.DPadDown, true, context.AllowedPlayer) || inputService.IsPressed(Buttons.LeftThumbstickDown, true, context.AllowedPlayer))
        {
          inputService.IsGamePadHandled(context.AllowedPlayer);
          HistoryDown();
        }

        if (inputService.IsPressed(Buttons.RightThumbstickUp, true, context.AllowedPlayer))
        {
          inputService.IsGamePadHandled(context.AllowedPlayer);
          LineOffset++;
        }

        if (inputService.IsPressed(Buttons.RightThumbstickDown, true, context.AllowedPlayer))
        {
          inputService.IsGamePadHandled(context.AllowedPlayer);
          LineOffset--;
        }
      }
#endif

      if (!inputService.IsKeyboardHandled 
          && IsFocusWithin
          && inputService.PressedKeys.Count > 0)
      {
        int numberOfPressedKeys = inputService.PressedKeys.Count;

#if !SILVERLIGHT
        // Handle ChatPadOrange/Green.
        if (inputService.IsPressed(Keys.ChatPadOrange, false))
        {
          if (_chatPadOrangeIsActive)
            _chatPadOrangeIsActive = false;   // ChatPadOrange is pressed a second time to disable the mode.
          else if (numberOfPressedKeys == 1)
            _chatPadOrangeIsActive = true;    // ChatPadOrange is pressed alone to enable the mode.
        }
        if (inputService.IsPressed(Keys.ChatPadGreen, false))
        {
          if (_chatPadGreenIsActive)
            _chatPadOrangeIsActive = false;   // ChatPadGreen is pressed a second time to disable the mode.
          else if (numberOfPressedKeys == 1)
            _chatPadGreenIsActive = true;     // ChatPadGreen is pressed alone to enable the mode.
        }
#endif

        // Check which modifier keys are pressed. We check this manually to detect ChatPadOrange/Green.
        ModifierKeys modifierKeys = ModifierKeys.None;
#if !SILVERLIGHT
        if (inputService.IsDown(Keys.LeftShift) || inputService.IsDown(Keys.RightShift))
          modifierKeys = modifierKeys | ModifierKeys.Shift;
        if (inputService.IsDown(Keys.LeftControl) || inputService.IsDown(Keys.RightControl))
          modifierKeys = modifierKeys | ModifierKeys.Control;
        if (inputService.IsDown(Keys.LeftAlt) || inputService.IsDown(Keys.RightAlt))
          modifierKeys = modifierKeys | ModifierKeys.Alt;
        if (_chatPadGreenIsActive || inputService.IsDown(Keys.ChatPadGreen))
          modifierKeys = modifierKeys | ModifierKeys.ChatPadGreen;
        if (_chatPadOrangeIsActive || inputService.IsDown(Keys.ChatPadOrange))
          modifierKeys = modifierKeys | ModifierKeys.ChatPadOrange;
#else
        if (inputService.IsDown(Keys.Shift))
          modifierKeys = modifierKeys | ModifierKeys.Shift;
        if (inputService.IsDown(Keys.Ctrl))
          modifierKeys = modifierKeys | ModifierKeys.Control;
        if (inputService.IsDown(Keys.Alt))
          modifierKeys = modifierKeys | ModifierKeys.Alt;
#endif

        if (inputService.IsPressed(Keys.Enter, true))
        {
          // ----- Enter --> Add current text to TextLines and raise CommandEntered.

          string text = Text.ToString();
          Text.Clear();

          WriteLine(Prompt + text);
          if (!string.IsNullOrEmpty(text))
          {
            // Add history entry.
            History.Remove(text);     // If the entry exists, we want to move it to the front.
            History.Add(text);
          }

          // Raise CommandExecuted event.
          string[] args = ParseCommand(text);
          if (args.Length > 0)
            OnCommandEntered(new ConsoleCommandEventArgs(args));

          LineOffset = 0;
          _historyIndex = -1;
          CaretIndex = 0;
          InvalidateArrange();
          inputService.IsKeyboardHandled = true;
        }

        if (inputService.IsPressed(Keys.Back, true))
        {
          // ----- Backspace --> Delete single character.

          if (Text.Length > 0 && CaretIndex > 0)
          {
            Text.Remove(CaretIndex - 1, 1);
            CaretIndex--;
            InvalidateArrange();
          }

          inputService.IsKeyboardHandled = true;
        }

        if (inputService.IsPressed(Keys.Delete, true))
        {
          // ----- Delete --> Delete single character.

          if (CaretIndex < Text.Length)
          {
            Text.Remove(CaretIndex, 1);
            InvalidateArrange();
          }

          inputService.IsKeyboardHandled = true;
        }

        if (inputService.IsPressed(Keys.Left, true))
        {
          // ----- Caret Left
          CaretIndex--;
          inputService.IsKeyboardHandled = true;
        }

        if (inputService.IsPressed(Keys.Right, true))
        {
          // ----- Caret Right
          CaretIndex++;
          inputService.IsKeyboardHandled = true;
        }

        if (inputService.IsPressed(Keys.Home, true))
        {
          // ----- Home
          CaretIndex = 0;
          inputService.IsKeyboardHandled = true;
        }

        if (inputService.IsPressed(Keys.End, true))
        {
          // ----- End
          CaretIndex = Text.Length;
          inputService.IsKeyboardHandled = true;
        }

        if (inputService.IsPressed(Keys.Up, true))
        {
          // ----- Up --> Get history entry. 
          HistoryUp();

          inputService.IsKeyboardHandled = true;
        }

        if (inputService.IsPressed(Keys.Down, true))
        {
          // ----- Down --> Get history entry.
          HistoryDown();
          inputService.IsKeyboardHandled = true;
        }

        if (inputService.IsPressed(Keys.PageUp, true))
        {
          // ----- PageUp --> Scroll up.
          PageUp();
          inputService.IsKeyboardHandled = true;
        }

        if (inputService.IsPressed(Keys.PageDown, true))
        {
          // ----- PageDown --> Scroll down.
          PageDown();
          inputService.IsKeyboardHandled = true;
        }

        // Cut/copy/paste
        if (modifierKeys == ModifierKeys.Control)
        {
          if (inputService.IsPressed(Keys.X, true))
          {
            Cut();
            inputService.IsKeyboardHandled = true;
          }
          if (inputService.IsPressed(Keys.C, true))
          {
            Copy();
            inputService.IsKeyboardHandled = true;
          }
          if (inputService.IsPressed(Keys.V, true))
          {
            Paste();
            inputService.IsKeyboardHandled = true;
          }
        }

        for (int i = 0; i < numberOfPressedKeys; i++)
        {
          // Add characters to current text.
          var key = inputService.PressedKeys[i];
          char c = uiService.KeyMap[key, modifierKeys];
          if (c == 0 || char.IsControl(c))
            continue;

          Text.Insert(CaretIndex, c.ToString());
          CaretIndex++;
          InvalidateArrange();
          inputService.IsKeyboardHandled = true;
        }

#if !SILVERLIGHT
        // Handle ChatPadOrange/Green.
        if (_chatPadOrangeIsActive && inputService.PressedKeys.Any(k => k != Keys.ChatPadOrange))
          _chatPadOrangeIsActive = false;   // Any other key is pressed. This disables the ChatPadOrangeMode.
        if (_chatPadGreenIsActive && inputService.PressedKeys.Any(k => k != Keys.ChatPadGreen))
          _chatPadGreenIsActive = false;   // Any other key is pressed. This disables the ChatPadGreenMode.
#endif
      }
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    protected override void OnArrange(Vector2F position, Vector2F size)
    {
      var screen = Screen;
      if (screen != null)
      {
        // Get the width of a single character.
        var font = screen.Renderer.GetFont(Font);
        _charWidth = font.MeasureString("A").X;

        // The number of visual rows and columns that fit into the console.
        Vector4F padding = Padding;
        _numberOfLines = (int)((size.Y - padding.Y - padding.W) / font.LineSpacing);
        _numberOfColumns = (int)((size.X - padding.X - padding.Z) / _charWidth);

        // Wrap text lines.
        // TODO: It is not really necessary to wrap all lines because some old wrapped line info is still correct - but this is simpler.
        WrapLines();

        // Update scroll bar properties.
        if (_verticalScrollBar != null)
        {
          float maxLineOffset = Math.Max(0, _wrappedLines.Count - _numberOfLines);
          _verticalScrollBar.Minimum = 0;
          _verticalScrollBar.Maximum = maxLineOffset;
          _verticalScrollBar.Value = maxLineOffset - LineOffset;
          _verticalScrollBar.LargeChange = _numberOfLines;
          _verticalScrollBar.ViewportSize = Math.Min(1, (float)(_numberOfLines) / _wrappedLines.Count);
        }
      }

      base.OnArrange(position, size);
    }


    /// <inheritdoc/>
    protected override void OnRender(UIRenderContext context)
    {
      if (!IsVisualValid)
      {
        // ----- Store VisualLines as info for the renderer.
        int lineCount = _wrappedLines.Count;                           // The total number of lines.
        int startLine = ComputeStartWrappedLine();                     // The index of the first visual line.
        int endLine = Math.Min(lineCount, startLine + _numberOfLines); // The exclusive end line.
        VisualLines.Clear();
        for (int i = startLine; i < endLine; i++)
          VisualLines.Add(_wrappedLines[i]);

        // ----- Determine the position for the caret line.
        // The number of lines for the wrapped current text.
        int numberOfLinesInCurrentText = (int)Math.Ceiling((float)(Prompt.Length + Text.Length) / _numberOfColumns);

        // The line index of the cursor relative to the wrapped current text.
        int cursorLineInCurrentText = (Prompt.Length + CaretIndex) / _numberOfColumns;

        // If the cursor is exactly in the last column of the last line, then it should not skip
        // to the next line (because there is no VisualLine for that).
        if (cursorLineInCurrentText * _numberOfColumns == (Prompt.Length + Text.Length))
          cursorLineInCurrentText--;

        // The line index of the cursor relative to startLine.
        VisualCaretY = lineCount - numberOfLinesInCurrentText + cursorLineInCurrentText - startLine;

        // The column index of the cursor.
        VisualCaretX = Prompt.Length + CaretIndex - cursorLineInCurrentText * _numberOfColumns;

        if (VisualCaretY < 0 || VisualCaretY >= _numberOfLines)
          VisualCaretX = VisualCaretY = -1;
      }

      base.OnRender(context);
    }


    /// <summary>
    /// Computes the visual lines from the console text lines.
    /// </summary>
    /// <remarks>
    /// Text lines are split into several lines if they are larger than _numberOfColumns
    /// or if they contain '\n'.
    /// </remarks>
    private void WrapLines()
    {
      _wrappedLines.Clear();

      // Temporarily, add text that is currently entered to the text lines.
      TextLines.Add(Prompt + Text);

      // Go through TextLines and split lines at if they are to large or contains newlines
      // characters.
      StringBuilder builder = new StringBuilder();
      foreach (string line in TextLines)
      {
        builder.Clear();
        int lineLength = line.Length;
        if (lineLength == 0)
        {
          // Empty line.
          _wrappedLines.Add(string.Empty);
          continue;
        }

        // Go through line character by character.
        for (int i = 0; i < lineLength; i++)
        {
          char c = line[i];
          if (c == '\n')                                // ----- A newline character.
          {
            _wrappedLines.Add(builder.ToString());
            builder.Clear();
          }
          else if (builder.Length < _numberOfColumns)   // ----- A character that fits onto the line.
          {
            builder.Append(c);
            if (i == lineLength - 1)
            {
              // Finish line if this is the last character.
              _wrappedLines.Add(builder.ToString());
              builder.Clear();
            }
          }
          else if (builder.Length >= _numberOfColumns)  // ----- Reached the console column limit.
          {
            // Wrap line.
            _wrappedLines.Add(builder.ToString());
            builder.Clear();
            i--;  // Handle the character again in the next loop.
          }
        }
      }

      // Remove the temporarily added current text.
      TextLines.RemoveAt(TextLines.Count - 1);
    }


    /// <summary>
    /// Computes the index of the first visible line in _wrappedLines.
    /// </summary>
    private int ComputeStartWrappedLine()
    {
      return Math.Max(0, _wrappedLines.Count - _numberOfLines - LineOffset);
    }


    /// <summary>
    /// Removes the current <see cref="Text"/> from the console and copies it to the clipboard.
    /// </summary>
    public void Cut()
    {
      if (Text.Length == 0)
        return;

      if (PlatformHelper.IsClipboardSupported)
        PlatformHelper.SetClipboardText(Text.ToString());
      else
        TextBox.ClipboardData = Text.ToString();

      Text.Clear();
      CaretIndex = 0;
      InvalidateArrange();
    }


    /// <summary>
    /// Copies the current <see cref="Text"/> to the clipboard.
    /// </summary>
    public void Copy()
    {
      if (Text.Length == 0)
        return;

      if (PlatformHelper.IsClipboardSupported)
        PlatformHelper.SetClipboardText(Text.ToString());
      else
        TextBox.ClipboardData = Text.ToString();

      InvalidateArrange();
    }


    /// <summary>
    /// Pastes the contents of the clipboard into the current <see cref="Text"/>
    /// </summary>
    public void Paste()
    {
      string data;
      if (PlatformHelper.IsClipboardSupported)
        data = PlatformHelper.GetClipboardText();
      else
        data = TextBox.ClipboardData;

      data = data.Replace("\n", string.Empty);

      Text.Insert(CaretIndex, data);
      CaretIndex += data.Length;
      InvalidateArrange();
    }
    #endregion
  }
}
