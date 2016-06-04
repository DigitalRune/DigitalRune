// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a layer of a <see cref="CloudMap"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="CloudMap"/> consists of several cloud map layers. A cloud map layer defines the 
  /// relative cloud density: A value of 0 means the sky is clear (no clouds), a positive value 
  /// increases the density (more clouds), and a negative value decreases the density (less clouds). 
  /// Cloud map layers are added together to get the overall cloud density. The density contribution
  /// of each layer is (pseudo code):
  /// </para>
  /// <para>
  /// <c>CloudDensity += DensityScale * (Texture[TextureMatrix * texCoords] + DensityOffset)</c>
  /// </para>
  /// <para>
  /// The <see cref="Texture"/> is optional. If <see cref="Texture"/> is <see langword="null"/>, a
  /// random noise texture is used. The random noise texture can be animated (see 
  /// <see cref="AnimationSpeed"/>). The property <see cref="AnimationSpeed"/> is only used to 
  /// animate the random noise texture. It is not used if a <see cref="Texture"/> is set.
  /// </para>
  /// <para>
  /// The <see cref="TextureMatrix"/> can be used to scale or translate the texture. By changing the
  /// translation part of the <see cref="TextureMatrix"/>, the layer can be animated to move with
  /// the wind.
  /// </para>
  /// <para>
  /// To disable a <see cref="CloudMapLayer"/> set the <see cref="DensityScale"/> to 0.
  /// </para>
  /// </remarks>
  public class CloudMapLayer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the cloud texture that defines the cloud density. (Optional)
    /// </summary>
    /// <value>
    /// The cloud texture that defines the cloud density. The default value is 
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// The <see cref="Texture"/> is optional. If <see cref="Texture"/>
    /// is <see langword="null"/>, a random noise texture is used.
    /// </remarks>
    public Texture2D Texture { get; set; }


    /// <summary>
    /// Gets or sets the matrix used to transform the texture coordinates.
    /// </summary>
    /// <value>
    /// The matrix used to transform the texture coordinates. The default value is 
    /// <see cref="Matrix33F.Identity"/>.
    /// </value>
    public Matrix33F TextureMatrix { get; set; }


    /// <summary>
    /// Gets or sets the density scale factor that is used to scale the density of this 
    /// <see cref="CloudMapLayer"/>.
    /// </summary>
    /// <value>
    /// The density scale factor that is used to scale the density of this 
    /// <see cref="CloudMapLayer"/>. The default value is 1.
    /// </value>
    public float DensityScale { get; set; }


    /// <summary>
    /// Gets or sets the density offset that is added to the density of this 
    /// <see cref="CloudMapLayer"/>.
    /// </summary>
    /// <value>
    /// The density offset that is added to the density of this <see cref="CloudMapLayer"/>. The
    /// default value is 0.
    /// </value>
    public float DensityOffset { get; set; }


    /// <summary>
    /// Gets or sets the animation speed. (Only used if <see cref="Texture"/> is 
    /// <see langword="null"/> - see remarks.)
    /// </summary>
    /// <value>
    /// The animation speed. The default value is 0.
    /// </value>
    /// <remarks>
    /// If <see cref="Texture"/> is <see langword="null"/>, a random noise texture is used. The 
    /// random noise texture can be animated by setting an <see cref="AnimationSpeed"/>. 
    /// <see cref="AnimationSpeed"/> is only used to animate the random noise texture. The property
    /// is ignored if a <see cref="Texture"/> is set.
    /// </remarks>
    public float AnimationSpeed { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudMapLayer"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="CloudMapLayer"/> class with default settings.
    /// </summary>
    public CloudMapLayer()
      : this(null, Matrix33F.Identity, 0, 1, 0)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="CloudMapLayer" /> struct.
    /// </summary>
    /// <param name="texture">The <see cref="Texture" />.</param>
    /// <param name="textureMatrix">The <see cref="TextureMatrix" />.</param>
    /// <param name="densityOffset">The <see cref="DensityOffset" />.</param>
    /// <param name="densityScale">The <see cref="DensityScale" />.</param>
    /// <param name="animationSpeed">The <see cref="AnimationSpeed"/>.</param>
    public CloudMapLayer(Texture2D texture, Matrix33F textureMatrix, float densityOffset, float densityScale, float animationSpeed)
    {
      Texture = texture;
      TextureMatrix = textureMatrix;
      DensityScale = densityScale;
      DensityOffset = densityOffset;
      AnimationSpeed = animationSpeed;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
