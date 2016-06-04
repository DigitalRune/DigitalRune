//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file RebuildZBuffer.fx
/// Reconstructs the Z-buffer from the G-buffer and optionally copies a texture
/// into the primary render target. (Because of different precision of the depth
/// values in the G-buffer, the resulting Z-buffer is only an approximation of
/// the original Z-buffer.)
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 ViewportSize : VIEWPORTSIZE;
float4x4 Projection : PROJECTION;
float CameraFar : CAMERAFAR;

// Declare texture GBuffer0 and sampler GBuffer0Sampler.
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);

// Optional: Write color or texture.
float4 Color;
texture SourceTexture;
sampler SourceSampler : register(s1) = sampler_state
{
  Texture = <SourceTexture>;
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
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


void PS(float2 texCoord : TEXCOORD,
        out float4 color : COLOR,
        out float depth : DEPTH,
        uniform const bool copyTexture,
        uniform const bool isPerspective)
{
  // Write color or texture to render target.
  if (copyTexture)
    color = tex2D(SourceSampler, texCoord);
  else
    color = Color;
  
  float4 gBuffer0 = tex2D(GBuffer0Sampler, texCoord);
  float linearZ = GetGBufferDepth(gBuffer0) * CameraFar;
  
  // This is what we want to do:
  //float4 positionClip = mul(float4(0, 0, -linearZ, 1), Projection);
  //depth = positionClip.z / positionClip.w;
  
  // Optimized versions:
  if (isPerspective)
  {
    // Perspective projection:
    // Since the Projection matrix has 0 elements, we only need z, and w is
    // equal to linearZ:
    depth = saturate((-linearZ * Projection._m22 + Projection._m32) / linearZ);
    
    // Effect compiler bug: Compiler removes saturate and pixel with a depth which is slightly above 1,
    // fail the depth test (even if DepthBufferFunction is Always)! --> We have to reformulate the equation.
    //depth = (-linearZ * Projection._m22 + Projection._m32) / linearZ;
    //depth = max(0, min(1, depth));
    // --> We can ignore this fix if we assume that the target z buffer is cleared to 1 anyway.
  }
  else
  {
    // Orthographic projection:
    depth = saturate((-linearZ * Projection._m22 + Projection._m32));
  }
}


void PSOrthographic_ColorAndDepth(float2 texCoord : TEXCOORD, out float4 color : COLOR, out float depth : DEPTH)
{
  PS(texCoord, color, depth, false, false);
}


void PSOrthographic_TextureAndDepth(float2 texCoord : TEXCOORD, out float4 color : COLOR, out float depth : DEPTH)
{
  PS(texCoord, color, depth, true, false);
}


void PSPerspective_ColorAndDepth(float2 texCoord : TEXCOORD, out float4 color : COLOR, out float depth : DEPTH)
{
  PS(texCoord, color, depth, false, true);
}


void PSPerspective_TextureAndDepth(float2 texCoord : TEXCOORD, out float4 color : COLOR, out float depth : DEPTH)
{
  PS(texCoord, color, depth, true, true);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique Orthographic
{
  pass ColorAndDepth
  {
#if !SM4
    VertexShader = compile vs_2_0 VS();
    PixelShader = compile ps_2_0 PSOrthographic_ColorAndDepth();
#else
    VertexShader = compile vs_4_0_level_9_1 VS();
    PixelShader = compile ps_4_0_level_9_1 PSOrthographic_ColorAndDepth();
#endif
  }
  
  pass TextureAndDepth
  {
#if !SM4
    VertexShader = compile vs_2_0 VS();
    PixelShader = compile ps_2_0 PSOrthographic_TextureAndDepth();
#else
    VertexShader = compile vs_4_0_level_9_1 VS();
    PixelShader = compile ps_4_0_level_9_1 PSOrthographic_TextureAndDepth();
#endif
  }
}


technique Perspective
{
  pass ColorAndDepth
  {
#if !SM4
    VertexShader = compile vs_2_0 VS();
    PixelShader = compile ps_2_0 PSPerspective_ColorAndDepth();
#else
    VertexShader = compile vs_4_0_level_9_1 VS();
    PixelShader = compile ps_4_0_level_9_1 PSPerspective_ColorAndDepth();
#endif
  }
  
  pass TextureAndDepth
  {
#if !SM4
    VertexShader = compile vs_2_0 VS();
    PixelShader = compile ps_2_0 PSPerspective_TextureAndDepth();
#else
    VertexShader = compile vs_4_0_level_9_1 VS();
    PixelShader = compile ps_4_0_level_9_1 PSPerspective_TextureAndDepth();
#endif
  }
}
