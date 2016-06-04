// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Graphics.SceneGraph;
#if PARTICLES
using DigitalRune.Particles;
#endif


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines the normal vector of a billboard.
  /// </summary>
  /// <remarks>
  /// Billboards are oriented, textured polygons. The billboard orientation is usually defined by 
  /// two vectors: the normal vector and the axis vector. The normal vector of a billboard is the 
  /// vector that points away from the billboard plane, usually towards the viewpoint (camera). The 
  /// <see cref="BillboardNormal"/> enumeration defines which normal vector is used for rendering.
  /// </remarks>
  /// <seealso cref="Billboard"/>
  /// <seealso cref="BillboardOrientation"/>
  public enum BillboardNormal
  {
    /// <summary>
    /// The billboard normal is parallel to the view vector of the camera, but points in the 
    /// opposite direction (= towards the camera). The billboard is always parallel to the view
    /// plane (screen) and rotates when the orientation of the camera changes.
    /// </summary>
    ViewPlaneAligned,


    /// <summary>
    /// The billboard normal vector points from the center of the billboard towards the camera 
    /// (viewpoint). The billboard always faces the camera and rotates when the position of the
    /// camera changes.
    /// </summary>
    ViewpointOriented,


    /// <summary>
    /// <para>
    /// The billboard normal is specified explicitly.
    /// </para>
    /// <para>
    /// The billboard normal is specified differently depending on the type of billboard:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <term>Billboards</term>
    /// <description>
    /// The normal vector of a regular billboard is given by the <see cref="BillboardNode"/>. The
    /// normal vector is the local z-axis (0, 0, 1) of the scene node.
    /// </description>
    /// </item>
    /// <item>
    /// <term>Particles</term>
    /// <description>
    /// The normal vector of particles is defined by a particle parameter. The 
    /// <see cref="ParticleSystem"/> needs to have a uniform or varying particle parameter called 
    /// "Normal".
    /// </description>
    /// </item>
    /// </list>
    /// </summary>
    Custom,
  }
}
