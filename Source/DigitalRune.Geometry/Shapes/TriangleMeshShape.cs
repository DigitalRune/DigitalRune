// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
#if !PORTABLE
using System.ComponentModel;
#endif
#if PORTABLE || WINDOWS
using System.Dynamic;
#endif


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a shape defined by an <see cref="ITriangleMesh"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <strong>One-sided and two-sided meshes:</strong> Per default, the collision detection treats
  /// the triangle mesh as one-sided (<see cref="IsTwoSided"/> is <see langword="false"/>); that
  /// means, only the front side of a triangle is solid. If the collision detection is used in a
  /// physics simulation, then objects can pass through the back side of a triangle. 
  /// <see cref="IsTwoSided"/> can be set to <see langword="true"/> to treat the mesh as two-sided
  /// (double-sided). 
  /// </para>
  /// <para>
  /// <strong>Meshes are hollow:</strong> Further, meshes are not treated as solid volumes. For
  /// example if a triangle mesh represents a sphere and another object is inside the sphere but 
  /// does not touch any triangles, then no collision is reported. 
  /// </para>
  /// <para>
  /// <strong>Spatial Partitioning:</strong> A spatial partitioning method (see 
  /// <see cref="Partition"/> can be used to improve runtime performance if the <see cref="Mesh"/>
  /// consists of a lot of triangles. A spatial partition improves the collision detection speed at
  /// the cost of additional memory. If <see cref="Partition"/> is <see langword="null"/>, no
  /// spatial partitioning method is used (which is the default). If a spatial partitioning scheme
  /// should be used, the property <see cref="Partition"/> must be set to a 
  /// <see cref="ISpatialPartition{T}"/> instance. The items in the spatial partition will be the
  /// indices of the mesh triangles. The triangle mesh shape will automatically fill and update the
  /// spatial partition. Following example shows how a complex triangle mesh shape can be improved
  /// by using an AABB tree:
  /// <code lang="csharp">
  /// <![CDATA[
  /// myTriangleMeshShape.Partition = new AabbTree<int>();
  /// ]]>
  /// </code>
  /// </para>
  /// <para>
  /// <strong>Shape Features:</strong> If a <see cref="TriangleMeshShape"/> is involved in a 
  /// <see cref="Contact"/> the shape feature property (<see cref="Contact.FeatureA"/> and
  /// <see cref="Contact.FeatureB"/>) contains the index of the triangle that causes the 
  /// <see cref="Contact"/>.
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> A <see cref="TriangleMeshShape"/> can be cloned. The clone will 
  /// reference the same <see cref="ITriangleMesh"/> (shallow copy)!
  /// If a <see cref="Partition"/> is in use, the spatial partition will be cloned. 
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class TriangleMeshShape : Shape
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The cached local space AABB
    internal Aabb _aabbLocal = new Aabb(new Vector3F(float.NaN), new Vector3F(float.NaN));
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>An inner point.</value>
    /// <remarks>
    /// This method returns a random vertex of the triangle mesh.
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get
      {
        // Return any point. - InnerPoint is mostly used for MPR. But MPR will not be used for 
        // TriangleMeshes. So the exact result doesn't matter.
        if (_mesh.NumberOfTriangles == 0)
          return new Vector3F();

        // Take a triangle in the "middle".
        Triangle triangle = _mesh.GetTriangle(_mesh.NumberOfTriangles / 2);
        return triangle.Vertex1;
      }
    }


    /// <summary>
    /// Gets or sets the triangle mesh.
    /// </summary>
    /// <value>The triangle mesh.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
    public ITriangleMesh Mesh
    {
      get { return _mesh; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (_mesh != value)
        {
          _mesh = value;

          // Update AABB, partition and raise Changed event.
          Invalidate();
        }
      }
    }
    private ITriangleMesh _mesh;


    /// <summary>
    /// Gets or set the spatial partition used to improve the performance of geometric queries.
    /// </summary>
    /// <value>
    /// The spatial partition. Per default no spatial partition is used and this property is 
    /// <see langword="null"/>.
    /// </value>
    /// <remarks>
    /// The spatial partition stores only the indices of the <see cref="Mesh"/>'s triangles. This
    /// property can be set to <see langword="null"/> to remove a spatial partition. If a spatial
    /// partition is set, the triangle mesh shape will automatically fill and update the spatial
    /// partition.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public ISpatialPartition<int> Partition
    {
      get { return _partition; }
      set
      {
        if (_partition != value)
        {
          // Clear old partition.
          if (_partition != null)
          {
            _partition.GetAabbForItem = null;
            _partition.Clear();
          }

          // Set new partition.
          _partition = value;

          // Fill new partition.
          if (_partition != null)
          {
            _partition.GetAabbForItem = i => Mesh.GetTriangle(i).Aabb;
            _partition.Clear();
            int numberOfTriangles = Mesh.NumberOfTriangles;
            for (int i = 0; i < numberOfTriangles; i++)
              _partition.Add(i);
          }
        }
      }
    }
    private ISpatialPartition<int> _partition;


    /// <summary>
    /// Gets or sets a value indicating whether contact welding is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if contact welding is enabled; otherwise, <see langword="false"/>.
    /// The default is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Contact welding is a process that improves collision detection contacts at the edges of
    /// triangles. Additional information is stored with the shape to support welding. If the mesh
    /// is only used for <see cref="CollisionQueryType.Boolean"/> queries or for ray casting,
    /// contact welding is not needed and <see cref="EnableContactWelding"/> can be set to 
    /// <see langword="false"/>. Further, contact welding is not supported for two-sided meshes
    /// (see <see cref="IsTwoSided"/>).
    /// </remarks>
    public bool EnableContactWelding
    {
      get
      {
        return TriangleNeighbors != null;
      }
      set
      {
        if (value != EnableContactWelding)
        {
          if (!value)
          {
            TriangleNeighbors = null;
          }
          else
          {
            TriangleNeighbors = new List<int>(Mesh.NumberOfTriangles * 3);
            ComputeTriangleNeighbors();
          }
        }
      }
    }


    /// <summary>
    /// Gets or sets the list of triangle neighbors.
    /// </summary>
    /// <value>The triangle neighbors.</value>
    /// <remarks>
    /// This list contains 3 entries for each triangle - one entry per edge. Each entry is the index
    /// of the neighbor triangle. For example the entry TriangleNeighbors[i * 3 + 1] is the index of
    /// the triangle that is the neighbor of Mesh.GetTriangle(i). And these two triangles touch at
    /// the edge opposite vertex 1.
    /// </remarks>
    internal List<int> TriangleNeighbors { get; set; }


    // The former tree envelope property is not needed anymore. If the AABBs in the
    // spatial partition should have an additional margin, use a partition that supports
    // this or use a GetAabbForItem method that computes an increased AABB.
    // public float TreeEnvelope { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether the mesh is two-sided.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this mesh is two-sided; otherwise, <see langword="false"/>.
    /// The default is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Per default, the collision detection treats the triangle mesh as one-sided 
    /// (<see cref="IsTwoSided"/> is <see langword="false"/>); that means, only the front side of a
    /// triangle is solid. If the collision detection is used in a physics simulation, then objects
    /// can pass through the back side of a triangle. <see cref="IsTwoSided"/> can be set to 
    /// <see langword="true"/> to treat the mesh as two-sided (double-sided). 
    /// </remarks>
    public bool IsTwoSided { get; set; }


