//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ShadowMap.fxh
/// Useful functions for shadow mapping.
//
// ----- Common function input parameters:
// \param[in] position        The receiver position in any space that works with the shadow matrix.
// \param[in] positionWorld   The receiver position in world space.
// \param[in] positionScreen  The receiver position in screen space (in pixels)
//                            Same as float4 vPos : SV_Position.
// \param[in] texCoord        The shadow map texture coordinates of the current receiver position.
// \param[in] shadowMatrix    Converts the specified receiver position to the light projection space.
// \param[in] shadowMatrices  Converts the specified receiver position to the light projection space.
//                            This is an array with on matrix per CSM cascade.
// \param[in] shadowMap       The shadow map. Can be a texture atlas containing several maps.
// \param[in] shadowMapSize   The shadow map size (whole atlas) in texels (width, height).
// \param[in] shadowMapBounds The bounds of the shadow map inside the texture atlas.
//                            (left, top, right, bottom) in texture coordinates.
//                            (0, 0, 1, 1) if shadow map is not a texture atlas.
// \param[in] currentDepth    The relative depth ([0, 1]) of the current receiver position
//                            in the light space (= distance from the light source).
// \param[in] filterBilinear  'true' to apply bilinear filtering. 'false' to use the
//                            simple average of all samples.
//
// ----- Common function return values:
// All GetShadow functions return:
// \return The shadow factor in the range [0, 1].
//         0 means no shadow. 1 means full shadow.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_SHADOWMAP_FXH
#define DIGITALRUNE_SHADOWMAP_FXH

#ifndef DIGITALRUNE_COMMON_FXH
#error "Common.fxh required. Please include Common.fxh before including ShadowMap.fxh."
#endif

#ifndef DIGITALRUNE_NOISE_FXH
#error "Noise.fxh required. Please include Noise.fxh before including ShadowMap.fxh."
#endif


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

#ifndef MAX_NUMBER_OF_PCF_SAMPLES
#define MAX_NUMBER_OF_PCF_SAMPLES 32
#endif

#define CASCADE_SELECTION_FAST 0
#define CASCADE_SELECTION_BEST 1
#define CASCADE_SELECTION_BEST_DITHERED 2
#define CASCADE_SELECTION_BEST_INTERPOLATED 3

// To clamp texture coordinates to shadow map rectangle to avoid sampling in
// wrong part in CSM texture atlas:
// #define CLAMP_TEXCOORDS_TO_SHADOW_MAP_BOUNDS 1


//-----------------------------------------------------------------------------
// Types
//-----------------------------------------------------------------------------

// TODO: These structs are outdated. - Will be updated once MonoGame supports structs.
// Defines the parameters of a filtered shadow.
struct ShadowParameters
{
  // The near plane distance of the shadow projection.
  float Near;
  
  // The far plane distance of the shadow projection.
  float Far;
  
  // The matrix that tranforms positions into the view space of the shadow source.
  float4x4 View;
  
  // The projection matrix of the shadow.
  float4x4 Projection;
  
  // The depth bias that is applied to the depth values to avoid shadow map aliasing artifacts
  float DepthBias;
  
  // The size of the shadow map in texels, e.g. (1024, 1024).
  float2 ShadowMapSize;
  
  // The scale factor that is applied to the sample offsets used for filtering or jitter sampling.
  float FilterRadius;
  
  // The resolution of the noise relative to the world space (texels per world unit).
  float JitterResolution;
};


// Defines the parameters of a filtered CSM shadow.
struct CascadedShadowParameters
{
  // The number of cascades. Max. 4 cascades are supported.
  int NumberOfCascades;
  
  // The absolute distances where the cascades end.
  // (split 0-1, split 1-2, split 2-3, max distance)
  // If cascades are not used, set the last components to large values.
  float4 Distances;
  
  // The view projection matrix of each cascade.
  float4x4 ViewProjections[4];
  
  // The depth bias that is applied to the depth values to avoid shadow map aliasing artifacts
  float DepthBias;
  
  // The size of the shadow map (whole atlas) in texels, e.g. (4096, 1024).
  float2 ShadowMapSize;
  
  // The scale factor of each cascade that is applied to the sample offsets used for filtering
  // or jitter sampling.
  float FilterRadius;
  
  // The noise resolution of each cascade relative to the world space (texels per world unit).
  float JitterResolution;
  
  // The relative distance in [0, 1] where the shadow starts to fade into the ShadowFog.
  float FadeOutRange;
  
  // The distance where the shadow ends and is replaced by shadow fog.
  float MaxDistance;
  
  // The shadow factor for objects beyond MaxDistance. See also ApplyShadowFog().
  float ShadowFog;
};


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

/// Declares the uniform const for a shadow map texture + sampler.
/// \param[in] name     The name of the shadow map texture constant.
/// \param[in] semantic The effect parameter semantic.
/// \remarks
/// Example: To declare ShadowMap and ShadowMapSampler for a directional light call
///   DECLARE_UNIFORM_SHADOWMAP(ShadowMap, DIRECTIONALLIGHTSHADOWMAP);
#define DECLARE_UNIFORM_SHADOWMAP(name, semantic) \
texture name : semantic; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MinFilter = POINT; \
  MagFilter = POINT; \
  MipFilter = POINT; \
}


/// Declares the uniform const for a shadow mask texture + sampler.
/// \param[in] name   The name of the shadow mask texture constant.
/// \param[in] index  The index of the light buffer.
/// \remarks
/// Example: To declare ShadowMask and ShadowMaskSampler call
///   DECLARE_UNIFORM_SHADOWMASK(ShadowMask);
#define DECLARE_UNIFORM_SHADOWMASK(name) \
texture name; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MinFilter = LINEAR; \
  MagFilter = LINEAR; \
  MipFilter = NONE; \
}


// Cascade colors which can be mixed into the display color to visualize the shadow map cascades
// for debugging.
// Example: float4 color = lerp(shadow.xxxx, CsmDebugCascadeColors[cascade], 0.2);
static const float4 CsmDebugCascadeColors[4] =
{
  float4(1, 0, 0, 1),   // Red
  float4(0, 1, 0, 1),   // Green
  float4(0, 0, 1, 1),   // Blue
  float4(1, 1, 0, 1),   // Yellow
};


//-----------------------------------------------------------------------------
// General Functions
//-----------------------------------------------------------------------------

/// Samples the shadow map.
/// \return The sampled shadow map depth value.
/// \remarks
/// The texture coordinates are clamped to the given shadowMapBounds if following
/// symbol is defined:
/// #define CLAMP_TEXCOORDS_TO_SHADOW_MAP_RECTANGLE 1
float SampleShadowMap(sampler2D shadowMap, float2 texCoord, float2 offset, float4 shadowMapBounds)
{
  texCoord = texCoord + offset;
  
  // Clamp texture coordinates to the allowed rectangle of the texture atlas.
#if CLAMP_TEXCOORDS_TO_SHADOW_MAP_BOUNDS
  texCoord = clamp(texCoord, shadowMapBounds.xy, shadowMapBounds.zw);
#endif
  
  return tex2Dlod(shadowMap, float4(texCoord, 0, 0)).r;
}


/// Computes the texture coordinates in the shadow map for a given position.
/// \return The texture coordinates for the shadow map. z is the projected
///         depth of the position which can be used for directional lights.
float3 GetShadowTexCoord(float4 position, float4x4 shadowMatrix)
{
  // Transform position from world space to the projection space of the light source.
  position = mul(position, shadowMatrix);
  
  // Perspective divide.
  position.xyz /= position.w;
  
  // Convert position from light projection space to light texture space.
  position.xy = float2(0.5, -0.5) * position.xy + float2(0.5, 0.5);
  
  return position.xyz;
}


