// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class MeshHelper
  {
    // Notes:
    // - We only merge vertex declarations if the elements are in the same order.
    //   That means, we would not merge
    //     VertexPositionTexCoordNormal with
    //     VertexPositionNormalTexCoord.
    //   This case could be merged. However, if this can occur it is better to
    //   change the content pipeline and define a fixed order for the elements.
    //
    // Windows Phone:
    // Windows Phone does not allow unsafe code. Alternatively, we could test
    // whether the data matches the most common vertex types (VertexPosition,
    // VertexPositionNormal, ...) and cast the data.

    private struct MergeJob
    {
      public Pose Pose;
      public Vector3F Scale;
      public Submesh Submesh;
      public byte MergedMaterialIndex;
      public byte VertexDeclarationIndex;

      public uint SortKey;
    }


    private class MergeJobComparer : IComparer<MergeJob>
    {
      public static readonly MergeJobComparer Instance = new MergeJobComparer();
      public int Compare(MergeJob x, MergeJob y)
      {
        return x.SortKey.CompareTo(y.SortKey);
      }
    }


    /// <summary>
    /// Merges the specified scene nodes (including descendants) into a single mesh.
    /// </summary>
    /// <param name="sceneNodes">The scene nodes.</param>
    /// <returns>The merged mesh.</returns>
    /// <remarks>
    /// <para>
    /// Only <see cref="MeshNode"/>s are merged. Other scene node types are ignored.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="sceneNodes"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Too many different materials. Merged mesh must have less than 256 materials.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// A submesh uses a vertex declaration which is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MeshHelper")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "sceneNodes")]
    public static Mesh Merge(IEnumerable<SceneNode> sceneNodes)
    {
      if (sceneNodes == null)
        throw new ArgumentNullException("sceneNodes");

      // Gather flat list of all mesh nodes. Each mesh node could have children.
      var meshNodes = new List<MeshNode>();
      foreach (var meshNode in sceneNodes)
        meshNodes.AddRange(meshNode.GetSubtree().OfType<MeshNode>());

      var mergedMesh = new Mesh();
      try
      {
        var jobs = new List<MergeJob>();

        // A list of all encountered vertex types.
        var vertexDeclarations = new List<VertexDeclaration>();

        // A list of total vertex counts for each vertex declaration.
        var vertexCounts = new List<int>();

        // For indices we only need one counter because we use only one shared index buffer.
        int indexCount = 0;

        var mergedAabb = new Aabb(new Vector3F(float.MaxValue), new Vector3F(float.MinValue));

        // Merge materials, create job list, merge AABBs, check if there is an occluder.
        bool hasOccluder = false;
        foreach (var meshNode in meshNodes)
        {
          var mesh = meshNode.Mesh;

#if ANIMATION
          if (mesh.Skeleton != null)
            throw new NotSupportedException("Cannot merge skinned meshes.");
#endif

          foreach (var submesh in mesh.Submeshes)
          {
            if (submesh.PrimitiveCount <= 0)
              continue;
            if (submesh.VertexBufferEx == null)
              continue;

            // Merge materials and get material index.
            var material = submesh.GetMaterial();
            int mergedMaterialIndex = mergedMesh.Materials.IndexOf(material);
            if (mergedMaterialIndex < 0)
            {
              mergedMaterialIndex = mergedMesh.Materials.Count;
              mergedMesh.Materials.Add(material);
            }

            if (mergedMaterialIndex > byte.MaxValue)
              throw new NotSupportedException("Cannot merge meshes. Merged mesh must not have more than 256 materials.");

            // Try to find index of existing matching vertex declaration.
            var vertexDeclaration = submesh.VertexBuffer.VertexDeclaration;
            int vertexDeclarationIndex = -1;
            for (int i = 0; i < vertexDeclarations.Count; i++)
            {
              if (AreEqual(vertexDeclarations[i], vertexDeclaration))
              {
                vertexDeclarationIndex = i;
                break;
              }
            }

            if (vertexDeclarationIndex < 0)
            {
              // Add new vertex declaration.
              vertexDeclarationIndex = vertexDeclarations.Count;
              vertexDeclarations.Add(vertexDeclaration);
              vertexCounts.Add(0);
            }

            if (vertexDeclarationIndex > byte.MaxValue)
              throw new NotSupportedException("Cannot merge meshes. Merged mesh must not have more than 256 different vertex declarations.");

            // Count total number of vertices per vertex declaration.
            vertexCounts[vertexDeclarationIndex] += submesh.VertexCount;

            // Count number of indices.
            if (submesh.IndexBuffer != null)
              indexCount += GetNumberOfIndices(submesh.PrimitiveType, submesh.PrimitiveCount);

            jobs.Add(new MergeJob
            {
              Pose = meshNode.PoseWorld,
              Scale = meshNode.ScaleWorld,
              Submesh = submesh,
              MergedMaterialIndex = (byte)mergedMaterialIndex,
              VertexDeclarationIndex = (byte)vertexDeclarationIndex,

              // We set a sort key by which we can quickly sort all jobs.
              // We can merge submeshes if they have the same material, vertex declaration,
              // primitive type.
              // Submeshes do not need to have an index buffer. We could merge a submesh with
              // and without index buffer by generating indices. However, we do not merge in
              // this case.
              //             -------------------------------------------------------------------------------
              // Sort key = |  unused  |  vertex type  |  material  |  primitive type  |  has index buffer  |
              //            |  8 bit   |     8 bit     |   8 bit    |      7 bit       |       1 bit        |
              //             -------------------------------------------------------------------------------
              SortKey = (uint)(vertexDeclarationIndex << 16
                               | mergedMaterialIndex << 8
                               | (int)submesh.PrimitiveType << 1
                               | ((submesh.IndexBuffer != null) ? 1 : 0)),
            });
          }

          // Merge AABBs.
          mergedAabb = Aabb.Merge(meshNode.Aabb, mergedAabb);

          hasOccluder |= (mesh.Occluder != null);
        }

        if (jobs.Count == 0)
        {
          mergedMesh.BoundingShape = Shape.Empty;
          return mergedMesh;
        }

        // Create new bounding shape from merged AABB.
        var extent = mergedAabb.Extent;
        if (Numeric.IsFinite(extent.X + extent.Y + extent.Z))
        {
          var boxShape = new BoxShape(extent);
          if (mergedAabb.Center.IsNumericallyZero)
            mergedMesh.BoundingShape = boxShape;
          else
            mergedMesh.BoundingShape = new TransformedShape(new GeometricObject(boxShape, new Pose(mergedAabb.Center)));
        }
        else
        {
          mergedMesh.BoundingShape = Shape.Infinite;
        }

        jobs.Sort(MergeJobComparer.Instance);

        MergeSubmeshes(jobs, mergedMesh, vertexDeclarations, vertexCounts, indexCount);

        if (hasOccluder)
          mergedMesh.Occluder = MergeOccluders(meshNodes);

        return mergedMesh;
      }
      catch
      {
        mergedMesh.Dispose();
        throw;
      }
    }


    /// <overloads>
    /// <summary>
    /// Merges the specified mesh instances into a single mesh.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Merges the specified mesh instances into a single mesh.
    /// </summary>
    /// <param name="mesh">The mesh.</param>
    /// <param name="scales">
    /// The scale factors. Can be <see langword="null"/> to use no scale, i.e. all scale factors are
    /// (1, 1, 1).
    /// </param>
    /// <param name="poses">The poses (positions and orientations).</param>
    /// <returns>The merged mesh.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="mesh"/> or <paramref name="poses"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of elements in <paramref name="poses"/> and <paramref name="scales"/> does not
    /// match.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Cannot merge skinned meshes.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Too many different vertex declarations. Merged mesh must not have more than 256 different
    /// vertex declarations.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// Too many different materials. Merged mesh would have more than 256 materials.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// A submesh uses a vertex declaration which is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MeshHelper")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "mesh")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "scales")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "poses")]
    public static Mesh Merge(Mesh mesh, IList<Vector3F> scales, IList<Pose> poses)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");
#if ANIMATION
      if (mesh.Skeleton != null)
        throw new NotSupportedException("Cannot merge skinned meshes.");
#endif
      if (poses == null)
        throw new ArgumentNullException("poses");

      if (scales == null)
      {
        var array = new Vector3F[poses.Count];
        for (int i = 0; i < array.Length; i++)
          array[i] = Vector3F.One;

        scales = array;
      }

      if (scales.Count != poses.Count)
        throw new ArgumentException("The number of elements in poses and scales must be equal.");

      var mergedMesh = new Mesh();
      try
      {
        var jobs = new List<MergeJob>();

        // A list of all encountered vertex types.
        var vertexDeclarations = new List<VertexDeclaration>();

        // A list of total vertex counts for each vertex declaration.
        var vertexCounts = new List<int>();

        // For indices we only need one counter because we use only one shared index buffer.
        int indexCount = 0;

        // Merge materials, create job list.
        foreach (var submesh in mesh.Submeshes)
        {
          if (submesh.PrimitiveCount <= 0)
            continue;
          if (submesh.VertexBufferEx == null)
            continue;

          // Merge materials and get material index.
          var material = submesh.GetMaterial();
          var mergedMaterialIndex = mergedMesh.Materials.IndexOf(material);
          if (mergedMaterialIndex < 0)
          {
            mergedMaterialIndex = mergedMesh.Materials.Count;
            mergedMesh.Materials.Add(material);
          }

          if (mergedMaterialIndex > byte.MaxValue)
            throw new NotSupportedException("Cannot merge meshes. Merged mesh must not have more than 256 materials.");

          // Try to find index of existing matching vertex declaration.
          int vertexDeclarationIndex = -1;
          for (int i = 0; i < vertexDeclarations.Count; i++)
          {
            if (AreEqual(vertexDeclarations[i], submesh.VertexBuffer.VertexDeclaration))
            {
              vertexDeclarationIndex = i;
              break;
            }
          }

          if (vertexDeclarationIndex < 0)
          {
            // Add new vertex declaration.
            vertexDeclarationIndex = vertexDeclarations.Count;
            vertexDeclarations.Add(submesh.VertexBuffer.VertexDeclaration);
            vertexCounts.Add(0);
          }

          if (vertexDeclarationIndex > byte.MaxValue)
            throw new NotSupportedException("Cannot merge meshes. Merged mesh must not have more than 256 different vertex declarations.");

          for (int instanceIndex = 0; instanceIndex < poses.Count; instanceIndex++)
          {
            // Count total number of vertices per vertex declaration.
            vertexCounts[vertexDeclarationIndex] += submesh.VertexCount;

            // Count number of indices.
            if (submesh.IndexBuffer != null)
              indexCount += GetNumberOfIndices(submesh.PrimitiveType, submesh.PrimitiveCount);

            jobs.Add(new MergeJob
            {
              Pose = poses[instanceIndex],
              Scale = scales[instanceIndex],
              Submesh = submesh,
              MergedMaterialIndex = (byte)mergedMaterialIndex,
              VertexDeclarationIndex = (byte)vertexDeclarationIndex,

              // We set a sort key by which we can quickly sort all jobs.
              // We can merge submeshes if they have the same material, vertex declaration,
              // primitive type.
              // Submeshes do not need to have an index buffer. We could merge a submesh with
              // and without index buffer by generating indices. However, we do not merge in
              // this case.
              //             -------------------------------------------------------------------------------
              // Sort key = |  unused  |  vertex type  |  material  |  primitive type  |  has index buffer  |
              //            |  8 bit   |     8 bit     |   8 bit    |      7 bit       |       1 bit        |
              //             -------------------------------------------------------------------------------
              SortKey = (uint)(vertexDeclarationIndex << 16
                               | mergedMaterialIndex << 8
                               | (int)submesh.PrimitiveType << 1
                               | ((submesh.IndexBuffer != null) ? 1 : 0)),
            });
          }
        }

        // Merge AABBs.
        var mergedAabb = new Aabb(new Vector3F(float.MaxValue), new Vector3F(float.MinValue));
        for (int instanceIndex = 0; instanceIndex < poses.Count; instanceIndex++)
        {
          mergedAabb = Aabb.Merge(
            mesh.BoundingShape.GetAabb(scales[instanceIndex], poses[instanceIndex]),
            mergedAabb);
        }

        if (jobs.Count == 0)
        {
          mergedMesh.BoundingShape = Shape.Empty;
          return mergedMesh;
        }

        // Create new bounding shape from merged AABB.
        var extent = mergedAabb.Extent;
        if (Numeric.IsFinite(extent.X + extent.Y + extent.Z))
        {
          var boxShape = new BoxShape(extent);
          if (mergedAabb.Center.IsNumericallyZero)
            mergedMesh.BoundingShape = boxShape;
          else
            mergedMesh.BoundingShape = new TransformedShape(new GeometricObject(boxShape, new Pose(mergedAabb.Center)));
        }
        else
        {
          mergedMesh.BoundingShape = Shape.Infinite;
        }

        jobs.Sort(MergeJobComparer.Instance);

        MergeSubmeshes(jobs, mergedMesh, vertexDeclarations, vertexCounts, indexCount);

        // Merge occluders.
        if (mesh.Occluder != null)
          mergedMesh.Occluder = MergeOccluders(mesh, scales, poses);

        return mergedMesh;
      }
      catch
      {
        mergedMesh.Dispose();
        throw;
      }
    }


    private static void MergeSubmeshes(List<MergeJob> mergeJobs, Mesh mergedMesh,
                                       List<VertexDeclaration> vertexDeclarations,
                                       List<int> totalVertexCounts, int totalIndexCount)
    {
      var graphicsDevice = mergeJobs[0].Submesh.VertexBuffer.GraphicsDevice;

      VertexBuffer vertexBuffer = null;
      IndexBuffer indexBuffer = null;
      if (totalIndexCount > ushort.MaxValue)
        indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, totalIndexCount, BufferUsage.None);
      else if (totalIndexCount > 0)
        indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, totalIndexCount, BufferUsage.None);

      Submesh submesh = null;
      int vertexDeclarationIndex = -1;
      int vertexCount = 0;
      int indexCount = 0;
      uint sortKey = 0;
      for (int jobIndex = 0; jobIndex < mergeJobs.Count; jobIndex++)
      {
        var job = mergeJobs[jobIndex];

        // As long as the sort key is equal we can merge into the current submesh.
        if (submesh != null && job.SortKey != sortKey)
          submesh = null;

        // We have to begin a new vertex buffer if the vertex declaration has changed.
        if (job.VertexDeclarationIndex != vertexDeclarationIndex)
          vertexBuffer = null;

        if (vertexBuffer == null)
        {
          vertexDeclarationIndex = job.VertexDeclarationIndex;
          vertexCount = 0;
          vertexBuffer = new VertexBuffer(
            graphicsDevice,
            vertexDeclarations[vertexDeclarationIndex],
            totalVertexCounts[vertexDeclarationIndex],
            BufferUsage.None);
        }

        if (submesh == null)
        {
          sortKey = job.SortKey;
          submesh = new Submesh
          {
            VertexBuffer = vertexBuffer,
            StartVertex = vertexCount,
            StartIndex = indexCount,
            PrimitiveType = job.Submesh.PrimitiveType,
            MaterialIndex = job.MergedMaterialIndex,
            IndexBuffer = (job.Submesh.IndexBuffer != null) ? indexBuffer : null,
          };
          mergedMesh.Submeshes.Add(submesh);
        }

        // ----- Merge indices
        if (job.Submesh.IndexBuffer != null)
        {
          Debug.Assert(indexBuffer != null);
          int submeshIndexCount = GetNumberOfIndices(job.Submesh.PrimitiveType, job.Submesh.PrimitiveCount);
          if (job.Submesh.IndexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
          {
            ushort[] indices16 = new ushort[submeshIndexCount];
            job.Submesh.IndexBuffer.GetData(job.Submesh.StartIndex * 2, indices16, 0, submeshIndexCount);

            if (indexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
            {
              for (int i = 0; i < indices16.Length; i++)
                indices16[i] = (ushort)(indices16[i] + submesh.VertexCount);

              indexBuffer.SetData(indexCount * 2, indices16, 0, indices16.Length);
            }
            else
            {
              uint[] indices32 = new uint[submeshIndexCount];
              for (int i = 0; i < indices16.Length; i++)
                indices32[i] = (uint)(indices16[i] + submesh.VertexCount);

              indexBuffer.SetData(indexCount * 4, indices32, 0, indices32.Length);
            }
          }
          else
          {
            uint[] indices32 = new uint[submeshIndexCount];
            job.Submesh.IndexBuffer.GetData(job.Submesh.StartIndex * 4, indices32, 0, submeshIndexCount);

            if (indexBuffer.IndexElementSize == IndexElementSize.SixteenBits)
            {
              ushort[] indices16 = new ushort[submeshIndexCount];
              for (int i = 0; i < indices32.Length; i++)
                indices16[i] = (ushort)(indices32[i] + submesh.VertexCount);

              indexBuffer.SetData(indexCount * 2, indices16, 0, indices16.Length);
            }
            else
            {
              for (int i = 0; i < indices32.Length; i++)
                indices32[i] = (uint)(indices32[i] + submesh.VertexCount);

              indexBuffer.SetData(indexCount * 4, indices32, 0, indices32.Length);
            }
          }

          indexCount += submeshIndexCount;
        }

        // ----- Merge vertices
        var vertexDeclaration = vertexBuffer.VertexDeclaration;
        int vertexStride = vertexDeclaration.VertexStride;

        // Get the whole vertex buffer as byte array.
        byte[] buffer = new byte[job.Submesh.VertexBuffer.VertexCount * vertexStride];
        job.Submesh.VertexBuffer.GetData(buffer);

        // Transform position and normals.
        TransformVertices(buffer, job.Submesh.StartVertex, job.Submesh.VertexCount, vertexDeclaration, job.Scale, job.Pose);

        vertexBuffer.SetData(
          vertexCount * vertexStride,
          buffer,
          job.Submesh.StartVertex * vertexStride,
          job.Submesh.VertexCount * vertexStride,
          1);

        submesh.VertexCount += job.Submesh.VertexCount;
        vertexCount += job.Submesh.VertexCount;
        submesh.PrimitiveCount += job.Submesh.PrimitiveCount;
      }
    }


    // Transforms all Position0/Normal0/Tangent0/Binormal0 elements.
    // TODO: Should we also transform Position1/Tangent2/...?
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Binormal")]
    private static void TransformVertices(byte[] buffer, int startVertex, int vertexCount,
                                          VertexDeclaration vertexDeclaration,
                                          Vector3F scale, Pose pose)
    {
      // If the transform does not have a scale/rotation/translation, we can abort.
      bool hasScale = Vector3F.AreNumericallyEqual(scale, Vector3F.One);
      if (!pose.HasRotation && !pose.HasTranslation && !hasScale)
        return;

      Matrix world = pose;
      world.M11 *= scale.X; world.M12 *= scale.X; world.M13 *= scale.X;
      world.M21 *= scale.Y; world.M22 *= scale.Y; world.M23 *= scale.Y;
      world.M31 *= scale.Z; world.M32 *= scale.Z; world.M33 *= scale.Z;

      Matrix worldInverseTranspose;
      bool scaleIsUniform = (Numeric.AreEqual(scale.X, scale.Y) && Numeric.AreEqual(scale.Y, scale.Z));
      if (scaleIsUniform)
      {
        // With a uniform scale we can use the normal world matrix to transform normals.
        worldInverseTranspose = world;
      }
      else
      {
        // We have to transform normals using the inverse transposed matrix.
        Matrix.Invert(ref world, out worldInverseTranspose);
        Matrix.Transpose(ref worldInverseTranspose, out worldInverseTranspose);
      }

      int vertexStride = vertexDeclaration.VertexStride;

#if WINDOWS || WINDOWS_UWP || XBOX           // Some PCL profiles can use unsafe. Profile328 cannot.
      unsafe
      {
        fixed (byte* pBuffer = buffer)
        {
          foreach (var element in vertexDeclaration.GetVertexElements())
          {
            if (element.UsageIndex > 0)
              continue;

            var usage = element.VertexElementUsage;
            if (usage != VertexElementUsage.Position && usage != VertexElementUsage.Normal
                && usage != VertexElementUsage.Tangent && usage != VertexElementUsage.Binormal)
            {
              continue;
            }

            if (element.VertexElementFormat != VertexElementFormat.Vector3)
              throw new NotSupportedException(
                "Cannot merge meshes. Vertex elements with the semantic Position, Normal, Tangent or Binormal must use format Vector3.");

            int offset = element.Offset;
            if (usage == VertexElementUsage.Position)
            {
              for (int i = 0; i < vertexCount; i++)
              {
                Vector3* pVector3 = (Vector3*)(pBuffer + (startVertex + i) * vertexStride + offset);
                Vector3.Transform(ref *pVector3, ref world, out *pVector3);
              }
            }
            else
            {
              for (int i = 0; i < vertexCount; i++)
              {
                Vector3* pVector3 = (Vector3*)(pBuffer + (startVertex + i) * vertexStride + offset);
                Vector3.TransformNormal(ref *pVector3, ref worldInverseTranspose, out *pVector3);
                (*pVector3).Normalize();
              }
            }
          }
        }
      }
#else
      using (var stream = new MemoryStream(buffer))
      using (var reader = new BinaryReader(stream))
      using (var writer = new BinaryWriter(stream))
      {
        foreach (var element in vertexDeclaration.GetVertexElements())
        {
          if (element.UsageIndex > 0)
            continue;

          var usage = element.VertexElementUsage;
          if (usage != VertexElementUsage.Position && usage != VertexElementUsage.Normal
              && usage != VertexElementUsage.Tangent && usage != VertexElementUsage.Binormal)
          {
            continue;
          }

          if (element.VertexElementFormat != VertexElementFormat.Vector3)
            throw new NotSupportedException(
              "Cannot merge meshes. Vertex elements with the semantic Position, Normal, Tangent or Binormal must use format Vector3.");

          int offset = element.Offset;
          if (usage == VertexElementUsage.Position)
          {
            for (int i = 0; i < vertexCount; i++)
            {
              int startIndex = (startVertex + i) * vertexStride + offset;
              Vector3 vector3 = ReadVector3(reader, startIndex);
              Vector3.Transform(ref vector3, ref world, out vector3);
              WriteVector3(writer, vector3, startIndex);
            }
          }
          else
          {
            for (int i = 0; i < vertexCount; i++)
            {
              int startIndex = (startVertex + i) * vertexStride + offset;
              Vector3 vector3 = ReadVector3(reader, startIndex);
              Vector3.TransformNormal(ref vector3, ref worldInverseTranspose, out vector3);
              vector3.Normalize();
              WriteVector3(writer, vector3, startIndex);
            }
          }
        }
      }
#endif
    }


#if !(WINDOWS || WINDOWS_UWP || XBOX)
    private static Vector3 ReadVector3(BinaryReader reader, int startIndex)
    {
      // Does endianness play a role? - It should not. DirectX vertex and index buffers are little 
      // endian on Windows and Xbox One. They are big endian on Xbox 360. So it seems the buffers 
      // use the endianness of the processor.

      reader.BaseStream.Seek(startIndex, SeekOrigin.Begin);
      Vector3 v = new Vector3(
        reader.ReadSingle(),
        reader.ReadSingle(),
        reader.ReadSingle());

      return v;
    }


    private static void WriteVector3(BinaryWriter writer, Vector3 vector, int startIndex)
    {
      writer.BaseStream.Seek(startIndex, SeekOrigin.Begin);
      writer.Write(vector.X);
      writer.Write(vector.Y);
      writer.Write(vector.Z);
    }
#endif


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Occluders")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "occluders")]
    private static Occluder MergeOccluders(IEnumerable<MeshNode> meshNodes)
    {
      var mergedVertices = new List<Vector3F>();
      var mergedIndices = new List<ushort>();

      foreach (var meshNode in meshNodes)
      {
        if (meshNode == null)
          continue;

        var pose = meshNode.PoseWorld;
        var scale = meshNode.ScaleWorld;
        var mesh = meshNode.Mesh;
        var occluder = mesh.Occluder;

        if (occluder == null)
          continue;

        Vector3F[] vertices = occluder.Vertices;
        ushort[] indices = occluder.Indices;

        if (mergedVertices.Count + vertices.Length > ushort.MaxValue)
          throw new GraphicsException("Cannot merge occluders - too many vertices. Occluders only support 16-bit indices. The total number of vertices must not exceed 65535.");

        int currentVertexCount = mergedVertices.Count;

        // Transform vertices to world space and merge into list.
        if (scale == Vector3F.One)
        {
          for (int i = 0; i < vertices.Length; i++)
            mergedVertices.Add(pose.ToWorldPosition(vertices[i]));
        }
        else
        {
          for (int i = 0; i < vertices.Length; i++)
            mergedVertices.Add(pose.ToWorldPosition(scale * vertices[i]));
        }

        // Add offset to indices and merge into list.
        for (int i = 0; i < indices.Length; i++)
          mergedIndices.Add((ushort)(indices[i] + currentVertexCount));
      }

      return new Occluder(mergedVertices.ToArray(), mergedIndices.ToArray());
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "Occluders")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "occluders")]
    private static Occluder MergeOccluders(Mesh mesh, IList<Vector3F> scales, IList<Pose> poses)
    {
      Debug.Assert(mesh != null);
      Debug.Assert(mesh.Occluder != null);
      Debug.Assert(poses != null);
      Debug.Assert(scales != null);
      Debug.Assert(poses.Count == scales.Count);

      var occluder = mesh.Occluder;
      var mergedVertices = new List<Vector3F>();
      var mergedIndices = new List<ushort>();

      for (int instanceIndex = 0; instanceIndex < poses.Count; instanceIndex++)
      {
        Pose pose = poses[instanceIndex];
        Vector3F scale = scales[instanceIndex];

        Vector3F[] vertices = occluder.Vertices;
        ushort[] indices = occluder.Indices;

        if (mergedVertices.Count + vertices.Length > ushort.MaxValue)
          throw new GraphicsException("Cannot merge occluders - too many vertices. Occluders only support 16-bit indices. The total number of vertices must not exceed 65535.");

        int currentVertexCount = mergedVertices.Count;

        // Transform vertices to world space and merge into list.
        if (scale == Vector3F.One)
        {
          for (int i = 0; i < vertices.Length; i++)
            mergedVertices.Add(pose.ToWorldPosition(vertices[i]));
        }
        else
        {
          for (int i = 0; i < vertices.Length; i++)
            mergedVertices.Add(pose.ToWorldPosition(scale * vertices[i]));
        }

        // Add offset to indices and merge into list.
        for (int i = 0; i < indices.Length; i++)
          mergedIndices.Add((ushort)(indices[i] + currentVertexCount));
      }

      return new Occluder(mergedVertices.ToArray(), mergedIndices.ToArray());
    }
  }
}
