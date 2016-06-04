//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Refraction.fx
/// Material with mesh skinning, faked reflection, and refraction.
//
// Notes:
// If a mesh is rendered with this effect and Alpha and BlendMode are 1, then
// the wrong z-sorting may appear because the individual triangles are not z-
// sorted. If this artifact is objectionable, try an Alpha or BlendMode value
// less than 1. (The perfect solution would be order-independent transparency...)
//
//-----------------------------------------------------------------------------

#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Material.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Fog.fxh"


//--------------------------------------------------------
// Constants
//--------------------------------------------------------

// The colors 3 selected wavelengths that correspond to the 3 refraction indices.
static const float3 WavelengthColors[3] = { { 1, 0, 0 },
                                            { 0, 1, 0 },
                                            { 0, 0, 1 } };

// ------ Following parameters are set automatically by default delegate effect parameter bindings:
float2 ViewportSize : VIEWPORTSIZE;
float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 ViewProjection : VIEWPROJECTION;
float3 CameraPosition : CAMERAPOSITION;
float4x3 Bones[72] : BONES;
float4 FogColor : FOGCOLOR;            // Color of fog (RGBA). If alpha is 0, fog is disabled.

float4 FogParameters : FOGPARAMETERS;  // (Fog Start, Fog End, Fog Density, Fog Height Falloff)
#define FogStart FogParameters.x
#define FogEnd FogParameters.y
#define FogDensity FogParameters.z
#define FogHeightFalloff FogParameters.w

texture SourceTexture : SOURCETEXTURE;
sampler2D SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
  MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = POINT;
  AddressU = CLAMP;
  AddressV = CLAMP;
};

// ----- Following parameters are set in the *.drmat files.
float3 Tint = float3(0.7, 0.7, 0.9);
float3 RefractionIndices = { 0.4, 0.6, 0.8 }; // { 0.80, 0.82, 0.84 };
float RefractionStrength = 0.04f;
float3 FresnelParameters = float3(0.0, 1, 3);
#define FresnelBias FresnelParameters.x
#define FresnelScale FresnelParameters.y
#define FresnelPower FresnelParameters.z

float Alpha = 1;
float BlendMode = 1;        // [0, 1], 0 = additive blending, 1 = alpha blending

DECLARE_UNIFORM_NORMALTEXTURE


//-----------------------------------------------------------------------------
// Input, output
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
  float3 Normal  : NORMAL;
  float3 Tangent : TANGENT;
  float3 Binormal : BINORMAL;
  uint4 BoneIndices : BLENDINDICES;
  float4 BoneWeights : BLENDWEIGHT;
};


struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionWorldAndFog : TEXCOORD1;  // W contains 1- Fog Intensity.
  float3 Normal : TEXCOORD2;
  float3 Tangent : TEXCOORD3;
  float3 Binormal : TEXCOORD4;
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionWorldAndFog : TEXCOORD1;
  float3 Normal : TEXCOORD2;
  float3 Tangent : TEXCOORD3;
  float3 Binormal : TEXCOORD4;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.TexCoord = input.TexCoord;

  // Mesh skinning.
  float4x3 skinningMatrix = 0;
  skinningMatrix += Bones[input.BoneIndices.x] * input.BoneWeights.x;
  skinningMatrix += Bones[input.BoneIndices.y] * input.BoneWeights.y;
  skinningMatrix += Bones[input.BoneIndices.z] * input.BoneWeights.z;
  skinningMatrix += Bones[input.BoneIndices.w] * input.BoneWeights.w;
  input.Position.xyz = mul(input.Position, skinningMatrix);
  input.Normal.xyz = mul(input.Normal, (float3x3)skinningMatrix);
  input.Tangent.xyz = mul(input.Tangent, (float3x3)skinningMatrix);
  input.Binormal.xyz = mul(input.Binormal, (float3x3)skinningMatrix);
  
  // World transform.
  output.Normal =  mul(input.Normal, (float3x3)World);
  output.Tangent = mul(input.Tangent, (float3x3)World);
  output.Binormal = mul(input.Binormal, (float3x3)World);
  float4 positionWorld = mul(input.Position, World);
  output.PositionWorldAndFog = float4(positionWorld.xyz, 1);
  
  // Projected position.
  output.Position = mul(positionWorld, ViewProjection);
  
  // Fog
  if (FogColor.a > 0)
  {
    float3 cameraToPositionVector = positionWorld.xyz - CameraPosition;
    float cameraDistance = length(cameraToPositionVector);

    // Smoothstep distance fog
    float smoothRamp = ComputeSmoothFogIntensity(cameraDistance, FogStart, FogEnd);
    
    // Exponential height-based fog
    float opticalLength = GetOpticalLengthInHeightFog(cameraDistance, FogDensity, cameraToPositionVector, FogHeightFalloff);
    float exponentialFog = ComputeExponentialFogIntensity(opticalLength, 1);  // fogDensity is included in opticalLength!

    // We use this fog parameter to scale the alpha in the pixel shader. The correct
    // way would be to apply a fog color, but scaling the alpha looks ok and works
    // with non-uniform/view-dependent fog colors.
    float fog = smoothRamp * exponentialFog * FogColor.a;
    output.PositionWorldAndFog.w = (1 - fog);
  }

  return output;
}


