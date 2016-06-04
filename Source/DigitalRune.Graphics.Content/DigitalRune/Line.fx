//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Line.fx
/// Renders anti-aliased lines.
//
// References:
// - [1] Fast Prefiltered Lines, GPU Gems 2, pp. 345
// - [2] NVIDIA SolidWireframe Sample
//
//-----------------------------------------------------------------------------

#include "Common.fxh"
#include "Encoding.fxh"
#include "Deferred.fxh"


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

// Use this define to anti-alias the line ends when "caps" are drawn.
//#define CAPS 1


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// A filter table as used in [1] - should be replaced with a lookup texture.
//static const float LineFilterTable[32] = {
//  255, 253, 250, 246, 241, 234, 226, 216,
//  203, 189, 173, 156, 138, 120, 102,  85,
//   70,  56,  43,  32,  24,  18,  14,  11,
//    8,   6,   4,   3,   2,   1,   0,   0
//};
// Lookup in the LineFilterTable --> Should be replaced with texture lookup!
//float GetPrefilteredAlpha(float parameter)
//{
//  parameter = saturate(1 - parameter);
//  int index = (int)(parameter * 32);
//  return lerp(LineFilterTable[index], LineFilterTable[index + 1], frac(parameter * 32)) / 255;
//}

static const float FilterRadius = 1.0f;

float2 ViewportSize;
float4x4 View;
float4x4 ViewInverse;
float4x4 Projection;
float CameraNear;


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Start : POSITION0;     // (Line start, start distance for dash patterns)
  float4 End : TEXCOORD0;       // (Line end, end distance for dash patterns)
  float4 Data : TEXCOORD1;      // (U, V, thickness, not used)
  float4 Color : TEXCOORD2;     // (RGBA)
  float4 Dash : TEXCOORD3;      // Prefix sum of (dash, gap, dash, gap)
};

struct VSOutput
{
  float4 Position : SV_Position;
  float2 Distance : TEXCOORD0;
  float4 Color : TEXCOORD1;
  float4 Dash : TEXCOORD2;
  float3 Edge0 : TEXCOORD3;
  float3 Edge1 : TEXCOORD4;
#if CAPS
  float3 Edge2 : TEXCOORD5;
  float3 Edge3 : TEXCOORD6;
#endif
};

struct PSInput
{
#if !MGFX
  float2 Position : VPOS;         // VPOS must be used instead of SV_Position on Xbox.
#else
  float4 Position : SV_Position;
#endif
  float2 Distance : TEXCOORD0;
  float4 Color : TEXCOORD1;
  float4 Dash : TEXCOORD2;
  float3 Edge0 : TEXCOORD3;
  float3 Edge1 : TEXCOORD4;
#if CAPS
  float3 Edge2 : TEXCOORD5;
  float3 Edge3 : TEXCOORD6;
#endif
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

// Note: Dash pattern may be computed in screen space or in world space. If
// dash pattern should be computed in world space, the distance values in the
// vertex data are negative. - This way we do not need to switch effect parameters
// between line nodes :-)

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  
  bool dashInScreenSpace = input.End.w > 0;
  float isEndVertex = input.Data.x;               // 0 for vertices at line start, 1 for vertices at line end.
  float upDownFactor = sign(input.Data.y - 0.5f); // 1 for down, -1 for up vertices.
  float thickness = input.Data.z;
  
  // start and end are in world space. Transform to view space.
  float4 startView = mul(float4(input.Start.xyz, 1), View);
  float4 endView = mul(float4(input.End.xyz, 1), View);
  
  // Clip to near plane - otherwise lines which end near the camera origin
  // (where planar z == 0) will disappear. (Projection singularity!)
  float deltaZ = abs(startView.z - endView.z);
  float pStart = saturate((startView.z - (-CameraNear)) / deltaZ);
  startView = lerp(startView, endView, pStart);
  float pEnd = saturate((endView.z - (-CameraNear)) / deltaZ);
  endView = lerp(endView, startView, pEnd);
    
  // ----- Code for a line where the width is constant in screen-space.
  float4 startProj = mul(startView, Projection);
  float4 endProj = mul(endView, Projection);
  
  output.Position = lerp(startProj, endProj, isEndVertex);
  
  // Homogeneous divide tranforms to clip space.
  float2 startClip = startProj.xy / startProj.w;
  float2 endClip = endProj.xy / endProj.w;
  
  // Clip space range [-1, 1] contains ViewportSize pixels.
  float2 pixelSizeClip = float2(2, 2) / ViewportSize;
  
  // Offset vertex to give the quad a width (including filter radius for anti-aliasing).
  float2 direction = normalize((endClip.xy - startClip.xy) / pixelSizeClip.xy);  // divide by pixel size to correct viewport aspect ratio.
  float2 normal = float2(-direction.y, direction.x) * upDownFactor;
  output.Position.xy += (normal * (thickness / 2 + FilterRadius)) * output.Position.w * pixelSizeClip;
  
  // TODO: Try to add "caps" for line end-points.
  // This simple code does not work - probably a problem with the near clip plane...
  //output.Position.xy += direction * thickness * lerp(-1, 1, isEndVertex) * output.Position.w * pixelSizeClip;
  
