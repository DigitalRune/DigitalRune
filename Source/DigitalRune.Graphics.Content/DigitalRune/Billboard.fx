//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Billboard.fx
/// Draws billboards and ribbons.
//
// BILLBOARDS
//
//   (0, 0) (1, 0)
//       +---+
//       |  /|
//       | / |
//       |/  |
//       + --+
//   (0, 1) (1, 1)
//
//  Only the center of the billboard is specified. The texture coordinates
//  identify the corner. The offset from the center to the corner is computed
//  in the vertex shader.
//
// RIBBONS
//  If the normal vector is not set, the vertex belongs to a ribbon.
//
//  (0, 0)          (1, 0)
//   --+--------------+--
//     |              |
//    p0             p1
//     |              |
//   --+--------------+--
//  (0, 1)          (1, 1)
//
//  The first two vertices define the left edge and the second two vertices
//  define the right edge of the of the segment. The offset from the center to
//  the upper and lower edge is computed in the vertex shader.
//
// INSTANCING
//  When using hardware instancing the texture coordinates are taken from
//  the first vertex stream. That means the semantic of TexCoord needs to be
//  TEXCOORD0.
//  Hardware instancing is currently not used, because it only works for
//  billboards, but you can't mix billboards and ribbons in a single draw call.
//
// SOFT PARTICLES
//  Soft particles are rendered by performing an explicit depth test. "Soft" in
//  this context means:
//  - Particles fade out near the camera. This check is done in the vertex
//    shader.
//  - Particles create soft edges when intersecting with other geometry. This
//    check is done in the pixel shader.
//
// HIGH-SPEED, OFF-SCREEN PARTICLES
//  Particles can be rendered into half-resolution, off-screen buffer to
//  improve performance. The shaders are identical, but a different blend state
//  needs to be set (see BillboardRenderer). A final full-screen pass is
//  required to upsample the off-screen buffer and combine the result with the
//  scene.
//-----------------------------------------------------------------------------

#include "Common.fxh"

// For depth test in pixel shader:
#include "Encoding.fxh"
#include "Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 View : VIEW;
float4x4 ViewInverse : VIEWINVERSE;
float4x4 ViewProjection : VIEWPROJECTION;
float4x4 Projection : PROJECTION;
float3 CameraPosition : CAMERAPOSITION;

// ----- Billboard texture (RGBA)
texture Texture : DIFFUSETEXTURE;
sampler TextureSampler = sampler_state
{
  Texture = <Texture>;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
};

// ----- Soft particles: Require depth test in shader
float2 ViewportSize : VIEWPORTSIZE;
DECLARE_UNIFORM_GBUFFER(DepthBuffer, 0);
float CameraNear : CAMERANEAR;
float CameraFar : CAMERAFAR;


//-----------------------------------------------------------------------------
// Input, output
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;   // Billboard: center; Ribbon: center of left or right edge
  float3 Normal : NORMAL;       // Billboard: normal; Ribbon: (0, 0, 0)
  float3 Axis : TANGENT;        // Up axis
  float4 Color : COLOR;         // Tint color
  float2 TexCoord : TEXCOORD0;  // Corner texture coordinates: (0, 0), (1, 0), (0, 1), or (1, 1)
  float4 Args0 : TEXCOORD1;     // x = Billboard orientation (0 = default, 1 = viewpoint-oriented)
                                // y = Axis in world space (0) or view space (1)
                                // z = Normal fixed (0) or axis fixed (1)
                                // w = Rotation angle [rad]
  float4 Args1 : TEXCOORD2;     // xy = Half size
                                // z = Soft particle distance threshold
                                // w = Alpha test (reference value)
  float4 Args2 : TEXCOORD3;     // xy = Scale of texture in texture atlas [0, 1].
                                // zw = Offset of texture in texture atlas [0, 1[.
  float4 Args3 : TEXCOORD4;     // xy = Number of tiles in x and y.
                                // z = Animation time (0 = first frame, 1 = last frame)
                                // w = Blend mode (0 = additive, 1 = alpha blended)
  
  // Other possible parameters:
  //   float TextureIndex;     // Index of texture atlas page? (Use cube map on D3D9 devices.)
  //   float EnableLighting;   // 0 ... unlit, 1 ... lit
  //   float FilterType;       // Texture filtering?
};

