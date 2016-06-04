//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file AmbientLight.fx
/// Renders an ambient light with hemispheric attenuation into the light buffer
/// for deferred lighting.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"
#include "../Lighting.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 WorldViewProjection : WORLDVIEWPROJECTION;  // (Only for clip geometry.)
float2 ViewportSize : VIEWPORTSIZE;
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);
float3 AmbientLight : AMBIENTLIGHT;
float AmbientLightAttenuation : AMBIENTLIGHTATTENUATION = 0;
float3 AmbientLightUp;   // Light up vector in world space.


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
};

struct VSOutput
{
  float2 TexCoord : TEXCOORD;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

float4 VSClip(float4 position : POSITION) : SV_Position
{
  return mul(position, WorldViewProjection);
}


float4 PSClip() : COLOR0
{
  return 0;
}


VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


float4 PS(float2 texCoord : TEXCOORD) : COLOR0
{
  // Get depth.
  float4 gBuffer0Sample = tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0));
  float depth = GetGBufferDepth(gBuffer0Sample);
  
  // Abort for skybox pixels.
  clip(0.9999f - depth);
  
  // Get normal.
  float4 gBuffer1Sample = tex2Dlod(GBuffer1Sampler, float4(texCoord, 0, 0));
  float3 normal = GetGBufferNormal(gBuffer1Sample);
  
  float3 result = ComputeAmbientLight(AmbientLight, AmbientLightAttenuation, AmbientLightUp, normal);
  return float4(result, 1);
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
  pass Clip
  {
    VertexShader = compile VSTARGET VSClip();
    PixelShader = compile PSTARGET PSClip();
  }
  pass Light
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