//-----------------------------------------------------------------------------
// Variance Shadow Maps (VSM)
//-----------------------------------------------------------------------------

/// Computes the depth moments that must be stored in a Variance Shadow Map (VSM).
/// \param[in] depth      The depth of the current pixel.
/// \param[in] applyBias  true if the variance (2nd moment) should consider the depth
///                       gradient of the current pixel. This costs a bit of performance
///                       but helps to remove surface acne. For most cases this is not
///                       necessary and applyBias can be false.
/// \return  The 1st and 2nd moment (average and variance) that must be stored in the
///          Variance Shadow Map.
float2 GetDepthMoments(float depth, bool applyBias)
{
  float2 moments;
  moments.x = depth;
  moments.y = depth * depth;
  
  if (applyBias)
  {
    // Consider the depth change at the current pixel to compute a better
    // variance to fight surface acne.
    float dx = ddx(depth);
    float dy = ddy(depth);
    moments.y = 0.25 * (dx * dx + dy * dy);
  }
  
  return moments;
}


/// Computes an upper bound for the probability that the given depth value is not occluded.
/// \param[in] moments      The first and second moments of the occluder depth (from the VSM).
/// \param[in] depth        The depth of the current pixel.
/// \param[in] minVariance  The minimum limit for the variance. The variance is clamped to
///                         this value.
/// \return The probability that a pixel at the given depth is not occluded.
float GetChebyshevUpperBound(float2 moments, float depth, float minVariance)
{
  // Compute variance.
  float variance = moments.y - moments.x * moments.x;
  variance = max(variance, minVariance + 0.0001f);
  
  // Compute upper bound for the probability.
  float delta = (depth - moments.x);
  float pMax = variance / (variance + delta * delta);
  
  // The inequality is only valid if depth >= moments.x.
  // If depth is <= moments.x the probability is p = 1 (100%).
  float p = (depth <= moments.x);
  return max(p, pMax);
}


//-----------------------------------------------------------------------------
// Normal Offset
//-----------------------------------------------------------------------------

/// Applies a slope-scaled normal offset to the specified position.
/// \param[in] position       The position.
/// \param[in] normal         The normal vector.
/// \param[in] light          The light vector (or light direction, sign does not matter).
/// \param[in] normalOffset   The max normal offset.
/// \return The position offset along the normal direction.
float3 ApplyNormalOffset(float3 position, float3 normal, float3 light, float normalOffset)
{
  float cosLight = abs(dot(normal, light));
  float slopeScale = saturate(1 - cosLight);
  return position + normal * slopeScale * normalOffset;
}


//-----------------------------------------------------------------------------
// Receiver Plane Depth Bias
//-----------------------------------------------------------------------------

/// Gets the receiver plane depth bias.
/// \param[in] duvdist_dx   Derivatives of u, v, and distance to light source w.r.t. to screen space x.
/// \param[in] duvdist_dy   Derivatives of u, v, and distance to light source w.r.t. to screen space y.
/// \return The receiver plane depth bias.
/// \remarks
/// The receiver plane depth bias is the derivative of the depth function (= distance to light source)
/// with respect to the uv texture coordinates.
float2 GetReceiverPlaneDepthBias(float3 duvdist_dx, float3 duvdist_dy)
{
  // duvdist_dx = (du/dx, dv/dx, ddist/dx)
  // duvdist_dy = (du/dy, dv/dy, ddist/dy)
  
  // Receiver plane depth bias.
  float2 ddist_duv; // (ddist/du, ddist/dx)
  
  // Invert texture space Jacobian and use chain rule to compute ddist/du and ddist/dv.
  //  |ddist/du| = |du/dx du/dy|-T * |ddist/dx|
  //  |ddist/dv|   |dv/dx dv/dy|     |ddist/dy|
  
  // Multiply ddist/dx and ddist/dy by inverse transpose of Jacobian.
  float invDet = 1.0 / ((duvdist_dx.x * duvdist_dy.y) - (duvdist_dx.y * duvdist_dy.x));
  
  // Top row of 2x2.
  ddist_duv.x = duvdist_dy.y * duvdist_dx.z;   // invJtrans[0][0] * ddist_dx
  ddist_duv.x -= duvdist_dx.y * duvdist_dy.z;  // invJtrans[0][1] * ddist_dy
  
  // Bottom row of 2x2.
  ddist_duv.y = duvdist_dx.x * duvdist_dy.z;   // invJtrans[1][1] * ddist_dy
  ddist_duv.y -= duvdist_dy.x * duvdist_dx.z;  // invJtrans[1][0] * ddist_dx
  ddist_duv *= invDet;
  
  // Clamp to minimize flickering triangles. Those are probably created when
  // triangles are steep in light space (e.g. a triangle is only a line segment
  // when viewed from the camera.
  //ddist_duv = clamp(ddist_duv, -10, 10);
  
  return ddist_duv;
}


/// Samples the shadow map and applies receiver plane depth bias.
float SampleShadowMap(sampler2D shadowMap, float2 texCoord, float2 offset, float4 shadowMapBounds, float2 ddist_duv)
{
  float d = SampleShadowMap(shadowMap, texCoord, offset, shadowMapBounds);
  d -= dot(offset, ddist_duv);  // Apply receiver plane depth offset.
  return d;
}


// Example with receiver plane depth bias:
//float ComputeShadowFactorPcfWithReceiverPlaneDepthBias(...)
//{
//  // Receiver plane depth bias.
//  float3 duvdist_dx = ddx(float3(texCoord.x, texCoord.y, currentDepth)); // Note: Use ddx_fine() in Direct3D 11.
//  float3 duvdist_dy = ddy(float3(texCoord.x, texCoord.y, currentDepth));
//  float2 ddist_duv = float2(0, 0);
//  ddist_duv = GetReceiverPlaneDepthBias(duvdist_dx, duvdist_dy);

//  // Apply static depth biasing to make up for incorrect fractional sampling on the shadow map grid.
//  float2 texelSize = 1.0 / shadowMapSize;
//  float fractionalSamplingError = dot(1 * texelSize, abs(ddist_duv));
//  currentDepth -= min(fractionalSamplingError, 0.01);

//  // Get offsets.
//  float2 offset0 = ...;
//  float4 samples;
//  samples.x = SampleShadowMap(shadowMap, texCoord, offset0, shadowMapBounds, ddist_duv);  // <-- ddist_duv
//  ...
//  samples = samples < currentDepth;
//  return saturate(dot(samples, 1) / 4.0f);
//}


//-----------------------------------------------------------------------------
// Shadow Map Filtering
//-----------------------------------------------------------------------------

/// Determines whether a position is in the shadow using a single sample.
/// \remarks
/// This is the simplest GetShadow function. The result is either 0 or 1 - which
/// creates hard-edged, aliased shadows.
float GetShadow(float currentDepth, float2 texCoord, sampler2D shadowMap, float2 shadowMapSize, float4 shadowMapBounds)
{
  float occluderDepth = SampleShadowMap(shadowMap, texCoord, float2(0, 0), shadowMapBounds);
  return occluderDepth < currentDepth;
}


