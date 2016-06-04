//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file GBuffer.fx
/// Renders the decal into the G-buffer.
/// Supports:
/// - Specular power
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Material.fxh"

// CREATE_GBUFFER creates automatically bound shader constants required for
// encoding data in the G-Buffer.
#define CREATE_GBUFFER 1
#include "../Deferred.fxh"

#include "../Decal.fxh"


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

// Possible defines
//#define ALPHA_TEST 1
//#define NORMAL_MAP 1


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 WorldView : WORLDVIEW;
float4x4 WorldViewInverse : WORLDVIEWINVERSE;
float4x4 UnscaledWorld : UNSCALEDWORLD; // TODO: We only need the float3x3 part.
float4x4 Projection : PROJECTION;

// 3D position and normal reconstruction from G-buffer.
float2 ViewportSize : VIEWPORTSIZE;
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);
float CameraFar : CAMERAFAR;

// The specular power is not used in the shader directly. (The DecalRenderer reads
// the effect parameter and sets the value as the BlendFactor.)
float SpecularPower : SPECULARPOWER;
DECLARE_UNIFORM_DIFFUSETEXTURE
#if ALPHA_TEST
float ReferenceAlpha = 0.9f;
#endif
#if NORMAL_MAP
DECLARE_UNIFORM_NORMALTEXTURE
#endif


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION0;
};

struct VSOutput
{
  float4 PositionView : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float Depth : TEXCOORD2;
  float4 Position : SV_Position;
};

struct PSInput
{
  float4 PositionView : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float Depth : TEXCOORD2;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.PositionView = mul(input.Position, WorldView);
  output.Position = mul(output.PositionView, Projection);
  output.PositionProj = output.Position;
  output.Depth = -output.PositionView.z / CameraFar;
  
  return output;
}


float4 PS(PSInput input) : COLOR
{
  float3 normalWorld;  // The surface normal in world space.
  float2 uvDecal;      // UV texture coordinates in decal space.
  float2 uvScreen;     // UV texture coordinates in screen space.
  DeferredDecal(input.PositionView, input.PositionProj, input.Depth,
                WorldViewInverse, ViewportSize,
                GBuffer0Sampler, GBuffer1Sampler,
                normalWorld, uvDecal, uvScreen);
  
  // Sample decal alpha.
  float alpha = tex2D(DiffuseSampler, uvDecal).a * DecalAlpha;
#if ALPHA_TEST
  clip(alpha - ReferenceAlpha);
#endif
  
  float4 gBuffer1;
  gBuffer1.a = alpha;
#if NORMAL_MAP
  // Sample decal normal.
  float3 normal = GetNormalDxt5nm(NormalSampler, uvDecal);
  
  // ----- Variant #1: Decal orientation determines normal direction.
  // + Fast and simple.
  // + Suitable for decals that ignore the surface (e.g. bullet holes).
  // - Wrong normals for decals that should wrap around the surface (e.g. blood, paper).
  // - GBuffer pass overrides normals, Material pass reads different normals.
  
  normal = mul(normal, (float3x3)UnscaledWorld);
  // -----
  
  // ----- Variant #2: Reconstruct surface from depth buffer.
  // + The normals follows the original geometry.
  // + Suitable for decals that should follow the surface, such as blood, paper.
  // +/- Normal maps of the original geometry are ignored.
  // - Artifacts around objects in front of the decal caused by z-discontinuities.
  
  //normal.y = -normal.y;
  //float3x3 cotangentFrame = CotangentFrame(normalView, input.PositionView, uvDecal);
  //normal = mul(normal, cotangentFrame);
  //TODO: When this code was last used, the GBuffer used view space normals. Now, the
  // GBuffer uses world space normals and the DeferredDecal() computes world space normals.
  // -----
  
  // ----- Variant #3: Combination of #1 and #2.
  // Read the normal from G-buffer 1 and reconstruct the normal from the depth buffer.
  // Check which normal is closer to the decal orientation. If the reconstructed normal
  // is closer use variant #2; otherwise use variant #1.
  // + Removes artifacts from z-discontinuities.
  
  // Not yet implemented.
  // -----
  
  // Store decal normal in GBuffer1.
  SetGBufferNormal(normal, gBuffer1);
#else
  // Keep normal in GBuffer1.
  gBuffer1.xyz = tex2Dlod(GBuffer1Sampler, float4(uvScreen, 0, 0)).xyz;
#endif
  
  return gBuffer1;
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

technique Default
{
  pass
  {
#if ALPHA_TEST
    AlphaBlendEnable = false;
#endif
    
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
