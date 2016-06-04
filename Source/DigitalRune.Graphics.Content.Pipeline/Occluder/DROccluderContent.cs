// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Stores the processed data for an <strong>Occluder</strong> asset.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class DROccluderContent : DRSceneNodeContent
  {
    /// <summary>
    /// Gets or sets the triangle vertices.
    /// </summary>
    /// <value>The triangle vertices.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public List<Vector3F> Vertices { get; set; }


    /// <summary>
    /// Gets or sets the triangle indices.
    /// </summary>
    /// <value>The triangle indices.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public List<int> Indices { get; set; }
  }
}
