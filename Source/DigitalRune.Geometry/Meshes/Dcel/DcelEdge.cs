// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;


namespace DigitalRune.Geometry.Meshes
{
  /// <summary>
  /// Edge data structure for a Doubly-Connected Edge List (DCEL).
  /// </summary>
  [DebuggerDisplay("{GetType().Name,nq}(Origin = {Origin.Position}, Tag = {Tag})")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class DcelEdge
  {
    /// <summary>
    /// Gets or sets the vertex where the edge starts.
    /// </summary>
    /// <value>The vertex where the edge starts.</value>
    public DcelVertex Origin { get; set; }


    /// <summary>
    /// Gets or sets the twin edge.
    /// </summary>
    /// <value>The twin edge.</value>
    public DcelEdge Twin { get; set; }


    /// <summary>
    /// Gets or sets the face for which this edge is part of the boundary.
    /// </summary>
    /// <value>The face for which this edge is part of the boundary.</value>
    public DcelFace Face { get; set; }


    /// <summary>
    /// Gets or sets the next edge (for the same face).
    /// </summary>
    /// <value>The next edge (for the same face).</value>
    public DcelEdge Next { get; set; }


    /// <summary>
    /// Gets or sets the previous edge (for the same face).
    /// </summary>
    /// <value>The previous edge (for the same face).</value>
    public DcelEdge Previous { get; set; }


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


    /// <summary>
    /// Gets the length of this edge.
    /// </summary>
    /// <value>The length.</value>
    internal float Length
    {
      get { return (Origin.Position - Twin.Origin.Position).Length; }
    }
  }
}
