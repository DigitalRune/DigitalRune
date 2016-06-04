// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides a cloud texture which is generated at runtime.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A cloud map defines a cloud texture using (up to) 8 <see cref="CloudMapLayer"/>s, which are
  /// sampled and added together. The resulting cloud texture is stored in <see cref="Texture"/>. A
  /// cloud map layer can contain a texture or random noise. In addition, layers can be animated. 
  /// See <see cref="CloudMapLayer"/> for more information. 
  /// </para>
  /// <para>
  /// To disable a cloud map layer, simply set the array entry to <see langword="null"/> or the 
  /// <see cref="CloudMapLayer.DensityScale"/> of the layer to 0.
  /// </para>
  /// <para>
  /// The class <see cref="LayeredCloudMap"/> only defines the settings for generating a cloud
  /// texture and stores the result, but it does not automatically generate the cloud texture. A 
  /// <see cref="CloudMapRenderer"/> must be used generate the cloud texture at runtime. A 
  /// <see cref="CloudMapRenderer"/> is a scene node renderer which handles 
  /// <see cref="CloudLayerNode"/>s. If a <see cref="CloudLayerNode"/> references a 
  /// <see cref="LayeredCloudMap"/>, the renderer creates the cloud texture and stores the result in
  /// the <see cref="CloudMap.Texture"/> property.
  /// </para>
  /// </remarks>
  public class LayeredCloudMap : CloudMap
  {
    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    internal const int NumberOfTextures = 8;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    internal int LastFrame = -1;

    // The current animation time of each layer.
    internal float[] AnimationTimes = new float[NumberOfTextures];

    // A packed texture for each layer. The final layer texture is created by lerping
    // between those two textures. The AnimationTime determines the lerp parameter.
    internal PackedTexture[] SourceLayers = new PackedTexture[NumberOfTextures];
    internal PackedTexture[] TargetLayers = new PackedTexture[NumberOfTextures];

    // The final lerped texture for each layer.
    internal RenderTarget2D[] LayerTextures = new RenderTarget2D[NumberOfTextures];

    internal Random Random;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the (up to) 8 layers that define cloud density.
    /// </summary>
    /// <value>
    /// The (up to) 8 layers that define the cloud density. By default, the cloud map is initialized 
    /// with 8 static, random noise textures.
    /// </value>
    /// <remarks>
    /// <see cref="Layers"/> is an array of 8 <see cref="CloudMapLayer"/>s. The cloud densities of 
    /// the layers are added together to determine the total cloud density. 
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
    public CloudMapLayer[] Layers { get; private set; }


    /// <summary>
    /// Gets or sets the cloud coverage.
    /// </summary>
    /// <value>
    /// The cloud coverage that defines how much of the sky is filled with clouds. Values less than 
    /// 0 or greater than 1 are allowed and might be necessary to remove all clouds or to fill the 
    /// whole sky. The default value is 0.5.
    /// </value>
    public float Coverage { get; set; }


    /// <summary>
    /// Gets or sets the cloud density.
    /// </summary>
    /// <value>
    /// The cloud density. The default value is 10.
    /// </value>
    public float Density { get; set; }


    /// <summary>
    /// Gets or sets the size of the cloud map in texels.
    /// </summary>
    /// <value>
    /// The size of the cloud map in texels. The default is 1024.
    /// </value>
    public int Size { get; set; }


    /// <summary>
    /// Gets or sets the random number generator seed.
    /// </summary>
    /// <value>
    /// The random number generator seed.
    /// </value>
    /// <remarks>
    /// An internal random number generator is used to created textures for 
    /// <see cref="CloudMapLayer"/>s where <see cref="CloudMapLayer.Texture"/> is 
    /// <see langword="null"/>. The random number generator is initialized with this seed value.
    /// The default value is 1234567.
    /// </remarks>
    public int Seed { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudMap" /> class.
    /// </summary>
    public LayeredCloudMap()
    {
      Coverage = 0.5f;
      Density = 10;
      Size = 1024;
      Seed = 1234567;

      // Initialize with 8 octaves of static noise.
      Layers = new CloudMapLayer[8];
      var scale = new Matrix33F(0.2f, 0, 0,
                                0, 0.2f, 0,
                                0, 0, 1);
      Layers[0] = new CloudMapLayer(null, scale * new Matrix33F(1, 0, 0, 0, 1, 0, 0, 0, 1), -0.5f, 1.0f, 0);
      Layers[1] = new CloudMapLayer(null, scale * new Matrix33F(2, 0, 0, 0, 2, 0, 0, 0, 1), -0.5f, 1.0f / 2.0f, 0);
      Layers[2] = new CloudMapLayer(null, scale * new Matrix33F(4, 0, 0, 0, 4, 0, 0, 0, 1), -0.5f, 1.0f / 4.0f, 0);
      Layers[3] = new CloudMapLayer(null, scale * new Matrix33F(8, 0, 0, 0, 8, 0, 0, 0, 1), -0.5f, 1.0f / 8.0f, 0);
      Layers[4] = new CloudMapLayer(null, scale * new Matrix33F(16, 0, 0, 0, 16, 0, 0, 0, 1), -0.5f, 1.0f / 16.0f, 0);
      Layers[5] = new CloudMapLayer(null, scale * new Matrix33F(32, 0, 0, 0, 32, 0, 0, 0, 1), -0.5f, 1.0f / 32.0f, 0);
      Layers[6] = new CloudMapLayer(null, scale * new Matrix33F(64, 0, 0, 0, 64, 0, 0, 0, 1), -0.5f, 1.0f / 64.0f, 0);
      Layers[7] = new CloudMapLayer(null, scale * new Matrix33F(128, 0, 0, 0, 128, 0, 0, 0, 1), -0.5f, 1.0f / 128.0f, 0);
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        for (int i = 0; i < NumberOfTextures; i++)
        {
          SourceLayers[i].SafeDispose();
          SourceLayers[i] = null;

          TargetLayers[i].SafeDispose();
          TargetLayers[i] = null;

          LayerTextures[i].SafeDispose();
          LayerTextures[i] = null;
        }
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Sets the cloud texture.
    /// </summary>
    /// <param name="texture">The cloud texture.</param>
    internal void SetTexture(Texture2D texture)
    {
      Texture = texture;
    }
    #endregion
  }
}
