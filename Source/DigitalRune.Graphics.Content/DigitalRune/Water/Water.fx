//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Water.fx
/// Water rendering for rivers, lakes, ocean.
//
// This water effect supports meshes, projected grid, reflection, refraction,
// soft edges, water flow, Fresnel, underwater fog, ...
//-----------------------------------------------------------------------------

// Notes:
// - Underwater fog is similar to Crysis fog, see GDC 2007 presentation.
//
// Todos:
// - Optimize.
// - Scale refraction more in world y direction than horizontal.
// - Filter normals and specular highlights in the distance.
//   (See LEAN and similar.)
//   Reduce specular power in the distance and use power to select a
//   reflection mipmap level.
// - Blur reflection. (More blur vertically.)
// - For flat surfaces we could combine the surface and underwater passes.


#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"
#include "../Fog.fxh"

#pragma warning( disable : 3571 )      // pow(f, e) - pow will not work for negative f.


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// ----- Global Parameters
float4x4 View;
float4x4 Projection;
float4 CameraParameters;
#define CameraPosition CameraParameters.xyz
#define CameraFar CameraParameters.w

float2 ViewportSize;
float Time;

float3 AmbientLight;
float3 DirectionalLightDirection;
float3 DirectionalLightIntensity;

DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);

texture RefractionTexture;
sampler2D RefractionSampler = sampler_state
{
  Texture = <RefractionTexture>;
  MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = POINT;
  AddressU = CLAMP;
  AddressV = CLAMP;
};


// ----- Projected Grid Parameters
float3 PushedBackCameraPosition;
float3 NearCorners[4];
float3 ProjectedGridParameters;
#define EdgeAttenuation ProjectedGridParameters.x
#define DistanceAttenuationStart ProjectedGridParameters.y
#define DistanceAttenuationEnd ProjectedGridParameters.z


// ----- Water Node Parameters
float4x4 World;
float SurfaceLevel;  // Same as node position y. Same as World._m31.

float2 ReflectionTypeParameters;
// -1 = no reflection texture, 0 = PlanarReflection, 1 = CubeReflection
#define ReflectionType ReflectionTypeParameters.x
// for cube maps: RGBM max value in gamma space. 1 for normal sRGB.
#define RgbmMaxValue ReflectionTypeParameters.y

// Converts from world to reflection texture space.
float4x4 ReflectionMatrix;

float2 ReflectionTextureSize;
texture PlanarReflectionMap;
sampler PlanarReflectionSampler = sampler_state
{
  Texture = <PlanarReflectionMap>;
  MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = POINT;
  AddressU = CLAMP;
  AddressV = CLAMP;
};

texture CubeReflectionMap;
samplerCUBE CubeReflectionSampler = sampler_state
{
  Texture = <CubeReflectionMap>;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
  AddressU = CLAMP;
  AddressV = CLAMP;
};


// ----- Water Parameters
texture NormalMap0;
sampler NormalSampler0 = sampler_state
{
  Texture = <NormalMap0>;
  AddressU = WRAP;
  AddressV = WRAP;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
};
texture NormalMap1;
sampler NormalSampler1 = sampler_state
{
  Texture = <NormalMap1>;
  AddressU = WRAP;
  AddressV = WRAP;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
};
float4 NormalMap0Parameters;
#define NormalMap0Scale NormalMap0Parameters.x
#define NormalMap0Offset NormalMap0Parameters.yz
#define NormalMap0Strength NormalMap0Parameters.w

float4 NormalMap1Parameters;
#define NormalMap1Scale NormalMap1Parameters.x
#define NormalMap1Offset NormalMap1Parameters.yz
#define NormalMap1Strength NormalMap1Parameters.w

float4 SpecularParameters;
#define SpecularColor SpecularParameters.xyz
#define SpecularPower SpecularParameters.w

float4 ReflectionParameters;
#define ReflectionColor ReflectionParameters.xyz
#define ReflectionDistortion ReflectionParameters.w

float4 RefractionParameters;
#define RefractionColor RefractionParameters.xyz
#define RefractionDistortion RefractionParameters.w

float3 UnderwaterFogParameters;
#define UnderwaterFogDensity UnderwaterFogParameters.xyz
//#define MaxDepth UnderwaterFogParameters.w

float3 FresnelParameters;
#define FresnelBias FresnelParameters.x
#define FresnelScale FresnelParameters.y
#define FresnelPower FresnelParameters.z

float IntersectionSoftness;
float3 WaterColor;
float3 ScatterColor;

texture FoamMap;
sampler FoamSampler = sampler_state
{
  Texture = <FoamMap>;
  AddressU = WRAP;
  AddressV = WRAP;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
};

float4 FoamParameters0;
#define FoamColor FoamParameters0.rgb
#define FoamMapScale FoamParameters0.w
float4 FoamParameters1;
#define FoamDistortion FoamParameters1.x
#define FoamShoreIntersection FoamParameters1.y
#define FoamCrestMin FoamParameters1.z
#define FoamCrestMax FoamParameters1.w

