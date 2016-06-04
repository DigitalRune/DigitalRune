#if STORAGE_READ_WRITE

// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;


namespace DigitalRune.Storages
{
  /// <summary>
  /// Specifies how the storage should open a file.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This is a redefinition of the <strong>System.IO.FileMode</strong> enumeration, which is 
  /// missing on certain platforms.
  /// </para>
  /// <para>
  /// Use <see cref="StorageHelper.FromSystemIO(System.IO.FileMode)"/> and 
  /// <see cref="StorageHelper.ToSystemIO(DigitalRune.Storages.FileMode)"/> to convert between 
  /// <strong>DigitalRune.Storages</strong> and <strong>System.IO</strong>.
  /// </para>
  /// </remarks>
  [Serializable]
  public enum FileMode
  {
    // FileMode.Create is equivalent to requesting that if the file does not exist, use 
    // FileMode.CreateNew; otherwise, use FileMode.Truncate.

    /// <summary>
    /// Creates a new file. If the file already exists, an <see cref="IOException"/> is thrown.
    /// </summary>
    CreateNew = 1,

    /// <summary>
    /// Creates a new file. If the file already exists, it will be overwritten. 
    /// </summary>
    Create = 2,

    /// <summary>
    /// Opens an existing file. A <see cref="FileNotFoundException"/> exception is thrown if the
    /// file does not exist.
    /// </summary>
    Open = 3,

    /// <summary>
    /// Opens a file if it exists; otherwise, creates a new file.
    /// </summary>
    OpenOrCreate = 4,

    // Opens an existing file. Once opened, the file is truncated so that its
    // size is zero bytes. The calling process must open the file with at least
    // WRITE access. An exception is raised if the file does not exist.
    /// <summary>
    /// Opens an existing file. When the file is opened, it is truncated so that its size is zero 
    /// bytes. Attempts to read from a file opened with <see cref="Truncate"/> cause an 
    /// <see cref="ArgumentException"/> exception.
    /// </summary>
    Truncate = 5,

    // Opens the file if it exists and seeks to the end.  Otherwise, 
    // creates a new file.
    /// <summary>
    /// Opens the file if it exists and seeks to the end of the file, or creates a new file.
    /// <see cref="Append"/> can be used only in conjunction with <see cref="FileAccess.Write"/>.
    /// Trying to seek to a position before the end of the file throws an <see cref="IOException"/>,
    /// and any attempt to read fails and throws a <see cref="NotSupportedException"/>.
    /// </summary>
    Append = 6,
  }
}
#endif
