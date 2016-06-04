//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file HdrFilter.fx
/// Performs high dynamic range (HDR) tone mapping and adds bloom.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Tonemapping.fxh"
#include "../Color.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

// The brightness threshold.
float BloomThreshold;

// An intensity factor that is applied to the bright areas.
float BloomIntensity = 2;

// The average gray value that corresponds to the average luminance.
// Good values according to "Programming Vertex and Pixel Shader": 0.18, 0.36, 0.54
float MiddleGray = 0.18;

// Limits for the exposure factor which adjust the brightness.
float MinExposure = 0;
float MaxExposure = 10;

float3 BlueShiftColor = float3(1.05, 0.97, 1.27);
float2 BlueShift;   // (one over blue shift center, blue shift exponent)

// The input texture.
Texture2D SceneTexture;
sampler SceneSampler = sampler_state
{
  Texture = <SceneTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;       // No filtering because we usually have float render targets.
  MinFilter = POINT;
  MipFilter = NONE;
};

// The bloom texture.
Texture2D BloomTexture;
sampler BloomSampler = sampler_state
{
  Texture = <BloomTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
  MipFilter = LINEAR;
};

// The luminance texture.
Texture2D LuminanceTexture;
sampler LuminanceSampler = sampler_state
{
  Texture = <LuminanceTexture>;
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
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


float4 PSBrightness(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 scene = tex2D(SceneSampler, texCoord);
  
  // Luminance info: (min, average, max).
  float3 luminance = tex2D(LuminanceSampler, float2(0.5, 0.5)).rgb;
  
  // ----- Method 1:
  // Map average luminance to a middle-gray.
  scene.rgb = AdjustExposure(scene.rgb, luminance.g, MiddleGray, 0, MinExposure, MaxExposure);
  
  // Cut off dark pixels.
  //scene.rgb = max(0, scene.rgb - BloomThreshold.xxx);
  
  // Alternatively, we can clamp the luminance to keep the chromacity intact:
  float pixelLuminance = dot(scene.rgb, LuminanceWeights);
  float cutLuminance = max(0, pixelLuminance - BloomThreshold);
  scene.rgb = cutLuminance * scene.rgb / pixelLuminance;
  
  // ----- Method 2:
  // Seen in MJP Bokeh II sample. We do the same as AdjustExposure() but we
  // apply the threshold to the log of the exposure factor:
  //float exposure = (MiddleGray / (luminance.g + 0.001f));
  //exposure = exp2(log2(exposure + 0.001f) - BloomThreshold);
  //scene.rgb = exposure * scene.rgb;
  
  // Map the result into the range [0, 1] with a tone mapping curve.
  //scene.rgb = TonemapReinhard(scene.rgb);
  //scene.rgb = ToGamma(scene.rgb);
  scene.rgb = TonemapFilmicWithGamma(scene.rgb);
  
  //scene.rgb = TonemapFilmicWithGamma(scene.rgb);
  // In "Programming Vertex and Pixel Shader: Advanced Tone Mapping" a modified
  // Reinhard operator is used: scene.rgb = scene.rgb / (BrightnessOffset + scene.rgb);
  //                            (e.g. BrightnessOffset = 5)
  
  return scene;
}


float4 PSCombine(float2 texCoord : TEXCOORD0, uniform bool enableBlueShift): COLOR
{
  float4 scene = tex2D(SceneSampler, texCoord);
  float4 bloom = tex2D(BloomSampler, texCoord)  * BloomIntensity;
  float3 luminance = tex2D(LuminanceSampler, float2(0.5, 0.5)).rgb;
  
  // We move the colors into a known range:
  // Map average luminance to a middle-gray.
  scene.rgb = AdjustExposure(scene.rgb, luminance.g, MiddleGray, 0, MinExposure, MaxExposure);
  
  // Map the result into the range [0, 1] with a tone mapping curve.
  //scene.rgb = TonemapReinhard(scene.rgb, luminance.b);
  //scene.rgb = TonemapReinhard(scene.rgb);
  //scene.rgb = ToGamma(scene.rgb);
  //scene.rgb = TonemapReinhard(scene.rgb);
  scene.rgb = TonemapFilmicWithGamma(scene.rgb);
  
  float3 sceneLinear = FromGamma(scene.rgb);
  float3 bloomLinear = FromGamma(bloom.rgb);
  
  // Blend mode "Add"
  scene.rgb = sceneLinear + bloomLinear;
  
  // Blend mode "Screen"
  //scene.rgb = bloomLinear + sceneLinear * (1 - saturate(bloomLinear));
  
  if (enableBlueShift)
  {
    // Convert tone-mapped color to CIE XYZ.
    float3 XYZ = mul(scene.rgb, RGBToXYZ);
    
    // Compute luminance as perceived by rods.
    float scotopicY = XYZ.y * (1.33 * (1 + (XYZ.y + XYZ.z) / XYZ.x) - 1.68);
    
    // Compute blue shift interpolation parameter
    float s = 1 / (1 + pow(max(0.000001, luminance.g * BlueShift.x), BlueShift.y));
    
    // Interpolate between cone image (normal scene) and rod image (fully
    // monochromatic blue shifted scene).
    scene.rgb = lerp(scene.rgb, scotopicY * BlueShiftColor, s);
  }
  
  scene.rgb = ToGamma(scene.rgb);
  
  return scene;
}
float4 PSCombineNoBlueShift(float2 texCoord : TEXCOORD0): COLOR { return PSCombine(texCoord, false); }
float4 PSCombineWithBlueShift(float2 texCoord : TEXCOORD0): COLOR { return PSCombine(texCoord, true); }


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
    PixelShader = compile PSTARGET PSCombineNoBlueShift();
  }
  
  pass CombineWithBlueShift
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCombineWithBlueShift();
  }
}