/// Determines whether a position is in the shadow using Percentage Closer Filtering (PCF).
/// \param[in] offsets  The offsets in texels relative to texCoord where the shadow
///                     map should be sampled.
/// \param[in] scale    A scale factor that is applied to the offsets.
/// \remarks
/// Several shadow map positions are sampled and the average of the 0/1 results
/// is returned.
float GetShadowPcf(float currentDepth,
                   float2 texCoord, float2 offsets[MAX_NUMBER_OF_PCF_SAMPLES], 
                   int numberOfSamples, float2 scale, 
                   sampler2D shadowMap, float2 shadowMapSize, float4 shadowMapBounds)
{
  float shadow = 0;
  [unroll]
  for (int i = 0; i < numberOfSamples; i++)
  {
    float2 offset = offsets[i] * scale / shadowMapSize;
    float occluderDepth = SampleShadowMap(shadowMap, texCoord, offset, shadowMapBounds);
    if (occluderDepth < currentDepth)
      shadow++;
  }
  
  shadow /= numberOfSamples;
  return shadow;
}


/// Determines whether a position is in the shadow using 2x2 PCF.
/// \remarks
/// 2x2 pixels around texCoord are sampled. The results are either averaged or
/// bilinearly filtered to create smooth edges.
float GetShadowPcf2x2(float currentDepth, float2 texCoord, bool filterBilinear,
                      sampler2D shadowMap, float2 shadowMapSize, float4 shadowMapBounds)
{
  float2x2 samples;
  float x = 0.5 / shadowMapSize.x;   // Half texel offset.
  float y = 0.5 / shadowMapSize.y;   // Half texel offset.
  samples._m00 = SampleShadowMap(shadowMap, texCoord, float2(-x, -y), shadowMapBounds);
  samples._m01 = SampleShadowMap(shadowMap, texCoord, float2( x, -y), shadowMapBounds);
  samples._m10 = SampleShadowMap(shadowMap, texCoord, float2(-x,  y), shadowMapBounds);
  samples._m11 = SampleShadowMap(shadowMap, texCoord, float2( x,  y), shadowMapBounds);
  samples = (samples <= currentDepth);
  
  float shadow = 0.0;
  if (filterBilinear)
  {
    // Compute lerp weight.
    // Note: Have to offset by 0.5 because of the sampling pattern.
    float2 weight = frac(texCoord * shadowMapSize - float2(0.5, 0.5));
    // Lerp rows and store in first row.
    samples[0] = lerp(samples[0], samples[1], weight.y);
    // Lerp columns of first row.
    shadow = lerp(samples._m00, samples._m01, weight.x);
  }
  else
  {
    // No filtering: Sum up elements and divide by the number of elements.
    shadow = dot(float4(samples), 1.0 / 4.0);
  }
  
  return shadow;
}


/// Determines whether a position is in the shadow using 3x3 PCF.
/// \remarks
/// 3x3 pixels around texCoord are sampled. The results are either averaged or
/// bilinearly filtered to create smooth edges.
float GetShadowPcf3x3(float currentDepth, float2 texCoord, bool filterBilinear,
                      sampler2D shadowMap, float2 shadowMapSize, float4 shadowMapBounds)
{
  float3x3 samples;
  float x = 1.0 / shadowMapSize.x;   // one texel offset.
  float y = 1.0 / shadowMapSize.y;   // one texel offset.
  samples._m00 = SampleShadowMap(shadowMap, texCoord, float2(-x, -y), shadowMapBounds);
  samples._m01 = SampleShadowMap(shadowMap, texCoord, float2( 0, -y), shadowMapBounds);
  samples._m02 = SampleShadowMap(shadowMap, texCoord, float2( x, -y), shadowMapBounds);
  samples._m10 = SampleShadowMap(shadowMap, texCoord, float2(-x,  0), shadowMapBounds);
  samples._m11 = SampleShadowMap(shadowMap, texCoord, float2( 0,  0), shadowMapBounds);
  samples._m12 = SampleShadowMap(shadowMap, texCoord, float2( x,  0), shadowMapBounds);
  samples._m20 = SampleShadowMap(shadowMap, texCoord, float2(-x,  y), shadowMapBounds);
  samples._m21 = SampleShadowMap(shadowMap, texCoord, float2( 0,  y), shadowMapBounds);
  samples._m22 = SampleShadowMap(shadowMap, texCoord, float2( x,  y), shadowMapBounds);
  samples = (samples <= currentDepth);
  
  float shadow = 0.0;
  if (filterBilinear)
  {
    // Compute lerp weight.
    float2 weight = frac(texCoord * shadowMapSize);
    
    // Filtering 3x3 samples to 2x2 samples where the result is always stored in the upper
    // left part of the samples matrix.
    // Note: See article in "ShaderX2 - Intro & Tutorial" book.
    samples._m00_m10_m20 = lerp(samples._m00_m10_m20, samples._m01_m11_m21, weight.x);
    samples._m01_m11_m21 = lerp(samples._m01_m11_m21, samples._m02_m12_m22, weight.x);
    samples._m00_m01 = lerp(samples._m00_m01, samples._m10_m11, weight.y);
    samples._m10_m11 = lerp(samples._m10_m11, samples._m20_m21, weight.y);
    
    // Now simple compute the average of the 2x2 samples.
    shadow = (samples._m00 + samples._m01 + samples._m10 + samples._m11) / 4;
  }
  else
  {
    // No filtering: Sum up elements and divide by the number of elements.
    shadow = (samples._m00 + samples._m01 + samples._m02
              + samples._m10 + samples._m11 + samples._m12
              + samples._m20 + samples._m21 + samples._m22) / 9;
  }
  
  return shadow;
}


/// Determines whether a position is in the shadow using Variance Shadow Mapping (VSM).
/// \param[in] minVariance    The min limit to which the variance is clamped. A min value
///                           helps to reduce aliasing.
/// \param[in] lightBleedingReduction   A value in the range [0, 1] that determines
///                           how much shadows will be darkened to reduce light bleeding
///                           artifacts. 0 means no darkening.
float GetShadowVsm(float currentDepth, float2 texCoord,
                   sampler2D shadowMap, float minVariance,
                   float lightBleedingReduction)
{
  float2 moments = tex2D(shadowMap, texCoord).rg;
  
  // Get probability that this pixel is lit.
  float p = GetChebyshevUpperBound(moments, currentDepth, minVariance);
  
  // To reduce light bleeding cut off values below lightBleedingReduction.
  // Rescale the rest to [0, 1].
  p = LinearStep(lightBleedingReduction, 1, p);
  
  return 1 - p;
}


/// Determines whether a position is in the shadow using Exponential Shadow Mapping (ESM).
/// \param[in] depthScale     The depth scale which was applied to the shadow map depth.
/// \param[in] overDarkening  A factor > 1 (e.g. 100) that determines how much shadows
///                           will be darkened
/// \remarks
/// This method assumes that the shadow map stores depthScale * depth and all
/// filtering is done in log-space.
float GetShadowEsm(float currentDepth, float depthScale,
                   float2 texCoord, sampler2D shadowMap,
                   float overDarkening)
{
  float occluderDepth = tex2D(shadowMap, texCoord).r;
  
  // Get probability that this pixel is lit.
  float p = clamp(exp(overDarkening * (occluderDepth - depthScale * currentDepth)), 0, 1);
  
  return 1 - p;
}


//float ComputeShadowFactorEsmBilinear(float currentDepth,
//                                     float depthScale,
//                                     float2 texCoord,
//                                     sampler2D shadowMap,
//                                     float2 shadowMapSize,
//                                     float overDarkening)
//{
//  float2x2 samples;
//  samples._m00 = tex2D(shadowMap, texCoord + float2(-0.5, -0.5) / shadowMapSize).r;
//  samples._m01 = tex2D(shadowMap, texCoord + float2( 0.5, -0.5) / shadowMapSize).r;
//  samples._m10 = tex2D(shadowMap, texCoord + float2(-0.5,  0.5) / shadowMapSize).r;
//  samples._m11 = tex2D(shadowMap, texCoord + float2( 0.5,  0.5) / shadowMapSize).r;

