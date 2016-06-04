//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Blur.fx
/// Applies a separable blur convolution filter.
//
// Notes:
// The Offsets and Weights arrays have a max size determined by
// NumberOfConvolutionFilterSamples, but only NumberOfSamples will be
// effectively used in the pixel shader.
//
// Bilateral weights:
// The bilateral weight is
// A) 1.0 - EdgeSharpness * abs(sampleDepth - depth) / (depth * CameraFar), or
// B) 1.0 - EdgeSharpness * abs(sampleDepth - depth)
// Method A reduces the sharpness (= depth sensitivity) with depth. When a
// shadow on a plane is blurred, we can use a high sharpness to produce
// anisotropic looking blur near the camera and still enough blur in the
// distance.
// Method B is good to avoid halos but less blur in the distance.
// --> Method A is good for isotropic blur because the results look more
// anisotropic and there is hardly a difference in halos. Method B is good for
// anisotropic blur because the blur is already super smooth and we primarily
// want to avoid halos.

//
// Optimizations:
// - Computation of texCoord+Offset could be moved to the vertex shader. This
//   was tested but did not result in any visible performance gain. Sometimes it
//   resulted in a tiny slowdown?!
// - Use a fixed number of samples and unroll loops.
// - Bilateral: When Offsets contains (0, 0) the depth of the center pixel is
//   sampled twice!
// - Anisotropic/bilateral: When applied to a scene, it might be wise to
//   early-out on skybox pixels (e.g. depth > 0.999).
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The max number of samples.
static const int MaxNumberOfSamples = 23;

// The sample offsets.
float2 Offsets[MaxNumberOfSamples];

// The sample weights.
float Weights[MaxNumberOfSamples];

// The viewport size in pixels.
float2 ViewportSize : VIEWPORTSIZE;

texture SourceTexture;
sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
};

// For bilateral and anisotropic and filtering.
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);

float4 BlurParameters0;
#define CameraFar BlurParameters0.x
#define AspectRatio BlurParameters0.y
#define EdgeSharpness BlurParameters0.z      // = 1.0 / EdgeSoftness * CameraFar
#define DepthScaling BlurParameters0.w


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


/// Computes the weighted average
///   weight0 * e^sample0 + weight1 * e^sample1 = e^x
/// and returns only log(e^x) = x.
/// \param[in]  weight0  The weight of the first sample.
/// \param[in]  sample0  The exponent of the first sample.
/// \param[in]  weight1  The weight of the second sample.
/// \param[in]  sample1  The exponent of the second sample.
/// \return The exponent of the computed average, see description above.
float4 LogCombine(float4 weight0, float4 sample0, float4 weight1, float4 sample1)
{
  // We can reformulate this as follows:
  //   e^x = weight0 * e^sample0 + weight1 * e^sample1
  //   e^x = e^sample0 * (weight0 + weight1 * e^sample1 / e^sample0)
  //   e^x = e^sample0 * (weight0 + weight1 * e^(sample1 - sample0))
  //   x = log(e^sample0 * (weight0 + weight1 * e^(sample1 - sample0)))
  //   x = log(e^sample0) + log(weight0 + weight1 * e^(sample1 - sample0))
  //   x = sample0 + log(weight0 + weight1 * e^(sample1 - sample0))
  return sample0 + log(weight0 + weight1 * exp(sample1 - sample0));
}


/// Applies a convolution filter kernel in linear-space.
/// \param[in]  texCoord The texture coordintes.
/// \return The filtered pixel color.
float4 PSFilter(float2 texCoord : TEXCOORD0, uniform bool isBilateral, uniform int numberOfSamples) : COLOR0
{
  if (isBilateral)
  {
    // Normalized depth in range [0, 1].
    float depth = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0)));
    
    float fullDepthScale = 1 / (depth * CameraFar);
    float depthScale = lerp(1, fullDepthScale, DepthScaling);
    
    float4 color = 0;
    float totalWeight = 0;
    [unroll]
    for(int i = 0; i < numberOfSamples; i++)
    {
      float4 sampleTexCoord = float4(texCoord + Offsets[i] * depthScale, 0, 0);
      float sampleWeight = Weights[i];
      float sampleDepth = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, sampleTexCoord));
      float bilateralWeight = max(0, 1.0 - EdgeSharpness * abs(sampleDepth - depth) * fullDepthScale);
      sampleWeight *= bilateralWeight;
      totalWeight += sampleWeight;
      
      color += tex2Dlod(SourceSampler, sampleTexCoord) * sampleWeight;
    }
    
    color /= totalWeight;
    return color;
  }
  else
  {
    float4 color = 0.0f;
    [unroll]
    for(int i = 0; i < numberOfSamples; i++)
      color += tex2Dlod(SourceSampler, float4(texCoord + Offsets[i], 0, 0)) * Weights[i];
    
    return color;
  }
}


