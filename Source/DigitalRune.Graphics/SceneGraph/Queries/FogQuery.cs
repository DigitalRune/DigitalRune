// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Returns the <see cref="FogNodes"/> that affect a specific scene node.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="FogQuery"/> can be executed against a scene by calling 
  /// <see cref="IScene.Query{T}"/>. The query can be used to get all <see cref="FogNodes"/> that
  /// affect the current a certain reference node in the scene. The reference node is typically 
  /// the current <see cref="CameraNode"/>.
  /// </para>
  /// <para>
  /// The <see cref="FogNodes"/> are sorted by their <see cref="FogNode.Priority"/> (descending),
  /// which means that the first node in the list is the most important fog effect.
  /// </para>
  /// </remarks>
  public class FogQuery : ISceneQuery
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public SceneNode ReferenceNode { get; private set; }


    /// <summary>
    /// Gets the fog nodes.
    /// </summary>
    /// <value>The fog nodes.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<FogNode> FogNodes { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FogQuery"/> class.
    /// </summary>
    public FogQuery()
    {
      FogNodes = new List<FogNode>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public void Reset()
    {
      ReferenceNode = null;
      FogNodes.Clear();
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public void Set(SceneNode referenceNode, IList<SceneNode> nodes, RenderContext context)
    {
      Reset();
      ReferenceNode = referenceNode;

      int numberOfNodes = nodes.Count;
      for (int i = 0; i < numberOfNodes; i++)
      {
        var fogNode = nodes[i] as FogNode;
        if (fogNode != null)
        {
          Debug.Assert(fogNode.ActualIsEnabled, "Scene query contains disabled nodes.");
          FogNodes.Add(fogNode);
        }
      }

      // Sort fog nodes.
      FogNodes.Sort(DescendingFogNodeComparer.Instance);
    }
    #endregion
  }
}
