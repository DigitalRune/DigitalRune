// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;


namespace DigitalRune
{
  /// <summary>
  /// Provides helper methods for working with unmanaged and managed memory blocks.
  /// </summary>
  internal static class MarshalHelper
  {
    /* Fast, but not thread-safe implementation of Marshal.SizeOf()!

    // Cache SizeOf() results because Marshal.SizeOf() is slow.
    private static readonly Dictionary<Type, int> SizeTable = new Dictionary<Type, int>();


    /// <summary>
    /// Returns the size of a type in bytes. (Fast, but not thread-safe!)
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The size of the type in bytes.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    public static int SizeOf(Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");

      int size;
      if (!SizeTable.TryGetValue(type, out size))
      {
        size = Marshal.SizeOf(type);
        SizeTable.Add(type, size);
      }

      return size;
    }
    //*/


    /// <summary>
    /// Converts a block of memory to a managed object of the specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="buffer">The buffer.</param>
    /// <returns>The managed object of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException">
    /// The layout of <typeparamref name="T"/> is not sequential or explicit.<br/>
    /// Or, <typeparamref name="T"/> is a generic type.
    /// </exception>
    private static T Convert<T>(byte[] buffer) where T : struct
    {
      GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
      try
      {
#if NET45
        // New generic version (avoids boxing).
        return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
#else
        return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
#endif
      }
      finally
      {
        handle.Free();
      }
    }


    /// <summary>
    /// Converts a managed object to a block of memory.
    /// </summary>
    /// <typeparam name="T">The source type.</typeparam>
    /// <param name="t">The managed object.</param>
    /// <returns>
    /// A block of memory containing a copy of <paramref name="t"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// The layout of <typeparamref name="T"/> is not sequential or explicit.
    /// </exception>
    private static byte[] Convert<T>(T t) where T : struct
    {
      byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
      GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
      try
      {
#if NET45
        // New generic version (avoids boxing).
        Marshal.StructureToPtr<T>(t, handle.AddrOfPinnedObject(), false);
#else
        Marshal.StructureToPtr(t, handle.AddrOfPinnedObject(), false);
#endif
      }
      finally
      {
        handle.Free();
      }

      return buffer;
    }


    /// <summary>
    /// Reads specified value from a stream.
    /// </summary>
    /// <typeparam name="T">The type to read.</typeparam>
    /// <param name="stream">The input stream.</param>
    /// <returns>The value read from the stream.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The layout of <typeparamref name="T"/> is not sequential or explicit.<br/>
    /// Or, <typeparamref name="T"/> is a generic type.
    /// </exception>
    public static T ReadStruct<T>(this Stream stream) where T : struct
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      // Read bytes into buffer.
      byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
      int count = 0;
      while (count < buffer.Length)
        count += stream.Read(buffer, count, buffer.Length - count);

      // Convert byte[] to T.
      return Convert<T>(buffer);
    }


    /// <summary>
    /// Writes the specified value to a stream.
    /// </summary>
    /// <typeparam name="T">The type to write</typeparam>
    /// <param name="stream">The output stream.</param>
    /// <param name="t">The value to write.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The layout of <typeparamref name="T"/> is not sequential or explicit.
    /// </exception>
    public static void WriteStruct<T>(this Stream stream, T t) where T : struct
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      // Convert T to byte[].
      byte[] buffer = Convert(t);

      // Write byte[] to stream.
      stream.Write(buffer, 0, buffer.Length);
    }
  }
}
