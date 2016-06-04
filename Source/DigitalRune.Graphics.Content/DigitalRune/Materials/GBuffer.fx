//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file GBuffer.fx
/// Renders the model into the G-buffer.
/// Supports:
/// - Specular power
//
// ----- Morphing (morph target animation)
// When morphing is enabled the binormal is excluded from the vertex attributes
// to stay within the max number of vertex attributes. The binormal is derived
// from normal and tangent in the vertex shader.
// References:
// - C. Beeson & K. Bjorke: Curtis Beeson: Skin in the "Dawn" Demo,
//   http://http.developer.nvidia.com/GPUGems/gpugems_ch03.html
// - C. Beeson, NVIDIA: Animation in the "Dawn" Demo,
//   http://http.developer.nvidia.com/GPUGems/gpugems_ch04.html
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Material.fxh"
#include "../Noise.fxh"

// CREATE_GBUFFER creates automatically bound shader constants required for
// encoding data in the G-Buffer.
#define CREATE_GBUFFER 1
#include "../Deferred.fxh"


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

// Possible defines
//#define NORMAL_MAP 1
//#define ALPHA_TEST 1
//#define TRANSPARENT 1
//#define MORPHING 1
//#define SKINNING 1


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float CameraFar : CAMERAFAR;

float SpecularPower : SPECULARPOWER;

#if ALPHA_TEST
float ReferenceAlpha = 0.9f;
DECLARE_UNIFORM_DIFFUSETEXTURE
#endif
#if TRANSPARENT
float InstanceAlpha : INSTANCEALPHA = 1;
#endif

#if NORMAL_MAP
DECLARE_UNIFORM_NORMALTEXTURE
#endif

#if MORPHING
float MorphWeight0 : MORPHWEIGHT0;
float MorphWeight1 : MORPHWEIGHT1;
float MorphWeight2 : MORPHWEIGHT2;
float MorphWeight3 : MORPHWEIGHT3;
float MorphWeight4 : MORPHWEIGHT4;
#endif

#if SKINNING
float4x3 Bones[72];
#endif

float SceneNodeType : SCENENODETYPE;


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION0;
#if ALPHA_TEST || NORMAL_MAP
  float2 TexCoord : TEXCOORD0;
#endif
  float3 Normal : NORMAL0;
#if NORMAL_MAP
  float3 Tangent : TANGENT0;
#if !MORPHING   // Exclude binormal when morphing is enabled.
  float3 Binormal : BINORMAL0;
#endif
#endif
#if MORPHING
  float3 MorphPosition0 : POSITION1;
  float3 MorphNormal0 : NORMAL1;
  float3 MorphPosition1 : POSITION2;
  float3 MorphNormal1 : NORMAL2;
  float3 MorphPosition2: POSITION3;
  float3 MorphNormal2: NORMAL3;
  float3 MorphPosition3 : POSITION4;
  float3 MorphNormal3: NORMAL4;
  float3 MorphPosition4 : POSITION5;
  float3 MorphNormal4 : NORMAL5;
#endif
#if SKINNING
  uint4 BoneIndices : BLENDINDICES0;
  float4 BoneWeights : BLENDWEIGHT0;
#endif
};


struct VSOutput
{
#if ALPHA_TEST || NORMAL_MAP
  float2 TexCoord : TEXCOORD0;
#endif
  float Depth : TEXCOORD1;
  float3 Normal : TEXCOORD2;
#if NORMAL_MAP
  float3 Tangent : TEXCOORD3;
  float3 Binormal : TEXCOORD4;
#endif
  float4 InstanceColorAndAlpha : TEXCOORD5;
  float4 Position : SV_Position;
};


struct PSInput
{
#if ALPHA_TEST || NORMAL_MAP
  float2 TexCoord : TEXCOORD0;
#endif
  float Depth : TEXCOORD1;
  float3 Normal : TEXCOORD2;
#if NORMAL_MAP
  float3 Tangent : TEXCOORD3;
  float3 Binormal : TEXCOORD4;
#endif
  float4 InstanceColorAndAlpha : TEXCOORD5;
#if TRANSPARENT
#if SM4
  float4 VPos : SV_Position;
#else
  float2 VPos : VPOS;
#endif
#endif
#if ALPHA_TEST
  float Face : VFACE;
#endif
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, float4x4 world, float alpha)
{
  float4 position = input.Position;
  float3 normal = input.Normal;
#if NORMAL_MAP
  float3 tangent = input.Tangent;
#if !MORPHING
  float3 binormal = input.Binormal;
#endif
#endif
  
#if MORPHING
  // ----- Apply morph targets.
  position.xyz += MorphWeight0 * input.MorphPosition0;
  position.xyz += MorphWeight1 * input.MorphPosition1;
  position.xyz += MorphWeight2 * input.MorphPosition2;
  position.xyz += MorphWeight3 * input.MorphPosition3;
  position.xyz += MorphWeight4 * input.MorphPosition4;
  
  normal += MorphWeight0 * input.MorphNormal0;
  normal += MorphWeight1 * input.MorphNormal1;
  normal += MorphWeight2 * input.MorphNormal2;
  normal += MorphWeight3 * input.MorphNormal3;
  normal += MorphWeight4 * input.MorphNormal4;
  normal = normalize(normal);
  
#if NORMAL_MAP
  // Orthonormalize the neutral tangent against the new normal. (Subtract the
  // collinear elements of the new normal from the neutral tangent and normalize.)
  tangent = tangent - dot(tangent, normal) * normal;
  //tangent = normalize(tangent); Tangent is normalized in pixel shader.
#endif
#endif
  
#if SKINNING
  // ----- Apply skinning matrix.
  float4x3 skinningMatrix = (float4x3)0;
  skinningMatrix += Bones[input.BoneIndices.x] * input.BoneWeights.x;
  skinningMatrix += Bones[input.BoneIndices.y] * input.BoneWeights.y;
  skinningMatrix += Bones[input.BoneIndices.z] * input.BoneWeights.z;
  skinningMatrix += Bones[input.BoneIndices.w] * input.BoneWeights.w;
  position.xyz = mul(position, skinningMatrix);
  normal = mul(normal, (float3x3)skinningMatrix);
#if NORMAL_MAP
  tangent = mul(tangent, (float3x3)skinningMatrix);
#if !MORPHING
  binormal = mul(binormal, (float3x3)skinningMatrix);
#endif
#endif
#endif
  
  // ----- Apply world, view, projection transformation.
  float4 positionWorld = mul(position, world);
  float4 positionView = mul(positionWorld, View);
  float4 positionProj = mul(positionView, Projection);
  float normalizedDepth = -positionView.z / CameraFar;
  float3 normalWorld = mul(normal, (float3x3)world);
#if NORMAL_MAP
  float3 tangentWorld = mul(tangent, (float3x3)world);
#if !MORPHING
  float3 binormalWorld = mul(binormal, (float3x3)world);
#else
  // Derive binormal from normal and tangent.
  float3 binormalWorld = cross(normalWorld, tangentWorld);
  //binormalWorld = normalize(binormalWorld); Binormal is normalized in pixel shader.
#endif
#endif
  
  // ----- Output
  VSOutput output = (VSOutput)0;
  output.Position = positionProj;
#if ALPHA_TEST || NORMAL_MAP
  output.TexCoord = input.TexCoord;
#endif
  output.Depth = normalizedDepth;
  output.Normal = normalWorld;
#if NORMAL_MAP
  output.Tangent = tangentWorld;
  output.Binormal = binormalWorld;
#endif
  output.InstanceColorAndAlpha = float4(0, 0, 0, alpha);
  return output;
}


