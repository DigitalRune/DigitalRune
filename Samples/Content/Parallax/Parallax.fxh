//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Parallax.fxh
/// Provides methods for Parallax Mapping.
//
// Parallax Mapping (PM) is also known as offset mapping, offset bump mapping
// (OBM), virtual displacement mapping. PM samples a height field using a single
// sample to offset the texture coordinates. PM creates the illusion of
// different heights and the bumps in the material look more realistic.
//
// Parallax Occlusion Mapping (POM) improves PM but is significantly more
// expensive. POM ray-traces the height field, taking several samples to find
// the actual intersection of the view ray and the height field.
// Optionally, self-shadowing can approximated too by ray-tracing the light ray.
//
// Height texture:
// P(O)M needs a gray scale texture where the red channel contains the height.
// Height is used like this:
// If the HeightBias is 0 and the sampled height is 0, we are on the surface
// of the triangle. The height is positive above the triangle and negative
// below the triangle. HeightScale is used to scale the height textures samples.
// The HeightBias is added too.
// To move the whole relief below the triangle surface, use HeightBias = -HeightScale.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_PARALLAX_FXH
  #define DIGITALRUNE_PARALLAX_FXH

//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// An effect which uses parallax mapping will want to add following parameters:

/*
// The height scale for the height map (used in the parallax mapping).
// Set a value = 0 to disable POM. Set a value > 0 to enable POM.
float ParallaxHeightScale = 0;

// The height bias for the height map (used in the parallax mapping, not in POM).
float ParallaxHeightBias = 0;

// The height texture.
texture HeightTexture : Height;
sampler2D HeightSampler = sampler_state
{
  Texture = <HeightTexture>;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
  AddressU = Wrap;
  AddressV = Wrap;
};

// ---- POM only:
// The size of the height texture in pixels (width, height).
float2 HeightTextureSize = float2(256, 256);

// The mip level threshold for transitioning between the full POM computation and normal mapping.
int ParallaxLodThreshold = 3;

// The minimum number of samples for sampling the height field profile.
int ParallaxMinNumberOfSamples = 4;

// The maximum number of samples for sampling the height field profile.
int ParallaxMaxNumberOfSamples = 20;

// For soft self-shadowing:
// A factor which defines the sampling distance.
float ShadowScale = 0.5;

// The number of samples for shadow computation.
int ShadowSamples = 4;

// The factor that reduces the influence of distant samples.
float ShadowFalloff = 0.33;

// A factor which makes shadows darker.
float ShadowStrength = 100;
*/


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Computes Parallax Mapping with Offset Limiting.
/// \param[in] texCoord       The original texture coordinates.
/// \param[in] heightSampler  The height map with height in R channel.
/// \param[in] heightScale    A scale factor with which the sample height is
///                           scaled.
/// \param[in] heightBias     A bias that is added to the computed height.
/// \param[in] viewDirectionTangent   (x,y) of the normalized view direction
///                           (pointing from the camera to the pixel) in tangent
///                           space.
/// \return
/// A vector containing the corrected texture coordinates and the height (in
/// world space units) above the triangle: (texCoord.x, texCoord.y, height).
float3 ParallaxMapping(float2 texCoord, sampler heightSampler,
  float heightScale, float heightBias, float2 viewDirectionTangent)
{
  // Sample height.
  float height = heightScale * tex2D(heightSampler, texCoord).r + heightBias;
  
  // Correct texture coordinate.
  texCoord = texCoord - height * viewDirectionTangent;
  
  return float3(texCoord.xy, height);
}


