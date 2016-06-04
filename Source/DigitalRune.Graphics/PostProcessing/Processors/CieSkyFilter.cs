// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Attenuates an image using the luminance distribution of the CIE Sky Model.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The CIE Sky Model describes how luminance is distributed in the sky. Several weather types 
  /// can be modeled, see <see cref="Parameters"/>. This post-processor reads the source texture and
  /// attenuates it by a factor determined by the luminance distribution.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class CieSkyFilter : PostProcessor
  {
    // Note:
    // This class is a separate class and not integrated into the gradient sky class
    // because it may be used for any sky box.
    // 
    // To apply CIE luminance to existing texture:
    //   graphicsDevice.BlendState = GraphicsHelper.BlendStateMultiply;
    //   context.SourceTexture = GraphicsService.GetDefaultTexture2DWhite();
    //   cieSkyFilter.Process(context);


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Vector3[] _cameraFrustumFarCorners = new Vector3[4];

    private readonly Effect _effect;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterFrustumCorners;
    private readonly EffectParameter _parameterSunDirection;
    private readonly EffectParameter _parameterExposure;
    private readonly EffectParameter _parameterAbcd;
    private readonly EffectParameter _parameterEAndStrength;
    private readonly EffectParameter _parameterSourceTexture;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// The parameters of the CIE sky luminance distribution.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Allows access to struct fields directly.")]
    public CieSkyParameters Parameters;


    /// <summary>
    /// Gets or sets the direction to the sun.
    /// </summary>
    /// <value>The direction to the sun. This vector is automatically normalized.</value>
    public Vector3F SunDirection
    {
      get { return _sunDirection; }
      set
      {
        _sunDirection = value;
        _sunDirection.TryNormalize();
      }
    }
    private Vector3F _sunDirection;


    /// <summary>
    /// Gets or sets the exposure factor used to scale the source texture.
    /// </summary>
    /// <value>The exposure factor used to scale the source texture. The default value is 1.</value>
    public float Exposure { get; set; }


    /// <summary>
    /// Gets or sets the strength of the attenuation.
    /// </summary>
    /// <value>The strength of the attenuation in the range [0, 1]. The default value is 1.</value>
    /// <remarks>
    /// If this value is 1, the original CIE sky luminance distribution is used. This makes only
    /// sense if the render pipeline uses HDR. For LDR the effect should be lessened using lower
    /// values for <see cref="Strength"/>.
    /// </remarks>
    public float Strength { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CieSkyFilter"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public CieSkyFilter(IGraphicsService graphicsService) : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/CieSkyFilter");
      _parameterViewportSize = _effect.Parameters["ViewportSize"];
      _parameterFrustumCorners = _effect.Parameters["FrustumCorners"];
      _parameterSunDirection = _effect.Parameters["SunDirection"];
      _parameterExposure = _effect.Parameters["Exposure"];
      _parameterAbcd = _effect.Parameters["Abcd"];
      _parameterEAndStrength = _effect.Parameters["EAndStrength"];
      _parameterSourceTexture = _effect.Parameters["SourceTexture"];

      Parameters = CieSkyParameters.Type12;
      SunDirection = new Vector3F(0, 1, 0);
      Exposure = 1;
      Strength = 1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void OnProcess(RenderContext context)
    {
      context.ThrowIfCameraMissing();

      var graphicsDevice = GraphicsService.GraphicsDevice;

      // Set sampler state. 
      // (Floating-point textures cannot use linear filtering. XNA would throw an exception.)
      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      // Set the render target - but only if no kind of alpha blending is currently set.
      // If alpha-blending is set, then we have to assume that the render target is already
      // set - everything else does not make sense.
      if (graphicsDevice.BlendState.ColorDestinationBlend == Blend.Zero
          && graphicsDevice.BlendState.AlphaDestinationBlend == Blend.Zero)
      {
        graphicsDevice.SetRenderTarget(context.RenderTarget);
        graphicsDevice.Viewport = context.Viewport;
      }

      _parameterViewportSize.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));

      var cameraNode = context.CameraNode;
      var cameraPose = cameraNode.PoseWorld;
      GraphicsHelper.GetFrustumFarCorners(cameraNode.Camera.Projection, _cameraFrustumFarCorners);

      // Convert frustum far corners from view space to world space.
      for (int i = 0; i < _cameraFrustumFarCorners.Length; i++)
        _cameraFrustumFarCorners[i] = (Vector3)cameraPose.ToWorldDirection((Vector3F)_cameraFrustumFarCorners[i]);

      _parameterFrustumCorners.SetValue(_cameraFrustumFarCorners);

      // The CIE model does not work if the sun is below the horizon. We simply project it back up
      // to the horizon. Since the sun light will be small or gone at night, the actual luminance
      // distribution does not matter.
      var sunDirection = SunDirection;
      if (sunDirection.Y < 0)
      {
        sunDirection.Y = 0;
        sunDirection.TryNormalize();
      }

      _parameterSunDirection.SetValue((Vector3)sunDirection);
      _parameterExposure.SetValue(Exposure);
      _parameterAbcd.SetValue(new Vector4(Parameters.A, Parameters.B, Parameters.C, Parameters.D));
      _parameterEAndStrength.SetValue(new Vector2(Parameters.E, Strength));
      _parameterSourceTexture.SetValue(context.SourceTexture);
      _effect.CurrentTechnique.Passes[0].Apply();

      graphicsDevice.DrawFullScreenQuad();
    }
    #endregion
  }
}
#endif
