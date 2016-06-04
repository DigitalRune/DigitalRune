// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class GraphicsHelper
  {
    // Note: Cube map faces are left-handed! Therefore +Z is actually -Z.
    private static readonly Vector3F[] CubeMapForwardDirections =
    { 
      Vector3F.UnitX, -Vector3F.UnitX, 
      Vector3F.UnitY, -Vector3F.UnitY,
      -Vector3F.UnitZ, Vector3F.UnitZ   // Switch Z because cube maps are left-handed.
    };


    private static readonly Vector3F[] CubeMapUpDirections =
    { 
      Vector3F.UnitY, Vector3F.UnitY,
      Vector3F.UnitZ, -Vector3F.UnitZ,
      Vector3F.UnitY, Vector3F.UnitY
    };


    /// <summary>
    /// Gets the camera forward direction for rendering into a cube map face.
    /// </summary>
    /// <param name="cubeMapFace">The cube map face.</param>
    /// <returns>
    /// The camera forward direction required to render the content of the
    /// given cube map face.
    /// </returns>
    public static Vector3F GetCubeMapForwardDirection(CubeMapFace cubeMapFace)
    {
      return CubeMapForwardDirections[(int)cubeMapFace];
    }


    /// <summary>
    /// Gets the camera up direction for rendering into a cube map face.
    /// </summary>
    /// <param name="cubeMapFace">The cube map face.</param>
    /// <returns>
    /// The camera up direction required to render the content of the
    /// given cube map face.
    /// </returns>
    public static Vector3F GetCubeMapUpDirection(CubeMapFace cubeMapFace)
    {
      return CubeMapUpDirections[(int)cubeMapFace];
    }
  }
}
