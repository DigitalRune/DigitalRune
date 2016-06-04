//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file TerrainGeometryLayer.fx
/// Renders data into the base clipmap.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// ----- General Constants
float2 ViewportSize;        // The viewport size in pixels.
float2 TerrainTileOrigin;       // World space origin of this tile.
float2 TerrainTileSize;         // World space size of this tile.
float TerrainClipmapLevel;  // The clipmap level which is currently being rendered.

// ----- Textures
// The height map with height in the Red channel.
float2 TerrainTileHeightTextureSize;
texture TerrainTileHeightTexture;
sampler TerrainTileHeightSampler = sampler_state
{
  Texture = <TerrainTileHeightTexture>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = POINT;       // Note: In the future we can use LINEAR.
  MagFilter = POINT;
  MipFilter = POINT;
};

// The normal map which stores normals like a standard "green-up" normal map.
// (That means, the up-component is stored in z!)
texture TerrainTileNormalTexture;
sampler TerrainTileNormalSampler = sampler_state
{
  Texture = <TerrainTileNormalTexture>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
};

// The hole map which stores "opacity" in the Alpha channel.
texture TerrainTileHoleTexture;
sampler TerrainTileHoleSampler = sampler_state
{
  Texture = <TerrainTileHoleTexture>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
};


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
};

struct VSOutput
{
  float2 TexCoord : TEXCOORD;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Shaders
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  
  // texCoord contains world space position (X, Z). Convert to texture coordinates.
  output.TexCoord = (input.TexCoord - TerrainTileOrigin) / TerrainTileSize;
  
  // The terrain tile origin and the height texture origin differ by half a texel because the
  // tile starts in the center of the first texel.
  output.TexCoord += 0.5 / TerrainTileHeightTextureSize;
  
  return output;
}

void PS(float2 texCoord : TEXCOORD0, out float4 color0 : COLOR0)
{
  // Note: This sampling assumes that mipmap level 0 has a POT size and each mipmap level
  // has half the resolution. This is not correct if level 0 has e.g. 1025 texels. In this
  // case the texCoord is not in the texel center if the level is > 0. This can is usually less
  // noticable for height, but can be very noticable for holes and if we use POINT sampling.
  // (Camera is moving over the terrain. Holes are jumping laterally.) It is not noticable if we use
  // LINEAR sampling.
  
  float height = tex2Dlod(TerrainTileHeightSampler, float4(texCoord, 0, TerrainClipmapLevel)).r;
  
  float3 normal = tex2Dlod(TerrainTileNormalSampler, float4(texCoord, 0, TerrainClipmapLevel)).rgb;
  normal = normal * 2.0 - 1.0;
  
  // Hole info in alpha!
  float hole = tex2Dlod(TerrainTileHoleSampler, float4(texCoord, 0, TerrainClipmapLevel)).a;
  
  //// Set everything outside the texture to "hole".
  //if (texCoord.x < 0 || texCoord.x > 1 || texCoord.y < 0 || texCoord.y > 1)
  //  hole = 0;
  
  color0 = 0;
  color0.r = height;
  
  // G and B store the horizontal normal components.
  color0.g = normal.x;
  color0.b = normal.y;
  
  // Partial derivative normals (can be additively blended)
  //color0.g = normal.x / normal.z; // Store partial derivative to allow additive blending!
  //color0.b = normal.y / normal.z;
  
  color0.a = hole;
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_3_0
#define PSTARGET ps_3_0
#else
#define VSTARGET vs_4_0
#define PSTARGET ps_4_0
#endif

technique
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
