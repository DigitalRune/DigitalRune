//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Lighting.fxh
/// Functions for lighting computation.
//
// ----- Blinn-Phong vs. original Phong
// This lighting functions use Blinn-Phong (N.H) instead of original Phong
// (R.V) specular lights.
//
// ----- Per-vertex vs. per-pixel lighting
// Lighting computations can be done per vertex or per pixel.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_LIGHTING_FXH
#define DIGITALRUNE_LIGHTING_FXH

#ifndef DIGITALRUNE_COMMON_FXH
#error "Common.fxh required. Please include Common.fxh before including Lighting.fxh."
#endif


//--------------------------------------------------------
// Constants
//--------------------------------------------------------

/// Declares the uniform const for a light texture + sampler.
/// \param[in] name     The name of the texture constant.
/// \param[in] semantic The semantic of the texture constant.
/// \remarks
/// Example:
///   DECLARE_UNIFORM_LIGHTTEXTURE(DirectionalLightTexture0, DIRECTIONALLIGHTTEXTURE0);
#define DECLARE_UNIFORM_LIGHTTEXTURE(name, semantic) \
texture name : semantic; \
sampler name##Sampler = sampler_state \
{ \
  Texture = <name>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MinFilter = LINEAR; \
  MagFilter = LINEAR; \
  MipFilter = LINEAR; \
}


//--------------------------------------------------------
// Functions
//--------------------------------------------------------

/// Computes the attenuation factor that is caused by the distance from the light
/// source.
/// \param[in] dist       The distance from the light source.
/// \param[in] range      The range of the light source.
/// \param[in] exponent   The attenuation exponent.
/// \return A value in the range [0, 1]. The value is 1 at the light source and
///         0 beyond the range of the light source.
float ComputeDistanceAttenuation(float dist, float range, float exponent = 2)
{
  return max(0, 1 - pow(max(0.0001, dist / range), exponent));
}


/// Computes the attenuation factor for the cone attenuation of a spotlight.
/// \param[in] lightDirection The normalized direction vector that points from the light
///                           source to the lit position.
/// \param[in] spotDirection  The normalized direction in which the spot is pointing.
/// \param[in] falloffAngle   The angle between the spotlight direction and the direction
///                           where the light intensity starts to fall off (in radians).
/// \param[in] cutoffAngle    The angle between the spotlight direction and the direction
///                           where the light intensity reaches 0 (in radians).
/// \return A value in the range [0, 1]. The value is 1 in the center of the cone.
///         It is < 1 after the falloff angle and 0 after the cutoff angle.
float ComputeConeAttenuation(float3 lightDirection, float3 spotDirection,  float falloffAngle, float cutoffAngle)
{
  float actualCos = dot(lightDirection, spotDirection);
  float falloffCos = cos(falloffAngle);
  float cutoffCos = cos(cutoffAngle);
  return smoothstep(cutoffCos, falloffCos, actualCos);
}


/// Computes the color that is caused by a ambient light.
/// \param[in] light  The diffuse color and intensity of the light.
/// \return The light color that should be added to the color of the lit position.
float3 ComputeAmbientLight(float3 light)
{
  return light;
}


/// Computes the light contribution that is caused by a ambient light.
/// \param[in] light        The diffuse color and intensity of the light.
/// \param[in] attenuation  The hemispheric attenuation factor of the ambient light ([0, 1]).
///                         (0 = no hemispheric lighting, 1 = one-sided hemispheric lighting)
/// \param[in] up           The up vector of the light.
/// \param[in] normal       The normal vector of the lit surface point.
/// \return The light color that should be added to the color of the lit position.
/// \remarks
/// This method computes ambient lighting mixed with one-sided hemispheric lighting.
float3 ComputeAmbientLight(float3 light, float attenuation, float3 up, float3 normal)
{
  // The hemispheric light strength:
  //  0 if normal points "down".
  //  1 if normal points "up".
  float hemisphericFactor = dot(normal, up) * 0.5 + 0.5;
  
  // Lerp between pure ambient and hemispheric lighting.
  return light * lerp(1, hemisphericFactor, attenuation);
}


