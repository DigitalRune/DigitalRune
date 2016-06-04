// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;
using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.Content.Pipeline
{
  partial class DRModelProcessor
  {
    private void BuildOccluders()
    {
      // Build occluders.
      var occluderNodes = _model.GetSubtree()
                                .OfType<DROccluderNodeContent>()
                                .ToList();

      foreach (var occluderNode in occluderNodes)
        BuildOccluder(occluderNode);

      foreach (var occluderNode in occluderNodes)
        ValidateOccluder(occluderNode);

      // If possible, assign occluder to parent mesh and remove occluder node.
      foreach (var occluderNode in occluderNodes)
      {
        if (occluderNode.Parent is DRMeshNodeContent
            && Pose.AreNumericallyEqual(occluderNode.PoseLocal, Pose.Identity)
            && Vector3F.AreNumericallyEqual(occluderNode.ScaleLocal, Vector3F.One)
            && (occluderNode.Children == null || occluderNode.Children.Count == 0))
        {
          var meshNode = (DRMeshNodeContent)occluderNode.Parent;
          meshNode.Mesh.Occluder = occluderNode.Occluder;
          meshNode.Children.Remove(occluderNode);
        }
      }
    }


    private /*static*/ void BuildOccluder(DROccluderNodeContent occluderNode)
    {
      var mesh = occluderNode.InputMesh;

      MergeDuplicatePositions(mesh, Numeric.EpsilonF);

      // Get all positions in mesh.
      var meshPositions = mesh.Positions.Select(p => (Vector3F)p).ToList();

      // Get all triangles in mesh.
      var meshIndices = new List<int>();
      foreach (var geometry in mesh.Geometry)
      {
        var positionIndices = geometry.Vertices.PositionIndices;
        var indices = geometry.Indices;
        for (int i = 0; i < indices.Count; i++)
          meshIndices.Add(positionIndices[indices[i]]);
      }

      OptimizeForCache(meshPositions, meshIndices, mesh.Identity);

      occluderNode.Occluder = new DROccluderContent
      {
        Vertices = meshPositions,
        Indices = meshIndices
      };
    }
  }
}
