// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="ScatteringSkyNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  internal class ScatteringSkyRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _parameterView;
    private readonly EffectParameter _parameterProjection;
    private readonly EffectParameter _parameterSunDirection;
    private readonly EffectParameter _parameterRadii;
    private readonly EffectParameter _parameterNumberOfSamples;
    private readonly EffectParameter _parameterBetaRayleigh;
    private readonly EffectParameter _parameterBetaMie;
    private readonly EffectParameter _parameterGMie;
    private readonly EffectParameter _parameterSunIntensity;
    private readonly EffectParameter _parameterTransmittance;
    private readonly EffectParameter _parameterBaseHorizonColor;
    private readonly EffectParameter _parameterBaseZenithColor;
    private readonly EffectPass _passLinear;
    private readonly EffectPass _passGamma;
    private readonly EffectPass _passLinearWithBaseColor;
    private readonly EffectPass _passGammaWithBaseColor;

    private readonly Submesh _submesh;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ScatteringSkyRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public ScatteringSkyRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        throw new NotSupportedException("The ScatteringSkyRenderer does not support the Reach profile.");


      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Sky/ScatteringSky");
      _parameterView = _effect.Parameters["View"];
      _parameterProjection = _effect.Parameters["Projection"];
      _parameterSunDirection = _effect.Parameters["SunDirection"];
      _parameterRadii = _effect.Parameters["Radii"];
      _parameterNumberOfSamples = _effect.Parameters["NumberOfSamples"];
      _parameterBetaRayleigh = _effect.Parameters["BetaRayleigh"];
      _parameterBetaMie = _effect.Parameters["BetaMie"];
      _parameterGMie = _effect.Parameters["GMie"];
      _parameterSunIntensity = _effect.Parameters["SunIntensity"];
      _parameterTransmittance = _effect.Parameters["Transmittance"];
      _parameterBaseHorizonColor = _effect.Parameters["BaseHorizonColor"];
      _parameterBaseZenithColor = _effect.Parameters["BaseZenithColor"];
      _passLinear = _effect.Techniques[0].Passes["Linear"];
      _passGamma = _effect.Techniques[0].Passes["Gamma"];
      _passLinearWithBaseColor = _effect.Techniques[0].Passes["LinearWithBaseColor"];
      _passGammaWithBaseColor = _effect.Techniques[0].Passes["GammaWithBaseColor"];

      _submesh = MeshHelper.GetBox(graphicsService);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is ScatteringSkyNode;
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (nodes.Count == 0)
        return;

      context.Validate(_effect);
      context.ThrowIfCameraMissing();

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.BlendState = BlendState.AlphaBlend;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

      // Camera properties
      var cameraNode = context.CameraNode;
      Matrix view = (Matrix)new Matrix44F(cameraNode.PoseWorld.Orientation.Transposed, new Vector3F());
      _parameterView.SetValue(view);
      Matrix projection = cameraNode.Camera.Projection;
      _parameterProjection.SetValue(projection);

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as ScatteringSkyNode;
        if (node == null)
          continue;

        // ScatteringSkyNode is visible in current frame.
        node.LastFrame = frame;

        _parameterSunDirection.SetValue((Vector3)node.SunDirection);
        _parameterSunIntensity.SetValue((Vector3)(node.SunIntensity * node.SunColor));
        _parameterRadii.SetValue(new Vector4(
          node.AtmosphereHeight + node.PlanetRadius,    // Atmosphere radius
          node.PlanetRadius,                            // Ground radius
          node.ObserverAltitude + node.PlanetRadius,    // Observer radius
          node.ScaleHeight));                           // Absolute Scale height
        _parameterNumberOfSamples.SetValue(node.NumberOfSamples);
        _parameterBetaRayleigh.SetValue((Vector3)node.BetaRayleigh);
        _parameterBetaMie.SetValue((Vector3)node.BetaMie);
        _parameterGMie.SetValue(node.GMie);
        _parameterTransmittance.SetValue(node.Transmittance);

        if (node.BaseHorizonColor.IsNumericallyZero && node.BaseZenithColor.IsNumericallyZero)
        {
          // No base color.
          if (context.IsHdrEnabled())
            _passLinear.Apply();
          else
            _passGamma.Apply();
        }
        else
        {
          // Add base color.
          _parameterBaseHorizonColor.SetValue((Vector4)new Vector4F(node.BaseHorizonColor, node.BaseColorShift));
          _parameterBaseZenithColor.SetValue((Vector3)node.BaseZenithColor);

          if (context.IsHdrEnabled())
            _passLinearWithBaseColor.Apply();
          else
            _passGammaWithBaseColor.Apply();
        }

        _submesh.Draw();
      }

      savedRenderState.Restore();
    }
    #endregion
  }
}
#endif
