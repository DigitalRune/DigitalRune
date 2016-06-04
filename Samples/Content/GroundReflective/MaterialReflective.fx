//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file MaterialReflective.fx
/// Combines the material of a model (e.g. textures) with the light buffer data
//  and a planar reflection.
//
//-----------------------------------------------------------------------------

#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Encoding.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Deferred.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Material.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 ViewProjection : VIEWPROJECTION;
float3 CameraPosition : CAMERAPOSITION;
float2 ViewportSize : VIEWPORTSIZE;

DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);
DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer1, 1);

float3 DiffuseColor : DIFFUSECOLOR;
float3 SpecularColor : SPECULARCOLOR;
DECLARE_UNIFORM_DIFFUSETEXTURE;
DECLARE_UNIFORM_SPECULARTEXTURE;

// The texture that contains the reflected scene. This parameter must be
// "PerInstance" because each instance needs an individual reflection texture.
texture ReflectionTexture < string Hint = "PerInstance"; >;
sampler ReflectionSampler = sampler_state
{
  Texture = <ReflectionTexture>;
  MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = POINT;
  AddressU = CLAMP;
  AddressV = CLAMP;
};

// The width and height of the reflection texture in texels.
float2 ReflectionTextureSize < string Hint = "PerInstance"; >;

// Converts from world space to reflection texture space.
float4x4 ReflectionMatrix < string Hint = "PerInstance"; >;

// Normal vector of the reflection plane.
float3 ReflectionNormal < string Hint = "PerInstance"; >;

// How much the normals distort the reflection.
float ReflectionBumpStrength = 0.1f; 

// The average distance of reflected objects. Used to compute the effect
// of normal mapping on the reflections.
float ReflectionDistance = 10;

// The color to use for reflections outside the ReflectionTexture.
// (Only relevant for very special cases, like reflections visible in other reflections.
float3 ReflectionBorderColor = float3(0.1, 0.1, 0.1);

float3 FresnelParameters = float3(0.0, 1, 3);
#define FresnelBias FresnelParameters.x
#define FresnelScale FresnelParameters.y
#define FresnelPower FresnelParameters.z


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float3 PositionWorld : TEXCOORD1;
  float4 PositionProj : TEXCOORD2;
  float4 Position : SV_Position;
};

struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float3 PositionWorld : TEXCOORD1;
  float4 PositionProj : TEXCOORD2;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.TexCoord = input.TexCoord;
  
  float4 positionWorld = mul(input.Position, World);
  output.PositionWorld = positionWorld.xyz;
  output.Position = mul(positionWorld, ViewProjection);
  output.PositionProj = output.Position;
  
  return output;
}


// Returns a value in [0, 1] depending on x.
// The returned value fades from 0 to 1 if x is in the interval [0, threshold] 
// and fades back to 0 if x in the interval [1 - threshold, 1].
float FadeBorders(float x, float threshold)
{
  // Fade in [0, threshold].
  float a = saturate(x / threshold);
  
  // Fade out [1 - threshold, 1].
  float b = 1 - saturate((x - (1 - threshold)) / threshold);
  
  return a * b;
}


