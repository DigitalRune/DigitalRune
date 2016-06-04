using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Graphics;
using DigitalRune.Graphics.SceneGraph;


namespace Samples.Graphics
{
  /// <summary>
  /// Returns the scene nodes that touch a specific reference scene node (usually the 
  /// <see cref="CameraNode"/>). The query is aware of <see cref="ProxyNode"/>s and automatically
  /// resolves the referenced nodes.
  /// </summary>
  internal class ProxyAwareQuery : ISceneQuery
  {
    /// <inheritdoc/>
    public SceneNode ReferenceNode { get; private set; }


    /// <summary>
    /// Gets the scene nodes that touch the <see cref="ReferenceNode"/>.
    /// </summary>
    /// <value>The scene nodes that touch the <see cref="ReferenceNode"/>.</value>
    public List<SceneNode> SceneNodes { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="ProxyAwareQuery"/> class.
    /// </summary>
    public ProxyAwareQuery()
    {
      SceneNodes = new List<SceneNode>();
    }


    /// <inheritdoc/>
    public void Reset()
    {
      ReferenceNode = null;
      SceneNodes.Clear();
    }


    /// <inheritdoc/>
    public void Set(SceneNode referenceNode, IList<SceneNode> nodes, RenderContext context)
    {
      Reset();
      ReferenceNode = referenceNode;

      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var node = nodes[i];
        Debug.Assert(node.ActualIsEnabled, "Scene query contains disabled nodes.");

        AddNode(node);
      }
    }


    private void AddNode(SceneNode node)
    {
      var proxyNode = node as ProxyNode;
      if (proxyNode != null)
      {
        if (proxyNode.Node != null)
          AddSubtree(proxyNode.Node);
      }
      else
      {
        SceneNodes.Add(node);
      }
    }


    private void AddSubtree(SceneNode node)
    {
      if (node.IsEnabled)
      {
        AddNode(node);
        if (node.Children != null)
          foreach (var childNode in node.Children)
            AddSubtree(childNode);
      }
    }
  }
}
