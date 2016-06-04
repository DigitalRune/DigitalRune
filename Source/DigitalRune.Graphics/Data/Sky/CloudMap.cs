// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides a cloud texture.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="CloudLayerNode"/> draws a cloud texture into the sky. The cloud texture is 
  /// provided by the <see cref="CloudMap"/>. Different types of cloud maps are available:
  /// </para>
  /// <list type="bullet">
  /// <item>
  /// <term><see cref="UserDefinedCloudMap"/></term>
  /// <description>
  /// This type provides a user-defined texture as the cloud texture.
  /// </description>
  /// </item>
  /// <item>
  /// <term><see cref="LayeredCloudMap"/></term>
  /// <description>
  /// This type provides a cloud texture which is dynamically generated from multiple layers of 
  /// textures or random noise. The cloud texture can be animated at runtime.
  /// </description>
  /// </item>
  /// </list>
  /// </remarks>
  public abstract class CloudMap : IDisposable
  {
    // Possible enhancements:
    // - Support colored textures for clouds.
    // - Support PackedTextures.
    // - Support packing several cloud map layers into one RGBA texture.
    // - ...


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------    

    /// <summary>
    /// Gets the cloud texture.
    /// </summary>
    /// <value>The cloud texture.</value>
    /// <remarks>
    /// The cloud texture stores the transmittance of the sky. The transmittance is the amount of 
    /// incident light that passes through a point in the sky. A value of 0 (black) means that the 
    /// point in the sky is covered by clouds, a value of 1 (white) means that the sky is clear.
    /// Cloud textures are usually stored using single-channel surface formats, such as
    /// <strong>SurfaceFormat.Alpha8</strong> (unsigned format, 8-bit alpha only).
    /// </remarks>
    public Texture2D Texture { get; protected set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Releases all resources used by an instance of the <see cref="CloudMap"/> class.
    /// </summary>
    /// <remarks>
    /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
    /// <see langword="true"/>, and then suppresses finalization of the instance.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="CloudMap"/> class 
    /// and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        Texture.SafeDispose();
        Texture = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
