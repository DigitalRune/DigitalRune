//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Intersection.fx
/// Renders a CSG intersection image using depth peeling.
//
// Rendering uses 1 render target which stores depth (encoded in RGB) and
// diffuse light intensity (in A). A = 0 is used to identify "empty" render
// target pixels (space without intersections).

// Pass Peel renders a single depth layer of a submesh. Peeling is done from
// front layers to back layers.
// Pass Mark is used to mark the stencil buffer.
// Pass Draw computes the shading (diffuse light) of the current layer.
// Pass Combine combines the previous intersection image (containing the previous
// layer or previous intersected objects) with the intersection image of the
// current layer.
// Pass Render combines the intersection image with the current backbuffer.
// Color is applied in this pass.
//
//-----------------------------------------------------------------------------

#include "../DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../DigitalRune.Graphics.Content/DigitalRune/Encoding.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 ViewportSize;

float3 CameraParameters;
#define CameraNear CameraParameters.x
#define CameraFarNearDelta CameraParameters.y   // Far - Near
#define DepthEpsilon CameraParameters.z

float4x4 World;
float4x4 View;
float4x4 Projection;

// The diffuse color of the intersection volume.
float4 Color;

texture Texture0;
sampler Sampler0 = sampler_state
{
  Texture = <Texture0>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = NONE;
};


//-----------------------------------------------------------------------------
// Structs
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float3 Normal : NORMAL;
};

struct VSOutput
{
  float4 PositionProj : TEXCOORD0;
  float3 Normal : TEXCOORD1;
  float4 Position : SV_Position;
};

struct PSInput
{
  float4 PositionProj : TEXCOORD0;
  float3 Normal : TEXCOORD1;
};


struct VSScreenSpaceDrawInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
};

