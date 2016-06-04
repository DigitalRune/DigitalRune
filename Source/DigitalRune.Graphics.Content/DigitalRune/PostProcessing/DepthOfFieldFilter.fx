//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file DepthOfField.fx
/// Creates a depth-of-field effect.
//
// Notes:
// CoC = Circle of Confusion
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The size of the source image in pixels.
float2 ScreenSize;

// The camera far distance.
float Far = 100;

// The near distance where the blur starts to decrease.
float NearBlurDistance = 2;
// The near distance where the objects start to be in focus.
float NearFocusDistance = 3;
// The far distance where objects start to get blurry.
float FarFocusDistance = 4;
// The far distance after which objects are maximal blurred.
float FarBlurDistance = 8;

// Blur offsets and weights.
static const int NumberOfConvolutionFilterSamples = 5;
float2 Offsets[NumberOfConvolutionFilterSamples];
float Weights[NumberOfConvolutionFilterSamples];

// The scene.
texture SceneTexture;
// This sampler is mapped to the s0 because the sampler state is set in C# code.
sampler SceneSampler : register(s0) = sampler_state
{
  Texture = <SceneTexture>;
};

// The blurred scene.
texture BlurTexture;
sampler BlurSampler = sampler_state
{
  Texture = <BlurTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  MipFilter = NONE;
};

// The depth texture.
texture DepthTexture;
sampler DepthSampler = sampler_state
{
  Texture = <DepthTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

// A downsampled version of the depth texture.
texture DownsampledDepthTexture;
sampler DownsampledDepthSampler = sampler_state
{
  Texture = <DownsampledDepthTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

// A downsampled texture containing CoC values.
texture DownsampledCocTexture;
sampler DownsampledCocSampler = sampler_state
{
  Texture = <DownsampledCocTexture>;
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
  output.Position = ScreenToProjection(input.Position, ScreenSize);
  output.TexCoord = input.TexCoord;
  return output;
}


// Compute the circle of confusion ([0, 1]) for a normalized linear depth ([0, 1]).
float ComputeCircleOfConfusion(float depth)
{
  // For non-linear clip space depth.
  // Convert depth from clip space to view space depth.
  // If depth is in given in clip space range [0, 1].
  //depth = ConvertDepthFromClipSpaceToViewSpace(depth, Near, Far);
  
  depth = depth * Far;
  
  float coc = 1 - smoothstep(NearBlurDistance, NearFocusDistance, depth)
              + smoothstep(FarFocusDistance, FarBlurDistance, depth);
  return coc;
}


// Reads the depth texture and returns the CoC.
float4 PSCoc(float2 texCoord : TEXCOORD0) : COLOR0
{
  float depth = GetGBufferDepth(tex2D(DepthSampler, texCoord));
  float coc = ComputeCircleOfConfusion(depth);
  return coc;
}


// Blurs the blur texture. The samples weights are multiplied with their CoC
// to avoid halos of focussed objects.
float4 PSBlurWithCoc(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 color = 0.0f;
  float4 totalWeights = 0.0f;
  
  float centerDepth = GetGBufferDepth(tex2D(DownsampledDepthSampler, texCoord));
  for(int i = 0; i < NumberOfConvolutionFilterSamples; i++ )
  {
    //float coc = ComputeCircleOfConfusion(depth);
    float coc = tex2D(DownsampledCocSampler, texCoord + Offsets[i]).r;
    
    // Pixels behind the current center pixel are fully blurred.
    // The current center pixel is also fully blurred.
    float depth = GetGBufferDepth(tex2D(DownsampledDepthSampler, texCoord + Offsets[i]));
    if (centerDepth < depth || i ==  NumberOfConvolutionFilterSamples / 2)   // TODO: Check assembler code. If necessary remove the branching manually using saturate().
      coc = 1;
    
    // The sample is weighted by the blur kernel weight and the CoC because
    // foccused texels (CoC = 0) should not blur out.
    float weight = Weights[i] * coc;
    
    color += tex2D(BlurSampler, texCoord + Offsets[i]) * weight;
    totalWeights += weight;
  }
  
  color /= totalWeights;
  return color;
}


// Creates the final depth-of-field image.
float4 PSDepthOfField(float2 texCoord : TEXCOORD0) : COLOR
{
  // Color of focused pixel.
  float4 normalColor = tex2D(SceneSampler, texCoord);
  
  // Color of fully blurred pixel.
  float4 blurredColor = tex2D(BlurSampler, texCoord);
  
  // Depth of the current pixel.
  float depth = GetGBufferDepth(tex2D(DepthSampler, texCoord));
  
  // Downsampled depth (= averaged depth).
  float downsampledDepth = GetGBufferDepth(tex2D(DownsampledDepthSampler, texCoord));
  
  // CoC of the current pixel.
  float coc = ComputeCircleOfConfusion(depth);
  
  // Blurred CoC of the current pixel.
  float blurredCoc = tex2D(DownsampledCocSampler, texCoord).x;
  
  // If we use only the CoC of the current pixel, the blurred near objects
  // have a sharp border. But instead they should blur over the focused pixels.
  // We compare the blurred depth with the exact depth. The blurred depth
  // is less than the exact depth where the near objects should blur over the
  // focused objects.
  if (downsampledDepth < depth)
  {
    // Estimate the CoC of the neighborhood: See GPU Gems 3.
    // TODO: We should blur this estimated neighborhood CoC image.
    coc = saturate(2 * max(coc, blurredCoc) - coc);
  }
  
  // Mix the sharp and the blurred scene image.
  float4 result = lerp(normalColor, blurredColor, coc);
  return result;
}



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
  // Takes depth-texture and outputs the circle of confusion (CoC).
  pass CircleOfConfusion
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCoc();
  }
  
  // Blurs the scene using the CoC texture.
  pass Blur
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSBlurWithCoc();
  }
  
  // Combines blurred and original scene.
  pass DepthOfField
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSDepthOfField();
  }
}
