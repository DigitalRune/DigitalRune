//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ImageBasedLight.fx
/// Uses the light defined by an environment map to add light to the deferred
/// light buffers.
//
// Cube maps can use different color encoding. This effect only supports RGBM.
// Normal sRGB textures can be used because they are like RGBM texture with
// max value 1.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// In pass Clip: The normal WVP matrix of the clip shape.
// In pass Light: A matrix which transforms from world space with camera at
// origin to local box space.
float4x4 Transform;

float2 ViewportSize : VIEWPORTSIZE;
DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);

float4 Parameters0;
#define DiffuseColor Parameters0.xyz      // Use (0, 0, 0) to disable the diffuse effect.
#define DiffuseMipLevel Parameters0.w     // Mip map level to use in texCUBElod for diffuse lighting.

float4 Parameters1;
#define SpecularColor Parameters1.xyz     // Use (0, 0, 0) to disable the specular effect.
#define RgbmMax Parameters1.w             // RGBM max value in gamma space.

float4 Parameters2;
#define BoundingBoxHalfWidth Parameters2.xyz
#define FadeOutRange Parameters2.w

float4 Parameters3;
#define ProjectionAabbMin Parameters3.xyz // AABB min for localization. Localization is disabled if min > max.
#define TextureSize Parameters3.w         // Size of one cube map face in texels.

float4 Parameters4;
#define ProjectionAabbMax Parameters4.xyz
#define BlendMode Parameters4.w

float PrecomputedTerm = log2(256 * sqrt(3)); // Precomputed term log2(TextureSize * sqrt(3))

