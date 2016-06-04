//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file DownsampleFilter.fx
/// Downsamples an image.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The screen size.
float2 SourceSize;
float2 TargetSize;

// The input texture.
texture SourceTexture;
sampler SourceSamplerLinear = sampler_state
{
  Texture = <SourceTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  MipFilter = NONE;
};
sampler SourceSamplerPoint = sampler_state
{
  Texture = <SourceTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};


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

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, TargetSize);
  output.TexCoord = input.TexCoord;
  return output;
}


// Downsampling to TargetSize = SourceSize / 2 using bilinear hardware filtering.
float4 PSLinear2(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 1 sample using hardware filtering.
  return tex2D(SourceSamplerLinear, texCoord);
}


// Downsampling to TargetSize = SourceSize / 4 using bilinear hardware filtering.
float4 PSLinear4(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 2x2 samples with 1 texel offset.
  float2 o = 1 / SourceSize;
  float4 s0 = tex2D(SourceSamplerLinear, texCoord + float2(-o.x, -o.y));
  float4 s1 = tex2D(SourceSamplerLinear, texCoord + float2( o.x, -o.y));
  float4 s2 = tex2D(SourceSamplerLinear, texCoord + float2(-o.x,  o.y));
  float4 s3 = tex2D(SourceSamplerLinear, texCoord + float2( o.x,  o.y));
  return (s0 + s1 + s2 + s3) / 4;
}


// Downsampling to TargetSize = SourceSize / 6 using bilinear hardware filtering.
float4 PSLinear6(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 3x3 samples with 2 texel offset.
  float2 o = 2 / SourceSize;
  float4 s0 = tex2D(SourceSamplerLinear, texCoord + float2(-o.x, -o.y));
  float4 s1 = tex2D(SourceSamplerLinear, texCoord + float2(   0, -o.y));
  float4 s2 = tex2D(SourceSamplerLinear, texCoord + float2( o.x, -o.y));
  float4 s3 = tex2D(SourceSamplerLinear, texCoord + float2(-o.x,    0));
  float4 s4 = tex2D(SourceSamplerLinear, texCoord + float2(   0,    0));
  float4 s5 = tex2D(SourceSamplerLinear, texCoord + float2( o.x,    0));
  float4 s6 = tex2D(SourceSamplerLinear, texCoord + float2(-o.x,  o.y));
  float4 s7 = tex2D(SourceSamplerLinear, texCoord + float2(   0,  o.y));
  float4 s8 = tex2D(SourceSamplerLinear, texCoord + float2( o.x,  o.y));
  return (s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8) / 9;
}


// Downsampling to TargetSize = SourceSize / 8 using bilinear hardware filtering.
float4 PSLinear8(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 4x4 samples
  float2 o = 1 / SourceSize;
  float4  s0 = tex2D(SourceSamplerLinear, texCoord + float2(-3 * o.x, -3 * o.y));
  float4  s1 = tex2D(SourceSamplerLinear, texCoord + float2(-1 * o.x, -3 * o.y));
  float4  s2 = tex2D(SourceSamplerLinear, texCoord + float2( 1 * o.x, -3 * o.y));
  float4  s3 = tex2D(SourceSamplerLinear, texCoord + float2( 3 * o.x, -3 * o.y));
  float4  s4 = tex2D(SourceSamplerLinear, texCoord + float2(-3 * o.x, -1 * o.y));
  float4  s5 = tex2D(SourceSamplerLinear, texCoord + float2(-1 * o.x, -1 * o.y));
  float4  s6 = tex2D(SourceSamplerLinear, texCoord + float2( 1 * o.x, -1 * o.y));
  float4  s7 = tex2D(SourceSamplerLinear, texCoord + float2( 3 * o.x, -1 * o.y));
  float4  s8 = tex2D(SourceSamplerLinear, texCoord + float2(-3 * o.x,  1 * o.y));
  float4  s9 = tex2D(SourceSamplerLinear, texCoord + float2(-1 * o.x,  1 * o.y));
  float4 s10 = tex2D(SourceSamplerLinear, texCoord + float2( 1 * o.x,  1 * o.y));
  float4 s11 = tex2D(SourceSamplerLinear, texCoord + float2( 3 * o.x,  1 * o.y));
  float4 s12 = tex2D(SourceSamplerLinear, texCoord + float2(-3 * o.x,  3 * o.y));
  float4 s13 = tex2D(SourceSamplerLinear, texCoord + float2(-1 * o.x,  3 * o.y));
  float4 s14 = tex2D(SourceSamplerLinear, texCoord + float2( 1 * o.x,  3 * o.y));
  float4 s15 = tex2D(SourceSamplerLinear, texCoord + float2( 3 * o.x,  3 * o.y));
  return (s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8 + s9 + s10 + s11 + s12 + s13 +s14 + s15) / 16;
}