//  // Compute lerp weight.
//  float2 weight = frac(texCoord * shadowMapSize - float2(0.5, 0.5));
//  samples[0] = lerp(samples[0], samples[1], weight.y);
//  float occluderDepth = lerp(samples._m00, samples._m01, weight.x);

//  // Get probability that this pixel is lit.
//  float p = clamp(exp(overDarkening * (occluderDepth - depthScale * currentDepth)), 0, 1);

//  // Note: Alternatively we could compute the probability for each sample and
//  // lerp the probabilities instead of the samples. - kinda like PCF - but
//  // lerping the depths is faster and usually looks better.

//  return 1 - p;
//}


/// Determines whether a position is in the shadow using jittered PCF.
/// \param[in] jitterMap      The texture map with random values in the range [0, 1].
/// \param[in] jitterMapSize  The size of the jitterMap in in texels.
/// \param[in] filterRadius   A scale factor that is applied to the random offsets.
/// \param[in] hideNoisePatterns  'true' if the occurance of patterns in the noise should
///                           be reduced by taking more random values from the jitterMap.
/// \remarks
/// This function uses Percentage Closer Filtering (PCF) at random positions
/// around texCoord. To get random values a precomputed random value texture map
/// (jitterMap) is used.
/// The random positions are relative to screen space, which means that the
/// noise is "fixed" to the screen - not to the world space positions of the
/// shadows.
float GetShadowPcfJitteredScreen(float2 positionScreen, float currentDepth, float2 texCoord,
                                 sampler2D shadowMap, float2 shadowMapSize,
                                 sampler2D jitterMap, float jitterMapSize, float filterRadius,
                                 bool hideNoisePatterns, float4 shadowMapBounds)
{
  // Compute jitter map texture coordinates from the current screen position.
  float2 jitterMapCoords = positionScreen / jitterMapSize;
  
  // We can sample the jitter map again to avoid visible blocks of noise.
  if (hideNoisePatterns)
    jitterMapCoords += tex2Dlod(jitterMap, float4(positionScreen / jitterMapSize / jitterMapSize, 0, 0)).xy;
  
  // Get a random offset.
  float2 offset = tex2Dlod(jitterMap, float4(jitterMapCoords, 0, 0)).xy * filterRadius / shadowMapSize;
  
  // Use the random offset and sample in 4 orthogonal directions.
  float4 samples;
  samples.x = SampleShadowMap(shadowMap, texCoord, float2(offset.x, offset.y), shadowMapBounds);
  samples.y = SampleShadowMap(shadowMap, texCoord, float2(-offset.x, -offset.y), shadowMapBounds);
  samples.z = SampleShadowMap(shadowMap, texCoord, float2(-offset.y, offset.x), shadowMapBounds);
  samples.w = SampleShadowMap(shadowMap, texCoord, float2(offset.y, -offset.x), shadowMapBounds);
  samples = samples < currentDepth;
  
  return dot(samples, 1.0 / 4.0);
}


/// Determines whether a position is in the shadow using jittered PCF.
/// \param[in] jitterMap          The texture map with random values in R and G in the range [0, 1].
/// \param[in] filterRadius       A scale factor that is applied to the random offsets.
/// \param[in] jitterResolution   Determines the resolution of the noise relative to
///                               the world space. Example value: 50.
/// \remarks
/// This function uses Percentage Closer Filtering (PCF) at random positions
/// around texCoord. To get random values a precomputed random value texture map
/// (jitterMap) is used.
/// The random positions are relative to world space, which means that the
/// noise is "fixed" in the world. This will create more stable noise than
/// screen space noise in GetShadowPcfJitteredScreen().
float GetShadowPcfJitteredWorld(
  float3 positionWorld, float currentDepth, float2 texCoord, sampler2D shadowMap, 
  float2 shadowMapSize, sampler2D jitterMap, float filterRadius, float jitterResolution,
  float4 shadowMapBounds)
{
  // Compute jitter map texture coordinates from the current world space position.
  // Note: When combining x/y/z this way, there will be an angle where one component
  // of jitterMapCoords will be constant. This will create streaks of noise instead
  // of dots.
  float2 jitterMapCoords;
  jitterMapCoords.x = positionWorld.x * jitterResolution - positionWorld.z * jitterResolution;
  jitterMapCoords.y = positionWorld.y * jitterResolution + positionWorld.z * jitterResolution;
  
  // Get a random offset. (We duplicate the offset to handle non-square shadow maps.)
  float4 offset = tex2Dlod(jitterMap, float4(jitterMapCoords, 0, 0)).xyxy * filterRadius.xxxx / shadowMapSize.xxyy;
  
  // Use the random offset and sample in 4 orthogonal directions.
  float4 samples;
  samples.x = SampleShadowMap(shadowMap, texCoord, float2(offset.x, offset.w), shadowMapBounds);
  samples.y = SampleShadowMap(shadowMap, texCoord, float2(-offset.x, -offset.w), shadowMapBounds);
  samples.z = SampleShadowMap(shadowMap, texCoord, float2(-offset.y, offset.z), shadowMapBounds);
  samples.w = SampleShadowMap(shadowMap, texCoord, float2(offset.y, -offset.z), shadowMapBounds);
  samples = samples < currentDepth;
  
  return dot(samples, 1.0 / 4.0);
}


/// Determines whether a position is in the shadow using jittered PCF.
/// \param[in] samples            The sample offsets and weights (offset x, offset y, weight).
/// \param[in] jitterMap          The texture map with random values in R and G in the range [0, 1].
/// \param[in] filterRadius       A scale factor that is applied to the random offsets.
/// \param[in] jitterResolution   Determines the resolution of the noise relative to
///                               the world space. Example value: 50.
/// \remarks
/// This function uses Percentage Closer Filtering (PCF) at random positions
/// around texCoord. To get random values a precomputed random value texture map
/// (jitterMap) is used.
/// The random positions are relative to world space, which means that the
/// noise is "fixed" in the world. This will create more stable noise than
/// screen space noise in GetShadowPcfJitteredScreen().
float GetShadowPcfJitteredWorld(
  float3 positionWorld, float currentDepth, float2 texCoord, float3 samples[MAX_NUMBER_OF_PCF_SAMPLES],
  sampler2D shadowMap, float2 shadowMapSize, sampler2D jitterMap, float filterRadius, 
  float jitterResolution, float4 shadowMapBounds, int numberOfSamples)
{
  // Compute jitter map texture coordinates from the current world space position.
  // Note: When combining x/y/z this way, there will be an angle where one component
  // of jitterMapCoords will be constant. This will create streaks of noise instead
  // of dots.
  float2 jitterMapCoords;
  jitterMapCoords.x = positionWorld.x * jitterResolution - positionWorld.z * jitterResolution;
  jitterMapCoords.y = positionWorld.y * jitterResolution + positionWorld.z * jitterResolution;
  
  // Get random vector.
  float2 randomVec = tex2Dlod(jitterMap, float4(jitterMapCoords, 0, 0)).xy;
  
  // Remove scale. Poisson taps are already scaled and random enough. An additional scale
  // creates more noise (single dots on the outside, less smooth).
  randomVec = normalize(randomVec);   // TODO: Use normalized jitterMap to avoid this step.
  
  float result = 0;
  [unroll]
  for (int i = 0; i < numberOfSamples; i++)
  {
    float2 offset = samples[i].xy;
    float weight = samples[i].z;
      
    // Apply random rotation.
    offset = reflect(offset, randomVec);
    
    offset *=  filterRadius.xx / shadowMapSize.xy;
    
    float sample = SampleShadowMap(shadowMap, texCoord, offset, shadowMapBounds);
    
    result += (sample < currentDepth) * weight;
  }
  return result;
}