VSOutput VSNoInstancing(VSInput input)
{
#if TRANSPARENT
  return VS(input, World, InstanceAlpha);
#else
  return VS(input, World, 1);
#endif
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
  
  return VS(input, world, colorAndAlpha.a);
}


void PS(PSInput input, out float4 depthBuffer : COLOR0, out float4 normalBuffer : COLOR1)
{
#if ALPHA_TEST
  float4 diffuse = tex2D(DiffuseSampler, input.TexCoord);
  clip(diffuse.a - ReferenceAlpha);
#endif
#if TRANSPARENT
  // Screen-door transparency
  float c = input.InstanceColorAndAlpha.a - Dither4x4(input.VPos.xy);
  // The alpha can be negative, which means the dither pattern is inverted.
  if (input.InstanceColorAndAlpha.a < 0)
    c = -(c + 1);
  
  clip(c);
#endif
  
  // Normalize tangent space vectors.
  float3 normal = normalize(input.Normal);
#if NORMAL_MAP
  float3 tangent = normalize(input.Tangent);
  float3 binormal = normalize(input.Binormal);
  
  // Normals maps are encoded using DXT5nm.
  float3 normalMapSample = GetNormalDxt5nm(NormalSampler, input.TexCoord);
  normal = normal * normalMapSample.z + tangent * normalMapSample.x - binormal * normalMapSample.y;
#endif
  
#if ALPHA_TEST
  normal = normal * sign(input.Face);
#endif
  
  depthBuffer = 1;
  normalBuffer = float4(0, 0, 0, 1);

#if !MGFX
  // Hack (XNA only): The following multiplication is completely unnecessary,
  // however there is a bug in XNA's fx compiler that prevents the code from
  // compiling otherwise.
  float sceneNodeType = SceneNodeType * (input.InstanceColorAndAlpha.a != 0);
#else
  // The correct line:
  float sceneNodeType = SceneNodeType;
#endif
  
  SetGBufferDepth(input.Depth, sceneNodeType, depthBuffer);
  SetGBufferNormal(normal.xyz, normalBuffer);
  SetGBufferSpecularPower(SpecularPower, depthBuffer, normalBuffer);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SKINNING && !MORPHING
#define SUPPORTS_INSTANCING 1
#endif

technique Default
#if !MGFX && SUPPORTS_INSTANCING     // TODO: Add Annotation support to MonoGame.
< string InstancingTechnique = "DefaultInstancing"; >
#endif
{
  pass
  {
#if ALPHA_TEST
    CullMode = NONE;
#else
    CullMode = CCW;
#endif
    
#if !SM4 && !ALPHA_TEST && !TRANSPARENT
    VertexShader = compile vs_2_0 VSNoInstancing();
    PixelShader = compile ps_2_0 PS();
#elif !SM4
    VertexShader = compile vs_3_0 VSNoInstancing();
    PixelShader = compile ps_3_0 PS();                   // VFACE requires ps 3.0 or ps 4.0.
#elif SM4 && !ALPHA_TEST && !TRANSPARENT
    VertexShader = compile vs_4_0_level_9_1 VSNoInstancing();
    PixelShader = compile ps_4_0_level_9_1 PS();
#else
    VertexShader = compile vs_4_0 VSNoInstancing();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}

#if SUPPORTS_INSTANCING
technique DefaultInstancing
{
  pass
  {
#if ALPHA_TEST
    CullMode = NONE;
#else
    CullMode = CCW;
#endif
    
#if !SM4
    VertexShader = compile vs_3_0 VSInstancing();
    PixelShader = compile ps_3_0 PS();
#else
    VertexShader = compile vs_4_0 VSInstancing();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}
#endif
