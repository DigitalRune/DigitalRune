// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Geometry;
using DigitalRune.Linq;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Stores the processed data for a <strong>SceneNode</strong> asset.
  /// </summary>
  public class DRSceneNodeContent : INamedObject
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the parent of this node.
    /// </summary>
    /// <value>The parent of this node.</value>
    public DRSceneNodeContent Parent { get; set; }


    /// <summary>
    /// Gets or sets the children of this node.
    /// </summary>
    /// <value>The children of this node.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
    public List<DRSceneNodeContent> Children { get; set; }


    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the pose (position and orientation) relative to the parent node.
    /// </summary>
    /// <value>The pose (position and orientation) relative to the parent node.</value>
    public Pose PoseLocal { get; set; }


    /// <summary>
    /// Gets or sets the scale.
    /// </summary>
    /// <value>The scale.</value>
    public Vector3F ScaleLocal { get; set; }


    /// <summary>
    /// Gets or sets the pose (position and orientation) relative to world space.
    /// </summary>
    /// <value>The pose (position and orientation) relative to world space.</value>
    public Pose PoseWorld { get; set; }


    /// <summary>
    /// Gets or sets the maximum distance up to which the scene node is rendered.
    /// </summary>
    /// <value>The <i>view-normalized</i> distance. The default value is 0 (= no limit).</value>
    public float MaxDistance { get; set; }


    /// <summary>
    /// Gets or sets the LOD level.
    /// </summary>
    /// <value>The LOD level. The default value is 0.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public int LodLevel { get; set; }       // Only relevant for DRLodGroupNodeContent.


    /// <summary>
    /// Gets or sets the LOD distance.
    /// </summary>
    /// <value>The LOD distance. The default value is 0.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public float LodDistance { get; set; }  // Only relevant for DRLodGroupNodeContent.


    /// <summary>
    /// Gets or sets a user-defined tag object.
    /// </summary>
    /// <value>User-defined tag object.</value>
    [ContentSerializer(ElementName = "UserData", SharedResource = true)]
    public object UserData { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DRSceneNodeContent"/> class.
    /// </summary>
    public DRSceneNodeContent()
    {
      PoseLocal = Pose.Identity;
      ScaleLocal = Vector3F.One;
      PoseWorld = Pose.Identity;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Traversal -----

    /// <summary>
    /// Gets the children of the given scene node.
    /// </summary>
    /// <returns>
    /// The children of scene node or an empty <see cref="IEnumerable{T}"/> if 
    /// <see cref="DRSceneNodeContent.Children"/> is <see langword="null"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public IEnumerable<DRSceneNodeContent> GetChildren()
    {
      return Children ?? LinqHelper.Empty<DRSceneNodeContent>();
    }


    /// <summary>
    /// Gets the root node.
    /// </summary>
    /// <returns>The root node.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public DRSceneNodeContent GetRoot()
    {
      var node = this;
      while (node.Parent != null)
        node = node.Parent;

      return node;
    }


    /// <summary>
    /// Gets the ancestors of the given scene node.
    /// </summary>
    /// <returns>The ancestors of this scene node.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public IEnumerable<DRSceneNodeContent> GetAncestors()
    {
      return TreeHelper.GetAncestors(this, node => node.Parent);
    }


    /// <summary>
    /// Gets the scene node and its ancestors scene.
    /// </summary>
    /// <returns>The scene node and its ancestors of the scene.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public IEnumerable<DRSceneNodeContent> GetSelfAndAncestors()
    {
      return TreeHelper.GetSelfAndAncestors(this, node => node.Parent);
    }


    /// <overloads>
    /// <summary>
    /// Gets the descendants of the given node.
    /// </summary>
    /// </overloads>
    /// <summary>
    /// Gets the descendants of the given node using a depth-first search.
    /// </summary>
    /// <returns>
    /// The descendants of this node in depth-first order.
    /// </returns>
    public IEnumerable<DRSceneNodeContent> GetDescendants()
    {
      return TreeHelper.GetDescendants(this, node => node.GetChildren(), true);
    }


    /// <summary>
    /// Gets the descendants of the given node using a depth-first or a breadth-first search.
    /// </summary>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>
    /// The descendants of this node.
    /// </returns>
    public IEnumerable<DRSceneNodeContent> GetDescendants(bool depthFirst)
    {
      return TreeHelper.GetDescendants(this, node => node.GetChildren(), depthFirst);
    }


    /// <overloads>
    /// <summary>
    /// Gets the subtree (the given node and all of its descendants).
    /// </summary>
    /// </overloads>
    /// <summary>
    /// Gets the subtree (the given node and all of its descendants) using a depth-first 
    /// search.
    /// </summary>
    /// <returns>
    /// The subtree (the given node and all of its descendants) in depth-first order.
    /// </returns>
    public IEnumerable<DRSceneNodeContent> GetSubtree()
    {
      return TreeHelper.GetSubtree(this, node => node.GetChildren(), true);
    }


    /// <summary>
    /// Gets the subtree (the given node and all of its descendants) using a depth-first or a 
    /// breadth-first search.
    /// </summary>
    /// <param name="depthFirst">
    /// If set to <see langword="true"/> then a depth-first search for descendants will be made; 
    /// otherwise a breadth-first search will be made.
    /// </param>
    /// <returns>
    /// The subtree (the given node and all of its descendants).
    /// </returns>
    public IEnumerable<DRSceneNodeContent> GetSubtree(bool depthFirst)
    {
      return TreeHelper.GetSubtree(this, node => node.GetChildren(), depthFirst);
    }


    /// <summary>
    /// Gets the leaves of the scene node.
    /// </summary>
    /// <returns>The leaves of the scene node.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
    public IEnumerable<DRSceneNodeContent> GetLeaves()
    {
      return TreeHelper.GetLeaves(this, node => node.GetChildren());
    }
    #endregion
    
    #endregion
  }
}
