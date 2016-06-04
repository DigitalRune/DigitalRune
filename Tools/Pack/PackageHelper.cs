// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;


namespace DigitalRune.Tools
{
    /// <summary>
    /// Tool for packaging files into ZIP archives.
    /// </summary>
    public class PackageHelper
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether this is a test run. (No files are changed.)
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this is a test run; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsTestRun { get; set; }


        /// <summary>
        /// Gets or sets the output stream for writing log messages.
        /// </summary>
        /// <value>The message writer for writing log messages.</value>
        public TextWriter MessageWriter { get; set; }


        /// <summary>
        /// Gets or sets the password for encrypting the content.
        /// </summary>
        /// <value>
        /// The password for encrypting the content. Can be <see langword="null"/> or empty.
        /// The default value is <see langword="null"/>.
        /// </value>
        public string Password { get; set; }


        /// <summary>
        /// Gets or sets the encryption algorithm.
        /// </summary>
        /// <value>
        /// The encryption algorithm. The default value is <see cref="EncryptionAlgorithm.PkzipWeak"/>.
        /// </value>
        public EncryptionAlgorithm Encryption { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageHelper"/> class.
        /// </summary>
        public PackageHelper()
        {
            Encryption = EncryptionAlgorithm.PkzipWeak;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Asynchronously creates/updates a ZIP archive.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Asynchronously creates/updates a ZIP archive.
        /// </summary>
        /// <param name="baseDirectory">
        /// The base directory. Can be <see langword="null"/> or empty.
        /// </param>
        /// <param name="searchPatterns">
        /// The search patterns relative to the <paramref name="baseDirectory"/> or the current working
        /// directory. May include wildcards ('?', '*').
        /// </param>
        /// <param name="recursive">
        /// If set to <see langword="true"/> all subdirectories will be included in the search.
        /// </param>
        /// <param name="packageFileName">The file name of the ZIP archive.</param>
        public Task PackAsync(string baseDirectory, IEnumerable<string> searchPatterns, bool recursive, string packageFileName)
        {
            return PackAsync(baseDirectory, searchPatterns, recursive, packageFileName, CancellationToken.None);
        }


        /// <summary>
        /// Asynchronously creates/updates a ZIP archive.
        /// </summary>
        /// <param name="baseDirectory">
        /// The base directory. Can be <see langword="null"/> or empty.
        /// </param>
        /// <param name="searchPatterns">
        /// The search patterns relative to the <paramref name="baseDirectory"/> or the current working
        /// directory. May include wildcards ('?', '*').
        /// </param>
        /// <param name="recursive">
        /// If set to <see langword="true"/> all subdirectories will be included in the search.
        /// </param>
        /// <param name="packageFileName">The file name of the ZIP archive.</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is 
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        public Task PackAsync(string baseDirectory, IEnumerable<string> searchPatterns, bool recursive, string packageFileName, CancellationToken cancellationToken)
        {
            return Task.Run(() => Pack(baseDirectory, searchPatterns, recursive, packageFileName, cancellationToken), cancellationToken);
        }


        /// <summary>
        /// Creates/updates a ZIP archive.
        /// </summary>
        /// <param name="baseDirectory">
        /// The base directory. Can be <see langword="null"/> or empty.
        /// </param>
        /// <param name="searchPatterns">
        /// The search patterns relative to the <paramref name="baseDirectory"/> or the current working
        /// directory. May include wildcards ('?', '*').
        /// </param>
        /// <param name="recursive">
        /// If set to <see langword="true"/> all subdirectories will be included in the search.
        /// </param>
        /// <param name="packageFileName">The file name of the ZIP archive.</param>
        /// <param name="cancellationToken">
        /// The token to monitor for cancellation requests. The default value is 
        /// <see cref="CancellationToken.None"/>.
        /// </param>
        public void Pack(string baseDirectory, IEnumerable<string> searchPatterns, bool recursive, string packageFileName, CancellationToken cancellationToken)
        {
            if (searchPatterns == null)
                throw new ArgumentNullException(nameof(searchPatterns));

            baseDirectory = string.IsNullOrEmpty(baseDirectory)
              ? Directory.GetCurrentDirectory()
              : Path.GetFullPath(baseDirectory);

            if (File.Exists(packageFileName))
                WriteLine("Updating existing package \"{0}\".", packageFileName);
            else
                WriteLine("Creating new package \"{0}\".", packageFileName);

            // Create/open ZIP archive.
            using (var zipFile = new ZipFile(packageFileName))
            {
                bool isDirty = false;

                zipFile.CompressionLevel = CompressionLevel.BestCompression;
                zipFile.Password = string.IsNullOrEmpty(Password) ? null : Password;
                zipFile.Encryption = string.IsNullOrEmpty(Password) ? EncryptionAlgorithm.None : Encryption;
                //zipFile.StatusMessageTextWriter = MessageWriter;
                zipFile.ZipErrorAction = ZipErrorAction.Throw;

                zipFile.SaveProgress += (s, e) =>
                                        {
                                            if (cancellationToken.IsCancellationRequested)
                                                e.Cancel = true;

                                    // Note: We could report the saving progress here.
                                };

                // Search for files.
                var fileNames = new List<string>();
                foreach (var searchPattern in searchPatterns)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    fileNames.AddRange(GetFiles(baseDirectory, searchPattern, recursive));
                }

                // Exclude output file in case it is included in the search results.
                fileNames.Remove(Path.GetFullPath(packageFileName));

                // Sort in ascending order.
                fileNames.Sort();

                // Create a copy of the original ZIP entries.
                var originalZipEntries = zipFile.Entries.ToList();

                // Add/update files in ZIP archive.
                foreach (var fileName in fileNames)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var fileInfo = new FileInfo(fileName);
                    string directory = fileInfo.DirectoryName;
                    string directoryInArchive = ChangePath(directory, baseDirectory, string.Empty);
                    string filenameInArchive = Path.Combine(directoryInArchive, fileInfo.Name);
                    filenameInArchive = NormalizePath(filenameInArchive);

                    var zipEntry = zipFile[filenameInArchive];
                    if (zipEntry == null)
                    {
                        // ----- New file.
                        MessageWriter.WriteLine("Adding \"{0}\".", filenameInArchive);
                        isDirty = true;
                        if (!IsTestRun)
                            zipFile.AddFile(fileName, directoryInArchive);
                    }
                    else
                    {
                        // ----- Existing file.
                        originalZipEntries.Remove(zipEntry);
                        if (fileInfo.LastWriteTimeUtc > zipEntry.ModifiedTime // Input file is newer.
                            || fileInfo.Length != zipEntry.UncompressedSize) // Different file size.
                        {
                            MessageWriter.WriteLine("Updating \"{0}\".", filenameInArchive);
                            isDirty = true;
                            if (!IsTestRun)
                                zipFile.UpdateFile(fileName, directoryInArchive);
                        }
                    }
                }

                // Remove obsolete files from ZIP archive.
                foreach (var zipEntry in originalZipEntries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (zipEntry.IsDirectory)
                        continue;

                    MessageWriter.WriteLine("Removing \"{0}\".", zipEntry.FileName);
                    isDirty = true;
                    zipFile.RemoveEntry(zipEntry);
                }

                // Remove empty directories from ZIP archive.
                foreach (var zipEntry in originalZipEntries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!zipEntry.IsDirectory)
                        continue;

                    if (IsEmptyDirectory(zipFile, zipEntry))
                    {
                        MessageWriter.WriteLine("Removing empty directory \"{0}\".", zipEntry.FileName);
                        isDirty = true;
                        zipFile.RemoveEntry(zipEntry);
                    }
                }

                if (isDirty)
                {
                    if (!IsTestRun)
                    {
                        // Save ZIP archive.
                        MessageWriter.WriteLine("Saving package \"{0}\".", packageFileName);
                        zipFile.Save();
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                else
                {
                    // Leave ZIP archive untouched.
                    MessageWriter.WriteLine("Package is up-to-date.");
                }
            }
        }


        /// <summary>
        /// Writes a log message.
        /// </summary>
        /// <param name="format">The message format string.</param>
        /// <param name="args">The message arguments.</param>
        private void WriteLine(string format, params object[] args)
        {
            MessageWriter?.WriteLine(format, args);
        }


        /// <summary>
        /// Searches for files.
        /// </summary>
        /// <param name="searchDirectory">The search directory.</param>
        /// <param name="searchPattern">The search pattern. May include wildcards ('?', '*').</param>
        /// <param name="recursive">
        /// If set to <see langword="true"/> all subdirectories will be included in the search.</param>
        /// <returns>The files (full path) that match the search pattern.</returns>
        private static IEnumerable<string> GetFiles(string searchDirectory, string searchPattern, bool recursive)
        {
            if (string.IsNullOrEmpty(searchDirectory))
                searchDirectory = ".";

            if (string.IsNullOrEmpty(searchPattern))
                searchPattern = "*";

            var searchOptions = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            try
            {
                // Check whether "searchDirectory/searchPattern" matches a directory.
                string path = Path.Combine(searchDirectory, searchPattern);
                if (Directory.Exists(path))
                {
                    // Return contents of directory.
                    return Directory.GetFiles(path, "*", searchOptions);
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // Ignore. searchPattern is not a directory.
            }

            // Apply search pattern in search directory.
            return Directory.GetFiles(searchDirectory, searchPattern, searchOptions);
        }


        /// <summary>
        /// Changes the leading part of a path.
        /// </summary>
        /// <param name="originalPath">The original path of a file or directory.</param>
        /// <param name="oldPath">The old path.</param>
        /// <param name="newPath">The new path.</param>
        /// <returns>The changed path.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="oldPath"/> is removed from <paramref name="originalPath"/> and 
        /// <paramref name="newPath"/> is added.
        /// </para>
        /// <para>
        /// This method does nothing if <paramref name="originalPath"/> does not start with 
        /// <paramref name="oldPath"/>.
        /// </para>
        /// </remarks>
        private static string ChangePath(string originalPath, string oldPath, string newPath)
        {
            if (originalPath.StartsWith(oldPath, StringComparison.OrdinalIgnoreCase))
                return newPath + originalPath.Substring(oldPath.Length);

            return originalPath;
        }


        /// <summary>
        /// Normalizes the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The normalized path.</returns>
        private static string NormalizePath(string path)
        {
            // Switch directory separators.
            path = path.Replace('\\', '/');

            // Reduce "./path" to "path".
            while (path.StartsWith("./", StringComparison.Ordinal))
                path = path.Substring(2);

            // Reduce "pathA/./pathB" to "pathA/pathB".
            path = path.Replace("/./", "/");

            // Trim leading '/'.
            if (path.Length > 0 && path[0] == '/')
                path = path.Substring(1);

            // Trim trailing '/'.
            while (path.Length > 0 && path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            return path;
        }


        /// <summary>
        /// Determines whether a specific ZIP entry is an empty directory.
        /// </summary>
        /// <param name="zipFile">The ZIP archive.</param>
        /// <param name="zipEntry">The ZIP entry to check.</param>
        /// <returns>
        /// <see langword="true"/> is <paramref name="zipEntry"/> is an empty directory; 
        /// otherwise, <see langword="false"/>.
        /// </returns>
        private static bool IsEmptyDirectory(ZipFile zipFile, ZipEntry zipEntry)
        {
            if (!zipEntry.IsDirectory)
                return false;

            string path = zipEntry.FileName;
            foreach (var otherZipEntry in zipFile)
            {
                if (!otherZipEntry.IsDirectory
                    && otherZipEntry.FileName.StartsWith(path, StringComparison.OrdinalIgnoreCase))
                {
                    // File in directory found. Directory in ZIP archive is not empty.
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
