//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Encoding.fxh
/// Functions to encode/decode values.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_ENCODING_FXH
#define DIGITALRUNE_ENCODING_FXH

// Notes:
// (An alternative to RGBM would be RGBD (http://iwasbeingirony.blogspot.com/2010_06_01_archive.html).
// RGBM better conserves the full range and is better in most cases. RGBD conserves the low lights
// better and can be used if you only need occasional highlights.)


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// Matrix for encoding RGB to LogLuv.
static const float3x3 LogLuvMatrix = float3x3(0.2209, 0.3390, 0.4184,
                                              0.1138, 0.6780, 0.7319,
                                              0.0102, 0.1130, 0.2969);
// Inverse matrix for decoding LogLuv to RGB.
static const float3x3 LogLuvMatrixInverse = float3x3(6.0013, -2.700,  -1.7995,
                                                    -1.332,   3.1029, -5.7720,
                                                     0.3007, -1.088,   5.6268);


/// Declares the uniform const for the normals fitting texture + sampler.
/// \remarks
/// The normals fitting textures is a lookup texture used to encode normals in
/// EncodeNormalBestFit().
#define DECLARE_UNIFORM_NORMALSFITTINGTEXTURE \
texture NormalsFittingTexture : NORMALSFITTINGTEXTURE; \
sampler NormalsFittingSampler = sampler_state \
{ \
  Texture = <NormalsFittingTexture>; \
  AddressU  = CLAMP; \
  AddressV  = CLAMP; \
  MinFilter = POINT; \
  MagFilter = POINT; \
  MipFilter = POINT; \
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

// ---- Accurate, but slow RGBE encoding
// Reference: Wolfgang Engel, "Programming Vertex and Pixel Shader", pp. 230

// Constants for RGBE encoding.
static const float RgbeBase = 1.04;
static const float RgbeOffset = 64.0;


/// Calculates the logarithm for a given y and base, such that base^x = y.
/// param[in] base    The base of the logarithm.
/// param[in] y       The number of whic to calculate the logarithm.
/// \return The logarithm of y.
float LogEnc(float base, float y)
{
  // We use an "obsfuscated" name because the same method is declared in
  // Misc.fxh but we don't want to include that or create a duplicate definition.
  return log(y) / log(base);
}


/// Encodes the given color to RGBE 8-bit format.
/// \param[in] color    The original color.
/// \return The color encoded as RGBE.
float4 EncodeRgbe_Engel(float3 color)
{
  // Get the largest component.
  float maxValue = max(max(color.r, color.g), color.b);
  
  float exponent = floor(LogEnc(RgbeBase, maxValue));
  
  float4 result;
  
  // Store the exponent in the alpha channel.
  result.a = clamp((exponent + RgbeOffset) / 255, 0.0, 1.0 );
  
  // Convert the color channels.
  result.rgb = color / pow(RgbeBase, result.a * 255 - RgbeOffset);
  
  return result;
}


/// Decodes the given color from RGBE 8-bit format.
/// \param[in] rgbe   The color encoded as RGBE.
/// \return The orginal color.
float3 DecodeRgbe_Engel(float4 rgbe)
{
  // Get exponent from alpha channel.
  float exponent = rgbe.a * 255 - RgbeOffset;
  float scale = pow(RgbeBase, exponent);
  
  return rgbe.rgb * scale;
}
// -----


/// Encodes the given color to RGBE 8-bit format.
/// \param[in] color    The original color.
/// \return The color encoded as RGBE.
float4 EncodeRgbe(float3 color)
{
  // Get the largest component.
  float maxValue = max(max(color.r, color.g), color.b);
  
  float exponent = ceil(log2(maxValue));
  
  float4 result;
  
  // Store the exponent in the alpha channel.
  result.a = (exponent + 128) / 255;
  
  // Convert the color channels.
  result.rgb = color / exp2(exponent);
  
  return result;
}


/// Decodes the given color from RGBE 8-bit format.
/// \param[in] rgbe   The color encoded as RGBE.
/// \return The orginal color.
float3 DecodeRgbe(float4 rgbe)
{
  // Get exponent from alpha channel.
  float exponent = rgbe.a * 255 - 128;
  
  return rgbe.rgb * exp2(exponent);
}


/// Encodes the given float value in the range [0, 1] in a RGBA format (4 x 8 bit).
/// \param[in] value    The original value.
/// \return The value encoded as RGBA.
/// \remarks
/// The result is undefined if value is <0 or >1.
float4 EncodeFloatInRgba(float value)
{
  float4 result = value * float4(1,
                                 255.0,
                                 255.0 * 255.0,
                                 255.0 * 255.0 * 255.0);
  result.yzw = frac(result.yzw);
  result -= result.yzww * float4(1.0 / 255.0,
                                 1.0 / 255.0,
                                 1.0 / 255.0,
                                 0.0);
  return result;
}


/// Decodes the float value that was stored in a RGBA format (4 x 8 bit).
/// \param[in] rgba    The value encoded as RGBA.
/// \return The original value.
float DecodeFloatFromRgba(float4 rgba)
{
  float4 factors = float4(1.0,
                          1.0 / 255.0,
                          1.0 / (255.0 * 255.0),
                          1.0 / (255.0 * 255.0 * 255.0));
  return dot(rgba, factors);
}


/// Encodes the given color to LogLuv format.
/// \param[in] color    The original color.
/// \return The color encoded as LogLuv.
float4 EncodeLogLuv(float3 color)
{
  // See http://xnainfo.com/content.php?content=17,
  //     http://realtimecollisiondetection.net/blog/?p=15.
  
  float3 Xp_Y_XYZp = mul(color, LogLuvMatrix);
  Xp_Y_XYZp = max(Xp_Y_XYZp, float3(1e-6, 1e-6, 1e-6));   // Avoid values <= 0.
  float4 result;
  result.xy = Xp_Y_XYZp.xy / Xp_Y_XYZp.z;
  float Le = 2 * log2(Xp_Y_XYZp.y) + 127;
  result.w = frac(Le);
  result.z = (Le - (floor(result.w*255.0f))/255.0f)/255.0f;
  return result;
}


/// Decodes the given color from LogLuv format.
/// \param[in] logLuv   The color encoded as LogLuv.
/// \return The orginal color.
float3 DecodeLogLuv(float4 logLuv)
{
  float Le = logLuv.z * 255 + logLuv.w;
  float3 Xp_Y_XYZp;
  Xp_Y_XYZp.y = exp2((Le - 127) / 2);
  Xp_Y_XYZp.z = Xp_Y_XYZp.y / logLuv.y;
  Xp_Y_XYZp.x = logLuv.x * Xp_Y_XYZp.z;
  float3 vRGB = mul(Xp_Y_XYZp, LogLuvMatrixInverse);
  return max(vRGB, 0);
}


/// Encodes the given color to RGBM format.
/// \param[in] color    The original color.
/// \param[in] maxValue The max value, e.g. 6 (if color is gamma corrected) =
///                     6 ^ 2.2 (if color is in linear space).
/// \return The color in RGBM format.
/// \remarks
/// The input color can be in linear space or in gamma space. It is recommended
/// convert the color to gamma space before encoding as RGBM.
/// See http://graphicrants.blogspot.com/2009/04/rgbm-color-encoding.html.
float4 EncodeRgbm(float3 color, float maxValue)
{
  float4 rgbm;
  color /= maxValue;
  rgbm.a = saturate(max(max(color.r, color.g), max(color.b, 1e-6)));
  rgbm.a = ceil(rgbm.a * 255.0) / 255.0;
  rgbm.rgb = color / rgbm.a;
  return rgbm;
}


/// Decodes the given color from RGBM format.
/// \param[in] rgbm      The color in RGBM format.
/// \param[in] maxValue  The max value, e.g. 6 (if color is gamma corrected) =
///                      6 ^ 2.2 (if color is in linear space).
/// \return The original RGB color (can be in linear or gamma space).
float3 DecodeRgbm(float4 rgbm, float maxValue)
{
  return maxValue * rgbm.rgb * rgbm.a;
}


/// Encodes a normal vector in 3 8-bit channels.
/// \param[in] normal                 The normal vector.
/// \param[in] normalsFittingSampler  The lookup texture for normal fitting.
/// \return The normal encoded for storage in an RGB texture (3 x 8 bit).
half3 EncodeNormalBestFit(half3 normal, sampler normalsFittingSampler)
{
  // Best-fit normal encoding as in "CryENGINE 3: Reaching the Speed of Light"
  // by Anton Kaplanyan (Crytek). See http://advances.realtimerendering.com/s2010/index.html
  
  // Renormalize (needed if any blending or interpolation happened before).
  normal.rgb = (half3)normalize(normal.rgb);
  // Get unsigned normal for cubemap lookup. (Note, the full float precision is required.)
  half3 unsignedNormal = abs(normal.rgb);
  // Get the main axis for cubemap lookup.
  half maxNAbs = max(unsignedNormal.z, max(unsignedNormal.x, unsignedNormal.y));
  // Get texture coordinates in a collapsed cubemap.
  float2 texcoord = unsignedNormal.z < maxNAbs ? (unsignedNormal.y < maxNAbs ? unsignedNormal.yz : unsignedNormal.xz) : unsignedNormal.xy;
  texcoord = texcoord.x < texcoord.y ? texcoord.yx : texcoord.xy;
  texcoord.y /= texcoord.x;
  // Fit normal into the edge of unit cube.
  normal.rgb /= maxNAbs;
  // Look-up fitting length and scale the normal to get the best fit.
  half fittingScale = (half)tex2D(normalsFittingSampler, texcoord).a;
  // Scale the normal to get the best fit.
  normal.rgb *= fittingScale;
  // Squeeze back to unsigned.
  normal.rgb = normal.rgb * 0.5h + 0.5h;
  return normal;
}


/// Decodes a normal that was encoded with EncodeNormalBestFit().
/// \param[in] encodedNormal    The encoded normal.
/// \return The original normal.
half3 DecodeNormalBestFit(half4 encodedNormal)
{
  return (half3)normalize(encodedNormal.xyz * 2 - 1);
}


/// Encodes a normal vector in 2 channels (with 8 bit or more per channel).
/// \param[in] normal   The normal vector.
/// \return The normal encoded for storage in 2 channels.
half2 EncodeNormalSphereMap(half3 normal)
{
  // See http://aras-p.info/texts/CompactNormalStorage.html.
  half2 encodedNormal = (half2)normalize(normal.xy) * (half)sqrt(-normal.z*0.5+0.5);
  encodedNormal = encodedNormal * 0.5 + 0.5;
  return encodedNormal;
}


/// Decodes a normal that was encoded with EncodeNormalSphereMap().
/// \param[in] encodedNormal    The encoded normal.
/// \return The original normal.
half3 DecodeNormalSphereMap(half4 encodedNormal)
{
  half4 nn = encodedNormal * half4(2, 2, 0, 0) + half4(-1, -1, 1, -1);
  half l = dot(nn.xyz, -nn.xyw);
  nn.z = l;
  nn.xy *= (half)sqrt(l);
  return nn.xyz * 2 + half3(0, 0, -1);
}


/// Encodes a normal vector in 2 channels (with 8 bit or more per channel).
/// \param[in] normal   The normal vector.
/// \return The normal encoded for storage in 2 channels.
half2 EncodeNormalStereographic(half3 normal)
{
  // See http://aras-p.info/texts/CompactNormalStorage.html.
  half scale = 1.7777;
  half2 result = normal.xy / (normal.z + 1);
  result /= scale;
  result = result * 0.5 + 0.5;
  return result;
}


/// Decodes a normal that was encoded with EncodeNormalStereographic().
/// \param[in] encodedNormal    The encoded normal.
/// \return The original normal.
half3 DecodeNormalStereographic(half4 encodedNormal)
{
  half scale = 1.7777;
  half3 nn = encodedNormal.xyz * half3(2 * scale, 2 * scale, 0) + half3(-scale, -scale, 1);
  half g = 2.0 / dot(nn.xyz, nn.xyz);
  half3 normal;
  normal.xy = g * nn.xy;
  normal.z = g - 1;
  return normal;
}


/// Encodes a specular power (to be stored as unsigned byte).
/// \param[in] specularPower   The specular power.
/// \return The encoded specular power [0, 1].
float EncodeSpecularPower(float specularPower)
{
  // Linear packing:
  //   Compress range [0, max] --> [0, 1].
  //   y = x / max
  //return specularPower / 100.0f;
  // Or:
  //return specularPower / 512.0f;
  
  // Logarithmic packing (similar to Killzone):
  //   Compress range [1, max] --> [0, 1].
  //   y = log2(x) / log2(max)
  return log2(specularPower + 0.0001f) / 17.6f; // max = 200000
  
  // Unreal Engine Elemental Demo
  //return (log2(specularPower + 0.0001f) + 1) / 19.0f;
}


/// Decodes the specular power (stored as unsigned byte).
/// \param[in] encodedSpecularPower    The encoded specular power [0, 1].
/// \return The original specular power.
float DecodeSpecularPower(float encodedSpecularPower)
{
  //return encodedSpecularPower * 100.0f;
  //return encodedSpecularPower * 512.0f;
  return exp2(encodedSpecularPower * 17.6f);
  //return exp2(encodedSpecularPower * 19.0f - 1);
}
#endif
