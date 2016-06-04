// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Meshes
{
  /// <summary>
  /// Face data structure for a Doubly-Connected Edge List (DCEL).
  /// </summary>
  /// <remarks>
  /// A face is defined by 3 or more edges (see <see cref="Boundary"/>). It can also contain holes 
  /// which are not connected via edges to the boundary (see <see cref="Holes"/>).
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Tag = {Tag})")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class DcelFace
  {

    /// <summary>
    /// Gets or sets an edge of the outer boundary.
    /// </summary>
    /// <value>An edge of the outer boundary.</value>
    public DcelEdge Boundary { get; set; }


    /// <summary>
    /// Gets or sets a list with one edge of the boundary for each hole; <see langword="null"/> if
    /// there are no holes.
    /// </summary>
    /// <value>
    /// A list with one edge of the boundary for each hole; <see langword="null"/> if there are no
    /// holes.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public List<DcelEdge> Holes { get; set; }


    /// <summary>
    /// Gets the normal of a convex face. (Not normalized. For internal use only.)
    /// </summary>
    /// <value>The normal of a convex face. (Not normalized. For internal use only.)</value>
    internal Vector3F Normal
    {
      // TODO: Maybe we could cache the normal vectors and not recompute them.
      // However, the normals must be invalidated when the topology changes.
      get
      {
        // Get the first 3 vertices.
        DcelEdge edge = Boundary;
        Vector3F v0 = edge.Origin.Position;
        edge = edge.Next;
        Vector3F v1 = edge.Origin.Position;
        edge = edge.Next;
        Vector3F v2 = edge.Origin.Position;

        // Compute face normal. (Not normalized.)
        Vector3F normal = Vector3F.Cross(v1 - v0, v2 - v0);

        // normal can be zero or close to zero when
        //   - face is a degenerate triangle,
        //   - face is a convex polygon with two collinear edges.
        // In the seconds case we could continue searching for edges that are not collinear.

        return normal;
      }
    }


    /// <summary>
    /// Gets or sets an integer flag that can be used by algorithms that operate on the 
    /// <see cref="DcelMesh"/>.
    /// </summary>
    /// <value>
    /// An integer flag that can be used by algorithms that operate on a <see cref="DcelMesh"/>.
    /// </value>
    /// <remarks>
    /// This property can be used by algorithms that operate on the <see cref="DcelMesh"/>. 
    /// </remarks>
    public int Tag { get; set; }


    /// <summary>
    /// Gets or sets the internal tag (same as <see cref="Tag"/> but only for internal use).
    /// </summary>
    /// <value>The internal tag.</value>
    internal int InternalTag { get; set; }


    /// <summary>
    /// Gets or sets user-defined data.
    /// </summary>
    /// <value>
    /// User-defined data.
    /// </value>
    /// <remarks>
    /// This property can be used by algorithms that operate on the <see cref="DcelMesh"/> or
    /// to store data related to this mesh element.
    /// </remarks>
    public object UserData { get; set; }


    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="DcelFace"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="DcelFace"/> class.
    /// </summary>
    public DcelFace()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DcelFace"/> class from a given edge.
    /// </summary>
    /// <param name="boundary">An edge of the outer boundary.</param>
    public DcelFace(DcelEdge boundary)
    {
      Boundary = boundary;
    }
  }
}
