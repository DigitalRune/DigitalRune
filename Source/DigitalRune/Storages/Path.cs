// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
#if !PORTABLE
using BclPath = System.IO.Path;
#endif

namespace DigitalRune.Storages
{
  /// <summary>
  /// A portable replacement of <strong>System.IO.Path</strong>.
  /// </summary>
  public static class Path
  {
    /// <summary>
    /// Provides a platform-specific character used to separate directory levels in a path string that reflects a hierarchical file system organization.
    /// </summary>
    public static readonly char DirectorySeparatorChar;


    /// <summary>
    /// Initializes static members of the <see cref="Path"/> class.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
    static Path()
    {
#if PORTABLE
      DirectorySeparatorChar = '\\';
      throw Portable.NotImplementedException;
#elif NETFX_CORE
      DirectorySeparatorChar = '\\';
#else
      DirectorySeparatorChar = BclPath.DirectorySeparatorChar;
#endif
    }


    /// <summary>
    /// Changes the extension of a path string.
    /// </summary>
    /// <param name="path">The path information to modify.</param>
    /// <param name="extension">
    /// The new extension (with or without a leading period). Specify <see langword="null"/> to 
    /// remove an existing extension from <paramref name="path"/>.
    /// </param>
    /// <returns>
    /// A string containing the modified path information. 
    /// </returns>
    public static string ChangeExtension(string path, string extension)
    {
#if PORTABLE
      throw Portable.NotImplementedException;
#else
      return BclPath.ChangeExtension(path, extension);
#endif
    }


    /// <summary>
    /// Combines two path strings.
    /// </summary>
    /// <param name="path1">The first path.</param>
    /// <param name="path2">The second path.</param>
    /// <returns>
    /// A string containing the combined paths. If one of the specified paths is a zero-length 
    /// string, this method returns the other path. If <paramref name="path2"/> contains an absolute 
    /// path, this method returns <paramref name="path2"/>.
    /// </returns>
    public static string Combine(string path1, string path2)
    {
#if PORTABLE
      throw Portable.NotImplementedException;
#else
      return BclPath.Combine(path1, path2);
#endif
    }


    /// <summary>
    /// Combines an array of strings into a path.
    /// </summary>
    /// <param name="paths">An array of parts of the path.</param>
    /// <returns>A string that contains the combined paths. </returns>
    public static string Combine(params string[] paths)
    {
#if PORTABLE
      throw Portable.NotImplementedException;
#elif WP7 || XBOX || UNITY
      //  WP7 only implements Path.Combine with two arguments.
      if (paths == null || paths.Length == 0)
        return string.Empty;

      if (paths.Length == 1)
        return paths[0];

      string result = BclPath.Combine(paths[0], paths[1]);
      for (int i = 2; i < paths.Length; i++)
        result = BclPath.Combine(result, paths[i]);

      return result;
#else
      return BclPath.Combine(paths);
#endif
    }


    /// <summary>
    /// Returns the directory information for the specified path string.
    /// </summary>
    /// <param name="path">The path of a file or directory.</param>
    /// <returns>
    /// Directory information for path, or <see langword="null"/> if <paramref name="path"/> denotes 
    /// a root directory or is null. Returns <see cref="string.Empty"/> if
    /// <paramref name="path"/> does not contain directory information. 
    /// </returns>
    public static string GetDirectoryName(string path)
    {
#if PORTABLE
      throw Portable.NotImplementedException;
#else
      return BclPath.GetDirectoryName(path);
#endif
    }


    /// <summary>
    /// Returns the absolute path for the specified path string.
    /// </summary>
    /// <param name="path">
    /// The file or directory for which to obtain absolute path information. 
    /// </param>
    /// <returns>
    /// A string containing the fully qualified location of <paramref name="path"/>, such as 
    /// "rootdir\MyFile.txt". 
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "path")]
    public static string GetFullPath(string path)
    {
#if PORTABLE
      throw Portable.NotImplementedException;
#elif NETFX_CORE || WP7 || WP8 || XBOX
      throw new NotSupportedException();
#else
      return BclPath.GetFullPath(path);
#endif
    }


    /// <summary>
    /// Gets a value indicating whether the specified path string contains a root.
    /// </summary>
    /// <param name="path">The path to test.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="path"/> contains a root; otherwise, 
    /// <see langword="false"/>. 
    /// </returns>
    public static bool IsPathRooted(string path)
    {
#if PORTABLE
      throw Portable.NotImplementedException;
#else
      return BclPath.IsPathRooted(path);
#endif
    }


    /// <summary>
    /// Creates a relative file path.
    /// </summary>
    /// <param name="rootFolder">The root folder. Must be an absolute (rooted) file path.</param>
    /// <param name="file">The file path.</param>
    /// <returns>
    /// A path to <paramref name="file"/> which is relative to <paramref name="rootFolder"/>.
    /// </returns>
    public static string GetRelativePath(string rootFolder, string file)
    {
      if (rootFolder == null)
        throw new ArgumentNullException("rootFolder");
      if (rootFolder.Length == 0 || !IsPathRooted(rootFolder))
          throw new ArgumentException("The root folder must be an absolute (rooted) file path.", "rootFolder");

      if (!IsPathRooted(file))
        return file;

      if (rootFolder[rootFolder.Length - 1] != DirectorySeparatorChar)
        rootFolder += DirectorySeparatorChar;

      var rootUri = new Uri(rootFolder);
      var fileUri = new Uri(file);
      var relativePath = rootUri.MakeRelativeUri(fileUri).ToString();
      relativePath = relativePath.Replace('/', DirectorySeparatorChar);
      relativePath = Uri.UnescapeDataString(relativePath);
      return relativePath;
    }
  }
}
