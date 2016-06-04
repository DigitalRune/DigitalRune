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
  /// Applies fog to opaque geometry using the current G-buffer content.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Fog properties are defined using <see cref="FogNode"/>s. This renderer uses a single 
  /// full-screen pass per <see cref="FogNode"/> to blend the fog color with the current render 
  /// target. It reads the depth buffer (<see cref="RenderContext.GBuffer0"/>) to determine the fog 
  /// intensity for each pixel. This does not work for alpha-blended (transparent) geometry. 
  /// Alpha-blended geometry should be rendered after the fog was applied to opaque geometry.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  public class FogRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Vector3[] _cameraFrustumFarCorners = new Vector3[4];

    private readonly Effect _effect;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterFrustumCorners;
    private readonly EffectParameter _parameterFogParameters;
    private readonly EffectParameter _parameterColor0;
    private readonly EffectParameter _parameterColor1;
    private readonly EffectParameter _parameterHeights;
    private readonly EffectParameter _parameterLightDirection;
    private readonly EffectParameter _parameterScatteringSymmetry;
    private readonly EffectParameter _parameterGBuffer0;
    private readonly EffectPass _passFog;
    private readonly EffectPass _passFogWithHeightFalloff;
    private readonly EffectPass _passFogWithPhase;
    private readonly EffectPass _passFogWithHeightFalloffWithPhase;

    private List<SceneNode> _fogNodes;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FogRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public FogRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      Order = 3;
      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Deferred/Fog");
      _parameterViewportSize = _effect.Parameters["ViewportSize"];
      _parameterFrustumCorners = _effect.Parameters["FrustumCorners"];
      _parameterFogParameters = _effect.Parameters["FogParameters"];
      _parameterColor0 = _effect.Parameters["Color0"];
      _parameterColor1 = _effect.Parameters["Color1"];
      _parameterHeights = _effect.Parameters["Heights"];
      _parameterLightDirection = _effect.Parameters["LightDirection"];
      _parameterScatteringSymmetry = _effect.Parameters["ScatteringSymmetry"];
      _parameterGBuffer0 = _effect.Parameters["GBuffer0"];
      _passFog = _effect.Techniques[0].Passes["Fog"];
      _passFogWithHeightFalloff = _effect.Techniques[0].Passes["FogWithHeightFalloff"];
      _passFogWithPhase = _effect.Techniques[0].Passes["FogWithPhase"];
      _passFogWithHeightFalloffWithPhase = _effect.Techniques[0].Passes["FogWithHeightFalloffWithPhase"];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      return node is FogNode;
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

      context.Validate(_effect);
      context.ThrowIfCameraMissing();
      context.ThrowIfGBuffer0Missing();

      // Fog is not used in all games. --> Early out, if possible.
      int numberOfNodes = nodes.Count;
      if (nodes.Count == 0)
        return;

      if (nodes.Count > 1)
      {
        // Get a sorted list of all fog nodes. 
        if (_fogNodes == null)
          _fogNodes = new List<SceneNode>();

        _fogNodes.Clear();
        for (int i = 0; i < numberOfNodes; i++)
        {
          var node = nodes[i] as FogNode;
          if (node != null)
          {
            _fogNodes.Add(node);
            node.SortTag = node.Priority;
          }
        }

        // Sort ascending. (Fog with lower priority is rendered first.)
        // Note: Since this list is a list of SceneNodes, we use the AscendingNodeComparer 
        // instead of the AscendingFogNodeComparer. The Priority was written to the SortTag, 
        // so this will work.
        _fogNodes.Sort(AscendingNodeComparer.Instance);
        nodes = _fogNodes;
        numberOfNodes = _fogNodes.Count;
      }

      var graphicsDevice = _effect.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);
      graphicsDevice.RasterizerState = RasterizerState.CullNone;
      graphicsDevice.DepthStencilState = DepthStencilState.None;
      graphicsDevice.BlendState = BlendState.AlphaBlend;

      var viewport = graphicsDevice.Viewport;
      _parameterViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));

      var cameraNode = context.CameraNode;
      var cameraPose = cameraNode.PoseWorld;
      GraphicsHelper.GetFrustumFarCorners(cameraNode.Camera.Projection, _cameraFrustumFarCorners);

      // Convert frustum far corners from view space to world space.
      for (int i = 0; i < _cameraFrustumFarCorners.Length; i++)
        _cameraFrustumFarCorners[i] = (Vector3)cameraPose.ToWorldDirection((Vector3F)_cameraFrustumFarCorners[i]);

      _parameterFrustumCorners.SetValue(_cameraFrustumFarCorners);
      _parameterGBuffer0.SetValue(context.GBuffer0);

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      bool directionalLightIsSet = false;
      float scatteringSymmetryStrength = 1;

      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i] as FogNode;
        if (node == null)
          continue;

        // FogNode is visible in current frame.
        node.LastFrame = frame;

        var fog = node.Fog;

        if (fog.Density <= Numeric.EpsilonF)
          continue;

        // Compute actual density and falloff.
        float fogDensity = fog.Density;
        float heightFalloff = fog.HeightFalloff;
        // In previous versions, we gave FogDensity * 2^(-h*y) to the effect. Following code
        // avoids numerical problems where this value is numerically 0. This is now handled
        // in the shader.
        //if (!Numeric.IsZero(heightFalloff))
        //{
        //  float cameraDensity = (float)Math.Pow(2, -heightFalloff * cameraPose.Position.Y);
        //  // Trick: If the heightFalloff is very large, the e^x function can quickly reach
        //  // the float limit! If this happens, the shader will not compute any fog and this
        //  // looks like the fog disappears. To avoid this problem we reduce the heightFalloff
        //  // to keep the result of e^x always within floating point range.
        //  const float Limit = 1e-37f;
        //  if (cameraDensity < Limit)
        //  {
        //    heightFalloff = (float)Math.Log(Limit) / -cameraPose.Position.Y / ConstantsF.Ln2;
        //    cameraDensity = Limit;
        //  }

        //  // Compute actual fog density.
        //  // fogDensity is at world space height 0. If the fog node is on another height,
        //  // we change the fogDensity. 
        //  fogDensity *= (float)Math.Pow(2, -heightFalloff * (-node.PoseWorld.Position.Y));
        //  // Combine camera and fog density.
        //  fogDensity *= cameraDensity;
        //}

        _parameterFogParameters.SetValue(new Vector4(fog.Start, fog.End, fogDensity, heightFalloff));
        _parameterColor0.SetValue((Vector4)fog.Color0);
        _parameterColor1.SetValue((Vector4)fog.Color1);

        // Compute world space reference heights. 
        var fogBaseHeight = node.PoseWorld.Position.Y;
        var height0 = fogBaseHeight + fog.Height0;
        var height1 = fogBaseHeight + fog.Height1;
        // Avoid division by zero in the shader.
        if (Numeric.AreEqual(height0, height1))
          height1 = height0 + 0.0001f;
        _parameterHeights.SetValue(new Vector4(
          cameraNode.PoseWorld.Position.Y, 
          fogBaseHeight, 
          height0, 
          height1));

        var scatteringSymmetry = fog.ScatteringSymmetry;
        bool useScatteringSymmetry = !scatteringSymmetry.IsNumericallyZero;

        if (useScatteringSymmetry)
        {
          if (!directionalLightIsSet)
          {
            scatteringSymmetryStrength = SetDirectionalLightParameter(context, cameraNode);
            directionalLightIsSet = true;
          }
        }

        if (!useScatteringSymmetry || Numeric.IsZero(scatteringSymmetryStrength))
        {
          // No phase function.
          if (Numeric.IsZero(heightFalloff))
            _passFog.Apply();
          else
            _passFogWithHeightFalloff.Apply();
        }
        else
        {
          // Use phase function.
          // Set parameters for phase function.
          _parameterScatteringSymmetry.SetValue((Vector3)scatteringSymmetry * scatteringSymmetryStrength);

          if (Numeric.IsZero(heightFalloff))
            _passFogWithPhase.Apply();
          else
            _passFogWithHeightFalloffWithPhase.Apply();
        }

        graphicsDevice.DrawFullScreenQuad();
      }

      if (_fogNodes != null)
        _fogNodes.Clear();

      savedRenderState.Restore();
    }


    // Sets effect parameter "LightDirection" and returns a scale factor that should be use
    // to scale the user defined scattering symmetry strength.
    private float SetDirectionalLightParameter(RenderContext context, CameraNode cameraNode)
    {
      var directionalLights = context.Scene.Query<GlobalLightQuery>(cameraNode, context).DirectionalLights;
      var lightDirection = directionalLights.Count > 0
                           ? -(Vector3)directionalLights[0].PoseWorld.Orientation.GetColumn(2)
                           : new Vector3(0, -1, 0);

      _parameterLightDirection.SetValue(lightDirection);

      // The scattering symmetry must disappear at the end of the twilight when the sun is
      // below the horizon. 
      if (lightDirection.Y < 0)
        return 1;
      const float limit = 0.2f;
      if (lightDirection.Y > limit)
        return 0;
      return 1 - lightDirection.Y / limit;
    }
    #endregion
  }
}
#endif
