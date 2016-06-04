//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file BloomFilter.fx
/// Adds a bloom effect.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

// The brightness threshold.
float BloomThreshold;

// An intensity factor that is applied to the bright areas.
float BloomIntensity = 2;

// The saturation of the bright parts.
// - Use values < 1 to decrease saturation.
// - Use values > 1 to increase saturation.
float BloomSaturation = 1;

// The input texture.
texture SceneTexture;
sampler SceneSampler = sampler_state
{
  Texture = <SceneTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  MipFilter = NONE;
};

// The bloom texture (downsampled, blurred scene).
texture BloomTexture;
sampler BloomSampler = sampler_state
{
  Texture = <BloomTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
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
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


float4 PSBrightness(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 color = tex2D(SceneSampler, texCoord);
  
  // Get luminance.
  float luminance = dot(color.xyz, LuminanceWeights);
  
  // Control saturation.
  color.rgb = lerp(luminance, color.rgb, BloomSaturation);
  
  // ----- Method 1:
  // From XNA bloom sample.
  // Extract bright parts by clamping dark values and renormalizing to [0, 1].
  color.rgb = saturate((color.rgb - BloomThreshold.xxx) / (1 - BloomThreshold.xxx));
  return color;
  
  // ----- Method 2:
  // From "Programming Vertex and Pixel Shader: Fake HDR"
  // Scale color with pixel luminance.
  color = color * luminance;
  return color;
}


float4 PSCombine(float2 texCoord : TEXCOORD0): COLOR
{
  float4 scene = tex2D(SceneSampler, texCoord);
  float4 bloom = tex2D(BloomSampler, texCoord) * BloomIntensity;
  
  // ----- Method 1: Add
  //return scene + bloom;
  
  // ----- Method 2: Lerp
  // From "Fake HDR". Works only for bright bloom scene.
  //return lerp(scene, bloom, 0.75);
  
  // ----- Method 3: Blend mode "Screen"
  // Screen blend mode is more subtle than simple Add and avoids saturating
  // the image.
  return scene * (1 - saturate(bloom)) + bloom;
  // Same as: return scene + bloom * saturate(1 - scene);
  
  // ----- Method 4: Adaptive Add/Lerp
  // From "Adaptive Glare".
  //float pixelLuminance = dot(scene, LuminanceWeights);
  //return lerp(scene + bloom, scene, pixelLuminance);
  
  // ----- Method X: ...
  // We could try many more blending modes. See Photoshop blending modes
  // for examples.
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
#else
#define VSTARGET vs_4_0_level_9_1
#define PSTARGET ps_4_0_level_9_1
#endif

technique
{
  pass Brightness
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSBrightness();
  }
  
  pass Combine
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCombine();
  }
}
