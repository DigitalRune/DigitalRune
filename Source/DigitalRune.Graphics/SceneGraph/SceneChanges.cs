// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Specifies a change in the scene graph.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags", Justification = "Currently not used as flags, but might be useful in the future.")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
  //[Flags]
  public enum SceneChanges
  {
    /// <summary>
    /// No change.
    /// </summary>
    None = 0,

    /// <summary>
    /// A scene node was added to the local subtree.
    /// </summary>
    NodeAdded = 1,
    
    /// <summary>
    /// A scene node was removed from the local subtree.
    /// </summary>
    NodeRemoved = 2,

    /// <summary>
    /// A scene node's <see cref="SceneNode.IsEnabled"/> flag has changed.
    /// </summary>
    IsEnabledChanged = 4,

    /// <summary>
    /// The bounding shape of a scene node has changed.
    /// </summary>
    ShapeChanged = 8,
    
    /// <summary>
    /// The pose of a scene node has changed.
    /// </summary>
    PoseChanged = 16,
    
    ///// <summary>
    ///// An unspecified change has happened to the local subtree.
    ///// </summary>
    //Any = ~0, // 0xffffffff
  }
}
