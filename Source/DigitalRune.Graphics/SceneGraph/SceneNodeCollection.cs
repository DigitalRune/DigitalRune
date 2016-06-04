// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Manages a collection of scene nodes as the children of another scene node.
  /// </summary>
  public class SceneNodeCollection
    : ChildCollection<SceneNode, SceneNode>,
      ICollection<SceneNode>  // The interface is necessary for the VS class diagrams!
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets (or sets) the parent which owns this child collection.
    /// </summary>
    /// <value>The parent.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
    public new SceneNode Parent
    {
      get { return base.Parent; }
      internal set { base.Parent = value; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="SceneNodeCollection"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="SceneNodeCollection"/> class.
    /// </summary>
    public SceneNodeCollection()
      : base(null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SceneNodeCollection" /> class with the specified
    /// initial capacity
    /// </summary>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    public SceneNodeCollection(int capacity)
      : base(null, capacity)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override SceneNode GetParent(SceneNode child)
    {
      return child.Parent;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void SetParent(SceneNode child, SceneNode parent)
    {
      Debug.Assert(child.Parent == null || child.Parent != parent, "SetParent() should not be called if the correct parent is already set.");

      child.Parent = parent;
    }
    #endregion
  }
}