/// Computes the light contribution that is caused by a point light.
/// \param[in] diffuse          The diffuse color and intensity of the light.
/// \param[in] specular         The specular color and intensity of the light.
/// \param[in] lightDirection   The direction of the light rays.
/// \param[in] viewDirection    The view direction.
/// \param[in] normal           The normal vector at the lit position.
/// \param[in] specularPower    The speculare exponent (shininess) of the material.
/// \param[out] resultDiffuse   The diffuse light color at the lit position.
/// \param[out] resultSpecular  The specular light color at the lit position.
/// \remarks
/// All direction vectors must be normalized.
void ComputeDirectionalLight(float3 diffuse, float3 specular, float3 lightDirection,
                             float3 viewDirection, float3 normal, float specularPower,
                             out float3 resultDiffuse, out float3 resultSpecular)
{
  float nDotL = saturate(dot(normal, -lightDirection));
  resultDiffuse = diffuse * nDotL;
  
  // Phong specular
  //float3 r = normalize(reflect(DirectionalLightDirection, normal));
  //float phong = pow(0.0001 + saturate(dot(r, -viewDirection)), specularPower);
  
  // Blinn-Phong specular
  float3 h = -normalize(lightDirection + viewDirection);
  float3 nDotH = saturate(dot(normal, h));
  float3 blinnPhong = pow(0.0001 + nDotH, specularPower);
  
  // Use a self-shadowing term. (e.g. N.L or saturate(4 * N.L)).
  float selfShadowingTerm = nDotL;
  
  resultSpecular = specular * blinnPhong * selfShadowingTerm;
}


/// Computes the light contribution that is caused by a point light.
/// \param[in] diffuse          The diffuse color and intensity of the light.
/// \param[in] specular         The specular color and intensity of the light.
/// \param[in] range            The range (radius) of the light.
/// \param[in] attenuation      The distance attenuation exponent.
/// \param[in] lightDirection   The direction of the light rays.
/// \param[in] lightDistance    The distance of the lit position to the light.
/// \param[in] viewDirection    The view direction.
/// \param[in] normal           The normal vector at the lit position.
/// \param[in] specularPower    The speculare exponent (shininess) of the material.
/// \param[out] resultDiffuse   The diffuse light color at the lit position.
/// \param[out] resultSpecular  The specular light color at the lit position.
/// \remarks
/// All direction vectors must be normalized.
void ComputePointLight(float3 diffuse, float3 specular,
                       float range, float attenuation,
                       float3 lightDirection, float lightDistance,
                       float3 viewDirection, float3 normal, float specularPower,
                       out float3 resultDiffuse, out float3 resultSpecular)
{
  float distanceAttenuation = ComputeDistanceAttenuation(lightDistance, range, attenuation);
  float nDotL = saturate(dot(normal, -lightDirection));
  resultDiffuse = diffuse * nDotL * distanceAttenuation;
  
  // Phong specular
  //float3 r = normalize(reflect(lightDirection, normal));
  //float phong = pow(0.0001 + saturate(dot(r, -viewDirection)), specularPower);
  
  // Blinn-Phong specular
  float3 h = -normalize(lightDirection + viewDirection);
  float3 nDotH = saturate(dot(normal, h));
  float3 blinnPhong = pow(0.0001 + nDotH, specularPower);
  
  // Use a self-shadowing term. (e.g. N.L or saturate(4 * N.L)).
  float selfShadowingTerm = nDotL;
  
  resultSpecular = specular * blinnPhong * selfShadowingTerm * distanceAttenuation;
}


