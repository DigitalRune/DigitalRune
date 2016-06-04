// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Graphics.SceneGraph;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines options for rendering decals.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames", Justification = "Can be extended with Flags in the future.")]
  public enum DecalOptions
  {
    /// <summary>
    /// The decal is applied to all types of geometry.
    /// </summary>
    ProjectOnAll,


    /// <summary>
    /// The decal is applied only to static geometry. (Only scene nodes where 
    /// <see cref="SceneNode.IsStatic"/> is set will receive the decal.)
    /// </summary>
    ProjectOnStatic
  }
}
