//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file MaterialVegetation.fx
/// Use this effect for plants (e.g. bushes, trees).
/// This effect is derived from the standard Material effect.
/// The vertex shader animates the vertex position to create a swaying
/// animation. Screen-door transparency is used to fade meshes out for LOD.
/// The pixel shader adds a simply translucency effect.
//
//-----------------------------------------------------------------------------


#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Encoding.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Deferred.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Material.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Noise.fxh"
#include "Vegetation.fxh"


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float2 ViewportSize : VIEWPORTSIZE;
float3 CameraPosition : CAMERAPOSITION;

DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer1, 1);

float3 DirectionalLightDiffuse : DIRECTIONALLIGHTDIFFUSE;
float3 DirectionalLightDirection : DIRECTIONALLIGHTDIRECTION;

float3 DiffuseColor : DIFFUSECOLOR;
float3 SpecularColor : SPECULARCOLOR;
float3 TranslucencyColor : TRANSLUCENCYCOLOR;
float ReferenceAlpha : REFERENCEALPHA = 0.9f;
DECLARE_UNIFORM_DIFFUSETEXTURE      // Diffuse (RGB) + Alpha (A)
DECLARE_UNIFORM_SPECULARTEXTURE     // Specular (RGB) + Emissive (A)

float Time : TIME;
float3 Wind : WIND;
float2 WindWaveParameters;  // (frequency (= 1 / wave length), randomness)
float3 SwayFrequencies;     // (trunk, branch, unused)
float3 SwayScales;          // (trunk, branch, leaf)

// min distance, max distance, transtion range
float3 LodDistances < string Hint = "PerInstance"; > = float3(0, 50, 1);

float3 VertexColorMask;

//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION0;
  float3 Normal : NORMAL0;
  float2 TexCoord : TEXCOORD0;
  float3 Color : COLOR0;
};


struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float3 PositionWorld : TEXCOORD1;
  float4 PositionProj : TEXCOORD2;
  float3 Normal : TEXCOORD3;
  float3 VertexColor : TEXCOORD4;
  float Alpha : TEXCOORD5;
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float3 PositionWorld : TEXCOORD1;
  float4 PositionProj : TEXCOORD2;
  float3 Normal : TEXCOORD3;
  float3 VertexColor : TEXCOORD4;
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
  
  VSOutput output = (VSOutput)0;
  output.Position = positionProj;
  output.PositionWorld = positionWorld;
  output.PositionProj = positionProj;
  output.Normal = normalWorld;
  output.TexCoord = input.TexCoord;
  output.VertexColor = input.Color;
  
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


float4 PS(PSInput input) : COLOR0
{
  // Debugging: Visualize vertex colors.
  if (any(VertexColorMask))
    return float4(input.VertexColor * VertexColorMask, 1);
  
  float4 diffuseMap = tex2D(DiffuseSampler, input.TexCoord);
  clip(diffuseMap.a - ReferenceAlpha);
  
  // Screen-door transparency
  float c = input.Alpha - Dither4x4(input.VPos.xy);
  // The alpha can be negative, which means the dither pattern is inverted.
  if (input.Alpha < 0)
    c = -(c + 1);
  
  clip(c);
  
  float4 specularMap = tex2D(SpecularSampler, input.TexCoord);
  float3 diffuse = FromGamma(diffuseMap.rgb);
  float3 specular = FromGamma(specularMap.rgb);
  
  // Get the screen space texture coordinate for this position.
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  
  float4 lightBuffer0Sample = tex2D(LightBuffer0Sampler, texCoordScreen);
  float4 lightBuffer1Sample = tex2D(LightBuffer1Sampler, texCoordScreen);
  
  float3 diffuseLight = GetLightBufferDiffuse(lightBuffer0Sample, lightBuffer1Sample);
  float3 specularLight = GetLightBufferSpecular(lightBuffer0Sample, lightBuffer1Sample);
  
  // ----- Translucency
  // TODO: Move this computation into the vertex shader.
  // View vector pointing to camera.
  float3 V = normalize(CameraPosition - input.PositionWorld);
  // Normal pointing away from surface.
  float3 N = normalize(input.Normal) * sign(input.Face);
  // Direction of light rays (= direction from light to shaded position).
  float3 L = DirectionalLightDirection;
  float VDotL = saturate(dot(V, L));
  VDotL = VDotL * VDotL;
  VDotL = VDotL * VDotL;
  float LDotN = max(0, dot(N, L) * 0.6 + 0.4);
  // TODO: subsurface texture/thickness texture
  //float thickness = sample subsurface/thickness texture map.
  //float thickness = diffuse.g;
  float thickness = 1;
  // Lerp between "view-dependent forward scattering" and "view-independent back lighting".
  float scatter = lerp(VDotL, LDotN, 0.2);
  float3 translucency = DirectionalLightDiffuse * TranslucencyColor * scatter * DiffuseColor * diffuse * thickness;
  
  return float4(
    DiffuseColor * diffuse * diffuseLight
    + SpecularColor * specular * specularLight + translucency,
    1);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique Default
#if !MGFX     // TODO: Add Annotation support to MonoGame.
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
