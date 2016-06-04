//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Cloud.fx
/// Clouds with simple lighting and forward scattering.
//
//-----------------------------------------------------------------------------

#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"


//--------------------------------------------------------
// Constants
//--------------------------------------------------------

float4x4 World : WORLD;
float4x4 WorldViewProjection : WORLDVIEWPROJECTION;
float3 CameraPosition : CAMERAPOSITION;

// Following 3 parameters must be updated at runtime, see CloudQuadSample.cs.
// The direction to the sun in world space.
float3 SunDirection : SUNDIRECTION;
// The sun light.
float3 SunLight : SUNLIGHT;
// The ambient light caused by the sky.
float3 SkyLight : SKYLIGHT;

// Density scale factor.
float DensityScale = 1.0;

// The number of samples and offsets scale for cloud lighting.
#define NumberOfSamples 8
float SampleDistance = 0.02;

// Scatter exponent, scale and offset for forward scattering.
float3 ScatterParameters = float3(10, 5, 0.3);
#define ScatterExponent ScatterParameters.x
#define ScatterScale ScatterParameters.y
#define ScatterOffset ScatterParameters.z

// Alpha scale factor.
float Alpha = 1;

// The density texture that defines the cloud structure.
texture DensityTexture;
sampler DensitySampler = sampler_state
{
  Texture = <DensityTexture>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
};


//-----------------------------------------------------------------------------
// Input, Output
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
  float3 Normal : NORMAL;
};

struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float3 PositionWorld : TEXCOORD1;
  float3 NormalWorld : TEXCOORD2;
  float4 PositionProj : SV_Position;
};

struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float3 PositionWorld : TEXCOORD1;
  float3 NormalWorld : TEXCOORD2;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.TexCoord = input.TexCoord;
  output.PositionWorld = mul(input.Position, World).xyz;
  output.PositionProj  = mul(input.Position, WorldViewProjection);
  output.NormalWorld = mul(input.Normal, (float3x3)World);
  return output;
}

// Samples the density texture and returns transmittance.
// 'Transmittance' defines how much the light is attenuated. It is 1 if the
// pixel is transparent and 0 if the pixel is opaque.
float GetTransmittance(float2 texCoord)
{
  return saturate(1 - FromGamma(tex2D(DensitySampler, texCoord).r) * DensityScale);
}


float4 PS(PSInput input) : COLOR
{
  // Get transmittance from a cloud texture.
  float2 texCoord = input.TexCoord;
  float transmittance = GetTransmittance(texCoord);
  
  // Abort early for near transparent pixels.
  clip(0.9999 - transmittance);
  
  // The direction from the camera to the current pixel.
  float3 viewDirection = normalize(input.PositionWorld - CameraPosition);
    
  // To light the clouds compute an offset vector that points to the sun.
  // The offset vector should be zero when we look straight at the sun.
  float3 offsetWorld = normalize(SunDirection - viewDirection);
  
  // Compute a cotangent frame matrix which converts between world space and texture space.
  float3x3 cotangentFrame = CotangentFrame(
    normalize(input.NormalWorld),
    input.PositionWorld,
    input.TexCoord);
  
  // Convert the offset from world to texture space.
  float2 offsetTexture = mul(cotangentFrame, offsetWorld).xy;

  // Average the texture samples from the current pixel to the sun.
  // (Similar to a directional blur.)
  // If the resulting average is low, the pixel should be darker because
  // sunlight is absorbed by the cloud until it reaches this pixel.
  float blur = transmittance;
  //float decay = 1;
  float totalWeight = 1;
  float currentWeight = 1;
  for (int i = 0; i < NumberOfSamples; i++)
  {
    texCoord += offsetTexture * SampleDistance;
    //currentWeight *= decay;
    totalWeight += currentWeight;
    blur += GetTransmittance(texCoord) * currentWeight;
  }
  blur /= totalWeight;
  
  // Cheap forward scattering using Phong distribution:
  // Exponent should be high (= a lot of forward scattering) when transmittance is high (= density is low).
  float exponent = ScatterExponent.x * transmittance;
  // Note: We add an epsilon because pow cannot compute pow(0, exponent)!
  float forwardScatter = pow(0.000001f + saturate(dot(viewDirection, SunDirection)), exponent);
  
  // The forward scattering term should be lower if the transmittance is low (= high density)
  // because light is absorbed.
  forwardScatter *= transmittance * ScatterScale + ScatterOffset;
  
  // Fade out the forward scattering to 90°.
  forwardScatter *= pow(0.000001f + saturate(dot(viewDirection, SunDirection)), ScatterExponent);
  
  // Cloud lighting.
  float3 color = SkyLight + SunLight * (blur + forwardScatter);
  
  float alpha = saturate(1 - transmittance);
  
  // Global user-defined alpha.
  alpha *= Alpha;
  
  // Premultiplied alpha.
  color *= alpha;
  
  return float4(color, alpha);
}


#if !SM4
  #define VSTARGET vs_3_0
  #define PSTARGET ps_3_0
#else
  #define VSTARGET vs_4_0_level_9_3
  #define PSTARGET ps_4_0_level_9_3
#endif

technique
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
