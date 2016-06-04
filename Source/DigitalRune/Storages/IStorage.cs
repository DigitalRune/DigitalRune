// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;


namespace DigitalRune.Storages
{
#pragma warning disable 1574

  // Workaround:
  // Using IStorage.OpenFile() in a VfsStorage may raise many FileNotFoundExceptions,
  // which may be reported as first chance exceptions. To avoid first chance exceptions
  // we internally use the TryOpenFile(string path).
  // Once all mobile platforms support some form of FileExists(), we can decide whether
  // to add TryOpenFile() or FileExists() to the API. For now, we keep the simple API.
  internal interface IStorageInternal
  {
    /// <summary>
    /// Opens the specified file for reading.
    /// </summary>
    /// <param name="path">The file to open.</param>
    /// <returns>
    /// A <see cref="Stream"/> object for reading the file. Returns <see langword="null"/> if file
    /// does not exist.
    /// </returns>
    Stream TryOpenFile(string path);
  }


  /// <summary>
  /// Provides access to files.
  /// </summary>
  /// <remarks>
  /// 
  /// <note type="important">
  /// <para>
  /// <see cref="DigitalRune.Storages"/> currently only provides read access to files! Full 
  /// read/write access, as well as support for additional platforms, will be added in a future
  /// version.
  /// </para>
  /// </note>
  /// 
  /// <para>
  /// The <see cref="IStorage"/> interface provides a common API to access files from different
  /// sources. The following implementations provide access to physical file systems on different
  /// platforms:
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// <term><see cref="FileSystemStorage"/></term>
  /// <description>provides access to the file system of the operating system.</description>
  /// </item>
  /// <item>
  /// <term><see cref="T:DigitalRune.Storages.TitleStorage"/></term>
  /// <description>provides access to title storage on Xbox 360.</description>
  /// </item>
  /// <item>
  /// <term>(Not yet implemented) UserStorage</term>
  /// <description>provides access to user storage on Xbox 360.</description>
  /// </item>
  /// <item>
  /// <term>(Not yet implemented) IsolatedStorage</term>
  /// <description>provides access to isolated storage in Silverlight.</description>
  /// </item>
  /// <item>
  /// <term>(Not yet implemented) WindowsStorage</term>
  /// <description>provides access to storage folders in Windows Store apps.</description>
  /// </item>
  /// </list>
  /// <para>
  /// Some storages are built on top of other storages. For example:
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// <term><see cref="VfsStorage"/></term>
  /// <description>
  /// maps existing storages into a virtual file system. Different storage devices and locations can
  /// be treated as one directory hierarchy.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="ZipStorage"/></term>
  /// <description>
  /// provides access to files stored in a ZIP archive. The ZIP archive can be read from any of the
  /// existing storages.
  /// </description>
  /// </item>
  /// </list>
  /// 
  /// <para>
  /// <strong>Case-Sensitivity:</strong><br/>
  /// File retrieval is case-sensitive if the storage provider (e.g. the platform OS) is
  /// case-sensitive. It is recommended to assume case-sensitivity to ensure that applications can
  /// be ported to non-Windows platforms.
  /// </para>
  /// <para>
  /// <strong>Directory Separator:</strong><br/>
  /// Storages accepts '\' and '/' as directory separators. Internally, paths are normalized to use
  /// '/'.
  /// </para>
  /// 
  /// <para>
  /// <strong>Possible Extensions:</strong><br/>
  /// The <see cref="IStorage"/> concept is highly extensible. Developers can provide custom 
  /// <see cref="IStorage"/> implementations to add support for new platforms or manipulate existing
  /// storages. Here are just a few features that could be implemented on top of 
  /// <see cref="IStorage"/>:
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// <term>Access control</term>
  /// <description>
  /// A storage may wrap another storage and implement access control to restrict user access or
  /// filter certain files. For example, a "ReadOnlyStorage" may prevent write access to an existing
  /// location.
  /// </description>
  /// </item>
  /// <item>
  /// <term>Archives</term>
  /// <description>
  /// Storages can be added to support other package formats, such as 7-Zip, BZIP2, PAK.
  /// </description>
  /// </item>
  /// <item>
  /// <term>Caching ("CachedStorage")</term>
  /// <description>
  /// Files from another storage could be cached in memory for faster access.
  /// </description>
  /// </item>
  /// <item>
  /// <term>Cloud storage</term>
  /// <description>A storage may access data in the cloud, such as OneDrive.</description>
  /// </item>
  /// <item>
  /// <term>Encryption ("EncryptedStorage")</term>
  /// <description>
  /// Data could be encrypted and decrypted when accessing files in an existing storage.
  /// </description>
  /// </item>
  /// <item>
  /// <term>Mapping ("RedirectStorage")</term>
  /// <description>Directories can transparently be mapped to different location.</description>
  /// </item>
  /// <item>
  /// <term>Redundancy ("MirroredStorage")</term>
  /// <description>File access could be mirrored across different storages.</description>
  /// </item>
  /// <item>
  /// <term>Ad-hoc storage</term>
  /// <description>
  /// Instead of accessing files on an existing devices or locations, files can be stored in custom
  /// data structures (DBs, BLOBs, ...).
  /// </description>
  /// </item>
  /// </list>
  /// </remarks>
  public interface IStorage : IDisposable
#pragma warning restore 1574
  {
    // Implementation details:
    // The interface IStorage provides all fundamental file operations.
    // The StorageHelper provides extensions method for derived operations.
    // The abstract base class Storage provides virtual methods for derived methods
    // in case a derived type contains a more efficient implementation.

    // Example:
    // CopyDirectory() is not a fundamental operation and not directly supported by
    // all storage providers.
    // StorageHelper provides an extension method for CopyDirectory() which is built
    // on the methods in IStorage. It checks whether the storage provider is derived
    // from Storage, in which case Storage.CopyDirectory() is called.
    // The base class Storage provides a virtual method CopyDirectory(). The base
    // implementation calls the StorageHelper.CopyDirectoryInternal() method.
    // Derived storage providers can override CopyDirectory() if a more efficient
    // implementation exists.


    /// <summary>
    /// Gets the real path and name of the specified file.
    /// </summary>
    /// <param name="path">The file to check.</param>
    /// <returns>
    /// The path where the specified file is located. If the file is located inside an archive, the
    /// path and name of the archive is returned; otherwise, <see langword="null"/> if the file was
    /// not found.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Storages, such as the <see cref="VfsStorage"/>, can be used to virtualize access to
    /// resources. File and directories from different location can be mapped into a virtual
    /// directory hierarchy. The method <see cref="GetRealPath"/> can be used to resolve the actual
    /// source of a file.
    /// </para>
    /// <para>
    /// This method can only be used to query for files, but not directories. (Multiple directories
    /// may be mapped to the same virtual path.)
    /// </para>
    /// <note type="warning">
    /// <para>
    /// Some storages hide the actual file location and may return <see langword="null"/> even if
    /// the file exists. The files inside the storage can still be opened with <see cref="OpenFile"/>
    /// but the real location is concealed. Therefore, <see cref="GetRealPath"/> cannot be used to
    /// check if a file exists inside a storage.
    /// </para>
    /// </note>
    /// </remarks>
    string GetRealPath(string path);


    /// <summary>
    /// Opens the specified file for reading.
    /// </summary>
    /// <param name="path">The file to open.</param>
    /// <returns>A <see cref="Stream"/> object for reading the file.</returns>
    Stream OpenFile(string path);


#if STORAGE_READ_WRITE

    // TODO: Failure behavior needs to be defined:
    // What happens if the file or directory does not exist? What happens if the
    // storage does not provide this functionality?
    // - Fail silently for non-critical operations?
    // - Throw NotSupportedException for critical operations?
    // - Make failure behavior configurable?

    // TODO: Document exceptions.

    // Notes:
    // Get/SetCurrentDirectory(path) is not supported by IStorage.This can be implemented
    // by creating a new storage that wraps an existing storage and adjusts the path.


    /// <summary>
    /// Gets a value indicating whether this storage is read only.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this storage is read only; otherwise, <see langword="false"/> if
    /// storage supports read and write.
    /// </value>
    bool IsReadOnly { get; }


    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    /// <param name="path">The file to check.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="path"/> is valid, the file exists and the
    /// caller has sufficient permissions to access the file; otherwise, <see langword="false"/>.
    /// </returns>
    bool FileExists(string path);


    /// <summary>
    /// Determines whether the specified directory exists.
    /// </summary>
    /// <param name="path">The directory to check.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="path"/> is valid, the directory exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool DirectoryExists(string path);


    /// <summary>
    /// Gets the names of files (without paths) in the specified directory.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <returns>
    /// The names (without paths) of the files in the specified directory, or an empty sequence if
    /// no files are found.
    /// </returns>
    IEnumerable<string> GetFiles(string path);


    /// <summary>
    /// Gets the names of subdirectories (without paths) in the specified directory.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <returns>
    /// The names (without paths) of the subdirectories in the specified directory, or an empty 
    /// sequence if no subdirectories are found.
    /// </returns>
    IEnumerable<string> GetDirectories(string path);


    /// <summary>
    /// Gets the names (without paths) of files and subdirectories in the specified directory.
    /// </summary>
    /// <param name="path">The directory to search.</param>
    /// <returns>
    /// The names (without paths) of the files and subdirectories in the specified directory, or an
    /// empty sequence if no entries are found.
    /// </returns>
    IEnumerable<string> GetEntries(string path);


    /// <summary>
    /// Creates the file at the specified path.
    /// </summary>
    /// <param name="path">The name of the file to create.</param>
    /// <param name="bufferSize">
    /// The number of bytes buffered for reads and writes to the file.
    /// </param>
    /// <param name="options">Options for creating the file.</param>
    /// <returns>A <see cref="Stream"/> object for accessing the newly created file.</returns>
    Stream CreateFile(string path, int bufferSize, FileOptions options);


    /// <summary>
    /// Creates all directories and subdirectories in the specified path.
    /// </summary>
    /// <param name="path">The directory to create.</param>
    void CreateDirectory(string path);


    /// <summary>
    /// Opens the specified file.
    /// </summary>
    /// <param name="path">The file to open.</param>
    /// <param name="mode">The mode for opening the file (create, open, truncate, etc.).</param>
    /// <param name="access">The file access (read, write).</param>
    /// <param name="share">The file share settings.</param>
    /// <returns>A <see cref="Stream"/> object for accessing the file.</returns>
    Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share);


