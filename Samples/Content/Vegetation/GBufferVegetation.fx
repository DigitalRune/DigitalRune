//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file GBufferVegetation.fx
/// Use this effect for plants (e.g. bushes, trees).
/// This effect is derived from the standard GBuffer effect.
/// The vertex shader animates the vertex position to create a swaying
/// animation. Screen-door transparency is used to fade meshes out for LOD.
//
//-----------------------------------------------------------------------------

#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Encoding.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Material.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Noise.fxh"
#include "Vegetation.fxh"

// CREATE_GBUFFER creates automatically bound shader constants required for
// encoding data in the G-Buffer.
#define CREATE_GBUFFER 1
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Deferred.fxh"


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

#define NORMAL_MAP 1

//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float CameraFar : CAMERAFAR;
float3 CameraPosition : CAMERAPOSITION;

float SpecularPower : SPECULARPOWER;
float ReferenceAlpha = 0.9f;
DECLARE_UNIFORM_DIFFUSETEXTURE

#if NORMAL_MAP
DECLARE_UNIFORM_NORMALTEXTURE
#endif

float SceneNodeType : SCENENODETYPE;

float Time : TIME;
float3 Wind : WIND;
float2 WindWaveParameters;  // (frequency (= 1 / wave length), randomness)
float3 SwayFrequencies;     // (trunk, branch, unused)
float3 SwayScales;          // (trunk, branch, leaf)

// min distance, max distance, transtion range
float3 LodDistances < string Hint = "PerInstance"; > = float3(0, 50, 1);

//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
  float3 Normal : NORMAL;
#if NORMAL_MAP
  float3 Tangent : TANGENT;
  float3 Binormal : BINORMAL;
#endif
  float3 Color : COLOR;
};


struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float Depth : TEXCOORD1;
  float3 Normal : TEXCOORD2;
#if NORMAL_MAP
  float3 Tangent : TEXCOORD3;
  float3 Binormal : TEXCOORD4;
#endif
  float Alpha : TEXCOORD5;
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float Depth : TEXCOORD1;
  float3 Normal : TEXCOORD2;
#if NORMAL_MAP
  float3 Tangent : TEXCOORD3;
  float3 Binormal : TEXCOORD4;
#endif
  float Alpha : TEXCOORD5;
#if SM4
  float4 VPos : SV_Position;
#else
  float2 VPos : VPOS;
#endif
  float Face : VFACE;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, float4x4 world)
{
  float3 positionWorld = mul(input.Position, world).xyz;
  float3 normalWorld = mul(input.Normal, (float3x3)world);
#if NORMAL_MAP
  float3 tangentWorld = mul(input.Tangent, (float3x3)world);
  float3 binormalWorld = mul(input.Binormal, (float3x3)world);
#endif
  
  // Compute wind sway offset.
  float3 swayOffset = ComputeSwayOffset(
    world, positionWorld, normalWorld,
    Wind, WindWaveParameters, Time,
    SwayFrequencies.x, SwayFrequencies.y, input.Color.g,
    SwayScales.x * input.Position.y,
    SwayScales.y * input.Color.r,
    SwayScales.z * input.Color.b);
  
  positionWorld += swayOffset;
  
  float3 positionView = mul(float4(positionWorld, 1), View).xyz;
  float4 positionProj = mul(float4(positionView, 1), Projection);
  float normalizedDepth = -positionView.z / CameraFar;
  
  VSOutput output = (VSOutput)0;
  output.Position = positionProj;
  output.TexCoord = input.TexCoord;
  output.Depth = normalizedDepth;
  output.Normal = normalWorld;
#if NORMAL_MAP
  output.Tangent = tangentWorld;
  output.Binormal = binormalWorld;
#endif
  
#if !MGFX
  // This is a near-1 value which can be multiplied to effect parameters to
  // workaround a DX9 HLSL compiler preshader bug.
  float dummy1 = 1 + positionWorld.y * 1e-30f;
#else
  float dummy1 = 1;
#endif
  
  // Compute alpha value for LOD fade in/out.
  float3 plantPosition = world._m30_m31_m32;  // = mul(float4(0, 0, 0, 1), world)
  float dist = length(CameraPosition - plantPosition);
  float fadeInAlpha = 1 - max(0, min(1, (LodDistances.x - dist) / LodDistances.z) * dummy1);
  float fadeOutAlpha = max(0, min(1, (LodDistances.y - dist) / LodDistances.z) * dummy1);
  if (fadeInAlpha < fadeOutAlpha)
    output.Alpha = -fadeInAlpha;   // Negative alpha to invert screen-door dither pattern.
  else
    output.Alpha = fadeOutAlpha;
  
  // If alpha is 0, move all vertices outside the camera frustum.
  if (abs(output.Alpha * dummy1) < 0.00001f)
    output.Position = 0;
  
  return output;
}


