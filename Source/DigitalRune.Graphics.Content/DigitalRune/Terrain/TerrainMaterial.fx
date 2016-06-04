//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file TerrainMaterial.fx
/// Renders the "Material" render pass of a terrain node.
/// Per default this effect uses vertex-based holes and no parallax occlusion
/// mapping.
//
//-----------------------------------------------------------------------------


#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"
#include "../Material.fxh"
#include "../Noise.fxh"
#include "../Color.fxh"


#define MAX_ANISOTROPY 8
#include "Terrain.fxh"

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
float2 ViewportSize : VIEWPORTSIZE;
float3 LodCameraPosition : LODCAMERAPOSITION;

DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer1, 1);

float3 DiffuseColor : DIFFUSECOLOR = float3(1, 1, 1);
float3 SpecularColor : SPECULARCOLOR = float3(1, 1, 1);
//#if EMISSIVE
//float3 EmissiveColor : EMISSIVECOLOR;
//#endif
DECLARE_UNIFORM_DIFFUSETEXTURE      // Diffuse (RGB) + Alpha (A)
DECLARE_UNIFORM_SPECULARTEXTURE     // Specular (RGB) + Emissive (A)

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

texture TerrainDetailClipmap1;
sampler TerrainDetailClipmapSampler1 = sampler_state
{
  Texture = <TerrainDetailClipmap1>;
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
  float4 PositionProj : TEXCOORD0;
  float LOD : TEXCOORD1;
  float3 PositionWorld: TEXCOORD2;
#if PARALLAX_MAPPING
  float3 ViewDirectionTangent : TEXCOORD3; // View direction in tangent space.
  float3 LightDirectionTangent : TEXCOORD4;  // LightDirection in tangent space.
#endif
  float4 Position : SV_Position;
};


struct PSInput
{
  float4 PositionProj : TEXCOORD0;
  float LOD : TEXCOORD1;
  float3 PositionWorld: TEXCOORD2;
#if PARALLAX_MAPPING
  float3 ViewDirectionTangent : TEXCOORD3; // View direction in tangent space.
  float3 LightDirectionTangent : TEXCOORD4;  // LightDirection in tangent space.
#endif
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, uniform bool isWireFrame)
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
  
  // Offset wireframe above normal terrain.
  if (isWireFrame)
    position.y += 0.01f;// * exp2(lod);
  
  float4 positionView = mul(float4(position, 1), View);
  float4 positionProj = mul(positionView, Projection);
  
  // ----- Output
  VSOutput output = (VSOutput)0;
  output.Position = positionProj;
  output.PositionProj = positionProj;
  output.LOD = clod;
  output.PositionWorld = position;
  
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


float4 GetDebugColor(float x)
{
  float3 c = FromHsv(float3(x, 1, 1));
  return float4(c.rgb, 1);
  
  //int index = round(x);
  //if (index == 0) return float4(0, 0, 0, 1);
  //if (index == 1) return float4(1, 0, 0, 1);
  //if (index == 2) return float4(1, 1, 0, 1);
  //if (index == 3) return float4(0, 1, 0, 1);
  //if (index == 4) return float4(0, 1, 1, 1);
  //if (index == 5) return float4(0, 0, 1, 1);
  //if (index == 6) return float4(1, 0, 1, 1);
  //return float4(1, 1, 1, 1);
}

float4 PS(PSInput input, uniform bool isWireFrame) : COLOR0
{
  float3 positionWorld = input.PositionWorld;
  float lod = round(input.LOD);
  
  if (isWireFrame)
  {
    float clod = input.LOD;
    float clodLow = floor(clod);
    float clodHigh = min(clodLow + 1, TerrainBaseClipmapNumberOfLevels - 1);
    float clodFrac = clod - clodLow;
    return GetDebugColor(input.LOD / 7);
    return lerp(GetDebugColor(clodLow / 7), GetDebugColor(clodHigh / 7), clodFrac);
  }
  
  float specularPower, alpha = 0;
  float3 normal = 0, diffuse = 0, specular = 0;
#if PARALLAX_MAPPING
  ComputeTerrainMaterial(
    TerrainDetailClipmapSampler0,
    TerrainDetailClipmapSampler1,
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
    normal, specularPower, diffuse, alpha, specular);
#else
  ComputeTerrainMaterial(
    TerrainDetailClipmapSampler0,
    TerrainDetailClipmapSampler1,
    TerrainDetailClipmapSampler2,
    TerrainDetailClipmapOrigins, TerrainDetailClipmapCellSizes, TerrainDetailClipmapCellsPerLevel,
    TerrainDetailClipmapNumberOfLevels, TerrainDetailClipmapNumberOfColumns,
    TerrainDetailClipmapLevelBias, TerrainDetailClipmapOffsets, TerrainDetailFadeRange,
    TerrainEnableAnisotropicFiltering,
    positionWorld, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    normal, specularPower, diffuse, alpha, specular);
#endif
  
#if PIXEL_HOLES
  clip(alpha - TerrainHoleThreshold);
#endif
  
  // Get the screen space texture coordinate for this position.
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  
  float4 lightBuffer0Sample = tex2D(LightBuffer0Sampler, texCoordScreen);
  float4 lightBuffer1Sample = tex2D(LightBuffer1Sampler, texCoordScreen);
  
  float3 diffuseLight = GetLightBufferDiffuse(lightBuffer0Sample, lightBuffer1Sample);
  float3 specularLight = GetLightBufferSpecular(lightBuffer0Sample, lightBuffer1Sample);
  
  return float4(DiffuseColor * diffuse * diffuseLight + SpecularColor * specular * specularLight, 1);
}

VSOutput VSDefault(VSInput input) { return VS(input, false); }
float4 PSDefault(PSInput input) : COLOR0 { return PS(input, false); }
VSOutput VSWireFrame(VSInput input) { return VS(input, true); }
float4 PSWireFrame(PSInput input) : COLOR0 { return PS(input, true); }


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique Default
{
  pass Default
  {
#if !SM4
    VertexShader = compile vs_3_0 VSDefault();
    PixelShader = compile ps_3_0 PSDefault();
#else
    VertexShader = compile vs_4_0 VSDefault();
    PixelShader = compile ps_4_0 PSDefault();
#endif
  }
  
  
  pass WireFrame
  {
    CullMode = CCW;
    FillMode = WIREFRAME;
    
#if !SM4
    VertexShader = compile vs_3_0 VSWireFrame();
    PixelShader = compile ps_3_0 PSWireFrame();
#else
    VertexShader = compile vs_4_0 VSWireFrame();
    PixelShader = compile ps_4_0 PSWireFrame();
#endif
  }
  
  pass Restore
  {
    FillMode = SOLID;
    CullMode = CCW;
  }
}
