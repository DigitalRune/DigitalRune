//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ObjectMotionblur.fx
/// Applies a motion blur effect using velocity buffers.
//
// See DirectX 9 SDK Sample "PixelMotionBlur" or
// http://mynameismjp.wordpress.com/samples-tutorials-tools/motion-blur-sample/
// For soft-edge motion blur, see the paper
//   "A Reconstruction Filter for Plausible Motion Blur".
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

// The number of samples taken to blur a pixel.
int NumberOfSamples = 5;

// Size of the source texture for downsampling.
float2 SourceSize;

// SoftZExtent for SoftZCompare(). Relative to the normalized linear depth [0, 1].
float SoftZExtent;

// The max motion blur radius in texture coordinates (not pixels).
float2 MaxBlurRadius;

// The input texture.
texture SourceTexture;
sampler SourceSampler : register(s0) = sampler_state
{
  Texture = <SourceTexture>;
};

// A texture with movement vectors.
texture VelocityTexture;
sampler VelocitySampler : register(s1) = sampler_state
{
  Texture = <VelocityTexture>;
};

// A second texture with movement vectors.
texture VelocityTexture2;
sampler VelocitySampler2 : register(s2) = sampler_state
{
  Texture = <VelocityTexture2>;
};

// The normalized linear planar depth (range [0, 1]).
texture GBuffer0;
sampler GBuffer0Sampler = sampler_state
{
  Texture = <GBuffer0>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

// A quadratic texture that contains random values.
texture JitterTexture;
sampler JitterSampler = sampler_state
{
  Texture = <JitterTexture>;
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


// Velocity motion blur with one velocity buffer.
float4 PSSingle(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get velocity of current frame.
  float2 velocity = tex2D(VelocitySampler, texCoord).xy;
  
  // Clamp max velocity. The max without artifacts is 1.4 * NumberOfSamples.
  // A bit larger is still ok.
  float2 maxVelocity = 2 * NumberOfSamples / ViewportSize;
  velocity = clamp(velocity, -maxVelocity, maxVelocity);
  
  // Sample along the motion vector.
  float4 result = 0;
  for (int i = 0; i < NumberOfSamples; i++)
    result += tex2D(SourceSampler, texCoord + velocity * i / (float)NumberOfSamples);
  
  result /= (float)NumberOfSamples;
  
  return result;
}


// Velocity motion blur with two velocity buffers.
float4 PSDual(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get velocity of current and last frame.
  float2 currentVelocity = tex2D(VelocitySampler, texCoord).xy;
  float2 lastVelocity = tex2D(VelocitySampler2, texCoord).xy;
  
  // Compare velocities and use larger one.
  float2 velocity = 0;
  if (dot(currentVelocity, currentVelocity) > dot(lastVelocity, lastVelocity))
    velocity = currentVelocity;
  else
    velocity = lastVelocity;
  
  // Clamp max velocity. The max without artifacts is 1.4 * NumberOfSamples.
  // A bit larger is still ok.
  float2 maxVelocity = 2 * NumberOfSamples / ViewportSize;
  velocity = clamp(velocity, -maxVelocity, maxVelocity);
  
  // Sample along the motion vector
  float4 result = 0;
  for (int i = 0; i < NumberOfSamples; i++)
    result += tex2D(SourceSampler, texCoord + velocity * i / (float)NumberOfSamples);
  
  result /= (float)NumberOfSamples;
  
  return result;
}


// Downsample a floating point velocity buffer. We store the max velocity in the
// downsampled buffer. The downsampled buffer is an 8-bit RGBA buffer, therefore
// we have to scale and bias the velocity to put it in the range [0, 1].
float4 PSDownsampleMaxFromFloatBuffer(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Get 2x2 samples with 0.5 texel offset.
  float2 o = 0.5 / SourceSize;
  
  float3 s[4];
  s[0].xy = tex2D(VelocitySampler, texCoord + float2(-o.x, -o.y)).xy;
  s[1].xy = tex2D(VelocitySampler, texCoord + float2( o.x, -o.y)).xy;
  s[2].xy = tex2D(VelocitySampler, texCoord + float2(-o.x,  o.y)).xy;
  s[3].xy = tex2D(VelocitySampler, texCoord + float2( o.x,  o.y)).xy;
  
  // Store length squared in z component.
  for (int i = 0; i < 4; i++)
    s[i].z = dot(s[i].xy, s[i].xy);
  
  // Find largest velocity.
  float3 max = s[0];
  for (i = 1; i < 4; i++)
    if (s[i].z > max.z)
      max = s[i];
  
  // Divide by 2 to get half-spread velocity. Divide by MaxBlurRadius to put
  // velocity into the range [-1, 1]. (Values beyond -1 and +1 will be clamped
  // because we write to an RGBA buffer.)
  max.xy = max.xy / MaxBlurRadius / 2;
  
  // Scale and bias to move values into the range [0, 1].
  return float4(max.xy / 2 + 0.5f, 0, 1);
}


// Same as PSDownsampleMaxFromFloatBuffer, but we downsample from 8-bit RGBA to
// 8-bit RGBA.
float4 PSDownsampleMax(float2 texCoord : TEXCOORD0) : COLOR0
{
  float2 o = 0.5 / SourceSize;
  
  float3 s[4];
  s[0].xy = tex2D(VelocitySampler, texCoord + float2(-o.x, -o.y)).xy * 2 - 1;
  s[1].xy = tex2D(VelocitySampler, texCoord + float2( o.x, -o.y)).xy * 2 - 1;
  s[2].xy = tex2D(VelocitySampler, texCoord + float2(-o.x,  o.y)).xy * 2 - 1;
  s[3].xy = tex2D(VelocitySampler, texCoord + float2( o.x,  o.y)).xy * 2 - 1;
  
  for (int i = 0; i < 4; i++)
    s[i].z = dot(s[i].xy, s[i].xy);
  
  float3 max = s[0];
  for (i = 1; i < 4; i++)
  {
    if (s[i].z > max.z)
      max = s[i];
  }
  
  return float4(max.xy / 2 + 0.5f, 0, 1);
}


// Each pixel samples its 8 neighbors and stores the max velocity.
float4 PSNeighborMax(float2 texCoord : TEXCOORD0) : COLOR0
{
  float2 o = 1 / ViewportSize;
  
  float3 max = float3(0, 0, 0);
  for (int x = -1; x <= 1; x++)
  {
    for (int y = -1; y <= 1; y++)
    {
      float3 s;
      s.xy = tex2D(VelocitySampler, texCoord + float2(x * o.x, y * o.y)).xy * 2 - 1;
      s.z = dot(s.xy, s.xy);
      if (s.z > max.z)
        max = s;
    }
  }
  
  return float4(max.xy / 2 + 0.5f, 0, 1);
}


/// Determines whether x is within y's point-spread function.
/// Assuming that y has an influence range r where the influence has a linear
/// falloff. The influence is 1 at y and 0 at a distance of r. The function
/// returns the influence that y has on x.
/// \param[in]  x  The first position.
/// \param[in]  y  The second position.
/// \param[in]  r  The influence range of y.
/// \return 1 means maximal influence (the distance between x and y is 0).
///         0 means no influence (the distance between x and y is greater than
///         r). A value between 1 to 0 is returned if the distance between x
///         and y is less than r.
float Cone(float2 x, float2 y, float r)
{
  return saturate(1 - length(x - y) / r);
}


/// Determines whether two positions are within a certain range.
/// The returned value is continuous instead of a discrete value for robustness
/// to limited precision.
/// \param[in]  x  The first position.
/// \param[in]  y  The second position.
/// \param[in]  r  The range.
/// \return 1 ... The distance between the positions is less than r.
///         0 ... The distance between the positions is greater than r.
///         A smooth transition from 1 to 0 is created if the distance is close
///         to r.
float Cylinder(float2 x, float2 y, float r)
{
  return 1.0f - smoothstep(0.95f * r, 1.05f * r, length(x - y));
}


/// Determines whether the depth value zA is closer than zB using a soft
/// tolerance. (A larger z-value means that the pixel is more distant.)
/// Arguments:
/// \param[in]  zA  The first depth value in the range [0, 1].
/// \param[in]  zB  The second depth value in the range [0, 1].
/// \return 1 ... zA is closer than zB.
///         0 ... zB is closer than zA.
///         A value between 0 and 1 means that zB is closer, but within a
///         tolerance.
float SoftDepthCompare(float zA, float zB)
{
  return saturate(1 - (zA - zB) / SoftZExtent);
}


// Velocity motion blur with one velocity buffer.
float4 PSSoftEdge(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Largest velocity in the neighborhood (half-spread velocity).
  float2 vn = (tex2D(VelocitySampler2, texCoord).rg * 2 - 1) * MaxBlurRadius;
  
  // If the half-spread velocity is less than a half pixel, we can abort early.
  if (length(vn) < (0.501f / ViewportSize.x))
    return tex2D(SourceSampler, texCoord);
  
  float2 velocity = tex2D(VelocitySampler, texCoord).xy;
  velocity /= 2;  // Convert to half-spread velocity.
  float velocityLength = length(velocity);
  
  float depth = GetGBufferDepth(tex2D(GBuffer0Sampler, texCoord));
  
  // The weight of the other samples is in the range [0,4[. Experiments have
  // show that 3 is a good choice. (The weight computation in the original
  // paper is not correct.)
  float weight = 3;
  float3 sum = weight * tex2D(SourceSampler, texCoord).rgb;
  
  // Get a random value in the range [-0.5f, 0.5f].
  float random = tex2D(JitterSampler, texCoord).a - 0.5f;
  
  // Sample along +/- vn.
  for (float i = 0; i < NumberOfSamples;  i++)
  {
    // Do not resample the center sample.
    // TODO: Does not work correctly with even number of samples!
    if ((int)i == (int)((NumberOfSamples - 1) / 2))
      continue;
    
    // Convert i into the range [-1, 1].
    float t = lerp(-1, 1, (i + random + 1.0f) / (NumberOfSamples + 1));
    
    float2 sampleCoord = texCoord + vn * t;
    float3 sampleColor = tex2Dlod(SourceSampler, float4(sampleCoord, 0, 0)).rgb;
    float sampleDepth = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(sampleCoord, 0, 0)));
    float2 sampleVelocity = tex2Dlod(VelocitySampler, float4(sampleCoord, 0, 0)).xy;
    sampleVelocity /= 2;  // Convert to half-spread velocity.
    float sampleVelocityLength = length(sampleVelocity);
    
    // Compare depths.
    float b = SoftDepthCompare(depth, sampleDepth);  // Sample is in background?
    float f = SoftDepthCompare(sampleDepth, depth);  // Sample is in foreground?
    
    // Compute a weight.
    float sampleWeight =
      // Case 1: Blurry sample is in front of current pixel.
      f * Cone(sampleCoord, texCoord, sampleVelocityLength)
      // Case 2: Sample is be behind current, blurry pixel.
      + b * Cone(texCoord, sampleCoord, velocityLength)
      // Case 3: Simultaneously blurry sample and pixel.
      + Cylinder(sampleCoord, texCoord, sampleVelocityLength) * Cylinder(texCoord, sampleCoord, velocityLength) * 2;
    
    weight += sampleWeight;
    sum += sampleColor * sampleWeight;
  }
  
  sum /= weight;
  return float4(sum, 1);
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
  pass Single
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSSingle();
  }
  
  pass Dual
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSDual();
  }
  
  pass DownsampleMaxFromFloatBuffer
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSDownsampleMaxFromFloatBuffer();
  }
  
  pass DownsampleMax
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSDownsampleMax();
  }
  
  pass NeighborMax
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSNeighborMax();
  }
  
  pass SoftEdge
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSSoftEdge();
  }
}
