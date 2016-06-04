//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file TerrainGBuffer.fx
/// Renders the "GBuffer" render pass of a terrain node
/// Per default this effect uses vertex-based holes and no parallax occlusion 
/// mapping.
//
//-----------------------------------------------------------------------------


#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Material.fxh"
#include "../Noise.fxh"
#include "../Color.fxh"

#define MAX_ANISOTROPY 8
#include "Terrain.fxh"

// CREATE_GBUFFER creates automatically bound shader constants required for
// encoding data in the G-Buffer.
#define CREATE_GBUFFER 1
#include "../Deferred.fxh"

// Warning: gradient-based operations must be moved out of flow control...
// Is caused by MipLevel, which is called before flow control!?
#pragma warning(disable : 4121)

//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

//#define PIXEL_HOLES 1     // 1 to enable texkill holes in pixel shader instead of vertex-based holes.
//#define PARALLAX_MAPPING 1  // 1 to enable parallax occlusion mapping.


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float3 LodCameraPosition : LODCAMERAPOSITION;
float CameraFar : CAMERAFAR;

texture TerrainBaseClipmap0;
sampler TerrainBaseClipmapSampler0 = sampler_state
{
  Texture = <TerrainBaseClipmap0>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MINFILTER = POINT;
  MAGFILTER = POINT;
  MIPFILTER = NONE;
};
float TerrainBaseClipmapCellSize = 1;
float TerrainBaseClipmapCellsPerLevel = 64;
float TerrainBaseClipmapNumberOfLevels = 6;
float TerrainBaseClipmapNumberOfColumns = 4;
float TerrainBaseClipmapLevelBias = 0.1;
float TerrainHoleThreshold = 0.3;
float2 TerrainBaseClipmapOrigins[9];
float NaN;

//#define TEXTURE_FILTER LINEAR
#define TEXTURE_FILTER ANISOTROPIC

texture TerrainDetailClipmap0;
sampler TerrainDetailClipmapSampler0 = sampler_state
{
  Texture = <TerrainDetailClipmap0>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MINFILTER = TEXTURE_FILTER;
  MAGFILTER = TEXTURE_FILTER;
  MIPFILTER = NONE;
  MaxAnisotropy = MAX_ANISOTROPY;
};
texture TerrainDetailClipmap2;
sampler TerrainDetailClipmapSampler2 = sampler_state
{
  Texture = <TerrainDetailClipmap2>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MINFILTER = TEXTURE_FILTER;
  MAGFILTER = TEXTURE_FILTER;
  MIPFILTER = NONE;
  MaxAnisotropy = MAX_ANISOTROPY;
};
float2 TerrainDetailClipmapOrigins[9];
float TerrainDetailClipmapCellSizes[9];
float TerrainDetailClipmapCellsPerLevel = 1024;
float TerrainDetailClipmapNumberOfLevels = 6;
float TerrainDetailClipmapNumberOfColumns = 4;
float TerrainDetailClipmapLevelBias;
float2 TerrainDetailClipmapOffsets[9];
float TerrainDetailFadeRange;
float TerrainEnableAnisotropicFiltering;

#if PARALLAX_MAPPING
float3 CameraPosition : CAMERAPOSITION;
float3 DirectionalLightDirection : DIRECTIONALLIGHTDIRECTION;
int ParallaxMinNumberOfSamples = 4;
int ParallaxMaxNumberOfSamples = 10;
float ParallaxHeightScale = 0.05;
float ParallaxHeightBias = -0.025;
int ParallaxLodThreshold = 2;
float ParallaxShadowScale = 0.08;
float ParallaxNumberOfShadowSamples = 4;
float ParallaxShadowFalloff = 0.5;
float ParallaxShadowStrength = 200;
#endif


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
};


struct VSOutput
{
  float Depth : TEXCOORD0;
  float3 PositionWorld: TEXCOORD1;
#if PARALLAX_MAPPING
  float3 ViewDirectionTangent : TEXCOORD2; // View direction in tangent space.
  float3 LightDirectionTangent : TEXCOORD3;  // LightDirection in tangent space.
#endif
  float4 Position : SV_Position;
};


struct PSInput
{
  float Depth : TEXCOORD0;
  float3 PositionWorld: TEXCOORD1;
#if PARALLAX_MAPPING
  float3 ViewDirectionTangent : TEXCOORD2; // View direction in tangent space.
  float3 LightDirectionTangent : TEXCOORD3;  // LightDirection in tangent space.
#endif
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
#if PIXEL_HOLES
  float holeThreshold = -1;
#else
  float holeThreshold = TerrainHoleThreshold;
#endif

