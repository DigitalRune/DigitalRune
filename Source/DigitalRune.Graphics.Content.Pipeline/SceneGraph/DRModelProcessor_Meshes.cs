// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;


namespace DigitalRune.Graphics.Content.Pipeline
{
  partial class DRModelProcessor
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private class SubmeshInfo
    {
      public GeometryContent Geometry;
      public int OriginalIndex;                 // The index of the Submesh/GeometryContent in the owning mesh.
      public VertexBufferContent VertexBuffer;
      public int VertexBufferIndex;             // Index into _vertexBuffers of the processor.
      public List<DRMorphTargetContent> MorphTargets;
      public object Material;                   // The XML file (string) or the local material (MaterialContent).
      public int MaterialIndex;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    private class SubmeshInfoComparer : Singleton<SubmeshInfoComparer>, IComparer<SubmeshInfo>
    {
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
      public int Compare(SubmeshInfo x, SubmeshInfo y)
      {
        int result = x.VertexBufferIndex - y.VertexBufferIndex;

        if (result == 0)
          result = x.MaterialIndex - y.MaterialIndex;

        if (result == 0)
          result = x.OriginalIndex - y.OriginalIndex;

        return result;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /*
    /// <summary>
    /// Gets or sets the value of the <strong>Generate Tangent Frames</strong> processor parameter.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if binormals and tangents should be generated if none are found; 
    /// otherwise <see langword="false"/>.
    /// </value>
    [DefaultValue(false)]
    [DisplayName("Generate Tangent Frames")]
    [Description("If enabled, binormals and tangents are generated if none were found; otherwise, any existing data remains unchanged.")]
    public virtual bool GenerateTangentFrames { get; set; }


    /// <summary>
    /// Gets or sets the value of the <strong>Swap Winding Order</strong> processor parameter.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the winding order of the model should be swapped; otherwise 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This is useful for models that appear to be drawn inside-out.
    /// </remarks>
    [DefaultValue(false)]
    [DisplayName("Swap Winding Order")]
    [Description("If enabled, the winding order of the model is swapped. Useful for models that appear to be drawn inside-out.")]
    public virtual bool SwapWindingOrder { get; set; }


    /// <summary>
    /// Gets or sets the value of the <strong>AABB Enabled</strong> processor parameter.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the model's AABB should be computed using 
    /// <see cref="AabbMinimum"/> and <see cref="AabbMaximum"/>; otherwise <see langword="false"/>. 
    /// </value>
    /// <remarks>
    /// The bounding shape of the model is usually computed automatically, but mesh animation (mesh 
    /// skinning, ragdolls, inverse kinematics, etc.) are not taken into account. The properties 
    /// <see cref="AabbEnabled"/>, <see cref="AabbMinimum"/>, and <see cref="AabbMaximum"/> can be 
    /// used to manually define a larger bounding shape which includes all possible animated states.
    /// </remarks>
    [DefaultValue(false)]
    [DisplayName("AABB Enabled")]
    [Description("If enabled, 'AABB Minimum' and 'AABB Maximum' can be used to specify a custom bounding shape.")]
    public virtual bool AabbEnabled { get; set; }


    /// <summary>
    /// Gets or sets the minimum corner of the model's AABB.
    /// </summary>
    /// <value>
    /// The minimum corner of the model's AABB. The AABB is ignored if any component is NaN.
    /// </value>
    /// <inheritdoc cref="AabbEnabled"/>
    [DisplayName("AABB Minimum"), Description("The minimum corner for the axis-aligned bounding box of the whole model; for example, \"-2;-1;-2\". Only used if 'AABB Enabled' is true.")]
    public virtual Vector3 AabbMinimum { get; set; }


    /// <summary>
    /// Gets or sets the maximum corner of the model's AABB.
    /// </summary>
    /// <value>
    /// The maximum corner of the model's AABB. The AABB is ignored if any component is NaN.
    /// </value>
    /// <inheritdoc cref="AabbEnabled"/>
    [DisplayName("AABB Maximum"), Description("The maximum corner for the axis-aligned bounding box of the whole model; for example, \"2;3;2\". Only used if 'AABB Enabled' is true.")]
    public virtual Vector3 AabbMaximum { get; set; }
    */
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    private void BuildMeshes()
    {
      _vertexBuffers = new List<VertexBufferContent>();
      _indices = new IndexCollection();
      _morphTargetVertexBuffer = CreateMorphTargetVertexBuffer();
      _materials = new Dictionary<object, object>();

      var meshNodes = _model.GetSubtree().OfType<DRMeshNodeContent>();
      foreach (var meshNode in meshNodes)
        BuildMesh(meshNode);
    }


    private void BuildMesh(DRMeshNodeContent meshNode)
    {
      var mesh = meshNode.InputMesh;
      var meshDescription = (_modelDescription != null) ? _modelDescription.GetMeshDescription(mesh.Name) : null;

      // Before modifying the base mesh: Prepare morph targets.
      bool hasMorphTargets = (meshNode.InputMorphTargets != null && meshNode.InputMorphTargets.Count > 0);
      if (hasMorphTargets)
      {
        // Convert absolute morph targets to relative morph targets ("delta blend shapes").
        MakeRelativeMorphTargets(mesh, meshNode.InputMorphTargets);

        // Add "VertexReorder" channel to base mesh.
        AddVertexReorderChannel(mesh);
      }

      if (meshDescription != null)
      {
        meshNode.MaxDistance = meshDescription.MaxDistance;
        meshNode.LodDistance = meshDescription.LodDistance;
      }
      else if (_modelDescription != null)
      {
        meshNode.MaxDistance = _modelDescription.MaxDistance;
        meshNode.LodDistance = 0;
      }

      // Ensure that model has tangents and binormals if required.
       AddTangentFrames(mesh, _modelDescription, meshDescription);

      // Process vertex colors, bone weights and bone indices.
      ProcessVertexChannels(mesh);

      if (_modelDescription != null && _modelDescription.SwapWindingOrder)
        MeshHelper.SwapWindingOrder(mesh);

      OptimizeForCache(mesh);

      if (hasMorphTargets)
      {
        // Get the vertex reorder maps for matching the morph targets with the
        // base mesh. (Removes the "VertexReorder" channel from the geometry.)
        _vertexReorderMaps = GetVertexReorderMaps(mesh);
      }

      var submeshInfos = BuildSubmeshInfos(mesh, meshNode.InputMorphTargets);

      // Sort submeshes by vertex declaration, material, and original index.
      Array.Sort(submeshInfos, SubmeshInfoComparer.Instance);

      // Build submeshes (including materials).
      var submeshes = BuildSubmeshes(mesh, submeshInfos);

      // Calculate a bounding shape for the whole mesh.
      var boundingShape = BuildBoundingShape(meshNode);

      meshNode.Mesh = new DRMeshContent
      {
        Name = mesh.Name,
        BoundingShape = boundingShape,
        Submeshes = submeshes,
#if ANIMATION
        Skeleton = _skeleton,
        Animations = _animations,
#endif
      };
    }


    private void AddTangentFrames(MeshContent mesh, ModelDescription modelDescription, MeshDescription meshDescription)
    {
      string textureCoordinateChannelName = VertexChannelNames.TextureCoordinate(0);
      string tangentChannelName = VertexChannelNames.Tangent(0);
      string binormalChannelName = VertexChannelNames.Binormal(0);

      bool normalsCalculated = false;
      for (int i = 0; i < mesh.Geometry.Count; i++)
      {
        var geometry = mesh.Geometry[i];

        // Check whether tangent frames are required.
        var submeshDescription = (meshDescription != null) ? meshDescription.GetSubmeshDescription(i) : null;
        if (submeshDescription != null && submeshDescription.GenerateTangentFrames
            || meshDescription != null && meshDescription.GenerateTangentFrames
            || modelDescription != null && modelDescription.GenerateTangentFrames)
        {
          // Ensure that normals are set.
          if (!normalsCalculated)
          {
            CalculateNormals(mesh, false);
            normalsCalculated = true;
          }

          var channels = geometry.Vertices.Channels;
          bool tangentsMissing = !channels.Contains(tangentChannelName);
          bool binormalsMissing = !channels.Contains(binormalChannelName);
          if (tangentsMissing || binormalsMissing)
          {
            // Texture coordinates are required for calculating tangent frames.
            if (!channels.Contains(textureCoordinateChannelName))
            {
              _context.Logger.LogWarning(
                null, mesh.Identity,
                "Texture coordinates missing in mesh '{0}', submesh {1}. Texture coordinates are required " +
                "for calculating tangent frames.",
                mesh.Name, i);

              channels.Add<Vector2>(textureCoordinateChannelName, null);
            }

            CalculateTangentFrames(
              geometry,
              textureCoordinateChannelName,
              tangentsMissing ? tangentChannelName : null,
              binormalsMissing ? binormalChannelName : null);
          }
        }
      }
    }


    // Build a SubmeshInfo for each GeometryContent.
    private SubmeshInfo[] BuildSubmeshInfos(MeshContent mesh, List<MeshContent> inputMorphs)
    {
      bool hasMorphTargets = (inputMorphs != null && inputMorphs.Count > 0);

      // A lookup table that maps each material to its index.
      // The key is the name of the XML file (string) or the local material (MaterialContent).
      var materialLookupTable = new Dictionary<object, int>();

      int numberOfSubmeshes = mesh.Geometry.Count;
      var submeshInfos = new SubmeshInfo[numberOfSubmeshes];
      for (int i = 0; i < numberOfSubmeshes; i++)
      {
        var geometry = mesh.Geometry[i];

        // Build morph targets for current submesh.
        List<DRMorphTargetContent> morphTargets = null;
        if (hasMorphTargets)
        {
          morphTargets = BuildMorphTargets(geometry, inputMorphs, i);
          if (morphTargets != null && morphTargets.Count > 0)
          {
            // When morph targets are used remove the "BINORMAL" channel. (Otherwise,
            // the number of vertex attributes would exceed the limit. Binormals need
            // to be reconstructed from normal and tangent in the vertex shader.)
            string binormalName = VertexChannelNames.Binormal(0);
            bool containsTangentFrames = geometry.Vertices.Channels.Remove(binormalName);

            if (containsTangentFrames)
            {
              // A submesh cannot use vertex colors and tangents at the same time.
              // This would also exceed the vertex attribute limit.
              string colorName = VertexChannelNames.Color(0);
              if (geometry.Vertices.Channels.Contains(colorName))
                geometry.Vertices.Channels.Remove(colorName);
            }
          }
        }

        var submeshInfo = new SubmeshInfo
        {
          Geometry = geometry,
          OriginalIndex = i,
          VertexBuffer = geometry.Vertices.CreateVertexBuffer(),
          MorphTargets = morphTargets
        };
        submeshInfo.VertexBufferIndex = GetVertexBufferIndex(submeshInfo.VertexBuffer.VertexDeclaration);

        // Get material file or local material.
        object material = (object)GetExternalMaterial(mesh, geometry) ?? geometry.Material;
        if (material == null)
        {
          var message = string.Format(CultureInfo.InvariantCulture, "Mesh \"{0}\" does not have a material.", mesh);
          throw new InvalidContentException(message, mesh.Identity);
        }

        int materialIndex;
        if (!materialLookupTable.TryGetValue(material, out materialIndex))
        {
          materialIndex = materialLookupTable.Count;
          materialLookupTable.Add(material, materialIndex);
        }

        submeshInfo.MaterialIndex = materialIndex;
        submeshInfo.Material = material;

        submeshInfos[i] = submeshInfo;
      }

      return submeshInfos;
    }


    // Returns the index of the vertex buffer (in _vertexBuffers) for the given vertex declaration.
    // If there is no matching vertex buffer, a new vertex buffer is added to _vertexBuffers.
    private int GetVertexBufferIndex(VertexDeclarationContent vertexDeclaration)
    {
      for (int i = 0; i < _vertexBuffers.Count; i++)
      {
        VertexDeclarationContent otherVertexDeclaration = _vertexBuffers[i].VertexDeclaration;

        // Compare vertex element count.
        if ((otherVertexDeclaration.VertexElements.Count != vertexDeclaration.VertexElements.Count))
          continue;

        int? vertexStride = vertexDeclaration.VertexStride;
        int? otherVertexStride = otherVertexDeclaration.VertexStride;

        // Compare vertex strides.
        if (vertexStride.GetValueOrDefault() != otherVertexStride.GetValueOrDefault())
          continue;

        // Check if either both have a vertex stride or not.
        if (vertexStride.HasValue == otherVertexStride.HasValue)
          continue;

        // Compare each vertex element structure.
        bool matchFound = true;
        for (int j = 0; j < otherVertexDeclaration.VertexElements.Count; j++)
        {
          if (vertexDeclaration.VertexElements[j] != otherVertexDeclaration.VertexElements[j])
          {
            matchFound = false;
            break;
          }
        }

        if (matchFound)
          return i;
      }

      // An identical vertex declaration has not been found.
      // --> Add vertex declaration to list.
      _vertexBuffers.Add(new VertexBufferContent { VertexDeclaration = vertexDeclaration });

      return _vertexBuffers.Count - 1;
    }


    private List<DRSubmeshContent> BuildSubmeshes(MeshContent mesh, SubmeshInfo[] submeshInfos)
    {
      var submeshes = new List<DRSubmeshContent>(mesh.Geometry.Count);
      for (int i = 0; i < submeshInfos.Length; i++)
      {
        var submeshInfo = submeshInfos[i];
        var geometry = submeshInfo.Geometry;

        // Append vertices to one of the _vertexBuffers.
        VertexBufferContent vertexBuffer = null;
        int vertexCount = 0;
        int vertexOffset = 0;
        if (submeshInfo.VertexBuffer.VertexData.Length > 0)
        {
          vertexBuffer = _vertexBuffers[submeshInfo.VertexBufferIndex];
          if (!vertexBuffer.VertexDeclaration.VertexStride.HasValue)
          {
            string message = string.Format(CultureInfo.InvariantCulture, "Vertex declaration of \"{0}\" does not have a vertex stride.", mesh);
            throw new InvalidContentException(message, mesh.Identity);
          }

          vertexCount = submeshInfo.Geometry.Vertices.VertexCount;
          vertexOffset = vertexBuffer.VertexData.Length / vertexBuffer.VertexDeclaration.VertexStride.Value;
          vertexBuffer.Write(vertexBuffer.VertexData.Length, 1, submeshInfo.VertexBuffer.VertexData);
        }

        // Append indices to _indices.
        int startIndex = 0;
        int primitiveCount = 0;
        if (geometry.Indices.Count > 0)
        {
          startIndex = _indices.Count;
          primitiveCount = geometry.Indices.Count / 3;
          _indices.AddRange(geometry.Indices);
        }

        // Build material.
        object material = BuildMaterial(submeshInfo.Material);

        // Create Submesh.
        DRSubmeshContent submesh = new DRSubmeshContent
        {
          IndexBuffer = _indices,
          VertexCount = vertexCount,
          StartIndex = startIndex,
          PrimitiveCount = primitiveCount,
          VertexBuffer = vertexBuffer,
          StartVertex = vertexOffset,
          MorphTargets = submeshInfo.MorphTargets,
          ExternalMaterial = material as ExternalReference<DRMaterialContent>,
          LocalMaterial = material as DRMaterialContent,
        };
        submeshes.Add(submesh);
      }
      return submeshes;
    }


    private Shape BuildBoundingShape(DRMeshNodeContent meshNode)
    {
      Shape boundingShape = Shape.Empty;

      var mesh = meshNode.InputMesh;
      if (mesh.Positions.Count > 0)
      {
        if (_modelDescription != null && _modelDescription.AabbEnabled)
        {
          // We assume that the AABB is given in the local space.
          Vector3F aabbMinimum = (Vector3F)_modelDescription.AabbMinimum;
          Vector3F aabbMaximum = (Vector3F)_modelDescription.AabbMaximum;
          Vector3F center = (aabbMaximum + aabbMinimum) / 2;
          Vector3F extent = aabbMaximum - aabbMinimum;
          if (center.IsNumericallyZero)
            boundingShape = new BoxShape(extent);
          else
            boundingShape = new TransformedShape(new GeometricObject(new BoxShape(extent), new Pose(center)));
        }
        else
        {
          // Best fit bounding shape.
          //boundingShape = ComputeBestFitBoundingShape(mesh);

          // Non-rotated bounding shape. This is usually larger but contains no rotations. 
          // (TransformedShapes with rotated children cannot be used with non-uniform scaling.)
          boundingShape = ComputeAxisAlignedBoundingShape(mesh);
        }
      }

      return boundingShape;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    private static Shape ComputeBestFitBoundingShape(MeshContent mesh)
    {
      List<Vector3F> points = mesh.Positions.Select(position => (Vector3F)position).ToList();

      var boundingShape = GeometryHelper.CreateBoundingShape(points);

      //  // Compute minimal sphere.
      //  Vector3F center;
      //  float radius;
      //  GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      //  SphereShape sphere = new SphereShape(radius);
      //  float sphereVolume = sphere.GetVolume();

      //  // Compute minimal box.
      //  Vector3F boxExtent;
      //  Pose boxPose;
      //  GeometryHelper.ComputeBoundingBox(points, out boxExtent, out boxPose);
      //  var box = new BoxShape(boxExtent);
      //  float boxVolume = box.GetVolume();

      //  // Return the object with the smallest volume.
      //  // A TransformedShape is used if the shape needs to be translated or rotated.
      //  if (sphereVolume < boxVolume)
      //  {
      //    if (center.IsNumericallyZero)
      //      boundingShape = sphere;
      //    else
      //      boundingShape = new TransformedShape(new GeometricObject(sphere, new Pose(center)));
      //  }
      //  else
      //  {
      //    if (!boxPose.HasTranslation && !boxPose.HasRotation)
      //      boundingShape = box;
      //    else
      //      boundingShape = new TransformedShape(new GeometricObject(box, boxPose));
      //  }
      //}
      //else
      //{
      //  boundingShape = Shape.Empty;
      //}

      return boundingShape;
    }


    private static Shape ComputeAxisAlignedBoundingShape(MeshContent mesh)
    {
      Debug.Assert(mesh.Positions.Count > 0);

      List<Vector3F> points = mesh.Positions.Select(position => (Vector3F)position).ToList();

      var boundingShape = GeometryHelper.CreateBoundingShape(points);

      // Compute minimal sphere.
      Vector3F center;
      float radius;
      GeometryHelper.ComputeBoundingSphere(points, out radius, out center);
      SphereShape sphere = new SphereShape(radius);
      float sphereVolume = sphere.GetVolume();

      // Compute minimal AABB.
      Aabb aabb = new Aabb(points[0], points[0]);
      for (int i = 1; i < points.Count; i++)
        aabb.Grow(points[i]);
      var boxPose = new Pose(aabb.Center);
      var box = new BoxShape(aabb.Extent);
      float boxVolume = box.GetVolume();

      // Return the object with the smallest volume.
      // A TransformedShape is used if the shape needs to be translated.
      if (sphereVolume < boxVolume)
      {
        if (center.IsNumericallyZero)
          boundingShape = sphere;
        else
          boundingShape = new TransformedShape(new GeometricObject(sphere, new Pose(center)));
      }
      else
      {
        if (!boxPose.HasTranslation)
          boundingShape = box;
        else
          boundingShape = new TransformedShape(new GeometricObject(box, boxPose));
      }

      return boundingShape;
    }
    #endregion
  }
}