/// Applies a convolution filter kernel in log-space where the samples are combined
/// using LogCombine().
/// \param[in]  texCoord The texture coordintes.
/// \return The filtered pixel color.
float4 PSLogFilter(float2 texCoord : TEXCOORD0, uniform int numberOfSamples) : COLOR0
{
  // We need at least 2 samples for the first LogCombine call! --> NumberOfSamples >= 2
  float4 sample0 = tex2D(SourceSampler, texCoord + Offsets[0]);
  float4 sample1 = tex2D(SourceSampler, texCoord + Offsets[1]);
  float4 color = LogCombine(Weights[0], sample0, Weights[1], sample1);
  [unroll]
  for(int i = 2; i < numberOfSamples; i++)
  {
    float4 sample = tex2Dlod(SourceSampler, float4(texCoord + Offsets[i], 0, 0));  // Specify mipmap level manually. Otherwise compiler prints a warning and wants to unroll loop...
    color = LogCombine(1, color, Weights[i], sample);
  }
  
  return color;
}


VSFrustumRayOutput VSAnisotropicFilter(VSFrustumRayInput input)
{
  return VSFrustumRay(input, ViewportSize, FrustumCorners);
}


void GetAnisotropy(float3 positionView, float3 normalView, out float2 axisMajor, out float2 axisMinor, out float radiusMajor, out float radiusMinor)
{
  // Original (Reference: Zheng & Saito, "Screen Space Anisotropic Blurred Soft Shadows")
  //float3 normalScreen = float3(0, 0, 1);  // The normal vector of the screen.
  //float3 axisMinor = normalize(normalView.x, normalView.y, 0);
  //float3 axisMajor = cross(axisMinor, normalView);
  //float radiusMinor = dot(normalView, normalScreen);
  //float radiusMajor = 1;
  
  // Issues not mentioned in the paper:
  // - normalView = (0, 0, z) needs to be handled explicitly.
  // - axisMajor = cross(axisMinor, normalView) does not work for normalView = (x, y, 0).
  //   Use cross(axisMinor, normalScreen) instead!
  // - radiusMinor = dot(normalView, normalScreen) is only correct at the center of
  //   the perspective projection. Use positionView instead!
  
  // Optimized version, handling edge cases:
  float normalLength = length(normalView.xy);
  if (normalLength > 0.0001)
  {
    // Workaround for FX compiler bug:
    // abs() is necessary in XNA, otherwise axisMinor becomes 0.
    axisMinor = abs(normalView.xy) / normalLength;
  }
  else
  {
    axisMinor = float2(0, 1);
  }
  
  axisMajor.x = axisMinor.y;
  axisMajor.y = -axisMinor.x;
  
  // Fast version (only correct at center):
  //radiusMinor = normalView.z;
  
  // Slow version (not accurate, but better approximation):
  radiusMinor = dot(normalView, -normalize(positionView));
  radiusMajor = 1;
}


/// Performs convolution filtering in linear-space using an anisotropic filter kernel.
float4 PSAnisotropicFilter(float2 texCoord : TEXCOORD0,
                           float3 frustumRay : TEXCOORD1,
                           uniform const bool isMajor,
                           uniform bool isBilateral,
                           uniform int numberOfSamples) : COLOR0
{
  // Normalized depth in range [0, 1].
  float gBufferDepth = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0)));
  
  // Reconstruct position and normal in view space.
  float3 positionView = frustumRay * gBufferDepth;
  float3 normalView = DeriveNormal(positionView);
  
  // Depth in range [0, Far].
  float depth = CameraFar * gBufferDepth;
  
  // The coefficients of the ellipse.
  float2 axisMajor, axisMinor;
  float radiusMajor, radiusMinor;
  GetAnisotropy(positionView, normalView, axisMajor, axisMinor, radiusMajor, radiusMinor);
  // Debug output:
  //return float4(abs(axisMajor.x), abs(axisMajor.y), 0, 1);
  //return float4(abs(axisMinor.x), abs(axisMinor.y), 0, 1);
  //return float4(length(axisMajor), length(axisMinor), 0, 1);
  //return float4(0, radiusMinor, 0, 1);
  
  // Correct aspect ratio.
  axisMajor.y *= AspectRatio;
  axisMinor.y *= AspectRatio;
  
  // Scale filter kernel by depth.
  float depthScale = lerp(1, 1 / depth, DepthScaling);
  radiusMajor *= depthScale;
  radiusMinor *= depthScale;
  
  float2 axis = isMajor ? axisMajor * radiusMajor : axisMinor * radiusMinor;
  float4 color = 0;
  float totalWeight = 0;
  [unroll]
  for(int i = 0; i < numberOfSamples; i++)
  {
    float4 sampleTexCoord = float4(texCoord + axis * Offsets[i].x, 0, 0);
    float sampleWeight = Weights[i];
    
    if (isBilateral)
    {
      float sampleDepth = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, sampleTexCoord));
      float bilateralWeight = max(0, 1.0 - EdgeSharpness * abs(sampleDepth - gBufferDepth));
      sampleWeight *= bilateralWeight;
      totalWeight += sampleWeight;
    }
    
    color += tex2Dlod(SourceSampler, sampleTexCoord) * sampleWeight;
  }
  
  if (isBilateral)
    color /= totalWeight;
  
  return color;
}

