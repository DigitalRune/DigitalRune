//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Fog.fx
/// Renders fog using full-screen pass. The pixel shader writes fog colors
/// which can be alpha blended with the back buffer (non-premultiplied).
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"
#include "../Fog.fxh"

#pragma warning( disable : 3571 )      // pow(f, e) - pow will not work for negative f.


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 ViewportSize : VIEWPORTSIZE;
DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);  // The usual frustum ray but in world space!
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);

// Color of fog (RGBA).
float4 Color0;
float4 Color1;
float4 Heights; // CameraHeight, Fog node height, Fog Height0, Fog Height1
#define CameraHeight Heights.x
#define ReferenceHeight Heights.y
#define Height0 Heights.z
#define Height1 Heights.w

// Combined fog parameters.
float4 FogParameters : FOGPARAMETERS;  // (Start, End, Density, HeightFalloff)
#define FogStart FogParameters.x
#define FogEnd FogParameters.y
#define FogDensity FogParameters.z
#define FogHeightFalloff FogParameters.w

float3 LightDirection: DIRECTIONALLIGHTDIRECTION;

float3 ScatteringSymmetry; // The scattering symmetry constant g for the phase function.


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSFrustumRayOutput VS(VSFrustumRayInput input)
{
  return VSFrustumRay(input, ViewportSize, FrustumCorners);
}


float4 PS(float2 texCoord : TEXCOORD0,
          float3 frustumRay : TEXCOORD1,
          uniform const bool hasHeightFalloff,
          uniform const bool usePhaseFunction) : COLOR0
{
  float depth = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0)));
  
  float3 cameraToPixel = frustumRay * depth;
  float dist = length(cameraToPixel);  // The distance travelled inside the fog.
  float3 cameraToPixelDirection = cameraToPixel / dist;
  
  // Smoothstep distance fog
  float smoothRamp = ComputeSmoothFogIntensity(dist, FogStart, FogEnd);
  
  // Exponential Fog
  float heightFallOff = FogHeightFalloff;
  if (!hasHeightFalloff)
    heightFallOff = 0;    // This will let the shader compiler optimize the next function.
  
  float referenceHeight = CameraHeight - ReferenceHeight + cameraToPixelDirection.y * FogStart;
  float distanceInFog = dist - FogStart;
  float3 fogDirection = cameraToPixelDirection;
  if (heightFallOff * fogDirection.y < 0)
  {
    // The camera is is looking into the denser parts of the fog. This is 
    // numerically bad because if the fog at the camera position is low,
    // then exp2(-heightFallOff * height) can get numerically 0 (less than 
    // 32-bit floating point can present). It is better to compute the optical 
    // length always from the denser to the less dense fog heights.
    // Move reference position to end of ray.
    referenceHeight += fogDirection.y * distanceInFog;
    // Reverse ray direction.
    fogDirection = -fogDirection;
  }
  
  float referenceDensity = FogDensity * exp2(-heightFallOff * referenceHeight);
  float opticalLength = GetOpticalLengthInHeightFog(distanceInFog, referenceDensity, fogDirection * distanceInFog, heightFallOff);
  float exponentialFog = ComputeExponentialFogIntensity(opticalLength, 1);  // fogDensity is already in opticalLength!
  
  float height = CameraHeight + cameraToPixel.y;
  float4 color = lerp(Color0, Color1, smoothstep(Height0, Height1, height));
  
  if (usePhaseFunction)
  {
    // Apply phase function.
    float nDotV = dot(cameraToPixelDirection, -LightDirection);
    color.rgb *= FogPhaseFunction(nDotV, ScatteringSymmetry);
  }
  
  return color * smoothRamp * exponentialFog;
}

float4 PSFog(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0
{
  return PS(texCoord, frustumRay, false, false);
}
float4 PSFogWithPhase(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0
{
  return PS(texCoord, frustumRay, false, true);
}
float4 PSFogWithHeightFalloff(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0
{
  return PS(texCoord, frustumRay, true, false);
}
float4 PSFogWithHeightFalloffWithPhase(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0
{
  return PS(texCoord, frustumRay, true, true);
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
  pass Fog
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFog();
  }
  
  pass FogWithHeightFalloff
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFogWithHeightFalloff();
  }
  
  pass FogWithPhase
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFogWithPhase();
  }
  
  pass FogWithHeightFalloffWithPhase
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFogWithHeightFalloffWithPhase();
  }
}
