// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Stores processing data for an <strong>OccluderNode</strong>.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class DROccluderNodeContent : DRSceneNodeContent
  {
    /// <summary>
    /// Gets or sets the imported <see cref="MeshContent"/>.
    /// </summary>
    /// <value>The imported <see cref="MeshContent"/>.</value>
    [ContentSerializerIgnore]
    public MeshContent InputMesh { get; set; }  // Only relevant for processing.


    /// <summary>
    /// Gets or sets the occluder.
    /// </summary>
    /// <value>The occluder.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [ContentSerializer(ElementName = "Occluder", SharedResource = true)]
    public DROccluderContent Occluder { get; set; }
  }
}
