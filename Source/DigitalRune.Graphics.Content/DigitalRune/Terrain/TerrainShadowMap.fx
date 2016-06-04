//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file TerrainShadowMap.fx
/// Renders the terrain into the shadow map.
/// See the the default ShadowMap.fx for more information.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Material.fxh"
#include "../Noise.fxh"
#include "../ShadowMap.fxh"
#include "../Color.fxh"
#include "Terrain.fxh"

// Warning: gradient-based operations must be moved out of flow control...
// Is caused by MipLevel, which is called before flow control!?
#pragma warning(disable : 4121)


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

// Possible defines.
//#define VSM_BIAS 1     // 1 to enable VSM bias to reduce acne.
//#define PIXEL_HOLES 1  // 1 to enable texkill holes in pixel shader instead of vertex-based holes.


//-----------------------------------------------------------------------------
// Static Constants
//-----------------------------------------------------------------------------

static const int DepthTypePlanar = 0;
static const int DepthTypeLinear = 1;

// The shadow map type.
static const int SMTypeDefault = 0; // Normal shadow map containing only the depth.
static const int SMTypeVsm = 1;     // Variance shadow map using unbiased second depth moment.


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float3 LodCameraPosition : LODCAMERAPOSITION;   // We need the normal camera for terrain not the shadow camera!!!

// The camera is the light camera not the player's camera.
float CameraNear : CAMERANEAR;
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

#if PIXEL_HOLES
float TerrainMipmapBias;
texture TerrainDetailClipmap0;
sampler TerrainDetailClipmapSampler0 = sampler_state
{
  Texture = <TerrainDetailClipmap0>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MINFILTER = ANISOTROPIC;
  MAGFILTER = LINEAR;
  MIPFILTER = NONE;
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
#endif


//-----------------------------------------------------------------------------
// Structs
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
};


struct VSOutput
{
  float3 DepthOrPositionView : TEXCOORD0;
#if PIXEL_HOLES
  float3 PositionWorld: TEXCOORD1;
#endif
  float4 Position : SV_Position;
};


struct PSInput
{
  float3 DepthOrPositionView : TEXCOORD0; // Stores depth in x, or position in xyz.
#if PIXEL_HOLES
  float3 PositionWorld: TEXCOORD1;
#endif
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, int depthType, float near)
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
  
  // ----- Output
  VSOutput output = (VSOutput)0;
  output.Position = positionProj;
  if (depthType == DepthTypePlanar)
  {
    // Compute planar view space distance (normalized).
    output.DepthOrPositionView.x = (-positionView.z - near) / (CameraFar - near);
  }
  else
  {
    // Pass position in view space to pixel shader.
    output.DepthOrPositionView = positionView.xyz;
  }
#if PIXEL_HOLES
  output.PositionWorld = position;
#endif
  return output;
}


float4 PS(PSInput input, uniform int depthType, uniform int smType) : COLOR
{
#if PIXEL_HOLES
  float3 positionWorld = input.PositionWorld;
  float specularPower, alpha = 0;
  float3 normal = 0, diffuse = 0, specular = 0;
  ComputeTerrainMaterial(
    TerrainDetailClipmapSampler0,
    TerrainDetailClipmapSampler0,
    TerrainDetailClipmapSampler0,
    TerrainDetailClipmapOrigins, TerrainDetailClipmapCellSizes, TerrainDetailClipmapCellsPerLevel,
    TerrainDetailClipmapNumberOfLevels, TerrainDetailClipmapNumberOfColumns,
    TerrainDetailClipmapLevelBias, TerrainDetailClipmapOffsets, TerrainDetailFadeRange,
    TerrainEnableAnisotropicFiltering,
    positionWorld, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    normal, specularPower, diffuse, alpha, specular);
  
  clip(alpha - TerrainHoleThreshold);
#endif
  
  float depth;
  if (depthType == DepthTypePlanar)
  {
    // Get normalized planar distance to near plane.
    depth = input.DepthOrPositionView.x;
  }
  else
  {
    // Compute normalized linear distance to camera.
    float3 positionView = input.DepthOrPositionView.xyz;
    depth = length(positionView) / CameraFar;
  }
  
  if (smType == SMTypeDefault)
  {
    return float4(depth.x, 0, 0, 1);
  }
  else if (smType == SMTypeVsm)
  {
#if VSM_BIAS
    bool useBias = true;
#else
    bool useBias = false;
#endif
    float2 moments = GetDepthMoments(depth, useBias);
    return float4(moments.x, moments.y, 0, 1);
  }
}


// TODO: MonoGame does not support parameters like this:
//   VertexShader = compile vs_2_0 VSNoInstancing(DepthTypePlanar);
// Therefore we need to add functions without uniform parameters.
VSOutput VSPlanar(VSInput input) { return VS(input, DepthTypePlanar, CameraNear); }
VSOutput VSPlanarPancaking(VSInput input) { return VS(input, DepthTypePlanar, 0); }
VSOutput VSLinear(VSInput input) { return VS(input, DepthTypeLinear, CameraNear); }

float4 PSPlanarDefault(PSInput input) : COLOR { return PS(input, DepthTypePlanar, SMTypeDefault); }
float4 PSPlanarVsm(PSInput input) : COLOR { return PS(input, DepthTypePlanar, SMTypeVsm); }
float4 PSLinearDefault(PSInput input) : COLOR { return PS(input, DepthTypeLinear, SMTypeDefault); }


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


technique Default
{
  pass
  {
    VertexShader = compile VSTARGET VSPlanar();
    PixelShader = compile PSTARGET PSPlanarDefault();
  }
}


technique Directional
{
  pass
  {
    VertexShader = compile VSTARGET VSPlanarPancaking();
    PixelShader = compile PSTARGET PSPlanarDefault();
  }
}


technique DirectionalVsm
{
  pass
  {
    //CullMode = NONE;  // Set in C# code.
    VertexShader = compile VSTARGET VSPlanarPancaking();
    PixelShader = compile PSTARGET PSPlanarVsm();
  }
}


technique Omnidirectional
{
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSLinearDefault();
  }
}
