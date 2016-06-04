// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DigitalRune.Storages
{
  /// <summary>
  /// Manages the storages that are mounted into a virtual file system.
  /// </summary>
  /// <remarks>
  /// The order of the entries in this collection defines the search order for files and
  /// directories if the virtual file system.
  /// </remarks>
  /// <seealso cref="VfsStorage"/>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class VfsMountInfoCollection : Collection<VfsMountInfo>
  {
    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="VfsMountInfoCollection"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="VfsMountInfoCollection"/>.
    /// </returns>
    public new List<VfsMountInfo>.Enumerator GetEnumerator()
    {
      return ((List<VfsMountInfo>)Items).GetEnumerator();
    }
  }
}
