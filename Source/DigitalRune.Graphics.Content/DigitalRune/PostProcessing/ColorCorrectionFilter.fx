//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ColorCorrectionFilter.fx
/// Color grading using a lookup table.
//
// See http://http.developer.nvidia.com/GPUGems2/gpugems2_chapter24.html.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

// x: The strength of the effect (expected range [0, 1]).
// y: The lerp factor to lerp from LookupTexture0 to LookupTexture1.
float2 Strength;

// The lookup table size (= number of entries per color channel.)
int LookupTableSize = 16;

// The input texture.
texture SourceTexture;
sampler SourceSampler : register(s0) = sampler_state
{
  Texture = <SourceTexture>;
};

// The color lookup texture (a 3D texture).
texture LookupTexture0;
sampler3D LookupSampler0 : register(s1) = sampler_state
{
  Texture = <LookupTexture0>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  MipFilter = NONE;
};

// Optional: The second lookup texture.
texture LookupTexture1;
sampler3D LookupSampler1 : register(s2) = sampler_state
{
  Texture = <LookupTexture1>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  MipFilter = NONE;
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
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


// Performs color lookup.
float3 ColorLookup(sampler3D lookupSampler, float3 color)
{
  // We want to sample the lookup table at the texel centers.
  // Sampling starts in the center of the first texel = half texel offset.
  float3 offset = 0.5f / LookupTableSize;
  
  // Color value 1 should sample the last texel (n - 1).
  float3 scale = (LookupTableSize - 1.0) / LookupTableSize;
  
  // Lookup new color value.
  return tex3D(lookupSampler, scale * color + offset).rgb;
}


// Transforms colors using the 3D lookup table. (Full color transformation.)
float4 PSFull(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 color = tex2D(SourceSampler, texCoord);
  color.rgb = ColorLookup(LookupSampler0, color.rgb);
  return color;
}


// Transforms colors using the 3D lookup table. (Partial color transformation.)
float4 PSPartial(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 color = tex2D(SourceSampler, texCoord);
  color.rgb = lerp(color.rgb, ColorLookup(LookupSampler0, color.rgb), Strength.x);
  return color;
}


// Lerp between two lookup textures.
float4 PSLerp(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 color = tex2D(SourceSampler, texCoord);
  
  float3 correctedColor = lerp(
    ColorLookup(LookupSampler0, color.rgb),
    ColorLookup(LookupSampler1, color.rgb),
    Strength.y);
  
  color.rgb = lerp(color.rgb, correctedColor, Strength.x);
  return color;
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
#else
#define VSTARGET vs_4_0_level_9_1
#define PSTARGET ps_4_0_level_9_1
#endif

technique
{
  pass Full
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFull();
  }
  
  pass Partial
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSPartial();
  }
  
  pass Lerp
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSLerp();
  }
}