/// Computes the light contribution that is caused by a spotlight.
/// \param[in] diffuse          The diffuse color and intensity of the light.
/// \param[in] specular         The specular color and intensity of the light.
/// \param[in] range            The range (radius) of the light.
/// \param[in] attenuation      The distance attenuation exponent.
/// \param[in] falloffAngle     The angle between the spotlight direction and the direction
///                             where the light intensity starts to fall off (in radians).
/// \param[in] cutoffAngle      The angle between the spotlight direction and the direction
///                             where the light intensity reaches 0 (in radians).
/// \param[in] spotDirection    The center direction of the spotlight cone.
/// \param[in] lightDirection   The direction of the light rays.
/// \param[in] lightDistance    The distance of the lit position to the light.
/// \param[in] viewDirection    The view direction.
/// \param[in] normal           The normal vector at the lit position.
/// \param[in] specularPower    The speculare exponent (shininess) of the material.
/// \param[out] resultDiffuse   The diffuse light color at the lit position.
/// \param[out] resultSpecular  The specular light color at the lit position.
/// \remarks
/// All direction vectors must be normalized.
void ComputeSpotlight(float3 diffuse, float3 specular,
                      float range, float attenuation,
                      float falloffAngle, float cutoffAngle,
                      float3 spotDirection,
                      float3 lightDirection, float lightDistance,
                      float3 viewDirection, float3 normal, float specularPower,
                      out float3 resultDiffuse, out float3 resultSpecular)
{
  // Compute N.L, distance attenuation and cone attenuation.
  float distanceAttenuation = ComputeDistanceAttenuation(lightDistance, range, attenuation);
  float coneAttenuation = ComputeConeAttenuation(lightDirection, spotDirection, falloffAngle, cutoffAngle);
  float nDotL = saturate(dot(normal, -lightDirection));
  resultDiffuse = diffuse * nDotL * distanceAttenuation * coneAttenuation;
  
  // Phong specular
  //float3 r = normalize(reflect(lightDirection, normal));
  //float phong = pow(0.0001 + saturate(dot(r, -viewDirection)), specularPower);
  
  // Blinn-Phong specular
  float3 h = -normalize(lightDirection + viewDirection);
  float3 nDotH = saturate(dot(normal, h));
  float3 blinnPhong = pow(0.0001 + nDotH, specularPower);
  
  // Use a self-shadowing term. (e.g. N.L or saturate(4 * N.L)).
  float selfShadowingTerm = nDotL;
  
  resultSpecular = specular * blinnPhong * selfShadowingTerm * distanceAttenuation * coneAttenuation;
}


/// Computes the light contribution that is caused by a projector light.
/// \param[in] diffuse          The diffuse color and intensity of the light.
/// \param[in] specular         The specular color and intensity of the light.
/// \param[in] lightTexture     The projected texture.
/// \param[in] textureMatrix    The texture matrix of the light.
/// \param[in] range            The range (radius) of the light.
/// \param[in] attenuation      The distance attenuation exponent.
/// \param[in] lightDirection   The direction of the light rays.
/// \param[in] lightDistance    The distance of the lit position to the light.
/// \param[in] viewDirection    The view direction.
/// \param[in] position         The position of the lit point.
/// \param[in] normal           The normal vector at the lit position.
/// \param[in] specularPower    The speculare exponent (shininess) of the material.
/// \param[out] resultDiffuse   The diffuse light color at the lit position.
/// \param[out] resultSpecular  The specular light color at the lit position.
/// \remarks
/// All direction vectors must be normalized.
void ComputeProjectorLight(float3 diffuse, float3 specular,
                           sampler2D lightTexture, float4x4 textureMatrix,
                           float range, float attenuation,
                           float3 lightDirection, float lightDistance,
                           float3 viewDirection, float3 position, float3 normal,
                           float specularPower,
                           out float3 resultDiffuse, out float3 resultSpecular)
{
  // Projected texture
  float4 lightTexCoord = mul(float4(position, 1), textureMatrix);
  float3 textureColor = FromGamma(tex2Dproj(lightTexture, lightTexCoord).xyz);
  
  // Clip back projection.
  float isInFront = (lightTexCoord.w >= 0);
  // Clip if pixel is outside the texture.
  lightTexCoord /= lightTexCoord.w;
  float isInside = all(float4(lightTexCoord.x, lightTexCoord.y, 1 - lightTexCoord.x, 1 - lightTexCoord.y) > 0);
  textureColor *= isInFront * isInside;
  
  // Diffuse
  float distanceAttenuation = ComputeDistanceAttenuation(lightDistance, range, attenuation);
  float nDotL = saturate(dot(normal, -lightDirection));
  resultDiffuse = diffuse * nDotL * distanceAttenuation * textureColor;
  
  // Phong specular
  //float3 r = normalize(reflect(lightDirection, normal));
  //float phong = pow(0.000001 + saturate(dot(r, -viewDirection)), specularPower);
  
  // Blinn-Phong specular
  float3 h = -normalize(lightDirection + viewDirection);
  float3 nDotH = saturate(dot(normal, h));
  float3 blinnPhong = pow(0.000001 + nDotH, specularPower);
  
  // Use a self-shadowing term. (e.g. N.L or saturate(4 * N.L)).
  float selfShadowingTerm = nDotL;
  
  resultSpecular = specular * blinnPhong * selfShadowingTerm * distanceAttenuation * textureColor;
}
#endif