/// Determines whether a position is in the shadow using a shadow cubemap of
/// an omni-directional light.
/// \param[in] currentDepth     The depth of the current position in the light space
///                             (= distance from the light source).
/// \param[in] lightDirection   The direction from the light to the current position.
///                             (Can be unnormalized.)
/// \param[in] shadowMap        The shadow map.
/// \return  The shadow factor in the range [0, 1].
///          0 means no shadow. 1 means full shadow.
/// \remarks
/// This is the simplest GetShadowCube function. The result is either 0
/// or 1 - which creates hard-edges, aliased shadows :-(.
float GetShadowCube(float currentDepth, float3 lightDirection, samplerCUBE shadowMap)
{
  lightDirection.z = -lightDirection.z;      // Cube maps are left handed -> switch z.
  float occluderDepth = texCUBElod(shadowMap, float4(lightDirection, 0)).r;
  return occluderDepth < currentDepth;
}


/// Determines whether a position is in the shadow using a shadow cubemap of
/// an omni-directional light.
/// \param[in] currentDepth     The depth of the current position in the light space
///                             (= distance from the light source).
/// \param[in] lightDirection   The direction from the light to the current position.
///                             (Must be normalized!)
/// \param[in] scale            A scale factor that is applied to the PCF sample offsets.
/// \param[in] filterBilinear   'true' to apply bilinear filtering. 'false' to use the
///                             simple average of all PCF samples.
/// \param[in] shadowMap        The shadow map.
/// \param[in] shadowMapSize    The size of the shadow map in texels (width and height).
/// \return The shadow factor in the range [0, 1].
///         0 means no shadow. 1 means full shadow.
/// \remarks
/// The function uses Percentage Closer Filteringfor cube maps. The PCF results
/// are either averaged or bilinear filtering is applied to create smooth edges.
float GetShadowCubePcf(float currentDepth, float3 lightDirection, float scale,
                       bool filterBilinear, samplerCUBE shadowMap, float shadowMapSize)
{
  lightDirection.z = -lightDirection.z;
  
  // Approximate bilinear cube map filtering:
  // See ShaderX2 - Floating-Point Cube Maps, pp.319
  
  // If lightDirection is normalized:
  lightDirection *= shadowMapSize;
  
  // If lightDirection is not normalized, this can be used:
  // Example: If z is the maxElement and we divide by this value, then
  // x/y are in the range [-1, 1] of the +z face. By multiplying with the half
  // resolution x/y are in the range [-ShadowMapSize/2, +ShadowMapSize/2].
  // Now, we have the right size and can get the neighbor pixels by adding 1.
  //float maxElement = max(abs(lightDirection.x), max(abs(lightDirection.y), abs(lightDirection.z)));
  //lightDirection = lightDirection / maxElement * ShadowMapSize.x / 2;
  
  // To get larger filter offsets, we can divide lightDirection by a value > 1.
  lightDirection /= scale;
  
  float3 weights = frac(lightDirection + 0.5f);
  lightDirection = floor(lightDirection + 0.5f);
  
  float4 occluders0, occluders1;
  occluders0.x = texCUBE(shadowMap, lightDirection + float3(0, 0, 0)).r;
  occluders0.y = texCUBE(shadowMap, lightDirection + float3(0, 0, 1)).r;
  occluders0.z = texCUBE(shadowMap, lightDirection + float3(0, 1, 0)).r;
  occluders0.w = texCUBE(shadowMap, lightDirection + float3(0, 1, 1)).r;
  occluders1.x = texCUBE(shadowMap, lightDirection + float3(1, 0, 0)).r;
  occluders1.y = texCUBE(shadowMap, lightDirection + float3(1, 0, 1)).r;
  occluders1.z = texCUBE(shadowMap, lightDirection + float3(1, 1, 0)).r;
  occluders1.w = texCUBE(shadowMap, lightDirection + float3(1, 1, 1)).r;
  occluders0 = currentDepth > occluders0;
  occluders1 = currentDepth > occluders1;
  
  // Average filter.
  float shadow;
  if (!filterBilinear)
  {
    shadow = dot(occluders0 + occluders1, 1.0 / 8.0);
  }
  else
  {
    // Bilinear filtering.
    occluders0 = lerp(occluders0, occluders1, weights.x);
    occluders0.xy = lerp(occluders0.xy, occluders0.zw, weights.y);
    occluders0.x = lerp(occluders0.x, occluders0.y, weights.z);
    shadow = occluders0.x;
  }
  
  return shadow;
}


/// Determines whether a position is in the shadow using Variance Shadow Mapping
/// (VSM) with a shadow cubemap of an omni-directional light.
/// \param[in] currentDepth   The depth of the current position in the light space
///                           (= distance from the light source).
/// \param[in] lightDirection The direction from the light to the current position.
///                           (Can be unnormalized.)
/// \param[in] shadowMap      The shadow map.
/// \param[in] minVariance    The min limit to which the variance is clamped. A min value
///                           helps to reduce aliasing.
/// \param[in] lightBleedingReduction   A value in the range [0, 1] that determines
///                           how much shadows will be darkened to reduce light bleeding
///                           artifacts. 0 means no darkening.
/// \return The shadow factor in the range [0, 1].
///         0 means no shadow. 1 means full shadow.
float GetShadowCubeVsm(float currentDepth, float3 lightDirection,
                       samplerCUBE shadowMap, float minVariance,
                       float lightBleedingReduction)
{
  lightDirection.z = -lightDirection.z;
  
  float2 moments = texCUBE(shadowMap, lightDirection).rg;
  
  // Get probability that this pixel is lit.
  float p = GetChebyshevUpperBound(moments, currentDepth, minVariance);
  
  // To reduce light bleeding cut of values below lightBleedingReduction.
  // Rescale the rest to [0, 1].
  p = LinearStep(lightBleedingReduction, 1, p);
  
  return 1 - p;
}


/// Determines whether a position is in the shadow using Exponential Shadow Mapping
/// (ESM) with a shadow cubemap of an omni-directional light.
/// \param[in] currentDepth   The depth of the current position in the light space
///                           (= distance from the light source).
/// \param[in] depthScale     The depth scale which was applied to the shadow map depth.
/// \param[in] lightDirection The direction from the light to the current position.
/// \param[in] shadowMap      The shadow map.
/// \param[in] overDarkening  A factor in the range > 1 (e.g. 100) that determines
///                           how much shadows will be darkened
/// \return The shadow factor in the range [0, 1].
///  0 means no shadow. 1 means full shadow.
float GetShadowCubeEsm(float currentDepth,
                       float depthScale,
                       float3 lightDirection,
                       samplerCUBE shadowMap,
                       float overDarkening)
{
  lightDirection.z = -lightDirection.z;
  
  currentDepth *= depthScale;
  
  float occluderDepth = texCUBE(shadowMap, lightDirection).r;
  
  // Get probability that this pixel is lit.
  float p = clamp(exp(overDarkening * (occluderDepth - currentDepth)), 0, 1);
  
  return 1 - p;
}


