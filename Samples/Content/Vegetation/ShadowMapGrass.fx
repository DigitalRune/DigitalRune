//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ShadowMapGrass.fx
/// Use this effect for grass meshes/billboards.
/// This effect is derived from the standard ShadowMap effect.
/// The vertex shader animates the vertex position to create a swaying
/// animation.
//
//-----------------------------------------------------------------------------

#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Material.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Noise.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/ShadowMap.fxh"
#include "Vegetation.fxh"


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

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;

// The camera is the light camera not the player's camera.
float CameraNear : CAMERANEAR;
float CameraFar : CAMERAFAR;

float ReferenceAlpha = 0.9f;
DECLARE_UNIFORM_DIFFUSETEXTURE

// Wind and sway parameters.
float Time : TIME;
float3 Wind : WIND;
float2 WindWaveParameters;  // (frequency (= 1 / wave length), randomness)
float3 SwayFrequencies;     // (trunk, branch, unused)
float3 SwayScales;          // (trunk, branch, leaf)


//-----------------------------------------------------------------------------
// Structs
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;
  float3 Normal : NORMAL0;
};


struct VSOutput
{
  float3 DepthOrPositionView : TEXCOORD0;
  float2 TexCoord : TEXCOORD1;
  float4 Position : SV_Position;
};


struct PSInput
{
  float3 DepthOrPositionView : TEXCOORD0; // Stores depth in x, or position in xyz.
  float2 TexCoord : TEXCOORD1;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, float4x4 world, int depthType, float near)
{
  float3 positionWorld = mul(input.Position, world).xyz;
  float3 normalWorld = mul(input.Normal, (float3x3)world);
  
  // Compute wind sway offset.
  float3 swayOffset = ComputeSwayOffset(
    world, positionWorld, normalWorld,
    Wind, WindWaveParameters, Time,
    SwayFrequencies.x, SwayFrequencies.y, 0,
    SwayScales.x * input.Position.y,
    0,
    SwayScales.z * input.Position.y);
  
  positionWorld += swayOffset;
  
  float3 positionView = mul(float4(positionWorld, 1), View).xyz;
  float4 positionProj = mul(float4(positionView, 1), Projection);
  
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
  
  output.TexCoord = input.TexCoord;
  
  return output;
}


VSOutput VSNoInstancing(VSInput input, uniform int depthType, uniform float near)
{
  return VS(input, World, depthType, near);
}


VSOutput VSInstancing(VSInput input,
                      float4 worldColumn0 : BLENDWEIGHT0,
                      float4 worldColumn1 : BLENDWEIGHT1,
                      float4 worldColumn2 : BLENDWEIGHT2,
                      float4 colorAndAlpha : BLENDWEIGHT3,
                      uniform int depthType,
                      uniform float near)
{
  float4x4 worldTransposed =
  {
    worldColumn0,
    worldColumn1,
    worldColumn2,
    float4(0, 0, 0, 1)
  };
  float4x4 world = transpose(worldTransposed);
  
  return VS(input, world, depthType, near);
}


