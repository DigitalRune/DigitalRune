// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using DigitalRune.Ionic.Zip;


namespace DigitalRune.Storages
{
  /// <summary>
  /// Provides access to the files stored in a ZIP archive.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="ZipStorage"/> does not directly read the ZIP archive from the OS file system.
  /// Instead, it opens the ZIP archive from another storage.
  /// </para>
  /// <para>
  /// The <see cref="PasswordCallback"/> needs to be set to read encrypted ZIP archives. The
  /// <see cref="ZipStorage"/> supports ZipCrypto (all platforms) and AES-256 encryption (Windows
  /// only).
  /// </para>
  /// <para>
  /// <strong>Thread-Safety:</strong> The <see cref="ZipStorage"/> is thread-safe. ZIP entries can
  /// be read simultaneously from one or multiple threads.
  /// </para>
  /// </remarks>
  public class ZipStorage : Storage, IStorageInternal
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly object _lock = new object();
    private readonly Stream _zipStream;
    private readonly ZipFile _zipFile;

#if ANDROID
    // A list of all temp files created in this session.
    private static List<string> _tempFiles = new List<string>();
#endif
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
    /// Gets the file name (incl. path) of the ZIP archive.
    /// </summary>
    /// <value>The file name (incl. path) of the ZIP archive.</value>
    public string FileName { get; private set; }


    /// <summary>
    /// Gets the storage that provides the ZIP archive.
    /// </summary>
    /// <value>The storage that provides the ZIP archive.
    /// </value>
    public Storage Storage { get; private set; }


    /// <summary>
    /// Gets or sets the callback method that provides the password for encrypted ZIP file entries.
    /// </summary>
    /// <value>
    /// The callback method that provides the password for encrypted ZIP file entries.
    /// </value>
    /// <remarks>
    /// The callback is a function which takes one string argument and returns a string.
    /// The function argument is the path of the entry that should be retrieved from the ZIP
    /// archive. The function returns the password that was used to protect the entry in the ZIP
    /// archive. The method may return any value (including <see langword="null"/> or ""), if the
    /// ZIP entry is not encrypted.
    /// </remarks>
    public Func<string, string> PasswordCallback { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ZipStorage"/> class.
    /// </summary>
    /// <param name="storage">The storage that contains the ZIP archive.</param>
    /// <param name="fileName">The file name (incl. path) of the ZIP archive.</param>
    /// <remarks>
    /// An exception is raised if the ZIP archive could not be opened.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="storage"/> or <paramref name="fileName"/> is null.
    /// </exception>
    public ZipStorage(Storage storage, string fileName)
    {
      if (storage == null)
        throw new ArgumentNullException("storage");
      if (fileName == null)
        throw new ArgumentNullException("fileName");

      Storage = storage;
      FileName = fileName;

      _zipStream = storage.OpenFile(fileName);

      try
      {
        _zipFile = ZipFile.Read(_zipStream);
        return;
      }
      catch
      {
        _zipStream.Dispose();
#if !ANDROID
        throw;
#endif
      }

#if ANDROID
      // Android asset streams do not support Stream.Length/Position or seeking.
      // We need to copy the asset first to normal file.
      string tempFileName = storage.GetRealPath(fileName) ?? fileName;
      tempFileName = tempFileName.Replace('\\', '_');
      tempFileName = tempFileName.Replace('/', '_');
      tempFileName = "DigitalRune_Temp_" + tempFileName;
      tempFileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), tempFileName);

      // We copy the files once for each session because the asset files could have change. 
      // To check if a file has changed, we would have to compare the whole file because ZIP files
      // store their dictionary at the end of the file. :-(
      // Filenames of the temp files are stored in a list. Other ZipStorages for the same
      // file will use the existing temp file.
      lock(_tempFiles)
      {
        if (!_tempFiles.Contains(tempFileName))
        {
          _tempFiles.Add(tempFileName);
          using (_zipStream = storage.OpenFile(fileName))
          {
            using (var dest = File.Create(tempFileName))
              _zipStream.CopyTo(dest);
          }
        }
      }

      _zipStream = File.OpenRead(tempFileName);
      try
      {
        _zipFile = ZipFile.Read(_zipStream);
      }
      catch
      {
        _zipStream.Dispose();
        throw;
      }
#endif
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="ZipStorage"/> class
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources;
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
          _zipFile.Dispose();
          _zipStream.Dispose();
        }
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override string GetRealPath(string path)
    {
      path = StorageHelper.NormalizePath(path);
      var zipEntry = _zipFile[path];
      if (zipEntry != null && !zipEntry.IsDirectory)
        return Storage.GetRealPath(FileName);

      return null;
    }


    /// <inheritdoc/>
    public override Stream OpenFile(string path)
    {
      var stream = TryOpenFile(path);
      if (stream != null)
        return stream;

#if SILVERLIGHT || WP7 || XBOX || PORTABLE 
      throw new FileNotFoundException("The file was not found in the ZIP archive.");
#else
      throw new FileNotFoundException("The file was not found in the ZIP archive.", path);
#endif
    }


    /// <inheritdoc/>
    Stream IStorageInternal.TryOpenFile(string path)
    {
      return TryOpenFile(path);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing MemoryStream.")]
    private Stream TryOpenFile(string path)
    {
      // The ZIP file is loaded as a single stream (_zipStream). Streams are not
      // thread-safe and require locking.
      lock (_lock)
      {
        // The current thread may read multiple ZIP entries simultaneously.
        // Example: The ContentManager reads "Model.xnb". While the "Model.xnb"
        // is still open, the ContentManager starts to read "Texture.xnb".
        // --> ZIP entries need to be copied into a temporary memory stream.
        var zipEntry = _zipFile[path];
        if (zipEntry != null && !zipEntry.IsDirectory)
        {
          string password = (PasswordCallback != null) ? PasswordCallback(path) : null;

          // Extract ZIP entry to memory.
          var uncompressedStream = new MemoryStream((int)zipEntry.UncompressedSize);
          zipEntry.ExtractWithPassword(uncompressedStream, password);

          // Reset memory stream for reading.
          uncompressedStream.Position = 0;

          return uncompressedStream;
        }

        return null;
      }
    }
    #endregion
  }
}
