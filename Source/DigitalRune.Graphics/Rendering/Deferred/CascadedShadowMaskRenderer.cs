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
  /// <see cref="CascadedShadow"/>.
  /// </summary>
  /// <inheritdoc cref="ShadowMaskRenderer"/>
  internal class CascadedShadowMaskRenderer : SceneNodeRenderer
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
    private readonly EffectParameter _parameterDistances;
    private readonly EffectParameter _parameterShadowMatrices;
    private readonly EffectParameter _parameterDepthBias;
    private readonly EffectParameter _parameterNormalOffset;
    private readonly EffectParameter _parameterLightDirection;
    private readonly EffectParameter _parameterNumberOfCascades;
    private readonly EffectParameter _parameterJitterMap;
    private readonly EffectParameter _parameterShadowMap;
    private readonly EffectParameter _parameterSamples;

    private Texture2D _jitterMap;

    // Temp. array for 4 matrices.
    private readonly Matrix[] _matrices = new Matrix[4];

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
    /// Initializes a new instance of the <see cref="CascadedShadowMaskRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public CascadedShadowMaskRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Deferred/CascadedShadowMask");

      _parameterViewInverse = _effect.Parameters["ViewInverse"];
      _parameterFrustumCorners = _effect.Parameters["FrustumCorners"];
      _parameterGBuffer0 = _effect.Parameters["GBuffer0"];
      _parameterParameters0 = _effect.Parameters["Parameters0"];
      _parameterParameters1 = _effect.Parameters["Parameters1"];
      _parameterParameters2 = _effect.Parameters["Parameters2"];
      _parameterDistances = _effect.Parameters["Distances"];
      _parameterShadowMatrices = _effect.Parameters["ShadowMatrices"];
      _parameterDepthBias = _effect.Parameters["DepthBias"];
      _parameterNormalOffset = _effect.Parameters["NormalOffset"];
      _parameterLightDirection = _effect.Parameters["LightDirection"];
      _parameterNumberOfCascades = _effect.Parameters["NumberOfCascades"];
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
      return lightNode != null && lightNode.Shadow is CascadedShadow;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
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

      // Set camera properties.
      var cameraNode = context.CameraNode;
      var cameraPose = cameraNode.PoseWorld;
      Matrix viewInverse = cameraPose;
      _parameterViewInverse.SetValue(viewInverse);
      _parameterGBuffer0.SetValue(context.GBuffer0);

      Viewport viewport = context.Viewport;
      _parameterParameters0.SetValue(new Vector2(viewport.Width, viewport.Height));

      // Set jitter map.
      if (_jitterMap == null)
        _jitterMap = NoiseHelper.GetGrainTexture(context.GraphicsService, NoiseHelper.DefaultJitterMapWidth);

      _parameterJitterMap.SetValue(_jitterMap);

      float cameraFar = context.CameraNode.Camera.Projection.Far;

      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var shadow = lightNode.Shadow as CascadedShadow;
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
          shadow.FadeOutRange,
          shadow.Distances[shadow.NumberOfCascades - 1],
          shadow.VisualizeCascades ? 1 : 0,
          shadow.ShadowFog));

        float filterRadius = shadow.FilterRadius;

        // If we use a subset of the Poisson kernel, we have to normalize the scale.
        int numberOfSamples = Math.Min(shadow.NumberOfSamples, StandardShadowMaskRenderer.PoissonKernel.Length);

        // Not all shader passes support cascade visualization. Use a similar pass instead.
        if (shadow.VisualizeCascades)
        {
          if (numberOfSamples < 0)
          {
            numberOfSamples = 4;
          }
          else if (numberOfSamples == 0)
          {
            numberOfSamples = 1;
            filterRadius = 0;
          }
        }

        // The best dithered CSM supports max 22 samples.
        if (shadow.CascadeSelection == ShadowCascadeSelection.BestDithered && numberOfSamples > 22)
          numberOfSamples = 22;

        if (numberOfSamples > 0)
          filterRadius /= StandardShadowMaskRenderer.PoissonKernel[numberOfSamples - 1].Length();

        _parameterParameters2.SetValue(new Vector4(
          shadow.ShadowMap.Width,
          shadow.ShadowMap.Height,
          filterRadius,
          // The StandardShadow.JitterResolution is the number of texels per world unit.
          // In the shader the parameter JitterResolution contains the division by the jitter map size.
          shadow.JitterResolution / _jitterMap.Width));

        // Split distances.
        if (_parameterDistances != null)
        {
          // Set not used entries to large values.
          Vector4F distances = shadow.Distances;
          for (int j = shadow.NumberOfCascades; j < 4; j++)
            distances[j] = 10 * cameraFar;

          _parameterDistances.SetValue((Vector4)distances);
        }

        Debug.Assert(shadow.ViewProjections.Length == 4);
        for (int j = 0; j < _matrices.Length; j++)
          _matrices[j] = viewInverse * shadow.ViewProjections[j];
        
        _parameterShadowMatrices.SetValue(_matrices);

        _parameterDepthBias.SetValue((Vector4)shadow.EffectiveDepthBias);
        _parameterNormalOffset.SetValue((Vector4)shadow.EffectiveNormalOffset);

        Vector3F lightBackwardWorld = lightNode.PoseWorld.Orientation.GetColumn(2);
        _parameterLightDirection.SetValue((Vector3)cameraPose.ToLocalDirection(lightBackwardWorld));
        _parameterNumberOfCascades.SetValue(shadow.NumberOfCascades);
        _parameterShadowMap.SetValue(shadow.ShadowMap);

        var rectangle = GraphicsHelper.GetViewportRectangle(cameraNode, viewport, lightNode);
        Vector2F texCoordTopLeft = new Vector2F(rectangle.Left / (float)viewport.Width, rectangle.Top / (float)viewport.Height);
        Vector2F texCoordBottomRight = new Vector2F(rectangle.Right / (float)viewport.Width, rectangle.Bottom / (float)viewport.Height);
        GraphicsHelper.GetFrustumFarCorners(cameraNode.Camera.Projection, texCoordTopLeft, texCoordBottomRight, _frustumFarCorners);
        _parameterFrustumCorners.SetValue(_frustumFarCorners);

        var pass = GetPass(numberOfSamples, shadow.CascadeSelection, shadow.VisualizeCascades);

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
    private EffectPass GetPass(int numberOfSamples, ShadowCascadeSelection cascadeSelection, bool visualizeCascades)
    {
      if (visualizeCascades)
        numberOfSamples = 32;    // Only these passes support cascade visualization.

      if (numberOfSamples < 0)
      {
        return _effect.Techniques[0].Passes[(int)cascadeSelection];
      }
      if (numberOfSamples == 0)
      {
        return _effect.Techniques[0].Passes[3 + (int)cascadeSelection];
      }
      if (numberOfSamples == 1)
      {
        return _effect.Techniques[0].Passes[6 + (int)cascadeSelection * 5];
      }
      if (numberOfSamples <= 4)
      {
        return _effect.Techniques[0].Passes[6 + (int)cascadeSelection * 5 + 1];
      }
      if (numberOfSamples <= 8)
      {
        return _effect.Techniques[0].Passes[6 + (int)cascadeSelection * 5 + 2];
      }
      if (numberOfSamples <= 16)
      {
        return _effect.Techniques[0].Passes[6 + (int)cascadeSelection * 5 + 3];
      }

      return _effect.Techniques[0].Passes[6 + (int)cascadeSelection * 5 + 4];
    }
    #endregion
  }
}
#endif