float4 PSNormal3(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, false, 3); }
float4 PSNormal5(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, false, 5); }
float4 PSNormal7(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, false, 7); }
float4 PSNormal9(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, false, 9); }
float4 PSNormal15(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, false, 15); }
float4 PSNormal23(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, false, 23); }

float4 PSBilateral3(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, true, 3); }
float4 PSBilateral5(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, true, 5); }
float4 PSBilateral7(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, true, 7); }
float4 PSBilateral9(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, true, 9); }
float4 PSBilateral15(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, true, 15); }
float4 PSBilateral23(float2 texCoord : TEXCOORD0) : COLOR0 { return PSFilter(texCoord, true, 23); }

float4 PSLogFilter3(float2 texCoord : TEXCOORD0) : COLOR0 { return PSLogFilter(texCoord, 3); }
float4 PSLogFilter5(float2 texCoord : TEXCOORD0) : COLOR0 { return PSLogFilter(texCoord, 5); }
float4 PSLogFilter7(float2 texCoord : TEXCOORD0) : COLOR0 { return PSLogFilter(texCoord, 7); }
float4 PSLogFilter9(float2 texCoord : TEXCOORD0) : COLOR0 { return PSLogFilter(texCoord, 9); }
float4 PSLogFilter15(float2 texCoord : TEXCOORD0) : COLOR0 { return PSLogFilter(texCoord, 15); }
float4 PSLogFilter23(float2 texCoord : TEXCOORD0) : COLOR0 { return PSLogFilter(texCoord, 23); }

float4 PSAnisotropic_MajorAxis3(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, false, 3); }
float4 PSAnisotropic_MajorAxis5(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, false, 5); }
float4 PSAnisotropic_MajorAxis7(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, false, 7); }
float4 PSAnisotropic_MajorAxis9(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, false, 9); }
float4 PSAnisotropic_MajorAxis15(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, false, 15); }
float4 PSAnisotropic_MajorAxis23(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, false, 23); }

float4 PSAnisotropic_MinorAxis3(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, false, 3); }
float4 PSAnisotropic_MinorAxis5(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, false, 5); }
float4 PSAnisotropic_MinorAxis7(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, false, 7); }
float4 PSAnisotropic_MinorAxis9(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, false, 9); }
float4 PSAnisotropic_MinorAxis15(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, false, 15); }
float4 PSAnisotropic_MinorAxis23(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, false, 23); }

float4 PSAnisotropicBilateral_MajorAxis3(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, true, 3); }
float4 PSAnisotropicBilateral_MajorAxis5(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, true, 5); }
float4 PSAnisotropicBilateral_MajorAxis7(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, true, 7); }
float4 PSAnisotropicBilateral_MajorAxis9(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, true, 9); }
float4 PSAnisotropicBilateral_MajorAxis15(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, true, 15); }
float4 PSAnisotropicBilateral_MajorAxis23(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, true, true, 23); }

