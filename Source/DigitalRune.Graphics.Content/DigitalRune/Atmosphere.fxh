//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Atmosphere.fxh
/// Provides functions for atmospheric scattering and aerial perspective.
//
// Explanations:
// Scale Height = the altitude (height above ground) at which the average
//                atmospheric density is found.
// Optical Depth = also called optical length, airmass, etc.
//
// References:
// [GPUGems2] GPU Gems 2: Accurate Atmospheric Scattering by Sean O'Neil.
// [GPUPro3]  An Approximation to the Chapman Grazing-Incidence Function for
//            Atmospheric Scattering, GPU Pro3, pp. 105.
// Papers bei Bruneton, Nishita, etc.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_ATMOSPHERE_FXH
#define DIGITALRUNE_ATMOSPHERE_FXH


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_COMMON_FXH
static const float Pi = 3.1415926535897932384626433832795;
#endif


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

// The scale function, which is a helper function for GetOpticalDepthONeil().
// The scale function assumes that the atmosphere is 2.5 % of the planet radius.
// cosZenith is the cosine of the angle between the "Up" vector and a ray.
// To approximate the optical depth of a ray from a point to the atmosphere compute
// exp(-h/H) * Scale(cosZenith).
// Atmospheric Scattering code is based on the implementation given in
// GPU Gems 2: Accurate Atmospheric Scattering by Sean O'Neil.
float ScaleFunction(float cosZenith, float HRelative)
{
  float x = 1.0 - cosZenith;
  return HRelative * exp(-0.00287 + x*(0.459 + x*(3.83 + x*(-6.80 + x*5.25))));
}


/// Computes the optical depth for a ray shooting into the sky.
/// \param[in] h          The absolute observer altitude (height above ground).
/// \param[in] H          The scale height.
/// \param[in] HRelative  H relative to the total height of the atmosphere.
///                       HRelative = H / (atmosphere radius - ground radius.
/// \param[in] cosZenith  The cosine of the angle between "up" and the ray.
/// \return The optical depth.
/// \remarks
/// This method is based on [GPUGems2]
float GetOpticalDepthONeil(float h, float H, float HRelative, float cosZenith)
{
  float opticalDepth = exp(-h/H) * ScaleFunction(cosZenith, HRelative);
  
  // The optical depth formulas from O'Neil and Schüler are similar but not quite
  // equal. They differ by varying factor f which was around 42 - 52 in some quick test.
  // Instead of applying the factor f = 50 here, we could divide the beta coefficients
  // by f and multiply the final colors by f...
  opticalDepth /= 50;
  
  return opticalDepth;
}


/// Computes the approximate Chapman function times exp(-hRelative).
/// \param[in] X          The planet radius divided by H.
/// \param[in] h          The observer altitude divided by H.
/// \param[in] cosZenith  The cosine of the angle between zenith (straight up) and the ray direction.
/// \return The approximate Chapman function corrected by transitive consistency
///         according to [GPUPro3].
float ChapmanApproximation(float X, float h, float cosZenith)
{
  float c = sqrt(X + h);
  if(cosZenith >= 0)
  {
    // Looking above horizon.
    return c / (c * cosZenith + 1) * exp(-h);
  }
  else
  {
    // Looking below horizon.
    float x0 = sqrt(1 - cosZenith * cosZenith) * (X + h);
    float c0 = sqrt(x0);
    return 2 * c0 * exp(X - x0) - c / (1 - c * cosZenith) * exp(-h);
  }
}


/// Computes the optical depth for a ray shooting into the sky.
/// \param[in] h              The absolute observer altitude (height above ground).
/// \param[in] H              The scale height.
/// \param[in] radiusGround   The absolute earth radius (ground level).
/// \param[in] cosZenith      The cosine of the angle between "up" and the ray.
/// \return The optical depth.
/// \remarks
/// This method is based on [GPUPro3].
float GetOpticalDepthSchueler(float h, float H, float radiusGround, float cosZenith)
{
  return H * ChapmanApproximation(radiusGround / H, h / H, cosZenith);
}


