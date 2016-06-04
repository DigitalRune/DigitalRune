//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file CubeMapShadowMask.fx
/// Creates the shadow mask for a cube map shadow.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"
#include "../Lighting.fxh"
#include "../Noise.fxh"
#include "../ShadowMap.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 ViewInverse : VIEWINVERSE;
DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);

float2 Parameters0;
#define ViewportSize Parameters0.xy

float4 Parameters1;
//#define Near Parameters1.x
#define Far Parameters1.y
#define DepthBias Parameters1.z
#define NormalOffset Parameters1.w

float3 Parameters2;
#define ShadowMapSize Parameters2.x
#define FilterRadius Parameters2.y
#define JitterResolution Parameters2.z

float3 LightPosition;         // Light position in view space.

float4x4 ShadowView;       // Converts from camera view to light view space.
DECLARE_UNIFORM_JITTERMAP(JitterMap);
DECLARE_UNIFORM_SHADOWMAP(ShadowMap, POINTLIGHTSHADOWMAP);

float3 Samples[MAX_NUMBER_OF_PCF_SAMPLES];


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSFrustumRayOutput VS(VSFrustumRayInput input)
{
  VSFrustumRayOutput output = VSFrustumRay(input, ViewportSize, FrustumCorners);
  
  // Fix for half-resolution shadow mask:
  // One half-res texel covers 4 full-res texels. We need to ensure that the
  // upper, left full-res texel is sampled when processing the half-res shadow
  // mask. Otherwise, the pixel shader might randomly sample one of the 4 full-
  // res texels, which leads to problems when reconstructing normals.
  output.TexCoord -= 0.00001;
  
  return output;
}


float4 PS(float2 texCoord : TEXCOORD0,
          float3 frustumRay : TEXCOORD1,
          uniform const int numberOfSamples) : COLOR
{
  // Get depth.
  float4 gBuffer0Sample = tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0));
  float depth = GetGBufferDepth(gBuffer0Sample);
  
  // Reconstruct the position and normal in view space.
  float3 positionView = frustumRay * depth;
  float3 normalView = DeriveNormal(positionView);
  
  // Abort for skybox pixels.
  clip(0.9999f - depth);
  
  // Normal offset
  float3 lightView = normalize(LightPosition - positionView);
  positionView = ApplyNormalOffset(positionView, normalView, lightView, NormalOffset);
  
  // Convert the position from view space to light space.
  float4 positionLight = mul(float4(positionView, 1), ShadowView);
  
  // Abort if the position is outside the light sphere radius (= camera far).
  float3 lightDirection = positionLight.xyz;
  float lightDistance = length(lightDirection);
  clip(Far - lightDistance);
  
  // Normalize lightDirection vector.
  // (Note: It could be left unnormalized for some GetShadow methods.)
  lightDirection = lightDirection / lightDistance;
  
  // Compute the linear normalized depth of this pixel and apply bias against surface
  // acne. Scale bias with depth because shadow texels also scale with depth.
  float depthOffset = lightDistance * DepthBias;
  float ourDepth = (lightDistance + depthOffset) / Far;
  
  // Compute the shadow factor (0 = no shadow, 1 = shadow).
  float shadow = 0;
  if (numberOfSamples < 0)
  {
    shadow = GetShadowCubePcfJitteredWorld(
      mul(float4(positionView, 1), ViewInverse).xyz, ourDepth, lightDirection, ShadowMapSampler,
      ShadowMapSize, JitterMapSampler, FilterRadius, JitterResolution);
  }
  else if (numberOfSamples <= 0)
  {
    shadow = GetShadowCube(ourDepth, lightDirection, ShadowMapSampler);
  }
  else
  {
    shadow = GetShadowCubePcfJitteredWorld(
      mul(float4(positionView, 1), ViewInverse).xyz, ourDepth, lightDirection, Samples, ShadowMapSampler,
      ShadowMapSize, JitterMapSampler, FilterRadius, JitterResolution, numberOfSamples);
  }
  
  // The shadow mask stores the inverse.
  return 1 - shadow.xxxx;
}

float4 PSUnfiltered(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR { return PS(texCoord, frustumRay, 0); }
float4 PSOptimized(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR { return PS(texCoord, frustumRay, -1); }
float4 PSPcf1(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR { return PS(texCoord, frustumRay, 1); }
float4 PSPcf4(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR { return PS(texCoord, frustumRay, 4); }
float4 PSPcf8(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR { return PS(texCoord, frustumRay, 8); }
float4 PSPcf16(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR { return PS(texCoord, frustumRay, 16); }
float4 PSPcf32(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR { return PS(texCoord, frustumRay, 32); }

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
  pass Optimized  { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSOptimized();  }
  pass Unfiltered { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSUnfiltered(); }
  pass Pcf1       { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSPcf1(); }
  pass Pcf4       { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSPcf4(); }
  pass Pcf8       { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSPcf8(); }
  pass Pcf16      { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSPcf16(); }
  pass Pcf32      { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSPcf32(); }
}