/// Determines whether a position is in the shadow using a shadow cubemap of
/// an omni-directional light.
/// \param[in] positionWorld    The current position in world space.
/// \param[in] currentDepth     The depth of the current position in the light space
///                             (= distance from the light source).
/// \param[in] lightDirection   The direction from the light to the current position.
///                             (Must be normalized!)
/// \param[in] shadowMap        The shadow map.
/// \param[in] shadowMapSize    The size of the shadow map in texels (width and height).
/// \param[in] jitterMap        The texture map with random values in the range [0, 1].
/// \param[in] filterRadius     A scale factor that is applied to the random offsets.
/// \param[in] jitterResolution Determines the resolution of the noise relative to
///                             the world space. Example value: 50.
/// \return The shadow factor in the range [0, 1].
///         0 means no shadow. 1 means full shadow.
/// The function uses Percentage Closer Filtering (PCF) for cube maps. The PCF
/// samples are taken in the light direction with random perurbations. To get
/// random values a precomputed random value texture map (jitterMap) is used.
/// The random values are relative to world space, which means that the noise
/// is "fixed" in the world.
float GetShadowCubePcfJitteredWorld(float3 positionWorld, float currentDepth, float3 lightDirection,
                                    samplerCUBE shadowMap, float shadowMapSize,
                                    sampler2D jitterMap, float filterRadius, float jitterResolution)
{
  lightDirection.z = -lightDirection.z;
  
  // Approximate bilinear cube map filtering:
  // See ShaderX2 - Floating-Point Cube Maps, pp. 319
  // If lightDirection is normalized.
  lightDirection *= shadowMapSize.x;
  
  // If lightDirection is not normalized, this can be used:
  // Example: If z is the maxElement and we divide by this value, then
  // x/y are in the range [-1, 1] of the +z face. By multiplying with the half
  // resolution x/y are in the range [-ShadowMapSize/2, +ShadowMapSize/2].
  // Now, we have the right size and can get the neighbor pixels by adding 1.
  //float maxElement = max(abs(lightDirection.x), max(abs(lightDirection.y), abs(lightDirection.z)));
  //lightDirection = lightDirection / maxElement * ShadowMapSize.x / 2;
  
  // Get a random offset.
  float2 jitterMapCoords;
  jitterMapCoords.x = positionWorld.x * jitterResolution - positionWorld.z * jitterResolution;
  jitterMapCoords.y = positionWorld.y * jitterResolution + positionWorld.z * jitterResolution;
  float3 offset = tex2Dlod(jitterMap, float4(jitterMapCoords, 0, 0)).xyz * filterRadius;
  
  // Now use the offset to create 6 directional offsets in the directions
  // forward, backward, left, right, up, down.
  float4 occluders0 = 0;
  float2 occluders1 = 0;
  float3 offsets[8];
  offsets[0] = offset;
  offsets[1] = -offsets[0];
  offsets[2] = normalize(cross(offsets[0], lightDirection)) * length(offset);
  offsets[3] = -offsets[2];
  offsets[4] = normalize(cross(offsets[0], offsets[1])) * length(offset);
  offsets[5] = -offsets[4];
  occluders0.x = texCUBElod(shadowMap, float4(lightDirection + offsets[0], 0)).r;
  occluders0.y = texCUBElod(shadowMap, float4(lightDirection + offsets[1], 0)).r;
  occluders0.z = texCUBElod(shadowMap, float4(lightDirection + offsets[2], 0)).r;
  occluders0.w = texCUBElod(shadowMap, float4(lightDirection + offsets[3], 0)).r;
  occluders1.x = texCUBElod(shadowMap, float4(lightDirection + offsets[4], 0)).r;
  occluders1.y = texCUBElod(shadowMap, float4(lightDirection + offsets[5], 0)).r;
  occluders0 = currentDepth > occluders0;
  occluders1 = currentDepth > occluders1;
  
  return 1.0 / 6.0 * (occluders0.x + occluders0.y + occluders0.z + occluders0.w + occluders1.x + occluders1.y);
}


float GetShadowCubePcfJitteredWorld(
  float3 positionWorld, float currentDepth, float3 lightDirection,
  float3 samples[MAX_NUMBER_OF_PCF_SAMPLES], samplerCUBE shadowMap, float shadowMapSize,
  sampler2D jitterMap, float filterRadius, float jitterResolution, int numberOfSamples)
{
  lightDirection.z = -lightDirection.z;
  
  // Get two orthonormal vectors. Light direction must be normalized.
  float3 normal0, normal1;
  GetOrthonormals(lightDirection, normal0, normal1);
  
  // Approximate bilinear cube map filtering:
  // See ShaderX2 - Floating-Point Cube Maps, pp. 319
  // If lightDirection is normalized.
  lightDirection *= shadowMapSize.x;
  
  float2 jitterMapCoords;
  jitterMapCoords.x = positionWorld.x * jitterResolution - positionWorld.z * jitterResolution;
  jitterMapCoords.y = positionWorld.y * jitterResolution + positionWorld.z * jitterResolution;
  
  // Get random vector.
  float2 randomVec = tex2Dlod(jitterMap, float4(jitterMapCoords, 0, 0)).xy;
  
  // Remove scale. Poisson taps are already scaled and random enough. An additional scale
  // creates more noise (single dots on the outside, less smooth).
  randomVec = normalize(randomVec);   // TODO: Use normalized jitterMap to avoid this step.
  
  float result = 0;
  [unroll]
  for (int i = 0; i < numberOfSamples; i++)
  {
    float2 offset = samples[i].xy;
    float weight = samples[i].z;
    
    // Apply random rotation.
    offset = reflect(offset, randomVec);
    
    offset *= filterRadius;
    
    float sample = texCUBElod(shadowMap, float4(lightDirection + normal0 * offset.x + normal1 * offset.y, 0)).r;
    result += (sample < currentDepth) * weight;
  }
  return result;
}


//-----------------------------------------------------------------------------
// CSM Functions
//-----------------------------------------------------------------------------

/// Converts texture coordinates of a single texture to the texture coordinates
/// for texture atlas containing several textures with equal size (horizontal layout).
/// \param[in] texCoord           The texture coordinates [0, 1].
/// \param[in] index              The texture index in the texture atlas.
/// \param[in] numberOfTextures   The number of textures in the atlas.
/// \return The correct texture coordinates to sample the texture atlas.
/// \remarks
/// The texture atlas consists of several textures in a horizontal row.
/// All 4 textures have the same size.
float2 ConvertToTextureAtlas(float2 texCoord, int index, int numberOfTextures)
{
  texCoord.x = ((float)index + texCoord.x) / (float)numberOfTextures;
  return texCoord;
}


/// Gets the bounds of a shadow cascade in the texture atlas.
/// \param[in] cascade            The cascade index.
/// \param[in] numberOfCascades   The number of cascades.
/// \param[in] shadowMapSize      The shadow map size (whole atlas) in texels (width, height).
/// \return The bounds (left, top, right, bottom) of the shadow cascade in texture atlas.
float4 GetShadowMapBounds(int cascade, int numberOfCascades, float2 shadowMapSize)
{
  float4 shadowMapBounds = float4(0, 0, 1, 1);
  shadowMapBounds.x = cascade / (float)numberOfCascades + 0.5 / shadowMapSize.x;
  shadowMapBounds.z = (cascade + 1) / (float)numberOfCascades - 0.5 / shadowMapSize.x;
  return shadowMapBounds;
}


