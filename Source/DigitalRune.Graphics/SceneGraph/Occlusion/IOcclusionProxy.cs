// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Represents a scene node with an occluder.
  /// </summary>
  internal interface IOcclusionProxy
  {
    /// <summary>
    /// Gets a value indicating whether an occluder is available.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if an occluder is available; otherwise, <see langword="false"/>.
    /// </value>
    bool HasOccluder { get; }


    /// <summary>
    /// Updates the occluder.
    /// </summary>
    void UpdateOccluder();


    /// <summary>
    /// Gets the occluder for rendering.
    /// </summary>
    /// <returns>The occluder.</returns>
    OccluderData GetOccluder();
  }
}
