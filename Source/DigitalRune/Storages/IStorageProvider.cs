// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Storages
{
  /// <summary>
  /// Provides access to a <see cref="IStorage"/>.
  /// </summary>
  public interface IStorageProvider
  {
    /// <summary>
    /// Gets the storage.
    /// </summary>
    /// <value>The storage.</value>
    IStorage Storage { get; }
  }
}
