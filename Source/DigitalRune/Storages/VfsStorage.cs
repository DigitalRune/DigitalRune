// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;


namespace DigitalRune.Storages
{
  /// <summary>
  /// Provides a virtual file system that maps existing storages into a virtual directory hierarchy.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The class defines a <see href="http://en.wikipedia.org/wiki/Virtual_file_system">"virtual file
  /// system"</see> to transparently access files from multiple storage locations.
  /// </para>
  /// <para>
  /// The <see cref="VfsStorage"/> is empty by default. Storages can be mounted into the virtual
  /// files system by adding a <see cref="VfsMountInfo"/> entry to the <see cref="MountInfos"/>
  /// property.
  /// </para>
  /// <para>
  /// The <see cref="VfsStorage"/> does not exclusively own the mounted storages. Storages can be
  /// shared between different virtual file systems. A storage may even be mounted multiple times
  /// at different mount points into the same virtual file system. The storages are not disposed
  /// when the <see cref="VfsStorage"/> is disposed.
  /// </para>
  /// <note type="warning">
  /// <para>
  /// Recursively mounting a <see cref="VfsStorage"/> to itself can lead to an endless loop when 
  /// searching for a files.
  /// </para>
  /// </note>
  /// <para>
  /// The <see cref="VfsStorage"/> can be used to virtualize access to different devices to improve
  /// performance. For example, game content provided on a DVD can (optionally) be installed on
  /// harddisk and cached in memory for faster access. The storages managing the content locations
  /// (memory, harddisk, DVD) can be mounted at the same mount point to the virtual file system.
  /// </para>
  /// <para>
  /// <strong>Search Order:</strong><br/>
  /// The order of the storages in <see cref="MountInfos"/> determines the search order for files
  /// and directories. A file will be read from the first storage in the list that contains a
  /// matching entry.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class VfsStorage : Storage, IStorageInternal
  {
    // Notes:
    // - System.IO accepts both '\' and '/' as directory separators.
    // - Windows.Storage (WinRT) accepts only '\' as directory separator!
    // - XNA TitleContainer supports both '\' and '/' as directory separators.

    // References: 
    // Virtual File System, http://en.wikipedia.org/wiki/Virtual_file_system
    // MonoGame\MonoGame.Framework\TitleContainer.cs


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override char DirectorySeparator
    {
      get { return '/'; }
    }


    /// <summary>
    /// Gets the storage providers mounted into the virtual file system.
    /// </summary>
    /// <value>The storage providers mounted into the virtual file system.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public VfsMountInfoCollection MountInfos { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="VfsStorage"/> class.
    /// </summary>
    public VfsStorage()
    {
      MountInfos = new VfsMountInfoCollection();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override string GetRealPath(string path)
    {
      path = StorageHelper.NormalizePath(path);
      foreach (var mountInfo in MountInfos)
      {
        if (path.StartsWith(mountInfo.MountPoint, StringComparison.Ordinal))
        {
          string relativePath = path.Substring(mountInfo.MountPoint.Length);
          string realPath = mountInfo.Storage.GetRealPath(relativePath);
          if (realPath != null)
            return realPath;
        }
      }

      return null;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    public override Stream OpenFile(string path)
    {
      var stream = TryOpenFile(path);
      if (stream != null)
        return stream;

#if SILVERLIGHT || WP7 || XBOX || PORTABLE
      throw new FileNotFoundException("The file was not found in the virtual file system.");
#else
      throw new FileNotFoundException("The file was not found in the virtual file system.", path);
#endif
    }


    /// <inheritdoc/>
    Stream IStorageInternal.TryOpenFile(string path)
    {
      return TryOpenFile(path);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    private Stream TryOpenFile(string path)
    {
      path = StorageHelper.NormalizePath(path);
      foreach (var mountInfo in MountInfos)
      {
        if (path.StartsWith(mountInfo.MountPoint, StringComparison.Ordinal))
        {
          string relativePath = path.Substring(mountInfo.MountPoint.Length);
          try
          {
            var storage = mountInfo.Storage;
            var storageInternal = storage as IStorageInternal;
            var stream = (storageInternal != null) ? storageInternal.TryOpenFile(relativePath) : storage.OpenFile(relativePath);
            if (stream != null)
              return stream;
          }
          // ReSharper disable once EmptyGeneralCatchClause
          catch
          {
            // Swallow exception. Try next storage provider.
          }
        }
      }

      return null;
    }
    #endregion
  }
}
