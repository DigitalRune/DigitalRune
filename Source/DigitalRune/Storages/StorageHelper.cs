// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;


namespace DigitalRune.Storages
{
  /// <summary>
  /// Provides conversion and extension methods for <see cref="DigitalRune.Storages"/>.
  /// </summary>
  internal static class StorageHelper
  {
    /// <summary>
    /// Gets the application installation folder.
    /// </summary>
    /// <value>The application installation folder.</value>
    internal static string BaseLocation
    {
      get
      {
#if PORTABLE
        throw Portable.NotImplementedException;
#elif NETFX_CORE || WP8
        return global::Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
#elif SILVERLIGHT || XBOX || WINDOWS_PHONE
        return string.Empty;
//#elif ANDROID
//        ???
#elif IOS
        return Foundation.NSBundle.MainBundle.ResourcePath;
//#elif MACOS
//        return MonoMac.Foundation.NSBundle.MainBundle.ResourcePath;     // Note: We do not have a Mac OS specific build yet!!
#elif PSM
        return "/Application";
#else
        return AppDomain.CurrentDomain.BaseDirectory;
#endif
      }
    }


    /// <summary>
    /// Switches the directory separator character in the specified path.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="directorySeparator">The desired directory separator character.</param>
    /// <returns>The path using only the specified directory separator.</returns>
    internal static string SwitchDirectorySeparator(string path, char directorySeparator)
    {
      switch (directorySeparator)
      {
        case '/':
          path = path.Replace('\\', '/');
          break;
        case '\\':
          path = path.Replace('/', '\\');
          break;
        default:
          path = path.Replace('\\', directorySeparator);
          path = path.Replace('/', directorySeparator);
          break;
      }

      return path;
    }


    /// <summary>
    /// Validates the mount point and normalizes the path.
    /// </summary>
    /// <param name="path">The mount point.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is invalid.
    /// </exception>
    internal static string NormalizeMountPoint(string path)
    {
      const string root = ""; // Path that represents the root directory.
      const string message = "Invalid mount point. Mount point needs to be specified relative to root directory of the virtual files system in canonical form.";

      // null or "" are valid mount points, same as '/'.
      if (String.IsNullOrEmpty(path))
        return root;

      // Paths with "pathA/pathB/../pathC" are not supported.
      if (path.Contains(".."))
        throw new ArgumentException(message, "path");

      // Switch to forward slashes '/'.
      path = path.Replace('\\', '/');

      // Reduce "./path" to "path".
      while (path.StartsWith("./", StringComparison.Ordinal))
        path = path.Substring(2);

      // Reduce "pathA/./pathB" to "pathA/pathB".
      path = path.Replace("/./", "/");

      if (path.Length == 0)
        return root;

      // Trim leading '/'. All mount points are relative to root directory!
      if (path[0] == '/')
        path = path.Substring(1);

      if (path.Length == 0)
        return root;

      // Rooted path, such as "C:\path" or "\\server\path", are not supported.
      if (Path.IsPathRooted(path))
        throw new ArgumentException(message, "path");

      // Trim trailing '/'.
      while (path.Length > 0 && path[path.Length - 1] == '/')
        path = path.Substring(0, path.Length - 1);

      if (path.Length == 0)
        return root;

      return path;
    }


    /// <summary>
    /// Validates and normalizes the path of a file in a storage.
    /// </summary>
    /// <param name="path">The path the file.</param>
    /// <returns>The normalized path.</returns>
    internal static string NormalizePath(string path)
    {
      const string message = "Invalid path. The path needs to be specified relative to the root directory in canonical form.";

      if (path == null)
        throw new ArgumentNullException("path");
      if (path.Length == 0)
        throw new ArgumentException("Invalid path. Path must not be empty.", "path");

      // Paths with "pathA/pathB/../pathC" are not supported.
      if (path.Contains(".."))
        throw new ArgumentException(message, "path");

      // Switch to forward slashes '/'.
      path = SwitchDirectorySeparator(path, '/');

      // Reduce "./path" to "path".
      while (path.StartsWith("./", StringComparison.Ordinal))
        path = path.Substring(2);

      // Reduce "pathA/./pathB" to "pathA/pathB".
      path = path.Replace("/./", "/");

      if (path.Length == 0)
        throw new ArgumentException(message, "path");

      // Trim leading '/'. All mount points are relative to root directory!
      if (path[0] == '/')
        path = path.Substring(1);

      if (path.Length == 0)
        throw new ArgumentException(message, "path");

      // Absolute paths are not supported.
      if (Path.IsPathRooted(path))
        throw new ArgumentException(message, "path");

      // Trim trailing '/'.
      while (path.Length > 0 && path[path.Length - 1] == '/')
        path = path.Substring(0, path.Length - 1);

      if (path.Length == 0)
        throw new ArgumentException(message, "path");

      return path;
    }


#if STORAGE_READ_WRITE

