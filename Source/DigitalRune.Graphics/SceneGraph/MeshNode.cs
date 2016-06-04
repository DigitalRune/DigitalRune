// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
#if ANIMATION
using DigitalRune.Animation.Character;
#endif
using DigitalRune.Graphics.Effects;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents an instance of a mesh in a 3D scene.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="Graphics.Mesh"/> describes the geometry and materials of a 3D object. A 
  /// <see cref="MeshNode"/> is used to position a mesh in a 3D scene. The mesh node defines its 
  /// position and orientation. Multiple mesh nodes can reference the same mesh, hence it is 
  /// possible to render the same mesh multiple times in a scene.
  /// </para>
  /// <para>
  /// <strong>Materials:</strong> Each mesh has one or more materials (see property
  /// <see cref="Graphics.Mesh.Materials"/>). When a mesh node is created from a mesh, a new 
  /// material instance (see class <see cref="MaterialInstance"/>) is created for each material.
  /// Each mesh node can override certain material properties defined in the base mesh. See 
  /// <see cref="MaterialInstance"/> for more details.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> When the referenced mesh is changed, the mesh node can become
  /// invalid. Do not add or remove materials or submeshes to or from a mesh as long as the mesh is 
  /// referenced by any mesh nodes. When the affected mesh nodes are rendered they can cause 
  /// exceptions or undefined behavior.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> When a <see cref="MeshNode"/> is cloned the 
  /// <see cref="MaterialInstances"/> are cloned (deep copy). But the <see cref="Mesh"/> and the
  /// <see cref="Skeleton"/> are only copied by reference (shallow copy). The original and the 
  /// cloned mesh node will reference the same <see cref="Graphics.Mesh"/> and the same
  /// <see cref="Skeleton"/>.
  /// </para>
  /// </remarks>
  public class MeshNode : SceneNode, IOcclusionProxy
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Optimization: Store the hash values of all render passes for fast lookup.
    private int[] _passHashes;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a collection of <see cref="MaterialInstance"/>s associated with the mesh.
    /// </summary>
    /// <value>A collection of <see cref="MaterialInstance"/>s associated with the mesh.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Material")]
#endif
    public MaterialInstanceCollection MaterialInstances
    {
      get { return _materialInstances; }
    }
    private MaterialInstanceCollection _materialInstances;


    /// <summary>
    /// Gets or sets the mesh.
    /// </summary>
    /// <value>The mesh.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public Mesh Mesh
    {
      get { return _mesh; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        bool isLoadedFromContentPipeline = (_mesh == null);

        _mesh = value;

        if (!isLoadedFromContentPipeline)
        {
          _materialInstances = new MaterialInstanceCollection(_mesh.Materials);
          _passHashes = _materialInstances.PassHashes;
          SetHasAlpha();

          // Ensure that MorphWeights are set. (Required for correct rendering.)
          MorphWeights = value.HasMorphTargets() ? new MorphWeightCollection(value) : null;

          OnInitializeShape();

#if ANIMATION
          // Reset skeleton pose if it has become invalid.
          if (_skeletonPose != null && _skeletonPose.Skeleton != value.Skeleton)
            _skeletonPose = null;
#endif

          // Invalidate OccluderData.
          RenderData = null;
        }
        else
        {
          // The MeshNode is loaded via the content pipeline. At this point shared
          // resources (e.g. Materials) may be missing.
          // --> The MeshNode will be initialized in OnAssetLoaded().
          Debug.Assert(MaterialInstances == null);
          Debug.Assert(MorphWeights == null);
#if ANIMATION
          Debug.Assert(SkeletonPose == null);
#endif
          Debug.Assert(RenderData == null);
        }
      }
    }
    private Mesh _mesh;


    /// <summary>
    /// Gets or sets the weights of the morph targets.
    /// </summary>
    /// <value>
    /// The weights of the morph targets. The default value depends on whether the mesh includes
    /// morph targets. If the mesh includes morph targets an empty 
    /// <see cref="MorphWeightCollection"/> (all weights are 0) is set by default; otherwise, 
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// A <see cref="MorphWeightCollection"/> is required, if the <see cref="Mesh"/> includes morph
    /// targets; otherwise, rendering may fail.
    /// </para>
    /// <para>
    /// The <see cref="MeshNode"/> does not verify whether the <see cref="MorphWeights"/> is
    /// compatible with the <see cref="Mesh"/>. The <see cref="MorphWeights"/> may include other
    /// morph targets than the <see cref="Mesh"/>. In this case only the morph targets that match
    /// are applied to the mesh during morph target animation.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MorphWeights")]
