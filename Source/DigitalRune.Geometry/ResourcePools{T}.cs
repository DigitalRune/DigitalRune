// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Geometry
{
  /// <summary>
  /// Provides resource pools for reusable generic collections or items. (For internal use only.)
  /// </summary>
  /// <typeparam name="T">The type used in the items.</typeparam>
  internal static class ResourcePools<T>
  {
    // ReSharper disable StaticFieldInGenericType
    public static readonly ResourcePool<T[]> Arrays8 = 
      new ResourcePool<T[]>(
        () => new T[8],
        null,
        array => Array.Clear(array, 0, 8));
    // ReSharper restore StaticFieldInGenericType
  }
}
