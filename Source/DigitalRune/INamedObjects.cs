// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune
{
  /// <summary>
  /// Represents an objects with a (unique) name.
  /// </summary>
  public interface INamedObject
  {
    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name of the object.</value>
    string Name { get; }
  }
}
