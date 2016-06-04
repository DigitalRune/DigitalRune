// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using Microsoft.Xna.Framework.Content;
#if ANIMATION
using DigitalRune.Animation.Character;
#endif


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Stores the processed data for a <strong>Mesh</strong> asset.
  /// </summary>
  public class DRMeshContent : INamedObject
  {
    /// <summary>
    /// Gets or sets the bounding shape for this mesh.
    /// </summary>
    /// <value>The bounding shape for this mesh.</value>
    public Shape BoundingShape { get; set; }


    /// <summary>
    /// Gets or sets the submeshes associated with this mesh.
    /// </summary>
    /// <value>The submeshes associated with this mesh.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public List<DRSubmeshContent> Submeshes { get; set; }


    /// <summary>
    /// Gets or sets the mesh name.
    /// </summary>
    /// <value>The mesh name.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the occluder.
    /// </summary>
    /// <value>The occluder.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [ContentSerializer(ElementName = "Occluder", SharedResource = true)]
    public DROccluderContent Occluder { get; set; }


#if ANIMATION
    /// <summary>
    /// Gets or sets the skeleton.
    /// </summary>
    /// <value>The skeleton.</value>
    [ContentSerializer(ElementName = "Skeleton", SharedResource = true)]
    public Skeleton Skeleton { get; set; }


    /// <summary>
    /// Gets or sets the animations.
    /// </summary>
    /// <value>The animations. Can be <see langword="null"/> if there are no animations.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    [ContentSerializer(ElementName = "Animations", SharedResource = true)]
    public Dictionary<string, SkeletonKeyFrameAnimation> Animations { get; set; }
#endif


    /// <summary>
    /// Gets or sets a user-defined object.
    /// </summary>
    /// <value>User-defined object.</value>
    [ContentSerializer(ElementName = "UserData", SharedResource = true)]
    public object UserData { get; set; }
  }
}