/// Applies "shadow fog".
/// \param[in] cascadeSelection   The cascade selection mode (see CASCADE_SELECTION constants).
/// \param[in] isLastCascade      true if in last cascade or beyond.
/// \param[in] shadow       The shadow factor to which the "shadow fog" is applied.
/// \param[in] shadowTexCoord     The shadow map texture coordinates.
/// \param[in] currentDistance    The distance of the current position to the camera.
/// \param[in] maxDistance        The max distance of the shadows.
/// \param[in] fadeOutRange       The relative fade out range in [0, 1].
/// \param[in] fogShadowFactor    The shadow factor for objects beyond fogEnd.
/// \return The shadow factor with applied "shadow fog".
/// \remarks
/// Shadow fog has the effect that all pixel beyond are shadowed using a
/// constant shadow factor. Below fogStart shadows are drawn normally.
/// Between fogStart and fogEnd the lit and shadowed parts fade into the
/// shadow fog.
float ApplyShadowFog(int cascadeSelection,
                     float isLastCascade,
                     float shadow,
                     float3 shadowTexCoord,
                     float currentDistance,
                     float maxDistance,
                     float fadeOutRange,
                     float fogShadowFactor)
{
  // Fade out using only planar distance.
  // f = 0 ... outside shadow (in fog)
  // f = 1 ... in shadow (no fog).
  float f = saturate((maxDistance - currentDistance) / (maxDistance * fadeOutRange));
  
  // Combine fade out near texture borders.
  if (cascadeSelection >= CASCADE_SELECTION_BEST)
  {
    // Convert coords to [-1, 1] range and take abs().
    float3 t = abs(shadowTexCoord * 2 - 1);
    // s = 0 ... outside shadow (in fog)
    // s = 1 ... in shadow (no fog).
    float s = (1 - max(max(t.x, t.y), t.z)) / (2.0 * fadeOutRange);
    
    // Note: saturate(s) does not work here. It will be removed by the compiler!
    
    // Use max of both fade out values so that we never fade out near the camera.
    // isLastCascade = false ... in shadow (no fog)
    // isLastCascade = true .... max(s, f)
    f = max(1 - isLastCascade * saturate(1 - s), f);
  }
  
  return lerp(fogShadowFactor, shadow, f);
}


/// Corrects the cascade index to remove mip map artifacts.
/// \param[in] cascade  The computed cascade index.
/// \return The new cascade index that should be used for the current pixel.
/// \remarks
/// If mipmaps are used, there can be errors (visible edges) at cascade edges.
/// To remove this artifacts, this method adjust the cascade index which corrects
/// the problem.
int FixCsmCascadeForMipMaps(int cascade)
{
  // TODO: Is it ok to place the constant IN the method?
  const int CascadePowLookup[8] = {0, 1, 1, 2, 2, 2, 2, 3};
  
  // Fixing-MipMap-Trick by Andrew Lauritzen:
  // The GPU always handle "quads" of 2x2 pixels to compute ddx() and ddy() of texture coordinates
  // to control the mipmap filtering. ddx(u) and ddy(u) is computed is the differences of u
  // in these pixels. The same ddx(u) and ddy(u) is assigned to all quad pixels.
  // But if some pixels in this 2x2 quad use cascade=0 and others use cascade=1, then the derivative
  // are garbage because different LightViewProjection matrices are used to compute the texture
  // coordinates --> The mipmap filtering will generate visible errors (e.g. strange edges) for
  // these pixels.
  // Approach: All quad pixels have to use the max cascade index of the 4 pixels. If we compute
  // ddx(cascade) then we see if we are critical pixels. But ddx(cascade) is equal for all 4 pixels
  // and only some of them must change their cascade.
  // Solution: Compute ddx(2^cascade). The result will be 0 for non-critical pixels and != 0 for
  // pixels in the critical quads. ddx(2^cascade) will be 2^cascade'. If all pixels in the quad choose
  // cascade* as their cascade index, then the derivatives will again make sense and mipmap filtering
  // will produces good results.
  int cascadePow = pow(2, cascade);
  int cascadeX = abs(ddx(cascadePow));
  int cascadeY = abs(ddy(cascadePow));
  int cascadeXY = abs(ddx(cascadeY));
  int cascadeMax = max(cascadeXY, max(cascadeX, cascadeY));
  return cascadeMax > 0 ? CascadePowLookup[cascadeMax-1] : cascade;
}


/// Chooses the Cascaded Shadow Map cascade by comparing cascade distances
/// and creates the texture coordinates for the shadow map.
/// \param[in]  cameraDistance    The camera z-distance.
/// \param[in]  cascadeDistances  The distances where the CSM cascades end.
/// \param[out] cascade           The cascade index.
/// \param[out] shadowTexCoord    The shadow map texture coordinates.
void ComputeCsmCascadeFast(float4 position,
                           float cameraDistance,
                           float4 cascadeDistances,
                           float4x4 shadowMatrices[4],
                           out int cascade,
                           out float3 shadowTexCoord)
{
  float3 greater = (cascadeDistances.xyz < cameraDistance);
  cascade = dot(greater, 1.0f);
  
  // For correct mipmap filtering:
  // cascade = FixCsmCascadeForMipMaps(cascade);
  
  shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
}


/// Chooses the Cascaded Shadow Map cascade by checking the exact texture
/// areas and creates the texture coordinates for the shadow map.
/// \param[out] cascade         The cascade index.
/// \param[out] shadowTexCoord  The shadow map texture coordinates.
/// \remarks
/// This function differs from ComputeCsmCascadeFast() in that it is slower but
/// chooses the best possible cascade at regions where cascades overlap.
void ComputeCsmCascadeBest(float4 position,
                           float4x4 shadowMatrices[4],
                           out int cascade,
                           out float3 shadowTexCoord)
{
  // We compute the shadow map texture coordinates and check if they are in the
  // shadow map texture space ([0, 1]). A small safety border is added to avoid
  // artifacts at cascade boundaries (especially when a texture atlas is used
  // and different shadow maps are next to each other).
  // Note that also the z coordinate of shadowTexCoord is checked because the
  // different light frustums have a different depth range.
  cascade = 0;
  shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
  
  const float border = 0.01;
  const float minimum = border;
  const float maximum = 1 - border;
  if (!IsInRange(shadowTexCoord, minimum, maximum))
  {
    cascade++;
    shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
    
    if (!IsInRange(shadowTexCoord, minimum, maximum))
    {
      cascade++;
      shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
      
      if (!IsInRange(shadowTexCoord, minimum, maximum))
      {
        cascade++;
        shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
      }
    }
  }
  
  // For correct mipmap filtering:
  //cascade = FixCsmCascadeForMipMaps(cascade);
  //shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
}


void DitherCascade(float4 position, float4x4 shadowMatrices[4], float2 vPos, inout int cascade, inout float3 shadowTexCoord)
{
  const float ditherRange = 0.05;  // 5%
  const float minimum = ditherRange;
  const float maximum = 1 - ditherRange;
  if (!IsInRange(shadowTexCoord, minimum, maximum))
  {
    // Check if cascade overlaps with next cascade.
    float3 shadowTexCoord2 = GetShadowTexCoord(position, shadowMatrices[cascade + 1]);
    if (IsInRange(shadowTexCoord2, 0, 1))
    {
      // Choose current cascade or next based on dither pattern.
      float3 t = abs(shadowTexCoord * 2 - 1);
      float p = (1 - max(max(t.x, t.y), t.z)) / (2 * ditherRange);
      if (Dither4x4(vPos) > p)
      {
        shadowTexCoord = shadowTexCoord2;
        cascade = cascade + 1;
      }
    }
  }
}


