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
  /// Renders <see cref="CloudLayerNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  internal class CloudLayerRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Effect _effect;
    private readonly EffectParameter _parameterView;
    private readonly EffectParameter _parameterProjection;
    private readonly EffectParameter _parameterSunDirection;
    private readonly EffectParameter _parameterSkyCurvature;
    private readonly EffectParameter _parameterTextureMatrix;
    private readonly EffectParameter _parameterNumberOfSamples;
    private readonly EffectParameter _parameterSampleDistance;
    private readonly EffectParameter _parameterScatterParameters;
    private readonly EffectParameter _parameterHorizonFade;
    private readonly EffectParameter _parameterSunLight;
    private readonly EffectParameter _parameterAmbientLight;
    private readonly EffectParameter _parameterTexture;
    private readonly EffectPass _passCloudRgbLinear;
    private readonly EffectPass _passCloudAlphaLinear;
    private readonly EffectPass _passCloudRgbGamma;
    private readonly EffectPass _passCloudAlphaGamma;
    private readonly EffectPass _passOcclusionRgb;
    private readonly EffectPass _passOcclusionAlpha;

    private readonly Submesh _submesh;
    private readonly Vector3F[] _queryGeometry;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CloudLayerRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The current graphics profile is Reach.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public CloudLayerRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      if (graphicsService.GraphicsDevice.GraphicsProfile == GraphicsProfile.Reach)
        throw new NotSupportedException("The CloudLayerRenderer does not support the Reach profile.");

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Sky/CloudLayer");
      _parameterView = _effect.Parameters["View"];
      _parameterProjection = _effect.Parameters["Projection"];
      _parameterSunDirection = _effect.Parameters["SunDirection"];
      _parameterSkyCurvature = _effect.Parameters["SkyCurvature"];
      _parameterTextureMatrix = _effect.Parameters["Matrix0"];
      _parameterNumberOfSamples = _effect.Parameters["NumberOfSamples"];
      _parameterSampleDistance = _effect.Parameters["SampleDistance"];
      _parameterScatterParameters = _effect.Parameters["ScatterParameters"];
      _parameterHorizonFade = _effect.Parameters["HorizonFade"];
      _parameterSunLight = _effect.Parameters["SunLight"];
      _parameterAmbientLight = _effect.Parameters["AmbientLight"];
      _parameterTexture = _effect.Parameters["NoiseTexture0"];
      _passCloudRgbLinear = _effect.Techniques[0].Passes["CloudRgbLinear"];
      _passCloudAlphaLinear = _effect.Techniques[0].Passes["CloudAlphaLinear"];
      _passCloudRgbGamma = _effect.Techniques[0].Passes["CloudRgbGamma"];
      _passCloudAlphaGamma = _effect.Techniques[0].Passes["CloudAlphaGamma"];
      _passOcclusionRgb = _effect.Techniques[0].Passes["OcclusionRgb"];
      _passOcclusionAlpha = _effect.Techniques[0].Passes["OcclusionAlpha"];

      // We render a spherical patch into the sky. But any mesh which covers the top
      // hemisphere works too.
      //_submesh = MeshHelper.CreateSpherePatch(graphicsService.GraphicsDevice, 1, 1.1f, 10);
      _submesh = MeshHelper.CreateBox(graphicsService.GraphicsDevice);

      _queryGeometry = new Vector3F[4];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is CloudLayerNode;
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
      if (numberOfNodes == 0)
        return;

      context.Validate(_effect);
      context.ThrowIfCameraMissing();

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      // Camera properties
      int viewportHeight = graphicsDevice.Viewport.Height;
      var cameraNode = context.CameraNode;
      var projection = cameraNode.Camera.Projection;
      _parameterProjection.SetValue(projection);

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as CloudLayerNode;
        if (node == null)
          continue;

        // CloudLayerNode is visible in current frame.
        node.LastFrame = frame;

        if (node.CloudMap.Texture == null)
          continue;

        var sunDirection = node.SunDirection;
        _parameterSunDirection.SetValue((Vector3)sunDirection);
        _parameterSkyCurvature.SetValue(node.SkyCurvature);
        _parameterTextureMatrix.SetValue((Matrix)new Matrix44F(node.TextureMatrix, Vector3F.Zero));

        // The sample at the pixel counts as one, the rest are for the blur.
        // Note: We must not set -1 because a for loop like
        //   for (int i = 0; i < -1, i++)
        // crashes the AMD DX9 WP8.1 graphics driver. LOL
        _parameterNumberOfSamples.SetValue(Math.Max(0, node.NumberOfSamples - 1));  

        _parameterSampleDistance.SetValue(node.SampleDistance);
        _parameterScatterParameters.SetValue(new Vector3(node.ForwardScatterExponent, node.ForwardScatterScale, node.ForwardScatterOffset));
        _parameterHorizonFade.SetValue(new Vector2(node.HorizonFade, node.HorizonBias));
        _parameterSunLight.SetValue((Vector3)node.SunLight);
        _parameterAmbientLight.SetValue(new Vector4((Vector3)node.AmbientLight, node.Alpha));
        _parameterTexture.SetValue(node.CloudMap.Texture);

        // Occlusion query.
        if (graphicsDevice.GraphicsProfile != GraphicsProfile.Reach && node.SunQuerySize >= Numeric.EpsilonF)
        {
          bool skipQuery = false;
          if (node.OcclusionQuery != null)
          {
            if (node.OcclusionQuery.IsComplete)
            {
              node.TryUpdateSunOcclusion();
            }
            else
            {
              // The previous query is still not finished. Do not start a new query, this would
              // create a SharpDX warning.
              skipQuery = true;
            }
          }
          else
          {
            node.OcclusionQuery = new OcclusionQuery(graphicsDevice);
          }

          if (!skipQuery)
          {
            node.IsQueryPending = true;

            float totalPixels = viewportHeight * node.SunQuerySize;
            totalPixels *= totalPixels;
            node.QuerySize = totalPixels;

            // Use a camera which looks at the sun.
            // Get an relative up vector which is not parallel to the forward direction.
            var lookAtUp = Vector3F.UnitY;
            if (Vector3F.AreNumericallyEqual(sunDirection, lookAtUp))
              lookAtUp = Vector3F.UnitZ;

            Vector3F zAxis = -sunDirection;
            Vector3F xAxis = Vector3F.Cross(lookAtUp, zAxis).Normalized;
            Vector3F yAxis = Vector3F.Cross(zAxis, xAxis);

            var lookAtSunView = new Matrix(xAxis.X, yAxis.X, zAxis.X, 0,
                                           xAxis.Y, yAxis.Y, zAxis.Y, 0,
                                           xAxis.Z, yAxis.Z, zAxis.Z, 0,
                                           0, 0, 0, 1);
            _parameterView.SetValue(lookAtSunView);

            graphicsDevice.BlendState = GraphicsHelper.BlendStateNoColorWrite;
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.RasterizerState = RasterizerState.CullNone;

            // Create small quad shortly behind the near plane. 
            // Note: We use an "untranslated" view matrix, so we can ignore the camera position.
            float width = (projection.Top - projection.Bottom) * node.SunQuerySize;
            Vector3F right = sunDirection.Orthonormal1 * (width / 2);
            Vector3F up = sunDirection.Orthonormal2 * (width / 2);
            Vector3F center = sunDirection * (projection.Near * 1.0001f);
            _queryGeometry[0] = center - up - right;
            _queryGeometry[1] = center + up - right;
            _queryGeometry[2] = center - up + right;
            _queryGeometry[3] = center + up + right;

            if (node.CloudMap.Texture.Format == SurfaceFormat.Alpha8)
              _passOcclusionAlpha.Apply();
            else
              _passOcclusionRgb.Apply();

            node.OcclusionQuery.Begin();
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _queryGeometry, 0, 2,
              VertexPosition.VertexDeclaration);
            node.OcclusionQuery.End();
          }
        }
        else
        {
          node.IsQueryPending = false;
          node.SunOcclusion = 0;
        }

        Matrix viewUntranslated = (Matrix)new Matrix44F(cameraNode.PoseWorld.Orientation.Transposed, new Vector3F(0));
        _parameterView.SetValue(viewUntranslated);

        // Render clouds.
        graphicsDevice.BlendState = BlendState.AlphaBlend;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;
        graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

        if (context.IsHdrEnabled())
        {
          if (node.CloudMap.Texture.Format == SurfaceFormat.Alpha8)
            _passCloudAlphaLinear.Apply();
          else
            _passCloudRgbLinear.Apply();
        }
        else
        {
          if (node.CloudMap.Texture.Format == SurfaceFormat.Alpha8)
            _passCloudAlphaGamma.Apply();
          else
            _passCloudRgbGamma.Apply();
        }

        _submesh.Draw();
      }

      savedRenderState.Restore();
    }
    #endregion
  }
}
#endif
