//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file GradientSky.fx
/// Renders a sky using simple predefined color gradients and a CIE sky
/// luminance distribution.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float3 SunDirection;  // The direction to the sun.

// Colors using premultiplied alpha.
float4 FrontColor;
float4 BackColor;
float4 ZenithColor;
float4 GroundColor;

float4 Shift;
#define FrontZenithShift Shift.x
#define BackZenithShift Shift.y
#define FrontGroundShift Shift.z
#define BackGroundShift Shift.w

// CIE luminance parameters
float4 Abcd = float4(-1.0, -0.32, 10.0, -3.0);
float2 EAndStrength = float2(0.45, 1);
#define A Abcd.x
#define B Abcd.y
#define C Abcd.z
#define D Abcd.w
#define E EAndStrength.x
#define CieStrength EAndStrength.y


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
  output.Position = mul(input.Position, mul(View, Projection)).xyww;  // Set z to w to move vertex to far clip plane.
  return output;
}


float4 LerpColors(float4 color0, float4 color1, float shift, float parameter)
{
  float4 colorAverage = (color0 + color1) / 2;
  if (parameter < shift)
    return lerp(color0, colorAverage, parameter / shift);
  else
    return lerp(colorAverage, color1, (parameter - shift) / (1 - shift));
}


float4 PS(float3 positionWorld : TEXCOORD, bool applyCie, bool correctGamma)
{
  float3 toPosition = normalize(positionWorld);
  
  float f = acos(toPosition.y) / Pi * 2; // 0 = zenith, 1 = horizon, 2 = ground
  float4 secondColor;
  float frontShift, backShift;
  if (toPosition.y >= 0)
  {
    f = 1 - f;
    secondColor = ZenithColor;
    frontShift = FrontZenithShift;
    backShift = BackZenithShift;
  }
  else
  {
    f = f - 1;
    secondColor = GroundColor;
    frontShift = FrontGroundShift;
    backShift = BackGroundShift;
  }
  
  float4 front = LerpColors(FrontColor, secondColor, frontShift, f);
  float4 back = LerpColors(BackColor, secondColor, backShift, f);
  
  // Cosine of angle between direction to sun and to pixel in the horizontal plane.
  float cosAngle = dot(SunDirection.xz, toPosition.xz) / (length(SunDirection.xz) * length(toPosition.xz));
  
  // Interpolation factor; 0 = back, 1 = front.
  f = cosAngle * 0.5 + 0.5;
  float4 color = lerp(back, front, f);
  
  if (applyCie)
  {
    // Angle between sun and pixel.
    float cosTheta = dot(SunDirection, toPosition);
    float theta = acos(cosTheta);
    
    // Angle between pixel and zenith (which is (0, 1, 0)).
    float cosPhi = toPosition.y;
    
    // Angle between sun and zenith.
    float cosThetaZ = SunDirection.y;
    
    // Compute relative luminance.
    float luminanceAttenuation =
      (1 + A * exp(B / cosPhi)) * (1 + C * (exp(D * theta) - exp(D * Pi / 2)) + E * cosTheta *cosTheta)
      / ((1 + A * exp(B / cosThetaZ)) * (1 + C * (1 - exp(D * Pi / 2)) + E));
    
    luminanceAttenuation = lerp(1, luminanceAttenuation, CieStrength);
    color.rgb *= luminanceAttenuation;
  }
  
  if (correctGamma)
    color.rgb = ToGamma(color.rgb);
  
  return color;
}
float4 PSLinear(float3 positionWorld : TEXCOORD0) : COLOR { return PS(positionWorld, false, false); }
float4 PSGamma(float3 positionWorld : TEXCOORD0)  : COLOR { return PS(positionWorld, false, true);  }
float4 PSCieLinear(float3 positionWorld : TEXCOORD0) : COLOR { return PS(positionWorld, true, false); }
float4 PSCieGamma(float3 positionWorld : TEXCOORD0)  : COLOR { return PS(positionWorld, true, true);  }


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_3_0
#define PSTARGET ps_3_0
#else
#define VSTARGET vs_4_0_level_9_3
#define PSTARGET ps_4_0_level_9_3
#endif

technique
{
  pass Linear
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSLinear();
  }
  
  pass Gamma
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSGamma();
  }
  
  pass CieLinear
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCieLinear();
  }
  
  pass CieGamma
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCieGamma();
  }
}
