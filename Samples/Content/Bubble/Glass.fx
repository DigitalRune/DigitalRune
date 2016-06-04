//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Glass.fx
/// A glass effect with environment map reflections and refraction.
//
//-----------------------------------------------------------------------------


//--------------------------------------------------------
// Constants
//--------------------------------------------------------

// The refraction indices and colors of 3 selected wavelengths (usually RGB).
static const float3 RefractionIndices = { 0.80, 0.82, 0.84 };
static const float4 WavelengthColors[3] = { { 1, 0, 0, 0 },
                                            { 0, 1, 0, 0 },
                                            { 0, 0, 1, 0 } };
                            
float4x4 World : WORLD;
float4x4 WorldViewProjection : WORLDVIEWPROJECTION;
float3 CameraPosition : CAMERAPOSITION;

float ReflectionStrength = 1.0;
float RefractionStrength = 1.0;
float FresnelBias = 0.01;
float Alpha = 0.5;
float BlendMode : BLENDMODE = 1;                // 0 = Additive alpha-blending, 1 = normal alpha-blending
texture CustomEnvironmentMap;
samplerCUBE EnvironmentMapSampler = sampler_state
{
  Texture = <CustomEnvironmentMap>;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
};


//-----------------------------------------------------------------------------
// Input, output
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float3 Normal	: NORMAL;
};


struct VSOutput
{
  float4 PositionProj : SV_Position;
  float3 NormalWorld : TEXCOORD0;
  float3 PositionToCameraWorld : TEXCOORD1;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.PositionProj  = mul(input.Position, WorldViewProjection);
  output.NormalWorld =  mul(input.Normal, (float3x3)World);
  float4 positionWorld = mul(input.Position, World);
  output.PositionToCameraWorld = CameraPosition - positionWorld.xyz;
  return output;
}


float4 PS(VSOutput input) : COLOR
{
  float3 N = normalize(input.NormalWorld);
  float3 V = normalize(input.PositionToCameraWorld);
  
  // Cubemaps are left-handed but our world space is right-handed. -->
  // Mirror z to convert to a left-handed space, otherwise the cube maps are mirrored.
  N.z = -N.z;
  V.z = -V.z;
      
  // Reflection
  float3 R = reflect(-V, N);
  float4 reflectionColor = texCUBE(EnvironmentMapSampler, R) * ReflectionStrength;
  // Use a non-linear function to increase contrast of the reflections.
  reflectionColor = pow(reflectionColor, 2);
  
  // Refraction
  float4 refractionColor = 0;
  for(int i = 0; i < 3; i++)
  {
    float3 T = refract(-V, N, RefractionIndices[i]);
    refractionColor += texCUBE(EnvironmentMapSampler, T) * WavelengthColors[i];
  }
  refractionColor *= RefractionStrength;
  
  // Compute approximate fresnel term from Schlick's approximation and use it
  // to lerp between transmission and reflection.
  float R0 = FresnelBias;
  float fresnel = saturate(R0 + (1 - R0) * pow(1.0 - max(dot(N, V), 0), 5));
  float4 result = lerp(refractionColor, reflectionColor, fresnel);
  
  // We use pre-multiplied alpha-blending.
  result.xzy *= Alpha;
  // For normal pre-multiplied alpha-blending, we have to set result.a to
  // Alpha. If we set result.a to 0, we get additive blending. If we set a value
  // between 0 and Alpha, we get a mixture of normal and additive blending.
  result.a = Alpha * BlendMode;
  return result;
}


#if !SM4
  #define VSTARGET vs_2_0
  #define PSTARGET ps_2_0
#else
  #define VSTARGET vs_4_0_level_9_1
  #define PSTARGET ps_4_0_level_9_1
#endif

technique
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