/// Chooses the Cascaded Shadow Map cascade by checking the exact texture
/// areas and creates the texture coordinates for the shadow map (using a
/// dither pattern to hide cascade transitions).
/// \param[in]  cascadeSelection  The cascade selection mode (see CascadeSelectionXXX constants).
/// \param[in]  numberOfCascades  The number of cascades.
/// \param[in]  vPos              The screen space position in pixels.
/// \param[out] cascade           The cascade index.
/// \param[out] shadowTexCoord    The shadow map texture coordinates.
/// \remarks
/// This function differs from ComputeCsmCascadeBest() in that it is slower but
/// hides cascades transitions using a dither pattern.
void ComputeCsmCascadeBestDithered(int numberOfCascades,
                                   float4 position,
                                   float4x4 shadowMatrices[4],
                                   float2 vPos,
                                   out int cascade,
                                   out float3 shadowTexCoord)
{
  // Choose the best cascade (highest quality).
  // We compute the shadow map texture coordinates and check if they are in the
  // shadow map texture space ([0, 1]).
  // Note that also the z coordinate of shadowTexCoord is checked because the
  // different light frustums have a different depth range.
  cascade = 0;
  shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[0]);
  if (IsInRange(shadowTexCoord, 0, 1))
  {
    DitherCascade(position, shadowMatrices, vPos, cascade, shadowTexCoord);
  }
  else
  {
    cascade++;
    shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
    if (IsInRange(shadowTexCoord, 0, 1))
    {
      DitherCascade(position, shadowMatrices, vPos, cascade, shadowTexCoord);
    }
    else
    {
      cascade++;
      shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
      if (IsInRange(shadowTexCoord, 0, 1))
      {
        DitherCascade(position, shadowMatrices, vPos, cascade, shadowTexCoord);
      }
      else
      {
        cascade++;
        shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
      }
    }
  }
  
  // For correct mipmap filtering:
  // cascade = FixCsmCascadeForMipMaps(cascade);
  // shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
}


/// Chooses the correct Cascaded Shadow Map cascade and creates the texture
/// coordinates for the shadow map.
/// \param[in]  cascadeSelection  The cascade selection mode (see CASCADE_SELECTION_* constants).
/// \param[in]  numberOfCascades  The number of cascades.
/// \param[in]  cameraDistance    The camera z-distance.
/// \param[in]  cascadeDistances  The camera distances where the CSM cascades begin.
/// \param[in]  vPos              The screen space position in pixels.
/// \param[out] cascade           The cascade index.
/// \param[out] shadowTexCoord    The shadow map texture coordinates.
void ComputeCsmCascade(int cascadeSelection,
                       int numberOfCascades,
                       float4 position,
                       float cameraDistance,
                       float4 cascadeDistances,
                       float4x4 shadowMatrices[4],
                       float2 vPos,
                       out int cascade,
                       out float3 shadowTexCoord)
{
  if (cascadeSelection <= CASCADE_SELECTION_FAST)
  {
    float3 greater = (cascadeDistances.xyz < cameraDistance);
    cascade = dot(greater, 1.0f);
    shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
  }
  else
  {
    // Choose the best cascade (highest quality).
    // We compute the shadow map texture coordinates and check if they are in the
    // shadow map texture space ([0, 1]).
    // Note that also the z coordinate of shadowTexCoord is checked because the
    // different light frustums have a different depth range.
    shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[0]);
    cascade = 0;
    for (; cascade < numberOfCascades - 1;)
    {
      if (IsInRange(shadowTexCoord, 0, 1))
      {
        // Found cascade.
        
        if (cascadeSelection >= CASCADE_SELECTION_BEST_DITHERED)
        {
          // Check if we are at border.
          const float ditherRange = 0.05;  // 5%
          const float minimum = ditherRange;
          const float maximum = 1 - ditherRange;
          if (!IsInRange(shadowTexCoord, minimum, maximum))
          {
            // Check if we overlap the next cascade.
            float3 shadowTexCoord2 = GetShadowTexCoord(position, shadowMatrices[cascade + 1]);
            if (IsInRange(shadowTexCoord2, 0, 1))
            {
              // Choose current cascade or next based on dither pattern.
              float3 t = abs(shadowTexCoord * 2 - 1);
              float p = (1 - max(max(t.x, t.y), t.z)) / (2 * ditherRange);
              if (Dither4x4(vPos) > p)
              {
                shadowTexCoord = shadowTexCoord2;
                cascade = cascade + 1;
              }
            }
          }
        }
        
        break;
      }
      else
      {
        // Not in this cascade. Try next.
        cascade++;
        shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
      }
    }
  }
  
  // For correct mipmap filtering:
  // cascade = FixCsmCascadeForMipMaps(cascade);
  // shadowTexCoord = GetShadowTexCoord(position, shadowMatrices[cascade]);
}


// For reference:
/// Chooses the nearest 2 Cascaded Shadow Map cascades and creates the texture
/// coordinates for the shadow maps and a lerp factor to interpolate between
/// both cascades.
/// \param[in]  cameraDistance        The camera z-distance.
/// \param[in]  cascadeDistances      The camera distances where the CSM cascades end.
/// \param[out] cascades              The two cascade indices.
/// \param[out] interpolationWeight   The interpolation weight to lerp between the two cascades.
/// \param[out] shadowTexCoords       The shadow map texture coordinates for both cascades.
void ComputeCsmCascadeInterpolated(float4 position,
                                   float cameraDistance,
                                   float4 cascadeDistances,
                                   float4x4 shadowMatrices[4],
                                   out int2 cascades,
                                   out float interpolationWeight,
                                   out float3 shadowTexCoords[2])
{
  // In the following we choose the 2 cascade that should be sampled.
  // If we are near a cascade border, we interpolate between both cascades.
  // Outside the interpolation area it would not be necessary to sample both
  // cascades since the interpolation parameter is either 0 or 1.
  
  const float border = 0.1;   // 10% interpolation area in each cascade.
  const float minimum = border;
  const float maximum = 1 - border;
  
  if (cameraDistance < lerp(cascadeDistances.x, cascadeDistances.y, minimum))
  {
    // In cascade 0 or up to 10% into cascade 1.
    // --> Lerp between cascade 0 and 1.
    cascades[0] = 0;
    cascades[1] = 1;
    interpolationWeight = LinearStep(maximum * cascadeDistances.x,
                                     lerp(cascadeDistances.x, cascadeDistances.y, minimum),
                                     cameraDistance);
  }
  else if (cameraDistance < lerp(cascadeDistances.y, cascadeDistances.z, minimum))
  {
    // In cascade 1 or up to 10% into cascade 2.
    // --> Lerp between cascade 1 and 2.
    cascades[0] = 1;
    cascades[1] = 2;
    interpolationWeight = LinearStep(lerp(cascadeDistances.x, cascadeDistances.y, maximum),
                                     lerp(cascadeDistances.y, cascadeDistances.z, minimum),
                                     cameraDistance);
  }
  else
  {
    // Otherwise: Lerp between cascade 2 and 3.
    cascades[0] = 2;
    cascades[1] = 3;
    interpolationWeight = LinearStep(lerp(cascadeDistances.y, cascadeDistances.z, maximum),
                                     lerp(cascadeDistances.z, cascadeDistances.w, minimum),
                                     cameraDistance);
  }
  
  shadowTexCoords[0] = GetShadowTexCoord(position, shadowMatrices[cascades[0]]);
  shadowTexCoords[1] = GetShadowTexCoord(position, shadowMatrices[cascades[1]]);
}
#endif
