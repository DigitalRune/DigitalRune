// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Game.UI.Consoles
{
  /// <summary>
  /// Defines a game console command.
  /// </summary>
  public class ConsoleCommand : INamedObject
  {
    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    /// <value>The name of the command.</value>
    /// <remarks>
    /// Command names are case-insensitive. And they are stored in lower-case.
    /// </remarks>
    public string Name { get; private set; }


    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>The description.</value>
    /// <remarks>
    /// This is the help text of the command.
    /// </remarks>
    public string Description { get; set; }


    /// <summary>
    /// Gets or sets the callback that is called when the command was entered.
    /// </summary>
    /// <value>
    /// The callback that is called when the command was entered. The input parameter for the
    /// callback method is the array of command arguments. (The first argument is the command name.)
    /// </value>
    /// <remarks>
    /// The callback method can throw an exception which will be printed as an error message on the
    /// console.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Action<string[]> Execute
    {
      get { return _execute; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _execute = value;
      }
    }
    private Action<string[]> _execute;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCommand"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCommand"/> class.
    /// </summary>
    /// <param name="name">The name (see <see cref="Name"/>).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is an empty string.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Case is not critical in Console. Preferring lower case.")]
    public ConsoleCommand(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("name must not be an empty string.", "name");

      Name = name.ToLowerInvariant();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCommand"/> class with the given 
    /// properties.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="callback">
    /// The callback method that handles the command (see <see cref="Execute"/>).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> or <paramref name="callback"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is an empty string.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "Case is not critical in Console. Preferring lower case.")]
    public ConsoleCommand(string name, string description, Action<string[]> callback)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("name must not be an empty string.", "name");
      if (callback == null)
        throw new ArgumentNullException("callback");

      Name = name.ToLowerInvariant();
      Description = description;
      Execute = callback;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
