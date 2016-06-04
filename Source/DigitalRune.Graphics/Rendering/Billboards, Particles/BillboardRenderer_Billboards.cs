// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Collections;
using Microsoft.Xna.Framework.Content;
#if !WP7
using DigitalRune.Graphics.PostProcessing;
#endif
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#if PARTICLES
using System;
using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Particles;
#endif


namespace DigitalRune.Graphics.Rendering
{
  partial class BillboardRenderer
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

#if PARTICLES
    /// <summary>
    /// Stores the index of a particle and its distance for per-particle depth sorting.
    /// </summary>
    private struct ParticleIndex
    {
      public float Distance;
      public int Index;
      public Vector3F Position;
    }


    /// <summary>
    /// Sorts particles back-to-front.
    /// </summary>
    private sealed class ParticleIndexComparer : IComparer<ParticleIndex>
    {
      public readonly static ParticleIndexComparer Instance = new ParticleIndexComparer();

      public int Compare(ParticleIndex indexA, ParticleIndex indexB)
      {
        if (indexA.Distance > indexB.Distance)
          return -1;

        if (indexA.Distance < indexB.Distance)
          return +1;

        return 0;
      }
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The blend state for rendering billboards into the off-screen buffer.
    /// </summary>
    private static readonly BlendState BlendStateOffscreen = new BlendState
    {
      Name = "BillboardRenderer.BlendStateOffscreen",
      ColorBlendFunction = BlendFunction.Add,
      ColorSourceBlend = Blend.One,
      ColorDestinationBlend = Blend.InverseSourceAlpha,

      // Separate alpha blend function (requires HiDef profile!).
      AlphaBlendFunction = BlendFunction.Add,
      AlphaSourceBlend = Blend.Zero,
      AlphaDestinationBlend = Blend.InverseSourceAlpha,
    };


    // ----- Not used: Combine pass is done in shader without alpha blending.
    ///// <summary>
    ///// The blend state for upsampling the off-screen buffer and combining the result with the
    ///// current render target.
    ///// </summary>
    //private static readonly BlendState BlendStateCombine = new BlendState
    //{
    //  Name = "BillboardRenderer.BlendStateCombine",
    //  ColorBlendFunction = BlendFunction.Add,
    //  ColorSourceBlend = Blend.One,
    //  ColorDestinationBlend = Blend.SourceAlpha,

    //  // Separate alpha blend function (requires HiDef profile!).
    //  AlphaBlendFunction = BlendFunction.Add,
    //  AlphaSourceBlend = Blend.Zero,
    //  AlphaDestinationBlend = Blend.One,
    //};

    private bool _hiDef;

    private Effect _billboardEffect;
    private IBillboardBatch _billboardBatch;

    // A white 1x1 texture that is used if no other texture is specified.
    // (Can be used for debugging.)
    private PackedTexture _debugTexture;

    // Billboard.fx (HiDef profile)
    private EffectParameter _parameterView;
    private EffectParameter _parameterViewInverse;
    private EffectParameter _parameterViewProjection;
    private EffectParameter _parameterProjection;
    private EffectParameter _parameterCameraPosition;
    private EffectParameter _parameterViewportSize;
    private EffectParameter _parameterDepthBuffer;
    private EffectParameter _parameterCameraNear;
    private EffectParameter _parameterCameraFar;
    private EffectParameter _parameterTexture;

    private EffectTechnique _techniqueHardLinear;
    private EffectTechnique _techniqueHardGamma;
    private EffectTechnique _techniqueSoftLinear;
    private EffectTechnique _techniqueSoftGamma;

    private Texture2D _depthBufferHalf;
    private RenderTarget2D _offscreenBuffer;
#if !WP7
    private UpsampleFilter _upsampleFilter;
#endif

