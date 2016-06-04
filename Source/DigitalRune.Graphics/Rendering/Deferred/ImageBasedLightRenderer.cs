// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !WP7
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="ImageBasedLight"/>s into the light buffer.
  /// </summary>
  /// <inheritdoc cref="LightRenderer"/>
  internal class ImageBasedLightRenderer : SceneNodeRenderer
  {
    // Notes:
    // Should IBLs be rendered before or after the ambient light? Currently,
    // we render IBL after the ambient light. --> IBL replaces the ambient light.
    // If you need a different behavior, you can swap the light renderer order:
    // Swap LightRenderer.Renderers[0] with LightRenderer.Renderers[1].
    //
    // Possible improvements:
    // - We could possible optimize blending of multiple IBLs using the stencil buffer:
    //   If we render front-to-back, then the front IBLs could mark the stencil where
    //   they have full opacity. This would save raster operations.
    //   Local IBLs will not have much overlap but all local IBLs overlap the global/infinite IBL.
    // - Hemispheric attenuation for IBL could help if the bottom of objects appear to bright.
    // - We could render up to 16 IBL in one pass (because we have 16 texture slots in XNA).
    //   + Only one fullscreen pass.
    //   + Most of the time we will have a full screen pass for global environment map anyway.
    //   + We can do proper order-independent blending (weighted average + divide by total weights).
    //   - restricted to 16 lights. 
    //   - Need shader permutations for less than 16 lights, e.g. 1, 2, 4, 8, 16
    //   - If we have N maps in one pass, the shader is very complex, but most pixels will only be
    //     affected by 1, 2 or maybe 3 probes.
    //   - If we render one pass per probe, we can use stencil tests (clip volume, marked stencil
    //     where a light probe is 100% and hide all other probes).



    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // Some nested types to sort light probes by a packed ulong sort-ID.
    private struct Job
    {
      public ulong SortId;
      public LightNode LightNode;
    }

    private class Comparer : IComparer<Job>
    {
      public static readonly Comparer Instance = new Comparer();
      public int Compare(Job x, Job y)
      {
        return x.SortId.CompareTo(y.SortId);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Temporary list for sorting.
    private readonly List<Job> _jobs = new List<Job>();

    private readonly Vector3[] _frustumFarCorners = new Vector3[4];

    private readonly Effect _effect;
    private readonly EffectParameter _parameterTransform;
    private readonly EffectParameter _parameterViewportSize;
    private readonly EffectParameter _parameterFrustumCorners;
    private readonly EffectParameter _parameterGBuffer0;
    private readonly EffectParameter _parameterGBuffer1;
    private readonly EffectParameter _parameterParameters0;
    private readonly EffectParameter _parameterParameters1;
    private readonly EffectParameter _parameterParameters2;
    private readonly EffectParameter _parameterParameters3;
    private readonly EffectParameter _parameterParameters4;
    private readonly EffectParameter _parameterPrecomputedTerm;
    private readonly EffectParameter _parameterEnvironmentMap;
    private readonly EffectPass _passClip;
    private readonly EffectPass _passDiffuseAndSpecularLight;
    private readonly EffectPass _passDiffuseLight;
    private readonly EffectPass _passSpecularLight;

    private Submesh _boxSubmesh;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageBasedLightRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public ImageBasedLightRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      _effect = graphicsService.Content.Load<Effect>("DigitalRune/Deferred/ImageBasedLight");
      _parameterTransform = _effect.Parameters["Transform"];
      _parameterViewportSize = _effect.Parameters["ViewportSize"];
      _parameterFrustumCorners = _effect.Parameters["FrustumCorners"];
      _parameterGBuffer0 = _effect.Parameters["GBuffer0"];
      _parameterGBuffer1 = _effect.Parameters["GBuffer1"];
      _parameterParameters0 = _effect.Parameters["Parameters0"];
      _parameterParameters1 = _effect.Parameters["Parameters1"];
      _parameterParameters2 = _effect.Parameters["Parameters2"];
      _parameterParameters3 = _effect.Parameters["Parameters3"];
      _parameterParameters4 = _effect.Parameters["Parameters4"];
      _parameterPrecomputedTerm = _effect.Parameters["PrecomputedTerm"];
      _parameterEnvironmentMap = _effect.Parameters["EnvironmentMap"];
      _passClip = _effect.CurrentTechnique.Passes["Clip"];
      _passDiffuseAndSpecularLight = _effect.CurrentTechnique.Passes["DiffuseAndSpecularLight"];
      _passDiffuseLight = _effect.CurrentTechnique.Passes["DiffuseLight"];
      _passSpecularLight = _effect.CurrentTechnique.Passes["SpecularLight"];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var lightNode = node as LightNode;
      return lightNode != null && lightNode.Light is ImageBasedLight;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
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
      graphicsDevice.BlendState = GraphicsHelper.BlendStateAdd;

      var viewport = graphicsDevice.Viewport;
      _parameterViewportSize.SetValue(new Vector2(viewport.Width, viewport.Height));
      _parameterGBuffer0.SetValue(context.GBuffer0);
      _parameterGBuffer1.SetValue(context.GBuffer1);

      var cameraNode = context.CameraNode;
      Pose cameraPose = cameraNode.PoseWorld;
      Matrix viewProjection = (Matrix)cameraNode.View * cameraNode.Camera.Projection;

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      context.CameraNode.LastFrame = frame;

      bool isHdrEnabled = context.IsHdrEnabled();

      // Copy nodes to list and sort them by persistent IDs. This is necessary to avoid popping when
      // light probes overlap.
      _jobs.Clear();
      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = nodes[i] as LightNode;
        if (lightNode == null)
          continue;

        var light = lightNode.Light as ImageBasedLight;
        if (light == null || light.Texture == null)
          continue;

        // Build sort-ID - high values for lights which should be rendered last.
        ulong sortId = 0;

        // Render infinite lights first and others later.
        if (!(light.Shape is InfiniteShape))
          sortId += ((ulong)1 << 32);  // Set high value above 32-bit range.

        // Sort by priority. Lights with higher priority should be rendered last
        // (= over the other lights).
        // Shift priority (signed int) to positive range and add it.
        sortId += (ulong)((long)lightNode.Priority + int.MaxValue + 1);

        // Shift sortId and add light.Id in least significant bits.
        sortId = (sortId << 16) | (ushort)light.Id;

        // Add to list for sorting.
        _jobs.Add(new Job
        {
          SortId = sortId,
          LightNode = lightNode,
        });
      }

      // Sort by ascending sort-ID value.
      _jobs.Sort(Comparer.Instance);

      numberOfNodes = _jobs.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var lightNode = _jobs[i].LightNode;
        var light = (ImageBasedLight)lightNode.Light;

        // LightNode is visible in current frame.
        lightNode.LastFrame = frame;

        // ReSharper disable CompareOfFloatsByEqualityOperator
        bool enableDiffuse = !(Numeric.IsNaN(light.DiffuseIntensity) || (light.DiffuseIntensity == 0.0f && light.BlendMode == 0.0f));
        // ReSharper restore CompareOfFloatsByEqualityOperator

        bool enableSpecular = !Numeric.IsNaN(light.SpecularIntensity);
        if (!enableDiffuse && !enableSpecular)
          continue;

        float hdrScale = isHdrEnabled ? light.HdrScale : 1;

        // We use 1x1 mipmap level for diffuse.
        // (2x2 is still okay, 4x4 already looks a bit like a specular reflection.)
        float diffuseIntensity = enableDiffuse ? light.DiffuseIntensity : 0.0f;
        _parameterParameters0.SetValue(new Vector4(
          (Vector3)light.Color * diffuseIntensity * hdrScale, // DiffuseColor
          Math.Max(0, light.Texture.LevelCount - 1)));        // Diffuse mip level.

        // Shader supports only RGBM.
        float rgbmMax;
        if (light.Encoding is RgbmEncoding)
        {
          rgbmMax = GraphicsHelper.ToGamma(((RgbmEncoding)light.Encoding).Max);
        }
        else if (light.Encoding is SRgbEncoding)
        {
          // Decoding RGBM with MaxValue 1 is equal to encoding sRGB, i.e. only
          // gamma-to-linear is performed (assuming that the cube map alpha channel is 1).
          rgbmMax = 1;
        }
        else
        {
          throw new NotSupportedException(
            "ImageBasedLight must use sRGB or RGBM encoding. Other encodings are not yet supported.");
        }

        _parameterParameters1.SetValue(new Vector4(
          (Vector3)light.Color * light.SpecularIntensity * hdrScale, // SpecularColor
          rgbmMax));

        // Bounding box can be a box shape or an infinite shape.
        var boundingBoxShape = lightNode.Shape as BoxShape;

        // Get extent of bounding box. For infinite shapes we simply set a large value.
        var boundingBoxExtent = boundingBoxShape != null
                              ? boundingBoxShape.Extent * lightNode.ScaleWorld
                              : new Vector3F(1e20f);

        // Falloff can only be used for box shapes but not for infinite shapes.
        float falloffRange = (boundingBoxShape != null) ? light.FalloffRange : 0;

        // AABB for localization in local space.
        // Use invalid min and max (min > max) to disable localization.
        Aabb projectionAabb = new Aabb(new Vector3F(1), new Vector3F(-1));
        if (light.EnableLocalizedReflection)
        {
          if (light.LocalizedReflectionBox.HasValue)
          {
            // User defined AABB.
            projectionAabb = light.LocalizedReflectionBox.Value;
            projectionAabb.Minimum *= lightNode.ScaleWorld;
            projectionAabb.Maximum *= lightNode.ScaleWorld;
          }
          else if (boundingBoxShape != null)
          {
            // AABB is equal to the bounding box.
            projectionAabb = new Aabb(-boundingBoxExtent / 2, boundingBoxExtent / 2);
          }
        }

        _parameterParameters2.SetValue(new Vector4(
          boundingBoxExtent.X / 2,
          boundingBoxExtent.Y / 2,
          boundingBoxExtent.Z / 2,
          falloffRange));

        _parameterParameters3.SetValue(new Vector4(
          projectionAabb.Minimum.X,
          projectionAabb.Minimum.Y,
          projectionAabb.Minimum.Z,
          light.Texture.Size));

        _parameterParameters4.SetValue(new Vector4(
          projectionAabb.Maximum.X,
          projectionAabb.Maximum.Y,
          projectionAabb.Maximum.Z,
          light.BlendMode));

        // Precomputed value for specular reflection lookup.
        const float sqrt3 = 1.7320508075688772935274463415059f;
        _parameterPrecomputedTerm.SetValue((float)Math.Log(light.Texture.Size * sqrt3, 2.0));

        _parameterEnvironmentMap.SetValue(light.Texture);

        // Compute screen space rectangle and FrustumFarCorners.
        var rectangle = GraphicsHelper.GetViewportRectangle(cameraNode, viewport, lightNode);
        var texCoordTopLeft = new Vector2F(rectangle.Left / (float)viewport.Width, rectangle.Top / (float)viewport.Height);
        var texCoordBottomRight = new Vector2F(rectangle.Right / (float)viewport.Width, rectangle.Bottom / (float)viewport.Height);
        GraphicsHelper.GetFrustumFarCorners(cameraNode.Camera.Projection, texCoordTopLeft, texCoordBottomRight, _frustumFarCorners);

        // Convert frustum far corners from view space to world space.
        for (int j = 0; j < _frustumFarCorners.Length; j++)
          _frustumFarCorners[j] = (Vector3)cameraPose.ToWorldDirection((Vector3F)_frustumFarCorners[j]);

        _parameterFrustumCorners.SetValue(_frustumFarCorners);

        EffectPass passLight = null;
        if (enableDiffuse &&  enableSpecular)
        {
          passLight = _passDiffuseAndSpecularLight;
        }
        else if (enableDiffuse)
        {
          // TODO: Can we disable writes to LightBuffer1?
          passLight = _passDiffuseLight;
        }
        else
        {
          // TODO: Can we disable writes to LightBuffer0?
          passLight = _passSpecularLight;
        }

        // Simply render fullscreen quad if we do not have a clip shape or a bounding box.
        if (lightNode.Clip == null && boundingBoxShape == null)
        {
          graphicsDevice.BlendState = BlendState.AlphaBlend;

          // Transform matrix transforms from world space with camera as origin to
          // local space. The lightNode.Scale is already in the other parameters and not
          // used in Transform.
          var pose = lightNode.PoseWorld;
          pose.Position -= cameraPose.Position;
          _parameterTransform.SetValue(pose.Inverse);

          passLight.Apply();
          graphicsDevice.DrawFullScreenQuad();
          continue;
        }

        // ----- Render clip mesh.
        graphicsDevice.DepthStencilState = GraphicsHelper.DepthStencilStateOnePassStencilFail;
        graphicsDevice.BlendState = GraphicsHelper.BlendStateNoColorWrite;
        if (lightNode.Clip != null)
        {
          // Using user-defined clip shape.
          var data = lightNode.RenderData as LightRenderData;
          if (data == null)
          {
            data = new LightRenderData();
            lightNode.RenderData = data;
          }

          data.UpdateClipSubmesh(context.GraphicsService, lightNode);
          _parameterTransform.SetValue((Matrix)data.ClipMatrix * viewProjection);
          _passClip.Apply();
          data.ClipSubmesh.Draw();

          graphicsDevice.DepthStencilState = lightNode.InvertClip
            ? GraphicsHelper.DepthStencilStateStencilEqual0
            : GraphicsHelper.DepthStencilStateStencilNotEqual0;
        }
        else
        {
          Debug.Assert(boundingBoxShape != null);

          // Use box submesh.
          if (_boxSubmesh == null)
            _boxSubmesh = MeshHelper.GetBox(context.GraphicsService);

          Matrix44F world = lightNode.PoseWorld
                            * Matrix44F.CreateScale(lightNode.ScaleLocal * boundingBoxShape.Extent);
          _parameterTransform.SetValue((Matrix)world * viewProjection);

          _passClip.Apply();
          _boxSubmesh.Draw();

          graphicsDevice.DepthStencilState = GraphicsHelper.DepthStencilStateStencilNotEqual0;
        }

        graphicsDevice.BlendState = BlendState.AlphaBlend;

        {
          // Transform matrix transforms from world space with camera as origin to
          // local space. The lightNode.Scale is already in the other parameters and not
          // used in Transform.
          var pose = lightNode.PoseWorld;
          pose.Position -= cameraPose.Position;
          _parameterTransform.SetValue(pose.Inverse);
        }

        // ----- Render full screen quad.
        passLight.Apply();
        graphicsDevice.DrawQuad(rectangle);
      }

      savedRenderState.Restore();
      _jobs.Clear();
    }
    #endregion
  }
}
#endif
