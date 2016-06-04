#if STORAGE_READ_WRITE

// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Storages
{
  /// <summary>
  /// Defines constants for read, write, or read/write access to a file.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This is a redefinition of the <strong>System.IO.FileAccess</strong> enumeration, which is 
  /// missing on certain platforms.
  /// </para>
  /// <para>
  /// Use <see cref="StorageHelper.FromSystemIO(System.IO.FileAccess)"/> and 
  /// <see cref="StorageHelper.ToSystemIO(DigitalRune.Storages.FileAccess)"/> to convert between 
  /// <strong>DigitalRune.Storages</strong> and <strong>System.IO</strong>.
  /// </para>
  /// </remarks>
  [Serializable]
  [Flags]
  public enum FileAccess
  {
    /// <summary>
    /// Read access to the file. Data can be read from the file. Combine with <see cref="Write"/>
    /// for read/write access.
    /// </summary>
    Read = 1,

    /// <summary>
    /// Write access to the file. Data can be written to the file. Combine with <see cref="Read"/>
    /// for read/write access.
    /// </summary>
    Write = 2,

    /// <summary>
    /// Read and write access to the file. Data can be written to and read from the file.
    /// </summary>
    ReadWrite = 3,
  }
}
#endif
