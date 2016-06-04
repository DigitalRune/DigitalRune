// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a morph target (blend shape) of a submesh.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <i>Morph target animation</i> (or per-vertex animation, blend shape interpolation) can be used
  /// to deform a model. It is primarily used for facial animation.
  /// </para>
  /// <para>
  /// The <see cref="Submesh"/> defines the base shape (neutral expression). A
  /// <see cref="MorphTarget"/> defines a deformed version of the base shape. Morph target animation
  /// typically involves several morph targets where each morph target represents a key shape
  /// (expressions such as "smile", "closed eye", "raised eyebrow"). A weight, usually in the range
  /// [0, 1], is assigned to each morph target. These weights control the influence of the morph
  /// targets and are animated over time. When the model is rendered the vertices are interpolated
  /// between the submesh and the morph targets.
  /// </para>
  /// <para>
  /// The submesh and the corresponding morph targets need to have the same structure. That is, the
  /// order of the vertices in the <see cref="VertexBuffer"/> needs to match the order of the
  /// vertices in the submesh. The <see cref="VertexBuffer"/> of morph target stores vertices of
  /// type <see cref="VertexPositionNormal"/>. Vertex positions and normals are relative to the
  /// submesh (difference between key shape and base shape, "delta shape").
  /// </para>
  /// <para>
  /// The morph target weights of a specific mesh instance are stored in the <see cref="MeshNode"/>
  /// (see property <see cref="MeshNode.MorphWeights"/>). These weights are used during rendering.
  /// </para>
  /// </remarks>
  /// <seealso cref="MorphWeightCollection"/>
  /// <seealso cref="Submesh.MorphTargets"/>
  /// <seealso cref="MeshNode.MorphWeights"/>
  [DebuggerDisplay("{GetType().Name,nq}(Name = {Name})")]
  public class MorphTarget : IDisposable, INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the name of the morph target.
    /// </summary>
    /// <value>The name of the morph target.</value>
    public string Name { get; internal set; }


    /// <summary>
    /// Gets or sets the vertex buffer.
    /// </summary>
    /// <value>The vertex buffer.</value>
    /// <remarks>
    /// Vertex buffers and index buffers may be shared between meshes, submeshes, or morph targets.
    /// </remarks>
    public VertexBuffer VertexBuffer { get; set; }


    /// <summary>
    /// Gets or sets the index of the first vertex in the vertex buffer that belongs to this morph
    /// target (a.k.a base vertex or vertex offset).
    /// </summary>
    /// <value>The index of the first vertex in the vertex buffer.</value>
    public int StartVertex { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="MorphTarget"/> class. 
    /// </summary>
    internal MorphTarget()
    {
      // This internal constructor is called when loaded from an asset.
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="MorphTarget"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="name"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="name"/> is empty.
    /// </exception>
    public MorphTarget(string name)
    {
      if (name == null)
        throw new ArgumentNullException("name");
      if (name.Length == 0)
        throw new ArgumentException("The name must not be empty.", "name");

      Name = name;
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
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