#define IsBillboard(input) any(input.Normal)
#define IsViewpointOriented(input) input.Args0.x
#define IsAxisInViewSpace(input) input.Args0.y
#define IsAxisFixed(input) input.Args0.z
#define GetAngle(input) input.Args0.w
#define GetWidth(input) input.Args1.x
#define GetHeight(input) input.Args1.y
#define GetSoftParticleThreshold(input) input.Args1.z
#define GetTextureScale(input) input.Args2.xy
#define GetTextureOffset(input) input.Args2.zw
#define GetTilesX(input) input.Args3.x
#define GetTilesY(input) input.Args3.y
#define GetAnimationTime(input) input.Args3.z
#define GetTextureAddressMode(input) input.Args3.w
#define GetReferenceAlpha(input) input.Args1.w
#define GetBlendMode(input) input.Args3.w


// Note: For pixel shader versions ps_1_1 - ps_2_0, diffuse and specular colors are
// saturated (clamped) in the range 0 to 1 before use by the shader.
// --> Color (HDR!) needs to be passed as TEXCOORD instead of COLOR!

struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float4 Color : TEXCOORD1;         // Premultiplied color, alpha includes blend mode.
  float ReferenceAlpha : TEXCOORD2;
  float4 Position : SV_Position;
};

struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float4 Color : TEXCOORD1;         // Premultiplied color, alpha includes blend mode.
  float ReferenceAlpha : TEXCOORD2;
};

struct VSOutputSoft
{
  float2 TexCoord : TEXCOORD0;
  float4 Color : TEXCOORD1;         // Premultiplied color, alpha includes blend mode.
  float4 PositionProj : TEXCOORD2;
  float2 Depth : TEXCOORD3;         // x = linear depth [0, 1], y = soft particle distance threshold
  float ReferenceAlpha : TEXCOORD4;
  float4 Position : SV_Position;
};

