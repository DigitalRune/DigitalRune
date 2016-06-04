// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using DigitalRune.Storages;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Content;
using DRPath = DigitalRune.Storages.Path;


namespace DigitalRune.Editor.Game
{
    internal sealed class MonoGameContent : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this instance has been disposed of.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance has been disposed of; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDisposed { get; private set; }

        public string RootDirectoryName { get; set; }
        public string FileName { get; set; }
        public VfsStorage Storage { get; }
        public StorageContentManager ContentManager { get; }
        public object Asset { get; }


        public MonoGameContent(string rootDirectoryName, string fileName, VfsStorage storage, StorageContentManager contentManager, object asset)
        {
            Debug.Assert(!string.IsNullOrEmpty(rootDirectoryName));
            Debug.Assert(!string.IsNullOrEmpty(fileName));
            Debug.Assert(storage != null);
            Debug.Assert(contentManager != null);
            Debug.Assert(asset != null);

            RootDirectoryName = rootDirectoryName;
            FileName = fileName;
            Storage = storage;
            ContentManager = contentManager;
            Asset = asset;
        }


        ///// <summary>
        ///// Releases unmanaged resources before an instance of the <see cref="MonoGameContent"/>
        ///// class is reclaimed by garbage collection.
        ///// </summary>
        ///// <remarks>
        ///// This method releases unmanaged resources by calling the virtual
        ///// <see cref="Dispose(bool)"/> method, passing in <see langword="false"/>.
        ///// </remarks>
        //~MonoGameContent()
        //{
        //    Dispose(false);
        //}


        /// <summary>
        /// Releases all resources used by an instance of the <see cref="MonoGameContent"/> class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in
        /// <see langword="true"/>, and then suppresses finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="MonoGameContent"/>
        /// class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        private /* protected virtual */ void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Asset.SafeDispose();
                    ContentManager.SafeDispose();
                    Storage.Dispose();
                }

                IsDisposed = true;
            }
        }
    }


    internal interface IMonoGameService
    {
        /// <summary>
        /// Loads the specified XNB file. (Throws on failure.)
        /// </summary>
        /// <param name="rootDirectoryName">The content root directory.</param>
        /// <param name="fileName">The absolute path and name of the XNB file.</param>
        /// <param name="cacheResult">
        /// <para>
        /// <see langword="true"/> to cache the result in the <see cref="IMonoGameService"/> and
        /// leave the ownership of the content to the <see cref="IMonoGameService"/>. The content is
        /// cached until another object takes ownership of the content or another XNB with different
        /// parameters is loaded.
        /// </para>
        /// <para>
        /// <see langword="false"/> to take ownership of the content. The
        /// <see cref="IMonoGameService"/> does not keep a reference of the content. The object that
        /// has ownership of the content is responsible for its disposal.
        /// </para>
        /// </param>
        /// <returns>The loaded content.</returns>
        MonoGameContent LoadXnb(string rootDirectoryName, string fileName, bool cacheResult);
    }


    internal sealed class MonoGameService : IMonoGameService, IDisposable
    {
        private readonly IServiceLocator _services;

        // Cache the most recently loaded content.
        // (If it is cached, the MonoGameService has ownership.)
        private MonoGameContent _cachedContent;


        /// <summary>
        /// Gets a value indicating whether this instance has been disposed of.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance has been disposed of; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDisposed { get; private set; }


        public MonoGameService(IServiceLocator services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            _services = services;
        }


        ///// <summary>
        ///// Releases unmanaged resources before an instance of the <see cref="MonoGameService"/>
        ///// class is reclaimed by garbage collection.
        ///// </summary>
        ///// <remarks>
        ///// This method releases unmanaged resources by calling the virtual
        ///// <see cref="Dispose(bool)"/> method, passing in <see langword="false"/>.
        ///// </remarks>
        //~MonoGameService()
        //{
        //    Dispose(false);
        //}


        /// <summary>
        /// Releases all resources used by an instance of the <see cref="MonoGameService"/> class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in
        /// <see langword="true"/>, and then suppresses finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Releases the unmanaged resources used by an instance of the <see cref="MonoGameService"/> class
        /// and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        private /* protected virtual */ void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _cachedContent?.Dispose();
                }

                IsDisposed = true;
            }
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public MonoGameContent LoadXnb(string rootDirectoryName, string fileName, bool cacheResult)
        {
            Debug.Assert(rootDirectoryName != null);
            Debug.Assert(rootDirectoryName.Length > 0);
            Debug.Assert(fileName != null);
            Debug.Assert(fileName.Length > 0);

            // Check whether content has already been loaded and is still cached.
            if (_cachedContent != null 
                && !_cachedContent.IsDisposed
                && _cachedContent.RootDirectoryName == rootDirectoryName
                && _cachedContent.FileName == fileName)
            {
                var result = _cachedContent;
                if (!cacheResult)
                    _cachedContent = null;

                return result;
            }

            // Clear cache.
            _cachedContent?.Dispose();
            _cachedContent = null;

            // External references in MonoGame are relative to the content root directory, not the
            // model. Loading the model fails, if a external reference cannot be resolved.
            // --> Try different content root directories.
            var rootDirectory = new DirectoryInfo(rootDirectoryName);
            while (rootDirectory != null)
            {
                VfsStorage storage = null;
                StorageContentManager content = null;
                try
                {
                    // Create a virtual file system which contains the DigitalRune effects and the content folder.
                    storage = new VfsStorage();
                    storage.MountInfos.Add(new VfsMountInfo(new ZipStorage(new TitleStorage("Content"), "DigitalRune.zip"), null));
                    storage.MountInfos.Add(new VfsMountInfo(new FileSystemStorage(rootDirectory.FullName), null));
                    content = new StorageContentManager(_services, storage);

                    string assetName = DRPath.GetRelativePath(rootDirectory.FullName, fileName);
                    var asset = content.Load<object>(assetName);

                    var result = new MonoGameContent(rootDirectoryName, fileName, storage, content, asset);

                    if (cacheResult)
                        _cachedContent = result;

                    return result;
                }
                catch (Exception exception)
                {
                    storage?.Dispose();
                    content?.Dispose();

                    if (exception is ContentLoadException
                        && exception.InnerException is ContentLoadException
                        && rootDirectory.Parent != null)
                    {
                        // ExternalReference could probably not be resolved.
                        // --> Retry with parent folder as content root.
                    }
                    else
                    {
                        // Asset could not be loaded.
                        throw;
                    }
                }

                rootDirectory = rootDirectory.Parent;
            }

            // Execution should never reach this point.
            throw new EditorException("Unexpected error.");
        }
    }
}
