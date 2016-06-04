// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Records a file change that has been caused by another application.
    /// </summary>
    internal class FileChangeRecord
    {
        /// <summary>
        /// Gets the affected document.
        /// </summary>
        /// <value>The affected document.</value>
        public Document Document { get; private set; }


        /// <summary>
        /// Gets the file system event arguments that describes the file changes.
        /// </summary>
        /// <value>The file system event arguments that describes the file changes.</value>
        public FileSystemEventArgs FileSystemEventArgs { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="FileChangeRecord"/> class.
        /// </summary>
        /// <param name="document">The affected document.</param>
        /// <param name="fileSystemEventArgs">
        /// The <see cref="System.IO.FileSystemEventArgs"/> instance containing the event data.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> or <paramref name="fileSystemEventArgs"/> are 
        /// <see langword="null"/>.
        /// </exception>
        public FileChangeRecord(Document document, FileSystemEventArgs fileSystemEventArgs)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (fileSystemEventArgs == null)
                throw new ArgumentNullException(nameof(fileSystemEventArgs));

            Document = document;
            FileSystemEventArgs = fileSystemEventArgs;
        }
    }
}
