//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file CascadedShadowMask.fx
/// Creates the shadow mask for a cascaded shadow.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"
#include "../Lighting.fxh"
#include "../Noise.fxh"

#define CLAMP_TEXCOORDS_TO_SHADOW_MAP_BOUNDS 1
#include "../ShadowMap.fxh"

#if SM4
#define PIXELPOS SV_Position
#else
#define PIXELPOS VPOS
#endif

// Warning: gradient-based operations must be moved out of flow control...
// Is caused by DeriveNormal, which is called before flow control!?
#pragma warning(disable : 4121)


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 ViewInverse : VIEWINVERSE;
DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);

float2 Parameters0;
#define ViewportSize Parameters0.xy

float4 Parameters1;
// The relative interval where the shadow fades into the ShadowFog.
#define FadeOutRange Parameters1.x
#define MaxDistance Parameters1.y
#define VisualizeCascades Parameters1.z
#define ShadowFog Parameters1.w

float4 Parameters2;
#define ShadowMapSize Parameters2.xy
#define FilterRadius Parameters2.z
#define JitterResolution Parameters2.w

float4 Distances;
float4x4 ShadowMatrices[4];
float4 DepthBias;
float4 NormalOffset;
float3 LightDirection;  // Direction to the light in view space.
int NumberOfCascades;

DECLARE_UNIFORM_JITTERMAP(JitterMap);
DECLARE_UNIFORM_SHADOWMAP(ShadowMap, DIRECTIONALLIGHTSHADOWMAP);

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
          float4 vPos : PIXELPOS,
          uniform const int cascadeSelection,
          uniform const int numberOfSamples,
          uniform const float visualizeCascades) : COLOR
{
  // Get depth.
  float4 gBuffer0Sample = tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0));
  float depth = GetGBufferDepth(gBuffer0Sample);
  
  // Reconstruct the position in view space.
  float3 positionView = frustumRay * depth;
  
  // Normal for normal offset has to be computed before branching.
  float3 normalView = DeriveNormal(positionView);
  
  // Abort for skybox pixels.
  clip(0.9999f - depth);
  
  // Compute the cascade index and the texture coords.
  int cascade;
  float3 shadowTexCoord;
  if (cascadeSelection == CASCADE_SELECTION_FAST)
  {
    ComputeCsmCascadeFast(float4(positionView, 1), -positionView.z, Distances, ShadowMatrices, cascade, shadowTexCoord);
  }
  else if (cascadeSelection == CASCADE_SELECTION_BEST)
  {
    ComputeCsmCascadeBest(float4(positionView, 1), ShadowMatrices, cascade, shadowTexCoord);
  }
  else if (cascadeSelection == CASCADE_SELECTION_BEST_DITHERED)
  {
    ComputeCsmCascadeBestDithered(NumberOfCascades, float4(positionView, 1), ShadowMatrices, vPos.xy, cascade, shadowTexCoord);
  }
  //else
  //{
  //  ComputeCsmCascade(cascadeSelection, NumberOfCascades, float4(positionView, 1), -positionView.z,
  //                    Distances, ShadowMatrices, vPos, cascade, shadowTexCoord);
  //}
  
#if !SM4
  // Abort if the texture coordinates are outside [0, 1].
  if (!IsInRange(shadowTexCoord, 0, 1))
  {
    return 1 - ShadowFog.xxxx;
  }
  
  // Apply normal offset.
  float3 offsetPositionView = ApplyNormalOffset(positionView, normalView, LightDirection, NormalOffset[cascade]);
  shadowTexCoord.xy = GetShadowTexCoord(float4(offsetPositionView, 1), ShadowMatrices[cascade]).xy;
#else
    // Workaround for NVIDIA DirectX 11 driver bug:
    // --> Apply normal offset in both branches. Otherwise, the NVIDIA driver removes DeriveNormal()
    // above and produces invalid values in the other branch.
  if (!IsInRange(shadowTexCoord, 0, 1))
  {
    float3 offsetPositionView = ApplyNormalOffset(positionView, normalView, LightDirection, NormalOffset[cascade]);
    shadowTexCoord.xy = GetShadowTexCoord(float4(offsetPositionView, 1), ShadowMatrices[cascade]).xy;
    return 1 - ShadowFog.xxxx;
  }
  else
  {
    float3 offsetPositionView = ApplyNormalOffset(positionView, normalView, LightDirection, NormalOffset[cascade]);
    shadowTexCoord.xy = GetShadowTexCoord(float4(offsetPositionView, 1), ShadowMatrices[cascade]).xy;
  }
