// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a batch of geometry information to submit to the graphics device during rendering.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="Graphics.Mesh"/> has a collection of <see cref="Graphics.Mesh.Materials"/> and is
  /// subdivided into several submeshes. Each <see cref="Submesh"/> describes a batch of primitives
  /// that use the same vertex buffer and the same material, which means a submesh can be rendered
  /// with one draw call. 
  /// </para>
  /// <para>
  /// The submesh references a <see cref="VertexBuffer"/> and an <see cref="IndexBuffer"/>. These
  /// buffers are usually shared with other submeshes of the same model.
  /// </para>
  /// <para>
  /// The submesh uses a continuous part of the <see cref="VertexBuffer"/>, starting at
  /// <see cref="StartVertex"/> and containing <see cref="VertexCount"/> vertices. The submesh also
  /// uses a continuous part of the <see cref="IndexBuffer"/>, starting at <see cref="StartIndex"/>.
  /// <see cref="PrimitiveCount"/> defines the number of primitives (usually triangles) that belong
  /// to this submesh.
  /// </para>
  /// </remarks>
  /// <seealso cref="EffectBinding"/>
  /// <seealso cref="Material"/>
  /// <seealso cref="Mesh"/>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class Submesh : IDisposable
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>Temporary ID set during rendering.</summary>
    internal uint Id;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the mesh that own this submesh.
    /// </summary>
    /// <value>The mesh.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Common")]
#endif
    public Mesh Mesh { get; internal set; }


    /// <summary>
    /// Gets or sets the type of the primitive.
    /// </summary>
    /// <value>The type of the primitive. The default type is triangle list.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public PrimitiveType PrimitiveType { get; set; }


    /// <summary>
    /// Gets or sets the vertex buffer.
    /// </summary>
    /// <value>The vertex buffer.</value>
    /// <remarks>
    /// Vertex buffers and index buffers may be shared between meshes, submeshes, or morph targets.
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public VertexBuffer VertexBuffer
    {
      get { return (VertexBufferEx != null) ? VertexBufferEx.Resource : null; }
      set { VertexBufferEx = VertexBufferEx.From(value); }
    }


    /// <summary>
    /// Gets the <see cref="VertexBufferEx"/>.
    /// </summary>
    /// <value>The <see cref="VertexBufferEx"/>.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    internal VertexBufferEx VertexBufferEx { get; private set; }


    /// <summary>
    /// Gets or sets the index of the first vertex in the vertex buffer that belongs to this submesh
    /// (a.k.a base vertex or vertex offset).
    /// </summary>
    /// <value>The index of the first vertex in the vertex buffer.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public int StartVertex { get; set; }


    /// <summary>
    /// Gets or sets the number of vertices.
    /// </summary>
    /// <value>The number of vertices.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public int VertexCount { get; set; }


    /// <summary>
    /// Gets or sets the index buffer.
    /// </summary>
    /// <value>The index buffer.</value>
    /// <remarks>
    /// Vertex buffers and index buffers may be shared between meshes or submeshes.
    /// </remarks>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public IndexBuffer IndexBuffer { get; set; }


    /// <summary>
    /// Gets or sets the location in the index array at which to start reading vertices.
    /// </summary>
    /// <value>Location in the index array at which to start reading vertices.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public int StartIndex { get; set; }


    /// <summary>
    /// Gets or sets the number of primitives (usually the number of triangles).
    /// </summary>
    /// <value>The number of primitives.</value>
#if !PORTABLE && !NETFX_CORE
    [Category("Graphics")]
#endif
    public int PrimitiveCount { get; set; }


    /// <summary>
    /// Gets or sets or sets the index of the material.
    /// </summary>
    /// <value>
    /// The index of the material in the <see cref="Graphics.Mesh.Materials"/> collection.
    /// The default value is 0.
    /// </value>
    /// <remarks>
    /// A <see cref="Material"/> defines the effect bindings per render pass.
    /// </remarks>
    /// <seealso cref="EffectBinding"/>
    /// <seealso cref="Material"/>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is out of range.
    /// </exception>
#if !PORTABLE && !NETFX_CORE
    [Category("Material")]
#endif
    public int MaterialIndex
    {
      get { return _materialIndex; }
      set
      {
        if (value < 0 || (Mesh != null && value >= Mesh.Materials.Count))
        {
          string message = String.Format(CultureInfo.InvariantCulture, "The material index (value = {0}) is out of range.", value);
          throw new ArgumentOutOfRangeException("value", message);
        }

        _materialIndex = value;
      }
    }
    private int _materialIndex;


    /// <summary>
    /// Gets a value indicating whether this submesh has morph targets.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this submesh has morph targets; otherwise,
    /// <see langword="false"/>.
    /// </value>
#if !PORTABLE && !NETFX_CORE
    [Category("Animation")]
#endif
    internal bool HasMorphTargets
    {
      get { return _morphTargets != null && _morphTargets.Count > 0; }
    }


    /// <summary>
    /// Gets or sets the morph targets of the submesh.
    /// </summary>
    /// <value>
    /// The morph targets of the submesh. The default value is <see langword="null"/>.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// The specified <see cref="MorphTargetCollection"/> cannot be assigned to the 
    /// <see cref="Submesh"/> because it already belongs to another <see cref="Submesh"/> instance.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
#if !PORTABLE && !NETFX_CORE
    [Category("Animation")]
#endif
    public MorphTargetCollection MorphTargets
    {
      get { return _morphTargets; }
      set
      {
        if (_morphTargets == value)
          return;

        if (value != null && value.Submesh != null)
          throw new InvalidOperationException("Cannot assign MorphTargetCollection to Submesh. The MorphTargetCollection already belongs to another Submesh.");

        if (_morphTargets != null)
          _morphTargets.Submesh = null;

        _morphTargets = value;

        if (value != null)
          value.Submesh = this;

        InvalidateMorphTargetNames();
      }
    }
    private MorphTargetCollection _morphTargets;


    /// <summary>
    /// Gets or sets user-defined data.
    /// </summary>
    /// <value>User-defined data.</value>
    /// <remarks>
    /// This property is intended for application-specific data and is not used by the submesh 
    /// itself. 
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
    /// Initializes a new instance of the <see cref="Submesh"/> class.
    /// </summary>
    public Submesh()
    {
      PrimitiveType = PrimitiveType.TriangleList;
    }


    /// <summary>
    /// Releases all resources used by an instance of the <see cref="Submesh"/> class.
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
    /// Releases the unmanaged resources used by an instance of the <see cref="Submesh"/> class 
    /// and optionally releases the managed resources.
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
        VertexBuffer.SafeDispose();
        IndexBuffer.SafeDispose();

        if (MorphTargets != null)
          foreach (var morphTarget in MorphTargets)
            morphTarget.Dispose();

        UserData.SafeDispose();
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Clears the morph target names, which are cached by the <see cref="Mesh"/>.
    /// </summary>
    internal void InvalidateMorphTargetNames()
    {
      if (Mesh != null)
        Mesh.InvalidateMorphTargetNames();
    }
    #endregion
  }
}