struct VSScreenSpaceDrawOutput
{
  float2 TexCoord : TEXCOORD;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Pass Peel
//-----------------------------------------------------------------------------

VSOutput VSPeel(VSInput input)
{
  VSOutput output = (VSOutput)0;
  float4 positionView = mul(input.Position, mul(World, View));
  output.PositionProj = mul(positionView, Projection);
  output.Position = output.PositionProj;
  return output;
}


float4 PSPeel(PSInput input) : COLOR0
{
  // Get screen space texture coordinate of current pixel.
  float4 positionProj = input.PositionProj;
  float2 texCoordScreen = ProjectionToScreen(positionProj, ViewportSize);
  
  // Get depth of previously peeled layer.
  float3 encodedDepth = tex2Dlod(Sampler0, float4(texCoordScreen, 0, 0)).rgb;
  float lastDepth = DecodeFloatFromRgba(float4(encodedDepth, 0));
  
  // Get depth of current pixel. (Non-linear clip space depth)
  float currentDepth = positionProj.z / positionProj.w;
  
  // Reject pixels which are in front of the last layer.
  clip(currentDepth - lastDepth - DepthEpsilon);
  
  // Store depth.
  return float4(EncodeFloatInRgba(currentDepth).rgb, 0);
}

//-----------------------------------------------------------------------------
// Pass Mark
//-----------------------------------------------------------------------------

float4 VSMark(float4 position : POSITION) : SV_Position
{
  return mul(position, mul(World, mul(View, Projection)));
}


float4 PSMark() : COLOR0
{
  // Output dummy value. Color writes are disabled.
  // This is needed because MonoGame does not support "PixelShader = NULL;" syntax.
  return float4(0, 0, 0, 0);
}


//-----------------------------------------------------------------------------
// Pass Draw
//-----------------------------------------------------------------------------

VSOutput VSDraw(VSInput input)
{
  VSOutput output = (VSOutput)0;
  float4 positionView = mul(input.Position, mul(World, View));
  output.PositionProj = mul(positionView, Projection);
  output.Position = output.PositionProj;
  output.Normal = mul(input.Normal, (float3x3)World);
  return output;
}


float4 PSDraw(PSInput input) : COLOR0
{
  // Get screen space texture coordinate of current pixel.
  float4 positionProj = input.PositionProj;
  float2 texCoordScreen = ProjectionToScreen(positionProj, ViewportSize);
  
  // Get depth of previously peeled layer.
  float lastDepth = DecodeFloatFromRgba(tex2Dlod(Sampler0, float4(texCoordScreen, 0, 0)));
  float currentDepth = positionProj.z / positionProj.w;
  
  // Reject pixels which are in front of the last layer.
  clip(currentDepth - lastDepth - DepthEpsilon);
  
  // Ambient light
  float3 light = float3(0.05333332f, 0.09882354f, 0.1819608f);
  
  // Directional key light
  float3 normal = normalize(input.Normal);
  light += float3(1, 0.9607844f, 0.8078432f) * saturate(dot(normal, -float3(-0.5265408f, -0.5735765f, -0.6275069f)));
  
  // Directional fill light
  light += float3(0.9647059f, 0.7607844f, 0.4078432f) * saturate(dot(normal, -float3(0.7198464f, 0.3420201f, 0.6040227f)));
  
  // Directional back light
  light += float3(0.3231373f, 0.3607844f, 0.3937255f) * saturate(dot(normal, -float3(0.4545195f, -0.7660444f, 0.4545195f)));
  
  // Store depth in RGB.
  float depth = positionProj.z / positionProj.w;
  float4 color = EncodeFloatInRgba(depth);
  
  // Store light intensity in A. Add small epsilon because A == 0 is used to
  // mark "empty" areas. 
  // Currently we store only R, i.e. the light is monochrome, G and B are ignored.
  color.a = light.r + 0.0001;
  return color;
}


//-----------------------------------------------------------------------------
// Pass Combine
//-----------------------------------------------------------------------------

VSScreenSpaceDrawOutput VSCombine(VSScreenSpaceDrawInput input)
{
  VSScreenSpaceDrawOutput output = (VSScreenSpaceDrawOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


void PSCombine(in float2 texCoord : TEXCOORD0,
               out float4 color : COLOR0,
               out float depth : DEPTH)
{
  // Just copy the input image.
  color = tex2Dlod(Sampler0, float4(texCoord, 0, 0));
  
  // Depth test is used to keep the nearest fragments.
  depth = saturate(DecodeFloatFromRgba(float4(color.rgb, 0)));
  
  // A = 0 marks empty areas.
  clip(color.a - 0.00001);
}


//-----------------------------------------------------------------------------
// Pass Render
//-----------------------------------------------------------------------------

VSScreenSpaceDrawOutput VSRender(VSScreenSpaceDrawInput input)
{
  VSScreenSpaceDrawOutput output = (VSScreenSpaceDrawOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


void PSRender(in float2 texCoord : TEXCOORD0,
              out float4 color : COLOR0,
              out float depth : DEPTH)
{
  // Get depth and light intensity.
  float4 s = tex2Dlod(Sampler0, float4(texCoord, 0, 0));
  float4 encodedDepth = float4(s.rgb, 0);
  float light = s.a;
  
  // Light intensity = 0 is used to mark empty areas.
  clip(light - 0.00001);
  
  // Combine tint/material color and diffuse light.
  color = Color;
  color.rgb *= light;
  
  // Output depth to let user use depth test.
  depth = saturate(DecodeFloatFromRgba(encodedDepth));
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
  pass Peel
  {
    VertexShader = compile VSTARGET VSPeel();
    PixelShader = compile PSTARGET PSPeel();
  }
  pass Mark
  {
    VertexShader = compile VSTARGET VSMark();
    PixelShader = compile PSTARGET PSMark();
  }
  pass Draw
  {
    VertexShader = compile VSTARGET VSDraw();
    PixelShader = compile PSTARGET PSDraw();
  }
  pass Combine
  {
    VertexShader = compile VSTARGET VSCombine();
    PixelShader = compile PSTARGET PSCombine();
  }
  pass Render
  {
    VertexShader = compile VSTARGET VSRender();
    PixelShader = compile PSTARGET PSRender();
  }
}
