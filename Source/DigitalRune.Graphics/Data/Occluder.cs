// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.Rendering;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines an occluder represented as an indexed triangle mesh.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <i>Occlusion culling</i> is the process of determining which scene nodes are hidden from a
  /// certain viewpoint. This is achieved by testing the scene nodes against a set of occluders. An
  /// <i>occluder</i> is an object within a scene that obscures the view and prevents objects behind
  /// it from being seen. In theory, any opaque object within a scene acts as occluder. However,
  /// most scene nodes (e.g. meshes) are too complex to be considered as occluders during occlusion
  /// culling.
  /// </para>
  /// <para>
  /// <strong>Occlusion Proxies:</strong><br/>
  /// The class <see cref="Occluder"/> defines a triangle mesh that acts as an occluder for
  /// occlusion culling. An <see cref="Occluder"/> is usually a lightweight representations of more
  /// complex scene node. Example: An <see cref="Occluder"/> storing a simple quad (2 triangles) can
  /// be used to approximate a complex wall (mesh with many triangles).
  /// </para>
  /// <para>
  /// An occluder can be assigned to a <see cref="Mesh"/> (property <see cref="Mesh.Occluder"/>) or
  /// can be added directly to a scene using an <see cref="OccluderNode"/>. The
  /// <see cref="OccluderNode"/> is often attached as child node to the scene node that it
  /// represents. This ensures that the occluder is updated automatically when the parent scene node
  /// is moved.
  /// </para>
  /// <para>
  /// The occluder geometry needs to be conservative, i.e. the triangle mesh needs to fit inside the
  /// object that it represents. Otherwise it may prevent visible objects from being rendered.
  /// </para>
  /// </remarks>
  /// <seealso cref="Mesh.Occluder"/>
  /// <seealso cref="OccluderNode"/>
  /// <seealso cref="OcclusionBuffer"/>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class Occluder : ITriangleMesh
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the triangle vertices.
    /// </summary>
    /// <value>The triangle vertices.</value>
    internal Vector3F[] Vertices { get; set; }


    /// <summary>
    /// Gets the triangle indices.
    /// </summary>
    /// <value>The triangle indices.</value>
    internal ushort[] Indices { get; set; }


    /// <summary>
    /// Gets the bounding shape.
    /// </summary>
    /// <value>The bounding shape.</value>
    internal TriangleMeshShape Shape
    {
      get
      {
        // Only create bounding shape when needed. (The bounding shape is only used
        // by the OccluderNode. Meshes do not need the bounding shape.)
        if (_shape == null)
          _shape = new TriangleMeshShape(this);

        return _shape;
      }
    }
    private TriangleMeshShape _shape;


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    int ITriangleMesh.NumberOfTriangles
    {
      get { return Indices.Length / 3; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Occluder"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="Occluder"/> class. (CLS-compliant constructor.
    /// If possible use <see cref="Occluder(DigitalRune.Mathematics.Algebra.Vector3F[],ushort[])"/>)
    /// </summary>
    /// <param name="vertices">The vertex array.</param>
    /// <param name="indices">The index array.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vertices"/> or <paramref name="indices"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="vertices"/> or <paramref name="indices"/> is empty.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
    public Occluder(Vector3F[] vertices, int[] indices)
      : this(vertices, ToUInt16(indices))
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Occluder"/> class. (CLS-compliant constructor.
    /// If possible use <see cref="Occluder(DigitalRune.Mathematics.Algebra.Vector3F[],ushort[])"/>)
    /// </summary>
    /// <param name="vertices">The vertex array.</param>
    /// <param name="indices">The index array.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vertices"/> or <paramref name="indices"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="vertices"/> or <paramref name="indices"/> is empty.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
    public Occluder(Vector3F[] vertices, short[] indices)
      : this(vertices, ToUInt16(indices))
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Occluder"/> class. (Recommended constructor.)
    /// </summary>
    /// <param name="vertices">The vertex array.</param>
    /// <param name="indices">The index array.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vertices"/> or <paramref name="indices"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="vertices"/> or <paramref name="indices"/> is empty.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
    [CLSCompliant(false)]
    public Occluder(Vector3F[] vertices, ushort[] indices)
    {
      if (vertices == null)
        throw new ArgumentNullException("vertices");
      if (vertices.Length == 0)
        throw new ArgumentException("Vertex array must not be empty.", "vertices");
      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Length == 0)
        throw new ArgumentException("Index array must not be empty.", "indices");
      if (indices.Length % 3 != 0)
        throw new ArgumentException("Invalid number of triangle indices. Multiple of 3 expected.", "indices");

      Vertices = vertices;
      Indices = indices;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Only for CLS-compliance.
    private static ushort[] ToUInt16(int[] indices)
    {
      if (indices == null)
        return null;

      var array = new ushort[indices.Length];
      for (int i = 0; i < indices.Length; i++)
      {
        if (indices[i] > ushort.MaxValue)
          throw new ArgumentException("Index out of range. Occluder only supports 16-bit indices.", "indices");

        array[i] = (ushort)indices[i];
      }

      return array;
    }


    // Only for CLS-compliance.
    private static ushort[] ToUInt16(short[] indices)
    {
      if (indices == null)
        return null;

      var array = new ushort[indices.Length];
      for (int i = 0; i < indices.Length; i++)
        array[i] = (ushort)indices[i];

      return array;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    Triangle ITriangleMesh.GetTriangle(int index)
    {
      if (index < 0 || index >= Indices.Length / 3)
        throw new ArgumentOutOfRangeException("index", "The index is out of range.");

      index *= 3;

      return new Triangle(
        Vertices[Indices[index + 0]],
        Vertices[Indices[index + 1]],
        Vertices[Indices[index + 2]]);
    }
    #endregion
  }
}