// Downsampling to TargetSize = SourceSize / 2 without hardware filtering.
float4 PSPoint2(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 2x2 samples with 0.5 texel offset.
  float2 o = 0.5 / SourceSize;
  float4 s0 = tex2D(SourceSamplerPoint, texCoord + float2(-o.x, -o.y));
  float4 s1 = tex2D(SourceSamplerPoint, texCoord + float2( o.x, -o.y));
  float4 s2 = tex2D(SourceSamplerPoint, texCoord + float2(-o.x,  o.y));
  float4 s3 = tex2D(SourceSamplerPoint, texCoord + float2( o.x,  o.y));
  return (s0 + s1 + s2 + s3) / 4;
}


// Downsampling to TargetSize = SourceSize / 3 without hardware filtering.
float4 PSPoint3(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 3x3 samples with 1.0 texel offset.
  float2 o = 1 / SourceSize;
  float4 s0 = tex2D(SourceSamplerPoint, texCoord + float2(-o.x, -o.y));
  float4 s1 = tex2D(SourceSamplerPoint, texCoord + float2(   0, -o.y));
  float4 s2 = tex2D(SourceSamplerPoint, texCoord + float2( o.x, -o.y));
  float4 s3 = tex2D(SourceSamplerPoint, texCoord + float2(-o.x,    0));
  float4 s4 = tex2D(SourceSamplerPoint, texCoord + float2(   0,    0));
  float4 s5 = tex2D(SourceSamplerPoint, texCoord + float2( o.x,    0));
  float4 s6 = tex2D(SourceSamplerPoint, texCoord + float2(-o.x,  o.y));
  float4 s7 = tex2D(SourceSamplerPoint, texCoord + float2(   0,  o.y));
  float4 s8 = tex2D(SourceSamplerPoint, texCoord + float2( o.x,  o.y));
  return (s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8) / 9;
}


// Downsampling to TargetSize = SourceSize / 4 without hardware filtering.
float4 PSPoint4(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 4x4 samples with offsets 0.5 and 1.5.
  float2 o = 0.5 / SourceSize;
  float4  s0 = tex2D(SourceSamplerPoint, texCoord + float2(-3 * o.x, -3 * o.y));
  float4  s1 = tex2D(SourceSamplerPoint, texCoord + float2(-1 * o.x, -3 * o.y));
  float4  s2 = tex2D(SourceSamplerPoint, texCoord + float2( 1 * o.x, -3 * o.y));
  float4  s3 = tex2D(SourceSamplerPoint, texCoord + float2( 3 * o.x, -3 * o.y));
  float4  s4 = tex2D(SourceSamplerPoint, texCoord + float2(-3 * o.x, -1 * o.y));
  float4  s5 = tex2D(SourceSamplerPoint, texCoord + float2(-1 * o.x, -1 * o.y));
  float4  s6 = tex2D(SourceSamplerPoint, texCoord + float2( 1 * o.x, -1 * o.y));
  float4  s7 = tex2D(SourceSamplerPoint, texCoord + float2( 3 * o.x, -1 * o.y));
  float4  s8 = tex2D(SourceSamplerPoint, texCoord + float2(-3 * o.x,  1 * o.y));
  float4  s9 = tex2D(SourceSamplerPoint, texCoord + float2(-1 * o.x,  1 * o.y));
  float4 s10 = tex2D(SourceSamplerPoint, texCoord + float2( 1 * o.x,  1 * o.y));
  float4 s11 = tex2D(SourceSamplerPoint, texCoord + float2( 3 * o.x,  1 * o.y));
  float4 s12 = tex2D(SourceSamplerPoint, texCoord + float2(-3 * o.x,  3 * o.y));
  float4 s13 = tex2D(SourceSamplerPoint, texCoord + float2(-1 * o.x,  3 * o.y));
  float4 s14 = tex2D(SourceSamplerPoint, texCoord + float2( 1 * o.x,  3 * o.y));
  float4 s15 = tex2D(SourceSamplerPoint, texCoord + float2( 3 * o.x,  3 * o.y));
  return (s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8 + s9 + s10 + s11 + s12 + s13 +s14 + s15) / 16;
}


