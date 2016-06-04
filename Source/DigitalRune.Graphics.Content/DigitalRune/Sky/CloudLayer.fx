//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file CloudLayer.fx
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Macros
//-----------------------------------------------------------------------------

#define DECLARE_UNIFORM_NOISETEXTURE(index) \
texture NoiseTexture##index; \
sampler NoiseSampler##index = sampler_state \
{ \
  Texture = <NoiseTexture##index>; \
  AddressU  = WRAP; \
  AddressV  = WRAP; \
  MinFilter = LINEAR; \
  MagFilter = LINEAR; \
  MipFilter = LINEAR; \
}


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// Type of cloud texture.
static const int TextureRgb = 1;     // RGB texture.
static const int TextureAlpha = 2;   // Alpha-only texture.


// ----- General Constants
// The viewport size in pixels.
float2 ViewportSize;

// ----- Constants for Lerp pass and Density pass
// Packed texture parameters (scale x/y, offset x/y) for the first two textures.
float4 Texture0Parameters = float4(1, 1, 0, 0);
float4 Texture1Parameters = float4(1, 1, 0, 0);
float LerpParameter;

float2 Density0, Density1, Density2, Density3, Density4, Density5, Density6, Density7;
float3x3 Matrix0, Matrix1, Matrix2, Matrix3, Matrix4, Matrix5, Matrix6, Matrix7;

float Coverage = 0.6;   // Higher values = more clouds.
float Density = 20;

// ----- Constants for Cloud pass
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float3 SunDirection;        // The direction to the sun in world space.
float SkyCurvature = 0.9;
// TextureMatrix is stored in Matrix0.
int NumberOfSamples = 16;
float SampleDistance = 0.005;
float3 ScatterParameters = float3(5, 1, 0.5); // x = scatter exponent, y = scatter scale, z = scatter offset
float2 HorizonFade = 0.05f; // x = horizon fade, y = horizon bias.
float3 SunLight;
float4 AmbientLight;        // xyz = AmbientLight, w = global Alpha.

// ----- Textures
DECLARE_UNIFORM_NOISETEXTURE(0);
DECLARE_UNIFORM_NOISETEXTURE(1);
DECLARE_UNIFORM_NOISETEXTURE(2);
DECLARE_UNIFORM_NOISETEXTURE(3);
DECLARE_UNIFORM_NOISETEXTURE(4);
DECLARE_UNIFORM_NOISETEXTURE(5);
DECLARE_UNIFORM_NOISETEXTURE(6);
DECLARE_UNIFORM_NOISETEXTURE(7);


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

struct VSOutputLerp
{
  float2 TexCoord0 : TEXCOORD0;
  float2 TexCoord1 : TEXCOORD1;
  float4 Position : SV_Position;
};


struct VSInputCloud
{
  float4 Position : POSITION0;
};

