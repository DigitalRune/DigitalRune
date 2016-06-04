// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game.UI.Consoles
{
  /// <summary>
  /// Represents an interactive console for debugging.
  /// </summary>
  /// <remarks>
  /// <para>
  /// If the user enters a command, the <see cref="CommandEntered"/> event is raised. If this event 
  /// is not handled (see <see cref="ConsoleCommandEventArgs.Handled"/>), the 
  /// <see cref="Interpreter"/> handles the command. 
  /// </para>
  /// <para>
  /// To add new commands, you can either handle the <see cref="CommandEntered"/> event, or
  /// add new commands to the <see cref="Interpreter"/> (see 
  /// <see cref="ConsoleCommandInterpreter.Commands"/>).
  /// </para>
  /// </remarks>
  public interface IConsole
  {
    /// <summary>
    /// Gets or sets the prompt text.
    /// </summary>
    /// <value>The prompt text. The default is "&gt; "</value>
    string Prompt { get; set; }


    /// <summary>
    /// Gets the default command interpreter.
    /// </summary>
    /// <value>The default command interpreter.</value>
    ConsoleCommandInterpreter Interpreter { get; }


    /// <summary>
    /// Event raised after a command was entered.
    /// </summary>
    /// <remarks>
    /// If an event handler handles a command, it must set the 
    /// <see cref="ConsoleCommandEventArgs.Handled"/> flag. Event handlers should check if this flag
    /// was already set by another handler. In some cases it make sense that more than one event
    /// handler handle a single command, for example: When "help" command was entered, all event
    /// handlers can output their help information.
    /// </remarks>
    event EventHandler<ConsoleCommandEventArgs> CommandEntered;


    /// <summary>
    /// Clears the console.
    /// </summary>
    void Clear();


    /// <summary>
    /// Writes an empty line in the console.
    /// </summary>
    void WriteLine();


    /// <summary>
    /// Writes a line of text in the console.
    /// </summary>
    /// <param name="text">The text.</param>
    void WriteLine(string text);
  }
}