  float3 position = input.Position.xyz;
  float3 normal;
  float clod;
  ComputeTerrainGeometry(
    TerrainBaseClipmapSampler0, TerrainBaseClipmapOrigins, 
    TerrainBaseClipmapCellSize, TerrainBaseClipmapCellsPerLevel,
    TerrainBaseClipmapNumberOfLevels, TerrainBaseClipmapNumberOfColumns, 
    TerrainBaseClipmapLevelBias, holeThreshold,
    LodCameraPosition, float3(NaN, NaN, NaN),
    position, normal, clod);
 
  float4 positionView = mul(float4(position, 1), View);
  float4 positionProj = mul(positionView, Projection);
  float normalizedDepth = -positionView.z / CameraFar;
  
  // ----- Output
  VSOutput output = (VSOutput)0;
  output.Depth = normalizedDepth;
  output.PositionWorld = position;
  output.Position = positionProj;
  
#if PARALLAX_MAPPING
  float3 tangent = cross(normal, float3(0, 0, 1));
  float3 binormal = cross(tangent, normal);
  // Matrix that transforms column(!) vectors from world space to tangent space.
  float3x3 worldToTangent = float3x3(tangent, binormal, normal);
  float3 viewDirectionWorld = output.PositionWorld - CameraPosition;
  output.ViewDirectionTangent = mul(worldToTangent, viewDirectionWorld);
  output.LightDirectionTangent = mul(worldToTangent, DirectionalLightDirection);
#endif
   
  return output;
}


void PS(PSInput input, out float4 depthBuffer : COLOR0, out float4 normalBuffer : COLOR1)
{
  float3 positionWorld = input.PositionWorld;
  
  depthBuffer = 1;
  normalBuffer = float4(0, 0, 0, 1);

  float specularPower = 0, alpha;
  float3 normal, diffuse, specularIntensity;
#if PARALLAX_MAPPING
  ComputeTerrainMaterial(
    TerrainDetailClipmapSampler0,
    TerrainDetailClipmapSampler0,   // Dummy value. Not needed here.
    TerrainDetailClipmapSampler2,
    TerrainDetailClipmapOrigins, TerrainDetailClipmapCellSizes, TerrainDetailClipmapCellsPerLevel, 
    TerrainDetailClipmapNumberOfLevels, TerrainDetailClipmapNumberOfColumns, 
    TerrainDetailClipmapLevelBias, TerrainDetailClipmapOffsets, TerrainDetailFadeRange,
    TerrainEnableAnisotropicFiltering,
    positionWorld, input.ViewDirectionTangent, input.LightDirectionTangent,
    ParallaxMinNumberOfSamples, ParallaxMaxNumberOfSamples, 
    ParallaxHeightScale, ParallaxHeightBias, ParallaxLodThreshold, 
    ParallaxNumberOfShadowSamples, ParallaxShadowScale, 
    ParallaxShadowFalloff, ParallaxShadowStrength,  
    normal, specularPower, diffuse, alpha, specularIntensity);
#else
    ComputeTerrainMaterial(
    TerrainDetailClipmapSampler0,
    TerrainDetailClipmapSampler0,   // Dummy value. Not needed here.
    TerrainDetailClipmapSampler0,   // Dummy value. Not needed here.
    TerrainDetailClipmapOrigins, TerrainDetailClipmapCellSizes, TerrainDetailClipmapCellsPerLevel, 
    TerrainDetailClipmapNumberOfLevels, TerrainDetailClipmapNumberOfColumns, 
    TerrainDetailClipmapLevelBias, TerrainDetailClipmapOffsets, TerrainDetailFadeRange,
    TerrainEnableAnisotropicFiltering,
    positionWorld, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  
    normal, specularPower, diffuse, alpha, specularIntensity);
#endif
  
#if PIXEL_HOLES
  clip(alpha - TerrainHoleThreshold);
#endif
  
  SetGBufferDepth(input.Depth, 1, depthBuffer);
  SetGBufferNormal(normal.xyz, normalBuffer);
  SetGBufferSpecularPower(specularPower, depthBuffer, normalBuffer);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique Default
{
  pass Default
  {
#if !SM4
    VertexShader = compile vs_3_0 VS();
    PixelShader = compile ps_3_0 PS();
#else
    VertexShader = compile vs_4_0 VS();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}
