// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;

#if XNA || MONOGAME
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endif


namespace DigitalRune.Geometry.Meshes
{
  /// <summary>
  /// Provides a simple <see cref="ITriangleMesh"/> implementation using vertex and index lists.
  /// </summary>
  /// <remarks>
  /// <para>
  /// The triangles are defined using a vertex list (see <see cref="Vertices"/>) and an index list
  /// (see <see cref="Indices"/>). (<see cref="NumberOfTriangles"/> is <c>Indices.Count / 3</c>. It
  /// is allowed to manipulate the vertex and index lists directly, e.g. a triangle can be added by
  /// adding 3 indices to <see cref="Indices"/>. 
  /// </para>
  /// <para>
  /// <strong>Vertex welding:</strong> The <see cref="Add(Triangle, bool)"/> method and its
  /// overloads can be used to add a new triangles to the mesh. These methods can perform 
  /// brute-force vertex welding to remove duplicate vertices. The brute-force approach is fast if
  /// the mesh has only a small number of vertices.
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Building a simple triangle mesh using brute-force vertex welding.
  /// TriangleMesh mesh = new TriangleMesh();
  /// 
  /// // Add triangles:
  /// mesh.AddTriangle(new Triangle(v0, v1, v2), true);
  /// mesh.AddTriangle(new Triangle(v4, v1, v0), true);
  /// ...
  /// ]]>
  /// </code>
  /// </para>
  /// <para>
  /// But vertex welding using brute-force is slow if there is a large number of vertices. 
  /// Therefore, when constructing complex meshes the method <see cref="WeldVertices(float)"/> 
  /// should be used. <see cref="WeldVertices(float)"/> implements a fast vertex welding algorithm 
  /// that handles large meshes efficiently. (However, this algorithm is more resource intensive
  /// than the brute-force approach.)
  /// <code lang="csharp">
  /// <![CDATA[
  /// // Building a complex triangle mesh using intelligent vertex welding.
  /// TriangleMesh mesh = new TriangleMesh();
  /// 
  /// // Add triangles:
  /// mesh.AddTriangle(new Triangle(v0, v1, v2), false);
  /// mesh.AddTriangle(new Triangle(v4, v1, v0), false);
  /// ...
  /// 
  /// // After all vertices are added, remove duplicates.
  /// mesh.WeldVertices(0.001f);
  /// ]]>
  /// </code>
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public sealed class TriangleMesh : ITriangleMesh
  {
    // NOTE: TriangleMesh is sealed because it implements a non-virtual Clone().
    // We could split the Clone() method into a Clone() and a virtual CloneCore() 
    // method to allow inheritance.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the number of triangles.
    /// </summary>
    /// <value>The number of triangles.</value>
    public int NumberOfTriangles
    {
      get
      {
        if (Indices == null)
          return 0;

        return Indices.Count / 3;
      }
    }


    /// <summary>
    /// Gets or sets the vertices.
    /// </summary>
    /// <value>The vertices. The default value is an empty list.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public List<Vector3F> Vertices { get; set; }


    /// <summary>
    /// Gets or sets the indices.
    /// </summary>
    /// <value>The indices. The default value is an empty list.</value>
    /// <remarks>
    /// Always 3 indices define 1 triangle. Each index is the index of a vertex in 
    /// <see cref="Vertices"/>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Indices")]
    public List<int> Indices { get; set; }


    /// <summary>
    /// Gets or sets custom information.
    /// </summary>
    /// <value>Custom data defined by the user.</value>
    public object Tag { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleMesh"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleMesh"/> class.
    /// </summary>
    public TriangleMesh()
    {
      Vertices = new List<Vector3F>();
      Indices = new List<int>();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleMesh"/> class with the specified 
    /// initial capacity.
    /// </summary>
    /// <param name="verticesCapacity">
    /// The number of vertices that the new triangle mesh can initially store.
    /// </param>
    /// <param name="indicesCapacity">
    /// The number of indices that the new triangle mesh can initially store.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="verticesCapacity"/> or <paramref name="indicesCapacity"/> is less than 0.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
    public TriangleMesh(int verticesCapacity, int indicesCapacity)
    {
      Vertices = new List<Vector3F>(verticesCapacity);
      Indices = new List<int>(indicesCapacity);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <summary>
    /// Creates a new <see cref="TriangleMesh"/> that is a copy of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="TriangleMesh"/> that is a copy of the current instance.
    /// </returns>
    public TriangleMesh Clone()
    {
      TriangleMesh clone = new TriangleMesh();

      // Clone indices.
      if (Indices != null)
        foreach (int index in Indices)
          clone.Indices.Add(index);

      // Clone vertices.
      if (Vertices != null)
        foreach (Vector3F vertex in Vertices)
          clone.Vertices.Add(vertex);

      clone.Tag = Tag;

      return clone;
    }
    #endregion


    /// <overloads>
    /// <summary>
    /// Adds triangles to the triangle mesh.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Adds the triangles of the specified mesh (without vertex welding).
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <remarks>
    /// This method does not perform vertex welding and does not remove degenerate triangles. 
    /// </remarks>
    public void Add(ITriangleMesh mesh)
    {
      Add(mesh, false);
    }


    /// <summary>
    /// Adds the triangles of the specified mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="weldVerticesBruteForce">
    /// If set to <see langword="true"/>, vertex welding is performed. A brute-force method is used
    /// for welding which can be very slow for large triangle meshes. For large meshes it is 
    /// recommended to call <see cref="WeldVertices()"/> after all triangles have been added.
    /// </param>
    /// <remarks>
    /// This method does not remove degenerate triangles. Use 
    /// <see cref="Add(Triangle, bool, float, bool)"/> if more control over vertex welding and if 
    /// removal of degenerate triangles is desired.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public void Add(ITriangleMesh mesh, bool weldVerticesBruteForce)
    {
      var triangleMesh = mesh as TriangleMesh;
      if (triangleMesh != null && !weldVerticesBruteForce)
      {
        // Special: mesh is TriangleMesh and no welding.

        if (triangleMesh.Vertices == null)
          return;
        if (triangleMesh.Indices == null)
          return;

        if (Vertices == null)
          Vertices = new List<Vector3F>(triangleMesh.Vertices.Count);

        int numberOfNewIndices = triangleMesh.Indices.Count;
        if (Indices == null)
          Indices = new List<int>(numberOfNewIndices);
        
        // Add new vertices.
        int oldNumberOfVertices = Vertices.Count;
        Vertices.AddRange(triangleMesh.Vertices);
        
        // Add new indices. Add offset to all indices.
        for (int i = 0; i < numberOfNewIndices; i++)
          Indices.Add(triangleMesh.Indices[i] + oldNumberOfVertices);
        
        return;
      }

      int numberOfTriangles = mesh.NumberOfTriangles;
      for (int i = 0; i < numberOfTriangles; i++)
        Add(mesh.GetTriangle(i), weldVerticesBruteForce);
    }


    /// <summary>
    /// Adds the triangle (without vertex welding).
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <remarks>
    /// This method does not perform vertex welding and does not remove degenerate triangles. 
    /// </remarks>
    public void Add(Triangle triangle)
    {
      Add(triangle, false, Numeric.EpsilonF, false);
    }


    /// <summary>
    /// Adds the triangle.
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="weldVerticesBruteForce">
    /// If set to <see langword="true"/>, vertex welding is performed. A brute-force method is used
    /// for welding which can be very slow for large triangle meshes. For large meshes it is 
    /// recommended to call <see cref="WeldVertices()"/> after all triangles have been added.
    /// </param>
    /// <remarks>
    /// This method does not remove degenerate triangles. Use 
    /// <see cref="Add(Triangle, bool, float, bool)"/> if more control over vertex welding and if 
    /// removal of degenerate triangles is desired.
    /// </remarks>
    public void Add(Triangle triangle, bool weldVerticesBruteForce)
    {
      Add(triangle, weldVerticesBruteForce, Numeric.EpsilonF, false);
    }


    /// <summary>
    /// Adds the triangle.
    /// </summary>
    /// <param name="triangle">The triangle.</param>
    /// <param name="weldVerticesBruteForce">
    /// If set to <see langword="true"/>, vertex welding is performed. A brute-force method is used
    /// for welding which can be very slow for large triangle meshes. For large meshes it is 
    /// recommended to call <see cref="WeldVertices()"/> after all triangles have been added.
    /// </param>
    /// <param name="vertexPositionTolerance">
    /// The vertex position tolerance. If the vertex positions are within this range, they are 
    /// treated as identical vertices. This value is used for vertex welding and to decide if a
    /// triangle is degenerate. 
    /// </param>
    /// <param name="removeDegenerateTriangles">
    /// If set to <see langword="true"/> degenerate triangles will not be added to the mesh.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="vertexPositionTolerance"/> is negative or 0.
    /// </exception>
    public void Add(Triangle triangle, bool weldVerticesBruteForce, float vertexPositionTolerance, bool removeDegenerateTriangles)
    {
      if (vertexPositionTolerance <= 0)
        throw new ArgumentOutOfRangeException("vertexPositionTolerance", "vertexPositionTolerance must be greater than 0.");

      // If desired, remove degenerate triangles.
      if (removeDegenerateTriangles
          && (Vector3F.AreNumericallyEqual(triangle.Vertex0, triangle.Vertex1, vertexPositionTolerance)
              || Vector3F.AreNumericallyEqual(triangle.Vertex0, triangle.Vertex2, vertexPositionTolerance)
              || Vector3F.AreNumericallyEqual(triangle.Vertex1, triangle.Vertex2, vertexPositionTolerance)))
      {
        // Ignore degenerated triangles.
        return;
      }

      // Create lists if they were removed.
      if (Vertices == null)
        Vertices = new List<Vector3F>();
      if (Indices == null)
        Indices = new List<int>();

      int numberOfVertices = Vertices.Count;
      if (!weldVerticesBruteForce)
      {
        // ----- Simply add vertices without vertex welding.
        Vertices.Add(triangle.Vertex0);
        Indices.Add(numberOfVertices);
        numberOfVertices++;
        Vertices.Add(triangle.Vertex1);
        Indices.Add(numberOfVertices);
        numberOfVertices++;
        Vertices.Add(triangle.Vertex2);
        Indices.Add(numberOfVertices);
      }
      else
      {
        // ----- Add vertices and try to merge vertices with the same position.

        // Loop through the 3 vertices of the triangle.
        for (int vertexIndex = 0; vertexIndex < 3; vertexIndex++)
        {
          Vector3F vertex = triangle[vertexIndex];

          // See if the vertex is already in the mesh.
          int index = numberOfVertices;
          for (int i = 0; i < numberOfVertices && index == numberOfVertices; i++)
            if (Vector3F.AreNumericallyEqual(Vertices[i], vertex, vertexPositionTolerance))
              index = i;

          // Add vertex if it is not in the mesh.
          if (index == numberOfVertices)
          {
            Vertices.Add(vertex);
            numberOfVertices++;
          }

          // Add index.
          Indices.Add(index);
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Removes duplicate vertices.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Removes duplicate vertices.
    /// </summary>
    /// <returns>The number of removed vertices.</returns>
    /// <remarks>
    /// <para>
    /// This method calls <see cref="WeldVertices(float)"/> with a vertex position tolerance of
    /// <see cref="Numeric.EpsilonF"/>.
    /// </para>
    /// <para>
    /// Vertex welding is also called vertex shifting or vertex merging. Vertices near each other
    /// are merged to a single vertex to remove duplicate, redundant vertices.
    /// </para>
    /// </remarks>
    public int WeldVertices()
    {
      return WeldVertices(Numeric.EpsilonF);
    }


    /// <summary>
    /// Removes duplicate vertices.
    /// </summary>
    /// <param name="vertexPositionTolerance">
    /// The vertex position tolerance. If the distance between two vertices is less than this value,
    /// the vertices are merged.
    /// </param>
    /// <returns>The number of removed vertices.</returns>
    /// <remarks>
    /// <para>
    /// Vertex welding is also called vertex shifting or vertex merging. Vertices near each other
    /// are merged to a single vertex to remove duplicate, redundant vertices.
    /// </para>
    /// </remarks>
    /// <example>
    /// The following examples shows how to apply vertex welding when building a complex mesh.
    /// <code lang="csharp">
    /// <![CDATA[
    /// // Building a complex triangle mesh using vertex welding.
    /// TriangleMesh mesh = new TriangleMesh();
    /// 
    /// // Add triangles:
    /// mesh.AddTriangle(new Triangle(v0, v1, v2), false);
    /// mesh.AddTriangle(new Triangle(v4, v1, v0), false);
    /// ...
    /// 
    /// // After all vertices are added, remove duplicates.
    /// mesh.WeldVertices(0.001f);
    /// ]]>
    /// </code>
    /// </example>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="vertexPositionTolerance"/> is negative or 0.
    /// </exception>
    public int WeldVertices(float vertexPositionTolerance)
    {
      if (Vertices == null || Indices == null)
        return 0;

      int[] vertexRemap;
      int numberOfMergedVertices = GeometryHelper.MergeDuplicatePositions(Vertices, vertexPositionTolerance, out vertexRemap);

      if (numberOfMergedVertices > 0)
      {
        // Remap Indices.
        int numberOfIndices = Indices.Count;
        for (int i = 0; i < numberOfIndices; i++)
          Indices[i] = vertexRemap[Indices[i]];
      }

      return numberOfMergedVertices;
    }


    /// <summary>
    /// Gets the triangle with the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The triangle with the given index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is out of range.
    /// </exception>
    /// <exception cref="GeometryException">
    /// Either <see cref="Vertices"/> or <see cref="Indices"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public Triangle GetTriangle(int index)
    {
      if (Vertices == null || Indices == null)
        throw new GeometryException("Invalid TriangleMesh. Vertex list or index list is null.");
      if (index < 0 || index >= NumberOfTriangles)
        throw new ArgumentOutOfRangeException("index", "The index is out of range.");

      index *= 3;

      return new Triangle(
        Vertices[Indices[index + 0]],
        Vertices[Indices[index + 1]],
        Vertices[Indices[index + 2]]);
    }


    /// <summary>
    /// Transforms all vertices by the given matrix.
    /// </summary>
    /// <param name="matrix">The transformation matrix.</param>
    public void Transform(Matrix44F matrix)
    {
      if (Vertices == null)
        return;

      int numberOfVertices = Vertices.Count;
      for (int i = 0; i < numberOfVertices; i++)
        Vertices[i] = matrix.TransformPosition(Vertices[i]);
    }


    /// <summary>
    /// Changes the winding order of all triangles.
    /// </summary>
    public void ReverseWindingOrder()
    {
      for (int i = 0; i < NumberOfTriangles; i++)
      {
        var dummy = Indices[i * 3 + 1];
        Indices[i * 3 + 1] = Indices[i * 3 + 2];
        Indices[i * 3 + 2] = dummy;
      }
    }


    /// <summary>
    /// Computes the mesh normals.
    /// </summary>
    /// <param name="useWeightedAverage">
    /// If set to <see langword="true"/> the influence of each triangle normal is weighted by the 
    /// triangle area; otherwise, all triangle normals have the same weight.
    /// </param>
    /// <param name="angleLimit">
    /// The angle limit in radians. Normals are only merged if the angle between the triangle
    /// normals is equal to or less than the angle limit. Set this value to -1 to disable the angle
    /// limit (all normals of one vertex are merged). 
    /// </param>
    /// <returns>
    /// If no angle limit is used (angle limit is -1), an array with one normal per vertex is 
    /// returned. If an angle limit is used, an array with one normal per index is used.
    /// </returns>
    /// <remarks>
    /// This method computes vertex normals by averaging the triangle normals. 
    /// <para>
    /// <strong>Angle limit:</strong> The <paramref name="angleLimit"/> can be used to keep sharp
    /// edges between triangles. If the angle limit is used, the returned array contains one entry
    /// per index (= 3 entries per triangle) because the normal of a vertex can have different
    /// direction for each neighbor triangle. If the angle limit is not used, the returned array
    /// contains one entry per vertex because the vertex normal of a single vertex is the same for
    /// all neighboring triangles.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Vector3F[] ComputeNormals(bool useWeightedAverage, float angleLimit)
    {
      if (Vertices == null || Indices == null)
        return null;

      Vector3F[] normals;
      if (angleLimit < 0)
      {
        // ----- Not using angle limit.
        normals = ComputeNormals(useWeightedAverage);
      }
      else
      {
        // ----- Using angle limit.
        normals = ComputeNormalsUsingAngleLimit(angleLimit, useWeightedAverage);
      }

      return normals;
    }


    private Vector3F[] ComputeNormals(bool useWeightedAverage)
    {
      var numberOfVertices = Vertices.Count;
      var numberOfTriangles = NumberOfTriangles;

      // One normal per vertex.
      Vector3F[] normals = new Vector3F[numberOfVertices];

      // Loop over all triangles and sum up the normals.
      for (int i = 0; i < numberOfTriangles; i++)
      {
        int i0 = Indices[i * 3 + 0];
        int i1 = Indices[i * 3 + 1];
        int i2 = Indices[i * 3 + 2];

        Vector3F v0 = Vertices[i0];
        Vector3F v1 = Vertices[i1];
        Vector3F v2 = Vertices[i2];

        // The unnormalized normal, the normal length is proportional to the triangle area.
        Vector3F normal = Vector3F.Cross(v1 - v0, v2 - v0);

        if (useWeightedAverage || normal.TryNormalize())
        {
          normals[i0] += normal;
          normals[i1] += normal;
          normals[i2] += normal;
        }
      }

      // Go over all normals and normalized them.
      for (int i = 0; i < numberOfVertices; i++)
      {
        var normal = normals[i];

        if (normal.TryNormalize())
          normals[i] = normal;
        else
          normals[i] = Vector3F.UnitY;
      }
      return normals;
    }


    private Vector3F[] ComputeNormalsUsingAngleLimit(float angleLimit, bool useWeightedAverage)
    {
      var numberOfVertices = Vertices.Count;
      var numberOfTriangles = NumberOfTriangles;

      // Array with the normalized triangle normals. The weight is stored in the W component.
      Vector4F[] triangleNormals = new Vector4F[numberOfTriangles];

      // An array of lists. One list per vertex. Each list contains all triangle 
      // normals for a vertex.
      List<Vector4F>[] normalsPerVertex = new List<Vector4F>[numberOfVertices];
      for (int i = 0; i < numberOfVertices; i++)
      {
        // For each vertex there will be on average less than 6 neighbor triangles.
        normalsPerVertex[i] = new List<Vector4F>(6);
      }

      // Loop over triangles and collect triangle normal info.
      for (int i = 0; i < numberOfTriangles; i++)
      {
        int i0 = Indices[i * 3 + 0];
        int i1 = Indices[i * 3 + 1];
        int i2 = Indices[i * 3 + 2];

        Vector3F v0 = Vertices[i0];
        Vector3F v1 = Vertices[i1];
        Vector3F v2 = Vertices[i2];

        Vector3F normal = Vector3F.Cross(v1 - v0, v2 - v0);
        float lengthSquared = normal.LengthSquared;
        if (!Numeric.IsZero(lengthSquared, Numeric.EpsilonFSquared))  // Degenerate triangles are ignored.
        {
          float weight = (float)Math.Sqrt(lengthSquared);

          var normal4 = new Vector4F(normal / weight, weight);
          triangleNormals[i] = normal4;
          normalsPerVertex[i0].Add(normal4);
          normalsPerVertex[i1].Add(normal4);
          normalsPerVertex[i2].Add(normal4);
        }
      }

      var cosAngleLimit = (float)Math.Cos(angleLimit);

      // The result array with one entry per index (3 entries per triangle).
      Vector3F[] normals = new Vector3F[numberOfTriangles * 3];

      // Loop over triangles.
      for (int i = 0; i < numberOfTriangles; i++)
      {
        var triangleNormal4 = triangleNormals[i];
        var triangleNormal = triangleNormal4.XYZ;

        if (triangleNormal4.W == 0)
        {
          // The triangle is degenerate. We set an arbitrary normalized vector for each vertex.
          normals[i * 3 + 0] = Vector3F.UnitY;
          normals[i * 3 + 1] = Vector3F.UnitY;
          normals[i * 3 + 2] = Vector3F.UnitY;
        }
        else
        {
          // Loop over the 3 vertices.
          for (int j = 0; j < 3; j++)
          {
            var indexIndex = i * 3 + j;
            var vertexIndex = Indices[indexIndex];
            var vertexNormal = new Vector3F();

            // Average all normals in the normal list of the current vertex.
            foreach (var normal4 in normalsPerVertex[vertexIndex])
            {
              var normal = normal4.XYZ;
              var weight = normal4.W;

              // Angle limit test.
              if (Numeric.IsGreaterOrEqual(Vector3F.Dot(triangleNormal, normal), cosAngleLimit))  // This also checks the triangleNormal against itself.
              {
                if (useWeightedAverage)
                  vertexNormal += normal * weight;
                else
                  vertexNormal += normal;
              }
            }

            vertexNormal.Normalize();
            normals[indexIndex] = vertexNormal;
          }
        }
      }

      return normals;
    }


#if XNA || MONOGAME
    /// <summary>
    /// Creates a triangle mesh from an XNA <see cref="Model"/>. (Only available in the
    /// XNA-compatible build, except Silverlight.)
    /// </summary>
    /// <param name="model">The XNA model.</param>
    /// <returns>
    /// A triangle mesh containing all triangles of the specified model.
    /// </returns>
    /// <remarks>
    /// This method is only available in XNA-compatible builds of DigitalRune.Geometry.dll. It is
    /// not available in "Any CPU" builds or Silverlight.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public static TriangleMesh FromModel(Model model)
    {
      // Similar code can be found on http://www.enchantedage.com/node/30 (by Jon Watte).
      // But this code was developed independently.

      if (model == null)
        throw new ArgumentNullException("model");
      
      var triangleMesh = new TriangleMesh();

      foreach (var modelMesh in model.Meshes)
      {
        // Get bone transformation.
        Matrix transform = GetAbsoluteTransform(modelMesh.ParentBone);

        foreach (var modelMeshPart in modelMesh.MeshParts)
        {
          // Get vertex element info.
          var vertexDeclaration = modelMeshPart.VertexBuffer.VertexDeclaration;
          var vertexElements = vertexDeclaration.GetVertexElements();

          // Get the vertex positions.
          var positionElement = vertexElements.First(e => e.VertexElementUsage == VertexElementUsage.Position);
          if (positionElement.VertexElementFormat != VertexElementFormat.Vector3)
            throw new NotSupportedException("For vertex positions only VertexElementFormat.Vector3 is supported.");

          var positions = new Vector3[modelMeshPart.NumVertices];
          modelMeshPart.VertexBuffer.GetData(
            modelMeshPart.VertexOffset * vertexDeclaration.VertexStride + positionElement.Offset,
            positions,
            0,
            modelMeshPart.NumVertices,
            vertexDeclaration.VertexStride);

          // Apply bone transformation.
          for (int i = 0; i < positions.Length; i++)
            positions[i] = Vector3.Transform(positions[i], transform);

          // Remember the number of vertices already in the mesh.
          int vertexCount = triangleMesh.Vertices.Count;

          // Add the vertices of the current modelMeshPart.
          foreach (Vector3 p in positions)
            triangleMesh.Vertices.Add((Vector3F)p);

          // Get indices.
          var indexElementSize = (modelMeshPart.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits) ? 2 : 4;
          if (indexElementSize == 2)
          {
            ushort[] indices = new ushort[modelMeshPart.PrimitiveCount * 3];
            modelMeshPart.IndexBuffer.GetData(
              modelMeshPart.StartIndex * 2,
              indices,
              0,
              modelMeshPart.PrimitiveCount * 3);

            // Add indices to triangle mesh.
            for (int i = 0; i < modelMeshPart.PrimitiveCount; i++)
            {
              // The three indices of the next triangle.
              // We add 'vertexCount' because the triangleMesh already contains other mesh parts.
              int i0 = indices[i * 3 + 0] + vertexCount;
              int i1 = indices[i * 3 + 1] + vertexCount;
              int i2 = indices[i * 3 + 2] + vertexCount;

              triangleMesh.Indices.Add(i0);
              triangleMesh.Indices.Add(i2);     // DigitalRune Geometry uses other winding order!
              triangleMesh.Indices.Add(i1);
            }
          }
          else
          {
            Debug.Assert(indexElementSize == 4);

            int[] indices = new int[modelMeshPart.PrimitiveCount * 3];
            modelMeshPart.IndexBuffer.GetData(
              modelMeshPart.StartIndex * 4,
              indices,
              0,
              modelMeshPart.PrimitiveCount * 3);

            // Add indices to triangle mesh.
            for (int i = 0; i < modelMeshPart.PrimitiveCount; i++)
            {
              // The three indices of the next triangle.
              // We add 'vertexCount' because the triangleMesh already contains other mesh parts.
              int i0 = indices[i * 3 + 0] + vertexCount;
              int i1 = indices[i * 3 + 1] + vertexCount;
              int i2 = indices[i * 3 + 2] + vertexCount;

              triangleMesh.Indices.Add(i0);
              triangleMesh.Indices.Add(i2);     // DigitalRune Geometry uses other winding order!
              triangleMesh.Indices.Add(i1);
            }
          }
        }
      }

      return triangleMesh;
    }


    private static Matrix GetAbsoluteTransform(ModelBone bone)
    {
      if (bone == null)
        return Matrix.Identity;
      return bone.Transform * GetAbsoluteTransform(bone.Parent);
    }
#endif
    #endregion
  }
}
