// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="DecalNode"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="DecalRenderer"/> is a scene node renderer that handles <see cref="DecalNode"/>s.
  /// Decals are rendered as <i>deferred decals</i> (<i>screen-space decals</i>). This means that 
  /// decal materials are projected onto the geometry buffer. Therefore, 
  /// <see cref="RenderContext.GBuffer0"/> and <see cref="RenderContext.GBuffer1"/> need to be set 
  /// in the render context.
  /// </para>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  public class DecalRenderer : SceneNodeRenderer
  {
    // Instancing:
    // Hardware instancing is not yet supported. Too much overhead in Direct3D 9.
    // Waiting for Direct3D 11 support:
    // - Store WorldView and WorldViewInverse transforms in constant buffer.
    // - In the vertex and pixel shader index the transforms using SV_InstanceID.
    // 
    // Notes:
    // Decals are sorted by DrawOrder and then by Material. In theory it is possible
    // to ignore the DrawOrder for decals that are not overlapping. This would improve
    // batching. However, the cost for determining whether decals overlap is probably 
    // too high.

    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    [DebuggerDisplay("Job({DrawOrder}, {MaterialKey}, Name = {Node.Name})")]
    private struct Job
    {
      /// <summary>The draw order.</summary>
      public int DrawOrder;

      /// <summary>The material key.</summary>
      public uint MaterialKey;

      /// <summary>The effect binding of the material instance.</summary>
      public EffectBinding MaterialInstanceBinding;

      public DecalNode DecalNode;
    }


    private class Comparer : IComparer<Job>
    {
      public static readonly IComparer<Job> Instance = new Comparer();
      public int Compare(Job x, Job y)
      {
        if (x.DrawOrder < y.DrawOrder)
          return -1;
        if (x.DrawOrder > y.DrawOrder)
          return +1;
        if (x.MaterialKey < y.MaterialKey)
          return -1;
        if (x.MaterialKey > y.MaterialKey)
          return +1;

        return 0;
      }
    }

    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private const int NumberOfVertices = 8;
    private const int NumberOfPrimitives = 12;  // Decal volume has 6 sides à 2 triangles

    // Rendering decals in "GBuffer" pass requires a custom blend state:
    //   Shader output = (decalNormal.X, decalNormal.Y, decalNormal.Z, decalAlpha)
    //   Render target = (normal.X, normal.Y, normal.Y, specularPower).
    private static readonly BlendState GBufferBlendState = 
      new BlendState
      {
        Name = "DecalRenderer.GBufferBlendState",

        // Normals (non-premultiplied alpha blending)
        ColorBlendFunction = BlendFunction.Add,
        ColorSourceBlend = Blend.SourceAlpha,
        ColorDestinationBlend = Blend.InverseSourceAlpha,

        // Specular power
        AlphaBlendFunction = BlendFunction.Add,
        AlphaSourceBlend = Blend.BlendFactor,
        AlphaDestinationBlend = Blend.InverseSourceAlpha,
        //BlendFactor = decalSpecularPower
      };

    // Counters used to create unique IDs.
    private uint _effectCount;

    private readonly ArrayList<Job> _jobs;

    private readonly VertexBuffer _vertexBuffer;
    private readonly IndexBuffer _indexBuffer;

    #region ----- Mesh Instancing -----
    // See MeshRenderer for reference.
    #endregion


    // For rendering using screen space quads:
    private readonly Vector3F[] _quadVertices = new Vector3F[4];
    private Aabb _cameraNearPlaneAabbWorld;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether decals may be clipped which intersect the 
    /// camera near plane. (Performance optimization)
    /// </summary>
    /// <value>
    /// <see langword="true"/> if whole decals are clipped when they intersect the camera 
    /// near plane; otherwise, <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// When this value is <see langword="true"/>, whole decals may "disappear" when the
    /// camera comes near the decal bounding box and the camera frustum near plane intersects the
    /// decal bounding box. This behavior is beneficial for performance and is usually not a 
    /// problem when the decal bounding box is thin. Also, in most games the camera will always
    /// keep some distance to other objects including decals. However, this property can be set
    /// to <see langword="false"/> to draw the decals in all cases with a small performance hit.
    /// </remarks>
    public bool ClipAtNearPlane { get; set; }


    ///// <summary>
    ///// Gets or sets a value indicating whether hardware instancing is enabled.
    ///// </summary>
    ///// <value>
    ///// <see langword="true"/> if hardware instancing is enabled; otherwise, 
    ///// <see langword="false"/>. The default value is <see langword="true"/>.
    ///// </value>
    ///// <remarks>
    ///// When this property is <see langword="true"/>, the renderer will render decals using hardware
    ///// instancing if the material supports instancing (see 
    ///// <see cref="EffectTechniqueDescription.InstancingTechnique"/>) and the batch size is 
    ///// sufficiently large.
    ///// </remarks>
    //public bool EnableInstancing { get; set; }


    ///// <summary>
    ///// Gets or sets the minimum batch size required to activate hardware instancing.
    ///// </summary>
    ///// <value>
    ///// The minimum batch size for hardware instancing. The default value is 4, which means that 
    ///// hardware instancing is used to render decals with 4 or more visible instances.
    ///// </value>
    //public int InstancingThreshold { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DecalRenderer"/> class.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    public DecalRenderer(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      Order = 1;
      ClipAtNearPlane = true;
      //EnableInstancing = true;
      //InstancingThreshold = 4;

      // Create decal volume: unit cube centered at (0, 0, -0.5).
      var vertices = new[]
      {
        new Vector3(-0.5f, -0.5f, 0.0f),  // 0: left, bottom, near
        new Vector3(-0.5f, 0.5f, 0.0f),   // 1: left, top, near
        new Vector3(0.5f, 0.5f, 0.0f),    // 2: right, top, near
        new Vector3(0.5f, -0.5f, 0.0f),   // 3: right, bottom, near
        new Vector3(-0.5f, -0.5f, -1.0f), // 4: left, bottom, far
        new Vector3(-0.5f, 0.5f, -1.0f),  // 5: left, top, far
        new Vector3(0.5f, 0.5f, -1.0f),   // 6: right, top, far
        new Vector3(0.5f, -0.5f, -1.0f)   // 7: right, bottom, far
      };
      var indices = new ushort[]
      {
        0, 1, 2, 0, 2, 3, // Front
        4, 5, 1, 4, 1, 0, // Left
        1, 5, 6, 1, 6, 2, // Top
        3, 2, 6, 3, 6, 7, // Right
        4, 0, 3, 4, 3, 7, // Bottom
        7, 6, 5, 7, 5, 4, // Back
      };
      _vertexBuffer = new VertexBuffer(graphicsService.GraphicsDevice, VertexPosition.VertexDeclaration, vertices.Length, BufferUsage.None);
      _vertexBuffer.SetData(vertices);
      _indexBuffer = new IndexBuffer(graphicsService.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
      _indexBuffer.SetData(indices);

      // Start with a reasonably large capacity to avoid frequent re-allocations.
      _jobs = new ArrayList<Job>(64);
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
          _vertexBuffer.Dispose();
          _indexBuffer.Dispose();
          // Dispose instancing buffers...
        }
      }

      base.Dispose(disposing);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override bool CanRender(SceneNode node, RenderContext context)
    {
      var decalNode = node as DecalNode;
      return decalNode != null && decalNode.IsPassSupported(context.RenderPassHash);
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      context.Validate(_vertexBuffer);
      context.ThrowIfCameraMissing();
      context.ThrowIfRenderPassMissing();
      Debug.Assert(_jobs.Count == 0, "Job list was not properly reset.");

      // Reset counters.
      _effectCount = 0;

      BatchJobs(nodes, context, order);
      if (_jobs.Count > 0)
      {
        ProcessJobs(context, order);
        _jobs.Clear();
      }

      if ((GlobalSettings.ValidationLevelInternal & GlobalSettings.ValidationLevelDevBasic) != 0)
        ValidateNodes(nodes, context);
    }


    private void BatchJobs(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      context.CameraNode.LastFrame = frame;

      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var decalNode = nodes[i] as DecalNode;
        if (decalNode == null)
          continue;

        // DecalNode is visible in current frame.
        decalNode.LastFrame = frame;

        context.SceneNode = decalNode;

        // Update material instance.
        EffectBinding materialInstanceBinding;
        if (!decalNode.MaterialInstance.TryGet(context.RenderPass, out materialInstanceBinding))
          continue;

        context.MaterialBinding = materialInstanceBinding.MaterialBinding;
        context.MaterialInstanceBinding = materialInstanceBinding;

        // Update all parameter bindings stored in the material instance.
        foreach (var binding in materialInstanceBinding.ParameterBindings)
          binding.Update(context);

        // Select technique for rendering.
        // (The technique binding must be called for each submesh because it can do 
        // "preshader" stuff, see the SkinnedEffectTechniqueBinding for example.)
        materialInstanceBinding.TechniqueBinding.Update(context);

        context.SceneNode = null;
        context.MaterialBinding = null;
        context.MaterialInstanceBinding = null;

        // Add draw job to list.
        var job = new Job
        {
          DrawOrder = decalNode.DrawOrder,
          MaterialKey = GetMaterialKey(materialInstanceBinding),
          MaterialInstanceBinding = materialInstanceBinding,
          DecalNode = decalNode,
        };
        _jobs.Add(ref job);
      }

      // Sort draw jobs.
      switch (order)
      {
        case RenderOrder.Default:
        case RenderOrder.FrontToBack: // Ignore
        case RenderOrder.BackToFront: // Ignore
          _jobs.Sort(Comparer.Instance);
          break;
        case RenderOrder.UserDefined:
          // Do nothing.
          break;
      }
    }


    /// <summary>
    /// Gets the material key for sorting draw jobs.
    /// </summary>
    /// <param name="materialInstanceBinding">The effect binding of a material instance.</param>
    /// <returns>The material key.</returns>
    private uint GetMaterialKey(EffectBinding materialInstanceBinding)
    {
      Debug.Assert(
        materialInstanceBinding.MaterialBinding != null,
        "The specified binding is not a material instance binding.");

      // ----------------------------------------------------
      // |   effect ID   |   material ID   |  technique ID  |
      // |    12 bit     |    12 bit       |   8 bit        |
      // ----------------------------------------------------

      uint effectId = GetEffectId(materialInstanceBinding.EffectEx);
      uint materialId = GetMaterialId(materialInstanceBinding);
      byte techniqueId = materialInstanceBinding.TechniqueBinding.Id;

      Debug.Assert(effectId <= 0xfff, "Max number of effect per render call exceeded.");
      Debug.Assert(materialId <= 0xfff, "Max number of materials per render call exceeded.");
      Debug.Assert(techniqueId <= 0xff, "Max number of techniques per render call exceeded.");

      return (effectId & 0xfff) << 20
             | (materialId & 0xfff) << 8
             | techniqueId;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    private void ProcessJobs(RenderContext context, RenderOrder order)
    {
      Effect currentEffect = null;
      EffectEx currentEffectEx = null;
      EffectBinding currentMaterialBinding = null;

      // Set render states for drawing decals.
      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      var savedRenderState = new RenderStateSnapshot(graphicsDevice);

      graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
      graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;

      if (!ClipAtNearPlane)
      {
        // Cache some info for near plane intersection tests.

        var cameraNode = context.CameraNode;
        var cameraPose = cameraNode.PoseWorld;
        var projection = cameraNode.Camera.Projection;

        // Get min and max of near plane AABB in view space.
        var min = new Vector3F(projection.Left, projection.Bottom, -projection.Near);
        var max = new Vector3F(projection.Right, projection.Top, -projection.Near);

        // Convert min and max to world space.
        min = cameraPose.ToWorldPosition(min);
        max = cameraPose.ToWorldPosition(max);

        // Get world space aabb
        _cameraNearPlaneAabbWorld = new Aabb(Vector3F.Min(min, max), Vector3F.Max(min, max));
      }

      // The BlendState is set below.
      bool isGBufferPass = string.Equals(context.RenderPass, "GBuffer", StringComparison.OrdinalIgnoreCase);  // InvariantCultureIgnoreCase would be better but is not available in WindowsStore.
      var blendState = isGBufferPass ? GBufferBlendState : BlendState.AlphaBlend;

      int index = 0;
      var jobs = _jobs.Array;
      int jobCount = _jobs.Count;
      while (index < jobCount)
      {
        // Update BlendState. (Needs to be done for each batch because decals can
        // change the blend mode in the material. For example, alpha-tested decals 
        // can disable alpha blending.)
        graphicsDevice.BlendState = blendState;

        uint materialKey = jobs[index].MaterialKey;
        var materialInstanceBinding = jobs[index].MaterialInstanceBinding;
        var materialBinding = materialInstanceBinding.MaterialBinding;
        var effectEx = materialBinding.EffectEx;

        Debug.Assert(effectEx != null, "EffectEx must not be null.");

        context.MaterialBinding = materialBinding;
        context.MaterialInstanceBinding = materialInstanceBinding;

        if (currentEffectEx != effectEx)
        {
          // ----- Next effect.
          currentEffectEx = effectEx;
          currentEffect = effectEx.Resource;

          // Reset ID. (Only used during state sorting.)
          ResetEffectId(effectEx);

          // Update and apply global bindings.
          foreach (var binding in currentEffectEx.ParameterBindings)
          {
            if (binding.Description.Hint == EffectParameterHint.Global)
            {
              binding.Update(context);
              binding.Apply(context);
            }
          }
        }

        if (currentMaterialBinding != materialBinding)
        {
          // ----- Next material.
          currentMaterialBinding = materialBinding;

          // Reset ID. (Only used during state sorting.)
          ResetMaterialId(materialBinding);

          // Update and apply material bindings.
          foreach (var binding in currentMaterialBinding.ParameterBindings)
          {
            binding.Update(context);
            binding.Apply(context);

            // In "GBuffer" pass the specular power is written to the alpha channel.
            // The specular power needs to be set as the BlendFactor. (See GBufferBlendState.)
            if (isGBufferPass && binding.Description.Semantic == DefaultEffectParameterSemantics.SpecularPower)
            {
              var specularPowerBinding = binding as EffectParameterBinding<float>;
              if (specularPowerBinding != null)
              {
                // Note: Specular power is currently encoded using log2 - see Deferred.fxh.
                // (Blending encoded values is mathematically not correct, but there are no
                // rules for blending specular powers anyway.)
                float specularPower = specularPowerBinding.Value;
                int encodedSpecularPower = (byte)((float)Math.Log(specularPower + 0.0001f, 2) / 17.6f * 255.0f);
                graphicsDevice.BlendFactor = new Color(255, 255, 255, encodedSpecularPower);
              }
            }
          }
        }

        // Note: EffectTechniqueBinding only returns the EffectTechnique, but does 
        // not set it as the current technique.
        var techniqueBinding = materialInstanceBinding.TechniqueBinding;
        var technique = techniqueBinding.GetTechnique(currentEffect, context);

        // See if there is an associated technique that supports hardware instancing.
        //var instancingTechnique = (EffectTechnique)null;
        //var techniqueDescription = currentEffectEx.TechniqueDescriptions[technique];
        //if (techniqueDescription != null)
        //  instancingTechnique = techniqueDescription.InstancingTechnique;

        //if (EnableInstancing && instancingTechnique != null)
        //{
        //  // ----- Instancing
        //  // Render all decals that share the same effect/material and batch instances 
        //  // into a single draw call.
        //  int count = 1;
        //  while (index + count < jobCount && jobs[index + count].MaterialKey == materialKey)
        //    count++;

        //  if (count >= InstancingThreshold)
        //  {
        //    // Draw decals using instancing.
        //    currentEffect.CurrentTechnique = instancingTechnique;
        //    var passBinding = techniqueBinding.GetPassBinding(instancingTechnique, context);
        //    DrawInstanced(ref passBinding, context, index, count);
        //    index += count;
        //  }
        //  else
        //  {
        //    // Draw decals without instancing.
        //    currentEffect.CurrentTechnique = technique;
        //    var passBinding = techniqueBinding.GetPassBinding(technique, context);
        //    Draw(ref passBinding, context, index, count, order);
        //    index += count;
        //  }
        //}
        //else
        {
          // ----- No instancing

          // Render all decals that share the same effect/material.
          int count = 1;
          while (index + count < jobCount && jobs[index + count].MaterialKey == materialKey)
            count++;

          currentEffect.CurrentTechnique = technique;
          var passBinding = techniqueBinding.GetPassBinding(technique, context);
          Draw(ref passBinding, context, index, count, order);
          index += count;
        }
      }

      context.MaterialBinding = null;
      context.MaterialInstanceBinding = null;

      savedRenderState.Restore();
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    private void Draw(ref EffectPassBinding passBinding, RenderContext context, int index, int count, RenderOrder order)
    {
      var jobs = _jobs.Array;
      var cameraNode = context.CameraNode;
      var cameraPose = cameraNode.PoseWorld;

      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      // Flag: true if the box vertex/index buffers are not set in the graphics device.
      bool setBoxBuffers = true;

      foreach (var pass in passBinding)
      {
        for (int i = index; i < index + count; i++)
        {
          var materialInstanceBinding = jobs[i].MaterialInstanceBinding;
          var decalNode = jobs[i].DecalNode;
          var decalPose = decalNode.PoseWorld;

          // Update and apply local, per-instance and per-pass bindings.
          foreach (var binding in materialInstanceBinding.ParameterBindings)
          {
            if (binding.Description.Hint == EffectParameterHint.PerPass)
              binding.Update(context);

            binding.Apply(context);
          }

          pass.Apply();

          bool drawWithQuad = false;
          if (!ClipAtNearPlane)
          {
            // ----- Check if near plane intersects the decal box.

            // First make a simple AABB check in world space.
            if (GeometryHelper.HaveContact(_cameraNearPlaneAabbWorld, decalNode.Aabb))
            {
              // Make exact check of decal box against camera near plane AABB in camera space.
              var decalBoxExtent = new Vector3F(1, 1, 1);
              decalBoxExtent *= decalNode.ScaleLocal;
              var decalBoxCenter = new Vector3F(0, 0, -decalNode.ScaleLocal.Z / 2);

              // Get pose of decal box in view space.
              var decalBoxPose = new Pose(
                cameraPose.ToLocalPosition(decalPose.Position + decalPose.Orientation * decalBoxCenter),
                cameraPose.Orientation.Transposed * decalPose.Orientation);

              // Aabb of camera near plane in view space.
              var projection = cameraNode.Camera.Projection;
              var cameraNearPlaneAabb = new Aabb(
                new Vector3F(projection.Left, projection.Bottom, -projection.Near),
                new Vector3F(projection.Right, projection.Top, -projection.Near));

              drawWithQuad = GeometryHelper.HaveContact(cameraNearPlaneAabb, decalBoxExtent, decalBoxPose, true);
            }
          }

          if (!drawWithQuad)
          {
            // Draw a box primitive.

            if (setBoxBuffers)
            {
              graphicsDevice.SetVertexBuffer(_vertexBuffer);
              graphicsDevice.Indices = _indexBuffer;
              setBoxBuffers = false;
            }

#if MONOGAME
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NumberOfPrimitives);
#else
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, NumberOfVertices, 0, NumberOfPrimitives);
#endif
          }
          else
          {
            // Draw a quad at the near plane because the camera is inside the box. 
            // The quad vertices must be given decal space!

            var projection = cameraNode.Camera.Projection;
            Vector3F scale = decalNode.ScaleWorld;
            Pose cameraToDecalPose = decalPose.Inverse * cameraPose;

            Vector4F scissor = GraphicsHelper.GetBounds(cameraNode, decalNode);
            // Use a bias to avoid that this quad is clipped by the near plane.
            const float bias = 1.0001f;
            float left = InterpolationHelper.Lerp(projection.Left, projection.Right, scissor.X) * bias;
            float top = InterpolationHelper.Lerp(projection.Top, projection.Bottom, scissor.Y) * bias;
            float right = InterpolationHelper.Lerp(projection.Left, projection.Right, scissor.Z) * bias;
            float bottom = InterpolationHelper.Lerp(projection.Top, projection.Bottom, scissor.W) * bias;
            float z = -projection.Near * bias;
            _quadVertices[0] = cameraToDecalPose.ToWorldPosition(new Vector3F(left, top, z));
            _quadVertices[0].X /= scale.X;
            _quadVertices[0].Y /= scale.Y;
            _quadVertices[0].Z /= scale.Z;
            _quadVertices[1] = cameraToDecalPose.ToWorldPosition(new Vector3F(right, top, z));
            _quadVertices[1].X /= scale.X;
            _quadVertices[1].Y /= scale.Y;
            _quadVertices[1].Z /= scale.Z;
            _quadVertices[2] = cameraToDecalPose.ToWorldPosition(new Vector3F(left, bottom, z));
            _quadVertices[2].X /= scale.X;
            _quadVertices[2].Y /= scale.Y;
            _quadVertices[2].Z /= scale.Z;
            _quadVertices[3] = cameraToDecalPose.ToWorldPosition(new Vector3F(right, bottom, z));
            _quadVertices[3].X /= scale.X;
            _quadVertices[3].Y /= scale.Y;
            _quadVertices[3].Z /= scale.Z;

            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, _quadVertices, 0, 2, VertexPosition.VertexDeclaration);

            // Remember that the device vertex/index buffers are not set anymore.
            setBoxBuffers = true;
          }
        }
      }
    }


    //private void DrawInstanced(ref EffectPassBinding passBinding, RenderContext context, int index, int count)
    //{
    //  See MeshRenderer.DrawInstanced() for reference.
    //}


    #region ----- Resource IDs -----

    // Each resource (effect, material) gets a unique ID, which is used for state 
    // sorting. The IDs are assigned during BatchJobs() and reset during ProcessJobs().

    private uint GetEffectId(EffectEx effectEx)
    {
      if (effectEx.Id == 0)
      {
        _effectCount++;
        effectEx.Id = _effectCount;
      }

      return effectEx.Id;
    }


    private static void ResetEffectId(EffectEx effectEx)
    {
      effectEx.Id = 0;
      effectEx.BindingCount = 0;
    }


    private static uint GetMaterialId(EffectBinding materialInstanceBinding)
    {
      Debug.Assert(
        materialInstanceBinding.MaterialBinding != null,
        "The specified binding is not a material instance binding.");

      var materialBinding = materialInstanceBinding.MaterialBinding;
      if (materialBinding.Id == 0)
      {
        var effectEx = materialBinding.EffectEx;
        effectEx.BindingCount++;
        materialBinding.Id = effectEx.BindingCount;
      }

      return materialBinding.Id;
    }


    private static void ResetMaterialId(EffectBinding materialBinding)
    {
      Debug.Assert(
        materialBinding.MaterialBinding == null,
        "The specified binding needs to be a material binding, not a material instance binding.");

      materialBinding.Id = 0;
    }


    private static void ValidateNodes(IList<SceneNode> nodes, RenderContext context)
    {
      for (int i = 0; i < nodes.Count; i++)
      {
        var node = nodes[i];
        var decalNode = node as DecalNode;
        if (decalNode != null)
        {
          var material = decalNode.Material;
          EffectBinding materialBinding;
          if (material.TryGet(context.RenderPass, out materialBinding))
          {
            if (materialBinding.Id != 0)
              throw new GraphicsException("Material ID has not been reset.");

            if (materialBinding.EffectEx.Id != 0)
              throw new GraphicsException("Effect ID has not been reset.");

            if (materialBinding.EffectEx.BindingCount != 0)
              throw new GraphicsException("Effect binding counter has not been reset.");
          }
        }
      }
    }
    #endregion

    #endregion
  }
}
