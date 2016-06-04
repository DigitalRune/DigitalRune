#if !WP7 && !WP8
using System.Collections.Generic;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics;

namespace Samples
{
  // A scene query which sorts the queried nodes into lists as required by the
  // DeferredGraphicsScreen.
  // See class documentation of ISceneQuery for additional information.
  //
  // - LOD is supported: The scene query evaluates LodGroupNodes and adds the 
  //   appropriate level of detail to the result.
  // - LOD blending is not supported: LOD switches are instantaneous.
  //   If you need to implement LOD blending, take a look at
  //   "Samples\Graphics\DeferredRendering\10-LodSample\SceneQueryWithLodBlending.cs"
  public class CustomSceneQuery : ISceneQuery
  {
    public SceneNode ReferenceNode { get; protected set; }

    public List<SceneNode> DecalNodes { get; private set; }
    public List<SceneNode> Lights { get; private set; }
    public List<SceneNode> LensFlareNodes { get; private set; }
    public List<SceneNode> SkyNodes { get; private set; }
    public List<SceneNode> FogNodes { get; private set; }

    // All other scene nodes where IsRenderable is true (e.g. MeshNodes).
    public List<SceneNode> RenderableNodes { get; private set; }


    public CustomSceneQuery()
    {
      DecalNodes = new List<SceneNode>();
      Lights = new List<SceneNode>();
      LensFlareNodes = new List<SceneNode>();
      SkyNodes = new List<SceneNode>();
      FogNodes = new List<SceneNode>();
      RenderableNodes = new List<SceneNode>();
    }


    public void Reset()
    {
      ReferenceNode = null;

      DecalNodes.Clear();
      Lights.Clear();
      LensFlareNodes.Clear();
      SkyNodes.Clear();
      FogNodes.Clear();
      RenderableNodes.Clear();
    }


    public virtual void Set(SceneNode referenceNode, IList<SceneNode> nodes, RenderContext context)
    {
      Reset();
      ReferenceNode = referenceNode;

      if (context.LodCameraNode == null)
      {
        // Simple: No distance culling, no LOD.
        for (int i = 0; i < nodes.Count; i++)
        {
          var node = nodes[i];
          if (node != null)
            AddNode(node);
        }
      }
      else
      {
        // Advanced: Distance culling and LOD selection.
        // If the scene uses LOD, the scene query needs to evaluate the LOD 
        // conditions. The RenderContext.LodCameraNode serves as reference for 
        // distance calculations.
        for (int i = 0; i < nodes.Count; i++)
        {
          var node = nodes[i];
          if (node != null)
            AddNodeEx(node, context);
        }
      }
    }


    // Sorts scene node into correct list.
    protected void AddNode(SceneNode node)
    {
      if (node is DecalNode)
        DecalNodes.Add(node);
      else if (node is LightNode)
        Lights.Add(node);
      else if (node is LensFlareNode)
        LensFlareNodes.Add(node);
      else if (node is SkyNode)
        SkyNodes.Add(node);
      else if (node is FogNode)
        FogNodes.Add(node);
      else if (node.IsRenderable)
        RenderableNodes.Add(node);

      // Unsupported types are simply ignored.
    }


    // Advanced: Distance culling and LOD selection.
    // (This method is virtual because it is overridden in the LodBlendingSample.)
    protected virtual void AddNodeEx(SceneNode node, RenderContext context)
    {
      bool hasMaxDistance = Numeric.IsPositiveFinite(node.MaxDistance);
      var lodGroupNode = node as LodGroupNode;
      bool isLodGroupNode = (lodGroupNode != null);

      float distance = 0;
      if (hasMaxDistance || isLodGroupNode)
      {
        // ----- Calculate view-normalized distance.
        // The view-normalized distance is the distance between scene node and 
        // camera node corrected by the camera field of view. This metric is used
        // for distance culling and LOD selection.
        var cameraNode = context.LodCameraNode;
        distance = GraphicsHelper.GetViewNormalizedDistance(node, cameraNode);

        // Apply LOD bias. (The LOD bias is a factor that can be used to increase
        // or decrease the viewing distance.)
        distance *= cameraNode.LodBias * context.LodBias;
      }

      // ----- Distance Culling: Check whether scene node is within MaxDistance.
      if (hasMaxDistance && distance >= node.MaxDistance)
        return;   // Ignore scene node.

      if (isLodGroupNode)
      {
        // ----- Evaluate LOD group.
        var lodSelection = lodGroupNode.SelectLod(context, distance);
        AddSubtree(lodSelection.Current, context);
      }
      else
      {
        // ----- Handle normal nodes.
        AddNode(node);
      }
    }


    // Adds the scene node including children to the lists.
    protected void AddSubtree(SceneNode node, RenderContext context)
    {
      if (node.IsEnabled)
      {
        AddNodeEx(node, context);

        if (node.Children != null)
          foreach (var childNode in node.Children)
            AddSubtree(childNode, context);
      }
    }
  }
}
#endif
