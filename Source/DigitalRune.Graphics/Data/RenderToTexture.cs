// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a 2D texture or cube map that was created using render-to-texture functionality.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see cref="RenderToTextureNode"/>s can be used to create scene captures, e.g. for real-time
  /// reflections. This class represents the target <see cref="Texture"/> and the
  /// <see cref="TextureMatrix"/> for projective texture mapping.
  /// </para>
  /// <para>
  /// The <see cref="Texture"/> must be set by the user. It is not created automatically.
  /// <see cref="Texture"/> must be a valid render target: <see cref="RenderTarget2D"/> or
  /// <see cref="RenderTargetCube"/>. The property specifies which kind of texture should be
  /// captured and the target resolution, format, etc. Render-to-texture renderers will render into
  /// this render target.
  /// </para>
  /// The <see cref="TextureMatrix"/> is set automatically. Its purpose depends on the type of the
  /// target texture (2D or cube map). See property <see cref="TextureMatrix"/> for details.
  /// </remarks>
  public class RenderToTexture
  {
    // TODO: Add property RenderToTexture.FrameRate as possible extension.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the render target.
    /// </summary>
    /// <value>
    /// The render target. Must be a <see cref="RenderTarget2D"/> or <see cref="RenderTargetCube"/>.
    /// </value>
    public Texture Texture { get; set; }


    /// <summary>
    /// Gets the texture matrix.
    /// </summary>
    /// <value>The texture matrix.</value>
    /// <remarks>
    /// <para>
    /// This matrix is set automatically.
    /// </para>
    /// <para>
    /// When the target <see cref="Texture"/> is a 2D render target, this matrix represents a
    /// projective texturing matrix. It transforms world space positions to texture space.
    /// </para>
    /// <para>
    /// When the target <see cref="Texture"/> is a cube map render target, this matrix represents 
    /// the orientation of the cube map. It transforms world space directions to texture space.
    /// Usually, the cube map will be aligned with the world space axes, and this matrix will
    /// be the identity matrix.
    /// </para>
    /// </remarks>
    public Matrix44F TextureMatrix { get; internal set; }


    /// <summary>
    /// Gets the number of the last frame in which the texture was rendered.
    /// </summary>
    /// <value>The number of the frame in which the texture was rendered.</value>
    /// <remarks>
    /// The property <see cref="LastFrame"/> can be used to determine when the texture was updated
    /// the last time. (Note: When using <see cref="RenderToTextureNode"/>s, the texture is usually
    /// only updated when the <see cref="RenderToTextureNode"/> is visible from the player's point
    /// of view.)
    /// </remarks>
    /// <seealso cref="IGraphicsService.Frame"/>
    public int LastFrame { get; internal set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderToTexture"/> class.
    /// </summary>
    public RenderToTexture()
    {
      LastFrame = -1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
