// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="GradientTextureSkyNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  internal class GradientTextureSkyRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _parameterView;
    private readonly EffectParameter _parameterProjection;
    private readonly EffectParameter _parameterSunDirection;
    private readonly EffectParameter _parameterTime;
    private readonly EffectParameter _parameterColor;
    private readonly EffectParameter _parameterFrontTexture;
    private readonly EffectParameter _parameterBackTexture;
    private readonly EffectParameter _parameterAbcd;
    private readonly EffectParameter _parameterEAndStrength;
    private readonly EffectPass _passLinear;
    private readonly EffectPass _passGamma;
    private readonly EffectPass _passCieLinear;
    private readonly EffectPass _passCieGamma;

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
    /// Initializes a new instance of the <see cref="GradientTextureSkyRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public GradientTextureSkyRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        throw new NotSupportedException("The GradientTextureSkyRenderer does not support the Reach profile.");

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Sky/GradientTextureSky");
      _parameterView = _effect.Parameters["View"];
      _parameterProjection = _effect.Parameters["Projection"];
      _parameterSunDirection = _effect.Parameters["SunDirection"];
      _parameterTime = _effect.Parameters["Time"];
      _parameterColor = _effect.Parameters["Color"];
      _parameterFrontTexture = _effect.Parameters["FrontTexture"];
      _parameterBackTexture = _effect.Parameters["BackTexture"];
      _parameterAbcd = _effect.Parameters["Abcd"];
      _parameterEAndStrength = _effect.Parameters["EAndStrength"];
      _passLinear = _effect.Techniques[0].Passes["Linear"];
      _passGamma = _effect.Techniques[0].Passes["Gamma"];
      _passCieLinear = _effect.Techniques[0].Passes["CieLinear"];
      _passCieGamma = _effect.Techniques[0].Passes["CieGamma"];

      _submesh = MeshHelper.GetBox(graphicsService);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is GradientTextureSkyNode;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
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
        var node = nodes[i] as GradientTextureSkyNode;
        if (node == null)
          continue;

        // GradientTextureSkyNode is visible in current frame.
        node.LastFrame = frame;

        _parameterSunDirection.SetValue((Vector3)node.SunDirection);
        _parameterTime.SetValue((float)node.TimeOfDay.TotalHours / 24);
        _parameterColor.SetValue((Vector4)node.Color);
        _parameterFrontTexture.SetValue(node.FrontTexture);
        _parameterBackTexture.SetValue(node.BackTexture);

        if (node.CieSkyStrength < Numeric.EpsilonF)
        {
          if (context.IsHdrEnabled())
            _passLinear.Apply();
          else
            _passGamma.Apply();
        }
        else
        {
          var p = node.CieSkyParameters;
          _parameterAbcd.SetValue(new Vector4(p.A, p.B, p.C, p.D));
          _parameterEAndStrength.SetValue(new Vector2(p.E, node.CieSkyStrength));

          if (context.IsHdrEnabled())
            _passCieLinear.Apply();
          else
            _passCieGamma.Apply();
        }
        _submesh.Draw();
      }

      savedRenderState.Restore();
    }
    #endregion
  }
}
#endif
