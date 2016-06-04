// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


using System;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides common names for render context data (see property <see cref="RenderContext.Data"/>
  /// of <see cref="RenderContext"/>).
  /// </summary>
  public static class RenderContextKeys
  {
    /// <summary>
    /// A 2D texture (or render target) containing a downsampled depth buffer (half width and half 
    /// height).
    /// </summary>
    public const string DepthBufferHalf = "DepthBufferHalf";


    /// <summary>
    /// A 2D texture (or render target) containing screen space velocities.
    /// </summary>
    public const string VelocityBuffer = "VelocityBuffer";


    /// <summary>
    /// A 2D texture (or render target) containing screen space velocities of the last frame.
    /// </summary>
    public const string LastVelocityBuffer = "LastVelocityBuffer";


    /// <summary>
    /// A <see cref="RebuildZBufferRenderer"/> which should be used when the depth buffer has to be
    /// restored.
    /// </summary>
    public const string RebuildZBufferRenderer = "RebuildZBufferRenderer";


    /// <summary>
    /// The <see cref="Graphics.Shadow"/>. (Only set if a shadow map is currently being rendered.)
    /// </summary>
    [Obsolete("Key has been declared obsolete. The current shadow map can be found in RenderContext.Object.")]
    public const string Shadow = "Shadow";


    /// <summary>
    /// The index of the shadow tile (e.g. cube map side or cascade).
    /// (Only set if a shadow map with tiles is currently being rendered.)
    /// </summary>
    /// <remarks>
    /// For cascaded shadow maps the tile index is the index of the cascade.
    /// For cube map shadow map the tile index is the cube map face index.
    /// </remarks>
    public const string ShadowTileIndex = "ShadowTileIndex";
  }
}