    /// <summary>
    /// Copies the specified file to a new location.
    /// </summary>
    /// <param name="sourcePath">The file to copy.</param>
    /// <param name="destinationPath">The destination path and name of the file.</param>
    /// <param name="overwrite">
    /// <see langword="true"/> to overwrite an existing destination file.
    /// </param>
    void CopyFile(string sourcePath, string destinationPath, bool overwrite);


    /// <summary>
    /// Deletes the specified file.
    /// </summary>
    /// <param name="path">The file to delete.</param>
    void DeleteFile(string path);


    /// <summary>
    /// Deletes the specified directory. The directory must be empty.
    /// </summary>
    /// <param name="path">The directory to delete.</param>
    void DeleteDirectory(string path);


    /// <summary>
    /// Moves the specified file to a new location. (Can be used to rename a file.)
    /// </summary>
    /// <param name="sourcePath">The file to move</param>
    /// <param name="destinationPath">The new path of the file.</param>
    void MoveFile(string sourcePath, string destinationPath);


    /// <summary>
    /// Moves the specified directory (incl. sub entries) to a new location. (Can be used to rename
    /// a directory.)
    /// </summary>
    /// <param name="oldPath">The directory to move.</param>
    /// <param name="newPath">The new path of the directory.</param>
    void MoveDirectory(string oldPath, string newPath);


