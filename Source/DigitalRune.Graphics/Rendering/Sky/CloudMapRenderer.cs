// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Generates the cloud textures for <see cref="LayeredCloudMap"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="CloudMapRenderer"/> is a <see cref="SceneNodeRenderer"/> that handles
  /// <see cref="CloudLayerNode"/>s. If a <see cref="CloudLayerNode"/> references a 
  /// <see cref="LayeredCloudMap"/> the renderer creates the cloud texture and stores the result in 
  /// the <see cref="CloudMap.Texture"/> property.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer changes the current render target of the graphics device because it uses the 
  /// graphics device to render the cloud maps into internal render targets. The render target
  /// and the viewport of the graphics device are undefined after rendering.
  /// </para>
  /// </remarks>
  public class CloudMapRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly CloudMapLayer EmptyLayer = new CloudMapLayer(null, Matrix33F.Identity, 0, 0, 0);
    private Texture2D _noiseTexture;

    private readonly Effect _effect;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterTexture0Parameters;
    private readonly EffectParameter _parameterTexture1Parameters;
    private readonly EffectParameter _parameterLerp;
    private readonly EffectParameter[] _parameterTextures;
    private readonly EffectParameter[] _parameterDensities;
    private readonly EffectParameter[] _parameterMatrices;
    private readonly EffectParameter _parameterCoverage;
    private readonly EffectParameter _parameterDensity;

    private readonly EffectPass _passLerp;
    private readonly EffectPass _passDensity;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudMapRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The current graphics profile is Reach.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public CloudMapRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        throw new NotSupportedException("The CloudMapRenderer does not support the Reach profile.");

      // One 512x512 noise texture is used for all layers. Each layer which does not have
      // a user defined texture uses a part of this texture.
      _noiseTexture = NoiseHelper.GetGrainTexture(graphicsService, 512);

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Sky/CloudLayer");
      _parameterViewportSize = _effect.Parameters["ViewportSize"];
      _parameterTexture0Parameters = _effect.Parameters["Texture0Parameters"];
      _parameterTexture1Parameters = _effect.Parameters["Texture1Parameters"];
      _parameterLerp = _effect.Parameters["LerpParameter"];

      _parameterTextures = new EffectParameter[LayeredCloudMap.NumberOfTextures];
      _parameterDensities = new EffectParameter[LayeredCloudMap.NumberOfTextures];
      _parameterMatrices = new EffectParameter[LayeredCloudMap.NumberOfTextures];
      for (int i = 0; i < LayeredCloudMap.NumberOfTextures; i++)
      {
        _parameterTextures[i] = _effect.Parameters["NoiseTexture" + i];
        _parameterDensities[i] = _effect.Parameters["Density" + i];
        _parameterMatrices[i] = _effect.Parameters["Matrix" + i];
      }

      _parameterCoverage = _effect.Parameters["Coverage"];
      _parameterDensity = _effect.Parameters["Density"];

      _passLerp = _effect.Techniques[0].Passes["Lerp"];
      _passDensity = _effect.Techniques[0].Passes["Density"];
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          _noiseTexture.Dispose();
          _noiseTexture = null;
        }
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var cloudLayerNode = node as CloudLayerNode;
      return cloudLayerNode != null && cloudLayerNode.CloudMap is LayeredCloudMap;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
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

      var originalRenderTarget = context.RenderTarget;
      var originalViewport = context.Viewport;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.BlendState = BlendState.Opaque;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.DepthStencilState = DepthStencilState.None;

      int frame = context.Frame;
      float deltaTime = (float)context.DeltaTime.TotalSeconds;

      for (int nodeIndex = 0; nodeIndex < numberOfNodes; nodeIndex++)
      {
        var cloudNode = nodes[nodeIndex] as CloudLayerNode;
        if (cloudNode == null)
          continue;

        var cloudMap = cloudNode.CloudMap as LayeredCloudMap;
        if (cloudMap == null)
          continue;

        // We update the cloud map only once per frame.
        if (cloudMap.LastFrame == frame)
          continue;

        cloudMap.LastFrame = frame;

        var layers = cloudMap.Layers;
        var animationTimes = cloudMap.AnimationTimes;
        var sources = cloudMap.SourceLayers;
        var targets = cloudMap.TargetLayers;
        var renderTargets = cloudMap.LayerTextures;

        // Animate the cloud map layers.
        for (int i = 0; i < LayeredCloudMap.NumberOfTextures; i++)
        {
          if (layers[i] == null || layers[i].Texture != null)
            continue;

          if (cloudMap.Random == null)
            cloudMap.Random = new Random(cloudMap.Seed);

          // Make sure there is a user-defined texture or data for procedural textures.
          if (sources[i] == null)
          {
            // Each octave is 128 x 128 (= 1 / 4 of the 512 * 512 noise texture).
            sources[i] = new PackedTexture(null, _noiseTexture, cloudMap.Random.NextVector2F(0, 1), new Vector2F(0.25f));
            targets[i] = new PackedTexture(null, _noiseTexture, cloudMap.Random.NextVector2F(0, 1), new Vector2F(0.25f));
            renderTargets[i] = new RenderTarget2D(graphicsDevice, 128, 128, false, SurfaceFormat.Alpha8, DepthFormat.None);
          }

          // Update animation time.
          animationTimes[i] += deltaTime * layers[i].AnimationSpeed;

          // Update source and target if animation time is beyond 1.
          if (animationTimes[i] > 1)
          {
            // Wrap animation time.
            animationTimes[i] = animationTimes[i] % 1;

            // Swap source and target.
            MathHelper.Swap(ref sources[i], ref targets[i]);

            // Set target to a new random part of the noise texture.
            targets[i].Offset = cloudMap.Random.NextVector2F(0, 1);
          }

          // Lerp source and target together to get the final noise texture.
          graphicsDevice.SetRenderTarget(renderTargets[i]);
          _parameterViewportSize.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
          _parameterTextures[0].SetValue(sources[i].TextureAtlas);
          _parameterTextures[1].SetValue(targets[i].TextureAtlas);
          _parameterTexture0Parameters.SetValue(new Vector4(sources[i].Scale.X, sources[i].Scale.Y, sources[i].Offset.X, sources[i].Offset.Y));
          _parameterTexture1Parameters.SetValue(new Vector4(targets[i].Scale.X, targets[i].Scale.Y, targets[i].Offset.X, targets[i].Offset.Y));
          _parameterLerp.SetValue(animationTimes[i]);
          _passLerp.Apply();
          graphicsDevice.DrawFullScreenQuad();
        }

        // Initialize the cloud map.
        if (cloudMap.Texture == null || cloudMap.Size != cloudMap.Texture.Width)
        {
          cloudMap.Texture.SafeDispose();

          var cloudTexture = new RenderTarget2D(
            graphicsDevice,
            cloudMap.Size,
            cloudMap.Size,
            false,
            SurfaceFormat.Alpha8,
            DepthFormat.None);

          cloudMap.SetTexture(cloudTexture);
        }

        // Combine the layers.
        graphicsDevice.SetRenderTarget((RenderTarget2D)cloudMap.Texture);
        _parameterViewportSize.SetValue(new Vector2(cloudMap.Texture.Width, cloudMap.Texture.Height));
        for (int i = 0; i < LayeredCloudMap.NumberOfTextures; i++)
        {
          var layer = layers[i] ?? EmptyLayer;
          _parameterTextures[i].SetValue(layer.Texture ?? renderTargets[i]);
          _parameterMatrices[i].SetValue((Matrix)new Matrix44F(layer.TextureMatrix, Vector3F.Zero));
          _parameterDensities[i].SetValue(new Vector2(layer.DensityScale, layer.DensityOffset));
        }
        _parameterCoverage.SetValue(cloudMap.Coverage);
        _parameterDensity.SetValue(cloudMap.Density);
        _passDensity.Apply();
        graphicsDevice.DrawFullScreenQuad();
      }

      savedRenderState.Restore();
      graphicsDevice.SetRenderTarget(null);
      context.RenderTarget = originalRenderTarget;
      context.Viewport = originalViewport;
    }
    #endregion
  }
}
#endif