struct VSOutputCloud
{
  float3 PositionWorld : TEXCOORD0;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Shaders
//-----------------------------------------------------------------------------

VSOutputLerp VSLerp(VSInput input)
{
  VSOutputLerp output = (VSOutputLerp)0;
  
  // Compute TexCoords for packed textures.
  output.TexCoord0 = input.TexCoord * Texture0Parameters.xy + Texture0Parameters.zw;
  output.TexCoord1 = input.TexCoord * Texture1Parameters.xy + Texture1Parameters.zw;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  
  return output;
}

float4 PSLerp(float2 texCoord0 : TEXCOORD0, float2 texCoord1 : TEXCOORD1) : COLOR0
{
  return lerp(tex2D(NoiseSampler0, texCoord0).a, tex2D(NoiseSampler1, texCoord1).a, LerpParameter).rrrr;
}


VSOutput VSDensity(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}

float4 PSDensity(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 n = 0;
  n += Density0.x * (tex2D(NoiseSampler0, mul(float3(texCoord, 1), Matrix0).xy) + Density0.y);
  n += Density1.x * (tex2D(NoiseSampler1, mul(float3(texCoord, 1), Matrix1).xy) + Density1.y);
  n += Density2.x * (tex2D(NoiseSampler2, mul(float3(texCoord, 1), Matrix2).xy) + Density2.y);
  n += Density3.x * (tex2D(NoiseSampler3, mul(float3(texCoord, 1), Matrix3).xy) + Density3.y);
  n += Density4.x * (tex2D(NoiseSampler4, mul(float3(texCoord, 1), Matrix4).xy) + Density4.y);
  n += Density5.x * (tex2D(NoiseSampler5, mul(float3(texCoord, 1), Matrix5).xy) + Density5.y);
  n += Density6.x * (tex2D(NoiseSampler6, mul(float3(texCoord, 1), Matrix6).xy) + Density6.y);
  n += Density7.x * (tex2D(NoiseSampler7, mul(float3(texCoord, 1), Matrix7).xy) + Density7.y);
  
  // Per default, we assume that the textures added up to a value in the range [-0.5, +0.5].
  n += 0.5;
  
  // Compute cloud cover parameter c.
  float4 density = max(0, n - (1 - Coverage));
  
  // Compute fog factor using exponential fog.
  // exp(-density * c) = exp2(-density * c / ln(2)) = exponential fog
  float4 transmittance = exp2(-density * Density);
  
  return transmittance;
}


VSOutputCloud VSCloud(VSInputCloud input)
{
  VSOutputCloud output = (VSOutputCloud)0;
  output.PositionWorld = input.Position.xyz;
  output.Position = mul(input.Position, mul(View, Projection)).xyww;  // Set z to w to move vertex to far clip plane.
  return output;
}

// Gets cloud texture coordinate for a given direction in world space.
// direction must be normalized.
float2 GetTextureCoordinate(float3 direction)
{
  float x = direction.x;
  float y = direction.y + HorizonFade.y;
  float z = direction.z;
  
  // We have to map the direction vector to texture coords.
  // fPlane(x) = x / y creates texture coordinates for a plane (= a lot of foreshortening).
  // fSphere(x) = x / (2 + 2 * y) creates texture coordinates for a paraboloid mapping (= almost no foreshortening).
  // fPlane(x) = x / (4 * y) is similar to fSphere(x) = x / (2 + 2 * y) for y near 1.
  float2 texCoord = lerp(float2(x / (4 * y), z / (4 * y)), float2(x / (2 + 2 * y), z / (2 + 2 * y)), SkyCurvature);
  float3 texCoord3 = float3(texCoord.x, texCoord.y, 1);
  return mul(texCoord3, Matrix0).xy + 0.5;  // Add 0.5 to get coordinates (0.5, 0.5) at zenith.
}

// Gets transmittance from the cloud textures for a given direction in world space.
// direction must be normalized.
float GetTransmittance(float3 direction, int textureType)
{
  float2 texCoord = GetTextureCoordinate(direction);
  
  if (textureType == TextureRgb)
    return tex2D(NoiseSampler0, texCoord).r;
  else
    return tex2D(NoiseSampler0, texCoord).a;
}

float4 PSCloud(float3 positionWorld : TEXCOORD0, int textureType, bool correctGamma) : COLOR0
{
  float3 viewDirection = normalize(positionWorld);
  
  // GetTransmittance gets a cloud texture sample in the given world space direction.
  float transmittance = GetTransmittance(viewDirection, textureType);
  
  // Abort early for near transparent pixels.
  clip(0.9999 - transmittance);
  
  // Abort early below horizon.
  clip(viewDirection.y + HorizonFade.y);
  
  // To blur the clouds we need to compute an offset vector that points to the sun.
  // The offset vector should be zero when we look straight at the sun.
  float3 offset = SunDirection;
  
  float blur = transmittance;
  //float decay = 1;
  float totalWeight = 1;
  float currentWeight = 1;
  float3 direction = viewDirection;
  for (int i = 0; i < NumberOfSamples; i++)
  {
    direction += offset * SampleDistance;
    //currentWeight *= decay;
    totalWeight += currentWeight;
    blur += GetTransmittance(normalize(direction), textureType) * currentWeight;
  }
  blur /= totalWeight;
  
  // ----- Cheap forward scattering using Phong distribution:
  // Higher transmittance = less density = more forward scattering.
  float exponent = ScatterParameters.x * transmittance;
  // Note: We add an epsilon because pow cannot compute pow(0, exponent)!
  float forwardScatter = pow(0.000001f + saturate(dot(viewDirection, SunDirection)), exponent);
  
  // Lower transmittance = higher density = lower intensity because light is absorbed.
  forwardScatter *= transmittance * ScatterParameters.y + ScatterParameters.z;
  
  // Term to fade out the forward scattering to 90°.
  forwardScatter *= pow(0.000001f + saturate(dot(viewDirection, SunDirection)), ScatterParameters.x);
  
  float3 color = AmbientLight.rgb + SunLight * (blur + forwardScatter);
  
  float alpha = saturate(1 - transmittance);
  
  // Fade out towards horizon.
  float up = viewDirection.y + HorizonFade.y;
  float fade = saturate((up - HorizonFade.x) / HorizonFade.x);
  //alpha = lerp(0 , alpha, fade);          // Lerp
  alpha *=  fade * fade * (3 - 2 * fade);   // Smoothstep
  
  // Global user-defined alpha.
  alpha *= AmbientLight.a;
  
  if (correctGamma)
    color = ToGamma(color);
  
  // Premultiplied alpha.
  color *= alpha;
  
  return float4(color, alpha);
}
float4 PSCloudRgbLinear(float3 positionWorld : TEXCOORD0) : COLOR0 { return PSCloud(positionWorld, TextureRgb, false); }
float4 PSCloudAlphaLinear(float3 positionWorld : TEXCOORD0) : COLOR0 { return PSCloud(positionWorld, TextureAlpha, false); }
float4 PSCloudRgbGamma(float3 positionWorld : TEXCOORD0) : COLOR0 { return PSCloud(positionWorld, TextureRgb, true); }
float4 PSCloudAlphaGamma(float3 positionWorld : TEXCOORD0) : COLOR0 { return PSCloud(positionWorld, TextureAlpha, true); }


float4 PSOcclusion(float3 positionWorld : TEXCOORD0, int textureType) : COLOR0
{
  float3 viewDirection = normalize(positionWorld);
  
  // GetTransmittance() gets a cloud texture sample in the given world space direction.
  float transmittance = GetTransmittance(viewDirection, textureType);
  
  float alpha = saturate(1 - transmittance);
  
  // Fade out towards horizon.
  float up = viewDirection.y + HorizonFade.y;
  float fade = saturate((up - HorizonFade.x) / HorizonFade.x);
  //alpha = lerp(0 , alpha, fade);         // Lerp
  alpha *= fade * fade * (3 - 2 * fade);   // Smoothstep
  
  // Global user-defined alpha.
  alpha *= AmbientLight.a;
  
  // Clip pixels below a certain opacity.
  clip(0.5 - alpha);
  
  return float4(1, 1, 1, 1);
}
float4 PSOcclusionRgb(float3 positionWorld : TEXCOORD0) : COLOR0 { return PSOcclusion(positionWorld, TextureRgb); }
float4 PSOcclusionAlpha(float3 positionWorld : TEXCOORD0) : COLOR0 { return PSOcclusion(positionWorld, TextureAlpha); }


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
  pass Lerp
  {
    VertexShader = compile VSTARGET VSLerp();
    PixelShader = compile PSTARGET PSLerp();
  }
  pass Density
  {
    VertexShader = compile VSTARGET VSDensity();
    PixelShader = compile PSTARGET PSDensity();
  }
  pass CloudRgbLinear
  {
    VertexShader = compile VSTARGET VSCloud();
    PixelShader = compile PSTARGET PSCloudRgbLinear();
  }
  pass CloudAlphaLinear
  {
    VertexShader = compile VSTARGET VSCloud();
    PixelShader = compile PSTARGET PSCloudAlphaLinear();
  }
  pass ClouRgbGamma
  {
    VertexShader = compile VSTARGET VSCloud();
    PixelShader = compile PSTARGET PSCloudRgbGamma();
  }
  pass CloudAlphaGamma
  {
    VertexShader = compile VSTARGET VSCloud();
    PixelShader = compile PSTARGET PSCloudAlphaGamma();
  }
  pass OcclusionRgb
  {
    VertexShader = compile VSTARGET VSCloud();
    PixelShader = compile PSTARGET PSOcclusionRgb();
  }
  pass OcclusionAlpha
  {
    VertexShader = compile VSTARGET VSCloud();
    PixelShader = compile PSTARGET PSOcclusionAlpha();
  }
}