int CausticsSampleCount;
float4 CausticsParameters;
#define CausticsSampleOffset CausticsParameters.x
#define CausticsDistortion CausticsParameters.y
#define CausticsExponent CausticsParameters.z
#define CausticsIntensity CausticsParameters.w


// ----- Wave Map
float4 WaveMapParameters; // x = scale, yz = offset, w = (0 = disable, 1 = wrap, 2 = clamp)
#define WaveMapScale WaveMapParameters.x
#define WaveMapOffset WaveMapParameters.yz
#define WaveMapIsTiling WaveMapParameters.w

float2 WaveMapSize;
texture DisplacementTexture;
sampler DisplacementSampler = sampler_state
{
  Texture = <DisplacementTexture>;
  AddressU = WRAP;
  AddressV = WRAP;
  MINFILTER = POINT;
  MAGFILTER = POINT;
  MIPFILTER = POINT;
};
texture WaveNormalMap;
sampler WaveNormalSampler = sampler_state
{
  Texture = <WaveNormalMap>;
  AddressU = WRAP;
  AddressV = WRAP;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
};


// ----- Water Flow
float4 FlowParameters0;
#define SlopeSpeed FlowParameters0.x
#define FlowMapSpeed FlowParameters0.y
#define CycleDuration FlowParameters0.z
#define MaxSpeed FlowParameters0.w

float3 FlowParameters1;
#define MinStrength FlowParameters1.x
#define NoiseMapScale FlowParameters1.y
#define NoiseMapStrength FlowParameters1.z

//float3x3 FlowMapTextureMatrix;  // :-( Not supported in MonoGame.
//float2x2 FlowMapWorldMatrix;
float4x4 FlowMapTextureMatrix;
float4x4 FlowMapWorldMatrix;

texture FlowMap;
sampler FlowSampler = sampler_state
{
  Texture = <FlowMap>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
};

texture NoiseMap;
sampler NoiseSampler = sampler_state
{
  Texture = <NoiseMap>;
  AddressU = WRAP;
  AddressV = WRAP;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
};


// ----- Fog
// Color of fog (RGBA).
float4 FogColor0;
float4 FogColor1;
float3 FogHeights;     // Reference Height (camera height - fog node height), Fog Height0, Fog Height1
#define FogHeightRef FogHeights.x
#define FogHeight0 FogHeights.y
#define FogHeight1 FogHeights.z

// Combined fog parameters.
float4 FogParameters : FOGPARAMETERS;  // (Start, End, Density, HeightFalloff)
#define FogStart FogParameters.x
#define FogEnd FogParameters.y
#define FogDensity FogParameters.z
#define FogHeightFalloff FogParameters.w

// The scattering symmetry constant g for the phase function.
float3 FogScatteringSymmetry;

// ----- Misc
// Used to bend waves up/down to avoid cutting the camera near plane.
float4 CameraMisc;
#define CameraClearUpperLimit CameraMisc.x
#define CameraClearLowerLimit CameraMisc.y
#define CameraClearStart CameraMisc.z
#define CameraClearEnd CameraMisc.w


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSOutput
{
  float2 TexCoord0 : TEXCOORD0;
  float2 TexCoord1 : TEXCOORD1;
  float3 PositionWorld: TEXCOORD2;
  float3 NormalOrWaveMapInfo : TEXCOORD3;  // World space normal or wave map texcoord xy.
  float4 PositionProj: TEXCOORD4;
  float Depth: TEXCOORD5;
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 TexCoord0 : TEXCOORD0;
  float2 TexCoord1 : TEXCOORD1;
  float3 PositionWorld: TEXCOORD2;
  float3 NormalOrWaveMapInfo : TEXCOORD3;
  float4 PositionProj: TEXCOORD4;
  float Depth: TEXCOORD5;
  float Face : VFACE;
};


struct VSUnderwaterOutput
{
  float3 PositionWorld: TEXCOORD0;
  float4 PositionProj: TEXCOORD1;
  float Depth: TEXCOORD2;
  float4 Position : SV_Position;
};


