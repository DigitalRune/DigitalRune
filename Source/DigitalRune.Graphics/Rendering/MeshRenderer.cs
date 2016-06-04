// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Collections;
using DigitalRune.Geometry;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Renders <see cref="MeshNode"/>s using state-sorting and hardware instancing.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>Render Target and Viewport:</strong><br/>
  /// This renderer renders into the current render target and viewport of the graphics device.
  /// </para>
  /// </remarks>
  public class MeshRenderer : SceneNodeRenderer
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Defines a draw job.
    /// </summary>
    [DebuggerDisplay("Job(Material = {MaterialKey}, Submesh = {SubmeshKey}, Distance = {DistanceKey})")]
    private struct Job
    {
      /// <summary>The material key.</summary>
      public uint MaterialKey;

      /// <summary>The submesh key.</summary>
      public uint SubmeshKey;

      /// <summary>
      /// The 16-bit normalized distance.
      /// </summary>
      public uint DistanceKey;

      // The distance is stored as uint:
      // - Faster than floating-point comparisons.
      // - Variable precision: Sorting meshes by object center is just an approximation.
      //   A limited precision (currently 16 bit) will suffice. We have 16 bit left for
      //   more precision or additional search criteria.

      /// <summary>The submesh.</summary>
      public Submesh Submesh;

      /// <summary>The effect binding of the material instance.</summary>
      public EffectBinding MaterialInstanceBinding;

      // This field was only added to process MeshInstancingNodes. Would be nice to remove it somehow.
      public IMeshInstancingNode MeshInstancingNode;
    }


    // Sort by material, submesh, and distance.
    private class DefaultComparer : IComparer<Job>
    {
      public static readonly DefaultComparer Instance = new DefaultComparer();
      public int Compare(Job x, Job y)
      {
        if (x.MaterialKey < y.MaterialKey)
          return -1;
        if (x.MaterialKey > y.MaterialKey)
          return +1;
        if (x.SubmeshKey < y.SubmeshKey)
          return -1;
        if (x.SubmeshKey > y.SubmeshKey)
          return +1;
        if (x.DistanceKey < y.DistanceKey)
          return -1;
        if (x.DistanceKey > y.DistanceKey)
          return +1;

        return 0;
      }
    }


    // Sort by distance, material, and submesh.
    private class DistanceComparer : IComparer<Job>
    {
      public static readonly DistanceComparer Instance = new DistanceComparer();
      public int Compare(Job x, Job y)
      {
        if (x.DistanceKey < y.DistanceKey)
          return -1;
        if (x.DistanceKey > y.DistanceKey)
          return +1;
        if (x.MaterialKey < y.MaterialKey)
          return -1;
        if (x.MaterialKey > y.MaterialKey)
          return +1;
        if (x.SubmeshKey < y.SubmeshKey)
          return -1;
        if (x.SubmeshKey > y.SubmeshKey)
          return +1;

        return 0;
      }
    }


    private struct MorphIndexAndWeight
    {
      public int Index;
      public float Weight;
    }


    // Sort morph targets in descending order by absolute weight.
    private class MorphComparer : IComparer<MorphIndexAndWeight>
    {
      public static readonly MorphComparer Instance = new MorphComparer();
      public int Compare(MorphIndexAndWeight x, MorphIndexAndWeight y)
      {
        float weightX = Math.Abs(x.Weight);
        float weightY = Math.Abs(y.Weight);
        return weightY.CompareTo(weightX);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Counters used to create unique IDs.
    private uint _effectCount;
    private uint _vertexBufferCount;

    // Start with a reasonably large capacity to avoid frequent re-allocations.
    private readonly ArrayList<Job> _jobs = new ArrayList<Job>(64);

    private readonly List<EffectParameterBinding> _perPassBindings = new List<EffectParameterBinding>();

    // Morphing
    private readonly List<MorphIndexAndWeight> _morphIndicesAndWeights = new List<MorphIndexAndWeight>();
    private readonly VertexBufferBinding[] _morphingVertexBufferBinding = new VertexBufferBinding[6];

    // Static mesh instancing
    private VertexBufferBinding[] _vertexBuffers;

    // Dynamic mesh Instancing 
    private InstanceRenderBatch<InstanceData> _instanceRenderBatch;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets a value indicating whether hardware instancing is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if hardware instancing is enabled; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    /// When this property is <see langword="true"/>, the renderer will render mesh instances using 
    /// hardware instancing if the material supports instancing (see 
    /// <see cref="EffectTechniqueDescription.InstancingTechnique"/>) and the batch size is 
    /// sufficiently large.
    /// </remarks>
    public bool EnableInstancing { get; set; }


    /// <summary>
    /// Gets or sets the minimum batch size required to activate hardware instancing.
    /// </summary>
    /// <value>
    /// The minimum batch size for hardware instancing. The default value is 4, which means that 
    /// hardware instancing is used to render models with 4 or more visible instances.
    /// </value>
    public int InstancingThreshold { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="MeshRenderer"/> class.
    /// </summary>
    public MeshRenderer()
    {
      Order = 0;
      EnableInstancing = true;
      InstancingThreshold = 4;
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          // Dispose managed resources.
          if (_instanceRenderBatch != null)
            _instanceRenderBatch.Dispose();
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
      var meshNode = node as MeshNode;
      return meshNode != null && meshNode.IsPassSupported(context.RenderPassHash);
    }


    /// <inheritdoc/>
    public override void Render(IList<SceneNode> nodes, RenderContext context, RenderOrder order)
    {
      ThrowIfDisposed();

      if (nodes == null)
        throw new ArgumentNullException("nodes");
      if (context == null)
        throw new ArgumentNullException("context");

      context.ThrowIfCameraMissing();
      context.ThrowIfRenderPassMissing();
      Debug.Assert(_jobs.Count == 0, "Job list was not properly reset.");

      // Reset counters.
      _effectCount = 0;
      _vertexBufferCount = 0;

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
      // Get camera properties used to calculate the distance of the scene node to the camera.
      var cameraNode = context.CameraNode;
      Pose cameraPose = cameraNode.PoseWorld;
      Vector3F cameraPosition = cameraPose.Position;
      Vector3F lookDirection = -cameraPose.Orientation.GetColumn(2);
      bool backToFront = (order == RenderOrder.BackToFront);

      // Update SceneNode.LastFrame for all visible nodes.
      int frame = context.Frame;
      cameraNode.LastFrame = frame;

      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var meshNode = nodes[i] as MeshNode;
        if (meshNode == null)
          continue;

        // MeshNode is visible in current frame.
        meshNode.LastFrame = frame;

        context.SceneNode = meshNode;

        // Update material instances.
        bool hasRenderPass = false;
        foreach (var materialInstance in meshNode.MaterialInstances)
        {
          // Some meshes might not have an effect binding for each render pass.
          EffectBinding materialInstanceBinding;
          if (!materialInstance.TryGet(context.RenderPass, out materialInstanceBinding))
            continue;

          hasRenderPass = true;

          context.MaterialBinding = materialInstanceBinding.MaterialBinding;
          context.MaterialInstanceBinding = materialInstanceBinding;

          // Update all parameter bindings stored in the material instance.
          foreach (var binding in materialInstanceBinding.ParameterBindings)
            binding.Update(context);

          // Select technique for rendering.
          // (The technique binding must be called for each submesh because it can do 
          // "preshader" stuff, see the SkinnedEffectTechniqueBinding for example.)
          materialInstanceBinding.TechniqueBinding.Update(context);
        }

        context.SceneNode = null;
        context.MaterialBinding = null;
        context.MaterialInstanceBinding = null;

        if (!hasRenderPass)
          continue;

        // Determine distance to camera.
        Vector3F cameraToNode = meshNode.PoseWorld.Position - cameraPosition;
        float distance = Vector3F.Dot(cameraToNode, lookDirection);
        if (backToFront)
          distance = -distance;

        var meshInstancingNode = meshNode as IMeshInstancingNode;
        if (meshInstancingNode != null)
          meshInstancingNode.UpdateInstanceVertexBuffer(context.GraphicsService.GraphicsDevice);

        uint sceneNodeType = (uint)(meshNode.IsStatic ? 1 : 0);

        // Add draw job to list.
        foreach (var submesh in meshNode.Mesh.Submeshes)
        {
          if (submesh.VertexBufferEx == null || submesh.VertexCount <= 0)
            continue;

          EffectBinding materialInstanceBinding;
          var materialInstance = meshNode.MaterialInstances[submesh.MaterialIndex];
          if (materialInstance.TryGet(context.RenderPass, out materialInstanceBinding))
          {
            var job = new Job
            {
              MaterialKey = GetMaterialKey(materialInstanceBinding, sceneNodeType),
              SubmeshKey = GetSubmeshKey(submesh, meshInstancingNode != null),
              DistanceKey = GetDistanceKey(distance),
              Submesh = submesh,
              MaterialInstanceBinding = materialInstanceBinding,
              MeshInstancingNode = meshInstancingNode,
            };
            _jobs.Add(ref job);
          }
        }
      }

      // Sort draw jobs.
      switch (order)
      {
        case RenderOrder.Default:
          _jobs.Sort(DefaultComparer.Instance);
          break;
        case RenderOrder.FrontToBack:
        case RenderOrder.BackToFront:
          _jobs.Sort(DistanceComparer.Instance);
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
    /// <param name="sceneNodeType">Type of the scene node (1 = static, 0 = dynamic).</param>
    /// <returns>The material key.</returns>
    private uint GetMaterialKey(EffectBinding materialInstanceBinding, uint sceneNodeType)
    {
      Debug.Assert(
        materialInstanceBinding.MaterialBinding != null,
        "The specified binding is not a material instance binding.");

      // ------------------------------------------------------------------
      // |   effect ID   |   material ID   |  technique ID  |  is static  |
      // |    12 bit     |    12 bit       |   7 bit        |    1 bit    |
      // ------------------------------------------------------------------
      // Notes:
      // SceneNodeType is an per-instance parameter. 0 = dynamic, 1 = static.
      // Hardware instancing does not contain the scene node type in the 
      // instancing data. Therefore, we do not merge draw calls for static and dynamic
      // objects.

      uint effectId = GetEffectId(materialInstanceBinding.EffectEx);
      uint materialId = GetMaterialId(materialInstanceBinding);
      uint techniqueId = materialInstanceBinding.TechniqueBinding.Id;

      Debug.Assert(effectId <= 0xfff, "Max number of effect per render call exceeded.");
      Debug.Assert(materialId <= 0xfff, "Max number of materials per render call exceeded.");
      Debug.Assert(techniqueId <= 0x7f, "The EffectTechniqueBinding.Id must be between 0 and 127.");

      return (effectId & 0xfff) << 20
             | (materialId & 0xfff) << 8
             | (techniqueId & 0x7f) << 1
             | (sceneNodeType & 0x1);
    }


    /// <summary>
    /// Gets the submesh key for sorting draw jobs.
    /// </summary>
    /// <param name="submesh">The submesh.</param>
    /// <param name="isMeshInstancingNode">
    /// If set to <see langword="true" /> the submesh is used in an 
    /// <see cref="MeshInstancingNode{T}"/>.
    /// </param>
    /// <returns>The submesh key</returns>
    private uint GetSubmeshKey(Submesh submesh, bool isMeshInstancingNode)
    {
      Debug.Assert(submesh.VertexBufferEx != null, "VertexBufferEx must not be null.");

      var vertexBufferEx = submesh.VertexBufferEx;
      var vertexBuffer = vertexBufferEx.Resource;
      var vertexDeclaration = vertexBuffer.VertexDeclaration;

      // Vertex stride is max 255 and always a multiple of 4.
      // --> Use the top 6 bit of the vertex stride for sorting.
      Debug.Assert(vertexDeclaration.VertexStride <= 255, "Max vertex stride should be 255.");
      Debug.Assert(vertexDeclaration.VertexStride % 4 == 0, "Vertex stride should be multiple of 4.");

      // -------------------------------------------------------
      // |  vertex stride  |  vertex buffer ID  |  submesh ID  |
      // |   6 bit         |   13 bit           |   13 bit     |
      // -------------------------------------------------------

      uint vertexStride = (uint)vertexDeclaration.VertexStride;
      uint vertexBufferId = GetVertexBufferId(vertexBufferEx);

      if (isMeshInstancingNode)
      {
        // For each MeshInstancingNode, we have to call GraphicsDevice.SetVertexBuffers to 
        // set the instancing buffer. Therefore, we cannot merge VBs. 
        // We use a special SubmeshKey to identify indexed meshes.
        return uint.MaxValue;
      }

      uint submeshId = GetSubmeshId(submesh);

      Debug.Assert(vertexBufferId <= 0x1fff, "Max number of vertex buffers per render call exceeded.");
      Debug.Assert(submeshId <= 0x1fff, "Max number of submeshes per render call exceeded.");

      return (vertexStride & 0xfc) << 24
             | (vertexBufferId & 0x1fff) << 13
             | (submeshId & 0x1fff);
    }


    /// <summary>
    /// Gets the distance key for sorting draw jobs.
    /// </summary>
    /// <param name="distance">The distance.</param>
    /// <returns>The key for sorting by distance.</returns>
    private static uint GetDistanceKey(float distance)
    {
      // ------------------------------
      // |    unused    |   distance   |
      // |    16 bit    |    16 bit    |
      // ------------------------------

      return Numeric.GetSignificantBitsSigned(distance, 16);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IndexBuffer")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MeshInstancingNode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Submeshes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private void ProcessJobs(RenderContext context, RenderOrder order)
    {
      Effect currentEffect = null;
      EffectEx currentEffectEx = null;
      EffectBinding currentMaterialBinding = null;

      var savedRenderState = new RenderStateSnapshot(context.GraphicsService.GraphicsDevice);

      int index = 0;
      var jobs = _jobs.Array;
      int jobCount = _jobs.Count;
      while (index < jobCount)
      {
        // Restore the render state. (Scene node renderers are allowed to mess it up.)
        if (index > 0)
          savedRenderState.Restore();

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
          }
        }

        // Note: EffectTechniqueBinding only returns the EffectTechnique, but does 
        // not set it as the current technique.
        var techniqueBinding = materialInstanceBinding.TechniqueBinding;
        var technique = techniqueBinding.GetTechnique(currentEffect, context);

        // See if there is an associated technique that supports hardware instancing.
        var instancingTechnique = (EffectTechnique)null;
        var techniqueDescription = currentEffectEx.TechniqueDescriptions[technique];
        if (techniqueDescription != null)
          instancingTechnique = techniqueDescription.InstancingTechnique;

        var meshInstancingNode = jobs[index].MeshInstancingNode;
        if (meshInstancingNode != null)
        {
          // ----- Static mesh instancing

          if (jobs[index].Submesh.IndexBuffer == null)
            throw new NotSupportedException("MeshInstancingNode<T> cannot be used with Submeshes without an IndexBuffer.");

          // If there is no instancing technique, then the first technique must support the instancing.
          // (This could be the case for effects which are only used with instancing, e.g. grass!?)
          instancingTechnique = instancingTechnique ?? technique;

          currentEffect.CurrentTechnique = instancingTechnique;
          var passBinding = techniqueBinding.GetPassBinding(instancingTechnique, context);
          DrawInstanced(ref passBinding, context, index, meshInstancingNode);
          index++;
        }
        else if (EnableInstancing
                 && instancingTechnique != null
                 && jobs[index].Submesh.IndexBuffer != null)   // DrawInstancedPrimitive requires an IndexBuffer.
        {
          // ----- Dynamic mesh instancing

          // Render all submeshes that share the same effect/material and batch 
          // instances into a single draw call.
          int count = 0; // Number of submeshes to draw without instancing.
          while (index + count < jobCount 
                 && jobs[index + count].MaterialKey == materialKey
                 && jobs[index + count].MeshInstancingNode == null)
          {
            // Count instances with the same submesh.
            var currentSubmeshKey = jobs[index + count].SubmeshKey;
            int instanceCount = 1;
            while (index + count + instanceCount < jobCount
                   && jobs[index + count + instanceCount].SubmeshKey == currentSubmeshKey)
              instanceCount++;

            if (instanceCount >= InstancingThreshold)
            {
              // Use hardware instancing.
              if (count > 0)
              {
                // Before switching to hardware instancing, submit the non-instanced meshes.
                currentEffect.CurrentTechnique = technique;
                var passBinding = techniqueBinding.GetPassBinding(technique, context);
                Draw(ref passBinding, context, index, count, order);
                index += count;
              }

              // Submit the instanced meshes.
              {
                currentEffect.CurrentTechnique = instancingTechnique;
                var passBinding = techniqueBinding.GetPassBinding(instancingTechnique, context);
                DrawInstanced(ref passBinding, context, index, instanceCount);
                index += instanceCount;
                count = 0;
              }
            }
            else
            {
              // Count instances, submit them later without hardware instancing.
              count += instanceCount;
            }
          }

          if (count > 0)
          {
            currentEffect.CurrentTechnique = technique;
            var passBinding = techniqueBinding.GetPassBinding(technique, context);
            Draw(ref passBinding, context, index, count, order);
            index += count;
          }
        }
        else if (materialInstanceBinding.MorphWeights != null)
        {
          // ----- Morphing

          // Render all submeshes that share the same effect/material.
          currentEffect.CurrentTechnique = technique;
          var passBinding = techniqueBinding.GetPassBinding(technique, context);
          do
          {
            DrawMorphing(ref passBinding, context, index);
            index++;
          } while (index < jobCount && jobs[index].MaterialKey == materialKey);
        }
        else
        {
          // ----- No mesh instancing, no morphing

          // Render all submeshes that share the same effect/material.
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


    private void Draw(ref EffectPassBinding passBinding, RenderContext context, int index, int count, RenderOrder order)
    {
      var jobs = _jobs.Array;

      if (order == RenderOrder.Default)
      {
        foreach (var pass in passBinding)
        {
          for (int i = index; i < index + count; i++)
          {
            var submesh = jobs[i].Submesh;
            var materialInstanceBinding = jobs[i].MaterialInstanceBinding;

            // Update and apply local, per-instance and per-pass bindings.
            foreach (var binding in materialInstanceBinding.ParameterBindings)
            {
              if (binding.Description.Hint == EffectParameterHint.PerPass)
                binding.Update(context);

              binding.Apply(context);
            }

            // Reset IDs. (Only used during state sorting.)
            ResetVertexBufferId(submesh.VertexBufferEx);
            ResetSubmeshId(submesh);

            pass.Apply();
            submesh.Draw();
          }
        }
      }
      else
      {
        for (int i = index; i < index + count; i++)
        {
          var submesh = jobs[i].Submesh;
          var materialInstanceBinding = jobs[i].MaterialInstanceBinding;

          // Apply local and per-instance bindings.
          foreach (var binding in materialInstanceBinding.ParameterBindings)
          {
            if (binding.Description.Hint == EffectParameterHint.PerPass)
              _perPassBindings.Add(binding);  // Will be updated and applied later.
            else
              binding.Apply(context);
          }

          // Reset IDs. (Only used during state sorting.)
          ResetVertexBufferId(submesh.VertexBufferEx);
          ResetSubmeshId(submesh);

          foreach (var pass in passBinding)
          {
            // Update and apply per-pass bindings.
            foreach (var binding in _perPassBindings)
            {
              binding.Update(context);
              binding.Apply(context);
            }

            pass.Apply();
            submesh.Draw();
          }

          _perPassBindings.Clear();
        }
      }
    }


    private void DrawMorphing(ref EffectPassBinding passBinding, RenderContext context, int index)
    {
      var jobs = _jobs.Array;
      var submesh = jobs[index].Submesh;
      var materialInstanceBinding = jobs[index].MaterialInstanceBinding;

      Debug.Assert(submesh.HasMorphTargets, "Submesh with morph targets expected.");

      // Get morph target weights.
      var morphTargets = submesh.MorphTargets;
      int numberOfMorphTargets = morphTargets.Count;
      var morphWeights = materialInstanceBinding.MorphWeights;
      _morphIndicesAndWeights.Clear();
      for (int i = 0; i < numberOfMorphTargets; i++)
      {
        float weight;
        morphWeights.TryGetValue(morphTargets[i].Name, out weight);
        _morphIndicesAndWeights.Add(new MorphIndexAndWeight { Index = i, Weight = weight });
      }

      // Sort morph targets in descending order by weight.
      _morphIndicesAndWeights.Sort(MorphComparer.Instance);

      // Apply local and per-instance bindings.
      foreach (var binding in materialInstanceBinding.ParameterBindings)
      {
        var usage = binding.Description;
        if (usage.Hint == EffectParameterHint.PerPass)
        {
          _perPassBindings.Add(binding);  // Will be updated and applied later.
        }
        else
        {
          if (usage.Semantic == DefaultEffectParameterSemantics.MorphWeight)
          {
            // Morph weights stored as individual parameters:
            //   float MorphWeight0 : MORPHWEIGHT0;
            var morphWeightBinding = binding as ConstParameterBinding<float>;
            if (morphWeightBinding != null)
            {
              int morphIndex = usage.Index;
              float weight = (morphIndex < numberOfMorphTargets) ? _morphIndicesAndWeights[morphIndex].Weight : 0;
              morphWeightBinding.Value = weight;
            }
            else
            {
              // Morph weights are stored as parameter array:
              //  float MorphWeight[n] : MORPHWEIGHT;
              var morphWeightArrayBinding = binding as ConstParameterArrayBinding<float>;
              if (morphWeightArrayBinding != null)
              {
                float[] values = morphWeightArrayBinding.Values;
                int baseIndex = usage.Index;
                int count = Math.Min(values.Length, numberOfMorphTargets - baseIndex);
                int i;
                for (i = 0; i < count; i++)
                  values[i] = _morphIndicesAndWeights[baseIndex + i].Weight;

                // Set unused values to 0.
                Array.Clear(values, i, values.Length - i);
              }
            }
          }

          binding.Apply(context);
        }
      }

      // Reset IDs. (Only used during state sorting.)
      ResetVertexBufferId(submesh.VertexBufferEx);
      ResetSubmeshId(submesh);

      foreach (var pass in passBinding)
      {
        // Update and apply per-pass bindings.
        foreach (var binding in _perPassBindings)
        {
          binding.Update(context);
          binding.Apply(context);
        }

        pass.Apply();

        // Set vertex buffer bindings:
        // - Vertex stream 0: base submesh
        // - Vertex stream 1-6: morph targets
        var vertexBuffer = submesh.VertexBuffer;
        _morphingVertexBufferBinding[0] = new VertexBufferBinding(vertexBuffer, submesh.StartVertex);

        // If weight is 0 or morph target is missing, bind the morph target with
        // the highest priority. (Avoid binding different vertex buffers.)
        var defaultMorphTarget = morphTargets[_morphIndicesAndWeights[0].Index];

        int i;
        int count = (numberOfMorphTargets < 5) ? numberOfMorphTargets : 5;
        for (i = 0; i < count; i++)
        {
          var entry = _morphIndicesAndWeights[i];
          // ReSharper disable once CompareOfFloatsByEqualityOperator
          var morph = (entry.Weight == 0) ? defaultMorphTarget : morphTargets[entry.Index];
          _morphingVertexBufferBinding[i + 1] = new VertexBufferBinding(morph.VertexBuffer, morph.StartVertex);
        }

        for (; i < 5; i++)
          _morphingVertexBufferBinding[i + 1] = new VertexBufferBinding(defaultMorphTarget.VertexBuffer, defaultMorphTarget.StartVertex);

        var graphicsDevice = vertexBuffer.GraphicsDevice;
        graphicsDevice.SetVertexBuffers(_morphingVertexBufferBinding);

        var indexBuffer = submesh.IndexBuffer;
        if (indexBuffer == null)
        {
          graphicsDevice.DrawPrimitives(
            submesh.PrimitiveType,
            0,
            submesh.PrimitiveCount);
        }
        else
        {
          graphicsDevice.Indices = indexBuffer;
          graphicsDevice.DrawIndexedPrimitives(
            submesh.PrimitiveType,
            0,
            0,
            submesh.VertexCount,
            submesh.StartIndex,
            submesh.PrimitiveCount);
        }
      }

      _perPassBindings.Clear();
    }


    private void DrawInstanced(ref EffectPassBinding passBinding, RenderContext context, int index, IMeshInstancingNode meshNode)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      var jobs = _jobs.Array;
      var submesh = jobs[index].Submesh;
      var materialInstanceBinding = jobs[index].MaterialInstanceBinding;

      // Reset IDs. (Only used during state sorting.)
      ResetVertexBufferId(submesh.VertexBufferEx);
      ResetSubmeshId(submesh);

      if (_vertexBuffers == null)
        _vertexBuffers = new VertexBufferBinding[2];

      _vertexBuffers[0] = new VertexBufferBinding(submesh.VertexBuffer, submesh.StartVertex, 0);
      _vertexBuffers[1] = new VertexBufferBinding(meshNode.InstanceVertexBuffer, 0, 1);
      graphicsDevice.SetVertexBuffers(_vertexBuffers);
      graphicsDevice.Indices = submesh.IndexBuffer;

      foreach (var pass in passBinding)
      {
        // Update and apply local, per-instance and per-pass bindings.
        foreach (var binding in materialInstanceBinding.ParameterBindings)
        {
          if (binding.Description.Hint == EffectParameterHint.PerPass)
            binding.Update(context);

          binding.Apply(context);
        }

        pass.Apply();

        graphicsDevice.DrawInstancedPrimitives(
            submesh.PrimitiveType,
            0,
            0,
            submesh.VertexCount,
            submesh.StartIndex,
            submesh.PrimitiveCount,
            meshNode.InstanceVertexBuffer.VertexCount);
      }

      // Reset vertex buffer to remove second vertex stream.
      graphicsDevice.SetVertexBuffer(null);
    }

    /// <exception cref="GraphicsException">
    /// Mesh cannot be rendered using hardware instancing.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    private void DrawInstanced(ref EffectPassBinding passBinding, RenderContext context, int index, int count)
    {
      var jobs = _jobs.Array;
      var submesh = jobs[index].Submesh;

      // Reset IDs. (Only used during state sorting.)
      ResetVertexBufferId(submesh.VertexBufferEx);
      ResetSubmeshId(submesh);

      // Get instance transforms from effect parameter bindings.
      if (_instanceRenderBatch == null)
      {
        _instanceRenderBatch = new InstanceRenderBatch<InstanceData>(
          context.GraphicsService.GraphicsDevice,
          new InstanceData[1024 * 1]);
      }

      _instanceRenderBatch.SetSubmesh(context, submesh, passBinding, jobs[index].MaterialInstanceBinding.ParameterBindings);

      var instanceData = _instanceRenderBatch.Instances;

      for (int i = 0; i < count; i++)
      {
        int instanceIndex;
        _instanceRenderBatch.Submit(1, out instanceIndex);

        foreach (var binding in jobs[index + i].MaterialInstanceBinding.ParameterBindings)
        {
          if (binding.Description.Semantic == SceneEffectParameterSemantics.World)
          {
            var matrixBinding = binding as EffectParameterBinding<Matrix>;
            if (matrixBinding == null)
            {
              throw new GraphicsException(
                string.Format(CultureInfo.InvariantCulture,
                              "Cannot render mesh \"{0}\" using instancing. " +
                              "The effect parameter \"World\" must be of type \"Matrix\" (\"float4x4\" in HLSL).",
                              submesh.Mesh.Name));
            }
            var world = matrixBinding.Value;

            instanceData[instanceIndex].Register0.X = world.M11;
            instanceData[instanceIndex].Register0.Y = world.M21;
            instanceData[instanceIndex].Register0.Z = world.M31;
            instanceData[instanceIndex].Register0.W = world.M41;
            instanceData[instanceIndex].Register1.X = world.M12;
            instanceData[instanceIndex].Register1.Y = world.M22;
            instanceData[instanceIndex].Register1.Z = world.M32;
            instanceData[instanceIndex].Register1.W = world.M42;
            instanceData[instanceIndex].Register2.X = world.M13;
            instanceData[instanceIndex].Register2.Y = world.M23;
            instanceData[instanceIndex].Register2.Z = world.M33;
            instanceData[instanceIndex].Register2.W = world.M43;
          }
          else if (binding.Description.Semantic == DefaultEffectParameterSemantics.InstanceColor)
          {
            var vector3Binding = binding as EffectParameterBinding<Vector3>;
            if (vector3Binding == null)
            {
              throw new GraphicsException(
                string.Format(CultureInfo.InvariantCulture,
                              "Cannot render mesh \"{0}\" using instancing. " +
                              "The effect parameter \"InstanceColor\" must be of type \"Vector3\" (\"float3\" in HLSL).",
                              submesh.Mesh.Name));
            }

            var color = vector3Binding.Value;
            instanceData[instanceIndex].Register3.X = color.X;
            instanceData[instanceIndex].Register3.Y = color.Y;
            instanceData[instanceIndex].Register3.Z = color.Z;
          }
          else if (binding.Description.Semantic == DefaultEffectParameterSemantics.InstanceAlpha)
          {
            var floatBinding = binding as EffectParameterBinding<float>;
            if (floatBinding == null)
            {
              throw new GraphicsException(
                string.Format(CultureInfo.InvariantCulture,
                              "Cannot render mesh \"{0}\" using instancing. " +
                              "The effect parameter \"InstanceAlpha\" must be of type \"float\".",
                              submesh.Mesh.Name));
            }
            var alpha = floatBinding.Value;

            instanceData[instanceIndex].Register3.W = alpha;
          }
          else if (binding.Description.Semantic == SceneEffectParameterSemantics.SceneNodeType)
          {
            // Scene node type is set normally using per-instance parameter bindings.
            // GetMaterialKey includes the scene node type, so that instances with different
            // scene node type are not merged.
          }
          else
          {
            throw new GraphicsException(
              string.Format(CultureInfo.InvariantCulture,
                            "Cannot render mesh \"{0}\" using instancing. " +
                            "The parameter \"{1}\" has the sort hint PerInstance or Local." +
                            "Instancing only supports following PerInstance/Local parameters: World, InstanceColor, InstanceAlpha",
                            submesh.Mesh.Name, binding.Parameter.Name));
          }
        }
      }

      _instanceRenderBatch.Flush();
    }


    #region ----- Resource IDs -----

    // Each resource (effect, material, vertex buffer, submesh) gets a unique ID, 
    // which is used for state sorting. The IDs are assigned during BatchJobs() and 
    // reset during ProcessJobs().

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


    private uint GetVertexBufferId(VertexBufferEx vertexBufferEx)
    {
      if (vertexBufferEx.Id == 0)
      {
        _vertexBufferCount++;
        vertexBufferEx.Id = _vertexBufferCount;
      }

      return vertexBufferEx.Id;
    }


    private static void ResetVertexBufferId(VertexBufferEx vertexBufferEx)
    {
      vertexBufferEx.Id = 0;
      vertexBufferEx.SubmeshCount = 0;
    }


    private static uint GetSubmeshId(Submesh submesh)
    {
      if (submesh.Id == 0)
      {
        var vertexBufferEx = submesh.VertexBufferEx;
        vertexBufferEx.SubmeshCount++;
        submesh.Id = vertexBufferEx.SubmeshCount;
      }

      return submesh.Id;
    }


    private static void ResetSubmeshId(Submesh submesh)
    {
      submesh.Id = 0;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static void ValidateNodes(IList<SceneNode> nodes, RenderContext context)
    {
      for (int i = 0; i < nodes.Count; i++)
      {
        var node = nodes[i];
        var meshNode = node as MeshNode;
        if (meshNode != null)
        {
          foreach (var submesh in meshNode.Mesh.Submeshes)
          {
            if (submesh.Id != 0)
              throw new GraphicsException("Submesh ID has not been reset.");

            var vertexBufferEx = submesh.VertexBufferEx;
            if (vertexBufferEx == null)
              continue;

            if (vertexBufferEx.Id != 0)
              throw new GraphicsException("Vertex buffer ID has not been reset.");

            if (vertexBufferEx.SubmeshCount != 0)
              throw new GraphicsException("Submesh counter has not been reset.");

            var material = submesh.GetMaterial();
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
    }
    #endregion

    #endregion
  }
}