#if PORTABLE || WINDOWS
    /// <exclude/>
#if !PORTABLE
    [Browsable(false)]
#endif
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public /*dynamic*/ object Internals
    {
      // Make internals visible to assemblies that cannot be added with InternalsVisibleTo().
      get
      {
        // ----- PCL Profile136 does not support dynamic.
        //dynamic internals = new ExpandoObject();
        //internals.AabbLocal = _aabbLocal;
        //internals.TriangleNeighbors = TriangleNeighbors;
        //return internals;

        IDictionary<string, object> internals = new ExpandoObject();
        internals["AabbLocal"] = _aabbLocal;
        internals["TriangleNeighbors"] = TriangleNeighbors;
        return internals;
      }
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleMeshShape"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleMeshShape"/> class.
    /// </summary>
    /// <remarks>
    /// The shape is initialized with a new empty <see cref="TriangleMesh"/>.
    /// </remarks>
    public TriangleMeshShape() : this(new TriangleMesh(), false, null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleMeshShape"/> class from the given 
    /// triangle mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    public TriangleMeshShape(ITriangleMesh mesh) : this(mesh, false, null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleMeshShape"/> class from the given
    /// triangle mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="enableContactWelding"> 
    /// If set to <see langword="true"/> contact welding is enabled; otherwise, the shape will not
    /// use contact welding. See <see cref="EnableContactWelding"/> for more information.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    public TriangleMeshShape(ITriangleMesh mesh, bool enableContactWelding)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      _mesh = mesh;

      EnableContactWelding = enableContactWelding;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleMeshShape"/> class from the given
    /// triangle mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="enableContactWelding"> 
    /// If set to <see langword="true"/> contact welding is enabled; otherwise, the shape will not
    /// use contact welding. See <see cref="EnableContactWelding"/> for more information.
    /// </param>
    /// <param name="partition">
    /// The spatial partition (see <see cref="Partition"/>). Can be <see langword="null"/> if no 
    /// partition should be used.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    public TriangleMeshShape(ITriangleMesh mesh, bool enableContactWelding, ISpatialPartition<int> partition)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      _mesh = mesh;
      Partition = partition;

      EnableContactWelding = enableContactWelding;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

#if XNA || MONOGAME
    /// <summary>
    /// Sets the triangle mesh. (For use by the content pipeline only.)
    /// </summary>
    /// <param name="mesh">The triangle mesh.</param>
    internal void SetMesh(ITriangleMesh mesh)
    {
      _mesh = mesh;
    }


    /// <summary>
    /// Sets the spatial partition. (For use by the content pipeline only.)
    /// </summary>
    /// <param name="partition">The spatial partition.</param>
    /// <remarks>
    /// This method is used internally to directly set the spatial partition. The spatial partition
    /// might already be initialized and should not be invalidated.
    /// </remarks>
    internal void SetPartition(ISpatialPartition<int> partition)
    {
      if (partition != null)
      {
        _partition = partition;
        _partition.GetAabbForItem = i => Mesh.GetTriangle(i).Aabb;

        // ----- Validate spatial partition.
        // Some spatial partitions, such as the CompressedAabbTree, are pre-initialized when 
        // loaded via content pipeline. Other spatial partitions need to be initialized manually.
        int numberOfTriangles = _mesh.NumberOfTriangles;
        if (_partition.Count != numberOfTriangles)
        {
          // The partition is not initialized.
          _partition.Clear();
          for (int i = 0; i < numberOfTriangles; i++)
            _partition.Add(i);

          _partition.Update(false);
        }
        else
        {
          // The partition is already initialized.
          Debug.Assert(Enumerable.Range(0, numberOfTriangles).All(_partition.Contains), "Invalid partition. The pre-initialized partition does not contain the same elements as the TriangleMeshShape.");
        }
      }
    }
#endif


    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new TriangleMeshShape();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (TriangleMeshShape)sourceShape;
      _mesh = source.Mesh;

      if (source.Partition != null)
        Partition = source.Partition.Clone();

      if (source.TriangleNeighbors != null)
        TriangleNeighbors = new List<int>(source.TriangleNeighbors);
    }
    #endregion


    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      if (Numeric.IsNaN(_aabbLocal.Minimum.X))
      {
        // ----- Recompute local cached AABB if it is invalid.

        if (Partition == null)
        {
          // ----- No spatial partition.
          _aabbLocal = new Aabb();

          if (_mesh != null && _mesh.NumberOfTriangles > 0)
          {
            bool isFirst = true;

            // Get AABB that contains all triangles.
            for (int i = 0; i < _mesh.NumberOfTriangles; i++)
            {
              Triangle triangle = _mesh.GetTriangle(i);

              for (int j = 0; j < 3; j++)
              {
                Vector3F vertex = triangle[j];
                if (isFirst)
                {
                  isFirst = false;
                  _aabbLocal = new Aabb(vertex, vertex);
                }
                else
                {
                  _aabbLocal.Grow(vertex);
                }
              }
            }
          }
        }
        else
        {
          // ----- With spatial partition.
          // Use spatial partition to determine local AABB.
          _aabbLocal = Partition.Aabb;
        }
      }

      // Apply scale and pose to AABB.
      return _aabbLocal.GetAabb(scale, pose);
    }


    /// <overloads>
    /// <summary>
    /// Invalidates the triangle mesh or a part of it.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Invalidates the triangle mesh.
    /// </summary>
    /// <remarks>
    /// This method must be called if the data stored in <see cref="Mesh"/> is changed. This method
    /// updates the <see cref="Partition"/> and raises the <see cref="Shape.Changed"/> event by
    /// calling <see cref="Shape.OnChanged"/>. This method also invalidates the mesh topology -
    /// which means that contact welding information is recomputed if contact welding is enabled
    /// (see <see cref="EnableContactWelding"/>).
    /// </remarks>
    public void Invalidate()
    {
      Invalidate(-1, true);
    }


    /// <summary>
    /// Invalidates the triangle mesh.
    /// </summary>
    /// <param name="invalidateTopology">
    /// if set to <see langword="true"/> the mesh topology is invalidated.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method must be called if the position of a triangle stored in <see cref="Mesh"/> is
    /// changed. This method updates the <see cref="Partition"/> and raises the
    /// <see cref="Shape.Changed"/> event by calling <see cref="Shape.OnChanged"/>.
    /// </para>
    /// <para>
    /// If the mesh topology has changed, <paramref name="invalidateTopology"/> must be set to
    /// <see langword="true"/>. The topology has changed if triangle neighbor relationships have
    /// changed. If each triangle has the same neighbor triangles as before and only the vertices
    /// were moved, <paramref name="invalidateTopology"/> can be <see langword="false"/>.
    /// </para>
    /// </remarks>
    public void Invalidate(bool invalidateTopology)
    {
      Invalidate(-1, invalidateTopology);
    }


    /// <summary>
    /// Invalidates the whole triangle mesh or a single triangle.
    /// </summary>
    /// <param name="triangleIndex">
    /// Index of the triangle. Can be -1 to invalidate the whole mesh.
    /// </param>
    /// <param name="invalidateTopology">
    /// If set to <see langword="true"/> the mesh topology is invalidated.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method must be called if the position of a triangle stored in <see cref="Mesh"/> is
    /// changed. This method updates the <see cref="Partition"/> and raises the
    /// <see cref="Shape.Changed"/> event by calling <see cref="Shape.OnChanged"/>.
    /// </para>
    /// <para>
    /// If the mesh topology has changed, <paramref name="invalidateTopology"/> must be set to
    /// <see langword="true"/>. The topology has changed if triangle neighbor relationships have
    /// changed. If each triangle has the same neighbor triangles as before and only the vertices
    /// were moved, <paramref name="invalidateTopology"/> can be <see langword="false"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="triangleIndex"/> is out of range.
    /// </exception>
    public void Invalidate(int triangleIndex, bool invalidateTopology)
    {
      int numberOfTriangles = Mesh.NumberOfTriangles;
      if (triangleIndex >= numberOfTriangles)
        throw new ArgumentOutOfRangeException("triangleIndex");

      // Set cached AABB to "invalid".
      _aabbLocal = new Aabb(new Vector3F(float.NaN), new Vector3F(float.NaN));

      // Fill new spatial partition.
      if (_partition != null)
      {
        if (numberOfTriangles != _partition.Count)
        {
          // Triangle count has changed. Re-initialize partition content.
          _partition.Clear();
          for (int i = 0; i < numberOfTriangles; i++)
            _partition.Add(i);
        }
        else
        {
          // Same number of triangles - invalidate the triangle.
          if (triangleIndex >= 0)
            _partition.Invalidate(triangleIndex);
          else
            _partition.Invalidate();
        }
      }

      if (invalidateTopology)
        ComputeTriangleNeighbors();

      if (triangleIndex < 0)
      {
        OnChanged(ShapeChangedEventArgs.Empty);
      }
      else
      {
        var eventArgs = ShapeChangedEventArgs.Create(triangleIndex);
        OnChanged(eventArgs);
        eventArgs.Recycle();
      }
    }


    /// <overloads>
    /// <summary>
    /// Gets the volume of this shape.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Computes the enclosed volume of the mesh.
    /// </summary>
    /// <returns>The enclosed volume of the mesh.</returns>
    /// <remarks>
    /// This method assumes that the mesh is a closed mesh without holes.
    /// </remarks>
    public float GetVolume()
    {
      return Mesh.GetVolume();
    }


    /// <summary>
    /// Computes the enclosed volume of the mesh.
    /// </summary>
    /// <param name="relativeError">Not used.</param>
    /// <param name="iterationLimit">Not used.</param>
    /// <returns>The enclosed volume of the mesh.</returns>
    /// <remarks>
    /// This method assumes that the mesh is a closed mesh without holes.
    /// </remarks>
    public override float GetVolume(float relativeError, int iterationLimit)
    {
      return Mesh.GetVolume();
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    /// <remarks>
    /// A deep copy of the <see cref="Mesh"/> is returned.
    /// </remarks>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      var triangleMesh = Mesh as TriangleMesh;
      if (triangleMesh != null)
        return triangleMesh.Clone();

      // Return a copy of the mesh.
      TriangleMesh mesh = new TriangleMesh();
      int numberOfTriangles = _mesh.NumberOfTriangles;
      for (int i = 0; i < numberOfTriangles; i++)
      {
        Triangle triangle = _mesh.GetTriangle(i);
        mesh.Add(triangle, false);
      }
      mesh.WeldVertices();

      return mesh;
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "TriangleMeshShape {{ NumberOfTriangles = {0} }}", _mesh.NumberOfTriangles);
    }


    // Used in ComputeTriangleNeighbors().
    private struct EdgeEntry
    {
      public int TriangleIndex;
      public int EdgeIndex;
    }


    /// <summary>
    /// Computes the <see cref="TriangleNeighbors"/>.
    /// </summary>
    private void ComputeTriangleNeighbors()
    {
      if (TriangleNeighbors == null)
        return;

      int numberOfTriangles = Mesh.NumberOfTriangles;

      // Fill list with -1.
      TriangleNeighbors.Clear();
      TriangleNeighbors.Capacity = numberOfTriangles * 3;
      for (int i = 0; i < numberOfTriangles * 3; i++)
        TriangleNeighbors.Add(-1);

      TriangleMesh mesh = Mesh as TriangleMesh;
      if (mesh != null)
      {
        // We have a TriangleMesh.

        // We use a hash table to store edge info. The edge vertex indices are the key.
        // There is one entry for each edge. The first neighbor triangle will create the
        // edge entry. The second triangle will find the edge entry and fill in the 
        // TriangleNeighbors info.
        var hashTable = new Dictionary<Pair<int, int>, EdgeEntry>(3 * numberOfTriangles);
        for (int triangleIndex = 0; triangleIndex < numberOfTriangles; triangleIndex++)
        {
          for (int i = 0, j = 2; i < 3; j = i, i++) // j is always "1 vertex index behind" i.
          {
            // The index of this edge (0, 1, or 2).
            var edgeIndex = (i + 1) % 3;

            // Get indices of the edge's vertices.
            int index0 = mesh.Indices[triangleIndex * 3 + i];
            int index1 = mesh.Indices[triangleIndex * 3 + j];

            // Sort indices.
            if (index0 > index1)
              MathHelper.Swap(ref index0, ref index1);

            EdgeEntry edge;
            if (hashTable.TryGetValue(new Pair<int, int>(index0, index1), out edge))
            {
              // Found a neighbor triangle!
              TriangleNeighbors[triangleIndex * 3 + edgeIndex] = edge.TriangleIndex;
              TriangleNeighbors[edge.TriangleIndex * 3 + edge.EdgeIndex] = triangleIndex;
            }
            else
            {
              // This is the first entry hashcode.
              edge = new EdgeEntry
              {
                TriangleIndex = triangleIndex,
                EdgeIndex = edgeIndex,
              };
              hashTable.Add(new Pair<int, int>(index0, index1), edge);
            }
          }
        }
      }
      else
      {
        // We only have an ITriangleMesh.
        // Do same as above, but this time we do not have indices. Instead we use vertices
        // directly.
        // We can use Pair<T> instead of Pair<T, T> because Pair<Vector3F>. We cannot use
        // Pair<int> above; maybe because the hashcode of two ints is to weak and there are
        // a lot of collisions in the hashtable.

        var hashTable = new Dictionary<Pair<Vector3F>, EdgeEntry>(3 * numberOfTriangles);

        for (int triangleIndex = 0; triangleIndex < numberOfTriangles; triangleIndex++)
        {
          var triangle = Mesh.GetTriangle(triangleIndex);

          for (int i = 0, j = 2; i < 3; j = i, i++) // j is always "1 vertex index behind" i.
          {
            // The index of this edge (0, 1, or 2).
            var edgeIndex = (i + 1) % 3;

            // Get vertices of the edge's vertices.
            var vertex0 = triangle[i];
            var vertex1 = triangle[j];

            // Sort vertices.
            //if (ShouldSwap(vertex0, vertex1))
            //  MathHelper.Swap(ref vertex0, ref vertex1);

            EdgeEntry edge;
            if (hashTable.TryGetValue(new Pair<Vector3F>(vertex0, vertex1), out edge))
            {
              // Found a neighbor triangle!
              TriangleNeighbors[triangleIndex * 3 + edgeIndex] = edge.TriangleIndex;
              TriangleNeighbors[edge.TriangleIndex * 3 + edge.EdgeIndex] = triangleIndex;
            }
            else
            {
              // This is the first entry hashcode.
              edge = new EdgeEntry
              {
                TriangleIndex = triangleIndex,
                EdgeIndex = edgeIndex,
              };
              hashTable.Add(new Pair<Vector3F>(vertex0, vertex1), edge);
            }
          }
        }
      }
    }


    ///// <summary>
    ///// Returns <see langword="true"/> if vector v0 is greater than vector v1 using lexicographic
    ///// ordering.
    ///// </summary>
    //private static bool ShouldSwap(Vector3F v0, Vector3F v1)
    //{
    //  if (v0.X != v1.X)
    //    return v0.X > v1.X;
    //  if (v0.Y != v1.Y)
    //    return v0.Y > v1.Y;
    //  return v0.Z > v1.Z;
    //}
    #endregion
  }
}