float4 PS(VSOutput input) : COLOR
{
  // Normalize tangent space vectors.
  float3 normal = normalize(input.Normal);
  float3 tangent = normalize(input.Tangent);
  float3 binormal = normalize(input.Binormal);
  
  // Apply normal mapping. (Normals maps are encoded using DXT5nm.)
  float3 normalMapSample = GetNormalDxt5nm(NormalSampler, input.TexCoord);
  normal = normal * normalMapSample.z + tangent * normalMapSample.x - binormal * normalMapSample.y;
  
  // Get position and view direction.
  float3 position = input.PositionWorldAndFog.xyz;
  float3 viewDirection = normalize(position - CameraPosition);
  
  // Reflection mapping using fake reflections:
  // The reflected view vector is used to make a lookup in the back buffer using
  // sphere mapping. This is not correct but looks plausible/interesting.
  float3 reflectedDirection = mul(reflect(viewDirection, normal), (float3x3)View);
  float2 sphereMapTexCoord = float2(reflectedDirection.x / 2 + 0.5, -reflectedDirection.y / 2 + 0.5);
  float3 reflectionColor = tex2D(SourceSampler, sphereMapTexCoord).rgb;
  
  // Refraction:
  // We sample three times using different refraction indices to get chromatic aberration.
  // The refracted direction is added to the current position. The resulting position is
  // converted to screen space and looked up in the backbuffer texture.
  float3 refractionColor = 0;
  for(int i = 0; i < 3; i++)
  {
    float3 refractedDirection = refract(-viewDirection, normal, RefractionIndices[i]);
    float4 refractedPosition = float4(position + refractedDirection * RefractionStrength, 1);
    float4 refractedPositionProj = mul(refractedPosition, ViewProjection);
    float2 texCoordScreen = ProjectionToScreen(refractedPositionProj, ViewportSize);
    refractionColor += tex2D(SourceSampler, texCoordScreen).rgb * WavelengthColors[i];
  }

  // Compute approximate fresnel term and use it to lerp between refraction and reflection.
  float fresnel = saturate(FresnelBias + FresnelScale * pow(abs(1.0 - max(dot(normal, -viewDirection), 0)), FresnelPower));
  float3 result = lerp(refractionColor, reflectionColor, fresnel) * Tint;
  
  // We use the fog to scale the alpha value instead of applying a fog color.
  // (This looks ok and works for non-uniform fog colors.)
  float fog = input.PositionWorldAndFog.w;
  float alpha = abs(Alpha * fog);

  // Premultiply alpha.
  result *= alpha;

  return float4(result, alpha * BlendMode);
}


#if !SM4
  #define VSTARGET vs_3_0
  #define PSTARGET ps_3_0
#else
  #define VSTARGET vs_4_0_level_9_3
  #define PSTARGET ps_4_0_level_9_3
#endif

technique
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
