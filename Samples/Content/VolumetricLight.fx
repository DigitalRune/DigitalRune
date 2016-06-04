//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file VolumetricLight.fx
/// Creates a volumetric light effect using raymarching.
//
// The effect supports point lights, spotlights and projector lights.
//
//-----------------------------------------------------------------------------

#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Encoding.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Deferred.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Lighting.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Noise.fxh"


//-----------------------------------------------------------------------------
// Defines, Constants
//-----------------------------------------------------------------------------

// Type of light.
static const int LightTypePoint = 0;      // Point light
static const int LightTypeSpot = 1;       // Spotlight
static const int LightTypeProjector = 2;  // Projector

// Type of light texture.
static const int TextureTypeNone = 0;     // No texture
static const int TextureTypeRgb = 1;      // RGB texture
static const int TextureTypeAlpha = 2;    // Alpha-only texture


//-----------------------------------------------------------------------------
// Parameters
//-----------------------------------------------------------------------------

float2 ViewportSize;
DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);  // In world space.
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);

// Volumetric light parameters
float3 Color;
int NumberOfSamples;
float2 DepthInterval;

// Light parameters
float3 LightDiffuse;
float3 LightPosition;        // Position in world space but relative to camera!
float LightRange;
float LightAttenuation;
texture LightTexture;
sampler LightTextureSampler = sampler_state 
{ 
  Texture = <LightTexture>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
};
float4x4 LightTextureMatrix;   // Converts from view to light texture space.
float LightTextureMipMap;
int TextureType;

// Only for spotlights.
float3 LightDirection;
float2 LightAngles;     // Falloff angle, cutoff angle

float2 RandomSeed;      // To animate the jittering.


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

// Returns the intensity of the light at a certain position.
// cameraToPosition is the vector from the camera to the lit position.
float3 GetPointLightIntensity(float3 cameraToPosition, int textureType)
{
  float3 lightDirection = cameraToPosition - LightPosition;
  float lightDistance = length(lightDirection);
  lightDirection = lightDirection / lightDistance;
  
  float distanceAttenuation = ComputeDistanceAttenuation(lightDistance, LightRange, LightAttenuation);

  float3 textureColor = float3(1, 1, 1);
  if (textureType != TextureTypeNone)
  { 
    if (textureType == TextureTypeRgb)
      textureColor = texCUBElod(LightTextureSampler, float4(mul(lightDirection, (float3x3)LightTextureMatrix), LightTextureMipMap)).rgb;
    else
      textureColor = texCUBElod(LightTextureSampler, float4(mul(lightDirection, (float3x3)LightTextureMatrix), LightTextureMipMap)).aaa;
    
    textureColor = FromGamma(textureColor);
  }
    
  return textureColor * LightDiffuse * distanceAttenuation;
}


float3 GetSpotlightIntensity(float3 cameraToPosition, int textureType)
{
  float3 lightDirection = cameraToPosition - LightPosition;
  float lightDistance = length(lightDirection);
  lightDirection = lightDirection / lightDistance;
  
  float distanceAttenuation = ComputeDistanceAttenuation(lightDistance, LightRange, LightAttenuation);
  float coneAttenuation = ComputeConeAttenuation(lightDirection, LightDirection, LightAngles.x, LightAngles.y);
     
  float3 textureColor = float3(1, 1, 1);
  if (textureType != TextureTypeNone)
  { 
    float4 lightTexCoord = mul(float4(cameraToPosition, 1), LightTextureMatrix);
    lightTexCoord.xy /= lightTexCoord.w;
    
    if (textureType == TextureTypeRgb)
      textureColor = FromGamma(tex2Dlod(LightTextureSampler, float4(lightTexCoord.xy, 0, LightTextureMipMap)).rgb);
    else
      textureColor = FromGamma(tex2Dlod(LightTextureSampler, float4(lightTexCoord.xy, 0, LightTextureMipMap)).aaa);
  }
    
  return textureColor * LightDiffuse * distanceAttenuation * coneAttenuation;
}