    /// <overloads>
    /// <summary>
    /// Converts <strong>System.IO</strong> value to <strong>DigitalRune.Storages</strong>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts a <strong>System.IO.FileAccess</strong> value to 
    /// <strong>DigitalRune.Storages.FileAccess</strong>.
    /// </summary>
    /// <param name="value">The <strong>System.IO.FileAccess</strong> value.</param>
    /// <returns>The <strong>DigitalRune.Storages.FileAccess</strong> value.</returns>
    public static FileAccess FromSystemIO(System.IO.FileAccess value)
    {
      return (FileAccess)value;
    }


    /// <overloads>
    /// <summary>
    /// Converts <strong>DigitalRune.Storages</strong> value to <strong>System.IO</strong>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts a <strong>DigitalRune.Storages.FileAccess</strong> value to 
    /// <strong>System.IO.FileAccess</strong>.
    /// </summary>
    /// <param name="value">The <strong>DigitalRune.Storages.FileAccess</strong> value.</param>
    /// <returns>The <strong>System.IO.FileAccess</strong> value.</returns>
    public static System.IO.FileAccess ToSystemIO(FileAccess value)
    {
      return (System.IO.FileAccess)value;
    }


    /// <summary>
    /// Converts a <strong>System.IO.FileAttributes</strong> value to 
    /// <strong>DigitalRune.Storages.FileAttributes</strong>.
    /// </summary>
    /// <param name="value">The <strong>System.IO.FileAttributes</strong> value.</param>
    /// <returns>The <strong>DigitalRune.Storages.FileAttributes</strong> value.</returns>
    public static FileAttributes FromSystemIO(System.IO.FileAttributes value)
    {
      return (FileAttributes)value;
    }


    /// <summary>
    /// Converts a <strong>DigitalRune.Storages.FileAttributes</strong> value to 
    /// <strong>System.IO.FileAttributes</strong>.
    /// </summary>
    /// <param name="value">The <strong>DigitalRune.Storages.FileAttributes</strong> value.</param>
    /// <returns>The <strong>System.IO.FileAttributes</strong> value.</returns>
    public static System.IO.FileAttributes ToSystemIO(FileAttributes value)
    {
      return (System.IO.FileAttributes)value;
    }


    /// <summary>
    /// Converts a <strong>System.IO.FileMode</strong> value to 
    /// <strong>DigitalRune.Storages.FileMode</strong>.
    /// </summary>
    /// <param name="value">The <strong>System.IO.FileMode</strong> value.</param>
    /// <returns>The <strong>DigitalRune.Storages.FileMode</strong> value.</returns>
    public static FileMode FromSystemIO(System.IO.FileMode value)
    {
      return (FileMode)value;
    }


    /// <summary>
    /// Converts a <strong>DigitalRune.Storages.FileMode</strong> value to 
    /// <strong>System.IO.FileMode</strong>.
    /// </summary>
    /// <param name="value">The <strong>DigitalRune.Storages.FileMode</strong> value.</param>
    /// <returns>The <strong>System.IO.FileMode</strong> value.</returns>
    public static System.IO.FileMode ToSystemIO(FileMode value)
    {
      return (System.IO.FileMode)value;
    }


    /// <summary>
    /// Converts a <strong>System.IO.FileOptions</strong> value to 
    /// <strong>DigitalRune.Storages.FileOptions</strong>.
    /// </summary>
    /// <param name="value">The <strong>System.IO.FileOptions</strong> value.</param>
    /// <returns>The <strong>DigitalRune.Storages.FileOptions</strong> value.</returns>
    public static FileOptions FromSystemIO(System.IO.FileOptions value)
    {
      return (FileOptions)value;
    }


    /// <summary>
    /// Converts a <strong>DigitalRune.Storages.FileOptions</strong> value to 
    /// <strong>System.IO.FileOptions</strong>.
    /// </summary>
    /// <param name="value">The <strong>DigitalRune.Storages.FileOptions</strong> value.</param>
    /// <returns>The <strong>System.IO.FileOptions</strong> value.</returns>
    public static System.IO.FileOptions ToSystemIO(FileOptions value)
    {
      return (System.IO.FileOptions)value;
    }


    /// <summary>
    /// Converts a <strong>System.IO.FileShare</strong> value to 
    /// <strong>DigitalRune.Storages.FileShare</strong>.
    /// </summary>
    /// <param name="value">The <strong>System.IO.FileShare</strong> value.</param>
    /// <returns>The <strong>DigitalRune.Storages.FileShare</strong> value.</returns>
    public static FileShare FromSystemIO(System.IO.FileShare value)
    {
      return (FileShare)value;
    }


    /// <summary>
    /// Converts a <strong>DigitalRune.Storages.FileShare</strong> value to 
    /// <strong>System.IO.FileShare</strong>.
    /// </summary>
    /// <param name="value">The <strong>DigitalRune.Storages.FileShare</strong> value.</param>
    /// <returns>The <strong>System.IO.FileShare</strong> value.</returns>
    public static System.IO.FileShare ToSystemIO(FileShare value)
    {
      return (System.IO.FileShare)value;
    }


    // CopyDirectory(string, string)
    // OpenRead(string)
    // OpenWrite(string)
    // ReplaceFile(string, string, string)
#endif
  }
}
