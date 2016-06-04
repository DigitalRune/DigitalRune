// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.PostProcessing
{
  /// <summary>
  /// Creates a motion blur that depends on the camera movement.
  /// </summary>
  /// <remarks>
  /// This effect blurs the image depending on the camera motion. It does not blur moving objects
  /// when the camera is standing still. This effect assumes that the camera uses a symmetric (not 
  /// skewed) frustum.
  /// </remarks>
  public class CameraMotionBlur : PostProcessor
  {
    // Note:
    // see "GPU Gems 3: Motion Blur as a Post-Processing Effect"

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Vector3[] _cameraFrustumFarCorners = new Vector3[4];

    private readonly Effect _effect;
    private readonly EffectParameter _viewportSizeParameter;
    private readonly EffectParameter _parameterFrustumCorners;
    private readonly EffectParameter _sourceTextureParameter;
    private readonly EffectParameter _gBuffer0Parameter;
    private readonly EffectParameter _viewInverseParameter;
    private readonly EffectParameter _viewProjOldParameter;
    private readonly EffectParameter _numberOfSamplesParameter;
    private readonly EffectParameter _strengthParameter;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the number of samples used to compute a blurred pixel.
    /// </summary>
    /// <value>The number of samples. The default value is 8.</value>
    public float NumberOfSamples { get; set; }


    /// <summary>
    /// Gets or sets the strength.
    /// </summary>
    /// <value>The strength. The default value is 0.6.</value>
    public float Strength { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CameraMotionBlur"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public CameraMotionBlur(IGraphicsService graphicsService)
      : base(graphicsService)
    {
      _effect = GraphicsService.Content.Load<Effect>("DigitalRune/PostProcessing/CameraMotionBlur");
      _viewportSizeParameter = _effect.Parameters["ViewportSize"];
      _parameterFrustumCorners = _effect.Parameters["FrustumCorners"];
      _sourceTextureParameter = _effect.Parameters["SourceTexture"];
      _gBuffer0Parameter = _effect.Parameters["GBuffer0"];
      _viewInverseParameter = _effect.Parameters["ViewInverse"];
      _viewProjOldParameter = _effect.Parameters["ViewProjOld"];
      _numberOfSamplesParameter = _effect.Parameters["NumberOfSamples"];
      _strengthParameter = _effect.Parameters["Strength"];

      NumberOfSamples = 8;
      Strength = 0.6f;
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
      context.ThrowIfGBuffer0Missing();

      var graphicsDevice = GraphicsService.GraphicsDevice;
      var cameraNode = context.CameraNode;
      var camera = cameraNode.Camera;
      var projection = camera.Projection;

      // Get required matrices.
      Matrix44F view = cameraNode.View;
      Matrix44F viewInverse = view.Inverse;

      Pose lastPoseWorld = cameraNode.LastPoseWorld ?? cameraNode.PoseWorld;
      Matrix44F lastView = lastPoseWorld.Inverse;
      Matrix44F lastProjection = camera.LastProjection ?? projection;
      Matrix44F lastViewProjection = lastProjection * lastView;

      if (TextureHelper.IsFloatingPointFormat(context.SourceTexture.Format))
        graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      else
        graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

      graphicsDevice.SetRenderTarget(context.RenderTarget);
      graphicsDevice.Viewport = context.Viewport;

      _viewportSizeParameter.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));

      GraphicsHelper.GetFrustumFarCorners(projection, _cameraFrustumFarCorners);
      _parameterFrustumCorners.SetValue(_cameraFrustumFarCorners);

      _sourceTextureParameter.SetValue(context.SourceTexture);
      _gBuffer0Parameter.SetValue(context.GBuffer0);
      _viewInverseParameter.SetValue((Matrix)viewInverse);
      _viewProjOldParameter.SetValue((Matrix)lastViewProjection);
      _numberOfSamplesParameter.SetValue((int)NumberOfSamples);
      _strengthParameter.SetValue(Strength);
      _effect.CurrentTechnique.Passes[0].Apply();
      graphicsDevice.DrawFullScreenQuad();

      _sourceTextureParameter.SetValue((Texture2D)null);
      _gBuffer0Parameter.SetValue((Texture2D)null);
    }
    #endregion
  }
}
#endif