float4 PSAnisotropicBilateral_MinorAxis3(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, true, 3); }
float4 PSAnisotropicBilateral_MinorAxis5(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, true, 5); }
float4 PSAnisotropicBilateral_MinorAxis7(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, true, 7); }
float4 PSAnisotropicBilateral_MinorAxis9(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, true, 9); }
float4 PSAnisotropicBilateral_MinorAxis15(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, true, 15); }
float4 PSAnisotropicBilateral_MinorAxis23(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0 { return PSAnisotropicFilter(texCoord, frustumRay, false, true, 23); }


//--------------------------------------------------------
// Techniques
//--------------------------------------------------------

#if !SM4
#define VSTARGET vs_3_0
#define PSTARGET ps_3_0
#else
#define VSTARGET vs_4_0
#define PSTARGET ps_4_0
#endif


#define TECHNIQUE(techniqueName, vsName, psName) \
technique techniqueName { pass { VertexShader = compile VSTARGET vsName(); PixelShader = compile PSTARGET psName(); } }

#define TECHNIQUE2(techniqueName, passName0, vsName0, psName0, passName1, vsName1, psName1) \
technique techniqueName \
{ \
  pass passName0 { VertexShader = compile VSTARGET vsName0(); PixelShader = compile PSTARGET psName0(); } \
  pass passName1 { VertexShader = compile VSTARGET vsName1(); PixelShader = compile PSTARGET psName1(); } \
}

TECHNIQUE(Normal3, VS, PSNormal3)
TECHNIQUE(Normal5, VS, PSNormal5)
TECHNIQUE(Normal7, VS, PSNormal7)
TECHNIQUE(Normal9, VS, PSNormal9)
TECHNIQUE(Normal15, VS, PSNormal15)
TECHNIQUE(Normal23, VS, PSNormal23)
  
TECHNIQUE(Bilateral3, VS, PSBilateral3)
TECHNIQUE(Bilateral5, VS, PSBilateral5)
TECHNIQUE(Bilateral7, VS, PSBilateral7)
TECHNIQUE(Bilateral9, VS, PSBilateral9)
TECHNIQUE(Bilateral15, VS, PSBilateral15)
TECHNIQUE(Bilateral23, VS, PSBilateral23)
  
// Filtering in log-space (for example, for Exponential Shadow Maps)
TECHNIQUE(Logarithmic3, VS, PSLogFilter3)
TECHNIQUE(Logarithmic5, VS, PSLogFilter5)
TECHNIQUE(Logarithmic7, VS, PSLogFilter7)
TECHNIQUE(Logarithmic9, VS, PSLogFilter9)
TECHNIQUE(Logarithmic15, VS, PSLogFilter15)
TECHNIQUE(Logarithmic23, VS, PSLogFilter23)
  
TECHNIQUE2(Anisotropic3, MajorAxis, VSAnisotropicFilter, PSAnisotropic_MajorAxis3, MinorAxis, VSAnisotropicFilter, PSAnisotropic_MinorAxis3)
TECHNIQUE2(Anisotropic5, MajorAxis, VSAnisotropicFilter, PSAnisotropic_MajorAxis5, MinorAxis, VSAnisotropicFilter, PSAnisotropic_MinorAxis5)
TECHNIQUE2(Anisotropic7, MajorAxis, VSAnisotropicFilter, PSAnisotropic_MajorAxis7, MinorAxis, VSAnisotropicFilter, PSAnisotropic_MinorAxis7)
TECHNIQUE2(Anisotropic9, MajorAxis, VSAnisotropicFilter, PSAnisotropic_MajorAxis9, MinorAxis, VSAnisotropicFilter, PSAnisotropic_MinorAxis9)
TECHNIQUE2(Anisotropic15, MajorAxis, VSAnisotropicFilter, PSAnisotropic_MajorAxis15, MinorAxis, VSAnisotropicFilter, PSAnisotropic_MinorAxis15)
TECHNIQUE2(Anisotropic23, MajorAxis, VSAnisotropicFilter, PSAnisotropic_MajorAxis23, MinorAxis, VSAnisotropicFilter, PSAnisotropic_MinorAxis23)
  
TECHNIQUE2(AnisotropicBilateral3, MajorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MajorAxis3, MinorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MinorAxis3)
TECHNIQUE2(AnisotropicBilateral5, MajorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MajorAxis5, MinorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MinorAxis5)
TECHNIQUE2(AnisotropicBilateral7, MajorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MajorAxis7, MinorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MinorAxis7)
TECHNIQUE2(AnisotropicBilateral9, MajorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MajorAxis9, MinorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MinorAxis9)
TECHNIQUE2(AnisotropicBilateral15, MajorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MajorAxis15, MinorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MinorAxis15)
TECHNIQUE2(AnisotropicBilateral23, MajorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MajorAxis23, MinorAxis, VSAnisotropicFilter, PSAnisotropicBilateral_MinorAxis23)
