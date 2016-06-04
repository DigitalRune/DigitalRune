// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides a user-defined cloud texture.
  /// </summary>
  [Obsolete("BasicCloudMap has been renamed to UserDefinedCloudMap.")]
  public class BasicCloudMap : UserDefinedCloudMap
  {
  }


  /// <summary>
  /// Provides a user-defined cloud texture.
  /// </summary>
  public class UserDefinedCloudMap : CloudMap
  {
    /// <summary>
    /// Gets or sets the cloud texture.
    /// </summary>
    /// <value>The cloud texture.</value>
    /// <inheritdoc cref="CloudMap.Texture"/>
    public new Texture2D Texture
    {
      get { return base.Texture; }
      set { base.Texture = value; }
    }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="UserDefinedCloudMap"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="UserDefinedCloudMap"/> class.
    /// </summary>
    public UserDefinedCloudMap() : this(null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="UserDefinedCloudMap"/> class.
    /// </summary>
    /// <param name="texture">The cloud texture.</param>
    public UserDefinedCloudMap(Texture2D texture)
    {
      Texture = texture;
    }
  }
}
