//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ScatteringSky.fx
/// Computes sky colors using atmospheric scattering.
//
// TODO: Possible optimizations:
// - Replace exp(x) with exp2(x / log(2)).
//
// Notes:
// - We do not use this for aerial perspective (terrain fogging because the
//   numeric integration is very expensive and we would have to do this every
//   frame). Bruneton's scattering is faster for aerial perspective (but would
//   requires DX11).
//   To fog terrain in a post-processor you simple attenuate the shaded terrain
//   pixel by the transmittance and add the inscatter (from camera to ray).
//   For a better terrain lighting, you would have to create a skylight light
//   type (not a post-processor), which attenuates the light by the transittance
//   (from sun to terrain) before shading the terrain.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Atmosphere.fxh"
#include "../Tonemapping.fxh"      // Only used for debugging.

#pragma warning(disable : 3571)    // pow(f, e) - pow will not work for negative f.


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 View : VIEW;
float4x4 Projection : PROJECTION;

float3 SunDirection;  // The direction to the sun.
float3 SunIntensity;

float4 Radii = float4(1.025, 1, 1, (1.025 - 1) * 0.25);
#define RadiusAtmosphere Radii.x      // Radius of the top of the atmosphere (from earth center)
#define RadiusGround Radii.y          // Earth radius (ground level)
#define RadiusCamera Radii.z          // Distance of camera from earth center
#define ScaleHeight Radii.w           // Scale height as altitude (height above ground)

// The number of sample points taken along the ray.
int NumberOfSamples = 3;

// Extinction/Scatter coefficient for Rayleigh.
float3 BetaRayleigh;

// Extinction/Scatter coefficient for Mie.
float3 BetaMie;

// g for Mie.
float GMie = -0.75;

float Transmittance = 1;

float4 BaseHorizonColor;   // BaseHorizonColor (RGB) + Shift
float3 BaseZenithColor;


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
};

struct VSOutput
{
  float3 PositionWorld : TEXCOORD;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Shaders
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.PositionWorld = input.Position.xyz;
  output.Position = mul(input.Position, mul(View, Projection)).xyww;  //  Set z to w to move vertex to far clip plane.
  return output;
}


float3 LerpColors(float3 color0, float3 color1, float shift, float parameter)
{
  float3 colorAverage = (color0 + color1) / 2;
  if (parameter < shift)
    return lerp(color0, colorAverage, parameter / shift);
  else
    return lerp(colorAverage, color1, (parameter - shift) / (1 - shift));
}


float4 PS(float3 positionWorld, bool useBaseColor, bool correctGamma)
{
  float3 cameraToSkyDirection = normalize(positionWorld);
  if (cameraToSkyDirection.y < -0.2f)
  {
    clip(-1);
    return 0;
  }
  
  // We shoot a ray from the camera to the vertex and take samples along the ray.
  float3 rayStart = float3(0, RadiusCamera, 0); // CameraPosition in planet space.
  
  // Get length of ray by shooting the ray against the atmosphere top.
  float dummy, rayLength;
  float hasHit = HitSphere(rayStart, cameraToSkyDirection, RadiusAtmosphere, dummy, rayLength);
  
  // For games in outer space we can abort if we do not hit the atmosphere.
  // clip(hasHit);
  // The ray start would have to be moved to the first point inside the atmosphere...
  
  float3 colorRayleigh, colorMie;
  float3 transmittance;
  ComputeAtmosphericScattering(rayStart, cameraToSkyDirection, rayLength, false,
                               SunDirection, RadiusGround, RadiusAtmosphere, 
                               ScaleHeight, NumberOfSamples, BetaRayleigh, BetaMie,
                               transmittance, colorRayleigh, colorMie);
  
  // Weigh the colors with the phase function and sum them up.
  // Note: This should be done in the PS. The above part could be done in the VS.
  float cosTheta = dot(SunDirection, cameraToSkyDirection);
  float4 color;
  color.rgb = colorRayleigh * PhaseFunctionRayleigh(cosTheta) + colorMie * PhaseFunction(cosTheta, GMie);
  color.rgb *= SunIntensity;
  
  if (useBaseColor)
  {
    float f = 1 - saturate(acos(cameraToSkyDirection.y) / Pi * 2); // 1 = zenith, 0 = horizon
    color.rgb += LerpColors(BaseHorizonColor.rgb, BaseZenithColor.rgb, BaseHorizonColor.a, f);
  }
  
  // transmittance is float3. We arbitrarily use the transmittance of the g channel.
  color.a = 1 - (transmittance.g * Transmittance); 
  
  //color.rgb = TonemapExponential(color.rgb);
  
  if (correctGamma)
    color.rgb = ToGamma(color.rgb);
  
  return color;
}
float4 PSLinear(float3 positionWorld : TEXCOORD0) : COLOR { return PS(positionWorld, false, false); }
float4 PSGamma(float3 positionWorld : TEXCOORD0)  : COLOR { return PS(positionWorld, false, true);  }
float4 PSLinearWithBaseColor(float3 positionWorld : TEXCOORD0) : COLOR { return PS(positionWorld, true, false); }
float4 PSGammaWithBaseColor(float3 positionWorld : TEXCOORD0)  : COLOR { return PS(positionWorld, true, true);  }


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
  pass Linear                  // Output linear color values. No base color.
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSLinear();
  }
  
  pass Gamma                   // Output gamma corrected values. No base color.
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSGamma();
  }
  pass LinearWithBaseColor     // Output linear color values. Add baes color.
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSLinearWithBaseColor();
  }
  
  pass GammaWithBaseColor      // Output gamma corrected values. Add base color.
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSGammaWithBaseColor();
  }
}
