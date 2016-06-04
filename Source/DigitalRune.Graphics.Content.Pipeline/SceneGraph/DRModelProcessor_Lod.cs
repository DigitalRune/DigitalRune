// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Linq;


namespace DigitalRune.Graphics.Content.Pipeline
{
  partial class DRModelProcessor
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Compares scene nodes by LOD distance and LOD level.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    private class LodComparer : Singleton<LodComparer>, IComparer<DRSceneNodeContent>
    {
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
      public int Compare(DRSceneNodeContent x, DRSceneNodeContent y)
      {
        if (x.LodDistance < y.LodDistance)
          return -1;
        if (x.LodDistance > y.LodDistance)
          return +1;
        if (x.LodLevel < y.LodLevel)
          return -1;
        if (x.LodLevel > y.LodLevel)
          return +1;

        return 0;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Traverses the scene graph and collapses all LODs into <strong>LodGroupNodes</strong>.
    /// </summary>
    private void CombineLodGroups()
    {
      // The LOD level is encoded in the scene node name. Example: "MeshXyz_LOD2"
      // --> Collect all LOD levels.
      var lodGroupNodes = new Dictionary<string, DRLodGroupNodeContent>();
      foreach (var node in _model.GetSubtree())
      {
        string name;
        int? lodLevel;
        ContentHelper.ParseSceneNodeName(node.Name, out name, out lodLevel);
        if (lodLevel.HasValue && !string.IsNullOrWhiteSpace(name))
        {
          // Scene node is part of LOD group.
          DRLodGroupNodeContent lodGroupNode;
          if (!lodGroupNodes.TryGetValue(name, out lodGroupNode))
          {
            lodGroupNode = new DRLodGroupNodeContent { Name = name };
            lodGroupNodes.Add(name, lodGroupNode);
          }

          lodGroupNode.Levels.Add(node);
          node.LodLevel = lodLevel.Value;
        }
      }

      // Detach scene nodes that belong to LOD group and replace them with a single LodGroupNode.
      foreach (var lodGroupNode in lodGroupNodes.Values)
      {
        // Find LOD0 (= the scene node that represents the highest level of detail).
        var lod0 = lodGroupNode.Levels.FirstOrDefault(n => n.LodLevel == 0);
        if (lod0 == null)
        {
          // LOD0 is not in list. The scene node does probably not have "_LOD0" in its name.
          // --> Search for scene node without extension "_LOD0".
          lod0 = _model.GetSubtree().FirstOrDefault(n => n.Name == lodGroupNode.Name);
          if (lod0 != null)
          {
            lodGroupNode.Levels.Add(lod0);
            lod0.LodLevel = 0;
          }
        }

        // Note: At this point all LODs should be registered in LOD group.
        lodGroupNode.Levels.Sort(LodComparer.Instance);

        // Set MaxDistance by checking all LOD nodes.
        lodGroupNode.MaxDistance = lodGroupNode.Levels
                                               .SelectMany(n => n.GetSubtree())
                                               .Max(n => n.MaxDistance);

        if (lod0 == null)
        {
          // LOD0 was not found.
          // --> As a fallback use first LOD.
          lod0 = lodGroupNode.Levels[0];
        }

        // Replace LOD0 with LodGroupNode.
        lodGroupNode.ScaleLocal = lod0.ScaleLocal;
        lodGroupNode.PoseLocal = lod0.PoseLocal;
        // We have to set PoseWorld too. PoseWorld/PoseLocal are separate properties in 
        // DRSceneNodeContent, unlike SceneNode where they update each other.
        lodGroupNode.PoseWorld = lod0.PoseWorld;
        lod0.Parent.Children.Add(lodGroupNode);

        // Remove all LODs from scene graph.
        foreach (var lodNode in lodGroupNode.Levels)
          lodNode.Parent.Children.Remove(lodNode);
      }
    }
    #endregion
  }
}