  // Compute edge equations similar to [1]. Warning [1] contains errors.
  // Line start/end points in pixels:
  float x0 = (startClip.x + 1) / 2 * ViewportSize.x;
  float y0 = (-startClip.y + 1) / 2 * ViewportSize.y;
  float x1 = (endClip.x + 1) / 2 * ViewportSize.x;
  float y1 = (-endClip.y + 1) / 2 * ViewportSize.y;
  // Factor which divides by line length.
  float k = 1 / length(float2(x1 - x0, y1 - y0));
  
  // Edge equations are stored in float3. dot(EdgeX, (p.x, p.y, 1)) gives the distance
  // from the line. Edge.xy is the normal vector of the edge. Edge.z is the
  // "Ordinatenabstand" with an additional term, so that the distance is 0 at
  // thickness / 2 + FilterRadius, which is where the anti-aliased pixels start.
  output.Edge0 = -float3(k * (y0 - y1), k * (x1 - x0), k * (x0 * y1 - x1 * y0) + (thickness / 2 - FilterRadius));
  output.Edge1 = -float3(k * (y1 - y0), k * (x0 - x1), k * (x1 * y0 - x0 * y1) + (thickness / 2 - FilterRadius));
#if CAPS
  output.Edge2 = -float3(k * (x1 - x0), k * (y1 - y0), k * (x0 * x0 + y0 * y0 - x0 * x1 - y0 * y1) + (thickness / 2 - FilterRadius));
  output.Edge3 = -float3(k * (x0 - x1), k * (y0 - y1), k * (x1 * x1 + y1 * y1 - x0 * x1 - y0 * y1) + (thickness / 2 - FilterRadius));
#endif
  
  // ----- Code for a line where the width is constant in world-space.
  //float3 direction = normalize(endView.xyz - startView.xyz);
  //float3 forward = float3(0, 0, -1);
  //float3 normal = normalize(cross(direction, forward)) * upDownFactor;
  //startView.xyz += normal * thickness;
  //endView.xyz += normal * thickness;
  //float3 vertexPosition = lerp(startView, endView, vertexInfo.x);
  //output.Position = mul(float4(vertexPosition, 1), Projection);
  // TODO: Compute edge equations for anti-aliasing...
  
  if (dashInScreenSpace)
  {
    // For interpolation of vertex attributes in screen space, we need to multiply
    // by w and divide by w in the pixel shader.
    output.Distance = float2(lerp(input.Start.w, input.End.w, isEndVertex) * output.Position.w, output.Position.w);
  }
  else
  {
    // If the line segment is clipped, we have to consider the offsets
    // between clipped and unclipped positions.
    if (pStart + pEnd > 0)
    {
      float3 startWorldClipped = mul(startView, ViewInverse).xyz;
      float3 endWorldClipped = mul(endView, ViewInverse).xyz;
      input.Start.w = -input.Start.w;
      input.End.w = -input.End.w;
      input.Start.w += length(startWorldClipped - input.Start.xyz);
      input.End.w = input.Start.w + length(endWorldClipped - startWorldClipped);
    }
    output.Distance = float2(lerp(input.Start.w, input.End.w, isEndVertex), 1);
  }
  
  output.Color = input.Color;
  output.Dash = input.Dash;
  return output;
}


float4 PS(PSInput input) : COLOR0
{
  // Handle dash patterns using texkill.
  float4 dash = input.Dash;
  // The distance from the start of the line list might be interpolated in screen-space!
  float dist = input.Distance.x / input.Distance.y;
  // How far are we in the dash pattern.
  float x = abs(dist % dash.w);
  if (x > dash.x && x < dash.y   // In first gap?
      || x > dash.z)             // In second gap?
    clip(-1);
  
  // Compute distance parameter for anti-aliasing.
  // Pixels with d in [0, 2 * FilterRadius] must be filtered.
  float3 pos = float3(input.Position.x, input.Position.y, 1);
  float4 d = float4(dot(input.Edge0, pos), dot(input.Edge1, pos), 0, 0);
#if CAPS
  d.zw = float2(dot(input.Edge2, pos), dot(input.Edge3, pos));
#endif
  
  // Using the lookup table of [1]
  //d = d  / (2 * FilterRadius);
  //if (any(d > 1))
  //  discard;
  //d = saturate(1 - d);
  //float alpha = GetPrefilteredAlpha(min(d.x, d.y));
  //#if CAPS
  //  alpha *= GetPrefilteredAlpha(min(d.z, d.w));
  //#endif
  
  // Using the exponential filter function of [2];
  d = d / FilterRadius;
  if (any(d > 2))
    clip(-1);
  d = max(d, 0);
#if CAPS == 0
  float e = (max(d.x, d.y));
  float alpha = exp2(-2 * e * e * e);
#else
  float2 e = float2(max(d.x, d.y), max(d.z, d.w));
  float2 a = exp2(-2 * e * e * e);
  float alpha = a.x * a.y;
#endif
  
  return input.Color * alpha;
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
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