#endif
  
  if (visualizeCascades > 0 && vPos.x < ViewportSize.x / 2)
  {
    //if (cascade % 2 == 0 && vPos.x % 2 == 0 && vPos.y % 2 == 0)
    //  return 0;
    //if (cascade % 2 == 1 && vPos.y % 2 == 0)
    //  return 0;
    return (uint)cascade % 2;
  }
  
  // Transform the texture coords to valid texture atlas coords.
  float2 atlasTexCoord = ConvertToTextureAtlas(shadowTexCoord.xy, cascade, NumberOfCascades);
  
  // Shadow map bounds (left, top, right, bottom) inside texture atlas.
  float4 shadowMapBounds = GetShadowMapBounds(cascade, NumberOfCascades, ShadowMapSize);
  
  // Since this shadow uses and orthographic projection, the pixel depth
  // is the z value of the shadow projection space.
  // Apply bias against surface acne.
  float ourDepth = shadowTexCoord.z + DepthBias[cascade];
  
  // Compute the shadow factor (0 = no shadow, 1 = shadow).
  float shadow;
  if (numberOfSamples < 0)
  {
    shadow = GetShadowPcfJitteredWorld(
      mul(float4(positionView, 1), ViewInverse).xyz, ourDepth, atlasTexCoord, ShadowMapSampler,
      ShadowMapSize, JitterMapSampler, FilterRadius, JitterResolution, shadowMapBounds);
  }
  else if (numberOfSamples <= 0)
  {
    shadow = GetShadow(ourDepth, atlasTexCoord, ShadowMapSampler, ShadowMapSize, shadowMapBounds);
  }
  else
  {
    shadow = GetShadowPcfJitteredWorld(
      mul(float4(positionView, 1), ViewInverse).xyz, ourDepth, atlasTexCoord, Samples, ShadowMapSampler,
      ShadowMapSize, JitterMapSampler, FilterRadius, JitterResolution, shadowMapBounds, numberOfSamples);
  }
  
  // Fade out the shadow in the distance.
  shadow = ApplyShadowFog(cascadeSelection, cascade >= NumberOfCascades - 1, shadow,
                          shadowTexCoord, -positionView.z, MaxDistance, FadeOutRange, ShadowFog);
  
  // The shadow mask stores the inverse.
  return 1 - shadow.xxxx;
}

float4 PSFastUnfiltered        (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_FAST,          0,  0); }
float4 PSFastOptimized         (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_FAST,          -1, 0); }
float4 PSFastPcf1              (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_FAST,          1,  0); }
float4 PSFastPcf4              (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_FAST,          4,  0); }
float4 PSFastPcf8              (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_FAST,          8,  0); }
float4 PSFastPcf16             (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_FAST,          16, 0); }
float4 PSFastPcf32             (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, float4 vPos : PIXELPOS) : COLOR { return PS(texCoord, frustumRay, vPos, CASCADE_SELECTION_FAST,          32, VisualizeCascades); }
float4 PSBestUnfiltered        (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_BEST,          0,  0); }
float4 PSBestOptimized         (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_BEST,          -1, 0); }
float4 PSBestPcf1              (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_BEST,          1,  0); }
float4 PSBestPcf4              (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_BEST,          4,  0); }
float4 PSBestPcf8              (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_BEST,          8,  0); }
float4 PSBestPcf16             (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR                         { return PS(texCoord, frustumRay, 0,    CASCADE_SELECTION_BEST,          16, 0); }
float4 PSBestPcf32             (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, float4 vPos : PIXELPOS) : COLOR { return PS(texCoord, frustumRay, vPos, CASCADE_SELECTION_BEST,          32, VisualizeCascades); }
float4 PSBestDitheredUnfiltered(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, float4 vPos : PIXELPOS) : COLOR { return PS(texCoord, frustumRay, vPos, CASCADE_SELECTION_BEST_DITHERED, 0,  0); }
float4 PSBestDitheredOptimized (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, float4 vPos : PIXELPOS) : COLOR { return PS(texCoord, frustumRay, vPos, CASCADE_SELECTION_BEST_DITHERED, -1, 0); }
float4 PSBestDitheredPcf1      (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, float4 vPos : PIXELPOS) : COLOR { return PS(texCoord, frustumRay, vPos, CASCADE_SELECTION_BEST_DITHERED, 1,  0); }
float4 PSBestDitheredPcf4      (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, float4 vPos : PIXELPOS) : COLOR { return PS(texCoord, frustumRay, vPos, CASCADE_SELECTION_BEST_DITHERED, 4,  0); }
float4 PSBestDitheredPcf8      (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, float4 vPos : PIXELPOS) : COLOR { return PS(texCoord, frustumRay, vPos, CASCADE_SELECTION_BEST_DITHERED, 8,  0); }
float4 PSBestDitheredPcf16     (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, float4 vPos : PIXELPOS) : COLOR { return PS(texCoord, frustumRay, vPos, CASCADE_SELECTION_BEST_DITHERED, 16, 0); }
float4 PSBestDitheredPcf22     (float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, float4 vPos : PIXELPOS) : COLOR { return PS(texCoord, frustumRay, vPos, CASCADE_SELECTION_BEST_DITHERED, 22, VisualizeCascades); }


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
  pass FastOptimized         { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSFastOptimized(); }
  pass BestOptimized         { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestOptimized(); }
  pass BestDitheredOptimized { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestDitheredOptimized(); }
  
  pass FastUnfiltered         { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSFastUnfiltered(); }
  pass BestUnfiltered         { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestUnfiltered(); }
  pass BestDitheredUnfiltered { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestDitheredUnfiltered(); }
  
  pass FastPcf1          { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSFastPcf1(); }
  pass FastPcf4          { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSFastPcf4(); }
  pass FastPcf8          { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSFastPcf8(); }
  pass FastPcf16         { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSFastPcf16(); }
  pass FastPcf32         { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSFastPcf32(); }
  pass BestPcf1          { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestPcf1(); }
  pass BestPcf4          { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestPcf4(); }
  pass BestPcf8          { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestPcf8(); }
  pass BestPcf16         { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestPcf16(); }
  pass BestPcf32         { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestPcf32(); }
  pass BestDitheredPcf1  { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestDitheredPcf1(); }
  pass BestDitheredPcf4  { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestDitheredPcf4(); }
  pass BestDitheredPcf8  { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestDitheredPcf8(); }
  pass BestDitheredPcf16 { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestDitheredPcf16(); }
  pass BestDitheredPcf22 { VertexShader = compile VSTARGET VS(); PixelShader = compile PSTARGET PSBestDitheredPcf22(); }
}
