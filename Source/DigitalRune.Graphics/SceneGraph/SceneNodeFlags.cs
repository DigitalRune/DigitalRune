// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Flags used in scene nodes.
  /// </summary>
  [Flags]
  internal enum SceneNodeFlags
  {
    None = 0,
    IsDisposed = 1 << 0,
    IsAabbDirty = 1 << 1,
    IsScaleWorldDirty = 1 << 2,
    IsPoseWorldDirty = 1 << 3,
    HasLastScaleWorld = 1 << 4,
    HasLastPoseWorld = 1 << 5,
    IsDirty = 1 << 6,       // General purpose flag. Usage depends on scene node type.
    IsEnabled = 1 << 7,
    IsStatic = 1 << 8,
    IsRenderable = 1 << 9,
    CastsShadows = 1 << 10,
    IsShadowCasterCulled = 1 << 11,
    HasAlpha = 1 << 12,     // Does the node have an Alpha value that can be changed?
    
    // Following flags share the same bit. Only one can be used per SceneNode type.
    IsAlphaSet = 1 << 13,   // Is the current Alpha value != 1?
    InvertClip = 1 << 13,   // LightNode.InvertClip
    
    IsDirtyScene = 1 << 14, // Like is IsDirty, but resetting is controlled by Scene.

    // Upper 16 bit are reserved for SceneNode.UserFlag.
    // Possible extensions: IsReflected, IsSelected, etc.
  }
}
