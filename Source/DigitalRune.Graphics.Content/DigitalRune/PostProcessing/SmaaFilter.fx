//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Smaa.fx
/// Applies a Enhanced Subpixel Morphological Anti-Aliasing (SMAA).
//
// SMAA has many customization options. See original SMAA sample from
// http://www.iryoku.com/smaa/.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels
float2 ViewportSize;

// The pixel size (= 1 / ViewportSize).
float2 PixelSize;

#define SMAA_PIXEL_SIZE PixelSize
#define SMAA_PRESET_MEDIUM 1
#define SMAA_HLSL_3 1
#include "../Smaa.fxh"

// The input texture
texture SourceTexture;
sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  MipFilter = POINT;
};

// The edges texture computed in the edge detection pass.
texture EdgesTexture;
sampler EdgesSampler = sampler_state
{
  Texture = <EdgesTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MipFilter = LINEAR;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
};

// The blend texture computed in the blending weight computation pass.
texture BlendTexture;
sampler BlendSampler = sampler_state
{
  Texture = <BlendTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MipFilter = LINEAR;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
};

// The area lookup texture (file SmaaAreaTexDX9.dds).
texture AreaLookupTexture;
sampler AreaLookupSampler = sampler_state
{
  Texture = <AreaLookupTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MipFilter = LINEAR;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
};

// The search lookup texture (file SmaaSearchTex.dds).
texture SearchLookupTexture;
sampler SearchLookupSampler = sampler_state
{
  Texture = <SearchLookupTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MipFilter = POINT;
  MinFilter = POINT;
  MagFilter = POINT;
};


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
};

struct VSEdgeDetectionOutput
{
  float2 TexCoord : TEXCOORD0;
  float4 Offset[3] : TEXCOORD1;
  float4 Position : SV_Position;
};

struct PSLumaEdgeDetectionInput
{
  float2 TexCoord : TEXCOORD0;
  float4 Offset[3] : TEXCOORD1;
};


struct VSBlendingWeightCalculationOutput
{
  float4 Position : SV_Position;
  float2 TexCoord : TEXCOORD0;
  float2 PixCoord : TEXCOORD1;
  float4 Offset[3] : TEXCOORD2;
};

struct PSBlendingWeightCalculationInput
{
  float4 Position : SV_Position;
  float2 TexCoord : TEXCOORD0;
  float2 PixCoord : TEXCOORD1;
  float4 Offset[3] : TEXCOORD2;
};


struct VSNeighborhoodBlendingOutput
{
  float2 TexCoord : TEXCOORD0;
  float4 Offset[2] : TEXCOORD1;
  float4 Position : SV_Position;
};

struct PSNeighborhoodBlendingInput
{
  float2 TexCoord : TEXCOORD0;
  float4 Offset[2] : TEXCOORD1;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSEdgeDetectionOutput VSEdgeDetection(VSInput input)
{
  VSEdgeDetectionOutput output = (VSEdgeDetectionOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  
  SMAAEdgeDetectionVS(output.Position, output.Position, output.TexCoord, output.Offset);
  
  return output;
}

float4 PSLumaEdgeDetection(PSLumaEdgeDetectionInput input) : COLOR
{
  return SMAALumaEdgeDetectionPS(input.TexCoord, input.Offset, SourceSampler);
}


VSBlendingWeightCalculationOutput VSBlendingWeightCalculation(VSInput input)
{
  VSBlendingWeightCalculationOutput output = (VSBlendingWeightCalculationOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  
  SMAABlendingWeightCalculationVS(output.Position, output.Position, output.TexCoord, output.PixCoord, output.Offset);
  
  return output;
}

float4 PSBlendingWeightCalculation(PSBlendingWeightCalculationInput input) : COLOR
{
  return SMAABlendingWeightCalculationPS(input.TexCoord, input.PixCoord, input.Offset, EdgesSampler, AreaLookupSampler, SearchLookupSampler, 0);
}


VSNeighborhoodBlendingOutput VSNeighborhoodBlending(VSInput input)
{
  VSNeighborhoodBlendingOutput output = (VSNeighborhoodBlendingOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  
  SMAANeighborhoodBlendingVS(output.Position, output.Position, output.TexCoord, output.Offset);
  
  return output;
}


float4 PSNeighborhoodBlending(PSNeighborhoodBlendingInput input) : COLOR
{
  return SMAANeighborhoodBlendingPS(input.TexCoord, input.Offset, SourceSampler, BlendSampler);
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
  pass LumaEdgeDetection
  {
    VertexShader = compile VSTARGET VSEdgeDetection();
    PixelShader = compile PSTARGET PSLumaEdgeDetection();
  }
  
  pass BlendWeightCalculation
  {
    VertexShader = compile VSTARGET VSBlendingWeightCalculation();
    PixelShader = compile PSTARGET PSBlendingWeightCalculation();
  }
  
  pass NeighborhoodBlending
  {
    VertexShader = compile VSTARGET VSNeighborhoodBlending();
    PixelShader = compile PSTARGET PSNeighborhoodBlending();
  }
}
