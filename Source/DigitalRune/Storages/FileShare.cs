#if STORAGE_READ_WRITE

// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Storages
{
  /// <summary>
  /// Controls how file access is shared with other processes when trying to open the same file.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This is a redefinition of the <strong>System.IO.FileShare</strong> enumeration, which is 
  /// missing on certain platforms.
  /// </para>
  /// <para>
  /// Use <see cref="StorageHelper.FromSystemIO(System.IO.FileShare)"/> and 
  /// <see cref="StorageHelper.ToSystemIO(DigitalRune.Storages.FileShare)"/> to convert between 
  /// <strong>DigitalRune.Storages</strong> and <strong>System.IO</strong>.
  /// </para>
  /// </remarks>
  [Serializable]
  [Flags]
  public enum FileShare
  {
    /// <summary>
    /// No sharing. Any request to open the file (by this process or 
    /// another process) will fail until the file is closed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Allows subsequent opening of the file for reading. If this flag is not specified, any
    /// request to open the file for reading (by this process or another process) will fail until
    /// the file is closed.
    /// </summary>
    Read = 1,

    /// <summary>
    /// Allows subsequent opening of the file for writing. If this flag is not specified, any
    /// request to open the file for writing (by this process or another process) will fail until
    /// the file is closed.
    /// </summary>
    Write = 2,

    // Allows subsequent opening of the file for writing or reading. If this flag
    // is not specified, any request to open the file for writing or reading (by
    // this process or another process) will fail until the file is closed.
    /// <summary>
    /// Allows subsequent opening of the file for reading or writing. If this flag is not specified,
    /// any request to open the file for reading or writing (by this process or another process)
    /// will fail until the file is closed.
    /// </summary>
    ReadWrite = 3,

    // Not supported: 
    //Delete = 4,
    //Inheritable = 0x10,
  }
}
#endif