/// Computes the transmittance looking from the given position to infinity.
/// \param[in] position       The observer position relative in planet space
///                           (planet center is origin).
/// \param[in] viewDirection  The normalized view direction.
/// \param[in] beta           The extinction coefficient.
/// \param[in] H              The scale height.
/// \param[in] radiusGround   The planet radius.
/// \return The transmittance.
float3 GetTransmittance(float3 position, float3 viewDirection, float3 beta, float H, float radiusGround)
{
  float radiusSquared = dot(position, position);
  float oneOverRadius = rsqrt(radiusSquared);
  float radius = radiusSquared * oneOverRadius;
  float h = radius - radiusGround;
  float cosZenith = dot(position, viewDirection) * oneOverRadius;
  
  return exp(-GetOpticalDepthSchueler(h, H, radiusGround, cosZenith) * beta);
}


/// Computes atmospheric scattering parameters for a viewing ray.
/// The ray must start at the observer and end at the end of the atmosphere
/// or at the terrain.
/// \param[in] rayStart           The ray origin.
/// \param[in] rayDirection       The normalized ray direction.
/// \param[in] rayLength          The ray length.
/// \param[in] hitGround          True if the ray hits the ground. False if looking
///                               at sky or terrain above the ground.
/// \param[in] sunDirection       The direction to the sun.
/// \param[in] radiusGround       The radius of the ground level (0 altitude).
/// \param[in] radiusAtmosphere   The radius of the top of the atmosphere.
/// \param[in] scaleHeight        The scale height.*
/// \param[in] numberOfSamples    The number of integration samples.
///                               This parameter is ignored on XBox. Xbox uses 3 samples.
/// \param[in] betaRayleigh       The extinction/scatter coefficient for Rayleigh.**
/// \param[in] betaMie            The extinction/scatter coefficient for Mie.**
/// \param[out] transmittance     The computed transmittance along the ray.
/// \param[out] colorRayleigh     The inscattered light color from Rayleigh scattering.
/// \param[out] colorMie          The inscattered light color from Mie scattering.
/// \remarks
/// The phase functions are not applied in this function because it is possible
/// to evaluate this function in a vertex shader. The phase functions must
/// be applied in the pixel shader.
/// * We assume that Rayleigh and Mie have the same scale height. In reality they
/// are different (8km for Rayleigh, 1.2km for Mie).
/// ** This function assumes that the extinction and scatter coefficients are equal.
/// This is usually correct for Rayleigh but not necessarily for Mie. For Mie
/// the real extinction coefficient would be bExtinction = bScatter + bAbsorption.
void ComputeAtmosphericScattering(float3 rayStart, float3 rayDirection, float rayLength, bool hitGround,
                                  float3 sunDirection, float radiusGround, float radiusAtmosphere, float scaleHeight,
                                  int numberOfSamples, float3 betaRayleigh, float3 betaMie,
                                  out float3 transmittance, out float3 colorRayleigh, out float3 colorMie)
{
  // Scale height (which is also an altitude).
  float H = scaleHeight;
  
  // For ground hits we have to march in the other direction - cannot compute
  // optical length for any path through the earth ground sphere.
  float neg = hitGround ? -1 : 1;
  
  // Ray end is the sky or the terrain hit point.
  float3 rayEnd = rayStart + rayDirection * rayLength;
  float radiusEnd = length(rayEnd);
  
  // Zenith direction is always the vector from the center of the earth to the point.
  float3 zenith = rayEnd / radiusEnd;
  
  // Altitude of ray end.
  float h = radiusEnd - radiusGround;
  
  // Optical depth of ray end (which is the sky or the terrain).
  float cosRay = dot(zenith, neg * rayDirection);
  float lastRayDepth = GetOpticalDepthSchueler(h, H, radiusGround, cosRay);
  
  // Optical depth of ray end to sun.
  float cosSun = dot(zenith, sunDirection);
  float lastSunDepth = GetOpticalDepthSchueler(h, H, radiusGround, cosSun);
  
#ifdef XBOX
  // XBox HLSL compiler problem: If numberOfSamples is defined by an effect parameter
  // and is not constant, the code does not work...
  numberOfSamples = 3;
#endif
  
  float segmentLength = rayLength / numberOfSamples;
  float3 T = 1;   // The ray transmittance (camera to sky/terrain).
  float3 S = 0;   // The inscattered light.
  for (int i = numberOfSamples - 1; i >= 0; i--)
  {
    float3 samplePoint = rayStart + i * segmentLength * rayDirection;
    float radius = length(samplePoint);
    zenith = samplePoint / radius;
    
    h = radius - radiusGround;
    
    // Optical depth of sample point to sky.
    cosRay = dot(zenith, neg * rayDirection);
    float sampleRayDepth = GetOpticalDepthSchueler(h, H, radiusGround, cosRay);
    // Optical depth of the current ray segment.
    float segmentDepth = neg * (sampleRayDepth - lastRayDepth);
    // Transmittance of the current ray segment.
    float3 segmentT = exp(-segmentDepth * (betaRayleigh + betaMie));
    
    // Optical depth of sample to sun.
    cosSun = dot(zenith, sunDirection);
    float sampleSunDepth = GetOpticalDepthSchueler(h, H, radiusGround, cosSun);
    // Average the depths of the segment end points.
    float segmentSunDepth = 0.5 * (sampleSunDepth + lastSunDepth);
    // Inscattered light is T * sun light intensity. We compute it for a sun intensity of 1.
    float3 segmentS = exp(-segmentSunDepth * (betaRayleigh + betaMie));
    
    // Last inscatter is attenuated by this segment.
    S = S * segmentT;
    
    // Add inscatter from current sample point.
    // Schüler uses this
    //S += (1 - segmentT) * segmentS;
    // O'Neil uses this. Not sure why the atmosphere height is here.
    //S += exp(-h / H) * segmentS * sampleLengthRelativeToAtmosphereHeight;
    // Since we do numeric integration, we need to really do this. (Btw.
    // beta coefficients have the unit [m^-1], so the length unit cancels itself
    // correctly.)
    //S += segmentS * segmentLength;
    // Some articles (O'Neil, Crytek) have exp(-h/H) in the integral:
    S += exp(-h / H) * segmentLength * segmentS;
    
    // Attenuation factors like T are combined by multiplication.
    T = T * segmentT;
    
    lastRayDepth = sampleRayDepth;
    lastSunDepth = sampleSunDepth;
  }
  
  transmittance = T;
  
  // Apply "scatter" coefficients. (The betas of the transmittance computation
  // are the "extinction" coefficients. We assume betaScatter = betaExtinction.)
  colorRayleigh = S * betaRayleigh;
  colorMie = S * betaMie;
}


