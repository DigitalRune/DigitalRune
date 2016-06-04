// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.IO;
using Microsoft.Xna.Framework;


namespace DigitalRune.Storages
{
  /// <summary>
  /// Provides access to the title's default storage location. (Only available in the XNA-compatible
  /// build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Graphics.dll.
  /// </remarks>
  public class TitleStorage : Storage
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    protected override char DirectorySeparator
    {
      get
      {
        return Path.DirectorySeparatorChar;
      }
    }


    /// <summary>
    /// Gets the root directory relative to the title container.
    /// </summary>
    /// <value>The root directory relative to the title container.</value>
    /// <remarks>
    /// All file access is relative to this root directory.
    /// </remarks>
    public string RootDirectory { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    
    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TitleStorage"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TitleStorage"/> class.
    /// </summary>
    public TitleStorage()
      : this(null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TitleStorage"/> class using the specified
    /// directory as the root directory.
    /// </summary>
    /// <param name="rootDirectory">The root directory to search for content.</param>
    public TitleStorage(string rootDirectory)
    {
      RootDirectory = rootDirectory;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override string GetRealPath(string path)
    {
      // Not supported.
      return null;
    }


    /// <inheritdoc/>
    public override Stream OpenFile(string path)
    {
      if (!string.IsNullOrEmpty(RootDirectory))
        path = Path.Combine(RootDirectory, path);

      return TitleContainer.OpenStream(path);
    }
    #endregion
  }
}