struct PSUnderwaterInput
{
  float3 PositionWorld: TEXCOORD0;
  float4 PositionProj: TEXCOORD1;
  float Depth: TEXCOORD2;
  float Face : VFACE;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(float4 position : POSITION,
            float3 normal : NORMAL,
            float2 texCoord : TEXCOORD,
            uniform bool enableProjectedGrid,
            uniform bool enableDisplacement)
{
  VSOutput output = (VSOutput)0;
  
  // ----- Compute PositionWorld and Normal
  if (!enableProjectedGrid)
  {
    // No projected grid.
    output.PositionWorld = mul(position, World).xyz;
    output.NormalOrWaveMapInfo = mul(normal, (float3x3)World);
  }
  else
  {
    // Projected grid
    // To compute view direction, we know the 4 corners of the grid and use the
    // texCoords to lerp the corner vectors.
    // Note: We could make the last lerp using pow(texCoord.y + 0.000001, k).
    // By changing k, the user can choose to have more resolution at the bottom
    // or the top.
    float3 viewDirection = lerp(lerp(NearCorners[0], NearCorners[1], texCoord.x),
                                lerp(NearCorners[2], NearCorners[3], texCoord.x),
                                texCoord.y);
    
    // Clamp view direction to a "down" direction to avoid "back projection".
    // If the camera is underwater, we must clamp to an "up" direction.
    float cameraToSeaSign = sign(PushedBackCameraPosition.y - SurfaceLevel);
    // cameraToSeaSign = +1 ... camera above water
    // cameraToSeaSign = -1 ... camera under water
    viewDirection.y = cameraToSeaSign * min(cameraToSeaSign * viewDirection.y, -1e-8);
    
    // PositionWorld = intersection of the camera position with the water surface.
    // The camera position can be PushedBack to have some grid outside the original
    // FOV, which we need when the surface is displaced.
    output.PositionWorld = PushedBackCameraPosition + viewDirection * (PushedBackCameraPosition.y - SurfaceLevel) / -viewDirection.y;
    output.NormalOrWaveMapInfo = float3(0, 1, 0);
  }
  
  // ----- Apply displacement map.
#if !OPENGL
  if (enableDisplacement)
  {
    // Wave map texture coordinates.
    output.NormalOrWaveMapInfo.xy = output.PositionWorld.xz * WaveMapScale + WaveMapOffset;
    
    // WRAP or CLAMP? Sampler uses WRAP. We have to clamp manually when needed.
    if (WaveMapIsTiling > 0)
      output.NormalOrWaveMapInfo.xy = saturate(output.NormalOrWaveMapInfo.xy);
    
    //float3 displacement = tex2Dlod(DisplacementSampler, float4(output.WaveTexCoord, 0, 0));
    float3 displacement = SampleLinearLod(DisplacementSampler,
                                          float4(output.NormalOrWaveMapInfo.xy, 0, 0),
                                          WaveMapSize).rgb;
    
    // Edge attenuation:
    // Convert texCoord from [0, 1] to [-1, 1] and flip negative to [0, 1].
    // --> isEdge is 1 at projected grid edge and 0 in the grid center.
    float2 isEdge = abs(texCoord.xy * 2 - 1);
    // Lerp displacement to 0 at edges.
    float2 a = (isEdge - (1 - EdgeAttenuation)) / EdgeAttenuation;
    // Note: Have to use clamp instead of saturate because of compiler bug.
    // a = saturate(a);
    a = clamp(a, 0.000001, 1);
    // a = 0 ... displacement
    // a = 1 ... no displacement
    float edgeAttenuation = (1 - a.x) * (1 - a.y);
    // edgeAttenuation = 0 ... no displacement
    // edgeAttenuation = 1 ... displacement
    
    // Distance attenuation:
    float dist = length(CameraPosition - output.PositionWorld);
    float distanceAttenuation = saturate(1 - (dist - DistanceAttenuationStart) / (DistanceAttenuationEnd - DistanceAttenuationStart));
    // distanceAttenuation = 0 ... no displacement
    // distanceAttenuation = 1 ... displacement
    
    // Bend waves up/down to avoid cutting the near plane:
    // - Before CameraClearStart we clip waves to the limits.
    // - After CameraClearEnd we do not influence the waves.
    // - Inbetween we additionally lerp out the displacement.
    float cameraClearArea = saturate((dist - CameraClearStart) / CameraClearEnd);
    
    displacement = displacement * edgeAttenuation * distanceAttenuation * cameraClearArea;
    output.PositionWorld += displacement;
    
    // Clamp height to limits.
    float clampedHeight = clamp(output.PositionWorld.y, CameraClearLowerLimit, CameraClearUpperLimit);
    output.PositionWorld.y = lerp(clampedHeight, output.PositionWorld.y, cameraClearArea);
  }
#endif
  
  float4 positionView = mul(float4(output.PositionWorld, 1), View);
  output.Position = mul(positionView, Projection);
  output.PositionProj = output.Position;
  output.Depth = -positionView.z / CameraFar;
  
  // Normal map texture coords.
  output.TexCoord0 = output.PositionWorld.xz * NormalMap0Scale + NormalMap0Offset;
  output.TexCoord1 = output.PositionWorld.xz * NormalMap1Scale + NormalMap1Offset;
  
  return output;
}

VSOutput VSMesh(float4 position : POSITION, float3 normal : NORMAL)
{
  return VS(position, normal, (float2)0, false, false);
}
VSOutput VSMeshDisplaced(float4 position : POSITION, float3 normal : NORMAL)
{
  return VS(position, normal, (float2)0, false, true);
}
VSOutput VSProjectedGrid(float2 texCoord : TEXCOORD0)
{
  return VS((float4)0, (float3)0, texCoord, true, true);
}


float3 ComputeCaustics(float3 position, float3 lightIntensity)
{
  // We could make the caustics depth-dependent, but without a high sample count,
  // this introduces artifacts.
  float3 depth = 1;  //SurfaceLevel - position.y
  
  float3 refractionToSurface = -DirectionalLightDirection * depth;
  float3 causticPosition = position + refractionToSurface;
  
  // For method 1 (add), the intial value must be 0.
  // For method 2 (multiply), the initial value must be 1.
  float3 caustics = 1;
  
  // Combine NxN samples.
  for (int i = 0; i < CausticsSampleCount; i++)
  {
    for (int j = 0; j < CausticsSampleCount; j++)
    {
      // A horizontal offset for the sample position
      float3 offset = depth * float3(
        (i - CausticsSampleCount / 2.0) * CausticsSampleOffset,
        0,
        (j - CausticsSampleCount / 2.0) * CausticsSampleOffset) ;
      
      // Lookup position.
      float2 texCoord = (causticPosition.xz + offset.xz) * WaveMapScale + WaveMapOffset;
      
      // The direction to the lookup position is:
      float3 refractedDirection = refractionToSurface + offset;
      
      // Sample wave normal map.
      float3 n = tex2Dlod(WaveNormalSampler, float4(texCoord, 0, 0)).xzy * 2 - 1;
      
      // Use wave normal to perturb refractedDirection.
      refractedDirection -= depth * CausticsDistortion * float3(n.x, 0, n.z);
      
      // Light contribution of this sample.
      float sampleIntensity = pow(
        max(0.0001, dot(normalize(refractedDirection), -DirectionalLightDirection)),
        CausticsExponent);
      
      // Method 1: Add intensity of samples. Creates smoother caustics.
      //caustics += sampleIntensity;
      
      // Method 2: Multiply intensities. Creates caustics with more contrast.
      caustics *= sampleIntensity;
    }
  }
  
  caustics *= CausticsIntensity;
  
  // Optional: Apply a non-linear curve to enhance contrast.
  //caustics = pow(caustics, 3);
  
  caustics *= lightIntensity;
  return caustics;
}


float4 PS(PSInput input, uniform bool enableDisplacement, uniform bool enableFlow, uniform bool enableFoam, uniform bool enableCaustics) : COLOR
{
  float epsilon = 0.00001; // To avoid division by zero.
  
  // Check if camera is underwater.
  //   Face = -1 ... backside of surface
  //   Face = +1 ... frontside of surface
  bool isUnderwater = input.Face < 0;
  
  // Get view distance and direction.
  float3 cameraToPosition = input.PositionWorld - CameraPosition;
  float cameraDistance = length(cameraToPosition);
  float3 viewDirection = cameraToPosition / cameraDistance;
  
  // Get the screen space texture coordinate for this pixel position.
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  
  // Sample G-Buffer depth.
  float4 gBuffer0Sample = tex2Dlod(GBuffer0Sampler, float4(texCoordScreen, 0, 0));
  float gBufferDepth = GetGBufferDepth(gBuffer0Sample);
  
  // Compute intersection softness.
  float waterDepth = gBufferDepth - input.Depth;
  
  // Early out for pixels outside the water.
  clip(waterDepth);
  
  // OPTIMIZE: Store IntersectionSoftness as CameraFar / IntersectionSoftness.
  float intersectionSoftness = saturate(waterDepth * CameraFar / IntersectionSoftness);
  // Note: Others use intersectionSoftness = 1 - 1 / (1 + k * d)
  
  // Get geometric surface normal.
  if (enableDisplacement)
  {
    float distanceAttenuation = saturate(1 - (cameraDistance - DistanceAttenuationStart) / (DistanceAttenuationEnd - DistanceAttenuationStart));
    input.NormalOrWaveMapInfo = tex2D(WaveNormalSampler, input.NormalOrWaveMapInfo.xy).xzy * 2 - 1;
    input.NormalOrWaveMapInfo.xz *= distanceAttenuation;
  }
  float3 surfaceNormal = normalize(input.NormalOrWaveMapInfo);
  
  // ----- Normal Map
  // Get bump map normal (unnormalized).
  float3 normal;
  if (!enableFlow)
  {
    // Combine two normal maps.
    float3 normal0 = GetNormalDxt5nm(NormalSampler0, input.TexCoord0);
    float3 normal1 = GetNormalDxt5nm(NormalSampler1, input.TexCoord1);
    normal0.xy *= NormalMap0Strength;
    normal1.xy *= NormalMap1Strength;
    
    normal = normal0 + normal1;
  }
  else
  {
    // Speed from inclined surfaces.
    float2 slopeDirection = surfaceNormal.xz;
    float l = length(slopeDirection);
    slopeDirection /= l + epsilon;  // Normalize.
    
    float2 slopeVelocity = SlopeSpeed * slopeDirection;
    
    // Sample flow map.
    float2 flowMapTexCoord = mul(float3(input.PositionWorld.xz, 1), (float3x3)FlowMapTextureMatrix).xy;
    float3 flowMap = tex2D(FlowSampler, flowMapTexCoord).rgb;
    float2 flowMapDirection = mul(flowMap.xy * 2 - 1, (float2x2)FlowMapWorldMatrix);
    float flowMapSpeed = flowMap.z;
    float2 flowMapVelocity = flowMapDirection * flowMapSpeed * FlowMapSpeed;
    float2 flowVelocity = slopeVelocity + flowMapVelocity;
    
    // Limit velocity to MaxSpeed.
    float flowSpeed = length(flowVelocity);
    flowVelocity /= flowSpeed + epsilon;
    flowSpeed = min(MaxSpeed, flowSpeed);
    flowVelocity *= flowSpeed;
    
    // Sample noise map. (Noise is used to offset the cycle.)
    float noiseValue = NoiseMapStrength * tex2Dlod(NoiseSampler, float4(flowMapTexCoord * NoiseMapScale, 0, 0)).r;
    
    // Current position in cycle.
    float cycle = (noiseValue + Time) / CycleDuration;
    
    // Absolute distance travelled over whole cycle.
    float2 flowOffset = flowVelocity * CycleDuration;
    
    // Relative position in cycle.
    // x goes from 0 to 1. (First normal map layer.)
    // y goes from 0.5 to 1 and 0 to 0.5. (Second normal map layer.)
    float2 flowParameter = frac(float2(cycle, cycle + 0.5));
    
    // Optional: Use an arbitrary constant offset in each cycle to avoid that the
    // same wave pattern repeats at the same place.
    float offset0 = floor(cycle) * 0.1;
    float offset1 = floor(cycle + 0.5) * 0.1 + 0.5;
    
    // First normal map layer.
    float3 normal0 = GetNormalDxt5nm(NormalSampler0, input.TexCoord0 + offset0 - ((flowParameter.x - 0.5) * flowOffset) * NormalMap0Scale);
    float3 normal1 = GetNormalDxt5nm(NormalSampler1, input.TexCoord1 + offset0 - ((flowParameter.x - 0.5) * flowOffset) * NormalMap1Scale);
    normal0.xy *= NormalMap0Strength;
    normal1.xy *= NormalMap1Strength;
    
    // Second normal map layer.
    float3 normal2 = GetNormalDxt5nm(NormalSampler0, input.TexCoord0 + offset1 - ((flowParameter.y - 0.5) * flowOffset) * NormalMap0Scale);
    float3 normal3 = GetNormalDxt5nm(NormalSampler1, input.TexCoord1 + offset1 - ((flowParameter.y - 0.5) * flowOffset) * NormalMap1Scale);
    normal2.xy *= NormalMap0Strength;
    normal3.xy *= NormalMap1Strength;
    
    // Lerp between both normal map layers.
    normal = lerp(normal0 + normal1, normal2 + normal3, abs(2 * flowParameter.x - 1));
    
    // Scale down normals to MinStrength at MaxSpeed.
    normal.xy = lerp(normal.xy, normal.xy * MinStrength, flowSpeed / MaxSpeed);
  }
  
  // Convert bump map normal from texture space to world space.
  float3x3 cotangentFrame = CotangentFrame(surfaceNormal, input.PositionWorld, input.TexCoord0);
  normal = normalize(mul(normal, cotangentFrame));
  
  // ----- Fresnel Factor
  float fresnel = saturate(FresnelBias + FresnelScale * pow(abs(1.0 - max(dot(normal, -viewDirection), 0)), FresnelPower));
  
  // Less reflection near intersection.
  fresnel *= intersectionSoftness;
  
  // ----- Specular Highlight
  //float nDotL = saturate(dot(normal, -LightDirection));
  // Blinn-Phong specular
  float3 v = viewDirection;
  v.y *= input.Face;  // Change y view direction for underwater specular.
  float3 h = -normalize(DirectionalLightDirection + v);
  float3 nDotH = saturate(dot(normal, h));
  
  float specularPower = SpecularPower;
  // Make sun reflection broader in the distance.
  //specularPower *= exp(-0.0031*cameraDistance);
  // Or
  //specularPower *= 1 / (1 + 0.01 * cameraDistance);
  // Or using hyperbolic z.
  //specularPower = lerp(MaxSpecularPower, MinSpecularPower, input.PositionProj.z / input.PositionProj.w);
  // TODO: If the specular power is changed, we also have to scale the intensity, see physically-based rendering.
  
  float3 blinnPhong = pow(nDotH + epsilon, specularPower);
  float3 sunLight = SpecularColor * DirectionalLightIntensity * blinnPhong;
  sunLight *= intersectionSoftness;
  //sunLight *= fresnel;  // ShaderX3 multiplies sunLight with Fresnel
  
  // ----- Reflection
  float3 reflection = ReflectionColor;
  if (!isUnderwater && ReflectionType >= 0)
  {
    [branch]
    if (ReflectionType == 0)
    {
      // Planar Reflection.
      float3 normalInPlane = normal - surfaceNormal * dot(normal, surfaceNormal);
      float4 offset = float4(normalInPlane * ReflectionDistortion * intersectionSoftness, 0);
      float4 lookupPosition = float4(input.PositionWorld, 1) + offset;
      float4 reflectionTexCoord = mul(lookupPosition, ReflectionMatrix);
      //reflection *= tex2Dproj(PlanarReflectionSampler, reflectionTexCoord);
      reflection *= SampleLinearLod(PlanarReflectionSampler, float4(reflectionTexCoord.xy / reflectionTexCoord.w, 0, 0), ReflectionTextureSize).rgb;
    }
    else
    {
      // Cube map reflection
      float3 reflectionNormal;
      if (enableDisplacement)
        reflectionNormal = lerp(float3(0, 1, 0), normal, ReflectionDistortion);
      else
        reflectionNormal = lerp(surfaceNormal, normal, ReflectionDistortion);
      
      float3 viewDirectionReflected = reflect(viewDirection, reflectionNormal);
      
      // Clamp to horizon to avoid sampling lower hemisphere (which is black or
      // useless in many cube maps).
      viewDirectionReflected.y = max(0, viewDirectionReflected.y);
      
      viewDirectionReflected = mul((float3x3)ReflectionMatrix, viewDirectionReflected);
      reflection *= FromGamma(DecodeRgbm(texCUBElod(CubeReflectionSampler, float4(viewDirectionReflected, 0)).rgba, RgbmMaxValue).rgb);
    }
  }
  
  // ----- Refraction
  float refractionStrength = RefractionDistortion * intersectionSoftness;
  
  // Using refract()
  //float3 refractedDirection = refract(-viewDirection, normal, refractionIndex);
  //float4 refractionLookupPosition = float4(input.PositionWorld + refractedDirection * refractionStrength,  1);
  
  // Using normal as offset.
  //float4 refractionLookupPosition = float4(input.PositionWorld + refractionStrength * -normal, 1);
  
  // Using normal as offset relative to view direction.
  float3 ortho0 = normalize(float3(0, -viewDirection.z, viewDirection.y));
  float3 ortho1 = cross(viewDirection, ortho0);
  float4 refractionLookupPosition = float4(input.PositionWorld + refractionStrength * (ortho0 * normal.x + ortho1 * normal.z), 1);
  
  float4 refractionLookupPositionProj = mul(refractionLookupPosition, mul(View, Projection));
  float2 refractionTexCoord = ProjectionToScreen(refractionLookupPositionProj, ViewportSize);
  
  // Rejected refraction pixels which are actually in front of the water.
  gBuffer0Sample = tex2Dlod(GBuffer0Sampler, float4(refractionTexCoord, 0, 0));
  float refractionDepth = GetGBufferDepth(gBuffer0Sample);
  float refractionDepthDelta;
  if (refractionDepth >= input.Depth)
  {
    // Sample is underwater.
    refractionDepthDelta = refractionDepth - input.Depth;
  }
  else
  {
    // Sample is above water. Sample undisturbed position instead.
    refractionTexCoord = texCoordScreen;
    refractionDepth = gBufferDepth;
    refractionDepthDelta = waterDepth;
  }
  
  // Position of refracted scene.
  float3 refractionPosition = CameraPosition + cameraToPosition * (refractionDepth / input.Depth);
  
  float3 refraction = tex2Dlod(RefractionSampler, float4(refractionTexCoord, 0, 0)).rgb;
  
  float surfaceLevel = input.PositionWorld.y;  //SurfaceLevel;
  
  // Light at surface.
  float nDotL = max(0, -DirectionalLightDirection.y);
  float3 lightColor = (AmbientLight + nDotL * DirectionalLightIntensity);
  
  // ----- Foam
  // Sample foam map.
  float3 foam = 0;
  float foamIntensity = 0;
  [branch]
  if (enableFoam)
  {
    foam = FoamColor * tex2D(FoamSampler, (input.PositionWorld.xz - normal.xz * FoamDistortion) * FoamMapScale).rgb;
    
    // No foam where foam map is black.
    foamIntensity = length(foam);
    
    // OPTIMIZE: Store FoamShoreIntersection as CameraFar / FoamShoreIntersection.
    // OPTIMIZE: Use FoamCrestRange instead FoamCrestMin and FoamCrestMax.
    float foamShoreAmount = 1 - saturate(refractionDepthDelta * CameraFar / FoamShoreIntersection);
    float foamCrestAmount = saturate((input.PositionWorld.y - SurfaceLevel - FoamCrestMin) / (FoamCrestMax - FoamCrestMin));
    
    // Use max of shore and crest foam.
    foamIntensity *= pow(max(foamShoreAmount, foamCrestAmount), 3);
    foamIntensity = saturate(foamIntensity);
    
    // Apply light to foam,
    foam *= AmbientLight + nDotL * DirectionalLightIntensity;
  }
  
  // ----- Subsurface scattering
  // Fake subsurface scattering of waves.
  float scatter = 0;
  if (enableDisplacement)
  {
    scatter = 1;
    // More scattering when we look towards the light.
    scatter *= saturate(-dot(viewDirection, DirectionalLightDirection) * 0.5 + 0.5);
    // More scattering when we look straight onto the wave surface.
    scatter *= saturate(-dot(viewDirection, normal));
    // Less scattering when camera is above water and looks down.
    scatter *= max(0, 1-abs(viewDirection.y));
    // More scattering when wave is high above surface level.
    scatter *= max(0, input.PositionWorld.y - SurfaceLevel);
    //scatter *= saturate(1 + DirectionalLightDirection.y);
  }
  
  lightColor *= (WaterColor + scatter * ScatterColor);
  
  // ----- Underwater Fog (see also comments in Underwater pass below).
  // Ray in the water.
  float3 o = isUnderwater ? CameraPosition : input.PositionWorld;
  float3 p = (isUnderwater && (refractionPosition.y > surfaceLevel)) ? input.PositionWorld : refractionPosition;
  
  float3 d = p - o;
  float l = length(d);
  d = d / (l + epsilon);
  
  // Attenuation.
  float3 c = UnderwaterFogDensity;
  float verticalDepth = max(0, surfaceLevel - p.y);
  float3 attenuation = exp(-c * (verticalDepth + l));
  
  // Inscatter.
  float verticalCameraDepth = max(0, surfaceLevel - o.y);
  float3 inscatter = exp(-c * verticalCameraDepth) * (exp(c * (d.y - 1) * l) - 1)
                                                     / ((d.y - 1) * c);
  
  // ----- Fog (see also FogRenderer and Fog.fx)
  float4 fogColor = 0;
  float fogIntensity = 0;
  [branch]
  if (FogDensity > 0)
  {
    // Height in world space.
    float height = input.PositionWorld.y;
    // Smoothstep distance fog
    float smoothRamp = ComputeSmoothFogIntensity(cameraDistance, FogStart, FogEnd);
    // Exponential Fog
    float heightFallOff = FogHeightFalloff;
    float referenceHeight = FogHeightRef + viewDirection.y * FogStart;
    float distanceInFog = cameraDistance - FogStart;
    //float3 fogDirection = viewDirection;  // XNA effect compiler bug destroys viewDirection vector.
    // --> If we modify fogDirection by a tiny bit, the error disappears.
    float3 fogDirection = viewDirection * 1.0000001f;
    if (heightFallOff * viewDirection.y < 0)
    {
      // See Fog.fx for detailed comments.
      referenceHeight += fogDirection.y * distanceInFog;
      fogDirection = -fogDirection;
    }
    float referenceDensity = FogDensity * exp2(-heightFallOff * referenceHeight);
    float opticalLength = GetOpticalLengthInHeightFog(distanceInFog, referenceDensity, fogDirection * distanceInFog, heightFallOff);
    float exponentialFog = ComputeExponentialFogIntensity(opticalLength, 1);  // fogDensity is already in opticalLength!
    fogColor = lerp(FogColor0, FogColor1, smoothstep(FogHeight0, FogHeight1, height));
    // Apply phase function.
    float nDotV = dot(viewDirection, -DirectionalLightDirection);
    fogColor.rgb *= FogPhaseFunction(nDotV, FogScatteringSymmetry);
    fogIntensity = fogColor.a * smoothRamp * exponentialFog;
  }
  
  // Underwater the specular highlight is part of the refraction.
  // Foam hides the refraction.
  // Fog is applied only to the refraction.
  if (isUnderwater)
  {
    refraction += sunLight;
    refraction = lerp(refraction, foam, foamIntensity);
    refraction = lerp(refraction, fogColor.rgb, fogIntensity);
  }
  
  // Caustics
  float3 caustics = 0;
  if (enableCaustics && !isUnderwater)
    caustics = ComputeCaustics(refractionPosition, nDotL * DirectionalLightIntensity);
  
  // Apply underwater fog to refraction
  float3 refractionColor = isUnderwater ? 1 : (RefractionColor + caustics);
  float3 underwaterColor = refraction * refractionColor * attenuation + inscatter * lightColor;
  
  // No reflection underwater.
  if (isUnderwater)
  {
    fresnel = 0;
  }
  
  // Apply Fresnel.
  float3 color = lerp(underwaterColor, reflection, fresnel);
  
  // Above water, the specular highlight is added to reflection/refraction result.
  // Foam replaces the normal water color.
  if (!isUnderwater)
  {
    color += sunLight;
    color = lerp(color, foam, foamIntensity);
    color = lerp(color, fogColor.rgb, fogIntensity);
  }
  
  // Soft intersection. (No depth-based fade out when we are under water.)
  if (!isUnderwater)
    color = lerp(refraction, color, saturate(refractionDepthDelta * CameraFar / IntersectionSoftness));
  
  return float4(color, 1);
}

float4 PSBasic(PSInput input) : COLOR { return PS(input, false, false, false, false); }
float4 PSFoam(PSInput input) : COLOR { return PS(input, false, false, true, false); }
float4 PSFlow(PSInput input) : COLOR { return PS(input, false, true, false, false); }
float4 PSFoamFlow(PSInput input) : COLOR { return PS(input, false, true, true, false); }
float4 PSDisplaced(PSInput input) : COLOR { return PS(input, true, false, false, false); }
float4 PSDisplacedFoam(PSInput input) : COLOR { return PS(input, true, false, true, false); }
float4 PSDisplacedFoamCaustics(PSInput input) : COLOR { return PS(input, true, false, true, true); }



VSUnderwaterOutput VSUnderwater(float4 position : POSITION)
{
  VSUnderwaterOutput output = (VSUnderwaterOutput)0;
  output.PositionWorld = mul(position, World).xyz;
  float4 positionView = mul(float4(output.PositionWorld, 1), View);
  output.Position = mul(positionView, Projection);
  output.PositionProj = output.Position;
  output.Depth = -positionView.z / CameraFar;
  return output;
}


float4 PSUnderwater(PSUnderwaterInput input, uniform bool enableCaustics) : COLOR
{
  // Get view distance and direction.
  float3 cameraToPosition = input.PositionWorld - CameraPosition;
  
  // Get the screen space texture coordinate for this pixel position.
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  
  // Sample G-Buffer depth.
  float4 gBuffer0Sample = tex2Dlod(GBuffer0Sampler, float4(texCoordScreen, 0, 0));
  float gBufferDepth = GetGBufferDepth(gBuffer0Sample);
  
  // Position of the non-water geometry.
  float3 scenePosition = CameraPosition + cameraToPosition * (gBufferDepth / input.Depth);
  
  // Sample current scene.
  float3 sceneColor = tex2Dlod(RefractionSampler, float4(texCoordScreen, 0, 0)).rgb;
  
  // If the camera and pixel are above the water level, then we do not fog
  // the pixel.
  if (CameraPosition.y > SurfaceLevel && scenePosition.y > SurfaceLevel)
    return float4(sceneColor, 1);
  
  // Ray (o -> p) in the water.
  float3 o = CameraPosition;
  float3 p = scenePosition;
  
  // Get the ray that is within SurfaceLevel and MaxDepth.
  // Move o forward to surface level.
  if (o.y > SurfaceLevel)
    o = o + cameraToPosition * max(o.y - SurfaceLevel, 0) / (abs(cameraToPosition.y) + 0.00001);
  
  // Ray direction vector d.
  float3 d = p - o;
  
  // Length of ray.
  float l = length(d);
  
  // Normalize d.
  d = d / (l + 0.00001);
  
  // Attenuation.
  float3 c = UnderwaterFogDensity;
  float verticalDepth = max(0, SurfaceLevel - p.y);
  float3 attenuation = exp(-c * (verticalDepth + l));
  
  // Inscatter.
  float nDotL = max(0, -DirectionalLightDirection.y);
  float3 lightColor = (AmbientLight + nDotL * DirectionalLightIntensity) * WaterColor;
  float verticalCameraDepth = max(0, SurfaceLevel - o.y);
  float3 inscatter = exp(-c * verticalCameraDepth) * (exp(c * (d.y - 1) * l) - 1)
                                                     / ((d.y - 1) * c);
  
  // Caustics
  float3 caustics = 0;
  if (enableCaustics)
    caustics = ComputeCaustics(scenePosition, nDotL * DirectionalLightIntensity);
  
  float3 color = sceneColor * (RefractionColor + caustics) * attenuation + inscatter * lightColor;
  return float4(color, 1);
}
float4 PSUnderwaterBasic(PSUnderwaterInput input) : COLOR { return PSUnderwater(input, false); }
float4 PSUnderwaterCaustics(PSUnderwaterInput input) : COLOR { return PSUnderwater(input, true); }


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4

#define PASS(NAME, VS, PS) \
  pass NAME\
  {\
    VertexShader = compile vs_3_0 VS(); \
    PixelShader = compile ps_3_0 PS(); \
  }
  
#else

#define PASS(NAME, VS, PS) \
  pass NAME\
  {\
    VertexShader = compile vs_4_0 VS(); \
    PixelShader = compile ps_4_0 PS(); \
  }
  
#endif


technique
{
  // ----- Surface Mesh without Displacement
  PASS(Mesh, VSMesh, PSBasic)
  PASS(MeshFoam, VSMesh, PSFoam)
  PASS(MeshFlow, VSMesh, PSFlow)
  PASS(MeshFoamFlow, VSMesh, PSFoamFlow)
    
  // ----- Surface Mesh with Displacement
  PASS(MeshDisplaced, VSMeshDisplaced, PSDisplaced)
  PASS(MeshDisplacedFoam, VSMeshDisplaced, PSDisplacedFoam)
  PASS(MeshDisplacedFoamCaustics, VSMeshDisplaced, PSDisplacedFoamCaustics)
    
  // ----- Projected Grid with Displacement
  PASS(ProjectedGrid, VSProjectedGrid, PSDisplaced)
  PASS(ProjectedGridFoam, VSProjectedGrid, PSDisplacedFoam)
  PASS(ProjectedGridFoamCaustics, VSProjectedGrid, PSDisplacedFoamCaustics)
    
  // ----- Underwater effect
  PASS(Underwater, VSUnderwater, PSUnderwaterBasic)
  PASS(UnderwaterCaustics, VSUnderwater, PSUnderwaterCaustics)
}