/*
//  Original implementation similar to O'Neil.
void ComputeAtmosphericScatteringOld(float3 rayStart, float3 rayDirection, float rayLength, bool hitTerrain,
  float3 sunDirection, float3 radiusGround, float3 radiusAtmosphere, float scaleHeight,
  int numberOfSamples, float3 betaRayleigh, float3 betaMie,
  out float3 transmittance, out float3 colorRayleigh, out float3 colorMie)
{
  // TODO: I think this does not yet work if camera hits terrain but is under the terrain.
  //       In this case we must shoot the ray from the camera but subtract the optical depth
  //       after the hit....

  // TODO: Compute transmittance.
  transmittance = 0;
  
  float H = scaleHeight;
  float h = length(rayStart) - radiusGround;    // Observer altitude.
  
  float neg = hitTerrain ? -1 : 1;
    
  // Cosine of angle between "Up" and ray direction.
  float cosZenith = dot(neg * rayDirection, normalize(rayStart));
  
  // Optical depth of the whole ray (camera to sky or terrain to sky if hitTerrain is true).
#if USE_ONEIL
  float HRelative = H / (radiusAtmosphere - radiusGround);
  float depthRayToSky = GetOpticalDepthONeil(h, H, HRelative, cosZenith);
#else
  float depthRayToSky = GetOpticalDepthSchueler(h, H, radiusGround, cosZenith);
#endif

  // The length of one integration segment on the ray.
  float sampleLengthAbsolute = rayLength / numberOfSamples;
  // The sample length relative to the height of the atmosphere.
  float sampleLengthRelative = sampleLengthAbsolute / (radiusAtmosphere - radiusGround);
  
  float3 sampleRay = rayDirection * sampleLengthAbsolute;
  float3 samplePoint = rayStart + sampleRay * 0.5;  // We sample in the middle of the sample segment.
  
  // Compute the integral of the In-Scattering equation.
  float3 S = 0;  // The inscattered light.
  for(int i=0; i<numberOfSamples; i++)
  {
    // The height of the sample point (to earth center).
    float radiusSample = length(samplePoint);
    // The altitude of the sample point.
    h = radiusSample - radiusGround;
        
    // Cosine of angle between zenith and sun direction. Zenith is in the
    // direction of the samplePoint vector because we are on a sphere!
    float cosSun = dot(sunDirection, samplePoint) / radiusSample;
        
    // Optical depth from sample to sun.
#if USE_ONEIL
    float depthSampleToSun = GetOpticalDepthONeil(h, H, HRelative, cosSun);
#else
    float depthSampleToSun = GetOpticalDepthSchueler(h, H, radiusGround, cosSun);
#endif
    
    // Cosine of angle between zenith and ray direction.
    float cosRay = dot(neg * rayDirection, samplePoint) / radiusSample;
    
    // Optical depth from camera to sample =
    //    depth of camera to sky - depth of sample to sky.
#if USE_ONEIL
    float depthSampleToCamera = depthRayToSky - GetOpticalDepthONeil(h, H, HRelative, cosRay);
#else
    float depthSampleToCamera = depthRayToSky - GetOpticalDepthSchueler(h, H, radiusGround, cosRay);
#endif
    
    // Optical depth from camera to sample to sun.
    float depthCameraToSun = depthSampleToSun + neg * depthSampleToCamera;
    // Transmittance for this sample.
    float3 sampleT = exp(-depthCameraToSun * (betaRayleigh + betaMie));
    
    // The result at the sample point must be multiplied by the sample length (numerical integration).
    // Then add integral result for this sample to integral sum.
    // Why is this multiplied with sampleLengthRelative and not sampleLengthAbsolute???
    // In O'Neil it is also multiplied with exp(-h/H). Other articles don't do this...
    S += sampleT * (exp(-h / H) * sampleLengthRelative);
    
    // Move to next sample point.
    samplePoint += sampleRay;
  }
    
  colorRayleigh = S * betaRayleigh;
  colorMie = S * betaMie;
} //*/