float4 PS(PSInput input) : COLOR0
{
  // Get the screen space texture coordinate for this position.
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  
  // Get normal.
  float4 gBuffer1Sample = tex2D(GBuffer1Sampler, texCoordScreen);
  float3 normal = GetGBufferNormal(gBuffer1Sample);
  
  normal = lerp(ReflectionNormal, normal, ReflectionBumpStrength);
  
  // Compute a world space lookup position which we can use to sample the
  // reflection map. The lookup position is modified by the normal map to
  // create "bumpy" reflections.
#if 1
  // Advanced perturbation method: 
  // Assume that reflection image is the image of a plane with a certain distance.
  // Compute perturbed lookup position that is exact for this plane.
  // (For more details, see "Reflections from Bumpy Surfaces", ShaderX³, pp. 107.)
  float3 viewDirection = input.PositionWorld - CameraPosition;
  float3 viewDirectionReflected = reflect(viewDirection, normal); // Using distorted normal, not plane normal!
  float cameraToPlaneDistance = -dot(viewDirection, ReflectionNormal);
  float t1 = cameraToPlaneDistance / (ReflectionDistance + cameraToPlaneDistance);  // This could be computed on CPU or in Vertex Shader.
  float3 cameraPositionReflected = CameraPosition - 2 * cameraToPlaneDistance * ReflectionNormal;
  float3 lookupPosition = input.PositionWorld;
  lookupPosition = 
    (1 - t1) * cameraPositionReflected
    + t1 * (lookupPosition + ReflectionDistance / dot(ReflectionNormal, viewDirectionReflected) * viewDirectionReflected);
  // Shorter version when average ReflectionDistance is infinity.
  //lookupPosition = cameraPositionReflected +  cameraToPlaneDistance / dot(ReflectionNormal, viewDirectionReflected) * viewDirectionReflected;
#else
  // Simple perturbation method:
  // Get the normal vector part in the reflection plane and use as offset.
  float3 normalInPlane = normal - normal * dot(normal, ReflectionNormal);
  float3 offset = normalInPlane * ReflectionBumpStrength * 100;
  float3 lookupPosition = input.PositionWorld + offset;
#endif
  
  // Convert world space lookup position to texture space.
  float4 reflectionTexCoord = mul(float4(lookupPosition, 1), ReflectionMatrix);
  
  // Sample texture. If texture uses a floating point format, we cannot use hardware
  // filtering and perform bilinear filtering manually.
  //float3 reflection = tex2Dproj(ReflectionSampler, reflectionTexCoord);
  float3 reflection = SampleLinear(ReflectionSampler, reflectionTexCoord.xy / reflectionTexCoord.w, ReflectionTextureSize).rgb;
    
#if 1
  // ------ Handling reflectionTexCoords outside [0,1].
  reflectionTexCoord.xy /= reflectionTexCoord.w;
  // If a single reflection is rendered, the texture coordinates will be in [0,1].
  // But if a reflection texture is rendered inside another reflection or from
  // a different camera, we might end up with reflectionTexCoords outside [0,1].
  // To avoid artifacts from the CLAMP texture address mode, we should return a
  // constant border color outside the valid range. We can
  // A) return border color outside [0,1].
  if (reflectionTexCoord.x < 0 || reflectionTexCoord.x > 1 || reflectionTexCoord.y < 0 || reflectionTexCoord.y > 1 || reflectionTexCoord.w < 0)
    reflection = ReflectionBorderColor;
  
  // B) fade to border color.
  // (Fading has the problem that it also affects the borders of regular reflections.)
  //reflection = lerp(
  //  ReflectionBorderColor, 
  //  reflection,
  //  FadeBorders(reflectionTexCoord.x, 0.1) * FadeBorders(reflectionTexCoord.y, 0.1) * (reflectionTexCoord.w >= 0));
#endif 

  // Compute approximate Fresnel term and use it to scale reflection.
  //float3 viewDirection = normalize(input.PositionWorld.xyz - CameraPosition);
  //float fresnel = saturate(FresnelBias + FresnelScale * pow(abs(1.0 - max(dot(normal, -viewDirection), 0)), FresnelPower));
  //reflection *= fresnel;

  float4 lightBuffer0Sample = tex2D(LightBuffer0Sampler, texCoordScreen);
  float4 lightBuffer1Sample = tex2D(LightBuffer1Sampler, texCoordScreen);
  
  float3 diffuseLight = GetLightBufferDiffuse(lightBuffer0Sample, lightBuffer1Sample);
  float3 specularLight = GetLightBufferSpecular(lightBuffer0Sample, lightBuffer1Sample);
  
  float4 diffuseMap = tex2D(DiffuseSampler, input.TexCoord);
  float4 specularMap = tex2D(SpecularSampler, input.TexCoord);
  float3 diffuse = FromGamma(diffuseMap.rgb);
  float3 specular = FromGamma(specularMap.rgb);

  return float4(
    DiffuseColor * diffuse * diffuseLight 
    + SpecularColor * specular * (specularLight + reflection), 
    1);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique Default
{
  pass
  {
#if !SM4
    VertexShader = compile vs_3_0 VS();
    PixelShader = compile ps_3_0 PS();
#else
    VertexShader = compile vs_4_0_level_9_3 VS();
    PixelShader = compile ps_4_0_level_9_3 PS();
#endif
  }
}
