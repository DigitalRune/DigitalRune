// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Creates the shadow mask from the shadow map of a light node with a
  /// <see cref="CubeMapShadow"/>.
  /// </summary>
  /// <inheritdoc cref="ShadowMaskRenderer"/>
  internal class CubeMapShadowMaskRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Vector3[] _frustumFarCorners = new Vector3[4];

    private readonly Effect _effect;
    private readonly EffectParameter _parameterViewInverse;
    private readonly EffectParameter _parameterFrustumCorners;
    private readonly EffectParameter _parameterGBuffer0;
    private readonly EffectParameter _parameterParameters0;
    private readonly EffectParameter _parameterParameters1;
    private readonly EffectParameter _parameterParameters2;
    private readonly EffectParameter _parameterLightPosition;
    private readonly EffectParameter _parameterShadowView;
    private readonly EffectParameter _parameterJitterMap;
    private readonly EffectParameter _parameterShadowMap;
    private readonly EffectParameter _parameterSamples;

    private Texture2D _jitterMap;

    private readonly Vector3[] _samples = new Vector3[StandardShadowMaskRenderer.PoissonKernel.Length];
    private int _lastNumberOfSamples;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CubeMapShadowMaskRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public CubeMapShadowMaskRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Deferred/CubeMapShadowMask");
      _parameterViewInverse = _effect.Parameters["ViewInverse"];
      _parameterFrustumCorners = _effect.Parameters["FrustumCorners"];
      _parameterGBuffer0 = _effect.Parameters["GBuffer0"];
      _parameterParameters0 = _effect.Parameters["Parameters0"];
      _parameterParameters1 = _effect.Parameters["Parameters1"];
      _parameterParameters2 = _effect.Parameters["Parameters2"];
      _parameterLightPosition = _effect.Parameters["LightPosition"];
      _parameterShadowView = _effect.Parameters["ShadowView"];
      _parameterJitterMap = _effect.Parameters["JitterMap"];
      _parameterShadowMap = _effect.Parameters["ShadowMap"];
      _parameterSamples = _effect.Parameters["Samples"];

      Debug.Assert(_parameterSamples.Elements.Count == _samples.Length);

      // TODO: Use struct parameter. Not yet supported in MonoGame.
      // Struct effect parameters are not yet supported in the MonoGame effect processor.
      //var parameterShadow = _effect.Parameters["ShadowParam"];
      //_parameterNear = parameterShadow.StructureMembers["Near"];
      //...
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var lightNode = node as LightNode;
      return lightNode != null && lightNode.Shadow is CubeMapShadow;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      int numberOfNodes = nodes.Count;
      if (numberOfNodes == 0)
        return;

      context.Validate(_effect);
      context.ThrowIfCameraMissing();

      var graphicsDevice = _effect.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;

      var cameraNode = context.CameraNode;
      _parameterViewInverse.SetValue(cameraNode.PoseWorld);
      _parameterGBuffer0.SetValue(context.GBuffer0);

      Viewport viewport = context.Viewport;
      _parameterParameters0.SetValue(new Vector2(viewport.Width, viewport.Height));

      if (_jitterMap == null)
        _jitterMap = NoiseHelper.GetGrainTexture(context.GraphicsService, NoiseHelper.DefaultJitterMapWidth);

      _parameterJitterMap.SetValue(_jitterMap);

      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var light = lightNode.Light as PointLight;
        if (light == null)
          return;

        var shadow = lightNode.Shadow as CubeMapShadow;
        if (shadow == null)
          continue;

        if (shadow.ShadowMap == null || shadow.ShadowMask == null)
          continue;

        // The effect must only render in a specific channel.
        // Do not change blend state if the correct write channels is already set, e.g. if this
        // shadow is part of a CompositeShadow, the correct blend state is already set.
        if ((int)graphicsDevice.BlendState.ColorWriteChannels != (1 << shadow.ShadowMaskChannel))
          graphicsDevice.BlendState = GraphicsHelper.BlendStateWriteSingleChannel[shadow.ShadowMaskChannel];

        _parameterParameters1.SetValue(new Vector4(
          shadow.Near,
          light.Range,
          shadow.EffectiveDepthBias,
          shadow.EffectiveNormalOffset));

        // If we use a subset of the Poisson kernel, we have to normalize the scale.
        int numberOfSamples = Math.Min(shadow.NumberOfSamples, StandardShadowMaskRenderer.PoissonKernel.Length);
        float filterRadius = shadow.FilterRadius;
        if (numberOfSamples > 0)
          filterRadius /= StandardShadowMaskRenderer.PoissonKernel[numberOfSamples - 1].Length();

        _parameterParameters2.SetValue(new Vector3(
          shadow.ShadowMap.Size,
          filterRadius,
          // The StandardShadow.JitterResolution is the number of texels per world unit.
          // In the shader the parameter JitterResolution contains the division by the jitter map size.
          shadow.JitterResolution / _jitterMap.Width));

        _parameterLightPosition.SetValue((Vector3)cameraNode.PoseWorld.ToLocalPosition(lightNode.PoseWorld.Position));

        _parameterShadowView.SetValue(lightNode.PoseWorld.Inverse * cameraNode.PoseWorld);
        _parameterShadowMap.SetValue(shadow.ShadowMap);

        var rectangle = GraphicsHelper.GetViewportRectangle(cameraNode, viewport, lightNode);
        Vector2F texCoordTopLeft = new Vector2F(rectangle.Left / (float)viewport.Width, rectangle.Top / (float)viewport.Height);
        Vector2F texCoordBottomRight = new Vector2F(rectangle.Right / (float)viewport.Width, rectangle.Bottom / (float)viewport.Height);
        GraphicsHelper.GetFrustumFarCorners(cameraNode.Camera.Projection, texCoordTopLeft, texCoordBottomRight, _frustumFarCorners);
        _parameterFrustumCorners.SetValue(_frustumFarCorners);

        var pass = GetPass(numberOfSamples);

        if (numberOfSamples > 0)
        {
          if (_lastNumberOfSamples != numberOfSamples)
          {
            // Create an array with the first n samples and the rest set to 0.
            _lastNumberOfSamples = numberOfSamples;
            for (int j = 0; j < numberOfSamples; j++)
            {
              _samples[j].Y = StandardShadowMaskRenderer.PoissonKernel[j].Y;
              _samples[j].X = StandardShadowMaskRenderer.PoissonKernel[j].X;
              _samples[j].Z = 1.0f / numberOfSamples;
            }

            // Set the rest to zero.
            for (int j = numberOfSamples; j < _samples.Length; j++)
              _samples[j] = Vector3.Zero;

            _parameterSamples.SetValue(_samples);
          }
          else if (i == 0)
          {
            // Apply offsets in the first loop.
            _parameterSamples.SetValue(_samples);
          }
        }

        pass.Apply();

        graphicsDevice.DrawQuad(rectangle);
      }

      _parameterGBuffer0.SetValue((Texture2D)null);
      _parameterJitterMap.SetValue((Texture2D)null);
      _parameterShadowMap.SetValue((Texture2D)null);
      savedRenderState.Restore();
    }


    // Chooses a suitable effect pass. See the list of available passes in the effect.
    private EffectPass GetPass(int numberOfSamples)
    {
      if (numberOfSamples < 0)
      {
        var pass = _effect.Techniques[0].Passes[0];
        Debug.Assert(pass.Name == "Optimized");
        return pass;
      }
      if (numberOfSamples == 0)
      {
        var pass = _effect.Techniques[0].Passes[1];
        Debug.Assert(pass.Name == "Unfiltered");
        return pass;
      }
      if (numberOfSamples == 1)
      {
        var pass = _effect.Techniques[0].Passes[2];
        Debug.Assert(pass.Name == "Pcf1");
        return pass;
      }
      if (numberOfSamples <= 4)
      {
        var pass = _effect.Techniques[0].Passes[3];
        Debug.Assert(pass.Name == "Pcf4");
        return pass;
      }
      if (numberOfSamples <= 8)
      {
        var pass = _effect.Techniques[0].Passes[4];
        Debug.Assert(pass.Name == "Pcf8");
        return pass;
      }
      if (numberOfSamples <= 16)
      {
        var pass = _effect.Techniques[0].Passes[5];
        Debug.Assert(pass.Name == "Pcf16");
        return pass;
      }
      else
      {
        var pass = _effect.Techniques[0].Passes[6];
        Debug.Assert(pass.Name == "Pcf32");
        return pass;
      }
    }
    #endregion
  }
}
#endif
