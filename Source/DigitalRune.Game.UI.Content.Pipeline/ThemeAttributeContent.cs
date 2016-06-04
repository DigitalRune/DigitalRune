// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.UI.Content.Pipeline
{
  /// <summary>
  /// Represents an attribute of the UI theme (<see cref="Name"/> and <see cref="Value"/>).
  /// </summary>
  public class ThemeAttributeContent : INamedObject
  {
    /// <summary>
    /// Gets or sets the name of the attribute.
    /// </summary>
    /// <value>The name of the attribute.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the value of the attribute.
    /// </summary>
    /// <value>The value of the attribute.</value>
    public string Value { get; set; }
  }
}
