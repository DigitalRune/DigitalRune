// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a cube map ("skybox") that is into the background of the current render target.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A "skybox" is a cube map that is used as the background of a scene. 
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="SkyboxNode"/> is cloned the <see cref="Texture"/> 
  /// is not duplicated. The <see cref="Texture"/> and the <see cref="Encoding"/> are copied by 
  /// reference (shallow copy). The original <see cref="SkyboxNode"/> and the cloned instance will 
  /// reference the same instances.
  /// </para>
  /// </remarks>
  public class SkyboxNode : SkyNode
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the cube map texture.
    /// </summary>
    /// <value>The cube map texture (using premultiplied alpha).</value>
    public TextureCube Texture { get; set; }


    /// <summary>
    /// Gets or sets the tint color.
    /// </summary>
    /// <value>The tint color. The default value is white (1, 1, 1).</value>
    /// <remarks>
    /// <para>
    /// The color values of the <see cref="Texture"/> are multiplied with this value. This can be
    /// used to tint a skybox, change its brightness or fade it in/out.
    /// </para>
    /// </remarks>
    public Vector3F Color { get; set; }


    /// <summary>
    /// Gets or sets the opacity of the skybox.
    /// </summary>
    /// <value>The opacity of the skybox. The default value is 1 (opaque).</value>
    /// <remarks>
    /// <para>
    /// The alpha values of the <see cref="Texture"/> are multiplied with this value. This can be
    /// used to fade in/out a skybox.
    /// </para>
    /// </remarks>
    public float Alpha { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether alpha blending is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if alpha blending is enabled; otherwise, <see langword="false"/>.
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool EnableAlphaBlending { get; set; }


    /// <summary>
    /// Gets or sets the color encoding used by the cube map texture.
    /// </summary>
    /// <value>
    /// The color encoding used by the <see cref="Texture"/>. The default value is 
    /// <see cref="ColorEncoding.SRgb"/>.
    /// </value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public ColorEncoding Encoding
    {
      get { return _encoding; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _encoding = value;
      }
    }
    private ColorEncoding _encoding;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="SkyboxNode"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="SkyboxNode"/> class.
    /// </summary>
    public SkyboxNode()
      : this(null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SkyboxNode" /> class.
    /// </summary>
    /// <param name="texture">The cube map texture (using premultiplied alpha).</param>
    public SkyboxNode(TextureCube texture)
    {
      Texture = texture;
      Color = new Vector3F(1, 1, 1);
      Alpha = 1.0f;
      Encoding = ColorEncoding.SRgb;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new SkyboxNode Clone()
    {
      return (SkyboxNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new SkyboxNode();
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SkyNode properties.
      base.CloneCore(source);

      // Clone SkyboxNode properties.
      var sourceTyped = (SkyboxNode)source;
      Texture = sourceTyped.Texture;
      Color = sourceTyped.Color;
      Alpha = sourceTyped.Alpha;
      EnableAlphaBlending = sourceTyped.EnableAlphaBlending;
      Encoding = sourceTyped.Encoding;
    }
    #endregion
  }
}