/// The Rayleigh phase function. Same as PhaseFunction(cosTheta, 0).
/// \param[in] cosTheta   The cosine of the angle between the direction to the light
///                       and the viewing direction (e.g. observer to sky).
float PhaseFunctionRayleigh(float cosTheta)
{
  float cosThetaSquared = cosTheta * cosTheta;
  return 3.0 / (16.0 * Pi) * (1 + cosThetaSquared);
}


/// The phase function (which can be used for Rayleigh and Mie).
/// \param[in] cosTheta   The cosine of the angle between the direction to the light
///                       and the viewing direction (e.g. observer to sky).
/// \param[in] g          Scattering symmetry constant g. g is usually 0 for
///                       Rayleigh and within [0.75, 0.999] for Mie scattering.
/// \return The amount of light scattered in the direction of the observer.
float PhaseFunction(float cosTheta, float g)
{
  // Note: Some multiply this by 4 * Pi and divide the extinction/scatter coefficient
  // by 4 * Pi. - But the current form is better because the integral of the sphere
  // is 1!
  float gSquared = g * g;
  float cosThetaSquared = cosTheta * cosTheta;
  
  // Cornette-Shanks phase function
  return 3 / (8 * Pi)
         * ((1.0 - gSquared) / (2.0 + gSquared))
         * (1.0 + cosThetaSquared)
         / pow(1.0 + gSquared - 2.0 * g * cosTheta, 1.5);  // Compiler warning: pow does not work for negative values.
  
  // Henyey-Greenstein phase function
  //return 1 / (4 * Pi) * (1.0 - gSquared) /
  //       pow(1.0 + gSquared - 2.0 * g * cosTheta, 1.5);
}
#endif
