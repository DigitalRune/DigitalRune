// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Defines the HLSL semantic of an input element.
  /// </summary>
  internal enum VertexElementSemantic
  {
    // Reference: https://msdn.microsoft.com/en-us/library/windows/desktop/bb509647.aspx

    /// <summary>Binormal ("BINORMAL")</summary>
    Binormal,
    
    /// <summary>Blend indices ("BLENDINDICES")</summary>
    BlendIndices,

    /// <summary>Blend weights ("BLENDWEIGHT") </summary>
    BlendWeight,

    /// <summary>Diffuse and specular color ("COLOR")</summary>
    Color,

    /// <summary>Normal vector ("NORMAL")</summary>
    Normal,

    /// <summary>Vertex position in object space ("POSITION")</summary>
    Position,

    /// <summary>Transformed vertex position ("POSITIONT")</summary>
    PositionTransformed,

    /// <summary>Point size ("PSIZE")</summary>
    PointSize,

    /// <summary>Tangent ("TANGENT")</summary>
    Tangent,

    /// <summary>Texture coordinates ("TEXCOORD")</summary>
    TextureCoordinate
  }
}