    // true, if rendering billboards or particles. (The value is set in 
    // BeginBillboards() and reset in EndBillboards().)
    private bool _billboardMode;

#if PARTICLES
    // For depth-sorting of particles, created on demand.
    private ArrayList<ParticleIndex> _particleIndices;
#endif
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    private void InitializeBillboards(IGraphicsService graphicsService)
    {
      _debugTexture = new PackedTexture(graphicsService.GetDefaultTexture2DWhite());

      var graphicsDevice = GraphicsService.GraphicsDevice;
      _hiDef = (graphicsDevice.GraphicsProfile == GraphicsProfile.HiDef);

      if (_hiDef)
      {
        // ----- HiDef profile
        _billboardEffect = GraphicsService.Content.Load<Effect>("DigitalRune/Billboard");
        _parameterView = _billboardEffect.Parameters["View"];
        _parameterViewInverse = _billboardEffect.Parameters["ViewInverse"];
        _parameterViewProjection = _billboardEffect.Parameters["ViewProjection"];
        _parameterProjection = _billboardEffect.Parameters["Projection"];
        _parameterCameraPosition = _billboardEffect.Parameters["CameraPosition"];
        _parameterViewportSize = _billboardEffect.Parameters["ViewportSize"];
        _parameterDepthBuffer = _billboardEffect.Parameters["DepthBuffer"];
        _parameterCameraNear = _billboardEffect.Parameters["CameraNear"];
        _parameterCameraFar = _billboardEffect.Parameters["CameraFar"];
        _parameterTexture = _billboardEffect.Parameters["Texture"];

        _techniqueHardLinear = _billboardEffect.Techniques["HardLinear"];
        _techniqueHardGamma = _billboardEffect.Techniques["HardGamma"];
        _techniqueSoftLinear = _billboardEffect.Techniques["SoftLinear"];
        _techniqueSoftGamma = _billboardEffect.Techniques["SoftGamma"];

        _billboardBatch = new BillboardBatchHiDef(graphicsDevice, BufferSize);
      }
      else
      {
        // ----- Reach profile
        _billboardEffect = new BasicEffect(graphicsDevice)
        {
          FogEnabled = false,
          LightingEnabled = false,
          TextureEnabled = true,
          VertexColorEnabled = true,
          World = Matrix.Identity,
        };

        _billboardBatch = new BillboardBatchReach(graphicsDevice, BufferSize);
      }
    }


