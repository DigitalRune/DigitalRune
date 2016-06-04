// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Storages
{
  /// <summary>
  /// Defines a point where a storage is mounted into a virtual file system.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="VfsMountInfo"/> instance be shared among multiple <see cref="VfsStorage"/>s.
  /// </para>
  /// <para>
  /// The path stored in <see cref="MountPoint"/> is automatically normalized. (For example:
  /// Backward slashes '\' are changed to forward slashes '/'.)
  /// </para>
  /// </remarks>
  /// <seealso cref="VfsStorage"/>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class VfsMountInfo
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the storage that is mounted into the virtual file system.
    /// </summary>
    /// <value>The storage that is mounted into the virtual file system.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public IStorage Storage
    {
      get { return _storage; }
      private set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _storage = value;
      }
    }
    private IStorage _storage;


    /// <summary>
    /// Gets the path at which the storage is mounted into the virtual file system.
    /// </summary>
    /// <value>
    /// The mount point. Can be <see langword="null"/> or empty, which is equivalent to the root 
    /// directory "/" of the virtual file system.
    /// </value>
    /// <remarks>
    /// The path value stored in this property is automatically normalized. (For example: Backward 
    /// slashes '\' are changed to forward slashes '/'.)
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// Invalid path specified as mount point.
    /// </exception>
    public string MountPoint
    {
      get { return _mountPoint; }
      private set
      {
        value = StorageHelper.NormalizeMountPoint(value);
        _mountPoint = value;
      }
    }
    private string _mountPoint;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="VfsMountInfo"/> class.
    /// </summary>
    /// <param name="storage">The storage to be mounted.</param>
    /// <param name="mountPoint">
    /// The path at which the storage should be mounted. Can be <see langword="null"/> or empty,
    /// which is equivalent to the root directory "/" of the virtual file system.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="storage"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="storage"/> is empty, or <paramref name="mountPoint"/> is invalid.
    /// </exception>
    public VfsMountInfo(IStorage storage, string mountPoint)
    {
      Storage = storage;
      MountPoint = mountPoint;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
