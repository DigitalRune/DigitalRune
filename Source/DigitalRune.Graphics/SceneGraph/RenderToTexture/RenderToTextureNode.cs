// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a scene node which renders data to a texture (e.g. a scene capture, an environment
  /// map, or a reflection image for a mirror).
  /// </summary>
  /// <remarks>
  /// <para>
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="RenderToTextureNode"/> is cloned, the
  /// <see cref="RenderToTexture"/> property is not duplicated. The <see cref="RenderToTexture"/> 
  /// property is copied by reference (shallow copy). The original <see cref="RenderToTextureNode"/> 
  /// and the cloned instance will reference the same <see cref="Graphics.RenderToTexture"/> object.
  /// </para>
  /// </remarks>
  /// <seealso cref="Graphics.RenderToTexture"/>
  public abstract class RenderToTextureNode : SceneNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the render-to-texture target.
    /// </summary>
    /// <value>The render-to-texture target.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public RenderToTexture RenderToTexture
    {
      get { return _renderToTexture; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _renderToTexture = value;
      }
    }
    private RenderToTexture _renderToTexture;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderToTextureNode" /> class.
    /// </summary>
    /// <param name="renderToTexture">The render texture target.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="renderToTexture"/> is <see langword="null"/>.
    /// </exception>
    protected RenderToTextureNode(RenderToTexture renderToTexture)
    {
      if (renderToTexture == null)
        throw new ArgumentNullException("renderToTexture");

      _renderToTexture = renderToTexture;
      Shape = Geometry.Shapes.Shape.Infinite;
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing, bool disposeData)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          if (disposeData)
            RenderToTexture.Texture.SafeDispose();
        }

        base.Dispose(disposing, disposeData);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