#if !PORTABLE && !NETFX_CORE
    [Category("Animation")]
#endif
    public MorphWeightCollection MorphWeights
    {
      get { return _morphWeights; }
      set
      {
        if (_mesh.HasMorphTargets())
        {
          if (value == null)
            throw new GraphicsException("MorphWeights cannot be null because the mesh includes morph targets.");
        }
        else
        {
          if (value != null)
            throw new GraphicsException("MorphWeights must be null because the mesh does not include morph targets.");
        }

        _morphWeights = value;

        // Assign MorphWeights to EffectBindings. (Required for rendering.)
        int numberOfSubmeshes = _mesh.Submeshes.Count;
        for (int i = 0; i < numberOfSubmeshes; i++)
        {
          var submesh = _mesh.Submeshes[i];
          if (submesh.HasMorphTargets)
            foreach (var materialInstanceBinding in _materialInstances[submesh.MaterialIndex].EffectBindings)
              materialInstanceBinding.MorphWeights = value;
        }
      }
    }
    private MorphWeightCollection _morphWeights;


#if ANIMATION
    /// <summary>
    /// Gets or sets the skeleton pose for mesh skinning.
    /// </summary>
    /// <value>The skeleton pose. Can be <see langword="null"/>.</value>
    /// <exception cref="ArgumentException">
    /// The <see cref="Skeleton"/> of the <see cref="SkeletonPose"/> is different from the 
    /// <see cref="Skeleton"/> of the <see cref="Mesh"/>.
    /// </exception>
#if !PORTABLE && !NETFX_CORE
    [Category("Animation")]
#endif
    public SkeletonPose SkeletonPose
    {
      get { return _skeletonPose; }
      set
      {
        if (value != null && Mesh.Skeleton != null && value.Skeleton != Mesh.Skeleton)
          throw new ArgumentException("Invalid skeleton pose. The skeleton pose has a skeleton which is different from mesh's skeleton.");

        _skeletonPose = value;
      }
    }
    private SkeletonPose _skeletonPose;