texture EnvironmentMap : ENVIRONMENTMAP;
samplerCUBE EnvironmentMapSampler = sampler_state
{
  Texture = <EnvironmentMap>;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

float4 VSClip(float4 position : POSITION) : SV_Position
{
  return mul(position, Transform);
}

float4 PSClip() : COLOR0
{
  return 0;
}


VSFrustumRayOutput VSLight(VSFrustumRayInput input)
{
  return VSFrustumRay(input, ViewportSize, FrustumCorners);
}


// Samples the environment map, including bilinear filtering and color decoding.
// direction does not need to be normalized.
float3 SampleEnvironmentMap(float3 direction, float mipLevel, float mipLevelSize)
{
  // Cube maps are left handed --> Sample with inverted z.
  direction.z *= -1;
  
  // In XNA we use manual bilinear filtering to filter across cube map faces.
  // DX11 hardware can do this automatically.
#if SM4
  float4 color = texCUBElod(EnvironmentMapSampler, float4(direction, mipLevel));
#else
  
  // Get tex coords from range [-1, 1] to range [-TextureSize, TextureSize].
  direction = normalize(direction);
  direction = direction * mipLevelSize;
  
  // ShaderX2 and Engel add a half pixel offset, but creates bad artifacts :-(
  //texCoord += float3(0.5, 0.5, 0.5);
  
  // Clamp to an integer coordinate.
  float3 tc0 = floor(direction);
  // Get a second integer coordinate.
  float3 tc1 = tc0 + float3(1.0, 1.0, 1.0);
  
  // Sample at the 8 possible positions made up of tc0 and tc1.
  float4 c000 = texCUBElod(EnvironmentMapSampler, float4(tc0.x, tc0.y, tc0.z, mipLevel));
  float4 c001 = texCUBElod(EnvironmentMapSampler, float4(tc0.x, tc0.y, tc1.z, mipLevel));
  float4 c010 = texCUBElod(EnvironmentMapSampler, float4(tc0.x, tc1.y, tc0.z, mipLevel));
  float4 c011 = texCUBElod(EnvironmentMapSampler, float4(tc0.x, tc1.y, tc1.z, mipLevel));
  float4 c100 = texCUBElod(EnvironmentMapSampler, float4(tc1.x, tc0.y, tc0.z, mipLevel));
  float4 c101 = texCUBElod(EnvironmentMapSampler, float4(tc1.x, tc0.y, tc1.z, mipLevel));
  float4 c110 = texCUBElod(EnvironmentMapSampler, float4(tc1.x, tc1.y, tc0.z, mipLevel));
  float4 c111 = texCUBElod(EnvironmentMapSampler, float4(tc1.x, tc1.y, tc1.z, mipLevel));
  
  // Lerp all results together similar to 2D bilinear filtering.
  float3 p = frac(direction);
  float4 color = lerp(lerp(lerp(c000, c010, p.y),
                           lerp(c100, c110, p.y),
                           p.x),
                      lerp(lerp(c001, c011, p.y),
                           lerp(c101, c111, p.y),
                           p.x),
                      p.z);
#endif
  
  return FromGamma(DecodeRgbm(color, RgbmMax));
}


// Maps a unit cube position to a unit sphere position.
// See http://mathproofs.blogspot.co.at/2005/07/mapping-cube-to-sphere.html.
float3 MapCubeToSphere(float3 position)
{
  float3 p = position;
  float3 p2 = p * p;
  return p * sqrt(1 - 0.5 * p2.yzx - 0.5 * p2.zxy + 0.333 * p2.yzx * p2.zxy);
}


// Returns the cube map lookup direction for texCUBE().
// The given position must be the shaded pixel position in local space of the cube map.
// The direction must be the uncorreted world space lookup direction.
// Optional: The lookup direction is localized using an AABB.
float3 LocalizeDirection(float3 positionLocal, float3 directionWorld)
{
  // Note: This method creates artifacts when the lookup position
  // is nearly on an AABB side and when the lookup direction is
  // nearly parallel to an AABB side.
  // We ignore these problems. The level designer should hide problematic cases.
  
  // Compute local direction.
  float3 directionLocal = mul(directionWorld, (float3x3)Transform);
  
  // If AABB is invalid (min > max), localization is disabled.
  if (ProjectionAabbMin.x > ProjectionAabbMax.x)
    return directionLocal;
  
  // Compute ray parameter where ray from position hits the box sides.
  float3 pMax = (ProjectionAabbMax - positionLocal) / directionLocal;
  float3 pMin = (ProjectionAabbMin - positionLocal) / directionLocal;
  
  // Get result of positive ray direction, not negative parameters.
  float3 pPos = max(pMin, pMax);
  //float3 pPos = (direction > 0) ? pMax : pMin;
  
  // To ignore any negative parameters which occur outside the box:
  pPos = abs(pPos);
  
  // Use parameter of closest hit.
  float p = min(min(pPos.x, pPos.y), pPos.z);
  
  // Compute hit position;
  float3 hitPosition = positionLocal + p * directionLocal;
  
  // Return new lookup direction (origin to hit).
  return normalize(hitPosition);
}


void PSLight(float2 texCoord : TEXCOORD0,
             float3 frustumRay : TEXCOORD1,
             out float4 lightBuffer0 : COLOR0,
             out float4 lightBuffer1 : COLOR1,
             uniform const bool hasDiffuse,
             uniform const bool hasSpecular)
{
  lightBuffer0 = 0;
  lightBuffer1 = 0;
  
  // Get depth from G-buffer.
  float4 gBuffer0Sample = tex2D(GBuffer0Sampler, texCoord);
  float depth = GetGBufferDepth(gBuffer0Sample);
  
  // Abort for skybox pixels.
  clip(0.9999f - depth);
  
  // Reconstruct view direction (camera to pixel) in world space.
  float3 cameraToPixel = frustumRay * depth;
  
  // Compute attenuation.
  float3 normalizedPosition = mul(float4(cameraToPixel, 1), Transform).xyz;
  float radius = length(MapCubeToSphere(normalizedPosition / BoundingBoxHalfWidth));
  
  // Compute attenuation factor (0 = no light, 1 = full light).
  // Linear fade out.
  //float attenuation = saturate((1 - radius) / (FadeOutRange + 0.000001f));
  // Smooth fade out.
  float attenuation = saturate(smoothstep(0, FadeOutRange, (1 - radius)));
  
  // Visulize attenuation using rings.
  //if (frac(10 * attenuation) > 0.95)
  //{
  //  lightBuffer0.a = 1;
  //  lightBuffer1.a = 1;
  //  return;
  //}
  
  clip(attenuation - 0.00001f);
  
  // Get normal and specular power from G-buffer.
  float4 gBuffer1Sample = tex2D(GBuffer1Sampler, texCoord);
  float3 normal = GetGBufferNormal(gBuffer1Sample);
  float specularPower = GetGBufferSpecularPower(gBuffer0Sample, gBuffer1Sample);
  
  // If DiffuseColor or SpecularColor is (0, 0, 0), the light is disabled.
  if (hasDiffuse)
  {
    // Diffuse light: Use normal direction to sample the lowest detail mipmap level.
    float3 diffuseDirection = LocalizeDirection(normalizedPosition, normal);
    lightBuffer0.rgb = DiffuseColor * SampleEnvironmentMap(
      diffuseDirection,
      DiffuseMipLevel,
      TextureSize / pow(2, DiffuseMipLevel)); // TODO: Term TextureSize / 2^miplevel can be precomputed.
    
    // We use premultiplied alpha.
    lightBuffer0.rgb *= attenuation;
    
    // Note: The blend mode only applies to the diffuse component. The specular
    // reflections always need to replace the existing reflection.
    lightBuffer0.a = attenuation * BlendMode;
  }
  else
  {
    lightBuffer0 = float4(0, 0, 0, 0);
  }
  
  if (hasSpecular)
  {
    // Boost specular power for debugging.
    //specularPower *= 100000;
    // Specular light: Use reflection vector to sample cube map.
    float3 viewDirectionReflected = reflect(cameraToPixel, normal);
    float3 specularDirection = LocalizeDirection(normalizedPosition, viewDirectionReflected);
    
    // The specular power (surface roughness) is used to select the mipmap level.
    //float mipLevel = max(0, log2(TextureSize * sqrt(3)) - 0.5 * log2(specularPower + 1));
    // The term log2(w * sqrt(3)) can be precomputed.
    float mipLevel = max(0, PrecomputedTerm - 0.5 * log2(specularPower + 1));
    
    lightBuffer1.rgb = SpecularColor * SampleEnvironmentMap(
      specularDirection,
      mipLevel,
      TextureSize / pow(2, mipLevel));
    
    lightBuffer1.rgb *= attenuation;
    lightBuffer1.a = attenuation;
  }
  else
  {
    lightBuffer1 = float4(0, 0, 0, 0);
  }
}


void PSDiffuseAndSpecularLight(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PSLight(texCoord, frustumRay, lightBuffer0, lightBuffer1, true, true);
}

void PSDiffuseLight(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PSLight(texCoord, frustumRay, lightBuffer0, lightBuffer1, true, false);
}

void PSSpecularLight(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1, out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PSLight(texCoord, frustumRay, lightBuffer0, lightBuffer1, false, true);
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
  // Render clip volume to set stencil buffer.
  pass Clip
  {
    VertexShader = compile VSTARGET VSClip();
    PixelShader = compile PSTARGET PSClip();
  }
  
  // Blend light to light buffers.
  pass DiffuseAndSpecularLight
  {
    VertexShader = compile VSTARGET VSLight();
    PixelShader = compile PSTARGET PSDiffuseAndSpecularLight();
  }

  pass DiffuseLight
  {
    VertexShader = compile VSTARGET VSLight();
    PixelShader = compile PSTARGET PSDiffuseLight();
  }

  pass SpecularLight
  {
    VertexShader = compile VSTARGET VSLight();
    PixelShader = compile PSTARGET PSSpecularLight();
  }
}
