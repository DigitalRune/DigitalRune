// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Linq;
using DigitalRune.Linq;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  partial class DRModelProcessor
  {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    private void ValidateInput()
    {
      if (_input is BoneContent)
      {
        // Root node cannot be a BoneContent.
        throw new InvalidContentException("The root node of the model is a bone. - This is not supported.", _input.Identity);
      }

      var tree = TreeHelper.GetSubtree(_input, n => n.Children, true);
      int numberOfSkeletons = 0;

      // Check each node in tree.
      foreach (var node in tree)
      {
        if (node is BoneContent)
        {
          // Each root BoneContent defines a new skeleton.
          if (!(node.Parent is BoneContent))
            numberOfSkeletons++;

          if (numberOfSkeletons > 1)
          {
            // ----- More than one skeleton.
            throw new InvalidContentException("Model contains more than one skeleton. Only one skeleton definition is supported per model.", _input.Identity);
          }
        }
        else
        {
          if (node.Parent is BoneContent)
          {
            // ----- Current node is a NodeContent or a MeshContent under a BoneContent.
            _context.Logger.LogWarning(
              null, _input.Identity,
              "Bone \"{0}\" contains node \"{1}\" which is not a bone. Bones must only have bones as children. The node might be ignored.",
              node.Parent.Name, node.Name);
          }

          var mesh = node as MeshContent;
          if (mesh != null)
          {
            if (ContentHelper.IsMorphTarget(mesh))
              ValidateMorphTarget(mesh);
            else if (ContentHelper.IsOccluder(mesh))
              ValidateOccluder(mesh);
            else
              ValidateMesh(mesh);
          }
        }
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void ValidateMesh(MeshContent mesh)
    {
      foreach (var geometry in mesh.Geometry)
      {
        if (GetExternalMaterial(mesh, geometry) != null)
        {
          // ----- External material.
          // The material is defined in an external XML file!
          // Ignore local material.
          continue;
        }

        // ----- Local material.
        // Submesh uses the material included in the model.
        var material = geometry.Material;
        var channels = geometry.Vertices.Channels;

        // Check if the geometry vertices contain the right number of texture coordinates.
        if (material != null && material.Textures.ContainsKey("Texture"))
        {
          if (!channels.Contains(VertexChannelNames.TextureCoordinate(0)))
          {
            string message = String.Format(
              CultureInfo.InvariantCulture,
              "Model \"{0}\" has texture but no texture coordinates.",
              geometry.Parent.Name);
            throw new InvalidContentException(message, geometry.Identity);
          }
        }

        if (material is DualTextureMaterialContent)
        {
          if (!channels.Contains(VertexChannelNames.TextureCoordinate(1)))
          {
            string message = String.Format(
              CultureInfo.InvariantCulture,
              "Model \"{0}\" uses DualTextureEffect but has only one set of texture coordinates.",
              geometry.Parent.Name);
            throw new InvalidContentException(message, geometry.Identity);
          }
        }

        // Check if the geometry vertices contain blend weights for mesh skinning.
        if (material is SkinnedMaterialContent)
        {
          // If the channel contains "Weights0", then we have a BoneWeightCollection that contains
          // the necessary data.
          if (!channels.Contains(VertexChannelNames.Weights()))
          {
            // Otherwise, we need "BlendIndices0" AND "BlendWeight0".
            var blendIndicesName = VertexChannelNames.EncodeName(VertexElementUsage.BlendIndices, 0);
            var blendWeightsName = VertexChannelNames.EncodeName(VertexElementUsage.BlendWeight, 0);

            if (!channels.Contains(blendIndicesName) || !channels.Contains(blendWeightsName))
            {
              string message = String.Format(
                CultureInfo.InvariantCulture,
                "Model \"{0}\" uses mesh skinning but vertices do not have bone weights.",
                geometry.Parent.Name);
              throw new InvalidContentException(message, geometry.Identity);
            }
          }
        }
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void ValidateMorphTarget(MeshContent morphTarget)
    {
      // Check whether morph target is the child of the base mesh.
      var baseMesh = morphTarget.Parent as MeshContent;
      if (baseMesh == null)
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Morph target \"{0}\" needs to be a child of (linked to) the base mesh.",
          morphTarget.Name);
        throw new InvalidContentException(message, morphTarget.Identity);
      }

      // Check whether number of positions matches.
      if (baseMesh.Positions.Count != morphTarget.Positions.Count)
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Number of positions in morph target \"{0}\" does not match base mesh. (Base mesh: {1}, morph target: {2})",
          morphTarget.Name, baseMesh.Positions.Count, morphTarget.Positions.Count);
        throw new InvalidContentException(message, morphTarget.Identity);
      }

      // Check whether number of submeshes matches.
      int numberOfSubmeshes = baseMesh.Geometry.Count;
      if (numberOfSubmeshes != morphTarget.Geometry.Count)
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Number of submeshes in morph target \"{0}\" does not match base mesh. (Base mesh: {1}, morph target: {2})",
          morphTarget.Name, numberOfSubmeshes, morphTarget.Geometry.Count);
        throw new InvalidContentException(message, morphTarget.Identity);
      }

      for (int i = 0; i < numberOfSubmeshes; i++)
      {
        var baseGeometry = baseMesh.Geometry[i];
        var morphGeometry = morphTarget.Geometry[i];

        // Check whether number of vertices matches.
        if (baseGeometry.Vertices.VertexCount != morphGeometry.Vertices.VertexCount)
        {
          string message = String.Format(
            CultureInfo.InvariantCulture,
            "Number vertices in morph target \"{0}\" does not match base mesh.",
            morphTarget.Name);
          throw new InvalidContentException(message, morphTarget.Identity);
        }

        // Check for "Normal" channel.
        if (!baseGeometry.Vertices.Channels.Contains(VertexChannelNames.Normal()))
        {
          string message = String.Format(
            CultureInfo.InvariantCulture,
            "Normal vectors missing in base mesh \"{0}\".",
            morphTarget.Name);
          throw new InvalidContentException(message, morphTarget.Identity);
        }

        if (!morphGeometry.Vertices.Channels.Contains(VertexChannelNames.Normal()))
        {
          string message = String.Format(
            CultureInfo.InvariantCulture,
            "Normal vectors missing in morph target \"{0}\".",
            morphTarget.Name);
          throw new InvalidContentException(message, morphTarget.Identity);
        }

        int baseNormalsCount = baseGeometry.Vertices.Channels[VertexChannelNames.Normal()].Count;
        int morphNormalsCount = morphGeometry.Vertices.Channels[VertexChannelNames.Normal()].Count;
        if (baseNormalsCount != morphNormalsCount)
        {
          string message = String.Format(
            CultureInfo.InvariantCulture,
            "Number of vertices in morph target \"{0}\" (submesh: {1}) does not match base mesh. (Base mesh: {2}, morph target: {3})",
            morphTarget.Name, i, baseNormalsCount, morphNormalsCount);
          throw new InvalidContentException(message, morphTarget.Identity);
        }

        // Check whether number of indices matches.
        if (baseGeometry.Indices.Count != morphGeometry.Indices.Count)
        {
          string message = String.Format(
            CultureInfo.InvariantCulture,
            "Indices in morph target \"{0}\" do not match base mesh.",
            morphTarget.Name);
          throw new InvalidContentException(message, morphTarget.Identity);
        }
      }

      // Check whether morph target has children.
      if (morphTarget.Children.Count > 0)
      {
        _context.Logger.LogWarning(
          null, _input.Identity,
          "The children of the morph target \"{0}\" will be ignored.",
          morphTarget.Name);
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    private void ValidateOccluder(MeshContent mesh)
    {
      // Optional: Add checks for occluder.
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "LODs")]
    private void ValidateOutput()
    {
      // Note: The material may be defined in an external XML file. The validation 
      // below only checks local materials.

#if ANIMATION
      // Check if SkinnedEffect is used and SkinnedEffect.MaxBones is exceeded.
      if (_skeleton != null && _skeleton.NumberOfBones > SkinnedEffect.MaxBones)
      {
        bool usesSkinnedEffect =
          _model.GetSubtree()
                .OfType<DRMeshNodeContent>()
                .SelectMany(meshNode => meshNode.Mesh.Submeshes)
                .Where(submesh => submesh.ExternalMaterial == null    // Ignore external materials.
                                  && submesh.LocalMaterial != null)
                .SelectMany(submesh => submesh.LocalMaterial.Passes.Values)
                .Any(binding => binding.EffectType == DREffectType.SkinnedEffect);
        if (usesSkinnedEffect)
        {
          var message = string.Format(
            CultureInfo.InvariantCulture,
            "Skeleton has {0} bones, but the maximum supported is {1}.",
            _skeleton.NumberOfBones, SkinnedEffect.MaxBones);
          throw new InvalidContentException(message, _rootBone.Identity);
        }
      }
#endif

      // Check LOD group nodes.
      var lodGroupNodes = _model.GetSubtree().OfType<DRLodGroupNodeContent>();
      foreach (var lodGroupNode in lodGroupNodes)
      {
        if (lodGroupNode.Levels.Count == 1)
        {
          _context.Logger.LogWarning(
            null, _input.Identity,
            "The LOD group \"{0}\" only has a single level of detail.",
            lodGroupNode.Name);

          goto Next;
        }

        for (int i = 0; i < lodGroupNode.Levels.Count - 1; i++)
        {
          for (int j = i + 1; j < lodGroupNode.Levels.Count; j++)
          {
            if (Numeric.AreEqual(lodGroupNode.Levels[i].LodDistance, lodGroupNode.Levels[j].LodDistance))
            {
              _context.Logger.LogWarning(
                null, _input.Identity,
                "Please update the LOD distances of LOD group \"{0}\". Multiple LODs have the same distance.",
                lodGroupNode.Name);

              goto Next;
            }
          }
        }

      Next:
        ;
      }
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void ValidateOccluder(DROccluderNodeContent occluderNode)
    {
      int numberOfVertices = occluderNode.Occluder.Vertices.Count;
      if (numberOfVertices > ushort.MaxValue)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Occluder is too complex: The occluder \"{0}\" has {1} vertices. Max allowed number of vertices is {2}.",
          occluderNode.Name, numberOfVertices, ushort.MaxValue);
        throw new InvalidContentException(message, _input.Identity);
      }
    }
  }
}