float4 PS(PSInput input, uniform int depthType, uniform int smType) : COLOR
{
  float4 diffuse = tex2D(DiffuseSampler, input.TexCoord);
  clip(diffuse.a - ReferenceAlpha);
  
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
VSOutput VSNoInstancingPlanar(VSInput input) { return VSNoInstancing(input, DepthTypePlanar, CameraNear); }
VSOutput VSNoInstancingPlanarPancaking(VSInput input) { return VSNoInstancing(input, DepthTypePlanar, 0); }
VSOutput VSNoInstancingLinear(VSInput input) { return VSNoInstancing(input, DepthTypeLinear, CameraNear); }
VSOutput VSInstancingPlanar(VSInput input, float4 worldColumn0 : BLENDWEIGHT0, float4 worldColumn1 : BLENDWEIGHT1, float4 worldColumn2 : BLENDWEIGHT2, float4 colorAndAlpha : BLENDWEIGHT3)
{
  return VSInstancing(input, worldColumn0, worldColumn1, worldColumn2, colorAndAlpha, DepthTypePlanar, CameraNear);
}
VSOutput VSInstancingPlanarPancaking(VSInput input, float4 worldColumn0 : BLENDWEIGHT0, float4 worldColumn1 : BLENDWEIGHT1, float4 worldColumn2 : BLENDWEIGHT2, float4 colorAndAlpha : BLENDWEIGHT3)
{
  return VSInstancing(input, worldColumn0, worldColumn1, worldColumn2, colorAndAlpha, DepthTypePlanar, 0);
}
VSOutput VSInstancingLinear(VSInput input, float4 worldColumn0 : BLENDWEIGHT0, float4 worldColumn1 : BLENDWEIGHT1, float4 worldColumn2 : BLENDWEIGHT2, float4 colorAndAlpha : BLENDWEIGHT3)
{
  return VSInstancing(input, worldColumn0, worldColumn1, worldColumn2, colorAndAlpha, DepthTypeLinear, CameraNear);
}

float4 PSPlanarDefault(PSInput input) : COLOR { return PS(input, DepthTypePlanar, SMTypeDefault); }
float4 PSPlanarVsm(PSInput input) : COLOR { return PS(input, DepthTypePlanar, SMTypeVsm); }
float4 PSLinearDefault(PSInput input) : COLOR { return PS(input, DepthTypeLinear, SMTypeDefault); }


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET_2_0 vs_2_0
#define PSTARGET_2_0 ps_2_0
#define VSTARGET_3_0 vs_3_0
#define PSTARGET_3_0 ps_3_0
#else
#define VSTARGET_2_0 vs_4_0_level_9_1
#define PSTARGET_2_0 ps_4_0_level_9_1
#define VSTARGET_3_0 vs_4_0_level_9_3
#define PSTARGET_3_0 ps_4_0_level_9_3
#endif


technique Default
#if !MGFX
< string InstancingTechnique = "DefaultInstancing"; >
#endif
{
  pass
  {
    CullMode = NONE;
    VertexShader = compile VSTARGET_2_0 VSNoInstancingPlanar();
    PixelShader = compile PSTARGET_2_0 PSPlanarDefault();
  }
}


technique DefaultInstancing
{
  pass
  {
    CullMode = NONE;
    VertexShader = compile VSTARGET_3_0 VSInstancingPlanar();
    PixelShader = compile PSTARGET_3_0 PSPlanarDefault();
  }
}


technique Directional
#if !MGFX
< string InstancingTechnique = "DirectionalInstancing"; >
#endif
{
  pass
  {
    CullMode = NONE;
    VertexShader = compile VSTARGET_2_0 VSNoInstancingPlanarPancaking();
    PixelShader = compile PSTARGET_2_0 PSPlanarDefault();
  }
}


technique DirectionalInstancing
{
  pass
  {
    CullMode = NONE;
    VertexShader = compile VSTARGET_3_0 VSInstancingPlanarPancaking();
    PixelShader = compile PSTARGET_3_0 PSPlanarDefault();
  }
}


technique DirectionalVsm
#if !MGFX
< string InstancingTechnique = "DirectionalVsmInstancing"; >
#endif
{
  pass
  {
    //CullMode = NONE;  // Set in C# code
    VertexShader = compile VSTARGET_3_0 VSNoInstancingPlanarPancaking();
    PixelShader = compile PSTARGET_3_0 PSPlanarVsm();
  }
}


technique DirectionalVsmInstancing
{
  pass
  {
    //CullMode = NONE; // Set in C# code.
    VertexShader = compile VSTARGET_3_0 VSInstancingPlanarPancaking();
    PixelShader = compile PSTARGET_3_0 PSPlanarVsm();
  }
}


technique Omnidirectional
#if !MGFX
< string InstancingTechnique = "OmnidirectionalInstancing"; >
#endif
{
  pass
  {
    CullMode = NONE;
    VertexShader = compile VSTARGET_2_0 VSNoInstancingLinear();
    PixelShader = compile PSTARGET_2_0 PSLinearDefault();
  }
}


technique OmnidirectionalInstancing
{
  pass
  {
    CullMode = NONE;
    VertexShader = compile VSTARGET_3_0 VSInstancingLinear();
    PixelShader = compile PSTARGET_3_0 PSLinearDefault();
  }
}
