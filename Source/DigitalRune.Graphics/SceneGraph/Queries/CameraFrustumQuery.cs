// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Returns the scene nodes that touch a specific reference scene node (usually the 
  /// <see cref="CameraNode"/>).
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="CameraFrustumQuery"/> can be executed against a scene by calling 
  /// <see cref="IScene.Query{T}"/>. The query can be used to get all scene nodes in a scene that 
  /// touch a certain reference node. For example: This query is typically used for <i>frustum 
  /// culling</i> to get all meshes and lights inside the camera frustum. The reference node in this
  /// example is the camera node.
  /// </para>
  /// <para>
  /// This scene query does not evaluate <see cref="LodGroupNode"/>s, i.e. the LOD conditions are 
  /// not evaluated. <see cref="LodGroupNode"/>s are simply added to <see cref="SceneNodes"/> 
  /// collection when they touch the reference node. 
  /// </para>
  /// </remarks>
  public class CameraFrustumQuery : ISceneQuery
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
    /// Gets the scene nodes that touch the <see cref="ReferenceNode"/>.
    /// </summary>
    /// <value>The scene nodes that touch the <see cref="ReferenceNode"/>.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<SceneNode> SceneNodes { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CameraFrustumQuery"/> class.
    /// </summary>
    public CameraFrustumQuery()
    {
      SceneNodes = new List<SceneNode>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public void Reset()
    {
      ReferenceNode = null;
      SceneNodes.Clear();
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
        var node = nodes[i];
        Debug.Assert(node.ActualIsEnabled, "Scene query contains disabled nodes.");
        SceneNodes.Add(node);
      }
    }
    #endregion
  }
}
