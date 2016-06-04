// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Stores the processed data for a <strong>Submesh</strong> asset.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class DRSubmeshContent
  {
    /// <summary>
    /// Gets or sets the vertex buffer associated with this submesh.
    /// </summary>
    /// <value>The vertex buffer associated with this submesh.</value>
    [ContentSerializer(ElementName = "VertexBuffer", SharedResource = true)]
    public VertexBufferContent VertexBuffer { get; set; }


    /// <summary>
    /// Gets or sets the index of the first vertex in the vertex buffer that belongs to this submesh
    /// (a.k.a base vertex or vertex offset).
    /// </summary>
    /// <value>The index of the first vertex in the vertex buffer.</value>
    public int StartVertex { get; set; }


    /// <summary>
    /// Gets or sets the number of vertices used in this submesh.
    /// </summary>
    /// <value>The number of vertices used in this submesh.</value>
    public int VertexCount { get; set; }


    /// <summary>
    /// Gets or sets the index buffer associated with this submesh.
    /// </summary>
    /// <value>The index buffer associated with this submesh.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    [ContentSerializer(ElementName = "IndexBuffer", SharedResource = true)]
    public IndexCollection IndexBuffer { get; set; }


    /// <summary>
    /// Gets or sets the location in the index buffer at which to start reading vertices.
    /// </summary>
    /// <value>The location in the index buffer at which to start reading vertices.</value>
    public int StartIndex { get; set; }


    /// <summary>
    /// Gets or sets the number of primitives to render for this submesh.
    /// </summary>
    /// <value>The number of primitives in this submesh.</value>
    public int PrimitiveCount { get; set; }


    /// <summary>
    /// Gets or sets the morph targets associated with this submesh.
    /// </summary>
    /// <value>The morph targets. The default value is <see langword="null"/>.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    public List<DRMorphTargetContent> MorphTargets { get; set; }


    /// <summary>
    /// Gets or sets the external material of this submesh.
    /// </summary>
    /// <value>The external material of this submesh.</value>
    /// <remarks>
    /// The material is assigned to the submesh using the model description (XML file). The material
    /// is defined in an external material definition (XML file). Materials can be shared between
    /// models. If <see cref="ExternalMaterial"/> is set, the property <see cref="LocalMaterial"/>
    /// can be ignored.
    /// </remarks>
    public ExternalReference<DRMaterialContent> ExternalMaterial { get; set; }


    /// <summary>
    /// Gets or sets the local material of this submesh, which is used if the model description (XML
    /// file) is missing or incomplete.
    /// </summary>
    /// <value>
    /// The local material of this submesh, which is used if the model description (XML file) is 
    /// missing or incomplete.
    /// </value>
    /// <remarks>
    /// This property is only used, if <see cref="ExternalMaterial"/> is <see langword="null"/>.
    /// </remarks>
    public DRMaterialContent LocalMaterial { get; set; }


    /// <summary>
    /// Gets or sets a user-defined object.
    /// </summary>
    /// <value>A user-defined object.</value>
    [ContentSerializer(ElementName = "UserData", SharedResource = true)]
    public object UserData { get; set; }
  }
}
