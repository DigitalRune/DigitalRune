//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Starfield.fx
/// Renders stars as viewpoint-oriented billboards.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 ViewportSize : VIEWPORTSIZE;
float4x4 WorldViewProjection : WORLDVIEWPROJECTION;
float3 Intensity;


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 PositionAndSize : POSITION;
  float4 Color : COLOR;
  float2 TexCoord : TEXCOORD;
};
#define GetPosition(input) input.PositionAndSize.xyz
#define GetSize(input) input.PositionAndSize.w


struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float4 Color: TEXCOORD1;
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float4 Color: TEXCOORD1;
};

//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  float3 position = GetPosition(input);
  float size = GetSize(input);
  
  VSOutput output = (VSOutput)0;
  output.TexCoord = (input.TexCoord * 2 - 1) * 2;
  output.Color = input.Color;
  output.Color.rgb *= Intensity;
  
  // ----- General Billboarding
  //// Get billboard axes.
  //float3 right = normalize(cross(position, float3(0, 1, 0)));
  //float3 up = normalize(cross(right, position));
  
  // Compute billboard vertex position.
  // Assumption: TexCoords are 0 or 1.
  // The size of the offset must be the half extent in each direction.
  //float2 offset = (texCoord - float2(0.5, 0.5)) * size;
  //position += offset.x * right + offset.y * up;
  //output.Position = mul(float4(position, 0), WorldViewProjection).xyww;
  
  // ----- Simplified Billboarding for Circles
  // Compute star position at far plane.
  output.Position = mul(float4(position, 0), WorldViewProjection).xyww;
  
  // The size of 1 px. (Clip space range [-1, 1] contains ViewportSize pixels.)
  float2 pixelSizeClip = float2(2, 2) / ViewportSize;
  
  // Compute billboard vertex position.
  // Assumption: TexCoords are 0 or 1.
  float2 offset = (input.TexCoord - float2(0.5, 0.5)) * size * pixelSizeClip;
  output.Position.x += offset.x;
  output.Position.y += offset.y;
  
  return output;
}


float4 PS(PSInput input, bool correctGamma)
{
  float4 color = input.Color;
  
  // The alpha is computed using the same formula as for line anti-aliasing.
  // See Line.fx.
  float x = length(input.TexCoord);
  float alpha = exp2(-2 * x * x * x);
  color.a *= alpha;
  
  if (correctGamma)
    color.rgb = ToGamma(color.rgb);
  
  return color;
}
float4 PSLinear(PSInput input) : COLOR { return PS(input, false); }
float4 PSGamma(PSInput input)  : COLOR { return PS(input, true);  }


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
#else
#define VSTARGET vs_4_0_level_9_3
#define PSTARGET ps_4_0_level_9_3
#endif

technique
{
  pass Linear     // Output linear color values.
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSLinear();
  }
  
  
  pass Gamma      // Output gamma corrected values.
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSGamma();
  }
}
