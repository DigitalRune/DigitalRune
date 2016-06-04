// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Provides helper methods for <see cref="Mesh"/>es, <see cref="Submesh"/>es and 
  /// <see cref="Material"/>s.
  /// </summary>
  public static partial class MeshHelper
  {
    /// <summary>
    /// Gets the material of a submesh.
    /// </summary>
    /// <param name="submesh">The submesh.</param>
    /// <returns>
    /// The material of this submesh, or <see langword="null"/> if the submesh is not assigned to a 
    /// mesh or if the <see cref="Submesh.MaterialIndex"/> is invalid.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="submesh"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Material GetMaterial(this Submesh submesh)
    {
      if (submesh == null)
        throw new ArgumentNullException("submesh");

      var mesh = submesh.Mesh;
      if (mesh == null)
        return null;

      var index = submesh.MaterialIndex;

      if (0 <= index && index < mesh.Materials.Count)
        return mesh.Materials[index];

      return null;
    }


    /// <summary>
    /// Sets the material for a submesh.
    /// </summary>
    /// <param name="submesh">The submesh.</param>
    /// <param name="material">The new material.</param>
    /// <remarks>
    /// The material can only be changed if the <see cref="Submesh"/> is part of a 
    /// <see cref="Mesh"/>. When the material is changed, the <see cref="Mesh.Materials"/> of the
    /// <see cref="Mesh"/> are automatically updated. (New materials are added. Unused materials are
    /// removed.)
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="submesh"/> or <paramref name="material"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="Submesh"/> is not yet part of a <see cref="Mesh"/>. Add the 
    /// <see cref="Submesh"/> to the <see cref="Mesh.Submeshes"/> collection of a <see cref="Mesh"/>
    /// before setting the material.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public static void SetMaterial(this Submesh submesh, Material material)
    {
      if (submesh == null)
        throw new ArgumentNullException("submesh");

      var mesh = submesh.Mesh;
      if (mesh == null)
        throw new InvalidOperationException("Cannot add material to submesh. Submesh needs to be added to Mesh first.");
      if (material == null)
        throw new ArgumentNullException("material");

      var oldMaterial = GetMaterial(submesh);
      if (oldMaterial == material)
        return;

      // Find out if the old or new material is used by other submeshes which belong
      // to the same mesh.
      bool oldMaterialStillInUse = false;
      bool newMaterialAlreadyInUse = false;
      foreach (var otherSubmesh in mesh.Submeshes)
      {
        if (otherSubmesh != submesh)
        {
          Material otherMaterial = GetMaterial(otherSubmesh);
          if (otherMaterial == oldMaterial)
            oldMaterialStillInUse = true;
          else if (otherMaterial == material)
            newMaterialAlreadyInUse = true;
        }
      }

      // Remove old material from parent mesh material collection if it is not used by 
      // any submesh.
      int oldIndex = submesh.MaterialIndex;
      if (!oldMaterialStillInUse && oldMaterial != null)
      {
        mesh.Materials.RemoveAt(oldIndex);

        // One material was removed --> Update indices of meshes.
        foreach (var otherSubmesh in mesh.Submeshes)
          if (otherSubmesh.MaterialIndex > oldIndex)
            otherSubmesh.MaterialIndex--;
      }

      // Add new material to parent mesh material collection if this is the first submesh
      // that uses this material.
      if (newMaterialAlreadyInUse)
      {
        // Get index of the new material.
        submesh.MaterialIndex = mesh.Materials.IndexOf(material);
      }
      else
      {
        // Add new material to the mesh at the end of the material collection.
        mesh.Materials.Add(material);
        submesh.MaterialIndex = mesh.Materials.Count - 1;
      }
    }


    /// <summary>
    /// Gets the names of all morph targets.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <returns>The names of all morph targets.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<string> GetMorphTargetNames(this Mesh mesh)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      return new ReadOnlyCollection<string>(mesh.GetMorphTargetNames());
    }


    /// <summary>
    /// Draws the <see cref="Submesh"/> using the currently active shader.
    /// </summary>
    /// <param name="submesh">The submesh.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="submesh"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// This method sets the <see cref="VertexDeclaration"/>, <see cref="Submesh.VertexBuffer"/>,
    /// and <see cref="Submesh.IndexBuffer"/> of the submesh and calls
    /// <see cref="GraphicsDevice.DrawIndexedPrimitives"/>. Effects are not handled in this method.
    /// The method assumes that the correct shader effect is already active.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static void Draw(this Submesh submesh)
    {
      if (submesh == null)
        throw new ArgumentNullException("submesh");

      Debug.Assert(!submesh.HasMorphTargets, "Submesh without morph targets expected.");

      var vertexBuffer = submesh.VertexBuffer;
      if (vertexBuffer == null || submesh.VertexCount <= 0)
        return;

      // VertexBuffer.GraphicsDevice is set to null when VertexBuffer is disposed of.
      // Check VertexBuffer.IsDisposed to avoid NullReferenceException.
      if (vertexBuffer.IsDisposed)
          throw new ObjectDisposedException("VertexBuffer", "Cannot draw mesh. The vertex buffer has already been disposed of.");

      var graphicsDevice = vertexBuffer.GraphicsDevice;
      graphicsDevice.SetVertexBuffer(vertexBuffer);

      var indexBuffer = submesh.IndexBuffer;
      if (indexBuffer == null)
      {
        graphicsDevice.DrawPrimitives(
          submesh.PrimitiveType,
          submesh.StartVertex,
          submesh.PrimitiveCount);
      }
      else
      {
        graphicsDevice.Indices = indexBuffer;
#if MONOGAME
        graphicsDevice.DrawIndexedPrimitives(
          submesh.PrimitiveType,
          submesh.StartVertex,
          submesh.StartIndex,
          submesh.PrimitiveCount);
#else
        graphicsDevice.DrawIndexedPrimitives(
          submesh.PrimitiveType,
          submesh.StartVertex,
          0,
          submesh.VertexCount,
          submesh.StartIndex,
          submesh.PrimitiveCount);
#endif
      }
    }


    /// <overloads>
    /// <summary>
    /// Creates a submesh to draw a triangle mesh.
    /// </summary>
    /// </overloads>
    ///
    /// <summary>
    /// Creates a submesh to draw a triangle mesh.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="mesh">The mesh.</param>
    /// <param name="angleLimit">
    /// The angle limit for normal vectors in radians. Normals are only merged if the angle between
    /// the triangle normals is equal to or less than the angle limit. Set this value to -1 to
    /// disable the angle limit (all normals of one vertex are merged). 
    /// </param>
    /// <returns>The submesh, or <see langword="null"/> if the mesh is empty.</returns>
    /// <remarks>
    /// The returned submesh will contain a triangle list that represents the given mesh. Each 
    /// vertex contains the position and the normal vector (no texture coordinates, no vertex 
    /// colors, etc.). The submesh will not use an index buffer.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> or <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateSubmesh(GraphicsDevice graphicsDevice, TriangleMesh mesh, float angleLimit)
    {
      VertexBuffer vertexBuffer;
      PrimitiveType primitiveType;
      int primitiveCount;
      CreateVertexBuffer(graphicsDevice, mesh, angleLimit, out vertexBuffer, out primitiveType, out primitiveCount);

      if (vertexBuffer == null)
        return null;

      return new Submesh
      {
        PrimitiveType = PrimitiveType.TriangleList,
        PrimitiveCount = primitiveCount,
        VertexBuffer = vertexBuffer,
        VertexCount = vertexBuffer.VertexCount
      };
    }


    /// <summary>
    /// Creates a submesh to draw a triangle mesh.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="mesh">The mesh.</param>
    /// <param name="angleLimit">
    /// The angle limit for normal vectors in radians. Normals are only merged if the angle between
    /// the triangle normals is equal to or less than the angle limit. Set this value to -1 to
    /// disable the angle limit (all normals of one vertex are merged). 
    /// </param>
    /// <returns>The submesh, or <see langword="null"/> if the mesh is empty.</returns>
    /// <remarks>
    /// The returned submesh will contain a triangle list that represents the given mesh. Each 
    /// vertex contains the position and the normal vector (no texture coordinates, no vertex 
    /// colors, etc.). The submesh will not use an index buffer.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> or <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static Submesh CreateSubmesh(GraphicsDevice graphicsDevice, ITriangleMesh mesh, float angleLimit)
    {
      var tm = mesh as TriangleMesh;
      if (tm == null)
      {
        tm = new TriangleMesh();
        tm.Add(mesh);
        tm.WeldVertices();
      }

      return CreateSubmesh(graphicsDevice, tm, angleLimit);
    }


    internal static void CreateVertexBuffer(GraphicsDevice graphicsDevice, TriangleMesh mesh, float angleLimit, out VertexBuffer vertexBuffer, out PrimitiveType primitiveType, out int primitiveCount)
    {
      // Note: We do not use shared vertices and an IndexBuffer because a vertex can be used by 
      // several triangles with different normals. 

      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      var numberOfTriangles = mesh.NumberOfTriangles;

      if (numberOfTriangles == 0)
      {
        primitiveType = PrimitiveType.TriangleList;
        primitiveCount = 0;
        vertexBuffer = null;
        return;
      }

      primitiveType = PrimitiveType.TriangleList;
      primitiveCount = numberOfTriangles;
      int vertexCount = numberOfTriangles * 3;

      // Create vertex data for a triangle list.
      var vertices = new VertexPositionNormal[vertexCount];

      // Create vertex normals. 
      var normals = mesh.ComputeNormals(false, angleLimit);

      for (int i = 0; i < numberOfTriangles; i++)
      {
        var i0 = mesh.Indices[i * 3 + 0];
        var i1 = mesh.Indices[i * 3 + 1];
        var i2 = mesh.Indices[i * 3 + 2];

        var v0 = mesh.Vertices[i0];
        var v1 = mesh.Vertices[i1];
        var v2 = mesh.Vertices[i2];

        Vector3F n0, n1, n2;
        if (angleLimit < 0)
        {
          // If the angle limit is negative, ComputeNormals() returns one normal per vertex.
          n0 = normals[i0];
          n1 = normals[i1];
          n2 = normals[i2];
        }
        else
        {
          // If the angle limits is >= 0, ComputeNormals() returns 3 normals per triangle.
          n0 = normals[i * 3 + 0];
          n1 = normals[i * 3 + 1];
          n2 = normals[i * 3 + 2];
        }

        // Add new vertex data.
        // DigitalRune.Geometry uses counter-clockwise front faces. XNA uses
        // clockwise front faces (CullMode.CullCounterClockwiseFace) per default. 
        // Therefore we change the vertex orientation of the triangles. 
        vertices[i * 3 + 0] = new VertexPositionNormal((Vector3)v0, (Vector3)n0);
        vertices[i * 3 + 1] = new VertexPositionNormal((Vector3)v2, (Vector3)n2);  // v2 instead of v1!
        vertices[i * 3 + 2] = new VertexPositionNormal((Vector3)v1, (Vector3)n1);
      }

      // Create a vertex buffer.
      vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormal), vertexCount, BufferUsage.None);
      vertexBuffer.SetData(vertices);
    }


    /// <summary>
    /// Gets the number of primitives for the given vertex/index buffer and primitive type.
    /// </summary>
    /// <param name="vertexBuffer">The vertex buffer.</param>
    /// <param name="indexBuffer">The index buffer.</param>
    /// <param name="primitiveType">The type of the primitive.</param>
    /// <returns>The number of primitives in the given vertex and index buffer.</returns>
    internal static int GetPrimitiveCount(VertexBuffer vertexBuffer, IndexBuffer indexBuffer, PrimitiveType primitiveType)
    {
      if (vertexBuffer == null && indexBuffer == null)
        return 0;

      // Compute number of primitives.
      var count = (indexBuffer == null) ? vertexBuffer.VertexCount : indexBuffer.IndexCount;
      switch (primitiveType)
      {
        case PrimitiveType.LineList:
          return count / 2;
        case PrimitiveType.LineStrip:
          return count - 1;
        case PrimitiveType.TriangleList:
          return count / 3;
        case PrimitiveType.TriangleStrip:
          return count - 2;
      }

      throw new NotSupportedException("Unknown primitive type.");
    }


    internal static int GetNumberOfIndices(PrimitiveType primitiveType, int primitiveCount)
    {
      switch (primitiveType)
      {
#if MONOGAME
        case PrimitiveType.PointList:
          return primitiveCount;
#endif
        case PrimitiveType.LineList:
          return primitiveCount * 2;
        case PrimitiveType.LineStrip:
          return primitiveCount + 1;
        case PrimitiveType.TriangleList:
          return primitiveCount * 3;
        case PrimitiveType.TriangleStrip:
          return primitiveCount + 2;
      }

      throw new NotSupportedException();
    }


    internal static bool AreEqual(VertexDeclaration vertexDeclarationA, VertexDeclaration vertexDeclarationB)
    {
      // Compare vertex strides.
      if (vertexDeclarationA.VertexStride != vertexDeclarationB.VertexStride)
        return false;

      // Compare vertex element count.
      var vertexElementsA = vertexDeclarationA.GetVertexElements();
      var vertexElementsB = vertexDeclarationB.GetVertexElements();
      if (vertexElementsA.Length != vertexElementsB.Length)
        return false;

      // Compare each vertex element structure.
      for (int j = 0; j < vertexElementsA.Length; j++)
      {
        if (vertexElementsA[j] != vertexElementsB[j])
          return false;
      }

      return true;
    }


    /// <overloads>
    /// <summary>
    /// Creates a <see cref="TriangleMesh"/> (DigitalRune.Geometry) for the <see cref="Mesh"/> or 
    /// <see cref="Submesh"/> (DigitalRune.Graphics).
    /// </summary>
    /// </overloads>
    ///
    /// <summary>
    /// Creates a <see cref="TriangleMesh"/> from a <see cref="Mesh"/>. 
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <returns>A triangle mesh containing all triangles of the specified mesh.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// A submeshes uses a primitive type other than triangle lists. Other primitive types are not 
    /// supported.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The vertex position format of a submesh is not <see cref="Vector3"/>. Only 
    /// <see cref="Vector3"/> is supported
    /// </exception>
    public static TriangleMesh ToTriangleMesh(this Mesh mesh)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      var triangleMesh = new TriangleMesh();
      foreach (var submesh in mesh.Submeshes)
        ToTriangleMesh(submesh, triangleMesh);

      return triangleMesh;
    }


    /// <summary>
    /// Creates a <see cref="TriangleMesh"/> from a <see cref="Submesh"/>. 
    /// </summary>
    /// <param name="submesh">The mesh.</param>
    /// <returns>
    /// A triangle mesh containing all triangles of the specified submesh.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="submesh"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// A submeshes uses a primitive type other than triangle lists. Other primitive types are not 
    /// supported.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The vertex position format of a submesh is not <see cref="Vector3"/>. Only 
    /// <see cref="Vector3"/> is supported
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static TriangleMesh ToTriangleMesh(this Submesh submesh)
    {
      var triangleMesh = new TriangleMesh();
      ToTriangleMesh(submesh, triangleMesh);
      return triangleMesh;
    }


    /// <summary>
    /// Adds the triangles from the specified <see cref="Submesh"/> to a 
    /// <see cref="TriangleMesh"/>.
    /// </summary>
    /// <param name="submesh">The submesh.</param>
    /// <param name="triangleMesh">
    /// The triangle mesh to which the triangles of the <paramref name="submesh"/> are added.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="submesh"/> or <paramref name="triangleMesh"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// A submeshes uses a primitive type other than triangle lists. Other primitive types are not 
    /// supported.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The vertex position format of a submesh is not <see cref="Vector3"/>. Only 
    /// <see cref="Vector3"/> is supported
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public static void ToTriangleMesh(this Submesh submesh, TriangleMesh triangleMesh)
    {
      // This method is similar to TriangleMesh.FromModel().

      if (submesh == null)
        throw new ArgumentNullException("submesh");
      if (triangleMesh == null)
        throw new ArgumentNullException("triangleMesh");

      if (submesh.PrimitiveType != PrimitiveType.TriangleList)
        throw new NotSupportedException("All submeshes must be triangle lists. ToTriangleMesh() does not support other primitive types.");

      if (submesh.VertexBuffer == null)
        return;
      if (triangleMesh.Vertices == null)
        triangleMesh.Vertices = new List<Vector3F>(submesh.VertexCount);
      if (triangleMesh.Indices == null)
        triangleMesh.Indices = new List<int>(submesh.PrimitiveCount * 3);

      // Get vertex element info.
      var vertexDeclaration = submesh.VertexBuffer.VertexDeclaration;
      var vertexElements = vertexDeclaration.GetVertexElements();

      // Get the vertex positions.
      var positionElement = vertexElements.First(e => e.VertexElementUsage == VertexElementUsage.Position);
      if (positionElement.VertexElementFormat != VertexElementFormat.Vector3)
        throw new NotSupportedException("For vertex positions only VertexElementFormat.Vector3 is supported.");

      Vector3[] positions = new Vector3[submesh.VertexCount];
      submesh.VertexBuffer.GetData(
        submesh.StartVertex * vertexDeclaration.VertexStride + positionElement.Offset,
        positions,
        0,
        submesh.VertexCount,
        vertexDeclaration.VertexStride);

      if (submesh.IndexBuffer != null)
      {
        // Remember the number of vertices already in the mesh.
        int vertexCount = triangleMesh.Vertices.Count;

        // Add the vertices of the current modelMeshPart.
        foreach (Vector3 p in positions)
          triangleMesh.Vertices.Add((Vector3F)p);

        // Get indices.
        int indexElementSize = (submesh.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits) ? 2 : 4;
        if (indexElementSize == 2)
        {
          ushort[] indices = new ushort[submesh.PrimitiveCount * 3];
          submesh.IndexBuffer.GetData(
            submesh.StartIndex * 2,
            indices,
            0,
            submesh.PrimitiveCount * 3);

          // Add indices to triangle mesh.
          for (int i = 0; i < submesh.PrimitiveCount; i++)
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
          int[] indices = new int[submesh.PrimitiveCount * 3];
          submesh.IndexBuffer.GetData(
            submesh.StartIndex * 4,
            indices,
            0,
            submesh.PrimitiveCount * 3);

          // Add indices to triangle mesh.
          for (int i = 0; i < submesh.PrimitiveCount; i++)
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
      else
      {
        // No index buffer:
        int vertexCount = triangleMesh.Vertices.Count;
        for (int i = 0; i < submesh.VertexCount; i += 3)
        {
          triangleMesh.Vertices.Add((Vector3F)positions[i]);
          triangleMesh.Vertices.Add((Vector3F)positions[i + 1]);
          triangleMesh.Vertices.Add((Vector3F)positions[i + 2]);

          triangleMesh.Indices.Add(i + vertexCount);
          triangleMesh.Indices.Add(i + 2 + vertexCount);     // DigitalRune Geometry uses other winding order!
          triangleMesh.Indices.Add(i + 1 + vertexCount);
        }
      }
    }
  }
}