    private void DisposeBillboards()
    {
      _billboardBatch.Dispose();

      // Note: Do not expose effect in HiDef profile. The effect is managed by
      // the ContentManager and may be shared.
      if (!_hiDef)
        _billboardEffect.Dispose();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Prepare effect for rendering billboards.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void PrepareBillboards(RenderContext context)
    {
      var graphicsDevice = GraphicsService.GraphicsDevice;
      var cameraNode = context.CameraNode;
      if (_hiDef)
      {
        // ----- HiDef profile
        _parameterView.SetValue((Matrix)cameraNode.View);
        _parameterViewInverse.SetValue((Matrix)cameraNode.ViewInverse);
        _parameterViewProjection.SetValue((Matrix)(cameraNode.Camera.Projection * cameraNode.View));
        _parameterProjection.SetValue(cameraNode.Camera.Projection);
        _parameterCameraPosition.SetValue((Vector3)cameraNode.PoseWorld.Position);
        _parameterCameraNear.SetValue(cameraNode.Camera.Projection.Near);
        _parameterCameraFar.SetValue(cameraNode.Camera.Projection.Far);

        // Select effect technique.
        if (EnableOffscreenRendering || EnableSoftParticles)
          _billboardEffect.CurrentTechnique = context.IsHdrEnabled() ? _techniqueSoftLinear : _techniqueSoftGamma;
        else
          _billboardEffect.CurrentTechnique = context.IsHdrEnabled() ? _techniqueHardLinear : _techniqueHardGamma;

        if (!EnableOffscreenRendering && !EnableSoftParticles)
        {
          // Render at full resolution.
          _parameterViewportSize.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
        }
        else if (!EnableOffscreenRendering && EnableSoftParticles)
        {
          // Render at full resolution with depth test in pixel shader.
          context.ThrowIfGBuffer0Missing();
          _parameterDepthBuffer.SetValue(context.GBuffer0);
          _parameterViewportSize.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
        }
        else if (EnableOffscreenRendering)
        {
          // Render at half resolution into off-screen buffer.
          object dummy;
          context.Data.TryGetValue(RenderContextKeys.DepthBufferHalf, out dummy);
          _depthBufferHalf = dummy as Texture2D;
          if (_depthBufferHalf == null)
          {
            string message = "Downsampled depth buffer is not set in render context. (The downsampled "
                             + "depth buffer (half width and height) is required by the BillboardRenderer "
                             + "to render off-screen particles. It needs to be stored in "
                             + "RenderContext.Data[RenderContextKeys.DepthBufferHalf].)";
            throw new GraphicsException(message);
          }

          _parameterDepthBuffer.SetValue(_depthBufferHalf);
          _parameterViewportSize.SetValue(new Vector2(_depthBufferHalf.Width, _depthBufferHalf.Height));
        }
      }
      else
      {
        // ----- Reach profile
        var basicEffect = (BasicEffect)_billboardEffect;
        basicEffect.View = (Matrix)cameraNode.View;
        basicEffect.Projection = cameraNode.Camera.Projection;
      }
    }


    // Sets the render states for rendering billboards.
    private void BeginBillboards(RenderContext context)
    {
      if (_billboardMode)
        return;

      _billboardMode = true;

      var graphicsDevice = GraphicsService.GraphicsDevice;
      graphicsDevice.RasterizerState = RasterizerState.CullNone;

      if (_hiDef)
      {
        // ----- HiDef profile
        if (!EnableOffscreenRendering && !EnableSoftParticles)
        {
          // Render at full resolution.
          graphicsDevice.BlendState = BlendState.AlphaBlend;
        }
        else if (!EnableOffscreenRendering && EnableSoftParticles)
        {
          // Render at full resolution with depth test in pixel shader.
          graphicsDevice.BlendState = BlendState.AlphaBlend;
          graphicsDevice.DepthStencilState = DepthStencilState.None;
        }
        else if (EnableOffscreenRendering)
        {
          // Render at half resolution into off-screen buffer.
          graphicsDevice.BlendState = BlendStateOffscreen;
          graphicsDevice.DepthStencilState = DepthStencilState.None;

          var sceneRenderTarget = context.RenderTarget;
          _offscreenBuffer = GraphicsService.RenderTargetPool.Obtain2D(
            new RenderTargetFormat(_depthBufferHalf.Width, _depthBufferHalf.Height, false, sceneRenderTarget.Format, DepthFormat.None));
          graphicsDevice.SetRenderTarget(_offscreenBuffer);

          graphicsDevice.Clear(Color.Black);
        }
      }
      else
      {
        // ----- Reach profile
        graphicsDevice.BlendState = BlendState.AlphaBlend;
      }
    }


    private void EndBillboards(RenderContext context)
    {
      if (!_billboardMode)
        return;

      _billboardMode = false;

      // Reset texture to prevent "memory leak".
      SetTexture(null);

#if !WP7
      if (EnableOffscreenRendering)
      {
        // ----- Combine off-screen buffer with scene.
        if (_upsampleFilter == null)
          _upsampleFilter = GraphicsService.GetUpsampleFilter();

        var graphicsDevice = GraphicsService.GraphicsDevice;
        graphicsDevice.BlendState = BlendState.Opaque;

        // The previous scene render target is bound as texture.
        // --> Switch scene render targets!
        var sceneTexture = context.RenderTarget;
        var renderTargetPool = GraphicsService.RenderTargetPool;
        var renderTarget = renderTargetPool.Obtain2D(new RenderTargetFormat(sceneTexture));
        context.SourceTexture = _offscreenBuffer;
        context.SceneTexture = sceneTexture;
        context.RenderTarget = renderTarget;

        _upsampleFilter.Mode = UpsamplingMode;
        _upsampleFilter.DepthThreshold = DepthThreshold;
        _upsampleFilter.RebuildZBuffer = true;

        _upsampleFilter.Process(context);

        context.SourceTexture = null;
        context.SceneTexture = null;
        renderTargetPool.Recycle(_offscreenBuffer);
        renderTargetPool.Recycle(sceneTexture);
        _depthBufferHalf = null;
        _offscreenBuffer = null;
      }
#endif
    }


    private void DrawBillboards(int index, int endIndex, RenderContext context)
    {
      // Update and apply effect.
      SetTexture(index);
      _billboardEffect.CurrentTechnique.Passes[0].Apply();

      _billboardBatch.Begin(context);

      var jobs = _jobs.Array;
      while (index < endIndex)
      {
        var node = jobs[index].Node;

#if PARTICLES
        var particleSystemData = jobs[index].ParticleSystemData;
        if (particleSystemData != null)
          Draw((ParticleSystemNode)node, particleSystemData);
        else
#endif
          Draw((BillboardNode)node);

        index++;
      }

      _billboardBatch.End();
    }


    private void SetTexture(int index)
    {
      var jobs = _jobs.Array;
      PackedTexture texture;

#if PARTICLES
      var particleSystemData = jobs[index].ParticleSystemData;
      if (particleSystemData != null)
      {
        // Particles
        texture = particleSystemData.Texture;
      }
      else
#endif
      {
        // Billboard
        var billboardNode = (BillboardNode)jobs[index].Node;
        var billboard = (ImageBillboard)billboardNode.Billboard;
        texture = billboard.Texture;
      }

      // Fallback
      if (texture == null)
        texture = _debugTexture;

      ResetTextureId(texture);

      SetTexture(texture.TextureAtlas);
    }


    private void SetTexture(Texture2D texture)
    {
      if (_hiDef)
      {
        // ----- HiDef profile
        _parameterTexture.SetValue(texture);
      }
      else
      {
        // ----- Reach profile
        var basicEffect = (BasicEffect)_billboardEffect;
        basicEffect.Texture = texture;
      }
    }


    private void Draw(BillboardNode node)
    {
      var billboard = (ImageBillboard)node.Billboard;
      var data = new BillboardArgs
      {
        Position = node.PoseWorld.Position,
        Normal = (billboard.Orientation.Normal == BillboardNormal.ViewPlaneAligned) ? _defaultNormal : node.Normal,
        Axis = node.Axis,
        Orientation = billboard.Orientation,
        Size = node.ScaleWorld.Y * billboard.Size, // Assume uniform scale for size.
        Softness = Numeric.IsNaN(billboard.Softness) ? -1 : billboard.Softness,
        Color = node.Color * billboard.Color,
        Alpha = node.Alpha * billboard.Alpha,
        ReferenceAlpha = billboard.AlphaTest,
        AnimationTime = (Numeric.IsNaN(node.AnimationTime)) ? billboard.AnimationTime : node.AnimationTime,
        BlendMode = billboard.BlendMode,
      };

      var texture = billboard.Texture ?? _debugTexture;
      _billboardBatch.DrawBillboard(ref data, texture);
    }


#if PARTICLES
    private void Draw(ParticleSystemNode node, ParticleSystemData particleSystemData)
    {
      // Scale and pose.
      Vector3F scale = Vector3F.One;
      Pose pose = Pose.Identity;
      bool requiresTransformation = (particleSystemData.ReferenceFrame == ParticleReferenceFrame.Local);
      if (requiresTransformation)
      {
        scale = node.ScaleWorld;
        pose = node.PoseWorld * particleSystemData.Pose;
      }

      // Tint color and alpha.
      Vector3F color = node.Color;
      float alpha = node.Alpha;
      float angleOffset = node.AngleOffset;

      if (particleSystemData.IsRibbon)
      {
        if (particleSystemData.AxisParameter == null)
        {
          // Ribbons with automatic axis.
          DrawParticleRibbonsAuto(particleSystemData, requiresTransformation, ref scale, ref pose, ref color, alpha);
        }
        else
        {
          // Ribbons with fixed axis.
          DrawParticleRibbonsFixed(particleSystemData, requiresTransformation, ref scale, ref pose, ref color, alpha);
        }
      }
      else if (particleSystemData.IsDepthSorted)
      {
        // Particles sorted by depth.
        DrawParticlesBackToFront(particleSystemData, requiresTransformation, ref scale, ref pose, ref color, alpha, angleOffset);
      }
      else
      {
        // Particles sorted by age.
        DrawParticlesOldToNew(particleSystemData, requiresTransformation, ref scale, ref pose, ref color, alpha, angleOffset);
      }
    }


    #region ----- Particles -----

    private void DrawParticlesOldToNew(ParticleSystemData particleSystemData, bool requiresTransformation, ref Vector3F scale, ref Pose pose, ref Vector3F color, float alpha, float angleOffset)
    {
      var b = new BillboardArgs
      {
        Orientation = particleSystemData.BillboardOrientation,
        Softness = particleSystemData.Softness,
        ReferenceAlpha = particleSystemData.AlphaTest,
      };

      int numberOfParticles = particleSystemData.Particles.Count;
      var particles = particleSystemData.Particles.Array;
      bool isViewPlaneAligned = (particleSystemData.BillboardOrientation.Normal == BillboardNormal.ViewPlaneAligned);
      bool isAxisInViewSpace = particleSystemData.BillboardOrientation.IsAxisInViewSpace;

      for (int i = 0; i < numberOfParticles; i++)
      {
        if (particles[i].IsAlive) // Skip dead particles.
        {
          if (requiresTransformation)
          {
            b.Position = pose.ToWorldPosition(particles[i].Position * scale);
            b.Normal = isViewPlaneAligned ? _defaultNormal : pose.ToWorldDirection(particles[i].Normal);
            b.Axis = isAxisInViewSpace ? particles[i].Axis : pose.ToWorldDirection(particles[i].Axis);
            b.Size = particles[i].Size * scale.Y; // Assume uniform scale for size.
          }
          else
          {
            b.Position = particles[i].Position;
            b.Normal = isViewPlaneAligned ? _defaultNormal : particles[i].Normal;
            b.Axis = particles[i].Axis;
            b.Size = particles[i].Size;
          }

          b.Angle = particles[i].Angle + angleOffset;
          b.Color = particles[i].Color * color;
          b.Alpha = particles[i].Alpha * alpha;
          b.AnimationTime = particles[i].AnimationTime;
          b.BlendMode = particles[i].BlendMode;

          var texture = particleSystemData.Texture ?? _debugTexture;
          _billboardBatch.DrawBillboard(ref b, texture);
        }
      }
    }


    private void DrawParticlesBackToFront(ParticleSystemData particleSystemData, bool requiresTransformation, ref Vector3F scale, ref Pose pose, ref Vector3F color, float alpha, float angleOffset)
    {
      var b = new BillboardArgs
      {
        Orientation = particleSystemData.BillboardOrientation,
        Softness = particleSystemData.Softness,
        ReferenceAlpha = particleSystemData.AlphaTest,
      };

      int numberOfParticles = particleSystemData.Particles.Count;
      var particles = particleSystemData.Particles.Array;

      if (_particleIndices == null)
      {
        _particleIndices = new ArrayList<ParticleIndex>(numberOfParticles);
      }
      else
      {
        _particleIndices.Clear();
        _particleIndices.EnsureCapacity(numberOfParticles);
      }

      // Use linear distance for viewpoint-oriented and world-oriented billboards.
      bool useLinearDistance = (particleSystemData.BillboardOrientation.Normal != BillboardNormal.ViewPlaneAligned);

      // Compute positions and distance to camera.
      for (int i = 0; i < numberOfParticles; i++)
      {
        if (particles[i].IsAlive) // Skip dead particles.
        {
          var particleIndex = new ParticleIndex();
          particleIndex.Index = i;
          if (requiresTransformation)
            particleIndex.Position = pose.ToWorldPosition(particles[i].Position * scale);
          else
            particleIndex.Position = particles[i].Position;

          // Planar distance: Project vector onto look direction.
          Vector3F cameraToParticle = particleIndex.Position - _cameraPose.Position;
          particleIndex.Distance = Vector3F.Dot(cameraToParticle, _cameraForward);
          if (useLinearDistance)
            particleIndex.Distance = cameraToParticle.Length * Math.Sign(particleIndex.Distance);

          _particleIndices.Add(ref particleIndex);
        }
      }

      // Sort particles back-to-front.
      _particleIndices.Sort(ParticleIndexComparer.Instance);

      bool isViewPlaneAligned = (particleSystemData.BillboardOrientation.Normal == BillboardNormal.ViewPlaneAligned);
      bool isAxisInViewSpace = particleSystemData.BillboardOrientation.IsAxisInViewSpace;

      // Draw sorted particles.
      var indices = _particleIndices.Array;
      numberOfParticles = _particleIndices.Count; // Dead particles have been removed.
      for (int i = 0; i < numberOfParticles; i++)
      {
        int index = indices[i].Index;
        b.Position = indices[i].Position;
        if (requiresTransformation)
        {
          b.Normal = isViewPlaneAligned ? _defaultNormal : pose.ToWorldDirection(particles[index].Normal);
          b.Axis = isAxisInViewSpace ? particles[index].Axis : pose.ToWorldDirection(particles[index].Axis);
          b.Size = particles[index].Size * scale.Y; // Assume uniform scale for size.
        }
        else
        {
          b.Normal = isViewPlaneAligned ? _defaultNormal : particles[index].Normal;
          b.Axis = particles[index].Axis;
          b.Size = particles[index].Size;
        }

        b.Angle = particles[index].Angle + angleOffset;
        b.Color = particles[index].Color * color;
        b.Alpha = particles[index].Alpha * alpha;
        b.AnimationTime = particles[index].AnimationTime;
        b.BlendMode = particles[index].BlendMode;

        var texture = particleSystemData.Texture ?? _debugTexture;
        _billboardBatch.DrawBillboard(ref b, texture);
      }
    }
    #endregion


    #region ----- Ribbons -----

    // Particle ribbons:
    // Particles can be rendered as ribbons (a.k.a. trails, lines). Subsequent living 
    // particles are connected using rectangles.
    //   +--------------+--------------+
    //   |              |              |
    //   p0             p1             p2
    //   |              |              |
    //   +--------------+--------------+
    // At least two living particles are required to create a ribbon. Dead particles 
    // ("NormalizedAge" ≥ 1) can be used as delimiters to terminate one ribbon and 
    // start the next ribbon.
    // 
    // p0 and p1 can have different colors and alpha values to create color gradients 
    // or a ribbon that fades in/out.

    private void DrawParticleRibbonsFixed(ParticleSystemData particleSystemData, bool requiresTransformation, ref Vector3F scale, ref Pose pose, ref Vector3F color, float alpha)
    {
      // At least two particles are required to create a ribbon.
      int numberOfParticles = particleSystemData.Particles.Count;
      if (numberOfParticles < 2)
        return;

      var particles = particleSystemData.Particles.Array;
      bool isAxisInViewSpace = particleSystemData.BillboardOrientation.IsAxisInViewSpace;
      int index = 0;
      do
      {
        // ----- Skip dead particles.
        while (index < numberOfParticles && !particles[index].IsAlive)
          index++;

        // ----- Start of new ribbon.
        int endIndex = index + 1;
        while (endIndex < numberOfParticles && particles[endIndex].IsAlive)
          endIndex++;

        int numberOfSegments = endIndex - index - 1;

        var p0 = new RibbonArgs
        {
          // Uniform parameters
          Softness = particleSystemData.Softness,
          ReferenceAlpha = particleSystemData.AlphaTest
        };

        var p1 = new RibbonArgs
        {
          // Uniform parameters
          Softness = particleSystemData.Softness,
          ReferenceAlpha = particleSystemData.AlphaTest
        };

        p0.Axis = particles[index].Axis;
        if (requiresTransformation)
        {
          p0.Position = pose.ToWorldPosition(particles[index].Position * scale);
          if (!isAxisInViewSpace)
            p0.Axis = pose.ToWorldDirection(p0.Axis);

          p0.Size = particles[index].Size.Y * scale.Y;
        }
        else
        {
          p0.Position = particles[index].Position;
          p0.Size = particles[index].Size.Y;
        }

        p0.Color = particles[index].Color * color;
        p0.Alpha = particles[index].Alpha * alpha;
        p0.AnimationTime = particles[index].AnimationTime;
        p0.BlendMode = particles[index].BlendMode;
        p0.TextureCoordinateU = 0;

        index++;
        while (index < endIndex)
        {
          p1.Axis = particles[index].Axis;
          if (requiresTransformation)
          {
            p1.Position = pose.ToWorldPosition(particles[index].Position * scale);
            if (!isAxisInViewSpace)
              p1.Axis = pose.ToWorldDirection(p1.Axis);

            p1.Size = particles[index].Size.Y * scale.Y;
          }
          else
          {
            p1.Position = particles[index].Position;
            p1.Size = particles[index].Size.Y;
          }

          p1.Color = particles[index].Color * color;
          p1.Alpha = particles[index].Alpha * alpha;
          p1.AnimationTime = particles[index].AnimationTime;
          p1.BlendMode = particles[index].BlendMode;
          p1.TextureCoordinateU = GetTextureCoordinateU1(index - 1, numberOfSegments, particleSystemData.TextureTiling);

          // Draw ribbon segment.
          var texture = particleSystemData.Texture ?? _debugTexture;
          _billboardBatch.DrawRibbon(ref p0, ref p1, texture);

          p0 = p1;
          p0.TextureCoordinateU = GetTextureCoordinateU0(index, numberOfSegments, particleSystemData.TextureTiling);
          index++;
        }
      } while (index < numberOfParticles);
    }


    private void DrawParticleRibbonsAuto(ParticleSystemData particleSystemData, bool requiresTransformation, ref Vector3F scale, ref Pose pose, ref Vector3F color, float alpha)
    {
      // At least two particles are required to create a ribbon.
      int numberOfParticles = particleSystemData.Particles.Count;
      if (numberOfParticles < 2)
        return;

      // The up axis is not defined and needs to be derived automatically:
      // - Compute tangents along the ribbon curve.
      // - Build cross-products of normal and tangent vectors.

      // Is normal uniform across all particles?
      Vector3F? uniformNormal;
      switch (particleSystemData.BillboardOrientation.Normal)
      {
        case BillboardNormal.ViewPlaneAligned:
          uniformNormal = _defaultNormal;
          break;

        case BillboardNormal.ViewpointOriented:
          uniformNormal = _cameraPose.Position - pose.Position;
          if (!uniformNormal.Value.TryNormalize())
            uniformNormal = _defaultNormal;
          break;

        default:
          var normalParameter = particleSystemData.NormalParameter;
          if (normalParameter == null)
          {
            uniformNormal = _defaultNormal;
          }
          else if (normalParameter.IsUniform)
          {
            uniformNormal = normalParameter.DefaultValue;
            if (requiresTransformation)
              uniformNormal = pose.ToWorldDirection(uniformNormal.Value);
          }
          else
          {
            // Normal is set in particle data.
            uniformNormal = null;
          }
          break;
      }

      var texture = particleSystemData.Texture ?? _debugTexture;
      var particles = particleSystemData.Particles.Array;
      int index = 0;
      do
      {
        // ----- Skip dead particles.
        while (index < numberOfParticles && !particles[index].IsAlive)
          index++;

        // ----- Start of new ribbon.
        int endIndex = index + 1;
        while (endIndex < numberOfParticles && particles[endIndex].IsAlive)
          endIndex++;

        int numberOfSegments = endIndex - index - 1;

        var p0 = new RibbonArgs
        {
          // Uniform parameters
          Softness = particleSystemData.Softness,
          ReferenceAlpha = particleSystemData.AlphaTest
        };

        var p1 = new RibbonArgs
        {
          // Uniform parameters
          Softness = particleSystemData.Softness,
          ReferenceAlpha = particleSystemData.AlphaTest
        };

        // Compute axes and render ribbon.
        // First particle.
        if (requiresTransformation)
        {
          p0.Position = pose.ToWorldPosition(particles[index].Position * scale);
          p0.Size = particles[index].Size.Y * scale.Y;
        }
        else
        {
          p0.Position = particles[index].Position;
          p0.Size = particles[index].Size.Y;
        }

        p0.Color = particles[index].Color * color;
        p0.Alpha = particles[index].Alpha * alpha;
        p0.AnimationTime = particles[index].AnimationTime;
        p0.BlendMode = particles[index].BlendMode;
        p0.TextureCoordinateU = 0;

        index++;
        Vector3F nextPosition;
        if (requiresTransformation)
          nextPosition = pose.ToWorldPosition(particles[index].Position * scale);
        else
          nextPosition = particles[index].Position;

        Vector3F normal;
        if (uniformNormal.HasValue)
        {
          // Uniform normal.
          normal = uniformNormal.Value;
        }
        else
        {
          // Varying normal.
          normal = particles[index].Normal;
          if (requiresTransformation)
            normal = pose.ToWorldDirection(normal);
        }

        Vector3F previousDelta = nextPosition - p0.Position;
        p0.Axis = Vector3F.Cross(normal, previousDelta);
        p0.Axis.TryNormalize();

        // Intermediate particles.
        while (index < endIndex - 1)
        {
          p1.Position = nextPosition;

          if (requiresTransformation)
          {
            nextPosition = pose.ToWorldPosition(particles[index + 1].Position * scale);
            p1.Size = particles[index].Size.Y * scale.Y;
          }
          else
          {
            nextPosition = particles[index + 1].Position;
            p1.Size = particles[index].Size.Y;
          }

          if (uniformNormal.HasValue)
          {
            // Uniform normal.
            normal = uniformNormal.Value;
          }
          else
          {
            // Varying normal.
            normal = particles[index].Normal;
            if (requiresTransformation)
              normal = pose.ToWorldDirection(normal);
          }

          Vector3F delta = nextPosition - p1.Position;
          Vector3F tangent = delta + previousDelta; // Note: Should we normalize vectors for better average?
          p1.Axis = Vector3F.Cross(normal, tangent);
          p1.Axis.TryNormalize();

          p1.Color = particles[index].Color * color;
          p1.Alpha = particles[index].Alpha * alpha;
          p1.AnimationTime = particles[index].AnimationTime;
          p1.BlendMode = particles[index].BlendMode;
          p1.TextureCoordinateU = GetTextureCoordinateU1(index - 1, numberOfSegments, particleSystemData.TextureTiling);

          // Draw ribbon segment.
          _billboardBatch.DrawRibbon(ref p0, ref p1, texture);

          p0 = p1;
          p0.TextureCoordinateU = GetTextureCoordinateU0(index, numberOfSegments, particleSystemData.TextureTiling);
          previousDelta = delta;
          index++;
        }

        // Last particle.
        p1.Position = nextPosition;

        if (uniformNormal.HasValue)
        {
          // Uniform normal.
          normal = uniformNormal.Value;
        }
        else
        {
          // Varying normal.
          normal = particles[index].Normal;
          if (requiresTransformation)
            normal = pose.ToWorldDirection(normal);
        }

        p1.Axis = Vector3F.Cross(normal, previousDelta);
        p1.Axis.TryNormalize();

        if (requiresTransformation)
          p1.Size = particles[index].Size.Y * scale.Y;
        else
          p1.Size = particles[index].Size.Y;

        p1.Color = particles[index].Color * color;
        p1.Alpha = particles[index].Alpha * alpha;
        p1.AnimationTime = particles[index].AnimationTime;
        p1.BlendMode = particles[index].BlendMode;
        p1.TextureCoordinateU = GetTextureCoordinateU1(index - 1, numberOfSegments, particleSystemData.TextureTiling);

        // Draw last ribbon segment.
        _billboardBatch.DrawRibbon(ref p0, ref p1, texture);
        index++;

      } while (index < numberOfParticles);
    }


    /// <summary>
    /// Gets the u texture coordinate at the start of a ribbon segment.
    /// </summary>
    /// <param name="i">The index of the segment in the current ribbon.</param>
    /// <param name="n">The number of segments in the current ribbon.</param>
    /// <param name="k">The tiling distance.</param>
    /// <returns>The u texture coordinate at the start of the ribbon segment.</returns>
    private static float GetTextureCoordinateU0(int i, int n, int k)
    {
      float texCoordU;
      if (k == 0)
      {
        // Texture is stretched along ribbon.
        texCoordU = (float)i / n;
      }
      else
      {
        // Texture repeats every k segments.
        texCoordU = (float)(i % k) / k;
      }

      return texCoordU;
    }


    /// <summary>
    /// Gets the u texture coordinate at the end of a ribbon segment.
    /// </summary>
    /// <param name="i">The index of the segment in the current ribbon.</param>
    /// <param name="n">The number of segments in the current ribbon.</param>
    /// <param name="k">The tiling distance.</param>
    /// <returns>The u texture coordinate at the end of the ribbon segment.</returns>
    private static float GetTextureCoordinateU1(int i, int n, int k)
    {
      float texCoordU;
      if (k == 0)
      {
        // Texture is stretched along ribbon.
        texCoordU = (float)(i + 1) / n;
      }
      else
      {
        // Texture repeats every k segments.
        texCoordU = (float)((i % k) + 1) / k;
      }

      return texCoordU;
    }
    #endregion
#endif

    #endregion
  }
}
