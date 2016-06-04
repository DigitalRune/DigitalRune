#if STORAGE_READ_WRITE

// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Storages
{
  /// <summary>
  /// Specifies advanced options for creating a file.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This is a redefinition of the <strong>System.IO.FileOptions</strong> enumeration, which is 
  /// missing on certain platforms.
  /// </para>
  /// <para>
  /// Use <see cref="StorageHelper.FromSystemIO(System.IO.FileOptions)"/> and 
  /// <see cref="StorageHelper.ToSystemIO(DigitalRune.Storages.FileOptions)"/> to convert between 
  /// <strong>DigitalRune.Storages</strong> and <strong>System.IO</strong>.
  /// </para>
  /// </remarks>
  [Serializable]
  [Flags]
  public enum FileOptions
  {
    /// <summary>
    /// Indicates that no additional options should be used when creating a file.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates that the system should write through any intermediate cache and go directly to
    /// disk.
    /// </summary>
    WriteThrough = unchecked((int)0x80000000),

    /// <summary>
    /// Indicates that a file can be used for asynchronous reading and writing.
    /// </summary>
    Asynchronous = unchecked((int)0x40000000),

    /// <summary>
    /// Indicates that the file is accessed randomly. The system can use this as a hint to optimize
    /// file caching.
    /// </summary>
    RandomAccess = 0x10000000,

    /// <summary>
    /// Indicates that a file is automatically deleted when it is no longer in use.
    /// </summary>
    DeleteOnClose = 0x04000000,

    /// <summary>
    /// Indicates that the file is to be accessed sequentially from beginning to end. The system can
    /// use this as a hint to optimize file caching. If an application moves the file pointer for
    /// random access, optimum caching may not occur; however, correct operation is still
    /// guaranteed. Specifying this flag can increase performance for applications that read large
    /// files using sequential access. Performance gains can be even more noticeable for
    /// applications that read large files mostly sequentially, but occasionally skip over small
    /// ranges of bytes.
    /// </summary>
    SequentialScan = 0x08000000,

    /// <summary>
    /// Indicates that a file is encrypted and can be decrypted only by using the same user account
    /// used for encryption.
    /// </summary>
    Encrypted = 0x00004000,
  }
}
#endif
