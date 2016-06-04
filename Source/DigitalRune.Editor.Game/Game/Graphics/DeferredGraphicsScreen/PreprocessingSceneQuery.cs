#if !WP7 && !WP8
using System.Collections.Generic;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;

namespace Samples.Graphics
{
  /// <summary>
  /// Collects all scene nodes which need some work done before the actual scene is rendered:
  /// CloudLayerNodes, WaterNodes, SceneCaptureNodes, PlanarReflectionNodes
  /// </summary>
  public class PreprocessingSceneQuery : ISceneQuery
  {
    public SceneNode ReferenceNode { get; private set; }
    public List<SceneNode> TerrainNodes { get; private set; }
    public List<SceneNode> CloudLayerNodes { get; private set; }
    public List<SceneNode> WaterNodes { get; private set; }
    public List<SceneNode> SceneCaptureNodes { get; private set; }
    public List<SceneNode> PlanarReflectionNodes { get; private set; }


    public PreprocessingSceneQuery()
    {
      TerrainNodes = new List<SceneNode>();
      CloudLayerNodes = new List<SceneNode>();
      WaterNodes = new List<SceneNode>();
      SceneCaptureNodes = new List<SceneNode>();
      PlanarReflectionNodes = new List<SceneNode>();
    }


    public void Reset()
    {
      ReferenceNode = null;
      TerrainNodes.Clear();
      CloudLayerNodes.Clear();
      WaterNodes.Clear();
      SceneCaptureNodes.Clear();
      PlanarReflectionNodes.Clear();
    }


    public void Set(SceneNode referenceNode, IList<SceneNode> nodes, RenderContext context)
    {
      Reset();
      ReferenceNode = referenceNode;

      for (int i = 0; i < nodes.Count; i++)
      {
#if !XBOX360
        if (nodes[i] is TerrainNode)
          TerrainNodes.Add(nodes[i]);
#endif
        if (nodes[i] is CloudLayerNode)
          CloudLayerNodes.Add(nodes[i]);
        else if (nodes[i] is WaterNode)
          WaterNodes.Add(nodes[i]);
        else if (nodes[i] is SceneCaptureNode)
          SceneCaptureNodes.Add(nodes[i]);
        else if (nodes[i] is PlanarReflectionNode)
          PlanarReflectionNodes.Add(nodes[i]);
      }
    }
  }
}
#endif
