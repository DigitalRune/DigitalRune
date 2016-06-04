// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  partial class DRModelProcessor
  {
    // Initialize _model.
    private void BuildSceneGraph()
    {
      var root = BuildSceneGraph(_input, null);
      if (root == null)
      {
        // Just for safety. (Should not occur in practice.)
        throw new InvalidOperationException("Invalid root node.");
      }

      _model = new DRModelNodeContent();

      // In most cases the root node is an empty node, which can be ignored.
      if (root.GetType() == typeof(DRSceneNodeContent)
          && root.PoseLocal == Pose.Identity
          && root.ScaleLocal == Vector3F.One
          && root.UserData == null)
      {
        // Throw away root, only use children.
        if (root.Children != null)
        {
          _model.Children = root.Children;
          foreach (var child in _model.Children)
            child.Parent = _model;
        }
      }
      else
      {
        _model.Children = new List<DRSceneNodeContent> { root };
        root.Parent = _model;
      }

      // Go through scene graph and update PoseWorld from the root to the leaves.
      _model.PoseWorld = _model.PoseLocal;
      foreach (var node in _model.GetDescendants())
        node.PoseWorld = node.Parent.PoseWorld * node.PoseLocal;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    private static DRSceneNodeContent BuildSceneGraph(NodeContent node, DRSceneNodeContent parent)
    {
      CheckForCyclicReferences(node);

      DRSceneNodeContent sceneNode;
      if (node is BoneContent)
      {
        // ----- BoneContent
        // Bones do not show up in the scene graph.
        sceneNode = null;
      }
      else if (node is MeshContent)
      {
        // ----- MeshContent
        var mesh = (MeshContent)node;
        string morphTargetName;
        if (ContentHelper.IsMorphTarget(mesh, out morphTargetName))
        {
          // ----- Morph Targets
          // Morph targets are stored in the parent mesh, they do not show up in
          // the scene graph. Children of morph targets are ignored!
          mesh.Name = morphTargetName;
          AddMorphTarget(parent, mesh);
          sceneNode = null;
        }
        else if (ContentHelper.IsOccluder(mesh))
        {
          // ----- OccluderNode
          sceneNode = new DROccluderNodeContent { InputMesh = mesh };
        }
        else
        {
          // ----- MeshNode
          sceneNode = new DRMeshNodeContent { InputMesh = mesh };
        }
      }
      else
      {
        // ----- Empty/unsupported node
        sceneNode = new DRSceneNodeContent();
      }

      if (sceneNode != null)
      {
        sceneNode.Name = node.Name;
        Pose pose;
        Vector3F scale;
        DecomposeTransform(node, out pose, out scale);
        sceneNode.PoseLocal = pose;
        sceneNode.ScaleLocal = scale;
        if (node.Children.Count > 0)
        {
          // Process children.
          sceneNode.Children = new List<DRSceneNodeContent>();

          // Recursively add children.
          foreach (var childNode in node.Children)
          {
            var childSceneNode = BuildSceneGraph(childNode, sceneNode);
            if (childSceneNode != null)
            {
              childSceneNode.Parent = sceneNode;
              sceneNode.Children.Add(childSceneNode);
            }
          }
        }
      }

      return sceneNode;
    }


    // Check for unallowed cyclic references.
    private static void CheckForCyclicReferences(NodeContent node)
    {
      if (TreeHelper.GetAncestors(node, n => n.Parent).Contains(node))
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cyclic reference (node \"{0}\") found in node hierarchy.",
          node);
        throw new InvalidOperationException(message);
      }
    }


    private static void DecomposeTransform(NodeContent node, out Pose pose, out Vector3F scale)
    {
      // Get local transform of node.
      Matrix44F transform = (Matrix44F)node.Transform;

      // Decompose transform into scale, rotation, and translation.
      Matrix33F rotation;
      Vector3F translation;
      transform.Decompose(out scale, out rotation, out translation);

      // Return pose (position + orientation).
      pose = new Pose(translation, rotation);
    }
  }
}
