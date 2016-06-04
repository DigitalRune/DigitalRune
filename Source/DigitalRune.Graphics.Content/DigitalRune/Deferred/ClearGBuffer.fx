//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ClearGBuffer.fx
/// Sets the G-buffer to default values.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float Depth;
float3 Normal;  // The encoded(!) normal value.
float SpecularPower;


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
};

struct VSOutput
{
  float4 GBuffer0 : COLOR0;
  float4 GBuffer1 : COLOR1;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = input.Position;
  
  // The input position must be in clip space ([-1, 1]).

  // -----------------------------------------------------
  // |                     Depth                         |
  // -----------------------------------------------------
  // |  normalX  |  normalY  |  normalZ  | SpecularPower |
  // -----------------------------------------------------
  output.GBuffer0 = Depth;
  output.GBuffer1.xyz = Normal;  // Assume normal is already encoded.
  output.GBuffer1.a = EncodeSpecularPower(SpecularPower);
  
  return output;
}


void PS(inout float4 gBuffer0 : COLOR0, inout float4 gBuffer1 : COLOR1)
{
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique
{
  pass
  {
#if !SM4
    VertexShader = compile vs_2_0 VS();
    PixelShader = compile ps_2_0 PS();
#else
    VertexShader = compile vs_4_0_level_9_1 VS();
    PixelShader = compile ps_4_0_level_9_1 PS();
#endif
  }
}
