// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Meshes
{
  /// <summary>
  /// Vertex data structure for a Doubly-Connected Edge List (DCEL).
  /// </summary>
  [DebuggerDisplay("{GetType().Name,nq}(Position = {Position}, Tag = {Tag})")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class DcelVertex
  {

    /// <summary>
    /// Gets or sets the coordinates of the vertex.
    /// </summary>
    /// <value>The coordinates of the vertex.</value>
    public Vector3F Position { get; set; }


    /// <summary>
    /// Gets or sets an outgoing edge.
    /// </summary>
    /// <value>An outgoing edge.</value>
    public DcelEdge Edge { get; set; }


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
    /// This property can be used by algorithms that operate on the <see cref="DcelMesh"/> or to
    /// store data related to this mesh element.
    /// </remarks>
    public object UserData { get; set; }



    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="DcelVertex"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="DcelVertex"/> class.
    /// </summary>
    public DcelVertex ()
    {
    }
   

    /// <summary>
    /// Initializes a new instance of the <see cref="DcelVertex"/> class with a given position and
    /// edge.
    /// </summary>
    /// <param name="position">The position.</param>
    /// <param name="edge">The edge.</param>
    public DcelVertex(Vector3F position, DcelEdge edge)
    {
      Position = position;
      Edge = edge;
    }
  }
}
