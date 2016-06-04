// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Stores the processed data for a <strong>MorphTarget</strong>.
  /// </summary>
  public class DRMorphTargetContent : INamedObject
  {
    /// <summary>
    /// Gets or sets the name of the morph target.
    /// </summary>
    /// <value>The name of the morph target.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the vertex buffer associated with this morph target.
    /// </summary>
    /// <value>The vertex buffer associated with this morph target.</value>
    [ContentSerializer(ElementName = "VertexBuffer", SharedResource = true)]
    public VertexBufferContent VertexBuffer { get; set; }


    /// <summary>
    /// Gets or sets the index of the first vertex in the vertex buffer that belongs to this morph
    /// target (a.k.a base vertex or vertex offset).
    /// </summary>
    /// <value>The index of the first vertex in the vertex buffer.</value>
    public int StartVertex { get; set; }
  }
}