VSOutput VSNoInstancing(VSInput input)
{
  return VS(input, World);
}


VSOutput VSInstancing(VSInput input,
                      float4 worldColumn0 : BLENDWEIGHT0,
                      float4 worldColumn1 : BLENDWEIGHT1,
                      float4 worldColumn2 : BLENDWEIGHT2,
                      float4 colorAndAlpha : BLENDWEIGHT3)
{
  float4x4 worldTransposed =
  {
    worldColumn0,
    worldColumn1,
    worldColumn2,
    float4(0, 0, 0, 1)
  };
  float4x4 world = transpose(worldTransposed);
  
  return VS(input, world);
}


void PS(PSInput input, out float4 depthBuffer : COLOR0, out float4 normalBuffer : COLOR1)
{
  float4 diffuse = tex2D(DiffuseSampler, input.TexCoord);
  clip(diffuse.a - ReferenceAlpha);
  
  // Screen-door transparency
  float c = input.Alpha - Dither4x4(input.VPos.xy);
  // The alpha can be negative, which means the dither pattern is inverted.
  if (input.Alpha < 0)
    c = -(c + 1);
  
  clip(c);
  
  // Normalize tangent space vectors.
  float3 normal = normalize(input.Normal);
#if NORMAL_MAP
  float3 tangent = normalize(input.Tangent);
  float3 binormal = normalize(input.Binormal);
  
  // Normals maps are encoded using DXT5nm.
  float3 normalMapSample = GetNormalDxt5nm(NormalSampler, input.TexCoord);
  normal = normal * normalMapSample.z + tangent * normalMapSample.x - binormal * normalMapSample.y;
#endif
  
  normal = normal * sign(input.Face);
  
#if !MGFX
  // Hack (XNA only): The following multiplication is completely unnecessary,
  // however there is a bug in XNA's fx compiler that prevents the code from
  // compiling otherwise.
  float sceneNodeType = SceneNodeType * (input.Alpha != 0);
#else
  // The correct line:
  float sceneNodeType = SceneNodeType;
#endif
  
  depthBuffer = 1;
  normalBuffer = float4(0, 0, 0, 1);
  SetGBufferDepth(input.Depth, sceneNodeType, depthBuffer);
  SetGBufferNormal(normal.xyz, normalBuffer);
  SetGBufferSpecularPower(SpecularPower, depthBuffer, normalBuffer);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique Default
#if !MGFX           // TODO: Add Annotation support to MonoGame.
< string InstancingTechnique = "DefaultInstancing"; >
#endif
{
  pass
  {
    CullMode = NONE;
    
#if !SM4
    VertexShader = compile vs_3_0 VSNoInstancing();
    PixelShader = compile ps_3_0 PS();
#else
    VertexShader = compile vs_4_0 VSNoInstancing();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}

technique DefaultInstancing
{
  pass
  {
    CullMode = NONE;
    
#if !SM4
    VertexShader = compile vs_3_0 VSInstancing();
    PixelShader = compile ps_3_0 PS();
#else
    VertexShader = compile vs_4_0 VSInstancing();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}
