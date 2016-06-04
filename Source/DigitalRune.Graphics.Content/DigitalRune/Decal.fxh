//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Decal.fxh
/// Functions for rendering decals.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_DECAL_FXH
#define DIGITALRUNE_DECAL_FXH

#ifndef DIGITALRUNE_COMMON_FXH
#error "Common.fxh required. Please include Common.fxh before including Deferred.fxh."
#endif

#ifndef DIGITALRUNE_DEFERRED_FXH
#error "Deferred.fxh required. Please include Deferred.fxh before including Decal.fxh."
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float DecalAlpha : DECALALPHA = 1.0;
float DecalNormalThreshold : DECALNORMALTHRESHOLD = 0.5;  // = cos(60°)
float3 DecalOrientation : DECALORIENTATION;
bool DecalOptions : DECALOPTIONS;   // true ... project on all


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Performs the deferred decal computations.
/// \param[in,out] positionView      The position in view space.
/// \param[in]     positionProj      The position in projection space.
/// \param[in]     depth             The view-space depth.
/// \param[in]     worldViewInverse  The inverse world-view transformation matrix.
/// \param[in]     viewportSize      The viewport size in pixels.
/// \param[in]     gBuffer0Sampler   The sampler for the depth buffer (GBuffer0).
/// \param[in]     gBuffer1Sampler   The sampler for the normal buffer (GBuffer1).
/// \param[out]    normalWorld       The surface normal in world space.
/// \param[out]    uvDecal           The decal texture coordinates.
/// \param[out]    uvScreen          The screen texture coordinates.
/// \param[out]    uvScreen          The screen texture coordinates.
void DeferredDecal(inout float4 positionView,
                   float4 positionProj,
                   float depth,
                   float4x4 worldViewInverse,
                   float2 viewportSize,
                   sampler gBuffer0Sampler,
                   sampler gBuffer1Sampler,
                   out float3 normalWorld,
                   out float2 uvDecal,
                   out float2 uvScreen)
{
  // Get the screen space texture coordinate for this position.
  uvScreen = ProjectionToScreen(positionProj, viewportSize);
  float4 gBuffer0 = tex2D(gBuffer0Sampler, uvScreen);
  
  // DecalOptions determines the target geometry:
  //   DecalOptions == false ... project on static geometry
  //   DecalOptions == true .... project on all geometry
  // The depth value in the G-buffer is negative, if the geometry is dynamic.
  // Combine this check together with manual depth test.
  float sceneDepth = DecalOptions ? abs(gBuffer0.x) : gBuffer0.x;
  clip(sceneDepth - depth);
  
  // Get correct positive depth value. (Pixels with negative depth values have already
  // been texkilled but the computations must still be executed for correct ddx/ddy
  // and mipmap computations!!!)
  sceneDepth = abs(gBuffer0.x);
  
  // Reconstruct correct 3D position from G-buffer.
  float3 frustumRay = positionView.xyz / depth;
  positionView = float4(frustumRay * sceneDepth, 1);
  
  // Transform 3D position from view space to local space of decal.
  float4 positionLocal = mul(positionView, worldViewInverse);
  
  // Assumption: positionLocal.w == 1;
  
  // Clip pixels outside the decal volume.
  // The decal volume is a unit cube centered (0, 0, -0.5).
  float3 distanceToCenter = abs(positionLocal.xyz - float3(0, 0, -0.5));
  clip(0.5 - distanceToCenter);
  
  // We can read the normal from G-buffer 1 or reconstruct the normal from depth.
  
  // ----- Variant #1: Read normal from G-buffer 1.
  // + Fast and simple.
  // - GBuffer pass overrides normals, Material pass reads different normals.
  
  float4 gBuffer1 = tex2D(gBuffer1Sampler, uvScreen);
  normalWorld = GetGBufferNormal(gBuffer1);
  // -----
  
  // ----- Variant #2: Reconstruct normal from depth.
  // + The normals follows the original geometry. (Better normal for threshold check.)
  // - Artifacts around objects in front of the decal caused by z-discontinuities.
  //
  // Reconstruct view-space normal from depth buffer:
  //   n = normalize(cross(dp/dx, dp/dy))
  //
  // where p is the position in view space,
  // dp/dx is the partial derivative of the p with respect to x,
  // dp/dy is the partial derivative of the p with respect to y.
  //
  // ddx() and ddy() calculate the partial derivatives with respect to screen space
  // coordinates. Note that y-axis points up in view space, but down in screen space!
  
  //normalView = normalize(cross(ddy(positionView.xyz), ddx(positionView.xyz)));
  // TODO: Convert to world space...
  // -----
  
  // ----- Variant #3: Combination of #1 and #2.
  // Read the normal from G-buffer 1 and reconstruct the normal from the depth buffer.
  // Check which normal is closer to the decal orientation. If the reconstructed normal
  // is closer use variant #2; otherwise use variant #1.
  // + Removes artifacts from z-discontinuities.
  
  // Not yet implemented.
  // -----
  
  // Clip pixel if normal deviates from projection direction.
  clip(dot(normalWorld, DecalOrientation) - DecalNormalThreshold);
  
  // Decal texture coordinate.
  uvDecal = float2(positionLocal.x + 0.5, 0.5 - positionLocal.y);
}
#endif