#endif
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="MeshNode"/> class.
    /// </summary>
    internal MeshNode()
    {
      // This internal constructor is called when loaded from an asset.
      // The mesh (shared resource) will be set later by using a fix-up code 
      // defined in MeshNodeReader.
      // When all fix-ups are executed, OnAssetLoaded (see below) is called.

      IsRenderable = true;
      CastsShadows = true;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="MeshNode"/> class.
    /// </summary>
    /// <param name="mesh">The <see cref="Mesh"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>. 
    /// </exception>
    public MeshNode(Mesh mesh)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      IsRenderable = true;
      CastsShadows = true;

      _mesh = mesh;

      _materialInstances = new MaterialInstanceCollection(_mesh.Materials);
      _passHashes = _materialInstances.PassHashes;
      SetHasAlpha();

      // Ensure that MorphWeights are set. (Required for correct rendering.)
      if (mesh.HasMorphTargets())
        MorphWeights = new MorphWeightCollection(mesh);

      // We do not call OnInitializeShape here because virtual methods should not be called
      // in constructor.
      Shape = mesh.BoundingShape;
    }


    /// <summary>
    /// Called when all fix-ups are executed and the asset is fully loaded.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="eventArgs">
    /// The <see cref="EventArgs"/> instance containing the event data.
    /// </param>
    internal void OnAssetLoaded(object sender, EventArgs eventArgs)
    {
      // Meshes are shared resources, so they are loaded deferred.
      // OnAssetLoaded is called when all shared resources are loaded.
      _materialInstances = new MaterialInstanceCollection(Mesh.Materials);
      _passHashes = _materialInstances.PassHashes;
      SetHasAlpha();

      // Ensure that MorphWeights are set. (Required for correct rendering.)
      if (_mesh.HasMorphTargets())
        MorphWeights = new MorphWeightCollection(_mesh);

      OnInitializeShape();
      
      // Note: The SkeletonPose is created by the ModelNode when the asset is loaded.
    }


    /// <inheritdoc/>
    protected override void Dispose(bool disposing, bool disposeData)
    {
      if (!IsDisposed)
      {
        if (disposing)
        {
          if (disposeData && _mesh != null)
            _mesh.Dispose();

          // The SkeletonPose may be shared between MeshNodes and therefore cannot
          // be recycled automatically.
        }

        base.Dispose(disposing, disposeData);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc cref="SceneNode.Clone"/>
    public new MeshNode Clone()
    {
      return (MeshNode)base.Clone();
    }


    /// <inheritdoc/>
    protected override SceneNode CreateInstanceCore()
    {
      return new MeshNode();
    }


    /// <inheritdoc/>
    protected override void CloneCore(SceneNode source)
    {
      // Clone SceneNode properties.
      base.CloneCore(source);

      // Clone MeshNode properties.
      var sourceTyped = (MeshNode)source;
      _mesh = sourceTyped.Mesh;
      _materialInstances = new MaterialInstanceCollection(sourceTyped.MaterialInstances);
      _passHashes = _materialInstances.PassHashes;

      if (sourceTyped.MorphWeights != null)
        MorphWeights = sourceTyped.MorphWeights.Clone();

#if ANIMATION
      if (sourceTyped.SkeletonPose != null)
        _skeletonPose = sourceTyped.SkeletonPose.Clone();
#endif
    }
    #endregion


    /// <summary>
    /// Determines whether the mesh node supports the specified render pass.
    /// </summary>
    /// <param name="passHash">The hash value of the render pass.</param>
    /// <returns>
    /// <see langword="true"/> if the mesh contains a material with the specified render pass; 
    /// otherwise, <see langword="false"/> if the mesh does not support the specified render 
    /// pass.
    /// </returns>
    /// <remarks>
    /// The method is used only internally as an optimization. Only the hash value is checked, 
    /// therefore the method may return a false positive!
    /// </remarks>
    internal bool IsPassSupported(int passHash)
    {
      for (int i = 0; i < _passHashes.Length; i++)
        if (_passHashes[i] == passHash)
          return true;

      return false;
    }


    /// <summary>
    /// Checks if the MaterialInstances contain an InstanceAlpha parameter binding and stores the
    /// result in the SceneNode flags.
    /// </summary>
    private void SetHasAlpha()
    {
      foreach (var materialInstance in _materialInstances)
      {
        foreach (var effectBinding in materialInstance.EffectBindings)
        {
          foreach (var parameterBinding in effectBinding.ParameterBindings)
          {
            if (ReferenceEquals(parameterBinding.Description.Semantic, DefaultEffectParameterSemantics.InstanceAlpha)
                && parameterBinding is ConstParameterBinding<float>)
            {
              // Found an InstanceAlpha parameter binding.
              SetFlag(SceneNodeFlags.HasAlpha);
              return;
            }
          }
        }
      }

      // No suitable InstanceAlpha parameter found.
      ClearFlag(SceneNodeFlags.HasAlpha);
    }


    /// <summary>
    /// Sets the <see cref="SceneNode.Shape"/> automatically.
    /// </summary>
    internal virtual void OnInitializeShape()
    {
      Shape = Mesh.BoundingShape;
    }


    #region ----- IOcclusionProxy -----

    /// <inheritdoc/>
    bool IOcclusionProxy.HasOccluder
    {
      get { return Mesh.Occluder != null; }
    }


    /// <inheritdoc/>
    void IOcclusionProxy.UpdateOccluder()
    {
      Debug.Assert(((IOcclusionProxy)this).HasOccluder, "Check IOcclusionProxy.HasOccluder before calling UpdateOccluder().");

      // The occluder data is created when needed and cached in RenderData.
      var data = RenderData as OccluderData;
      if (data == null)
      {
        data = new OccluderData(Mesh.Occluder);
        RenderData = data;
        IsDirty = true;
      }

      if (IsDirty)
      {
        data.Update(Mesh.Occluder, PoseWorld, ScaleWorld);
        IsDirty = false;
      }
    }


    /// <inheritdoc/>
    OccluderData IOcclusionProxy.GetOccluder()
    {
      Debug.Assert(((IOcclusionProxy)this).HasOccluder, "Call IOcclusionProxy.HasOccluder before calling GetOccluder().");
      Debug.Assert(!IsDirty, "Call IOcclusionProxy.HasOccluder and UpdateOccluder() before calling GetOccluder().");

      return (OccluderData)RenderData;
    }
    #endregion

    #endregion
  }
}
