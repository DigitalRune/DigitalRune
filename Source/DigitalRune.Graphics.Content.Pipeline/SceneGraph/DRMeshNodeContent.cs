// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Stores processing data for a <strong>MeshNode</strong>.
  /// </summary>
  public class DRMeshNodeContent : DRSceneNodeContent
  {
    /// <summary>
    /// Gets or sets the imported <see cref="MeshContent"/>.
    /// </summary>
    /// <value>The imported <see cref="MeshContent"/>.</value>
    [ContentSerializerIgnore]
    public MeshContent InputMesh { get; set; }    // Only relevant for processing.


    /// <summary>
    /// Gets or sets the morph targets associated with the <see cref="InputMesh"/>.
    /// </summary>
    /// <value>The morph targets of the <see cref="InputMesh"/>.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    [ContentSerializerIgnore]
    public List<MeshContent> InputMorphTargets { get; set; } // Only relevant for processing.


    /// <summary>
    /// Gets or sets the mesh.
    /// </summary>
    /// <value>The mesh.</value>
    [ContentSerializer(ElementName = "Mesh", SharedResource = true)]
    public DRMeshContent Mesh { get; set; }
  }
}
