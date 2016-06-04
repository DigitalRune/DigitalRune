using System.Collections.Generic;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;


namespace Samples.Graphics
{
  /// <summary>
  /// The scene query used in the <see cref="WeightedBlendedOITSample"/>.
  /// </summary>
  /// <remarks>
  /// A flag indicates which scene nodes should be rendered as transparent (= alpha blended). These
  /// scene nodes are stored in separate list.
  /// </remarks>
  public class WeightedBlendedOITQuery : ISceneQuery
  {
    public SceneNode ReferenceNode { get; private set; }
    public List<SceneNode> SceneNodes { get; private set; }
    public List<SceneNode> TransparentNodes { get; private set; }


    public WeightedBlendedOITQuery()
    {
      SceneNodes = new List<SceneNode>();
      TransparentNodes = new List<SceneNode>();
    }


    public void Reset()
    {
      ReferenceNode = null;
      SceneNodes.Clear();
      TransparentNodes.Clear();
    }


    public void Set(SceneNode referenceNode, IList<SceneNode> nodes, RenderContext context)
    {
      Reset();
      ReferenceNode = referenceNode;

      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i];
        if ((WboitFlags)node.UserFlags == WboitFlags.Transparent)
          TransparentNodes.Add(node);
        else
          SceneNodes.Add(node);
      }
    }
  }
}