float3 GetProjectorLightIntensity(float3 cameraToPosition, int textureType)
{
  float3 lightDirection = cameraToPosition - LightPosition;
  float lightDistance = length(lightDirection);
  lightDirection = lightDirection / lightDistance;
  
  float distanceAttenuation = ComputeDistanceAttenuation(lightDistance, LightRange, LightAttenuation);
  
  float4 lightTexCoord = mul(float4(cameraToPosition, 1), LightTextureMatrix);
  
  // Back projection?
  if (lightTexCoord.w < 0)
    return 0;
  
  lightTexCoord.xy /= lightTexCoord.w;
  
  // Check if pixel is outside the texture.
  if (any(float4(lightTexCoord.x, lightTexCoord.y, 1 - lightTexCoord.x, 1 - lightTexCoord.y) < 0))
    return 0;
  
  float3 textureColor;
  if (textureType == TextureTypeRgb)
    textureColor = FromGamma(tex2Dlod(LightTextureSampler, float4(lightTexCoord.xy, 0, LightTextureMipMap)).rgb);
  else
    textureColor = FromGamma(tex2Dlod(LightTextureSampler, float4(lightTexCoord.xy, 0, LightTextureMipMap)).aaa);

  return LightDiffuse * textureColor * distanceAttenuation;
}


VSFrustumRayOutput VS(VSFrustumRayInput input)
{
  return VSFrustumRay(input, ViewportSize, FrustumCorners);
}


float4 PS(float2 texCoord : TEXCOORD0, 
          float3 frustumRay : TEXCOORD1,
          uniform const int lightType,
          uniform const int textureType) : COLOR
{
  // Get depth.
  float4 gBuffer0Sample = tex2D(GBuffer0Sampler, texCoord);
  float depth = GetGBufferDepth(gBuffer0Sample);
  
  // We sample several positions along the view ray. start and end define
  // the depth values of the light AABB.
  float start = DepthInterval.x;
  float end = min(DepthInterval.y, depth);

  // Abort if the start position is already hidden by geometry.
  clip(depth - start);
  
  float stepSize = (end - start) / NumberOfSamples;
  
  // Add random offset to start to add jitter and hide banding.
  float random = Noise2(RandomSeed + texCoord);   // Random value in [0, 1]
  start = start + random * stepSize;
  
  // Average light intensities along the view ray.
  // Note: Color contains an intensity factor divided by NumberOfSamples.
  float3 color = 0;
  for (int i = 0; i < NumberOfSamples; i++)
  {
    if (lightType == LightTypePoint)
      color.rgb += Color * GetPointLightIntensity(frustumRay * (start + i * stepSize), textureType);
    else if (lightType == LightTypeSpot)
      color.rgb += Color * GetSpotlightIntensity(frustumRay * (start + i * stepSize), textureType) ;
    else 
      color.rgb += Color * GetProjectorLightIntensity(frustumRay * (start + i * stepSize), textureType);
  }
  
  return float4(color, 0);  // Alpha = 0 results in additive blending.
}

float4 PSPointLight(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR
{
  return PS(texCoord, frustumRay, LightTypePoint, TextureTypeNone);
}

float4 PSPointLightTextureRgb(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR
{
  return PS(texCoord, frustumRay, LightTypePoint, TextureTypeRgb);
}

float4 PSPointLightTextureAlpha(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR
{
  return PS(texCoord, frustumRay, LightTypePoint, TextureTypeAlpha);
}

float4 PSSpotlight(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR
{
  return PS(texCoord, frustumRay, LightTypeSpot, TextureTypeNone);
}

float4 PSSpotlightTextureRgb(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR
{
  return PS(texCoord, frustumRay, LightTypeSpot, TextureTypeRgb);
}

float4 PSSpotlightTextureAlpha(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR
{
  return PS(texCoord, frustumRay, LightTypeSpot, TextureTypeAlpha);
}

float4 PSProjectorLightTextureRgb(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR
{
  return PS(texCoord, frustumRay, LightTypeProjector, TextureTypeRgb);
}

float4 PSProjectorLightTextureAlpha(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR
{
  return PS(texCoord, frustumRay, LightTypeProjector, TextureTypeAlpha);
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
  pass PointLight
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSPointLight();
  }
  
  pass PointLightTextureRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSPointLightTextureRgb();
  }
  
  pass PointLightTextureAlpha
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSPointLightTextureAlpha();
  }
  
  pass Spotlight
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSSpotlight();
  }
  
  pass SpotlightTextureRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSSpotlightTextureRgb();
  }
  
  pass SpotlightTextureAlpha
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSSpotlightTextureAlpha();
  }
  
  pass ProjectorLightTextureRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSProjectorLightTextureRgb();
  }
  
  pass ProjectorLightTextureAlpha
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSProjectorLightTextureAlpha();
  }
}
