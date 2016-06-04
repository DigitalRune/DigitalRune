// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Storages
{
  /// <summary>
  /// Extends the XNA <see cref="ContentManager"/> and loads assets from any <see cref="IStorage"/>
  /// location.
  /// </summary>
  public class StorageContentManager : ContentManager, IStorageProvider
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the storage that provides the content.
    /// </summary>
    /// <value>The storage that provides the content.</value>
    public IStorage Storage { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="StorageContentManager"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="StorageContentManager"/> class reading content
    /// from the <see cref="IStorage"/> service.
    /// </summary>
    /// <param name="serviceProvider">
    /// The service provider the <seealso cref="ContentManager"/> should use to locate services.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    public StorageContentManager(IServiceProvider serviceProvider)
      : this(serviceProvider, null, null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="StorageContentManager"/> class reading content
    /// from the specified storage.
    /// </summary>
    /// <param name="serviceProvider">
    /// The service provider the <seealso cref="ContentManager"/> should use to locate services.
    /// </param>
    /// <param name="storage">
    /// The storage that provides the content. (If this parameter is <see langword="null"/>, the 
    /// <see cref="StorageContentManager"/> automatically looks for the <see cref="IStorage"/>
    /// service.)
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    public StorageContentManager(IServiceProvider serviceProvider, IStorage storage)
      : this(serviceProvider, storage, null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="StorageContentManager"/> class.
    /// </summary>
    /// <param name="serviceProvider">
    /// The service provider the <seealso cref="ContentManager"/> should use to locate services.
    /// </param>
    /// <param name="storage">
    /// The storage that provides the content. (If this parameter is <see langword="null"/>, the 
    /// <see cref="StorageContentManager"/> automatically looks for the <see cref="IStorage"/>
    /// service.)
    /// </param>
    /// <param name="rootDirectory">
    /// The root directory to search for content. Can be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="serviceProvider"/> is <see langword="null"/>.
    /// </exception>
    public StorageContentManager(IServiceProvider serviceProvider, IStorage storage, string rootDirectory)
      : base(serviceProvider, rootDirectory ?? string.Empty)
    {
      if (serviceProvider == null)
        throw new ArgumentNullException("serviceProvider");
      if (storage == null)
        storage = serviceProvider.GetService(typeof(IStorage)) as IStorage;

      if (storage == null)
        throw new ArgumentNullException("storage", "The specified storage is null and the service provider does not contain an IStorage service.");

      Storage = storage;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------


    /// <summary>
    /// Opens a stream for reading the specified asset. Derived classes can replace this to
    /// implement pack files or asset compression.
    /// </summary>
    /// <param name="assetName">The name of the asset being read.</param>
    /// <returns>The opened stream.</returns>
    protected override Stream OpenStream(string assetName)
    {
      try
      {
        string path = assetName;
        path = Path.ChangeExtension(path, ".xnb");
        if (!string.IsNullOrEmpty(RootDirectory))
          path = Path.Combine(RootDirectory, path);

        return Storage.OpenFile(path);
      }
      catch (Exception exception)
      {
        throw new ContentLoadException(
          "Asset could not be loaded. See inner exception for details.", 
          exception);
      }
    }
    #endregion
  }
}