struct PSInputSoft
{
  float2 TexCoord : TEXCOORD0;
  float4 Color : TEXCOORD1;         // Premultiplied color, alpha includes blend mode.
  float4 PositionProj : TEXCOORD2;
  float2 Depth : TEXCOORD3;         // x = linear depth [0, 1], y = soft particle distance threshold
  float ReferenceAlpha : TEXCOORD4;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Gets the texture coordinate for a tile of a tile set.
/// \param[in] texCoord The texture coordinate [0,1].
/// \param[in] tileX    The number of columns in the tile set.
/// \param[in] tileY    The number of rows in the tile set.
/// \param[in] time     The normalized animation time: 0 = first tile, 1 = last tile
/// \return The color in linear space.
/// \remarks
/// Tile sheet must not contain empty tiles. For example, 3 tiles can be packed
/// as 1x3 or 3x1 tile sheet, but not as 2x2!
float2 GenerateTextureCoordinate(float2 texCoord, int tileX, int tileY, float time)
{
  float tx = tileX;
  float ty = tileY;
  float itx = 1 / tx;
  float ity = 1 / ty;
  
  // Texture coordinates relative to first tile:
  texCoord = float2(texCoord.x * itx, texCoord.y * ity);
  
  // Calculate and apply offset of current tile.
  //
  // Wanted:
  //   offsetX ... x-offset of current frame relative to tile sheet.
  //   offsetY ... y-offset of current frame relative to tile sheet.
  //
  // When tile sheet is flat list:
  //   index = floor(time * numberOfTiles)
  // When tile sheet is stacked:
  //   indexY = floor(time * ty)
  //   indexX = ?
  // Conversion from stacked to flat list:
  //   index = indexY * tx + indexX
  //
  // => indexX = index - indexY * tx
  //    Size of a tile = 1/tx = itx
  //    offsetX = indexX * itx
  //            = index * itx - indexY * tx * itx
  //            = index * itx - indexY
  //            = floor(time * numberOftiles) / tx - floor(time * ty)
  //            = floor(time * tx * ty) * itx - floor(time * ty)
  float offsetX = floor(time * tx * ty) * itx - floor(time * ty);
  
  // When tile sheet is packed from top to bottom.
  float offsetY = floor(time * ty) * ity;
  
  // Or, when tile sheet is packed from bottom to top.
  // float offsetY = 1 - ity - floor(time * ty) * ity;
  
  texCoord += float2(offsetX, offsetY);
  return texCoord;
}


/// Gets the offset to the corner of the billboard/ribbon.
/// \param[in] input  The vertex shader input.
/// \return The offset to the corner.
float3 GetOffset(VSInput input)
{
  if (IsBillboard(input))
  {
    // ----- BILLBOARD
    // Determine billboard axes: right/up/normal
    float3 normal;
    if (IsViewpointOriented(input))
    {
      // Normal points towards camera.
      normal = normalize(CameraPosition - input.Position.xyz);
    }
    else
    {
      // Normal given in vertex data.
      normal = input.Normal;
    }
    
    float3 up = input.Axis;
    if (IsAxisInViewSpace(input))
    {
      // Convert axis from view space to world space.
      up = mul(input.Axis, (float3x3)ViewInverse);
    }
    
    const float epsilon = 1e-5f;
    if (1 - dot(normal, up) < epsilon)
    {
      // Normal and axis are parallel.
      // --> Bend normal by adding a fraction of the camera down vector.
      normal -= ViewInverse._m10_m11_m12 * 0.001f;
      normalize(normal);
    }
    
    float3 right = normalize(cross(up, normal));
    if (IsAxisFixed(input))
    {
      // Make normal perpendicular to right and up.
      normal = cross(right, up);
    }
    else
    {
      // Make up perpendicular to normal and right.
      up = cross(normal, right);
    }
    
    // ----- Determine offset of billboard corner from billboard center.
    float3 offset = right * (input.TexCoord.x - 0.5) * GetWidth(input) - up * (input.TexCoord.y - 0.5) * GetHeight(input);
    
    if (GetAngle(input))
    {
      // Apply rotation.
      float x = normal.x;
      float y = normal.y;
      float z = normal.z;
      float x2 = x * x;
      float y2 = y * y;
      float z2 = z * z;
      float xy = x * y;
      float xz = x * z;
      float yz = y * z;
      float cosa = cos(GetAngle(input));
      float sina = sin(GetAngle(input));
      float xsin = x * sina;
      float ysin = y * sina;
      float zsin = z * sina;
      float oneMinusCos = 1.0 - cosa;
      float3x3 rotation = { x2 + cosa * (1.0f - x2),  xy * oneMinusCos + zsin,  xz * oneMinusCos - ysin,
                            xy * oneMinusCos - zsin,  y2 + cosa * (1.0f - y2),  yz * oneMinusCos + xsin,
                            xz * oneMinusCos + ysin,  yz * oneMinusCos - xsin,  z2 + cosa * (1.0f - z2)  };
      offset = mul(offset, rotation);
    }
    
    return offset;
  }
  else
  {
    // ----- RIBBON
    return -input.Axis * (input.TexCoord.y - 0.5) * GetHeight(input);
  }
}


/// Discards the pixel if the alpha value is less than the specified reference value.
/// \param[in] alpha          The alpha value.
/// \param[in] referenceAlpha The reference value.
void AlphaTest(float alpha, float referenceAlpha)
{
  // Subtract a small constant (epsilon < 1/255) to discard transparent pixels.
  const float epsilon = 0.001;
  
  clip(alpha - referenceAlpha - epsilon);
}


VSOutput VSHard(VSInput input, uniform const bool gammaCorrection)
{
  VSOutput output = (VSOutput)0;
  
  // ----- Position
  // Position in world space.
  float4 position = input.Position;
  
  // The vertex is at the center of the billboard/ribbon.
  // --> Move vertex to corner.
  position.xyz += GetOffset(input);
  
  // Position in projection space.
  output.Position = mul(position, ViewProjection);
  
  // ----- Color, alpha and blend mode
  output.Color = input.Color;
  if (gammaCorrection)
  {
    // Apply gamma correction. (Not necessary when using sRGB texture formats!)
    output.Color.rgb = ToGamma(output.Color.rgb);
  }
  
  // Premultiply tint color.
  output.Color.rgb *= output.Color.a;
  
  // Apply blend mode to alpha.
  output.Color.a *= GetBlendMode(input);
  
  // Reference value for alpha test.
  output.ReferenceAlpha = GetReferenceAlpha(input);
  
  // ----- Texture coordinates
  // Get texture coordinates of current animation frame.
  float2 texCoord = GenerateTextureCoordinate(input.TexCoord, GetTilesX(input), GetTilesY(input), GetAnimationTime(input));
  
  // Get texture coordinates within texture atlas.
  output.TexCoord = texCoord * GetTextureScale(input) + GetTextureOffset(input);
  
  return output;
}
VSOutput VSHardLinear(VSInput input) { return VSHard(input, false); }
VSOutput VSHardGamma(VSInput input) { return VSHard(input, true); }


VSOutputSoft VSSoft(VSInput input, uniform const bool gammaCorrection)
{
  VSOutputSoft output = (VSOutputSoft)0;
  
  // ----- Position
  // Position in world space.
  float4 position = input.Position;
  
  // The vertex is at the center of the billboard/ribbon.
  // --> Move vertex to corner.
  position.xyz += GetOffset(input);
  
  // Position in view space.
  float4 positionView = mul(position, View);
  
  // Position in projection space.
  output.Position = mul(positionView, Projection);
  
  // ----- Depth test in pixel shader and soft particles
  output.PositionProj = output.Position;
  
  float depth = -positionView.z;
  float softParticleDistanceThreshold = GetSoftParticleThreshold(input);
  if (softParticleDistanceThreshold)
  {
    if (softParticleDistanceThreshold < 0)
    {
      // Automatic soft particle distance threshold.
      // --> Choose minimum of width and height.
      softParticleDistanceThreshold = min(GetWidth(input), GetHeight(input));
    }
    
    // ----- Fade out particles near camera.
    // If the distance to the near plane is greater than softParticleDistanceThreshold
    // the particle is fully visible. From there the particle starts to fade out and
    // the becomes invisible when the vertex is at or behind the near plane.
    //
    // (Note: The correct solution would be to start the fade out at a distance of
    // softParticleDistanceThreshold / 2 and end the fade out at a distance of
    // softParticleDistanceThreshold / 2 behind the billboard. But this means that
    // we would have to back the vertex back by softParticleDistanceThreshold / 2.
    // Otherwise, the billboard would be clipped!)
    float nearFade = saturate((depth - CameraNear) / softParticleDistanceThreshold);
    input.Color.a *= nearFade;
    
    // Offset particle towards viewer.
    depth -= softParticleDistanceThreshold / 2;
  }
  
  output.Depth.x = depth;
  output.Depth.y = softParticleDistanceThreshold;
  
  // Normalize depth and soft particle distance threshold.
  output.Depth /= CameraFar;
  
  // ----- Color, alpha and blend mode
  output.Color = input.Color;
  if (gammaCorrection)
  {
    // Apply gamma correction. (Not necessary when using sRGB texture formats!)
    output.Color.rgb = ToGamma(output.Color.rgb);
  }
  
  // Premultiply tint color.
  output.Color.rgb *= output.Color.a;
  
  // Apply blend mode to alpha.
  output.Color.a *= GetBlendMode(input);
  
  // Reference value for alpha test.
  output.ReferenceAlpha = GetReferenceAlpha(input);
  
  // ----- Texture coordinates
  // Get texture coordinates of current animation frame.
  float2 texCoord = GenerateTextureCoordinate(input.TexCoord, GetTilesX(input), GetTilesY(input), GetAnimationTime(input));
  
  // Get texture coordinates within texture atlas.
  output.TexCoord = texCoord * GetTextureScale(input) + GetTextureOffset(input);
  
  return output;
}
VSOutputSoft VSSoftLinear(VSInput input) { return VSSoft(input, false); }
VSOutputSoft VSSoftGamma(VSInput input) { return VSSoft(input, true); }


float4 PSHard(PSInput input, uniform const bool gammaCorrection) : COLOR
{
  float4 textureColor = tex2D(TextureSampler, input.TexCoord);  // (Note: Texture is premultiplied.)
  
  // When using frame interpolation.
  //float4 frame0 = tex2D(DiffuseSampler, input.TexCoord0);
  //float4 frame1 = tex2D(DiffuseSampler, input.TexCoord1);
  //float4 textureColor = lerp(frame0, frame1, input.SubFrameStep);
  
  // ----- Alpha test
  AlphaTest(textureColor.a, input.ReferenceAlpha);
  
  // ----- Color
  float4 color;
  
  if (!gammaCorrection)
  {
    // Convert gamma corrected, premultiplied color to linear, premultiplied color.
    // (Not necessary when using sRGB texture formats!)
    textureColor.rgb /= textureColor.a;
    textureColor.rgb = FromGamma(textureColor.rgb);
    textureColor.rgb *= textureColor.a;
  }
  
  // The vertex color is the tint color.
  color.rgb = textureColor.rgb * input.Color.rgb;
  
  // The vertex alpha contains the blend mode: 0 = additive, 1 = alpha blended
  // (Intermediate values are allowed to mix additive and alpha blending.)
  color.a = textureColor.a * input.Color.a;
  
  return color;
}
float4 PSHardLinear(PSInput input) : COLOR { return PSHard(input, false); }
float4 PSHardGamma(PSInput input) : COLOR { return PSHard(input, true); }


float4 PSSoft(PSInputSoft input, uniform const bool gammaCorrection) : COLOR
{
  float4 textureColor = tex2D(TextureSampler, input.TexCoord);  // (Note: Texture is premultiplied.)
  
  // When using frame interpolation.
  //float4 frame0 = tex2D(DiffuseSampler, input.TexCoord0);
  //float4 frame1 = tex2D(DiffuseSampler, input.TexCoord1);
  //float4 textureColor = lerp(frame0, frame1, input.SubFrameStep);
  
  // ----- Alpha test
  AlphaTest(textureColor.a, input.ReferenceAlpha);
  
  // ----- Depth test and soft particles
  // Get the screen space texture coordinate for this position.
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  float depth = GetGBufferDepth(tex2Dlod(DepthBufferSampler, float4(texCoordScreen, 0, 0)));
  float diff = depth - input.Depth.x;
  
  // Depth test.
  clip(diff);
  
  // Soft particles.
  float softParticleDistanceThreshold = input.Depth.y;
  float softness = 1;
  if (softParticleDistanceThreshold)
    softness = saturate(diff / softParticleDistanceThreshold);
  
  // ----- Color
  float4 color;
  
  if (!gammaCorrection)
  {
    // Convert gamma corrected, premultiplied color to linear, premultiplied color.
    // (Not necessary when using sRGB texture formats!)
    textureColor.rgb /= textureColor.a;
    textureColor.rgb = FromGamma(textureColor.rgb);
    textureColor.rgb *= textureColor.a;
  }
  
  // The vertex color is the tint color.
  color.rgb = textureColor.rgb * input.Color.rgb;
  
  // The vertex alpha contains the blend mode: 0 = additive, 1 = alpha blended
  // (Intermediate values are allowed to mix additive and alpha blending.)
  color.a = textureColor.a * input.Color.a;
  
  // Apply soft particle factor.
  color *= softness;
  
  return color;
}
float4 PSSoftLinear(PSInputSoft input) : COLOR { return PSSoft(input, false); }
float4 PSSoftGamma(PSInputSoft input) : COLOR { return PSSoft(input, true); }


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
#define VSTARGET_3_0 vs_3_0
#define PSTARGET_3_0 ps_3_0
#else
#define VSTARGET vs_4_0_level_9_1
#define PSTARGET ps_4_0_level_9_1

// Can't use flow control in these profiles.
//#define VSTARGET_3_0 vs_4_0_level_9_3
//#define PSTARGET_3_0 ps_4_0_level_9_3

#define VSTARGET_3_0 vs_4_0
#define PSTARGET_3_0 ps_4_0
#endif

// (Hard) particles, no gamma correction.
technique HardLinear
{
  pass
  {
    VertexShader = compile VSTARGET VSHardLinear();
    PixelShader = compile PSTARGET PSHardLinear();
  }
}


// (Hard) particles with gamma correction.
technique HardGamma
{
  pass
  {
    VertexShader = compile VSTARGET VSHardGamma();
    PixelShader = compile PSTARGET PSHardGamma();
  }
}


// Soft particles, no gamma correction.
technique SoftLinear
{
  pass
  {
    VertexShader = compile VSTARGET_3_0 VSSoftLinear();
    PixelShader = compile PSTARGET_3_0 PSSoftLinear();
  }
}


// Soft particles with gamma correction.
technique SoftGamma
{
  pass
  {
    VertexShader = compile VSTARGET_3_0 VSSoftGamma();
    PixelShader = compile PSTARGET_3_0 PSSoftGamma();
  }
}
