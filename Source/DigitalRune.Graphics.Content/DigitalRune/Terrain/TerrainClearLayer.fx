//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file TerrainClearLayer.fx
/// Sets all clipmap pixels to constant user-defined values.
//
// If this effect writes 4 color results for 4 render targets, we get a warning
// if less than 4 render targets are set. To avoid the warning, this effect
// contains 4 techniques for 1 to 4 render targets.
// The correct technique is chosen by the TerrainClipmapRenderer.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 ViewportSize;            // The viewport size in pixels.
float4 TerrainClearValues[4];   // The clear values per render target.


//-----------------------------------------------------------------------------
// Shaders
//-----------------------------------------------------------------------------

float4 VS(float4 position : POSITION) : SV_Position
{
  return ScreenToProjection(position, ViewportSize);
}


void PS1(out float4 color0 : COLOR0)
{
  color0 = TerrainClearValues[0];
}


void PS2(out float4 color0 : COLOR0, out float4 color1 : COLOR1)
{
  color0 = TerrainClearValues[0];
  color1 = TerrainClearValues[1];
}


void PS3(out float4 color0 : COLOR0, out float4 color1 : COLOR1,
         out float4 color2 : COLOR2)
{
  color0 = TerrainClearValues[0];
  color1 = TerrainClearValues[1];
  color2 = TerrainClearValues[2];
}


void PS4(out float4 color0 : COLOR0, out float4 color1 : COLOR1,
         out float4 color2 : COLOR2, out float4 color3 : COLOR3)
{
  color0 = TerrainClearValues[0];
  color1 = TerrainClearValues[1];
  color2 = TerrainClearValues[2];
  color3 = TerrainClearValues[3];
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

technique RenderTargets1
{
  pass
  {
    AlphaBlendEnable = FALSE;
    
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS1();
  }
}


technique RenderTargets2
{
  pass
  {
    AlphaBlendEnable = FALSE;
    
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS2();
  }
}


technique RenderTargets3
{
  pass
  {
    AlphaBlendEnable = FALSE;
    
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS3();
  }
}


technique RenderTargets4
{
  pass
  {
    AlphaBlendEnable = FALSE;
    
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS4();
  }
}