/// Computes the corrected texture coordinates using Parallax Occlusion Mapping
/// with soft self-shadowing. 
/// Shadow computation is experimental!
/// This method does nothing on Xbox!
/// \param[in] texCoord         The original texture coordinates.
/// \param[in] heightSampler    The height map with height in R channel.
/// \param[in] viewDirection    The view direction vector (camera to pixel) in
///                             tangent space.
/// \param[in] parallaxOffset   The maximal parallax offset vector computed with
///                             ComputeMaxParallaxOffset().
/// \param[in] heightScale      A scale factor with which the sample height is
///                             scaled.
/// \param[in] heightBias       A bias that is added to the computed height.
/// \param[in] mipLevel         The current mip map level.
/// \param[in] lodThreshold     A mip map level threshold for LOD. If the mipLevel is
///                             greater than this threshold, no parallax occlusion is
///                             computed.
/// \param[in] minNumberOfSamples The minimal number of samples to sample the height map profile.
/// \param[in] maxNumberOfSamples The maximal number of samples to sample the height map profile.
/// \param[in] lightDirection   The direction of light rays in tangent space.
/// \param[in] shadowScale      A factor which defines the sampling distance.
/// \param[in] shadowSamples    The number of samples for shadow computation.
/// \param[in] shadowFalloff    The factor that reduces the influence of distant samples.
/// \param[in] shadowStrength   A factor which makes shadows darker.
/// \return
/// A vector containing the corrected texture coordinates and the height (in
/// world space units) above the triangle and a shadow factor (0 = no shadow,
/// 1 = full shadow):  (texCoord.x, texCoord.y, height, shadow).
/// \remarks
/// This funtion computes POM by stepping through the height field in linear steps.
/// The function assumes that the height field is linear between two samples.
/// From the found height field hit, the light ray is traced using a fixed number
/// of samples. The max height relative to the actual height is used as an
/// occlusion factor for soft self-shadows.
/// See also book ShaderX5.
float4 ParallaxOcclusionMapping(float2 texCoord, sampler2D heightSampler,
  float3 viewDirection, float heightScale, float heightBias, float mipLevel,
  int lodThreshold, int minNumberOfSamples, int maxNumberOfSamples, float3 lightDirection,
  float shadowScale, int shadowSamples, float shadowFalloff, float shadowStrength)
{
#ifndef XBOX
  float height = 0;
  float shadow = 0;
  if (mipLevel < (float) lodThreshold && heightScale > 0)
  {
    // Compute furthest amount of parallax displacement. We get this displacement if the view
    // vector hits the bottom (height = 0) of the height map.
    float2 parallaxOffset = viewDirection.xy / abs(viewDirection.z) * heightScale;

    // Derivatives must not be computed in dynamic branch, so we compute them manually here.
    float2 dx = ddx(texCoord);
    float2 dy = ddy(texCoord);
    
    // Compute a dynamic number of steps for the linear search. Depending on the viewing angle
    // we lerp between minNumberOfSamples and maxNumberOfSamples.
    int numberOfSteps = (int) lerp(maxNumberOfSamples, minNumberOfSamples, -viewDirection.z);
    
    // Size of a step (relative to 1).
    float stepSize = 1.0 / (float) numberOfSteps;
    
    // Size of a step in texture coordinates.
    float2 texCoordStep = stepSize * parallaxOffset;
    
    // Because of the height bias we must change the start texture coordinate.
    float2 texCoordBias = -parallaxOffset * (1 + heightBias / heightScale);
    
    // The texture coordinate where we begin sampling the height texture.
    float2 currentTexCoord = texCoord + texCoordBias;
    float currentBound = 1.0;
    
    float2 pt1 = 0;
    float2 pt2 = 0;
    
    // Find hit of view ray against height vield.
    float previousHeight = 1;
    int stepIndex = 0;
    while (stepIndex < numberOfSteps)
    {
       currentTexCoord += texCoordStep;

       // Sample height map which in this case is stored in the R channel.
       float currentHeight = tex2Dgrad(heightSampler, currentTexCoord, dx, dy).x;
       currentBound -= stepSize;

       if (currentHeight > currentBound)
       {
          pt1 = float2(currentBound, currentHeight );
          pt2 = float2(currentBound + stepSize, previousHeight);

          // Abort loop.
          stepIndex = numberOfSteps + 1;
       }
       else
       {
          stepIndex++;
          previousHeight = currentHeight;
       }
    }

    // Assume that the height is linear between two samples and compute the
    // height of the hit between the last two samples.
    float hitHeight;    // = height where view ray hits height field in range [0, 1].
    float delta2 = pt2.x - pt2.y;
    float delta1 = pt1.x - pt1.y;
    float denominator = delta2 - delta1;
    
    // Avoid division by zero.
    if (denominator == 0.0f)
       hitHeight = 0.0f;
    else
       hitHeight = (pt1.x * delta2 - pt2.x * delta1) / denominator;
    
    // Convert relative height to absolute height.
    height = heightBias + hitHeight * heightScale;
    
    // Correct texture coordinate.
    float2 originalTexCoord = texCoord;
    texCoord = texCoord + parallaxOffset * (1 - hitHeight) + texCoordBias;
    
    // Experimental soft self-shadowing:
    // From the hit position, sample to the light using fixed steps.
    lightDirection = lightDirection * shadowScale;
    float shadowHeight = hitHeight;
    for (int i = 1; i <= shadowSamples; i++)
    {
      float h = tex2Dgrad(heightSampler, float2(texCoord - lightDirection.xy * i / shadowSamples), dx, dy).x;
      
      // Reduce influence of distant samples.
      h = lerp(h, 0, shadowFalloff * i / shadowSamples);
      
      // Remember max height.
      shadowHeight = max(shadowHeight, h);
    }
    
    // The max found (height - hitHeight) is a measure of how much the pixel
    // is occluded.
    shadow = saturate((shadowHeight - hitHeight) * shadowStrength * heightScale);
    //shadow *= shadow;
    
    // Within the last LOD level, lerp POM influence to 0.
    if (mipLevel > (float)(lodThreshold - 1))
    {
      texCoord = lerp(texCoord, originalTexCoord, frac(mipLevel));
      shadow = lerp(shadow, 0, frac(mipLevel));
    }
  }
  
  return float4(texCoord, height, shadow);
#else
  return float4(texCoord, 0, 0);
#endif
}
#endif