    // ----- Advanced functionality (probably not supported on all platforms):

    /// <summary>
    /// Gets the attributes of the specified file.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The attributes of the file on the path.</returns>
    FileAttributes GetFileAttributes(string path);


    /// <summary>
    /// Sets the attributes of the specified file.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <param name="attributes">The attributes of the file on the path.</param>
    void SetAttributes(string path, FileAttributes attributes);


    /// <summary>
    /// Gets the creation date and time of the specified file or directory.
    /// </summary>
    /// <param name="path">
    /// The file or directory for which to obtain creation date and time information.
    /// </param>
    /// <returns>
    /// The time that the file or directory was created. This value is expressed in local time.
    /// </returns>
    DateTimeOffset GetCreationTime(string path);


    /// <summary>
    /// Sets the date and time the specified file or directory was created.
    /// </summary>
    /// <param name="path">
    /// The file or directory for which to set the creation date and time information.
    /// </param>
    /// <param name="time">
    /// The date and time the file or directory was last created. This value is expressed in local
    /// time.
    /// </param>
    void SetCreationTime(string path, DateTimeOffset time);


    /// <summary>
    /// Gets the time the specified file or directory was last accessed.
    /// </summary>
    /// <param name="path">
    /// The file or directory for which to obtain access date and time information.
    /// </param>
    /// <returns>
    /// The time that the file or directory was last accessed. This value is expressed in local
    /// time.
    /// </returns>
    DateTimeOffset GetLastAccessTime(string path);


    /// <summary>
    /// Sets the date and time the specified file or directory was last accessed.
    /// </summary>
    /// <param name="path">
    /// The file or directory for which to set the access date and time information.
    /// </param>
    /// <param name="time">
    /// The time that the file or directory was last accessed. This value is expressed in local
    /// time.
    /// </param>
    void SetLastAccessTime(string path, DateTimeOffset time);


    /// <summary>
    /// Gets the date and time the specified file or directory was last written to.
    /// </summary>
    /// <param name="path">
    /// The file or directory for which to obtain the modification date and time information.
    /// </param>
    /// <returns>
    /// The time that the file or directory was last written to. This value is expressed in local 
    /// time.
    /// </returns>
    DateTimeOffset GetLastWriteTime(string path);


    /// <summary>
    /// Gets the date and time the specified file or directory was last written to.
    /// </summary>
    /// <param name="path">
    /// The file or directory for which to set the modification date and time information.
    /// </param>
    /// <param name="time">
    /// The time that the file or directory was last written to. This value is expressed in local 
    /// time.
    /// </param>
    void SetLastWriteTime(string path, DateTimeOffset time);


    /// <summary>
    /// Gets the size of the specified file in bytes.
    /// </summary>
    /// <param name="path">The file for which to determine the size.</param>
    /// <returns>The size of the file in bytes.</returns>
    long GetFileSize(string path);
#endif
  }
}
