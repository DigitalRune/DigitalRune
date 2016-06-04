//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file TerrainHoleLayer.fx
/// Renders hole information into the detail clipmap.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 ViewportSize;            // The viewport size in pixels.
float2 TerrainTileOrigin;       // World space origin of this tile.
float2 TerrainTileSize;         // World space size of this tile.

// The hole map which stores "opacity" in the Alpha channel.
float2 TerrainTileHoleTextureSize;  // The size of the TerrainTileHoleTexture in texels.
texture TerrainTileHoleTexture;
sampler HoleSampler = sampler_state
{
  Texture = <TerrainTileHoleTexture>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = POINT;
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
  output.TexCoord += 0.5 / TerrainTileHoleTextureSize;
  
  return output;
}


void PS(float2 texCoord : TEXCOORD0, out float4 color0 : COLOR0)
{
  // Hole info in alpha:
  //float hole = tex2Dlod(HoleSampler, float4(texCoord, 0, 0)).a;
  float hole = tex2D(HoleSampler, texCoord).a;
  
  // (normal.x, normal.z, specular exponent, hole alpha)
  color0 = float4(0, 0, 0, hole);
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
    // Detail layers normally use alpha-blending. This layer doesn't because we need to write into
    // the alpha channel.
    AlphaBlendEnable = FALSE;
    
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
