//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file FogSphere.fx
/// Renders a simple sphere of fog.
//
//-----------------------------------------------------------------------------

#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Encoding.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 ViewportSize;
float4x4 World;
float4x4 WorldInverse;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float CameraFar;

DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);

float3 Color;
float BlendMode;    // [0, 1], 0 = additive blending, 1 = alpha blending
float Density;
float Falloff;
float IntersectionSoftness;


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
};

struct VSOutput
{
  float3 PositionLocal : TEXCOORD1;
  float3 PositionWorld : TEXCOORD2;
  float4 PositionProj : TEXCOORD3;
  float Depth: TEXCOORD4;
  float4 Position : SV_Position;
};

struct PSInput
{
  float3 PositionLocal : TEXCOORD1;
  float3 PositionWorld : TEXCOORD2;
  float4 PositionProj : TEXCOORD3;
  float Depth: TEXCOORD4;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------
VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;

  // Optional: 
  // The tesselated sphere does not fully enclose the actual sphere. To avoid 
  // the tesselated look, we can increase the sphere. The pixel shader will cast
  // a ray against the actual sphere and clip outside pixels.
  input.Position.xyz = input.Position.xyz * 1.05f;
  
  float4 positionWorld = mul(input.Position, World);
  float4 positionView = mul(positionWorld, View);
  output.PositionLocal = input.Position.xyz;
  output.PositionWorld = positionWorld.xyz;
  output.Position = mul(positionView, Projection);
  output.PositionProj = output.Position;
  output.Depth = -positionView.z / CameraFar;

  return output;
}


float4 PS(VSOutput input) : COLOR
{
  // Vector from camera to pixel position in world space.
  float3 cameraToPosition = input.PositionWorld - CameraPosition;
  float cameraToPositionLength = length(cameraToPosition);
  
  // Get the screen space texture coordinate for this pixel position.
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  
  // Get depth from G-buffer.
  float4 gBuffer0Sample = tex2Dlod(GBuffer0Sampler, float4(texCoordScreen, 0, 0));
  float gBufferDepth = GetGBufferDepth(gBuffer0Sample);
  
  // input.Depth is the normalized distance of this pixel in the CameraForward direction.
  // gBufferDepth is the normalized distance of the scene at this pixel in the CameraForward direction.
  // relativeSceneDistance is the ray parameter of the scene at this pixel of the cameraToPosition ray.
  float relativeSceneDistance = gBufferDepth / input.Depth;
  
  // sceneDistance is the linear distance of the scene in world space.
  //float sceneDistance = relativeSceneDistance * cameraToPositionLength;

  // Transform CameraPosition to the local space of the sphere.
  float3 cameraPositionLocal = mul(float4(CameraPosition, 1), WorldInverse).xyz;
  
  // Transform cameraToPosition vector to the local space of the sphere.
  float3 cameraToPositionLocal = input.PositionLocal - cameraPositionLocal;
  // Same as:
  //float3 viewDirectionLocal = mul(cameraToPosition, WorldInverse);
  
  float cameraToPositionLocalLength = length(cameraToPositionLocal);
  
  // Compute intersection of the ray (origin = cameraPosition, direction = normalize(cameraToPositionLocal))
  // with the sphere. 'enter' and 'exit' are the distances of the ray hits. 'enter' is 0 if camera is inside sphere.
  float enter, exit;
  float hit = HitSphere(cameraPositionLocal, cameraToPositionLocal / cameraToPositionLocalLength, 1, enter, exit);
  
  // Abort if we did not hit the sphere.
  clip(hit);
  
  // Abort if the sphere hit is "behind" the camera.
  clip(exit);
  
  // sceneDistanceLocal is the linear distance of the scene in local space.
  float sceneDistanceLocal = relativeSceneDistance * cameraToPositionLocalLength;
  
  // Abort if the scene is in front of the sphere. (Pixel is occluded.)
  clip(sceneDistanceLocal - enter);
  
  // Optional: exit should be the minimum of the scene distance and the sphere distance.
  //exit = min(exit, sceneDistanceLocal);
  
  // Make the fog intensity relative to the ray length that is inside the sphere.
  // We use the Alpha parameter to scale the intensity.
  // (Divide by 2 because the diameter of the sphere is 2.)
  float intensity = pow(saturate(Density * (exit - enter) / 2), Falloff);
  
  // Distance between enter and scene in world space.
  float frontIntersectionDistance = (sceneDistanceLocal - enter) / cameraToPositionLocalLength * cameraToPositionLength;
  
  // Soft intersection of sphere front side with scene:
  intensity *= saturate(frontIntersectionDistance / IntersectionSoftness);
  
  return float4(Color * intensity, intensity * BlendMode);
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
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
