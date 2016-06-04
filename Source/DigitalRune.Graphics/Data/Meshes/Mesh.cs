// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.SceneGraph;
#if ANIMATION
using DigitalRune.Animation.Character;
#endif


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a mesh of a 3D model.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A mesh represents the geometry and materials of a 3D object that can be rendered. A mesh owns 
  /// a collection of <see cref="Materials"/> and is divided into <see cref="Submesh"/>es. Each 
  /// <see cref="Submesh"/> describes a batch of primitives (usually triangles) that use one 
  /// material and can be rendered with a single draw call.
  /// </para>
  /// <para>
  /// The mesh can be rendered by creating a <see cref="MeshNode"/> and adding it to a 3D scene.
  /// </para>
  /// <para>
  /// <strong>Morph Target Animation:</strong> Submeshes may include morph targets (see
  /// <see cref="Submesh.MorphTargets"/>). The extension method
  /// <see cref="MeshHelper.GetMorphTargetNames"/> can be used to get a list of all morph targets
  /// included in a mesh. The current <see cref="MeshNode.MorphWeights"/> are stored in the
  /// <see cref="MeshNode"/>.
  /// </para>
  /// <para>
  /// <strong>Skeletal Animation:</strong> The mesh may contain a <see cref="Skeleton"/>, which can
  /// be used to animate (deform) the mesh. The current <see cref="MeshNode.SkeletonPose"/> is
  /// stored in the <see cref="MeshNode"/>. The property <see cref="MeshNode.SkeletonPose"/> can be
  /// animated. A set of key frame animations can be stored in <see cref="Animations"/>.
  /// </para>
  /// <para>
  /// <strong>Bounding shape:</strong> The bounding shape of the mesh is usually created by the 
  /// content pipeline and stored in the <see cref="BoundingShape"/> property. It is not updated
  /// automatically when the vertex buffer changes. The user who changes the vertex buffer is 
  /// responsible for updating or replacing the shape stored in <see cref="BoundingShape"/>.
  /// If the mesh can be deformed on the GPU (e.g. using mesh skinning), then the bounding shape
  /// must be large enough to contain all possible deformations.
  /// </para>
  /// <para>
  /// The properties of the bounding shape can be changed at any time. But it is not allowed to 
  /// replace the bounding shape while the <see cref="Mesh"/> is in use, i.e. referenced by a 
  /// scene node.
  /// </para>
  /// <para>
  /// For example, if the bounding shape is a <see cref="SphereShape"/>, the radius of the sphere 
  /// can be changed at any time. But it is not allowed to replace the <see cref="SphereShape"/> 
  /// with a <see cref="BoxShape"/> as long as the mesh is used in a scene. Replacing the 
  /// bounding shape will not raise any exceptions, but the mesh may no longer be rendered 
  /// correctly.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> Meshes are currently <strong>not</strong> cloneable.
  /// </para>
  /// </remarks>
  /// <seealso cref="EffectBinding"/>
  /// <seealso cref="Material"/>
  /// <seealso cref="MeshNode"/>
  /// <seealso cref="Submesh"/>
  public class Mesh : INamedObject, IDisposable
  {
    // Note: Meshes are not cloneable because meshes/submeshes from one model usually
    // share vertex and index buffers. Therefore, it makes little sense to duplicate 
    // the shared buffers for clones. 

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private string[] _cachedMorphTargetNames;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the bounding shape of this mesh.
    /// </summary>
    /// <value>
    /// The bounding shape of this mesh. Must not be <see langword="null"/>.
    /// The default value is <see cref="Shape.Infinite"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The properties of the bounding shape can be changed at any time. But it is not allowed to 
    /// replace the bounding shape while the <see cref="Mesh"/> is in use, i.e. referenced by a 
    /// scene node.
    /// </para>
    /// <para>
    /// For example, if the bounding shape is a <see cref="SphereShape"/>, the radius of the sphere 
    /// can be changed at any time. But it is not allowed to replace the <see cref="SphereShape"/> 
    /// with a <see cref="BoxShape"/> as long as the mesh is used in a scene. Replacing the bounding
    /// shape will not raise any exceptions, but the mesh may no longer be rendered correctly.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
#if !PORTABLE && !NETFX_CORE
    [Category("Geometry")]
#endif
    public Shape BoundingShape
    {
      get { return _boundingShape; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        _boundingShape = value;
      }
    }
    private Shape _boundingShape;


    /// <summary>
    /// Gets a collection of materials associated with this mesh.
    /// </summary>
    /// <value>
    /// A collection of materials associated with this mesh.
    /// </value>
    /// <remarks>
    /// A <see cref="Material"/> defines the effect bindings per render pass.
    /// </remarks>
    /// <seealso cref="EffectBinding"/>
    /// <seealso cref="Material"/>
#if !PORTABLE && !NETFX_CORE
    [Category("Material")]
#endif
    public MaterialCollection Materials { get; private set; }


    /// <summary>
    /// Gets the collection of <see cref="Submesh"/>es that make up this mesh. Each submesh is 
    /// composed of a set of primitives that share the same material. 
    /// </summary>
    /// <value>The <see cref="Submesh"/>es that make up this mesh.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !PORTABLE && !NETFX_CORE
    [Category("Common")]
#endif
    public SubmeshCollection Submeshes { get; private set; }


    /// <summary>
    /// Gets or sets the name of this mesh.
    /// </summary>
    /// <value>The name of this mesh.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Common")]
#endif
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the occluder that is rendered into the occlusion buffer.
    /// </summary>
    /// <value>The occluder. The default value is <see langword="null"/>.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
#if !PORTABLE && !NETFX_CORE
    [Category("Misc")]
#endif
    public Occluder Occluder { get; set; }


#if ANIMATION
    /// <summary>
    /// Gets or sets the skeleton for mesh skinning.
    /// </summary>
    /// <value>The skeleton. Can be <see langword="null"/>.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Animation")]
#endif
    public Skeleton Skeleton { get; set; }


    /// <summary>
    /// Gets or sets the animations.
    /// </summary>
    /// <value>The animations. Can be <see langword="null"/> if there are no animations.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
#if !PORTABLE && !NETFX_CORE
    [Category("Animation")]
#endif
    public Dictionary<string, SkeletonKeyFrameAnimation> Animations { get; set; }
#endif


    /// <summary>
    /// Gets or sets a user-defined object.
    /// </summary>
    /// <value>A user-defined object.</value>
    /// <remarks>
    /// This property is intended for application-specific data and is not used by the mesh itself. 
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Misc")]
#endif
    public object UserData { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Mesh"/> class.
    /// </summary>
    public Mesh()
    {
      BoundingShape = Shape.Infinite;
      Materials = new MaterialCollection();
      Submeshes = new SubmeshCollection(this);
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="Mesh"/> class.
    /// </summary>
    /// <remarks>
    /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in 
    /// <see langword="true"/>, and then suppresses finalization of the instance.
    /// </remarks>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// Releases the unmanaged resources used by an instance of the <see cref="Mesh"/> class and
    /// optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> to release both managed and unmanaged resources; 
    /// <see langword="false"/> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Dispose managed resources.
        foreach (var submesh in Submeshes)
          submesh.Dispose();

        UserData.SafeDispose();
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Morph Targets -----

    internal void InvalidateMorphTargetNames()
    {
      _cachedMorphTargetNames = null;
    }


    internal string[] GetMorphTargetNames()
    {
      if (_cachedMorphTargetNames != null)
        return _cachedMorphTargetNames;

      // Get the names of all morph targets.
      var names = new List<string>();
      foreach (var submesh in Submeshes)
        if (submesh.MorphTargets != null)
          foreach (var morphTarget in submesh.MorphTargets)
            names.Add(morphTarget.Name);

      // Sort names in ascending order.
      names.Sort(String.CompareOrdinal);

      // Remove duplicates.
      for (int i = names.Count - 1; i > 0; i--)
        if (names[i] == names[i - 1])
          names.RemoveAt(i);

      _cachedMorphTargetNames = names.ToArray();
      return _cachedMorphTargetNames;
    }


    internal bool HasMorphTargets()
    {
      if (_cachedMorphTargetNames != null && _cachedMorphTargetNames.Length > 0)
        return true;

      foreach (var submesh in Submeshes)
        if (submesh.HasMorphTargets)
          return true;

      return false;
    }
    #endregion

    #endregion
  }
}