// Downsampling to TargetSize = SourceSize / 2 without hardware filtering.
// Using abs() because Depth Buffer may store info in the sign of the values.
float4 PSPoint2Depth(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 2x2 samples with 0.5 texel offset.
  float2 o = 0.5 / SourceSize;
  float4 s0 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-o.x, -o.y)));
  float4 s1 = abs(tex2D(SourceSamplerPoint, texCoord + float2( o.x, -o.y)));
  float4 s2 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-o.x,  o.y)));
  float4 s3 = abs(tex2D(SourceSamplerPoint, texCoord + float2( o.x,  o.y)));
  return (s0 + s1 + s2 + s3) / 4;
}


// Downsampling to TargetSize = SourceSize / 3 without hardware filtering.
// Using abs() because Depth Buffer may store info in the sign of the values.
float4 PSPoint3Depth(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 3x3 samples with 1.0 texel offset.
  float2 o = 1 / SourceSize;
  float4 s0 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-o.x, -o.y)));
  float4 s1 = abs(tex2D(SourceSamplerPoint, texCoord + float2(   0, -o.y)));
  float4 s2 = abs(tex2D(SourceSamplerPoint, texCoord + float2( o.x, -o.y)));
  float4 s3 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-o.x,    0)));
  float4 s4 = abs(tex2D(SourceSamplerPoint, texCoord + float2(   0,    0)));
  float4 s5 = abs(tex2D(SourceSamplerPoint, texCoord + float2( o.x,    0)));
  float4 s6 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-o.x,  o.y)));
  float4 s7 = abs(tex2D(SourceSamplerPoint, texCoord + float2(   0,  o.y)));
  float4 s8 = abs(tex2D(SourceSamplerPoint, texCoord + float2( o.x,  o.y)));
  return (s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8) / 9;
}


// Downsampling to TargetSize = SourceSize / 4 without hardware filtering.
// Using abs() because Depth Buffer may store info in the sign of the values.
float4 PSPoint4Depth(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 4x4 samples with offsets 0.5 and 1.5.
  float2 o = 0.5 / SourceSize;
  float4  s0 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-3 * o.x, -3 * o.y)));
  float4  s1 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-1 * o.x, -3 * o.y)));
  float4  s2 = abs(tex2D(SourceSamplerPoint, texCoord + float2( 1 * o.x, -3 * o.y)));
  float4  s3 = abs(tex2D(SourceSamplerPoint, texCoord + float2( 3 * o.x, -3 * o.y)));
  float4  s4 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-3 * o.x, -1 * o.y)));
  float4  s5 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-1 * o.x, -1 * o.y)));
  float4  s6 = abs(tex2D(SourceSamplerPoint, texCoord + float2( 1 * o.x, -1 * o.y)));
  float4  s7 = abs(tex2D(SourceSamplerPoint, texCoord + float2( 3 * o.x, -1 * o.y)));
  float4  s8 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-3 * o.x,  1 * o.y)));
  float4  s9 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-1 * o.x,  1 * o.y)));
  float4 s10 = abs(tex2D(SourceSamplerPoint, texCoord + float2( 1 * o.x,  1 * o.y)));
  float4 s11 = abs(tex2D(SourceSamplerPoint, texCoord + float2( 3 * o.x,  1 * o.y)));
  float4 s12 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-3 * o.x,  3 * o.y)));
  float4 s13 = abs(tex2D(SourceSamplerPoint, texCoord + float2(-1 * o.x,  3 * o.y)));
  float4 s14 = abs(tex2D(SourceSamplerPoint, texCoord + float2( 1 * o.x,  3 * o.y)));
  float4 s15 = abs(tex2D(SourceSamplerPoint, texCoord + float2( 3 * o.x,  3 * o.y)));
  return (s0 + s1 + s2 + s3 + s4 + s5 + s6 + s7 + s8 + s9 + s10 + s11 + s12 + s13 +s14 + s15) / 16;
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if SM4
#define VSTARGET vs_4_0_level_9_3
#define PSTARGET ps_4_0_level_9_3
#elif OPENGL
#define VSTARGET vs_3_0
#define PSTARGET ps_3_0
#else
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
#endif

technique
{
  // ----- Passes using bilinear hardware filtering.
  pass Linear2
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSLinear2();
  }
  
  pass Linear4
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSLinear4();
  }
  
  pass Linear6
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSLinear6();
  }
  
  pass Linear8
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSLinear6();
  }
  
  // ----- Passes without hardware filtering.
  pass Point2
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSPoint2();
  }
  
  pass Point3
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSPoint3();
  }
  
  pass Point4
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSPoint4();
  }
  
  // ----- Passes without hardware filtering for depth buffers.
  pass Point2Depth
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSPoint2Depth();
  }
  
  pass Point3Depth
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSPoint3Depth();
  }
  
  pass Point4Depth
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSPoint4Depth();
  }
}
