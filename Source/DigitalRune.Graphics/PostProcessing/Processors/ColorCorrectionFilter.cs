// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Diagnostics;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Modifies an image using a color lookup table (a.k.a. "color grading").
  /// </summary>
  /// <remarks>
  /// <para>
  /// This processor uses a 3D volume texture as a color lookup table to transform the colors of an 
  /// image. <see cref="LookupTextureA"/> can be set directly. But it is usually easier to load it 
  /// from a 2D texture. The method <see cref="CreateLookupTexture2D(GraphicsDevice)"/> creates a 
  /// default 2D lookup texture (without any color transformations). Use 
  /// <see cref="ConvertLookupTexture"/> to convert a 2D lookup texture to a 3D lookup texture.
  /// </para>
  /// <para>
  /// <strong>Workflow:</strong><br/>
  /// To define a color lookup texture, the artist can use following steps:
  /// <list type="number">
  /// <item>
  /// Make a screenshot of the game and load it in <b>Photoshop</b>.
  /// </item>
  /// <item>
  /// Load the default lookup texture in <b>Photoshop</b>.
  /// </item>
  /// <item>
  /// Copy the lookup texture in <b>Photoshop</b> and paste it into the document with the 
  /// screenshot.
  /// </item>
  /// <item>
  /// Apply color manipulations to the screenshot and the lookup texture. (Usually using adjustment 
  /// layers.)
  /// </item>
  /// <item>
  /// Select the lookup table inside the screenshot. (Select the layer with the lookup table and use
  /// the menu <b>Select | Load Selection</b>.)
  /// </item>
  /// <item>
  /// Select the menu <b>Edit | Copy Merged</b> to select the lookup table including the 
  /// adjustments.
  /// </item>
  /// <item>
  /// Paste the lookup texture into a new image and save it.
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// (Of course the color lookup texture can also be created in any other image editing tool.)
  /// </para>
  /// <para>
  /// <strong>Interpolation between lookup textures:</strong><br/>
  /// Optionally, you can set a second lookup texture in <see cref="LookupTextureB"/> and use
  /// <see cref="InterpolationParameter"/> to interpolate between both. This can be used to
  /// gradually change the color correction when transitioning to a new zone in the game level. If
  /// only <see cref="LookupTextureA"/> or <see cref="LookupTextureB"/> is set, then one lookup
  /// texture is applied with the specified <see cref="Strength"/> and 
  /// <see cref="InterpolationParameter"/> is ignored.
  /// </para>
  /// </remarks>
  public class ColorCorrectionFilter : PostProcessor
  {
    // Reference: See http://http.developer.nvidia.com/GPUGems2/gpugems2_chapter24.html.


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _strengthParameter;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _lookupTexture0Parameter;
    private readonly EffectParameter _lookupTexture1Parameter;
    private readonly EffectParameter _lookupTableSizeParameter;
    private readonly EffectPass _fullColorLookupPass;
    private readonly EffectPass _partialColorLookupPass;
    private readonly EffectPass _lerpColorLookupPass;

    // A default texture without a color transformation.
    private Texture3D _defaultLookupTexture;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the primary color lookup texture (a 3D texture). Same as
    /// <see cref="LookupTextureA"/>.
    /// </summary>
    /// <value>
    /// The primary color lookup texture. The default value is <see langword="null"/> (no
    /// transformation).
    /// </value>
    /// <remarks>
    /// <para>
    /// The 3D color lookup texture can be set from a 2D texture using
    /// <see cref="SetLookupTexture"/>.
    /// </para>
    /// </remarks>
    [Obsolete("This property is the same as LookupTextureA and will be removed in the future.")]
    public Texture3D LookupTexture { get; set; }


    /// <summary>
    /// Gets or sets the primary color lookup texture (a 3D texture).
    /// </summary>
    /// <value>
    /// The primary color lookup texture. The default value is <see langword="null"/> (no
    /// transformation).
    /// </value>
    /// <remarks>
    /// Use <see cref="ConvertLookupTexture"/> to convert a 2D lookup texture to a 3D lookup
    /// texture.
    /// </remarks>
    public Texture3D LookupTextureA { get; set; }


    /// <summary>
    /// Gets or sets a secondary, optional color lookup texture (a 3D texture).
    /// </summary>
    /// <value>
    /// The secondary, optional color lookup texture. The default value is <see langword="null"/>
    /// (no transformation).
    /// </value>
    /// <remarks>
    /// <para>
    /// If this <see cref="LookupTextureB"/> is not <see langword="null"/>, then the filter 
    /// interpolates the color correction result of <see cref="LookupTextureA"/> and 
    /// <see cref="LookupTextureB"/>. <see cref="InterpolationParameter"/> defines the interpolation
    /// weight: Set <see cref="InterpolationParameter"/> to 0 to use only
    /// <see cref="LookupTextureA"/>. Set <see cref="InterpolationParameter"/> to 1 to use only
    /// <see cref="LookupTextureB"/>. Use a value in the range ]0, 1[ to apply both lookup textures
    /// and interpolate the results.
    /// </para>
    /// <para>
    /// If only one of the lookup textures is set, then this lookup texture is applied and
    /// <see cref="InterpolationParameter"/> is ignored.
    /// </para>
    /// <para>
    /// <see cref="LookupTextureA"/> and <see cref="LookupTextureB"/> must have the same size.
    /// </para>
    /// </remarks>
    public Texture3D LookupTextureB { get; set; }


    /// <summary>
    /// Gets or sets the strength of the effect.
    /// </summary>
    /// <value>
    /// The strength factor. If this value is 0.0, the source image is not changed. If this value is 
    /// 1.0, the colors of the source image are converted based on the lookup texture. If this value
    /// is between 0.0 and 1.0, a linear interpolation between the source image and the color graded
    /// image is returned. The default value is 1.0.
    /// </value>
    public float Strength { get; set; }


    /// <summary>
    /// Gets or sets the interpolation parameter for interpolating between the result of
    /// <see cref="LookupTextureA"/> and <see cref="LookupTextureB"/>.
    /// </summary>
    /// <value>
    /// The interpolation parameter for interpolating between the result of
    /// <see cref="LookupTextureA"/> and <see cref="LookupTextureB"/>. The default value is 0.
    /// </value>
    /// <inheritdoc cref="LookupTextureB"/>
    public float InterpolationParameter { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorCorrectionFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public ColorCorrectionFilter(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      var effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/ColorCorrectionFilter");
      _viewportSizeParameter = effect.Parameters["ViewportSize"];
      _strengthParameter = effect.Parameters["Strength"];
      _sourceTextureParameter = effect.Parameters["SourceTexture"];
      _lookupTexture0Parameter = effect.Parameters["LookupTexture0"];
      _lookupTexture1Parameter = effect.Parameters["LookupTexture1"];
      _lookupTableSizeParameter = effect.Parameters["LookupTableSize"];
      _fullColorLookupPass = effect.CurrentTechnique.Passes["Full"];
      _partialColorLookupPass = effect.CurrentTechnique.Passes["Partial"];
      _lerpColorLookupPass = effect.CurrentTechnique.Passes["Lerp"];

      Strength = 1.0f;
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _defaultLookupTexture.SafeDispose();
        _defaultLookupTexture = null;
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    protected override void OnProcess(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;

      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      graphicsDevice.SetRenderTarget(context.RenderTarget);
      graphicsDevice.Viewport = context.Viewport;

      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
      _sourceTextureParameter.SetValue(context.SourceTexture);
      _strengthParameter.SetValue(new Vector2(Strength, InterpolationParameter));

      if (LookupTextureA == null && LookupTextureB == null)
      {
        if (_defaultLookupTexture == null)
          _defaultLookupTexture = ConvertLookupTexture(CreateLookupTexture2D(graphicsDevice));

        Debug.Assert(_defaultLookupTexture != null, "Failed to create 3D lookup texture.");

        _lookupTexture0Parameter.SetValue(_defaultLookupTexture);
        _lookupTableSizeParameter.SetValue(_defaultLookupTexture.Width);
        _fullColorLookupPass.Apply();
      }
      else if (LookupTextureA == null)
      {
        ApplyPassWithOneLookupTexture(LookupTextureB);
      }
      else if (LookupTextureB == null)
      {
        ApplyPassWithOneLookupTexture(LookupTextureA);
      }
      else
      {
        if (Numeric.AreEqual(InterpolationParameter, 0))
        {
          ApplyPassWithOneLookupTexture(LookupTextureA);
        }
        else if (Numeric.AreEqual(InterpolationParameter, 1))
        {
          ApplyPassWithOneLookupTexture(LookupTextureB);
        }
        else
        {
          _lookupTexture0Parameter.SetValue(LookupTextureA);
          _lookupTexture1Parameter.SetValue(LookupTextureB);
          _lookupTableSizeParameter.SetValue(LookupTextureA.Width);
          _lerpColorLookupPass.Apply();
        }
      }

      graphicsDevice.DrawFullScreenQuad();

      _sourceTextureParameter.SetValue((Texture2D)null);
      _lookupTexture0Parameter.SetValue((Texture2D)null);
      _lookupTexture1Parameter.SetValue((Texture2D)null);
    }


    private void ApplyPassWithOneLookupTexture(Texture3D lookupTexture)
    {
      _lookupTexture0Parameter.SetValue(lookupTexture);
      _lookupTableSizeParameter.SetValue(lookupTexture.Width);

      if (Numeric.AreEqual(Strength, 1))
        _fullColorLookupPass.Apply();
      else
        _partialColorLookupPass.Apply();
    }


    /// <summary>
    /// Sets the 3D lookup texture (<see cref="LookupTextureA"/>).
    /// (Overwrites the existing 3D lookup texture.)
    /// </summary>
    /// <param name="lookupTexture2D">The lookup texture as a 2D texture.</param>
    /// <remarks>
    /// This method overwrites the content of the existing 3D <see cref="LookupTextureA"/>. If the
    /// existing texture does not have the correct size, it is replaced by a new 3D lookup texture.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="lookupTexture2D"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="lookupTexture2D"/> is not a color texture or does not have the expected 
    /// format.
    /// </exception>
    [Obsolete("Use the static method ColorCorrectionFilter.ConvertLookupTexture(Texture2D) instead.")]
    public void SetLookupTexture(Texture2D lookupTexture2D)
    {
      if (lookupTexture2D == null)
        throw new ArgumentNullException("lookupTexture2D");
      if (lookupTexture2D.Format != SurfaceFormat.Color || lookupTexture2D.Width != lookupTexture2D.Height * lookupTexture2D.Height)
        throw new ArgumentException("Invalid 2D lookup texture. (A typical lookup texture is a 256 x 16 32-bit color texture.)", "lookupTexture2D");

      int size = lookupTexture2D.Height;
      if (LookupTextureA != null && LookupTextureA.Width != size)
      {
        LookupTextureA.Dispose();
        LookupTextureA = null;
      }

      if (LookupTextureA == null)
        LookupTextureA = new Texture3D(GraphicsService.GraphicsDevice, size, size, size, false, SurfaceFormat.Color);

      CopyData(lookupTexture2D, LookupTextureA);
    }
    #endregion


    //--------------------------------------------------------------
    #region Static Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Creates the default 2D lookup texture (no color transformations).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Creates the default 2D lookup texture (no color transformations) with 16 entries per color
    /// channel.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <returns>The default 2D lookup texture which contains no color transformations.</returns>
    public static Texture2D CreateLookupTexture2D(GraphicsDevice graphicsDevice)
    {
      return CreateLookupTexture2D(graphicsDevice, 16);
    }


    /// <summary>
    /// Creates the default 2D lookup texture (no color transformations) with the specified lookup
    /// table size.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="size">
    /// The size of the lookup table (= the number of entries per color channel). The recommended
    /// size is 16.
    /// </param>
    /// <returns>The default 2D lookup texture which contains no color transformations.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="size"/> is 0 or negative.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Texture2D CreateLookupTexture2D(GraphicsDevice graphicsDevice, int size)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (size < 1)
        throw new ArgumentOutOfRangeException("size", "The lookup table size must be greater than 0.");

      // Convert values [0, size - 1] to [0.0, 1.0].
      float scale = (size == 1) ? 1.0f : 1.0f / (size - 1.0f);

      // Create a 2D texture.
      var texture = new Texture2D(graphicsDevice, size * size, size, false, SurfaceFormat.Color);

      // Red ..... X
      // Green ... Y
      // Blue .... Z (separate images stacked horizontally)
      var data = new Color[size * size * size];
      for (int g = 0; g < size; g++)
        for (int b = 0; b < size; b++)
          for (int r = 0; r < size; r++)
            data[g * size * size + b * size + r] = new Color(r * scale, g * scale, b * scale);

      texture.SetData(data);
      return texture;
    }


    /// <summary>
    /// Converts a 2D lookup texture to a 3D lookup texture.
    /// </summary>
    /// <param name="texture2D">The 2D lookup texture.</param>
    /// <returns>The 3D lookup texture.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture2D"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="texture2D"/> is not a color texture or does not have the expected format.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Texture3D ConvertLookupTexture(Texture2D texture2D)
    {
      if (texture2D == null)
        throw new ArgumentNullException("texture2D");
      if (texture2D.Format != SurfaceFormat.Color || texture2D.Width != texture2D.Height * texture2D.Height)
        throw new ArgumentException("Invalid 2D lookup texture. (A typical lookup texture is a 256 x 16 32-bit color texture.)", "texture2D");

      int size = texture2D.Height;
      var texture3D = new Texture3D(texture2D.GraphicsDevice, size, size, size, false, SurfaceFormat.Color);
      CopyData(texture2D, texture3D);
      return texture3D;
    }


    private static void CopyData(Texture2D texture2D, Texture3D texture3D)
    {
      Debug.Assert(texture2D != null, "texture2D must not be null.");
      Debug.Assert(texture3D != null, "texture3D must not be null");
      Debug.Assert(
        texture2D.Format == SurfaceFormat.Color || texture2D.Width == texture2D.Height * texture2D.Height,
        "Invalid 2D lookup texture. (A typical lookup texture is a 256 x 16 32-bit color texture.)");
      Debug.Assert(texture2D.Height == texture3D.Width, "Size of 2D texture does not match size of 3D texture.");

      int size = texture2D.Height;
      var data2D = new Color[size * size * size];
      texture2D.GetData(data2D);

      // The memory layout of the 2D texture does not match the 3D texture.
      var data3D = new Color[size * size * size];
      for (int b = 0; b < size; b++)
        for (int g = 0; g < size; g++)
          for (int r = 0; r < size; r++)
            data3D[b * size * size + g * size + r] = data2D[g * size * size + b * size + r];

      texture3D.SetData(data3D);
    }
    #endregion
  }
}
#endif
